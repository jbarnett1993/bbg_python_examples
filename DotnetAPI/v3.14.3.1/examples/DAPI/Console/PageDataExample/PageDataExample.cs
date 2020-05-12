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
** PageDataExample.cs
**
** This Example shows how to retrieve page based data using V3 API
** Usage: 
**      		-t			<Topic  	= "0708/012/0001">
**                                    i.e."Broker ID/Category/Page Number"
**      		-ip 		<ipAddress	= localhost>
**      		-p 			<tcpPort	= 8194>
** e.g. PageDataExample -t "0708/012/0001" -ip localhost -p 8194
*/

using System.Collections.Generic;
using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using Datatype = Bloomberglp.Blpapi.Schema.Datatype;
using ArrayList = System.Collections.ArrayList;
using Hashtable = System.Collections.Hashtable;
using System.Text;

namespace Bloomberglp.Blpapi.Examples
{
    public class PageDataExample
    {
        private static readonly Name EXCEPTIONS = Name.GetName("exceptions");
        private static readonly Name FIELD_ID = Name.GetName("fieldId");
        private static readonly Name REASON = Name.GetName("reason");
        private static readonly Name CATEGORY = Name.GetName("category");
        private static readonly Name DESCRIPTION = Name.GetName("description");
        private static readonly Name PAGEUPDATE = Name.GetName("PageUpdate");
        private static readonly Name ROWUPDATE = Name.GetName("rowUpdate");
        private static readonly Name NUMROWS = Name.GetName("numRows");
        private static readonly Name NUMCOLS = Name.GetName("numCols");
        private static readonly Name ROWNUM = Name.GetName("rowNum");
        private static readonly Name SPANUPDATE = Name.GetName("spanUpdate");
        private static readonly Name STARTCOL = Name.GetName("startCol");
        private static readonly Name LENGTH = Name.GetName("length");
        private static readonly Name TEXT = Name.GetName("text");

        private string      d_host;
        private int         d_port;
        private ArrayList   d_topics;
        private Hashtable   d_topicTable; 

        
        public static void Main(string[] args)
        {
            PageDataExample example = new PageDataExample();
            example.run(args);
            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PageDataExample()
        {
            d_host      = "localhost";
            d_port      = 8194;
            d_topics    = new ArrayList();
            d_topicTable = new Hashtable();
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
            if (!session.OpenService("//blp/pagedata"))
            {
                System.Console.Error.WriteLine("Failed to open //blp/pagedata");
                return;
            }

            subscribe(session);

            // wait for events from session.
            eventLoop(session);

        }

        /// <summary>
        /// Function to create subscription list and subscribe to pagedata
        /// </summary>
        /// <param name="session"></param>
        private void subscribe(Session session)
        {
            System.Collections.Generic.List<Subscription> subscriptions
                = new System.Collections.Generic.List<Subscription>();
            d_topicTable.Clear();

            List<string> fields = new List<string>();
            fields.Add("6-23");
            // Following commented code shows some of the sample values 
            // that can be used for field other than above
            // e.g. fields.Add("1");
            //      fields.Add("1,2,3");
            //      fields.Add("1,6-10,15,16");

            foreach (string topic in d_topics)
            {
                subscriptions.Add(new Subscription("//blp/pagedata/" + topic,
                                                    fields,
                                                    null,
                                                    new CorrelationID(topic)));
                d_topicTable.Add(topic, new ArrayList());
            }
            session.Subscribe(subscriptions);

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
                if (string.Compare(args[i], "-t", true) == 0
					&& i + 1 < args.Length)
                {
                    d_topics.Add(args[++i]);
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

            // set default topics if nothing is specified
            if (d_topics.Count == 0)
            {
                d_topics.Add("0708/012/0001");
                d_topics.Add("1102/1/274");
            }

            return true;
        }

        /// <summary>
        /// Print usage of the Program
        /// </summary>
        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("	Retrieve page based data using V3 API");
            System.Console.WriteLine("      [-t			<Topic  	= \"0708/012/0001\">]");
            System.Console.WriteLine("             i.e.\"Broker ID/Category/Page Number\"");
            System.Console.WriteLine("      [-ip        <ipAddress	= localhost>");
            System.Console.WriteLine("      [-p         <tcpPort	= 8194>");
            System.Console.WriteLine("e.g. PageDataExample -t \"0708/012/0001\" -ip localhost -p 8194");
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
                try
                {
                    switch (eventObj.Type)
                    {
                        case Event.EventType.SUBSCRIPTION_DATA:
                            processSubscriptionDataEvent(eventObj, session);
                            break;
                        case Event.EventType.SUBSCRIPTION_STATUS:
                            processSubscriptionStatus(eventObj, session);
                            break;
                        default:
                            processMiscEvents(eventObj, session);
                            break;
                    }
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine(e.ToString());
                }
            }
        }

        /// <summary>
        /// Process SubscriptionStatus event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processSubscriptionStatus(Event eventObj, Session session)
        {
            System.Console.WriteLine("Processing SUBSCRIPTION_STATUS");
            foreach (Message msg in eventObj.GetMessages())
            {
                string topic = (string)msg.CorrelationID.Object;
                System.Console.WriteLine(System.DateTime.Now.ToString("s") +
                    ": " + topic + " - " + msg.MessageType);

                if (msg.HasElement(REASON))
                {
                    // This can occur on SubscriptionFailure.
                    Element reason = msg.GetElement(REASON);
                    System.Console.WriteLine("\t" +
                            reason.GetElement(CATEGORY).GetValueAsString() +
                            ": " + reason.GetElement(DESCRIPTION).GetValueAsString());
                }

                if (msg.HasElement(EXCEPTIONS))
                {
                    // This can occur on SubscriptionStarted if at least
                    // one field is good while the rest are bad.
                    Element exceptions = msg.GetElement(EXCEPTIONS);
                    for (int i = 0; i < exceptions.NumValues; ++i)
                    {
                        Element exInfo = exceptions.GetValueAsElement(i);
                        Element fieldId = exInfo.GetElement(FIELD_ID);
                        Element reason = exInfo.GetElement(REASON);
                        System.Console.WriteLine("\t" + fieldId.GetValueAsString() +
                                ": " + reason.GetElement(CATEGORY).GetValueAsString());
                    }
                }
                System.Console.WriteLine("");
            }
        }

        /// <summary>
        /// Process SubscriptionData event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processSubscriptionDataEvent(Event eventObj, Session session)
        {
            System.Console.WriteLine("Processing SUBSCRIPTION_DATA");
            foreach (Message msg in eventObj.GetMessages())
            {
                string topic = (string)msg.CorrelationID.Object;
                System.Console.WriteLine(System.DateTime.Now.ToString("s") +
                    ": " + topic + " - " + msg.MessageType);
                //System.Console.WriteLine(msg.AsElement);
                if(msg.MessageType.Equals("PageUpdate")){
                    processPageElement(msg.AsElement, topic);
                }else if(msg.MessageType.Equals("RowUpdate")){
                    processRowElement(msg.AsElement, topic);
                }

                //showUpdatedPage(topic);
            }
        }

        /// <summary>
        /// Show whole page content for specific topic
        /// </summary>
        /// <param name="topic"></param>
        private void showUpdatedPage(string topic)
        {
            ArrayList rowList = (ArrayList)d_topicTable[topic];
            foreach (StringBuilder str in rowList)
            {
                System.Console.WriteLine(str.ToString());
            }          
        }

        /// <summary>
        /// Process PageUpdate Event/PageUpdate Element
        /// </summary>
        /// <param name="pageElement"></param>
        /// <param name="topic"></param>
        private void processPageElement(Element pageElement, string topic)
        {
            Element eleNumRows = pageElement.GetElement(NUMROWS);
            int numRows = eleNumRows.GetValueAsInt32();
            Element eleNumCols = pageElement.GetElement(NUMCOLS);
            int numCols = eleNumCols.GetValueAsInt32();
            System.Console.WriteLine("Page Contains " + numRows + " Rows & " + numCols + " Columns");
            Element eleRowUpdates = pageElement.GetElement(ROWUPDATE);
            int numRowUpdates = eleRowUpdates.NumValues;
            System.Console.WriteLine("Processing " + numRowUpdates + " RowUpdates");
            for (int i = 0; i < numRowUpdates; ++i)
            {
                Element rowUpdate = eleRowUpdates.GetValueAsElement(i);
                processRowElement(rowUpdate, topic);
            }
        }

        /// <summary>
        /// Process RowUpdate Event/ rowUpdate Element
        /// </summary>
        /// <param name="rowElement"></param>
        /// <param name="topic"></param>
        private void processRowElement(Element rowElement, string topic)
        {
            Element eleRowNum = rowElement.GetElement(ROWNUM);
            int rowNum = eleRowNum.GetValueAsInt32();
            Element eleSpanUpdates = rowElement.GetElement(SPANUPDATE);
            int numSpanUpdates = eleSpanUpdates.NumValues;
            //System.Console.WriteLine("Processing " + numSpanUpdates + " spanUpdate");
            for (int i = 0; i < numSpanUpdates; ++i)
            {
                Element spanUpdate = eleSpanUpdates.GetValueAsElement(i);
                processSpanElement(spanUpdate, rowNum, topic);
            } 
        }

        /// <summary>
        /// <summary>
        /// Process spanUpdate Element
        /// </summary>
        /// <param name="spanElement"></param>
        /// <param name="rowNum"></param>
        /// <param name="topic"></param>
        private void processSpanElement(Element spanElement, int rowNum, string topic)
        {
            Element eleStartCol = spanElement.GetElement(STARTCOL);
            int startCol = eleStartCol.GetValueAsInt32();
            Element eleLength = spanElement.GetElement(LENGTH);
            int len = eleLength.GetValueAsInt32();
            Element eleText = spanElement.GetElement(TEXT);
            string text = eleText.GetValueAsString();
            System.Console.WriteLine("Row : " + rowNum + 
                                     ",Col: " + startCol + 
                                     "(Len: " + len + ")" + 
                                     "\tNew Text: " + text);
            ArrayList rowList = (ArrayList) d_topicTable[topic];
            while (rowList.Count < rowNum)
            {
                rowList.Add(new StringBuilder());
            }
            StringBuilder rowText = (StringBuilder)rowList[rowNum - 1];
            if (rowText.Length == 0)
            {
                rowText.Append(text.PadRight(80));
            }
            else
            {
                string strToReplace = rowText.ToString().Substring(startCol - 1, len);
                rowText.Replace(strToReplace, text, startCol - 1, len);
                System.Console.WriteLine(rowText.ToString());
            }
        }

        /// <summary>
        /// Process events other than subscription data/status event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processMiscEvents(Event eventObj, Session session)
        {
            System.Console.WriteLine("Processing " + eventObj.Type);
            foreach (Message msg in eventObj.GetMessages())
            {
                System.Console.WriteLine(System.DateTime.Now.ToString("s") +
                    ": " + msg.MessageType + "\n");
            }
        }
    }
}
