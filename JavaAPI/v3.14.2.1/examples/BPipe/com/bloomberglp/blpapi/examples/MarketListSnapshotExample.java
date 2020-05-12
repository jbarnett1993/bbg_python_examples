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

import java.util.ArrayList;
import java.util.logging.Level;
import java.io.IOException;

import com.bloomberglp.blpapi.Datetime;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Event.EventType;
import com.bloomberglp.blpapi.EventQueue;
import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.InvalidRequestException;
import com.bloomberglp.blpapi.Logging;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Identity;
import com.bloomberglp.blpapi.SessionOptions.ServerAddress;

public class MarketListSnapshotExample
{
	private String MKTLIST_SVC = "//blp/mktlist";
	private String AUTH_SVC = "//blp/apiauth";

    private static final Name RESPONSE_CODE = new Name("responseCode");
    private static final Name RESULT_TEXT = new Name("resultText");
    private static final Name SOURCE_ID = new Name("sourceId");
    private static final Name RESULT_CODE = new Name("resultCode");
    private static final Name SNAPSHOT = new Name("snapshot");
	private static final Name AUTHORIZATION_SUCCESS = Name.getName("AuthorizationSuccess");
	private static final Name TOKEN_SUCCESS = Name.getName("TokenGenerationSuccess");

	private ArrayList<String> d_hosts;
    private int               d_port;
    private String     d_authOption;
    private String     d_name;
    private Session    d_session;
    private Identity   d_identity;
    private String     d_security;

    /**
     * @param args
     */
    public static void main(String[] args) throws Exception
    {
        System.out.println("Market List Snapshot Example");
        MarketListSnapshotExample example = new MarketListSnapshotExample();
        example.run(args);

        System.out.println("Press ENTER to quit");
        System.in.read();
    }

    public MarketListSnapshotExample()
    {
    	d_hosts = new ArrayList<String>();
        d_port = 8194;
        d_security = "";
        d_authOption="";
		d_name="";
		d_session = null;
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
	
	    sessionOptions.setDefaultSubscriptionService(MKTLIST_SVC);
	
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

    private void run(String[] args) throws Exception
    {
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
        if (!d_session.openService(MKTLIST_SVC)) {
        	System.err.println("Failed to open " + MKTLIST_SVC);
        	d_session.stop();
        	return;
        }

        try {
            sendDataRequest(d_session);
        } catch (InvalidRequestException e) {
            e.printStackTrace();
        }

        // wait for events from session.
        eventLoop(d_session);

        d_session.stop();
    }

    private void eventLoop(Session session) throws Exception
    {
        boolean done = false;
        while (!done) {
            Event event = session.nextEvent();
            if (event.eventType() == Event.EventType.PARTIAL_RESPONSE) {
                System.out.println("Processing Partial Response");
                processResponseEvent(event);
            }
            else if (event.eventType() == Event.EventType.RESPONSE) {
                System.out.println("Processing Response");
                processResponseEvent(event);
                done = true;
            } else {
                MessageIterator msgIter = event.messageIterator();
                while (msgIter.hasNext()) {
                    Message msg = msgIter.next();
                    System.out.println(msg.asElement());
                    if (event.eventType() == Event.EventType.SESSION_STATUS) {
                        if (msg.messageType().equals("SessionTerminated") ||
                            msg.messageType().equals("SessionStartupFailure")) {
                            done = true;
                        }
                    }
                }
            }
        }
    }

	// Read the bulk data contents
	void processBulkData(Element dataItem)
	{
		System.out.println();
		System.out.println("\t" + dataItem.name());
        // Get the total number of Bulk data points
        int numofBulkValues = dataItem.numValues();
        for (int bvCtr = 0; bvCtr < numofBulkValues; bvCtr++) {
            Element  bulkElement = dataItem.getValueAsElement(bvCtr);                                   
            // Get the number of sub fields for each bulk data element
			int numofBulkElements = bulkElement.numElements();									
            // Read each field in Bulk data
            for (int beCtr = 0; beCtr < numofBulkElements; beCtr++){
                Element  elem = bulkElement.getElement(beCtr);
                System.out.println("\t\t" + elem.name() + " = " + 
					elem.getValueAsString());
            }
            System.out.println();
        }
	}

	// return true if processing is completed, false otherwise
    private void processResponseEvent(Event event)
    throws Exception
    {
        MessageIterator msgIter = event.messageIterator();
        while (msgIter.hasNext()) {
            Message msg = msgIter.next();
            
            if (msg.hasElement(RESPONSE_CODE, true)) {
            	Element responseCode = msg.getElement(RESPONSE_CODE);
				int resultCode = responseCode.getElementAsInt32(RESULT_CODE);
				if (resultCode > 0)
				{
					String message = responseCode.getElementAsString(RESULT_TEXT);
					String sourceId = responseCode.getElementAsString(SOURCE_ID);
					System.out.println("Request Failed: " + message);
					System.out.println("Source ID: " + sourceId);
					System.out.println("Result Code: " + resultCode);
					continue;
				}

            }
            
            Element snapshot = msg.getElement(SNAPSHOT);
			int numElements = snapshot.numElements();
			for (int i = 0; i < numElements; ++i)
			{
                Element dataItem = snapshot.getElement(i);
                // Checking if the data item is Bulk data item
				if (dataItem.isArray()){
					processBulkData(dataItem);
				}else{
					System.out.println("\t" + dataItem.name() + " = " +
							dataItem.getValueAsString());
				}
			}
        }
    }

    private void sendDataRequest(Session session) throws Exception
    {
        Service mktListService = session.getService(MKTLIST_SVC);
        Request request = mktListService.createRequest("SnapshotRequest");
		request.set("security", d_security);

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

    private void registerCallback(int verbosity)
    {
        Logging.Callback loggingCallback = new Logging.Callback() {
            public void onMessage(long threadId, Level level, Datetime dateTime,
                                  String loggerName, String message) {
                System.out.println(loggerName + " [" + level.toString() + "] Thread ID = "
                                   + threadId + " " + message);
                }
        };

        Level logLevel = Level.OFF;
        if (verbosity > 0) {
            switch (verbosity) {
            case 1:
                logLevel = Level.INFO;
                break;
            case 2:
                logLevel = Level.FINE;
                break;
            case 3:
                logLevel = Level.FINER;
                break;
            default:
                logLevel = Level.FINEST;
                break;
            };
        }
        Logging.registerCallback(loggingCallback, logLevel);
    }

    private boolean parseCommandLine(String[] args)
    {
        int verbosity = 0;
        for (int i = 0; i < args.length; ++i) {
            if (args[i].equalsIgnoreCase("-s") && i + 1 < args.length) {
                d_security = args[++i];
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
            else if (args[i].equalsIgnoreCase("-v")) {
                ++verbosity;
            }
            else if (args[i].equalsIgnoreCase("-h")) {
                printUsage();
                return false;
            }
        }

        if (verbosity > 0) {
            registerCallback(verbosity);
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

        if (d_security.length() == 0) {
            d_security = "//blp/mktlist/secids/bsym/IBM";
        }

        return true;
    }

    private void printUsage()
    {
        System.out.println("Usage:");
        System.out.println("\tMarket List Snapshot Example ");
        System.out.println("\t\t[-s \t<security = /blp/mktlist/secids/bsym/IBM>");
        System.out.println("\t\t[-ip\t<ipAddress = localhost>");
        System.out.println("\t\t[-p \t<tcpPort = 8194>");
        System.out.println("\t\t[-v \tVerbose");
		System.out.println("\t\t[-auth \t<authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]");
		System.out.println("\t\t[-n \t<name = applicationName or directoryService>]");
		System.out.println("Notes:");
		System.out.println(" -Specify only LOGON to authorize 'user' using Windows login name.");
		System.out.println(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println(" -Specify APPLICATION and name(Application Name) to authorize application.");
    }
}
