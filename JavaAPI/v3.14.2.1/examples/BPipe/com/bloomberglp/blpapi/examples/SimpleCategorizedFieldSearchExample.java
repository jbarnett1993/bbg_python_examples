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
import java.io.IOException;

import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Event.EventType;
import com.bloomberglp.blpapi.EventQueue;
import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.Identity;
import com.bloomberglp.blpapi.SessionOptions.ServerAddress;

public class SimpleCategorizedFieldSearchExample {

    private static final String APIFLDS_SVC = "//blp/apiflds";
	private static final String AUTH_SVC = "//blp/apiauth";

    private static final int CAT_NAME_LEN   = 40;
    private static final int ID_LEN         = 13;
    private static final int MNEMONIC_LEN   = 36;
    private static final int DESC_LEN       = 40;
    private static final String PADDING     = 
        "                                            ";
    private static final Name FIELD_ID       = new Name("id");
    private static final Name FIELD_MNEMONIC = new Name("mnemonic");
    private static final Name FIELD_DATA     = new Name("fieldData");
    private static final Name FIELD_DESC     = new Name("description");
    private static final Name FIELD_INFO     = new Name("fieldInfo");
    private static final Name FIELD_ERROR    = new Name("fieldError");
    private static final Name FIELD_MSG      = new Name("message");
    private static final Name CATEGORY       = new Name("category");
    private static final Name CATEGORY_NAME  = new Name("categoryName");
    private static final Name CATEGORY_ID    = new Name("categoryId");
    private static final Name FIELD_SEARCH_ERROR = new Name("fieldSearchError");
	private static final Name AUTHORIZATION_SUCCESS = Name.getName("AuthorizationSuccess");
	private static final Name TOKEN_SUCCESS = Name.getName("TokenGenerationSuccess");

	private ArrayList<String>       d_hosts;
    private int                     d_port;
    private String                  d_authOption;
    private String                  d_name;
    private Identity                d_identity;
    private Session           		d_session;

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

    public static void main(String[] args) throws Exception
    {
        SimpleCategorizedFieldSearchExample example = 
            new SimpleCategorizedFieldSearchExample();
        example.run(args);
        
        System.out.println("Press ENTER to quit");
        System.in.read();
    }

    private void run(String[] args) throws Exception
    {
    	d_hosts = new ArrayList<String>();
        d_port = 8194;
        d_authOption="";
		d_name="";
		d_session = null;

        if (!parseCommandLine(args)) {
            return ;
        }

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
        if (!d_session.openService(APIFLDS_SVC)) {
        	System.err.println("Failed to open " + APIFLDS_SVC);
        	d_session.stop();
        	return;
        }

        Service fieldInfoService = d_session.getService(APIFLDS_SVC);
        Request request = fieldInfoService.createRequest(
        "CategorizedFieldSearchRequest");
        request.set ("searchSpec", "last price");
        Element exclude = request.getElement("exclude");
        exclude.setElement ("fieldType", "Static");
        request.set ("returnFieldDocumentation", false);

		if (d_authOption == null)
		{
            System.out.println("Sending Request: " + request);
			d_session.sendRequest(request, null);
		}
		else
		{
			// request data with identity object
            System.out.println("Sending Request with user's Identity: " + request);
			d_session.sendRequest(request, d_identity, null);
		}

        boolean bGotResponse = false;
        while (!bGotResponse) {
            try {
                Event event = d_session.nextEvent();
                MessageIterator msgIter = event.messageIterator();

                while (msgIter.hasNext()) {
                    Message msg = msgIter.next();

                    if (event.eventType() != Event.EventType.RESPONSE &&
                            event.eventType() != Event.EventType.PARTIAL_RESPONSE) {
                        continue;
                    }
                    if (msg.hasElement(FIELD_SEARCH_ERROR)) {
                        System.out.println(msg);
                        continue;
                    }
                    Element categories = msg.getElement(CATEGORY);
                    int numCategories = categories.numValues();

                    for (int catIdx=0; catIdx < numCategories; ++catIdx) {

                        Element category = categories.getValueAsElement(catIdx);
                        String Name = category.getElementAsString(
                                CATEGORY_NAME);
                        String Id = category.getElementAsString(CATEGORY_ID);

                        System.out.println ("\n  Category Name:" +
                                padString (Name, CAT_NAME_LEN) +
                                "\tId:" + Id) ;

                        Element fields = category.getElement(FIELD_DATA);
                        int numElements = fields.numValues();

                        printHeader();
                        for (int i=0; i < numElements; i++) {
                            printField (fields.getValueAsElement(i));
                        }
                    }
                    System.out.println();
                }
                if (event.eventType() == Event.EventType.RESPONSE) break;
            } 

            catch (Exception ex) {
                System.out.println ("Got Exception:" + ex);
            }
        }
    }

    private void printField (Element field)
    {
        String fldId, fldMnemonic, fldDesc;

        fldId       = field.getElementAsString(FIELD_ID);
        if (field.hasElement(FIELD_INFO)) {
            Element fldInfo     = field.getElement (FIELD_INFO) ;
            fldMnemonic = fldInfo.getElementAsString(FIELD_MNEMONIC);
            fldDesc     = fldInfo.getElementAsString(FIELD_DESC);

            System.out.println( padString(fldId, ID_LEN) +
                    padString (fldMnemonic, MNEMONIC_LEN) +
                    padString (fldDesc, DESC_LEN));
        }
        else {
            Element fldError = field.getElement(FIELD_ERROR) ;
            fldDesc = fldError.getElementAsString(FIELD_MSG) ;

            System.out.println ("\n ERROR: " + fldId + " - " + fldDesc) ;
        }
    }

    private void printHeader ()
    {
        System.out.println( padString("FIELD ID", ID_LEN) +
                padString("MNEMONIC", MNEMONIC_LEN) +
                padString("DESCRIPTION", DESC_LEN));
        System.out.println( padString("-----------", ID_LEN) +
                padString("-----------", MNEMONIC_LEN) +
                padString("-----------", DESC_LEN));
    }

    private static String padString(String str, int width)
    {
        if (str.length() >= width || str.length() >= PADDING.length() ) return str;
        else return str + PADDING.substring(0, width-str.length());
    }
    private boolean parseCommandLine(String[] args)
    {
        for (int i = 0; i < args.length; ++i)
        {
        	if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length)
            {
                d_hosts.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-p") && i + 1 < args.length)
            {
                d_port = Integer.parseInt(args[++i]);
            }
			else if(args[i].equalsIgnoreCase("-auth") && i + 1 < args.length) {
				d_authOption = args[++i];
			}
			else if(args[i].equalsIgnoreCase("-n") && i + 1 < args.length) {
				d_name = args[++i];
			}
            else if (args[i].equalsIgnoreCase("-h"))
            {
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
        return(true);
    }

    private void printUsage()
    {
        System.out.println("Usage:");
        System.out.println(
        	"	Retrieve field information in categorized form ");
        System.out.println("        [-ip        <ipAddress	= localhost>");
        System.out.println("        [-p         <tcpPort	= 8194>");
		System.out.println("        [-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]");
		System.out.println("        [-n         <name = applicationName or directoryService>]");
		System.out.println("Notes:");
		System.out.println(" -Specify only LOGON to authorize 'user' using Windows login name.");
		System.out.println(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println(" -Specify APPLICATION and name(Application Name) to authorize application.");
    }
}
