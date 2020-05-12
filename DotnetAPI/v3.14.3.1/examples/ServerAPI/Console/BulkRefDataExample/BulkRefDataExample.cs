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
** BulfRefDataExample.cs
**
** This Example shows how to Retrieve reference data/Bulk reference data using Server API
** Usage: 
**      		-s			<security	= CAC Index>
**      		-f			<field		= INDX_MWEIGHT>
**      		-ip 		<ipAddress	= localhost>
**      		-p 			<tcpPort	= 8194>
** e.g. BulfRefDataExample -s "CAC Index" -f INDX_MWEIGHT -ip localhost -p 8194
*/

using Event = Bloomberglp.Blpapi.Event;
using Element = Bloomberglp.Blpapi.Element;
using InvalidRequestException = Bloomberglp.Blpapi.InvalidRequestException;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using Datatype = Bloomberglp.Blpapi.Schema.Datatype;

using ArrayList = System.Collections.ArrayList;

namespace Bloomberglp.Blpapi.Examples
{
    public class BulkRefDataExample
    {
        private static readonly Name SECURITY_DATA = Name.GetName("securityData");
        private static readonly Name SECURITY = Name.GetName("security");
        private static readonly Name FIELD_DATA = Name.GetName("fieldData");
        private static readonly Name RESPONSE_ERROR = Name.GetName("responseError");
        private static readonly Name SECURITY_ERROR = Name.GetName("securityError");
        private static readonly Name FIELD_EXCEPTIONS = Name.GetName("fieldExceptions");
        private static readonly Name FIELD_ID = Name.GetName("fieldId");
        private static readonly Name ERROR_INFO = Name.GetName("errorInfo");
        private static readonly Name CATEGORY = Name.GetName("category");
        private static readonly Name MESSAGE = Name.GetName("message");

        private string d_host;
        private int d_port;
        private ArrayList d_securities;
        private ArrayList d_fields;

        public static void Main(string[] args)
        {
            System.Console.WriteLine("Reference Data/Bulk Reference Data Example");
            BulkRefDataExample example = new BulkRefDataExample();
            example.run(args);

            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BulkRefDataExample()
        {
            d_host = "localhost";
            d_port = 8194;
            d_securities = new ArrayList();
            d_fields = new ArrayList();
        }

        /// <summary>
        /// Read command line arguments, 
        /// Establish a Session
        /// Identify and Open refdata Service
        /// Send ReferenceDataRequest to the Service 
        /// Event Loop and Response Processing
        /// </summary>
        /// <param name="args"></param>
        private void run(string[] args)
        {
            if (!parseCommandLine(args)) return;

            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerHost = d_host;
            sessionOptions.ServerPort = d_port;

            System.Console.WriteLine("Connecting to " + d_host + ":" + d_port);
            Session session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                System.Console.Error.WriteLine("Failed to start session.");
                return;
            }
            if (!session.OpenService("//blp/refdata"))
            {
                System.Console.Error.WriteLine("Failed to open //blp/refdata");
                return;
            }

            try
            {
                sendRefDataRequest(session);
            }
            catch (InvalidRequestException e)
            {
                System.Console.WriteLine(e.ToString());
            }

            // wait for events from session.
            eventLoop(session);

            session.Stop();
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
                    foreach (Message msg in eventObj)
                    {
                        System.Console.WriteLine(msg.AsElement);
                        if (eventObj.Type == Event.EventType.SESSION_STATUS)
                        {
                            if (msg.MessageType.Equals("SessionTerminated"))
                            {
                                done = true;
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
            foreach (Message msg in eventObj)
            {
                if (msg.HasElement(RESPONSE_ERROR))
                {
                    printErrorInfo("REQUEST FAILED: ", msg.GetElement(RESPONSE_ERROR));
                    continue;
                }

                Element securities = msg.GetElement(SECURITY_DATA);
                int numSecurities = securities.NumValues;
                System.Console.WriteLine("\nProcessing " + numSecurities
                                            + " securities:");
                for (int secCnt = 0; secCnt < numSecurities; ++secCnt)
                {
                    Element security = securities.GetValueAsElement(secCnt);
                    string ticker = security.GetElementAsString(SECURITY);
                    System.Console.WriteLine("\nTicker: " + ticker);
                    if (security.HasElement("securityError"))
                    {
                        printErrorInfo("\tSECURITY FAILED: ",
                            security.GetElement(SECURITY_ERROR));
                        continue;
                    }

                    Element fields = security.GetElement(FIELD_DATA);
                    if (fields.NumElements > 0)
                    {
                        System.Console.WriteLine("FIELD\t\tVALUE");
                        System.Console.WriteLine("-----\t\t-----");
                        int numElements = fields.NumElements;
                        for (int eleCtr = 0; eleCtr < numElements; ++eleCtr)
                        {
                            Element field = fields.GetElement(eleCtr);
                            // Checking if the field is Bulk field
                            if (field.Datatype == Datatype.SEQUENCE)
                            {
                                processBulkField(field);
                            }
                            else
                            {
                                processRefField(field);
                            }
                        }
                    }

                    System.Console.WriteLine("");
                    Element fieldExceptions = security.GetElement(FIELD_EXCEPTIONS);
                    if (fieldExceptions.NumValues > 0)
                    {
                        System.Console.WriteLine("FIELD\t\tEXCEPTION");
                        System.Console.WriteLine("-----\t\t---------");
                        for (int k = 0; k < fieldExceptions.NumValues; ++k)
                        {
                            Element fieldException =
                                fieldExceptions.GetValueAsElement(k);
                            printErrorInfo(fieldException.GetElementAsString(FIELD_ID)
                                + "\t\t", fieldException.GetElement(ERROR_INFO));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Read the reference bulk field contents
        /// </summary>
        /// <param name="refBulkField"></param>
        private void processBulkField(Element refBulkField)
        {
            System.Console.WriteLine("\n" + refBulkField.Name);
            // Get the total number of Bulk data points
            int numofBulkValues = refBulkField.NumValues;
            for (int bvCtr = 0; bvCtr < numofBulkValues; bvCtr++)
            {
                Element bulkElement = refBulkField.GetValueAsElement(bvCtr);
                // Get the number of sub fields for each bulk data element
                int numofBulkElements = bulkElement.NumElements;
                // Read each field in Bulk data
                for (int beCtr = 0; beCtr < numofBulkElements; beCtr++)
                {
                    Element elem = bulkElement.GetElement(beCtr);
                    System.Console.WriteLine("\t\t" + elem.Name + " = "
                                            + elem.GetValueAsString());
                }
            }
        }

        /// <summary>
        /// Read the reference field contents
        /// </summary>
        /// <param name="reffield"></param>
        private void processRefField(Element reffield)
        {
            System.Console.WriteLine(reffield.Name + "\t\t"
                                    + reffield.GetValueAsString());

        }

        /// <summary>
        /// Function to create and send ReferenceDataRequest
        /// </summary>
        /// <param name="session"></param>
        private void sendRefDataRequest(Session session)
        {
            Service refDataService = session.GetService("//blp/refdata");
            Request request = refDataService.CreateRequest("ReferenceDataRequest");

            // Add securities to request
            Element securities = request.GetElement("securities");

            for (int i = 0; i < d_securities.Count; ++i)
            {
                securities.AppendValue((string)d_securities[i]);
            }

            // Add fields to request
            Element fields = request.GetElement("fields");
            for (int i = 0; i < d_fields.Count; ++i)
            {
                fields.AppendValue((string)d_fields[i]);
            }

            System.Console.WriteLine("Sending Request: " + request);
            session.SendRequest(request, null);
        }

        /// <summary>
        /// Parses the command line arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool parseCommandLine(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (string.Compare(args[i], "-s", true) == 0
					&& i + 1 < args.Length)
                {
                    d_securities.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-f", true) == 0
					&& i + 1 < args.Length)
                {
                    d_fields.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-ip", true) == 0
					&& i + 1 < args.Length)
                {
                    d_host = args[++i];
                }
                else if (string.Compare(args[i], "-p", true) == 0
					&& i + 1 < args.Length)
                {
                    d_port = int.Parse(args[++i]);
                }
                else if (string.Compare(args[i], "-h", true) == 0)
                {
                    printUsage();
                    return false;
                }
            }

            // handle default arguments
            if (d_securities.Count == 0)
            {
                d_securities.Add("CAC Index");
            }

            if (d_fields.Count == 0)
            {
                d_fields.Add("INDX_MWEIGHT");
            }

            return true;
        }

        /// <summary>
        /// Prints error information
        /// </summary>
        /// <param name="leadingStr"></param>
        /// <param name="errorInfo"></param>
        private void printErrorInfo(string leadingStr, Element errorInfo)
        {
            System.Console.WriteLine(leadingStr + errorInfo.GetElementAsString(CATEGORY) +
                " (" + errorInfo.GetElementAsString(MESSAGE) + ")");
        }

        /// <summary>
        /// Print usage of the Program
        /// </summary>
        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("	Retrieve reference data/Bulk reference"
                                    + " data using Server API");
            System.Console.WriteLine("      [-s         <security   = CAC Index>");
            System.Console.WriteLine("      [-f         <field      = INDX_MWEIGHT>");
            System.Console.WriteLine("      [-ip        <ipAddress  = localhost>");
            System.Console.WriteLine("      [-p         <tcpPort    = 8194>");
        }
    }
}

