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
 ContributionsFullPageExample.cpp: 
	This program demonstrate contributing page data to a given monitor for a contribution ID. 
	It does following:
		1. Establishing a session(ProviderSession) for publication of data to the bloomberg 
		   network.
		2. Obtaining authorization for the publishing identity(user/application) using authorization 
		   request
		    - Generate the token for identity(user/application)
		    - Initiating the api authorization Service (//blp//apiauth) 
			- Authorize the user/application using authorisation request with token set.
		3. Resolving the topic to be published. 
			- Creating resolution list 
			- Add topic associated with the designated service to resolution list
			- Register the service automatically on which topic will be published
			- Resolve the resolution list synchronously
		4. Publishing event for the resolved topic to the designated service.
 Usage: 
    ContributionsFullPageExample -help 
	ContributionsFullPageExample -?
	   Print the usage for the program on the console

 Example usage:
	ContributionsFullPageExample -ip <appliance IP> -p <port no> -s <service name> -contrib <Contributor ID> 
	   Prints the response on the console of the command line requested data

************************************************************************************************************/
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

#include <iterator>
#include <iostream>
#include <sstream>
#include <string>
#include <list>
#include <stdlib.h>
#include <time.h>

#ifndef WIN32
#include <unistd.h>
#define SLEEP(s) sleep(s)
#else 
#include <windows.h>
#define SLEEP(s) Sleep(s * 1000)
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
    const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
	const Name AUTHORIZATION_FAILURE("AuthorizationFailure");
    const Name TOKEN("token");
	const Name TOPIC_SUBSCRIBED("TopicSubscribed");
	const Name TOPIC("topic");
	const Name SERVICE_NAME("serviceName");
	const Name RESOLVED_TOPIC("resolvedTopic");

	// page constants
	static const int pageRows = 25;		 // number of rows per page
	static const int pageColumns = 80;   // number of columns
	static const int dataColumns = 6;	 // number of data colums per row
	static const int maxAttributes = 12; // max. number for attribute random number generator  
}

class MyStream {
    std::string d_id;
    Topic d_topic;

public:
    MyStream() : d_id("") {};
    MyStream(std::string const& id) : d_id(id) {}
    void setTopic(Topic const& topic) {d_topic = topic;}
    std::string const& getId() {return d_id;}
    Topic const& getTopic() {return d_topic;}
};

typedef std::list<MyStream*> MyStreams;


/*********************************************************************************
Class:		 MyEventHandler
Description: This class implements provider event handler interface for the session. 
             All the event generated for the session are processed by various
			 functions in this class.
**********************************************************************************/
class MyEventHandler : public ProviderEventHandler {

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
	bool processResolutionStatus(const Event &event){
		MessageIterator msgIter(event);
		while (msgIter.next()) {
			Message msg = msgIter.message();
			msg.print(std::cout);
			if (msg.messageType() == RESOLUTION_SUCCESS) {
				std::cout << "Successfully resolved \'" << msg.getElement(RESOLVED_TOPIC).getValueAsString() 
						  << "\' topic for publishing " << std::endl
						  << std::endl;
			} else if (msg.messageType() == RESOLUTION_FAILURE) {
				std::cout << "Failed to resolve topic for publishing " << std::endl
						  << std::endl;
			} else {
			}
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
			}
		}
		return true;
	}
public:
    bool processEvent(const Event& event, ProviderSession* session);
};


/****************************************************************************************
Function    : processEvent
Description : This function is an Event Handler to process events generated on the session
******************************************************************************************/
bool MyEventHandler::processEvent(const Event& event, ProviderSession* session) {
	switch (event.eventType())
		{                
		case Event::SERVICE_STATUS:
			// Process events SERVICE_STATUS
			std::cout << "Processing service status svent... " << std::endl;
			return processServiceStatus(event);
			break;
		case Event::RESOLUTION_STATUS:
			// Process events RESOLUTION_STATUS
			std::cout << "Processing resolution status event... " << std::endl;
			return processResolutionStatus(event);
			break;
		default:
			//Process events other than SERVICE_STATUS and RESOLUTION_STATUS
			return processMiscEvents(event);
			break;
		}
        
    return true;
}

/**************************************************************************************
  Class for demonstrating market data local publishing.
*************************************************************************************/
class ContributionsFullPageExample
{
private:
    std::vector<std::string> d_hosts;			// IP Address of the appliance
    int                      d_port;				// port number 
    std::string              d_service;				// service for publishing the topic
	std::string              d_authOption;			// authentication option user/application
	std::string              d_name;				// DirectoryService/ApplicationName
    bool                     d_skipAuthorization;   // Skip authorization 
	bool					 d_isInitialPaint;		// flag initial paint
	ProviderSession			*providerSession;		// session for publishing
	Identity				providerIdentity;		// user/application identity 
	float					pageData[pageRows][dataColumns];	// page cache
	int                      d_contribId;				// contribution ID
	int                      page;
	int                      monitor;

	int                      d_priority;        // priority of this publisher app
	std::string              d_groupId;         // Group ID for publisher


public:
   ContributionsFullPageExample()
	   	// Default values
        : d_hosts()
        , d_port(8196)
        , d_authOption("LOGON")   // OS_LOGON default authentication
		, d_name()
		, d_service()
		, d_priority(10)
		, d_contribId(0)
		, monitor(0)
		, page(0)
	{
    }

	/*******************************************************************************
	Function    : printUsage
	Description : This function prints the usage of the program on command line.
	******************************************************************************/
    void printUsage()
    {
        std::cout << "Usage:" << std::endl
                  << "Contribute a page for a contributor " << std::endl
                  << "\t[-ip   <ipAddress>]  \tAppliance name or IP (mandatory) " << std::endl
                  << "\t[-p    <tcpPort>]    \tserver port (default: 8196)" << std::endl
                  << "\t[-s    <service>]    \tservice name (mandatory) " << std::endl
                  << "\t[-t       <topic = /contributorID/montior/product>]" << std::endl
				  << "\t[-contrib       <Contribution ID>]	\t Contributor ID (Mandatory)" << std::endl
				  << "\t[-monitor    <monitor>]    \tGPGX monitor (Mandatory)" << std::endl
				  << "\t[-page    <page>]    \tGPGX monitor page (Mandatory)" << std::endl
				  << "\t[-auth    <authenticationOption = LOGON or APPLICATION OR DIRSVC>]" << std::endl
				  << "\t[-n       <name = applicationName or directoryService>]" << std::endl
				  << "Notes:" << std::endl
				  << " -Specify only LOGON to authorize 'user' using Windows/unix login name." << std::endl
				  << " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << std::endl
				  << " -Specify APPLICATION and name(Application Name) to authorize application." << std::endl
				  << std::endl;
    }

	/***********************************************************************************
	Function    : parseCommandLine
	Description : This function parses the command line arguments.If the command
				  line argument are not provided properly, it print the usage on 
				  commandline. 
	*********************************************************************************/    
	bool parseCommandLine(int argc, char **argv)
    {
		if (argc == 2) {
			// print usage if user ask for help using following option
			if (!strcmp(argv[1], "-?") || !strcmp(argv[1], "/?") || 
				!strcmp(argv[1],"-help") || !strcmp(argv[1],"-h")) {
				printUsage();
				return false;
			}
		} 	
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) 
                d_hosts.push_back(argv[i]);
            else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) 
                d_port = std::atoi(argv[i]);
            else if (!std::strcmp(argv[i],"-s") &&  i + 1 < argc) 
                d_service = argv[i];
			else if (!std::strcmp(argv[i],"-auth") &&  i + 1 < argc) 
                d_authOption = argv[i];
			else if (!std::strcmp(argv[i],"-n") &&  i + 1 < argc) 
                d_name = argv[i];
			else if (!std::strcmp(argv[i],"-contrib") &&  i + 1 < argc) 
                d_contribId = std::atoi(argv[i]);
			else if (!std::strcmp(argv[i],"-monitor") && i + 1 < argc)
               monitor = std::atoi(argv[i]);
			else if (!std::strcmp(argv[i],"-page") && i + 1 < argc)
               page = std::atoi(argv[i]);
            else { 
                printUsage();
                return false;
			}
		}

		// check for contrib ID
		if (d_contribId == 0) {
			 std::cout << "Please provide contributor id" << std::endl;
			 printUsage();
             return false;
		}

		// check for monitor
		if (!monitor){
			 std::cout << "Please specify monitor for contributor ID" << std::endl;
             return false;
		}

		// check for page
		if (!page){
			 std::cout << "Please specify page on the specified monitor" << std::endl;
             return false;
		}

		// check for service name
		if (!std::strcmp(d_service.c_str(),"")){
			 std::cout << "Please provide service name" << std::endl;
			 printUsage();
             return false;
		}

		// check for host name
		if (d_hosts.empty()){
			 std::cout << "Please provide appliance IP or name" << std::endl;
             return false;
		}

		// check for appliation name
		if ((!std::strcmp(d_authOption.c_str(),"APPLICATION")) && (!std::strcmp(d_name.c_str(), ""))){
			 std::cout << "Application name cannot be NULL for application authorization." << std::endl;
			 printUsage();
             return false;
		}
        return true;
	}

	/**************************************************************************************************
	Function    : publish                                                                                     
	Description : This function resolves the topic and publish event for the resoved topic
				   on a specific service. 
	***************************************************************************************************/
	void publish(ProviderSession *session, Identity *identity) {
		ResolutionList resolutionList;
		Topic topic;

		TopicList topicList;
		std::stringstream  d_topic; 
		d_topic << "/page/" << d_contribId << "/" << monitor << "/" << page;
        topicList.add((d_service + d_topic.str()).c_str(),
            CorrelationId(new MyStream(d_topic.str())));

        session->createTopics(
            &topicList,
            ProviderSession::AUTO_REGISTER_SERVICES,
            providerIdentity);

        MyStreams myStreams;

        for (size_t i = 0; i < topicList.size(); ++i) {
            MyStream *stream = reinterpret_cast<MyStream*>(
                topicList.correlationIdAt(i).asPointer());
            int resolutionStatus = topicList.statusAt(i);
            if (resolutionStatus == TopicList::CREATED) {
                Topic topic = session->getTopic(topicList.messageAt(i));
                stream->setTopic(topic);
                myStreams.push_back(stream);
            }
            else {
                std::cout
                    << "Stream '"
                    << stream->getId()
                    << "': topic not resolved, status = "
                    << resolutionStatus
                    << std::endl;
            }
        }
		
		// get handle for the publishing service on which the topic will be published
        Service service = providerSession->getService(d_service.c_str());		
		//service.print(std::cout);
        // Now we will start contribution
		std::cout << "Contributing now..." << std::endl;
        int value=1;
		// Create an event suitable for publishing to this Service. 
		Event event = service.createPublishEvent();
		// Create event formatter for creating the event for publishing
		EventFormatter eventFormatter(event);

		// Create publishing event for each resolved topic. 
			for (MyStreams::iterator iter = myStreams.begin();
            iter != myStreams.end(); ++iter)
		{
				initPageData();
				// Append the rowUpdate data to the event. 
				eventFormatter.appendMessage("PageData", (*iter)->getTopic());
				eventFormatter.pushElement("rowUpdate");
				// push rowUpdate element
				for (int i = 0; i < pageRows; ++i) {
					// append element to rowUpdate
					eventFormatter.appendElement();
					eventFormatter.setElement("rowNum", i + 1);
					// push spanUpdate element 
					eventFormatter.pushElement("spanUpdate");
					// append element to spanUpdate
					eventFormatter.appendElement();
					eventFormatter.setElement("startCol", 1);
					eventFormatter.setElement("length", pageColumns);
					eventFormatter.setElement("text", getRow(i).c_str());
					//eventFormatter.setElement("fgColor", "WHITE");
					// pop appended element to spanUpdate
					eventFormatter.popElement(); 
					// pop pushed spanUpdate element
					eventFormatter.popElement();
					// pop appended element to rowUpdate
					eventFormatter.popElement();
				}
				// pop pushed rowUpdate element
				eventFormatter.popElement();

				eventFormatter.setElement("productCode", monitor);
					eventFormatter.setElement("pageNumber", page);
				eventFormatter.setElement("contributorId", d_contribId);
			}

		// print event on the console
		MessageIterator iter(event);
		while (iter.next()) {
			Message msg = iter.message();
			std::cout << d_topic.str() << " - "; 
			msg.print(std::cout) <<std::endl; 
		}	

		// publish above created event
		providerSession->publish(event);

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

		EventQueue authEventQueue;
		providerSession->sendAuthorizationRequest(authRequest, &identity, CorrelationId(1), &authEventQueue );
	            
		// Poll the event queue until we get a RESPONSE or REQUEST_STATUS event
		while(isRunning)
		{
			Event event = authEventQueue.nextEvent();
			if (event.eventType() == Event::RESPONSE || event.eventType() == Event::REQUEST_STATUS || event.eventType() == Event::PARTIAL_RESPONSE) 
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
 
	/**********************************************************************************************
	Function    : setAuthOptions                                                                                     
	Description : This function set the authentication option for the session. Authentication can  
				be done on user or application. User can be authenticated either by:
				   - using windows/unix login
				   - some active directory service property
	***********************************************************************************************/
	void setAuthOptions (SessionOptions *sessionOptions)
	{
		std::string authOptions;

		if (!std::strcmp(d_authOption.c_str(),"APPLICATION")) { //  Authenticate application
			// Set Application Authentication Option
			authOptions = "AuthenticationMode=APPLICATION_ONLY;";
			authOptions+= "ApplicationAuthenticationType=APPNAME_AND_KEY;";
			// ApplicationName is the entry in EMRS.
			authOptions+= "ApplicationName=" + d_name;
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
		sessionOptions->setAuthenticationOptions(authOptions.c_str());
	}

/**************************************************************************************************
 * Function    : run                                                                                     
 * Description : This function runs the application to demonstrate how to publish 
 *               data on a topic. It does following:
 * 			   1. Reads command line arguments.
 *			   2. Establishes a provider session which facilitates connection to the 
 *			      bloomberg network
 *             3. Authorize the identity for publishing the data. 
 *			   4. Resolve the topic on the designated service.
 *			   5. Once topic is resolved, it contribute the page on the specified monitor/page
 * Arguments   : int argc, char **argv - Command line parameters.
 * Returns     : void
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

		setAuthOptions(&sessionOptions);
		sessionOptions.setAutoRestartOnDisconnection(true);
        sessionOptions.setNumStartAttempts(d_hosts.size());

		std::cout << "Connecting to port " << d_port
                  << " on ";
        std::copy(d_hosts.begin(), d_hosts.end(), std::ostream_iterator<std::string>(std::cout, " "));
        std::cout << std::endl;

		// Create and Start() the session
        providerSession = new ProviderSession(sessionOptions, new MyEventHandler());
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
			std::cerr <<"Service registeration failed: " << d_service << std::endl;
            return;
		} else {
			//std::cerr <<"Service registered: " << d_service << std::endl;
		}


		publish(providerSession, &providerIdentity);
        providerSession->stop();
		//SLEEP(1);
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
		for (i=0; i<pageRows; ++i)
		{
			for (j=0; j<dataColumns; ++j)
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
		if (row < 10)
		{
			temp << "Row 0" << row;
		}
		else
		{
			temp << "Row " << row;
		}
		rowData = temp.str() + ":     ";
		for (i=0; i < dataColumns; ++i)
		{
			temp.str("");
			temp.precision(2);
			temp << std::fixed << pageData[row][i];
			std::string temp2 = std::string(temp.str());
			rowData += padString(temp2,10);
		}
		return rowData;
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
	
	int generateNumber(int maxValue)
	{
		return rand() % maxValue;
	}

	/*****************************************************************************
	Function    : generatePrice
	Description : This function pad string with space
	*****************************************************************************/
	std::string padString(std::string value, int maxLength)
	{
		std::string temp = value;
		int padCount = maxLength - temp.length();
		for (int i=0; i<padCount; ++i)
		{
			temp += " ";
		}
		return temp;
	}
};

/**********************************************************
		Application entry point.
***********************************************************/
int main(int argc, char **argv)
{
    std::cout << "***********************************************************************" << std::endl
			  <<"                   ContributionsFullPageExample        "<< std::endl
			  <<"                   --------------------------------        "<< std::endl
			  << "This application demonstrate publishing page data on just one topic with no" << std::endl 
			  << "indication if anyone is consuming that data. The data published by this" << std::endl 
			  << "application is just an integer value that is incremented and published" << std::endl
			  << "every five seconds." << std::endl
			  << "***********************************************************************" << std::endl 
		      << std::endl; 
    ContributionsFullPageExample example;
    try {
        example.run(argc, argv);
    } catch (Exception &e) {
		SLEEP(5);
        std::cerr << "Library Exception!!! " << e.description() << std::endl;
    } 
    // wait for enter key to exit application
    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}

