/* Copyright 2015. Bloomberg Finance L.P.
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
using Session = Bloomberglp.Blpapi.Session;
using Name = Bloomberglp.Blpapi.Name;
using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Service = Bloomberglp.Blpapi.Service;
using Identity = Bloomberglp.Blpapi.Identity;
using Request = Bloomberglp.Blpapi.Request;
using EventHandler = Bloomberglp.Blpapi.EventHandler;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using CorrrelationID = Bloomberglp.Blpapi.CorrelationID;
using EventQueue = Bloomberglp.Blpapi.EventQueue;
using RequestTemplate = Bloomberglp.Blpapi.RequestTemplate;
using System.Collections.Generic;

namespace Bloomberglp.Blpapi.Examples
{
    class SnapshotRequestTemplateExample
    {
        private Name AUTHORIZATION_SUCCESS
                = Name.GetName("AuthorizationSuccess");
        private Name TOKEN_SUCCESS
                = Name.GetName("TokenGenerationSuccess");

        private const String AUTH_USER
                = "AuthenticationType=OS_LOGON";
        private const String AUTH_APP_PREFIX
                = "AuthenticationMode=APPLICATION_ONLY;"
                        + "ApplicationAuthenticationType=APPNAME_AND_KEY;"
                        + "ApplicationName=";
        private const String AUTH_USER_APP_PREFIX
                = "AuthenticationMode=USER_AND_APPLICATION;"
                        + "AuthenticationType=OS_LOGON;"
                        + "ApplicationAuthenticationType=APPNAME_AND_KEY;"
                        + "ApplicationName=";
        private const String AUTH_DIR_PREFIX
                = "AuthenticationType=DIRECTORY_SERVICE;DirSvcPropertyName=";
        private const String AUTH_OPTION_NONE = "none";
        private const String AUTH_OPTION_USER = "user";
        private const String AUTH_OPTION_APP  = "app=";
        private const String AUTH_OPTION_USER_APP = "userapp=";
        private const String AUTH_OPTION_DIR  = "dir=";

        private const String d_defaultHost      = "localhost";
        private const int    d_defaultPort      = 8194;
        private const String d_defaultService   = "//viper/mktdata";
        private const int    d_defaultMaxEvents = int.MaxValue;

        private String              d_service = d_defaultService;
        private List<String>        d_hosts = new List<String>();
        private int                 d_port = d_defaultPort;
        private int                 d_maxEvents = d_defaultMaxEvents;

        private String              d_authOptions = AUTH_USER;
        private List<String>        d_topics = new List<String>();
        private List<String>        d_fields = new List<String>();
        private List<String>        d_options = new List<String>();

        private void PrintUsage() {
            Console.WriteLine("Create a snapshot request template and send a request using the request template.");
            Console.WriteLine("Usage:");
            Console.WriteLine("\t[-ip   <ipAddress>]  \tserver name or IP (default: localhost)");
            Console.WriteLine("\t[-p    <tcpPort>]    \tserver port (default: 8194)");
            Console.WriteLine("\t[-s    <service>]    \tservice name (default: //viper/mktdata)");
            Console.WriteLine("\t[-t    <topic>]      \ttopic to subscribe to (default: \"/ticker/IBM Equity\")");
            Console.WriteLine("\t[-f    <field>]      \tfield to subscribe to (default: empty)");
            Console.WriteLine("\t[-o    <option>]     \tsubscription options (default: empty)");
            Console.WriteLine("\t[-me   <maxEvents>]  \tstop after this many events (default: Int.MaxValue)");
            Console.WriteLine("\t[-auth <option>]     \tauthentication option: user|none|app=<app>|userapp=<app>|dir=<property> (default: user)");
        }

        private bool ParseCommandLine(String[] args) {
            for (int i = 0; i < args.Length; ++i) {
                if (string.Compare("-s", args[i], true) == 0
                        && i + 1 < args.Length) {
                    d_service = args[++i];
                } else if (string.Compare("-t", args[i], true) == 0
                        && i + 1 < args.Length) {
                    d_topics.Add(args[++i]);
                } else if (string.Compare("-f", args[i], true) == 0
                        && i + 1 < args.Length) {
                    d_fields.Add(args[++i]);
                } else if (string.Compare("-o", args[i], true) == 0
                        && i + 1 < args.Length) {
                    d_options.Add(args[++i]);
                } else if (string.Compare("-ip", args[i], true) == 0
                        && i + 1 < args.Length) {
                    d_hosts.Add(args[++i]);
                } else if (string.Compare("-p", args[i], true) == 0
                        && i + 1 < args.Length) {
                    d_port = int.Parse(args[++i]);
                } else if (string.Compare("-me", args[i], true) == 0
                        && i + 1 < args.Length) {
                    d_maxEvents = int.Parse(args[++i]);
                } else if (string.Compare("-auth", args[i], true) == 0
                        && i + 1 < args.Length) {
                    ++i;
                    if (string.Compare(AUTH_OPTION_NONE, args[i], true) == 0) {
                        d_authOptions = null;
                    } else if (string.Compare(AUTH_OPTION_USER,
                                              args[i],
                                              true) == 0) {
                        d_authOptions = AUTH_USER;
                    } else if (string.Compare(AUTH_OPTION_APP,
                                              0,
                                              args[i], 0,
                                              AUTH_OPTION_APP.Length,
                                              true) == 0) {
                        d_authOptions = AUTH_APP_PREFIX
                                + args[i].Substring(AUTH_OPTION_APP.Length);
                    } else if (string.Compare(AUTH_OPTION_DIR,
                                              0,
                                              args[i],
                                              0,
                                              AUTH_OPTION_DIR.Length,
                                              true) == 0) {
                        d_authOptions = AUTH_DIR_PREFIX
                                + args[i].Substring(AUTH_OPTION_DIR.Length);
                    } else if (string.Compare(AUTH_OPTION_USER_APP,
                                              0,
                                              args[i],
                                              0,
                                              AUTH_OPTION_USER_APP.Length,
                                              true) == 0) {
                        d_authOptions = AUTH_USER_APP_PREFIX
                            + args[i].Substring(AUTH_OPTION_USER_APP.Length);

                    } else {
                        PrintUsage();
                        return false;
                    }
                } else {
                    PrintUsage();
                    return false;
                }
            }

            if (d_hosts.Count == 0) {
                d_hosts.Add(d_defaultHost);
            }
            if (d_topics.Count == 0) {
                d_topics.Add("/ticker/IBM Equity");
            }

            return true;
        }

        void PrintMessage(Event eventObj) {
            foreach (Message msg in eventObj) {
                Console.WriteLine(msg.ToString());
            }
        }

        private void Run(String[] args) {
            if (!ParseCommandLine(args))
                return;

            SessionOptions.ServerAddress[] servers
                = new SessionOptions.ServerAddress[d_hosts.Count];

            for (int i = 0; i < d_hosts.Count; ++i) {
                servers[i]
                    = new SessionOptions.ServerAddress(d_hosts[i], d_port);
            }

            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerAddresses = servers;
            sessionOptions.DefaultSubscriptionService = d_service;
            sessionOptions.DefaultTopicPrefix = "ticker";
                // normally defaults to "ticker"
            sessionOptions.AuthenticationOptions = d_authOptions;
            sessionOptions.AutoRestartOnDisconnection = true;
            sessionOptions.NumStartAttempts = servers.Length;

            Console.WriteLine("Connecting to");
            foreach (SessionOptions.ServerAddress server in
                sessionOptions.ServerAddresses) {
                Console.WriteLine(" " + server);
            }
            Console.WriteLine();

            Session session = new Session(sessionOptions);
            if (!session.Start()) {
                for (;;) {
                    Event e = session.TryNextEvent();
                    if (e == null)
                        break;
                    PrintMessage(e);
                }
                Console.Error.WriteLine("Failed to start session.");
                return;
            }

            Identity identity = null;
            if (d_authOptions != null) {
                bool isAuthorized = false;
                identity = session.CreateIdentity();
                if (session.OpenService("//blp/apiauth")) {
                    Service authService = session.GetService("//blp/apiauth");
                    isAuthorized = Authorize(authService,
                                             identity,
                                             session,
                                             new CorrelationID());
                }
                else {
                    Console.Error.WriteLine("Failed to open //blp/apiauth.");
                }
                if (!isAuthorized) {
                    Console.Error.WriteLine("No authorization");
                    return;
                }
            }

            String fieldsString = "?fields=";
            for(int iField = 0; iField < d_fields.Count; ++iField) {
                if(0 != iField) {
                    fieldsString += ",";
                }
                fieldsString += d_fields[iField];
            }

            // NOTE: resources used by a snapshot request template are
            // released only when 'RequestTemplateTerminated' message
            // is received or when the session is destroyed.  In order
            // to release resources when request template is not needed
            // anymore, user should call the 'Session.cancel' and pass
            // the correlation id used when creating the request template,
            // or call 'RequestTemplate.close'. If the 'Session.cancel'
            // is used, all outstanding requests are canceled and the
            // underlying subscription is closed immediately. If the
            // handle is closed with the 'RequestTemplate.close', the
            // underlying subscription is closed only when all outstanding
            // requests are served.
            Console.WriteLine("Creating snapshot request templates\n");
            List<RequestTemplate> snapshots = new List<RequestTemplate>();
            for (int iTopic = 0; iTopic < d_topics.Count; ++iTopic) {
                String subscriptionString
                        = d_service + d_topics[iTopic] + fieldsString;
                RequestTemplate requestTemplate
                        = session.createSnapshotRequestTemplate(
                                subscriptionString,
                                new CorrelationID(iTopic),
                                identity);
                snapshots.Add(requestTemplate);
            }

            int eventCount = 0;
            while (true) {
                Event eventObj = session.NextEvent(1000);
                foreach (Message msg in eventObj) {
                    if (eventObj.Type == Event.EventType.RESPONSE ||
                        eventObj.Type == Event.EventType.PARTIAL_RESPONSE) {
                        long iTopic = msg.CorrelationID.Value;
                        String topic = d_topics[(int)iTopic];
                        Console.WriteLine(topic + " - SNAPSHOT - ");
                    }
                    Console.WriteLine(msg);
                }
                if (eventObj.Type == Event.EventType.RESPONSE) {
                    if (++ eventCount >= d_maxEvents) {
                        break;
                    }
                }
                if (eventObj.Type == Event.EventType.TIMEOUT) {
                    Console.WriteLine(
                              "Sending request using the request templates\n");
                    for (int iTopic = 0; iTopic < snapshots.Count; ++iTopic) {
                        session.SendRequest(snapshots[iTopic],
                                            new CorrelationID(iTopic));
                    }
                }
            }
        }

        private bool Authorize(Service       authService,
                               Identity      identity,
                               Session       session,
                               CorrelationID cid) {

            EventQueue tokenEventQueue = new EventQueue();
            try {
                session.GenerateToken(new CorrelationID(), tokenEventQueue);
            } catch (Exception e) {
                Console.WriteLine("Generate token failed with exception: \n"
                                  + e);
                return false;
            }
            String token = null;
            int timeoutMilliSeconds = 10000;
            Event eventObj = tokenEventQueue.NextEvent(timeoutMilliSeconds);
            if (eventObj.Type == Event.EventType.TOKEN_STATUS ||
                    eventObj.Type == Event.EventType.REQUEST_STATUS) {
                foreach (Message msg in eventObj) {
                    Console.WriteLine(msg.ToString());
                    if (msg.MessageType == TOKEN_SUCCESS) {
                        token = msg.GetElementAsString("token");
                    }
                }
            }
            if (token == null){
                Console.Error.WriteLine("Failed to get token");
                return false;
            }

            Request authRequest = authService.CreateAuthorizationRequest();
            authRequest.Set("token", token);

            session.SendAuthorizationRequest(authRequest, identity, cid);

            long startTime = System.DateTime.Now.Ticks;
            const int WAIT_TIME = 10 * 1000; // 10 seconds

            while (true) {
                eventObj = session.NextEvent(WAIT_TIME);
                if (eventObj.Type == Event.EventType.RESPONSE
                        || eventObj.Type == Event.EventType.PARTIAL_RESPONSE
                        || eventObj.Type == Event.EventType.REQUEST_STATUS) {
                    foreach (Message msg in eventObj) {
                        Console.WriteLine(msg.ToString());
                        if (msg.MessageType == AUTHORIZATION_SUCCESS) {
                            return true;
                        } else {
                            return false;
                        }
                    }
                }
                if (System.DateTime.Now.Ticks - startTime > WAIT_TIME * 10000){
                    return false;
                }
            }
        }

        public static void Main(string[] args) {
            Console.WriteLine("SnapshotRequestTemplateExample");
            SnapshotRequestTemplateExample example
                    = new SnapshotRequestTemplateExample();
            try {
                example.Run(args);
            } catch (System.IO.IOException e) {
                Console.Error.WriteLine(e.StackTrace);
            } catch (System.Threading.ThreadInterruptedException e) {
                Console.Error.WriteLine(e.StackTrace);
            }
        }
    }
}
