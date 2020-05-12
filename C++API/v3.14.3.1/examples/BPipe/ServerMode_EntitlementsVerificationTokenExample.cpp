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

// ServerMode_EntitlementsVerificationTokenExample.cpp
//
// This program demonstrates a server mode application that authorizes its
// users with tokens returned by a generateToken request. For the purposes
// of this demonstration, the "GetAuthorizationToken" program can be used
// to generate a token and display it on the console. For ease of demonstration
// this application takes one or more 'tokens' on the command line. But in a real
// server mode application the 'token' would be received from the client
// applications using some IPC mechanism.
//
// Workflow:
// * connect to server
// * open services
// * send authorization request for each 'token' which represents a user.
// * send "ReferenceDataRequest" for all specified 'securities'
// * for each response message, check which users are entitled to receive
//   that message before distributing that message to the user.
//
// Command line arguments:
// -ip <serverHostNameOrIp>
// -p  <serverPort>
// -t  <user's token>
// -s  <security>
// -a  <application name authentication>
// Multiple securities and tokens can be specified.
//

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

using namespace BloombergLP;
using namespace blpapi;

namespace {

    const Name RESPONSE_ERROR("responseError");
    const Name SECURITY_DATA("securityData");
    const Name SECURITY("security");
    const Name EID_DATA("eidData");
    const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
    const Name TOKEN_SUCCESS("TokenGenerationSuccess");
	const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name TOKEN("token");
	
	const std::string AUTH_APP_PREFIX = "AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";

	const char* REFRENCEDATA_REQUEST = "ReferenceDataRequest";
    const char* APIAUTH_SVC          = "//blp/apiauth";
    const char* REFDATA_SVC          = "//blp/refdata";

    void printEvent(const Event &event)
    {
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            CorrelationId correlationId = msg.correlationId();
            if (correlationId.asInteger() != 0) {
                std::cout << "Correlator: " << correlationId.asInteger() << std::endl;
            }
            msg.print(std::cout);
            std::cout << std::endl;
        }
    }

} // anonymous namespace

class SessionEventHandler: public  EventHandler
{
    std::vector<Identity>   &d_identities;
    std::vector<std::string>     &d_tokens;

    void printFailedEntitlements(std::vector<int> &failedEntitlements,
        int numFailedEntitlements)
    {
        for (int i = 0; i < numFailedEntitlements; ++i) {
            std::cout << failedEntitlements[i] << " ";
        }
        std::cout << std::endl;
    }

    void distributeMessage(Message &msg)
    {
        Service service = msg.service();

        std::vector<int> failedEntitlements;
        Element securities = msg.getElement(SECURITY_DATA);
        int numSecurities = securities.numValues();

        std::cout << "Processing " << numSecurities << " securities:" 
            << std::endl;
        for (int i = 0; i < numSecurities; ++i) {
            Element security     = securities.getValueAsElement(i);
            std::string ticker   = security.getElementAsString(SECURITY);
            Element entitlements;
            if (security.hasElement(EID_DATA)) {
                entitlements = security.getElement(EID_DATA);
            }

            int numUsers = d_identities.size();
            if (entitlements.isValid() && entitlements.numValues() > 0) {
                // Entitlements are required to access this data
                failedEntitlements.resize(entitlements.numValues());
                for (int j = 0; j < numUsers; ++j) {
                    std::memset(&failedEntitlements[0], 0,
                        sizeof(int) * failedEntitlements.size());
                    int numFailures = failedEntitlements.size();
                    if (d_identities[j].hasEntitlements(service, entitlements, 
                        &failedEntitlements[0], &numFailures)) {
                            std::cout << "User #" << (j+1)
                                      << " is entitled to get data for: " << ticker
                                      << std::endl;
                            // Now Distribute message to the user. 
                    }
                    else {
                        std::cout << "User #" << (j+1)
                                  << " is NOT entitled to get data for: "
                                  << ticker << " - Failed eids: "
                                  << std::endl;
                        printFailedEntitlements(failedEntitlements, numFailures);
                    }
                }
            }
            else {
                // No Entitlements are required to access this data.
                for (int j = 0; j < numUsers; ++j) {
                    std::cout << "User: " << d_tokens[j] <<
                        " is entitled to get data for: " 
                        << ticker << std::endl;
                    // Now Distribute message to the user. 
                }
            }
        }
    }

    void processResponseEvent(const Event &event)
    {
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            if (msg.hasElement(RESPONSE_ERROR)) {
                msg.print(std::cout) << std::endl;
                continue;
            }
            // We have a valid response. Distribute it to all the users.
            distributeMessage(msg);
        }
    }

public :

    SessionEventHandler(std::vector<Identity> &identities,
                        std::vector<std::string> &tokens) : 
    d_identities(identities), d_tokens(tokens) {
    }

    bool processEvent(const Event &event, Session *session)
    {
        switch(event.eventType()) {
        case Event::SESSION_STATUS:
        case Event::SERVICE_STATUS:
        case Event::REQUEST_STATUS:
        case Event::AUTHORIZATION_STATUS:
		case Event::SUBSCRIPTION_STATUS:
            printEvent(event);
            break;

        case Event::RESPONSE:
        case Event::PARTIAL_RESPONSE:
            try {
                processResponseEvent(event);
            } 
            catch (Exception &e) {
                std::cerr << "Library Exception!!! " << e.description()
                    << std::endl;
                return true;
            } catch (...) {
                std::cerr << "Unknown Exception!!!" << std::endl;
                return true;
            }
            break;
         }
        return true;
    }
};

class ServerMode_EntitlementsVerificationTokenExample {

    std::string               d_host;
    int                       d_port;
    std::vector<std::string>  d_securities;
    std::vector<Identity>     d_identities;
    std::vector<std::string>  d_tokens;
	std::string				  d_appName;
	Identity                  d_appIdentity;

    Session                  *d_session;


    void printUsage()
    {
        std::cout 
            << "Usage:" << '\n'
            << "    Entitlements verification token example" << '\n'
            << "        [-s     <security   = MSFT US Equity>]" << '\n'
            << "        [-a     <application name authentication>]" << '\n'
            << "        [-t     <user's token string>]"
            << " ie. token value returned in generateToken response" << '\n'
            << "        [-ip    <ipAddress  = localhost>]" << '\n'
            << "        [-p     <tcpPort    = 8194>]" << '\n'
            << "Note:" << '\n'
            << "Multiple securities and tokens can be specified."
            << std::endl;
    }


    void createSession()
    {
		std::string authOptions;

		SessionOptions sessionOptions;
        sessionOptions.setServerHost(d_host.c_str());
        sessionOptions.setServerPort(d_port);

		// set application authentication for server side data request/subscription
		authOptions.append(AUTH_APP_PREFIX);
		authOptions.append(d_appName);
		sessionOptions.setAuthenticationOptions(authOptions.c_str());

        std::cout << "Connecting to " + d_host + ":" << d_port << std::endl;

        d_session = new Session(sessionOptions, 
            new SessionEventHandler(d_identities, d_tokens));
        bool sessionStarted = d_session->start();
        if (!sessionStarted) {
            std::cerr << "Failed to start session. Exiting..." << std::endl;
            std::exit(-1);
        }
    }

    void openServices()
    {
        if (!d_session->openService(APIAUTH_SVC)) {
            std::cout << "Failed to open service: " << APIAUTH_SVC 
                << std::endl;
            std::exit(-1);
        }

        if (!d_session->openService(REFDATA_SVC)) {
            std::cout << "Failed to open service: " << REFDATA_SVC 
                << std::endl;
            std::exit(-2);
        }
    }

	bool authorizeApplication()
    {
		std::string token;
        Service authService = d_session->getService(APIAUTH_SVC);

        EventQueue tokenEventQueue;
        d_session->generateToken(CorrelationId(), &tokenEventQueue);
        Event event = tokenEventQueue.nextEvent();
        if (event.eventType() == Event::TOKEN_STATUS) {
            MessageIterator iter(event);
            while (iter.next()) {
                Message msg = iter.message();
                msg.print(std::cout);
                if (msg.messageType() == TOKEN_SUCCESS) {
                    token = msg.getElementAsString(TOKEN);
					std::cout << "token = " << token.c_str() << std::endl;
                }
                else if (msg.messageType() == TOKEN_FAILURE) {
                    break;
                }
            }
        }
        if (token.length() == 0) {
            std::cout << "Failed to get applicaton token" << std::endl;
            return false;
        }

        Request authRequest = authService.createAuthorizationRequest();
        authRequest.set(TOKEN, token.c_str());

        EventQueue authQueue;
		d_appIdentity = d_session->createIdentity();
        d_session->sendAuthorizationRequest(
            authRequest, &d_appIdentity, CorrelationId(), &authQueue);

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
							std::string seatType;
							switch (d_appIdentity.getSeatType())
							{
								case 0: //Identity::SeatType::BPS:
									seatType = "BPS";
									break;
								case -1: //Identity::SeatType::INVALID_SEAT:
									seatType = "Invalid Seat";
									break;
								case 1: //Identity::SeatType::NONBPS:
									seatType = "Non-BPS";
									break;
							}
							std::cout << "Application Identity seat type is " << seatType << std::endl << std::endl;
                            return true;
                        }
                        else {
							std::cout << "Application authorization failed" << std::endl << std::endl;
                            return false;
                        }
                    }
            }
        }
    }


    bool authorizeUsers()
    {
        Service authService = d_session->getService(APIAUTH_SVC);
        bool is_any_user_authorized = false;

        // Authorize each of the users
        d_identities.reserve(d_tokens.size());
        for (size_t i = 0; i < d_tokens.size(); ++i) {
            d_identities.push_back(d_session->createIdentity());
            Request authRequest = authService.createAuthorizationRequest();
            authRequest.set("token", d_tokens[i].c_str());

            CorrelationId correlator(&d_tokens[i]);
            EventQueue eventQueue;
            d_session->sendAuthorizationRequest(authRequest,
                &d_identities[i], correlator, &eventQueue);

            Event event = eventQueue.nextEvent();
            if (event.eventType() == Event::RESPONSE ||
                event.eventType() == Event::REQUEST_STATUS) {

                    MessageIterator msgIter(event);
                    while (msgIter.next()) {
                        Message msg = msgIter.message();
                        if (msg.messageType() == AUTHORIZATION_SUCCESS) {
                            std::cout << "User #" << (i+1)
                                      << " authorization success"
                                      << std::endl;
                            is_any_user_authorized = true;
                        }
                        else {
                            std::cout << "User #" << (i+1)
                                      << " authorization failed"
                                      << std::endl;
                            printEvent(event);
                        }
                    }
                }
        }
        return is_any_user_authorized;
    }

    void sendRefDataRequest()
    {
        Service service = d_session->getService(REFDATA_SVC);
        Request request = service.createRequest(REFRENCEDATA_REQUEST);

        // Add securities.
        Element securities = request.getElement("securities");
        for (size_t i = 0; i < d_securities.size(); ++i) {
            securities.appendValue(d_securities[i].c_str());
        }

        // Add fields
        Element fields = request.getElement("fields");
        fields.appendValue("PX_LAST");
        fields.appendValue("DS002");

        request.set("returnEids", true);

        // Send the request using the server's credentials
        std::cout << "Sending RefDataRequest using server " 
                  << "credentials..." << std::endl;
        d_session->sendRequest(request, d_appIdentity);
    }



    bool parseCommandLine(int argc, char **argv)
    {
        int tokenCount = 0;
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-s") ) {
                if (i + 1 >= argc) return false;
                d_securities.push_back(argv[++i]);
            }
            else if (!std::strcmp(argv[i],"-a")) {
				if (i + 1 >= argc) return false;
				d_appName = std::string(argv[++i]);
            }
            else if (!std::strcmp(argv[i],"-t")) {
                if (i + 1 >= argc) return false;
                d_tokens.push_back(argv[++i]);
                ++tokenCount;
                std::cout << "User #" << tokenCount
                          << " token: " << argv[i]
                          << std::endl;
            }
            else if (!std::strcmp(argv[i],"-ip")) {
                if (i + 1 >= argc) return false;
                d_host = argv[++i];
            }
            else if (!std::strcmp(argv[i],"-p")) {
                if (i + 1 >= argc) return false;
                d_port = std::atoi(argv[++i]);
            }
            else {
                // fail parse on any unknown command line argument
                return false;
            }
        }

        if (!d_appName.size()) {
            std::cout << "No server side Application Name were specified" << std::endl;
            return false;
        }

		if (!d_tokens.size()) {
            std::cout << "No tokens were specified" << std::endl;
            return false;
        }

        if (!d_securities.size()) {
            d_securities.push_back("MSFT US Equity");
        }

        return true;
    }



public:

    ServerMode_EntitlementsVerificationTokenExample() {
        d_session = NULL;
        d_host = "localhost";
        d_port = 8194;
    }

    ~ServerMode_EntitlementsVerificationTokenExample() {
        if (d_session) delete d_session;
    }

    void run(int argc, char **argv) {
        if (!parseCommandLine(argc, argv)) {
            printUsage();
            return;
        }

        createSession();
        openServices();

		// Authorize server side Application Name Identity for use with request/subscription
		if (authorizeApplication()) {
			// Authorize all the users that are interested in receiving data
			if (authorizeUsers()) {
				// Make the various requests that we need to make
				sendRefDataRequest();
			}
		}
    }

};

int main(int argc, char **argv)
{
    std::cout << "Entitlements Verification Token Example" 
              << std::endl;

    ServerMode_EntitlementsVerificationTokenExample example;
    try {
        example.run(argc, argv);
    } catch (Exception &e) {
        std::cerr << "Library Exception!!! " << e.description() 
                  << std::endl;
    } catch (...) {
        std::cerr << "Unknown Exception!!!" << std::endl;
    }

    // wait for enter key to exit application
    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}
