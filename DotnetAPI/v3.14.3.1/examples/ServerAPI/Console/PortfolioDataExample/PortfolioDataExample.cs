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
** PortfolioDataExample: 
**  This program shows client how
**    1. To download their portfolio holdings using API.
**    2. To view their portfolio positions as of a specific date in order to see 
**       how current market movements have affected their portfolio's constituent 
**       weights.
**
**    It uses Reference Data Service(//blp/refdata) provided by API.
**    It does following:
**        1. Establishing a session which facilitates connection to the bloomberg 
**           network
**        2. Initiating the Reference Data Service(//blp/refdata) for static data.
**        3. Creating and sending request to the session.  
**            - Creating 'PortfolioDataRequest' request 
**            - Adding porfolio tickers/porfolio fields to request
**            - Sending the request
**        4. Event Handling of the responses received.
**
** * The fields available are
**      - PORTFOLIO_MEMBERS: Returns a list of Bloomberg identifiers representing 
**        the members of a user's custom portfolio.
**      - PORTFOLIO_MPOSITION: Returns a list of Bloomberg identifiers 
**        representing the members of a user's custom portfolio as well as the  
**        position for each security in the user's custom portfolio.
**      - PORTFOLIO_MWEIGHT: Returns a list of Bloomberg identifiers representing 
**        the members of a user's custom portfolio as well as the percentage 
**        weight for each security in the user's custom portfolio.
**      - PORTFOLIO_DATA: Returns a list of the Bloomberg identifiers, positions, 
**        market values, cost, cost date, and cost foreign exchange rate of each 
**        security in a user's custom portfolio. 
** 
** Usage: 
**  Retrieve portfolio data
**      [-s         <security       = UXXXXXXX-X Client>
**      [-f         <field          = PORTFOLIO_DATA>
**      [-o         <Reference Date = 20091101>
**      [-ip        <ipAddress      = localhost>
**      [-p         <tcpPort        = 8194>
** 
** * Note:The user's portfolio is identified by its Portfolio ID, which can be
**        found on the upper right hand corner of the toolbar on the portfolio's 
**        PRTU page. This information can also be accessed historically by using 
**        the REFERENCE_DATE override field and supplying the date in ÅeYYYYMMDD' 
**        format. Run {DOCS #2054005 <GO>} for an example of an API spreadsheet 
**        with the new portfolio fields.
** 
** Example usage:
**    PortfolioDataRequest -h
**       Print the usage for the program on the console
**
**    PortfolioDataRequest
**       Run the program with default values specified for security and fields. 
**       Parses the response of PortfolioDataRequest & 
**       prints the response message on the console. 	   
**
**    PortfolioDataRequest -ip localhost -p 8194 -s "5497224-1 Client" 
**                          -f PORTFOLIO_MEMBERS -f PORTFOLIO_DATA 
**       Download the portfolio holdings
** 
**    PortfolioDataRequest -s "5497224-1 Client" -f PORTFOLIO_MPOSITION -o 20091101
**       Specifying the REFERENCE_DATE override to view portfolio positions
**       as of a specific date
**
**    Program prints the response on the console of the command line requested data
**
******************************************************************************/

using Event = Bloomberglp.Blpapi.Event;
using Element = Bloomberglp.Blpapi.Element;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;

using ArrayList = System.Collections.ArrayList;
using Datatype = Bloomberglp.Blpapi.Schema.Datatype;

namespace Bloomberglp.Blpapi.Examples
{

    public class PortfolioDataExample
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
        private string d_override;

        public static void Main(string[] args)
        {
            System.Console.WriteLine("Portfolio Data Example");
            PortfolioDataExample example = new PortfolioDataExample();
            example.run(args);

            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        public PortfolioDataExample()
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
        /// Send PortfolioDataRequest to the Service 
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
                sendPortfolioDataRequest(session);
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
                    foreach (Message msg in eventObj.GetMessages())
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
            foreach (Message msg in eventObj.GetMessages())
            {
                if (msg.HasElement(RESPONSE_ERROR))
                {
                    printErrorInfo("REQUEST FAILED: ", msg.GetElement(RESPONSE_ERROR));
                    continue;
                }

                Element securities = msg.GetElement(SECURITY_DATA);
                int numSecurities = securities.NumValues;
                System.Console.WriteLine("Processing " + numSecurities + " securities:");
                for (int i = 0; i < numSecurities; ++i)
                {
                    Element security = securities.GetValueAsElement(i);
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
                        for (int j = 0; j < numElements; ++j)
                        {
                            Element field = fields.GetElement(j);
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
                            printErrorInfo(fieldException.GetElementAsString(FIELD_ID) +
                                "\t\t", fieldException.GetElement(ERROR_INFO));
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
                System.Console.WriteLine();
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
        /// Function to create and send PortfolioDataRequest
        /// </summary>
        /// <param name="session"></param>
        private void sendPortfolioDataRequest(Session session)
        {
            Service refDataService = session.GetService("//blp/refdata");
            Request request = refDataService.CreateRequest("PortfolioDataRequest");

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

            // If specified, use REFERENCE_DATE override field 
            // to get portfolio information historically.
            // The date must be in 'YYYYMMDD' format
            if (d_override != null && d_override.Length != 0)
            {
                Element overrides = request["overrides"];
                Element override1 = overrides.AppendElement();
                override1.SetElement("fieldId", "REFERENCE_DATE");
                override1.SetElement("value", d_override);
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
                if (string.Compare(args[i], "-s", true) == 0 && (i + 1) < args.Length)
                {
                    d_securities.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-f", true) == 0 && (i + 1) < args.Length)
                {
                    d_fields.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-o", true) == 0 && (i + 1) < args.Length)
                {
                    d_override = args[++i];
                }
                else if (string.Compare(args[i], "-ip", true) == 0 && (i + 1) < args.Length)
                {
                    d_host = args[++i];
                }
                else if (string.Compare(args[i], "-p", true) == 0 && (i + 1) < args.Length)
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
                d_securities.Add("U5497224-1 Client");
            }

            if (d_fields.Count == 0)
            {
                d_fields.Add("PORTFOLIO_MEMBERS");
                d_fields.Add("PORTFOLIO_MPOSITION");
                d_fields.Add("PORTFOLIO_MWEIGHT");
                d_fields.Add("PORTFOLIO_DATA");
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
            System.Console.WriteLine("  Retrieve Portfolio data ");
            System.Console.WriteLine("      [-s         <security       = \"UXXXXXXX-X Client\">");
            System.Console.WriteLine("      [-f         <field          = PORTFOLIO_DATA>");
            System.Console.WriteLine("      [-o         <Reference Date = 20091101>");
            System.Console.WriteLine("      [-ip        <ipAddress      = localhost>");
            System.Console.WriteLine("      [-p         <tcpPort        = 8194>");
            System.Console.WriteLine("Notes: ");
            System.Console.WriteLine("1) Multiple securities & fields can be specified");
            System.Console.WriteLine("2) The user's portfolio is identified by its Portfolio ID, which can be");
            System.Console.WriteLine("   found on the upper right hand corner of the toolbar on the portfolio's");
            System.Console.WriteLine("   PRTU page. This information can also be accessed historically by using");
            System.Console.WriteLine("   the REFERENCE_DATE override field[-o] & supplying the date in 'YYYYMMDD'");
            System.Console.WriteLine("   format.");
       }
    }
}