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
** PortfolioDataExample.cpp
**  This program shows client how
**    1. To download their portfolio holdings using API.
**    2. To view their portfolio positions as of a specific date in order to see 
**       how current market movements have affected their portfolio's constituent 
**       weights.
**
**    It uses Reference Data Service(//blp/refdata) provided by API.
**    It does following:
**        1. Establishing a session which facilitates connection to the bloomberg 
**           network
**        2. Initiating the Reference Data Service(//blp/refdata) for static data.
**        3. Creating and sending request to the session.  
**            - Creating 'PortfolioDataRequest' request 
**            - Adding porfolio tickers/porfolio fields to request
**            - Sending the request
**        4. Event Handling of the responses received.
**
** * The fields available are
**      - PORTFOLIO_MEMBERS: Returns a list of Bloomberg identifiers representing 
**        the members of a user's custom portfolio.
**      - PORTFOLIO_MPOSITION: Returns a list of Bloomberg identifiers 
**        representing the members of a user's custom portfolio as well as the  
**        position for each security in the user's custom portfolio.
**      - PORTFOLIO_MWEIGHT: Returns a list of Bloomberg identifiers representing 
**        the members of a user's custom portfolio as well as the percentage 
**        weight for each security in the user's custom portfolio.
**      - PORTFOLIO_DATA: Returns a list of the Bloomberg identifiers, positions, 
**        market values, cost, cost date, and cost foreign exchange rate of each 
**        security in a user's custom portfolio. 
** 
** Usage: 
**  Retrieve portfolio data
**      [-s         <security       = UXXXXXXX-X Client>
**      [-f         <field          = PORTFOLIO_DATA>
**      [-o         <Reference Date = 20091101>
**      [-ip        <ipAddress      = localhost>
**      [-p         <tcpPort        = 8194>
** 
** * Note:The user's portfolio is identified by its Portfolio ID, which can be
**        found on the upper right hand corner of the toolbar on the portfolio's 
**        PRTU page. This information can also be accessed historically by using 
**        the REFERENCE_DATE override field and supplying the date in ÅeYYYYMMDD' 
**        format. Run {DOCS #2054005 <GO>} for an example of an API spreadsheet 
**        with the new portfolio fields.
** 
** Example usage:
**    PortfolioDataRequest -h
**       Print the usage for the program on the console
**
**    PortfolioDataRequest
**       Run the program with default values specified for security and fields. 
**       Parses the response of PortfolioDataRequest & 
**       prints the response message on the console. 	   
**
**    PortfolioDataRequest -ip localhost -p 8194 -s "5497224-1 Client" 
**                          -f PORTFOLIO_MEMBERS -f PORTFOLIO_DATA 
**       Download the portfolio holdings
** 
**    PortfolioDataRequest -s "5497224-1 Client" -f PORTFOLIO_MPOSITION -o 20091101
**       Specifying the REFERENCE_DATE override to view portfolio positions
**       as of a specific date
**
**    Program prints the response on the console of the command line requested data
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
    const Name PORTFOLIO_DATA_REQ("PortfolioDataRequest");
};

class PortfolioDataExample 
{
    string         d_host;
    int            d_port;
    vector<string> d_securities;
    vector<string> d_fields;
    string         d_override;

private:

    /**
    * Parses the command line arguments
    */
    bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!strcmp(argv[i],"-s") && (i + 1) < argc) {
                d_securities.push_back(argv[++i]);
            } else if (!strcmp(argv[i],"-f") && (i + 1) < argc) {
                d_fields.push_back(argv[++i]);
            } else if (!strcmp(argv[i],"-o") && (i + 1) < argc) {
                d_override = argv[++i];
            } else if (!strcmp(argv[i],"-ip") && (i + 1) < argc) {
                d_host = argv[++i];
            } else if (!strcmp(argv[i],"-p") &&  (i + 1) < argc) {
                 d_port = atoi(argv[++i]);
            } else {
                printUsage();
                return false;
            }
        }

        // handle default arguments
        if (d_securities.size() == 0) {
            d_securities.push_back("U5497224-1 Client");
        }

        if (d_fields.size() == 0) {
            d_fields.push_back("PORTFOLIO_MEMBERS");
            d_fields.push_back("PORTFOLIO_MPOSITION");
            d_fields.push_back("PORTFOLIO_MWEIGHT");
            d_fields.push_back("PORTFOLIO_DATA");
        }

        return true;
    }

    /**
    * Prints Error Information
    */
   void printErrorInfo(const char *leadingStr, const Element &errorInfo)
    {
        cout << leadingStr
            << errorInfo.getElementAsString(CATEGORY)
            << " ("
            << errorInfo.getElementAsString(MESSAGE)
            << ")" << endl;
    }

   /**
    * Prints Program Usage
    */
    void printUsage()
    {
        cout << "Usage:" << endl
            << "    Retrieve Portfolio data" << endl
            << "      [-s         <security       = \"UXXXXXXX-X Client\">" << endl
            << "      [-f         <field          = PORTFOLIO_DATA>" << endl
            << "      [-o         <Reference Date = 20091101>" << endl
            << "      [-ip        <ipAddress      = localhost>" << endl
            << "      [-p         <tcpPort        = 8194>" << endl
            << "Notes: " << endl
            << "1) Multiple securities & fields can be specified" << endl
            << "2) The user's portfolio is identified by its Portfolio ID, which can be" << endl
            << "   found on the upper right hand corner of the toolbar on the portfolio's" << endl
            << "   PRTU page. This information can also be accessed historically by using" << endl
            << "   the REFERENCE_DATE override field[-o] & supplying the date in 'YYYYMMDD'" << endl
            << "   format." << endl;
    }

   /**
    * Function to send PortfolioDataRequest
    */
    void sendPortfolioDataRequest(Session &session)
    {
        Service refDataService = session.getService("//blp/refdata");
        Request request = refDataService.createRequest("PortfolioDataRequest");

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

        // If specified, use REFERENCE_DATE override field 
        // to get portfolio information historically.
        // The date must be in 'YYYYMMDD' format
        if(d_override.length() != 0){
            Element overrides = request.getElement("overrides");
            Element override1 = overrides.appendElement();
            override1.setElement("fieldId", "REFERENCE_DATE");
            override1.setElement("value", d_override.c_str());
        }

		cout << "Sending Request: " << endl;
		request.print(std::cout);
		
		session.sendRequest(request);
    }

    /**
     * Function to handle response event
     */
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

    /**
     * Read the reference bulk field data
     */
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
                cout << "\t" << elem.name() << " = " 
                     << elem.getValueAsString() << endl;
            }
			cout << endl;
        }
    }

    /**
     * Read the reference field data
     */
    void processRefField(Element reffield)
    {
        cout << reffield.name() << "\t\t" ;
        cout << reffield.getValueAsString() << endl;
    }

    /**
     * Polls for an event or a message in an event loop
     */
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
    PortfolioDataExample()
    {
        d_host = "localhost";
        d_port = 8194;
    }

    // Destructor
    ~PortfolioDataExample()
    {
    }

    // Function reads command line arguments, 
    // Establish a Session
    // Identify and Open refdata Service
    // Send PortfolioDataRequest to the Service 
    // Event Loop and Response Processing
    void run(int argc, char **argv)
    {
        if (!parseCommandLine(argc, argv)) return;

        SessionOptions sessionOptions;
        sessionOptions.setServerHost(d_host.c_str());
        sessionOptions.setServerPort(d_port);

        cout << "Connecting to " + d_host + ":" << d_port << endl;
        Session session(sessionOptions);
        if (!session.start()) {
            cout << "Failed to start session." << endl;
            return;
        }
        if (!session.openService("//blp/refdata")) {
            cout << "Failed to open //blp/refdata" << endl;
            return;
        }      
        sendPortfolioDataRequest(session);

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
    cout << "PortfolioDataExample" << endl;
    PortfolioDataExample example;
    example.run(argc, argv);

    cout << "Press ENTER to quit" << endl;
    char dummy[2];
    cin.getline(dummy, 2);
    return 0;
}