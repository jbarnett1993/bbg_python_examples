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
 
// This program requests a 'token' and displays it on the console.
// Refer to the following examples that accept a 'token' on the command line
// and use it to authorize users:
//      ServerMode_EntitleVerificationTokenExample
//      ServerMode_EntitleVerificationSubscriptionTokenExample
//
// By default this program will generate a 'token' based on the current
// logged in user. The "-d" option can be used to specify a property to look up
// via active directory services. For example, "-d mail" would look up the 
// value for the property "mail" which could be the email address of the user.
//
// Workflow:
// * set options based on what information will be used to generate the 'token'
// * connect to server
// * call generateToken to request a 'token'
// * look for "TOKEN_STATUS" events for success or failure.
//
// Command line arguments:
// -ip   <serverHostNameOrIp>
// -p    <serverPort>
// -auth <authenticationOption = LOGON or APPLICATION or DIRSVC>
// -n    <name = applicationName or directoryService>
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using CorrelationID = Bloomberglp.Blpapi.CorrelationID;
using Event = Bloomberglp.Blpapi.Event;
using EventQueue = Bloomberglp.Blpapi.EventQueue;
using Element = Bloomberglp.Blpapi.Element;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using Identity = Bloomberglp.Blpapi.Identity;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;


namespace ServerMode_GetAuthorizationToken
{
    class ServerMode_GetAuthorizationToken
    {
        private Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
		private Name AUTHORIZATION_FAILURE = Name.GetName("AuthorizationFailure");
		private Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");
		private Name TOKEN_FAILURE = Name.GetName("TokenGenerationFailure");

		private String serverHost;
		private int serverPort;
        private String authOption;  // authentication option user/application
        private string dsName;      // DirectoryService
        private String name;        // ApplicationName
		private Session session;
		

        public ServerMode_GetAuthorizationToken()
		{
			serverHost = "localhost";
			serverPort = 8194;
            authOption = "LOGON";
            name = "";
            dsName = "";
			session = null;
		}

		public void Run(String[] args)
		{
			if (!ParseCommandLine(args)) return;


			session = CreateSession();

			try
			{
				if (!session.Start())
				{
					System.Console.WriteLine("Failed to start session.");
					return;
				}

				// Authenticate user using Generate Token Request 
				if(!GenerateToken()) return;

			}
			finally
			{
				session.Stop();
			}

		}

		public static void Main(String[] args)
		{
			System.Console.WriteLine("GenerateToken");
            ServerMode_GetAuthorizationToken example = new ServerMode_GetAuthorizationToken();
			example.Run(args);
			System.Console.WriteLine("Press ENTER to quit");
			System.Console.Read();
		}

		#region private Helper member

		private Session CreateSession()
		{
            String authOptions = string.Empty;

			SessionOptions sessionOptions = new SessionOptions();
			sessionOptions.ServerHost = serverHost;
			sessionOptions.ServerPort = serverPort;

            if (authOption == "APPLICATION")
			{
                // Set Application Authentication Option
                authOptions = "AuthenticationMode=APPLICATION_ONLY;";
                authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
                // ApplicationName is the entry in EMRS.
                authOptions += "ApplicationName=" + name;
            }
            else if (authOption == "USER_APP")
            {
                // Set User and Application Authentication Option
                authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
                authOptions += "AuthenticationType=OS_LOGON;";
                authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
                // ApplicationName is the entry in EMRS.
                authOptions += "ApplicationName=" + name;
            }
            else if (authOption == "USER_DS_APP")
            {
                // Set User and Application Authentication Option
                authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
                authOptions += "AuthenticationType=DIRECTORY_SERVICE;";
                authOptions += "DirSvcPropertyName=" + dsName + ";";
                authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
                // ApplicationName is the entry in EMRS.
                authOptions += "ApplicationName=" + name;
            }
            else if (authOption == "DIRSVC")
            {
                // Authenticate user using active directory service property
                authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
                authOptions += "DirSvcPropertyName=" + dsName;
            }
            else // default to "LOGON"
            {
                // Authenticate user using windows/unix login name
                authOptions = "AuthenticationType=OS_LOGON";
            }

			System.Console.WriteLine("Authentication Options = " + authOptions);
			sessionOptions.AuthenticationOptions = authOptions;
			System.Console.WriteLine("Connecting to " + serverHost + ":" + serverPort);	

			session = new Session(sessionOptions);
			return session;
		}

		private bool GenerateToken()
		{
			bool isTokenSuccess = false;
			bool isRunning = false;

			CorrelationID tokenReqId = new CorrelationID(99);
			EventQueue tokenEventQueue = new EventQueue();

			session.GenerateToken(tokenReqId, tokenEventQueue);

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
							isTokenSuccess = true;
							isRunning = true;
							break;
						}
						else if (msg.MessageType == TOKEN_FAILURE)
						{
							Console.WriteLine("Received : " + TOKEN_FAILURE.ToString());
							isRunning = true;
							break;
						}
						else
						{
							Console.WriteLine("Error while Token Generation");
							isRunning = true;
							break;
						}
					}
				}
			}

			return isTokenSuccess;
		}

	
		private void PrintUsage()
		{
			System.Console.WriteLine("Usage:");
			System.Console.WriteLine("	Generate a token for authorization ");
			System.Console.WriteLine("		[-ip 		<ipAddress	= localhost>]");
			System.Console.WriteLine("		[-p 		<tcpPort	= 8194>]");
            System.Console.WriteLine("      [-auth  <authenticationOption = LOGON (default) or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>]");
            System.Console.WriteLine("      [-n     <name = applicationName>]");
            System.Console.WriteLine("      [-ds    <name = directoryService>]");
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine(" -Specify only LOGON to authorize 'user' using Windows login name.");
            System.Console.WriteLine(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
            System.Console.WriteLine(" -Specify APPLICATION and name(Application Name) to authorize application.");
        }

		private bool ParseCommandLine(String[] args)
		{
			for (int i = 0; i < args.Length; ++i)
			{
				if (string.Compare("-ip", args[i], true) == 0
					&& i + 1 < args.Length)
				{
					serverHost = args[++i];
				}
				else if (string.Compare("-p", args[i], true) == 0
					&& i + 1 < args.Length)
				{
                    int outPort = 0;
                    if (int.TryParse(args[++i], out outPort))
                    {
                        serverPort = outPort;
                    }
				}
				else if (string.Compare("-auth", args[i], true) == 0
					&& i + 1 < args.Length)
				{
					authOption = args[++i].Trim();
				}
                else if (string.Compare("-ds", args[i], true) == 0
					&& i + 1 < args.Length)
                {
                    dsName = args[i + 1].Trim();
                }
                else if (string.Compare("-n", args[i], true) == 0
					&& i + 1 < args.Length)
                {
                    name = args[++i].Trim();
                }
                else if (string.Compare(args[i], "-h", true) == 0)
				{
					PrintUsage();
					return false;
				}
			}

            // check for application name
		    if ((authOption == "APPLICATION") && (name == "")){
			     System.Console.WriteLine("Application name cannot be NULL for application authorization.");
			     PrintUsage();
                 return false;
		    }
            if (authOption == "USER_DS_APP" && (name == "" || dsName == ""))
            {
                System.Console.WriteLine("Application or DS name cannot be NULL for application authorization.");
                PrintUsage();
                return false;
            }
            // check for Directory Service name
		    if ((authOption == "DIRSVC") && (dsName == "")){
			     System.Console.WriteLine("Directory Service property name cannot be NULL for DIRSVC authorization.");
			     PrintUsage();
                 return false;
		    }

			return true;
		}

		#endregion
    }
}
