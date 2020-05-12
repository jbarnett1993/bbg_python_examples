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

/*****************************************************************************************************
 PagedataInteractivePublisherExample.cpp: 
	This program will demonstrate interactive page data publishing. This application will publishes page 
	data for a topic only when there are consumers(subscribers). To have 
	subscriber to the topic, please start application "PageDataSubscriberExample" or subscribe using container. 
	The Bloomberg infrastructure sends event to this application only when the data is needed for a topic. 
	The data published by this application is just random value and is published 
	every two seconds.
	It does following:
		1. Establishing a session(ProviderSession) for publication of data to the bloomberg 
		   network.
		2. Obtaining authorization for the publishing identity(user/application) using authorization 
		   request
		    - Generate the token for identity(user/application)
		    - Initiating the api authorization Service (//blp//apiauth) 
			- Authorize the user/application using authorisation request with token set.
		3. Register the service for the identity
		   - Set the service registeration options (groupID and priority) 
		   - Register to provide a Service
		3. Create the topic on the session to be published. 
			- Add topic to the topicList 
			- Create topics on the session
		4. Publishing events for the active topics of the designated service.
 Usage: 
    PagedataInteractivePublisherExample -help 
	PagedataInteractivePublisherExample -?
	   Print the usage for the program on the console

	PagedataInteractivePublisherExample
	   Run the program with default values. Publish random value for page data on the console
	   every two seconds for subscribed topic.

  Example usage:
	PagedataInteractivePublisherExample -ip <appliance IP> -p <port no> -s <service name> -g <GroupID> -pri <priority>
	   Prints the response on the console of the command line requested data

************************************************************************************************************/

#include "BlpThreadUtil.h"
#include <blpapi_resolutionlist.h>
#include <blpapi_providersession.h>
#include <blpapi_eventdispatcher.h>

#include <blpapi_event.h>
#include <blpapi_message.h>
#include <blpapi_element.h>
#include <blpapi_name.h>
#include <blpapi_defs.h>
#include <blpapi_exception.h>
#include <blpapi_topic.h>
#include <blpapi_eventformatter.h>

#include <iostream>
#include <sstream>
#include <map>
#include <vector>
#include <string>
#include <list>
#include <time.h>
#include <iterator>

#ifndef WIN32
#include <unistd.h>
#define SLEEP(s) sleep(s)
#else 
#define SLEEP(s) Sleep(s * 1000)
#define sleep(s) Sleep(s)
#endif // WIN32

using namespace BloombergLP;
using namespace blpapi;

namespace {
	// Names used for accessing elements
	const Name SERVICE_REGISTERED("ServiceRegistered");
	const Name SERVICE_REGISTER_FAILURE("ServiceRegisterFailure");
	const Name RESOLUTION_SUCCESS("ResolutionSuccess");
	const Name RESOLUTION_FAILURE("ResolutionFailure");
    const Name TOKEN_SUCCESS("TokenGenerationSuccess");
    const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name PERMISSION_REQUEST("PermissionRequest");
    const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
	const Name AUTHORIZATION_FAILURE("AuthorizationFailure");
	const Name TOPIC_SUBSCRIBED("TopicSubscribed");
    const Name TOPIC_UNSUBSCRIBED("TopicUnsubscribed");
    const Name TOPIC_RECAP("TopicRecap");
    const Name TOKEN("token");
	const Name SERVICE_NAME("serviceName");
	const Name SESSION_TERMINATED("SessionTerminated");
	const Name TOPIC("topic");
	const Name TOPIC_CREATED("TopicCreated");

	const std::string AUTH_USER       = "AuthenticationType=OS_LOGON";
	const std::string AUTH_APP_PREFIX = "AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
	const std::string AUTH_USER_APP_PREFIX = "AuthenticationMode=USER_AND_APPLICATION;AuthenticationType=OS_LOGON;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
	const std::string AUTH_DIR_PREFIX = "AuthenticationType=DIRECTORY_SERVICE;DirSvcPropertyName=";
	const char* AUTH_OPTION_NONE      = "none";
	const char* AUTH_OPTION_USER      = "user";
	const char* AUTH_OPTION_APP       = "app=";
	const char* AUTH_OPTION_USER_APP  = "userapp=";
	const char* AUTH_OPTION_DIR       = "dir=";
	
	bool g_running = true;
	unsigned long g_pubCount = 0;

	// page constants
	static const int pageRows = 25;		 // number of rows per page
	static const int pageColumns = 80;	 // number of columns
	static const int dataColumns = 7;	 // number of data colums per row
	static const int maxAttributes = 12; // max. number for attribute random number generator  

/**
 * A class for topic used for publishing
 */

	char timebuffer [80];  // allocating here, so we don't have expense in function
std::string getTimeStr()
{
	time_t rawtime;
	struct tm * timeinfo;
	time ( &rawtime );
	timeinfo = localtime ( &rawtime );
	strftime (timebuffer,80,"%Y%m%d %H:%M:%S|",timeinfo);
	return timebuffer;

   //time_t t;
   //time(&t);
   //std::string ret(ctime(&t));
   //return ret.substr(0, ret.size()-1);  // get rid of terminating '\n'
}


class  MyStream{
    const std::string d_id;  
	volatile bool d_isInitialPaintSent;

	Topic d_topic;
    bool  d_isSubscribed;

	float pageData[pageRows][dataColumns];
public:
	MyStream() : d_id(""),
          d_isInitialPaintSent(false)
	      {initPageData();};

    MyStream(std::string const& id) 
		:	d_id(id),
			d_isInitialPaintSent(false) 
        {initPageData();}

    std::string const& getId()
    {
        return d_id;
    }

	bool isInitialPaintSent() const { return d_isInitialPaintSent; }
    void setIsInitialPaintSent(bool value) { d_isInitialPaintSent = value; }

	void setTopic(Topic topic) {
        d_topic = topic;
    }

    void setSubscribedState(bool isSubscribed) {
        d_isSubscribed = isSubscribed;
    }

    Topic& topic() {
        return d_topic;
    }

    bool isAvailable() {
        return d_topic.isValid() && d_isSubscribed;
    }


	float getPageData(int row, int col) { return pageData[row][col]; }
	float setPageData(int row, int col) { pageData[row][col] = generatePrice(); return pageData[row][col];}

	
	/*****************************************************************************
	Function    : initPageData
	Description : This function initialize the page data with random prices. 
	*****************************************************************************/
	void initPageData()
	{
		int i,j;
		seedRand();
		for (i=0; i<pageRows; i++)
		{
			for (j=0; j<dataColumns; j++)
			{
				pageData[i][j] = generatePrice();
			}
		}
	}

	/*****************************************************************************
	Function    : getRow
	Description : This function return requested row data for page
	*****************************************************************************/
	std::string getRow(int row)
	{
		std::string rowData;
		int i;
		std::stringstream temp;
		char buffer[81];
		sprintf_s(buffer, 81,"Row %02d:  ",row +1);

		temp << buffer; 
		for (i=0; i < dataColumns; i++)
		{
			sprintf_s(buffer, 81, "%10.2f",pageData[row][i]);
			temp << buffer;
		}
		return temp.str();
	}

	/*****************************************************************************
	Function    : seedRand
	Description : This function seed the radom number generator for price
	*****************************************************************************/
	void seedRand()
	{
		// seed rand
		srand((int)time(NULL));
	}

	/*****************************************************************************
	Function    : generatePrice
	Description : This function gernerate random price
	*****************************************************************************/
	float generatePrice()
	{
		// seed 
		return ((float)(rand())) / 100 ;
	}
	
	/*****************************************************************************
	Function    : generateNumber
	Description : This function gernerate random number. Use for generating 
				  random row and column
	*****************************************************************************/
	int generateNumber(int maxValue)
	{
		return rand() % maxValue;
	}
};

typedef std::map<std::string, MyStream*> MyStreams;
MyStreams		 g_streams; //Active publication map
int g_availableTopicCount;
Mutex     g_mutex;

} // namespace {
#ifdef _WIN32 
DWORD WINAPI ThreadFunc(LPVOID arg);
#else
void* printerThread(void *arg);
#endif

// implementing provider event handler interface
class MyEventHandler : public ProviderEventHandler {
	const std::string d_serviceName;

public:
	MyEventHandler(const std::string& serviceName) : d_serviceName(serviceName) {}

	/*****************************************************************************
	Function    : processMiscEvents
	Description : This function handles events other than service status and 
				  resolution status event. This function gets the messages from
				  the event and print them on the console. 
	*****************************************************************************/
	bool processMiscEvents(const Event &event){
		MessageIterator msgIter(event);
		while (msgIter.next()) {
			Message msg = msgIter.message();
			msg.print(std::cout);
		}
		return true;
	}

	/*****************************************************************************
	Function    : processResolutionStatus
	Description : This function handles resolution status event.
				  This function reads messages in the event element and prints 
				  them on the console.
	*****************************************************************************/
	bool processResolutionStatus(const Event &event, ProviderSession* session){		
		MessageIterator msgIter(event);
		while (msgIter.next()) {
			Message msg = msgIter.message();
			msg.print(std::cout);
		}

		return true;
	}

	/**********************************************************************************************
	Function    : processTopicStatus
	Description : This function handles all event related to topic status.
				  This function reads messages in the event element and update the active publication
				  map based on the topic status (subscribed, unsubscribed, recap)
	***********************************************************************************************/
	bool processTopicStatus(const Event &event, ProviderSession* session){
		ResolutionList			 resolutionList;
//		DWORD					 waitResult; 

		TopicList topicList;
		MessageIterator msgIter(event);
		while (msgIter.next()) {
			Message msg = msgIter.message();			
			msg.print(std::cout);			
			if (msg.messageType() == TOPIC_SUBSCRIBED) {
                std::string topicStr = msg.getElementAsString("topic");
                MutexGuard guard(&g_mutex);
                MyStreams::iterator it = g_streams.find(topicStr);
                if (it == g_streams.end()) {
                    // TopicList knows how to add an entry based on a
                    // TOPIC_SUBSCRIBED message.
                    topicList.add(msg);
                    it = (g_streams.insert(MyStreams::value_type(
                                     topicStr,
                                     new MyStream(topicStr)))).first;
                }
                it->second->setSubscribedState(true);
                if (it->second->isAvailable()) {
                    ++g_availableTopicCount;
                }
			} else if (msg.messageType() == TOPIC_UNSUBSCRIBED) {
				std::string topicStr = msg.getElementAsString("topic");
                MutexGuard guard(&g_mutex);
                MyStreams::iterator it = g_streams.find(topicStr);
                if (it == g_streams.end()) {
                    // we should never be coming here. TOPIC_UNSUBSCRIBED can
                    // not come before a TOPIC_SUBSCRIBED or TOPIC_CREATED
                    continue;
                }
                if (it->second->isAvailable()) {
                    --g_availableTopicCount;
                }
                it->second->setSubscribedState(false);


			} else if (msg.messageType() == TOPIC_CREATED) {				
				std::string topicStr = msg.getElementAsString("topic");
				std::cout << "creating topic " << topicStr << std::endl;
                MutexGuard guard(&g_mutex);
                MyStreams::iterator it = g_streams.find(topicStr);
                if (it == g_streams.end()) {
                    it = (g_streams.insert(MyStreams::value_type(
                                     topicStr,
                                     new MyStream(topicStr)))).first;
                }
                try {
                    Topic topic = session->getTopic(msg);
                    it->second->setTopic(topic);
                } catch (blpapi::Exception &e) {
                    std::cerr
                        << "Exception in Session::getTopic(): "
                        << e.description()
                        << std::endl;
                    continue;
                }
                if (it->second->isAvailable()) {
                    ++g_availableTopicCount;
                }
			}else if (msg.messageType()==TOPIC_RECAP) {	
				try {
                    std::string topicStr = msg.getElementAsString("topic");
                    MyStreams::iterator iter = g_streams.find(topicStr);
                    MutexGuard guard(&g_mutex);
                    if (iter == g_streams.end() || !iter->second->isAvailable()) {
                        continue;
                    }
                    Topic topic = session->getTopic(msg);
                    Service service = topic.service();
                    CorrelationId recapCid = msg.correlationId();

					// Create an event suitable for publishing to this service
					Event recapEvent = service.createPublishEvent();
					// Create event formatter for creating the event for publishing
					EventFormatter eventFormatter(recapEvent);
					// Create publishing event for the topic
					// Append initial paint data for the topic
					eventFormatter.appendRecapMessage(topic, &recapCid);
					eventFormatter.setElement("numRows", pageRows);
					eventFormatter.setElement("numCols", pageColumns);


					// push rowUpdate element
					eventFormatter.pushElement("rowUpdate");

					for (int i = 0; i < pageRows; i++) {
						// append element to rowUpdate
						eventFormatter.appendElement();
						eventFormatter.setElement("rowNum", i+1);
						// push spanUpdate element
						eventFormatter.pushElement("spanUpdate");
						// append element to spanUpdate
						eventFormatter.appendElement();
						eventFormatter.setElement("startCol", 1);
						eventFormatter.setElement("length", pageColumns);
						eventFormatter.setElement("text", iter->second->getRow(i).c_str());
						eventFormatter.setElement("fgColor", "WHITE");
						// pop appended element to spanUpdate
						eventFormatter.popElement(); 
						// pop pushed spanUpdate element
						eventFormatter.popElement();
						// pop appended element to rowUpdate
						eventFormatter.popElement();
					}
                    eventFormatter.popElement();
                    guard.release()->unlock();
					//std::cout << "about to publish the recap event" << std::endl;
                    session->publish(recapEvent);
					//std::cout << "published it" << std::endl;
                } catch (blpapi::Exception &e) {
                    std::cerr
                        << "Exception in Session::getTopic(): "
                        << e.description()
                        << std::endl;
                    continue;
                }
      
			}
			else {
				std::cout << msg << std::endl;
				//unknown message
			}
		}
		
		if (topicList.size() > 0) {
			// createTopicsAsync will result in RESOLUTION_STATUS, TOPIC_CREATED events.
            session->createTopicsAsync(topicList);
        }
		return true;
	}

	/*****************************************************************************
	Function    : processServiceStatus
	Description : This function handles all event related to service status.
				  This function reads messages in the event
				  element and prints them on the console.
	*****************************************************************************/
	bool processServiceStatus(const Event &event){
		MessageIterator msgIter(event);
		while (msgIter.next()) {
			Message msg = msgIter.message();
			msg.print(std::cout);
			if (msg.messageType() == SERVICE_REGISTERED) {
				if (msg.hasElement(SERVICE_NAME, true)) 
				std::cout << "Successfully registered \'" << msg.getElement(SERVICE_NAME).getValueAsString() 
						  << "\' service for publishing " << std::endl
						  << std::endl;
			} else if (msg.messageType() == SERVICE_REGISTER_FAILURE) {
				if (msg.hasElement(SERVICE_NAME, true)) 
				std::cout << "Failed to register \'" << msg.getElement(SERVICE_NAME).getValueAsString() 
						  << "\' service for publishing " << std::endl
						  << std::endl;
			} else {
				//unknown service status message
			}
		}
		return true;
	}

	/*****************************************************************************
	Function    : processSessionStatus
	Description : This function handles all event related to session status.
				  This function reads messages in the event
				  element and prints them on the console.
	*****************************************************************************/
	bool processSessionStatus(const Event &event){
		MessageIterator msgIter(event);
		while (msgIter.next()) {
			Message msg = msgIter.message();
			msg.print(std::cout);
			if (msg.messageType() == SESSION_TERMINATED) {
				
				std::cout << "Session terminated event recieved. Stopping Publishing..." << std::endl;
				g_running = false;		  
			} 
		}
		return true;
	}

	/*****************************************************************************
	Function    : processRequestStatus
	Description : This function handles all event related to permissioning request.
				  This function reads messages in the event
				  element and create the permission reesponse to provide access/deny
				  for each topic in the request.
	*****************************************************************************/
	bool processRequestStatus(const Event &event, ProviderSession* session){
		Service service = session->getService(d_serviceName.c_str());
        MessageIterator iter(event);
        while (iter.next()) {
            Message msg = iter.message();
            msg.print(std::cout);
            if (msg.messageType() == PERMISSION_REQUEST) {
                // Create permission response event. A responseEvent can only be
                // for single request so we can specify the
                // correlationId - which establishes context -
                // when we create the Event.
                Event response = service.createResponseEvent(msg.correlationId());
                int permission = 1; // ALLOWED: 0, DENIED: 1
                EventFormatter ef(response);
                if (msg.hasElement("uuid")) {
                    int uuid = msg.getElementAsInt32("uuid");
                    permission = 0;
                }
                if (msg.hasElement("applicationId")) {
                    int applicationId = msg.getElementAsInt32("applicationId");
                    permission = 0;
                }
                if (msg.hasElement("seatType")) {
                    int seatType = msg.getElementAsInt32("seatType");
                    if (seatType == Identity::INVALID_SEAT) {
                        permission = 1;
                    }
                    else {
                        permission = 0;
                    }
                }

                // In appendResponse the string is the name of the
                // operation, the correlationId indicates
                // which request we are responding to.
                ef.appendResponse("PermissionResponse");
                ef.pushElement("topicPermissions");
                // For each of the topics in the request, add an entry
                // to the response
                Element topicsElement = msg.getElement("topics");
                for (size_t i = 0; i < topicsElement.numValues(); ++i) {
                    ef.appendElement();
                    ef.setElement("topic", topicsElement.getValueAsString(i));
                    ef.setElement("result", permission); //ALLOWED: 0, DENIED: 1
                    if (permission == 1) {// DENIED
                        ef.pushElement("reason");
                        ef.setElement("source", "My Publisher Name");
                        ef.setElement("category", "NOT_AUTHORIZED"); // or BAD_TOPIC, or custom
                        ef.setElement("subcategory", "Publisher Controlled");
                        ef.setElement("description",
                            "Permission denied by My Publisher Name");
                        ef.popElement();
                    }
                    ef.popElement();
                }
                ef.popElement();
                // Service is implicit in the Event. sendResponse has a
                // second parameter - partialResponse -
                // that defaults to false.
                session->sendResponse(response);
            }
		}
		return true;
	}
    bool processEvent(const Event& event, ProviderSession* session);

};
/****************************************************************************************
Function    : processEvent
Description : This function is an Event Handler to process events generated on the session
******************************************************************************************/
bool MyEventHandler::processEvent(const Event& event, ProviderSession* session) {
	switch (event.eventType())
	{
		// Process events SERVICE_STATUS
		case Event::TOPIC_STATUS:
			std::cout << "Processing topic status event... " << std::endl;
			return processTopicStatus(event, session);
			break;
		case Event::SERVICE_STATUS:
			// Process events SERVICE_STATUS
			std::cout << "Processing service status event... " << std::endl;
			return processServiceStatus(event);
			break;
		case Event::RESOLUTION_STATUS:
			// Process events RESOLUTION_STATUS
			std::cout << "Processing resolution status event... " << std::endl;
			return processResolutionStatus(event, session);
			break;
		case Event::SESSION_STATUS:
			// Process events SESSION_STATUS
			std::cout << "Processing session status event... " << std::endl;
			return processSessionStatus(event);
			break;
		case Event::REQUEST:
			// Process events REQUEST - permissioning request
			std::cout << "Processing Permissioning request event... " << std::endl;
			return processRequestStatus(event, session);
			break;
		default:
			//Process other events 
			return processMiscEvents(event);
			break;
		}        
    return true;
}
/**************************************************************************************
  Class for demonstrating market data interactive publishing.
*************************************************************************************/
class PagedataInteractivePublisherExample
{
private:
    std::vector<std::string> d_hosts;			// IP Address of the appliances
    int                      d_port;			// port number 
    std::string              d_service;			// service for publishing the topic
	std::string              d_authOptions;		// authentication option user/application
	std::string              d_name;	        // DirectoryService/ApplicationName
	bool					 d_registerServiceResponse; //register service
	std::string              d_topic;			// topic on which data to be published
	int                      d_priority;        // priority of this publisher app
	std::string              d_groupId;         // Group ID for publisher
	int                      d_sleepTime;       // amount of time to sleep between publishings
	int                      d_numPerInterval;  // num messages between each sleep

	ProviderSession			 *providerSession;	// session
	Identity				 providerIdentity;  // publishing identity
	DWORD threadID_;
	HANDLE handle_;

public:
	PagedataInteractivePublisherExample()
		: d_port(8196)
        , d_service("//viper/page")
        , d_authOptions(AUTH_USER)
        , d_priority(10)
		, d_name("")
		, d_sleepTime(500)
		, d_numPerInterval(1)

    {
    }

    /*******************************************************************************
	Function    : printUsage
	Description : This function prints the usage of the program on command line.
	******************************************************************************/
    void printUsage()
    {
		std::cout
            << "Publish on a topic. " << std::endl
            << "Usage:" << std::endl
            << "\t[-ip   <ipAddress>]  \tserver name or IP (default: localhost)" << std::endl
            << "\t[-p    <tcpPort>]    \tserver port (default: 8194)" << std::endl
            << "\t[-s    <service>]    \tservice name (default: //viper/page)" << std::endl
            << "\t[-g    <groupId>]    \tpublisher groupId (defaults to unique value)" << std::endl
            << "\t[-pri  <priority>]   \tset publisher priority level (default: 10)" << std::endl
			<< "\t[-auth <option>]     \tauthentication option: " << std::endl
			<< "\t                     \tuser|none|app=<app>|userapp=<app>|dir=<property> (default: user)" << std::endl
			<< "\t[-sleep <num>]       \tNumber of ms to wait between each publish (0 for no delay)" << std::endl
			<< "\t                     \tDue to different system limitations, there" << std::endl
			<< "\t                     \ta minimum period that may not be possible" << std::endl
			<< "\t[-numperperiod <num] \tnumber of updates to send between each sleep" << std::endl;
    }

	/***********************************************************************************
	Function    : parseCommandLine
	Description : This function parses the command line arguments.If the command
				  line argument are not provided properly, it print the usage on 
				  commandline. 
	*********************************************************************************/    
	bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i], "-ip") && i + 1 < argc)
                d_hosts.push_back(argv[++i]);
            else if (!std::strcmp(argv[i], "-p") &&  i + 1 < argc)
                d_port = std::atoi(argv[++i]);
            else if (!std::strcmp(argv[i], "-s") &&  i + 1 < argc)
                d_service = argv[++i];
			else if (!std::strcmp(argv[i], "-sleep") && i+1 < argc)
				d_sleepTime = atoi(argv[++i]);
			else if (!std::strcmp(argv[i], "-numperperiod") && i+1 < argc)
				d_numPerInterval = atoi(argv[++i]);
            else if (!std::strcmp(argv[i],"-g") && i + 1 < argc)
                d_groupId = argv[++i];
            else if (!std::strcmp(argv[i],"-pri") && i + 1 < argc)
                d_priority = std::atoi(argv[++i]);
            else if (!std::strcmp(argv[i], "-auth") && i + 1 < argc) {
                ++ i;
                if (!std::strcmp(argv[i], AUTH_OPTION_NONE)) {
                    d_authOptions.clear();
                }
                else if (strncmp(argv[i], AUTH_OPTION_APP, strlen(AUTH_OPTION_APP)) == 0) {
                    d_authOptions.clear();
                    d_authOptions.append(AUTH_APP_PREFIX);
                    d_authOptions.append(argv[i] + strlen(AUTH_OPTION_APP));
                }
                else if (strncmp(argv[i], AUTH_OPTION_USER_APP, strlen(AUTH_OPTION_USER_APP)) == 0) {
                    d_authOptions.clear();
                    d_authOptions.append(AUTH_USER_APP_PREFIX);
                    d_authOptions.append(argv[i] + strlen(AUTH_OPTION_USER_APP));
                }
                else if (strncmp(argv[i], AUTH_OPTION_DIR, strlen(AUTH_OPTION_DIR)) == 0) {
                    d_authOptions.clear();
                    d_authOptions.append(AUTH_DIR_PREFIX);
                    d_authOptions.append(argv[i] + strlen(AUTH_OPTION_DIR));
                }
                else if (!std::strcmp(argv[i], AUTH_OPTION_USER)) {
                    d_authOptions.assign(AUTH_USER);
                }
                else {
                    printUsage();
                    return false;
                }
            }
            else {
                printUsage();
                return false;
            }
        }

        if (d_hosts.size() == 0) {
            d_hosts.push_back("localhost");
        }

        return true;
    }

	/**********************************************************************************************
	 * Function    : publish                                                                                     
	 * Description : This function publish page data for topics in the active resolution 
					 list. The data published is just a random value that is publised every 
					 two seconds. 
	 **********************************************************************************************/
	void publish(ProviderSession *session, Identity *identity) {	
		Event event;
		int value=1;
		// get handle for the publishing service on which the topic will be published
		Service service = providerSession->getService(d_service.c_str());

		while (g_running) 
		{			
			MutexGuard guard(&g_mutex);
			if (0 == g_availableTopicCount) {
				guard.release()->unlock();
				SLEEP(1);																
				continue;
			}

			// Now we will start publishing
			//std::cout << "Publishing now..." << std::endl;
			// Create an event suitable for publishing to this service.
			// Event can be created based on the service schema.
			//Messages and element are added based on the schema 
			//event = service.createPublishEvent();
			// Create event formatter for creating the event for publishing
			//EventFormatter eventFormatter(event);

			// Create publishing event for each resolved topic
			for (MyStreams::iterator iter = g_streams.begin();
				 iter != g_streams.end(); ++iter)
			{
				if (!iter->second->isAvailable()) {
					continue;
				}
				event = service.createPublishEvent();
				EventFormatter eventFormatter(event);

				//Send initial paint for the page
				if (!iter->second->isInitialPaintSent()) {
					// Append initial paint page data for the topic
					// on the service if one is not sent and set the 
					// initial paint flag to true
					eventFormatter.appendRecapMessage(iter->second->topic());
					eventFormatter.setElement("numRows", pageRows);
					eventFormatter.setElement("numCols", pageColumns);
					// push rowUpdate element
					eventFormatter.pushElement("rowUpdate");
					for (int i = 0; i < pageRows; i++) {
						// append element to rowUpdate
						eventFormatter.appendElement(); 
						eventFormatter.setElement("rowNum", i+1);
						// push spanUpdate element 
						eventFormatter.pushElement("spanUpdate"); 
						// append element to spanUpdate
						eventFormatter.appendElement();
						eventFormatter.setElement("startCol", 1);
						eventFormatter.setElement("length", pageColumns);
						eventFormatter.setElement("text", iter->second->getRow(i).c_str());
						eventFormatter.setElement("fgColor", "WHITE");
						// pop appended element to spanUpdate
						eventFormatter.popElement();  
						// pop pushed spanUpdate element
						eventFormatter.popElement();
						// pop appended element to rowUpdate
						eventFormatter.popElement();
					}
					// pop pushed rowUpdate element
					eventFormatter.popElement(); 
					iter->second->setIsInitialPaintSent(true);
				}
				else
				{
					//Sending row updates for the page.

					// Append the rowUpdate data to the event. 
					eventFormatter.appendMessage("RowUpdate", iter->second->topic());
					// get random row and data column to update
					int row = iter->second->generateNumber(pageRows);
					int dataCol = iter->second->generateNumber(dataColumns);
					float currentPrice = iter->second->getPageData(row, dataCol);
					float newPrice = iter->second->setPageData(row, dataCol);
					char buffer[20];
					sprintf_s(buffer, 20, "%10.2f",newPrice);
					
					int pageCol = (dataCol+1) * 10;
					eventFormatter.setElement("rowNum", row + 1);
					// push spanUpdate element
					eventFormatter.pushElement("spanUpdate");
					// append element to spanUpdate
					eventFormatter.appendElement();
					Name START_COL("startCol");
					eventFormatter.setElement(START_COL, pageCol);
					eventFormatter.setElement("length", 10);
					eventFormatter.setElement("text", buffer);
					std::string color = "WHITE";
					if (newPrice > currentPrice)
					{
						color = "LIGHTGREEN";
					}
					else if (newPrice < currentPrice)
					{
						color = "RED";
					}
					eventFormatter.setElement("fgColor", color.c_str());
					// push attr element
					eventFormatter.pushElement("attr");
					switch (iter->second->generateNumber(maxAttributes))
					{
						case 1:
							// blink
							eventFormatter.appendValue("BLINK");
							break;
						case 2:
							// intensify
							eventFormatter.appendValue("INTENSIFY");
							break;
						case 3:
							// reverse
							eventFormatter.appendValue("REVERSE");
							break;
						case 4:
							// underline
							eventFormatter.appendValue("UNDERLINE");
							break;
						case 5:
							// intensify and underline
							eventFormatter.appendValue("INTENSIFY");
							eventFormatter.appendValue("UNDERLINE");
							break;
						case 6:
							// intensify and reverse
							eventFormatter.appendValue("INTENSIFY");
							eventFormatter.appendValue("REVERSE");
							break;
						case 7:
							// intensify and blink
							eventFormatter.appendValue("INTENSIFY");
							eventFormatter.appendValue("BLINK");
							break;
						case 8:
							// reverse and underline
							eventFormatter.appendValue("REVERSE");
							eventFormatter.appendValue("UNDERLINE");
							break;
						default:
							// normal
							break;
					}
					// pop pushed attr element
					eventFormatter.popElement();
					// pop appended element to spanUpdate
					eventFormatter.popElement();
					// pop pushed spanUpdate element
					eventFormatter.popElement();
				}
				session->publish(event);
				g_pubCount++;
				if (0 != d_sleepTime) {
					if (0 == g_pubCount % d_numPerInterval) {
						sleep(d_sleepTime);
					}
				}
				//SLEEP(1);
				//::Sleep(1);
			}						
			// print event on the console
			
			//MessageIterator iter(event);
			//while (iter.next()) {
				//Message msg = iter.message();
				//std::cout << msg << std::endl;
				
			//}  

	        // publish above created event
			//session->publish(event);
			//pubCount++;
			
			
			
			// sleep to 2 seconds
			//SLEEP(2); 
			//::Sleep(10);
		}
	}
	/**********************************************************************************************
	Function    : isAuthorised                                                                                     
	Description : This function authorizes the user/application for publishing data
				Follwing steps are taken for authorization:
				 - Authorize the generated token using authorization request.
				 - On success, populate the identity                    
	***********************************************************************************************/
	bool isAuthorised(std::string token, Identity &identity)
	{
		bool isAuthorised = false;
		bool isRunning = true;

	    
		// Open the authorization service	
		if (!providerSession->openService("//blp/apiauth")) 
		{
			std::cerr << "Failed to open //blp/apiauth" << std::endl;
			return false; 
		}
		std::cout << "Opening //blp/apiauth service. " << std::endl;
		std::cout << std::endl;
		Service authService = providerSession->getService("//blp/apiauth");

		// Create the authorization request
		Request authRequest = authService.createAuthorizationRequest();

		// set the token to be authorized
		authRequest.set("token", token.c_str());

		// send authorzation request with EventQueue
		EventQueue authEventQueue;
		providerSession->sendAuthorizationRequest(authRequest, &identity, CorrelationId(1), &authEventQueue );
	            
		// Poll the event queue until we get a RESPONSE or REQUEST_STATUS event
		while(isRunning)
		{
			Event event = authEventQueue.nextEvent();
			if (event.eventType() == Event::RESPONSE || event.eventType() == Event::REQUEST_STATUS|| event.eventType() == Event::PARTIAL_RESPONSE) 
			{
				MessageIterator msgIter(event);
				while (msgIter.next()) 
				{
					//get message from the event
					Message msg = msgIter.message();   
				    std::cout << "Processing authorization request status Event" << std::endl;
					msg.print(std::cout);
					isRunning = false;
					if (msg.messageType() == AUTHORIZATION_SUCCESS) 
					{
						// Authorization succeeded and the Identity is populated with entitlements.
						isAuthorised = true;
					} 
					else if (msg.messageType() == AUTHORIZATION_FAILURE) 
					{
						// Authorization failed.
						std::cout << "Authorization FAILED" << std::endl;
					} 
					else
					{
						// Unknown message type. Authorization failed.
						std::cout << "Unknown Authorization FAILED" << std::endl;
					} 
				}
			}
		}
		return isAuthorised;
	}

	/***************************************************************************************************
	Function    : generateToken                                                                                     
	Description : This function generate token for the user/application using Generate Token Request. 
			   The token generated is an alphanumeric string. 
	           
	 **************************************************************************************************/
	 bool generateToken(std::string *token)
	{
		bool isTokenSuccess = false;
		bool isRunning = true;
		CorrelationId tokenReqId(99);
		EventQueue tokenEventQueue;

		// Submit the token request.  This will generate token for User/Application
		// specified in the session options earlier
		providerSession->generateToken(tokenReqId, &tokenEventQueue);

		// Wait for the token response
		while(isRunning)
		{
			// Check each event in the queue for TOKEN_STATUS
			Event event = tokenEventQueue.nextEvent();
			if (event.eventType() == Event::TOKEN_STATUS) 
			{
				std::cout << std::endl;
				std::cout << "Processing Token Status Event" << std::endl;
				MessageIterator msgIter(event);
				while (msgIter.next()) 
				{
					Message msg = msgIter.message();
					msg.print(std::cout);
					isRunning = false;
					// the token request succeeded.
					if (msg.messageType() == TOKEN_SUCCESS) 
					{
						isTokenSuccess = true;
						// Get the token from message
						*token = msg.getElementAsString(TOKEN);
					} 
					else if (msg.messageType() == TOKEN_FAILURE) 
					{
						// Token request failed
						// The most likely reason would be that the user/application either doesn't exist
						// or is incorrectly configured, on EMRS					
						std::cout << "Token generation failed. " << std::endl;
						std::cout << "It could be possible that identity in not configured or doesn't" << std::endl;
						std::cout << "exsit in EMRS. Contact your adminstrator" << std::endl;
					}
					else
					{
						std::cout << "Unknown token status." << std::endl;
					}
				}
			}
		}
		return isTokenSuccess;
	}

	 void printStats() {
		 
	 }

	 void ThreadEntry(void) {
		 unsigned long lastVal=0;
		 unsigned long temp;
		 //char buffer[20];
		 while(1) {
			temp = g_pubCount;
			
			std::cout << getTimeStr() << "     Topics: " << g_availableTopicCount 
				<<  ",  Events: " << (temp - lastVal) << ",  Total: " << temp << std::endl;
			lastVal = temp;
			SLEEP(1);
		 }
	 }
 

/**************************************************************************************************
 * Function    : run                                                                                     
 * Description : This function runs the application to demonstrate how to publish 
 *               page data on a topic. It does following:
 * 			   1. Reads command line arguments.
 *			   2. Establishes a provider session which facilitates connection to the 
 *			      bloomberg network
 *             3. Authorize the identity for publishing the data. 
 *			   4. Resolve the topic on the designated service.
 *			   5. Once topic is resolved, it publishes data which is incremented every 2 seconds.
 **************************************************************************************************/
    void run(int argc, char **argv)
    {

		// read command line parameters
        if (!parseCommandLine(argc, argv)) return;
        
		// Set the host and port for the session. 
        SessionOptions sessionOptions;
        for (size_t i = 0; i < d_hosts.size(); ++i) {
            sessionOptions.setServerAddress(d_hosts[i].c_str(), d_port, i);
        }

		sessionOptions.setAuthenticationOptions(d_authOptions.c_str());
		sessionOptions.setAutoRestartOnDisconnection(true);
        sessionOptions.setNumStartAttempts(d_hosts.size());
	
		std::cout << "Connecting to port " << d_port
                  << " on ";
        std::copy(d_hosts.begin(), d_hosts.end(), std::ostream_iterator<std::string>(std::cout, " "));
        std::cout << std::endl;

		// Create and Start() the session
        providerSession = new ProviderSession(sessionOptions, new MyEventHandler(d_service));
        if (!providerSession->start()) {
            std::cerr <<"Failed to start session." << std::endl;
            return;
        }

		// Create identity object
		providerIdentity = providerSession->createIdentity();

		// Publishing to appliance. Authenticate and authorize user/application.
		// On success, populate providerIdentity
		std::string token;

		// Authenticate user/application using Generate Token Request 
		if(!generateToken(&token))
		{
			std::cout << "Exiting...." << std::endl;
			return;
		}
		std::cout << "Token Generation successfull. Checking authorization..." << std::endl
			      << std::endl;

		// Authorize the generated token using authorization request.
		// On success, the providerIdentity is populated with entitlements.
		 if(!isAuthorised(token, providerIdentity))
		 {
			std::cout << "Authorization failed. Exiting...." << std::endl;
			return;
		  }
		  std::cout << "Authorization successfull. Identity populated successfully." << std::endl
			  << std::endl;

		// Register service for the session
		ServiceRegistrationOptions serviceOptions;
		// Set group ID
        serviceOptions.setGroupId(d_groupId.c_str(), d_groupId.size());
		// Set priority
		serviceOptions.setServicePriority(d_priority);
		if (!providerSession->registerService(d_service.c_str(), providerIdentity, serviceOptions))
        {
			std::cerr <<"Service registeration faied: " << d_service << std::endl;
            return;
		} else {
			//std::cout <<"Service registered: " << d_service << std::endl;
		}
#ifdef _WIN32
        handle_ = CreateThread(NULL,0, // security, stack size
		ThreadFunc, // start
		(void *) this,0, &threadID_); // param, creation flags, id
#else
#endif
		publish(providerSession, &providerIdentity);
        providerSession->stop();
    }
};

#ifdef _WIN32 
DWORD WINAPI ThreadFunc(LPVOID arg){
#else
void* printerThread(void *arg) {
#endif
	PagedataInteractivePublisherExample *h = (PagedataInteractivePublisherExample *) arg;
	h->ThreadEntry();
	return 0;
}

/**
 * Application entry point.
 */
int main(int argc, char **argv)
{
    std::cout << "****************************************************************************" << std::endl
			  <<"                   PagedataInteractivePublisherExample        "<< std::endl
			  <<"                   --------------------------------        "<< std::endl
			  << "This application demonstrate publishing page data for a topic only when there are" << std::endl 
			  << "subscriber for that data. To have subscriber to the topic, you will have to" << std::endl
			  << "launch \"PageDataSubscriberExample\". Once there are subscribers, this" << std::endl 
			  << "application publishes data as an integer value that is incremented" << std::endl
			  << "every two seconds." << std::endl
			  << "****************************************************************************" << std::endl 
		      << std::endl; 
    PagedataInteractivePublisherExample example;
	// run the interactive publishing appication
    //try {
        example.run(argc, argv);
    //} catch (Exception &e) {
	//	SLEEP(1);
    //    std::cerr << "Library Exception!!! " << e.description() << std::endl;
    //} 
    // wait for enter key to exit application

    std::cout << "Press ENTER to quit" << std::endl;
	
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}

