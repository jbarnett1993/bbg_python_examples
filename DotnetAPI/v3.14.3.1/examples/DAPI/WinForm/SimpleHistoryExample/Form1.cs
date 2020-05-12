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
/// - Make asynchronous and synchronous historical request
///   using //blp/refdata service.
/// ==========================================================
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
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
            const string security = "security";
            const string date = "date";
            textBoxCurrencyCode.Text = "USD";
            textBoxMaxPoints.Text = "1000";
            comboBoxPricing.SelectedIndex = 0;
            comboBoxNonTradingDayMethod.SelectedIndex = 1;
            comboBoxNonTradingDayValue.SelectedIndex = 2;
            comboBoxPeriodicityAdjustment.SelectedIndex = 0;
            comboBoxOverrideOption.SelectedIndex = 0;
            dateTimePickerStart.Value = DateTime.Now.AddDays(-1);

            // add columns to grid
            if (d_data == null)
                d_data = new DataTable();
            d_data.Columns.Add(security);
            d_data.Columns.Add(date);
            dataGridViewData.DataSource = d_data;
            dataGridViewData.Columns[security].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        /// <summary>
        /// Add fields
        /// </summary>
        /// <param name="fields"></param>
        private void addFields(string fields)
        {
            // Tokenize the string into what (we hope) are Security strings
            char[] sep = { '\r', '\n', '\t', ',' };
            string[] words = fields.Split(sep);
            foreach (string field in words)
            {
                if (field.Trim().Length > 0)
                {
                    // add fields
                    if (!d_data.Columns.Contains(field.Trim()))
                    {
                        d_data.Columns.Add(field.Trim());
                        dataGridViewData.Columns[field.Trim()].SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                }
            }
            setControlStates();
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
            // require at lease 4 characters to enable buttons
            if (textBoxSecurity.Text.Length > 4)
            {
                labelField.Enabled = true;
                buttonSendRequest.Enabled = (dataGridViewData.ColumnCount > 2);
                buttonClearFields.Enabled = buttonSendRequest.Enabled;
            }
            else
            {
                labelField.Enabled = false;
                buttonSendRequest.Enabled = labelField.Enabled;
                buttonClearFields.Enabled = buttonSendRequest.Enabled;
            }
            buttonClearData.Enabled = dataGridViewData.RowCount > 0;
            textBoxField.Enabled = labelField.Enabled;
            buttonAddFields.Enabled = labelField.Enabled;
        }

        /// <summary>
        /// Clear security data
        /// </summary>
        private void clearData()
        {
            d_data.Rows.Clear();
            d_data.AcceptChanges();
            toolStripStatusLabel1.Text = string.Empty;
        }
        #endregion end methods

        #region Control Events
        /// <summary>
        /// Update control states
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxSecurity_KeyDown(object sender, KeyEventArgs e)
        {
            setControlStates();
        }

        /// <summary>
        /// Add field button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAddFields_Click(object sender, EventArgs e)
        {
            if (textBoxField.Text.Trim().Length > 0)
            {
                addFields(textBoxField.Text.ToUpper().Trim());
                textBoxField.Text = string.Empty;
                setControlStates();
            }
        }

        /// <summary>
        /// Enter key pressed to add field to grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                buttonAddFields_Click(this, new EventArgs());
            }
        }

        /// <summary>
        /// Only allow numeric keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxMaxPoints_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.KeyValue == 190 || (e.KeyValue >= 48 && e.KeyValue <= 57) ||
                e.KeyData == Keys.Back || e.KeyData == Keys.Left || e.KeyData == Keys.Right))
            {
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Only allow upper and lower character keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCurrencyCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (!((e.KeyValue >= 65 && e.KeyValue <= 90) ||
                e.KeyData == Keys.Back || e.KeyData == Keys.Left || e.KeyData == Keys.Right))
            {
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Allow drag and drop of fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewView_DragDrop(object sender, DragEventArgs e)
        {
            // Get the entire text object that has been dropped on us.
            string tmp = e.Data.GetData(DataFormats.Text).ToString();
            addFields(tmp);
        }

        /// <summary>
        /// Mouse drag over grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewView_DragEnter(object sender, DragEventArgs e)
        {
            if (buttonAddFields.Enabled)
            {
                if (e.Data.GetDataPresent(DataFormats.Text))
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Allow user to delete single field or security from grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewData_KeyDown(object sender, KeyEventArgs e)
        {
            DataGridView dataGrid = (DataGridView)sender;
            if (e.KeyData == Keys.Delete && dataGrid.SelectedCells.Count > 0)
            {
                int rowIndex = dataGrid.SelectedCells[0].RowIndex;
                int columnIndex = dataGrid.SelectedCells[0].ColumnIndex;
                if (columnIndex > 1)
                {
                    // remove field
                    d_data.Columns.RemoveAt(columnIndex);
                    d_data.AcceptChanges();

                    if (dataGrid.Columns.Count > columnIndex && columnIndex > 0)
                        dataGrid.Rows[rowIndex].Cells[columnIndex].Selected = true;
                    else
                        if (dataGrid.Columns.Count > 1 && dataGrid.Columns.Count == columnIndex)
                            dataGrid.Rows[rowIndex].Cells[columnIndex - 1].Selected = true;
                }
            }
        }

        /// <summary>
        /// Remove all fields button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearFields_Click(object sender, EventArgs e)
        {
            for (int index = d_data.Columns.Count - 1; index > 1; index--)
            {
                d_data.Columns.RemoveAt(index);
            }
            clearData();
            d_data.AcceptChanges();
            setControlStates();
        }

        /// <summary>
        /// Clear data button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearData_Click(object sender, EventArgs e)
        {
            clearData();
            setControlStates();
        }

        /// <summary>
        /// Select periodicity adjustment and setup list of periodicity selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxPeriodicityAdjustment_SelectedIndexChanged(object sender, EventArgs e)
        {
            object[] actualList = new object[] {"DAILY", "WEEKLY", "MONTHLY", "QUARTERLY", 
                "SEMI_ANNUALLY", "YEARLY"};
            object[] fiscalList = new object[] { "QUARTERLY", "SEMI_ANNUALLY", "YEARLY" };
            comboBoxPeriodicitySelection.Items.Clear();
            // set periodicity options
            switch (comboBoxPeriodicityAdjustment.SelectedItem.ToString())
            {
                case "FISCAL":
                    comboBoxPeriodicitySelection.Items.AddRange(fiscalList);
                    break;
                default:
                    comboBoxPeriodicitySelection.Items.AddRange(actualList);
                    break;
            }
            comboBoxPeriodicitySelection.SelectedIndex = 0;
        }

        /// <summary>
        /// Send historical request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            // clear data before submitting request
            clearData();
            if (!createSession())
            {
                toolStripStatusLabel1.Text = "Failed to start session.";
                return;
            }
            // start reference 
            if (!d_session.OpenService("//blp/refdata"))
            {
                toolStripStatusLabel1.Text = "Failed to open //blp/refdata";
                return;
            }
            toolStripStatusLabel1.Text = "Connected sucessfully";
            // get refdata service
            Service refDataService = d_session.GetService("//blp/refdata");
            // create historical request
            Request request = refDataService.CreateRequest("HistoricalDataRequest");
            // set security to request
            Element securities = request.GetElement("securities");
            securities.AppendValue(textBoxSecurity.Text);
            // set fields to request
            Element fields = request.GetElement("fields");
            for (int fieldIndex = 2; fieldIndex < dataGridViewData.ColumnCount; fieldIndex++)
                fields.AppendValue(dataGridViewData.Columns[fieldIndex].Name);
            // set historical request properties
            request.Set("periodicityAdjustment", comboBoxPeriodicityAdjustment.SelectedItem.ToString());
            request.Set("periodicitySelection", comboBoxPeriodicitySelection.SelectedItem.ToString());
            request.Set("currency", textBoxCurrencyCode.Text.Trim());
            if (tabControlDates.SelectedIndex == 0)
            {
                request.Set("startDate", dateTimePickerStart.Value.ToString("yyyyMMdd"));
                request.Set("endDate", dateTimePickerEndDate.Value.ToString("yyyyMMdd"));
            }
            else
            {
                request.Set("startDate", textBoxRelStartDate.Text.ToUpper().Trim());
                request.Set("endDate", textBoxRelEndDate.Text.ToUpper().Trim());
            }
            string nonTradingDayValue = string.Empty;
            switch (comboBoxNonTradingDayValue.SelectedIndex)
            {
                case 0:
                    nonTradingDayValue = "NON_TRADING_WEEKDAYS";
                    break;
                case 1:
                    nonTradingDayValue = "ALL_CALENDAR_DAYS";
                    break;
                case 2:
                    nonTradingDayValue = "ACTIVE_DAYS_ONLY";
                    break;
            }
            request.Set("nonTradingDayFillOption", nonTradingDayValue);
            string nonTradingDayMethod = string.Empty;
            switch (comboBoxNonTradingDayMethod.SelectedIndex)
            {
                case 0:
                    nonTradingDayMethod = "NIL_VALUE";
                    break;
                case 1:
                    nonTradingDayMethod = "PREVIOUS_VALUE";
                    break;
            }
            request.Set("nonTradingDayFillMethod", nonTradingDayMethod);
            string overrideOption = string.Empty;
            switch (comboBoxOverrideOption.SelectedIndex)
            {
                case 0:
                    overrideOption = "OVERRIDE_OPTION_CLOSE";
                    break;
                case 1:
                    overrideOption = "OVERRIDE_OPTION_GPA";
                    break;
            }
            request.Set("overrideOption", overrideOption);
            request.Set("maxDataPoints", textBoxMaxPoints.Text);
            request.Set("returnEids", true);
            // send request
            d_session.SendRequest(request, null);
            toolStripStatusLabel1.Text = "Submitted request. Waiting for response...";
            if (radioButtonSynch.Checked)
            {
                // synchronous request
                Application.DoEvents();
                while (true)
                {
                    // process data
                    Event eventObj = d_session.NextEvent();
                    toolStripStatusLabel1.Text = "Processing data...";
                    processEvent(eventObj, d_session);
                    if (eventObj.Type == Event.EventType.RESPONSE)
                    {
                        // response completed
                        break;
                    }
                }
                setControlStates();
                toolStripStatusLabel1.Text = "Completed";
            }
        }
        #endregion

        #region Bloomberg API Events
        /// <summary>
        /// Bloomberg data event
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
                            // process data
                            processRequestDataEvent(eventObj, session);
                            setControlStates();
                            toolStripStatusLabel1.Text = "Completed";
                            break;
                        case Event.EventType.PARTIAL_RESPONSE:
                            // process partial data
                            processRequestDataEvent(eventObj, session);
                            break;
                        default:
                            // process misc events
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
            string securityName = string.Empty;
            Boolean hasFieldError = false;
            // clear column tag of field error message.
            foreach (DataGridViewColumn col in dataGridViewData.Columns)
            {
                col.Tag = null;
            }
            // process message
            foreach (Message msg in eventObj)
            {
                if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("HistoricalDataResponse")))
                {
                    // process errors
                    if (msg.HasElement(RESPONSE_ERROR))
                    {
                        Element error = msg.GetElement(RESPONSE_ERROR);
                        dataGridViewData.Rows.Add(new object[]{ error.GetElementAsString(MESSAGE)});
                    }
                    else
                    {
                        Element secDataArray = msg.GetElement(SECURITY_DATA);
                        int numberOfSecurities = secDataArray.NumValues;
                        if (secDataArray.HasElement(SECURITY_ERROR))
                        {
                            // security error
                            Element secError = secDataArray.GetElement(SECURITY_ERROR);
                            dataGridViewData.Rows.Add(new object[] { secError.GetElementAsString(MESSAGE) });
                        }
                        if (secDataArray.HasElement(FIELD_EXCEPTIONS))
                        {
                            // field error
                            Element error = secDataArray.GetElement(FIELD_EXCEPTIONS);
                            for (int errorIndex = 0; errorIndex < error.NumValues; errorIndex++)
                            {
                                Element errorException = error.GetValueAsElement(errorIndex);
                                string field = errorException.GetElementAsString(FIELD_ID);
                                Element errorInfo = errorException.GetElement(ERROR_INFO);
                                string message = errorInfo.GetElementAsString(MESSAGE);
                                dataGridViewData.Columns[field].Tag = message;
                                hasFieldError = true;
                            } // end for 
                        } // end if
                        // process securities data
                        for (int index = 0; index < numberOfSecurities; index++)
                        {
                            foreach (Element secData in secDataArray.Elements)
                            {
                                switch (secData.Name.ToString())
                                {
                                    case "eidsData":
                                        // process security eid data here
                                        break;
                                    case "security":
                                        // security name
                                        securityName = secData.GetValueAsString();
                                        break;
                                    case "fieldData":
                                        if (hasFieldError && secData.NumValues == 0)
                                        {
                                            // no data but have field error
                                            object[] dataValues = new object[dataGridViewData.ColumnCount];
                                            dataValues[0] = securityName;
                                            int fieldIndex = 0;
                                            foreach (DataGridViewColumn col in dataGridViewData.Columns)
                                            {
                                                if (col.Tag != null)
                                                {
                                                    dataValues[fieldIndex] = col.Tag.ToString();
                                                }
                                                fieldIndex++;
                                            }
                                            d_data.Rows.Add(dataValues);
                                        }
                                        else
                                        {
                                            // get field data
                                            d_data.BeginLoadData();
                                            for (int pointIndex = 0; pointIndex < secData.NumValues; pointIndex++)
                                            {
                                                int fieldIndex = 0;
                                                object[] dataValues = new object[dataGridViewData.ColumnCount];
                                                Element fields = secData.GetValueAsElement(pointIndex);
                                                foreach (DataGridViewColumn col in dataGridViewData.Columns)
                                                {
                                                    try
                                                    {
                                                        if (col.Name == "security")
                                                            dataValues[fieldIndex] = securityName;
                                                        else
                                                        {
                                                            if (fields.HasElement(col.Name))
                                                            {
                                                                Element item = fields.GetElement(col.Name);
                                                                if (item.IsArray)
                                                                {
                                                                    // bulk field data
                                                                    dataValues[fieldIndex] = "Bulk Data";
                                                                }
                                                                else
                                                                {
                                                                    // field data
                                                                    dataValues[fieldIndex] = item.GetValueAsString();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // no field value
                                                                if (col.Tag == null)
                                                                {
                                                                    dataValues[fieldIndex] = DBNull.Value;
                                                                }
                                                                else
                                                                {
                                                                    if (col.Tag.ToString().Length > 0)
                                                                    {
                                                                        // field has error
                                                                        dataValues[fieldIndex] = col.Tag.ToString();
                                                                    }
                                                                    else
                                                                    {
                                                                        dataValues[fieldIndex] = DBNull.Value;
                                                                    }
                                                                }
                                                            }
                                                        }  // end if
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        // display error
                                                        dataValues[fieldIndex] = ex.Message;
                                                    }
                                                    finally
                                                    {
                                                        fieldIndex++;
                                                    }
                                                } // end foreach 
                                                // add data to data table
                                                d_data.Rows.Add(dataValues);
                                            } // end for
                                            d_data.EndLoadData();
                                        }
                                        break;
                                } // end switch
                            } // end foreach
                        } // end for
                    } // end else 
                } // end if
            }
        }

        /// <summary>
        /// Request status event
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