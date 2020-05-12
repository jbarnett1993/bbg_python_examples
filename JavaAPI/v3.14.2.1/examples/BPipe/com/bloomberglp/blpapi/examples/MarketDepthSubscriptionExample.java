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

import com.bloomberglp.blpapi.CorrelationID;
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

public class MarketDepthSubscriptionExample
{
    private static final Name EXCEPTIONS = Name.getName("exceptions");
    private static final Name FIELD_ID = Name.getName("fieldId");
    private static final Name REASON = Name.getName("reason");
    private static final Name CATEGORY = Name.getName("category");
    private static final Name DESCRIPTION = Name.getName("description");
	private static final Name AUTHORIZATION_SUCCESS = Name.getName("AuthorizationSuccess");
	private static final Name AUTHORIZATION_FAILURE = Name.getName("AuthorizationFailure");
	private static final Name TOKEN_SUCCESS = Name.getName("TokenGenerationSuccess");
	private static final Name TOKEN_FAILURE = Name.getName("TokenGenerationFailure");

    private SessionOptions    d_sessionOptions;
    private Session           d_session;
    private ArrayList<String> d_securities;
    private ArrayList<String> d_options;
    private SubscriptionList  d_subscriptions;
    private SimpleDateFormat  d_dateFormat;
	private String  		  d_authOption;
	private String  		  d_name;
	private Identity 		  d_identity;

	private ArrayList<String> d_hosts;
	private Integer           d_port;
    private String 			  d_token;
    private String 			  d_authServiceName;
    private String 			  d_mktDepthServiceName;
    
    /**
     * @param args
     */
    public static void main(String[] args) throws java.lang.Exception
    {
        System.out.println("Realtime Market Depth Subscription Example");
        MarketDepthSubscriptionExample example = new MarketDepthSubscriptionExample();
        example.run(args);
    }

    public MarketDepthSubscriptionExample()
    {
        d_port = 8194;
        d_hosts = new ArrayList<String>();
    	d_sessionOptions = new SessionOptions();
        
        d_authOption = "NONE";
        d_securities = new ArrayList<String>();
        d_options = new ArrayList<String>();
        d_subscriptions = new SubscriptionList();
        d_dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss.SSS");
        d_name = "";
        
        d_authServiceName = "//blp/apiauth";
        d_mktDepthServiceName = "//blp/mktdepthdata";
    }

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
		
		d_sessionOptions.setServerAddresses(servers);
		d_sessionOptions.setAutoRestartOnDisconnection(true);
		d_sessionOptions.setNumStartAttempts(d_hosts.size());

        d_session = new Session(d_sessionOptions, new SubscriptionEventHandler());
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
	
	boolean isBPipeAuthorized() throws Exception 
	{
		boolean isRunning = true;
		
		EventQueue authEventQueue = new EventQueue();
		Service authService = d_session.getService(d_authServiceName);
		Request authRequest = authService.createAuthorizationRequest();
		authRequest.set("token", d_token);

		d_identity = d_session.createIdentity();
		d_session.sendAuthorizationRequest(authRequest, d_identity, authEventQueue, new CorrelationID(1));

		while(isRunning)
		{
			Event event = authEventQueue.nextEvent();
			System.out.println("process auth Events");
			MessageIterator msgIter = event.messageIterator();
			while (msgIter.hasNext()) {
				Message msg = msgIter.next();
				if (msg.messageType() == AUTHORIZATION_SUCCESS) {
					System.out.println("Authorization SUCCESS");
					isRunning = false;
				} else if (msg.messageType() == AUTHORIZATION_FAILURE) {
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
	

	private void run(String[] args) throws Exception
    {
        if (!parseCommandLine(args)) return;
        if (!createSession()) return;

        if (d_authOption == "NONE")
        {
        	System.out.println("Subscribing...");
        	d_session.subscribe(d_subscriptions);
        }
        else
        {
            // Authenticate user using Generate Token Request 
            if (!generateToken()) return;

            //Authorization : pass Token into authorization request. Returns User handle with user's entitlements info set by server.
            if (!isBPipeAuthorized()) return;

            System.out.println("Subscribing...\n");
            d_session.subscribe(d_subscriptions, d_identity);
        }
        
        // wait for enter key to exit application
        System.in.read();

        d_session.stop();
        System.out.println("Exiting.");
    }

    class SubscriptionEventHandler implements EventHandler
    {
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

        private boolean processSubscriptionStatus(Event event, Session session)
        throws Exception
        {
            System.out.println("Processing SUBSCRIPTION_STATUS: " + event.eventType().toString());
            MessageIterator msgIter = event.messageIterator();
            while (msgIter.hasNext()) {
                Message msg = msgIter.next();
                String topic = (String) msg.correlationID().object();
                System.out.println(
                        d_dateFormat.format(Calendar.getInstance().getTime()) +
                        ": " + topic + " - " + msg.messageType());

                System.out.println("MESSAGE: " + msg);
                System.out.println("");
            }
            return true;
        }

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
                        ": " + topic + " - " + msg.toString());
            }
            return true;
        }

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

    private boolean parseCommandLine(String[] args)
    {
        for (int i = 0; i < args.length; ++i) {
            if (args[i].equalsIgnoreCase("-s") && i + 1 < args.length) {
                d_securities.add(args[++i]);
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
            else if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length) {
                d_hosts.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-p") && i + 1 < args.length) {
                d_port = Integer.parseInt(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-h")) {
                printUsage();
                return false;
            }
        }
        
        // check for application name
        if (d_authOption.equalsIgnoreCase("APPLICATION") && (d_name.length() == 0)) {
        	System.out.println("Application name cannot be NULL for application authorization.");
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

        if (d_securities.size() == 0) {
            d_securities.add("/ticker/VOD LN Equity");
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
        
        int index = 0;
        for (String security : d_securities) {
        	String tempSecurity = security;
        	if (!tempSecurity.startsWith("/"))
        	{
        		tempSecurity = "/" + tempSecurity;
        	}
        	if (!tempSecurity.startsWith("//"))
        	{
        		tempSecurity = d_mktDepthServiceName + tempSecurity;
        	}
            d_subscriptions.add(new Subscription(tempSecurity + subscriptionOptions, new CorrelationID(security)));
            System.out.println("Subscription string: " + d_subscriptions.get(index++));
        }

        return true;
    }

    private void printUsage()
    {
        System.out.println("Usage:");
        System.out.println("	Retrieve realtime market depth data using Bloomberg V3 API");
        System.out.println("		[-s			<security	= \"/ticker/VOD LN Equity\">");
        System.out.println("		[-o			<type=MBO, type=MBL, type=TOP or type=MMQ>");
        System.out.println("		[-ip 		<ipAddress	= localhost>");
        System.out.println("		[-p 		<tcpPort	= 8194>");
		System.out.println("		[-auth      <authenticationOption = LOGON or APPLICATION or DIRSVC>]");
		System.out.println("        [-n         <name = applicationName or directoryService>]");
		System.out.println("Notes:");
		System.out.println(" -Specify only LOGON to authorize 'user' using Windows login name.");
		System.out.println(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println(" -Specify APPLICATION and name(Application Name) to authorize application.");
    }
}
