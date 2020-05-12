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
 MktBarSubscriptionWithEvents.cs: 
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

using Event = Bloomberglp.Blpapi.Event;
using EventHandler = Bloomberglp.Blpapi.EventHandler;
using Element = Bloomberglp.Blpapi.Element;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using Subscription = Bloomberglp.Blpapi.Subscription;
using CorrelationID = Bloomberglp.Blpapi.CorrelationID;
using Datetime = Bloomberglp.Blpapi.Datetime;

using System;
using System.Collections.Generic;

public class MktBarSubscriptionWithEvents
{
    private static Name TIME = new Name("TIME");
    private static Name OPEN = new Name("OPEN");
    private static Name HIGH = new Name("HIGH");
    private static Name LOW = new Name("LOW");
    private static Name CLOSE = new Name("CLOSE");
    private static Name NUMBER_OF_TICKS = new Name("NUMBER_OF_TICKS");
    private static Name VOLUME = new Name("VOLUME");
    private static Name EXCEPTIONS = new Name("exceptions");
    private static Name FIELD_ID = new Name("fieldId");
    private static Name REASON = new Name("reason");
    private static Name CATEGORY = new Name("category");
    private static Name DESCRIPTION = new Name("description");

    private string d_host;
    private int d_port;
    private SessionOptions d_sessionOptions;
    private Session d_session;
    private List<string> d_securities;
    private List<string> d_fields;
    private List<string> d_options;
    private List<Subscription> d_subscriptions;

    public static void Main(string[] args)
    {
        System.Console.WriteLine("Realtime Event Handler Example");
        MktBarSubscriptionWithEvents example = new MktBarSubscriptionWithEvents();
        example.run(args);
    }

    public MktBarSubscriptionWithEvents()
    {
        d_sessionOptions = new SessionOptions();
        d_host = "localhost";
        d_port = 8194;

        d_sessionOptions = new SessionOptions();
        d_securities = new List<string>();
        d_fields = new List<string>();
        d_options = new List<string>();
        d_subscriptions = new List<Subscription>();
    }

    /*****************************************************************************
    Function    : createSession
    Description : This function creates a session object and opens the market 
                    bar service.  Returns false on failure of either.
    Arguments   : none
    Returns     : bool
    *****************************************************************************/
    private bool createSession()
    {
        if (d_session != null) d_session.Stop();

        System.Console.WriteLine("Connecting to " + d_sessionOptions.ServerHost +
                           ":" + d_sessionOptions.ServerPort);
        d_session = new Session(d_sessionOptions, new EventHandler(processEvent));
        if (!d_session.Start())
        {
            System.Console.WriteLine("Failed to start session");
            return false;
        }
        System.Console.WriteLine("Connected successfully\n");

        if (!d_session.OpenService("//blp/mktbar"))
        {
            System.Console.WriteLine("Failed to open service //blp/mktbar");
            d_session.Stop();
            return false;
        }

        System.Console.WriteLine("Subscribing...");
        d_session.Subscribe(d_subscriptions);

        return true;
    }

    /*****************************************************************************
    Function    : parseCommandLine
    Description : This function parses input arguments and/or sets default arguments
                    Only returns false on -h.
    Arguments   : string array
    Returns     : bool
    *****************************************************************************/
    private bool parseCommandLine(string[] args)
    {
        for (int i = 0; i < args.Length; ++i)
        {
            if (string.Compare(args[i], "-s", true) == 0
				&& i + 1 < args.Length)
            {
                d_securities.Add(args[++i]);
            }
            else if (string.Compare(args[i], "-f", true) == 0
				&& i + 1 < args.Length)
            {
                d_fields.Add(args[++i]);
            }
            else if (string.Compare(args[i], "-o", true) == 0
				&& i + 1 < args.Length)
            {
                d_options.Add(args[++i]);
            }
            else if (string.Compare(args[i], "-ip", true) == 0
				&& i + 1 < args.Length)
            {
                d_host = args[++i];
            }
            else if (string.Compare(args[i], "-p", true) == 0
				&& i + 1 < args.Length)
            {
                d_port = int.Parse(args[++i]);
            }
            if (string.Compare(args[i], "-h", true) == 0)
            {
                printUsage();
                return false;
            }
        }

        if (d_fields.Count == 0)
        {
            d_fields.Add("LAST_PRICE");
        }

        if (d_securities.Count == 0)
        {
            d_securities.Add("//blp/mktbar/ticker/IBM US Equity");
            d_securities.Add("//blp/mktbar/ticker/VOD LN Equity");
        }

        if (d_options.Count == 0)
        {
            string start_time_str = "start_time=" + DateTime.UtcNow.AddMinutes(0).ToString("HH:mm");
            string end_time_str = "end_time=" + DateTime.UtcNow.AddMinutes(30).ToString("HH:mm");

            System.Console.WriteLine(start_time_str);

            d_options.Add("bar_size=1");
            d_options.Add(start_time_str);
            d_options.Add(end_time_str);

        }

        foreach (string security in d_securities)
            d_subscriptions.Add(new Subscription(
                security, d_fields, d_options, new CorrelationID(security)));

        return true;
    }

    /*****************************************************************************
    Function    : printUsage
    Description : This function prints instructions for use to the console
    Arguments   : none
    Returns     : void
    *****************************************************************************/
    private void printUsage()
    {
        System.Console.WriteLine("Usage:");
        System.Console.WriteLine("	Retrieve realtime data");
        System.Console.WriteLine("		[-s			<security	= IBM US Equity>");
        System.Console.WriteLine("		[-f			<field		= LAST_PRICE>");
        System.Console.WriteLine("		[-o			<subscriptionOptions>");
        System.Console.WriteLine("		[-ip 		<ipAddress	= localhost>");
        System.Console.WriteLine("		[-p 		<tcpPort	= 8194>");
        System.Console.WriteLine("Press ENTER to quit");
    }

    /*****************************************************************************
    Function    : processSubscriptionStatus
    Description : Processes subscription status messages returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
    private void processSubscriptionStatus(Event eventObj, Session session)
    {
        System.Console.WriteLine("Processing SUBSCRIPTION_STATUS");
        foreach (Message msg in eventObj)
        {
            string topic = (string)msg.CorrelationID.Object;
            System.Console.WriteLine(System.DateTime.Now.ToString("s") +
                ": " + topic + " - " + msg.MessageType);

            if (msg.HasElement(REASON))
            {
                // This occurs if a bad security is subscribed to
                Element reason = msg.GetElement(REASON);
                System.Console.WriteLine("\t" +
                        reason.GetElement(CATEGORY).GetValueAsString() +
                        ": " + reason.GetElement(DESCRIPTION).GetValueAsString());
            }

            if (msg.HasElement(EXCEPTIONS))
            {
                // This can occur on SubscriptionStarted if an
                // invalid field is passed in
                Element exceptions = msg.GetElement(EXCEPTIONS);
                for (int i = 0; i < exceptions.NumValues; ++i)
                {
                    Element exInfo = exceptions.GetValueAsElement(i);
                    Element fieldId = exInfo.GetElement(FIELD_ID);
                    Element reason = exInfo.GetElement(REASON);
                    System.Console.WriteLine("\t" + fieldId.GetValueAsString() +
                            ": " + reason.GetElement(CATEGORY).GetValueAsString());
                }
            }
            System.Console.WriteLine("");
        }
    }

    /*****************************************************************************
    Function    : processSubscriptionDataEvent
    Description : Processes all field data returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
    private void processSubscriptionDataEvent(Event eventObj, Session session)
    {

        System.Console.WriteLine("Processing SUBSCRIPTION_DATA");
        foreach (Message msg in eventObj)
        {
            string topic = (string)msg.CorrelationID.Object;
            System.Console.WriteLine(System.DateTime.Now.ToString("s")
                + ": " + topic + " - " + msg.MessageType);

            CheckAspectFields(msg);

        }
    }

    /*****************************************************************************
    Function    : processMiscEvents
    Description : Processes any message returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
    private void processMiscEvents(Event eventObj, Session session)
    {
        System.Console.WriteLine("Processing " + eventObj.Type);
        foreach (Message msg in eventObj)
        {
            System.Console.WriteLine(System.DateTime.Now.ToString("s") +
                ": " + msg.MessageType + "\n");
        }
    }

    /*****************************************************************************
    Function    : processEvent
    Description : Processes session events
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
    public void processEvent(Event eventObj, Session session)
    {
        try
        {
            switch (eventObj.Type)
            {
                case Event.EventType.SUBSCRIPTION_DATA:
                    processSubscriptionDataEvent(eventObj, session);
                    break;
                case Event.EventType.SUBSCRIPTION_STATUS:
                    processSubscriptionStatus(eventObj, session);
                    break;
                default:
                    processMiscEvents(eventObj, session);
                    break;
            }
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine(e.ToString());
        }
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
		if(msg.HasElement(TIME))
		{
			Datetime time = msg.GetElementAsDatetime(TIME);
			String time_str = msg.GetElementAsString(TIME);
			System.Console.WriteLine("Time : " + time_str + "\n");
		}
		if(msg.HasElement(OPEN))
		{
			int open = msg.GetElementAsInt32(OPEN);
			String open_str = msg.GetElementAsString(OPEN);
			System.Console.WriteLine("Open : " + open_str + "\n");
		}
		if(msg.HasElement(HIGH))
		{
			int high = msg.GetElementAsInt32(HIGH);
			String high_str = msg.GetElementAsString(HIGH);
			System.Console.WriteLine("High : " + high_str + "\n");
		}
		if(msg.HasElement(LOW))
		{
			int low = msg.GetElementAsInt32(LOW);
			String low_str = msg.GetElementAsString(LOW);
			System.Console.WriteLine("Low : " + low_str + "\n");
		}
		if(msg.HasElement(CLOSE))
		{
			int close = msg.GetElementAsInt32(CLOSE);
			String close_str = msg.GetElementAsString(CLOSE);
			System.Console.WriteLine("Close : " + close_str + "\n");
		}
		if(msg.HasElement(NUMBER_OF_TICKS))
		{
			int number_of_ticks = msg.GetElementAsInt32(NUMBER_OF_TICKS);
			String number_of_ticks_str = msg.GetElementAsString(NUMBER_OF_TICKS);
			System.Console.WriteLine("Number of Ticks : " + number_of_ticks_str + "\n");
		}
		if(msg.HasElement(VOLUME))
		{
			float volume = msg.GetElementAsInt64(VOLUME);
			String volume_str = msg.GetElementAsString(VOLUME);
			System.Console.WriteLine("Volume : " + volume_str + "\n");
		}
		System.Console.WriteLine("\n");
	}

    /*****************************************************************************
    Function    : run
    Description : Performs the main functions of the program
    Arguments   : string array
    Returns     : void
    *****************************************************************************/
    private void run(string[] args)
    {
        try
        {
            if (!parseCommandLine(args)) return;
            if (!createSession()) return;

            // wait for enter key to exit application
            System.Console.Read();

            d_session.Stop();
            System.Console.WriteLine("Exiting.");
        }
        catch
        {
        }
    }
}