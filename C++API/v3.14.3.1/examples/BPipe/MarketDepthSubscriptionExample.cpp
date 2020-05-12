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
#include <iostream>

using namespace std;
using namespace BloombergLP;
using namespace blpapi;

namespace {
    Name EXCEPTIONS("exceptions");
    Name FIELD_ID("fieldId");
    Name REASON("reason");
    Name CATEGORY("category");
    Name DESCRIPTION("description");

	const Name TOKEN_SUCCESS("TokenGenerationSuccess");
	const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
	const Name TOKEN("token");

    const char* authServiceName = "//blp/apiauth";
	const std::string mktDepthServiceName = "//blp/mktdepthdata";
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

    bool processSubscriptionStatus(const Event &event)
    {
        char timeBuffer[64];
        getTimeStamp(timeBuffer, sizeof(timeBuffer));

		std::cout << "Processing SUBSCRIPTION_STATUS" << std::endl;
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            std::string *topic = reinterpret_cast<std::string*>(
                msg.correlationId().asPointer());
			std::cout << timeBuffer << ": " << topic->c_str() << " - " << msg.messageType().string() << std::endl;
            if (msg.hasElement(REASON)) {
                // This can occur on SubscriptionFailure.
				msg.print(std::cout);
            }

            if (msg.hasElement(EXCEPTIONS)) {
                // This can occur on SubscriptionStarted if at least
                // one field is good while the rest are bad.
                Element exceptions = msg.getElement(EXCEPTIONS);
                for (size_t i = 0; i < exceptions.numValues(); ++i) {
                    Element exInfo = exceptions.getValueAsElement(i);
                    Element fieldId = exInfo.getElement(FIELD_ID);
                    Element reason = exInfo.getElement(REASON);
					std::cout << "        " << fieldId.getValueAsString() << ": " 
						<< reason.getElement(CATEGORY).getValueAsString() << std::endl;
                }
            }
        }
        return true;
    }

    bool processSubscriptionDataEvent(const Event &event)
    {
        char timeBuffer[64];
        getTimeStamp(timeBuffer, sizeof(timeBuffer));
		
		std::cout << "Processing SUBSCRIPTION_DATA" << std::endl;
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
		    
			printFragType(msg.fragmentType());
            std::string *topic = reinterpret_cast<std::string*>(
                msg.correlationId().asPointer());
			std::cout << timeBuffer << ": " << topic->c_str() << " - " ;
			msg.print(std::cout);

        }
        return true;
    }

    bool processMiscEvents(const Event &event)
    {
        char timeBuffer[64];
        getTimeStamp(timeBuffer, sizeof(timeBuffer));

        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
			std::cout << timeBuffer << ": " << msg.messageType().string() << std::endl;
        }
        return true;
    }

	void printFragType(int type)
	{
	   if(type == BloombergLP::blpapi::Message::FRAGMENT_NONE)
	   {
		  std::cout<<"No Frags\n\n";
	   }
	   else if(type == BloombergLP::blpapi::Message::FRAGMENT_START)
	   { 
		  std::cout<<"Start Frag\n\n";
	   }
	   else if(type == BloombergLP::blpapi::Message::FRAGMENT_INTERMEDIATE)
	   {
		  std::cout<<"Intermediate Frag\n\n"; 
	   }
	   else if(type == BloombergLP::blpapi::Message::FRAGMENT_END)
	   {
		  std::cout<<"End Frag\n\n"; 
	   }
	   else
	   {
		  std::cout<<"Not Defined Frag\n\n";
	   }
	}

public:
    SubscriptionEventHandler()
    {
    }

    bool processEvent(const Event &event, Session *session)
    {
        try {
            switch (event.eventType())
            {                
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
			std::cout << "Library Exception !!! " << e.description().c_str() << std::endl;
        }

        return false;
    }
};

class MarketDepthSubscriptionExample
{
	std::vector<std::string>	 d_hosts;			// IP Addresses of the Managed B-Pipes
    int							 d_port;
	std::string					 d_authOption;		// authentication option user/application
	std::string					 d_name;	        // DirectoryService/ApplicationName
    SessionOptions               d_sessionOptions;
    Session                     *d_session;
    SubscriptionEventHandler    *d_eventHandler;
    std::vector<std::string>     d_securities;
	std::vector<std::string>     d_options;
    SubscriptionList             d_subscriptions; 

    void createSession() { 
		std::string authOptions;

        SessionOptions sessionOptions;
		if (!std::strcmp(d_authOption.c_str(),"APPLICATION")) { //  Authenticate application
			// Set Application Authentication Option
			authOptions = "AuthenticationMode=APPLICATION_ONLY;";
			authOptions+= "ApplicationAuthenticationType=APPNAME_AND_KEY;";
			// ApplicationName is the entry in EMRS.
			authOptions+= "ApplicationName=" + d_name;
		} else {
			// Set User authentication option
			if (!strcmp(d_authOption.c_str(), "LOGON")) {   			
				// Authenticate user using windows/unix login name
				authOptions = "AuthenticationType=OS_LOGON";
			} else if (!strcmp(d_authOption.c_str(), "DIRSVC")) {		
				// Authenticate user using active directory service property
				authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
				authOptions += "DirSvcPropertyName=" + d_name;
			} else {
				// default to no auth
				d_authOption = "NONE";
			}
		}

		std::cout << "Authentication Options = " << authOptions << std::endl;

		// Add the authorization options to the sessionOptions
		if (d_authOption != "NONE")
		{
			sessionOptions.setAuthenticationOptions(authOptions.c_str());
		}
        sessionOptions.setAutoRestartOnDisconnection(true);
        sessionOptions.setNumStartAttempts(d_hosts.size());

        for (size_t i = 0; i < d_hosts.size(); ++i) { // override default 'localhost:8194'
            sessionOptions.setServerAddress(d_hosts[i].c_str(), d_port, i);
        }

        std::cout << "Connecting to port " << d_port << " on ";

        for (size_t i = 0; i < sessionOptions.numServerAddresses(); ++i) {
            unsigned short port;
            const char *host;
            sessionOptions.getServerAddress(&host, &port, i);
            std::cout << (i? ", ": "") << host;
        }
        std::cout << std::endl;

		d_session = new Session(sessionOptions, new SubscriptionEventHandler());
        bool sessionStarted = d_session->start();
        if (!sessionStarted) {
            std::cerr << "Failed to start session. Exiting..." << std::endl;
            std::exit(-1);
        }
    }   

    bool parseCommandLine(int argc, char **argv)
    {
		std::string subscriptionOptions = "";
		std::string tmpOption;
		std::string tmpSecurity;

        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-s") && i + 1 < argc) {
                d_securities.push_back(argv[++i]);
                continue;
            } else if (!std::strcmp(argv[i],"-o") && i + 1 < argc) {
                d_options.push_back(argv[++i]);
                continue;
            } else if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_hosts.push_back(argv[++i]);
                continue;
			} else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_port = std::atoi(argv[++i]);
                continue;
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

		//default arguments
		if (d_hosts.size() == 0)
		{
			std::cout << "Missing host IP address." << std::endl;
			printUsage();
            return false;
		}

        if (d_securities.size() == 0) {
            d_securities.push_back(mktDepthServiceName + "/bsym/LN/VOD");
			d_securities.push_back(mktDepthServiceName + "/bsym/US/AAPL");
        }

		if (d_options.size() == 0) {
			// by order
			d_options.push_back("type=MBO");
		}

		for (size_t j = 0; j < d_options.size(); ++j) {
			tmpOption = d_options[j];
			if (subscriptionOptions.length() == 0)
			{
				subscriptionOptions = "?" + tmpOption;
			}
			else
			{
				subscriptionOptions = subscriptionOptions + "&" + tmpOption;
			}
		}

        for (size_t i = 0; i < d_securities.size(); ++i) {
			std::string security = d_securities[i];

			// add market depth service to security
			int index = security.find("/");
			if (index != 0)
			{
				security = "/" + security;
			}
			index = security.find("//");
			if (index != 0)
			{
				security = mktDepthServiceName + security;
			}
            // add subscription to subscription list
			tmpSecurity = security + subscriptionOptions;
            d_subscriptions.add(tmpSecurity.c_str(), CorrelationId(&d_securities[i]));
			std::cout << "Subscription string: " << d_subscriptions.topicStringAt(i) << std::endl;
        }

        return true;
    }

    bool authorize(Identity * identity)
    {
        EventQueue tokenEventQueue;
        d_session->generateToken(CorrelationId(), &tokenEventQueue);
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

		if (!d_session->openService(authServiceName)) {
			cerr <<"Failed to open " << authServiceName << endl;
			return false;
		}

        Service authService = d_session->getService(authServiceName);
        Request authRequest = authService.createAuthorizationRequest();
        authRequest.set(TOKEN, token.c_str());

        EventQueue authQueue;
        d_session->sendAuthorizationRequest(
            authRequest, identity, CorrelationId(), &authQueue);

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

    /*****************************************************************************
    Function    : printUsage
    Description : This function prints the usage of the program on command line.
    Argument    : void
    Returns     : void
    *****************************************************************************/
    void printUsage()
    {
		cout << "Usage:" << std::endl
            << "    Retrieve realtime market depth data using Bloomberg V3 API;" << std::endl
			<< std::endl
            << "      [-s    <security   = ""/bsym/LN/VOD"">" << std::endl
			<< "      [-o    <type=MBO, type=MBL, type=TOP or type=MMQ>" << std::endl
            << "      [-ip   <ipAddress  = localhost>" << std::endl
            << "      [-p    <tcpPort    = 8194>" << std::endl
			<< "      [-auth      <authenticationOption = NONE or LOGON or APPLICATION or DIRSVC>]" << std::endl
			<< "      [-n         <name = applicationName or directoryService>]" << std::endl
			<< "Notes:" << std::endl
			<< " -Specify only LOGON to authorize 'user' using Windows/unix login name." << std::endl
			<< " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << std::endl
			<< " -Specify APPLICATION and name(Application Name) to authorize application." << std::endl;
    }

public:

    MarketDepthSubscriptionExample()
    : d_session(0)
    , d_eventHandler(0)
    {
    }

    ~MarketDepthSubscriptionExample()
    {
        if (d_session) delete d_session;
        if (d_eventHandler) delete d_eventHandler ;
    }

    void run(int argc, char **argv)
    {
        if (!parseCommandLine(argc, argv)) return;
        
		createSession();

		if (d_authOption == "NONE")
		{
			d_session->subscribe(d_subscriptions);
		}
		else
		{
			// Authorize all the users that are interested in receiving data
			Identity identity = d_session->createIdentity();
			if (authorize(&identity)) {
				// subscribe
				d_session->subscribe(d_subscriptions, identity);
			}
		}

        // wait for enter key to exit application
		std::cout << "Press ENTER to quit" << std::endl << std::endl;
        getchar();

        d_session->stop();
		std::cout << "Exiting..." << std::endl;
    }
};

int main(int argc, char **argv)
{
    setbuf(stdout, NULL);
	std::cout << "MarketDepthSubscriptionExample" << std::endl;
    MarketDepthSubscriptionExample example;
    try {
        example.run(argc, argv);
    } catch (Exception &e) {
		std::cout << "Library Exception!!! " << e.description().c_str() << std::endl;
    }
    return 0;
}
