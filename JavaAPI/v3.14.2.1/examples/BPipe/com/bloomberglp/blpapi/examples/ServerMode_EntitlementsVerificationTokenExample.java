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
 
//ServerMode_EntitlementsVerificationTokenExample.java

//This program demonstrates a server mode application that authorizes its
//users with tokens returned by a generateToken request. For the purposes
//of this demonstration, the "GetAuthorizationToken" program can be used
//to generate a token and display it on the console. For ease of demonstration
//this application takes one or more 'tokens' on the command line. But in a real
//server mode application the 'token' would be received from the client
//applications using some IPC mechanism.

//Workflow:
//* connect to server
//* open services
//* generate application token
//* send authorization request for application
//* send authorization request for each 'token' which represents a user.
//* send "ReferenceDataRequest" for all specified 'securities' using application Identity
//* for each response message, check which users are entitled to receive
//that message before distributing that message to the user.

//Command line arguments:
//-ip <serverHostNameOrIp>
//-p  <serverPort>
//-a  <application name authentication>
//-t  <user's token>
//-s  <security>
//Multiple securities and tokens can be specified.
//You can use the ServerMode_GetAuthorizationToken sample to generate the tokens.


package com.bloomberglp.blpapi.examples;

import java.io.IOException;
import java.util.*;

import com.bloomberglp.blpapi.*;

public class ServerMode_EntitlementsVerificationTokenExample {

	private final Name RESPONSE_ERROR = new Name("responseError");
	private final Name SECURITY_DATA = new Name("securityData");
	private final Name SECURITY = new Name("security");
	private final Name EID_DATA = new Name("eidData");
	private final Name AUTHORIZATION_SUCCESS = new Name("AuthorizationSuccess");
    private static final Name AUTHORIZATION_FAILURE = new Name("AuthorizationFailure");
    private static final Name TOKEN_SUCCESS = new Name("TokenGenerationSuccess");
    private static final Name TOKEN_FAILURE = new Name("TokenGenerationFailure");
	private final String REFRENCEDATA_REQUEST = "ReferenceDataRequest";

    private final String AUTH_APP_PREFIX = "AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
    private final String  APIAUTH_SVC          = "//blp/apiauth";
	private final String REFDATA_SVC          = "//blp/refdata";

	private String d_host="";
	private int d_port=0;
	private Vector<String> d_securities;
	private Vector<Identity> d_identities;
	private Vector<String> d_tokens;
	private Session d_session;
    private String 	 d_appName;
    private Identity d_appIdentity;
    private Service  d_apiAuthSvc;
    private String	 d_token;

	public ServerMode_EntitlementsVerificationTokenExample(){
		d_host = "localhost";
		d_port = 8194;
		d_securities = new Vector<String>();
		d_identities = new Vector<Identity>();
		d_tokens = new Vector<String>();
		d_appName = "";
		d_token = "";
	}

	private void createSession() throws IOException, InterruptedException{
		SessionOptions sessionOptions = new SessionOptions();
		sessionOptions.setServerHost(d_host);
		sessionOptions.setServerPort(d_port);
		sessionOptions.setAuthenticationOptions(AUTH_APP_PREFIX + d_appName.trim());

		System.out.println("Connecting to " +  d_host + ":" + d_port);

		d_session = new Session(sessionOptions, new SessionEventHandler(d_identities,d_tokens));
		boolean sessionStarted = d_session.start();
		if (!sessionStarted) {
			System.err.println("Failed to start session. Exiting..." );
			System.exit(-1);
		}
	}

	private void openServices() throws InterruptedException, IOException
	{
		if (!d_session.openService("//blp/apiauth")) {
			System.out.println( "Failed to open service: //blp/apiauth" );
			System.exit(-1);
		}

		if (!d_session.openService("//blp/refdata")) {
			System.out.println( "Failed to open service: //blp/refdata" );
			System.exit(-2);
		}
        d_apiAuthSvc = d_session.getService(APIAUTH_SVC);
	}

    private boolean GenerateApplicationToken() throws InterruptedException, IOException
    {
        boolean isTokenSuccess = false;
        boolean isRunning = false;

        d_token = "";
        CorrelationID tokenReqId = new CorrelationID(99);
        EventQueue tokenEventQueue = new EventQueue();
        
        System.out.println("Application token generation");
        d_session.generateToken(tokenReqId, tokenEventQueue);

        while (!isRunning)
        {
            Event eventObj = tokenEventQueue.nextEvent();
            if (eventObj.eventType().equals(Event.EventType.TOKEN_STATUS))
            {
                System.out.println("processTokenEvents");
				MessageIterator msgIter = eventObj.messageIterator();
				while(msgIter.hasNext()) {
					Message msg = msgIter.next();
                    System.out.println(msg.toString());
                    if (msg.messageType().equals(TOKEN_SUCCESS))
                    {
                        d_token = msg.getElementAsString("token");
                        System.out.println("Application token sucess");
                        isTokenSuccess = true;
                        isRunning = true;
                        break;
                    }
                    else if (msg.messageType().equals(TOKEN_FAILURE))
                    {
                        System.out.println("Application token failure");
                        isRunning = true;
                        break;
                    }
                    else
                    {
                        System.out.println("Error while application token generation");
                        isRunning = true;
                        break;
                    }
                }
            }
        }

        return isTokenSuccess;
    }

    private boolean authorizeApplication() throws IOException, InterruptedException
    {
        boolean isAuthorized = false;
        boolean isRunning = true;
        d_appIdentity = null;

        Request authRequest = d_apiAuthSvc.createAuthorizationRequest();

        authRequest.set("token", d_token);
        d_appIdentity = d_session.createIdentity();
        EventQueue authEventQueue = new EventQueue();

        d_session.sendAuthorizationRequest(authRequest, d_appIdentity, authEventQueue, new CorrelationID(1));

        while (isRunning)
        {
            Event eventObj = authEventQueue.nextEvent();
            System.out.println("processEvent");
            if (eventObj.eventType().equals(Event.EventType.RESPONSE) || eventObj.eventType().equals(Event.EventType.REQUEST_STATUS))
            {
				MessageIterator msgIter = eventObj.messageIterator();
				while(msgIter.hasNext()) {
					Message msg = msgIter.next();
                    if (msg.messageType().equals(AUTHORIZATION_SUCCESS))
                    {
                        System.out.println("Application authorization SUCCESS");

                        isAuthorized = true;
                        isRunning = false;
                        break;
                    }
                    else if (msg.messageType().equals(AUTHORIZATION_FAILURE))
                    {
                        System.out.println("Application authorization FAILED");
                        System.out.println(msg.toString());
                        isRunning = false;
                    }
                    else
                    {
                        System.out.println(msg.toString());
                        isRunning = false;
                    }
                }
            }
        }
        return isAuthorized;
    }

	private boolean authorizeUsers() throws InterruptedException, IOException{
		Service authService = d_session.getService(APIAUTH_SVC);
		boolean is_any_user_authorized = false;

		//Authorize each of the users
		d_identities = new Vector<Identity>(d_tokens.size());
		for (int i=0; i<d_tokens.size(); ++i) {
			d_identities.add(d_session.createIdentity());
			Request authRequest = authService.createAuthorizationRequest();
			authRequest.set("token", d_tokens.get(i));

			CorrelationID correlator = new CorrelationID(d_tokens.get(i));
			EventQueue eventQueue = new EventQueue();
			d_session.sendAuthorizationRequest(authRequest,
					d_identities.get(i), eventQueue, correlator);

			Event event = eventQueue.nextEvent();
			if (event.eventType().equals(Event.EventType.RESPONSE) ||
					event.eventType().equals(Event.EventType.RESPONSE)){
				MessageIterator msgIter = event.messageIterator();
				while(msgIter.hasNext()) {
					Message msg = msgIter.next();
					if(msg.messageType().equals(AUTHORIZATION_SUCCESS)){
						System.out.println("User #" + i+1 + " authorization success");
						is_any_user_authorized = true;
					}
					else {
						System.out.println("User #" + i+1 + " authorization failed");
						printEvent(event);
					}
				}
			}                   
		}       
		return is_any_user_authorized;       
	}

	private void printEvent(Event event) throws IOException {
		MessageIterator msgIter = event.messageIterator();
		while(msgIter.hasNext()) {
			Message msg = msgIter.next();
			CorrelationID correlationId = msg.correlationID();
			if(correlationId != null) {
				System.out.println("Correlator: " + correlationId);
			}
			msg.print(System.out);
		}
	}

	private void sendRefDataRequest() throws IOException{
		Service service = d_session.getService(REFDATA_SVC);
		Request request = service.createRequest(REFRENCEDATA_REQUEST);

		//Add securities.
		Element securities = request.getElement("securities");
		for (int i=0; i<d_securities.size(); ++i){
			securities.appendValue(d_securities.get(i));
		}

		//Add fields
		Element fields = request.getElement("fields");
		fields.appendValue("PX_LAST");
		fields.appendValue("DS002");

		request.set("returnEids", true);

		// Send the request using the server's credentials
		System.out.println("Sending RefDataRequest using server credentials...");
		d_session.sendRequest(request, d_appIdentity, null);
	}

	private void run(String[] args){
		if (!parseCommandLine(args)) {
			return;
		}

		try
		{
			createSession();
			openServices();
	
		    // Generate server side Application Name token
            if (GenerateApplicationToken())
            {
                // Authorize server side Application Name Identity for use with request/subscription
                if (authorizeApplication())
                {
					// Authorize all the users that are interested in receiving data
					if (authorizeUsers()) {
						// Make the various requests that we need to make with application's Identity
						sendRefDataRequest();
					}
                }
            }
			System.out.println("Press ENTER to quit");
			System.in.read();
			d_session.stop();
			System.out.println("Exiting...");
		}
		catch (Exception e) {
			e.printStackTrace();
		}
	}

	private boolean parseCommandLine(String[] args){
		int tokenCount = 0;
		int len = args.length;
		for (int i=0; i<len; ++i){
			if(args[i].equals("-s") && i + 1 < len) {
				d_securities.add(args[++i]);
			}
			else if(args[i].equals("-a") && i + 1 < len) {
				d_appName = args[++i]; 
			}
			else if(args[i].equals("-t") && i + 1 < len) {
				d_tokens.add(args[++i]);
				++tokenCount;
				System.out.println("User #" + tokenCount + " token: " + args[i]);
			}
			else if(args[i].equals("-ip") && i + 1 < len) {
				d_host = args[++i];
			}
			else if(args[i].equals("-p") && i + 1 < len) {
				d_port = Integer.parseInt(args[++i]);
			}
			else
			{
				printUsage();
				return false;
			}
		}
		
        if (d_appName.length() == 0)
        {
            System.out.println("No server side Application Name were specified");
            printUsage();
            return false;
        }

		if(d_tokens.size()==0) {
			System.out.println("No tokens were specified");
			printUsage();
			return false;
		}

		if(d_securities.size()==0) {
			d_securities.add("MSFT US Equity");
		}

		return true;
	}

	private void printUsage(){
		System.out.println("Usage: ");
		System.out.println("    Entitlements verification token example");
		System.out.println("        [-s     <security   = MSFT US Equity>]");
		System.out.println("        [-a		<application name authentication>]");
		System.out.println("        [-t     <user's token string>]");
		System.out.println("        ie. token value returned in generateToken response");
		System.out.println("        [-ip    <ipAddress  = localhost>]");
		System.out.println("        [-p     <tcpPort    = 8194>]");
		System.out.println("Note:");
		System.out.println("Multiple securities and tokens can be specified.");
	}

	public static void main(String[] args) {
		System.out.println("Entitlements Verification Token Example");	
		ServerMode_EntitlementsVerificationTokenExample example = new ServerMode_EntitlementsVerificationTokenExample();
		try {
			example.run(args);
		}
		catch (Exception e) {
			System.err.println( "Library Exception!!! " + e.getMessage()); 
		}
	}

	class SessionEventHandler implements EventHandler{
		public SessionEventHandler(Vector<Identity> identities, Vector <String>tokens){
			d_identities = identities;
			d_tokens = tokens;
		}

		void printFailedEntitlements(Vector<Integer> failedEntitlements){
	        for (int i = 0; i < failedEntitlements.size(); ++i) {
	            System.out.print(failedEntitlements.get(i) + " ");
	        }
	        System.out.println();
		}
		
		private void distributeMessage(Message msg)
		{
			Service service = msg.service();
			Vector<Integer> failedEntitlements = new Vector<Integer>();
			Element securities = msg.getElement(SECURITY_DATA);
			int numSecurities = securities.numValues();

			System.out.println("Processing " + numSecurities + " securities:");
			for(int i=0; i<numSecurities; ++i){
				Element security = securities.getValueAsElement(i);
				String ticker    = security.getElementAsString(SECURITY);
				Element entitlements=null;
				if (security.hasElement(EID_DATA)) {
					entitlements = security.getElement(EID_DATA);
				}

				int numUsers = d_identities.size();
				if (!entitlements.isNull() && entitlements.numValues() > 0) {
					failedEntitlements.setSize(entitlements.numValues());
					for (int j = 0; j < numUsers; ++j) {
						failedEntitlements.clear();						
						if(d_identities.get(j).hasEntitlements(entitlements,service,failedEntitlements)){
							System.out.println("User #" + (j+1)
									+ " is entitled to get data for: " + ticker);
						}
						else {
							System.out.println("User #" + (j+1)
									+ " is NOT entitled to get data for: "
									+ ticker + " - Failed eids: ");
							printFailedEntitlements(failedEntitlements);
						}				
					}					
				}
				else {
					for (int j = 0; j < numUsers; ++j) {
						System.out.println( "User: " + d_tokens.get(j) 
								+ " is entitled to get data for: " 
								+ticker );
					}
				}
			}
		}

		private void processResponseEvent(Event event) throws IOException
		{
			MessageIterator msgIter = event.messageIterator();
			while (msgIter.hasNext()) {
				Message msg = msgIter.next();
				if (msg.hasElement(RESPONSE_ERROR)) {
					msg.print(System.out);
					continue;
				}
				// We have a valid response. Distribute it to all the users.
				distributeMessage(msg);
			}
		}

		public void processEvent(Event event, Session session) {
			switch(event.eventType().intValue()){
			case Event.EventType.Constants.SESSION_STATUS:
			case Event.EventType.Constants.SERVICE_STATUS:
			case Event.EventType.Constants.REQUEST_STATUS:
			case Event.EventType.Constants.AUTHORIZATION_STATUS:
			case Event.EventType.Constants.SUBSCRIPTION_STATUS:
				try {
					printEvent(event);
				} catch (IOException e1) {
					System.err.println("IO Exception!!! " + e1.getMessage());
				}
				break;
			case Event.EventType.Constants.RESPONSE:
			case Event.EventType.Constants.PARTIAL_RESPONSE:
				try {
					processResponseEvent(event);
				}
				catch (Exception e) {
					System.err.println("Library Exception!!! " + e.getMessage());
					return;
				}
				break;
			}
			return;
		}
	}
}
