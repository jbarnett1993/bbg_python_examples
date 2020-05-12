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

/*****************************************************************************
 SecurityEntitlementsExample.cpp:
	This program demonstrates how to request the Exchange IDs required for realtime data
	for a list of securities.  These can be stored and compared to user EID's in order 
	to distribute realtime data correctly.  This v3 example works in the same style as the
	functionality available for Entitlements checking for v2.  Only the EID's enabled on the 
	Server API default UUID will be returned from this request.  This is in order to avoid 
	unnecessary comparisons where neither User or Server have permission as this data never 
	needs fitering.
	Program does the following:
		1. Establishes a session which facilitates connection to the bloomberg
		   network.
		2. Initiates the API Auth Service(//blp/apiauth) for authorisation checking.
		3. Creates and sends the request via the session.
			- Creates a list of securities to check
			- Adds securities to request
			- Sends request
        4. Uses an synchronous loop to print the returned message data.
 Usage:
    SecurityEntitlementsExample -h
	   Print the usage for the program on the console

	SecurityEntitlementsExample
	   If you run the program with default values, program prints the EIDs enabled for
	   the default list of securities.

    example usage:
	SecurityEntitlementsExample
	SecurityEntitlementsExample -ip localhost -p 8194
	SecurityEntitlementsExample -p 8294 -s "IBM US Equity"
                                        -ip "127.0.0.1"

	Prints the response on the console of the command line requested data
******************************************************************************/

#include <blpapi_session.h>
#include <blpapi_eventdispatcher.h>

#include <blpapi_event.h>
#include <blpapi_message.h>
#include <blpapi_element.h>
#include <blpapi_name.h>
#include <blpapi_request.h>
#include <blpapi_subscriptionlist.h>
#include <blpapi_defs.h>
#include <blpapi_exception.h>

#include <iostream>
#include <vector>
#include <string>
#include <stdlib.h>
#include <string.h>

using namespace BloombergLP;
using namespace blpapi;

namespace {

    const Name RESPONSE_ERROR("ResponseError");
	const Name SECURITY_ENTITLEMENTS_RESPONSE("SecurityEntitlementsResponse");
	const Name SEQ_NUM("sequenceNumber");

    const char* SECURITY_ENTITLEMENTS_REQUEST = "SecurityEntitlementsRequest";
    const char* APIAUTH_SVC = "//blp/apiauth";

} // anonymous namespace

class SecurityEntitlementsExample 
{
	std::string d_host;
	int			d_port;
	Session		*d_session;
	std::vector<std::string>  d_securities;
	Service apiAuthSvc;

	/*	Constructor	*/
    public: SecurityEntitlementsExample() 
	{
        d_host = "localhost";
        d_port = 8194;
    }

	/*	Destructor	*/
    ~SecurityEntitlementsExample() 
	{
		if(d_session == NULL)
		{}
		else
		{
			d_session->stop();		
		}
    }

	/*
	parseCommandLine
	parses input arguments
	if none are entered IBM US Equity is used as a security 
	if no host and port are specified then the values set in the SecurityEntitlementsExample 
	object are used
	*/
   bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-s") ) {
                if (i + 1 >= argc) return false;
                d_securities.push_back(argv[++i]);
            }
            else if (!std::strcmp(argv[i],"-ip")) {
                if (i + 1 >= argc) return false;
                d_host = argv[++i];
            }
            else if (!std::strcmp(argv[i],"-p")) {
                if (i + 1 >= argc) return false;
                d_port = std::atoi(argv[++i]);
            }
            else return false;
        }

        if (d_securities.size() <= 0) {
            d_securities.push_back("IBM US Equity");
            d_securities.push_back("VOD LN Equity");
        }

        return true;
    }



	/*
	printEvent
	prints messages returned from Bloomberg to the console screen
	*/
    void printEvent(const Event &event)
    {
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            CorrelationId correlationId = msg.correlationId();
            if (correlationId.asInteger() != 0) {
                printf("Correlator: %i\n", correlationId.asInteger());
            }
            msg.print(std::cout);
            printf("\n");
        }
    }

	/*
	printUsage
	prints usage on -h from command line arguments
	*/
    void printUsage()
    {
        std::cout 
            << "Usage:" << '\n'
            << "    Entitlements verification example" << '\n'
            << "        [-s     <security   = IBM US Equity>]" << '\n'
            << "        [-ip    <ipAddress  = localhost>]" << '\n'
            << "        [-p     <tcpPort    = 8194>]" << '\n'
            << "Note:" << '\n'
            <<"Multiple securities and credentials can be" <<
            " specified." << std::endl;
    }

	/*
	creates a session
	*/

    void createSession()
    {
        SessionOptions sessionOptions;
        sessionOptions.setServerHost(d_host.c_str());
        sessionOptions.setServerPort(d_port);

        std::cout << "Connecting to " + d_host + ":" << d_port << std::endl;
        d_session = new Session(sessionOptions);
        bool sessionStarted = d_session->start();
        if (!sessionStarted) {
            std::cerr << "Failed to start session. Exiting..." << std::endl;
            std::exit(-1);
        }
    }

	/*
	opens the apiauth service
	only one service is opened in this instance but multiple services could be added in the
	same manner.
	*/
    void openServices()
    {
        if (!d_session->openService(APIAUTH_SVC)) {
            std::cout << "Failed to open service: " << APIAUTH_SVC 
                << std::endl;
            std::exit(-1);
        }		
		apiAuthSvc = d_session->getService(APIAUTH_SVC);
    }   

	/*	
	sendSecEntRequest	
	loads the securities and sends the security entitlements request with a correlation id
	*/
	void sendSecEntRequest()
    {
		Request request = apiAuthSvc.createRequest("SecurityEntitlementsRequest");

        // Add securities to request
        Element securities = request.getElement("securities");
        for (size_t i = 0; i < d_securities.size(); ++i) {
            securities.appendValue(d_securities[i].c_str());
        }

        std::cout << "Sending Request: " << request << std::endl;
		
		d_session->sendRequest(request, CorrelationId(this));
    }

	/*	
	eventloop	
	parses the returned data until a RESPONSE event is returned
	extracts eid information for each security and prints to the console
	has a 5 minute timeout
	*/
    void eventLoop(Session &session) 
	{
		bool done = false;
		int timeout = 5000 * 6;

		while (!done) 
		{
			Event d_event = session.nextEvent(timeout);
			MessageIterator *msgIter = new MessageIterator(d_event);
			if (d_event.eventType() == Event::TIMEOUT)
			{
				printEvent(d_event);
				done = true;
			}
			else if (d_event.eventType() == Event::RESPONSE ||
				d_event.eventType() == Event::PARTIAL_RESPONSE ||
				d_event.eventType() == Event::REQUEST_STATUS)
			{
				while(msgIter->next())
				{
					Message msg = msgIter->message();
					// the following line prints out the entire message
					// msg.print(std::cout, 0,1);

					if (msg.messageType() == RESPONSE_ERROR)
					{
						printEvent(d_event);
						done = true;
					}
					else if (msg.messageType() == SECURITY_ENTITLEMENTS_RESPONSE)
					{
						Element eidData = msg.getElement("eidData");
						if (eidData.numValues() == 0)
						{
							printEvent(d_event);
						}
						else
						{
							for (int i = 0; i < (int)eidData.numValues(); ++i)
							{
								Element item = eidData.getValueAsElement(i);
								// for status: 0 is success
								int status = item.getElementAsInt32("status");
								printf("%s\t:\t", d_securities[i].c_str());
								if (0 == status)
								{
									Element eids = item.getElement("eids");
									for (int j = 0; j < (int)eids.numValues(); ++j)
									{
										printf("%i ", eids.getValueAsInt32(j));
									}
								}
								else
								{
									printf("Failed %i\n", status);
								}
								printf("\n");
							}
						}
					}
					if (d_event.eventType() == Event::RESPONSE)
					{
						done = true;
					}
				}//end of while
			}//end of else if
			else
			{
				if(msgIter->next())
				{
					Message msg = msgIter->message();
					msg.print(std::cout);
				}
			}
		}//end of while       
	}//end of function

	/*	
	run	
	parses the input arguments
	sets up a session
	opens the service auth service
	creates a security entitlements request
	waits synchronously for the entitlements response
	*/
public: void run(int argc, char **argv) 
	{

        if (!parseCommandLine(argc, argv)) {
            printUsage();
            return;
        }

        createSession();
        openServices();

		sendSecEntRequest();

		eventLoop(*d_session);

        d_session->stop();
        std::cout << "Exiting...\n";
    }
};

int main(int argc, char **argv)
{
	std::cout << "Security Entitlements Example" << std::endl;

	SecurityEntitlementsExample *example = new SecurityEntitlementsExample();
	try 
	{
		example->run(argc, argv);
	} 
	catch (Exception &e) 
	{
		std::cerr << "Library Exception!!! " << e.description() 
				  << std::endl;
	} 
	catch (...) 
	{
		std::cerr << "Unknown Exception!!!" << std::endl;
	}

	// wait for enter key to exit application
	std::cout << "Press ENTER to quit" << std::endl;
	char dummy[2];
	std::cin.getline(dummy, 2);

	return 0;
}


