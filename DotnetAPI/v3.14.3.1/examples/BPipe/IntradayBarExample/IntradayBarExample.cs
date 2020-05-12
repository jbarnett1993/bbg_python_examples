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

using ArrayList = System.Collections.ArrayList;

using DateTime = System.DateTime;
using DayOfWeek = System.DayOfWeek;
using Datetime = Bloomberglp.Blpapi.Datetime;
using Event = Bloomberglp.Blpapi.Event;
using Element = Bloomberglp.Blpapi.Element;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;

namespace Bloomberglp.Blpapi.Examples
{

	public class IntradayBarExample
	{
        private const string REFDATA_SVC = "//blp/refdata";
        private const string AUTH_SVC = "//blp/apiauth";

		private static readonly Name BAR_DATA       = Name.GetName("barData");
		private static readonly Name BAR_TICK_DATA  = Name.GetName("barTickData");
		private static readonly Name OPEN 	        = Name.GetName("open");
		private static readonly Name HIGH 	        = Name.GetName("high");
		private static readonly Name LOW 	        = Name.GetName("low");
		private static readonly Name CLOSE	        = Name.GetName("close");
		private static readonly Name VOLUME	        = Name.GetName("volume");
		private static readonly Name NUM_EVENTS     = Name.GetName("numEvents");
		private static readonly Name TIME	        = Name.GetName("time");
		private static readonly Name RESPONSE_ERROR = Name.GetName("responseError");
        private static readonly Name CATEGORY       = Name.GetName("category");
		private static readonly Name MESSAGE        = Name.GetName("message");
        private static readonly Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
        private static readonly Name AUTHORIZATION_FAILURE = Name.GetName("AuthorizationFailure");
        private static readonly Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");
        private static readonly Name TOKEN_FAILURE = Name.GetName("TokenGenerationFailure");

        private ArrayList               d_hosts;
		private int						d_port;
        private string                  d_authOption;
        private string                  d_dsName;
        private string                  d_name;
        private string                  d_token;
        private SessionOptions          d_sessionOptions;
        private Session                 d_session;
        private Identity                d_identity;
        private string                  d_security;
		private string                  d_eventType;
		private int						d_barInterval;
		private bool					d_gapFillInitialBar;
		private string                  d_startDateTime;
		private string                  d_endDateTime;

		public static void Main(string[] args)
		{
			System.Console.WriteLine("Intraday Bars Example");
			IntradayBarExample example = new IntradayBarExample();
			example.run(args);

			System.Console.WriteLine("Press ENTER to quit");
			System.Console.Read();
		}

        private DateTime getPreviousTradingDate()
        {
            DateTime tradedOn = DateTime.Now;
            tradedOn = tradedOn.AddDays(-1);
            if (tradedOn.DayOfWeek == DayOfWeek.Sunday)
            {
                tradedOn = tradedOn.AddDays(-2);
            }
            else if (tradedOn.DayOfWeek == DayOfWeek.Saturday)
            {
                tradedOn = tradedOn.AddDays(-1);
            }
            return tradedOn;
        }

		public IntradayBarExample()
		{
			d_port = 8194;
            d_hosts = new ArrayList();
            d_dsName = "";
            d_name = "";

            d_barInterval = 60;
			d_security = "IBM US Equity";
			d_eventType = "TRADE";
			d_gapFillInitialBar = false;
            DateTime prevTradedDate = getPreviousTradingDate();
            d_startDateTime = prevTradedDate.Year.ToString() + "-" +
                              prevTradedDate.Month.ToString() + "-" +
                              prevTradedDate.Day.ToString() +
                              "T13:30:00";
            prevTradedDate = prevTradedDate.AddDays(1); // next day for end date
            d_endDateTime = prevTradedDate.Year.ToString() + "-" +
                              prevTradedDate.Month.ToString() + "-" +
                              prevTradedDate.Day.ToString() +
                              "T13:30:00";        
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
                authOptions += "ApplicationName=" + d_dsName;
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

            // Authenticate user using Generate Token Request 
            if (!GenerateToken(out d_token)) return;

            //Authorization : pass Token into authorization request. Returns User handle with user's entitlements info set by server.
            if (!IsBPipeAuthorized(d_token, out d_identity)) return;

            // check if user is BPS user
            switch (d_identity.SeatType)
            {
                case SeatType.BPS:
                    // send request
                    sendIntradayBarRequest();
                    // wait for events from session.
                    eventLoop();
                    break;
                case SeatType.NONBPS:
                    System.Console.WriteLine("User must be a BPS user to get premium data.");
                    break;
                default:
                    System.Console.WriteLine("User is invalid.");
                    break;
            }

			d_session.Stop();
		}

		private void eventLoop()
		{
			bool done = false;
			while (!done) 
			{
				Event eventObj = d_session.NextEvent();
				if (eventObj.Type == Event.EventType.PARTIAL_RESPONSE) 
				{
					System.Console.WriteLine("Processing Partial Response");
					processResponseEvent(eventObj, d_session);
				}
				else if (eventObj.Type == Event.EventType.RESPONSE) 
				{
					System.Console.WriteLine("Processing Response");
					processResponseEvent(eventObj, d_session);
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

      private void processMessage(Message msg)
      {
          if (msg.HasElement(RESPONSE_ERROR))
          {
              // Intraday bar request exception
              Element reason = msg.GetElement(RESPONSE_ERROR);
              System.Console.WriteLine("\t" +
                      reason.GetElement(CATEGORY).GetValueAsString() +
                      ": " + reason.GetElement(MESSAGE).GetValueAsString());
          }
          else
          {
              Element data = msg.GetElement(BAR_DATA).GetElement(BAR_TICK_DATA);
              int numBars = data.NumValues;
              System.Console.WriteLine("Response contains " + numBars + " bars");
              System.Console.WriteLine("Datetime\t\tOpen\t\tHigh\t\tLow\t\tClose" +
                  "\t\tNumEvents\tVolume");
              for (int i = 0; i < numBars; ++i)
              {
                  Element bar = data.GetValueAsElement(i);
                  Datetime time = bar.GetElementAsDate(TIME);
                  double open = bar.GetElementAsFloat64(OPEN);
                  double high = bar.GetElementAsFloat64(HIGH);
                  double low = bar.GetElementAsFloat64(LOW);
                  double close = bar.GetElementAsFloat64(CLOSE);
                  int numEvents = bar.GetElementAsInt32(NUM_EVENTS);
                  long volume = bar.GetElementAsInt64(VOLUME);
                  System.DateTime sysDatetime = time.ToSystemDateTime();
                  System.Console.WriteLine(
                      sysDatetime.ToString("s") + "\t" +
                      open.ToString("C") + "\t\t" +
                      high.ToString("C") + "\t\t" +
                      low.ToString("C") + "\t\t" +
                      close.ToString("C") + "\t\t" +
                      numEvents + "\t\t" +
                      volume);
              }
          }
      }


		// return true if processing is completed, false otherwise
		private void processResponseEvent(Event eventObj, Session session)
		{
            foreach (Message msg in eventObj)
			{
                processMessage(msg); 
			}
		}

		private void sendIntradayBarRequest()
		{
            if (!d_session.OpenService(REFDATA_SVC))
            {
                System.Console.WriteLine("Failed to open service: " + REFDATA_SVC);
                return;
            }
            Service refDataService = d_session.GetService(REFDATA_SVC);
			Request request =  refDataService.CreateRequest("IntradayBarRequest");

			// only one security/eventType per request
			request.Set("security", d_security);
			request.Set("eventType", d_eventType);
			request.Set("interval", d_barInterval);

			request.Set("startDateTime", d_startDateTime);
			request.Set("endDateTime", d_endDateTime);

			if (d_gapFillInitialBar) 
			{
				request.Set("gapFillInitialBar", d_gapFillInitialBar);
			}

			System.Console.WriteLine("Sending Request: " + request);
			d_session.SendRequest(request, d_identity, null);
		}

		private bool parseCommandLine(string[] args)
		{
            bool flag = true;
            string dateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
			for (int i = 0; i < args.Length; ++i) 
			{
				if (string.Compare(args[i], "-s", true) == 0
					&& i + 1 < args.Length) 
				{
					d_security = args[++i];
				}
				else if (string.Compare(args[i],"-e", true) == 0
					&& i + 1 < args.Length) 
				{
					d_eventType = args[++i];
				}
				else if (string.Compare(args[i], "-ip", true) == 0
					&& i + 1 < args.Length) 
				{
					d_hosts.Add(args[++i]);
				}
				else if (string.Compare(args[i],"-p", true) == 0
					&& i + 1 < args.Length) 
				{
                    int outPort = 0;
                    if (int.TryParse(args[++i], out outPort))
                    {
                        d_port = outPort;
                    }
                }
				else if (string.Compare(args[i],"-b", true) == 0
					&& i + 1 < args.Length) 
				{
					d_barInterval = int.Parse(args[++i]);
				}
                else if (string.Compare(args[i], "-g", true) == 0)
                {
                    d_gapFillInitialBar = true;
                }
                else if (string.Compare(args[i], "-sd", true) == 0
					&& i + 1 < args.Length)
                {
                    d_startDateTime = args[++i];

                    if (!isDateTimeValid(d_startDateTime, dateTimeFormat))
                    {
                        flag = false;
                        System.Console.WriteLine("Invalid start date/time: " + d_startDateTime + ".");
                    }
                }
                else if (string.Compare(args[i], "-ed", true) == 0
					&& i + 1 < args.Length)
                {
                    d_endDateTime = args[++i];

                    if (!isDateTimeValid(d_endDateTime, dateTimeFormat))
                    {
                        flag = false;
                        System.Console.WriteLine("Invalid end date/time: " + d_endDateTime + ".");
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
                else 
                {
					printUsage();
					return false;
				}
			}

            // handle default arguments
            if (d_hosts.Count == 0)
            {
                System.Console.WriteLine("Missing host IP");
                printUsage();
                return false;
            }

            // check for appliation name
            if ((d_authOption == "APPLICATION"  || d_authOption == "USER_APP") && (d_name == ""))
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
            if (d_authOption == "DIRSVC" && d_dsName == "")
            {
                System.Console.WriteLine("Directory Service property name cannot be NULL for DIRSVC authorization.");
                printUsage();
                return false;
            }

            return flag;
		}

        /// <summary>
        /// Validate if date or date/time is valid
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private bool isDateTimeValid(string dateTime, string format)
        {
            DateTime outDateTime;
            System.IFormatProvider formatProvider = new System.Globalization.CultureInfo("en-US", true);
            return DateTime.TryParseExact(dateTime, format, formatProvider,
                System.Globalization.DateTimeStyles.AllowWhiteSpaces, out outDateTime);
        }

		private void printErrorInfo(string leadingStr, Element errorInfo)    
		{
			System.Console.WriteLine(leadingStr + errorInfo.GetElementAsString(CATEGORY) +
				" (" + errorInfo.GetElementAsString(MESSAGE) + ")");
		}

		private void printUsage()
		{
			System.Console.WriteLine("Usage:");
			System.Console.WriteLine("	Retrieve intraday bars");
			System.Console.WriteLine("      [-s     <security	= IBM US Equity>");
			System.Console.WriteLine("      [-e     <event		= TRADE>");
			System.Console.WriteLine("      [-b     <barInterval= 60>");
			System.Console.WriteLine("      [-sd    <startDateTime  = 2008-08-11T13:30:00>");
			System.Console.WriteLine("      [-ed    <endDateTime    = 2008-08-12T13:30:00>");
			System.Console.WriteLine("      [-g     <gapFillInitialBar = false>");
			System.Console.WriteLine("      [-ip    <ipAddress	= localhost>");
			System.Console.WriteLine("      [-p     <tcpPort	= 8194>");
            System.Console.WriteLine("      [-auth  <authenticationOption = LOGON (default) or APPLICATION or DIRSVC or USER_APP>]");
            System.Console.WriteLine("      [-n     <name = applicationName or directoryService>]");
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine("1) All times are in GMT.");
			System.Console.WriteLine("2) Only one security can be specified.");
			System.Console.WriteLine("3) Only one event can be specified.");
            System.Console.WriteLine("4) Specify only LOGON to authorize 'user' using Windows login name.");
            System.Console.WriteLine("5) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
            System.Console.WriteLine("6) Specify APPLICATION and name(Application Name) to authorize application.");
        }
	}
}