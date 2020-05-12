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
using EventHandler = Bloomberglp.Blpapi.EventHandler;
using Element = Bloomberglp.Blpapi.Element;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Session = Bloomberglp.Blpapi.Session;

using ArrayList = System.Collections.ArrayList;
using Thread = System.Threading.Thread;
using System.Collections.Generic;
using System.IO;
using System;

namespace Bloomberglp.Blpapi.Examples
{
    public class SubscriptionWithEventHandlerExample
    {
        private const string MKTDATA_SVC = "//blp/mktdata";
        private const string AUTH_SVC = "//blp/apiauth";

        private static readonly Name EXCEPTIONS = Name.GetName("exceptions");
        private static readonly Name FIELD_ID = Name.GetName("fieldId");
        private static readonly Name REASON = Name.GetName("reason");
        private static readonly Name ERROR_CODE = Name.GetName("errorCode");
        private static readonly Name CATEGORY = Name.GetName("category");
        private static readonly Name DESCRIPTION = Name.GetName("description");
        private static readonly Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
        private static readonly Name AUTHORIZATION_FAILURE = Name.GetName("AuthorizationFailure");
        private static readonly Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");
        private static readonly Name TOKEN_FAILURE = Name.GetName("TokenGenerationFailure");
        private static readonly Name eventTypeName = Name.GetName("MKTDATA_EVENT_TYPE");
        private static readonly Name eventSubTypeName = Name.GetName("MKTDATA_EVENT_SUBTYPE");

        private List<string> d_hosts;
        private int d_port;
        private string d_authOption;
        private string d_dsName;
        private string d_name;
        private string d_token;
        private Identity d_identity;
        private SessionOptions d_sessionOptions;
        private Session d_session;
        private List<string> d_securities;
        private List<string> d_fields;
        private List<string> d_options;
        private List<Subscription> d_subscriptions;
        private string d_service;

        public static void Main(string[] args)
        {
            System.Console.WriteLine("Realtime Event Handler Example");
            SubscriptionWithEventHandlerExample example = new SubscriptionWithEventHandlerExample();
            example.run(args);
            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        public SubscriptionWithEventHandlerExample()
        {
            d_hosts = new List<string>();
            d_port = 8194;
            d_sessionOptions = new SessionOptions();
            d_securities = new List<string>();
            d_fields = new List<string>();
            d_options = new List<string>();
            d_subscriptions = new List<Subscription>();
            d_service = string.Empty;
            d_name = "";
            d_dsName = "";
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
                index++;
            }

            // auto restart on disconnect
            d_sessionOptions.ServerAddresses = servers;
            d_sessionOptions.AutoRestartOnDisconnection = true;
            d_sessionOptions.NumStartAttempts = d_hosts.Count;
            // change default subscription service
            if (d_service.Length > 0)
            {
                d_sessionOptions.DefaultSubscriptionService = d_service;
            }

            d_session = new Session(d_sessionOptions, processEvent);
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
            if (!createSession())
            {
                System.Console.WriteLine("Fail to open session");
                return;
            }

            if (d_authOption != "NONE")
            {
                // Authenticate user using Generate Token Request 
                if (!GenerateToken(out d_token)) return;

                //Authorization : pass Token into authorization request. Returns User handle with user's entitlements info set by server.
                if (!IsBPipeAuthorized(d_token, out d_identity)) return;
            }

            if (!d_session.OpenService(MKTDATA_SVC))
            {
                System.Console.Error.WriteLine("Failed to open " + MKTDATA_SVC);
                return;
            }

            System.Console.WriteLine("Subscribing...\n");
            if (d_authOption == "NONE")
            {
                d_session.Subscribe(d_subscriptions);
            }
            else
            {
                // subscribe with Identity
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
                    string category = string.Empty;
                    string description = string.Empty;
                    if (reason.HasElement(ERROR_CODE, true))
                    {
                        if (reason.HasElement(CATEGORY))
                        {
                            category = reason.GetElement(CATEGORY).GetValueAsString();
                        }
                        if (reason.HasElement(DESCRIPTION))
                        {
                            description = reason.GetElement(DESCRIPTION).GetValueAsString();
                        }
                        System.Console.WriteLine("\t" +
                                category + ": " + description);
                    }
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
                    + ": " + topic + " - " + msg.MessageType);

                foreach (Element field in msg.Elements)
                {
                    if (field.IsNull)
                    {
                        System.Console.WriteLine("\t\t" + field.Name + " is NULL");
                        continue;
                    }

                    processElement(field);
                }
            }
        }

        private void processElement(Element element)
        {
            if (element.IsArray)
            {
                System.Console.WriteLine("\t\t" + element.Name);
                // process array
                int numOfValues = element.NumValues;
                for (int i = 0; i < numOfValues; ++i)
                {
                    // process array data
                    processElement(element.GetValueAsElement(i));
                }
            }
            else if (element.NumElements > 0)
            {
                System.Console.WriteLine("\t\t" + element.Name);
                int numOfElements = element.NumElements;
                for (int i = 0; i < numOfElements; ++i)
                {
                    // process child elements
                    processElement(element.GetElement(i));
                }
            }
            else
            {
                // Assume all values are scalar.
                System.Console.WriteLine("\t\t" + element.Name
                    + " = " + element.GetValueAsString());
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

        private bool readFile(string fileName, ref List<string> dataList)
        {
            bool status = false;
            string line = string.Empty;
            try
            {
                StreamReader file = new StreamReader(fileName);
                while ((line = file.ReadLine()) != null)
                {
                    dataList.Add(line.Trim());
                }
                file.Close();
                status = true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error: " + ex.Message);
            }

            return status;
        }

        private bool parseCommandLine(string[] args)
        {
            string secFileName = string.Empty;
            string fldFileName = string.Empty;

            for (int i = 0; i < args.Length; ++i)
            {
                if (string.Compare(args[i], "-s", true) == 0
					&& i + 1 < args.Length)
                {
                    d_securities.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-f", true) == 0
					&& i + 1 < args.Length)
                {
                    d_fields.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-o", true) == 0
					&& i + 1 < args.Length)
                {
                    d_options.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-service", true) == 0
					&& i + 1 < args.Length)
                {
                    d_service = args[++i];
                }
                else if (string.Compare(args[i], "-sFile", true) == 0
					&& i + 1 < args.Length)
                {
                    secFileName = args[++i];
                }
                else if (string.Compare(args[i], "-fFile", true) == 0
					&& i + 1 < args.Length)
                {
                    fldFileName = args[++i];
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
            // check for directory service and application names
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

            if (fldFileName.Length > 0)
            {
                if (!readFile(fldFileName, ref d_fields))
                {
                    System.Console.WriteLine("Unable to read file: " + fldFileName);
                }
            }

            if (d_fields.Count == 0)
            {
                d_fields.Add("LAST_PRICE");
                d_fields.Add("TIME");
            }

            if (secFileName.Length > 0)
            {
                if (!readFile(secFileName, ref d_securities))
                {
                    System.Console.WriteLine("Unable to read file: " + secFileName);
                }
            }

            if (d_securities.Count == 0)
            {
                d_securities.Add("IBM US Equity");
            }

            foreach (string security in d_securities)
            {
                d_subscriptions.Add(new Subscription(
                    security, d_fields, d_options, new CorrelationID(security)));
            }
            return true;
        }

        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("  Retrieve realtime data");
            System.Console.WriteLine("      [-s     <security   = IBM US Equity>");
            System.Console.WriteLine("      [-f     <field      = LAST_PRICE>");
            System.Console.WriteLine("      [-o     <subscriptionOptions>");
            System.Console.WriteLine("      [-ip    <ipAddress  = localhost>");
            System.Console.WriteLine("      [-p     <tcpPort    = 8194>");
            System.Console.WriteLine("      [-sFile <security list file>");
            System.Console.WriteLine("      [-fFile <field list file>");
            System.Console.WriteLine("      [-auth  <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>]");
            System.Console.WriteLine("      [-n     <name = applicationName>]");
            System.Console.WriteLine("      [-ds    <name = directoryService>]");
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine("1) Specify only LOGON to authorize 'user' using Windows login name.");
            System.Console.WriteLine("2) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
            System.Console.WriteLine("3) Specify APPLICATION and name(Application Name) to authorize application.\n");
            System.Console.WriteLine("Press ENTER to quit");
        }
    }
}