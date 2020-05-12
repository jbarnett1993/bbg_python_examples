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
** SimpleCustomVWAPExample.cs
 
**  This program demonstrate how to make subscription to particular 
**  security/ticker to get realtime VWAP updates using V3 API. 
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

using System;
using System.Collections.Generic;
using Bloomberglp.Blpapi;

namespace Bloomberglp.Blpapi.Examples
{
    class SimpleCustomVWAPExample
    {
        private string d_host;
        private int d_port;
        private List<String> d_securities;
        private List<String> d_fields;
        private List<String> d_overrides;

        public static void Main(string[] args)
        {
            System.Console.WriteLine("Custom VWAP Example");
            SimpleCustomVWAPExample example = new SimpleCustomVWAPExample();
            example.run(args);
            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SimpleCustomVWAPExample()
        {
            d_host = "localhost";
            d_port = 8194;
            d_securities = new List<String>();
            d_fields = new List<String>();
            d_overrides = new List<String>();
        }

        /// <summary>
        /// Read command line arguments, 
        /// Establish a Session
        /// Open mktvwap Service
        /// Subscribe to securities and fields with VWAP overrides
        /// Event Loop
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
            if (!session.OpenService("//blp/mktvwap"))
            {
                System.Console.Error.WriteLine("Failed to open //blp/mktvwap");
                return;
            }
            sessionOptions.DefaultSubscriptionService = "//blp/mktvwap";

            System.Collections.Generic.List<Subscription> subscriptions
                = new System.Collections.Generic.List<Subscription>();

            // User must be enabled for real-time data for the exchanges the securities
            // they are monitoring for custom VWAP trade. 
            // Otherwise, Subscription will fail for those securities and/or you will 
            // receive #N/A N/A instead of valid tick data. 
            foreach (string security in d_securities)
            {
                subscriptions.Add(new Subscription(security,
                                                    d_fields,
                                                    d_overrides,
                                                    new CorrelationID(security)));
            }

            session.Subscribe(subscriptions);

            // wait for events from session.
            eventLoop(session);

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
                foreach (Message msg in eventObj)
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
                else if (string.Compare(args[i], "-o", true) == 0
					&& i + 1 < args.Length)
                {
                    d_overrides.Add(args[++i]);
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
                d_securities.Add("IBM US Equity");
                d_securities.Add("VOD LN Equity");
                d_securities.Add("6758 JT Equity");
            }

            if (d_fields.Count == 0)
            {
                // Subscribing to Bloomberg defined VWAP and VWAP Volume
                d_fields.Add("VWAP");
                d_fields.Add("RT_VWAP_VOLUME");

                // Subscribing to Market defined VWAP and VWAP Volume
                d_fields.Add("MARKET_DEFINED_VWAP_REALTIME");
                d_fields.Add("RT_MKT_VWAP_VOLUME");
            }

            if (d_overrides.Count == 0)
            {
                d_overrides.Add("VWAP_START_TIME=09:00");
            }

            return true;
        }

        /// <summary>
        /// Print usage of the Program
        /// </summary>
        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("    Retrieve customized realtime vwap data using Bloomberg V3 API");
            System.Console.WriteLine("      [-s         <security   = \"IBM US Equity\">]");
            System.Console.WriteLine("      [-f         <field      = VWAP>]");
            System.Console.WriteLine("      [-o         <overrides  = VWAP_START_TIME=09:00>]");
            System.Console.WriteLine("      [-ip        <ipAddress  = localhost>]");
            System.Console.WriteLine("      [-p         <tcpPort    = 8194>]");
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine("Multiple securities, vwap fields & overrides can be specified.");
        }
    }
}
