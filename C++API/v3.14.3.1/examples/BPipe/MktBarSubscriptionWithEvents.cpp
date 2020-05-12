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
 MktBarSubscriptionWithEvents.cpp:
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

	MktBarWithEventHandlerExample
	   If you run the program with default values, program prints the streaming
	   updates on the console for two default securities specfied
	   1. Ticker - "//blp/mktbar/ticker/IBM US Equity"
	   2. Ticker - "//blp/mktbar/ticker/VOD LN Equity"
	   for field LAST_PRICE, bar_size=5, start_time=<local time + 2 minutes>,
                                end_time=<local_time+32 minutes>

    example usage:
	MktBarWithEventHandlerExample
	MktBarWithEventHandlerExample -ip localhost -p 8194
	MktBarWithEventHandlerExample -p 8194 -s "//blp/mktbar/ticker/VOD LN Equity"
                                        -s "//blp/mktbar/ticker/IBM US Equity"
									    -f "LAST_PRICE" -o "bar_size=5.0"
                                        -o "start_time=15:00" -o "end_time=15:30"

	Prints the response on the console of the command line requested data
******************************************************************************/

#include <blpapi_session.h>
#include <blpapi_eventdispatcher.h>

#include <blpapi_event.h>
#include <blpapi_message.h>
#include <blpapi_element.h>
#include <blpapi_name.h>
#include <blpapi_request.h>
#include <blpapi_subscriptionlist.h>
#include <blpapi_defs.h>
#include <blpapi_correlationid.h>

#include <vector>
#include <string>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <stdio.h>

using namespace BloombergLP;
using namespace blpapi;

namespace {
    Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
    Name AUTHORIZATION_FAILURE("AuthorizationFailure");
    Name TOKEN_SUCCESS("TokenGenerationSuccess");
    Name TOKEN_FAILURE("TokenGenerationFailure");
	Name TOKEN("token");

	Name EXCEPTIONS("exceptions");
    Name FIELD_ID("fieldId");
    Name REASON("reason");
    Name CATEGORY("category");
    Name DESCRIPTION("description");

	Name TIME("TIME");
	Name OPEN("OPEN");
	Name HIGH("HIGH");
	Name LOW("LOW");
	Name CLOSE("CLOSE");
	Name NUMBER_OF_TICKS("NUMBER_OF_TICKS");
	Name VOLUME("VOLUME");

	const char *MKTBAR_SVC = "//blp/mktbar";
	const char *AUTH_SVC = "//blp/apiauth";
}

class SubscriptionEventHandler: public EventHandler
{
    size_t getTimeStamp(char *buffer, size_t bufSize)
    {
        const char *format = "%Y/%m/%d %X";
        time_t now = time(0);
#ifdef WIN32
        tm *timeInfo = localtime(&now);
#else
        tm _timeInfo;
        tm *timeInfo = localtime_r(&now, &_timeInfo);
#endif
        return strftime(buffer, bufSize, format, timeInfo);
    }
    /*****************************************************************************
    Function    : processSubscriptionStatus
    Description : Processes subscription status messages returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
    bool processSubscriptionStatus(const Event &event)
    {
        char timeBuffer[64];
        getTimeStamp(timeBuffer, sizeof(timeBuffer));

        fprintf(stdout, "Processing SUBSCRIPTION_STATUS\n");
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            std::string *topic = reinterpret_cast<std::string*>(
                msg.correlationId().asPointer());
            fprintf(stdout, "%s: %s - %s\n",
                timeBuffer,
                topic->c_str(),
                msg.messageType().string());

            if (msg.hasElement(REASON)) {
                // This can occur on SubscriptionFailure.
                Element reason = msg.getElement(REASON);
				if (reason.hasElement(DESCRIPTION, true) && reason.hasElement(CATEGORY))
				{
					fprintf(stdout, "        %s: %s\n",
						reason.getElement(CATEGORY).getValueAsString(),
						reason.getElement(DESCRIPTION).getValueAsString());
				}
            }

            if (msg.hasElement(EXCEPTIONS)) {
                // This can occur on SubscriptionStarted if at least
                // one field is good while the rest are bad.
                Element exceptions = msg.getElement(EXCEPTIONS);
                for (size_t i = 0; i < exceptions.numValues(); ++i) {
                    Element exInfo = exceptions.getValueAsElement(i);
                    Element fieldId = exInfo.getElement(FIELD_ID);
                    Element reason = exInfo.getElement(REASON);
                    fprintf(stdout, "        %s: %s\n",
                            fieldId.getValueAsString(),
                            reason.getElement(CATEGORY).getValueAsString());
                }
            }
        }
        return true;
    }

	/*****************************************************************************
    Function    : processSubscriptionDataEvent
    Description : Processes all field data returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
    bool processSubscriptionDataEvent(const Event &event)
    {
        char timeBuffer[64];
        getTimeStamp(timeBuffer, sizeof(timeBuffer));

        fprintf(stdout, "\nProcessing SUBSCRIPTION_DATA\n");
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            std::string *topic = reinterpret_cast<std::string*>(
                msg.correlationId().asPointer());
			std::cout << topic->c_str()<< "\n";

			const char* msg_type = msg.messageType().string();

			// This event is received at the start of each security bar
			if(strcmp(msg_type,"MarketBarStart") == 0)
			{
				std::cout << msg_type << std::endl;
				CheckAspectFields(msg);
			}
			// Market Updates are sent throughout the bar every time a trade occurs
			if(strcmp(msg_type,"MarketBarUpdate") == 0)
			{
				std::cout << msg_type << std::endl;
				CheckAspectFields(msg);
			}
			// This event is only sent at the end of the security session
			if(strcmp(msg_type,"MarketBarEnd") == 0)
			{
				std::cout << msg_type << std::endl;
				CheckAspectFields(msg);
				fprintf(stdout, "Press ENTER to quit\n\n");
			}

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
	void CheckAspectFields(Message msg)
	{
		// extract data for each specific element
		// it's anticipated that an application will require this data
		// in the correct format.  this is retrieved to demonstrate
		// but is not used later in the code.
		if(msg.hasElement(TIME))
		{
			Datetime time = msg.getElementAsDatetime(TIME);
			const char *time_str = msg.getElementAsString(TIME);
			std::cout << "Time : " << time_str << "\n";
		}
		if(msg.hasElement(OPEN))
		{
			int open = msg.getElementAsInt32(OPEN);
			const char *open_str = msg.getElementAsString(OPEN);
			std::cout << "Open : " << open_str << "\n";
		}
		if(msg.hasElement(HIGH))
		{
			int high = msg.getElementAsInt32(HIGH);
			const char *high_str = msg.getElementAsString(HIGH);
			std::cout << "High : " << high_str << "\n";
		}
		if(msg.hasElement(LOW))
		{
			int low = msg.getElementAsInt32(LOW);
			const char *low_str = msg.getElementAsString(LOW);
			std::cout << "Low : " << low_str << "\n";
		}
		if(msg.hasElement(CLOSE))
		{
			int close = msg.getElementAsInt32(CLOSE);
			const char *close_str = msg.getElementAsString(CLOSE);
			std::cout << "Close : " << close_str << "\n";
		}
		if(msg.hasElement(NUMBER_OF_TICKS))
		{
			int number_of_ticks = msg.getElementAsInt32(NUMBER_OF_TICKS);
			const char *number_of_ticks_str = msg.getElementAsString(NUMBER_OF_TICKS);
			std::cout << "Number of Ticks : " << number_of_ticks_str << "\n";
		}
		if(msg.hasElement(VOLUME))
		{
			Int64 volume = msg.getElementAsInt64(VOLUME);
			const char *volume_str = msg.getElementAsString(VOLUME);
			std::cout << "Volume : " << volume_str << "\n";
		}
		std::cout << std::endl;
	}

	/*****************************************************************************
    Function    : processMiscEvents
    Description : Processes any message returned from Bloomberg
    Arguments   : Event, Session
    Returns     : void
    *****************************************************************************/
    bool processMiscEvents(const Event &event)
    {
        char timeBuffer[64];
        getTimeStamp(timeBuffer, sizeof(timeBuffer));

        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            fprintf(stdout, "%s: %s\n",
                timeBuffer, msg.messageType().string());
        }
        return true;
    }

public:
    SubscriptionEventHandler()
    {
    }

	/*****************************************************************************
	Function    : processEvent
	Description : Processes session events
	Arguments   : Event, Session
	Returns     : void
	*****************************************************************************/

    bool processEvent(const Event &event, Session *session)
    {
        try {
            switch (event.eventType())
            {            
			// Market Bars come back as subscription data events
            case Event::SUBSCRIPTION_DATA:
                return processSubscriptionDataEvent(event);
                break;
            case Event::SUBSCRIPTION_STATUS:
                return processSubscriptionStatus(event);
                break;
            default:
                return processMiscEvents(event);
                break;
            }
        } catch (Exception &e) {
            fprintf(stdout, "Library Exception !!! %s\n",
                e.description().c_str());
        }

        return false;
    }
};

class MktBarWithEventHandlerExample
{
	std::vector<std::string>				 d_hosts;		// IP Addresses of appliances
    int							 d_port;
	std::string					 d_authOption;	// authentication option user/application
	std::string					 d_name;	    // DirectoryService/ApplicationName
    Identity					 d_identity;
    SessionOptions 				 d_sessionOptions;
    Session          			*d_session;
    SubscriptionEventHandler    *d_eventHandler;
    std::vector<std::string>  	 d_securities;
    std::vector<std::string>  	 d_fields;
    std::vector<std::string>  	 d_options; 
    SubscriptionList             d_subscriptions; 

	/*****************************************************************************
    Function    : createSession
    Description : This function creates a session object and opens the market
                    bar service.  Returns false on failure of either.
    Arguments   : none
    Returns     : bool
    *****************************************************************************/
    bool createSession() { 
		std::cout << "Connecting to port " << d_port << " on server: "; 
        for (size_t i = 0; i < d_hosts.size(); ++i) {
            d_sessionOptions.setServerAddress(d_hosts[i].c_str(), d_port, i);
			std::cout << d_hosts[i].c_str();
        }
		std::cout << std::endl;
        d_sessionOptions.setServerPort(d_port);
        d_sessionOptions.setAutoRestartOnDisconnection(true);
        d_sessionOptions.setNumStartAttempts(d_hosts.size());
		
		std::string authOptions = getAuthOptions();
		if (authOptions.size() > 0) {
			d_sessionOptions.setAuthenticationOptions(authOptions.c_str());
		}
		d_eventHandler = new SubscriptionEventHandler();
        d_session = new Session(d_sessionOptions, d_eventHandler);

        if (!d_session->start()) {
            fprintf(stderr, "Failed to start session\n");
            return false;
        }

        fprintf(stdout, "Connected successfully\n");

        if (!d_session->openService(MKTBAR_SVC)) {
            fprintf(stderr, "Failed to open service %s",  MKTBAR_SVC);
            d_session->stop();
            return false;
        }

        return true;
    }   

	// construct authentication option string
	std::string getAuthOptions()
	{
		std::string authOptions;
		if (!std::strcmp(d_authOption.c_str(),"APPLICATION")) { //  Authenticate application
			// Set Application Authentication Option
			authOptions = "AuthenticationMode=APPLICATION_ONLY;";
			authOptions+= "ApplicationAuthenticationType=APPNAME_AND_KEY;";
			// ApplicationName is the entry in EMRS.
			authOptions+= "ApplicationName=" + d_name;
		} else if (!strcmp(d_authOption.c_str(), "NONE")) {
			// do nothing
		} else if (!strcmp(d_authOption.c_str(), "USER_APP")) {
            // Set User and Application Authentication Option
            authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
            authOptions += "AuthenticationType=OS_LOGON;";
            authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
            // ApplicationName is the entry in EMRS.
            authOptions += "ApplicationName=" + d_name;
		} else if (!strcmp(d_authOption.c_str(), "DIRSVC")) {		
				// Authenticate user using active directory service property
				authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
				authOptions += "DirSvcPropertyName=" + d_name;
		} else {
				// Authenticate user using windows/unix login name
				authOptions = "AuthenticationType=OS_LOGON";
		}

		std::cout << "Authentication Options = " << authOptions << std::endl;
		return authOptions;
	}

	bool authorize(Session * session)
    {
        EventQueue tokenEventQueue;
        session->generateToken(CorrelationId(), &tokenEventQueue);
        std::string token;
        Event event = tokenEventQueue.nextEvent();
        if (event.eventType() == Event::TOKEN_STATUS) {
            MessageIterator iter(event);
            while (iter.next()) {
                Message msg = iter.message();
                msg.print(std::cout);
                if (msg.messageType() == TOKEN_SUCCESS) {
                    token = msg.getElementAsString(TOKEN);
                }
                else if (msg.messageType() == TOKEN_FAILURE) {
                    break;
                }
            }
        }
        if (token.length() == 0) {
            std::cout << "Failed to get token" << std::endl;
            return false;
        }

        session->openService(AUTH_SVC);
		Service authService = session->getService(AUTH_SVC);
        Request authRequest = authService.createAuthorizationRequest();
        authRequest.set(TOKEN, token.c_str());

        EventQueue authQueue;
		d_identity = session->createIdentity();
        session->sendAuthorizationRequest(
            authRequest, &d_identity, CorrelationId(), &authQueue);

        while (true) {
            Event event = authQueue.nextEvent();
            if (event.eventType() == Event::RESPONSE ||
                event.eventType() == Event::REQUEST_STATUS ||
                event.eventType() == Event::PARTIAL_RESPONSE) {
                    MessageIterator msgIter(event);
                    while (msgIter.next()) {
                        Message msg = msgIter.message();
                        msg.print(std::cout);
                        if (msg.messageType() == AUTHORIZATION_SUCCESS) {
                            return true;
                        }
                        else {
                            std::cout << "Authorization failed" << std::endl;
                            return false;
                        }
                    }
            }
        }
    }

	// Ensures time is in HH:mm format
	char *padtime(int time_int, char* time_str)
	{
		if(time_int < 10)
		{
			const int buf_size = 2;
			const char temp1[buf_size] =  "0";
			char *temp2;
			temp2 = new (char[buf_size]);
			strcpy_s(temp2, buf_size, temp1);
			strcat_s(temp2, buf_size, time_str);
			strcpy_s(time_str, buf_size, temp2);
		}
		return time_str;
	}

	// Gets currect time from P.C.
	void GetMyTime(char** hour_str, char** min_str, int &hour, int &min, int add_secs)
	{
		time_t Time1 = time(0) + add_secs;
		struct tm gmt;
		gmtime_s(&gmt, &Time1);

		hour = gmt.tm_hour;
		min = gmt.tm_min;

		_itoa_s(hour,*hour_str, 3,10);
		_itoa_s(min,*min_str, 3, 10);
	}

	// Sets time as a string
	char *SetTimeString(char *set_str, int add_time)
	{
		int hour;
		int min;
		char *hour_str;
		const int time_buf_size = 3;
		hour_str = new (char[time_buf_size]);
		char *min_str;
		min_str = new (char[time_buf_size]);

		char *total_time_str;
		const int buf_size = 20;
		total_time_str = new (char[buf_size]);

		GetMyTime(&hour_str, &min_str, hour, min, add_time);

		hour_str = padtime(hour, hour_str);
		min_str = padtime(min, min_str);

		strcpy_s(total_time_str, buf_size, set_str);
		strcat_s(total_time_str, buf_size, hour_str);
		strcat_s(total_time_str, buf_size, ":");
		strcat_s(total_time_str, buf_size, min_str);
		
		std::cout << total_time_str << std::endl;

		delete []hour_str;
		delete []min_str;

		return total_time_str;
	}

	/*****************************************************************************
    Function    : parseCommandLine
    Description : This function parses input arguments and/or sets default arguments
                    Only returns false on -h.
    Arguments   : string array
    Returns     : bool
    *****************************************************************************/
    bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-s") && i + 1 < argc) {
                d_securities.push_back(argv[++i]);
            } else if (!std::strcmp(argv[i],"-f") && i + 1 < argc) {
                d_fields.push_back(argv[++i]);
            } else if (!std::strcmp(argv[i],"-o") && i + 1 < argc) {
                d_options.push_back(argv[++i]);
            } else if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_hosts.push_back(argv[++i]);
            } else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_port = atoi(argv[++i]);
			} else if (!std::strcmp(argv[i],"-auth") &&  i + 1 < argc) {
				d_authOption = argv[++i];
			} else if (!std::strcmp(argv[i],"-n") &&  i + 1 < argc) {
                d_name = argv[++i];
            } else {
                printUsage();
				return false;
            }
        }

		// check for hosts
        if (d_hosts.size() == 0) {
			 std::cout << "Missing host ip." << std::endl;
			 printUsage();
             return false;
        }

		// check for appliation name
		if ((!std::strcmp(d_authOption.c_str(),"APPLICATION") || !std::strcmp(d_authOption.c_str(), "USER_APP")) && (!std::strcmp(d_name.c_str(), ""))){
			 std::cout << "Application name cannot be NULL for application authorization." << std::endl;
			 printUsage();
             return false;
		}
		// check for Directory Service name
		if ((!std::strcmp(d_authOption.c_str(),"DIRSVC")) && (!std::strcmp(d_name.c_str(), ""))){
			 std::cout << "Directory Service property name cannot be NULL for DIRSVC authorization." << std::endl;
			 printUsage();
             return false;
		}

		// handle default arguments
        if (d_fields.size() == 0) {
            d_fields.push_back("LAST_PRICE");
        }

        if (d_securities.size() == 0) {
            d_securities.push_back("//blp/mktbar/ticker/VOD LN Equity");
            d_securities.push_back("//blp/mktbar/ticker/IBM US Equity");
        }

		if(d_options.size() == 0)
		{
			const int start_buf_size = 17;
			const int end_buf_size = 15;
			char *start_time_str;
			start_time_str = new (char[start_buf_size]);
			char *end_time_str;
			end_time_str = new (char[end_buf_size]);

			char* ret_start_time = new char[start_buf_size];
			ret_start_time = SetTimeString("start_time=",120);
			strcpy_s(start_time_str, start_buf_size, ret_start_time);
			char* ret_end_time = new char[end_buf_size];
			ret_end_time = SetTimeString("end_time=",1920);
			strcpy_s(end_time_str, end_buf_size, ret_end_time);

			d_options.push_back("bar_size=5");
			d_options.push_back(start_time_str);
			d_options.push_back(end_time_str);

			delete []start_time_str;
			delete []end_time_str;
			delete []ret_start_time;
			delete []ret_end_time;
		}

        for (size_t i = 0; i < d_securities.size(); ++i) {
            d_subscriptions.add(d_securities[i].c_str(), d_fields, d_options,
                                CorrelationId(&d_securities[i]));
        }

        return true;
    }
    /*****************************************************************************
    Function    : printUsage
    Description : This function prints instructions for use to the console
    Arguments   : none
    Returns     : void
    *****************************************************************************/
    void printUsage()
    {
        const char *usage = 
            "Usage:\n"
            "   Retrieve realtime data\n"
            "       [-s <security = //blp/mktbar/ticker/IBM US Equity>\n"
            "       [-f <field = LAST_PRICE>\n"
			"       [-o <\"bar_size=5\">\n"
            "		[-ip <ipAddress = localhost>\n"
            "		[-p <tcpPort = 8194>\n"
			"       [-auth  <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]\n"
			"       [-n     <name = applicationName or directoryService>]\n"
            "Notes:\n"
            " -Specify only LOGON to authorize 'user' using Windows login name.\n"
            " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.\n"
            " -Specify APPLICATION and name(Application Name) to authorize application.";
		fprintf(stdout, "%s\n", usage);
    }

public:

    MktBarWithEventHandlerExample()
    : d_session(0)
    , d_eventHandler(0)
    {
		d_port = 8194;
		d_name = "";
    }

    ~MktBarWithEventHandlerExample()
    {
        if (d_session) delete d_session;
		if (d_eventHandler) delete d_eventHandler ;
    }

	void wait_For_Exit()
	{
		// wait for enter key to exit application
        fprintf(stdout, "Press ENTER to quit\n\n");
        getchar();

        d_session->stop();
        fprintf(stdout, "Exiting...\n");
	}

	/*****************************************************************************
    Function    : run
    Description : Performs the main functions of the program
    Arguments   : string array
    Returns     : void
    *****************************************************************************/
    void run(int argc, char **argv)
    {
        if (!parseCommandLine(argc, argv)) return;
        if (!createSession()) return;

		if (strcmp(d_authOption.c_str(), "NONE")) {
			if (!authorize(d_session)) {
				return;
			}
			fprintf(stdout, "Subscribing with Identity...\n");
			d_session->subscribe(d_subscriptions, d_identity);
		} else {
			fprintf(stdout, "Subscribing...\n");
			d_session->subscribe(d_subscriptions);
		}

		wait_For_Exit();
    }
};

int main(int argc, char **argv)
{
    setvbuf(stdout, NULL, _IONBF, 0);
    fprintf(stdout, "MktBarWithEventHandlerExample\n");
    MktBarWithEventHandlerExample example;
    try {
        example.run(argc, argv);
    } catch (Exception &e) {
        fprintf(stderr, "Library Exception!!! %s\n",
            e.description().c_str());
    }
    return 0;
}