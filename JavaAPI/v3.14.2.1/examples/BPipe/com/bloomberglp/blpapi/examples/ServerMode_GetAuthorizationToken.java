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
 
// ServerMode_GetAuthorizationToken.java
//
// This program requests a 'token' and displays it on the console.
// Refer to the following examples that accept a 'token' on the command line
// and use it to authorize users:
//      EntitleVerificationTokenExample
//      EntitleVerificationSubscriptionTokenExample
//
// By default this program will generate a 'token' based on the current
// logged in user. The "-d" option can be used to specify a property to look up
// via active directory services. For example, "-d mail" would look up the 
// value for the property "mail" which could be the email address of the user.
//
// Workflow:
// * set options based on what information will be used to generate the 'token'
// * connect to server
// * call generateToken to request a 'token'
// * look for "TOKEN_STATUS" events for success or failure.
//
// Command line arguments:
// -ip <serverHostNameOrIp>
// -p  <serverPort>
// -d  <dirSvcProperty>
//

package com.bloomberglp.blpapi.examples;

import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Event.EventType;

public class ServerMode_GetAuthorizationToken {
	private static final Name TOKEN_SUCCESS = new Name("TokenGenerationSuccess");
	private static final Name TOKEN_FAILURE = new Name("TokenGenerationFailure");
	
	private String  d_host;
	private int     d_port;
	private String  d_authOption;
	private String  d_name;
	private Session d_session;
	
	public ServerMode_GetAuthorizationToken(){
		d_host="localhost";
		d_port=8194;
		d_authOption="LOGON";
		d_name="";
		d_session=null;
	}
	
	private void run(String[] args) throws Exception {
		if(!parseCommandLine(args)) return;
		
		SessionOptions sessionOptions = new SessionOptions();
		sessionOptions.setServerHost(d_host);
		sessionOptions.setServerPort(d_port);

		// default authentication to user OS logon
		String authOptions = "AuthenticationType=OS_LOGON";
		if(d_authOption.equals("APPLICATION")){
            // Set Application Authentication Option
            authOptions = "AuthenticationMode=APPLICATION_ONLY;";
            authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
            // ApplicationName is the entry in EMRS.
            authOptions += "ApplicationName=" + d_name;
		} else {
            // Set User authentication option
            if (d_authOption.equals("DIRSVC")) {
                // Authenticate user using active directory service property
                authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
                authOptions += "DirSvcPropertyName=" + d_name;
            }
		}
		
		System.out.println("authOptions = " + authOptions);
		sessionOptions.setAuthenticationOptions(authOptions);
		
		System.out.println("Connecting to " + d_host + ":" + d_port);
		
		d_session = new Session(sessionOptions);
		if (!d_session.start()) {
			System.err.println("Failed to start session."); 
			return;
	    }
		
		CorrelationID corrId = d_session.generateToken();
		
		while(true){
			Event event = d_session.nextEvent();
			event.eventType();
			if(event.eventType().equals(EventType.TOKEN_STATUS)) {
				processTokenStatus(event, corrId);
                break;
			}
			else {
				if (!processEvent(event)) {
                    break;
                }
			}
		}
	}
	
	private boolean processTokenStatus(Event event, CorrelationID expectedCorrelationId) {
		MessageIterator msgIter = event.messageIterator();
		while (msgIter.hasNext()) {
			Message msg = msgIter.next();
			if(!expectedCorrelationId.equals(msg.correlationID())){
				System.err.println("Received message for unknown correlationId: " +
						msg.correlationID());
				System.out.println(msg);
				continue;
			}
			
			if(msg.messageType().equals(TOKEN_SUCCESS)) {
				//handle token generation success
			}
			else if (msg.messageType().equals(TOKEN_FAILURE)) {
	            // handle token generation failure
	        }
			System.out.println(msg);
		}
		return true;
	}
	
    private boolean processEvent(Event event)
    {
        MessageIterator msgIter = event.messageIterator();
        while (msgIter.hasNext()) {
            Message msg = msgIter.next();
                System.out.println(msg);
        }
        return true;
    }
	
	private void printUsage() {
		System.out.println("Usage:");
		System.out.println("        [-ip 		<ipAddress	= localhost>" );
		System.out.println("        [-p 		<tcpPort	= 8194>");
		System.out.println("		[-auth      <authenticationOption = LOGON or APPLICATION or DIRSVC>]");
		System.out.println("        [-n         <name = applicationName or directoryService>]");
		System.out.println("Notes:");
		System.out.println(" -Specify only LOGON to authorize 'user' using Windows login name.");
		System.out.println(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println(" -Specify APPLICATION and name(Application Name) to authorize application.");
		System.out.println("Press ENTER to quit");
	}
	
	private boolean parseCommandLine(String[] args) {
		int len = args.length;
		for (int i=0; i<len; ++i) {
			if(args[i].equals("-ip") && i + 1 < len) d_host = args[++i];
			else if(args[i].equals("-p") && i + 1 < len ) d_port=Integer.parseInt(args[++i]);
			else if(args[i].equals("-auth") && i + 1 < len ) {
				d_authOption = args[++i];
			}
			else if(args[i].equals("-n") && i + 1 < len ) {
				d_name = args[++i];
			}
			else {
				printUsage();
				return false;
			}
		}
        // check for application name
        if (d_authOption.equals("APPLICATION") && (d_name == ""))
        {
        	System.out.println("Application name cannot be NULL for application authorization.");
            printUsage();
            return false;
        }
        // check for Directory Service name
        if (d_authOption.equals("DIRSVC") && (d_name == ""))
        {
        	System.out.println("Directory Service property name cannot be NULL for DIRSVC authorization.");
            printUsage();
            return false;
        }
		return true;
	}
	
	public static void main(String[] args) {
		System.out.println("GetAuthorizationToken");
		ServerMode_GetAuthorizationToken getToken = new ServerMode_GetAuthorizationToken();
		try{
			getToken.run(args);
			System.out.println("Press ENTER to quit");
	        System.in.read();
		}
		catch( Exception e) {
			System.out.println("Library Exception!!! " + e.getMessage());
		}
	}

}
