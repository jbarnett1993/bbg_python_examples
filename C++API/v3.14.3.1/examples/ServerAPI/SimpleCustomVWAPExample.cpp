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
 SimpleCustomVWAPExample.cpp: 
    This program demonstrate how to make subscription to particular 
    security/ticker to get realtime VWAP updates using Polling method. 
    It uses Market Data service(//blp/mktvwap) provided by API.
    It does following:
        1. Establishing a session which facilitates connection to the bloomberg 
           network.
        2. Initiating the Market Data Data Service(//blp/mktvwap) for realtime
           data.
        3. Creating and sending request to the session.
            - Creating subscription list
            - Add securities and fields to subscription list
            - Specifies VWAP Overrides option
            - Subscribe to realtime data
        4. Event Handling of the responses received.
 Usage: 
    SimpleCustomVWAPExample -help 
    SimpleCustomVWAPExample -?
       Print the usage for the program on the console

    SimpleCustomVWAPExample
       Run the program with default values. Prints the realtime VWAP updates 
       on the console for three default securities specfied
       1. Ticker - "IBM US Equity"
       2. Ticker - "6758 JT Equity" 
       3. Ticker - "VOD LN Equity"

    example usage:
    SimpleCustomVWAPExample
    SimpleCustomVWAPExample -ip localhost -p 8194	

    SimpleCustomVWAPExample -o VWAP_START_TIME=11:00 -o VWAP_END_TIME=15:00

    - Subscribing to Bloomberg defined VWAP and VWAP Volume
    SimpleCustomVWAPExample -s "AAPL US Equity" -f VWAP -f RT_VWAP_VOLUME 
                    -o VWAP_START_TIME=11:00
    
    - Subscribing to Market defined VWAP and VWAP Volume
    SimpleCustomVWAPExample -s "AAPL US Equity" -f MARKET_DEFINED_VWAP_REALTIME 
                    -f RT_MKT_VWAP_VOLUME -o VWAP_START_TIME=11:00
    
    - Subscribing to both Bloomberg defined & Market defined VWAP and VWAP Volume
    SimpleCustomVWAPExample -s "AAPL US Equity" -f VWAP -f RT_VWAP_VOLUME 
                    -f MARKET_DEFINED_VWAP_REALTIME -f RT_MKT_VWAP_VOLUME

    Prints the response on the console of the command line requested data

******************************************************************************/

#include <blpapi_defs.h>
#include <blpapi_correlationid.h>
#include <blpapi_element.h>
#include <blpapi_event.h>
#include <blpapi_exception.h>
#include <blpapi_message.h>
#include <blpapi_session.h>
#include <blpapi_subscriptionlist.h>

#include <vector>
#include <iostream>
#include <string>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <stdio.h>

using namespace std;
using namespace BloombergLP;
using namespace blpapi;

class SimpleCustomVWAPExample
{
    string               d_host;
    int                  d_port;
    vector<string>       d_securities;
    vector<string>       d_fields;
    vector<string>       d_overrides;

private:

    /*****************************************************************************
    Function    : printUsage
    Description : This function prints the usage of the program on command line.
    Argument    : void
    Returns     : void
    *****************************************************************************/
    void printUsage()
    {
        cout << "Usage:" << endl
            << "    Retrieve customized realtime vwap data using Bloomberg V3 API" 
            << endl
            << "      [-s         <security   = \"IBM US Equity\">" << endl
            << "      [-f         <field      = VWAP>" << endl
            << "      [-f         <overrides  = VWAP_START_TIME=09:00>" << endl
            << "      [-ip        <ipAddress = localhost>" << endl
            << "      [-p         <tcpPort   = 8194>" << endl
            << "Notes:" << endl
            << "Multiple securities, vwap fields & overrides can be specified." << endl;
    }

    /*****************************************************************************
    Function    : parseCommandLine
    Description : This function parses the command line arguments.If the command
                  line argument are not provided properly, it calls printUsage to 
                  print the usage on commandline. If no commnd line arguments are 
                  specified this fuction will set default values for 
                  security/fields/overrides
    Argument	: Command line parameters
    Returns		: bool: 
                  true, if successfully set the input argument for the request 
                  from command line or using default values otherwise false
    *****************************************************************************/
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
            if (!strcmp(argv[i],"-s") && i + 1 < argc) {
                d_securities.push_back(argv[++i]);
            } else if (!strcmp(argv[i],"-f") && i + 1 < argc) {
                d_fields.push_back(argv[++i]);
            } else if (!strcmp(argv[i],"-o") && i + 1 < argc) {
                d_overrides.push_back(argv[++i]);
            } else if (!strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_host = argv[++i];
            } else if (!strcmp(argv[i],"-p") &&  i + 1 < argc) {
                 d_port = atoi(argv[++i]);
            } else {
                printUsage();
                return false;
            }
        }

        // Default securities, fields and override if not specified on command line
        if (d_securities.size() == 0) {
            d_securities.push_back("IBM US Equity");
            d_securities.push_back("VOD LN Equity");
            d_securities.push_back("6758 JT Equity");
        }

        if (d_fields.size() == 0) {
            // Subscribing to Bloomberg defined VWAP and VWAP Volume
            d_fields.push_back("VWAP");
            d_fields.push_back("RT_VWAP_VOLUME");

            // Subscribing to Market defined VWAP and VWAP Volume
            d_fields.push_back("MARKET_DEFINED_VWAP_REALTIME");
            d_fields.push_back("RT_MKT_VWAP_VOLUME");
        }

        if (d_overrides.size() == 0) {
            d_overrides.push_back("VWAP_START_TIME=09:00");
        }

        return true;
    }

    /*****************************************************************************
    Function    : getTimeStamp
    Description : Sets current local time to string buffer
    Argument    : buffer - Pointer to string buffer
                  bufSize - size of string buffer
    Returns     : size_t - no.of characters placed in the buffer
    *****************************************************************************/
    size_t getTimeStamp(char *buffer, size_t bufSize)
    {
        const char *format = "%Y-%m-%dT%X";

        time_t now = time(0);
#ifdef WIN32
        tm *timeInfo = localtime(&now);
#else
        tm _timeInfo;
        tm *timeInfo = localtime_r(&now, &_timeInfo);
#endif
        return strftime(buffer, bufSize, format, timeInfo);
    }

    /*****************************************************************************
    Function    : eventLoop
    Description : This function waits for the session events and 
                  handles subscription data and subscription status events. 
                  This function reads update data messages in the event
                  element and prints them on the console.
    Argument    : reference to session object
    Returns     : void
    *****************************************************************************/
    void eventLoop(Session &session)
    {
        char timeBuffer[64];
        while (true) {
            Event event = session.nextEvent();
            MessageIterator msgIter(event);
            while (msgIter.next()) {
                Message msg = msgIter.message();
                if (event.eventType() == Event::SUBSCRIPTION_STATUS ||
                    event.eventType() == Event::SUBSCRIPTION_DATA) {
                    string *topic = 
                        reinterpret_cast<string*>(msg.correlationId().asPointer());
                    getTimeStamp(timeBuffer, sizeof(timeBuffer));
                    cout << timeBuffer << ": " << topic->c_str() << " - ";
                }
                msg.print(cout) << endl;
            }
        }
    }

public:

    // Constructor
    SimpleCustomVWAPExample()
    {
        d_host = "localhost";
        d_port = 8194;
    }

    // Destructor
    ~SimpleCustomVWAPExample()
    {
    }

    /*****************************************************************************
    Function    : run                                                                                     
    Description : This function runs the application to demonstrate how to make 
                  subscription to particular security/ticker to get realtime 
                  streaming updates. It does following:
                  1. Reads command line argumens.
                  2. Establishes a session which facilitates connection to the 
                      bloomberg network
                  3. Opens a mktvwap service with the session. 
                  4. create and send subscription request.
                  5. Event Loop and Response Handling.
    Arguments   : int argc, char **argv - Command line parameters.
    Returns     : void
    *****************************************************************************/
    void run(int argc, char **argv)
    {
        // read command line parameters
        if (!parseCommandLine(argc, argv)) return;

        SessionOptions sessionOptions;
        sessionOptions.setServerHost(d_host.c_str());
        sessionOptions.setServerPort(d_port);

        // Set default subscription service as //blp/mktvwap instead of 
        // default //blp/mktdata in order to get realtime market vwap data.
        sessionOptions.setDefaultSubscriptionService("//blp/mktvwap");

        cout << "Connecting to " + d_host + ":" << d_port << endl;
        Session session(sessionOptions);
        // Start a Session
        if (!session.start()) {
            cout << "Failed to start session." << endl;
            return;
        }
        // Open mktvwap Service
        if (!session.openService("//blp/mktvwap")) {
            cerr <<"Failed to open //blp/mktvwap" << endl;
            return;
        }

        // User must be enabled for real-time data for the exchanges the securities
        // they are monitoring for custom VWAP trade. 
        // Otherwise, Subscription will fail for those securities and/or you will 
        // receive #N/A N/A instead of valid tick data. 
        SubscriptionList subscriptions;
        int secCnt = d_securities.size();
        for(int i=0; i<secCnt; i++){
            subscriptions.add((char *)d_securities[i].c_str(), d_fields, 
                d_overrides, CorrelationId(&d_securities[i]));
        }

        // Make subscription to realtime streaming data
        session.subscribe(subscriptions);
        
        // wait for events from session.
        eventLoop(session);
    }

};

/*********************************
Program entry point.
**********************************/
int main(int argc, char **argv)
{
    cout << "Custom VWAP Example" << endl;
    SimpleCustomVWAPExample example;
    try {
        example.run(argc, argv);
    } 
    catch (Exception &e) {
            cerr << "Library Exception!!! " << e.description() 
                      << endl;
    }

    // wait for enter key to exit application
    cout << "Press ENTER to quit" << endl;
    char dummy[2];
    cin.getline(dummy, 2);
    return 0;
}
