/* Copyright 2012. Bloomberg Finance L.P.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do
 * so, subject to the following conditions:  The above copyright notice and this
 * permission notice shall be included in all copies or substantial portions of
 * the Software.  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES
 * OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
 
using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using ArrayList = System.Collections.ArrayList;
using ServerAddress = Bloomberglp.Blpapi.SessionOptions.ServerAddress;

namespace Bloomberglp.Blpapi.Examples
{
    public class MarketListSubscriptionExample
    {
        private int d_port = 8194; 
        private ArrayList d_securities;
        private ArrayList d_hosts;
        private System.Collections.Generic.List<Subscription> d_subscriptions; 
        private string d_authOption = "";
        private string d_name = "";
        private Session d_session;
        private static string authServiceName = "//blp/apiauth";
        private static string mktListServiceName = "//blp/mktlist";
        private static Name TOKEN_SUCCESS = new Name("TokenGenerationSuccess");
        private static Name TOKEN_FAILURE = new Name("TokenGenerationFailure");
        private static Name TOKEN = new Name("token");
        private static Name AUTHORIZATION_SUCCESS = new Name("AuthorizationSuccess");
        private static Name EXCEPTIONS = new Name("exceptions");
        private static Name REASON = new Name("reason");
        private static Name CATEGORY = new Name("category");
	    private static Name DESCRIPTION = new Name("description");

        public static void Main(string[] args)
        {
            System.Console.WriteLine("MarketListSubscriptionExample");
            MarketListSubscriptionExample example = new MarketListSubscriptionExample();
            example.run(args);

            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        public MarketListSubscriptionExample()
        {
            d_securities = new ArrayList();
            d_hosts = new ArrayList();
            d_subscriptions = new System.Collections.Generic.List<Subscription>();
        }

        private void run(string[] args)
        {
            if (!parseCommandLine(args)) return;

            //create and open the session		
            createSession();

            if (string.Compare(d_authOption, "NONE", true)==0)
            {
                d_session.Subscribe(d_subscriptions);
            }
            else
            {
                // Authorize all the users that are interested in receiving data
                Identity identity = d_session.CreateIdentity();
                if (authorize(identity))
                {
                    // subscribe
                    d_session.Subscribe(d_subscriptions, identity);
                }
            }
        }

        private bool authorize(Identity identity)
        {
            EventQueue tokenEventQueue = new EventQueue();
            d_session.GenerateToken(new CorrelationID(), tokenEventQueue);
            string token = "";
            bool state = false;

            Event eventObj=tokenEventQueue.NextEvent();
            if (eventObj.Type == Event.EventType.TOKEN_STATUS) {
                foreach (Message msg in eventObj)
                {
                    msg.Print(System.Console.Out);
                    //if (msg.MessageType == TOKEN_SUCCESS)
                    if (msg.MessageType.Equals(TOKEN_SUCCESS))
                    {
                        token = msg.GetElementAsString(TOKEN);
                    }
                    else if (msg.MessageType.Equals(TOKEN_FAILURE))
                    {
                        break;
                    }
                }               
            }

            if (token.Length == 0) {
                System.Console.WriteLine("Failed to get token");
                return false;
            }

            if (!d_session.OpenService(authServiceName))
            {
                System.Console.Error.WriteLine("Failed to open " + authServiceName);
                return false;
            }

            Service authService = d_session.GetService(authServiceName);
            Request authRequest = authService.CreateAuthorizationRequest();
            authRequest.Set(TOKEN, token);

            EventQueue authQueue=new EventQueue();
            d_session.SendAuthorizationRequest(authRequest, identity, authQueue, new CorrelationID());

            bool done = false;
            while (!done)
            {
                Event eventObj2 = authQueue.NextEvent();
                if(eventObj2.Type.Equals(Event.EventType.RESPONSE) ||
                    eventObj2.Type.Equals(Event.EventType.REQUEST_STATUS) ||
                    eventObj2.Type.Equals(Event.EventType.PARTIAL_RESPONSE))
                {
                    foreach (Message msg in eventObj2)
                    {
                        msg.Print(System.Console.Out);
                        if(msg.MessageType.Equals(AUTHORIZATION_SUCCESS))
                        {
                            state = true;
                            done = true;
                            break;
                        }
                        else
                        {
                            System.Console.WriteLine("Authorization failed");
                            state = false;
                            done = true;
                            break;
                        }
                    }
                }
            }
            return state;
        }

        private void createSession()
        {
            string authOptions = "";
            SessionOptions sessionOptions = new SessionOptions();
            if (string.Compare(d_authOption, "APPLICATION", true) == 0)
            {
                // Set Application Authentication Option
                authOptions = "AuthenticationMode=APPLICATION_ONLY;";
                authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
                // ApplicationName is the entry in EMRS.
                authOptions += "ApplicationName=" + d_name;
            }
            else
            {
                // Set User authentication option
                if (string.Compare(d_authOption, "LOGON", true) == 0)
                {
                    // Authenticate user using windows/unix login name
                    authOptions = "AuthenticationType=OS_LOGON";
                }
                else if (string.Compare(d_authOption, "DIRSVC", true) == 0)
                {
                    // Authenticate user using active directory service property
                    authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
                    authOptions += "DirSvcPropertyName=" + d_name;
                }
                else
                {
                    // default to no auth
                    d_authOption = "NONE";
                }
            }

            System.Console.WriteLine("Authentication Options = " + authOptions);

            if (string.Compare(d_authOption, "NONE", true) != 0)
            {
                sessionOptions.AuthenticationOptions = authOptions;
            }
            sessionOptions.AutoRestartOnDisconnection = true;
            sessionOptions.NumStartAttempts = d_hosts.Count;

            SessionOptions.ServerAddress[] servers = new SessionOptions.ServerAddress[d_hosts.Count];

            for (int i = 0; i < d_hosts.Count; ++i)
            {
                servers[i] = new ServerAddress((string)d_hosts[i], d_port);
            }
            sessionOptions.ServerAddresses = servers;
            System.Console.Write("Connecting to port " + d_port + " on ");

            for (int i=0; i < sessionOptions.ServerAddresses.Length; ++i)
            {
                ServerAddress host = sessionOptions.ServerAddresses[i];
                System.Console.Write((i > 0 ? ", " : "") + host.Host);
            }

            System.Console.WriteLine();

            d_session = new Session(sessionOptions, new EventHandler(processEvent));

            bool sessionStarted = d_session.Start();
            if (!sessionStarted)
            {
                System.Console.Error.WriteLine("Failed to start session. Exiting...");
                System.Environment.Exit(-1);
            }
        }

        private void processSubscriptionDataEvent(Event eventObj) {
            System.Console.WriteLine("Processing SUBSCRIPTION_DATA");
            foreach (Message msg in eventObj)
            {
                System.Console.WriteLine(msg);

                string topic = msg.CorrelationID.ToString();
                System.Console.WriteLine("Fragment Type: " + msg.FragmentType.ToString());
                System.Console.WriteLine(getTimeStamp() + ": " + topic + " - " + msg.MessageType.ToString());

                int numFields = msg.AsElement.NumElements;
                for (int i = 0; i < numFields; ++i)
                {
                    Element field = msg.AsElement.GetElement(i);

                    if (field.NumValues < 1)
                    {
                        System.Console.WriteLine("        " + field.Name + " is NULL");
                        continue;
                    }
                }
            }
        }

        private void processSubscriptionStatus(Event eventObj) {
            System.Console.WriteLine("Processing SUBSCRIPTION_STATUS");
            foreach (Message msg in eventObj)
            {
                string topic = msg.CorrelationID.ToString();
                System.Console.WriteLine(getTimeStamp() + ": " + topic + " - " + msg.MessageType.ToString());

                if (msg.HasElement(REASON))
                {
                    Element reason = msg.GetElement(REASON);
                    System.Console.WriteLine("        " + 
							reason.GetElement(CATEGORY).GetValueAsString() + " " +
							reason.GetElement(DESCRIPTION).GetValueAsString());  
                }

                if (msg.HasElement(EXCEPTIONS)) {
					Element exceptions = msg.GetElement(EXCEPTIONS);
					for (int i = 0; i < exceptions.NumValues; ++i) {
						Element exInfo = exceptions.GetValueAsElement(i);
						Element reason = exInfo.GetElement(REASON);
                        System.Console.WriteLine("        " + reason.GetElement(CATEGORY).GetValueAsString());	
					}
				}

            }
        }

        private void processMiscEvents(Event eventObj) {
            foreach (Message msg in eventObj)
            {
                System.Console.WriteLine(getTimeStamp() + ": " + msg.MessageType.ToString());
            }
        }

        private void processEvent(Event eventObj, Session session)
        {
            try
            {
                switch (eventObj.Type)
                {
                    case Event.EventType.SUBSCRIPTION_DATA:
                        processSubscriptionDataEvent(eventObj);
                        break;
                    case Event.EventType.SUBSCRIPTION_STATUS:
                        processSubscriptionStatus(eventObj);
                        break;
                    default:
                        processMiscEvents(eventObj);
                        break;
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message.ToString());
            }
        }

        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
		    System.Console.WriteLine("	MarketListSubscriptionExample ");
		    System.Console.WriteLine("        [-s         <security   = //blp/mktlist/chain/bsym/US/IBM>");
		    System.Console.WriteLine("        [-ip        <ipAddress	= localhost>");
		    System.Console.WriteLine("        [-p         <tcpPort	= 8194>");
		    System.Console.WriteLine("        [-auth      <authenticationOption = NONE or LOGON or APPLICATION or DIRSVC>]" );
		    System.Console.WriteLine("        [-n         <name = applicationName or directoryService>]");
		    System.Console.WriteLine("Notes:");
		    System.Console.WriteLine(" -Specify only LOGON to authorize 'user' using Windows/unix login name.");
		    System.Console.WriteLine(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
            System.Console.WriteLine(" -Specify APPLICATION and name(Application Name) to authorize application.");
        }

        private string getTimeStamp()
        {
            System.DateTime now = System.DateTime.Now;
            return now.ToString();
        }

        private bool parseCommandLine(string[] args)
        {
            int argLen = args.Length;
            for (int i = 0; i < argLen; ++i)
            {
                if (string.Compare(args[i], "-s", true) == 0 && i + 1 < argLen)
                {
                    d_securities.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-ip", true) == 0 && i + 1 < argLen)
                {
                    d_hosts.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-p", true) == 0 && i + 1 < argLen)
                {
                    int outPort = 0;
                    if (int.TryParse(args[++i], out outPort))
                    {
                        d_port = outPort;
                    }
                }
                else if (string.Compare(args[i], "-auth", true) == 0 && i + 1 < argLen)
                {
                    d_authOption = args[++i];
                }
                else if (string.Compare(args[i], "-n", true) == 0 && i + 1 < argLen)
                {
                    d_name = args[++i];
                }
                else if (string.Compare(args[i], "-h", true) == 0)
                {
                    printUsage();
                    return false;
                }
            }

            // check for appliation name
            if (string.Compare(d_authOption, "APPLICATION", true)==0 && string.Compare(d_name, "", true)==0)
            {
                System.Console.WriteLine("Application name cannot be NULL for application authorization.");
                printUsage();
                return false;
            }

            // check for Directory Service name
            if (string.Compare(d_authOption, "DIRSVC", true) == 0 && string.Compare(d_name, "", true) == 0)
            {
                System.Console.WriteLine("Directory Service property name cannot be NULL for DIRSVC authorization.");
                printUsage();
                return false;
            }

            if (d_hosts.Count == 0)
		    {
			    System.Console.WriteLine("Missing host IP address.");
			    printUsage();
                return false;
		    }

            if (d_securities.Count == 0)
            {
                d_securities.Add(mktListServiceName + "/chain/bsym/US/IBM");
            }

            foreach (string security in d_securities)
            {
                string sec = security;
                int index = sec.IndexOf("/");
                if (index != 0)
                {
                    sec = "/" + sec;
                }

                index = sec.IndexOf("//");

                if (index != 0)
                {
                    sec = mktListServiceName + sec;
                }
                d_subscriptions.Add(new Subscription(sec, new CorrelationID(security)));	
            }

            return true;
        }
    }
}
