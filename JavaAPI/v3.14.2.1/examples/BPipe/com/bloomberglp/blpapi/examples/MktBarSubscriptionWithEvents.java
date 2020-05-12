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

/*****************************************************************************
 MktBarSubscriptionWithEvents.java:
	This program demonstrates how to make a subscription to particular security/
	ticker to get realtime streaming updates at specified bar_size using
	"bar_size" options available. It uses the Market Bar service(//blp/mktbar)
	provided by API. Program does the following:
		1. Establishes a session which facilitates connection to the bloomberg
		   network.
		2. Initiates the Market Bar Service(//blp/mktbar) for realtime
		   data.
		3. Authorize BPS user/application
		3. Creates and sends the request via the session.
			- Creates a subscription list
			- Adds securities, fields and options to subscription list
			  Option specifies the bar_size duration for market bars, the start and end times.
			- Subscribes to realtime market bars with authorized Identity
		4. Event Handling of the responses received.
        5. Parsing of the message data.
 Usage:
    MktBarSubscriptionWithEvents -h
	   Print the usage for the program on the console

	MktBarSubscriptionWithEvents
	   If you run the program with default values, program prints the streaming
	   updates on the console for two default securities specfied
	   1. Ticker - "//blp/mktbar/ticker/IBM US Equity"
	   2. Ticker - "//blp/mktbar/ticker/VOD LN Equity"
	   for field LAST_PRICE, bar_size=5, start_time=<local time + 2 minutes>,
                                end_time=<local_time+32 minutes>

    example usage:
	MktBarSubscriptionWithEvents -ip <BPipe host IP> -p 8194
	MktBarSubscriptionWithEvents -ip <BPipe host IP> -p 8194 -s "//blp/mktbar/ticker/VOD LN Equity"
                                        -s "//blp/mktbar/ticker/IBM US Equity"
									    -f "LAST_PRICE" -o "bar_size=5.0"
                                        -o "start_time=15:00" -o "end_time=15:30"
										-auth LOGON
	Prints the response on the console of the command line requested data

******************************************************************************/
package com.bloomberglp.blpapi.examples;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.TimeZone;
import java.text.DateFormat;
import java.io.*;

import javax.management.timer.Timer;

import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Datetime;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Event.EventType;
import com.bloomberglp.blpapi.Identity.SeatType;
import com.bloomberglp.blpapi.EventQueue;
import com.bloomberglp.blpapi.EventHandler;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Subscription;
import com.bloomberglp.blpapi.SubscriptionList;
import com.bloomberglp.blpapi.Identity;
import com.bloomberglp.blpapi.SessionOptions.ServerAddress;

public class MktBarSubscriptionWithEvents
{
	private static final String AUTH_SVC = "//blp/apiauth";

	private static final Name AUTHORIZATION_SUCCESS = Name.getName("AuthorizationSuccess");
	private static final Name TOKEN_SUCCESS = Name.getName("TokenGenerationSuccess");

	private static final Name TIME = new Name("TIME");
	private static final Name OPEN = new Name("OPEN");
	private static final Name HIGH = new Name("HIGH");
	private static final Name LOW = new Name("LOW");
	private static final Name CLOSE = new Name("CLOSE");
	private static final Name NUMBER_OF_TICKS = new Name("NUMBER_OF_TICKS");
	private static final Name VOLUME = new Name("VOLUME");

    private static final Name EXCEPTIONS = new Name("exceptions");
    private static final Name FIELD_ID = new Name("fieldId");
    private static final Name REASON = new Name("reason");
    private static final Name CATEGORY = new Name("category");
    private static final Name DESCRIPTION = new Name("description");

	private ArrayList<String> d_hosts;
	private int				  d_port;
    private String            d_authOption;
    private String            d_name;
    private String 			  d_service;
    private Identity          d_identity;
    private SessionOptions    d_sessionOptions;
    private Session           d_session;
    private ArrayList<String> d_securities;
    private ArrayList<String> d_fields;
    private ArrayList<String> d_options;
    private SubscriptionList  d_subscriptions;
    private SimpleDateFormat  d_dateFormat;
    
    /**
     * @param args
     */
    public static void main(String[] args) throws java.lang.Exception
    {
        System.out.println("Realtime Market Bars with Event Handler Example");
        MktBarSubscriptionWithEvents example = new MktBarSubscriptionWithEvents();
        example.run(args);
    }

    public MktBarSubscriptionWithEvents()
    {
    	d_hosts = new ArrayList<String>();
    	d_port = 8194;
    	d_authOption = "";
        d_securities = new ArrayList<String>();
        d_fields = new ArrayList<String>();
        d_options = new ArrayList<String>();
        d_service = "//blp/mktbar";
        d_subscriptions = new SubscriptionList();
        d_dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss.SSS");
    }

    /*****************************************************************************
    Function    : createSession
    Description : This function creates a session object and opens the market
                    bar service.  Returns false on failure of either.
    Arguments   : none
    Returns     : bool
    *****************************************************************************/
    private boolean createSession() throws IOException, InterruptedException
    {
        if (d_session != null) d_session.stop();

		String authOptions = null;
		if(d_authOption.equalsIgnoreCase("APPLICATION")){
	        // Set Application Authentication Option
	        authOptions = "AuthenticationMode=APPLICATION_ONLY;";
	        authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
	        // ApplicationName is the entry in EMRS.
	        authOptions += "ApplicationName=" + d_name;
		} else {
	        // Set User authentication option
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
	                // Authenticate user using windows/unix login name (default)
	                authOptions = "AuthenticationType=OS_LOGON";
	            }
            }
		}
	
		System.out.println("authOptions = " + authOptions);

		d_sessionOptions = new SessionOptions();
		if (d_authOption != null)
		{
			d_sessionOptions.setAuthenticationOptions(authOptions);
		}
	
		ServerAddress[] servers = new ServerAddress[d_hosts.size()];
		for (int i = 0; i < d_hosts.size(); ++i) {
			servers[i] = new ServerAddress(d_hosts.get(i), d_port);
		}
	
		d_sessionOptions.setServerAddresses(servers);
		d_sessionOptions.setAutoRestartOnDisconnection(true);
		d_sessionOptions.setNumStartAttempts(d_hosts.size());
		d_sessionOptions.setDefaultSubscriptionService(d_service);
		
		System.out.print("Connecting to port " + d_port + " on server:");
		for (ServerAddress server: d_sessionOptions.getServerAddresses()) {
			System.out.print(" " + server);
		}
		System.out.println();
        d_session = new Session(d_sessionOptions, new SubscriptionEventHandler());
        
        
        if (!d_session.start()) {
            System.err.println("Failed to start session");
            return false;
        }
        System.out.println("Connected successfully\n");

        if (!d_session.openService(d_service)) {
            System.err.println("Failed to open service " + d_service);
            d_session.stop();
            return false;
        }

        return true;
    }

    /*****************************************************************************
    Function    : authorize
    Description : This function authorize user/application provided in the .  Returns false on failure of either.
    			  command line parameter -auth.  
    Arguments   : none
    Returns     : bool
    *****************************************************************************/
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

    /*****************************************************************************
    Function    : run
    Description : Performs the main functions of the program
    Arguments   : string array
    Returns     : void
    *****************************************************************************/

    private void run(String[] args) throws Exception
    {
    	Boolean status = true;
        if (!parseCommandLine(args)) return;
        if (!createSession()) return;

		if (d_authOption != null) {
			d_identity = d_session.createIdentity();
			if (!authorize()) {
				status = false;
			}
		}

        if (d_identity.seatType() == SeatType.BPS)
        {
        	// only for BPS user/application
            System.out.println("Subscribing with Identity...");
            d_session.subscribe(d_subscriptions, d_identity);
        }
        else
        {
        	System.out.println("User is " + d_identity.seatType() + 
        			" and is not authorize to use " + d_service + ".");
        	status = false;
        }

        if (status)
        {
	        // wait for enter key to exit application
	        System.in.read();
        }
        d_session.stop();
        System.out.println("Exiting.");
    }

    class SubscriptionEventHandler implements EventHandler
    {
	/*****************************************************************************
	Function    : processEvent
	Description : Processes session events
	Arguments   : Event, Session
	Returns     : void
	*****************************************************************************/
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

    /*****************************************************************************
    Function    : processSubscriptionStatus
    Description : Processes subscription status messages returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
        private boolean processSubscriptionStatus(Event event, Session session)
        throws Exception
        {
            System.out.println("Processing SUBSCRIPTION_STATUS");
            MessageIterator msgIter = event.messageIterator();
            while (msgIter.hasNext()) {
                Message msg = msgIter.next();
                String topic = (String) msg.correlationID().object();
                System.out.println(
                        d_dateFormat.format(Calendar.getInstance().getTime()) +
                        ": " + topic + " - " + msg.messageType());

                if (msg.hasElement(REASON)) {
                    // This can occur on SubscriptionFailure.
                    Element reason = msg.getElement(REASON);
                    System.out.println("\t" +
                            reason.getElement(CATEGORY).getValueAsString() +
                            ": " + reason.getElement(DESCRIPTION).getValueAsString());
                }

                System.out.println("");
            }
            return true;
        }

    /*****************************************************************************
    Function    : processSubscriptionDataEvent
    Description : Processes all field data returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
        private boolean processSubscriptionDataEvent(Event event, Session session)
        throws Exception
        {
            System.out.println("Processing SUBSCRIPTION_DATA");
            MessageIterator msgIter = event.messageIterator();
            while (msgIter.hasNext()) {
                Message msg = msgIter.next();
                String topic = (String) msg.correlationID().object();
                System.out.println(
                        d_dateFormat.format(Calendar.getInstance().getTime()) +
                        ": " + topic + " - " + msg.messageType());
                CheckAspectFields(msg);
            }
            return true;
        }

    /*****************************************************************************
    Function    : processMiscEvents
    Description : Processes any message returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
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

    /*****************************************************************************
    Function    : parseCommandLine
    Description : This function parses input arguments and/or sets default arguments
                    Only returns false on -h.
    Arguments   : string array
    Returns     : bool
    *****************************************************************************/
    private boolean parseCommandLine(String[] args)
    {
        for (int i = 0; i < args.length; ++i) {
            if (args[i].equalsIgnoreCase("-s") && i + 1 < args.length) {
                d_securities.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-f") && i + 1 < args.length) {
                d_fields.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-o") && i + 1 < args.length) {
                d_options.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length) {
            	d_hosts.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-p") && i + 1 < args.length) {
            	d_port = Integer.parseInt(args[++i]);
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

        // check for host ip
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

        if (d_fields.size() == 0) {
            d_fields.add("LAST_PRICE");
        }

        if (d_securities.size() == 0) {
            d_securities.add("//blp/mktbar/ticker/IBM US Equity");
            d_securities.add("//blp/mktbar/ticker/VOD LN Equity");
        }

        if(d_options.size() == 0)
        {
        	String start_time_str = "start_time=";
        	String end_time_str = "end_time=";

        	DateFormat dateFormat = new SimpleDateFormat("HH:mm");

        	TimeZone zone = dateFormat.getTimeZone();
        	int minutesOffset = zone.getOffset(new Date().getTime());
        	String start_time = dateFormat.format(new Date().getTime() + (1L * Timer.ONE_MINUTE) - minutesOffset);
        	String end_time = dateFormat.format(new Date().getTime() + (32L * Timer.ONE_MINUTE) - minutesOffset);

        	start_time_str += start_time;
        	end_time_str += end_time;

        	d_options.add("bar_size=5");
        	d_options.add(start_time_str);
        	d_options.add(end_time_str);
        	System.out.print("Start time : " + start_time + "\n");
        	System.out.print("End time : " + end_time + "\n");
        	System.out.print("bar_size : 5\n");
        }

        for (int i = 0; i < d_securities.size(); ++i) {
            String security = (String)d_securities.get(i);
            Subscription subscription = new Subscription(security, d_fields, d_options,
                    new CorrelationID(security));
            System.out.println(subscription.toString());
            d_subscriptions.add(subscription);
        }

        return true;
    }

    /*****************************************************************************
    Function    : CheckAspectFields
    Description : Processes any field that can be contained within the market
                    bar message.
    Arguments   : Message
    Returns     : void
    *****************************************************************************/
	private void CheckAspectFields(Message msg)
	{
		// extract data for each specific element
		// it's anticipated that an application will require this data
		// in the correct format.  this is retrieved for demonstration
		// but is not used later in the code.
		if(msg.hasElement(TIME))
		{
			Datetime time = msg.getElementAsDatetime(TIME);
			String time_str = msg.getElementAsString(TIME);
			System.out.print("Time : " + time_str + "\n");
		}
		if(msg.hasElement(OPEN))
		{
			double open = msg.getElementAsFloat64(OPEN);
			String open_str = msg.getElementAsString(OPEN);
			System.out.print("Open : " + open_str + "\n");
		}
		if(msg.hasElement(HIGH))
		{
			double high = msg.getElementAsFloat64(HIGH);
			String high_str = msg.getElementAsString(HIGH);
			System.out.print("High : " + high_str + "\n");
		}
		if(msg.hasElement(LOW))
		{
			double low = msg.getElementAsFloat64(LOW);
			String low_str = msg.getElementAsString(LOW);
			System.out.print("Low : " + low_str + "\n");
		}
		if(msg.hasElement(CLOSE))
		{
			double close = msg.getElementAsFloat64(CLOSE);
			String close_str = msg.getElementAsString(CLOSE);
			System.out.print("Close : " + close_str + "\n");
		}
		if(msg.hasElement(NUMBER_OF_TICKS))
		{
			int number_of_ticks = msg.getElementAsInt32(NUMBER_OF_TICKS);
			String number_of_ticks_str = msg.getElementAsString(NUMBER_OF_TICKS);
			System.out.print("Number of Ticks : " + number_of_ticks_str + "\n");
		}
		if(msg.hasElement(VOLUME))
		{
			long volume = msg.getElementAsInt64(VOLUME);
			String volume_str = msg.getElementAsString(VOLUME);
			System.out.print("Volume : " + volume_str + "\n");
		}
		System.out.print("\n");
	}

    /*****************************************************************************
    Function    : printUsage
    Description : This function prints instructions for use to the console
    Arguments   : none
    Returns     : void
    *****************************************************************************/
    private void printUsage()
    {
        System.out.println("Usage:");
        System.out.println("	Retrieve realtime data ");
        System.out.println("		[-s         <security	= IBM US Equity>");
        System.out.println("		[-f         <field		= LAST_PRICE>");
        System.out.println("		[-o         <subscriptionOptions>");
        System.out.println("		[-ip        <ipAddress	= localhost>");
        System.out.println("		[-p         <tcpPort	= 8194>");
		System.out.println("		[-auth      <authenticationOption = LOGON (default) or APPLICATION or DIRSVC or USER_APP>]");
		System.out.println("		[-n         <name = applicationName or directoryService>]");
		System.out.println("Notes:");
		System.out.println(" -Specify only LOGON to authorize 'user' using Windows login name.");
		System.out.println(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println(" -Specify APPLICATION and name(Application Name) to authorize application.");
        System.out.println("Press ENTER to quit");
    }
}
