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

import com.bloomberglp.blpapi.Datetime;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Schema.Datatype;

public class HistoryExample{


    private static final String SECURITY_DATA = "securityData";
    private static final String SECURITY_NAME = "security";
    private static final String DATE = "date";

    private static final String FIELD_ID = "fieldId";
    private static final String FIELD_DATA = "fieldData";
    private static final String SECURITY_ERROR = "securityError";
    private static final String ERROR_MESSAGE = "message";
    private static final String FIELD_EXCEPTIONS = "fieldExceptions";
    private static final String ERROR_INFO = "errorInfo";

    private SessionOptions    		d_sessionOptions;
    private Session           		d_session;
    private ArrayList<String>		d_securities;
    private ArrayList<String>		d_fields;
    private ArrayList<String>		d_options;
    private String			  		d_startDate;
    private String			  		d_endDate;

	public HistoryExample()
    {
        d_sessionOptions = new SessionOptions();
        d_sessionOptions.setServerHost("localhost");
        d_sessionOptions.setServerPort(8294);

        d_securities = new ArrayList<String>();
        d_fields = new ArrayList<String>();
        d_startDate = "null";
        d_endDate = "null";
        d_options = new ArrayList<String>();
    }

    private void printUsage()
    {
        System.out.println("Usage:");
        System.out.println("	Retrieve realtime data ");
        System.out.println("		[-s			<security	= IBM US Equity>");
        System.out.println("		[-f			<field		= LAST_PRICE>");
        System.out.println("		[-o			<subscriptionOptions>");
        System.out.println("		[-ip 		<ipAddress	= localhost>");
        System.out.println("		[-p 		<tcpPort	= 8194>");
        System.out.println("Press ENTER to quit");
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
            else if (args[i].equalsIgnoreCase("-o") && i + 1 < args.length)
            {
                d_options.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length)
            {
                d_sessionOptions.setServerHost(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-p") && i + 1 < args.length)
            {
                d_sessionOptions.setServerPort(Integer.parseInt(args[++i]));
            }
            else if (args[i].equalsIgnoreCase("-sd") && i + 1 < args.length)
            {
            	d_startDate = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-ed") && i + 1 < args.length)
            {
            	d_endDate = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-h"))
            {
                printUsage();
				return false;
            }
        }

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
		System.out.print("\r");

	    if (fieldData.numValues() > 0)
	    {
	    	int numValues = fieldData.numValues();
			try
			{
			int datatype;

			// Extract the field data dependant on the data type and print it to the screen
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
			    	System.out.print("\r");
				}//end of for
			}//end of try
			catch(Exception e)
			{
				System.out.println(e.toString());
			}
	    }//end of if
	}//end of method

    private void run(String[] args) throws Exception
    {
        String serverHost = "localhost";
        int serverPort = 8194;

    	if(!parseCommandLine(args))
    	{
    		return;
    	}

        SessionOptions sessionOptions = new SessionOptions();
        sessionOptions.setServerHost(serverHost);
        sessionOptions.setServerPort(serverPort);

        System.out.println("Connecting to " + serverHost + ":" + serverPort);
        Session session = new Session(sessionOptions);

        if (!session.start())
        {
            System.err.println("Failed to start session.");
            return;
        }
        if (!session.openService("//blp/refdata"))
        {
            System.err.println("Failed to open //blp/refdata");
            return;
        }
        Service refDataService = session.getService("//blp/refdata");
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

        System.out.println("Sending Request: " + request);
        session.sendRequest(request, null);

        while (true)
        {
	        try
	        {
	        Event eventObj = session.nextEvent();

		    MessageIterator msgIter = eventObj.messageIterator();

			    while (msgIter.hasNext())
			    {
			    	Message msg = msgIter.next();
			    	// parse all the messages that come in and get each message element
			        // if a partial response has been received we need to wait around for more data
			        if (eventObj.eventType() != Event.EventType.RESPONSE &&
			        eventObj.eventType() != Event.EventType.PARTIAL_RESPONSE)
			        {
			        	continue;
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
