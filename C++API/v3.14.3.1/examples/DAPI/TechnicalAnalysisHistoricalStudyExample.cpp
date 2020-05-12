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
** TechnicalAnalysisHistoricalStudyExample.cpp
**
** This Example shows how to use Technical Analysis service ("//blp/tasvc")
** to retrieve Historical data for specified study request.
**
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

#include <time.h>
#include <sstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <stdlib.h>
#include <string.h>

using namespace BloombergLP;
using namespace blpapi;

namespace {
    const Name SECURITY_NAME("securityName");
    const Name STUDY_DATA("studyData");
    const Name RESPONSE_ERROR("responseError");
    const Name SECURITY_ERROR("securityError");
    const Name FIELD_EXCEPTIONS("fieldExceptions");
    const Name FIELD_ID("fieldId");
    const Name ERROR_INFO("errorInfo");
    const Name CATEGORY("category");
    const Name MESSAGE("message");
    const Name SESSION_STARTUP_FAILURE("SessionStartupFailure");
    const Name SESSION_TERMINATED("SessionTerminated");
};

class TechnicalAnalysisHistoricalStudyExample {

    std::string         d_host;
    int                 d_port;

    // Prints Program Usage
    void printUsage()
    {
        std::cout << "Usage:" << std::endl
            << "    Technical Analysis Historical Study Example " << std::endl
            << "        [-ip        <ipAddress  = localhost>" << std::endl
            << "        [-p         <tcpPort    = 8194>" << std::endl;
        std::cout << "Press ENTER to quit" <<std::endl;
    }

    // Parses the command line arguments
    bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) 
                d_host = argv[++i];
            else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) 
                d_port = std::atoi(argv[++i]);
            else { 
                printUsage();
                return false;
            }
        }
        return true;
    }

    // Create Technical Analysis Historical EOD - DMI Study Request
	Request createDMIStudyRequest(Service &tasvcService) 
    {
        Request request = tasvcService.createRequest("studyRequest");

        Element priceSource = request.getElement("priceSource");
        // set security name
        priceSource.setElement("securityName", "IBM US Equity");

        Element dataRange = priceSource.getElement("dataRange");
        dataRange.setChoice("historical");

		Element historical = dataRange.getElement("historical");
        historical.setElement("startDate", "20100501"); // set study start date
        historical.setElement("endDate", "20100528"); // set study start date

        // DMI study example - set study attributes
        Element studyAttributes = request.getElement("studyAttributes");
        studyAttributes.setChoice("dmiStudyAttributes");

        Element dmiStudy = studyAttributes.getElement("dmiStudyAttributes");
        dmiStudy.setElement("period", 14); // DMI study interval
        dmiStudy.setElement("priceSourceHigh", "PX_HIGH");
        dmiStudy.setElement("priceSourceClose", "PX_LAST");
        dmiStudy.setElement("priceSourceLow", "PX_LOW");

        return request;            
    }

    // Create Technical Analysis Historical EOD - BOLL Study Request
    Request createBOLLStudyRequest(Service &tasvcService) 
    {
        Request request = tasvcService.createRequest("studyRequest");

        Element priceSource = request.getElement("priceSource");
        // set security name
        priceSource.setElement("securityName", "AAPL US Equity");

        Element dataRange = priceSource.getElement("dataRange");
        dataRange.setChoice("historical");

		Element historical = dataRange.getElement("historical");
        historical.setElement("startDate", "20100701"); // set study start date
        historical.setElement("endDate", "20100716"); // set study start date

        // BOLL study example - set study attributes
        Element studyAttributes = request.getElement("studyAttributes");
        studyAttributes.setChoice("bollStudyAttributes");

        Element bollStudy = studyAttributes.getElement("bollStudyAttributes");
        bollStudy.setElement("period", 30); // BOLL study interval
        bollStudy.setElement("lowerBand", 2); 
        bollStudy.setElement("priceSourceClose", "PX_LAST"); 
        bollStudy.setElement("upperBand", 4); 

        return request;            
    }

    // Prints Error Information
    void printErrorInfo(const char *leadingStr, const Element &errorInfo)
    {
        std::cout << leadingStr
            << errorInfo.getElementAsString(CATEGORY)
            << " ("
            << errorInfo.getElementAsString(MESSAGE)
            << ")" << std::endl;
    }

    // Function to handle response event
    void processResponseEvent(Event eventObj)
    {
        MessageIterator msgIter(eventObj);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            if (msg.asElement().hasElement(RESPONSE_ERROR)) {
                printErrorInfo("REQUEST FAILED: ", 
                    msg.getElement(RESPONSE_ERROR));
                continue;
            }

            Element security = msg.getElement(SECURITY_NAME);
			std::cout << security << std::endl;
            std::string ticker = security.getValueAsString();
            std::cout << "\nTicker: " + ticker << std::endl;
            if (security.hasElement("securityError")) {
                printErrorInfo("\tSECURITY FAILED: ",
                    security.getElement(SECURITY_ERROR));
                continue;
            }

			// Parse study data received
            const Element fields = msg.getElement(STUDY_DATA);
            if (fields.numValues() > 0) {
                size_t numValues = fields.numValues();
                for (size_t j = 0; j < numValues; ++j) {
                    Element field = fields.getValueAsElement(j);
                    for (size_t k = 0; k < field.numElements(); k++)
                    {
                        Element element = field.getElement(k);
						std::cout << "\t" << element.name() << " = " <<
							element.getValueAsString() << std::endl;
                    }
					std::cout << std::endl;
                }
            }
            std::cout << std::endl;
			// Handle FIELD_EXCEPTIONS if any
            Element fieldExceptions = msg.getElement(FIELD_EXCEPTIONS);
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

    // Polling for the the events
    void eventLoop(Session &session) {
        bool done = false;
        while (!done) {
            Event eventObj = session.nextEvent();
            if (eventObj.eventType() == Event::PARTIAL_RESPONSE) {
                std::cout <<"Processing Partial Response" << std::endl;
                processResponseEvent(eventObj);
            }
            else if (eventObj.eventType() == Event::RESPONSE) {
                std::cout <<"Processing Response" << std::endl;
                processResponseEvent(eventObj);
                done = true;
            } else {
                MessageIterator msgIter(eventObj);
                while (msgIter.next()) {
                    Message msg = msgIter.message();
	                msg.print(std::cout) << std::endl;
                    if (eventObj.eventType() == Event::SESSION_STATUS) {
                        if (msg.messageType() == SESSION_TERMINATED) {
                            done = true;
                        }
                    }
                }
            }
        }
    }

public:

    // Function reads command line arguments, 
    // Establish a Session
    // Identify and Open tasvc Service
    // Sends DMI and BOLL StudyRequest to the Service 
    // Event Loop and Response Processing
    void run(int argc, char **argv) 
	{
        d_host = "localhost";
        d_port = 8194;

        if (!parseCommandLine(argc, argv)) return;

		SessionOptions sessionOptions;
        sessionOptions.setServerHost(d_host.c_str());
        sessionOptions.setServerPort(d_port);

        std::cout <<"Connecting to " << d_host << ":" << d_port << std::endl;
        Session session(sessionOptions);
        if (!session.start()) {
            std::cerr << "Failed to start session." << std::endl;
            return;
        }
        std::cout <<"Connecting successfully" << std::endl;
        if (!session.openService("//blp/tasvc")) {
            std::cerr << "Failed to open //blp/tasvc" << std::endl;
            return;
        }
        std::cout <<"//blp/tasvc opened successfully" << std::endl;

        Service tasvcService = session.getService("//blp/tasvc");
        // Create DMI Study Request
        Request dmiStudyRequest = createDMIStudyRequest(tasvcService);
        std::cout << "Sending Request: " << dmiStudyRequest << std::endl;
        session.sendRequest(dmiStudyRequest);
        // wait for events from session.
        eventLoop(session);

        // Create BOLL Study Request
        Request bollStudyRequest = createBOLLStudyRequest(tasvcService);
        std::cout <<"Sending Request: " << bollStudyRequest << std::endl;
        session.sendRequest(bollStudyRequest);
        // wait for events from session.
        eventLoop(session);

        session.stop();
    }

};

int main(int argc, char **argv)
{
    std::cout << "Technical Analysis Historical Study Example" << std::endl;

    TechnicalAnalysisHistoricalStudyExample example;
    example.run(argc, argv);

    // wait for enter key to exit application
    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}
