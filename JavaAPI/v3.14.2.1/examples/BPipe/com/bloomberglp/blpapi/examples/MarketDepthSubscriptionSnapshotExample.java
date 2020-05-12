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
package com.bloomberglp.blpapi.examples;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.io.*;
import java.nio.channels.Channel;

import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Datetime;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.EventQueue;
import com.bloomberglp.blpapi.EventHandler;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.Identity;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Subscription;
import com.bloomberglp.blpapi.SubscriptionList;

public class MarketDepthSubscriptionSnapshotExample
{
    private static final Name EXCEPTIONS = Name.getName("exceptions");
    private static final Name FIELD_ID = Name.getName("fieldId");
    private static final Name SOURCE = Name.getName("source");
    private static final Name ERROR_CODE = Name.getName("errorCode");
    private static final Name REASON = Name.getName("reason");
    private static final Name CATEGORY = Name.getName("category");
    private static final Name SUBCATEGORY = Name.getName("subcategory");
    private static final Name DESCRIPTION = Name.getName("description");
	private static final Name AUTHORIZATION_SUCCESS = Name.getName("AuthorizationSuccess");
	private static final Name AUTHORIZATION_FAILURE = Name.getName("AuthorizationFailure");
	private static final Name TOKEN_SUCCESS = Name.getName("TokenGenerationSuccess");
	private static final Name TOKEN_FAILURE = Name.getName("TokenGenerationFailure");

    final int BIDSIDE = 0;
    final int ASKSIDE = 1;
   
    final int UNKNOWN = -1;
    final int BYORDER = 0;
    final int BYLEVEL = 1;

    final int SIZE = 2;

	private SessionOptions    d_sessionOptions;
    private Session           d_session;
    private String			  d_security;
    private ArrayList<String> d_options;
    private SubscriptionList  d_subscriptions;
    private SimpleDateFormat  d_dateFormat;
	private String  		  d_authOption;
	private String  		  d_name;
	private String			  d_dsName;
	private Identity 		  d_identity;
	private SubscriptionEventHandler  d_eventHandler;
	private ArrayList<String> d_hosts;
	private Integer           d_port;
    private String 			  d_token;
    private String 			  d_authServiceName;
    private String 			  d_mktDepthServiceName;
    
    private ByOrderBook[] 	  d_orderBooks;
    private ByLevelBook[] 	  d_levelBooks;
    int 					  d_marketDepthBook[];
    int 	d_pricePrecision;
    boolean d_showTicks;
    String d_consoleWrite;

    /**
     * @param args
     */
    public static void main(String[] args) throws java.lang.Exception
    {
        System.out.println("Realtime Market Depth Snapshot Subscription Example");
        MarketDepthSubscriptionSnapshotExample example = new MarketDepthSubscriptionSnapshotExample();
        example.run(args);
    }

    public MarketDepthSubscriptionSnapshotExample()
    {
        d_port = 8194;
        d_hosts = new ArrayList<String>();
    	d_sessionOptions = new SessionOptions();
        
        d_authOption = "NONE";
        d_security = "";
        d_options = new ArrayList<String>();
        d_subscriptions = new SubscriptionList();
        d_dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss.SSSZ");
        d_name = "";
        d_dsName = "";
        d_pricePrecision = 4;
        d_marketDepthBook = new int[] {UNKNOWN};
        
        d_authServiceName = "//blp/apiauth";
        d_mktDepthServiceName = "//blp/mktdepthdata";

        d_orderBooks = new ByOrderBook[SIZE];			
        d_orderBooks[BIDSIDE] = new ByOrderBook();
        d_orderBooks[ASKSIDE] = new ByOrderBook();
        d_levelBooks = new ByLevelBook[SIZE];
        d_levelBooks[BIDSIDE] = new ByLevelBook();
        d_levelBooks[ASKSIDE] = new ByLevelBook();
        d_showTicks = false;
        d_consoleWrite = "";
    }

	/*------------------------------------------------------------------------------------
	 * Name			: createSession
	 * Description	: The create session with session option provided
	 * Arguments	: none
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
    private boolean createSession() throws Exception
    {
        if (d_session != null) d_session.stop();

		String authOptions = "";
		if(d_authOption.compareToIgnoreCase("APPLICATION") == 0){
            // Set Application Authentication Option
            authOptions = "AuthenticationMode=APPLICATION_ONLY;";
            authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
            // ApplicationName is the entry in EMRS.
            authOptions += "ApplicationName=" + d_name;
		} else {
            // Set User authentication option
            if (d_authOption.compareToIgnoreCase("LOGON") == 0)
            {
                // Authenticate user using windows/unix login name
                authOptions = "AuthenticationType=OS_LOGON";
            }
            
            else
            {
                if (d_authOption.compareToIgnoreCase("DIRSVC") == 0)
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
		}

		if (d_authOption.compareToIgnoreCase("NONE") != 0)
		{
			d_sessionOptions.setAuthenticationOptions(authOptions);
		}
		System.out.println("Authentication Options = " + d_sessionOptions.authenticationOptions());

		
		System.out.print("Connecting to port " + d_port + " on host(s):");
		SessionOptions.ServerAddress[] servers = new SessionOptions.ServerAddress[d_hosts.size()];
		for (int i = 0; i < d_hosts.size(); ++i) {
			servers[i] = new SessionOptions.ServerAddress(d_hosts.get(i), d_port);
			System.out.print(" " + d_hosts.get(i));
		}
		System.out.println("");
		
		d_sessionOptions.setServerAddresses(servers);
		d_sessionOptions.setAutoRestartOnDisconnection(true);
		d_sessionOptions.setNumStartAttempts(d_hosts.size());
		
		d_eventHandler =  new SubscriptionEventHandler(d_orderBooks, d_levelBooks, d_showTicks, d_marketDepthBook);
        d_session = new Session(d_sessionOptions, d_eventHandler);
        if (!d_session.start()) {
            System.err.println("Failed to start session");
            return false;
        }
        System.out.println("Connected successfully\n");

        if (!d_session.openService(d_authServiceName)) {
            System.err.println("Failed to open service: " + d_authServiceName);
            d_session.stop();
            return false;
        }

        if (!d_session.openService(d_mktDepthServiceName)) {
            System.err.println("Failed to open service: " + d_mktDepthServiceName);
            d_session.stop();
            return false;
        }
        
        return true;
    }

    /*------------------------------------------------------------------------------------
     * Name			: generateToken
     * Description	: generate token for user/application
     * Arguments	: none
     * Returns		: true - successful, false - failed
     *------------------------------------------------------------------------------------*/
    private Boolean generateToken()  throws Exception
    {
    	Boolean isTokenSuccess = false;
    	EventQueue queue = new EventQueue();
		CorrelationID tokenReqId = new CorrelationID(99);
		d_session.generateToken(tokenReqId, queue);

		int time = 0;
		while (true) {
			Event event = queue.nextEvent(1000);
			if (event.eventType() == Event.EventType.TOKEN_STATUS) {
				time = 0;
				isTokenSuccess = processTokenStatus(event);
				break;
			} else if (event.eventType() == Event.EventType.TIMEOUT) {
				if (++ time > 20) {
					System.err.println("Generate token timeout");
					break;
				}
			}
		}
    	
    	return isTokenSuccess;
    }
    
    /*------------------------------------------------------------------------------------
     * Name			: processTokenStatus
     * Description	: process generate token event
     * Arguments	: event is the generate token event
     * Returns		: true - successful, false - failed
     *------------------------------------------------------------------------------------*/
	boolean processTokenStatus(Event event) throws Exception {
		System.out.println("processTokenEvents");
		MessageIterator msgIter = event.messageIterator();
		while (msgIter.hasNext()) {
			Message msg = msgIter.next();
			System.out.println(msg);

			if (msg.messageType() == TOKEN_SUCCESS) {
				d_token = msg.getElementAsString("token");

			} else if (msg.messageType() == TOKEN_FAILURE) {
				return false;
			}
		}
		return true;
	}
	
	/*------------------------------------------------------------------------------------
	 * Name			: isBPipeAuthorized
	 * Description	: authorize user/application
	 * Arguments	: none
	 * Returns		: true - successful, false - failed
	 *------------------------------------------------------------------------------------*/
	boolean isBPipeAuthorized() throws Exception 
	{
		boolean isRunning = true;
		// authorization request
		EventQueue authEventQueue = new EventQueue();
		Service authService = d_session.getService(d_authServiceName);
		Request authRequest = authService.createAuthorizationRequest();
		authRequest.set("token", d_token);
		// send authorization request
		d_identity = d_session.createIdentity();
		d_session.sendAuthorizationRequest(authRequest, d_identity, authEventQueue, new CorrelationID(1));

		while(isRunning)
		{
			// process authorization event
			Event event = authEventQueue.nextEvent();
			System.out.println("process auth Events");
			MessageIterator msgIter = event.messageIterator();
			while (msgIter.hasNext()) {
				Message msg = msgIter.next();
				if (msg.messageType() == AUTHORIZATION_SUCCESS) {
					// success
					System.out.println("Authorization SUCCESS");
					isRunning = false;
				} else if (msg.messageType() == AUTHORIZATION_FAILURE) {
					// authorization failed
					System.out.println("Authorization FAILED");
					System.out.println(msg);
					return false;
				} else {
					System.out.println(msg);
					if (event.eventType() == Event.EventType.RESPONSE) {
						System.out.println("Got Final Response");
						return false;
					}
				}
			}
		}
		return true;
	}

	/*------------------------------------------------------------------------------------
	 * Name			: run 
	 * Description	: start application process
	 * Arguments	: args are the argument values
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
	private void run(String[] args) throws Exception
    {
        if (!parseCommandLine(args)) return;
        if (!createSession()) return;

        if (d_authOption == "NONE")
        {
        	System.out.println("Subscribing without Identity...");
        	d_session.subscribe(d_subscriptions);
        }
        else
        {
            // Authenticate user using Generate Token Request 
            if (!generateToken()) return;

            //Authorization : pass Token into authorization request. Returns User handle with user's entitlements info set by server.
            if (!isBPipeAuthorized()) return;

            System.out.println("Subscribing with Identity...\n");
            d_session.subscribe(d_subscriptions, d_identity);
        }
        
        byte[] keys = new byte[10];
        boolean displayMenu = true;
        while (true)
        {
        	if (displayMenu) {
	        	synchronized (d_consoleWrite) {
					printMenu();
				}
	        	displayMenu = false;
        	}

    		if (System.in.read(keys, 0, 10) >= 0){
	        	// take only the first key and ignore the rest
	    		char key = (char)keys[0];
		        System.out.println("");
		        
		        if ((key == 'v') || (key == 'V'))
		        {
		        	// view market depth book
		        	switch (d_marketDepthBook[0])
		        	{
			        	case BYLEVEL:
			        		ShowByLevelBook();
			        		break;
			        	case BYORDER:
			        		ShowByOrderBook();
			        		break;
			        	default:
			        		System.out.println("Unknown book type");
			        		break;
		        	}
		        	displayMenu = true;
		        } else if ((key == 't') || (key == 'T')) {
		        	// show ticks
		        	d_showTicks = !d_showTicks;
		        	d_eventHandler.showTicks(d_showTicks);
		        	displayMenu = true;
		        } else if ((key == 'q') || (key == 'Q')) {
		        	// quite
		        	break;
		        } 
    		}
        }
        
        // unsubscribe
        d_session.unsubscribe(d_subscriptions);
        // stop session
        d_session.stop();
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

        synchronized (d_consoleWrite) {
	        ByOrderBook[] book = d_orderBooks;
			// get BID/ASK size
            bookSize[BIDSIDE] = book[BIDSIDE].getBookSize();
            bookSize[ASKSIDE] = book[ASKSIDE].getBookSize();
            size = bookSize[BIDSIDE] > bookSize[ASKSIDE] ? bookSize[BIDSIDE] : bookSize[ASKSIDE];

            int offset = 0;
            if (d_pricePrecision < 4)
		        offset = 0;
            else
	            offset = d_pricePrecision - 4;

	        System.out.println("-------------------------------------------------------------------------------------------------");
	        System.out.println("MAXIMUM WINDOW SIZE: " + book[BIDSIDE].getWindowSize());
		    System.out.println("BOOK TYPE          : " + book[BIDSIDE].getBookType());
	        System.out.println("-------------------------------------------------------------------------------------------------");
	        System.out.println("                 --- BID ---                                     --- ASK ---");
		    System.out.println(" POS  BROKER  PRICE" +  String.format("%" + (offset + 5) + "s", "") + 
                "SIZE        TIME     ---      BROKER  PRICE" + String.format("%" + (offset + 5) + "s", "") +
                "SIZE        TIME   ");

	        for (i=0; i<size; ++i)
	        {
		        String row;

		        // format book for bid side
		        BookEntry entry = book[BIDSIDE].getEntry(i);
		        if (entry != null) 
		        {
			        row = String.format("%6s", entry.getBroker()) + " ";
			        row += String.format("%9." + d_pricePrecision + "f", entry.getPrice()) + " ";
                    row += String.format("%6s", entry.getSize()) + " ";
                    row += String.format("%13s", entry.getTime()) + " ";
		        }
		        else
                    row = String.format("%" + (38 + offset) + "s", "");

		        // format book or ask side
                entry = book[ASKSIDE].getEntry(i);
                if (entry != null)
		        {
                    row += "  ---     ";
			        row += String.format("%6s", entry.getBroker()) + " ";
			        row += String.format("%9." + d_pricePrecision + "f", entry.getPrice()) + " ";
                    row += String.format("%6s", entry.getSize()) + " ";
                    row += String.format("%13s", entry.getTime()) + " ";
		        }
                // display row
                System.out.println(" " + String.format("%3s", (i + 1)) + " " + row);
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

        synchronized (d_consoleWrite) {
            ByLevelBook[] book = d_levelBooks;
            // get BID/ASK size
            bookSize[BIDSIDE] = book[BIDSIDE].getBookSize();
            bookSize[ASKSIDE] = book[ASKSIDE].getBookSize();
            size = bookSize[BIDSIDE] > bookSize[ASKSIDE] ? bookSize[BIDSIDE] : bookSize[ASKSIDE];

            int offset = 0;
            if (d_pricePrecision < 4)
                offset = 0;
            else
                offset = d_pricePrecision - 4;

            System.out.println("-------------------------------------------------------------------------------------------------");
            System.out.println("MAXIMUM WINDOW SIZE: " + book[BIDSIDE].getWindowSize());
            System.out.println("BOOK TYPE          : " + book[BIDSIDE].getBookType());
            System.out.println("-------------------------------------------------------------------------------------------------");
            System.out.println("                 --- BID ---                                 --- ASK ---");
            System.out.println(" POS     PRICE" + String.format("%" + (offset + 6) + "s", "") +
                " SIZE    #-ORD       TIME       ---    PRICE" + String.format("%" + (offset + 6) + "s", "") +
                " SIZE    #-ORD       TIME");

            for (i = 0; i < size; ++i)
            {
                String row;
                // format book for bid side
                BookEntry entry = book[BIDSIDE].getEntry(i);
                if (entry != null)
                {
                    row = String.format("%9." + d_pricePrecision + "f", entry.getPrice()) + " ";
                    row += String.format("%9s", entry.getSize()) + " ";
                    row += String.format("%6s" , entry.getNumberOrders()) + " ";
                    row += String.format("%13s", entry.getTime()) + " ";
                }
                else
                    row = String.format("%" + (41 + offset) + "s", ""); 

                // format book or ask side
                entry = book[ASKSIDE].getEntry(i);
                if (entry != null)
                {
                    row += "    ---  ";
                    row += String.format("%9." + d_pricePrecision + "f", entry.getPrice()) + " ";
                    row += String.format("%9s", entry.getSize()) + " ";
                    row += String.format("%6s", entry.getNumberOrders()) + " ";
                    row += String.format("%13s", entry.getTime()) + " ";
                }
                // display row
                System.out.println(" " + String.format("%3s", (i + 1)) + "   " + row);
            }
        }
    }

	/*------------------------------------------------------------------------------------
	 * Name			: parseCommandLine
	 * Description	: process command line parameters
	 * Arguments	: none
	 * Returns		: true - successful, false - failed
	 *------------------------------------------------------------------------------------*/
    private boolean parseCommandLine(String[] args)
    {
        for (int i = 0; i < args.length; ++i) {
            if (args[i].equalsIgnoreCase("-s")) {
                d_security = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-o") && i + 1 < args.length) {
                d_options.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-auth") && i + 1 < args.length) {
                d_authOption = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-n") && i + 1 < args.length) {
            	d_name = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-ds") && i + 1 < args.length) {
                d_dsName = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length) {
                d_hosts.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-p") && i + 1 < args.length) {
                d_port = Integer.parseInt(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-pr") && i + 1 < args.length) {
                d_pricePrecision = Integer.parseInt(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-st")) {
                d_showTicks = true;
            }
            else {
                printUsage();
                return false;
            }
        }
        
        synchronized (d_consoleWrite) {
	        // check for application name
	        if ((d_authOption.equalsIgnoreCase("APPLICATION") || d_authOption.equalsIgnoreCase("USER_APP")) && (d_name.length() == 0)) {
	        	System.out.println("Application name cannot be NULL for application authorization.");
	            printUsage();
	            return false;
	        }
	        
	        // check for directory service and application name
	        if (d_authOption.equalsIgnoreCase("USER_DS_APP") && (d_name.length() == 0 || d_dsName.length() == 0))
	        {
                System.out.println("Application or DS name cannot be NULL for application authorization.");
                printUsage();
                return false;
	        }
	        
	        // check for Directory Service name
	        if (d_authOption.equalsIgnoreCase("DIRSVC") && (d_name.length() == 0))
	        {
	        	System.out.println("Directory Service property name cannot be NULL for DIRSVC authorization.");
	            printUsage();
	            return false;
	        }
	
	        if (d_hosts.size() == 0) {
                System.out.println("Missing host IP");
                printUsage();
                return false;
	        }
	
	        if (d_security.length() == 0) {
	            d_security = d_mktDepthServiceName + "/ticker/VOD LN Equity";
	        }
	
			if (d_options.size() == 0) {
				d_options.add("type=MBO");
			}
				
	        // construct subscription option string
	        String subscriptionOptions = "";
	        for (String opt : d_options) {
	        	if (subscriptionOptions == "")
	        	{
	        		subscriptionOptions = "?" + opt;
	        	}
	        	else
	        	{
	        		subscriptionOptions += "&" + opt;
	        	}
	        }
	        
        	String tempSecurity = d_security;
        	if (!tempSecurity.startsWith("/"))
        	{
        		tempSecurity = "/" + tempSecurity;
        	}
        	if (!tempSecurity.startsWith("//"))
        	{
        		tempSecurity = d_mktDepthServiceName + tempSecurity;
        	}
            d_subscriptions.add(new Subscription(tempSecurity + subscriptionOptions, new CorrelationID(d_security)));
            System.out.println("Subscription string: " + d_subscriptions.get(0));
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
        System.out.println("Usage:");
        System.out.println("	Retrieve realtime market depth data using Bloomberg V3 API");
        System.out.println("		[-s			<security	= \"/bsym/LN/VOD\">");
        System.out.println("		[-o			<type=MBO, type=MBL, type=TOP or type=MMQ>");
        System.out.println("		[-pr		<precision  = 4>");
        System.out.println("		[-st		<show ticks>");
        System.out.println("		[-ip 		<ipAddress	= localhost>");
        System.out.println("		[-p 		<tcpPort	= 8194>");
		System.out.println("		[-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>]");
		System.out.println("		[-n         <name = applicationName>]");
		System.out.println("		[-ds        <Directory Service name = directoryService>]");
		System.out.println("Notes:");
		System.out.println(" -Specify only LOGON to authorize 'user' using Windows login name.");
		System.out.println(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println(" -Specify APPLICATION and name(Application Name) to authorize application.");
    }
    
	/*------------------------------------------------------------------------------------
	 * Name			: printMenu
	 * Description	: print usage menu
	 * Arguments	: none
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
    private void printMenu()
    {
        System.out.println("-------------------------------------------------------------");
        System.out.println(" Enter 'v' or 'V' to show the current market depth cache book");
        System.out.println(" Enter 't' or 'T' to toggle show ticks on/off");
        System.out.println(" Enter 'q' or 'Q' to quit");
        System.out.println("-------------------------------------------------------------");
    }

    // event handler class
    class SubscriptionEventHandler implements EventHandler
    {
    	private ByOrderBook[] d_orderBooks;
    	private ByLevelBook[] d_levelBooks;
    	private boolean d_showTicks = false;
    	private int d_marketDepthBook[];
    	private boolean d_gapDetected = false;
    	private boolean d_bidRetran = false;
    	private boolean d_askRetran = false;
    	private boolean d_resubscribed = false;
    	private long d_sequenceNumber = 0;
    	private String d_consoleWrite = "";
    	
    	private final Name MarketDepthUpdates = Name.getName("MarketDepthUpdates");
        private final Name MarketDataEvents = Name.getName("MarketDataEvents");
        private final Name MKTDEPTH_EVENT_TYPE = Name.getName("MKTDEPTH_EVENT_TYPE");
        private final Name MKTDEPTH_EVENT_SUBTYPE = Name.getName("MKTDEPTH_EVENT_SUBTYPE");
        private final Name MD_GAP_DETECTED = Name.getName("MD_GAP_DETECTED");
        private final Name MD_TABLE_CMD_RT = Name.getName("MD_TABLE_CMD_RT");
        private final Name MD_BOOK_TYPE = Name.getName("MD_BOOK_TYPE");
        private final Name MD_MULTI_TICK_UPD_RT = Name.getName("MD_MULTI_TICK_UPD_RT");
        private final Name MBO_WINDOW_SIZE = Name.getName("MBO_WINDOW_SIZE");
        private final Name MBO_ASK_POSITION_RT = Name.getName("MBO_ASK_POSITION_RT");
        private final Name MBO_ASK_RT = Name.getName("MBO_ASK_RT");
        private final Name MBO_ASK_BROKER_RT = Name.getName("MBO_ASK_BROKER_RT");
        private final Name MBO_ASK_COND_CODE_RT = Name.getName("MBO_ASK_COND_CODE_RT");
        private final Name MBO_ASK_SIZE_RT = Name.getName("MBO_ASK_SIZE_RT");
        private final Name MBO_TABLE_ASK = Name.getName("MBO_TABLE_ASK");
        private final Name MBO_BID_POSITION_RT = Name.getName("MBO_BID_POSITION_RT");
        private final Name MBO_BID_RT = Name.getName("MBO_BID_RT");
        private final Name MBO_BID_BROKER_RT = Name.getName("MBO_BID_BROKER_RT");
        private final Name MBO_BID_COND_CODE_RT = Name.getName("MBO_BID_COND_CODE_RT");
        private final Name MBO_BID_SIZE_RT = Name.getName("MBO_BID_SIZE_RT");
        private final Name MBO_TABLE_BID = Name.getName("MBO_TABLE_BID");
        private final Name MBO_TIME_RT = Name.getName("MBO_TIME_RT");
        private final Name MBO_SEQNUM_RT = Name.getName("MBO_SEQNUM_RT");
        private final Name MBL_WINDOW_SIZE = Name.getName("MBL_WINDOW_SIZE");
        private final Name MBL_ASK_POSITION_RT = Name.getName("MBL_ASK_POSITION_RT");
        private final Name MBL_ASK_RT = Name.getName("MBL_ASK_RT");
        private final Name MBL_ASK_NUM_ORDERS_RT = Name.getName("MBL_ASK_NUM_ORDERS_RT");
        private final Name MBL_ASK_COND_CODE_RT = Name.getName("MBL_ASK_COND_CODE_RT");
        private final Name MBL_ASK_SIZE_RT = Name.getName("MBL_ASK_SIZE_RT");
        private final Name MBL_TABLE_ASK = Name.getName("MBL_TABLE_ASK");
        private final Name MBL_BID_POSITION_RT = Name.getName("MBL_BID_POSITION_RT");
        private final Name MBL_BID_RT = Name.getName("MBL_BID_RT");
        private final Name MBL_BID_NUM_ORDERS_RT = Name.getName("MBL_BID_NUM_ORDERS_RT");
        private final Name MBL_BID_COND_CODE_RT = Name.getName("MBL_BID_COND_CODE_RT");
        private final Name MBL_BID_SIZE_RT = Name.getName("MBL_BID_SIZE_RT");
        private final Name MBL_TABLE_BID = Name.getName("MBL_TABLE_BID");
        private final Name MBL_TIME_RT = Name.getName("MBL_TIME_RT");
        private final Name MBL_SEQNUM_RT = Name.getName("MBL_SEQNUM_RT");
        private final Name NONE = Name.getName("NONE");

        private final Name ADD = Name.getName("ADD");
        private final Name DEL = Name.getName("DEL");
        private final Name DELALL = Name.getName("DELALL");
        private final Name DELBETTER = Name.getName("DELBETTER");
        private final Name DELSIDE = Name.getName("DELSIDE");
        private final Name EXEC = Name.getName("EXEC");
        private final Name MOD = Name.getName("MOD");
        private final Name REPLACE = Name.getName("REPLACE");
        private final Name REPLACE_BY_BROKER = Name.getName("REPLACE_BY_BROKER");
        private final Name CLEARALL = Name.getName("CLEARALL");
        private final Name REPLACE_CLEAR = Name.getName("REPLACE_CLEAR");
        private final Name REPLACE_BY_PRICE = Name.getName("REPLACE_BY_PRICE");
    	private final Name MARKET_BY_LEVEL = Name.getName("MARKET_BY_LEVEL");
    	private final Name MARKET_BY_ORDER = Name.getName("MARKET_BY_ORDER");

        private final Name ASK = Name.getName("ASK");
        private final Name BID = Name.getName("BID");
        private final Name ASK_RETRANS = Name.getName("ASK_RETRANS");
        private final Name BID_RETRANS = Name.getName("BID_RETRANS");
        private final Name TABLE_INITPAINT = Name.getName("TABLE_INITPAINT");
        private final Name TABLE_UPDATE = Name.getName("TABLE_UPDATE");

        private final Name[][] PRICE_FIELD = {{MBO_BID_RT, MBO_ASK_RT}, 
        		      					{MBL_BID_RT, MBL_ASK_RT}};
        private final Name[][] SIZE_FIELD = {{MBO_BID_SIZE_RT, MBO_ASK_SIZE_RT}, 
        								{MBL_BID_SIZE_RT, MBL_ASK_SIZE_RT}};
        private final Name[][] POSITION_FIELD = {{MBO_BID_POSITION_RT, MBO_ASK_POSITION_RT}, 
    		     							{MBL_BID_POSITION_RT, MBL_ASK_POSITION_RT}};
        private final Name[][] ORDER_FIELD = {{NONE, NONE},
    		      						{MBL_BID_NUM_ORDERS_RT, MBL_ASK_NUM_ORDERS_RT}};
        private final Name[][] BROKER_FIELD = {{MBO_BID_BROKER_RT, MBO_ASK_BROKER_RT}, 
    		       {						NONE, NONE}};
        private final Name[] TIME_FIELD = {MBO_TIME_RT, MBL_TIME_RT};

        final int BIDSIDE = 0;
        final int ASKSIDE = 1;
       
        final int UNKNOWN = -1;
        final int BYORDER = 0;
        final int BYLEVEL = 1;

    	/*------------------------------------------------------------------------------------
    	 * Name			: SubscriptionEventHandler
    	 * Description	: event handler constructor
    	 * Arguments	: none
    	 * Returns		: none
    	 *------------------------------------------------------------------------------------*/
    	public SubscriptionEventHandler(ByOrderBook[] orderBook, ByLevelBook[] levelBook,
    			boolean showTicks, int[] marketDepthBook)
    	{
    		d_orderBooks = orderBook;
    		d_levelBooks = levelBook;
    		d_showTicks = showTicks;
    		d_marketDepthBook = marketDepthBook;
    	}
    	
    	/*------------------------------------------------------------------------------------
    	 * Name			: showTicks
    	 * Description	: show tick data flag
    	 * Arguments	: none
    	 * Returns		: none
    	 *------------------------------------------------------------------------------------*/
    	void showTicks(Boolean tick)
    	{
    		synchronized (d_consoleWrite) {
    			d_showTicks = tick;
    		}
    	}

    	/*------------------------------------------------------------------------------------
    	 * Name			: processEvent
    	 * Description	: process events
    	 * Arguments	: none
    	 * Returns		: true - successful, false - failed
    	 *------------------------------------------------------------------------------------*/
        public void processEvent(Event event, Session session)
        {
            try {
                switch (event.eventType().intValue())
                {                
                case Event.EventType.Constants.SUBSCRIPTION_DATA:
                    processSubscriptionDataEvent(event, session);
                    break;
                case Event.EventType.Constants.SUBSCRIPTION_STATUS:
                    processSubscriptionStatus(event, session);
                    break;
                default:
                    processMiscEvents(event, session);
                    break;
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        }

    	/*------------------------------------------------------------------------------------
    	 * Name			: processSubscriptionStatus
    	 * Description	: process subscription status events
    	 * Arguments	: none
    	 * Returns		: true - successful
    	 *------------------------------------------------------------------------------------*/
        private boolean processSubscriptionStatus(Event event, Session session)
        throws Exception
        {
            System.out.println("Processing SUBSCRIPTION_STATUS: " + event.eventType().toString());
            MessageIterator msgIter = event.messageIterator();
            while (msgIter.hasNext()) {
                Message msg = msgIter.next();
                String topic = msg.correlationID().object().toString();
                System.out.println(
                        d_dateFormat.format(Calendar.getInstance().getTime()) +
                        ": " + topic + " - " + msg.messageType());

                if (msg.hasElement(REASON, true)) {
                    // This can occur on SubscriptionFailure.
                	String temp = "";
                    Element reason = msg.getElement(REASON);
                    if (reason.hasElement(SOURCE, true)){
                    	temp = "\tsource: " + reason.getElement(SOURCE).getValueAsString();
                    }	
                    if (reason.hasElement(ERROR_CODE, true)){
                    	temp += "\n\terrorCode: " + reason.getElement(ERROR_CODE).getValueAsString();
                    }	
                    if (reason.hasElement(CATEGORY, true)){
                    	temp += "\n\tcategory: " + reason.getElement(CATEGORY).getValueAsString();
                    }	
                    if (reason.hasElement(DESCRIPTION, true)){
                    	temp += "\n\tdescription: " + reason.getElement(DESCRIPTION).getValueAsString();
                    }	
                    if (reason.hasElement(SUBCATEGORY, true)){
                    	temp += "\n\tsubcategory: " + reason.getElement(SUBCATEGORY).getValueAsString();
                    }	
                    System.out.println(temp);
                }

                if (msg.hasElement(EXCEPTIONS, true)) {
                    // This can occur on SubscriptionStarted if at least
                    // one field is good while the rest are bad.
                    Element exceptions = msg.getElement(EXCEPTIONS);
                    for (int i = 0; i < exceptions.numValues(); ++i) {
                        Element exInfo = exceptions.getValueAsElement(i);
                        Element fieldId = exInfo.getElement(FIELD_ID);
                        Element reason = exInfo.getElement(REASON);
                        System.out.println("\t" + fieldId.getValueAsString() +
                                ": " + reason.getElement(CATEGORY).getValueAsString() +
                                " " + reason.getElement(DESCRIPTION).getValueAsString());
                    }
                }
                System.out.println("");
            }
            return true;
        }

    	/*------------------------------------------------------------------------------------
    	 * Name			: processSubscriptionDataEvent
    	 * Description	: process market depth data events
    	 * Arguments	: event is the data event
    	 *              : session is the API session
    	 * Returns		: none
    	 *------------------------------------------------------------------------------------*/
        private boolean processSubscriptionDataEvent(Event event, Session session)
        throws Exception
        {
            //System.out.println("Processing SUBSCRIPTION_DATA");
            MessageIterator msgIter = event.messageIterator();
            while (msgIter.hasNext()) {
                Message msg = msgIter.next();
                // Market Depth data
                if (d_showTicks)
                {
                	// output tick message
                    synchronized (d_consoleWrite) {
                        // output message
                        System.out.println(d_dateFormat.format(Calendar.getInstance().getTime()) + ": Fragment Type - " +
                            msg.fragmentType().toString());
                        System.out.println(msg.toString());
                    }
                }
                
                // setup book type before processing data
                if (d_marketDepthBook[0] == UNKNOWN)
                {
                	if (msg.hasElement(MKTDEPTH_EVENT_TYPE))
                	{
                		Element bookType = msg.getElement(MKTDEPTH_EVENT_TYPE);
                		Name value = bookType.getValueAsName();
                		if (value.equals(MARKET_BY_ORDER))
                		{
                			d_marketDepthBook[0] = BYORDER;
                		}
                		else if (value.equals(MARKET_BY_LEVEL))
                		{
                			d_marketDepthBook[0] = BYLEVEL;
                		}
                	}
                }

				// process base on book type
                switch (d_marketDepthBook[0])
                {
                    case BYLEVEL:
                        processByLevelMessage(msg);
                        break;
                    case BYORDER:
                        processByOrderMessage(msg);
                        break;
                    default:
                    	System.out.println(d_dateFormat.format(Calendar.getInstance().getTime()) + ": Unknown book type. Can not process message.");
                        System.out.println(d_dateFormat.format(Calendar.getInstance().getTime()) + ": Fragment Type - " +
                                msg.fragmentType().toString());
                            System.out.println(msg.toString());
                    	break;
                }
            }
            return true;
        }

    	/*------------------------------------------------------------------------------------
    	 * Name			: processByOrderEvent
    	 * Description	: process by order message
    	 * Arguments	: msg is the tick data message
    	 *              : session is the API session
    	 * Returns		: none
    	 *------------------------------------------------------------------------------------*/
        private boolean processByOrderMessage(Message msg) throws IOException
        {
            int side = -1;
		    int position = -1;
		    boolean bidRetran = false;
		    boolean askRetran = false;
		    
		    // get gap detection flag (AMD book only)
		    if (msg.hasElement(MD_GAP_DETECTED, true) && !d_gapDetected) {
		    	d_gapDetected = true;
		    	synchronized (d_consoleWrite) {
		    		System.out.println("Bloomberg detected a gap in data stream.");
				}
		    }

		    // get event sub type 
            Name subType = msg.getElement(MKTDEPTH_EVENT_SUBTYPE).getValueAsName();
            // get retran flags
            bidRetran = subType.equals(BID_RETRANS);
            askRetran = subType.equals(ASK_RETRANS);
            // BID or ASK message
		    if (subType.equals(BID) || subType.equals(ASK) 
		    		|| bidRetran || askRetran) {
			    if(subType.equals(BID) || bidRetran) {
			    	// BID side
				    side = BIDSIDE;
			    } else if (subType.equals(ASK) || askRetran) {
			    	// ASK side
			    	side = ASKSIDE;
			    }

			    // get position
			    position = -1;
			    if (msg.hasElement(POSITION_FIELD[BYORDER][side], true)) {
				    position = msg.getElement(POSITION_FIELD[BYORDER][side]).getValueAsInt32();
				    if (position > 0) --position;
			    }
			    
			    //  BID/ASK retran message
			    if (askRetran || bidRetran) {
				    // check for multi tick
			    	if (msg.hasElement(MD_MULTI_TICK_UPD_RT, true)) {
			    		// multi tick
			    		if (msg.getElement(MD_MULTI_TICK_UPD_RT).getValueAsInt32() == 0 ) {
					    	// last multi tick message, reset sequence number so next non-retran
			    			// message sequence number will be use as new starting number
					    	d_sequenceNumber = 0;
			    			if (askRetran && d_askRetran) {
			    				// end of ask retran
			    				d_askRetran = false;
			    		    	synchronized (d_consoleWrite) {
			    		    		System.out.println("Ask retran completed.");
			    				}
			    			} else if (bidRetran && d_bidRetran) {
			    				// end of ask retran
			    				d_bidRetran = false;
			    		    	synchronized (d_consoleWrite) {
			    		    		System.out.println("Bid retran completed.");
			    				}
			    			}
			    			if (!(d_askRetran || d_bidRetran)) {
			    				// retran completed
			    		    	synchronized (d_consoleWrite) {
			    		    		if (d_gapDetected) {
			    		    			// gap detected retran completed
			    		    			d_gapDetected = false;
			    		    			System.out.println("Gap detected retran completed.");
			    		    		} else {
			    		    			// normal retran completed
			    		    			System.out.println("Retran completed.");
			    		    		}
			    				}
			    			}
			    		} else {
			    			if (askRetran && !d_askRetran) {
			    				// start of ask retran
			    				d_askRetran = true;
			    		    	synchronized (d_consoleWrite) {
			    		    		System.out.println("Ask retran started.");
			    				}
			    			} else if (bidRetran && !d_bidRetran) {
			    				// start of ask retran
			    				d_bidRetran = true;
			    		    	synchronized (d_consoleWrite) {
			    		    		System.out.println("Bid retran started.");
			    				}
			    			}
			    		}
			    	}
			    } else if (msg.hasElement(MBO_SEQNUM_RT, true)) {
			    	// get sequence number
			    	long currentSequence = msg.getElementAsInt64(MBO_SEQNUM_RT);
			    	if (d_sequenceNumber == 0 || d_sequenceNumber == 1 ||
	                        (currentSequence == 1 && d_sequenceNumber > 1)) {
			    		// use current sequence number
			    		d_sequenceNumber = currentSequence;
			    	} else if ((d_sequenceNumber + 1 != currentSequence) && !d_gapDetected) {
			    		if (!d_resubscribed)
			    		{
					    	// previous tick sequence can not be smaller than current tick 
					    	// sequence number - 1 and NOT in gap detected mode. 
				    		synchronized (d_consoleWrite) {
				    			System.out.println("Warning: Gap detected - previous sequence number is " + 
				    					d_sequenceNumber + " and current tick sequence number is " +
				    					currentSequence + ").");
				    		}
				    		// gap detected, re-subscribe to securities
							d_session.resubscribe(d_subscriptions);
							d_resubscribed = true;
			    		}
			    	} else if (d_sequenceNumber >= currentSequence) {
			    		// previous tick sequence number can not be greater or equal
			    		// to current sequence number
			    		synchronized (d_consoleWrite) {
			    			System.out.println("Warning: Current Sequence number (" + currentSequence + 
			    					") is smaller or equal to previous tick sequence number (" +
			    					d_sequenceNumber + ").");
			    		}
			    	} else {
			    		// save current sequence number
			    		d_sequenceNumber = currentSequence;
			    	}
			    }

			    // get command
			    Name cmd = msg.getElement(MD_TABLE_CMD_RT).getValueAsName();
			    if (cmd.equals(CLEARALL)) {
				    d_orderBooks[side].doClearAll();
			    } else if (cmd.equals(DEL)) {
					    d_orderBooks[side].doDel(position);
			    } else if (cmd.equals(DELALL)) {
				    d_orderBooks[side].doDelAll();
			    } else if (cmd.equals(DELBETTER)) {
				    d_orderBooks[side].doDelBetter(position);
			    } else if (cmd.equals(DELSIDE)) {
				    d_orderBooks[side].doDelSide();
			    } else if (cmd.equals(REPLACE_CLEAR)) {
				    d_orderBooks[side].doReplaceClear(position);
			    } else {
				    // process other data commands
				    // get price
			    	double price = msg.getElement(PRICE_FIELD[BYORDER][side]).getValueAsFloat64();
				    // get size
				    int size = 0;
				    if (msg.hasElement(SIZE_FIELD[BYORDER][side], true)) {
					    size = (int)msg.getElement(SIZE_FIELD[BYORDER][side]).getValueAsInt64();
				    }
				    // get broker
				    String broker = "";
				    if (msg.hasElement(BROKER_FIELD[BYORDER][side], true)) {
					    broker = msg.getElement(BROKER_FIELD[BYORDER][side]).getValueAsString();
				    }
				    // get time
				    Datetime timeStamp = msg.getElement(TIME_FIELD[BYORDER]).getValueAsDatetime();
				    // create entry
				    BookEntry  entry = new BookEntry(broker, price, String.format("%02d", timeStamp.hour()) + 
				    		":" + String.format("%02d", timeStamp.minute()) + 
				    		":" + String.format("%02d", timeStamp.second()) + 
				    		"." + String.format("%03d", timeStamp.milliSecond()), 0, size);

				    // process data command
				    if(cmd.equals(ADD))
					    d_orderBooks[side].doAdd(position, entry);
				    else if(cmd.equals(MOD))
					    d_orderBooks[side].doMod(position, entry);
				    else if(cmd.equals(REPLACE))
					    d_orderBooks[side].doReplace(position, entry);
				    else if(cmd.equals(REPLACE_BY_BROKER))
					    d_orderBooks[side].doReplaceByBroker(entry);
				    else if(cmd.equals(EXEC))
					    d_orderBooks[side].doExec(position, entry);
			    }
		    } else if (subType.equals(TABLE_INITPAINT)) {
			    // init paint 
			    if (msg.fragmentType() == Message.Fragment.START ||
				    msg.fragmentType() == Message.Fragment.NONE) {
				    if (msg.hasElement(MBO_WINDOW_SIZE, true) ){
					    d_orderBooks[ASKSIDE].setWindowSize((int)msg.getElementAsInt64(MBO_WINDOW_SIZE));
                        d_orderBooks[BIDSIDE].setWindowSize(d_orderBooks[ASKSIDE].getWindowSize());
				    }
				    d_orderBooks[ASKSIDE].setBookType(msg.getElementAsString(MD_BOOK_TYPE));
                    d_orderBooks[BIDSIDE].setBookType(d_orderBooks[ASKSIDE].getBookType());
				    // clear cache
				    d_orderBooks[ASKSIDE].doClearAll();
				    d_orderBooks[BIDSIDE].doClearAll();
			    }

			    // ASK table
			    if (msg.hasElement(MBO_TABLE_ASK, true)){
				    // has ask table array
				    Element askTable = msg.getElement(MBO_TABLE_ASK);
				    int numOfItems = askTable.numValues();
				    for (int index = 0; index < numOfItems; ++index) {
					    Element ask = askTable.getValueAsElement(index);
					    // get command
					    Name cmd = ask.getElement(MD_TABLE_CMD_RT).getValueAsName(); 
					    // get position
					    position = -1;
					    if (ask.hasElement(POSITION_FIELD[BYORDER][ASKSIDE], true)) {
						    position = ask.getElement(POSITION_FIELD[BYORDER][ASKSIDE]).getValueAsInt32();
						    if (position > 0) --position;
					    }
					    // get price
					    double askPrice = ask.getElement(PRICE_FIELD[BYORDER][ASKSIDE]).getValueAsFloat64();
					    // get size
					    int askSize = 0;
					    if (ask.hasElement(SIZE_FIELD[BYORDER][ASKSIDE], true)) {
						    askSize = (int)ask.getElement(SIZE_FIELD[BYORDER][ASKSIDE]).getValueAsInt64();
					    }
					    // get broker
					    String askBroker = "";
					    if (ask.hasElement(BROKER_FIELD[BYORDER][ASKSIDE], true)) {
						    askBroker = ask.getElement(BROKER_FIELD[BYORDER][ASKSIDE]).getValueAsString();
					    }
					    // get time
					    Datetime timeStamp = ask.getElement(TIME_FIELD[BYORDER]).getValueAsDatetime();
					    // create entry
					    BookEntry entry = new BookEntry(askBroker, askPrice, String.format("%02d", timeStamp.hour()) + 
					    		":" + String.format("%02d", timeStamp.minute()) + 
					    		":" + String.format("%02d", timeStamp.second()) + 
					    		"." + String.format("%03d", timeStamp.milliSecond()), 0, askSize);

					    // process data command
					    if(cmd.equals(ADD))
						    d_orderBooks[ASKSIDE].doAdd(position, entry);
					    else if(cmd.equals(MOD))
						    d_orderBooks[ASKSIDE].doMod(position, entry);
					    else if(cmd.equals(REPLACE))
						    d_orderBooks[ASKSIDE].doReplace(position, entry);
					    else if(cmd.equals(REPLACE_BY_BROKER))
						    d_orderBooks[ASKSIDE].doReplaceByBroker(entry);
					    else if(cmd.equals(EXEC))
						    d_orderBooks[ASKSIDE].doExec(position, entry);
				    }
			    }
			    // BID table
			    if (msg.hasElement(MBO_TABLE_BID, true)){
				    // has bid table array
				    Element bidTable = msg.getElement(MBO_TABLE_BID);
				    int numOfItems = bidTable.numValues();
				    for (int index = 0; index < numOfItems; ++index) {
					    Element bid = bidTable.getValueAsElement(index);
					    Name cmd = bid.getElement(MD_TABLE_CMD_RT).getValueAsName(); 
					    // get position
					    position = -1;
					    if (bid.hasElement(POSITION_FIELD[BYORDER][BIDSIDE], true)) {
						    position = bid.getElement(POSITION_FIELD[BYORDER][BIDSIDE]).getValueAsInt32();
						    if (position > 0) --position;
					    }
					    // get price
					    double bidPrice = bid.getElement(PRICE_FIELD[BYORDER][BIDSIDE]).getValueAsFloat64();
					    // get size
					    int bidSize = 0;
					    if (bid.hasElement(SIZE_FIELD[BYORDER][BIDSIDE], true)) {
						    bidSize = (int)bid.getElement(SIZE_FIELD[BYORDER][BIDSIDE]).getValueAsInt64();
					    }
					    // get broker
					    String bidBroker = "";
					    if (bid.hasElement(BROKER_FIELD[BYORDER][BIDSIDE], true)) {
						    bidBroker = bid.getElement(BROKER_FIELD[BYORDER][BIDSIDE]).getValueAsString();
					    }
					    // get time
					    Datetime timeStamp = bid.getElement(TIME_FIELD[BYORDER]).getValueAsDatetime();
					    // create entry
					    BookEntry entry = new BookEntry(bidBroker, bidPrice, String.format("%02d", timeStamp.hour()) + 
					    		":" + String.format("%02d", timeStamp.minute()) + 
					    		":" + String.format("%02d", timeStamp.second()) + 
					    		"." + String.format("%03d", timeStamp.milliSecond()), 0, bidSize);

					    // process data processing command
					    if(cmd.equals(ADD))
						    d_orderBooks[BIDSIDE].doAdd(position, entry);
					    else if(cmd.equals(MOD))
						    d_orderBooks[BIDSIDE].doMod(position, entry);
					    else if(cmd.equals(REPLACE))
						    d_orderBooks[BIDSIDE].doReplace(position, entry);
					    else if(cmd.equals(REPLACE_BY_BROKER))
						    d_orderBooks[BIDSIDE].doReplaceByBroker(entry);
					    else if(cmd.equals(EXEC))
						    d_orderBooks[BIDSIDE].doExec(position, entry);
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
    	 *              : session is the API session
    	 * Returns		: none
    	 *------------------------------------------------------------------------------------*/
        private boolean processByLevelMessage(Message msg) throws IOException
        {
            int side = -1;
		    int position = -1;
		    boolean bidRetran = false;
		    boolean askRetran = false;

		    // get gap detection flag (AMD book only)
		    if (msg.hasElement(MD_GAP_DETECTED, true) && !d_gapDetected) {
		    	d_gapDetected = true;
		    	synchronized (d_consoleWrite) {
		    		System.out.println("Bloomberg detected a gap in data stream.");
				}
		    }

		    // get event subtype
            Name subType = msg.getElement(MKTDEPTH_EVENT_SUBTYPE).getValueAsName();
            // get retran flags
            bidRetran = subType.equals(BID_RETRANS);
            askRetran = subType.equals(ASK_RETRANS);
            // BID or ASK message
		    if (subType.equals(BID) || subType.equals(ASK) 
		    		|| bidRetran || askRetran) {
			    if(subType.equals(BID) || bidRetran) {
				    side = BIDSIDE;
			    } else if (subType.equals(ASK) || askRetran) {
				    side = ASKSIDE;
			    }

			    // get position
			    position = -1;
			    if (msg.hasElement(POSITION_FIELD[BYLEVEL][side], true)) {
				    position = msg.getElement(POSITION_FIELD[BYLEVEL][side]).getValueAsInt32();
				    if (position > 0) --position;
			    }
			    
			    //  BID/ASK retran message
			    if (askRetran || bidRetran) {
				    // check for multi tick
			    	if (msg.hasElement(MD_MULTI_TICK_UPD_RT, true)) {
			    		// multi tick
			    		if (msg.getElement(MD_MULTI_TICK_UPD_RT).getValueAsInt32() == 0 ) {
					    	// last multi tick message, reset sequence number so next non-retran
			    			// message sequence number will be use as new starting number
					    	d_sequenceNumber = 0;
			    			if (askRetran && d_askRetran) {
			    				// end of ask retran
			    				d_askRetran = false;
			    		    	synchronized (d_consoleWrite) {
			    		    		System.out.println("Ask retran completed.");
			    				}
			    			} else if (bidRetran && d_bidRetran) {
			    				// end of ask retran
			    				d_bidRetran = false;
			    		    	synchronized (d_consoleWrite) {
			    		    		System.out.println("Bid retran completed.");
			    				}
			    			}
			    			if (!(d_askRetran || d_bidRetran)) {
			    				// retran completed
			    		    	synchronized (d_consoleWrite) {
			    		    		if (d_gapDetected) {
			    		    			// gap detected retran completed
			    		    			d_gapDetected = false;
			    		    			System.out.println("Gap detected retran completed.");
			    		    		} else {
			    		    			// normal retran completed
			    		    			System.out.println("Retran completed.");
			    		    		}
			    				}
			    			}
			    		} else {
			    			if (askRetran && !d_askRetran) {
			    				// start of ask retran
			    				d_askRetran = true;
			    		    	synchronized (d_consoleWrite) {
			    		    		System.out.println("Ask retran started.");
			    				}
			    			} else if (bidRetran && !d_bidRetran) {
			    				// start of ask retran
			    				d_bidRetran = true;
			    		    	synchronized (d_consoleWrite) {
			    		    		System.out.println("Bid retran started.");
			    				}
			    			}
			    		}
			    	}
			    } else if (msg.hasElement(MBL_SEQNUM_RT, true)) {
			    	// get sequence number
			    	long currentSequence = msg.getElementAsInt64(MBL_SEQNUM_RT);
			    	if (d_sequenceNumber == 0 || d_sequenceNumber == 1 ||
	                        (currentSequence == 1 && d_sequenceNumber > 1)) {
			    		// use current sequence number
			    		d_sequenceNumber = currentSequence;
			    	} else if ((d_sequenceNumber + 1 != currentSequence) && !d_gapDetected) {
			    		if (!d_resubscribed)
			    		{
					    	// previous tick sequence can not be smaller than current tick 
					    	// sequence number - 1 and NOT in gap detected mode. 
				    		synchronized (d_consoleWrite) {
				    			System.out.println("Warning: Gap detected - previous sequence number is " + 
				    					d_sequenceNumber + " and current tick sequence number is " +
				    					currentSequence + ").");
				    		}
				    		// gap detected, re-subscribe to securities
							d_session.resubscribe(d_subscriptions);
							d_resubscribed = true;
			    		}
			    	} else if (d_sequenceNumber >= currentSequence) {
			    		// previous tick sequence number can not be greater or equal
			    		// to current sequence number
			    		synchronized (d_consoleWrite) {
			    			System.out.println("Warning: Current Sequence number (" + currentSequence + 
			    					") is smaller or equal to previous tick sequence number (" +
			    					d_sequenceNumber + ").");
			    		}
			    	} else {
			    		// save current sequence number
			    		d_sequenceNumber = currentSequence;
			    	}
			    }

			    // get command
			    Name cmd = msg.getElement(MD_TABLE_CMD_RT).getValueAsName();
			    if (cmd.equals(CLEARALL)) {
				    d_levelBooks[side].doClearAll();
			    } else if (cmd.equals(DEL)) {
				    d_levelBooks[side].doDel(position);
			    } else if (cmd.equals(DELALL)) {
				    d_levelBooks[side].doDelAll();
			    } else if (cmd.equals(DELBETTER)) {
				    d_levelBooks[side].doDelBetter(position);
			    } else if (cmd.equals(DELSIDE)) {
				    d_levelBooks[side].doDelSide();
			    } else if (cmd.equals(REPLACE_CLEAR)) {
				    d_levelBooks[side].doReplaceClear(position);
			    } else {
				    // process other commands
			    	// get price
				    double price = msg.getElement(PRICE_FIELD[BYLEVEL][side]).getValueAsFloat64();
				    // get size
				    int size = 0;
				    if (msg.hasElement(SIZE_FIELD[BYLEVEL][side], true)) {
					    size = (int)msg.getElement(SIZE_FIELD[BYLEVEL][side]).getValueAsInt64();
				    }
				    // get number of order
				    int numOrder = 0;
				    if (msg.hasElement(ORDER_FIELD[BYLEVEL][side], true)) {
					    numOrder = (int)msg.getElement(ORDER_FIELD[BYLEVEL][side]).getValueAsInt64();
				    }
				    // get time
				    Datetime timeStamp = msg.getElement(TIME_FIELD[BYLEVEL]).getValueAsDatetime();
				    // create entry
				    BookEntry entry = new BookEntry(price, String.format("%02d", timeStamp.hour()) + 
				    		":" + String.format("%02d", timeStamp.minute()) + 
				    		":" + String.format("%02d", timeStamp.second()) + 
				    		"." + String.format("%03d", timeStamp.milliSecond()), numOrder, size);

				    // process data command
				    if(cmd.equals(ADD))
					    d_levelBooks[side].doAdd(position, entry);
				    else if(cmd.equals(MOD))
					    d_levelBooks[side].doMod(position, entry);
				    else if(cmd.equals(REPLACE))
					    d_levelBooks[side].doReplace(position, entry);
				    else if(cmd.equals(EXEC))
					    d_levelBooks[side].doExec(position, entry);
			    }
		    } else {
			    if (subType.equals(TABLE_INITPAINT)) {
				    if (msg.fragmentType() == Message.Fragment.START ||
					    msg.fragmentType() == Message.Fragment.NONE) {
					    // init paint
					    if (msg.hasElement(MBL_WINDOW_SIZE, true)){
						    d_levelBooks[ASKSIDE].setWindowSize((int)msg.getElementAsInt64(MBL_WINDOW_SIZE));
                            d_levelBooks[BIDSIDE].setWindowSize(d_levelBooks[ASKSIDE].getWindowSize());
					    }
					    d_levelBooks[ASKSIDE].setBookType(msg.getElementAsString(MD_BOOK_TYPE));
                        d_levelBooks[BIDSIDE].setBookType(d_levelBooks[ASKSIDE].getBookType());
					    // clear cache
					    d_levelBooks[ASKSIDE].doClearAll();
					    d_levelBooks[BIDSIDE].doClearAll();
				    }

				    // ASK table
				    if (msg.hasElement(MBL_TABLE_ASK, true)){
					    // has ask table array
					    Element askTable = msg.getElement(MBL_TABLE_ASK);
					    int numOfItems = askTable.numValues();
					    for (int index = 0; index < numOfItems; ++index) {
						    Element ask = askTable.getValueAsElement(index);
						    Name cmd = ask.getElement(MD_TABLE_CMD_RT).getValueAsName(); 
						    // get position
						    position = -1;
						    if (ask.hasElement(POSITION_FIELD[BYLEVEL][ASKSIDE], true)) {
							    position = ask.getElement(POSITION_FIELD[BYLEVEL][ASKSIDE]).getValueAsInt32();
							    if (position > 0) --position;
						    }
						    // get price
						    double askPrice = ask.getElement(PRICE_FIELD[BYLEVEL][ASKSIDE]).getValueAsFloat64();
						    // get size
						    int askSize = 0;
						    if (ask.hasElement(SIZE_FIELD[BYLEVEL][ASKSIDE], true)) {
							    askSize = (int)ask.getElement(SIZE_FIELD[BYLEVEL][ASKSIDE]).getValueAsInt64();
						    }
						    // get number of order
						    int askNumOrder = 0;
						    if (ask.hasElement(ORDER_FIELD[BYLEVEL][ASKSIDE], true)) {
							    askNumOrder = (int)ask.getElement(ORDER_FIELD[BYLEVEL][ASKSIDE]).getValueAsInt64();
						    }
						    // get time
						    Datetime timeStamp = ask.getElement(TIME_FIELD[BYLEVEL]).getValueAsDatetime();
						    // create entry
						    BookEntry entry = new BookEntry(askPrice, String.format("%02d", timeStamp.hour()) + 
						    		":" + String.format("%02d", timeStamp.minute()) + 
						    		":" + String.format("%02d", timeStamp.second()) + 
						    		"." + String.format("%03d", timeStamp.milliSecond()), askNumOrder, askSize);
						    // process command
						    if(cmd.equals(ADD))
							    d_levelBooks[ASKSIDE].doAdd(position, entry);
						    else if(cmd.equals(MOD))
							    d_levelBooks[ASKSIDE].doMod(position, entry);
						    else if(cmd.equals(REPLACE))
							    d_levelBooks[ASKSIDE].doReplace(position, entry);
						    else if(cmd.equals(EXEC))
							    d_levelBooks[ASKSIDE].doExec(position, entry);
					    }
				    }
				    if (msg.hasElement(MBL_TABLE_BID, true)){
					    // has bid table array
					    Element bidTable = msg.getElement(MBL_TABLE_BID);
					    int numOfItems = bidTable.numValues();
					    for (int index = 0; index < numOfItems; ++index) {
						    Element bid = bidTable.getValueAsElement(index);
						    // get command
						    Name cmd = bid.getElement(MD_TABLE_CMD_RT).getValueAsName();
						    // get position
						    position = -1;
						    if (bid.hasElement(POSITION_FIELD[BYLEVEL][BIDSIDE], true)) {
							    position = bid.getElement(POSITION_FIELD[BYLEVEL][BIDSIDE]).getValueAsInt32();
							    if (position > 0) --position;
						    }
						    // get price
						    double bidPrice = bid.getElement(PRICE_FIELD[BYLEVEL][BIDSIDE]).getValueAsFloat64();
						    // get size
						    int bidSize = 0;
						    if (bid.hasElement(SIZE_FIELD[BYLEVEL][BIDSIDE], true)) {
							    bidSize = (int)bid.getElement(SIZE_FIELD[BYLEVEL][BIDSIDE]).getValueAsInt64();
						    }
						    // get number of order
						    int bidNumOrder = 0;
						    if (bid.hasElement(ORDER_FIELD[BYLEVEL][BIDSIDE], true)) {
							    bidNumOrder = (int)bid.getElement(ORDER_FIELD[BYLEVEL][BIDSIDE]).getValueAsInt64();
						    }
						    // get time
						    Datetime timeStamp = bid.getElement(TIME_FIELD[BYLEVEL]).getValueAsDatetime();
						    // create entry
						    BookEntry entry = new BookEntry(bidPrice, String.format("%02d", timeStamp.hour()) + 
						    		":" + String.format("%02d", timeStamp.minute()) + 
						    		":" + String.format("%02d", timeStamp.second()) + 
						    		"." + String.format("%03d", timeStamp.milliSecond()), bidNumOrder, bidSize);

						    // process command
						    if(cmd.equals(ADD))
							    d_levelBooks[BIDSIDE].doAdd(position, entry);
						    else if(cmd.equals(MOD))
							    d_levelBooks[BIDSIDE].doMod(position, entry);
						    else if(cmd.equals(REPLACE))
							    d_levelBooks[BIDSIDE].doReplace(position, entry);
						    else if(cmd.equals(EXEC))
							    d_levelBooks[BIDSIDE].doExec(position, entry);
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
    	 * Name			: processMiscEvents
    	 * Description	: process misc
    	 * Arguments	: event is the API event
    	 * Returns		: true - successful, false - failed
    	 *------------------------------------------------------------------------------------*/
        private boolean processMiscEvents(Event event, Session session)
        throws Exception
        {
            System.out.println("Processing " + event.eventType());
            MessageIterator msgIter = event.messageIterator();
            while (msgIter.hasNext()) {
                Message msg = msgIter.next();
                System.out.println(
                        d_dateFormat.format(Calendar.getInstance().getTime()) +
                        ": " + msg.messageType() + "\n");
            }
            return true;
        }
    }

}
