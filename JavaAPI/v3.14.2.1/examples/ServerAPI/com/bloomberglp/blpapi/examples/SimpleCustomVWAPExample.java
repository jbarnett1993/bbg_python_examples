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
** SimpleCustomVWAPExample.java
 
**  This program demonstrate how to make subscription to particular 
**  security/ticker to get custom realtime VWAP updates using V3 API. 
**  It uses Market Data service(//blp/mktvwap) provided by API.
**  It does following:
**    1. Establishing a session which facilitates connection to the Bloomberg 
**       network.
**    2. Initiating the Market VWAP Service(//blp/mktvwap) for realtime
**       vwap data.
**    3. Creating and sending request to the session.
**        - Creating subscription list
**        - Add securities and vwap fields to subscription list
**        - Specifies VWAP Overrides option
**        - Subscribe to realtime data
**    4. Event Handling of the responses received.
** Usage: 
**  SimpleCustomVWAPExample -h
**     Print the usage for the program on the console
**
**  SimpleCustomVWAPExample
**     Run the program with default values. Prints the realtime VWAP updates 
**     on the console for three default securities specfied
**     1. Ticker - "IBM US Equity"
**     2. Ticker - "6758 JT Equity" 
**     3. Ticker - "VOD LN Equity"
**
**  Subscribing to Bloomberg defined VWAP and VWAP Volume
**  SimpleCustomVWAPExample -s "AAPL US Equity" -f VWAP -f RT_VWAP_VOLUME 
**                         -o VWAP_START_TIME=11:00
**  
**  Subscribing to Market defined VWAP and VWAP Volume
**  SimpleCustomVWAPExample -s "AAPL US Equity" -f MARKET_DEFINED_VWAP_REALTIME 
**                        -f RT_MKT_VWAP_VOLUME -o VWAP_START_TIME=11:00
**  
**  Subscribing to both Bloomberg defined & Market defined VWAP and VWAP Volume
**  SimpleCustomVWAPExample -s "AAPL US Equity" -f VWAP -f RT_VWAP_VOLUME 
**                      -f MARKET_DEFINED_VWAP_REALTIME -f RT_MKT_VWAP_VOLUME
**
**  Prints the response on the console of the command line requested data
*/

package com.bloomberglp.blpapi.examples;

import java.util.ArrayList;
import java.text.SimpleDateFormat;
import java.util.Date;

import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Subscription;
import com.bloomberglp.blpapi.SubscriptionList;

public class SimpleCustomVWAPExample {


    private String    d_host;
    private int       d_port;
    private ArrayList<String> d_securities;
    private ArrayList<String> d_fields;
    private ArrayList<String> d_overrides;
    SimpleDateFormat dateFormat; 

    /**
     * @param args
     */
    public static void main(String[] args) throws Exception {
        System.out.println("Custom VWAP Example");
        SimpleCustomVWAPExample example = new SimpleCustomVWAPExample();
        example.run(args);

        System.out.println("Press ENTER to quit");
        System.in.read();

    }
    /**
     * Constructor
     */
    public SimpleCustomVWAPExample()
    {
        d_host = "localhost";
        d_port = 8194;
        d_securities = new ArrayList<String>();
        d_fields = new ArrayList<String>();
        d_overrides = new ArrayList<String>();
        dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
    }

    /**
     * Reads command line arguments 
     * Establish a Session
     * Open mktvwap Service
     * Subscribe to securities and fields with VWAP overrides
     * Event Loop
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
        if (!session.openService("//blp/mktvwap")) {
            System.err.println("Failed to open //blp/mktvwap");
            return;
        }
        sessionOptions.setDefaultSubscriptionService("//blp/mktvwap");
       
        SubscriptionList subscriptions = new SubscriptionList();
        // User must be enabled for real-time data for the exchanges the securities
        // they are monitoring for custom VWAP trade. 
        // Otherwise, Subscription will fail for those securities and/or you will 
        // receive #N/A N/A instead of valid tick data. 
        for (int i=0; i<d_securities.size(); i++)    	   
        {
            String security = d_securities.get(i); 
            subscriptions.add(new Subscription(security,
                                                d_fields, 
                                                d_overrides, 
                                                new CorrelationID(security)));
        }

        session.subscribe(subscriptions);

        // wait for events from session.
        eventLoop(session);
    }

    /**
     * Polls for an event or a message in an event loop
     */
    private void eventLoop(Session session) throws Exception
    {
        while (true) {
            Event event = session.nextEvent();
            MessageIterator msgIter = event.messageIterator();
            while (msgIter.hasNext()) {
                Message msg = msgIter.next();
                if (event.eventType() == Event.EventType.SUBSCRIPTION_STATUS) {
                    System.out.println("Processing SUBSCRIPTION_STATUS");
                    String topic = (String)msg.correlationID().object();
                    String datetime = dateFormat.format(new Date()); 
                    System.out.println(datetime + ": " + topic + ": " + msg.asElement());
                }else if(event.eventType() == Event.EventType.SUBSCRIPTION_DATA){
                    System.out.println("Processing SUBSCRIPTION_DATA");
                    String topic = (String)msg.correlationID().object();
                    String datetime = dateFormat.format(new Date()); 
                    System.out.println(
                            datetime + ": " + topic + " - " + msg.messageType());
                    Element eleFields = msg.asElement();
                    for(int i=0; i<eleFields.numElements(); i++)
                    {
                        Element field = eleFields.getElement(i);
                        System.out.println("\t\t" + field.name() + " = " +
                            field.getValueAsString());
                    }
                
                }
            }
        }
    }

    /**
     * Parses the command line arguments
     */
     private boolean parseCommandLine(String[] args)
      {
          for (int i = 0; i < args.length; ++i) {
              if (args[i].equalsIgnoreCase("-s") && i + 1 < args.length) {
                  d_securities.add(args[++i]);
              }
              else if (args[i].equalsIgnoreCase("-f") && i + 1 < args.length) {
                  d_fields.add(args[++i]);
              }
              else if (args[i].equalsIgnoreCase("-o") && i + 1 < args.length) {
                  d_overrides.add(args[++i]);
              }
              else if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length) {
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

          // handle default arguments
          if (d_securities.size() == 0) {
              d_securities.add("IBM US Equity");
              d_securities.add("VOD LN Equity");
              d_securities.add("6758 JT Equity");
         }

          if (d_fields.size() == 0) {
              // Subscribing to Bloomberg defined VWAP and VWAP Volume
              d_fields.add("VWAP");
              d_fields.add("RT_VWAP_VOLUME");

              // Subscribing to Market defined VWAP and VWAP Volume
              d_fields.add("MARKET_DEFINED_VWAP_REALTIME");
              d_fields.add("RT_MKT_VWAP_VOLUME");
          }

          if (d_overrides.size() == 0) {
              d_overrides.add("VWAP_START_TIME=09:00");
          }

          return true;
      }

    /**
     * Prints Program Usage
     */
    private void printUsage()
     {
        System.out.println("Usage:");
        System.out.println("    Retrieve customized realtime vwap data using Bloomberg V3 API");
        System.out.println("      [-s         <security   = \"IBM US Equity\">]");
        System.out.println("      [-f         <field      = VWAP>]");
        System.out.println("      [-o         <overrides  = VWAP_START_TIME=09:00>]");
        System.out.println("      [-ip        <ipAddress  = localhost>]");
        System.out.println("      [-p         <tcpPort    = 8194>]");
        System.out.println("Notes:");
        System.out.println("Multiple securities, vwap fields & overrides can be specified.");
      }
}
