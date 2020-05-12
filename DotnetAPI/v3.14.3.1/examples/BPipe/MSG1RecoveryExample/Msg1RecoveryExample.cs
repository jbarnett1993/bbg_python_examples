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
 * 
 * All materials including all software, equipment and documentation made 
 * available by Bloomberg are for informational purposes only. Bloomberg and its 
 * affiliates make no guarantee as to the adequacy, correctness or completeness 
 * of, and do not make any representation or warranty (whether express or 
 * implied) or accept any liability with respect to, these materials. No right, 
 * title or interest is granted in or to these materials and you agree at all 
 * times to treat these materials in a confidential manner. All materials and 
 * services provided to you by Bloomberg are governed by the terms of any 
 * applicable Bloomberg Agreement(s).
 */

using System;
using ArrayList = System.Collections.ArrayList;
using System.Text;
using Event = Bloomberglp.Blpapi.Event;
using Element = Bloomberglp.Blpapi.Element;
using InvalidRequestException = Bloomberglp.Blpapi.InvalidRequestException;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;

namespace Bloomberglp.Blpapi.Examples
{
    public class Msg1RecoveryExample
    {
        private const string MSG1_SERVICE = "//blp/msgscrape";
        private const string AUTH_SVC = "//blp/apiauth";
        private const string REPLAY = "replay";
        private const string STATUS_INFO = "statusInfo";

        private static readonly Name ERROR_RESPONSE = Name.GetName("errorResponse");
        private static readonly Name REPLAY_RESPONSE = Name.GetName("replayResponse");
        private static readonly Name ERROR_MESSAGE = Name.GetName("errorMsg");
        private static readonly Name MARKET_DATAS = Name.GetName("marketDatas");
        private static readonly Name START = Name.GetName("start");
        private static readonly Name END = Name.GetName("end");
        private static readonly Name FILTER = Name.GetName("filter");
        private static readonly Name EID = Name.GetName("eid");
        private static readonly Name SERIAL = Name.GetName("serial");
        private static readonly Name TIME = Name.GetName("time");
        private static readonly Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
        private static readonly Name AUTHORIZATION_FAILURE = Name.GetName("AuthorizationFailure");
        private static readonly Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");
        private static readonly Name TOKEN_FAILURE = Name.GetName("TokenGenerationFailure");

        private enum FilterChoiceType
        {
            ALL,
            LAST_UPDATE_ONLY,
        }

        private ArrayList d_hosts;
        private int d_port;
        private string d_authOption;
        private string d_dsName;
        private string d_name;
        private string d_token;
        private Identity d_identity;
        private string d_requestType;
        private Name d_startType;
        private Name d_endType;
        private int d_startSerial;
        private int d_endSerial;
        private Datetime d_startTime;
        private Datetime d_endTime;
        private FilterChoiceType d_filter;
        private int d_eid;
        private bool d_eidProvided;

        private SessionOptions d_sessionOptions;
        private Session d_session;

        public static void Main(string[] args)
        {
            System.Console.WriteLine("Msg1 Recovery Example");
            Msg1RecoveryExample example = new Msg1RecoveryExample();
            example.run(args);

            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        public Msg1RecoveryExample()
        {
            d_port = 8194;
            d_hosts = new ArrayList();
            d_startType = SERIAL;
            d_startSerial = 0;
            d_endType = TIME;
            d_endTime = new Datetime(DateTime.Now);
            d_filter = FilterChoiceType.LAST_UPDATE_ONLY;
            d_eid = 0;
            d_eidProvided = false;
            d_requestType = STATUS_INFO;
            d_authOption = "LOGON";
            d_dsName = "";
            d_name = "";

        }

        private void run(string[] args)
        {
            if (!parseCommandLine(args)) return;

            // create session
            if (!createSession())
            {
                System.Console.Error.WriteLine("Failed to open session");
                return;
            }

            // Authenticate user using Generate Token Request 
            if (!GenerateToken(out d_token)) return;

            //Authorization : pass Token into authorization request. Returns User handle with user's entitlements info set by server.
            if (!IsBPipeAuthorized(d_token, out d_identity)) return;

            if (!d_session.OpenService(MSG1_SERVICE))
            {
                System.Console.Error.WriteLine("Failed to open " + MSG1_SERVICE);
                return;
            }

            try
            {
                if (d_requestType == STATUS_INFO)
                {
                    sendMSG1StatusRequest(d_session);
                }
                else if (d_requestType == REPLAY)
                {
                    sendMSG1RecoverRequest(d_session);
                }
            }
            catch (InvalidRequestException e)
            {
                System.Console.WriteLine(e.ToString());
            }

            // wait for events from session.
            eventLoop(d_session);

            d_session.Stop();
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

        private void eventLoop(Session session)
        {
            bool done = false;
            while (!done)
            {
                Event eventObj = session.NextEvent();
                if (eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    System.Console.WriteLine("Processing Partial Response");
                    processResponseEvent(eventObj);
                }
                else if (eventObj.Type == Event.EventType.RESPONSE)
                {
                    System.Console.WriteLine("Processing Response");
                    processResponseEvent(eventObj);
                    done = true;
                }
                else
                {
                    foreach (Message msg in eventObj)
                    {
                        System.Console.WriteLine(msg.AsElement);
                        if (eventObj.Type == Event.EventType.SESSION_STATUS)
                        {
                            if (msg.MessageType.Equals("SessionTerminated"))
                            {
                                done = true;
                            }
                        }
                    }
                }
            }
        }

        void sendMSG1StatusRequest(Session session)
        {
            Service service = session.GetService(MSG1_SERVICE);
            Identity id = session.CreateIdentity();
            Request request = service.CreateRequest(STATUS_INFO);
            if (d_eidProvided)
            {
                request.GetElement(EID).SetValue(d_eid);
            }
            System.Console.WriteLine("Sending request: " + request.ToString());

            // request data with identity object
            session.SendRequest(request, d_identity, null);
        }

        void sendMSG1RecoverRequest(Session session)
        {
            Service service = session.GetService(MSG1_SERVICE);
            Identity id = session.CreateIdentity();

            Request request = service.CreateRequest(REPLAY);
            request.GetElement(START).SetChoice(d_startType);
            if (d_startType == TIME)
            {
                request.GetElement(START).SetChoice(TIME).SetValue(d_startTime);
            }
            else if (d_startType == SERIAL)
            {
                request.GetElement(START).SetChoice(SERIAL).SetValue(d_startSerial);
            }
            if (d_endType == TIME)
            {
                request.GetElement(END).SetChoice(TIME).SetValue(d_endTime);
            }
            else if (d_endType == SERIAL)
            {
                request.GetElement(END).SetChoice(SERIAL).SetValue(d_endSerial);
            }
            if (d_eidProvided)
            {
                request.GetElement(EID).SetValue(d_eid);
            }
            request.GetElement(FILTER).SetValue(Enum.GetName(d_filter.GetType(), d_filter));
            System.Console.WriteLine("Sending request: " + request.ToString());

            // request data with identity object
            session.SendRequest(request, d_identity, null);
        }

        private void processResponseEvent(Event eventObj)
        {
            foreach (Message msg in eventObj)
            {
                if (msg.AsElement.Name.Equals(ERROR_RESPONSE))
                {
                    printErrorInfo("REQUEST FAILED: ", msg.AsElement);
                    continue;
                }
                else if (msg.AsElement.Name.Equals(REPLAY_RESPONSE))
                {
                    System.Console.WriteLine("# of Recovered data: " + msg.GetElement(MARKET_DATAS).NumValues);
                    continue;
                }
                else
                {
                    System.Console.WriteLine("Received Response: " + msg.ToString());
                    continue;
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
                    d_requestType = REPLAY;
                    int startSerial = 0;
                    string startArg = args[++i];
                    if (int.TryParse(startArg, out startSerial))
                    {
                        d_startType = SERIAL;
                        d_startSerial = startSerial;
                    }
                    else
                    {
                        try
                        {
                            d_startTime = new Datetime(Convert.ToDateTime(startArg));
                            d_startType = TIME;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Error: '{0}' is not in the proper Datetime format.", startArg);
                            printUsage();
                            return false;
                        }
                    }
                }
                else if (string.Compare(args[i], "-e", true) == 0
                    && i + 1 < args.Length)
                {
                    d_requestType = REPLAY;
                    int endSerial = 0;
                    string endArg = args[++i];
                    if (int.TryParse(endArg, out endSerial))
                    {
                        d_endType = SERIAL;
                        d_endSerial = endSerial;
                    }
                    else
                    {
                        try
                        {
                            d_endTime = new Datetime(Convert.ToDateTime(endArg));
                            d_endType = TIME;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Error: '{0}' is not in the proper Datetime format.", endArg);
                            printUsage();
                            return false;
                        }
                    }
                }
                else if (string.Compare(args[i], "-f", true) == 0
                    && i + 1 < args.Length)
                {
                    d_requestType = REPLAY;
                    string filter = args[++i];
                    try
                    {
                        d_filter = (FilterChoiceType)Enum.Parse(d_filter.GetType(), filter, true);
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Error: '{0}' is not a supported filter type.", filter);
                        printUsage();
                        return false;
                    }
                }
                else if (string.Compare(args[i], "-eid") == 0
                    && i + 1 < args.Length)
                {
                    string eidArg = args[++i];
                    if (int.TryParse(eidArg, out d_eid))
                    {
                        d_eidProvided = true;
                    }
                    else
                    {
                        Console.WriteLine("Error: '{0}' is not an integer", eidArg);
                        printUsage();
                        return false;
                    }
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
                else
                {
                    Console.WriteLine("Error: unknown argument '{0}'", args[i]);
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
                d_hosts.Add("localhost");
            }

            return true;
        }

        private void printErrorInfo(string leadingStr, Element errorInfo)
        {
            System.Console.WriteLine(leadingStr + " (" + errorInfo.GetElementAsString(ERROR_MESSAGE) + ")");
        }

        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("	Retrieve MSG1 data ");
            System.Console.WriteLine("      [-s     <start	    = 0>");
            System.Console.WriteLine("      [-e     <end  	    = " + DateTime.Now + ">");
            System.Console.WriteLine("      [-eid   <eid        = an EID>");
            System.Console.WriteLine("      [-f     <filter	    = LAST_UPDATE_ONLY>");
            System.Console.WriteLine("      [-ip    <ipAddress	= localhost>");
            System.Console.WriteLine("      [-p     <tcpPort	= 8194>");
            System.Console.WriteLine("      [-auth  <authenticationOption = LOGON(default) or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>]");
            System.Console.WriteLine("      [-n     <name = applicationName>]");
            System.Console.WriteLine("      [-ds    <dsName = directoryService>]");
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine("1) This example client make a status infomation query by default.");
            System.Console.WriteLine("2) Specify start and end to request MSG1 recovory request.");
            System.Console.WriteLine("Notes on MSG1 recovery:");
            System.Console.WriteLine("1) Specify start as either a number (as serial id) or time (as timestamp).");
            System.Console.WriteLine("2) Specify end as either a number (as serial id) or time (as timestamp).");
            System.Console.WriteLine("3) Specify filter as 'ALL' or 'LAST_UPDATE_ONLY'.\n");
            System.Console.WriteLine("4) Sepcify the EID whose data needed to be recovered. This field is optional. It is only necessary when one B-PIPE client has multiple EIDs enabled and the EID specified is different from the EID mapped to the default proxy UUID.");
            System.Console.WriteLine("Notes on authorization:");
            System.Console.WriteLine("1) Specify only LOGON to authorize 'user' using Windows login name.");
            System.Console.WriteLine("2) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
            System.Console.WriteLine("3) Specify APPLICATION and name(Application Name) to authorize application.\n");
        }

    }
}
