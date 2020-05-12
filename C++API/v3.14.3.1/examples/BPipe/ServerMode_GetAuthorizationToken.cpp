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

// ServerMode_GetAuthorizationToken.cpp
//
// This program requests a 'token' and displays it on the console.
// Refer to the following examples that accept a 'token' on the command line
// and use it to authorize users:
//      EntitleVerificationTokenExample
//      EntitleVerificationSubscriptionTokenExample
//
// By default this program will generate a 'token' based on the current
// logged in user. The "-d" option can be used to specify a property to look up
// via active directory services. For example, "-d mail" would look up the 
// value for the property "mail" which could be the email address of the user.
//
// Workflow:
// * set options based on what information will be used to generate the 'token'
// * connect to server
// * call generateToken to request a 'token'
// * look for "TOKEN_STATUS" events for success or failure.
//
// Command line arguments:
// -ip <serverHostNameOrIp>
// -p  <serverPort>
// -auth <authenticationOption = LOGON or APPLICATION or DIRSVC>]
// -n   <name = applicationName or directoryService>]
//

#include <blpapi_defs.h>
#include <blpapi_correlationid.h>
#include <blpapi_element.h>
#include <blpapi_event.h>
#include <blpapi_exception.h>
#include <blpapi_message.h>
#include <blpapi_session.h>

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
}

class ServerMode_GetAuthorizationToken
{
	std::string					d_host;		// IP Address of appliances
    int                         d_port;
	std::string                 d_authOption;	// authentication option user/application
	std::string                 d_name;	        // DirectoryService/ApplicationName

    Session                    *d_session;

    void printUsage()
    {
        std::cout << "Usage:" << std::endl
            << "	Generate a token for authorization " << std::endl
            << "		[-ip 		<ipAddress	= localhost>" << std::endl
            << "		[-p 		<tcpPort	= 8194>" << std::endl
			<< "        [-auth      <authenticationOption = LOGON or APPLICATION or DIRSVC>]" << std::endl
			<< "        [-n         <name = applicationName or directoryService>]" << std::endl
			<< "Notes:" << std::endl
			<< " -Specify only LOGON to authorize 'user' using Windows login name." << std::endl
			<< " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << std::endl;
        std::cout << "Press ENTER to quit" <<std::endl;
    }

    bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) 
                d_host = argv[++i];
            else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc)
                d_port = std::atoi(argv[++i]);
            else if (!std::strcmp(argv[i],"-auth") &&  i + 1 < argc) {
				d_authOption = argv[++i];
			} else if (!std::strcmp(argv[i],"-n") &&  i + 1 < argc) {
                d_name = argv[++i];
            } else {
                printUsage();
                return false;
            }
        }

		// check for appliation name
		if ((!std::strcmp(d_authOption.c_str(), "USER_APP")) && (!std::strcmp(d_name.c_str(), ""))){
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

    bool processTokenStatus(const Event &event,
                            const CorrelationId &expectedCorrelationId)
    {
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            if (expectedCorrelationId != msg.correlationId()) {
                std::cerr << "Received message for unknown correlationId: "
                          << msg.correlationId()
                          << std::endl;
                msg.print(std::cout);
                continue;
            }

            if (msg.messageType() == TOKEN_SUCCESS) {
                // handle token generation success
            } else if (msg.messageType() == TOKEN_FAILURE) {
                // handle token generation failure
            }
            msg.print(std::cout);
        }

        return true;
    }

    bool processEvent(const Event &event)
    {
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
                msg.print(std::cout);
        }
        return true;
    }

public:
    ServerMode_GetAuthorizationToken()
        : d_host("127.0.0.1"), d_port(8194), d_authOption("LOGON"), d_name(""), d_session(0)
    {
    }

    ~ServerMode_GetAuthorizationToken()
    {
        if (d_session) {
            d_session->stop();
            delete d_session;
        }
    }

    void run(int argc, char **argv)
    {
		std::string authOptions;

		if (!parseCommandLine(argc, argv)) return;

        SessionOptions sessionOptions;
        sessionOptions.setServerHost(d_host.c_str());
        sessionOptions.setServerPort(d_port);
        sessionOptions.setAutoRestartOnDisconnection(true);

		if (!std::strcmp(d_authOption.c_str(),"USER_APP")) { //  Authenticate application
            // Set User and Application Authentication Option
            authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
            authOptions += "AuthenticationType=OS_LOGON;";
            authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
            // ApplicationName is the entry in EMRS.
            authOptions += "ApplicationName=" + d_name;
		} else {
			// Set User authentication option
			if (!strcmp(d_authOption.c_str(), "LOGON")) {   			
				// Authenticate user using windows/unix login name
				authOptions = "AuthenticationType=OS_LOGON";
			} else if (!strcmp(d_authOption.c_str(), "DIRSVC")) {		
				// Authenticate user using active directory service property
				authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
				authOptions += "DirSvcPropertyName=" + d_name;
			}
		}

		std::cout << "Authentication Options = " << authOptions << std::endl;
		// Add the authorization options to the sessionOptions
		sessionOptions.setAuthenticationOptions(authOptions.c_str());

        d_session = new Session(sessionOptions);
        if (!d_session->start()) {
            std::cerr <<"Failed to start session." << std::endl;
            return;
        }

        CorrelationId corrId = d_session->generateToken();

        while (true) {
            Event event = d_session->nextEvent();
            if (event.eventType() == Event::TOKEN_STATUS) {
                processTokenStatus(event, corrId);
                //break;
            } else {
                if (!processEvent(event)) {
                   // break;
                }
            }
        }
    }
};

int main(int argc, char **argv)
{
    std::cout << "ServerMode_GetAuthorizationToken" << std::endl;
    ServerMode_GetAuthorizationToken getToken;
    try {
        getToken.run(argc, argv);
    } 
    catch (Exception &e) {
        std::cerr << "Library Exception!!! " << e.description() 
            << std::endl;
    }

    // wait for enter key to exit application
    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);

    return 0;
}
