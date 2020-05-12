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

import java.text.DecimalFormat;
import java.text.SimpleDateFormat;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.TimeZone;

import com.bloomberglp.blpapi.Datetime;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Event.EventType;
import com.bloomberglp.blpapi.EventQueue;
import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.InvalidRequestException;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Identity;
import com.bloomberglp.blpapi.SessionOptions.ServerAddress;

public class IntradayBarExample {

	private String REFDATA_SVC = "//blp/refdata";
	private String AUTH_SVC = "//blp/apiauth";

    private static final Name BAR_DATA       = new Name("barData");
    private static final Name BAR_TICK_DATA  = new Name("barTickData");
    private static final Name OPEN           = new Name("open");
    private static final Name HIGH           = new Name("high");
    private static final Name LOW            = new Name("low");
    private static final Name CLOSE          = new Name("close");
    private static final Name VOLUME         = new Name("volume");
    private static final Name NUM_EVENTS     = new Name("numEvents");
    private static final Name TIME           = new Name("time");
    private static final Name RESPONSE_ERROR = new Name("responseError");
    private static final Name CATEGORY       = new Name("category");
    private static final Name MESSAGE        = new Name("message");
	private static final Name AUTHORIZATION_SUCCESS = Name.getName("AuthorizationSuccess");
	private static final Name TOKEN_SUCCESS = Name.getName("TokenGenerationSuccess");

	private ArrayList<String> d_hosts;
    private int               d_port;
    private String     		  d_authOption;
    private String     		  d_name;
    private Session    		  d_session;
    private Identity   		  d_identity;
    private String            d_security;
    private String            d_eventType;
    private int               d_barInterval;
    private boolean           d_gapFillInitialBar;
    private String            d_startDateTime;
    private String            d_endDateTime;
    private SimpleDateFormat  d_dateFormat;
    private DecimalFormat     d_decimalFormat;

    public static void main(String[] args) throws Exception {
        System.out.println("Intraday Bars Example");
        IntradayBarExample example = new IntradayBarExample();
        example.run(args);

        System.out.println("Press ENTER to quit");
        System.in.read();
    }

    private Calendar getPreviousTradingDate()
    {
		Calendar prevDate = Calendar.getInstance(TimeZone.getTimeZone("GMT"));
		prevDate.roll(Calendar.DAY_OF_MONTH, -1);
		if (prevDate.get(Calendar.DAY_OF_WEEK) == Calendar.SUNDAY) {
		    prevDate.roll(Calendar.DAY_OF_MONTH, -2);
		}
		else if (prevDate.get(Calendar.DAY_OF_WEEK) == Calendar.SATURDAY) {
		    prevDate.roll(Calendar.DAY_OF_MONTH, -1);
		}
		return prevDate;
    }

    public IntradayBarExample() {
    	d_hosts = new ArrayList<String>();
    	d_port = 8194;
        d_authOption="";
		d_name="";
		d_session = null;
    	
        d_barInterval = 60;
        d_security = "IBM US Equity";
        d_eventType = "TRADE";
        d_gapFillInitialBar = false;

        d_dateFormat = new SimpleDateFormat();
        d_dateFormat.applyPattern("MM/dd/yyyy k:mm");
        d_decimalFormat = new DecimalFormat();
        d_decimalFormat.setMaximumFractionDigits(3);
    }

    private boolean createSession()	throws IOException, InterruptedException 
    {
		String authOptions = null;
		if(d_authOption.equalsIgnoreCase("APPLICATION")){
	        // Set Application Authentication Option
	        authOptions = "AuthenticationMode=APPLICATION_ONLY;";
	        authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
	        // ApplicationName is the entry in EMRS.
	        authOptions += "ApplicationName=" + d_name;
		} else {
	        // Set User authentication option
	        if (d_authOption.equalsIgnoreCase("NONE"))
	        {
	        	d_authOption = null;
	        }
	        else
	        {
	            if (d_authOption.equalsIgnoreCase("USER_APP"))
	            {
	                // Set User and Application Authentication Option
	                authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
	                authOptions += "AuthenticationType=OS_LOGON;";
	                authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
	                // ApplicationName is the entry in EMRS.
	                authOptions += "ApplicationName=" + d_name;
	            }
	            else
	            {
		            if (d_authOption.equalsIgnoreCase("DIRSVC"))
		            {
		                // Authenticate user using active directory service property
		                authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
		                authOptions += "DirSvcPropertyName=" + d_name;
		            }
		            else
		            {
		                // Authenticate user using windows/unix login name
		                authOptions = "AuthenticationType=OS_LOGON";
		            }
	            }
	        }
		}
	
		System.out.println("authOptions = " + authOptions);
		SessionOptions sessionOptions = new SessionOptions();
		if (d_authOption != null)
		{
			sessionOptions.setAuthenticationOptions(authOptions);
		}
	
	    sessionOptions.setDefaultSubscriptionService(REFDATA_SVC);
	
		ServerAddress[] servers = new ServerAddress[d_hosts.size()];
		for (int i = 0; i < d_hosts.size(); ++i) {
			servers[i] = new ServerAddress(d_hosts.get(i), d_port);
		}
	
		sessionOptions.setServerAddresses(servers);
	    sessionOptions.setAutoRestartOnDisconnection(true);
	    sessionOptions.setNumStartAttempts(d_hosts.size());
		
		System.out.print("Connecting to port " + d_port + " on server:");
		for (ServerAddress server: sessionOptions.getServerAddresses()) {
			System.out.print(" " + server);
		}
		System.out.println();
	    d_session = new Session(sessionOptions);
	    
	    return d_session.start();
	}
	
	private boolean authorize()
		throws IOException, InterruptedException {
		Event event;
		MessageIterator msgIter;
	
		EventQueue tokenEventQueue = new EventQueue();
		CorrelationID corrlationId = new CorrelationID(99);
		d_session.generateToken(corrlationId, tokenEventQueue);
		String token = null;
		int timeoutMilliSeonds = 10000;
		event = tokenEventQueue.nextEvent(timeoutMilliSeonds);
		if (event.eventType() == EventType.TOKEN_STATUS) {
			MessageIterator iter = event.messageIterator();
			while (iter.hasNext()) {
				Message msg = iter.next();
				System.out.println(msg.toString());
				if (msg.messageType() == TOKEN_SUCCESS) {
					token = msg.getElementAsString("token");
				}
			}
		}
		if (token == null){
			System.err.println("Failed to get token");
			return false;
		}
	
		if (d_session.openService(AUTH_SVC)) {
			Service authService = d_session.getService(AUTH_SVC);
			Request authRequest = authService.createAuthorizationRequest();
			authRequest.set("token", token);
	
			EventQueue authEventQueue = new EventQueue();
	
			d_session.sendAuthorizationRequest(authRequest, d_identity,
					authEventQueue, new CorrelationID(d_identity));
	
			while (true) {
				event = authEventQueue.nextEvent();
				if (event.eventType() == EventType.RESPONSE
						|| event.eventType() == EventType.PARTIAL_RESPONSE
						|| event.eventType() == EventType.REQUEST_STATUS) {
					msgIter = event.messageIterator();
					while (msgIter.hasNext()) {
						Message msg = msgIter.next();
						System.out.println(msg);
						if (msg.messageType() == AUTHORIZATION_SUCCESS) {
							return true;
						} else {
							System.err.println("Not authorized");
							return false;
						}
					}
				}
			}
		}
		return false;
	}

    private void run(String[] args) throws Exception {
        if (!parseCommandLine(args)) return;

        if (!createSession()) {
            System.err.println("Failed to start session.");
            return;
        }

		if (d_authOption != null) {
			d_identity = d_session.createIdentity();
			if (!authorize()) {
				return;
			}
		}

        System.out.println("Connected successfully.");
        if (!d_session.openService(REFDATA_SVC)) {
        	System.err.println("Failed to open " + REFDATA_SVC);
        	d_session.stop();
        	return;
        }

        try {
        	sendIntradayBarRequest(d_session);
        } catch (InvalidRequestException e) {
            e.printStackTrace();
        }

        // wait for events from session.
        eventLoop(d_session);

        d_session.stop();
    }

    private void eventLoop(Session session) throws Exception {
        boolean done = false;
        while (!done) {
            Event event = session.nextEvent();
            if (event.eventType() == Event.EventType.PARTIAL_RESPONSE) {
                System.out.println("Processing Partial Response");
                processResponseEvent(event, session);
            }
            else if (event.eventType() == Event.EventType.RESPONSE) {
                System.out.println("Processing Response");
                processResponseEvent(event, session);
                done = true;
            } else {
                MessageIterator msgIter = event.messageIterator();
                while (msgIter.hasNext()) {
                    Message msg = msgIter.next();
                    System.out.println(msg);
                    if (event.eventType() == Event.EventType.SESSION_STATUS) {
                        if (msg.messageType().equals("SessionTerminated")) {							
                            done = true;
                        }
                    }
                }
            }
        }
    }

    private void processMessage(Message msg) throws Exception {
        Element data = msg.getElement(BAR_DATA).getElement(BAR_TICK_DATA);
        int numBars = data.numValues();
        System.out.println("Response contains " + numBars + " bars");
        System.out.println("Datetime\t\tOpen\t\tHigh\t\tLow\t\tClose" +
                           "\t\tNumEvents\tVolume");            
        for (int i = 0; i < numBars; ++i) {
            Element bar = data.getValueAsElement(i);
            Datetime time = bar.getElementAsDate(TIME);
            double open = bar.getElementAsFloat64(OPEN);
            double high = bar.getElementAsFloat64(HIGH);
            double low = bar.getElementAsFloat64(LOW);
            double close = bar.getElementAsFloat64(CLOSE);
            int numEvents = bar.getElementAsInt32(NUM_EVENTS);
            long volume = bar.getElementAsInt64(VOLUME);

            System.out.println(d_dateFormat.format(time.calendar().getTime()) + "\t" +
                    d_decimalFormat.format(open) + "\t\t" +
                    d_decimalFormat.format(high) + "\t\t" +
                    d_decimalFormat.format(low) + "\t\t" +
                    d_decimalFormat.format(close) + "\t\t" +
                    d_decimalFormat.format(numEvents) + "\t\t" +
                    d_decimalFormat.format(volume));
        }
    }

    private void processResponseEvent(Event event, Session session) throws Exception {
        MessageIterator msgIter = event.messageIterator();
        while (msgIter.hasNext()) {
            Message msg = msgIter.next();
            if (msg.hasElement(RESPONSE_ERROR)) {
                printErrorInfo("REQUEST FAILED: ", msg.getElement(RESPONSE_ERROR));
                continue;
            }
            processMessage(msg);
        }
    }

    private void sendIntradayBarRequest(Session session) throws Exception
    {
        Service refDataService = session.getService("//blp/refdata");
        Request request = refDataService.createRequest(
        		"IntradayBarRequest");

        // only one security/eventType per request
        request.set("security", d_security);
        request.set("eventType", d_eventType);
        request.set("interval", d_barInterval);

        if (d_startDateTime == null || d_endDateTime == null) {
			Calendar tradedOn = getPreviousTradingDate();
			
			// set the end date for next day
			request.set("startDateTime",  new Datetime(tradedOn.get(Calendar.YEAR),
					  tradedOn.get(Calendar.MONTH),
					  tradedOn.get(Calendar.DAY_OF_MONTH),
					  13, 30, 0, 0));
			request.set("endDateTime", new Datetime(tradedOn.get(Calendar.YEAR),
					tradedOn.get(Calendar.MONTH),
					tradedOn.get(Calendar.DAY_OF_MONTH), 
					20, 35, 0, 0));
        }
        else {
	        request.set("startDateTime", d_startDateTime);
	        request.set("endDateTime", d_endDateTime);
        }
        
        if (d_gapFillInitialBar) {
            request.set("gapFillInitialBar", d_gapFillInitialBar);
        }

        if (d_authOption == null)
        {
		    System.out.println("Sending Request: " + request);
        	session.sendRequest(request, null);
        }
        else
        {
        	// request data with identity object
            System.out.println("Sending Request with user's Identity: " + request);
        	session.sendRequest(request, d_identity, null);
        }
    }

    private boolean parseCommandLine(String[] args)
    {
        for (int i = 0; i < args.length; ++i) {
            if (args[i].equalsIgnoreCase("-s") && i + 1 < args.length) {
                d_security = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-e") && i + 1 < args.length) {
                d_eventType = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length) {
                d_hosts.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-p") && i + 1 < args.length) {
                d_port = Integer.parseInt(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-b") && i + 1 < args.length) {
                d_barInterval = Integer.parseInt(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-g")) {
                d_gapFillInitialBar = true;
            }
            else if (args[i].equalsIgnoreCase("-sd") && i + 1 < args.length) {
                d_startDateTime = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-ed") && i + 1 < args.length) {
                d_endDateTime = args[++i];
            }
			else if(args[i].equalsIgnoreCase("-auth") && i + 1 < args.length) {
				d_authOption = args[++i];
			}
			else if(args[i].equalsIgnoreCase("-n") && i + 1 < args.length) {
				d_name = args[++i];
			}
            else if (args[i].equalsIgnoreCase("-h")) {
                printUsage();
                return false;
            }
        }

        // handle default arguments
        if (d_hosts.size() == 0)
        {
        	System.out.println("Missing host IP");
        	printUsage();
        	return false;
        }

        // check for application name
        if ((d_authOption.equalsIgnoreCase("APPLICATION")  || d_authOption.equalsIgnoreCase("USER_APP")) && (d_name.equalsIgnoreCase("")))
        {
        	System.out.println("Application name cannot be NULL for application authorization.");
            printUsage();
            return false;
        }
        // check for Directory Service name
        if ((d_authOption.equalsIgnoreCase("DIRSVC")) && (d_name.equalsIgnoreCase("")))
        {
        	System.out.println("Directory Service property name cannot be NULL for DIRSVC authorization.");
            printUsage();
            return false;
        }

        return true;
    }

    private void printErrorInfo(String leadingStr, Element errorInfo)
    throws Exception
    {
        System.out.println(leadingStr + errorInfo.getElementAsString(CATEGORY) +
                           " (" + errorInfo.getElementAsString(MESSAGE) + ")");
    }

    private void printUsage()
    {
        System.out.println("Usage:");
        System.out.println("  Retrieve intraday bars");
        System.out.println("    [-s     <security	= IBM US Equity>");
        System.out.println("    [-e     <event		= TRADE>");
        System.out.println("    [-b     <barInterval= 60>");
        System.out.println("    [-sd    <startDateTime  = 2008-08-11T13:30:00>");
        System.out.println("    [-ed    <endDateTime    = 2008-08-12T13:30:00>");
        System.out.println("    [-g     <gapFillInitialBar = false>");
        System.out.println("    [-ip    <ipAddress	= localhost>");
        System.out.println("    [-p     <tcpPort	= 8194>");
		System.out.println("    [-auth  <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]");
		System.out.println("    [-n     <name = applicationName or directoryService>]");
		System.out.println("Notes:");
        System.out.println("1) All times are in GMT.");
        System.out.println("2) Only one security can be specified.");
        System.out.println("3) Only one event can be specified.");
		System.out.println("4) Specify only LOGON to authorize 'user' using Windows login name.");
		System.out.println("5) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println("6) Specify APPLICATION and name(Application Name) to authorize application.");
    }
}
