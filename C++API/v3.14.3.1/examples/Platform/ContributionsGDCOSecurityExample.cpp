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

/******************************************************************************************************************************************
 ContributionsGDCOSecurityExample.cpp: 
	This program will demonstrate how to load security in GDCO on the terminal. 
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
		3. Create the topic on the session which will be loaded in the GDCO. 
			- Add topic to the topicList 
			- Create topics on the session
		4. Publishing events for the created topics of the designated service.
 Usage: 
    ContributionsGDCOSecurityExample -help 
	ContributionsGDCOSecurityExample -?
	   Print the usage for the program on the console

	ContributionsGDCOSecurityExample -ip <appliance IP> -p <port no> -s <service name> -g <GroupID> -pri <priority> -gdcoID <GDCO#> 
	                                 -mon <monitor#> -page <Page#> -loadType <type> -identifierType <page access type> -auth <option>
	   Prints the response on the console of the command line requested data
 NOTE: If there is a mismatch in the 'loadType', 'identifierType' or 'topic' than what is set on the monitor of the GDCO in use, the
        event will be sent but will not upload the security on the monitor. Please contact contribution representative to find the 
		setting for the monitor on the terminal. 

******************************************************************************************************************************************/
///#include "BlpThreadUtil.h"
#include <blpapi_topiclist.h>
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
#include <iterator>
#include <string>
#include <list>

#ifndef WIN32
#include <unistd.h>  // sleep()
#define SLEEP(s) sleep(s)
#else
#include <windows.h> // Sleep()
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
	const Name TOPIC_SUBSCRIBED("TopicSubscribed");
    const Name TOPIC_UNSUBSCRIBED("TopicUnsubscribed");
    const Name TOPIC_RECAP("TopicRecap");
    const Name TOKEN("token");
	const Name SERVICE_NAME("serviceName");
	const Name SESSION_TERMINATED("SessionTerminated");
	const Name TOPIC("topic");
	const Name TOPIC_CREATED("TopicCreated");
    const Name RESOLVED_TOPIC("resolvedTopic");
}

/*********************************************************************************
Class:		 MyEventHandler
Description: This class implements provider event handler interface for the session. 
             All the event generated for the session are processed by various
			 functions in this class.
**********************************************************************************/
class MyEventHandler : public ProviderEventHandler {
	const std::string d_serviceName;

public:
	MyEventHandler(const std::string& serviceName) : d_serviceName(serviceName) {}
    bool processEvent(const Event& event, ProviderSession* session);

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
        std::cout << "Processing service status event... " << std::endl;
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

class ContributionsLoadSecurityExample
{
    std::vector<std::string> d_hosts;			// IP Address of the appliances
    int                      d_port;			// port number 
    std::string              d_service;			// service for publishing the topic
	std::string              d_authOption;		// authentication option user/application
	std::string              d_name;	        // DirectoryService/ApplicationName
	bool					 d_registerServiceResponse; //register service
	std::string              d_topic;			// topic on which data to be published
	int                      d_priority;        // priority of this publisher app
	std::string              d_groupId;         // Group ID for publisher
	int                      page;
	int                      gdco;
	int                      monitor;
	std::string              loadIndicator;	
	std::string              identifierType;	
	int                      clearSent;

	ProviderSession			 *providerSession;	// session
	Identity				 providerIdentity;  // publishing identity

	std::vector<std::string> loadSecurities;

    /*******************************************************************************
	Function    : printUsage
	Description : This function prints the usage of the program on command line.
	******************************************************************************/
    void printUsage()
    {
        std::cout << std::endl
				  <<"Usage:" << std::endl
                  << "\t[-ip   <ipAddress>]  \tAppliance name or IP (mandatory) " << std::endl
                  << "\t[-p    <tcpPort>]    \tserver port (default: 8196)" << std::endl
                  << "\t[-s    <service>]    \tservice name (mandatory) " << std::endl
                  << "\t[-t    <topic>]    \tSecurities to be uploaded on the monitor (mandatory)" << std::endl
			      << "\t[-gdco    <GDCO>]    \tGDCO PageID (mandatory)" << std::endl
				  << "\t[-monitor    <monitor>]    \tGDCO monitor (Mandatory)" << std::endl
				  << "\t[-page    <page>]    \tGDCO monitor page (Mandatory)" << std::endl
				  << "\t[-loadType    <loadType = MATURITY or ABSOLUTE (Mandatory)>]" << std::endl
				  << "\t[-identifierType    <identifierType = NONE, ISIN, CUSIP, TICKER, NEW_ISIN, BBG_NUMBER(Mandatory)>]" << std::endl
				  << "\t[-auth    <authenticationOption = LOGON or APPLICATION or DIRSVC (default: LOGON)>]" << std::endl
		          << "\t[-n       <name = applicationName or directoryService>]" << std::endl
		          << "\t[-g		  <groupId> = publisher groupId (defaults to unique value)]" << std::endl
		          << "\t[-pri    <priority>] = publisher priority level (default: 10)]" << std::endl
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
            if (!std::strcmp(argv[i],"-ip") && ++i < argc) 
                d_hosts.push_back(argv[i]);
            else if (!std::strcmp(argv[i],"-p") &&  ++i < argc) 
                d_port = std::atoi(argv[i]);
            else if (!std::strcmp(argv[i],"-s") &&  ++i < argc) 
                d_service = argv[i];
			else if (!std::strcmp(argv[i],"-auth") &&  ++i < argc) 
                d_authOption = argv[i];
			else if (!std::strcmp(argv[i],"-n") &&  ++i < argc) 
                d_name = argv[i];
			else if (!std::strcmp(argv[i],"-g") && ++i < argc)
                d_groupId = argv[i];
			else if (!std::strcmp(argv[i],"-gdco") && ++i < argc)
               gdco = std::atoi(argv[i]);
			else if (!std::strcmp(argv[i],"-monitor") && ++i < argc)
               monitor = std::atoi(argv[i]);
			else if (!std::strcmp(argv[i],"-page") && ++i < argc)
               page = std::atoi(argv[i]);
			else if (!std::strcmp(argv[i],"-loadType") &&  ++i < argc) 
                loadIndicator = argv[i];
			else if (!std::strcmp(argv[i],"-identifierType") &&  ++i < argc) 
			{
				if (!std::strcmp(argv[i], "NONE")) 
					identifierType = "PAGE_ACCESS_TYPE_NONE";
				else if (!std::strcmp(argv[i], "CUSIP")) 
					identifierType = "PAGE_ACCESS_TYPE_CUSIP";
				else if (!std::strcmp(argv[i], "ISIN")) 
					identifierType = "PAGE_ACCESS_TYPE_ISIN";
				else if (!std::strcmp(argv[i], "TICKER")) 
					identifierType = "PAGE_ACCESS_TYPE_TICKER";
				else if (!std::strcmp(argv[i], "NEW_ISIN")) 
					identifierType = "PAGE_ACCESS_TYPE_NEW_ISIN";
				else if (!std::strcmp(argv[i], "BBG_NUMBER")) 
					identifierType = "PAGE_ACCESS_TYPE_BBG_NUMBER";
				else { 
					printUsage();
					return false;
				}
			}
            else if (!std::strcmp(argv[i],"-pri") && ++i < argc)
                d_priority = std::atoi(argv[i]);
			else if (!std::strcmp(argv[i],"-t") && ++i < argc) 
                loadSecurities.push_back(argv[i]);
           else { 
                printUsage();
                return false;
            }
        }
		if (loadSecurities.empty()) {
            std::cout << "Please provide securities to be uploaded" << std::endl;
             return false;
        }

		// check for service name
		if (!std::strcmp(d_service.c_str(),"")){
			 std::cout << "Please provide service name" << std::endl;
             return false;
		}

		// check for GDCO 
		if (!gdco){
			 std::cout << "Please specify GDCO number" << std::endl;
             return false;
		}

		// check for monitor
		if (!monitor){
			 std::cout << "Please specify monitor for GDCO" << std::endl;
             return false;
		}

		// check for page
		if (!page){
			 std::cout << "Please specify page on the specified monitor" << std::endl;
             return false;
		}

		// check for loadType
		if (!std::strcmp(loadIndicator.c_str(),"")){
			 std::cout << "Please provide load tpye. To find which loadtype to use for the GDCO in use," << std::endl
				       << "please contact contribution representative." << std::endl;
             return false;
		}

		// check for service name
		if (!std::strcmp(identifierType.c_str(),"")){
			  std::cout << "Please provide identifier type. To find which identifierType to use for the GDCO in use," << std::endl
				        <<  "please contact contribution representative." << std::endl;
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
             return false;
		}
        return true;
    }

public:

    ContributionsLoadSecurityExample()
		// Default values
        : d_hosts()
        , d_port(8196)
		, clearSent(0)
        , d_topic("/PAGE/test")
		, page(0)
        , d_authOption("LOGON")   // OS_LOGON default authentication
		, d_name()
		, d_service()
		, loadIndicator()
		, identifierType()
		, d_priority(10)
		, gdco(0)
		, monitor(0)
	{
    }

	/**************************************************************************************************
	Function    : publish                                                                                     
	Description : This function resolves the topic and publish event for the resoved topic
				   on a specific service. 
    ***************************************************************************************************/
    void publish(ProviderSession *session, Identity *identity) {
		TopicList topicList;
		Topic topic;

		// add topic to the resolution list
        topicList.add((d_service + d_topic).c_str());

		 // createTopics() is synchronous, topicList will be updated
        // with the results of topic creation (resolution will happen
        // under the covers)
        providerSession->createTopics(
            &topicList,
            ProviderSession::DONT_REGISTER_SERVICES,
            providerIdentity);
       
		//Parse the topicList to check resolution status of the list of topics
        for (size_t i = 0; i < topicList.size(); ++i) {
            int resolutionStatus = topicList.statusAt(i);
            if (resolutionStatus == TopicList::CREATED) {
                topic = providerSession->getTopic(topicList.messageAt(i));
			}
            else {
				//TBD
            }
        }

		// get handle for the service on which the security will be contributed
        Service service = providerSession->getService(d_service.c_str());

        // Now start loading security on the monitor

		// create publishing event for the service
        Event event = service.createPublishEvent();
		EventFormatter eventFormatter(event);

		// Below block demonstrate how to clear a monitor. Clearing a monitor
		// will clear all pages with the monitor. A page with the monitor can be 
		// cleared by deleting the rows on the page. For details on element, please refer to
		// the service schema.
		if (clearSent == 0) {
			   //Append one or more messages to the event
				eventFormatter.appendMessage("MonitorablePageData", topic);		
				// push clearMonitorablePage element
				eventFormatter.pushElement("clearMonitorablePage");
				eventFormatter.setElement("pageID", gdco); // GDCO number
				eventFormatter.setElement("pageSubID", monitor);  //Monitor number
				eventFormatter.popElement();
				clearSent++; 
				providerSession->publish(event);
				SLEEP(5); // To ensure monitor is cleared before loading securities on monitor
		}

		// Below block demonstrate how to add securities on a page in a monitor.
		// For details on element , please refer to the service schema.
		for (int i = 0; i < loadSecurities.size(); ++i) {
			eventFormatter.appendMessage("MonitorablePageData", topic);
			// push loadMonitorableSecurities element
			eventFormatter.pushElement("loadMonitorableSecurities");

			eventFormatter.setElement("pageID", gdco); // GDCO 
			eventFormatter.setElement("pageSubID", monitor); // Monitor

			// Operation to perform on the monitor.
			// Possible operations are: ADD, DELETE, MODIFY
			eventFormatter.setElement("pageOperation", "ADD"); 

			// identifierType has to match with what is setup on the terminal for the monitor on the GDCO# in use.
			// Contact your contribution representative and use the appropiate type.
			// If there is a mismatch in the identifierType sent by the application and what is 
			// on the terminal, the packet will not be displayed on the GDCO.
			// Possible values for identifierType are:
			//     - PAGE_ACCESS_TYPE_ISIN
			//	   - PAGE_ACCESS_TYPE_NONE
			//     - PAGE_ACCESS_TYPE_BBG_NUMBER
			//     - PAGE_ACCESS_TYPE_CUSIP
			//     - PAGE_ACCESS_TYPE_TICKER
			//     - PAGE_ACCESS_TYPE_NEW_ISIN
			eventFormatter.setElement("identifierType", identifierType.c_str());
			eventFormatter.setElement("identifier", loadSecurities[i].c_str());

			// loadIndicatior specify how to load the securities on the page. 
			// loadIndicatior has to match with what is setup on the terminal for the monitor on the GDCO# in use.
			// Contact your contribution representative and use the appropiate type.
			// Possible values are ABSOLUTE_ORDER/MATURITY_ORDER
			if (!std::strcmp(loadIndicator.c_str(),"ABSOLUTE")) { 
				eventFormatter.setElement("loadIndicator", "ABSOLUTE_ORDER"); 
			} else {
				eventFormatter.setElement("loadIndicator", "MATURITY_ORDER"); 
			}
			
			// set page and row number only if monitor is set for absolute order sorting
			if (!(std::strcmp(loadIndicator.c_str(),"ABSOLUTE"))) {
				eventFormatter.setElement("pageNumber", page);// page 
				eventFormatter.setElement("rowNum", i+1);   // row
			}

			eventFormatter.popElement();
		}
		// print event on the console
		MessageIterator iter(event);
		while (iter.next()) {
			Message msg = iter.message();
			std::cout << msg << std::endl;
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
 * Description : This function runs the application to demonstrate how to load securities on the  
 *               GDCO monitor. It does following:
 * 			   1. Reads command line arguments.
 *			   2. Establishes a provider session which facilitates connection to the 
 *			      bloomberg network
 *             3. Authorize the identity for publishing the data. 
 *			   4. Resolve the topic on the designated service.
 *			   5. Once topic is resolved, it publish the event to load securities on the GDCO
 *                monitor.
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
			std::cerr <<"Service registeration failed: " << d_service << std::endl;
            return;
		} else {
			//std::cerr <<"Service registered: " << d_service << std::endl;
		}

		publish(providerSession, &providerIdentity);
        providerSession->stop();
	}
};

/**
 * Application entry point.
 */
int main(int argc, char **argv)
{
    std::cout << "ContributionsGDCOLoadSecurityExample" << std::endl;
	std::cout << "****************************************************************************" << std::endl
			  <<"                   ContributionsGDCOLoadSecurityExample        "<< std::endl
			  <<"                   ------------------------------------        "<< std::endl
			  << "This application demonstrate uploading the securities in a page on a GDCO monitor. " << std::endl 
			  << "This application will clear the specifed page on a specified GDCO " << std::endl
			  << " monitor and then load the security/ticker specified on the input. " << std::endl 
			  << "In order to view contributed prices for the loaded ticker, please run " << std::endl
			  << "ContributionsMktdataExample\" for the uploaded security. " << std::endl
			  << "NOTE: This application will successfully load the security on the monitor " << std::endl
			  << "on the termianl if there is no mismatch between the setting on the terminal" << std::endl
			  << "and the element you are sending in the publish event. Please contact your " << std::endl
			  << "contribution representative for details" << std::endl
			  << "****************************************************************************" << std::endl 
		      << std::endl; 
    ContributionsLoadSecurityExample example;
	// run the appication to load security in a page of a GDCO monitor
    try {
        example.run(argc, argv);
    } catch (Exception &e) {
        std::cerr << "Library Exception!!! " << e.description() << std::endl;
    }
    // wait for enter key to exit application
    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}
