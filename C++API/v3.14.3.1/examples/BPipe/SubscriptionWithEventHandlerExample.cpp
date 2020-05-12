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
#include <fstream>

using namespace std;
using namespace BloombergLP;
using namespace blpapi;

namespace {
    const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
    const Name AUTHORIZATION_FAILURE("AuthorizationFailure");
    const Name TOKEN_SUCCESS("TokenGenerationSuccess");
    const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name TOKEN("token");

	const Name EXCEPTIONS("exceptions");
    const Name FIELD_ID("fieldId");
    const Name REASON("reason");
	const Name ERROR_CODE("errorCode");
    const Name CATEGORY("category");
    const Name DESCRIPTION("description");
	const Name SlowConsumerWarning("SlowConsumerWarning");
	const Name SlowConsumerWarningCleared("SlowConsumerWarningCleared");
	const Name EventSubTypeName("MKTDATA_EVENT_SUBTYPE");

	const char *MKTDATA_SVC = "//blp/mktdata";
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

			msg.print(std::cout);

            if (msg.hasElement(REASON)) {
                // This can occur on SubscriptionFailure.
                Element reason = msg.getElement(REASON);
				std::string category;
				std::string description;

				Element tempElement;
				if (reason.hasElement(ERROR_CODE, true))
				{
					// has error code
					if (!reason.getElement(&tempElement, CATEGORY))
					{
						category = tempElement.getValueAsString();
					}
					if (!reason.getElement(&tempElement, DESCRIPTION))
					{
						description = tempElement.getValueAsString();
					}

					fprintf(stdout, "        %s: %s\n",
							category.c_str(),
							description.c_str());
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
            fprintf(stdout, "%s: %s - %s\n",
                timeBuffer,
                topic->c_str(),
                msg.messageType().string());

            int numFields = msg.asElement().numElements();
            for (int i = 0; i < numFields; ++i) {
                const Element field = msg.asElement().getElement(i);
                if (field.numValues() < 1) {
                    fprintf(stdout, "        %s is NULL\n",
                        field.name().string());
                    continue;
                }

				processElement(field);
            }
        }
        return true;
    }

	void processElement(const Element &element)
	{
        if (element.isArray())
        {
			cout << "        " << element.name() << endl;
            // process array
            int numOfValues = element.numValues();
            for (int i = 0; i < numOfValues; ++i)
            {
                // process array data
                processElement(element.getValueAsElement(i));
            }
        }
        else if (element.numElements() > 0)
        {
            cout << "        " << element.name() << endl;
            int numOfElements = element.numElements();
            for (int i = 0; i < numOfElements; ++i)
            {
                // process child elements
                processElement(element.getElement(i));
            }
        }
        else
        {
            // Assume all values are scalar.
            fprintf(stdout, "        %s = %s\n",
                element.name().string(), element.getValueAsString());
        }
	}

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
            fprintf(stdout, "Library Exception !!! %s\n",
                e.description().c_str());
        }

        return false;
    }
};

class SubscriptionWithEventHandlerExample
{
    vector<string>				 d_hosts;		// IP Addresses of appliances
    int							 d_port;
	std::string					 d_authOption;	// authentication option user/application
	std::string					 d_name;	        // DirectoryService/ApplicationName
    Identity					 d_identity;
    SessionOptions               d_sessionOptions;
    Session                     *d_session;
    SubscriptionEventHandler    *d_eventHandler;
    std::vector<std::string>     d_securities;
    std::vector<std::string>     d_fields;
    std::vector<std::string>     d_options; 
    SubscriptionList             d_subscriptions; 
	string						 d_service;

    bool createSession() { 
		cout << "Connecting to port " << d_port << " on server: "; 
        for (size_t i = 0; i < d_hosts.size(); ++i) {
            d_sessionOptions.setServerAddress(d_hosts[i].c_str(), d_port, i);
			cout << d_hosts[i].c_str();
        }
        d_sessionOptions.setServerPort(d_port);
        d_sessionOptions.setAutoRestartOnDisconnection(true);
        d_sessionOptions.setNumStartAttempts(d_hosts.size());
		
		if (d_service.size() > 0) {
			// change subscription service
			d_sessionOptions.setDefaultSubscriptionService(d_service.c_str());
		}

		string authOptions = getAuthOptions();
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

        if (!d_session->openService(MKTDATA_SVC)) {
            fprintf(stderr, "Failed to open service %s",  MKTDATA_SVC);
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

    bool parseCommandLine(int argc, char **argv)
    {
		std::string secFileName;
		std::string fldFileName;

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
			} else if (!std::strcmp(argv[i],"-service") && i + 1 < argc) {
				d_service = argv[++i];
			} else if (!std::strcmp(argv[i],"-auth") &&  i + 1 < argc) {
				d_authOption = argv[++i];
			} else if (!std::strcmp(argv[i],"-n") &&  i + 1 < argc) {
                d_name = argv[++i];
			} else if (!std::strcmp(argv[i],"-sFile") && i + 1 < argc) {
				secFileName = argv[++i];
			} else if (!std::strcmp(argv[i],"-fFile") && i + 1 < argc) {
				fldFileName = argv[++i];
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
		if (fldFileName.length() > 0) {
			if(!readFields(fldFileName)) {
				std::cout << "Unable to read field file: " << fldFileName << std::endl;			
				return false;
			}
		}

		if (d_fields.size() == 0) {
            d_fields.push_back("LAST_PRICE");
        }

		if (secFileName.length() > 0) {
			if(!readSecurities(secFileName)) {
				std::cout << "Unable to read security file: " << secFileName << std::endl;			
				return false;
			}
		}

        if (d_securities.size() == 0) {
            d_securities.push_back("IBM US Equity");
        }

        for (size_t i = 0; i < d_securities.size(); ++i) {
            d_subscriptions.add(d_securities[i].c_str(), d_fields, d_options,
                                CorrelationId(&d_securities[i]));
        }

        return true;
    }

	bool readFields(std::string file) {
		bool stateFlag = false;
		int fldCount = 0;
		std::string fld;
		try {
			ifstream fldFile(file.c_str());
			if (fldFile) {
				d_fields.clear();
				while(getline(fldFile, fld, '\n')) {
					// process line
					d_fields.push_back(fld);
				}
			}
			fldFile.close();
			stateFlag = true;
		} catch (Exception &e) {
			fprintf(stderr, "Exception: %s\n",
				e.description().c_str());
		}
		return stateFlag;
	}

	bool readSecurities(std::string file) {
		bool stateFlag = false;
		std::string sec;
		try {
			ifstream secFile(file.c_str());
			if (secFile) {
				while(getline(secFile, sec, '\n')) {
					// process line
					sec.erase(sec.find_last_not_of(" ") + 1);
					sec.erase(0, sec.find_first_not_of(" "));
					d_securities.push_back(sec);
				}
			}
			secFile.close();
			stateFlag = true;
		} catch (Exception &e) {
			fprintf(stderr, "Exception: %s\n",
				e.description().c_str());
		}
		return stateFlag;
	}

    void printUsage()
    {
        const char *usage = 
            "Usage:\n"
            "    Retrieve realtime data\n"
            "        [-s     <security   = IBM US Equity>\n"
            "        [-f     <field      = LAST_PRICE>\n"
            "        [-o     <subscriptionOptions>\n"
            "        [-ip    <ipAddress  = localhost>\n"
            "        [-p     <tcpPort    = 8194>\n"
		    "        [-sFile <security list file>\n"
			"        [-fFile <field list file>\n"
			"        [-auth  <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]\n"
			"        [-n     <name = applicationName or directoryService>]\n"
            "Notes:\n"
            " -Specify only LOGON to authorize 'user' using Windows login name.\n"
            " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.\n"
            " -Specify APPLICATION and name(Application Name) to authorize application.";
        fprintf(stdout, "%s\n", usage);
    }

public:

    SubscriptionWithEventHandlerExample()
    : d_session(0)
    , d_eventHandler(0)
    {
		d_service = "";
		d_port = 8194;
		d_name = "";
	}

    ~SubscriptionWithEventHandlerExample()
    {
        if (d_session) delete d_session;
        if (d_eventHandler) delete d_eventHandler ;
    }

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

        // wait for enter key to exit application
        fprintf(stdout, "Press ENTER to quit\n\n");
        getchar();

        d_session->stop();
        fprintf(stdout, "Exiting...\n");
    }
};

int main(int argc, char **argv)
{
    setvbuf(stdout, NULL, _IONBF, 0);
    fprintf(stdout, "SubscriptionWithEventHandlerExample\n");
    SubscriptionWithEventHandlerExample example;
    try {
        example.run(argc, argv);
    } catch (Exception &e) {
        fprintf(stderr, "Library Exception!!! %s\n",
            e.description().c_str());
    }
    // wait for enter key to exit application
    fprintf(stdout, "Press ENTER to quit\n");
    getchar();
    return 0;
}
