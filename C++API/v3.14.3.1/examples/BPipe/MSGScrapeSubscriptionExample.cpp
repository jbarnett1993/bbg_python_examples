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
 MSGScrapeSubscriptionExample.cpp: 
    This program demonstrate how to make subscription to particular 
    security/ticker to get realtime msgscrape data. 
    It uses Market Data service(//blp/msgscrape) provided by API.
    It does following:
        1. Establishing a session which facilitates connection to the bloomberg 
           network.
        2. Initiating the Market Data Data Service(//blp/msgscrape) for realtime
           data.
        3. Creating and sending request to the session.
            - Creating subscription list
            - Add securities and fields to subscription list
            - Subscribe to realtime data
        4. Event Handling of the responses received.
 Usage: 
    MSGScrapeSubscriptionExample -help 
    MSGScrapeSubscriptionExample -?
       Print the usage for the program on the console

    MSGScrapeSubscriptionExample
       Run the program with default values. Prints the realtime msgscrape data 
       on the console for three default securities specfied
       Ticker - "MSGSCRP MSG1 Curncy"

    example usage:
    MSGScrapeSubscriptionExample
    MSGScrapeSubscriptionExample -ip localhost -p 8194 -o EID=44321

    Prints the response on the console of the command line requested data

******************************************************************************/

#include <blpapi_defs.h>
#include <blpapi_correlationid.h>
#include <blpapi_element.h>
#include <blpapi_event.h>
#include <blpapi_exception.h>
#include <blpapi_message.h>
#include <blpapi_session.h>
#include <blpapi_subscriptionlist.h>

#include <vector>
#include <iostream>
#include <string>
#include <time.h>
#include <map>

using namespace std;
using namespace BloombergLP;
using namespace blpapi;

namespace {
    const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
    const Name AUTHORIZATION_FAILURE("AuthorizationFailure");
    const Name TOKEN_SUCCESS("TokenGenerationSuccess");
    const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name TOKEN("token");
    const string MSGSCRAPE_SVC = "//blp/msgscrape";
    const string AUTH_SVC = "//blp/apiauth";
}

class MSGScrapeSubscriptionExample
{
    vector<string>       d_hosts;
    int                  d_port;
	std::string          d_authOption;	// authentication option user/application
	std::string          d_name;	    // DirectoryService/ApplicationName
    vector<string>       d_securities;
    vector<string>       d_fields;
	vector<string>		 d_options;
	std::map<long, string> d_secruityLookup;	

private:

    /*****************************************************************************
    Function    : printUsage
    Description : This function prints the usage of the program on command line.
    Argument    : void
    Returns     : void
    *****************************************************************************/
    void printUsage()
    {
		cout << "Usage:" << std::endl
            << "    Subscribe to MSGScrape data using Bloomberg V3 API" << std::endl
            << "      [-s         <security   = \"MSGSCRP MSG1 Curncy\">" << std::endl
            << "      [-f         <field      = BID>" << std::endl
			<< "      [-o         <option     = EID=44321>" << std::endl
            << "      [-ip        <ipAddress = localhost>" << std::endl
            << "      [-p         <tcpPort   = 8194>" << std::endl
			<< "      [-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC>]" << std::endl
			<< "      [-n         <name = applicationName or directoryService>]" << std::endl
			<< "Notes:" << std::endl
			<< " -Specify only LOGON to authorize 'user' using Windows/unix login name." << std::endl
			<< " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << std::endl
			<< " -Specify APPLICATION and name(Application Name) to authorize application." << std::endl
			<< "Example: MSGScrapeSubscriptionExample ip <Host IP> -p <Host Port> -s \"MSGSCRP MSG1 Curncy\"" << std::endl
			<< "         MSGScrapeSubscriptionExample ip <Host IP> -p <Host Port> -s \"MSGSCRP MSG1 Curncy\" -o \"EID=44321\"" << std::endl;
    }

    /*****************************************************************************
    Function    : parseCommandLine
    Description : This function parses the command line arguments.If the command
                  line argument are not provided properly, it calls printUsage to 
                  print the usage on commandline. If no commnd line arguments are 
                  specified this fuction will set default values for 
                  security/fields/overrides
    Argument	: Command line parameters
    Returns		: bool: 
                  true, if successfully set the input argument for the request 
                  from command line or using default values otherwise false
    *****************************************************************************/
    bool parseCommandLine(int argc, char **argv)
    {
        if (argc == 2) {
            // print usage if user ask for help using following option
            if (!strcmp(argv[1], "-?") || !strcmp(argv[1], "/?") || 
                !strcmp(argv[1],"-help") || !strcmp(argv[1],"-h")) {
                printUsage();
                return false;
            }
        } 	

        for (int i = 1; i < argc; ++i) {
            if (!strcmp(argv[i],"-s") && i + 1 < argc) {
                d_securities.push_back(argv[++i]);
            } else if (!strcmp(argv[i],"-f") && i + 1 < argc) {
                d_fields.push_back(argv[++i]);
            } else if (!strcmp(argv[i],"-o") && i + 1 < argc) {
                d_options.push_back(argv[++i]); 
            } else if (!strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_hosts.push_back(argv[++i]);
            } else if (!strcmp(argv[i],"-p") &&  i + 1 < argc) {
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

		// check for appliation name
		if ((!std::strcmp(d_authOption.c_str(),"APPLICATION")) && (!std::strcmp(d_name.c_str(), ""))){
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
        if (d_hosts.size() == 0)
        {
			std::cout << "Missing host IP" << std::endl;
            printUsage();
            return false;
        }

        if (d_securities.size() == 0) {
            d_securities.push_back("MSGSCRP MSG1 Curncy");
        }

        if (d_fields.size() == 0) {
            // Subscribing to fields
            d_fields.push_back("ASK");
            d_fields.push_back("ASK_SIZE");
            d_fields.push_back("BID");
            d_fields.push_back("BID_SIZE");
			d_fields.push_back("SCRAPED_GROUP_NAME_RT");
        }

        return true;
    }

    /*****************************************************************************
    Function    : processTokenStatus
    Description : process token event status
    Argument    : subscriptionIdentity - pointer to Identity 
				  session - pointer to current session
    Returns     : true - token success, false - token failed
    *****************************************************************************/
    bool authorize(Identity * subscriptionIdentity, Session * session)
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

        session->openService(AUTH_SVC.c_str());
        Service authService = session->getService(AUTH_SVC.c_str());
        Request authRequest = authService.createAuthorizationRequest();
        authRequest.set(TOKEN, token.c_str());

        EventQueue authQueue;
        session->sendAuthorizationRequest(
            authRequest, subscriptionIdentity, CorrelationId(), &authQueue);

        while (true) {
            Event event = authQueue.nextEvent();
            if (event.eventType() == Event::RESPONSE ||
                event.eventType() == Event::REQUEST_STATUS ||
                event.eventType() == Event::PARTIAL_RESPONSE)
            {
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


    /*****************************************************************************
    Function    : getTimeStamp
    Description : Sets current local time to string buffer
    Argument    : buffer - Pointer to string buffer
                  bufSize - size of string buffer
    Returns     : size_t - no.of characters placed in the buffer
    *****************************************************************************/
    size_t getTimeStamp(char *buffer, size_t bufSize)
    {
        const char *format = "%Y-%m-%dT%X";

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
    Function    : eventLoop
    Description : This function waits for the session events and 
                  handles subscription data and subscription status events. 
                  This function reads update data messages in the event
                  element and prints them on the console.
    Argument    : reference to session object
    Returns     : void
    *****************************************************************************/
    void eventLoop(Session &session)
    {
        char timeBuffer[64];
        while (true) {
            Event event = session.nextEvent();
            MessageIterator msgIter(event);
            while (msgIter.next()) {
                Message msg = msgIter.message();
                if (event.eventType() == Event::SUBSCRIPTION_STATUS ||
                    event.eventType() == Event::SUBSCRIPTION_DATA) {
                    long securityKey = msg.correlationId().asInteger();
					map<long, string>::iterator security = d_secruityLookup.find(securityKey);
					getTimeStamp(timeBuffer, sizeof(timeBuffer));
					if (security == d_secruityLookup.end())
						cout << timeBuffer << ": " << securityKey << " - ";
					else
						cout << timeBuffer << ": " << security->second << " - ";
                }
                msg.print(cout) << endl;
            }
        }
    }

public:

    // Constructor
    MSGScrapeSubscriptionExample()
    {
        d_port = 8194;
		d_name = "";
    }

    // Destructor
    ~MSGScrapeSubscriptionExample()
    {
    }

    /*****************************************************************************
    Function    : run                                                                                     
    Description : This function runs the application to demonstrate how to make 
                  subscription to particular security/ticker to get realtime 
                  streaming updates. It does following:
                  1. Reads command line argumens.
                  2. Establishes a session which facilitates connection to the 
                      bloomberg network
                  3. Opens a msgscrape service with the session. 
                  4. create and send subscription request.
                  5. Event Loop and Response Handling.
    Arguments   : int argc, char **argv - Command line parameters.
    Returns     : void
    *****************************************************************************/
    void run(int argc, char **argv)
    {
        // read command line parameters
        if (!parseCommandLine(argc, argv)) return;

        SessionOptions sessionOptions;
        for (size_t i = 0; i < d_hosts.size(); ++i) { // override default 'localhost:8194'
            sessionOptions.setServerAddress(d_hosts[i].c_str(), d_port, i);
        }

        sessionOptions.setServerPort(d_port);
        sessionOptions.setAutoRestartOnDisconnection(true);
        sessionOptions.setNumStartAttempts(d_hosts.size());

		std::string authOptions;
		if (!std::strcmp(d_authOption.c_str(),"APPLICATION")) { //  Authenticate application
			// Set Application Authentication Option
			authOptions = "AuthenticationMode=APPLICATION_ONLY;";
			authOptions+= "ApplicationAuthenticationType=APPNAME_AND_KEY;";
			// ApplicationName is the entry in EMRS.
			authOptions+= "ApplicationName=" + d_name;
		} else {
			// Set User authentication option
			if (!strcmp(d_authOption.c_str(), "NONE")) {   			
				// No authentication
			} else if (!strcmp(d_authOption.c_str(), "DIRSVC")) {		
				// Authenticate user using active directory service property
				authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
				authOptions += "DirSvcPropertyName=" + d_name;
			} else {
				// Authenticate user using windows/unix login name
				authOptions = "AuthenticationType=OS_LOGON";
			}
		}

		std::cout << "Authentication Options = " << authOptions << std::endl;
		if (strcmp(d_authOption.c_str(), "NONE")) {   			
			// Add the authorization options to the sessionOptions
			sessionOptions.setAuthenticationOptions(authOptions.c_str());
		}

        // Set default subscription service as //blp/msgscrape instead of 
        // default //blp/mktdata in order to get realtime MSG Scrape data.
        sessionOptions.setDefaultSubscriptionService(MSGSCRAPE_SVC.c_str());

        std::cout << "Connecting to port " << d_port
                  << " on ";
        for (size_t i = 0; i < sessionOptions.numServerAddresses(); ++i) {
            unsigned short port;
            const char *host;
            sessionOptions.getServerAddress(&host, &port, i);
            std::cout << (i? ", ": "") << host;
        }
        std::cout << std::endl;

		Session session(sessionOptions);
        // Start a Session
        if (!session.start()) {
            cout << "Failed to start session." << endl;
            return;
        }

		Identity subscriptionIdentity;
		if (strcmp(d_authOption.c_str(), "NONE")) {
			// Authorization
			subscriptionIdentity = session.createIdentity();
			if (!d_authOption.empty()) {
				if (!authorize(&subscriptionIdentity, &session)) {
					return;
				}
			}
		}

        // Open service
        if (!session.openService(MSGSCRAPE_SVC.c_str())) {
			cerr <<"Failed to open " << MSGSCRAPE_SVC.c_str() << endl;
            return;
        }
		// Options
		int optCnt = d_options.size();
		for(int index=0; index < optCnt; index++)
		{
			cout << "Option " << index + 1 << ": " << d_options[index] << endl;
		}

        SubscriptionList subscriptions;
        int secCnt = d_securities.size();
		int correlationIdKey = 1;
        for(int i=0; i<secCnt; i++){
			if (optCnt == 0) { 
				d_secruityLookup.insert(make_pair(correlationIdKey, d_securities[i]));
				subscriptions.add((char *)d_securities[i].c_str(), d_fields, 
					d_options, CorrelationId(correlationIdKey++));
			} else {
				// subscribe to each EID
				for(int index=0; index < optCnt; index++)
				{
					if (d_options[index].find("EID=") == 0) {
						// has EID prefix, subscribe to EID
						vector<string> subscriptionOption;
						subscriptionOption.push_back(d_options[index]);
						subscriptions.add((char *)d_securities[i].c_str(), d_fields, 
							subscriptionOption, CorrelationId(correlationIdKey));
						cout << "Subscription to " << d_securities[i].c_str() << " with " << d_options[index] << std::endl;
						d_secruityLookup.insert(make_pair(correlationIdKey++, d_securities[i] + "(" + d_options[index] + ")"));
					}
				}
			}
        }

        // Make subscription to realtime streaming data
		if (!strcmp(d_authOption.c_str(), "NONE")) {
	        session.subscribe(subscriptions);
		} else {
			// subscribe with identity object
			session.subscribe(subscriptions, subscriptionIdentity);
		}
        
        // wait for events from session.
        eventLoop(session);
    }

};

/*********************************
Program entry point.
**********************************/
int main(int argc, char **argv)
{
    cout << "MSG Scrape Subscription Example" << endl;
    MSGScrapeSubscriptionExample example;
    try {
        example.run(argc, argv);
    } 
    catch (Exception &e) {
            cerr << "Library Exception!!! " << e.description() 
                      << endl;
    }

    // wait for enter key to exit application
    cout << "Press ENTER to quit" << endl;
    char dummy[2];
    cin.getline(dummy, 2);
    return 0;
}
