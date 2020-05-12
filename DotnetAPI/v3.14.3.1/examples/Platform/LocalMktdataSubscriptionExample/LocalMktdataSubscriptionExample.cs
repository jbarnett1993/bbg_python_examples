/* Copyright 2012. Bloomberg Finance L.P.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:  The above
 * copyright notice and this permission notice shall be included in all copies
 * or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Bloomberglp.Blpapi;
using System.IO;
using System.Threading;

namespace Bloomberglp.Blpapi.Examples
{
    public class MktdataSubscriptionExample
    {
        private const String AUTH_USER = "AuthenticationType=OS_LOGON";
        private const String AUTH_APP_PREFIX = "AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
        private const String AUTH_USER_APP_MANUAL_PREFIX = "AuthenticationMode=USER_AND_APPLICATION;AuthenticationType=MANUAL;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
        private const String AUTH_DIR_PREFIX = "AuthenticationType=DIRECTORY_SERVICE;DirSvcPropertyName=";
        private const String AUTH_OPTION_NONE = "none";
        private const String AUTH_OPTION_USER = "user";
        private const String AUTH_OPTION_APP = "app=";
        private const String AUTH_OPTION_DIR = "dir=";
        private const String AUTH_OPTION_MANUAL = "manual=";

        private Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
        private Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");

        private const String d_defaultHost      = "localhost";
        private const int    d_defaultPort      = 8194;
        private const String d_defaultService   = "//viper/mktdata";
        private const int    d_defaultMaxEvents = int.MaxValue;

        private List<String> d_hosts       = new List<String>();
        private int          d_port        = d_defaultPort;
        private String       d_service     = d_defaultService;
        private int          d_maxEvents   = d_defaultMaxEvents;
        private String       d_authOptions = AUTH_USER;
        private List<String> d_topics      = new List<String>();
        private List<String> d_fields      = new List<String>();
        private List<String> d_options     = new List<String>();

        private String d_clientCredentials         = null;
        private String d_clientCredentialsPassword = null;
        private String d_trustMaterial             = null;
        private String d_manualUserName            = null;
        private String d_manualIPAddress           = null;

        public MktdataSubscriptionExample()
        {

        }

        public void Run(String[] args)
        {
            if (!ParseCommandLine(args)) return;

            SessionOptions sessionOptions = new SessionOptions();
            SessionOptions.ServerAddress[] servers = new SessionOptions.ServerAddress[d_hosts.Count];
            for (int i = 0; i < d_hosts.Count; ++i)
            {
                servers[i] = new SessionOptions.ServerAddress(d_hosts[i], d_port);
            }

            sessionOptions.ServerAddresses = servers;
            sessionOptions.AutoRestartOnDisconnection = true;
            sessionOptions.NumStartAttempts = d_hosts.Count;
            sessionOptions.DefaultSubscriptionService = d_service;
            sessionOptions.AuthenticationOptions = d_authOptions;

            if (d_clientCredentials != null && d_trustMaterial != null) {
                using (System.Security.SecureString password = new System.Security.SecureString())
                {
                    foreach (var c in d_clientCredentialsPassword)
                    {
                        password.AppendChar(c);
                    }

                    TlsOptions tlsOptions = TlsOptions.CreateFromFiles(d_clientCredentials, password, d_trustMaterial);
                    sessionOptions.TlsOptions = tlsOptions;
                }
            }

            System.Console.WriteLine("Connecting to port " + d_port + " on ");
            foreach (string host in d_hosts)
            {
                System.Console.WriteLine(host + " ");
            }
            Session session = new Session(sessionOptions);

            if (!session.Start())
            {
                System.Console.Error.WriteLine("Failed to start session.");
                return;
            }

            Identity identity = null;
            if (d_authOptions != null)
            {
                bool isAuthorized = false;
                identity = session.CreateIdentity();
                if (session.OpenService("//blp/apiauth"))
                {
                    Service authService = session.GetService("//blp/apiauth");
                    if (Authorize(authService, identity, session, new CorrelationID()))
                    {
                        isAuthorized = true;
                    }
                }
                if (!isAuthorized)
                {
                    System.Console.Error.WriteLine("No authorization");
                    return;
                }
            }

            List<Subscription> subscriptions = new List<Subscription>();
            foreach (String topic in d_topics)
            {
                subscriptions.Add(new Subscription(
                    d_service + topic,
                    d_fields,
                    d_options,
                    new CorrelationID(topic)));
            }
            session.Subscribe(subscriptions, identity);
            ProcessSubscriptionResponse(session);
        }

        private bool Authorize(
                Service authService,
                Identity identity,
                Session session,
                CorrelationID cid)
        {
            EventQueue tokenEventQueue = new EventQueue();
            try
            {
                if (d_manualIPAddress != null && d_manualUserName != null)
                {
                    session.GenerateToken(d_manualUserName,
                                          d_manualIPAddress,
                                          new CorrelationID(tokenEventQueue),
                                          tokenEventQueue);
                }
                else
                {
                    session.GenerateToken(new CorrelationID(tokenEventQueue), tokenEventQueue);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return false;
            }
            String token = null;
            const int timeoutMilliSeconds = 10000;
            Event eventObj = tokenEventQueue.NextEvent(timeoutMilliSeconds);
            if (eventObj.Type == Event.EventType.TOKEN_STATUS ||
                eventObj.Type == Event.EventType.REQUEST_STATUS)
            {
                foreach (Message msg in eventObj)
                {
                    System.Console.WriteLine(msg.ToString());
                    if (msg.MessageType == TOKEN_SUCCESS)
                    {
                        token = msg.GetElementAsString("token");
                    }
                }
            }
            if (token == null)
            {
                System.Console.WriteLine("Failed to get token");
                return false;
            }

            Request authRequest = authService.CreateAuthorizationRequest();
            authRequest.Set("token", token);

            session.SendAuthorizationRequest(authRequest, identity, cid);

            long startTime = System.DateTime.Now.Ticks;
            const int WAIT_TIME = 10 * 1000; // 10 seconds

            while (true)
            {
                eventObj = session.NextEvent(WAIT_TIME);
                if (eventObj.Type == Event.EventType.RESPONSE
                    || eventObj.Type == Event.EventType.PARTIAL_RESPONSE
                    || eventObj.Type == Event.EventType.REQUEST_STATUS)
                {
                    foreach (Message msg in eventObj)
                    {
                        System.Console.WriteLine(msg.ToString());
                        if (msg.MessageType == AUTHORIZATION_SUCCESS)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                if (System.DateTime.Now.Ticks - startTime > WAIT_TIME * 10000)
                {
                    return false;
                }
            }
        }

        private void PrintUsage()
        {
            Console.WriteLine(
"Usage:\n" +
" [-ip    <ipAddress>] \tserver name or IP      (default = " + d_defaultHost + ")\n" +
" [-p     <tcpPort>]   \tserver port            (default = " + d_defaultPort + ")\n" +
" [-s     <service>]   \tservice name           (default = " + d_defaultService + ")\n" +
" [-t     <topic>]     \ttopic to subscribed to (default = \"/ticker/IBM Equity\")\n" +
" [-f    <field>]      \tfield to subscribe to  (default: empty)\n" +
" [-o    <option>]     \tsubscription options   (default: empty)\n" +
" [-me   <maxEvents>]  \tmax number of events   (default = " + d_defaultMaxEvents + ")\n" +
" [-auth <option>]     \tauthorization option   (default = user)\n" +
"\t\tnone\n" +
"\t\tuser                 as a user using OS logon information\n" +
"\t\tdir=<property>       as a user using directory services\n" +
"\t\tapp=<app>            as the specified application\n" +
"\t\tuserapp=<app>        as user and application using logon information\n" +
"\t\t                     for the user\n" +
"\t\tmanual=<app,ip,user> as user and application, with manually provided\n" +
"\t\t                     IP address and EMRS user\n" +
"\n" +
"TLS OPTIONS (specify all or none):\n" +
" [-tls-client-credentials <file>]          \tname a PKCS#12 file to use as a source of client credentials\n" +
" [-tls-client-credentials-password <file>] \tspecify password for accessing client credentials\n" +
" [-tls-trust-material <file>]              \tname a PKCS#7 file to use as a source of trusted certificates\n");
        }

        private bool ParseCommandLine(String[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    if (string.Compare("-s", args[i], true) == 0 && i + 1 < args.Length)
                    {
                        d_service = args[++i];
                    }
                    else if (string.Compare("-ip", args[i], true) == 0 && i + 1 < args.Length)
                    {
                        d_hosts.Add(args[++i]);
                    }
                    else if (string.Compare("-p", args[i], true) == 0 && i + 1 < args.Length)
                    {
                        d_port = int.Parse(args[++i]);
                    }
                    else if (string.Compare("-me", args[i], true) == 0 && i + 1 < args.Length)
                    {
                        d_maxEvents = int.Parse(args[++i]);
                    }
                    else if (string.Compare("-t", args[i], true) == 0 && i + 1 < args.Length)
                    {
                        d_topics.Add(args[++i]);
                    }
                    else if (string.Compare("-f", args[i], true) == 0 && i + 1 < args.Length)
                    {
                        d_fields.Add(args[++i]);
                    }
                    else if (string.Compare("-o", args[i], true) == 0 && i + 1 < args.Length)
                    {
                        d_options.Add(args[++i]);
                    }
                    else if (string.Compare("-auth", args[i], true) == 0 && i + 1 < args.Length)
                    {
                        ++i;
                        if (string.Compare(AUTH_OPTION_NONE, args[i], true) == 0)
                        {
                            d_authOptions = null;
                        }
                        else if (string.Compare(AUTH_OPTION_USER, args[i], true) == 0)
                        {
                            d_authOptions = AUTH_USER;
                        }
                        else if (string.Compare(AUTH_OPTION_APP, 0, args[i], 0,
                                            AUTH_OPTION_APP.Length, true) == 0)
                        {
                            d_authOptions = AUTH_APP_PREFIX
                                + args[i].Substring(AUTH_OPTION_APP.Length);
                        }
                        else if (string.Compare(AUTH_OPTION_DIR, 0, args[i], 0,
                                            AUTH_OPTION_DIR.Length, true) == 0)
                        {
                            d_authOptions = AUTH_DIR_PREFIX
                                + args[i].Substring(AUTH_OPTION_DIR.Length);
                        }
                        else if (string.Compare(AUTH_OPTION_MANUAL, 0, args[i],
                                    0, AUTH_OPTION_MANUAL.Length, true) == 0)
                        {
                            string[] parms = args[i].Substring(AUTH_OPTION_MANUAL.Length).Split(',');
                            if (parms.Length != 3) {
                                PrintUsage();
                                return false;
                             }
                            d_authOptions = AUTH_USER_APP_MANUAL_PREFIX + parms[0];
                            d_manualIPAddress = parms[1];
                            d_manualUserName  = parms[2];
                        }
                        else
                        {
                            PrintUsage();
                            return false;
                        }
                    }
                    else if (string.Compare("-tls-client-credentials", args[i], true) == 0 && i + 1 < args.Length)
                    {
                        d_clientCredentials = args[++i];
                    }
                    else if (string.Compare("-tls-client-credentials-password", args[i], true) == 0 && i + 1 < args.Length)
                    {
                        d_clientCredentialsPassword = args[++i];
                    }
                    else if (string.Compare("-tls-trust-material", args[i], true) == 0 && i + 1 < args.Length)
                    {
                        d_trustMaterial = args[++i];
                    }
                    else
                    {
                        PrintUsage();
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                PrintUsage();
                return false;
            }

            if (d_hosts.Count == 0)
            {
                d_hosts.Add("localhost");
            }
            if (d_topics.Count == 0)
            {
                d_topics.Add("/ticker/IBM Equity");
            }

            return true;
        }

        private void ProcessSubscriptionResponse(Session session)
        {
            int eventCount = 0;
            while (true)
            {
                Event eventObj = session.NextEvent();
                foreach (Message msg in eventObj)
                {
                    if (eventObj.Type == Event.EventType.SUBSCRIPTION_DATA ||
                        eventObj.Type == Event.EventType.SUBSCRIPTION_STATUS)
                    {
                        string topic = (string)msg.CorrelationID.Object;
                        System.Console.WriteLine(topic + ": " + msg.AsElement);
                    }
                    else
                    {
                        System.Console.WriteLine(msg.AsElement);
                    }
                }

                if (eventObj.Type == Event.EventType.SUBSCRIPTION_DATA)
                {
                    if (++eventCount >= d_maxEvents)
                    {
                        break;
                    }
                }
            }
        }

        public static void Main(String[] args)
        {
            MktdataSubscriptionExample example = new MktdataSubscriptionExample();
            try
            {
                example.Run(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

}
