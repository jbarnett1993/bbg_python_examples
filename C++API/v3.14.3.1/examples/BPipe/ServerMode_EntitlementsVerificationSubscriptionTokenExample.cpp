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
//----------------------------------------------------------------------------
// ServerMode_EntitlementsVerificationSubscriptionTokenExample.cpp
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
// * subscribe to all specified 'securities'
// * for each subscription data message, check which users are entitled to 
//   receive that data before distributing that message to the user.
//
// Command line arguments:
// -ip <serverHostNameOrIp>
// -p  <serverPort>
// -t  <user's token>
// -s  <security>
// -f  <field>
// -a  <application name authentication>
// Multiple securities and tokens can be specified but the application
// is limited to one field.
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

    const Name EID("EID");
    const Name TOKEN_SUCCESS("TokenGenerationSuccess");
	const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name TOKEN("token");
	const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
	
	const std::string AUTH_APP_PREFIX = "AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";

    const char* APIAUTH_SVC             = "//blp/apiauth";
    const char* MKTDATA_SVC             = "//blp/mktdata";
	const int BPS_USER	= 0;
	const int INVALID_USER = -1;
	const int NONBPS_USER = 1;

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
        }
    }

} // anonymous namespace

class SessionEventHandler: public  EventHandler
{
    std::vector<Identity>    &d_identities;
    std::vector<std::string> &d_tokens;
    std::vector<std::string> &d_securities;
    Name                      d_fieldName;
	
    void processSubscriptionDataEvent(const Event &event)
    {
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            Service service = msg.service();

            int index = (int)msg.correlationId().asInteger();
            std::string &topic = d_securities[index];
            if (!msg.hasElement(d_fieldName)) {
                continue;
            }
            std::cout << "\t" << topic << std::endl;
            Element field = msg.getElement(d_fieldName);
            if (!field.isValid()) {
                continue;
            }
            bool needsEntitlement = msg.hasElement(EID);
            for (size_t i = 0; i < d_identities.size(); ++i) {
                Identity *handle = &d_identities[i];
                if (!needsEntitlement ||
                    handle->hasEntitlements(service, 
                        msg.getElement(EID), 0, 0))
                {
                        std::cout << "User #" << (i+1) << " is entitled"
                            << " for " << field << std::endl;
                }
                else {
                    std::cout << "User #" << (i+1) << " is NOT entitled"
                        << " for " << d_fieldName << std::endl;
                }
            }
        }
    }

public :
    SessionEventHandler(std::vector<Identity>      &identities,
                        std::vector<std::string>   &tokens,
                        std::vector<std::string>   &securities,
                        const std::string          &field)
        : d_identities(identities)
        , d_tokens(tokens)
        , d_securities(securities)
        , d_fieldName(field.c_str())
    {
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

        case Event::SUBSCRIPTION_DATA:
            try {
                processSubscriptionDataEvent(event);
            } catch (Exception &e) {
                std::cerr << "Library Exception!!! " << e.description() << std::endl;
            } catch (...) {
                std::cerr << "Unknown Exception!!!" << std::endl;
            }
            break;
        }
        return true;
    }
};

class ServerMode_EntitlementsVerificationSubscriptionTokenExample {

    std::string               d_host;
    int                       d_port;
    std::string               d_field;
    std::vector<std::string>  d_securities;
    std::vector<Identity>     d_identities;
    std::vector<std::string>  d_tokens;
	std::string				  d_appName;
	Identity                  d_appIdentity;
	int						  d_appSeatType;

    SubscriptionList          d_subscriptions;
    Session                  *d_session;

    void printUsage()
    {
        std::cout << "Usage:" << '\n'
            << "    Entitlements verification example" << '\n'
            << "        [-s     <security   = MSFT US Equity>]" << '\n'
            << "        [-f     <field  = BEST_BID1>]" << '\n'
            << "        [-a     <application name authentication>]" << '\n'
            << "        [-t     <user's token string>]"
            << " ie. token value returned in generateToken response" << '\n'
            << "        [-ip    <ipAddress  = localhost>]" << '\n'
            << "        [-p     <tcpPort    = 8194>]" << '\n'
            << "Note:" << '\n'
            << "Multiple securities and tokens can be specified."
            << " Only one field can be specified."
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

        std::cout << "Connecting to " << d_host << ":" << d_port << std::endl;

        d_session = new Session(sessionOptions, 
            new SessionEventHandler(d_identities, d_tokens, d_securities, d_field));
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

        if (!d_session->openService(MKTDATA_SVC)) {
            std::cout << "Failed to open service: " << MKTDATA_SVC
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
							d_appSeatType = d_appIdentity.getSeatType();
							switch (d_appSeatType)
							{
								case BPS_USER: //Identity::SeatType::BPS:
									seatType = "BPS";
									break;
								case INVALID_USER: //Identity::SeatType::INVALID_SEAT:
									seatType = "Invalid Seat";
									break;
								case NONBPS_USER: //Identity::SeatType::NONBPS:
									seatType = "Non-BPS";
									break;
							}
							std::cout << "Application Identity seat type is " << seatType << std::endl;
                            return true;
                        }
                        else {
                            std::cout << "Application authorization failed" << std::endl;
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
							if (validateSeatType(d_identities[i].getSeatType())) {
                            std::cout << "User #" << (i+1)
                                      << " authorization success"
                                      << std::endl;
                            is_any_user_authorized = true;
							} else {
								std::cout << "User #" << (i+1)
									<< " authorization successed"
									<< " but user NONBPS SeatType is not"
									<< " valid with server BPS SeatType."
									<< std::endl;
							}
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

	// validate application SeatType again user SeatType
	//     application  |    user      | 
	// -----------------+ -------------+------------
	//      BPS_USER    |  BPS_USER    | Valid
	//      BPS_USER    |  NONBPS_USER | Invalid
	//      NONBPS_USER |  BPS_USER    | Valid
	//      NONBPS_USER |  NONBPS_USER | Valid
	bool validateSeatType(int userSeatType)
	{
		bool statusFlag = false;

		if (!(d_appSeatType == BPS_USER && userSeatType == NONBPS_USER)){
				// user and application SeatType are valid
				statusFlag = true;
		}
		return statusFlag;
	}

    bool parseCommandLine(int argc, char **argv)
    {
        int tokenCount = 0;
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-s") && i + 1 < argc) {
                d_securities.push_back(argv[++i]);
                continue;
            }

            if (!std::strcmp(argv[i],"-f") && i + 1 < argc) {
                d_field = std::string(argv[++i]);
                continue;
            }

            if (!std::strcmp(argv[i],"-a") && i + 1 < argc) {
                d_appName = std::string(argv[++i]);
                continue;
            }

            if (!std::strcmp(argv[i],"-t") && i + 1 < argc) {
                d_tokens.push_back(argv[++i]);
                ++tokenCount;
                std::cout << "User #" << tokenCount
                          << " token: " << argv[i]
                          << std::endl;
                continue;
            }
            if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_host = argv[++i];
                continue;
            }
            if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_port = std::atoi(argv[++i]);
                continue;
            }
            printUsage();
            return false;
        }

        if (!d_appName.size()) {
			std::cout << "No server side Application Name were specified" << std::endl << std::endl;
            printUsage();
            return false;
        }

        if (!d_tokens.size()) {
			std::cout << "No tokens were specified" << std::endl << std::endl;
            printUsage();
            return false;
        }

        if (!d_securities.size()) {
            d_securities.push_back("MSFT US Equity");
        }

        for (size_t i = 0; i < d_securities.size(); ++i) {
            d_subscriptions.add(d_securities[i].c_str(), d_field.c_str(), "",
                CorrelationId(i));
        }
        return true;
    }

public:

    ServerMode_EntitlementsVerificationSubscriptionTokenExample() {
        d_session = NULL;
        d_host = "localhost";
        d_port = 8194;
        d_field = "BEST_BID1";
    }

    ~ServerMode_EntitlementsVerificationSubscriptionTokenExample() {
        if (d_session) delete d_session;
    }

    void run(int argc, char **argv) {
        if (!parseCommandLine(argc, argv)) return;

        createSession();
        openServices();

		// Authorize server side Application Name Identity for use with request/subscription
		if (authorizeApplication()) {
			// Authorize all the users that are interested in receiving data
			if (authorizeUsers()) {
				// Make the various requests that we need to make with application's Identity 
				std::cout << "Subscribing..." << std::endl;
				d_session->subscribe(d_subscriptions, d_appIdentity);
			} else {
				std::cerr << "Unable to authorize users, Press Enter to Exit"
						  << std::endl;
			}
		}
    }
};

int main(int argc, char **argv)
{
    std::cout << "Entitlements Verification Subscription Token Example"
        << std::endl;

    ServerMode_EntitlementsVerificationSubscriptionTokenExample example;
    try {
        example.run(argc, argv);
    } catch (Exception &e) {
        std::cerr << "main: Library Exception!!! " << e.description()
            << std::endl;
    } catch (...) {
        std::cerr << "main: Unknown Exception!!!" << std::endl;
    }

    // wait for enter key to exit application
    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);

    return 0;
}

