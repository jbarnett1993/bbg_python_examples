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
//         	-t	   <Topic  	= "0708/012/0001">
//                        i.e."Broker ID/Category/Page Number"
//     		-ip    <ipAddress	= localhost>
//     		-p 	   <tcpPort	= 8194>
//          -auth  <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>
//          -n     <name = applicationName or directoryService>
//  Notes:
//    -Specify only LOGON to authorize 'user' using Windows login name.
//    -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.
//    -Specify APPLICATION and name(Application Name) to authorize application.
//
//  example usage:
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
    const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
    const Name AUTHORIZATION_FAILURE("AuthorizationFailure");
    const Name TOKEN_SUCCESS("TokenGenerationSuccess");
    const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name TOKEN("token");

	const char *AUTH_SVC = "//blp/apiauth";

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
		Topic rowList = d_topicTable[topic];
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
	std::vector<std::string>	 d_hosts;		// IP Addresses of appliances
    int							 d_port;
	std::string					 d_authOption;	// authentication option user/application
	std::string					 d_name;	        // DirectoryService/ApplicationName
    Identity					 d_identity;
    SessionOptions               d_sessionOptions;
    Session                     *d_session;
    SubscriptionEventHandler    *d_eventHandler;
	TopicMap					 d_topicTable;
	Topic						 d_topics;
    SubscriptionList             d_subscriptions; 
	std::string				     d_service;

	bool createSession() { 
		std::cout << "Connecting to port " << d_port << " on server: "; 
        for (size_t i = 0; i < d_hosts.size(); ++i) {
            d_sessionOptions.setServerAddress(d_hosts[i].c_str(), d_port, i);
			std::cout << d_hosts[i].c_str();
        }
		std::cout << std::endl;
        d_sessionOptions.setAutoRestartOnDisconnection(true);
        d_sessionOptions.setNumStartAttempts((int)d_hosts.size());
		
		if (d_service.size() > 0) {
			// change subscription service
			d_sessionOptions.setDefaultSubscriptionService(d_service.c_str());
		}

		std::string authOptions = getAuthOptions();
		if (authOptions.size() > 0) {
			d_sessionOptions.setAuthenticationOptions(authOptions.c_str());
		}
		d_eventHandler = new SubscriptionEventHandler(d_topicTable);
        d_session = new Session(d_sessionOptions, d_eventHandler);

        if (!d_session->start()) {
            fprintf(stderr, "Failed to start session\n");
            return false;
        }

        fprintf(stdout, "Connected successfully\n");

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
		if (strcmp(d_authOption.c_str(), "NONE")) {
			// subscribe with Identity
			d_session->subscribe(d_subscriptions, d_identity);
		}
		else
		{
			d_session->subscribe(d_subscriptions);
		}
	}

    bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 0; i < argc; ++i) {
            if (i == 0) continue; // ignore the program name.

            if (!std::strcmp(argv[i],"-t") && i + 1 < argc) {
                d_topics.push_back(argv[++i]);
                continue;
            }
            if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
				d_hosts.push_back(argv[++i]);
                continue;
            }
            if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_port = std::atoi(argv[++i]);
                continue;
            }
			if (!std::strcmp(argv[i],"-auth") &&  i + 1 < argc) {
				d_authOption = argv[++i];
				continue;
			}
			if (!std::strcmp(argv[i],"-n") &&  i + 1 < argc) {
                d_name = argv[++i];
				continue;
			}
            printUsage();
            return false;
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
            "        [-t    <Topic	= 0708/012/0001>\n"
            "        [          i.e.\"Broker ID/Category/Page Number\"\n"
            "        [-ip   <ipAddress  = localhost>\n"
            "        [-p    <tcpPort    = 8194>\n"
			"        [-auth  <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]\n"
			"        [-n     <name = applicationName or directoryService>]\n"
            "Notes:\n"
            " -Specify only LOGON to authorize 'user' using Windows login name.\n"
            " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.\n"
            " -Specify APPLICATION and name(Application Name) to authorize application.\n";
			"e.g. PageDataExample -t \"0708/012/0001\" -ip localhost -p 8194\n";
        fprintf(stdout, "%s\n", usage);
    }

public:

    PageDataExample()
    : d_session(0)
    , d_eventHandler(0)
    {
        d_port = 8194;
		d_name = "";
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

		if (strcmp(d_authOption.c_str(), "NONE")) {
			if (!authorize(d_session)) {
				return;
			}
		}
		
		subscribe();

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
