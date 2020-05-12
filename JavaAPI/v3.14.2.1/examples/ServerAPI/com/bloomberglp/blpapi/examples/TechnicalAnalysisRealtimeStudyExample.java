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
** TechnicalAnalysisRealtimeStudyExample.java
**
** This Example shows how to use Technical Analysis service ("//blp/tasvc")
** to retrieve realtime data for specified study request.
** 
*/

package com.bloomberglp.blpapi.examples;

import java.util.ArrayList;
import java.util.Calendar;
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

public class TechnicalAnalysisRealtimeStudyExample {
	
    SimpleDateFormat dateFormat; 
    private String    d_host;
    private int       d_port;

    /**
     * @param args
     */
    public static void main(String[] args) throws Exception {
        System.out.println("Technical Analysis Real time Study Example ");
        TechnicalAnalysisRealtimeStudyExample example = new TechnicalAnalysisRealtimeStudyExample();
        example.run(args);

        System.out.println("Press ENTER to quit");
        System.in.read();

    }
    /**
     * Constructor
     */
    public TechnicalAnalysisRealtimeStudyExample()
    {
        d_host = "localhost";
        d_port = 8194;
        dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
    }

    /**
     * Reads command line arguments 
     * Establish a Session
     * Open tasvc Service
     * Subscribe to securities and fields with overrides specified
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
        if (!session.openService("//blp/tasvc")) {
            System.err.println("Failed to open //blp/tasvc");
            return;
        }
        sessionOptions.setDefaultSubscriptionService("//blp/tasvc");
       
        SubscriptionList subscriptions = new SubscriptionList();
        
        // Create Technical Analysis WLPR Study Subscription
        Subscription wlprSubscription = createWLPRStudySubscription();
        System.out.println("Subscribing to: " + wlprSubscription.subscriptionString());
        subscriptions.add(wlprSubscription);
        
        // Create Technical Analysis MAO Study Subscription
        Subscription maoSubscription = createMAOStudySubscription();
        System.out.println("Subscribing to: " + maoSubscription.subscriptionString());
        subscriptions.add(maoSubscription);

        // Create Technical Analysis EMAVG Study Subscription
        Subscription emavgSubscription = createEMAVGStudySubscription();
        System.out.println("Subscribing to: " + emavgSubscription.subscriptionString());
        subscriptions.add(emavgSubscription);

        // NOTE: User must be entitled to receive realtime data for securities subscribed
        session.subscribe(subscriptions);

        // wait for events from session.
        eventLoop(session);
    }

   // Create Technical Analysis WLPR Study Subscription 
   private Subscription createWLPRStudySubscription()
   {
	   Subscription wlprSubscription;
       ArrayList<String> fields = new ArrayList<String>();
       fields.add("WLPR");
       
       ArrayList<String> overrides = new ArrayList<String>();      
       overrides.add("priceSourceClose=LAST_PRICE");
       overrides.add("priceSourceHigh=HIGH");
       overrides.add("priceSourceLow=LOW");
       overrides.add("periodicitySelection=DAILY");
       overrides.add("period=14");
      
       wlprSubscription = new Subscription("IBM US Equity", //security
							       			fields ,        //field                           
							       			overrides,      //options
							       			new CorrelationID("IBM US Equity_WLPR"));
       return wlprSubscription;            
   }

   // Create Technical Analysis MAO Study Subscription 
   private Subscription createMAOStudySubscription()
   {
	   Subscription maoSubscription;
       ArrayList<String> fields = new ArrayList<String>();
       fields.add("MAO");
      
       ArrayList<String> overrides = new ArrayList<String>();
       overrides.add("priceSourceClose1=LAST_PRICE");
       overrides.add("priceSourceClose2=LAST_PRICE");
       overrides.add("maPeriod1=6");
       overrides.add("maPeriod2=36");
       overrides.add("maType1=Simple");
       overrides.add("maType2=Simple");
       overrides.add("oscType=Difference");
       overrides.add("periodicitySelection=DAILY");
       overrides.add("sigPeriod=9");
       overrides.add("sigType=Simple");
      
       maoSubscription = new Subscription("VOD LN Equity",  //security
							       			fields ,        //field                           
							       			overrides,      //options
							       			new CorrelationID("VOD LN Equity_MAO"));
       return maoSubscription;            
   }

   // Create Technical Analysis EMAVG Study Subscription 
   private Subscription createEMAVGStudySubscription()
   {
       Subscription emavgSubscription;
       ArrayList<String> fields = new ArrayList<String>();
       fields.add("EMAVG");

       ArrayList<String> overrides = new ArrayList<String>();
       overrides.add("priceSourceClose=LAST_PRICE");
       overrides.add("periodicitySelection=DAILY");
       overrides.add("period=14");

       emavgSubscription = new Subscription("6758 JT Equity", //security
                                            fields,           //field                           
                                            overrides,        //options
                                            new CorrelationID("6758 JT Equity_EMAVG"));
       return emavgSubscription;
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
                
                }else{
                    System.out.println("Processing " + event.eventType());
                    System.out.println(
                    		dateFormat.format(Calendar.getInstance().getTime()) +
                            ": " + msg.asElement() + "\n");
                }
            }
        }
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
        System.out.println("	Technical Analysis Real time Study Example ");
        System.out.println("		[-ip 		<ipAddress	= localhost>");
        System.out.println("		[-p 		<tcpPort	= 8194>");
    }
}
