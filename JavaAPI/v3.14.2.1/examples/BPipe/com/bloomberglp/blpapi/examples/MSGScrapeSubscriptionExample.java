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

import java.io.IOException;
import java.util.ArrayList;

import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Event.EventType;
import com.bloomberglp.blpapi.EventQueue;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Subscription;
import com.bloomberglp.blpapi.SubscriptionList;
import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.Identity;
import com.bloomberglp.blpapi.SessionOptions.ServerAddress;

public class MSGScrapeSubscriptionExample
{
	private String MSGSCRAPE_SVC = "//blp/msgscrape";
	private String AUTH_SVC = "//blp/apiauth";
	
	private static final Name AUTHORIZATION_SUCCESS = Name
		.getName("AuthorizationSuccess");
	private static final Name TOKEN_SUCCESS = Name
		.getName("TokenGenerationSuccess");

	private ArrayList<String> d_hosts;
	private ArrayList<String> d_securities;
    private ArrayList<String> d_fields;
    private ArrayList<String> d_options;
    private int d_port;
	private String  d_name;
	private String  d_authOption;
	private Session d_session;
	private Identity d_identity;

    /**
     * @param args
     */
    public static void main(String[] args) throws Exception
    {
    	MSGScrapeSubscriptionExample example = new MSGScrapeSubscriptionExample();
        example.run(args);
        System.out.println("Press ENTER to quit");
        System.in.read();
    }

    private void run(String[] args) throws Exception
    {
        d_port = 8194;
        d_securities = new ArrayList<String>();
        d_fields = new ArrayList<String>();
        d_options = new ArrayList<String>();
        d_hosts = new ArrayList<String>();
        d_authOption="";
		d_name="";
		d_session = null;

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
        if (!d_session.openService(MSGSCRAPE_SVC)) {
        	System.err.println("Failed to open " + MSGSCRAPE_SVC);
        	d_session.stop();
        	return;
        }

		// Options
		int optCnt = 1;
		for(String opt : d_options)
		{
			System.out.println("Option " + optCnt++ + ": " + opt);
		}

        SubscriptionList subscriptions = new SubscriptionList();

        for (String security : d_securities)
        {
	        subscriptions.add(new Subscription(
	                security,
	                d_fields,
	                d_options,
	        		new CorrelationID(security)));
        }
        System.out.println("Subscribing...");
        if (d_authOption == null)
        {
        	d_session.subscribe(subscriptions);
        }
        else
        {
			// subscribe data with identity object
        	d_session.subscribe(subscriptions, d_identity);
        }

        while (true) {
            Event event = d_session.nextEvent();
            MessageIterator msgIter = event.messageIterator();
            while (msgIter.hasNext()) {
                Message msg = msgIter.next();
                if (event.eventType() == Event.EventType.SUBSCRIPTION_DATA ||
                	event.eventType() == Event.EventType.SUBSCRIPTION_STATUS) {
                	String topic = (String)msg.correlationID().object();
                    System.out.println(topic + ": " + msg.asElement());
                }
            }
        }
    }
    
    private boolean createSession()
    	throws IOException, InterruptedException {
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

		System.out.println("authOptions = " + authOptions);
		SessionOptions sessionOptions = new SessionOptions();
		if (d_authOption != null)
		{
			sessionOptions.setAuthenticationOptions(authOptions);
			System.out.println("Authentication Options = " + sessionOptions.authenticationOptions());
		}

        sessionOptions.setDefaultSubscriptionService(MSGSCRAPE_SVC);

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

    private boolean parseCommandLine(String[] args)
    {
        for (int i = 0; i < args.length; ++i) {
            if (args[i].equalsIgnoreCase("-s")) {
                d_securities.add(args[i+1]);
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
            else {
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
        
        if (d_options.size() > 1)
        {
        	System.out.println("Only one EID can be provided");
        	printUsage();
        	return false;
        }
        
        if (d_securities.size() == 0) {
            d_securities.add("MSGSCRP MSG1 Curncy");
        }

        if (d_fields.size() == 0) {
            d_fields.add("BID");
            d_fields.add("BID_SIZE");
            d_fields.add("ASK");
            d_fields.add("ASK_SIZE");
        }

        // check for application name
        if ((d_authOption == "APPLICATION") && (d_name == ""))
        {
        	System.out.println("Application name cannot be NULL for application authorization.");
            printUsage();
            return false;
        }
        // check for Directory Service name
        if ((d_authOption == "DIRSVC") && (d_name == ""))
        {
        	System.out.println("Directory Service property name cannot be NULL for DIRSVC authorization.");
            printUsage();
            return false;
        }

        return true;
    }

    private void printUsage()
    {
        System.out.println("Usage:");
        System.out.println("  Subscribe to MSG Scrape data ");
        System.out.println("        [-s         <security = MSGSCRP MSG1 Curncy>");
        System.out.println("        [-f         <field = BID>");
        System.out.println("        [-o         <options = EID=44321>");
        System.out.println("        [-ip        <ipAddress = localhost>");
        System.out.println("        [-p         <tcpPort = 8194>");
		System.out.println("		[-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC>]");
		System.out.println("        [-n         <name = applicationName or directoryService>]");
		System.out.println("Notes:");
		System.out.println(" -Only one EID can be specified.");
		System.out.println(" -Specify only LOGON to authorize 'user' using Windows login name.");
		System.out.println(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println(" -Specify APPLICATION and name(Application Name) to authorize application.");
		System.out.println("");
		System.out.println("Example: MSGScrapeSubscriptionExample ip <Host IP> -p <Host Port> -s \"MSGSCRP MSG1 Curncy\"");
        System.out.println("         MSGScrapeSubscriptionExample ip <Host IP> -p <Host Port> -s \"MSGSCRP MSG1 Curncy\" -o EID=44321");
    }
}
