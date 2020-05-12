// Added fragment check. PC.
// fixed d_identity/identity error. PC.

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

#include <blpapi_defs.h>
#include <blpapi_correlationid.h>
#include <blpapi_element.h>
#include <blpapi_event.h>
#include <blpapi_exception.h>
#include <blpapi_message.h>
#include <blpapi_session.h>
#include <blpapi_subscriptionlist.h>

#include <iostream>
#include <string>
#include <stdlib.h>
#include <string.h>
#include <vector>

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
}

class MarketListSubscriptionExample
{
	std::vector<std::string> d_hosts;	    // IP Addresses of appliances
    int                      d_port;
	std::string			     d_authOption;	// authentication option user/application
	std::string			     d_name;	    // DirectoryService/ApplicationName
	Session				     *d_session;
	std::vector<std::string> d_securities;

private:
	void printUsage()
    {
        std::cout << "Usage:" << std::endl
            << "    MarketListSubscriptionExample " << std::endl
			<< "        [-s      <security   = //blp/mktlist/chain/bsym/US/IBM>" << std::endl
            << "        [-ip     <ipAddress  = localhost>" << std::endl
            << "        [-p      <tcpPort    = 8194>" << std::endl
			<< "        [-auth   <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]" << std::endl
            << "        [-n      <name = applicationName or directoryService>]" << std::endl
            << "Notes:" << std::endl
            << " -Specify only LOGON to authorize 'user' using Windows login name." << std::endl
            << " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << std::endl
            << " -Specify APPLICATION and name(Application Name) to authorize application." << std::endl;
    }


    bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 0; i < argc; ++i) {
            if (i == 0) continue; // ignore the program name.
            if (!std::strcmp(argv[i],"-s") && i + 1 < argc) {
                d_securities.push_back(argv[++i]);
            } else if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_hosts.push_back(argv[++i]);
            } else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_port = std::atoi(argv[++i]);
			} else if (!std::strcmp(argv[i],"-auth") &&  i + 1 < argc) {
				d_authOption = argv[++i];
			} else if (!std::strcmp(argv[i],"-n") &&  i + 1 < argc) {
				d_name = argv[++i];
			} else if (!std::strcmp(argv[i], "-h") && i + 1 < argc) {
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

		// check for application name
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
		if(d_securities.size()==0){
			d_securities.push_back("//blp/mktlist/chain/bsym/US/IBM");
		}

        return true;
    }

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
        sessionOptions.setNumStartAttempts((int)d_hosts.size());

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

		d_session = new Session(sessionOptions);
        bool sessionStarted = d_session->start();
        if (!sessionStarted) {
            std::cerr << "Failed to start session. Exiting..." << std::endl;
            std::exit(-1);
        }
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
            std::cerr << "Failed to get token" << std::endl;
            return false;
        }

		if (!d_session->openService(authServiceName)) {
			std::cerr << "Failed to open " << authServiceName << std::endl;
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

public:
	MarketListSubscriptionExample() : d_session(0), d_port(8194) {}

	~MarketListSubscriptionExample()
	{
		if (d_session) delete d_session;
	}


	void printFragType(int type)
	{
		if (type == Message::FRAGMENT_NONE)
		{
			std::cout << "Fragment Type - Message::FRAGMENT_NONE" << std::endl;
		}
		else if (type == Message::FRAGMENT_START)
		{
			std::cout << "Fragment Type - Message::FRAGMENT_START" << std::endl;
		}
		else if (type == Message::FRAGMENT_INTERMEDIATE)
		{
			std::cout << "Fragment Type - Message::FRAGMENT_INTERMEDIATE" << std::endl;
		}
		else if (type == Message::FRAGMENT_END)
		{
			std::cout << "Fragment Type - Message::FRAGMENT_END" << std::endl;
		}
		else
		{
			std::cout << "Fragment Type - Unknown" << std::endl;
		}
	}



    void run(int argc, char **argv)
    {
        if (!parseCommandLine(argc, argv)) return;
		std::string serviceName = "//blp/mktlist";

		createSession();

		if (!d_session->openService(serviceName.c_str())) {
            std::cerr << "Failed to open " << serviceName << std::endl;
            return;
        }

		SubscriptionList subscriptions;

		for (int i=0; i < (int)d_securities.size(); ++i) {
			const char *security = d_securities[i].c_str();
			subscriptions.add(security, CorrelationId((char *)security));
		}

		if (std::strcmp(d_authOption.c_str(),"NONE"))
		{
			// Authorize all the users that are interested in receiving data
			Identity identity = d_session->createIdentity();
			if (authorize(&identity)) {
				std::cout << "Subscribing with Identity..." << std::endl;
				d_session->subscribe(subscriptions, identity);
			}
			else
			{
				return;
			}
		}
		else
		{
			std::cout << "Subscribing with no Identity..." << std::endl;
			d_session->subscribe(subscriptions);
		}
		while (true) {
			Event event = d_session->nextEvent();
			MessageIterator msgIter(event);
			while (msgIter.next()) {
				Message msg = msgIter.message();
				printFragType(msg.fragmentType());
				if (event.eventType() == Event::SUBSCRIPTION_STATUS ||
					event.eventType() == Event::SUBSCRIPTION_DATA) {
					const char *topic = (char *)msg.correlationId().asPointer();
					std::cout << topic << " - ";
				}
				msg.print(std::cout) << std::endl;
			}
		}
	}
};

int main(int argc, char **argv)
{
    std::cout << "MarketListSubscriptionExample" << std::endl;
    MarketListSubscriptionExample example;
    try {
        example.run(argc, argv);
    } 
    catch (Exception &e) {
        std::cerr << "Library Exception!!! " << e.description()<< std::endl;
    }

    // wait for enter key to exit application
    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}

