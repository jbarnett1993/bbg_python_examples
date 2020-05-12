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
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.EventHandler;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Subscription;
import com.bloomberglp.blpapi.SubscriptionList;

public class PageDataExample
{
    private static final Name EXCEPTIONS = new Name("exceptions");
    private static final Name FIELD_ID = new Name("fieldId");
    private static final Name REASON = new Name("reason");
    private static final Name CATEGORY = new Name("category");
    private static final Name DESCRIPTION = new Name("description");
    private static final Name PAGEUPDATE = new Name("PageUpdate");
    private static final Name ROWUPDATE = new Name("rowUpdate");
    private static final Name NUMROWS = new Name("numRows");
    private static final Name NUMCOLS = new Name("numCols");
    private static final Name ROWNUM = new Name("rowNum");
    private static final Name SPANUPDATE = new Name("spanUpdate");
    private static final Name STARTCOL = new Name("startCol");
    private static final Name LENGTH = new Name("length");
    private static final Name TEXT = new Name("text");

    private SessionOptions    d_sessionOptions;
    private Session           d_session;
    private HashMap<String, ArrayList<StringBuilder>>	  d_topicTable;
    private ArrayList<String> d_topics;
    private SimpleDateFormat  d_dateFormat;
    private String            d_service;

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
        d_sessionOptions = new SessionOptions();
        d_sessionOptions.setServerHost("localhost");
        d_sessionOptions.setServerPort(8194);

        d_service = "//blp/mktdata";
        d_topicTable = new HashMap<String, ArrayList<StringBuilder>>();
        d_topics = new ArrayList<String>();
        d_dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss.SSS");
    }

    private boolean createSession() throws Exception
    {
        if (d_session != null) d_session.stop();

        System.out.println("Connecting to " + d_sessionOptions.getServerHost()
				+ ":" + d_sessionOptions.getServerPort());
		if (!"//blp/mktdata".equalsIgnoreCase(d_service)) {
			d_sessionOptions.setDefaultSubscriptionService(d_service);
		}
        d_session = new Session(d_sessionOptions, new SubscriptionEventHandler());
        if (!d_session.start()) {
            System.err.println("Failed to start session");
            return false;
        }
        System.out.println("Connected successfully\n");

        if (!d_session.openService(d_service)) {
            System.err.println("Failed to open service: " + d_service);
            d_session.stop();
            return false;
        }

        System.out.println("Subscribing...");
       
        subscribe();

        return true;
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
    	d_session.subscribe(subscriptions);
    }
    
    private void run(String[] args) throws Exception
    {
        if (!parseCommandLine(args)) return;
        if (!createSession()) return;

        // wait for enter key to exit application
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

        private void showUpdateedPage(String topic)
        {
        	ArrayList<StringBuilder> rowList = d_topicTable.get(topic);
        	for (StringBuilder str : rowList)
        	{
        		System.out.print(str.toString());
        	}
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
            if (args[i].equalsIgnoreCase("-t") && i + 1 < args.length) {
                d_topics.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length) {
                d_sessionOptions.setServerHost(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-p") && i + 1 < args.length) {
                d_sessionOptions.setServerPort(Integer.parseInt(args[++i]));
            }
            else if (args[i].equalsIgnoreCase("-h")) {
                printUsage();
                return false;
            }
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
        System.out.println("		[-t			<Topic	= 0708/012/0001>");
        System.out.println("		[			i.e.\"Broker ID/Category/Page Number\"");
        System.out.println("		[-ip 		<ipAddress	= localhost>");
        System.out.println("		[-p 		<tcpPort	= 8194>");
        System.out.println("e.g. PageDataExample -t \"0708/012/0001\" -ip localhost -p 8194");
        System.out.println("Press ENTER to quit");
    }
}
