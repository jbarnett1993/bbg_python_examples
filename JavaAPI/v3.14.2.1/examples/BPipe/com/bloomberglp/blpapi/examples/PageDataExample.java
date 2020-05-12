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

// *****************************************************************************
// This program demonstrates how to make a subscription to Page Based data. 
//   It uses the Market Bar service(//blp/pagedata) 
//	provided by API. Program does the following:
//		1. Establishes a session which facilitates connection to the bloomberg 
//		   network.
//		2. Initiates the Page data Service(//blp/pagedata) for realtime
//		   data.
//		3. Creates and sends the request via the session.
//			- Creates a subscription list
//			- Adds Page data topic to subscription list.
//			- Subscribes to realtime Page data
//		4. Event Handling of the responses received.
//       5. Parsing of the message data.
// Usage: 
//         	-t			<Topic  	= "0708/012/0001">
//                                   i.e."Broker ID/Category/Page Number"
//     		-ip 		<ipAddress	= localhost>
//     		-p 			<tcpPort	= 8194>
//          -auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>
//		    -n         <name = applicationName or directoryService>
//Notes:
// -Specify only LOGON to authorize 'user' using Windows login name.
// -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.
//
//   example usage:
//	PageDataExample -t "0708/012/0001" -ip localhost -p 8194
//
// Prints the response on the console of the command line requested data
//******************************************************************************/

package com.bloomberglp.blpapi.examples;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.HashMap;

import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Event.EventType;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.EventHandler;
import com.bloomberglp.blpapi.EventQueue;
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

public class PageDataExample
{
	private static final String AUTH_SVC = "//blp/apiauth";

	private static final Name AUTHORIZATION_SUCCESS = Name.getName("AuthorizationSuccess");
	private static final Name TOKEN_SUCCESS = Name.getName("TokenGenerationSuccess");

	private static final Name EXCEPTIONS = new Name("exceptions");
    private static final Name FIELD_ID = new Name("fieldId");
    private static final Name REASON = new Name("reason");
    private static final Name CATEGORY = new Name("category");
    private static final Name DESCRIPTION = new Name("description");
    private static final Name ROWUPDATE = new Name("rowUpdate");
    private static final Name NUMROWS = new Name("numRows");
    private static final Name NUMCOLS = new Name("numCols");
    private static final Name ROWNUM = new Name("rowNum");
    private static final Name SPANUPDATE = new Name("spanUpdate");
    private static final Name STARTCOL = new Name("startCol");
    private static final Name LENGTH = new Name("length");
    private static final Name TEXT = new Name("text");

	private ArrayList<String> d_hosts;
    private int               d_port;
    private String            d_authOption;
    private String            d_name;
    private Identity          d_identity;
    private Session           d_session;
    private HashMap<String, ArrayList<StringBuilder>>	  d_topicTable;
    private ArrayList<String> d_topics;
    private SimpleDateFormat  d_dateFormat;
    private String            d_service;

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
	
		ServerAddress[] servers = new ServerAddress[d_hosts.size()];
		for (int i = 0; i < d_hosts.size(); ++i) {
			servers[i] = new ServerAddress(d_hosts.get(i), d_port);
		}
	
		sessionOptions.setServerAddresses(servers);
	    sessionOptions.setAutoRestartOnDisconnection(true);
	    sessionOptions.setNumStartAttempts(d_hosts.size());
		sessionOptions.setDefaultSubscriptionService(d_service);
		
		System.out.print("Connecting to port " + d_port + " on server:");
		for (ServerAddress server: sessionOptions.getServerAddresses()) {
			System.out.print(" " + server);
		}
		System.out.println();
	    d_session = new Session(sessionOptions, new SubscriptionEventHandler());
	    
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
    
    /**
     * @param args
     */
    public static void main(String[] args) throws java.lang.Exception
    {
        System.out.println("Page Data Event Handler Example");
        PageDataExample example = new PageDataExample();
        example.run(args);
    }

    public PageDataExample()
    {
    	d_hosts = new ArrayList<String>();
    	d_port = 8194;
        
        d_service = "//blp/mktdata";
        d_topicTable = new HashMap<String, ArrayList<StringBuilder>>();
        d_topics = new ArrayList<String>();
        d_dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss.SSS");
        d_authOption = "";
        d_name = "";
    }

    private void subscribe() throws IOException
    {
    	SubscriptionList subscriptions = new SubscriptionList();
    	ArrayList<String> fields = new ArrayList<String>();
    	
    	d_topicTable.clear();
    	fields.add("6-23");
        // Following commented code shows some of the sample values 
        // that can be used for field other than above
        // e.g. fields.Add("1");
        //      fields.Add("1,2,3");
        //      fields.Add("1,6-10,15,16");

    	for (String topic : d_topics) {
			subscriptions.add(new Subscription("//blp/pagedata/" + topic, 
					fields, new CorrelationID(topic)));
			d_topicTable.put(topic, new ArrayList<StringBuilder>());
		} 
        if (d_authOption == null)
        {
	        System.out.println("Subscribing...");
        	d_session.subscribe(subscriptions);
        }
        else
        {
            System.out.println("Subscribing with Identity...");
            d_session.subscribe(subscriptions, d_identity);
        }
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

		subscribe();
		
		// wait for enter key to exit application
		System.out.println("Press ENTER to quit");
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
                System.out.println("MESSAGE: " + msg);
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

                if (msg.hasElement(EXCEPTIONS)) {
                    // This can occur on SubscriptionStarted if at least
                    // one field is good while the rest are bad.
                    Element exceptions = msg.getElement(EXCEPTIONS);
                    for (int i = 0; i < exceptions.numValues(); ++i) {
                        Element exInfo = exceptions.getValueAsElement(i);
                        Element fieldId = exInfo.getElement(FIELD_ID);
                        Element reason = exInfo.getElement(REASON);
                        System.out.println("\t" + fieldId.getValueAsString() +
                                ": " + reason.getElement(CATEGORY).getValueAsString());
                    }
                }
                System.out.println("");
            }
            return true;
        }

        private boolean processSubscriptionDataEvent(Event event, Session session)
        throws Exception
        {
            System.out.println("Processing SUBSCRIPTION_DATA");
            for (Message msg : event)
            {
                String topic = (String) msg.correlationID().object();
                System.out.println(
                        d_dateFormat.format(Calendar.getInstance().getTime()) +
                        ": " + topic + " - " + msg.messageType());

                if (msg.messageType().equals("PageUpdate"))
                {
                	processPageElement(msg.asElement(), topic);
                }else if (msg.messageType().equals("RowUpdate")) {
					processRowElement(msg.asElement(), topic);
				}
            }
            return true;
        }

        private void processPageElement(Element pageElement, String topic)
        {
        	Element eleNumRows = pageElement.getElement(NUMROWS);
        	int numRows = eleNumRows.getValueAsInt32();
        	Element eleNumCols = pageElement.getElement(NUMCOLS);
        	int numCols = eleNumCols.getValueAsInt32();
        	System.out.println("Page Contains " + numRows + " Rows & " +
        			numCols + " Columns");
        	Element eleRowUpdates = pageElement.getElement(ROWUPDATE);
        	int numRowUpdates = eleRowUpdates.numValues(); 
        	for (int i = 0; i < numRowUpdates - 1; i++) {
				Element rowUpdate = eleRowUpdates.getValueAsElement(i);
				processRowElement(rowUpdate, topic);
			}
        }
        
        private void processRowElement(Element rowElement, String topic)
        {
        	Element eleRowNum = rowElement.getElement(ROWNUM);
        	int rowNum = eleRowNum.getValueAsInt32();
        	Element eleSpanUpdates = rowElement.getElement(SPANUPDATE);
        	int numSpanUpdates = eleSpanUpdates.numValues();
        	
        	for (int i = 0; i < numSpanUpdates; i++) {
				Element spanUpdate = eleSpanUpdates.getValueAsElement(i);
				processSpanElement(spanUpdate, rowNum, topic);
			}
        }
        
        private void processSpanElement(Element spanElement, int rowNum, String topic)
        {
        	Element eleStartCol = spanElement.getElement(STARTCOL);
        	int startCol = eleStartCol.getValueAsInt32();
        	Element eleLength = spanElement.getElement(LENGTH);
        	int len = eleLength.getValueAsInt32();
        	Element eleText = spanElement.getElement(TEXT);
        	String text = eleText.getValueAsString();
        	System.out.println("Row : " + rowNum +
        			", Col : " + startCol +
        			" (Len : " + len + ")" +
        			" New Text : " + text);
        	ArrayList<StringBuilder> rowList = d_topicTable.get(topic);
        	while (rowList.size() < rowNum)
        	{
        		rowList.add(new StringBuilder());
        	}
        	
        	StringBuilder rowText = rowList.get(rowNum - 1);
        	if (rowText.length() == 0) {
        		rowText.append(String.format("%80s", text));
        	} else {
        		rowText.replace(startCol - 1, startCol - 1 + len, text);
        		System.out.println(rowText.toString());
        	}
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
            if (args[i].equalsIgnoreCase("-t")) {
                d_topics.add(args[++i]);
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

        // set default topics if nothing is specified
        if (d_topics.size() == 0) {
            d_topics.add("0708/012/0001");
            d_topics.add("1102/1/274");
        }

        return true;
    }

    private void printUsage()
    {
        System.out.println("Usage:");
        System.out.println("	Retrieve page based data using V3 API");
        System.out.println("        [-t         <Topic	= 0708/012/0001>");
        System.out.println("        [              i.e.\"Broker ID/Category/Page Number\"");
        System.out.println("        [-ip        <ipAddress	= localhost>");
        System.out.println("        [-p         <tcpPort	= 8194>");
		System.out.println("        [-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]");
		System.out.println("        [-n         <name = applicationName or directoryService>]");
		System.out.println("Notes:");
		System.out.println(" -Specify only LOGON to authorize 'user' using Windows login name.");
		System.out.println(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println(" -Specify APPLICATION and name(Application Name) to authorize application.");
        System.out.println("e.g. PageDataExample -t \"0708/012/0001\" -ip localhost -p 8194");
    }
}
