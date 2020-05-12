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
/*
** PageDataExample.cs
**
** This Example shows how to retrieve page based data using V3 API
** Usage: 
**      -t			<Topic  	= "0708/012/0001">
**               i.e."Broker ID/Category/Page Number"
**      -ip 		<ipAddress	= localhost>
**      -p 			<tcpPort	= 8194>
**      -auth  <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>
**      -n     <name = applicationName>
**      -ds    <name = directoryService>
** Notes:
** 1) Specify only LOGON to authorize 'user' using Windows login name.
** 2) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.
** 3) Specify APPLICATION and name(Application Name) to authorize application.
** e.g. PageDataExample -t "0708/012/0001" -ip localhost -p 8194
*/

using System.Collections.Generic;
using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using Datatype = Bloomberglp.Blpapi.Schema.Datatype;
using ArrayList = System.Collections.ArrayList;
using Hashtable = System.Collections.Hashtable;
using System.Text;

namespace Bloomberglp.Blpapi.Examples
{
    public class PageDataExample
    {
        private const string AUTH_SVC = "//blp/apiauth";
        private const string PAGEDATA_SVC = "//blp/pagedata";

        private static readonly Name EXCEPTIONS = new Name("exceptions");
        private static readonly Name FIELD_ID = new Name("fieldId");
        private static readonly Name REASON = new Name("reason");
        private static readonly Name CATEGORY = new Name("category");
        private static readonly Name DESCRIPTION = new Name("description");
        private static readonly Name PAGEUPDATE = new Name("PageUpdate");
        private static readonly Name ROWUPDATE = new Name("rowUpdate");
        private static readonly Name NUMROWS = new Name("numRows");
        private static readonly Name NUMCOLS = new Name("numCols");
        private static readonly Name ROWNUM = new Name("rowNum");
        private static readonly Name SPANUPDATE = new Name("spanUpdate");
        private static readonly Name STARTCOL = new Name("startCol");
        private static readonly Name LENGTH = new Name("length");
        private static readonly Name TEXT = new Name("text");
        private static readonly Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
        private static readonly Name AUTHORIZATION_FAILURE = Name.GetName("AuthorizationFailure");
        private static readonly Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");
        private static readonly Name TOKEN_FAILURE = Name.GetName("TokenGenerationFailure");

        private ArrayList   d_hosts;
        private int         d_port;
        private string      d_authOption;
        private string      d_dsName;
        private string      d_name;
        private string      d_token;
        private SessionOptions d_sessionOptions;
        private Session     d_session;
        private Identity    d_identity;
        private ArrayList   d_topics;
        private Hashtable   d_topicTable; 

        
        public static void Main(string[] args)
        {
            PageDataExample example = new PageDataExample();
            example.run(args);
            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PageDataExample()
        {
            d_hosts     = new ArrayList();
            d_port      = 8194;
            d_topics    = new ArrayList();
            d_topicTable = new Hashtable();
            d_dsName = "";
            d_name = "";
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

        /// <summary>
        /// Read command line arguments, 
        /// Establish a Session
        /// Identify and Open refdata Service
        /// Send ReferenceDataRequest to the Service 
        /// Event Loop and Response Processing
        /// </summary>
        /// <param name="args"></param>
        private void run(string[] args)
        {
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
            }

            if (!d_session.OpenService(PAGEDATA_SVC))
            {
                System.Console.Error.WriteLine("Failed to open " + PAGEDATA_SVC);
                return;
            }

            subscribe(d_session);

            // wait for events from session.
            eventLoop(d_session);

        }

        /// <summary>
        /// Function to create subscription list and subscribe to pagedata
        /// </summary>
        /// <param name="session"></param>
        private void subscribe(Session session)
        {
            System.Collections.Generic.List<Subscription> subscriptions
                = new System.Collections.Generic.List<Subscription>();
            d_topicTable.Clear();

            List<string> fields = new List<string>();
            fields.Add("6-23");
            // Following commented code shows some of the sample values 
            // that can be used for field other than above
            // e.g. fields.Add("1");
            //      fields.Add("1,2,3");
            //      fields.Add("1,6-10,15,16");

            foreach (string topic in d_topics)
            {

                subscriptions.Add(new Subscription(PAGEDATA_SVC  + "/" + topic,
                                                    fields,
                                                    null,
                                                    new CorrelationID(topic)));
                d_topicTable.Add(topic, new ArrayList());
            }
            if (d_authOption == "NONE")
            {
                session.Subscribe(subscriptions);
            }
            else
            {
                session.Subscribe(subscriptions, d_identity);
            }

        }

        /// <summary>
        /// Parses the command line arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool parseCommandLine(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (string.Compare(args[i], "-t", true) == 0
					&& i + 1 < args.Length)
                {
                    d_topics.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-ip", true) == 0
					&& i + 1 < args.Length)
                {
                    d_hosts.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-p", true) == 0
					&& i + 1 < args.Length)
                {
                    d_port = int.Parse(args[++i]);
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

            // check for host ip
            if (d_hosts.Count == 0)
            {
                System.Console.WriteLine("Missing host IP");
                printUsage();
                return false;
            }

            // set default topics if nothing is specified
            if (d_topics.Count == 0)
            {
                d_topics.Add("0708/012/0001");
                d_topics.Add("1102/1/274");
            }

            return true;
        }

        /// <summary>
        /// Print usage of the Program
        /// </summary>
        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("	Retrieve page based data using V3 API");
            System.Console.WriteLine("      [-t			<Topic  	= \"0708/012/0001\">]");
            System.Console.WriteLine("             i.e.\"Broker ID/Category/Page Number\"");
            System.Console.WriteLine("      [-ip        <ipAddress	= localhost>");
            System.Console.WriteLine("      [-p         <tcpPort	= 8194>");
            System.Console.WriteLine("      [-auth  <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>]");
            System.Console.WriteLine("      [-n     <name = applicationName>]");
            System.Console.WriteLine("      [-ds    <name = directoryService>]");
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine("1) Specify only LOGON to authorize 'user' using Windows login name.");
            System.Console.WriteLine("2) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
            System.Console.WriteLine("3) Specify APPLICATION and name(Application Name) to authorize application.");
            System.Console.WriteLine("e.g. PageDataExample -t \"0708/012/0001\" -ip localhost -p 8194\n");
        }
        /// <summary>
        /// Polls for an event or a message in an event loop
        /// & Processes the event generated
        /// </summary>
        /// <param name="session"></param>
        private void eventLoop(Session session)
        {
            while (true)
            {
                Event eventObj = session.NextEvent();
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
        }

        /// <summary>
        /// Process SubscriptionStatus event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processSubscriptionStatus(Event eventObj, Session session)
        {
            System.Console.WriteLine("Processing SUBSCRIPTION_STATUS");
            foreach (Message msg in eventObj.GetMessages())
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
                    for (int i = 0; i < exceptions.NumValues; ++i)
                    {
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

        /// <summary>
        /// Process SubscriptionData event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processSubscriptionDataEvent(Event eventObj, Session session)
        {
            System.Console.WriteLine("Processing SUBSCRIPTION_DATA");
            foreach (Message msg in eventObj.GetMessages())
            {
                string topic = (string)msg.CorrelationID.Object;
                System.Console.WriteLine(System.DateTime.Now.ToString("s") +
                    ": " + topic + " - " + msg.MessageType);
                //System.Console.WriteLine(msg.AsElement);
                if(msg.MessageType.Equals("PageUpdate")){
                    processPageElement(msg.AsElement, topic);
                }else if(msg.MessageType.Equals("RowUpdate")){
                    processRowElement(msg.AsElement, topic);
                }

                //showUpdatedPage(topic);
            }
        }

        /// <summary>
        /// Show whole page content for specific topic
        /// </summary>
        /// <param name="topic"></param>
        private void showUpdatedPage(string topic)
        {
            ArrayList rowList = (ArrayList)d_topicTable[topic];
            foreach (StringBuilder str in rowList)
            {
                System.Console.WriteLine(str.ToString());
            }          
        }

        /// <summary>
        /// Process PageUpdate Event/PageUpdate Element
        /// </summary>
        /// <param name="pageElement"></param>
        /// <param name="topic"></param>
        private void processPageElement(Element pageElement, string topic)
        {
            Element eleNumRows = pageElement.GetElement(NUMROWS);
            int numRows = eleNumRows.GetValueAsInt32();
            Element eleNumCols = pageElement.GetElement(NUMCOLS);
            int numCols = eleNumCols.GetValueAsInt32();
            System.Console.WriteLine("Page Contains " + numRows + " Rows & " + numCols + " Columns");
            Element eleRowUpdates = pageElement.GetElement(ROWUPDATE);
            int numRowUpdates = eleRowUpdates.NumValues;
            System.Console.WriteLine("Processing " + numRowUpdates + " RowUpdates");
            for (int i = 0; i < numRowUpdates; ++i)
            {
                Element rowUpdate = eleRowUpdates.GetValueAsElement(i);
                processRowElement(rowUpdate, topic);
            }
        }

        /// <summary>
        /// Process RowUpdate Event/ rowUpdate Element
        /// </summary>
        /// <param name="rowElement"></param>
        /// <param name="topic"></param>
        private void processRowElement(Element rowElement, string topic)
        {
            Element eleRowNum = rowElement.GetElement(ROWNUM);
            int rowNum = eleRowNum.GetValueAsInt32();
            Element eleSpanUpdates = rowElement.GetElement(SPANUPDATE);
            int numSpanUpdates = eleSpanUpdates.NumValues;
            //System.Console.WriteLine("Processing " + numSpanUpdates + " spanUpdate");
            for (int i = 0; i < numSpanUpdates; ++i)
            {
                Element spanUpdate = eleSpanUpdates.GetValueAsElement(i);
                processSpanElement(spanUpdate, rowNum, topic);
            } 
        }

        /// <summary>
        /// <summary>
        /// Process spanUpdate Element
        /// </summary>
        /// <param name="spanElement"></param>
        /// <param name="rowNum"></param>
        /// <param name="topic"></param>
        private void processSpanElement(Element spanElement, int rowNum, string topic)
        {
            Element eleStartCol = spanElement.GetElement(STARTCOL);
            int startCol = eleStartCol.GetValueAsInt32();
            Element eleLength = spanElement.GetElement(LENGTH);
            int len = eleLength.GetValueAsInt32();
            Element eleText = spanElement.GetElement(TEXT);
            string text = eleText.GetValueAsString();
            System.Console.WriteLine("Row : " + rowNum + 
                                     ",Col: " + startCol + 
                                     "(Len: " + len + ")" + 
                                     "\tNew Text: " + text);
            ArrayList rowList = (ArrayList) d_topicTable[topic];
            while (rowList.Count < rowNum)
            {
                rowList.Add(new StringBuilder());
            }
            StringBuilder rowText = (StringBuilder)rowList[rowNum - 1];
            if (rowText.Length == 0)
            {
                rowText.Append(text.PadRight(80));
            }
            else
            {
                string strToReplace = rowText.ToString().Substring(startCol - 1, len);
                rowText.Replace(strToReplace, text, startCol - 1, len);
                System.Console.WriteLine(rowText.ToString());
            }
        }

        /// <summary>
        /// Process events other than subscription data/status event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processMiscEvents(Event eventObj, Session session)
        {
            System.Console.WriteLine("Processing " + eventObj.Type);
            foreach (Message msg in eventObj.GetMessages())
            {
                System.Console.WriteLine(System.DateTime.Now.ToString("s") +
                    ": " + msg.MessageType + "\n");
            }
        }
    }
}
