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
    const Name DATA("data");
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
};

class EqsDataExample 
{
    std::string              d_host;
    int                      d_port;
    std::string				 d_screenName;
	std::string				 d_screenType;

    bool parseCommandLine(int argc, char **argv)
    {
		// default screenType to General
        d_screenType = "PRIVATE";

        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-s") && i + 1 < argc) {
                d_screenName = argv[++i];
            } else if (!std::strcmp(argv[i],"-t") && i + 1 < argc) {
				d_screenType = argv[++i];
            } else if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_host = argv[++i];
            } else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_port = std::atoi(argv[++i]);
            } else {
                printUsage();
                return false;
            }
        }

        // handle default arguments
        if (d_screenName.size() == 0) {
                printUsage();
                return false;
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
            << "    Retrieve EQS data " << std::endl
            << "        [-s         <screenName	= S&P500>" << std::endl
            << "        [-t         <screenType	= GLOBAL or PRIVATE>" << std::endl
            << "        [-ip        <ipAddress  = localhost>" << std::endl
            << "        [-p         <tcpPort    = 8194>" << std::endl;
    }

    void sendEqsDataRequest(Session &session)
    {
        Service refDataService = session.getService("//blp/refdata");
        Request request = refDataService.createRequest("BeqsRequest");
		request.set("screenName", d_screenName.c_str());
		request.set("screenType",d_screenType.c_str()); 

		std::cout << "Sending Request: " << request << std::endl;
        session.sendRequest(request);
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

            Element data = msg.getElement(DATA);
            Element securities = data.getElement(SECURITY_DATA);
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
    EqsDataExample()
    {
        d_host = "localhost";
        d_port = 8194;
    }

    ~EqsDataExample()
    {
    }

    void run(int argc, char **argv)
    {
        if (!parseCommandLine(argc, argv)) return;

        SessionOptions sessionOptions;
        sessionOptions.setServerHost(d_host.c_str());
        sessionOptions.setServerPort(d_port);

        std::cout << "Connecting to " + d_host + ":" << d_port << std::endl;
        Session session(sessionOptions);
        if (!session.start()) {
            std::cout << "Failed to start session." << std::endl;
            return;
        }
        if (!session.openService("//blp/refdata")) {
            std::cout << "Failed to open //blp/refdata" << std::endl;
            return;
        }      
        sendEqsDataRequest(session);

        // wait for events from session.
        try {
            eventLoop(session);
        } catch (Exception &e) {
            std::cerr << "Library Exception !!!" << e.description() << std::endl;
        } catch (...) {
            std::cerr << "Unknown Exception !!!" << std::endl;
        }


        session.stop();
    }
};

int main(int argc, char **argv)
{
    std::cout << "EqsDataExample" << std::endl;
    EqsDataExample example;
    example.run(argc, argv);

    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}
