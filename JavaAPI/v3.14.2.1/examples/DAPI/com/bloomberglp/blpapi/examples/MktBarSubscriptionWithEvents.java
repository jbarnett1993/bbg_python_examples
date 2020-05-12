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

/*****************************************************************************
 MktBarSubscriptionWithEvents.java:
	This program demonstrates how to make a subscription to particular security/
	ticker to get realtime streaming updates at specified interval using
	"bar_size" options available. It uses the Market Bar service(//blp/mktbar)
	provided by API. Program does the following:
		1. Establishes a session which facilitates connection to the bloomberg
		   network.
		2. Initiates the Market Bar Service(//blp/mktbar) for realtime
		   data.
		3. Creates and sends the request via the session.
			- Creates a subscription list
			- Adds securities, fields and options to subscription list
			  Option specifies the bar_size duration for market bars, the start and end times.
			- Subscribes to realtime market bars
		4. Event Handling of the responses received.
        5. Parsing of the message data.
 Usage:
    MktBarSubscriptionWithEvents -h
	   Print the usage for the program on the console

	MktBarSubscriptionWithEvents
	   If you run the program with default values, program prints the streaming
	   updates on the console for two default securities specfied
	   1. Ticker - "//blp/mktbar/ticker/IBM US Equity"
	   2. Ticker - "//blp/mktbar/ticker/VOD LN Equity"
	   for field LAST_PRICE, bar_size=5, start_time=<local time + 2 minutes>,
                                end_time=<local_time+32 minutes>

    example usage:
	MktBarSubscriptionWithEvents
	MktBarSubscriptionWithEvents -ip localhost -p 8194
	MktBarSubscriptionWithEvents -p 8194 -s "//blp/mktbar/ticker/VOD LN Equity"
                                        -s "//blp/mktbar/ticker/IBM US Equity"
									    -f "LAST_PRICE" -o "bar_size=5.0"
                                        -o "start_time=15:00" -o "end_time=15:30"

	Prints the response on the console of the command line requested data

******************************************************************************/
package com.bloomberglp.blpapi.examples;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.TimeZone;
import java.text.DateFormat;

import javax.management.timer.Timer;

import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Datetime;
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

public class MktBarSubscriptionWithEvents
{
	private static final Name TIME = new Name("TIME");
	private static final Name OPEN = new Name("OPEN");
	private static final Name HIGH = new Name("HIGH");
	private static final Name LOW = new Name("LOW");
	private static final Name CLOSE = new Name("CLOSE");
	private static final Name NUMBER_OF_TICKS = new Name("NUMBER_OF_TICKS");
	private static final Name VOLUME = new Name("VOLUME");

    private static final Name EXCEPTIONS = new Name("exceptions");
    private static final Name FIELD_ID = new Name("fieldId");
    private static final Name REASON = new Name("reason");
    private static final Name CATEGORY = new Name("category");
    private static final Name DESCRIPTION = new Name("description");

    private SessionOptions    d_sessionOptions;
    private Session           d_session;
    private ArrayList         d_securities;
    private ArrayList         d_fields;
    private ArrayList         d_options;
    private SubscriptionList  d_subscriptions;
    private SimpleDateFormat  d_dateFormat;
    
    /**
     * @param args
     */
    public static void main(String[] args) throws java.lang.Exception
    {
        System.out.println("Realtime Market Bars with Event Handler Example");
        MktBarSubscriptionWithEvents example = new MktBarSubscriptionWithEvents();
        example.run(args);
    }

    public MktBarSubscriptionWithEvents()
    {
        d_sessionOptions = new SessionOptions();
        d_sessionOptions.setServerHost("localhost");
        d_sessionOptions.setServerPort(8194);

        d_securities = new ArrayList();
        d_fields = new ArrayList();
        d_options = new ArrayList();
        d_subscriptions = new SubscriptionList();
        d_dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss.SSS");
    }

    /*****************************************************************************
    Function    : createSession
    Description : This function creates a session object and opens the market
                    bar service.  Returns false on failure of either.
    Arguments   : none
    Returns     : bool
    *****************************************************************************/
    private boolean createSession() throws Exception
    {
        if (d_session != null) d_session.stop();

        System.out.println("Connecting to " + d_sessionOptions.getServerHost() +
                           ":" + d_sessionOptions.getServerPort());
        d_session = new Session(d_sessionOptions, new SubscriptionEventHandler());
        if (!d_session.start()) {
            System.err.println("Failed to start session");
            return false;
        }
        System.out.println("Connected successfully\n");

        if (!d_session.openService("//blp/mktbar")) {
            System.err.println("Failed to open service //blp/mktbar");
            d_session.stop();
            return false;
        }

        System.out.println("Subscribing...");
        d_session.subscribe(d_subscriptions);

        return true;
    }

    /*****************************************************************************
    Function    : run
    Description : Performs the main functions of the program
    Arguments   : string array
    Returns     : void
    *****************************************************************************/

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
	/*****************************************************************************
	Function    : processEvent
	Description : Processes session events
	Arguments   : Event, Session
	Returns     : void
	*****************************************************************************/
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

    /*****************************************************************************
    Function    : processSubscriptionStatus
    Description : Processes subscription status messages returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
        private boolean processSubscriptionStatus(Event event, Session session)
        throws Exception
        {
            System.out.println("Processing SUBSCRIPTION_STATUS");
            MessageIterator msgIter = event.messageIterator();
            while (msgIter.hasNext()) {
                Message msg = msgIter.next();
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

                System.out.println("");
            }
            return true;
        }

    /*****************************************************************************
    Function    : processSubscriptionDataEvent
    Description : Processes all field data returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
        private boolean processSubscriptionDataEvent(Event event, Session session)
        throws Exception
        {
            System.out.println("Processing SUBSCRIPTION_DATA");
            MessageIterator msgIter = event.messageIterator();
            while (msgIter.hasNext()) {
                Message msg = msgIter.next();
                String topic = (String) msg.correlationID().object();
                System.out.println(
                        d_dateFormat.format(Calendar.getInstance().getTime()) +
                        ": " + topic + " - " + msg.messageType());
                CheckAspectFields(msg);
            }
            return true;
        }

    /*****************************************************************************
    Function    : processMiscEvents
    Description : Processes any message returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
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

    /*****************************************************************************
    Function    : parseCommandLine
    Description : This function parses input arguments and/or sets default arguments
                    Only returns false on -h.
    Arguments   : string array
    Returns     : bool
    *****************************************************************************/
    private boolean parseCommandLine(String[] args)
    {
        for (int i = 0; i < args.length; ++i) {
            if (args[i].equalsIgnoreCase("-s") && i + 1 < args.length) {
                d_securities.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-f") && i + 1 < args.length) {
                d_fields.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-o") && i + 1 < args.length) {
                d_options.add(args[++i]);
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

        if (d_fields.size() == 0) {
            d_fields.add("LAST_PRICE");
        }

        if (d_securities.size() == 0) {
            d_securities.add("//blp/mktbar/ticker/IBM US Equity");
            d_securities.add("//blp/mktbar/ticker/VOD LN Equity");
        }

        if(d_options.size() == 0)
        {
        	String start_time_str = "start_time=";
        	String end_time_str = "end_time=";

        	DateFormat dateFormat = new SimpleDateFormat("HH:mm");

        	TimeZone zone = dateFormat.getTimeZone();
        	int minutesOffset = zone.getOffset(new Date().getTime());
        	String start_time = dateFormat.format(new Date().getTime() + (2L * Timer.ONE_MINUTE) - minutesOffset);
        	String end_time = dateFormat.format(new Date().getTime() + (32L * Timer.ONE_MINUTE) - minutesOffset);

        	start_time_str += start_time;
        	end_time_str += end_time;

        	d_options.add("bar_size=5");
        	d_options.add(start_time_str);
        	d_options.add(end_time_str);
        	System.out.print("Start time : " + start_time + "\n");
        	System.out.print("End time : " + end_time + "\n");
        	System.out.print("bar_size : 5\n");
        }

        for (int i = 0; i < d_securities.size(); ++i) {
            String security = (String)d_securities.get(i);
            d_subscriptions.add(new Subscription(security, d_fields, d_options,
                                new CorrelationID(security)));
        }

        return true;
    }

    /*****************************************************************************
    Function    : CheckAspectFields
    Description : Processes any field that can be contained within the market
                    bar message.
    Arguments   : Message
    Returns     : void
    *****************************************************************************/
	private void CheckAspectFields(Message msg)
	{
		// extract data for each specific element
		// it's anticipated that an application will require this data
		// in the correct format.  this is retrieved for demonstration
		// but is not used later in the code.
		if(msg.hasElement(TIME))
		{
			Datetime time = msg.getElementAsDatetime(TIME);
			String time_str = msg.getElementAsString(TIME);
			System.out.print("Time : " + time_str + "\n");
		}
		if(msg.hasElement(OPEN))
		{
			int open = msg.getElementAsInt32(OPEN);
			String open_str = msg.getElementAsString(OPEN);
			System.out.print("Open : " + open_str + "\n");
		}
		if(msg.hasElement(HIGH))
		{
			int high = msg.getElementAsInt32(HIGH);
			String high_str = msg.getElementAsString(HIGH);
			System.out.print("High : " + high_str + "\n");
		}
		if(msg.hasElement(LOW))
		{
			int low = msg.getElementAsInt32(LOW);
			String low_str = msg.getElementAsString(LOW);
			System.out.print("Low : " + low_str + "\n");
		}
		if(msg.hasElement(CLOSE))
		{
			int close = msg.getElementAsInt32(CLOSE);
			String close_str = msg.getElementAsString(CLOSE);
			System.out.print("Close : " + close_str + "\n");
		}
		if(msg.hasElement(NUMBER_OF_TICKS))
		{
			int number_of_ticks = msg.getElementAsInt32(NUMBER_OF_TICKS);
			String number_of_ticks_str = msg.getElementAsString(NUMBER_OF_TICKS);
			System.out.print("Number of Ticks : " + number_of_ticks_str + "\n");
		}
		if(msg.hasElement(VOLUME))
		{
			float volume = msg.getElementAsInt64(VOLUME);
			String volume_str = msg.getElementAsString(VOLUME);
			System.out.print("Volume : " + volume_str + "\n");
		}
		System.out.print("\n");
	}

    /*****************************************************************************
    Function    : printUsage
    Description : This function prints instructions for use to the console
    Arguments   : none
    Returns     : void
    *****************************************************************************/
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
}
