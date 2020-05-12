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
#include <blpapi_exception.h>

#include <iostream>
#include <vector>
#include <string>
#include <stdlib.h>
#include <string.h>

using namespace std;
using namespace BloombergLP;
using namespace blpapi;
namespace {
    const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
    const Name AUTHORIZATION_FAILURE("AuthorizationFailure");
    const Name TOKEN_SUCCESS("TokenGenerationSuccess");
    const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name TOKEN("token");

	const Name LAST_PRICE("LAST_PRICE");

	const char *MKTDATA_SVC = "//blp/mktdata";
	const char *REFDATA_SVC = "//blp/refdata";
	const char *AUTH_SVC = "//blp/apiauth";
}

class MyEventHandler :public EventHandler {
    // Process events using callback

    public: 
    bool processEvent(const Event &event, Session *session) {
        try {
            if (event.eventType() == Event::SUBSCRIPTION_DATA) {
                MessageIterator msgIter(event);
                while (msgIter.next()) {
                    Message msg = msgIter.message();
                    if (msg.hasElement(LAST_PRICE)) {
                        Element field = msg.getElement(LAST_PRICE);
                        std::cout << field.name() << " = "
                            << field.getValueAsString() << std::endl;
                    }
                }
            }
            return true;
        } catch (Exception &e) {
            std::cerr << "Library Exception!!! " << e.description()
                << std::endl;
        } catch (...) {
            std::cerr << "Unknown Exception!!!" << std::endl;
        }
        return false;
    }
};

class SimpleBlockingRequestExample {

    CorrelationId d_cid;
    EventQueue d_eventQueue;
	std::vector<std::string> d_hosts;		// IP Addresses of appliances
    int					d_port;
	std::string			d_authOption;	// authentication option user/application
	std::string			d_name;	        // DirectoryService/ApplicationName
    Identity			d_identity;
	Session				*d_session;

    void printUsage()
    {
        std::cout << "Usage:" << std::endl
            << "    Retrieve reference data " << std::endl
            << "        [-ip        <ipAddress  = localhost>" << std::endl
            << "        [-p         <tcpPort   = 8194>" << std::endl
			<< "        [-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]" << std::endl
			<< "        [-n         <name = applicationName or directoryService>]" << std::endl
			<< "Notes:" << std::endl
			<< " -Specify only LOGON to authorize 'user' using Windows login name." << std::endl
			<< " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << std::endl
			<< " -Specify APPLICATION and name(Application Name) to authorize application." << std::endl;
    }

    bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_hosts.push_back(argv[++i]);
			} else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_port = std::atoi(argv[++i]);
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

		return true;
    }

public:
    SimpleBlockingRequestExample(): d_session(0)
		, d_cid((int)1)
		, d_port(8194)
		, d_name("") 
	{
    }

	~SimpleBlockingRequestExample()
	{
		if (d_session) delete d_session;
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

	bool authorize()
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

        d_session->openService(AUTH_SVC);
		Service authService = d_session->getService(AUTH_SVC);
        Request authRequest = authService.createAuthorizationRequest();
        authRequest.set(TOKEN, token.c_str());

        EventQueue authQueue;
		d_identity = d_session->createIdentity();
        d_session->sendAuthorizationRequest(
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

    void run(int argc, char **argv) {
		string authOptions;

        if (!parseCommandLine(argc, argv)) return;

        SessionOptions sessionOptions;
		cout << "Connecting to port " << d_port << " on server: "; 
        for (size_t i = 0; i < d_hosts.size(); ++i) {
            sessionOptions.setServerAddress(d_hosts[i].c_str(), d_port, i);
			cout << d_hosts[i].c_str();
        }
		cout << endl;

        sessionOptions.setServerPort(d_port);
        sessionOptions.setAutoRestartOnDisconnection(true);
        sessionOptions.setNumStartAttempts(d_hosts.size());
		authOptions = getAuthOptions();
		if (authOptions.size() > 0) {
			sessionOptions.setAuthenticationOptions(authOptions.c_str());
		}

		d_session = new Session(sessionOptions, new MyEventHandler());
        if (!d_session->start()) {
            std::cerr << "Failed to start session." << std::endl;
            return;
        }
        if (!d_session->openService(MKTDATA_SVC)) {
            std::cerr << "Failed to open " << MKTDATA_SVC << std::endl;
            return;
        }
        if (!d_session->openService(REFDATA_SVC)) {
            std::cerr <<"Failed to open " << REFDATA_SVC << std::endl;
            return;
        }

		if (authOptions.size() > 0) {
			if (!authorize()) {
				// fail authentication
				return;
			}
		}

        SubscriptionList subscriptions;
        subscriptions.add("IBM US Equity", "LAST_PRICE", "", d_cid);
		if (authOptions.size() > 0) {
		    std::cout << "Subscribing to IBM US Equity with Identity object" << std::endl;
			d_session->subscribe(subscriptions, d_identity);
		} else {
	        std::cout << "Subscribing to IBM US Equity" << std::endl;
			d_session->subscribe(subscriptions);
		}

		Service refDataService = d_session->getService(REFDATA_SVC);
        Request request = refDataService.createRequest("ReferenceDataRequest");
        request.append("securities", "IBM US Equity");
        request.append("fields", "DS002");

        CorrelationId cid(this);
		if (authOptions.size() > 0) {
	        std::cout << "Requesting reference data IBM US Equity with Identity object" << std::endl;
	        d_session->sendRequest(request, d_identity, cid, &d_eventQueue);
		} else {
	        std::cout << "Requesting reference data IBM US Equity" << std::endl;
			d_session->sendRequest(request, cid, &d_eventQueue);
		}

        while (true) {
            Event event = d_eventQueue.nextEvent();
            MessageIterator msgIter(event);
            while (msgIter.next()) {
                Message msg = msgIter.message();
                msg.print(std::cout);
            }
            if (event.eventType() == Event::RESPONSE) {
                break;
            }
        }
    }
};

int main(int argc, char **argv) {
    std::cout << "SimpleBlockingRequestExample" << std::endl;
    SimpleBlockingRequestExample example;
    try {
        example.run(argc, argv);
    } catch (Exception &e) {
        std::cerr << "Library Exception!!! " << e.description() << std::endl;
    }
    // wait for enter key to exit application
    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}
