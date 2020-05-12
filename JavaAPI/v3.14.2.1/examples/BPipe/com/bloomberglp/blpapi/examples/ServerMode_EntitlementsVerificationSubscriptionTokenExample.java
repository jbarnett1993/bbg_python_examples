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
 
//ServerMode_EntitlementsVerificationSubscriptionTokenExample.java

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
//* subscribe to all specified 'securities' using application Identity
//* for each subscription data message, check which users are entitled to 
//receive that data before distributing that message to the user.

//Command line arguments:
//-ip <serverHostNameOrIp>
//-p  <serverPort>
//-a  <application name authentication>
//-t  <user's token>
//-s  <security>
//-f  <field>
//Multiple securities and tokens can be specified but the application
//is limited to one field.
//You can use the ServerMode_GetAuthorizationToken sample to generate the tokens.

package com.bloomberglp.blpapi.examples;

import java.io.IOException;
import java.util.Vector;

import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.EventHandler;
import com.bloomberglp.blpapi.EventQueue;
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

public class ServerMode_EntitlementsVerificationSubscriptionTokenExample {
	private String  d_host;
	private int     d_port;
	private String  d_field;
	private Vector<String> d_securities;
	private Vector<Identity> d_identities;
	private Vector<String> d_tokens;
	private SubscriptionList   d_subscriptions;
	private Session            d_session;
    private String 	 d_appName;
    private Identity d_appIdentity;
    private Service  d_apiAuthSvc;
    private String	 d_token;

    private final String AUTH_APP_PREFIX = "AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
	private final String APIAUTH_SVC = "//blp/apiauth";
	private final String MKTDATA_SVC = "//blp/mktdata";
	
	public static final Name EID = new Name("EID");
	public static final Name AUTHORIZATION_SUCCESS = new Name("AuthorizationSuccess");
    private static final Name AUTHORIZATION_FAILURE = new Name("AuthorizationFailure");
    private static final Name TOKEN_SUCCESS = new Name("TokenGenerationSuccess");
    private static final Name TOKEN_FAILURE = new Name("TokenGenerationFailure");

	public ServerMode_EntitlementsVerificationSubscriptionTokenExample(){
		d_session = null;
		d_host = "localhost";
		d_port = 8194;
		d_field = "BEST_BID1";
		d_appName = "";

		d_identities = new Vector<Identity>();
		d_tokens = new Vector<String>();
		d_securities = new Vector<String>();
		d_subscriptions = new SubscriptionList();
	}

	private void printUsage(){
		System.out.println("    Entitlements verification example");
		System.out.println("        [-s     <security   = MSFT US Equity>]");
		System.out.println("        [-f     <field  = BEST_BID1>]");
		System.out.println("        [-a		<application name authentication>]");
		System.out.println("        [-t     <user's token string>]");
		System.out.println("        ie. token value returned in generateToken response");
		System.out.println("        [-ip    <ipAddress  = localhost>]");
		System.out.println("        [-p     <tcpPort    = 8194>]");
		System.out.println("Note:");
		System.out.println("Multiple securities and tokens can be specified.");
		System.out.println("Only one field can be specified.");	
	}

	private boolean parseCommandLine(String[] args){
		int tokenCount = 0;
		int len = args.length;
		for (int i=0; i<len; ++i){
			if(args[i].equals("-s") && i + 1 < len) {
				d_securities.add(args[++i]);
			}
			else if(args[i].equals("-f") && i + 1 < len) {
				d_field = args[++i].toUpperCase(); //field should be in upper case
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

		for (int i=0; i<d_securities.size(); ++i){
			System.out.println(i);
			d_subscriptions.add(new Subscription(d_securities.get(i), d_field, "", new CorrelationID((long)i)));
		}
		return true;
	}

	private void createSession() throws IOException, InterruptedException{
		SessionOptions sessionOptions = new SessionOptions();
		sessionOptions.setServerHost(d_host);
		sessionOptions.setServerPort(d_port);
		sessionOptions.setAuthenticationOptions(AUTH_APP_PREFIX + d_appName.trim());

		System.out.println("Connecting to " + d_host + ":" + d_port);
		d_session = new Session(sessionOptions, 
				new SessionEventHandler(d_identities, d_tokens, d_securities, d_field));		
		boolean sessionStarted;
		sessionStarted = d_session.start();
		if (!sessionStarted) {
			System.err.println("Failed to start session. Exiting...");
			System.exit(-1);
		}
	}

	private void openServices() throws InterruptedException, IOException{
		if (!d_session.openService(APIAUTH_SVC)) {
			System.out.println("Failed to open service: " + APIAUTH_SVC);
			System.exit(-1);
		}
		if (!d_session.openService(MKTDATA_SVC)) {
			System.out.println("Failed to open service: " + MKTDATA_SVC);
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

        Service authService = d_session.getService(APIAUTH_SVC);


        Request authRequest = authService.createAuthorizationRequest();

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
                    }
                }
            }
        }
        return isAuthorized;
    }

	private boolean authorizeUsers() throws IOException, InterruptedException{
		boolean is_any_user_authorized = false;

		//Authorize each of the users
		d_identities = new Vector<Identity>(d_tokens.size());

		for(int i=0; i< d_tokens.size(); ++i){
			d_identities.add(d_session.createIdentity());
			Request authRequest = d_apiAuthSvc.createAuthorizationRequest();
			authRequest.set("token", d_tokens.get(i));

			CorrelationID correlator = new CorrelationID(d_tokens.get(i));      	
			EventQueue eventQueue = new EventQueue();
			d_session.sendAuthorizationRequest(authRequest, d_identities.get(i), eventQueue, correlator);

			Event event = eventQueue.nextEvent();
			if(event.eventType().equals(Event.EventType.RESPONSE) || 
					event.eventType().equals(Event.EventType.REQUEST_STATUS)) {
				MessageIterator msgIter = event.messageIterator();
				while(msgIter.hasNext()) {
					Message msg = msgIter.next();
					if(msg.messageType().equals(AUTHORIZATION_SUCCESS)) {
						System.out.println("User #" + (i+1) + " authorization success");
						is_any_user_authorized = true;
					}
					else {
						System.out.println("User #" + (i+1) + " authorization failed");
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

	public void run(String[] args) throws IOException{
		if (!parseCommandLine(args)) return;
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
						d_session.subscribe(d_subscriptions, d_appIdentity);
					} else {
						System.err.println("Unable to authorize users, Press Enter to Exit");
					}
                }
            }
			System.in.read();
			d_session.stop();
			System.out.println("Exiting.");
		}
		catch (Exception e) {
			e.printStackTrace();
		}
	}

	public static void main(String[] args) throws IOException {
		System.out.println("Entitlements Verification Subscription Token Example");	
		ServerMode_EntitlementsVerificationSubscriptionTokenExample example = new ServerMode_EntitlementsVerificationSubscriptionTokenExample();
		example.run(args);
	}


	class SessionEventHandler implements EventHandler{
		private Name                d_fieldName;

		public SessionEventHandler(Vector<Identity> identities, Vector <String>tokens, Vector <String>securities, String field){
			d_identities = identities;
			d_tokens = tokens;
			d_securities = securities;
			d_fieldName = new Name(field);
		}

		void processSubscriptionDataEvent(Event event)
	    {
			MessageIterator msgIter = event.messageIterator();
			while (msgIter.hasNext()){
				Message msg = msgIter.next();
				Service service = msg.service();
				
				int index = (int)msg.correlationID().value();
				String topic = d_securities.get(index);
				if (!msg.hasElement(d_fieldName, true)) {
	                continue;
	            }
				System.out.println("\t" + topic);
				Element field = msg.getElement(d_fieldName);
	            if (field.isNull()) {
	                continue;
	            }
	            boolean needsEntitlement = msg.hasElement(EID);
	            
	            for (int i=0; i<d_identities.size(); ++i) {
	            	Identity handle = d_identities.get(i);
	            	if(!needsEntitlement || handle.hasEntitlements(msg.getElement(EID),service)){
	            		System.out.println("User #" + (i+1) + " is entitled for " + field.toString());
	            	}
	            	else {
	            		System.out.println("User #" + (i+1) + " is NOT entitled for " + d_fieldName);
	            	}
	            }
			}
	    }
		
		public void processEvent(Event event, Session session) {
			try {
				switch (event.eventType().intValue())
				{  
				case Event.EventType.Constants.SESSION_STATUS:
				case Event.EventType.Constants.SERVICE_STATUS:
				case Event.EventType.Constants.REQUEST_STATUS:
				case Event.EventType.Constants.AUTHORIZATION_STATUS:
				case Event.EventType.Constants.SUBSCRIPTION_STATUS:
					printEvent(event);
					break;	
				case Event.EventType.Constants.SUBSCRIPTION_DATA:
					try {
						processSubscriptionDataEvent(event);
					} catch (Exception e) {
						System.err.println("Library Exception!!! " + e.getMessage());
					}
					break;
				}

			} catch (Exception e) {
				e.printStackTrace();
			}
		}
	}
}
