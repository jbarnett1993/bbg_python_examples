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

// *****************************************************************************
// PageDataExample.cpp: 
// This program demonstrates how to make a subscription to Page Based data. 
//   It uses the Market Bar service(//blp/pagedata) 
//	provided by API. Program does the following:
//		1. Establishes a session which facilitates connection to the bloomberg 
//		   network.
//		2. Initiates the Page data Service(//blp/pagedata) for realtime
//		   data.
//		3. Creates and sends the request via the session.
//			- Creates a subscription list
//			- Adds Page data topic to subscription list.
//			- Subscribes to realtime Page data
//		4. Event Handling of the responses received.
//       5. Parsing of the message data.
// Usage: 
//         	-t			<Topic  	= "0708/012/0001">
//                                   i.e."Broker ID/Category/Page Number"
//     		-ip 		<ipAddress	= localhost>
//     		-p 			<tcpPort	= 8194>
//
//   example usage:
//	PageDataExample -t "0708/012/0001" -ip localhost -p 8194
//
// Prints the response on the console of the command line requested data
//******************************************************************************/
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

#include <map>
#include <vector>
#include <string>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <stdio.h>

using namespace BloombergLP;
using namespace blpapi;

namespace {
    Name EXCEPTIONS("exceptions");
    Name FIELD_ID("fieldId");
    Name REASON("reason");
    Name CATEGORY("category");
    Name DESCRIPTION("description");
    Name PAGEUPDATE("PageUpdate");
    Name ROWUPDATE("rowUpdate");
    Name NUMROWS("numRows");
    Name NUMCOLS("numCols");
    Name ROWNUM("rowNum");
    Name SPANUPDATE("spanUpdate");
    Name STARTCOL("startCol");
    Name LENGTH("length");
    Name TEXT_("text");

	typedef std::vector<std::string> Topic;
	typedef std::map<std::string, Topic> TopicMap;
}

class SubscriptionEventHandler: public EventHandler
{
	TopicMap d_topicTable;

    size_t getTimeStamp(char *buffer, size_t bufSize)
    {
        const char *format = "%Y/%m/%d %X";

        time_t now = time(0);
#ifdef WIN32
		tm tmInfo;
		tm *timeInfo = &tmInfo;
		localtime_s(timeInfo, &now);
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

            if (msg.hasElement(REASON)) {
                // This can occur on SubscriptionFailure.
                Element reason = msg.getElement(REASON);
                fprintf(stdout, "        %s: %s\n",
                        reason.getElement(CATEGORY).getValueAsString(),
                        reason.getElement(DESCRIPTION).getValueAsString());
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

			const char* msg_type = msg.messageType().string();
			if (strcmp(msg_type, "PageUpdate") == 0)
			{
				processPageElement(msg.asElement(), *topic);
			} else if (strcmp(msg_type,"RowUpdate") == 0) {
				processRowElement(msg.asElement(), *topic);
			}
        }
        return true;
    }

	void showUpdateedPage(std::string topic)
	{
		std::vector<std::string> rowList = d_topicTable[topic];
	}

	void processPageElement(Element pageElement, std::string topic)
	{
    	Element eleNumRows = pageElement.getElement(NUMROWS);
    	int numRows = eleNumRows.getValueAsInt32();
    	Element eleNumCols = pageElement.getElement(NUMCOLS);
    	int numCols = eleNumCols.getValueAsInt32();
		std::cout << "Page Contains " << numRows << " Rows & " << numCols << " Columns" << std::endl;
    	Element eleRowUpdates = pageElement.getElement(ROWUPDATE);
    	size_t numRowUpdates = eleRowUpdates.numValues(); 
    	for (size_t i = 0; i < numRowUpdates - 1; i++) {
			Element rowUpdate = eleRowUpdates.getValueAsElement(i);
			processRowElement(rowUpdate, topic);
		}
	}

	void processRowElement(Element rowElement, std::string topic)
	{
    	Element eleRowNum = rowElement.getElement(ROWNUM);
    	size_t rowNum = eleRowNum.getValueAsInt32();
    	Element eleSpanUpdates = rowElement.getElement(SPANUPDATE);
    	size_t numSpanUpdates = eleSpanUpdates.numValues();
    	
    	for (size_t i = 0; i < numSpanUpdates; i++) {
			Element spanUpdate = eleSpanUpdates.getValueAsElement(i);
			processSpanElement(spanUpdate, rowNum, topic);
		}
	}

	void processSpanElement(Element spanElement, size_t rowNum, std::string topic)
    {
    	Element eleStartCol = spanElement.getElement(STARTCOL);
    	int startCol = eleStartCol.getValueAsInt32();
    	Element eleLength = spanElement.getElement(LENGTH);
    	int len = eleLength.getValueAsInt32();
    	Element eleText = spanElement.getElement(TEXT_);
		std::string text = eleText.getValueAsString();
		fprintf(stdout, "Row : %d, Col : %d, (Len : %d New Text : %s\n",
            rowNum, startCol, len, text.c_str());

		Topic *rowList = &d_topicTable[topic]; //it->second;
    	while (rowList->size() < rowNum)
    	{
			std::string row;
    		rowList->push_back(row);
    	}
    	
		std::string *rowText = &rowList->at(rowNum - 1);
    	if (rowText->length() == 0) {
    		rowText->append(text.insert(0, 80 - text.length(), ' '));
    	} else {
    		rowText->replace(startCol - 1, len, text);
			std::cout << rowText->c_str() << std::endl;
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
	SubscriptionEventHandler(TopicMap topicTable) : d_topicTable()
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

class PageDataExample
{

    SessionOptions               d_sessionOptions;
    Session                     *d_session;
    SubscriptionEventHandler    *d_eventHandler;
	TopicMap d_topicTable;
    Topic     d_topics;
    SubscriptionList             d_subscriptions; 

    bool createSession() { 
        fprintf(stdout, "Connecting to %s:%d\n",
                d_sessionOptions.serverHost(),
                d_sessionOptions.serverPort());

		d_eventHandler = new SubscriptionEventHandler(d_topicTable);
        d_session = new Session(d_sessionOptions, d_eventHandler);

        if (!d_session->start()) {
            fprintf(stderr, "Failed to start session\n");
            return false;
        }

        fprintf(stdout, "Connected successfully\n");

        if (!d_session->openService("//blp/pagedata")) {
            fprintf(stderr, "Failed to open service //blp/pagedata\n");
            d_session->stop();
            return false;
        }

        fprintf(stdout, "Subscribing...\n");
        subscribe();

        return true;
    }   

	void subscribe(){
		std::string topic;
		std::string service;
		std::vector<std::string> fields;
		std::vector<std::string> options;
		d_topicTable.clear();
		fields.push_back("6-23");
        // Following commented code shows some of the sample values 
        // that can be used for field other than above
        // e.g. fields.Add("1");
        //      fields.Add("1,2,3");
        //      fields.Add("1,6-10,15,16");
		
        for (size_t i = 0; i < d_topics.size(); ++i) {
			topic = d_topics[i];
			service = "//blp/pagedata/";
			d_subscriptions.add(service.append(topic).c_str(),
				fields, options, CorrelationId(&d_topics[i]));
			std::vector<std::string> topicItem;
			d_topicTable[topic] = topicItem;
		}
		d_session->subscribe(d_subscriptions);
	}

    bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-t") && i + 1 < argc) {
                d_topics.push_back(argv[++i]);
            } else if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_sessionOptions.setServerHost(argv[++i]);
            } else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_sessionOptions.setServerPort(std::atoi(argv[++i]));
            } else {
				printUsage();
				return false;
			}
        }

        if (d_topics.size() == 0) {
            d_topics.push_back("0708/012/0001");
            d_topics.push_back("1102/1/274");
        }

        return true;
    }

    void printUsage()
    {
        const char *usage = 
            "Usage:\n"
            "    Retrieve realtime page data\n"
            "        [-t			<Topic	= 0708/012/0001>\n"
            "        [			i.e.\"Broker ID/Category/Page Number\"\n"
            "        [-ip   <ipAddress  = localhost>\n"
            "        [-p    <tcpPort    = 8194>\n"
			"e.g. PageDataExample -t \"0708/012/0001\" -ip localhost -p 8194\n";
        fprintf(stdout, "%s\n", usage);
    }

public:

    PageDataExample()
    : d_session(0)
    , d_eventHandler(0)
    {
        d_sessionOptions.setServerHost("localhost");
        d_sessionOptions.setServerPort(8194);
    }

    ~PageDataExample()
    {
        if (d_session) delete d_session;
        if (d_eventHandler) delete d_eventHandler ;
    }

    void run(int argc, char **argv)
    {
        if (!parseCommandLine(argc, argv)) return;
        if (!createSession()) return;

        // wait for enter key to exit application
        fprintf(stdout, "Press ENTER to quit\n\n");
        getchar();

        d_session->stop();
        fprintf(stdout, "Exiting...\n");
    }
};

int main(int argc, char **argv)
{
    setvbuf(stdout, NULL, _IONBF, BUFSIZ);
    fprintf(stdout, "PageDataExample\n");
    PageDataExample example;
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
