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
/// ==========================================================
/// Purpose of this example:
/// - Make asynchronous and synchronous Intraday Tick
///   request using //blp/refdata service.
/// ==========================================================
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Element = Bloomberglp.Blpapi.Element;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using EventHandler = Bloomberglp.Blpapi.EventHandler;
using CorrelationID = Bloomberglp.Blpapi.CorrelationID;
using BDateTime = Bloomberglp.Blpapi.Datetime;

namespace Bloomberglp.Blpapi.Examples
{
    public partial class Form1 : Form
    {
        private static readonly Name EXCEPTIONS = new Name("exceptions");
        private static readonly Name FIELD_ID = new Name("fieldId");
        private static readonly Name REASON = new Name("reason");
        private static readonly Name CATEGORY = new Name("category");
        private static readonly Name DESCRIPTION = new Name("description");
        private static readonly Name ERROR_CODE = new Name("errorCode");
        private static readonly Name SOURCE = new Name("source");
        private static readonly Name SECURITY_ERROR = new Name("securityError");
        private static readonly Name MESSAGE = new Name("message");
        private static readonly Name RESPONSE_ERROR = new Name("responseError");
        private static readonly Name SECURITY_DATA = new Name("securityData");
        private static readonly Name FIELD_EXCEPTIONS = new Name("fieldExceptions");
        private static readonly Name ERROR_INFO = new Name("errorInfo");

        private SessionOptions d_sessionOptions;
        private Session d_session;
        private DataTable d_data;
        private String d_requestSecurity = String.Empty;
        
        public Form1()
        {
            InitializeComponent();
            string serverHost = "localhost";
            int serverPort = 8194;

            // set sesson options
            d_sessionOptions = new SessionOptions();
            d_sessionOptions.ServerHost = serverHost;
            d_sessionOptions.ServerPort = serverPort;
            // initialize UI controls
            initUI();
        }

        #region methods
        /// <summary>
        /// Initialize form controls
        /// </summary>
        private void initUI()
        {
            // add columns to table
            if (d_data == null)
                d_data = new DataTable();
            d_data.Columns.Add("security");
            d_data.Columns.Add("time");
            d_data.Columns.Add("type");
            d_data.Columns.Add("value");
            d_data.Columns.Add("size");
            d_data.Columns.Add("conditionCodes");
            d_data.Columns.Add("exchangeCode");
            d_data.Columns.Add("brokerBuyCode");
            d_data.Columns.Add("brokerSellCode");
            d_data.AcceptChanges();
            // set grid data source
            dataGridViewData.DataSource = d_data;
            // default to TRADE
            checkedListBoxEventTypes.SetItemChecked(0, true);
            // Intraday request need the time to be in GMT.
            dateTimePickerStartDate.Value = DateTime.Now.ToUniversalTime().AddMinutes(-5);
            dateTimePickerEndDate.Value = DateTime.Now.ToUniversalTime();
        }

        /// <summary>
        /// Create session
        /// </summary>
        /// <returns></returns>
        private bool createSession()
        {
            if (d_session != null)
            {
                // Session.Stop needs to be called asynchronously to 
                // prevent blocking, while waiting for GUI event processing 
                // to return.
                d_session.Stop(AbstractSession.StopOption.ASYNC);
            }
            
            toolStripStatusLabel1.Text = "Connecting...";
            if (radioButtonAsynch.Checked)
            {
                // create asynchronous session
                d_session = new Session(d_sessionOptions, new EventHandler(processEvent));
            }
            else
            {
                // create synchronous session
                d_session = new Session(d_sessionOptions);
            }
            return d_session.Start();
        }

        /// <summary>
        /// Manage control states
        /// </summary>
        private void setControlStates()
        {
            // require at lease 4 characters, event type checked and valid time period to enable button
            buttonSendRequest.Enabled = textBoxSecurity.Text.Length > 4 && 
                                        checkedListBoxEventTypes.CheckedItems.Count > 0 &&
                                        dateTimePickerEndDate.Value > dateTimePickerStartDate.Value;
            buttonClearAll.Enabled = listBoxSecurities.Items.Count > 0 || d_data.Rows.Count > 0;
        }

        /// <summary>
        /// Clear security data
        /// </summary>
        private void clearSecurityData(string sec)
        {
            DataRow[] rows = d_data.Select("security = '" + sec + "'");
            foreach (DataRow row in rows)
                row.Delete();
            toolStripStatusLabel1.Text = string.Empty;
        }

        /// <summary>
        /// Remove all securities and fields from grid
        /// </summary>
        private void clearAll()
        {
            d_data.Rows.Clear();
            d_data.AcceptChanges();
            listBoxSecurities.Items.Clear();
            dataGridViewData.DataSource = null;
            setControlStates();
            toolStripStatusLabel1.Text = string.Empty;
        }
        #endregion end methods

        #region Control Events
        /// <summary>
        /// Selected security to display data 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxSecurities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxSecurities.SelectedIndex != -1)
            {
                // create table for security's intraday data
                DataTable table = d_data.Clone();
                // get data for security
                DataRow[] rows = d_data.Select("security = '" + listBoxSecurities.SelectedItem.ToString() + "'");
                foreach (DataRow row in rows)
                {
                    table.Rows.Add(row.ItemArray);
                }
                // display data
                dataGridViewData.DataSource = table;
                foreach (DataGridViewColumn col in dataGridViewData.Columns)
                {
                    if (col.Name == "security")
                        col.Visible = false;
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                dataGridViewData.Refresh();
            }
        }

        /// <summary>
        /// Validate security text length and send request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxSecurity_KeyDown(object sender, KeyEventArgs e)
        {
            setControlStates();
            if (e.KeyCode == Keys.Return && buttonSendRequest.Enabled)
            {
                buttonSendRequest_Click(this, new EventArgs());
            }
        }

        /// <summary>
        /// Select event type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkedListBoxEventTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            setControlStates();
        }

        /// <summary>
        /// date/time changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateTimePickerEndDate_ValueChanged(object sender, EventArgs e)
        {
            setControlStates();
        }

        /// <summary>
        /// Remove securities and fields button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            clearAll();
        }

        /// <summary>
        /// Submit Intraday Tick Request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            DateTime startDate = dateTimePickerStartDate.Value;
            DateTime endDate = dateTimePickerEndDate.Value;
            // check time period size
            if (endDate.Subtract(startDate).Minutes > 30)
            {
                if (MessageBox.Show("You have selected a large time period for the intraday tick request. Request may\n" +
                    "take a long period of time to process depending on the amount data.\n\nDo you wish to continue?", 
                    "Intraday Tick Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                    MessageBoxDefaultButton.Button2) ==  DialogResult.No)
                {
                    return;
                }
            }
            // clear security data if requested before
            d_requestSecurity = textBoxSecurity.Text.Trim();
            clearSecurityData(d_requestSecurity);
            // create session
            if (!createSession())
            {
                toolStripStatusLabel1.Text = "Failed to start session.";
                return;
            }
            // open reference data service
            if (!d_session.OpenService("//blp/refdata"))
            {
                toolStripStatusLabel1.Text = "Failed to open //blp/refdata";
                return;
            }
            toolStripStatusLabel1.Text = "Connected sucessfully";
            Service refDataService = d_session.GetService("//blp/refdata");
            // create intraday tick request
            Request request = refDataService.CreateRequest("IntradayTickRequest");
            // set request parameters
            request.Set("includeConditionCodes", checkBoxIncludeConditionCode.Checked);
            request.Set("includeExchangeCodes", checkBoxIncludeExchangeCode.Checked);
            request.Set("includeBrokerCodes", checkBoxIncludeBrokerCode.Checked);
            Element eventTypes = request.GetElement("eventTypes");
            foreach (object item in checkedListBoxEventTypes.CheckedItems)
            {
                eventTypes.AppendValue(item.ToString());
            }
            request.Set("security", d_requestSecurity);
            request.Set("startDateTime", new BDateTime(startDate.Year, startDate.Month, startDate.Day,
                    startDate.Hour, startDate.Minute, startDate.Second, 0));
            request.Set("endDateTime", new BDateTime(endDate.Year, endDate.Month, endDate.Day,
                endDate.Hour, endDate.Minute, endDate.Second, 0));
            // create correlation id
            CorrelationID cID = new CorrelationID(1);
            d_session.Cancel(cID);
            // send request
            d_session.SendRequest(request, cID);
            toolStripStatusLabel1.Text = "Submitted request. Waiting for response...";
            if (radioButtonSynch.Checked)
            {
                // allow UI to update
                Application.DoEvents();
                // synchronous mode. Wait for reply before proceeding.
                while (true)
                {
                    Event eventObj = d_session.NextEvent();
                    toolStripStatusLabel1.Text = "Processing data...";
                    Application.DoEvents();
                    processEvent(eventObj, d_session);
                    if (eventObj.Type == Event.EventType.RESPONSE)
                    {
                        break;
                    }
                }
                // select requested security in listbox to disply data
                setControlStates();
                int itemIndex = -1;
                if (!listBoxSecurities.Items.Contains(d_requestSecurity))
                    itemIndex = listBoxSecurities.Items.Add(d_requestSecurity);
                else
                {
                    listBoxSecurities.SelectedIndex = -1;
                    itemIndex = listBoxSecurities.Items.IndexOf(d_requestSecurity);
                }
                listBoxSecurities.SelectedIndex = itemIndex;
                toolStripStatusLabel1.Text = "Completed";
            }
        }
        #endregion

        #region Bloomberg API Events
        /// <summary>
        /// Data event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processEvent(Event eventObj, Session session)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(processEvent), new object[] { eventObj, session });
            }
            else
            {
                try
                {
                    switch (eventObj.Type)
                    {
                        case Event.EventType.RESPONSE:
                            // process final respose for request
                            processRequestDataEvent(eventObj, session);
                            // display data
                            setControlStates();
                            int itemIndex = -1;
                            if (!listBoxSecurities.Items.Contains(d_requestSecurity))
                                itemIndex = listBoxSecurities.Items.Add(d_requestSecurity);
                            else
                            {
                                listBoxSecurities.SelectedIndex = -1;
                                itemIndex = listBoxSecurities.Items.IndexOf(d_requestSecurity);
                            }
                            listBoxSecurities.SelectedIndex = itemIndex;
                            toolStripStatusLabel1.Text = "Completed";
                            break;
                        case Event.EventType.PARTIAL_RESPONSE:
                            // process partial response
                            processRequestDataEvent(eventObj, session);
                            break;
                        default:
                            processMiscEvents(eventObj, session);
                            break;
                    }
                }
                catch (System.Exception e)
                {
                    toolStripStatusLabel1.Text = e.Message.ToString();
                }
            }
        }

        /// <summary>
        /// Process subscription data
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processRequestDataEvent(Event eventObj, Session session)
        {
            d_data.BeginLoadData();
            // process message
            foreach (Message msg in eventObj)
            {
                if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("IntradayTickResponse")))
                {
                    if (msg.HasElement(RESPONSE_ERROR))
                    {
                        // process error
                        Element error = msg.GetElement(RESPONSE_ERROR);
                        if (msg.NumElements == 1)
                        {
                            d_data.Rows.Add(new object[] { d_requestSecurity, error.GetElementAsString(MESSAGE) });
                            return;
                        }
                    }
                    // process tick data
                    Element tickDataArray = msg.GetElement("tickData");
                    int numberOfTicks = tickDataArray.NumValues;
                    foreach (Element tickData in tickDataArray.Elements)
                    {
                        if (tickData.Name.ToString() == "tickData")
                        {
                            for (int pointIndex = 0; pointIndex < tickData.NumValues; pointIndex++)
                            {
                                int fieldIndex = 0;
                                object[] dataValues = new object[d_data.Columns.Count];
                                Element fields = tickData.GetValueAsElement(pointIndex);
                                foreach (DataColumn col in d_data.Columns)
                                {
                                    if (fields.HasElement(col.ColumnName))
                                    {
                                        // tick field data
                                        Element item = fields.GetElement(col.ColumnName);
                                        dataValues[fieldIndex] = item.GetValueAsString();
                                    }
                                    else
                                    {
                                        if (col.ColumnName == "security")
                                            dataValues[fieldIndex] = d_requestSecurity;
                                        else
                                            dataValues[fieldIndex] = DBNull.Value;
                                    }
                                    fieldIndex++;
                                } // end foreach 
                                // save tick data
                                d_data.Rows.Add(dataValues);
                            }
                        } // end if
                    } // end foreach
                }
            }
            d_data.EndLoadData();
        }

        /// <summary>
        /// Process miscellaneous events
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processMiscEvents(Event eventObj, Session session)
        {
            foreach (Message msg in eventObj)
            {
                switch (msg.MessageType.ToString())
                {
                    case "SessionStarted":
                        // "Session Started"
                        break;
                    case "SessionTerminated":
                    case "SessionStopped":
                        // "Session Terminated"
                        break;
                    case "ServiceOpened":
                        // "Reference Service Opened"
                        break;
                    case "RequestFailure":
                        Element reason = msg.GetElement(REASON);
                        string message = string.Concat("Error: Source-", reason.GetElementAsString(SOURCE),
                            ", Code-", reason.GetElementAsString(ERROR_CODE), ", category-", reason.GetElementAsString(CATEGORY),
                            ", desc-", reason.GetElementAsString(DESCRIPTION));
                        toolStripStatusLabel1.Text = message;
                        break;
                    default:
                        toolStripStatusLabel1.Text = msg.MessageType.ToString();
                        break;
                }
            }
        }
        #endregion
    }
}