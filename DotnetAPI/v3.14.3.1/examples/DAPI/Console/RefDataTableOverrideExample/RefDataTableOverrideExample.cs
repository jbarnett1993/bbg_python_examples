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
** RefDataTableOverrideExample.cs
**
** This Example shows how to retrieve bulk data(data-set) using scalar 
** and table overrides in V3 API
** Usage: 
**      		-s			<security	= CWHL 2006-20 1A1 Mtge>
**      		-f			<field		= MTG_CASH_FLOW>
**      		-ip 		<ipAddress	= localhost>
**      		-p 			<tcpPort	= 8194>
** e.g. BulfRefDataExample -s "CWHL 2006-20 1A1 Mtge" -f MTG_CASH_FLOW -ip localhost -p 8194
*/
using Element = Bloomberglp.Blpapi.Element;
using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using Datatype = Bloomberglp.Blpapi.Schema.Datatype;

using ArrayList = System.Collections.ArrayList;

namespace Bloomberglp.Blpapi.Examples
{
    /// <summary>
    /// Structure for user to enter default vector of rate, 
    /// duration, and transition which is incorporated into mortgage
    /// analysis scenarios.
    /// </summary>
    public struct RateVector
    {
        public float rate;
        public int duration;
        public string transition;

        public RateVector(float rate, int duration, string transition)
        {
            this.rate = rate;
            this.duration = duration;
            this.transition = transition;
        }
    } 

    public class RefDataTableOverrideExample
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

         /// <summary>
        /// Constructor
        /// </summary>
        public RefDataTableOverrideExample()
        {
            d_host = "localhost";
            d_port = 8194;
            d_securities = new ArrayList();
            d_fields = new ArrayList();
        }
        public static void Main(string[] args)
        {
            RefDataTableOverrideExample example = new RefDataTableOverrideExample();
            example.run(args);
            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        /// <summary>
        /// Function to handle response event. This function gets the message 
        /// from the event and traverse the message to get the data from 
        /// reference and bulk data field.
        /// </summary>
        /// <param name="eventObj"></param>
        private void processResponseEvent(Event eventObj)
        {
            // Iterate through messages received
            foreach (Message msg in eventObj)
            {
                // If a request cannot be completed for any reason, the responseError
                // element is returned in the response. responseError contains detailed 
                // information regarding the failure.
                if (msg.HasElement(RESPONSE_ERROR))
                {
                    printErrorInfo("REQUEST FAILED: ", msg.GetElement(RESPONSE_ERROR));
                    continue;
                }
                // Get the number of securities received in message
                Element securities = msg.GetElement(SECURITY_DATA);
                int numSecurities = securities.NumValues;
                System.Console.WriteLine("\nProcessing " + numSecurities
                                            + " securities:");
                for (int secCnt = 0; secCnt < numSecurities; ++secCnt)
                {
                    // Get security element
                    Element security = securities.GetValueAsElement(secCnt);
                    string ticker = security.GetElementAsString(SECURITY);
                    System.Console.WriteLine("\nTicker: " + ticker);
                    // Checking if there is any Security Error
                    if (security.HasElement("securityError"))
                    {
                        printErrorInfo("\tSECURITY FAILED: ",
                            security.GetElement(SECURITY_ERROR));
                        continue;
                    }
                    // Get fieldData Element
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
                    // Get fieldException Element
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
        /// This function traverse through the bulk data set and 
        /// and prints each field in the bulk data on the console.
        /// </summary>
        /// <param name="refBulkField"></param>
        private void processBulkField(Element refBulkField)
        {
            System.Console.WriteLine("\n" + refBulkField.Name);
            // Get the total number of bulk data points
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
        /// This function reads reference field element and prints it 
        /// on the console.
        /// </summary>
        /// <param name="reffield"></param>
        private void processRefField(Element reffield)
        {
            System.Console.WriteLine(reffield.Name + "\t\t"
                                    + reffield.GetValueAsString());

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
            // Create and start the session
            Session session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                System.Console.WriteLine("Failed to start session.");
                return;
            }
            // open the Reference Data Service
            if (!session.OpenService("//blp/refdata"))
            {
                System.Console.Error.WriteLine("Failed to open //blp/refdata");
                return;
            }
            try
            {
                sendRefDataTableOverrideRequest(session);
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
        /// & processes the event generated
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
        /// Function to create and send ReferenceDataRequest with scalar and 
        /// table overrides
        /// </summary>
        /// <param name="session"></param>
        private void sendRefDataTableOverrideRequest(Session session)
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

            // Add scalar overrides to request.
            Element overrides = request["overrides"];
            Element override1 = overrides.AppendElement();
            override1.SetElement("fieldId", "ALLOW_DYNAMIC_CASHFLOW_CALCS");
            override1.SetElement("value", "Y");
            Element override2 = overrides.AppendElement();
            override2.SetElement("fieldId", "LOSS_SEVERITY");
            override2.SetElement("value", 31);

            // Add table overrides to request.
            Element tableOverrides = request.GetElement("tableOverrides");
            Element tableOverride = tableOverrides.AppendElement();         
            tableOverride.SetElement("fieldId", "DEFAULT_VECTOR");
            Element rows = tableOverride.GetElement("row");

            // intialise the rate vector
            RateVector[] rateVector = {new RateVector(1.0F, 12, "S"),    // S = Step
                                       new RateVector(2.0F, 12, "R")};   // R = Ramp 
           
            /* Vector attributes are specified in the first three rows of the overrides
             * in the format as follows:
             * Row 1: Anchor - {Anchor Type} - 
             * Row 2: Type - {Default Type} -
             * Row 3: Rate - Duration - Transition-
               ------------------------------
               |Anchor |PROJ     |<blank>    |
               ------------------------------
               |Type   |CDR	     |<blank>    |
               ------------------------------
               |Rate   |Duration |Transition |
               ------------------------------
               |  1    |  12     |  S        |
               ------------------------------
               |  1    |  12     |  R        |
               ------------------------------
             */
            Element row = rows.AppendElement();
            Element cols = row.GetElement("value");
            cols.AppendValue("Anchor");  // Anchor type
            cols.AppendValue("PROJ");    // PROJ = Projected
            row = rows.AppendElement();
            cols = row.GetElement("value");
            cols.AppendValue("Type");    // Type of default
            cols.AppendValue("CDR");     // CDR = Conditional Default Rate

            foreach (RateVector vector in rateVector)
            {

                row = rows.AppendElement();
                cols = row.GetElement("value");
                cols.AppendValue(vector.rate);
                cols.AppendValue(vector.duration);
                cols.AppendValue(vector.transition);
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
                    int outPort = 0;
                    if (int.TryParse(args[++i], out outPort))
                    {
                        d_port = outPort;
                    }
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
                d_securities.Add("CWHL 2006-20 1A1 Mtge");
            }

            if (d_fields.Count == 0)
            {
                d_fields.Add("MTG_CASH_FLOW");
                d_fields.Add("SETTLE_DT");
            }

            return true;
        }
        /// <summary>
        /// Print usage of the Program
        /// </summary>
        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("	Retrieve Bulk reference data using scalar and table overrides"
                                    + " from Desktop/Server API");
            System.Console.WriteLine("      [-s         <security	= CWHL 2006-20 1A1 Mtge>");
            System.Console.WriteLine("      [-f         <field		= MTG_CASH_FLOW>");
            System.Console.WriteLine("      [-ip        <ipAddress	= localhost>");
            System.Console.WriteLine("      [-p         <tcpPort	= 8194>");
        }
    }
}
