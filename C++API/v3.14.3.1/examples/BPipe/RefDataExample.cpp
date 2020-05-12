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
#include <blpapi_exception.h>
#include <blpapi_defs.h>

#include <iostream>
#include <vector>
#include <string>
#include <stdlib.h>
#include <string.h>

using namespace BloombergLP;
using namespace blpapi;
namespace {
    const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
    const Name AUTHORIZATION_FAILURE("AuthorizationFailure");
    const Name TOKEN_SUCCESS("TokenGenerationSuccess");
    const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name TOKEN("token");

	const Name SECURITY_DATA("securityData");
    const Name SECURITY("security");
    const Name FIELD_DATA("fieldData");
    const Name RESPONSE_ERROR("responseError");
    const Name SECURITY_ERROR("securityError");
    const Name FIELD_EXCEPTIONS("fieldExceptions");
    const Name FIELD_ID("fieldId");
    const Name ERROR_INFO("errorInfo");
    const Name CATEGORY("category");
    const Name MESSAGE("message");
    const Name SESSION_TERMINATED("SessionTerminated");
    const Name SESSION_STARTUP_FAILURE("SessionStartupFailure");

	const char *REFDATA_SVC = "//blp/refdata";
	const char *AUTH_SVC = "//blp/apiauth";
};

class RefDataExample 
{
	std::vector<std::string> d_hosts;		// IP Addresses of appliances
    int					d_port;
	std::string			d_authOption;	// authentication option user/application
	std::string			d_name;	        // DirectoryService/ApplicationName
    Identity			d_identity;
	Session				*d_session;
    std::vector<std::string> d_securities;
    std::vector<std::string> d_fields;

    bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-s") && i + 1 < argc) {
                d_securities.push_back(argv[++i]);
            } else if (!std::strcmp(argv[i],"-f") && i + 1 < argc) {
                d_fields.push_back(argv[++i]);
            } else if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_hosts.push_back(argv[++i]);
            } else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_port = std::atoi(argv[++i]);
			} else if (!std::strcmp(argv[i],"-auth") &&  i + 1 < argc) {
				d_authOption = argv[++i];
			} else if (!std::strcmp(argv[i],"-n") &&  i + 1 < argc) {
				d_name = argv[++i];
			} else if (!std::strcmp(argv[i], "-h")) {
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
        if (d_securities.size() == 0) {
            d_securities.push_back("IBM US Equity");
        }

        if (d_fields.size() == 0) {
            d_fields.push_back("PX_LAST");
        }

        return true;
    }

    void printErrorInfo(const char *leadingStr, const Element &errorInfo)
    {
        std::cout << leadingStr
            << errorInfo.getElementAsString(CATEGORY)
            << " ("
            << errorInfo.getElementAsString(MESSAGE)
            << ")" << std::endl;
    }

    void printUsage()
    {
        std::cout << "Usage:" << std::endl
            << "    Retrieve reference data " << std::endl
            << "        [-s         <security   = IBM US Equity>" << std::endl
            << "        [-f         <field      = PX_LAST>" << std::endl
            << "        [-ip        <ipAddress  = localhost>" << std::endl
            << "        [-p         <tcpPort    = 8194>" << std::endl
			<< "        [-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]" << std::endl
            << "        [-n         <name = applicationName or directoryService>]" << std::endl
            << "Notes:" << std::endl
            << " -Specify only LOGON to authorize 'user' using Windows login name." << std::endl
            << " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << std::endl
            << " -Specify APPLICATION and name(Application Name) to authorize application." << std::endl;
    }

	bool startSession(){

        SessionOptions sessionOptions;
		std::string authOptions;

		std::cout << "Connecting to port " << d_port << " on server: "; 
        for (size_t i = 0; i < d_hosts.size(); ++i) {
            sessionOptions.setServerAddress(d_hosts[i].c_str(), d_port, i);
			std::cout << d_hosts[i].c_str();
        }
		std::cout << std::endl;

        sessionOptions.setServerPort(d_port);
        sessionOptions.setAutoRestartOnDisconnection(true);
        sessionOptions.setNumStartAttempts(d_hosts.size());
		authOptions = getAuthOptions();
		if (authOptions.size() > 0) {
			sessionOptions.setAuthenticationOptions(authOptions.c_str());
		}
        d_session = new Session(sessionOptions);

        if (!d_session->start()) {
            std::cerr << "Failed to connect!" << std::endl;
            return false;
        }
        if (!d_session->openService(REFDATA_SVC)) {
			std::cerr << "Failed to open " << REFDATA_SVC << std::endl;
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
					std::cout << "token = " << token.c_str() << std::endl;
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

    void sendRefDataRequest(Session &session)
    {
        Service refDataService = session.getService("//blp/refdata");
        Request request = refDataService.createRequest("ReferenceDataRequest");

        // Add securities to request
        Element securities = request.getElement("securities");
        for (size_t i = 0; i < d_securities.size(); ++i) {
            securities.appendValue(d_securities[i].c_str());
        }

        // Add fields to request
        Element fields = request.getElement("fields");
        for (size_t i = 0; i < d_fields.size(); ++i) {
            fields.appendValue(d_fields[i].c_str());
        }

		if (!strcmp(d_authOption.c_str(), "NONE")) {
			std::cout << "Sending request: " << request << std::endl;
			d_session->sendRequest(request);
		} else {
			std::cout << "Sending request with user's Identity: " << request << std::endl;
			if (d_identity.getSeatType() == Identity::BPS) {
				std::cout << "BPS User" << std::endl;
			} else if (d_identity.getSeatType() == Identity::NONBPS) {
				std::cout << "NON-BPS User" << std::endl;
			} else if (d_identity.getSeatType() == Identity::INVALID_SEAT) {
				std::cout << "Invalid User" << std::endl;
			}
			d_session->sendRequest(request, d_identity);
		}
    }
    // return true if processing is completed, false otherwise
    void processResponseEvent(Event event)
    {
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            if (msg.asElement().hasElement(RESPONSE_ERROR)) {
                printErrorInfo("REQUEST FAILED: ", 
                    msg.getElement(RESPONSE_ERROR));
                continue;
            }

            Element securities = msg.getElement(SECURITY_DATA);
            size_t numSecurities = securities.numValues();
            std::cout << "Processing " << (unsigned int)numSecurities 
                      << " securities:"<< std::endl;
            for (size_t i = 0; i < numSecurities; ++i) {
                Element security = securities.getValueAsElement(i);
                std::string ticker = security.getElementAsString(SECURITY);
                std::cout << "\nTicker: " + ticker << std::endl;
                if (security.hasElement("securityError")) {
                    printErrorInfo("\tSECURITY FAILED: ",
                        security.getElement(SECURITY_ERROR));
                    continue;
                }

                if (security.hasElement(FIELD_DATA)) {
                    const Element fields = security.getElement(FIELD_DATA);
                    if (fields.numElements() > 0) {
                        std::cout << "FIELD\t\tVALUE"<<std::endl;
                        std::cout << "-----\t\t-----"<< std::endl;
                        size_t numElements = fields.numElements();
                        for (size_t j = 0; j < numElements; ++j) {
                            Element field = fields.getElement(j);
                            std::cout << field.name() << "\t\t" <<
                                field.getValueAsString() << std::endl;
                        }
                    }
                }
                std::cout << std::endl;
                Element fieldExceptions = security.getElement(FIELD_EXCEPTIONS);
                if (fieldExceptions.numValues() > 0) {
                    std::cout << "FIELD\t\tEXCEPTION" << std::endl;
                    std::cout << "-----\t\t---------" << std::endl;
                    for (size_t k = 0; k < fieldExceptions.numValues(); ++k) {
                        Element fieldException =
                            fieldExceptions.getValueAsElement(k);
                        Element errInfo = fieldException.getElement(ERROR_INFO);
                        std::cout << fieldException.getElementAsString(FIELD_ID)
                                  << "\t\t"
                                  << errInfo.getElementAsString(CATEGORY)
                                  << " ( "
                                  << errInfo.getElementAsString(MESSAGE)
                                  << ")"
                                  << std::endl;
                    }
                }
            }
        }
    }

    void eventLoop(Session &session)
    {
        bool done = false;
        while (!done) {
            Event event = session.nextEvent();
            if (event.eventType() == Event::PARTIAL_RESPONSE) {
                std::cout << "Processing Partial Response" << std::endl;
                processResponseEvent(event);
            }
            else if (event.eventType() == Event::RESPONSE) {
                std::cout << "Processing Response" << std::endl;
                processResponseEvent(event);
                done = true;
            } else {
                MessageIterator msgIter(event);
                while (msgIter.next()) {
                    Message msg = msgIter.message();
                    if (event.eventType() == Event::SESSION_STATUS) {
                        if (msg.messageType() == SESSION_TERMINATED ||
                            msg.messageType() == SESSION_STARTUP_FAILURE) {
                            done = true;
                        }
                    }
                }
            }
        }
    }

public:
	RefDataExample() : d_session(0)
    {
        d_port = 8194;
		d_name = "";
    }

    ~RefDataExample()
    {
		if (d_session) delete d_session;
    }

    void run(int argc, char **argv)
    {
        if (!parseCommandLine(argc, argv)) return;

        if (!startSession()) return;

		if (strcmp(d_authOption.c_str(), "NONE")) {
			if (!authorize()) {
				return;
			}
		}

		sendRefDataRequest(*d_session);

        // wait for events from session.
        try {
            eventLoop(*d_session);
        } catch (Exception &e) {
            std::cerr << "Library Exception !!!" << e.description() << std::endl;
        } catch (...) {
            std::cerr << "Unknown Exception !!!" << std::endl;
        }

        d_session->stop();
    }
};

int main(int argc, char **argv)
{
    std::cout << "RefDataExample" << std::endl;
    RefDataExample example;
    example.run(argc, argv);

    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}
