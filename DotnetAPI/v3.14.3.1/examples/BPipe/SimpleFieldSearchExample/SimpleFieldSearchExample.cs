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

using System;
using System.Collections.Generic;
using Event = Bloomberglp.Blpapi.Event;
using Element = Bloomberglp.Blpapi.Element;
using InvalidRequestException = Bloomberglp.Blpapi.InvalidRequestException;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;

namespace Bloomberglp.Blpapi.Examples
{
    public class SimpleFieldSearchExample
    {
        private const String APIFLDS_SVC = "//blp/apiflds";
        private const string AUTH_SVC = "//blp/apiauth";
        private const int ID_LEN = 13;
        private const int MNEMONIC_LEN = 36;
        private const int DESC_LEN = 40;
        private const String PADDING =
            "                                            ";
        private static readonly Name FIELD_ID = new Name("id");
        private static readonly Name FIELD_MNEMONIC = new Name("mnemonic");
        private static readonly Name FIELD_DATA = new Name("fieldData");
        private static readonly Name FIELD_DESC = new Name("description");
        private static readonly Name FIELD_INFO = new Name("fieldInfo");
        private static readonly Name FIELD_ERROR = new Name("fieldError");
        private static readonly Name FIELD_MSG = new Name("message");
        private static readonly Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
        private static readonly Name AUTHORIZATION_FAILURE = Name.GetName("AuthorizationFailure");
        private static readonly Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");
        private static readonly Name TOKEN_FAILURE = Name.GetName("TokenGenerationFailure");

        private List<string> d_hosts;
        private int d_port;
        private string d_authOption;
        private string d_dsName;
        private string d_name;
        private string d_token;
        private SessionOptions d_sessionOptions;
        private Session d_session;
        private Identity d_identity;

        public static void Main(String[] args)
        {
            SimpleFieldSearchExample example = new SimpleFieldSearchExample();
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
                
        private void run(String[] args)
        {
            d_port = 8194;
            d_hosts = new List<string>();
            d_dsName = "";
            d_name = "";

            if (!parseCommandLine(args))
            {
                return;
            }

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

            if (!d_session.OpenService(APIFLDS_SVC))
            {
                System.Console.Error.WriteLine("Failed to open " + APIFLDS_SVC);
                return;
            }

            Service fieldInfoService = d_session.GetService(APIFLDS_SVC);
            Request request = fieldInfoService.CreateRequest(
                "FieldSearchRequest");
            request.Set("searchSpec", "mutual fund");
            request.Set("returnFieldDocumentation", false);
			
            System.Console.WriteLine("Sending Request: " + request);
            if (d_authOption == "NONE")
            {
                d_session.SendRequest(request, null);
            }
            else
            {
                // request data with identity object
                d_session.SendRequest(request, d_identity, null);
            }

            while (true)
            {
                try
                {
                    Event eventObj = d_session.NextEvent();
                    foreach (Message msg in eventObj)
                    {
                        if (eventObj.Type != Event.EventType.RESPONSE &&
                            eventObj.Type != Event.EventType.PARTIAL_RESPONSE)
                        {
                            continue;
                        }

                        Element fields = msg.GetElement(FIELD_DATA);
                        int numElements = fields.NumValues;

                        printHeader();
                        for (int i = 0; i < numElements; i++)
                        {
                            printField(fields.GetValueAsElement(i));
                        }
                        System.Console.WriteLine();
                    }
                    if (eventObj.Type == Event.EventType.RESPONSE) break;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Got Exception:" + ex);
                }
            }
        }
        private void printField(Element field)
        {
            String fldId, fldMnemonic, fldDesc;

            fldId = field.GetElementAsString(FIELD_ID);
            if (field.HasElement(FIELD_INFO))
            {
                Element fldInfo = field.GetElement(FIELD_INFO);
                fldMnemonic = fldInfo.GetElementAsString(FIELD_MNEMONIC);
                fldDesc = fldInfo.GetElementAsString(FIELD_DESC);

                System.Console.WriteLine(padString(fldId, ID_LEN) +
                                    padString(fldMnemonic, MNEMONIC_LEN) +
                                    padString(fldDesc, DESC_LEN));
            }
            else
            {
                Element fldError = field.GetElement(FIELD_ERROR);
                fldDesc = fldError.GetElementAsString(FIELD_MSG);

                System.Console.WriteLine("\n ERROR: " + fldId + " - " + fldDesc);
            }
        }

        private void printHeader()
        {
            System.Console.WriteLine(padString("FIELD ID", ID_LEN) +
                                      padString("MNEMONIC", MNEMONIC_LEN) +
                                      padString("DESCRIPTION", DESC_LEN));
            System.Console.WriteLine(padString("-----------", ID_LEN) +
                                      padString("-----------", MNEMONIC_LEN) +
                                      padString("-----------", DESC_LEN));
        }

        private static String padString(String str, int width)
        {
            if (str.Length >= width || str.Length >= PADDING.Length) return str;
            else return str + PADDING.Substring(0, width - str.Length);
        }

        private bool parseCommandLine(String[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (string.Compare(args[i], "-ip", true) == 0
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
                    return (false);
                }
            }

            // handle default arguments
            if (d_hosts.Count == 0)
            {
                System.Console.WriteLine("Missing host IP");
                printUsage();
                return false;
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
            return (true);
        }

        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("	Retrieve field search information");
            System.Console.WriteLine("      [-ip    <ipAddress  = localhost>");
            System.Console.WriteLine("      [-p     <tcpPort	= 8194>");
            System.Console.WriteLine("      [-auth  <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>]");
            System.Console.WriteLine("      [-n     <name = applicationName>]");
            System.Console.WriteLine("      [-ds    <name = directoryService>]");
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine("1) Specify only LOGON to authorize 'user' using Windows login name.");
            System.Console.WriteLine("2) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
            System.Console.WriteLine("3) Specify APPLICATION and name(Application Name) to authorize application.\n");
        }
    }
}
