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
 /*
** BulfRefDataExample.java
**
** This Example shows how to Retrieve reference data/Bulk reference data 
** using Server API
** Usage: 
**      		-s			<security	= CAC Index>
**      		-f			<field		= INDX_MWEIGHT>
**      		-ip 		<ipAddress	= localhost>
**      		-p 			<tcpPort	= 8194>
**				-auth       <authenticationOption = LOGON (default) or NONE 
**								or APPLICATION or DIRSVC or USER_APP>
**				-n 			<name = applicationName or directoryService>
** e.g. BulfRefDataExample -s "CAC Index" -f INDX_MWEIGHT -ip localhost -p 8194
*/
package com.bloomberglp.blpapi.examples;

import java.util.ArrayList;
import java.io.IOException;

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
import com.bloomberglp.blpapi.Schema.Datatype;
import com.bloomberglp.blpapi.Identity;
import com.bloomberglp.blpapi.SessionOptions.ServerAddress;

public class BulkRefDataExample
{
	private String REFDATA_SVC = "//blp/refdata";
	private String AUTH_SVC = "//blp/apiauth";

	private static final Name SECURITY_DATA = new Name("securityData");
    private static final Name SECURITY = new Name("security");
    private static final Name FIELD_DATA = new Name("fieldData");
    private static final Name RESPONSE_ERROR = new Name("responseError");
    private static final Name SECURITY_ERROR = new Name("securityError");
    private static final Name FIELD_EXCEPTIONS = new Name("fieldExceptions");
    private static final Name FIELD_ID = new Name("fieldId");
    private static final Name ERROR_INFO = new Name("errorInfo");
    private static final Name CATEGORY = new Name("category");
    private static final Name MESSAGE = new Name("message");
	private static final Name AUTHORIZATION_SUCCESS = Name.getName("AuthorizationSuccess");
	private static final Name TOKEN_SUCCESS = Name.getName("TokenGenerationSuccess");

	private ArrayList<String> d_hosts;
    private int       d_port;
    private String     d_authOption;
    private String     d_name;
    private Session    d_session;
    private Identity   d_identity;
    private ArrayList<String> d_securities;
    private ArrayList<String> d_fields;
    
    /**
     * @param args
     */
    public static void main(String[] args) throws Exception
    {
        System.out.println("Reference Data/Bulk Reference Data Example");
        BulkRefDataExample example = new BulkRefDataExample();
        example.run(args);

        System.out.println("Press ENTER to quit");
        System.in.read();
    }

    /**
     * Constructor
     */
    public BulkRefDataExample()
    {
    	d_hosts = new ArrayList<String>();
        d_port = 8194;
        d_securities = new ArrayList<String>();
        d_fields = new ArrayList<String>();
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

    /**
     * Reads command line arguments 
     * Establish a Session
     * Identify and Open refdata Service
     * Send ReferenceDataRequest to the Service 
     * Event Loop and Response Processing
     */
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
        if (!d_session.openService(REFDATA_SVC)) {
        	System.err.println("Failed to open " + REFDATA_SVC);
        	d_session.stop();
        	return;
        }

        try {
            sendRefDataRequest(d_session);
        } catch (InvalidRequestException e) {
            e.printStackTrace();
        }

        // wait for events from session.
        eventLoop(d_session);

        d_session.stop();
    }

    /**
     * Polls for an event or a message in an event loop
     */
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

    /**
     * Function to handle response event
     */
    private void processResponseEvent(Event event)
    throws Exception
    {
        MessageIterator msgIter = event.messageIterator();
        while (msgIter.hasNext()) {
            Message msg = msgIter.next();
            if (msg.hasElement(RESPONSE_ERROR)) {
                printErrorInfo("REQUEST FAILED: ", msg.getElement(RESPONSE_ERROR));
                continue;
            }

            Element securities = msg.getElement(SECURITY_DATA);
            int numSecurities = securities.numValues();
            System.out.println("Processing " + numSecurities + " securities:");
            for (int i = 0; i < numSecurities; ++i) {
                Element security = securities.getValueAsElement(i);
                String ticker = security.getElementAsString(SECURITY);
                System.out.println("\nTicker: " + ticker);
                if (security.hasElement("securityError")) {
                    printErrorInfo("\tSECURITY FAILED: ",
                                   security.getElement(SECURITY_ERROR));
                    continue;
                }

                if (security.hasElement(FIELD_DATA)) {
                    Element fields = security.getElement(FIELD_DATA);
                    if (fields.numElements() > 0) {
                        System.out.println("FIELD\t\tVALUE");
                        System.out.println("-----\t\t-----");
                        int numElements = fields.numElements();
                        for (int j = 0; j < numElements; ++j) {
                            Element field = fields.getElement(j);
                            // Checking if the field is Bulk field
                            if(field.datatype() == Datatype.SEQUENCE){
                            	processBulkField(field);
                            }else{
                            	processRefField(field);
                            }
                        }
                    }
                }
                System.out.println("");
                Element fieldExceptions = security.getElement(FIELD_EXCEPTIONS);
                if (fieldExceptions.numValues() > 0) {
                    System.out.println("FIELD\t\tEXCEPTION");
                    System.out.println("-----\t\t---------");
                    for (int k = 0; k < fieldExceptions.numValues(); ++k) {
                        Element fieldException =
                            fieldExceptions.getValueAsElement(k);
                        printErrorInfo(fieldException.getElementAsString(FIELD_ID) 
                        		+ "\t\t", fieldException.getElement(ERROR_INFO));
                    }
                }
            }
        }
    }

    /**
     * Read the reference bulk field data
     */
    private void processBulkField(Element refBulkField)
    {
    	System.out.println("\n" + refBulkField.name());
        // Get the total number of Bulk data points
        int numofBulkValues = refBulkField.numValues();
        for (int bvCtr = 0; bvCtr < numofBulkValues; bvCtr++){
            Element bulkElement = refBulkField.getValueAsElement(bvCtr);
            // Get the number of sub fields for each bulk data element
            int numofBulkElements = bulkElement.numElements();
            // Read each field in Bulk data
            for (int beCtr = 0; beCtr < numofBulkElements; beCtr++){
                Element elem = bulkElement.getElement(beCtr);
                System.out.println("\t\t" + elem.name() + 
                				"\t\t" + elem.getValueAsString());
            }
        }
    }

    /**
     * Read the reference field data
     */
   private void processRefField(Element reffield)
    {
        System.out.println(reffield.name() + "\t\t" 
        					+ reffield.getValueAsString());
    }

   /**
    * Function to send Reference data request
    */
  private void sendRefDataRequest(Session session) throws Exception
    {
        Service refDataService = session.getService("//blp/refdata");
        Request request = refDataService.createRequest("ReferenceDataRequest");

        // Add securities to request
        Element securities = request.getElement("securities");

        for (int i = 0; i < d_securities.size(); ++i) {
            securities.appendValue((String)d_securities.get(i));
        }

        // Add fields to request
        Element fields = request.getElement("fields");
        for (int i = 0; i < d_fields.size(); ++i) {
            fields.appendValue((String)d_fields.get(i));
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

  /**
   * Parses the command line arguments
   */
   private boolean parseCommandLine(String[] args)
    {
        for (int i = 0; i < args.length; ++i) {
            if (args[i].equalsIgnoreCase("-s") && i + 1 < args.length) {
                d_securities.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-f") && i + 1 < args.length) {
                d_fields.add(args[++i]);
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

        if (d_securities.size() == 0) {
            d_securities.add("CAC Index");
       }

        if (d_fields.size() == 0) {
            d_fields.add("INDX_MWEIGHT");
        }

        return true;
    }

   /**
    * Prints Error Information
    */
   private void printErrorInfo(String leadingStr, Element errorInfo)
    throws Exception
    {
        System.out.println(leadingStr + errorInfo.getElementAsString(CATEGORY) +
                           " (" + errorInfo.getElementAsString(MESSAGE) + ")");
    }

   /**
    * Prints Program Usage
    */
   private void printUsage()
    {
        System.out.println("Usage:");
        System.out.println("    Retrieve reference data/Bulk reference" 
        					+ " data using Server API");
        System.out.println("      [-s         <security   = CAC Index>");
        System.out.println("      [-f         <field      = INDX_MWEIGHT>");
        System.out.println("      [-ip        <ipAddress  = localhost>");
        System.out.println("      [-p         <tcpPort    = 8194>");
		System.out.println("      [-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]");
		System.out.println("      [-n         <name = applicationName or directoryService>]");
		System.out.println("Notes:");
		System.out.println(" -Specify only LOGON to authorize 'user' using Windows login name.");
		System.out.println(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println(" -Specify APPLICATION and name(Application Name) to authorize application.");
    }
}
