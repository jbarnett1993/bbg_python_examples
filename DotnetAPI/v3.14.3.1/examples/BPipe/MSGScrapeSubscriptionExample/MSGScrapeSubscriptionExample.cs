//----------------------------------------------------------------------------
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A  PARTICULAR PURPOSE.
//----------------------------------------------------------------------------

using ArrayList = System.Collections.ArrayList;

using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;

using System.Collections.Generic;

namespace Bloomberglp.Blpapi.Examples
{
    public class MSGScrapeSubscriptionExample
    {
        private const string MSGSCRAPE_SVC = "//blp/msgscrape";
        private const string AUTH_SVC = "//blp/apiauth";

        private static readonly Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
        private static readonly Name AUTHORIZATION_FAILURE = Name.GetName("AuthorizationFailure");
        private static readonly Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");
        private static readonly Name TOKEN_FAILURE = Name.GetName("TokenGenerationFailure");

        private ArrayList d_hosts;
        private int d_port;
        private string d_authOption;
        private string d_dsName;
        private string d_name;
        private string d_token;
        private SessionOptions d_sessionOptions;
        private Session d_session;
        private Identity d_identity;
        private List<string> d_securities;
        private List<string> d_fields;
        private List<string> d_options;
        private List<Subscription> d_subscriptions;

        public static void Main(string[] args)
        {
            MSGScrapeSubscriptionExample example = new MSGScrapeSubscriptionExample();
            example.run(args);
            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
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
            else if (d_authOption == "USER_APP")
            {
                // Set User and Application Authentication Option
                authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
                authOptions += "AuthenticationType=OS_LOGON;";
                authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
                // ApplicationName is the entry in EMRS.
                authOptions += "ApplicationName=" + d_name;
            }
            else if (d_authOption == "USER_DS_APP")
            {
                // Set User and Application Authentication Option
                authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
                authOptions += "AuthenticationType=DIRECTORY_SERVICE;";
                authOptions += "DirSvcPropertyName=" + d_dsName + ";";
                authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
                // ApplicationName is the entry in EMRS.
                authOptions += "ApplicationName=" + d_name;
            }
            else if (d_authOption == "DIRSVC")
            {
                // Authenticate user using active directory service property
                authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
                authOptions += "DirSvcPropertyName=" + d_dsName;
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

            // use //blp/msgscrape service as defause subscription service
            d_sessionOptions.DefaultSubscriptionService = MSGSCRAPE_SVC;

            d_session = new Session(d_sessionOptions);
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
            d_hosts = new ArrayList();
            d_securities = new List<string>();
            d_fields = new List<string>();
            d_options = new List<string>();
            d_subscriptions = new List<Subscription>();
            d_dsName = "";
            d_name = "";

            d_port = 8194;

            // subscription fields
            d_fields.Add("BID");
            d_fields.Add("BID_SIZE");
            d_fields.Add("ASK");
            d_fields.Add("ASK_SIZE");

            if (!parseCommandLine(args)) return;

            // create session
            if (!createSession())
            {
                System.Console.Error.WriteLine("Failed to open session");
                return;
            }

            if (d_authOption != "NONE")
            {
                // Authenticate user using Generate Token Request 
                if (!GenerateToken(out d_token)) return;

                //Authorization : pass Token into authorization request. Returns User handle with user's entitlements info set by server.
                if (!IsBPipeAuthorized(d_token, out d_identity)) return;

                if (!d_session.OpenService(MSGSCRAPE_SVC))
                {
                    System.Console.Error.WriteLine("Failed to open " + MSGSCRAPE_SVC);
                    return;
                }
            }
            // print subscription options
            if (d_options.Count > 0)
            {
                System.Console.WriteLine("Subscription options:");
                int count = 1;
                foreach (string opt in d_options)
                {
                    System.Console.WriteLine("\tOption " + count + ": " + opt);
                }
            }

            // subscribe to MSG1 data
            if (d_authOption == "NONE")
            {
                d_session.Subscribe(d_subscriptions);
            }
            else
            {
                // subscribe with identity object
                d_session.Subscribe(d_subscriptions, d_identity);
            }

            while (true)
            {
                Event eventObj = d_session.NextEvent();
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
                else if (string.Compare(args[i], "-o", true) == 0
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
                else if (string.Compare("-ds", args[i], true) == 0
					&& i + 1 < args.Length)
                {
                    d_dsName = args[++i].Trim();
                }
                else if (string.Compare("-n", args[i], true) == 0
					&& i + 1 < args.Length)
                {
                    d_name = args[++i].Trim();
                }
                else if (string.Compare(args[i], "-h", true) == 0)
                {
                    printUsage();
                    return false;
                }
            }

            // check for application name
            if ((d_authOption == "APPLICATION" || d_authOption == "USER_APP") && (d_name == ""))
            {
                System.Console.WriteLine("Application name cannot be NULL for application authorization.");
                printUsage();
                return false;
            }
            if (d_authOption == "USER_DS_APP" && (d_name == "" || d_dsName == ""))
            {
                System.Console.WriteLine("Application or DS name cannot be NULL for application authorization.");
                printUsage();
                return false;
            }
            // check for Directory Service name
            if ((d_authOption == "DIRSVC") && (d_dsName == ""))
            {
                System.Console.WriteLine("Directory Service property name cannot be NULL for DIRSVC authorization.");
                printUsage();
                return false;
            }

            // handle default arguments
            if (d_hosts.Count == 0)
            {
                System.Console.WriteLine("Missing host IP");
                printUsage();
                return false;
            }

            // check if more than one EID
            if (d_options.Count > 1)
            {
                System.Console.WriteLine("Only one EID can be provided");
                printUsage();
                return false;
            }

            if (d_securities.Count == 0)
            {
                d_securities.Add("MSGSCRP MSG1 Curncy");
            }

            foreach (string security in d_securities)
            {
                Subscription subscription = new Subscription(
                    security, d_fields, d_options, new CorrelationID(security));
                System.Console.WriteLine(subscription.SubscriptionString);
                d_subscriptions.Add(new Subscription(
                    security, d_fields, d_options, new CorrelationID(security)));
            }

            return true;
        }

        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("	Subscribe to MSG Scrape data");
            System.Console.WriteLine("		[-s			<security	= MSGSCRP MSG1 Curncy>");
            System.Console.WriteLine("		[-o			<option     = EID=44321>");
            System.Console.WriteLine("		[-ip 		<ipAddress	= localhost>");
            System.Console.WriteLine("		[-p 		<tcpPort	= 8194>");
            System.Console.WriteLine("      [-auth  <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>]");
            System.Console.WriteLine("      [-n     <name = applicationName>]");
            System.Console.WriteLine("      [-ds    <dsName = directoryService>]");
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine("1) Only one EID can be specified.");
            System.Console.WriteLine("2) Specify only LOGON to authorize 'user' using Windows login name.");
            System.Console.WriteLine("3) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
            System.Console.WriteLine("4) Specify APPLICATION and name(Application Name) to authorize application.\n");
            System.Console.WriteLine("Example: MSGScrapeSubscriptionExample ip <Host IP> -p <Host Port> -s \"MSGSCRP MSG1 Curncy\"");
            System.Console.WriteLine("         MSGScrapeSubscriptionExample ip <Host IP> -p <Host Port> -s \"MSGSCRP MSG1 Curncy\" -o EID=44321");
        }
    }
}
