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
** TechnicalAnalysisRealtimeStudyExample.cpp
**
** This Example shows how to use Technical Analysis service ("//blp/tasvc")
** to retrieve realtime data for specified study request.
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

#include <vector>
#include <sstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <stdio.h>

using namespace std;
using namespace BloombergLP;
using namespace blpapi;

namespace {
    Name EXCEPTIONS("exceptions");
    Name FIELD_ID("fieldId");
    Name REASON("reason");
    Name CATEGORY("category");
    Name DESCRIPTION("description");
}

class TechnicalAnalysisRealtimeStudyExample {

    std::string         d_host;
    int                 d_port;

    void printUsage()
    {
        cout << "Usage:" << std::endl
            << "    Technical Analysis Realtime Study Example " << std::endl
            << "        [-ip        <ipAddress  = localhost>" << std::endl
            << "        [-p         <tcpPort    = 8194>" << std::endl;
        cout << "Press ENTER to quit" <<std::endl;
    }

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

    size_t getTimeStamp(char *buffer, size_t bufSize)
    {
        const char *format = "%Y/%m/%d %X";

        time_t now = time(0);
#ifdef WIN32
        tm *timeInfo = localtime(&now);
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
        blpapi::CorrelationId corrId;

        fprintf(stdout, "Processing SUBSCRIPTION_STATUS\n");
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            corrId = msg.correlationId();
            fprintf(stdout, "%s: %s - %s\n",
					timeBuffer,
					(char *)corrId.asPointer(),
					msg.messageType().string());

            if (msg.hasElement(REASON, true)) {
                // This can occur on SubscriptionFailure.
                Element reason = msg.getElement(REASON);
                fprintf(stdout, "        %s: %s\n",
                    reason.hasElement(CATEGORY, true) ? reason.getElement(CATEGORY).getValueAsString() : " ",
                    reason.hasElement(DESCRIPTION, true) ? reason.getElement(DESCRIPTION).getValueAsString() : " ");
            }

            if (msg.hasElement(EXCEPTIONS, true)) {
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
        blpapi::CorrelationId corrId;
        while (msgIter.next()) {
            Message msg = msgIter.message();
            corrId = msg.correlationId();
            fprintf(stdout, "%s: %s - %s\n",
                    timeBuffer,
                    (char *)corrId.asPointer(),
                    msg.messageType().string());

            size_t numFields = msg.asElement().numElements();
            for (int i = 0; i < numFields; ++i) {
                const Element field = msg.asElement().getElement(i);
                if (field.numValues() < 1) {
                    fprintf(stdout, "        %s is NULL\n",
                            field.name().string());
                    continue;
                }

                // Assume all values are scalar.
                fprintf(stdout, "        %s = %s\n",
                    field.name().string(), field.getValueAsString());
            }
        }
        return true;
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

    void eventLoop(Session &session) {
        bool done = false;
        while (!done) {
            Event eventObj = session.nextEvent();
                try {
                    switch (eventObj.eventType())
                    {
                    case Event::SUBSCRIPTION_DATA:
                        processSubscriptionDataEvent(eventObj);
                        break;
                    case Event::SUBSCRIPTION_STATUS:
                        processSubscriptionStatus(eventObj);
                        break;
                    default:
                        processMiscEvents(eventObj);
                        break;
                    }
                } catch (Exception &e) {
                        fprintf(stdout, "Library Exception !!! %s\n",
                                e.description().c_str());
                }
        }
    }

        // Create Technical Analysis WLPR Study Subscription
        void addWLPRStudySubscription(SubscriptionList& subscriptions)
        {
                vector<string>       fields;
                fields.push_back("WLPR");

                vector<string>       overrides;
                overrides.push_back("priceSourceClose=LAST_PRICE");
                overrides.push_back("priceSourceHigh=HIGH");
                overrides.push_back("priceSourceLow=LOW");
                overrides.push_back("periodicitySelection=DAILY");
                overrides.push_back("period=14");

                subscriptions.add("IBM US Equity", //security
                                     fields,        //field
                                     overrides,      //options
                                     CorrelationId((char *)"IBM US Equity_WLPR"));
                return;
        }

        // Create Technical Analysis MAO Study Subscription
        void addMAOStudySubscription(SubscriptionList& subscriptions)
        {
                vector<string>       fields;
                fields.push_back("MAO");

                vector<string>       overrides;
                overrides.push_back("priceSourceClose1=LAST_PRICE");
                overrides.push_back("priceSourceClose2=LAST_PRICE");
                overrides.push_back("maPeriod1=6");
                overrides.push_back("maPeriod2=36");
                overrides.push_back("maType1=Simple");
                overrides.push_back("maType2=Simple");
                overrides.push_back("oscType=Difference");
                overrides.push_back("periodicitySelection=DAILY");
                overrides.push_back("sigPeriod=9");
                overrides.push_back("sigType=Simple");

                subscriptions.add("VOD LN Equity",  //security
                                     fields,         //field
                                     overrides,      //options
                                     CorrelationId((char *)"VOD LN Equity_MAO"));
                return;
        }

        // Create Technical Analysis EMAVG Study Subscription
        void addEMAVGStudySubscription(SubscriptionList& subscriptions)
        {
                vector<string>       fields;
                fields.push_back("EMAVG");

                vector<string>       overrides;
                overrides.push_back("priceSourceClose=LAST_PRICE");
                overrides.push_back("periodicitySelection=DAILY");
                overrides.push_back("period=14");

                subscriptions.add("6758 JT Equity", //security
                                    fields,           //field
                                    overrides,        //options
                                    CorrelationId((char *)"6758 JT Equity_EMAVG"));
                return;
        }

public:

    void run(int argc, char **argv)
    {
        d_host = "localhost";
        d_port = 8194;

        if (!parseCommandLine(argc, argv)) return;

        SessionOptions sessionOptions;
        sessionOptions.setServerHost(d_host.c_str());
        sessionOptions.setServerPort(d_port);

        cout <<"Connecting to " << d_host << ":" << d_port << std::endl;
        sessionOptions.setDefaultSubscriptionService("//blp/tasvc");
        Session session(sessionOptions);
        if (!session.start()) {
            std::cerr << "Failed to start session." << std::endl;
            return;
        }
        cout <<"Connecting successfully" << std::endl;
        if (!session.openService("//blp/tasvc")) {
            std::cerr << "Failed to open //blp/tasvc" << std::endl;
            return;
        }
        cout <<"//blp/tasvc opened successfully" << std::endl;

        SubscriptionList        subscriptions;

        // Add Technical Analysis WLPR Study Subscription
        addWLPRStudySubscription(subscriptions);

        // Add Technical Analysis MAO Study Subscription
        addMAOStudySubscription(subscriptions);

        // Add Technical Analysis EMAVG Study Subscription
        addEMAVGStudySubscription(subscriptions);

        // NOTE: User must be entitled to receive realtime data for securities subscribed
        session.subscribe(subscriptions);

        eventLoop(session);

        session.stop();
    }
};

int main(int argc, char **argv)
{
    cout << "Technical Analysis Realtime Study Example" << std::endl;

    TechnicalAnalysisRealtimeStudyExample example;
    example.run(argc, argv);

    // wait for enter key to exit application
    cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    cin.getline(dummy, 2);
    return 0;
}
