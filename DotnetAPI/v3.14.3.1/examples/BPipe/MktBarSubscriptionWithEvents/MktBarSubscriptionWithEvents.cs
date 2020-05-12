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
using EventQueue = Bloomberglp.Blpapi.EventQueue;
using Request = Bloomberglp.Blpapi.Request;
using Name = Bloomberglp.Blpapi.Name;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using Subscription = Bloomberglp.Blpapi.Subscription;
using CorrelationID = Bloomberglp.Blpapi.CorrelationID;
using Datetime = Bloomberglp.Blpapi.Datetime;
using Identity = Bloomberglp.Blpapi.Identity;
using SeatType = Bloomberglp.Blpapi.SeatType;

using System;
using System.Collections.Generic;

public class MktBarSubscriptionWithEvents
{
    private const string MKTBAR_SVC = "//blp/mktbar";
    private const string AUTH_SVC = "//blp/apiauth";

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
    private static Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
    private static Name AUTHORIZATION_FAILURE = Name.GetName("AuthorizationFailure");
    private static Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");
    private static Name TOKEN_FAILURE = Name.GetName("TokenGenerationFailure");


    private List<string> d_hosts;
    private int d_port;
    private string d_authOption;
    private string d_dsName;
    private string d_name;
    private string d_token;
    private Identity d_identity;
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
        d_hosts = new List<string>();
        d_port = 8194;

        d_sessionOptions = new SessionOptions();
        d_securities = new List<string>();
        d_fields = new List<string>();
        d_options = new List<string>();
        d_subscriptions = new List<Subscription>();
        d_authOption = string.Empty;
        d_name = string.Empty;
        d_dsName = string.Empty;

    }

    /*****************************************************************************
    Function    : createSession
    Description : This function creates a session object and opens the market 
                    bar service.  Returns false on failure.
    Arguments   : none
    Returns     : bool
    *****************************************************************************/
    private bool createSession()
    {
        if (d_session != null) d_session.Stop();

        string authOptions = string.Empty;

        d_sessionOptions = new SessionOptions();

        if (d_authOption == "APPLICATION")
        {
            // Set Application Authentication Option
            authOptions = "AuthenticationMode=APPLICATION_ONLY;";
            authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
            // ApplicationName is the entry in EMRS.
            authOptions += "ApplicationName=" + d_name;
        }
        else if (d_authOption == "USER_APP")
        {
            // Set User and Application Authentication Option
            authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
            authOptions += "AuthenticationType=OS_LOGON;";
            authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
            // ApplicationName is the entry in EMRS.
            authOptions += "ApplicationName=" + d_name;
        }
        else if (d_authOption == "USER_DS_APP")
        {
            // Set User and Application Authentication Option
            authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
            authOptions += "AuthenticationType=DIRECTORY_SERVICE;";
            authOptions += "DirSvcPropertyName=" + d_dsName + ";";
            authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
            // ApplicationName is the entry in EMRS.
            authOptions += "ApplicationName=" + d_name;
        }
        else if (d_authOption == "DIRSVC")
        {
            // Authenticate user using active directory service property
            authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
            authOptions += "DirSvcPropertyName=" + d_dsName;
        }
        else
        {
            // Authenticate user using windows/unix login name (default)
            authOptions = "AuthenticationType=OS_LOGON";
        }

        System.Console.WriteLine("Authentication Options = " + authOptions);
        d_sessionOptions.AuthenticationOptions = authOptions;

        SessionOptions.ServerAddress[] servers = new SessionOptions.ServerAddress[d_hosts.Count];
        int index = 0;
        System.Console.WriteLine("Connecting to port " + d_port.ToString() + " on host(s):");
        foreach (string host in d_hosts)
        {
            servers[index] = new SessionOptions.ServerAddress(host, d_port);
            System.Console.WriteLine(host);
            index++;
        }

        // auto restart on disconnect
        d_sessionOptions.ServerAddresses = servers;
        d_sessionOptions.AutoRestartOnDisconnection = true;
        d_sessionOptions.NumStartAttempts = d_hosts.Count;
        // change default subscription service
        d_sessionOptions.DefaultSubscriptionService = MKTBAR_SVC;

        // create session and start
        d_session = new Session(d_sessionOptions, processEvent);
        return d_session.Start();
    }

    /*****************************************************************************
    Function    : GenerateToken
    Description : This function generate token using SessionOptions.AuthenticationOptions
                  information.  Returns false on failure.
    Arguments   : reference token string
    Returns     : bool
    *****************************************************************************/
    private bool GenerateToken(out string token)
    {
        bool isTokenSuccess = false;
        bool isRunning = false;

        token = string.Empty;
        CorrelationID tokenReqId = new CorrelationID(99);
        EventQueue tokenEventQueue = new EventQueue();

        d_session.GenerateToken(tokenReqId, tokenEventQueue);

        while (!isRunning)
        {
            Event eventObj = tokenEventQueue.NextEvent();
            if (eventObj.Type == Event.EventType.TOKEN_STATUS)
            {
                System.Console.WriteLine("processTokenEvents");
                foreach (Message msg in eventObj)
                {
                    System.Console.WriteLine(msg.ToString());
                    if (msg.MessageType == TOKEN_SUCCESS)
                    {
                        token = msg.GetElementAsString("token");
                        isTokenSuccess = true;
                        isRunning = true;
                        break;
                    }
                    else if (msg.MessageType == TOKEN_FAILURE)
                    {
                        System.Console.WriteLine("Received : " + TOKEN_FAILURE.ToString());
                        isRunning = true;
                        break;
                    }
                    else
                    {
                        System.Console.WriteLine("Error while Token Generation");
                        isRunning = true;
                        break;
                    }
                }
            }
        }

        return isTokenSuccess;
    }

    /*****************************************************************************
    Function    : IsBPipeAuthorized
    Description : This function authorize the token. A valid Identiy will contain
                  the user/application credentials on authorization success. 
                  Returns false on failure.
    Arguments   : token string from GenerateToken(), reference to Identity
    Returns     : bool
    *****************************************************************************/
    private bool IsBPipeAuthorized(string token, out Identity identity)
    {
        bool isAuthorized = false;
        bool isRunning = true;
        identity = null;

        if (!d_session.OpenService(AUTH_SVC))
        {
            System.Console.Error.WriteLine("Failed to open " + AUTH_SVC);
            return (isAuthorized = false);

        }
        Service authService = d_session.GetService(AUTH_SVC);


        Request authRequest = authService.CreateAuthorizationRequest();

        authRequest.Set("token", token);
        identity = d_session.CreateIdentity();
        EventQueue authEventQueue = new EventQueue();

        d_session.SendAuthorizationRequest(authRequest, identity, authEventQueue, new CorrelationID(1));

        while (isRunning)
        {
            Event eventObj = authEventQueue.NextEvent();
            System.Console.WriteLine("processEvent");
            if (eventObj.Type == Event.EventType.RESPONSE || eventObj.Type == Event.EventType.REQUEST_STATUS)
            {
                foreach (Message msg in eventObj)
                {
                    if (msg.MessageType == AUTHORIZATION_SUCCESS)
                    {
                        System.Console.WriteLine("Authorization SUCCESS");

                        isAuthorized = true;
                        isRunning = false;
                        break;
                    }
                    else if (msg.MessageType == AUTHORIZATION_FAILURE)
                    {
                        System.Console.WriteLine("Authorization FAILED");
                        System.Console.WriteLine(msg);
                        isRunning = false;
                    }
                    else
                    {
                        System.Console.WriteLine(msg);
                    }
                }
            }
        }
        return isAuthorized;
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
                d_hosts.Add(args[++i]);
            }
            else if (string.Compare(args[i], "-p", true) == 0
				&& i + 1 < args.Length)
            {
                d_port = int.Parse(args[++i]);
            }
            else if (string.Compare(args[i], "-auth", true) == 0
               && i + 1 < args.Length)
            {
                d_authOption = args[++i].Trim();
            }
            else if (string.Compare(args[i], "-ds", true) == 0
                && i + 1 < args.Length)
            {
                d_dsName = args[++i].Trim();
            }
            else if (string.Compare(args[i], "-n", true) == 0
                && i + 1 < args.Length)
            {
                d_name = args[++i].Trim();
            }
            if (string.Compare(args[i], "-h", true) == 0)
            {
                printUsage();
                return false;
            }
        }

        // check for application name
        if ((d_authOption == "APPLICATION" || d_authOption == "USER_APP") && (d_name == ""))
        {
            System.Console.WriteLine("Application name cannot be NULL for application authorization.");
            printUsage();
            return false;
        }
        // check for directory service and application names
        if (d_authOption == "USER_DS_APP" && (d_name == "" || d_dsName == ""))
        {
            System.Console.WriteLine("Application or DS name cannot be NULL for application authorization.");
            printUsage();
            return false;
        }
        // check for Directory Service name
        if ((d_authOption == "DIRSVC") && (d_dsName == ""))
        {
            System.Console.WriteLine("Directory Service property name cannot be NULL for DIRSVC authorization.");
            printUsage();
            return false;
        }

        // handle default arguments
        if (d_hosts.Count == 0)
        {
            System.Console.WriteLine("Missing host IP");
            printUsage();
            return false;
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
        System.Console.WriteLine("		[-s         <security	= IBM US Equity>");
        System.Console.WriteLine("		[-f         <field		= LAST_PRICE>");
        System.Console.WriteLine("		[-o         <subscriptionOptions>");
        System.Console.WriteLine("		[-ip        <ipAddress	= localhost>");
        System.Console.WriteLine("		[-p         <tcpPort	= 8194>");
        System.Console.WriteLine("      [-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>]");
        System.Console.WriteLine("      [-n         <name = applicationName>]");
        System.Console.WriteLine("      [-ds        <name = directoryService>]");
        System.Console.WriteLine("Notes:");
        System.Console.WriteLine("1) Specify only LOGON to authorize 'user' using Windows login name.");
        System.Console.WriteLine("2) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
        System.Console.WriteLine("3) Specify APPLICATION and name(Application Name) to authorize application.\n");
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
			System.Console.WriteLine("Time : " + time_str);
		}
		if(msg.HasElement(OPEN))
		{
			double open = msg.GetElementAsFloat64(OPEN);
			String open_str = msg.GetElementAsString(OPEN);
			System.Console.WriteLine("Open : " + open_str);
		}
		if(msg.HasElement(HIGH))
		{
            double high = msg.GetElementAsFloat64(HIGH);
			String high_str = msg.GetElementAsString(HIGH);
			System.Console.WriteLine("High : " + high_str);
		}
		if(msg.HasElement(LOW))
		{
            double low = msg.GetElementAsFloat64(LOW);
			String low_str = msg.GetElementAsString(LOW);
			System.Console.WriteLine("Low : " + low_str);
		}
		if(msg.HasElement(CLOSE))
		{
            double close = msg.GetElementAsFloat64(CLOSE);
			String close_str = msg.GetElementAsString(CLOSE);
			System.Console.WriteLine("Close : " + close_str);
		}
		if(msg.HasElement(NUMBER_OF_TICKS))
		{
			int number_of_ticks = msg.GetElementAsInt32(NUMBER_OF_TICKS);
			String number_of_ticks_str = msg.GetElementAsString(NUMBER_OF_TICKS);
			System.Console.WriteLine("Number of Ticks : " + number_of_ticks_str);
		}
		if(msg.HasElement(VOLUME))
		{
			long volume = msg.GetElementAsInt64(VOLUME);
			String volume_str = msg.GetElementAsString(VOLUME);
			System.Console.WriteLine("Volume : " + volume_str);
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
            if (!createSession())
            {
                System.Console.WriteLine("Fail to open session");
                return;
            }

            // Authenticate user using Generate Token Request 
            if (!GenerateToken(out d_token)) return;

            //Authorization : pass Token into authorization request. Returns User/Application Identity containing entitlements info.
            if (!IsBPipeAuthorized(d_token, out d_identity)) return;

            // check if this is a BPS seat type before subscribing to Market Bar data
            if (d_identity.SeatType == SeatType.BPS)
            {
                // subscribe with Identity
                d_session.Subscribe(d_subscriptions, d_identity);

                // wait for enter key to exit application
                System.Console.Read();
            }

            d_session.Stop();
            System.Console.WriteLine("Exiting.");
        }
        catch(Exception ex)
        {
            System.Console.WriteLine("Exception: " + ex.Message );
        }
    }
}