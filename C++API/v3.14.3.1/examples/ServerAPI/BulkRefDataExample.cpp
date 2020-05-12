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
** BulkRefDataExample.cpp
**
** This Example shows how to Retrieve reference data/Bulk reference data 
** using Server Api
** Usage: 
**      		-s			<security	= CAC Index>
**      		-f			<field		= INDX_MWEIGHT>
**      		-ip 		<ipAddress	= localhost>
**      		-p 			<tcpPort	= 8194>
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
};

class BulkRefDataExample 
{
    string d_host;
    int			d_port;
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
                d_host = argv[++i];
            } else if (!strcmp(argv[i],"-p") &&  i + 1 < argc) {
                 d_port = atoi(argv[++i]);
            } else {
                printUsage();
                return false;
            }
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
            << "      [-p         <tcpPort   = 8194>" << endl;
    }

    // Function to send Reference data request
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

        cout << "Sending Request: " << request << endl;
        session.sendRequest(request);
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
        d_host = "localhost";
        d_port = 8194;
    }

    // Destructor
    ~BulkRefDataExample()
    {
    }

    // Function reads command line arguments, 
    // Establish a Session
    // Identify and Open refdata Service
    // Send ReferenceDataRequest to the Service 
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