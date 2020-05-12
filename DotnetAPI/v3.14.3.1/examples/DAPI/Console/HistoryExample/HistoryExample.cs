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

using System;
using System.Collections.Generic;
using System.Text;
using ArrayList = System.Collections.ArrayList;

using Datetime = Bloomberglp.Blpapi.Datetime;
using Event = Bloomberglp.Blpapi.Event;
using Element = Bloomberglp.Blpapi.Element;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using DataType = Bloomberglp.Blpapi.Schema.Datatype;

namespace HistoryExample
{
    public class HistoryExample
    {
        private const String REFDATA_SVC = "//blp/refdata";

        private static readonly Name FIELD_ID = Name.GetName("fieldId");
        private static readonly Name SECURITY_DATA = Name.GetName("securityData");
        private static readonly Name SECURITY_NAME = Name.GetName("security");
        private static readonly Name FIELD_DATA = Name.GetName("fieldData");
        private static readonly Name DATE = Name.GetName("date");
        private static readonly Name RESPONSE_ERROR = Name.GetName("responseError");
        private static readonly Name SECURITY_ERROR = Name.GetName("securityError");
        private static readonly Name FIELD_EXCEPTIONS = Name.GetName("fieldExceptions");
        private static readonly Name ERROR_INFO = Name.GetName("errorInfo");
        private static readonly Name CATEGORY = Name.GetName("category");
        private static readonly Name MESSAGE = Name.GetName("message");

        private string d_host;
        private int d_port;
        private ArrayList d_securities;
        private ArrayList d_fields;
        private string d_startDate;
        private string d_endDate;

        public static void Main(string[] args)
        {
            HistoryExample example = new HistoryExample();
            example.run(args);

            System.Console.WriteLine("Press ENTER to quit");
            System.Console.Read();
        }

        public HistoryExample()
        {
            d_host = "localhost";
            d_port = 8194;
            d_securities = new ArrayList();
            d_fields = new ArrayList();
            d_startDate = "null";
            d_endDate = "null";
        }

        private void run(string[] args)
        {
            // if only -h was entered for usage return without starting session
            if (!parseCommandLine(args))
            {
                return;
            }

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

            if (!session.OpenService(REFDATA_SVC))
            {
                System.Console.WriteLine("Failed to open service: " + REFDATA_SVC);
                return;
            }

            Service fieldInfoService = session.GetService(REFDATA_SVC);
            Request request = fieldInfoService.CreateRequest("HistoricalDataRequest");

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

            request.Set("startDate", d_startDate);
            request.Set("endDate", d_endDate);

            System.Console.WriteLine("Sending Request: " + request);
            session.SendRequest(request, null);

            eventLoop(session);

            session.Stop();
        }//end run

        private bool parseCommandLine(string[] args)
        {
            string dateFormat = "yyyyMMdd";

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
                else if (string.Compare(args[i], "-sd", true) == 0
					&& i + 1 < args.Length)
                {
                    d_startDate = args[++i];
                }
                else if (string.Compare(args[i], "-ed", true) == 0
					&& i + 1 < args.Length)
                {
                    d_endDate = args[++i];
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
            }

            if (d_fields.Count == 0)
            {
                d_fields.Add("PX_LAST");
            }

            if (!isDateTimeValid(d_startDate, dateFormat))
            {
                d_startDate = "20090901";
            }

            if (!isDateTimeValid(d_endDate, dateFormat))
            {
                d_endDate = "20090930";
            }

            return true;
        }//end parseCommandLine

        /// <summary>
        /// Validate if date or date time is valid
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private bool isDateTimeValid(string dateTime, string format)
        {
            DateTime outDateTime;
            System.IFormatProvider formatProvider = new System.Globalization.CultureInfo("en-US", true);
            return DateTime.TryParseExact(dateTime, format, formatProvider,
                System.Globalization.DateTimeStyles.AllowWhiteSpaces, out outDateTime);
        }

        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("	Retrieve historical data ");
            System.Console.WriteLine("		[-s		<security	= IBM US Equity>");
            System.Console.WriteLine("		[-f		<field		= PX_LAST>");
            System.Console.WriteLine("    [-sd  <startDateTime  = 20091026");
            System.Console.WriteLine("    [-ed  <endDateTime    = 20091030");
            System.Console.WriteLine("		[-ip 	<ipAddress	= localhost>");
            System.Console.WriteLine("		[-p 	<tcpPort	= 8194>");
        }

        private void eventLoop(Session session)
        {
            //run through all events expected - signified by a "RESPONSE" event
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
        }//end eventLoop

        private void processResponseEvent(Event eventObj)
        {
            foreach (Message msg in eventObj)
            {
                if (msg.HasElement(RESPONSE_ERROR))
                {
                    printErrorInfo("REQUEST FAILED: ", msg.GetElement(RESPONSE_ERROR));
                    continue;
                }
                if ((eventObj.Type != Event.EventType.PARTIAL_RESPONSE) && (eventObj.Type != Event.EventType.RESPONSE))
                {
                    continue;
                }
                if (!ProcessErrors(msg))
                {
                    ProcessExceptions(msg);
                    ProcessFields(msg);
                }
            }
        }//end processResponseEvent

        private void printErrorInfo(string leadingStr, Element errorInfo)
        {
            System.Console.WriteLine(leadingStr +
                                     errorInfo.GetElementAsString(CATEGORY) + " (" +
                                     errorInfo.GetElementAsString(MESSAGE) + ")");
        }//end printErrorInfo

        bool ProcessExceptions(Message msg)
        {
            Element securityData = msg.GetElement(SECURITY_DATA);
            Element field_exceptions = securityData.GetElement(FIELD_EXCEPTIONS);

            if (field_exceptions.NumValues > 0)
            {
                Element element = field_exceptions.GetValueAsElement(0);
                Element field_id = element.GetElement(FIELD_ID);
                Element error_info = element.GetElement(ERROR_INFO);
                Element error_message = error_info.GetElement(MESSAGE);
                System.Console.WriteLine(field_id);
                System.Console.WriteLine(error_message);
                return true;
            }
            return false;
        }

        bool ProcessErrors(Message msg)
        {
            Element securityData = msg.GetElement(SECURITY_DATA);

            if (securityData.HasElement(SECURITY_ERROR))
            {
                Element security_error = securityData.GetElement(SECURITY_ERROR);
                Element error_message = security_error.GetElement(MESSAGE);
                System.Console.WriteLine(error_message);
                return true;
            }
            return false;
        }

        void ProcessFields(Message msg)
        {
            String delimiter = "\t";

            //Print out the date column header
            System.Console.Write("DATE" + delimiter + delimiter);

            // Print out the field column headers
            for (int k = 0; k < d_fields.Count; k++)
            {
                System.Console.Write(d_fields[k].ToString() + delimiter);
            }
            System.Console.Write("\n\n");

            Element securityData = msg.GetElement(SECURITY_DATA);
            Element fieldData = securityData.GetElement(FIELD_DATA);

            //Iterate through all field values returned in the message
            if (fieldData.NumValues > 0)
            {
                for (int j = 0; j < fieldData.NumValues; j++)
                {
                    Element element = fieldData.GetValueAsElement(j);

                    //Print out the date
                    Datetime date = element.GetElementAsDatetime(DATE);
                    System.Console.Write(date.DayOfMonth + "/" + date.Month + "/" + date.Year + delimiter);

                    //Check for the presence of all the fields requested
                    for (int k = 0; k < d_fields.Count; k++)
                    {
                        String temp_field_str = d_fields[k].ToString();
                        if (element.HasElement(temp_field_str))
                        {
                            Element temp_field = element.GetElement(temp_field_str);
                            Name TEMP_FIELD_STR = Name.GetName(temp_field_str);

                            int datatype = temp_field.Datatype.GetHashCode();

                            //Extract the value dependent on the dataype and print to the console
                            switch (datatype)
                            {
                                case (int)DataType.BOOL://Bool
                                    {
                                        bool field1;
                                        field1 = element.GetElementAsBool(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.CHAR://Char
                                    {
                                        char field1;
                                        field1 = element.GetElementAsChar(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.INT32://Int32
                                    {
                                        Int32 field1;
                                        field1 = element.GetElementAsInt32(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.INT64://Int64
                                    {
                                        Int64 field1;
                                        field1 = element.GetElementAsInt64(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.FLOAT32://Float32
                                    {
                                        float field1;
                                        field1 = element.GetElementAsFloat32(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.FLOAT64://Float64
                                    {
                                        double field1;
                                        field1 = element.GetElementAsFloat64(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.STRING://String
                                    {
                                        String field1;
                                        field1 = element.GetElementAsString(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.DATE://Date
                                    {
                                        Datetime field1;
                                        field1 = element.GetElementAsDatetime(TEMP_FIELD_STR);
                                        System.Console.Write(field1.Year + '/' + field1.Month + '/' + field1.DayOfMonth + delimiter);
                                        break;
                                    }
                                case (int)DataType.TIME://Time
                                    {
                                        Datetime field1;
                                        field1 = element.GetElementAsDatetime(TEMP_FIELD_STR);
                                        System.Console.Write(field1.Hour + '/' + field1.Minute + '/' + field1.Second + delimiter);
                                        break;
                                    }
                                case (int)DataType.DATETIME://Datetime
                                    {
                                        Datetime field1;
                                        field1 = element.GetElementAsDatetime(TEMP_FIELD_STR);
                                        System.Console.Write(field1.Year + '/' + field1.Month + '/' + field1.DayOfMonth + '/');
                                        System.Console.Write(field1.Hour + '/' + field1.Minute + '/' + field1.Second + delimiter);
                                        break;
                                    }
                                default:
                                    {
                                        String field1;
                                        field1 = element.GetElementAsString(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                            }//end of switch
                        }//end of if
                        System.Console.WriteLine("");
                    }//end of for
                }//end of for
            }//end of if
        }//end of method

    }//end of class
}//end of namespace
