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
** TechnicalAnalysisIntradayStudyExample.cs
**
** This Example shows how to use Technical Analysis service ("//blp/tasvc")
** to retrieve Intraday data for specified study request.
** 
*/
using System;
using System.Collections.Generic;
using System.Text;
using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;

namespace Bloomberglp.Blpapi.Examples
{

    public class TechnicalAnalysisIntradayStudyExample
    {
        private static readonly Name SECURITY_NAME = Name.GetName("securityName");
        private static readonly Name SECURITY = Name.GetName("security");
        private static readonly Name STUDY_DATA = Name.GetName("studyData");
        private static readonly Name RESPONSE_ERROR = Name.GetName("responseError");
        private static readonly Name SECURITY_ERROR = Name.GetName("securityError");
        private static readonly Name FIELD_EXCEPTIONS = Name.GetName("fieldExceptions");
        private static readonly Name FIELD_ID = Name.GetName("fieldId");
        private static readonly Name ERROR_INFO = Name.GetName("errorInfo");
        private static readonly Name CATEGORY = Name.GetName("category");
        private static readonly Name MESSAGE = Name.GetName("message");

        private string d_host;
        private int d_port;

        public static void Main(string[] args)
        {
            TechnicalAnalysisIntradayStudyExample example = new TechnicalAnalysisIntradayStudyExample();
            System.Console.WriteLine("Technical Analysis Intraday Study Example");
            example.run(args);
            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        private void run(string[] args)
        {
            d_host = "localhost";
            d_port = 8194;

            if (!parseCommandLine(args)) return;

            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerHost = d_host;
            sessionOptions.ServerPort = d_port;

            System.Console.WriteLine("Connecting to " + d_host + ":" + d_port);
            Session session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                System.Console.WriteLine("Failed to start session.");
                return;
            }
            if (!session.OpenService("//blp/tasvc"))
            {
                System.Console.Error.WriteLine("Failed to open //blp/tasvc");
                return;
            }
            Service tasvcService = session.GetService("//blp/tasvc");

            // Create DMI Study Request
            Request dmiStudyRequest = createDMIStudyRequest(tasvcService);
            System.Console.WriteLine("Sending Request: " + dmiStudyRequest);
            session.SendRequest(dmiStudyRequest, null);
            eventLoop(session);

            // Create SMAVG Study Request
            Request smavgStudyRequest = createSMAVGStudyRequest(tasvcService);
            System.Console.WriteLine("Sending Request: " + smavgStudyRequest);
            session.SendRequest(smavgStudyRequest, null);
            eventLoop(session);
        }

        // Create Technical Analysis Intraday - DMI Study Request
        Request createDMIStudyRequest(Service tasvcService)
        {
            Request request = tasvcService.CreateRequest("studyRequest");

            Element priceSource = request.GetElement("priceSource");
            // set security name
            priceSource.SetElement("securityName", "IBM US Equity");

            Element dataRange = priceSource.GetElement("dataRange");
            dataRange.SetChoice("intraday");

            // set intraday price data
            Element intraday = dataRange.GetElement("intraday");
            intraday.SetElement("eventType", "TRADE"); // intraday event type
            intraday.SetElement("interval", 60); // intraday interval
            intraday.SetElement("startDate", "2010-05-26T13:30:00"); // set study start date
            intraday.SetElement("endDate", "2010-05-27T13:30:00"); // set study start date

            // DMI study example - set study attributes
            Element studyAttributes = request.GetElement("studyAttributes");
            studyAttributes.SetChoice("dmiStudyAttributes");

            Element dmiStudy = studyAttributes.GetElement("dmiStudyAttributes");
            // set intraday data price sources for study
            dmiStudy.SetElement("period", 14); // DMI study interval
            dmiStudy.SetElement("priceSourceHigh", "high");
            dmiStudy.SetElement("priceSourceClose", "close");
            dmiStudy.SetElement("priceSourceLow", "low");

            return request;            
        }

        // Create Technical Analysis Intraday - SMAVG Study Request
        Request createSMAVGStudyRequest(Service tasvcService)
        {
            Request request = tasvcService.CreateRequest("studyRequest");

            Element priceSource = request.GetElement("priceSource");
            // set security name
            priceSource.SetElement("securityName", "IBM US Equity");

            Element dataRange = priceSource.GetElement("dataRange");
            dataRange.SetChoice("intraday");

            // set intraday price data
            Element intraday = dataRange.GetElement("intraday");
            intraday.SetElement("eventType", "TRADE"); // intraday event type
            intraday.SetElement("interval", 60); // intraday interval
            intraday.SetElement("startDate", "2010-05-26T13:30:00"); // set study start date
            intraday.SetElement("endDate", "2010-05-27T13:30:00"); // set study start date

            // SMAVG study example - set study attributes
            Element studyAttributes = request.GetElement("studyAttributes");
            studyAttributes.SetChoice("smavgStudyAttributes");

            Element smavgStudy = studyAttributes.GetElement("smavgStudyAttributes");
            // set intraday data price sources for study
            smavgStudy.SetElement("period", 14); // SMAVG study interval
            smavgStudy.SetElement("priceSourceClose", "close");

            return request;
        }

        /// <summary>
        /// Polls for an event or a message in an event loop
        /// & Processes the event generated
        /// </summary>
        /// <param name="session"></param>
        private void eventLoop(Session session)
        {
            bool done = false;
            while (!done)
            {
                Event eventObj = session.NextEvent();
                if (eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    System.Console.WriteLine("Processing Partial Response");
                    processResponseEvent(eventObj);
                }
                else if (eventObj.Type == Event.EventType.RESPONSE)
                {
                    System.Console.WriteLine("Processing Response");
                    processResponseEvent(eventObj);
                    done = true;
                }
                else
                {
                    foreach (Message msg in eventObj.GetMessages())
                    {
                        System.Console.WriteLine(msg.AsElement);
                        if (eventObj.Type == Event.EventType.SESSION_STATUS)
                        {
                            if (msg.MessageType.Equals("SessionTerminated"))
                            {
                                System.Console.WriteLine("SessionTerminated...Exiting");
                                Environment.Exit(0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function to handle response event
        /// </summary>
        /// <param name="eventObj"></param>
        private void processResponseEvent(Event eventObj)
        {
            foreach (Message msg in eventObj.GetMessages())
            {
                if (msg.HasElement(RESPONSE_ERROR))
                {
                    printErrorInfo("REQUEST FAILED: ", msg.GetElement(RESPONSE_ERROR));
                    continue;
                }

                Element security = msg.GetElement(SECURITY_NAME);
                string ticker = security.GetValueAsString();
                System.Console.WriteLine("\nTicker: " + ticker);
                if (security.HasElement("securityError"))
                {
                    printErrorInfo("\tSECURITY FAILED: ",
                        security.GetElement(SECURITY_ERROR));
                    continue;
                }

                Element fields = msg.GetElement(STUDY_DATA);
                if (fields.NumValues > 0)
                {
                    int numValues = fields.NumValues;
                    for (int j = 0; j < numValues; ++j)
                    {
                        Element field = fields.GetValueAsElement(j);
                        for (int k = 0; k < field.NumElements; k++)
                        {
                            Element element = field.GetElement(k);
                            System.Console.WriteLine("\t" + element.Name + " = " +
                                element.GetValueAsString());
                        }
                        System.Console.WriteLine("");
                    }
                }
                System.Console.WriteLine("");
                Element fieldExceptions = msg.GetElement(FIELD_EXCEPTIONS);
                if (fieldExceptions.NumValues > 0)
                {
                    System.Console.WriteLine("FIELD\t\tEXCEPTION");
                    System.Console.WriteLine("-----\t\t---------");
                    for (int k = 0; k < fieldExceptions.NumValues; ++k)
                    {
                        Element fieldException =
                            fieldExceptions.GetValueAsElement(k);
                        printErrorInfo(fieldException.GetElementAsString(FIELD_ID) +
                            "\t\t", fieldException.GetElement(ERROR_INFO));
                    }
                }
            }
        }

        private void printErrorInfo(string leadingStr, Element errorInfo)
        {
            System.Console.WriteLine(leadingStr + errorInfo.GetElementAsString(CATEGORY) +
                " (" + errorInfo.GetElementAsString(MESSAGE) + ")");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool parseCommandLine(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (string.Compare(args[i], "-ip", true) == 0
					&& i + 1 < args.Length)
                {
                    d_host = args[++i];
                }
                else if (string.Compare(args[i], "-p", true) == 0
					&& i + 1 < args.Length)
                {
                    int outPort = 0;
                    if (int.TryParse(args[++i], out outPort))
                    {
                        d_port = outPort;
                    }
                }
                if (string.Compare(args[i], "-h", true) == 0)
                {
                    printUsage();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("  Technical Analysis Intraday Study Example ");
            System.Console.WriteLine("          [-ip            <ipAddress      = localhost>");
            System.Console.WriteLine("          [-p             <tcpPort        = 8194>");
        }
    }
}