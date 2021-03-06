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

using Event = Bloomberglp.Blpapi.Event;
using EventHandler = Bloomberglp.Blpapi.EventHandler;
using Element = Bloomberglp.Blpapi.Element;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Session = Bloomberglp.Blpapi.Session;

using ArrayList = System.Collections.ArrayList;
using Thread = System.Threading.Thread;
using System.Collections.Generic;
using System;

namespace Bloomberglp.Blpapi.Examples
{
    public class SourceRefSubscriptionExample
    {
        private const string SOURCEREF_SVC = "//blp/srcref";
        private const string AUTH_SVC = "//blp/apiauth";

        private static readonly Name EXCEPTIONS = new Name("exceptions");
        private static readonly Name FIELD_ID = new Name("fieldId");
        private static readonly Name REASON = new Name("reason");
        private static readonly Name CATEGORY = new Name("category");
        private static readonly Name DESCRIPTION = new Name("description");
        private static readonly Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
        private static readonly Name AUTHORIZATION_FAILURE = Name.GetName("AuthorizationFailure");
        private static readonly Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");
        private static readonly Name TOKEN_FAILURE = Name.GetName("TokenGenerationFailure");

        private List<string> d_hosts;
        private int d_port;
        private string d_authOption;
        private string d_name;
        private SessionOptions d_sessionOptions;
        private Session d_session;
        private List<string> d_securities;
        private List<string> d_options;
        private string subscriptionOptions;
        private List<Subscription> d_subscriptions;
        private Identity d_identity;

        private string d_token;

        public static void Main(string[] args)
        {
            System.Console.WriteLine("SourceRefSubscriptionExample");
            SourceRefSubscriptionExample example = new SourceRefSubscriptionExample();
            example.run(args);
        }

        public SourceRefSubscriptionExample()
        {
            d_port = 8194;
            d_sessionOptions = new SessionOptions();
            d_securities = new List<string>();
            d_subscriptions = new List<Subscription>();
            d_hosts = new List<string>();
            d_options = new List<string>();
        }

        private bool createSession()
        {
            if (d_session != null) d_session.Stop();

            string authOptions = string.Empty;

            d_sessionOptions = new SessionOptions();

            if (d_authOption == "APPLICATION")
            {
                // Set Application Authentication Option
                authOptions = "AuthenticationMode=APPLICATION_ONLY;";
                authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
                // ApplicationName is the entry in EMRS.
                authOptions += "ApplicationName=" + d_name;
            }
            else if (d_authOption == "DIRSVC")
            {
                // Authenticate user using active directory service property
                authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
                authOptions += "DirSvcPropertyName=" + d_name;
            }
            else if (d_authOption == "NONE")
            {
                // do nothing
            }
            else
            {
                // Authenticate user using windows/unix login name
                authOptions = "AuthenticationType=OS_LOGON";
            }

            System.Console.WriteLine("Authentication Options = " + authOptions);
            d_sessionOptions.AuthenticationOptions = authOptions;

            SessionOptions.ServerAddress[] servers = new SessionOptions.ServerAddress[d_hosts.Count];
            int index = 0;
            System.Console.WriteLine("Connecting to port " + d_port.ToString() + " on host(s):");
            foreach (string host in d_hosts)
            {
                servers[index] = new SessionOptions.ServerAddress(host, d_port);
                System.Console.WriteLine(host);
            }

            // auto restart on disconnect
            d_sessionOptions.ServerAddresses = servers;
            d_sessionOptions.AutoRestartOnDisconnection = true;
            d_sessionOptions.NumStartAttempts = d_hosts.Count;

            d_session = new Session(d_sessionOptions, new EventHandler(processEvent));
            return d_session.Start();
        }

        private bool GenerateToken(out string token)
        {
            bool isTokenSuccess = false;
            bool isRunning = false;

            token = string.Empty;
            CorrelationID tokenReqId = new CorrelationID(99);
            EventQueue tokenEventQueue = new EventQueue();

            d_session.GenerateToken(tokenReqId, tokenEventQueue);

            while (!isRunning)
            {
                Event eventObj = tokenEventQueue.NextEvent();
                if (eventObj.Type == Event.EventType.TOKEN_STATUS)
                {
                    System.Console.WriteLine("processTokenEvents");
                    foreach (Message msg in eventObj)
                    {
                        System.Console.WriteLine(msg.ToString());
                        if (msg.MessageType == TOKEN_SUCCESS)
                        {
                            token = msg.GetElementAsString("token");
                            isTokenSuccess = true;
                            isRunning = true;
                            break;
                        }
                        else if (msg.MessageType == TOKEN_FAILURE)
                        {
                            System.Console.WriteLine("Received : " + TOKEN_FAILURE.ToString());
                            isRunning = true;
                            break;
                        }
                        else
                        {
                            System.Console.WriteLine("Error while Token Generation");
                            isRunning = true;
                            break;
                        }
                    }
                }
            }
            return isTokenSuccess;
        }

        private bool IsBPipeAuthorized(string token, out Identity identity)
        {
            bool isAuthorized = false;
            bool isRunning = true;
            identity = null;

            if (!d_session.OpenService(AUTH_SVC))
            {
                System.Console.Error.WriteLine("Failed to open " + AUTH_SVC);
                return (isAuthorized = false);

            }
            Service authService = d_session.GetService(AUTH_SVC);


            Request authRequest = authService.CreateAuthorizationRequest();

            authRequest.Set("token", token);
            identity = d_session.CreateIdentity();
            EventQueue authEventQueue = new EventQueue();

            d_session.SendAuthorizationRequest(authRequest, identity, authEventQueue, new CorrelationID(1));

            while (isRunning)
            {
                Event eventObj = authEventQueue.NextEvent();
                System.Console.WriteLine("processEvent");
                if (eventObj.Type == Event.EventType.RESPONSE || eventObj.Type == Event.EventType.REQUEST_STATUS)
                {
                    foreach (Message msg in eventObj)
                    {
                        if (msg.MessageType == AUTHORIZATION_SUCCESS)
                        {
                            System.Console.WriteLine("Authorization SUCCESS");

                            isAuthorized = true;
                            isRunning = false;
                            break;
                        }
                        else if (msg.MessageType == AUTHORIZATION_FAILURE)
                        {
                            System.Console.WriteLine("Authorization FAILED");
                            System.Console.WriteLine(msg);
                            isRunning = false;
                        }
                        else
                        {
                            System.Console.WriteLine(msg);
                        }
                    }
                }
            }
            return isAuthorized;
        }

        private void run(string[] args)
        {
            if (!parseCommandLine(args)) return;
            // create session
            if (!createSession()) return;

            System.Console.WriteLine("Connected successfully\n");

            if (!d_session.OpenService(SOURCEREF_SVC))
            {
                System.Console.Error.WriteLine("Failed to open service " + SOURCEREF_SVC);
                d_session.Stop();
                return;
            }

            if (d_authOption == "NONE")
            {
                System.Console.WriteLine("Subscribing...\n");
                d_session.Subscribe(d_subscriptions);
            }
            else
            {
                // Authenticate user using Generate Token Request 
                if (!GenerateToken(out d_token)) return;

                //Authorization : pass Token into authorization request. Returns User handle with user's entitlements info set by server.
                if (!IsBPipeAuthorized(d_token, out d_identity)) return;

                System.Console.WriteLine("Subscribing...\n");
                d_session.Subscribe(d_subscriptions, d_identity);
            }

            // wait for enter key to exit application
            System.Console.Read();

            d_session.Stop();
            System.Console.WriteLine("Exiting.");
        }

        public void processEvent(Event eventObj, Session session)
        {
            try
            {
                switch (eventObj.Type)
                {
                    case Event.EventType.SUBSCRIPTION_DATA:
                        processSubscriptionDataEvent(eventObj, session);
                        break;
                    case Event.EventType.SUBSCRIPTION_STATUS:
                        processSubscriptionStatus(eventObj, session);
                        break;
                    default:
                        processMiscEvents(eventObj, session);
                        break;
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
        }

        private void processSubscriptionStatus(Event eventObj, Session session)
        {
            System.Console.WriteLine("Processing SUBSCRIPTION_STATUS");
            foreach (Message msg in eventObj)
            {
                string topic = (string)msg.CorrelationID.Object;
                System.Console.WriteLine(System.DateTime.Now.ToString("s") +
                    ": " + topic + " - " + msg.MessageType);

                if (msg.HasElement(REASON))
                {
                    // This can occur on SubscriptionFailure.
                    Element reason = msg.GetElement(REASON);
                    System.Console.WriteLine("\t" +
                            reason.GetElement(CATEGORY).GetValueAsString() +
                            ": " + reason.GetElement(DESCRIPTION).GetValueAsString());
                }

                if (msg.HasElement(EXCEPTIONS))
                {
                    // This can occur on SubscriptionStarted if at least
                    // one field is good while the rest are bad.
                    Element exceptions = msg.GetElement(EXCEPTIONS);
                    for (int i = 0; i < exceptions.NumValues; ++i) {
                        Element exInfo = exceptions.GetValueAsElement(i);
                        Element fieldId = exInfo.GetElement(FIELD_ID);
                        Element reason = exInfo.GetElement(REASON);
                        System.Console.WriteLine("\t" + fieldId.GetValueAsString() +
                                ": " + reason.GetElement(CATEGORY).GetValueAsString());
                    }
                }
                System.Console.WriteLine("");
            }
        }

        private void processSubscriptionDataEvent(Event eventObj, Session session)
        {
            System.Console.WriteLine("Processing SUBSCRIPTION_DATA");
            foreach (Message msg in eventObj)
            {
                string topic = (string)msg.CorrelationID.Object;
                System.Console.WriteLine(System.DateTime.Now.ToString("s")
                    + ": " + topic + " - " + msg.ToString());
            }
        }

        private void processMiscEvents(Event eventObj, Session session)
        {
            System.Console.WriteLine("Processing " + eventObj.Type);
            foreach (Message msg in eventObj)
            {
                System.Console.WriteLine(System.DateTime.Now.ToString("s") +
                    ": " + msg.MessageType + "\n");
            }
        }

        private bool parseCommandLine(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (string.Compare(args[i], "-s", true) == 0
                    && i + 1 < args.Length)
                {
                    d_securities.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-t", true) == 0
                    && i + 1 < args.Length)
                {
                    d_options.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-ip", true) == 0
                    && i + 1 < args.Length)
                {
                    d_hosts.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-p", true) == 0
                    && i + 1 < args.Length)
                {
                    int outPort = 0;
                    if (int.TryParse(args[++i], out outPort))
                    {
                        d_port = outPort;
                    }
                }
                else if (string.Compare("-auth", args[i], true) == 0
                    && i + 1 < args.Length)
                {
                    d_authOption = args[++i].Trim();
                }
                else if (string.Compare("-n", args[i], true) == 0
                    && i + 1 < args.Length)
                {
                    d_name = args[++i].Trim();
                }
                else
                {
                    printUsage();
                    return false;
                }
            }

		    // check for appliation name
		    if (d_authOption == "APPLICATION" && (d_name == "")){
			     Console.WriteLine("Application name cannot be NULL for application authorization.");
			     printUsage();
                 return false;
		    }
		    // check for Directory Service name
		    if (d_authOption == "DIRSVC" && d_name == ""){
                Console.WriteLine("Directory Service property name cannot be NULL for DIRSVC authorization.");
			    printUsage();
                return false;
		    }

            if (d_securities.Count == 0)
            {
                d_securities.Add("/conditioncodes/eid/14003");
            }

            if (d_hosts.Count == 0)
            {
                Console.WriteLine("Missing Host IP.");
				printUsage();
				return false;
            }

            if (d_options.Count == 0)
            {
                subscriptionOptions = "";
            }
            else
            {
                for (int i = 0; i < d_options.Count; ++i)
                {
                    if (i == 0)
                    {
                        subscriptionOptions = "?type=" + d_options[i];
                    }
                    else
                    {
                        subscriptionOptions += "," + d_options[i];
                    }
                }
            }

            foreach (string security in d_securities)
            {
                string tempSecurity = security;
                // add source reference service to security
                if (!tempSecurity.StartsWith("/"))
                {
                    tempSecurity = "/" + tempSecurity;
                }
                if (!tempSecurity.StartsWith("//"))
                {
                    tempSecurity = SOURCEREF_SVC + tempSecurity;
                }
                // add subscription to subscription list
                Subscription subscription = new Subscription(tempSecurity + subscriptionOptions, new CorrelationID(security));
                System.Console.WriteLine(subscription.ToString());
                d_subscriptions.Add(subscription);
            }
            return true;
        }

        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("	Retrieve realtime source reference data using Bloomberg v3 API");
            System.Console.WriteLine("		[-s			<security   = \"/conditioncodes/eid/14003>\" or \"/tradingstatuses/eid/14003>\">");
            System.Console.WriteLine("		[-t			<type       = TRADE,QUOTE,MKTDEPTH or PERIOD,STATUS>");
            System.Console.WriteLine("		[-ip 		<ipAddress	= localhost>");
            System.Console.WriteLine("		[-p 		<tcpPort	= 8194>");
            System.Console.WriteLine("		[-auth      <authenticationOption = NONE or LOGON or APPLICATION or DIRSVC>]");
            System.Console.WriteLine("      [-n         <name = applicationName or directoryService>]");
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine(" -Specify only LOGON to authorize 'user' using Windows login name.");
            System.Console.WriteLine(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
            System.Console.WriteLine(" -Specify APPLICATION and name(Application Name) to authorize application.");
        }
    }
}