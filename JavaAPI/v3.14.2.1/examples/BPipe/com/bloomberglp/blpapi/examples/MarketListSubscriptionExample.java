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
import java.util.Date;
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
import com.bloomberglp.blpapi.SessionOptions.ServerAddress;

public class MarketListSubscriptionExample
{	
	private ArrayList<String> d_hosts;
	private int       d_port = 8194;
	private String    d_authOption="" ;
	private String    d_name="";
	private SubscriptionList  d_subscriptions; 
	private ArrayList<String> d_securities;
	private Session d_session = null;
	private static final Name TOKEN_SUCCESS = new Name("TokenGenerationSuccess");
	private static final Name TOKEN_FAILURE = new Name("TokenGenerationFailure");
	private static final Name AUTHORIZATION_SUCCESS = new Name("AuthorizationSuccess");
	private static final Name TOKEN = new Name("token");
	private static final Name EXCEPTIONS = new Name("exceptions");
	private static final Name REASON = new Name("reason");
	private static final Name CATEGORY = new Name("category");
	private static final Name DESCRIPTION = new Name("description");
	private static final String authServiceName = "//blp/apiauth";
	private static final String mktListServiceName = "//blp/mktlist";

	public MarketListSubscriptionExample()
	{
		d_hosts = new ArrayList<String>();
		d_subscriptions = new SubscriptionList();
		d_securities = new ArrayList<String>();
	}

	public static void main(String[] args)
	{
		System.out.println("SimpleMktListExample");
		try {
			MarketListSubscriptionExample example = new MarketListSubscriptionExample();
			example.run(args);
			System.out.println("Press ENTER to quit");
			System.in.read();
		}
		catch (Exception e){
			System.err.println(e.getMessage());
			e.printStackTrace();
		}
	}

	private void run(String[] args) throws Exception
	{
		if (!parseCommandLine(args)) return;
		//create and open the session		
		createSession();

		if(d_authOption.equals("NONE")) {
			d_session.subscribe(d_subscriptions);
		}
		else {
			Identity identity = d_session.createIdentity();
			if (authorize(identity)) {
				// subscribe
				d_session.subscribe(d_subscriptions, identity);
			}
		}
	}

	private boolean authorize(Identity identity) throws Exception{
		EventQueue tokenEventQueue = new EventQueue();
		d_session.generateToken(new CorrelationID(), tokenEventQueue);
		
		String token = "";
		Event event = tokenEventQueue.nextEvent();

		if (event.eventType().equals(Event.EventType.TOKEN_STATUS)) {
			MessageIterator iter = event.messageIterator();
			while (iter.hasNext()) {

				Message msg = iter.next();
				msg.print(System.out);
				if (msg.messageType().equals(TOKEN_SUCCESS)) {
					token = msg.getElementAsString(TOKEN);
				}
				else if (msg.messageType().equals(TOKEN_FAILURE)) {
					break;
				}
			}
		}

		if (token.length() == 0) {
			System.out.println("Failed to get token");
			return false;
		}

		if (!d_session.openService(authServiceName)) {
			System.err.println("Failed to open " + authServiceName);
			return false;
		}

		Service authService = d_session.getService(authServiceName);
		Request authRequest = authService.createAuthorizationRequest();
		authRequest.set(TOKEN, token);

		EventQueue authQueue = new EventQueue();
		d_session.sendAuthorizationRequest(authRequest, identity, authQueue, new CorrelationID());		

		while (true) {
			event = authQueue.nextEvent();
			if (event.eventType().equals(Event.EventType.RESPONSE) ||
					event.eventType().equals(Event.EventType.REQUEST_STATUS) ||
					event.eventType().equals(Event.EventType.PARTIAL_RESPONSE)) {
				MessageIterator msgIter = event.messageIterator();
				while (msgIter.hasNext()) {
					Message msg = msgIter.next();
					msg.print(System.out);
					if (msg.messageType().equals(AUTHORIZATION_SUCCESS)) {
						System.out.println("Authorization successful" );
						return true;
					}
					else {
						System.out.println("Authorization failed" );
						return false;
					}
				}
			}
		}
	}

	private void createSession() throws Exception{ 
		String authOptions = "";
		SessionOptions sessionOptions = new SessionOptions();

		if (d_authOption.equals("APPLICATION")) { //  Authenticate application
			// Set Application Authentication Option
			authOptions = "AuthenticationMode=APPLICATION_ONLY;";
			authOptions+= "ApplicationAuthenticationType=APPNAME_AND_KEY;";
			// ApplicationName is the entry in EMRS.
			authOptions+= "ApplicationName=" + d_name;
		} else {
			// Set User authentication option
			if (d_authOption.equals("LOGON")) {   			
				// Authenticate user using windows/unix login name
				authOptions = "AuthenticationType=OS_LOGON";
			} else if (d_authOption.equals("DIRSVC")) {		
				// Authenticate user using active directory service property
				authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
				authOptions += "DirSvcPropertyName=" + d_name;
			} else {
				// default to no auth
				d_authOption = "NONE";
			}
		}

		System.out.println("Authentication Options = " + authOptions);

		// Add the authorization options to the sessionOptions
		if (!d_authOption.equals("NONE"))
		{
			sessionOptions.setAuthenticationOptions(authOptions);
		}
		sessionOptions.setAutoRestartOnDisconnection(true);
		sessionOptions.setNumStartAttempts(d_hosts.size());

		ServerAddress sAddress[] = new ServerAddress[d_hosts.size()];
		for (int i = 0; i < d_hosts.size(); ++i) { // override default 'localhost:8194'
			sAddress[i] = new ServerAddress(d_hosts.get(i), d_port);
		}
		sessionOptions.setServerAddresses(sAddress);

		System.out.print("Connecting to port " + d_port + " on ");

		for (int i=0; i<sAddress.length; i++) {
			ServerAddress address = sAddress[i];
			System.out.print((i>0 ? ", " :"") + address.host());
		}

		System.out.println();

		d_session = new Session(sessionOptions, new MktListEventHandler());
		boolean sessionStarted = d_session.start();
		if (!sessionStarted) {
			System.err.println("Failed to start session. Exiting..." );
			System.exit(-1);
		}
	}

	private boolean parseCommandLine(String[] args)
	{
		int argsLen = args.length;
		for (int i = 0; i < argsLen; ++i) {
			if (args[i].equalsIgnoreCase("-s") && i + 1 < argsLen) {
				d_securities.add(args[++i]);
			}
			else if (args[i].equalsIgnoreCase("-ip") && i + 1 < argsLen) {
				d_hosts.add(args[++i]);
			}
			else if (args[i].equalsIgnoreCase("-p") && i + 1 < argsLen) {
				d_port = Integer.parseInt(args[++i]);
			}
			else if (args[i].equalsIgnoreCase("-auth") && i + 1 < argsLen) {
				d_authOption = args[++i];
			}
			else if(args[i].equalsIgnoreCase("-n") && i + 1 < argsLen) {
				d_name = args[++i];
			}
			else if (args[i].equalsIgnoreCase("-h")) {
				printUsage();
				return false;
			}
		}

		// check for application name
		if ((d_authOption.equals("APPLICATION")) && (d_name.equals(""))){
			System.out.println("Application name cannot be NULL for application authorization.");
			printUsage();
			return false;
		}
		// check for Directory Service name
		if ((d_authOption.equals("DIRSVC")) && (d_name.equals(""))){
			System.out.println("Directory Service property name cannot be NULL for DIRSVC authorization.");
			printUsage();
			return false;
		}

		//default arguments
		if (d_hosts.size() == 0)
		{
			System.out.println("Missing host IP address.");
			printUsage();
			return false;
		}

		if (d_securities.size() == 0) {
			d_securities.add(mktListServiceName + "/chain/bsym/US/IBM");
		}

		for (String security: d_securities) {
			int index = security.indexOf("/");
			if (index != 0)
			{
				security = "/" + security;
			}

			index = security.indexOf("//");
			if (index != 0)
			{
				security = mktListServiceName + security;
			}
			// add subscription to subscription list         
			d_subscriptions.add(new Subscription(security, new CorrelationID(security)));	
		}

		return true;
	}

	private void printUsage()
	{
		System.out.println("Usage:");
		System.out.println("	SimpleMktListExample ");
		System.out.println("        [-s         <security   = //blp/mktlist/chain/bsym/US/IBM>");
		System.out.println("        [-ip        <ipAddress	= localhost>");
		System.out.println("        [-p         <tcpPort	= 8194>");
		System.out.println("        [-auth      <authenticationOption = NONE or LOGON or APPLICATION or DIRSVC>]" );
		System.out.println("        [-n         <name = applicationName or directoryService>]");
		System.out.println("Notes:");
		System.out.println(" -Specify only LOGON to authorize 'user' using Windows/unix login name.");
		System.out.println(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println(" -Specify APPLICATION and name(Application Name) to authorize application.");
	}

	private String getTimeStamp(){
		Date date = new Date();
		SimpleDateFormat sdf = new SimpleDateFormat("yyyyMMdd - hh:mm:ss");
		return sdf.format(date.getTime());
	}

	/* 
	 ************************************************************************************
	 * This class is used to implement Asynchronous request.
	 ***********************************************************************************
	 */
	class MktListEventHandler implements EventHandler
	{
		/* 
		 ***********************************************************************************
		 * This mehtod process asynchronous events
		 ***********************************************************************************
		 */
		public void processEvent(Event event, Session session)
		{
			try {
				switch (event.eventType().intValue())
				{                
				case Event.EventType.Constants.SUBSCRIPTION_DATA:
					processSubscriptionDataEvent(event);
					break;
				case Event.EventType.Constants.SUBSCRIPTION_STATUS:
					processSubscriptionStatus(event);
					break;
				default:
					processMiscEvents(event);
				}
			} catch (Exception e) {
				System.err.println("Library Exception !!!" + e.getMessage());
				e.printStackTrace();
			}
		}

		private void processSubscriptionDataEvent(Event event) {
			MessageIterator msgIter = event.messageIterator();
			System.out.println("Processing SUBSCRIPTION_DATA");
			while (msgIter.hasNext()) {
				Message msg = msgIter.next();
				//print out the raw text of the message
				System.out.println(msg.toString());
				String topic = msg.correlationID().toString();
				System.out.println("Fragment Type: " + msg.fragmentType().toString());
				System.out.println(getTimeStamp() + ": " + topic + " - " + msg.messageType().toString());

				int numFields = msg.asElement().numElements();
				for (int i = 0; i < numFields; ++i) {
					Element field = msg.asElement().getElement(i);
					if (field.numValues() < 1) {
						System.out.println("        " + field.name() + " is NULL");
						continue;
					}
				}
			}
		}

		private void processSubscriptionStatus(Event event) {
			MessageIterator msgIter = event.messageIterator();
			System.out.println("Processing SUBSCRIPTION_STATUS");
			while (msgIter.hasNext()) {
				Message msg = msgIter.next();

				String topic = msg.correlationID().toString();
				System.out.println(getTimeStamp() + ": " + topic + " - " + msg.messageType().toString());

				if (msg.hasElement(REASON)) {
					//This can occur on SubscriptionFailure.
					Element reason = msg.getElement(REASON);
					System.out.println("        " + 
							reason.getElement(CATEGORY).getValueAsString() + " " +
							reason.getElement(DESCRIPTION).getValueAsString());  
				}

				if (msg.hasElement(EXCEPTIONS)) {
					Element exceptions = msg.getElement(EXCEPTIONS);
					for (int i = 0; i < exceptions.numValues(); ++i) {
						Element exInfo = exceptions.getValueAsElement(i);
						Element reason = exInfo.getElement(REASON);      		
						System.out.println("        " + reason.getElement(CATEGORY).getValueAsString());	
					}
				}

			}
		}

		private void processMiscEvents(Event event) {
			MessageIterator msgIter = event.messageIterator();
			while (msgIter.hasNext()) {
				Message msg = msgIter.next();
				System.out.println(getTimeStamp() + ": " + msg.messageType().toString());
			}
		}		
	}
}

