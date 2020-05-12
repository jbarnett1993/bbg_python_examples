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

import com.bloomberglp.blpapi.Datetime;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Event.EventType;
import com.bloomberglp.blpapi.EventQueue;
import com.bloomberglp.blpapi.CorrelationID;
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
import com.bloomberglp.blpapi.InvalidRequestException;

public class HistoryExample{
	private String REFDATA_SVC = "//blp/refdata";
	private String AUTH_SVC = "//blp/apiauth";

    private static final String SECURITY_DATA = "securityData";
    private static final String SECURITY_NAME = "security";
    private static final String DATE = "date";

    private static final String FIELD_ID = "fieldId";
    private static final String FIELD_DATA = "fieldData";
    private static final String RESPONSE_ERROR = "responseError";
    private static final String SECURITY_ERROR = "securityError";
    private static final String ERROR_MESSAGE = "message";
    private static final String FIELD_EXCEPTIONS = "fieldExceptions";
    private static final String ERROR_INFO = "errorInfo";
	private static final Name AUTHORIZATION_SUCCESS = Name.getName("AuthorizationSuccess");
	private static final Name TOKEN_SUCCESS = Name.getName("TokenGenerationSuccess");

	private ArrayList<String>       d_hosts;
    private int                     d_port;
    private String                  d_authOption;
    private String                  d_name;
    private Identity                d_identity;
    private Session           		d_session;
    private ArrayList<String>		d_securities;
    private ArrayList<String>		d_fields;
    private String			  		d_startDate;
    private String			  		d_endDate;

	public HistoryExample()
    {
    	d_hosts = new ArrayList<String>();
        d_port = 8194;
        d_authOption="";
		d_name="";
		d_session = null;

        d_securities = new ArrayList<String>();
        d_fields = new ArrayList<String>();
        d_startDate = "null";
        d_endDate = "null";
    }

    private void printUsage()
    {
        System.out.println("Usage:");
        System.out.println("	Retrieve History data ");
        System.out.println("		[-s			<security	= IBM US Equity>");
        System.out.println("		[-f			<field		= LAST_PRICE>");
        System.out.println("		[-sd		<startDateTime  = 20091026>");
        System.out.println("		[-ed		<endDateTime    = 20091030>");
        System.out.println("		[-ip 		<ipAddress	= localhost>");
        System.out.println("		[-p 		<tcpPort	= 8194>");
		System.out.println("        [-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]");
		System.out.println("        [-n         <name = applicationName or directoryService>]");
		System.out.println("Notes:");
		System.out.println(" -Specify only LOGON to authorize 'user' using Windows login name.");
		System.out.println(" -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out.println(" -Specify APPLICATION and name(Application Name) to authorize application.");
    }

    private boolean parseCommandLine(String[] args)
    {
        for (int i = 0; i < args.length; ++i)
        {
            if (args[i].equalsIgnoreCase("-s") && i + 1 < args.length)
            {
                d_securities.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-f") && i + 1 < args.length)
            {
                d_fields.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length)
            {
                d_hosts.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-p") && i + 1 < args.length)
            {
                d_port = Integer.parseInt(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-sd") && i + 1 < args.length)
            {
            	d_startDate = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-ed") && i + 1 < args.length)
            {
            	d_endDate = args[++i];
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

        // handle default arguments
        if (d_fields.size() == 0)
        {
            d_fields.add("PX_LAST");
        }

        if (d_securities.size() == 0)
        {
            d_securities.add("IBM US Equity");
        }

        if (d_startDate.equals("null"))
        {
        	d_startDate = "20091001";
        }

        if (d_endDate.equals("null"))
        {
        	d_endDate = "20091101";
        }

        return true;
    }

    public static void main(String[] args) throws Exception
    {
		HistoryExample example = new HistoryExample();
        example.run(args);
        System.out.println("Press ENTER to quit");
        System.in.read();
    }

    private Boolean Process_Exceptions(Message msg)
    {
	    Element securityData = msg.getElement(SECURITY_DATA);
	    Element field_exceptions = securityData.getElement(FIELD_EXCEPTIONS);

	    if (field_exceptions.numValues() > 0)
	    {
		    Element element = field_exceptions.getValueAsElement(0);
		    Element field_id = element.getElement(FIELD_ID);
		    Element error_info = element.getElement(ERROR_INFO);
		    Element error_message = error_info.getElement(ERROR_MESSAGE);
		    System.out.println(field_id);
		    System.out.println(error_message);
		    return true;
	    }
	    return false;
    }

    private Boolean Process_Errors(Message msg)
    {
	    Element securityData = msg.getElement(SECURITY_DATA);

	    if (securityData.hasElement(SECURITY_ERROR))
	    {
		    Element security_error = securityData.getElement(SECURITY_ERROR);
		    Element error_message = security_error.getElement(ERROR_MESSAGE);
		    System.out.println(error_message);
		    return true;
	    }
	    return false;
    }

    private void Process_Fields(Message msg)
    {
    	String delimiter = "\t";

	    Element securityData = msg.getElement(SECURITY_DATA);
	    Element fieldData = securityData.getElement(FIELD_DATA);

	    //Print out the date column header
	    System.out.print("DATE" + delimiter + delimiter);

	    //Print out the fields column headers
		for(int k = 0; k < d_fields.size(); k++)
		{
			System.out.print(d_fields.get(k).toString());
			System.out.print(delimiter);
		}
		System.out.println();

	    if (fieldData.numValues() > 0)
	    {
	    	int numValues = fieldData.numValues();
			try
			{
			int datatype;

			// Extract the field data dependent on the data type and print it to the screen
			for(int k = 0; k < numValues; k++)
			{
				Element element = fieldData.getValueAsElement(k);
				Datetime date = element.getElementAsDatetime(DATE);
				System.out.print(date.dayOfMonth() + "/" + date.month() + "/" + date.year() + delimiter);

			    for(int m = 0; m < d_fields.size(); m++)
			    {
			    	String fieldString = d_fields.get(m).toString().toUpperCase();

				    if(element.hasElement(fieldString))
				    {
				    	Element field_Element = element.getElement(fieldString);
				    	datatype = field_Element.datatype().intValue();

						  switch(datatype)
						  {
						  case Datatype.Constants.BOOL://Bool
							  {
								 Boolean field1;
								 field1 = element.getElementAsBool(fieldString);
								 System.out.print(field1.toString() + delimiter);
								 break;
							  }
						  case Datatype.Constants.CHAR://Char
							  {
								 char field1;
								 field1 = element.getElementAsChar(fieldString);
								 System.out.print(field1 + delimiter);
								 break;
							  }
						  case Datatype.Constants.INT32://Int32
							  {
								 int field1;
								 field1 = element.getElementAsInt32(fieldString);
								 System.out.print(field1 + delimiter);
								 break;
							  }
						  case Datatype.Constants.INT64://Int64
							  {
								 long field1;
								 field1 = element.getElementAsInt64(fieldString);
								 System.out.print(field1 + delimiter);
								 break;
							  }
						  case Datatype.Constants.FLOAT32://Float32
							  {
								 float field1;
								 field1 = element.getElementAsFloat32(fieldString);
								 System.out.print(field1 + delimiter);
								 break;
							  }
						  case  Datatype.Constants.FLOAT64://Float64
							  {
								 double field1;
								 field1 = element.getElementAsFloat64(fieldString);
								 System.out.print(field1 + delimiter);
								 break;
							  }
						  case Datatype.Constants.STRING://String
							  {
								 String field1;
								 field1 = element.getElementAsString(fieldString);
								 System.out.print(field1 + delimiter);
								 break;
							  }
						  case Datatype.Constants.DATE://Date
							  {
								 Datetime field1;
								 field1 = element.getElementAsDate(fieldString);
								 System.out.print(field1 + delimiter);
								 break;
							  }
						  case Datatype.Constants.TIME://Time
							  {
								 Datetime field1;
								 field1 = element.getElementAsTime(fieldString);
								 System.out.print(field1 + delimiter);
								 break;
							  }
						  case Datatype.Constants.DATETIME://Datetime
							  {
								 Datetime field1;
								 field1 = element.getElementAsDatetime(fieldString);
								 System.out.print(field1 + delimiter);
								 break;
							  }
						  default:
							  {
								  String field1 = "uninitialised";
								  field1 = element.getElementAsString(fieldString);
								  System.out.print(field1 + delimiter);
								  break;
							  }
						  }//end of switch
				    	}//end of if
			    	}//end of for
			        System.out.println();
				}//end of for
			}//end of try
			catch(Exception e)
			{
				System.out.println(e.toString());
			}
	    }//end of if
	}//end of method

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

    private void run(String[] args) throws Exception
    {
    	boolean done = false;
    	
    	if(!parseCommandLine(args))
    	{
    		return;
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
        if (!d_session.openService(REFDATA_SVC)) {
        	System.err.println("Failed to open " + REFDATA_SVC);
        	d_session.stop();
        	return;
        }

        try {
			Service refDataService = d_session.getService(REFDATA_SVC);
		    Request request = refDataService.createRequest("HistoricalDataRequest");
		
		    Element securities = request.getElement("securities");
		
		    // there should be only on security for this history request
		    securities.appendValue((String)d_securities.get(0).toString());
		
		    Element fields = request.getElement("fields");
		    for(int i = 0; i < d_fields.size(); i++)
		    {
		        fields.appendValue((String)d_fields.get(i).toString());
		    }
		
		    request.set("periodicitySelection", "DAILY");
		    request.set("startDate", d_startDate);
		    request.set("endDate", d_endDate);
		
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
        } catch (InvalidRequestException e) {
            e.printStackTrace();
        }

        while (!done)
        {
	        try
	        {
		        Event eventObj = d_session.nextEvent();
			    MessageIterator msgIter = eventObj.messageIterator();
			    while (msgIter.hasNext())
			    {
			    	Message msg = msgIter.next();
			    	// parse all the messages that come in and get each message element
			        // if a partial response has been received we need to wait around for more data
			        if (eventObj.eventType() != Event.EventType.RESPONSE &&
			        eventObj.eventType() != Event.EventType.PARTIAL_RESPONSE)
			        {
				        System.out.println(msg.toString());
			        	continue;
			        }
			        
	                if (msg.hasElement(RESPONSE_ERROR))
	                {
	                	System.out.println("REQUEST FAILED: " + msg.getElement(RESPONSE_ERROR));
	                    return;
	                }
			        
			        Element securityData = msg.getElement(SECURITY_DATA);
			        Element securityName = securityData.getElement(SECURITY_NAME);

			        System.out.println(securityName);

			        if(!Process_Errors(msg))
			        {
						Process_Exceptions(msg);
				        Process_Fields(msg);
			        }

			        // if the whole response has been received we're finished here
			        if (eventObj.eventType() == Event.EventType.RESPONSE)
			        {
			        	done = true;
			        	break;
			        }
			    }//end of while
	        }//end of try
	        catch (Exception ex)
	        {
	        System.out.println("Got Exception:" + ex);
	        }
        }//end of function

    }//end of class
}//end of namespace
