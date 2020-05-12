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

/*
*
** BulkRefDataExample.cpp
**
** This Example shows how to Retrieve reference data/Bulk reference data 
** using Server Api
** Usage: 
**      		-s			<security	= CAC Index>
**      		-f			<field		= INDX_MWEIGHT>
**      		-ip 		<ipAddress	= localhost>
**      		-p 			<tcpPort	= 8194>
**              -auth       <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>
**              -n          <name = applicationName or directoryService
** e.g. BulkRefDataExample -s "CAC Index" -f INDX_MWEIGHT -ip localhost -p 8194
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

using namespace std;
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
    const Name REF_DATA_REQ("ReferenceDataRequest");

	const char *REFDATA_SVC = "//blp/refdata";
	const char *AUTH_SVC = "//blp/apiauth";
};

class BulkRefDataExample 
{
    vector<string> d_hosts;		// IP Addresses of appliances
    int			d_port;
	std::string d_authOption;	// authentication option user/application
	std::string d_name;	        // DirectoryService/ApplicationName
    Identity	d_identity;
    vector<string> d_securities;
    vector<string> d_fields;

private:

    // Parses the command line arguments
	bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!strcmp(argv[i],"-s") && i + 1 < argc) {
                d_securities.push_back(argv[++i]);
            } else if (!strcmp(argv[i],"-f") && i + 1 < argc) {
                d_fields.push_back(argv[++i]);
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
            d_securities.push_back("CAC Index");
        }

        if (d_fields.size() == 0) {
            d_fields.push_back("INDX_MWEIGHT");
        }

        return true;
    }

    // Prints Error Information
    void printErrorInfo(const char *leadingStr, const Element &errorInfo)
    {
        cout << leadingStr
            << errorInfo.getElementAsString(CATEGORY)
            << " ("
            << errorInfo.getElementAsString(MESSAGE)
            << ")" << endl;
    }

    // Prints Program Usage
	void printUsage()
    {
        cout << "Usage:" << endl
            << "    Retrieve reference data/Bulk reference data using Server Api" 
			<< endl
            << "      [-s         <security  = CAC Index>" << endl
            << "      [-f         <field     = INDX_MWEIGHT>" << endl
            << "      [-ip        <ipAddress = localhost>" << endl
            << "      [-p         <tcpPort   = 8194>" << endl
			<< "      [-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]" << endl
			<< "      [-n         <name = applicationName or directoryService>]" << endl
			<< "Notes:" << endl
			<< " -Specify only LOGON to authorize 'user' using Windows login name." << endl
			<< " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << endl
			<< " -Specify APPLICATION and name(Application Name) to authorize application." << endl;
    }

    // Function to send Reference data request
	void sendRefDataRequest(Session &session)
    {
        Service refDataService = session.getService(REFDATA_SVC);
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
			cout << "Sending request: " << request << endl;
			session.sendRequest(request);
		} else {
			cout << "Sending request with user's Identity: " << request << endl;
			if (d_identity.getSeatType() == Identity::BPS) {
				cout << "BPS User" << endl;
			} else if (d_identity.getSeatType() == Identity::NONBPS) {
				cout << "NON-BPS User" << endl;
			} else if (d_identity.getSeatType() == Identity::INVALID_SEAT) {
				cout << "Invalid User" << endl;
			}
			session.sendRequest(request, d_identity);
		}
    }

    // Function to handle response event
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
            cout << "Processing " << (unsigned int)numSecurities 
					  << " securities:" << endl;

			for (size_t i = 0; i < numSecurities; ++i) {
                Element security = securities.getValueAsElement(i);
                string ticker = security.getElementAsString(SECURITY);
                cout << "\nTicker: " + ticker << endl;
                if (security.hasElement("securityError")) {
                    printErrorInfo("\tSECURITY FAILED: ",
                        security.getElement(SECURITY_ERROR));
                    continue;
                }

				// Handle FIELD_DATA
                if (security.hasElement(FIELD_DATA)) {
                    const Element fields = security.getElement(FIELD_DATA);
                    if (fields.numElements() > 0) {
                        cout << "FIELD\t\tVALUE"<<endl;
                        cout << "-----\t\t-----"<< endl;
                        size_t numElements = fields.numElements();
                        for (size_t j = 0; j < numElements; ++j) {
                            const Element  field = fields.getElement(j);
                            // Checking if the field is Bulk field
							if (field.datatype() == DataType::SEQUENCE){
								processBulkField(field);
							}else{
								processRefField(field);
							}
                        }
                    }
                }
                cout << endl;
				// Handle FIELD_EXCEPTIONS if any
                Element fieldExceptions = security.getElement(FIELD_EXCEPTIONS);
                if (fieldExceptions.numValues() > 0) {
                    cout << "FIELD\t\tEXCEPTION" << endl;
                    cout << "-----\t\t---------" << endl;
                    for (size_t k = 0; k < fieldExceptions.numValues(); ++k) {
                        Element fieldException =
                            fieldExceptions.getValueAsElement(k);
                        Element errInfo = fieldException.getElement(ERROR_INFO);
                        cout << fieldException.getElementAsString(FIELD_ID)
                                  << "\t\t"
                                  << errInfo.getElementAsString(CATEGORY)
                                  << " ( "
                                  << errInfo.getElementAsString(MESSAGE)
                                  << ")"
                                  << endl;
                    }
                }
            }
        }
    }

	// Read the reference bulk field contents
	void processBulkField(Element refBulkfield)
	{
		cout << endl << refBulkfield.name() << endl ;
        // Get the total number of Bulk data points
        size_t numofBulkValues = refBulkfield.numValues();
        for (size_t bvCtr = 0; bvCtr < numofBulkValues; bvCtr++) {
            const Element  bulkElement = refBulkfield.getValueAsElement(bvCtr);                                   
            // Get the number of sub fields for each bulk data element
			size_t numofBulkElements = bulkElement.numElements();									
            // Read each field in Bulk data
            for (size_t beCtr = 0; beCtr < numofBulkElements; beCtr++){
                const Element  elem = bulkElement.getElement(beCtr);
				cout << elem.name() << "\t\t" 
					 << elem.getValueAsString() << endl;
            }
        }
	}

	// Read the reference field contents
	void processRefField(Element reffield)
	{
		cout << reffield.name() << "\t\t" ;
		cout << reffield.getValueAsString() << endl;
	}

    // Polling for the the events
	void eventLoop(Session &session)
    {
        bool done = false;
        while (!done) {
            Event event = session.nextEvent();
            if (event.eventType() == Event::PARTIAL_RESPONSE) {
                cout << "Processing Partial Response" << endl;
                processResponseEvent(event);
            }
            else if (event.eventType() == Event::RESPONSE) {
                cout << "Processing Response" << endl;
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
    // Constructor
    BulkRefDataExample()
    {
        d_port = 8194;
		d_name = "";
    }

    // Destructor
    ~BulkRefDataExample()
    {
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

    // Function reads command line arguments, 
    // Establish a Session
    // Identify and Open refdata Service
    // Send ReferenceDataRequest to the Service 
    // Event Loop and Response Processing
    void run(int argc, char **argv)
    {
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
        Session session(sessionOptions);

        if (!session.start()) {
            cout << "Failed to start session." << endl;
            return;
        }

		if (strcmp(d_authOption.c_str(), "NONE")) {
			if (!authorize(&session)) {
				return;
			}
		}

		if (!session.openService(REFDATA_SVC)) {
            cout << "Failed to open " << REFDATA_SVC << endl;
            return;
        }      

        sendRefDataRequest(session);

        // wait for events from session.
        try {
            eventLoop(session);
        } catch (Exception &e) {
            cerr << "Library Exception !!!" << e.description() << endl;
        } catch (...) {
            cerr << "Unknown Exception !!!" << endl;
        }

        session.stop();
    }
};

int main(int argc, char **argv)
{
    cout << "BulfRefDataExample" << endl;
    BulkRefDataExample example;
    example.run(argc, argv);

    cout << "Press ENTER to quit" << endl;
    char dummy[2];
    cin.getline(dummy, 2);
    return 0;
}