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
using System;

namespace Bloomberglp.Blpapi.Examples
{
    public class MarketDepthSubscriptionSnapshotExample
    {
        
        private const string MKTDEPTH_SVC = "//blp/mktdepthdata";
        private const string AUTH_SVC = "//blp/apiauth";

        private static readonly Name EXCEPTIONS = new Name("exceptions");
        private static readonly Name FIELD_ID = new Name("fieldId");
        private static readonly Name REASON = new Name("reason");
        private static readonly Name SOURCE = new Name("source");
        private static readonly Name ERROR_CODE = new Name("errorCode");
        private static readonly Name CATEGORY = new Name("category");
        private static readonly Name SUBCATEGORY = new Name("subcategory");
        private static readonly Name DESCRIPTION = new Name("description");
        private static readonly Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
        private static readonly Name AUTHORIZATION_FAILURE = Name.GetName("AuthorizationFailure");
        private static readonly Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");
        private static readonly Name TOKEN_FAILURE = Name.GetName("TokenGenerationFailure");

        private static readonly Name MarketDepthUpdates = Name.GetName("MarketDepthUpdates");
        private static readonly Name MKTDEPTH_EVENT_TYPE = Name.GetName("MKTDEPTH_EVENT_TYPE");
        private static readonly Name MKTDEPTH_EVENT_SUBTYPE = Name.GetName("MKTDEPTH_EVENT_SUBTYPE");
        private static readonly Name MD_GAP_DETECTED = Name.GetName("MD_GAP_DETECTED");
        private static readonly Name MD_MULTI_TICK_UPD_RT = Name.GetName("MD_MULTI_TICK_UPD_RT");
        private static readonly Name MD_TABLE_CMD_RT = Name.GetName("MD_TABLE_CMD_RT");
        private static readonly Name MD_BOOK_TYPE = Name.GetName("MD_BOOK_TYPE");
        private static readonly Name MBO_WINDOW_SIZE = Name.GetName("MBO_WINDOW_SIZE");
        private static readonly Name MBO_ASK_POSITION_RT = Name.GetName("MBO_ASK_POSITION_RT");
        private static readonly Name MBO_ASK_RT = Name.GetName("MBO_ASK_RT");
        private static readonly Name MBO_ASK_BROKER_RT = Name.GetName("MBO_ASK_BROKER_RT");
        private static readonly Name MBO_ASK_COND_CODE_RT = Name.GetName("MBO_ASK_COND_CODE_RT");
        private static readonly Name MBO_ASK_SIZE_RT = Name.GetName("MBO_ASK_SIZE_RT");
        private static readonly Name MBO_TABLE_ASK = Name.GetName("MBO_TABLE_ASK");
        private static readonly Name MBO_BID_POSITION_RT = Name.GetName("MBO_BID_POSITION_RT");
        private static readonly Name MBO_BID_RT = Name.GetName("MBO_BID_RT");
        private static readonly Name MBO_BID_BROKER_RT = Name.GetName("MBO_BID_BROKER_RT");
        private static readonly Name MBO_BID_COND_CODE_RT = Name.GetName("MBO_BID_COND_CODE_RT");
        private static readonly Name MBO_BID_SIZE_RT = Name.GetName("MBO_BID_SIZE_RT");
        private static readonly Name MBO_TABLE_BID = Name.GetName("MBO_TABLE_BID");
        private static readonly Name MBO_TIME_RT = Name.GetName("MBO_TIME_RT");
        private static readonly Name MBO_SEQNUM_RT = Name.GetName("MBO_SEQNUM_RT");
        private static readonly Name MBL_WINDOW_SIZE = Name.GetName("MBL_WINDOW_SIZE");
        private static readonly Name MBL_ASK_POSITION_RT = Name.GetName("MBL_ASK_POSITION_RT");
        private static readonly Name MBL_ASK_RT = Name.GetName("MBL_ASK_RT");
        private static readonly Name MBL_ASK_NUM_ORDERS_RT = Name.GetName("MBL_ASK_NUM_ORDERS_RT");
        private static readonly Name MBL_ASK_COND_CODE_RT = Name.GetName("MBL_ASK_COND_CODE_RT");
        private static readonly Name MBL_ASK_SIZE_RT = Name.GetName("MBL_ASK_SIZE_RT");
        private static readonly Name MBL_TABLE_ASK = Name.GetName("MBL_TABLE_ASK");
        private static readonly Name MBL_BID_POSITION_RT = Name.GetName("MBL_BID_POSITION_RT");
        private static readonly Name MBL_BID_RT = Name.GetName("MBL_BID_RT");
        private static readonly Name MBL_BID_NUM_ORDERS_RT = Name.GetName("MBL_BID_NUM_ORDERS_RT");
        private static readonly Name MBL_BID_COND_CODE_RT = Name.GetName("MBL_BID_COND_CODE_RT");
        private static readonly Name MBL_BID_SIZE_RT = Name.GetName("MBL_BID_SIZE_RT");
        private static readonly Name MBL_TABLE_BID = Name.GetName("MBL_TABLE_BID");
        private static readonly Name MBL_TIME_RT = Name.GetName("MBL_TIME_RT");
        private static readonly Name MBL_SEQNUM_RT = Name.GetName("MBL_SEQNUM_RT");
        private static readonly Name NONE = Name.GetName("NONE");

        private static readonly Name ADD = Name.GetName("ADD");
        private static readonly Name DEL = Name.GetName("DEL");
        private static readonly Name DELALL = Name.GetName("DELALL");
        private static readonly Name DELBETTER = Name.GetName("DELBETTER");
        private static readonly Name DELSIDE = Name.GetName("DELSIDE");
        private static readonly Name EXEC = Name.GetName("EXEC");
        private static readonly Name MOD = Name.GetName("MOD");
        private static readonly Name REPLACE = Name.GetName("REPLACE");
        private static readonly Name REPLACE_BY_BROKER = Name.GetName("REPLACE_BY_BROKER");
        private static readonly Name MARKET_BY_LEVEL = Name.GetName("MARKET_BY_LEVEL");
        private static readonly Name MARKET_BY_ORDER = Name.GetName("MARKET_BY_ORDER");
        private static readonly Name CLEARALL = Name.GetName("CLEARALL");
        private static readonly Name REPLACE_CLEAR = Name.GetName("REPLACE_CLEAR");
        private static readonly Name REPLACE_BY_PRICE = Name.GetName("REPLACE_BY_PRICE");

        private static readonly Name ASK = Name.GetName("ASK");
        private static readonly Name BID = Name.GetName("BID");
        private static readonly Name ASK_RETRANS = Name.GetName("ASK_RETRANS");
        private static readonly Name BID_RETRANS = Name.GetName("BID_RETRANS");
        private static readonly Name TABLE_INITPAINT = Name.GetName("TABLE_INITPAINT");
        private static readonly Name TABLE_UPDATE = Name.GetName("TABLE_UPDATE");

	    private Name[,] PRICE_FIELD = new Name[2,2] {{MBO_BID_RT, MBO_ASK_RT}, 
							      {MBL_BID_RT, MBL_ASK_RT}};
	    private Name[,] SIZE_FIELD = new Name[2,2] {{MBO_BID_SIZE_RT, MBO_ASK_SIZE_RT}, 
							     {MBL_BID_SIZE_RT, MBL_ASK_SIZE_RT}};
	    private Name[,] POSITION_FIELD = new Name[2,2] {{MBO_BID_POSITION_RT, MBO_ASK_POSITION_RT}, 
								     {MBL_BID_POSITION_RT, MBL_ASK_POSITION_RT}};
	    private Name[,] ORDER_FIELD = new Name[2,2] {{NONE, NONE},
							      {MBL_BID_NUM_ORDERS_RT, MBL_ASK_NUM_ORDERS_RT}};
	    private Name[,] BROKER_FIELD = new Name[2,2] {{MBO_BID_BROKER_RT, MBO_ASK_BROKER_RT}, 
							       {NONE, NONE}};
	    private Name[] TIME_FIELD = new Name[2] {MBO_TIME_RT, MBL_TIME_RT};

        const int BIDSIDE = 0;
        const int ASKSIDE = 1;
       

        const int BYORDER = 0;
        const int BYLEVEL = 1;
        const int UNKNOWN = -1;

        const int SIZE = 2;
        const string DateTimeFormat = @"MM-dd-yyyy HH:mm:ss.ffffzzz";

        private List<string> d_hosts;
        private int d_port;
        private string d_authOption;
        private string d_dsName;
        private string d_name;
        private string d_token;
        private Identity d_identity;
        private SessionOptions d_sessionOptions;
        private Session d_session;
        private string d_security;
        private List<string> d_options;
        private List<Subscription> d_subscriptions;
        private string d_service;

        private ByOrderBook[] d_orderBooks;
        private ByLevelBook[] d_levelBooks;
        int d_marketDepthBook;
        int d_pricePrecision;
        bool d_showTicks;
        bool d_gapDetected = false;
        bool d_askRetran = false;
        bool d_bidRetran = false;
        bool d_resubscribed = false;
        long d_sequenceNumber = 0;
        string d_consoleWrite;
        
        

        public static void Main(string[] args)
        {
            System.Console.WriteLine("Market Depth Subscription Snapshot Example");
            MarketDepthSubscriptionSnapshotExample example = new MarketDepthSubscriptionSnapshotExample();
            example.run(args);
        }

        public MarketDepthSubscriptionSnapshotExample()
        {
            d_hosts = new List<string>();
            d_port = 8194;
            d_sessionOptions = new SessionOptions();
            d_security = string.Empty;
            d_options = new List<string>();
            d_subscriptions = new List<Subscription>();
            d_service = string.Empty;
            d_name = "";
            d_dsName = "";
            d_pricePrecision = 4;

            d_orderBooks = new ByOrderBook[SIZE];
            d_orderBooks[BIDSIDE] = new ByOrderBook();
            d_orderBooks[ASKSIDE] = new ByOrderBook();
            d_levelBooks = new ByLevelBook[SIZE];
            d_levelBooks[BIDSIDE] = new ByLevelBook();
            d_levelBooks[ASKSIDE] = new ByLevelBook();
            d_showTicks = false;
            d_consoleWrite = string.Empty;
        }

        /*------------------------------------------------------------------------------------
         * Name			: createSession
         * Description	: The create session with session option provided
         * Arguments	: none
         * Returns		: none
         *------------------------------------------------------------------------------------*/
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

        /*------------------------------------------------------------------------------------
         * Name			: GenerateToken
         * Description	: generate token for user/application
         * Arguments	: out token will return the token string
         * Returns		: true - successful, false - failed
         *------------------------------------------------------------------------------------*/
        private bool GenerateToken(out string token)
        {
            bool isTokenSuccess = false;
            bool isRunning = false;

            token = string.Empty;
            CorrelationID tokenReqId = new CorrelationID(99);
            EventQueue tokenEventQueue = new EventQueue();
            // generate token
            d_session.GenerateToken(tokenReqId, tokenEventQueue);

            while (!isRunning)
            {
                // get token request event
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

        /*------------------------------------------------------------------------------------
         * Name			: isBPipeAuthorized
         * Description	: authorize user/application
         * Arguments	: token from generate token request
         *              : identity of user/app authorized
         * Returns		: true - successful, false - failed
         *------------------------------------------------------------------------------------*/
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
            // get auth service
            Service authService = d_session.GetService(AUTH_SVC);
            // authorization request
            Request authRequest = authService.CreateAuthorizationRequest();
            authRequest.Set("token", token);
            identity = d_session.CreateIdentity();
            EventQueue authEventQueue = new EventQueue();
            // send authorization request
            d_session.SendAuthorizationRequest(authRequest, identity, authEventQueue, new CorrelationID(1));

            while (isRunning)
            {
                // process authorization event
                Event eventObj = authEventQueue.NextEvent();
                System.Console.WriteLine("processEvent");
                if (eventObj.Type == Event.EventType.RESPONSE || eventObj.Type == Event.EventType.REQUEST_STATUS)
                {
                    foreach (Message msg in eventObj)
                    {
                        if (msg.MessageType == AUTHORIZATION_SUCCESS)
                        {
                            // success
                            System.Console.WriteLine("Authorization SUCCESS");

                            isAuthorized = true;
                            isRunning = false;
                            break;
                        }
                        else if (msg.MessageType == AUTHORIZATION_FAILURE)
                        {
                            // authorization failed
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

        /*------------------------------------------------------------------------------------
         * Name			: run 
         * Description	: start application process
         * Arguments	: args are the argument values
         * Returns		: none
         *------------------------------------------------------------------------------------*/
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

            // open market depth service
            if (!d_session.OpenService(MKTDEPTH_SVC))
            {
                System.Console.Error.WriteLine("Failed to open " + MKTDEPTH_SVC);
                return;
            }

            if (d_authOption == "NONE")
            {
                System.Console.WriteLine("Subscribing without Identity...\n");
                d_session.Subscribe(d_subscriptions);
            }
            else
            {
                // subscribe with Identity
                System.Console.WriteLine("Subscribing with Identity...\n");
                d_session.Subscribe(d_subscriptions, d_identity);
            }

            ConsoleKeyInfo key;
            while (true)
            {
                lock (d_consoleWrite)
                {
                    printMenu();
                }

                // wait for enter key to exit application
                key = System.Console.ReadKey();
                System.Console.WriteLine("");

                if ((key.KeyChar == 'v') || (key.KeyChar == 'V'))
                {
                    // view market depth book
                    switch (d_marketDepthBook)
                    {
                        case BYLEVEL:
                            ShowByLevelBook();
                            break;
                        case BYORDER:
                            ShowByOrderBook();
                            break;
                        default:
                            lock (d_consoleWrite)
                            {
                                System.Console.WriteLine("Unknown book type");
                            }
                            break;
                    }
                }
                else if ((key.KeyChar == 't') || (key.KeyChar == 'T'))
                {
                    // show ticks
                    d_showTicks = !d_showTicks;
                }
                else if ((key.KeyChar == 'q') || (key.KeyChar == 'Q'))
                {
                    // quite
                    break;
                }
                else
                {
                    lock (d_consoleWrite)
                    {
                        System.Console.WriteLine("Unknown command: '" + key.KeyChar.ToString() + "'");
                    }
                }
            }
            // unsubscribe  
            d_session.Unsubscribe(d_subscriptions);
            // stop session
            d_session.Stop();
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
            lock (d_consoleWrite)
            {
                System.Console.WriteLine("Processing SUBSCRIPTION_STATUS");
                foreach (Message msg in eventObj)
                {
                    string topic = msg.CorrelationID.Object.ToString();
                    System.Console.WriteLine(System.DateTime.Now.ToString(DateTimeFormat) +
                        ": " + topic + " - " + msg.MessageType);

                    if (msg.HasElement(REASON))
                    {
                        // This can occur on SubscriptionFailure.
                        string temp = "";
                        Element reason = msg.GetElement(REASON);
                        if (reason.HasElement(SOURCE, true))
                        {
                            temp = "\tsource: " + reason.GetElement(SOURCE).GetValueAsString();
                        }
                        if (reason.HasElement(ERROR_CODE, true))
                        {
                            temp += "\n\terrorCode: " + reason.GetElement(ERROR_CODE).GetValueAsString();
                        }
                        if (reason.HasElement(CATEGORY, true))
                        {
                            temp += "\n\tcategory: " + reason.GetElement(CATEGORY).GetValueAsString();
                        }
                        if (reason.HasElement(DESCRIPTION, true))
                        {
                            temp += "\n\tdescription: " + reason.GetElement(DESCRIPTION).GetValueAsString();
                        }
                        if (reason.HasElement(SUBCATEGORY, true))
                        {
                            temp += "\n\tsubcategory: " + reason.GetElement(SUBCATEGORY).GetValueAsString();
                        }	

                        System.Console.WriteLine(temp);
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
        }

        private void processSubscriptionDataEvent(Event eventObj, Session session)
        {
            foreach (Message msg in eventObj)
            {
                if (msg.MessageType.Equals(MarketDepthUpdates))
                {
                    // Market Depth data
                    if (d_showTicks)
                    {
                        lock (d_consoleWrite)
                        {
                            // output message
                            System.Console.WriteLine(System.DateTime.Now.ToString(DateTimeFormat) + ": Fragment Type - " +
                                msg.FragmentType.ToString());
                            System.Console.WriteLine(msg.ToString());
                        }
                    }

                    // setup book type before processing data
                    if (d_marketDepthBook == UNKNOWN)
                    {
                        if (msg.HasElement(MKTDEPTH_EVENT_TYPE))
                        {
                            Element eventType = msg.GetElement(MKTDEPTH_EVENT_TYPE);
                            if (eventType.GetValueAsName().Equals(MARKET_BY_ORDER))
                            {
                                d_marketDepthBook = BYORDER;
                            }
                            else if (eventType.GetValueAsName().Equals(MARKET_BY_LEVEL))
                            {
                                d_marketDepthBook = BYLEVEL;
                            }
                        }
                    }

                    // process message
                    switch (d_marketDepthBook)
                    {
                        case BYLEVEL:
                            processByLevelEvent(msg);
                            break;
                        case BYORDER:
                            processByOrderEvent(msg);
                            break;
                        default:
                            // unknow book type
                            lock (d_consoleWrite)
                            {
                                // output message
                                System.Console.WriteLine(System.DateTime.Now.ToString(DateTimeFormat) + ": Unknown book type. Can not process message.");
                                System.Console.WriteLine(System.DateTime.Now.ToString(DateTimeFormat) + ": Fragment Type - " +
                                    msg.FragmentType.ToString());
                                System.Console.WriteLine(msg.ToString());
                            }
                            break;
                    }
                }
            }
        }

	    bool processByOrderEvent(Message msg)
        {
            int side = -1;
		    int position = -1;
            bool bidRetran = false;
            bool askRetran = false;

            // get gap detection flag (AMD book only)
		    if (msg.HasElement(MD_GAP_DETECTED, true) && !d_gapDetected) {
		    	d_gapDetected = true;
		    	lock (d_consoleWrite) {
		    		System.Console.WriteLine("Bloomberg detected a gap in data stream.");
				}
		    }

            // get event sub-type 
            Name subType = msg.GetElement(MKTDEPTH_EVENT_SUBTYPE).GetValueAsName();
            // get retran flags
            bidRetran = subType.Equals(BID_RETRANS);
            askRetran = subType.Equals(ASK_RETRANS);
            // BID or ASK message
            if (subType.Equals(BID) || subType.Equals(ASK) ||
                bidRetran || askRetran)
            {
                if (subType.Equals(BID) || bidRetran)
                {
				    side = BIDSIDE;
                }
                else if (subType.Equals(ASK) || askRetran)
                {
				    side = ASKSIDE;
			    }
                // get position
                position = -1;
			    if (msg.HasElement(POSITION_FIELD[BYORDER, side], true)) {
				    position = msg.GetElement(POSITION_FIELD[BYORDER, side]).GetValueAsInt32();
				    if (position > 0) --position;
			    }

			    //  BID/ASK retran message
			    if (askRetran || bidRetran) {
				    // check for multi tick
			    	if (msg.HasElement(MD_MULTI_TICK_UPD_RT, true)) {
			    		// multi tick
			    		if (msg.GetElement(MD_MULTI_TICK_UPD_RT).GetValueAsInt32() == 0 ) {
					    	// last multi tick message, reset sequence number so next non-retran
			    			// message sequence number will be use as new starting number
					    	d_sequenceNumber = 0;
			    			if (askRetran && d_askRetran) {
			    				// end of ask retran
			    				d_askRetran = false;
			    		    	lock (d_consoleWrite) {
			    		    		System.Console.WriteLine("Ask retran completed.");
			    				}
			    			} else if (bidRetran && d_bidRetran) {
			    				// end of ask retran
			    				d_bidRetran = false;
			    		    	lock (d_consoleWrite) {
			    		    		System.Console.WriteLine("Bid retran completed.");
			    				}
			    			}
			    			if (!(d_askRetran || d_bidRetran)) {
			    				// retran completed
			    		    	lock (d_consoleWrite) {
			    		    		if (d_gapDetected) {
			    		    			// gap detected retran completed
			    		    			d_gapDetected = false;
			    		    			System.Console.WriteLine("Gap detected retran completed.");
			    		    		} else {
			    		    			// normal retran completed
			    		    			System.Console.WriteLine("Retran completed.");
			    		    		}
			    				}
			    			}
			    		} else {
			    			if (askRetran && !d_askRetran) {
			    				// start of ask retran
			    				d_askRetran = true;
			    		    	lock (d_consoleWrite) {
			    		    		System.Console.WriteLine("Ask retran started.");
			    				}
			    			} else if (bidRetran && !d_bidRetran) {
			    				// start of ask retran
			    				d_bidRetran = true;
			    		    	lock (d_consoleWrite) {
                                    System.Console.WriteLine("Bid retran started.");
			    				}
			    			}
			    		}
			    	}
			    } else if (msg.HasElement(MBO_SEQNUM_RT, true)) {
			    	// get sequence number
                    long currentSequence = msg.GetElementAsInt64(MBO_SEQNUM_RT);
                    if (d_sequenceNumber == 0 || d_sequenceNumber == 1 ||
                        (currentSequence == 1 && d_sequenceNumber > 1))
                    {
                        // use current sequence number
                        d_sequenceNumber = currentSequence;
                    }
                    else if ((d_sequenceNumber + 1 != currentSequence) && !d_gapDetected)
                    {
                        if (!d_resubscribed)
			    		{
					    	// previous tick sequence can not be smaller than current tick 
					    	// sequence number - 1 and NOT in gap detected mode. 
				    		lock (d_consoleWrite) {
				    			System.Console.WriteLine("Warning: Gap detected - previous sequence number is " + 
				    					d_sequenceNumber + " and current tick sequence number is " +
				    					currentSequence + ").");
				    		}
				    		// gap detected, re-subscribe to securities
							d_session.Resubscribe(d_subscriptions);
							d_resubscribed = true;
			    		}
			    	} else if (d_sequenceNumber >= currentSequence) {
			    		// previous tick sequence number can not be greater or equal
			    		// to current sequence number
			    		lock (d_consoleWrite) {
			    			System.Console.WriteLine("Warning: Current Sequence number (" + currentSequence + 
			    					") is smaller or equal to previous tick sequence number (" +
			    					d_sequenceNumber + ").");
			    		}
			    	} else {
			    		// save current sequence number
			    		d_sequenceNumber = currentSequence;
			    	}
			    }

			    // get command
			    Name cmd = msg.GetElement(MD_TABLE_CMD_RT).GetValueAsName();
			    if (cmd.Equals(CLEARALL)) {
				    d_orderBooks[side].doClearAll();
			    } else if (cmd.Equals(DEL)) {
				    d_orderBooks[side].doDel(position);
			    } else if (cmd.Equals(DELALL)) {
				    d_orderBooks[side].doDelAll();
			    } else if (cmd.Equals(DELBETTER)) {
				    d_orderBooks[side].doDelBetter(position);
			    } else if (cmd.Equals(DELSIDE)) {
				    d_orderBooks[side].doDelSide();
			    } else if (cmd.Equals(REPLACE_CLEAR)) {
				    d_orderBooks[side].doReplaceClear(position);
			    } else {
				    // process other commands
                    // get price
				    double price = msg.GetElement(PRICE_FIELD[BYORDER, side]).GetValueAsFloat64();
				    // get size
                    int size = 0;
				    if (msg.HasElement(SIZE_FIELD[BYORDER, side], true)) {
					    size = (int)msg.GetElement(SIZE_FIELD[BYORDER, side]).GetValueAsInt64();
				    }
                    // get broker
				    string broker = string.Empty;
				    if (msg.HasElement(BROKER_FIELD[BYORDER, side], true)) {
					    broker = msg.GetElement(BROKER_FIELD[BYORDER, side]).GetValueAsString();
				    }
                    // get time
				    Datetime timeStamp = msg.GetElement(TIME_FIELD[BYORDER]).GetValueAsDatetime();
                    // create entry
                    BookEntry  entry = new BookEntry(broker, price, timeStamp.ToSystemDateTime().ToString("HH:mm:ss.fff"), 	0, size);

                    // process data command
				    if(cmd.Equals(ADD))
					    d_orderBooks[side].doAdd(position, ref entry);
				    else if(cmd.Equals(MOD))
					    d_orderBooks[side].doMod(position, ref entry);
				    else if(cmd.Equals(REPLACE))
					    d_orderBooks[side].doReplace(position, ref entry);
				    else if(cmd.Equals(REPLACE_BY_BROKER))
					    d_orderBooks[side].doReplaceByBroker(ref entry);
				    else if(cmd.Equals(EXEC))
					    d_orderBooks[side].doExec(position, ref entry);
			    }
		    } else {
			    if (subType.Equals(TABLE_INITPAINT)) {
				    if (msg.FragmentType == Message.Fragment.START ||
					    msg.FragmentType == Message.Fragment.NONE) {
					    // init paint
					    if (msg.HasElement(MBO_WINDOW_SIZE, true) ){
						    d_orderBooks[ASKSIDE].WindowSize = (int) msg.GetElementAsInt64(MBO_WINDOW_SIZE);
                            d_orderBooks[BIDSIDE].WindowSize = d_orderBooks[ASKSIDE].WindowSize;
					    }
					    d_orderBooks[ASKSIDE].BookType = msg.GetElementAsString(MD_BOOK_TYPE);
                        d_orderBooks[BIDSIDE].BookType = d_orderBooks[ASKSIDE].BookType;
					    // clear cache
					    d_orderBooks[ASKSIDE].doClearAll();
					    d_orderBooks[BIDSIDE].doClearAll();
				    }

				    if (msg.HasElement(MBO_TABLE_ASK, true)){
					    // has ask table array
					    Element askTable = msg.GetElement(MBO_TABLE_ASK);
					    int numOfItems = askTable.NumValues;
					    for (int index = 0; index < numOfItems; ++index) {
						    Element ask = askTable.GetValueAsElement(index);
						    // get command
                            Name cmd = ask.GetElement(MD_TABLE_CMD_RT).GetValueAsName();
                            // get position
                            position = -1;
                            if (ask.HasElement(POSITION_FIELD[BYORDER, ASKSIDE], true))
                            {
                                position = ask.GetElement(POSITION_FIELD[BYORDER, ASKSIDE]).GetValueAsInt32();
                                if (position > 0) --position;
                            }
                            // get price
                            double askPrice = ask.GetElement(PRICE_FIELD[BYORDER, ASKSIDE]).GetValueAsFloat64();
						    // get size
                            int askSize = 0;
						    if (ask.HasElement(SIZE_FIELD[BYORDER, ASKSIDE], true)) {
							    askSize = (int)ask.GetElement(SIZE_FIELD[BYORDER, ASKSIDE]).GetValueAsInt64();
						    }
                            // get broker
						    string askBroker = string.Empty;
						    if (ask.HasElement(BROKER_FIELD[BYORDER, ASKSIDE], true)) {
							    askBroker = ask.GetElement(BROKER_FIELD[BYORDER, ASKSIDE]).GetValueAsString();
						    }
                            // get time
						    Datetime timeStamp = ask.GetElement(TIME_FIELD[BYORDER]).GetValueAsDatetime();
						    // create entry
                            BookEntry entry = new BookEntry(askBroker, askPrice, timeStamp.ToSystemDateTime().ToString("HH:mm:ss.fff"), 0, askSize);

                            // process data command
						    if(cmd.Equals(ADD))
							    d_orderBooks[ASKSIDE].doAdd(position, ref entry);
						    else if(cmd.Equals(MOD))
							    d_orderBooks[ASKSIDE].doMod(position, ref entry);
						    else if(cmd.Equals(REPLACE))
							    d_orderBooks[ASKSIDE].doReplace(position, ref entry);
						    else if(cmd.Equals(REPLACE_BY_BROKER))
							    d_orderBooks[ASKSIDE].doReplaceByBroker(ref entry);
						    else if(cmd.Equals(EXEC))
							    d_orderBooks[ASKSIDE].doExec(position, ref entry);
					    }
				    }
				    if (msg.HasElement(MBO_TABLE_BID, true)){
					    // has bid table array
					    Element bidTable = msg.GetElement(MBO_TABLE_BID);
					    int numOfItems = bidTable.NumValues;
					    for (int index = 0; index < numOfItems; ++index) {
						    Element bid = bidTable.GetValueAsElement(index);
						    // get command
                            Name cmd = bid.GetElement(MD_TABLE_CMD_RT).GetValueAsName();
                            // get position
                            position = -1;
                            if (bid.HasElement(POSITION_FIELD[BYORDER, BIDSIDE], true))
                            {
                                position = bid.GetElement(POSITION_FIELD[BYORDER, BIDSIDE]).GetValueAsInt32();
                                if (position > 0) --position;
                            }
                            // get price
                            double bidPrice = bid.GetElement(PRICE_FIELD[BYORDER, BIDSIDE]).GetValueAsFloat64();
						    // get size
                            int bidSize = 0;
						    if (bid.HasElement(SIZE_FIELD[BYORDER, BIDSIDE], true)) {
							    bidSize = (int)bid.GetElement(SIZE_FIELD[BYORDER, BIDSIDE]).GetValueAsInt64();
						    }
                            // get broker
						    string bidBroker = string.Empty;
						    if (bid.HasElement(BROKER_FIELD[BYORDER, BIDSIDE], true)) {
							    bidBroker = bid.GetElement(BROKER_FIELD[BYORDER, BIDSIDE]).GetValueAsString();
						    }
                            // get time
						    Datetime timeStamp = bid.GetElement(TIME_FIELD[BYORDER]).GetValueAsDatetime();
						    // create entry
                            BookEntry entry = new BookEntry(bidBroker, bidPrice, timeStamp.ToSystemDateTime().ToString("HH:mm:ss.fff"), 0, bidSize);

                            // process data command
						    if(cmd.Equals(ADD))
							    d_orderBooks[BIDSIDE].doAdd(position, ref entry);
						    else if(cmd.Equals(MOD))
							    d_orderBooks[BIDSIDE].doMod(position, ref entry);
						    else if(cmd.Equals(REPLACE))
							    d_orderBooks[BIDSIDE].doReplace(position, ref entry);
						    else if(cmd.Equals(REPLACE_BY_BROKER))
							    d_orderBooks[BIDSIDE].doReplaceByBroker(ref entry);
						    else if(cmd.Equals(EXEC))
							    d_orderBooks[BIDSIDE].doExec(position, ref entry);
					    }
				    }
                    // clear sequence number so next sequence number is pickup
                    d_sequenceNumber = 0;
                    // clear re-subscribed flag
                    d_resubscribed = false;
                }
		    }
            return true;
	    }

        /*------------------------------------------------------------------------------------
         * Name			: processByLevelEvent
         * Description	: process by level message
         * Arguments	: msg is the tick data message
         * Returns		: none
         *------------------------------------------------------------------------------------*/
        bool processByLevelEvent(Message msg)
        {
            int side = -1;
		    int position = -1;
            bool bidRetran = false;
            bool askRetran = false;

            // get gap detection flag (AMD book only)
            if (msg.HasElement(MD_GAP_DETECTED, true) && !d_gapDetected)
            {
                d_gapDetected = true;
                lock (d_consoleWrite)
                {
                    System.Console.WriteLine("Bloomberg detected a gap in data stream.");
                }
            }

            // get event subtype
            Name subType = msg.GetElement(MKTDEPTH_EVENT_SUBTYPE).GetValueAsName();
            // get retran flags
            bidRetran = subType.Equals(BID_RETRANS);
            askRetran = subType.Equals(ASK_RETRANS);
            // BID or ASK message
            if (subType.Equals(BID) || subType.Equals(ASK) ||
                bidRetran || askRetran)
            {
                // set book side
                if (subType.Equals(BID) || bidRetran)
                {
				    side = BIDSIDE;
                }
                else if (subType.Equals(ASK) || askRetran)
                {
				    side = ASKSIDE;
			    }

                // get position
                position = -1;
                if (msg.HasElement(POSITION_FIELD[BYLEVEL, side], true))
                {
                    position = msg.GetElement(POSITION_FIELD[BYLEVEL, side]).GetValueAsInt32();
                    if (position > 0) --position;
                }

			    //  BID/ASK retran message
			    if (askRetran || bidRetran) {
				    // check for multi tick
			    	if (msg.HasElement(MD_MULTI_TICK_UPD_RT, true)) {
			    		// multi tick
			    		if (msg.GetElement(MD_MULTI_TICK_UPD_RT).GetValueAsInt32() == 0 ) {
					    	// last multi tick message, reset sequence number so next non-retran
			    			// message sequence number will be use as new starting number
					    	d_sequenceNumber = 0;
			    			if (askRetran && d_askRetran) {
			    				// end of ask retran
			    				d_askRetran = false;
			    		    	lock (d_consoleWrite) {
			    		    		System.Console.WriteLine("Ask retran completed.");
			    				}
			    			} else if (bidRetran && d_bidRetran) {
			    				// end of ask retran
			    				d_bidRetran = false;
			    		    	lock (d_consoleWrite) {
			    		    		System.Console.WriteLine("Bid retran completed.");
			    				}
			    			}
			    			if (!(d_askRetran || d_bidRetran)) {
			    				// retran completed
			    		    	lock (d_consoleWrite) {
			    		    		if (d_gapDetected) {
			    		    			// gap detected retran completed
			    		    			d_gapDetected = false;
			    		    			System.Console.WriteLine("Gap detected retran completed.");
			    		    		} else {
			    		    			// normal retran completed
			    		    			System.Console.WriteLine("Retran completed.");
			    		    		}
			    				}
			    			}
			    		} else {
			    			if (askRetran && !d_askRetran) {
			    				// start of ask retran
			    				d_askRetran = true;
			    		    	lock (d_consoleWrite) {
			    		    		System.Console.WriteLine("Ask retran started.");
			    				}
			    			} else if (bidRetran && !d_bidRetran) {
			    				// start of ask retran
			    				d_bidRetran = true;
			    		    	lock (d_consoleWrite) {
                                    System.Console.WriteLine("Bid retran started.");
			    				}
			    			}
			    		}
			    	}
			    } else if (msg.HasElement(MBL_SEQNUM_RT, true)) {
			    	// get sequence number
			    	long currentSequence = msg.GetElementAsInt64(MBL_SEQNUM_RT);
			    	if (d_sequenceNumber == 0 || d_sequenceNumber == 1 ||
                        (currentSequence == 1 && d_sequenceNumber > 1)) {
			    		// use current sequence number
			    		d_sequenceNumber = currentSequence;
			    	} else if ((d_sequenceNumber + 1 != currentSequence) && !d_gapDetected ) {
			    		if (!d_resubscribed)
			    		{
					    	// previous tick sequence can not be smaller than current tick 
					    	// sequence number - 1 and NOT in gap detected mode. 
				    		lock (d_consoleWrite) {
				    			System.Console.WriteLine("Warning: Gap detected - previous sequence number is " + 
				    					d_sequenceNumber + " and current tick sequence number is " +
				    					currentSequence + ").");
				    		}
				    		// gap detected, re-subscribe to securities
							d_session.Resubscribe(d_subscriptions);
							d_resubscribed = true;
			    		}
			    	} else if (d_sequenceNumber >= currentSequence) {
			    		// previous tick sequence number can not be greater or equal
			    		// to current sequence number
			    		lock (d_consoleWrite) {
			    			System.Console.WriteLine("Warning: Current Sequence number (" + currentSequence + 
			    					") is smaller or equal to previous tick sequence number (" +
			    					d_sequenceNumber + ").");
			    		}
			    	} else {
			    		// save current sequence number
			    		d_sequenceNumber = currentSequence;
			    	}
			    }

                // get command
			    Name cmd = msg.GetElement(MD_TABLE_CMD_RT).GetValueAsName();
			    if (cmd.Equals(CLEARALL)) {
				    d_levelBooks[side].doClearAll();
			    } else if (cmd.Equals(DEL)) {
				    d_levelBooks[side].doDel(position);
			    } else if (cmd.Equals(DELALL)) {
				    d_levelBooks[side].doDelAll();
			    } else if (cmd.Equals(DELBETTER)) {
				    d_levelBooks[side].doDelBetter(position);
			    } else if (cmd.Equals(DELSIDE)) {
				    d_levelBooks[side].doDelSide();
			    } else if (cmd.Equals(REPLACE_CLEAR)) {
				    d_levelBooks[side].doReplaceClear(position);
			    } else {
				    // process other commands
                    // get price
				    double price = msg.GetElement(PRICE_FIELD[BYLEVEL, side]).GetValueAsFloat64();
                    // get size
				    int size = 0;
				    if (msg.HasElement(SIZE_FIELD[BYLEVEL, side], true)) {
					    size = (int)msg.GetElement(SIZE_FIELD[BYLEVEL, side]).GetValueAsInt64();
				    }
                    // get number of order
				    int numOrder = 0;
				    if (msg.HasElement(ORDER_FIELD[BYLEVEL, side], true)) {
					    numOrder = (int)msg.GetElement(ORDER_FIELD[BYLEVEL, side]).GetValueAsInt64();
				    }
                    // get time
				    Datetime timeStamp = msg.GetElement(TIME_FIELD[BYLEVEL]).GetValueAsDatetime();
                    // create entry
                    BookEntry entry = new BookEntry(price, timeStamp.ToSystemDateTime().ToString("HH:mm:ss.fff"), numOrder, size);

                    // process data command
				    if(cmd.Equals(ADD))
					    d_levelBooks[side].doAdd(position, entry);
				    else if(cmd.Equals(MOD))
					    d_levelBooks[side].doMod(position, ref entry);
				    else if(cmd.Equals(REPLACE))
					    d_levelBooks[side].doReplace(position, ref entry);
				    else if(cmd.Equals(EXEC))
					    d_levelBooks[side].doExec(position, ref entry);
			    }
		    } else {
			    if (subType.Equals(TABLE_INITPAINT)) {
				    if (msg.FragmentType == Message.Fragment.START ||
					    msg.FragmentType == Message.Fragment.NONE) {
					    // init paint
					    if (msg.HasElement(MBL_WINDOW_SIZE, true)){
						    d_levelBooks[ASKSIDE].WindowSize = (int) msg.GetElementAsInt64(MBL_WINDOW_SIZE);
                            d_levelBooks[BIDSIDE].WindowSize = d_levelBooks[ASKSIDE].WindowSize;
					    }
					    d_levelBooks[ASKSIDE].BookType = msg.GetElementAsString(MD_BOOK_TYPE);
                        d_levelBooks[BIDSIDE].BookType = d_levelBooks[ASKSIDE].BookType;
					    // clear cache
					    d_levelBooks[ASKSIDE].doClearAll();
					    d_levelBooks[BIDSIDE].doClearAll();
				    }

				    if (msg.HasElement(MBL_TABLE_ASK, true)){
					    // has ask table array
					    Element askTable = msg.GetElement(MBL_TABLE_ASK);
					    int numOfItems = askTable.NumValues;
					    for (int index = 0; index < numOfItems; ++index) {
						    Element ask = askTable.GetValueAsElement(index);
                            // get command
						    Name cmd = ask.GetElement(MD_TABLE_CMD_RT).GetValueAsName();
                            // get position
                            position = -1;
                            if (ask.HasElement(POSITION_FIELD[BYLEVEL, ASKSIDE], true))
                            {
                                position = ask.GetElement(POSITION_FIELD[BYLEVEL, ASKSIDE]).GetValueAsInt32();
                                if (position > 0) --position;
                            }
                            // get price
                            double askPrice = ask.GetElement(PRICE_FIELD[BYLEVEL, ASKSIDE]).GetValueAsFloat64();
						    // get size
                            int askSize = 0;
						    if (ask.HasElement(SIZE_FIELD[BYLEVEL, ASKSIDE], true)) {
							    askSize = (int)ask.GetElement(SIZE_FIELD[BYLEVEL, ASKSIDE]).GetValueAsInt64();
						    }
                            // get number of order
						    int askNumOrder = 0;
						    if (ask.HasElement(ORDER_FIELD[BYLEVEL, ASKSIDE], true)) {
							    askNumOrder = (int) ask.GetElement(ORDER_FIELD[BYLEVEL, ASKSIDE]).GetValueAsInt64();
						    }
                            // get time
						    Datetime timeStamp = ask.GetElement(TIME_FIELD[BYLEVEL]).GetValueAsDatetime();
						    // create entry
                            BookEntry entry = new BookEntry(askPrice, timeStamp.ToSystemDateTime().ToString("HH:mm:ss.fff"), askNumOrder, askSize);

                            // process data command
						    if(cmd.Equals(ADD))
							    d_levelBooks[ASKSIDE].doAdd(position, entry);
						    else if(cmd.Equals(MOD))
							    d_levelBooks[ASKSIDE].doMod(position, ref entry);
						    else if(cmd.Equals(REPLACE))
							    d_levelBooks[ASKSIDE].doReplace(position, ref entry);
						    else if(cmd.Equals(EXEC))
							    d_levelBooks[ASKSIDE].doExec(position, ref entry);
					    }
				    }
				    if (msg.HasElement(MBL_TABLE_BID, true)){
					    // has bid table array
					    Element bidTable = msg.GetElement(MBL_TABLE_BID);
					    int numOfItems = bidTable.NumValues;
					    for (int index = 0; index < numOfItems; ++index) {
						    Element bid = bidTable.GetValueAsElement(index);
                            // get command
						    Name cmd = bid.GetElement(MD_TABLE_CMD_RT).GetValueAsName();
                            // get position
                            position = -1;
                            if (bid.HasElement(POSITION_FIELD[BYLEVEL, BIDSIDE], true))
                            {
                                position = bid.GetElement(POSITION_FIELD[BYLEVEL, BIDSIDE]).GetValueAsInt32();
                                if (position > 0) --position;
                            }
                            // get price
                            double bidPrice = bid.GetElement(PRICE_FIELD[BYLEVEL, BIDSIDE]).GetValueAsFloat64();
						    // get size
                            int bidSize = 0;
						    if (bid.HasElement(SIZE_FIELD[BYLEVEL, BIDSIDE], true)) {
							    bidSize = (int) bid.GetElement(SIZE_FIELD[BYLEVEL, BIDSIDE]).GetValueAsInt64();
						    }
                            // get number of order
						    int bidNumOrder = 0;
						    if (bid.HasElement(ORDER_FIELD[BYLEVEL, BIDSIDE], true)) {
							    bidNumOrder = (int) bid.GetElement(ORDER_FIELD[BYLEVEL, BIDSIDE]).GetValueAsInt64();
						    }
                            // get time
						    Datetime timeStamp = bid.GetElement(TIME_FIELD[BYLEVEL]).GetValueAsDatetime();
						    // create entry
                            BookEntry entry = new BookEntry(bidPrice, timeStamp.ToSystemDateTime().ToString("HH:mm:ss.fff"), bidNumOrder, bidSize);

                            // process data command
						    if(cmd.Equals(ADD))
							    d_levelBooks[BIDSIDE].doAdd(position, entry);
						    else if(cmd.Equals(MOD))
							    d_levelBooks[BIDSIDE].doMod(position, ref entry);
						    else if(cmd.Equals(REPLACE))
							    d_levelBooks[BIDSIDE].doReplace(position, ref entry);
						    else if(cmd.Equals(EXEC))
							    d_levelBooks[BIDSIDE].doExec(position, ref entry);
					    }
				    }
			    }
		    }
            return true;
        }

        /*------------------------------------------------------------------------------------
         * Name			: processMiscEvents
         * Description	: process misc
         * Arguments	: event is the API event
         *              : session
         * Returns		: none
         *------------------------------------------------------------------------------------*/
        private void processMiscEvents(Event eventObj, Session session)
        {
            System.Console.WriteLine("Processing " + eventObj.Type);
            foreach (Message msg in eventObj)
            {
                System.Console.WriteLine(System.DateTime.Now.ToString("s") +
                    ": " + msg.MessageType + "\n");
            }
        }

        /*----------------------------------------------------------------
         * Name			: ShowByOrderBook
         * Description	: dumps the current order book to the console
         * Arguments	: none
         * Returns		: none
         *---------------------------------------------------------------*/
        private void ShowByOrderBook()
	    {
		    int i;
		    int size;
            int[] bookSize = new int[SIZE];

	        lock (d_consoleWrite)
            {
		        ByOrderBook[] book = d_orderBooks;

                // get BID/ASK size
                bookSize[BIDSIDE] = book[BIDSIDE].BookSize;
                bookSize[ASKSIDE] = book[ASKSIDE].BookSize;
                size = bookSize[BIDSIDE] > bookSize[ASKSIDE] ? bookSize[BIDSIDE] : bookSize[ASKSIDE];

	            int offset = 0;
	            if (d_pricePrecision < 4)
			        offset = 0;
	            else
		            offset = d_pricePrecision - 4;

		        System.Console.WriteLine("-------------------------------------------------------------------------------------------------");
		        System.Console.WriteLine("MAXIMUM WINDOW SIZE: " + book[BIDSIDE].WindowSize);
			    System.Console.WriteLine("BOOK TYPE          : " + book[BIDSIDE].BookType);
		        System.Console.WriteLine("-------------------------------------------------------------------------------------------------");
		        System.Console.WriteLine("                 --- BID ---                                     --- ASK ---");
			    System.Console.WriteLine("  POS BROKER    PRICE" +  string.Format("{0, " + (offset + 4) + "}", "") + 
                    "SIZE      TIME       ---     BROKER    PRICE" + string.Format("{0, " + (offset + 4) + "}", "") +
                    "SIZE      TIME   ");

		        for (i=0; i<size; ++i)
		        {
			        string row;
			        // format book for bid side
			        BookEntry entry = book[BIDSIDE].getEntry(i);
			        if (entry != null) 
			        {
				        row = string.Format("{0,5}", entry.Broker) + " ";
				        row += string.Format("{0,10:F" + d_pricePrecision + "}", entry.Price) + " ";
                        row += string.Format("{0,6}", entry.Size) + " ";
                        row += string.Format("{0,13}", entry.Time) + " ";
			        }
			        else
                        row = string.Format("{0, " + (38 + offset) + "}", "");

			        // format book or ask side
                    entry = book[ASKSIDE].getEntry(i);
                    if (entry != null)
			        {
                        row += "  ---     ";
				        row += string.Format("{0,5}", entry.Broker) + " ";
				        row += string.Format("{0,10:F" + d_pricePrecision + "}", entry.Price) + " ";
                        row += string.Format("{0,6}", entry.Size) + " ";
                        row += string.Format("{0,13}", entry.Time) + " ";
			        }
                    // display row
                    System.Console.WriteLine(" " + string.Format("{0,4}", (i + 1)) + " " + row);
	            }
            }
	    }

        /*----------------------------------------------------------------
         * Name			: ShowByLevelBook
         * Description	: dumps the current order book to the console
         * Arguments	: none
         * Returns		: none
         *---------------------------------------------------------------*/
        private void ShowByLevelBook()
        {
            int i;
            int size;
            int[] bookSize = new int[SIZE];

            lock (d_consoleWrite)
            {
                ByLevelBook[] book = d_levelBooks;

                // get BID/ASK size
                bookSize[BIDSIDE] = book[BIDSIDE].BookSize;
                bookSize[ASKSIDE] = book[ASKSIDE].BookSize;
                size = bookSize[BIDSIDE] > bookSize[ASKSIDE] ? bookSize[BIDSIDE] : bookSize[ASKSIDE];

                int offset = 0;
                if (d_pricePrecision < 4)
                    offset = 0;
                else
                    offset = d_pricePrecision - 4;

                System.Console.WriteLine("-------------------------------------------------------------------------------------------------");
                System.Console.WriteLine("MAXIMUM WINDOW SIZE: " + book[BIDSIDE].WindowSize);
                System.Console.WriteLine("BOOK TYPE          : " + book[BIDSIDE].BookType);
                System.Console.WriteLine("-------------------------------------------------------------------------------------------------");
                System.Console.WriteLine("                 --- BID ---                                 --- ASK ---");
                System.Console.WriteLine(" POS     PRICE" + string.Format("{0, " + (offset + 3) + "}", "") +
                    "   SIZE   #-ORD    TIME       ---      PRICE" + string.Format("{0, " + (offset + 3) + "}", "") +
                    "   SIZE   #-ORD    TIME");

                for (i = 0; i < size; ++i)
                {
                    string row;

                    // format book for bid side
                    BookEntry entry = book[BIDSIDE].getEntry(i);
                    if (entry != null)
                    {
                        row = string.Format("{0,8:F" + d_pricePrecision + "}", entry.Price) + " ";
                        row += string.Format("{0,9}", entry.Size) + " ";
                        row += string.Format("{0,5}" , entry.NumberOrders) + " ";
                        row += string.Format("{0,13}", entry.Time) + " ";
                    }
                    else
                        row = string.Format("{0, " + (39 + offset) + "}", ""); 

                    // format book or ask side
                    entry = book[ASKSIDE].getEntry(i);
                    if (entry != null)
                    {
                        row += " ---    ";
                        row += string.Format("{0,8:F" + d_pricePrecision + "}", entry.Price) + " ";
                        row += string.Format("{0,9}", entry.Size) + " ";
                        row += string.Format("{0,5}", entry.NumberOrders) + " ";
                        row += string.Format("{0,13}", entry.Time) + " ";
                    }
                    // display row
                    System.Console.WriteLine(" " + string.Format("{0,3}", (i + 1)) + "   " + row);
                }
            }
        }

        /*------------------------------------------------------------------------------------
         * Name			: parseCommandLine
         * Description	: process command line parameters
         * Arguments	: none
         * Returns		: true - successful, false - failed
         *------------------------------------------------------------------------------------*/
        private bool parseCommandLine(string[] args)
        {
            string subscriptionOptions = "";
            
            for (int i = 0; i < args.Length; ++i)
            {
                if (string.Compare(args[i], "-s", true) == 0
					&& i + 1 < args.Length)
                {
                    d_security = args[++i];
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
                else if (string.Compare(args[i], "-pr", true) == 0
					&& i + 1 < args.Length)
                {
                    int outPrecision = 0;
                    if (int.TryParse(args[++i], out outPrecision))
                    {
                        d_pricePrecision = outPrecision;
                    }
                }
                else if (string.Compare(args[i], "-st", true) == 0
					&& i + 1 < args.Length)
                {
                    d_showTicks = true;
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

            lock (d_consoleWrite)
            {
                // check for application name
                if ((d_authOption == "APPLICATION" || d_authOption == "USER_APP") && (d_name == ""))
                {
                    System.Console.WriteLine("Application name cannot be NULL for application authorization.");
                    printUsage();
                    return false;
                }
                // check for directory service and application name
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

                if (d_security.Trim().Length == 0)
                {
                    d_security = MKTDEPTH_SVC + "/ticker/VOD LN Equity";
                }

                if (d_options.Count == 0)
                {
                    // by order
                    d_options.Add("type=MBO");
                }

                // construct subscription option string
                foreach (string option in d_options)
                {
                    if (subscriptionOptions.Length == 0)
                    {
                        subscriptionOptions = "?" + option;
                    }
                    else
                    {
                        subscriptionOptions += "&" + option;
                    }
                }

                // default to UNKNOWN book type
                d_marketDepthBook = UNKNOWN;

                // add market depth service to security
                int index = d_security.IndexOf("/");
                if (index != 0)
                {
                    d_security = "/" + d_security;
                }
                index = d_security.IndexOf("//");
                if (index != 0)
                {
                    d_security = MKTDEPTH_SVC + d_security;
                }

                Subscription subscription = new Subscription(d_security + subscriptionOptions,
                    new CorrelationID(d_security));
                d_subscriptions.Add(subscription);
                System.Console.WriteLine("Subscription string: " + subscription.SubscriptionString);
            }
            return true;
        }

        /*------------------------------------------------------------------------------------
         * Name			: printUsage
         * Description	: prints the usage of the program on command line
         * Arguments	: none
         * Returns		: none
         *------------------------------------------------------------------------------------*/
        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("  Retrieve realtime market depth data using Bloomberg V3 API");
            System.Console.WriteLine(@"      [-s     <security   = ""/ticker/VOD LN Equity"">");
            System.Console.WriteLine("      [-o     <type=MBO, type=MBL, type=TOP or type=MMQ>");
            System.Console.WriteLine("      [-pr    <precision  = 4>");
            System.Console.WriteLine("      [-st    <show ticks>");
            System.Console.WriteLine("      [-ip    <ipAddress  = localhost>");
            System.Console.WriteLine("      [-p     <tcpPort    = 8194>");
            System.Console.WriteLine("      [-auth  <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>]");
            System.Console.WriteLine("      [-n     <name = applicationName>]");
            System.Console.WriteLine("      [-ds    <Directory Service name = directoryService>]");
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine("1) Specify only LOGON to authorize 'user' using Windows login name.");
            System.Console.WriteLine("2) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
            System.Console.WriteLine("3) Specify APPLICATION and name(Application Name) to authorize application.\n");
            System.Console.WriteLine("Press ENTER to quit");
            Console.Read();
        }

        /*------------------------------------------------------------------------------------
         * Name			: printMenu
         * Description	: print usage menu
         * Arguments	: none
         * Returns		: none
         *------------------------------------------------------------------------------------*/
        private void printMenu()
	    {
            System.Console.WriteLine("-------------------------------------------------------------");
            System.Console.WriteLine(" Enter 'v' or 'V' to show the current market depth cache book");
            System.Console.WriteLine(" Enter 't' or 'T' to toggle show ticks on/off");
            System.Console.WriteLine(" Enter 'q' or 'Q' to quit");
            System.Console.WriteLine("-------------------------------------------------------------");
	    }

    }
}