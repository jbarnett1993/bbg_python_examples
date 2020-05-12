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
** TechnicalAnalysisHistoricalStudyExample.java
**
** This Example shows how to use Technical Analysis service ("//blp/tasvc")
** to retrieve Historical data for specified study request.
** 
*/
package com.bloomberglp.blpapi.examples;

import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Name;

public class TechnicalAnalysisHistoricalStudyExample {

        private static final Name SECURITY_NAME = new Name("securityName");
        private static final Name STUDY_DATA = new Name("studyData");
        private static final Name RESPONSE_ERROR = new Name("responseError");
        private static final Name SECURITY_ERROR = new Name("securityError");
        private static final Name FIELD_EXCEPTIONS = new Name("fieldExceptions");
        private static final Name FIELD_ID = new Name("fieldId");
        private static final Name ERROR_INFO = new Name("errorInfo");
        private static final Name CATEGORY = new Name("category");
        private static final Name MESSAGE = new Name("message");
        private static final Name SESSION_STARTUP_FAILURE = new Name("SessionStartupFailure");
        private static final Name SESSION_TERMINATED = new Name("SessionTerminated");

        private String    d_host;
        private int       d_port;

        public static void main(String[] args) throws Exception
        {
            System.out.println("Technical Analysis Historical Study Example ");
        	TechnicalAnalysisHistoricalStudyExample example = new TechnicalAnalysisHistoricalStudyExample();
            example.run(args);
            System.out.println("Press ENTER to quit");
            System.in.read();
        }

        private void run(String[] args) throws Exception
        {
            d_host = "localhost";
            d_port = 8194;
            
            if (!parseCommandLine(args)) return;

            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.setServerHost(d_host);
            sessionOptions.setServerPort(d_port);

            System.out.println("Connecting to " + d_host + ":" + d_port);
            Session session = new Session(sessionOptions);
            if (!session.start()) {
                System.err.println("Failed to start session.");
                return;
            }
             if (!session.openService("//blp/tasvc")) {
                System.err.println("Failed to open //blp/tasvc");
                return;
            }
            Service tasvcService = session.getService("//blp/tasvc");

            // Create DMI Study Request
            Request dmiStudyRequest = createDMIStudyRequest(tasvcService);
            System.out.println("Sending Request: " + dmiStudyRequest);
            session.sendRequest(dmiStudyRequest, null);
            // wait for events from session.
            eventLoop(session);

            // Create BOLL Study Request
            Request bollStudyRequest = createBOLLStudyRequest(tasvcService);
            System.out.println("Sending Request: " + bollStudyRequest);
            session.sendRequest(bollStudyRequest, null);
            // wait for events from session.
            eventLoop(session);

            session.stop();
        }

	// Create Technical Analysis Historical EOD - DMI Study Request
        private Request createDMIStudyRequest(Service tasvcService)
        {
            Request request = tasvcService.createRequest("studyRequest");

            Element priceSource = request.getElement("priceSource");
            // set security name
            priceSource.setElement("securityName", "IBM US Equity");

            Element dataRange = priceSource.getElement("dataRange");
            dataRange.setChoice("historical");

            // set historical price data
            Element historical = dataRange.getElement("historical");
            historical.setElement("startDate", "20100501"); // set study start date
            historical.setElement("endDate", "20100528"); // set study start date

            // DMI study example - set study attributes
            Element studyAttributes = request.getElement("studyAttributes");
            studyAttributes.setChoice("dmiStudyAttributes");

            Element dmiStudy = studyAttributes.getElement("dmiStudyAttributes");
            dmiStudy.setElement("period", 14); // DMI study interval
            dmiStudy.setElement("priceSourceHigh", "PX_HIGH");
            dmiStudy.setElement("priceSourceClose", "PX_LAST");
            dmiStudy.setElement("priceSourceLow", "PX_LOW");

            return request;
        }
        
        // Create Technical Analysis Historical EOD - BOLL Study Request
        private Request createBOLLStudyRequest(Service tasvcService) {
            Request request = tasvcService.createRequest("studyRequest");

            Element priceSource = request.getElement("priceSource");
            // set security name
            priceSource.setElement("securityName", "AAPL US Equity");

            Element dataRange = priceSource.getElement("dataRange");
            dataRange.setChoice("historical");

            // set historical price data
            Element historical = dataRange.getElement("historical");
            historical.setElement("startDate", "20100701"); // set study start date
            historical.setElement("endDate", "20100716"); // set study start date

            // BOLL study example - set study attributes
            Element studyAttributes = request.getElement("studyAttributes");
            studyAttributes.setChoice("bollStudyAttributes");

            Element bollStudy = studyAttributes.getElement("bollStudyAttributes");
            bollStudy.setElement("period", 30); // BOLL study interval
            bollStudy.setElement("lowerBand", 2);
            bollStudy.setElement("priceSourceClose", "PX_LAST");
            bollStudy.setElement("upperBand", 4);

           return request;
		}

        private void eventLoop(Session session) throws Exception
        {
            boolean done = false;
            while (!done) {
                Event event = session.nextEvent();
                if (event.eventType() == Event.EventType.PARTIAL_RESPONSE) {
                    System.out.println("Processing Partial Response");
                    processResponseEvent(event);
                }
                else if (event.eventType() == Event.EventType.RESPONSE) {
                    System.out.println("Processing Response");
                    processResponseEvent(event);
                    done = true;
                } else {
                    MessageIterator msgIter = event.messageIterator();
                    while (msgIter.hasNext()) {
                        Message msg = msgIter.next();
                        System.out.println(msg.asElement());
                        if (event.eventType() == Event.EventType.SESSION_STATUS) {
                            if (msg.messageType() == SESSION_TERMINATED ||
                                msg.messageType() == SESSION_STARTUP_FAILURE) {                           
                                System.out.println("SessionTerminated...Exiting");
                                System.exit(0);
                            }
                        }
                    }
                }
            }
        }

        // return true if processing is completed, false otherwise
        private void processResponseEvent(Event event)
        throws Exception
        {
            MessageIterator msgIter = event.messageIterator();
            while (msgIter.hasNext()) {
                Message msg = msgIter.next();
                if (msg.hasElement(RESPONSE_ERROR)) {
                    printErrorInfo("REQUEST FAILED: ", msg.getElement(RESPONSE_ERROR));
                    continue;
                }
                Element security = msg.getElement(SECURITY_NAME);
                String ticker = security.getValueAsString();
                System.out.println("\nTicker: " + ticker);
                if (security.hasElement("securityError"))
                {
                    printErrorInfo("\tSECURITY FAILED: ",
                        security.getElement(SECURITY_ERROR));
                    continue;
                }

                Element fields = msg.getElement(STUDY_DATA);
                if (fields.numValues() > 0)
                {
                    int numValues = fields.numValues();
                    for (int j = 0; j < numValues; ++j)
                    {
                        Element field = fields.getValueAsElement(j);
                        for (int k = 0; k < field.numElements(); k++)
                        {
                            Element element = field.getElement(k);
                            System.out.println("\t" + element.name() + " = " +
                                element.getValueAsString());
                        }
                        System.out.println("");
                    }
                }
                System.out.println("");
                Element fieldExceptions = msg.getElement(FIELD_EXCEPTIONS);
                if (fieldExceptions.numValues() > 0)
                {
                    System.out.println("FIELD\t\tEXCEPTION");
                    System.out.println("-----\t\t---------");
                    for (int k = 0; k < fieldExceptions.numValues(); ++k)
                    {
                        Element fieldException =
                            fieldExceptions.getValueAsElement(k);
                        printErrorInfo(fieldException.getElementAsString(FIELD_ID) +
                            "\t\t", fieldException.getElement(ERROR_INFO));
                    }
                }
            }
        }

        private void printErrorInfo(String leadingStr, Element errorInfo)
        {
            System.out.println(leadingStr + errorInfo.getElementAsString(CATEGORY) +
                " (" + errorInfo.getElementAsString(MESSAGE) + ")");
        }

        private boolean parseCommandLine(String[] args)
        {
            for (int i = 0; i < args.length; ++i) {
                if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length) {
                    d_host = args[++i];
                }
                else if (args[i].equalsIgnoreCase("-p") && i + 1 < args.length) {
                    d_port = Integer.parseInt(args[++i]);
                }
                else if (args[i].equalsIgnoreCase("-h")) {
                    printUsage();
                    return false;
                }
            }
            return true;
        }

        private void printUsage()
        {
            System.out.println("Usage:");
            System.out.println("	Technical Analysis Historical Study Example ");
            System.out.println("		[-ip 		<ipAddress	= localhost>");
            System.out.println("		[-p 		<tcpPort	= 8194>");
        }
}

