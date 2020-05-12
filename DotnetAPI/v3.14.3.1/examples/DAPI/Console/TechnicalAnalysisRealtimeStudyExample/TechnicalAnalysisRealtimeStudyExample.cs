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
** TechnicalAnalysisRealtimeStudyExample.cs
**
** This Example shows how to use Technical Analysis service ("//blp/tasvc")
** to retrieve realtime data for specified study request.
** 
*/
using System;
using System.Collections.Generic;
using System.Collections;
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
    public class TechnicalAnalysisRealtimeStudyExample
    {
        private string d_host;
        private int d_port;

        public static void Main(string[] args)
        {
            TechnicalAnalysisRealtimeStudyExample example = new TechnicalAnalysisRealtimeStudyExample();
            System.Console.WriteLine("Technical Analysis Realtime Study Example");
            example.run(args);
            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        /// <summary>
        /// Establish a Session
        /// Open tasvc Service
        /// Subscribe to securities and fields with overrides specified
        /// Event Loop
        /// </summary>
        /// <param name="args"></param>
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
                System.Console.Error.WriteLine("Failed to start session.");
                return;
            }
            if (!session.OpenService("//blp/tasvc"))
            {
                System.Console.Error.WriteLine("Failed to open //blp/tasvc");
                return;
            }
            sessionOptions.DefaultSubscriptionService = "//blp/tasvc";

            System.Collections.Generic.List<Subscription> subscriptions
                = new System.Collections.Generic.List<Subscription>();

            // Create Technical Analysis WLPR Study Subscription
            Subscription wlprSubscription = createWLPRStudySubscription();
            System.Console.WriteLine("Subscribing to: " + wlprSubscription.SubscriptionString);
            subscriptions.Add(wlprSubscription);

            // Create Technical Analysis MAO Study Subscription
            Subscription maoSubscription = createMAOStudySubscription();
            System.Console.WriteLine("Subscribing to: " + maoSubscription.SubscriptionString);
            subscriptions.Add(maoSubscription);

            // Create Technical Analysis EMAVG Study Subscription
            Subscription emavgSubscription = createEMAVGStudySubscription();
            System.Console.WriteLine("Subscribing to: " + emavgSubscription.SubscriptionString);
            subscriptions.Add(emavgSubscription);

            // NOTE: User must be entitled to receive realtime data for securities subscribed
            session.Subscribe(subscriptions);

            // wait for events from session.
            eventLoop(session);

        }

        // Create Technical Analysis WLPR Study Subscription 
        private Subscription createWLPRStudySubscription()
        {
            Subscription wlprSubscription;
            List<String> fields = new List<String>();
            fields.Add("WLPR");

            List<String> overrides = new List<String>();
            overrides.Add("priceSourceClose=LAST_PRICE");
            overrides.Add("priceSourceHigh=HIGH");
            overrides.Add("priceSourceLow=LOW");
            overrides.Add("periodicitySelection=DAILY");
            overrides.Add("period=14");

            wlprSubscription = new Subscription("IBM US Equity", //security
                                                 fields,        //field                           
                                                 overrides,      //options
                                                 new CorrelationID("IBM US Equity_WLPR"));
            return wlprSubscription;
        }

        // Create Technical Analysis MAO Study Subscription 
        private Subscription createMAOStudySubscription()
        {
            Subscription maoSubscription;
            List<String> fields = new List<String>();
            fields.Add("MAO");

            List<String> overrides = new List<String>();
            overrides.Add("priceSourceClose1=LAST_PRICE");
            overrides.Add("priceSourceClose2=LAST_PRICE");
            overrides.Add("maPeriod1=6");
            overrides.Add("maPeriod2=36");
            overrides.Add("maType1=Simple");
            overrides.Add("maType2=Simple");
            overrides.Add("oscType=Difference");
            overrides.Add("periodicitySelection=DAILY");
            overrides.Add("sigPeriod=9");
            overrides.Add("sigType=Simple");

            maoSubscription = new Subscription("VOD LN Equity",  //security
                                                 fields,         //field                           
                                                 overrides,      //options
                                                 new CorrelationID("VOD LN Equity_MAO"));
            return maoSubscription;
        }

        // Create Technical Analysis EMAVG Study Subscription 
        private Subscription createEMAVGStudySubscription()
        {
            Subscription emavgSubscription;
            List<String> fields = new List<String>();
            fields.Add("EMAVG");

            List<String> overrides = new List<String>();
            overrides.Add("priceSourceClose=LAST_PRICE");
            overrides.Add("periodicitySelection=DAILY");
            overrides.Add("period=14");

            emavgSubscription = new Subscription("6758 JT Equity", //security
                                                 fields,           //field                           
                                                 overrides,        //options
                                                 new CorrelationID("6758 JT Equity_EMAVG"));
            return emavgSubscription;
        }

        /// <summary>
        /// Polls for an event or a message in an event loop
        /// & Processes the event generated
        /// </summary>
        /// <param name="session"></param>
        private void eventLoop(Session session)
        {
            while (true)
            {
                Event eventObj = session.NextEvent();
                foreach (Message msg in eventObj.GetMessages())
                {
                    if (eventObj.Type == Event.EventType.SUBSCRIPTION_STATUS)
                    {
                        System.Console.WriteLine("Processing SUBSCRIPTION_STATUS");
                        string topic = (string)msg.CorrelationID.Object;
                        System.Console.WriteLine(System.DateTime.Now.ToString("s") +
                                                 ": " + topic + ": " + msg.AsElement);
                    }
                    else if (eventObj.Type == Event.EventType.SUBSCRIPTION_DATA)
                    {
                        System.Console.WriteLine("\nProcessing SUBSCRIPTION_DATA");
                        string topic = (string)msg.CorrelationID.Object;
                        System.Console.WriteLine(System.DateTime.Now.ToString("s")
                                                 + ": " + topic + " - " + msg.MessageType);
                        foreach (Element field in msg.Elements)
                        {
                            if (!field.IsNull)
                            {
                                System.Console.WriteLine("\t\t" + field.Name + " = " +
                                    field.GetValueAsString());
                            }
                        }
                    }
                    else
                    {
                        System.Console.WriteLine(msg.AsElement);
                    }
                }
            }
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
            System.Console.WriteLine("  Technical Analysis Realtime Study Example ");
            System.Console.WriteLine("          [-ip            <ipAddress      = localhost>");
            System.Console.WriteLine("          [-p             <tcpPort        = 8194>");
        }
    }
}


