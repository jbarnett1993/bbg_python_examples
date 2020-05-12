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
 PortfolioDataExample: 
    This program shows client how
    1. To download their portfolio holdings using API.
    2. To view their portfolio positions as of a specific date in order to see 
       how current market movements have affected their portfolio's constituent 
       weights.

    It uses Reference Data Service(//blp/refdata) provided by API.
    It does following:
        1. Establishing a session which facilitates connection to the bloomberg 
           network
        2. Initiating the Reference Data Service(//blp/refdata) for static data.
        3. Creating and sending request to the session.  
            - Creating 'PortfolioDataRequest' request 
            - Adding porfolio tickers/porfolio fields to request
            - Sending the request
        4. Event Handling of the responses received.

 * The fields available are
      - PORTFOLIO_MEMBERS: Returns a list of Bloomberg identifiers representing 
        the members of a user's custom portfolio.
      - PORTFOLIO_MPOSITION: Returns a list of Bloomberg identifiers 
        representing the members of a user's custom portfolio as well as the  
        position for each security in the user's custom portfolio.
      - PORTFOLIO_MWEIGHT: Returns a list of Bloomberg identifiers representing 
        the members of a user's custom portfolio as well as the percentage 
        weight for each security in the user's custom portfolio.
      - PORTFOLIO_DATA: Returns a list of the Bloomberg identifiers, positions, 
        market values, cost, cost date, and cost foreign exchange rate of each 
        security in a user's custom portfolio. 
 
 Usage: 
  Retrieve portfolio data
      [-s         <security       = UXXXXXXX-X Client>
      [-f         <field          = PORTFOLIO_DATA>
      [-o         <Reference Date = 20091101>
      [-ip        <ipAddress      = localhost>
      [-p         <tcpPort        = 8194>
 
 * Note:The user's portfolio is identified by its Portfolio ID, which can be
        found on the upper right hand corner of the toolbar on the portfolio's 
        PRTU page. This information can also be accessed historically by using 
        the REFERENCE_DATE override field and supplying the date in ‘YYYYMMDD' 
        format. Run {DOCS #2054005 <GO>} for an example of an API spreadsheet 
        with the new portfolio fields.
 
 Example usage:
    PortfolioDataRequest -h
       Print the usage for the program on the console

    PortfolioDataRequest
       Run the program with default values specified for security and fields. 
       Parses the response of PortfolioDataRequest & 
       prints the response message on the console. 	   

    PortfolioDataRequest -ip localhost -p 8194 -s "5497224-1 Client" 
                          -f PORTFOLIO_MEMBERS -f PORTFOLIO_DATA 
       Download the portfolio holdings
 
    PortfolioDataRequest -s "5497224-1 Client" -f PORTFOLIO_MPOSITION -o 20091101
       Specifying the REFERENCE_DATE override to view portfolio positions
       as of a specific date

    Program prints the response on the console of the command line requested data

******************************************************************************/
package com.bloomberglp.blpapi.examples;

import java.util.ArrayList;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.InvalidRequestException;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Schema.Datatype;

public class PortfolioDataExample
{
    private static final Name SECURITY_DATA = new Name("securityData");
    private static final Name SECURITY = new Name("security");
    private static final Name FIELD_DATA = new Name("fieldData");
    private static final Name RESPONSE_ERROR = new Name("responseError");
    private static final Name SECURITY_ERROR = new Name("securityError");
    private static final Name FIELD_EXCEPTIONS = new Name("fieldExceptions");
    private static final Name FIELD_ID = new Name("fieldId");
    private static final Name ERROR_INFO = new Name("errorInfo");
    private static final Name CATEGORY = new Name("category");
    private static final Name MESSAGE = new Name("message");

    private String    d_host;
    private int       d_port;
    private ArrayList d_securities;
    private ArrayList d_fields;
    private String    d_override;

    /**
     * @param args
     */
    public static void main(String[] args) throws Exception
    {
        System.out.println("Portfolio Data Example");
        PortfolioDataExample example = new PortfolioDataExample();
        example.run(args);

        System.out.println("Press ENTER to quit");
        System.in.read();
    }

    /**
     * Constructor
     */
    public PortfolioDataExample()
    {
        d_host = "localhost";
        d_port = 8194;
        d_securities = new ArrayList();
        d_fields = new ArrayList();
    }

    /**
     * Reads command line arguments 
     * Establish a Session
     * Identify and Open refdata Service
     * Send PortfolioDataRequest to the Service 
     * Event Loop and Response Processing
     */
    private void run(String[] args) throws Exception
    {
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
        if (!session.openService("//blp/refdata")) {
            System.err.println("Failed to open //blp/refdata");
            return;
        }

        try {
            sendPortfolioDataRequest(session);
        } catch (InvalidRequestException e) {
            e.printStackTrace();
        }

        // wait for events from session.
        eventLoop(session);

        session.stop();
    }

    /**
     * Polls for an event or a message in an event loop
     */
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
                        if (msg.messageType().equals("SessionTerminated") ||
                            msg.messageType().equals("SessionStartupFailure")) {
                            done = true;
                        }
                    }
                }
            }
        }
    }

    /**
     * Function to handle response event
     */
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

            Element securities = msg.getElement(SECURITY_DATA);
            int numSecurities = securities.numValues();
            System.out.println("Processing " + numSecurities + " securities:");
            for (int i = 0; i < numSecurities; ++i) {
                Element security = securities.getValueAsElement(i);
                String ticker = security.getElementAsString(SECURITY);
                System.out.println("\nTicker: " + ticker);
                if (security.hasElement("securityError")) {
                    printErrorInfo("\tSECURITY FAILED: ",
                                   security.getElement(SECURITY_ERROR));
                    continue;
                }

                if (security.hasElement(FIELD_DATA)) {
                    Element fields = security.getElement(FIELD_DATA);
                    if (fields.numElements() > 0) {
                        System.out.println("FIELD\tVALUE");
                        System.out.println("-----\t-----");
                        int numElements = fields.numElements();
                        for (int j = 0; j < numElements; ++j) {
                            Element field = fields.getElement(j);
                            // Checking if the field is Bulk field
                            if(field.datatype() == Datatype.SEQUENCE){
                                processBulkField(field);
                            }else{
                                processRefField(field);
                            }
                        }
                    }
                }
                System.out.println("");
                Element fieldExceptions = security.getElement(FIELD_EXCEPTIONS);
                if (fieldExceptions.numValues() > 0) {
                    System.out.println("FIELD\tEXCEPTION");
                    System.out.println("-----\t---------");
                    for (int k = 0; k < fieldExceptions.numValues(); ++k) {
                        Element fieldException =
                            fieldExceptions.getValueAsElement(k);
                        printErrorInfo(fieldException.getElementAsString(FIELD_ID) 
                                + "\t", fieldException.getElement(ERROR_INFO));
                    }
                }
            }
        }
    }

    /**
     * Read the reference bulk field data
     */
    private void processBulkField(Element refBulkField)
    {
        System.out.println("\n" + refBulkField.name());
        // Get the total number of Bulk data points
        int numofBulkValues = refBulkField.numValues();
        for (int bvCtr = 0; bvCtr < numofBulkValues; bvCtr++){
            Element bulkElement = refBulkField.getValueAsElement(bvCtr);
            // Get the number of sub fields for each bulk data element
            int numofBulkElements = bulkElement.numElements();
            // Read each field in Bulk data
            for (int beCtr = 0; beCtr < numofBulkElements; beCtr++){
                Element elem = bulkElement.getElement(beCtr);
                System.out.println("\t" + elem.name() + 
                                " = " + elem.getValueAsString());
            }
            System.out.println();
        }
    }

    /**
     * Read the reference field data
     */
   private void processRefField(Element reffield)
    {
        System.out.println(reffield.name() + "\t" 
                            + reffield.getValueAsString());
    }

   /**
    * Function to send PortfolioDataRequest
    */
  private void sendPortfolioDataRequest(Session session) throws Exception
    {
        Service refDataService = session.getService("//blp/refdata");
        Request request = refDataService.createRequest("PortfolioDataRequest");

        // Add securities to request
        Element securities = request.getElement("securities");

        for (int i = 0; i < d_securities.size(); ++i) {
            securities.appendValue((String)d_securities.get(i));
        }

        // Add fields to request
        Element fields = request.getElement("fields");
        for (int i = 0; i < d_fields.size(); ++i) {
            fields.appendValue((String)d_fields.get(i));
        }
        // If specified, use REFERENCE_DATE override field 
        // to get portfolio information historically.
        // The date must be in 'YYYMMDD' format
        if(d_override != null && d_override.length() != 0){
            Element overrides = request.getElement("overrides");
            Element override1 = overrides.appendElement();
            override1.setElement("fieldId", "REFERENCE_DATE");
            override1.setElement("value", d_override);
        }

        System.out.println("Sending Request: " + request);
        session.sendRequest(request, null);
    }

  /**
   * Parses the command line arguments
   */
   private boolean parseCommandLine(String[] args)
    {
        for (int i = 0; i < args.length; ++i) {
            if (args[i].equalsIgnoreCase("-s") && (i + 1) < args.length) {
                d_securities.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-f") && (i + 1) < args.length) {
                d_fields.add(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-o") && (i + 1) < args.length) {
                d_override = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-ip") && (i + 1) < args.length) {
                d_host = args[++i];
            }
            else if (args[i].equalsIgnoreCase("-p") && (i + 1) < args.length) {
                d_port = Integer.parseInt(args[++i]);
            }
            else if (args[i].equalsIgnoreCase("-h")) {
                printUsage();
                return false;
            }
        }

        // handle default arguments
        if (d_securities.size() == 0) {
            d_securities.add("U5497224-1 Client");
       }

        if (d_fields.size() == 0) {
            d_fields.add("PORTFOLIO_MEMBERS");
            d_fields.add("PORTFOLIO_MPOSITION");
            d_fields.add("PORTFOLIO_MWEIGHT");
            d_fields.add("PORTFOLIO_DATA");
        }

        return true;
    }

   /**
    * Prints Error Information
    */
   private void printErrorInfo(String leadingStr, Element errorInfo)
    throws Exception
    {
        System.out.println(leadingStr + errorInfo.getElementAsString(CATEGORY) +
                           " (" + errorInfo.getElementAsString(MESSAGE) + ")");
    }

   /**
    * Prints Program Usage
    */
   private void printUsage()
    {
        System.out.println("Usage:");
        System.out.println("   Retrieve Portfolio data");
        System.out.println("      [-s       <security       = \"UXXXXXXX-X Client\">");
        System.out.println("      [-f       <field          = PORTFOLIO_DATA>");
        System.out.println("      [-o       <Reference Date = 20091101>");
        System.out.println("      [-ip      <ipAddress      = localhost>");
        System.out.println("      [-p       <tcpPort        = 8194>");
        System.out.println("Notes: ");
        System.out.println("1) Multiple securities & fields can be specified");
        System.out.println("2) The user's portfolio is identified by its Portfolio ID, which can be");
        System.out.println("   found on the upper right hand corner of the toolbar on the portfolio's");
        System.out.println("   PRTU page. This information can also be accessed historically by using");
        System.out.println("   the REFERENCE_DATE override field[-o] & supplying the date in 'YYYYMMDD'");
        System.out.println("   format.");
    }
}
