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
// ==========================================================
/// Purpose of this example:
/// - Subscribe to Custom VWAP, Bloomberg VWAP, and 
///   Market VWAP using //blp/mktvwap service.
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
using Subscription = Bloomberglp.Blpapi.Subscription;

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
        private List<Subscription> d_subscriptions;
        private Boolean d_isSubscribed = false;

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
            dataGridViewData.Columns.Add("security", "security");
            // default with VWap fields
            textBoxField.Text = "VWAP,RT_VWAP_VOLUME,VWAP_TURNOVER_RT,VWAP_NUM_TRADES_RT,VWAP_STANDARD_DEV_RT";
            // default with Market VWap fields
            //textBoxField.Text = "MARKET_DEFINED_VWAP_REALTIME,RT_MKT_VWAP_VOLUME," +
            //        "MKT_DEF_VWAP_TURNOVER_RT,MKT_DEF_VWAP_NUM_TRADES_RT," +
            //        "MKT_DEF_VWAP_STANDARD_DEV_RT";
        }

        /// <summary>
        /// Add securities to grid
        /// </summary>
        /// <param name="securities"></param>
        private void addSecurities(string securities)
        {
            // Tokenize the string into what (we hope) are Security strings
            char[] sep = { '\r', '\n', '\t', ',' };
            string[] words = securities.Split(sep);
            foreach (string security in words)
            {
                if (security.Trim().Length > 0)
                {
                    // add security
                    dataGridViewData.Rows.Add(security.Trim());
                }
            }
            setControlStates();
        }

        /// <summary>
        /// Add fields to grid
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
                    if (!dataGridViewData.Columns.Contains(field.Trim()))
                    {
                        dataGridViewData.Columns.Add(field.Trim(), field.Trim());
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
            if (d_session == null)
            {
                toolStripStatusLabel1.Text = "Connecting...";
                d_session = new Session(d_sessionOptions, new EventHandler(processEvent));
            }
            return d_session.Start();
        }

        /// <summary>
        /// Manage control states
        /// </summary>
        private void setControlStates()
        {
            buttonSendRequest.Enabled = dataGridViewData.Rows.Count > 0 && dataGridViewData.Columns.Count > 1 && !d_isSubscribed;
            buttonClearFields.Enabled = dataGridViewData.Columns.Count > 1 && !d_isSubscribed;
            buttonClearData.Enabled = buttonSendRequest.Enabled;
            buttonClearAll.Enabled = (dataGridViewData.Rows.Count > 0 || dataGridViewData.Columns.Count > 1) && !d_isSubscribed;
            buttonStopSubscribe.Enabled = d_isSubscribed;
            labelSecurity.Enabled = !d_isSubscribed;
            textBoxSecurity.Enabled = !d_isSubscribed;
            buttonAddSecurity.Enabled = !d_isSubscribed;
            labelField.Enabled = !d_isSubscribed;
            textBoxField.Enabled = !d_isSubscribed;
            buttonAddField.Enabled = !d_isSubscribed;
            labelOverride.Enabled = !d_isSubscribed; 
            textBoxOverride.Enabled = labelOverride.Enabled;
            buttonAddOverride.Enabled = labelOverride.Enabled;
            listViewOverrides.Enabled = labelOverride.Enabled;
            labelOverrideNote.Enabled = labelOverride.Enabled;
        }

        /// <summary>
        /// Clear security data
        /// </summary>
        private void clearData()
        {
            if (dataGridViewData.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewData.Rows)
                {
                    for (int index = 1; index < dataGridViewData.Columns.Count; index++)
                    {
                        row.Cells[index].Value = string.Empty;
                    }
                }
            }
            toolStripStatusLabel1.Text = string.Empty;
        }

        /// <summary>
        /// Remove all fields from grid
        /// </summary>
        private void clearFields()
        {
            for (int index = dataGridViewData.Columns.Count - 1; index > 0; index--)
            {
                dataGridViewData.Columns.RemoveAt(index);
            }
            toolStripStatusLabel1.Text = string.Empty;
        }

        /// <summary>
        /// Remove all securities and fields from grid
        /// </summary>
        private void clearAll()
        {
            d_isSubscribed = false;
            dataGridViewData.Rows.Clear();
            clearFields();
            setControlStates();
        }

        /// <summary>
        /// Stop all subscriptions
        /// </summary>
        public void Stop()
        {
            if (d_subscriptions != null && d_isSubscribed)
            {
                d_session.Unsubscribe(d_subscriptions);
                toolStripStatusLabel1.Text = "Subscription stopped.";
                foreach (DataGridViewRow row in dataGridViewData.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.BackColor = Color.White;
                    }
                }
            }
            d_isSubscribed = false;
        }
        #endregion end methods

        #region Control Events
        /// <summary>
        /// Add security button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAddSecurity_Click(object sender, EventArgs e)
        {
            if (textBoxSecurity.Text.Trim().Length > 0)
            {
                addSecurities(textBoxSecurity.Text.Trim());
                textBoxSecurity.Text = string.Empty;
                setControlStates();
            }
        }

        /// <summary>
        /// Enter key pressed to add security to grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxSecurity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                buttonAddSecurity_Click(this, new EventArgs());
            }
        }

        /// <summary>
        /// Add field button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAddField_Click(object sender, EventArgs e)
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
                buttonAddField_Click(this, new EventArgs());
            }
        }

        /// <summary>
        /// Stop subscription button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonStopSubscribe_Click(object sender, EventArgs e)
        {
            Stop();
            setControlStates();
        }

        /// <summary>
        /// Remove all fields button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearFields_Click(object sender, EventArgs e)
        {
            clearFields();
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
        }

        /// <summary>
        /// Remove securities and fields button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            clearAll();
            setControlStates();
        }

        /// <summary>
        /// Allow drag and drop of securities and fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewData_DragDrop(object sender, DragEventArgs e)
        {
            // Get the entire text object that has been dropped on us.
            string tmp = e.Data.GetData(DataFormats.Text).ToString();
            // Tokenize the string into what (we hope) are Security strings
            char[] sep = { '\r', '\n', '\t' };
            string[] words = tmp.Split(sep);
            foreach (string sec in words)
            {
                if (sec.Trim().Length > 0)
                {
                    if (sec.Trim().Contains(" "))
                    {
                        // add securities
                        dataGridViewData.Rows.Add(new object[] { sec.Trim() });
                    }
                    else
                    {
                        // add fields
                        dataGridViewData.Columns.Add(sec.Trim(), sec.Trim());
                    }
                }
            }
            setControlStates();
        }

        /// <summary>
        /// Mouse drag over grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewData_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// Allow user to delete single field or security from grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewData_KeyDown(object sender, KeyEventArgs e)
        {
            DataGridView dataGrid = (DataGridView)sender;
            if (e.KeyData == Keys.Delete && dataGrid.SelectedCells.Count > 0 && !d_isSubscribed)
            {
                int rowIndex = dataGrid.SelectedCells[0].RowIndex;
                int columnIndex = dataGrid.SelectedCells[0].ColumnIndex;
                if (columnIndex > 0)
                {
                    // remove field
                    dataGrid.Columns.RemoveAt(columnIndex);
                }
                else
                {
                    // remove security
                    dataGrid.Rows.RemoveAt(rowIndex);
                }
                if (dataGrid.DataSource != null)
                    ((DataSet)dataGrid.DataSource).AcceptChanges();
                if (dataGrid.Columns.Count > columnIndex && columnIndex > 0)
                    dataGrid.Rows[rowIndex].Cells[columnIndex].Selected = true;
                else
                    if (dataGrid.Columns.Count > 1 && dataGrid.Columns.Count == columnIndex)
                        dataGrid.Rows[rowIndex].Cells[columnIndex - 1].Selected = true;
            }
        }

        /// <summary>
        /// Drag and drop override fields and values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewOverrides_DragDrop(object sender, DragEventArgs e)
        {
            // Get the entire text object that has been dropped on us.
            string tmp = e.Data.GetData(DataFormats.Text).ToString();
            List<string> values = new List<string>();
            List<string> fields = new List<string>();
            // Tokenize the string into what (we hope) are Security strings
            char[] sep = { '\r', '\n', '\t' };
            string[] words = tmp.Split(sep);
            foreach (string sec in words)
            {
                if (sec.Contains("="))
                {
                    string[] ovr = sec.Split(new char[] { '=' });

                    if (ovr[0].Trim().Length > 0)
                    {
                        ListViewItem item = listViewOverrides.Items.Add(ovr[0].Trim());
                        item.SubItems.Add(ovr[1].Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Mouse drag over override listView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewOverrides_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// Allow user to delete single override field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewOverrides_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && listViewOverrides.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewOverrides.SelectedItems)
                {
                    item.Remove();
                }
            }
        }

        /// <summary>
        /// Enter key pressed to add override field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxOverride_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                buttonAddOverride_Click(sender, new EventArgs());
            }
        }

        /// <summary>
        /// Add override field to list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAddOverride_Click(object sender, EventArgs e)
        {
            if (textBoxOverride.Text.Length == 0 || !textBoxOverride.Text.Contains("="))
            {
                MessageBox.Show("Missing field or missing '=' seperator between field and value", "Add Field", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxOverride.Focus();
            }
            else
            {
                string[] input = textBoxOverride.Text.Split(new char[] { ',' });
                foreach (string overrideItem in input)
                {
                    if (overrideItem.Trim().Length > 0 && overrideItem.Contains("="))
                    {
                        string[] ovr = overrideItem.Split(new char[] { '=' });
                        ListViewItem item = listViewOverrides.Items.Add(ovr[0].Trim());
                        item.SubItems.Add(ovr[1].Trim());
                    }
                }
                textBoxOverride.Text = string.Empty;
            }
            setControlStates();
        }

        /// <summary>
        /// Subscribe to custom VWAP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            const string prefix = @"//blp/mktvwap/ticker/";
            // clear data 
            clearData();
            // create session
            if (!createSession())
            {
                toolStripStatusLabel1.Text = "Failed to start session.";
                return;
            }
            if (!d_session.OpenService("//blp/mktvwap"))
            {
                toolStripStatusLabel1.Text = "Failed to open //blp/mktvwap";
                return;
            }
            toolStripStatusLabel1.Text = "Connected sucessfully";

            List<string> fields = new List<string>();
            List<string> options = new List<string>();
            d_subscriptions = new List<Subscription>();
            // populate fields
            for (int fieldIndex = 1; fieldIndex < dataGridViewData.Columns.Count; fieldIndex++)
                fields.Add(dataGridViewData.Columns[fieldIndex].Name);
            // add overrides
            foreach (ListViewItem item in listViewOverrides.Items)
            {
                options.Add(item.Text + "=" + item.SubItems[1].Text);
            }
            // create subscription 
            foreach (DataGridViewRow secRow in dataGridViewData.Rows)
            {
                string security = secRow.Cells["security"].Value.ToString();
                d_subscriptions.Add(new Subscription(prefix + security, fields, options, new CorrelationID(secRow)));
            }

            // subscribe to securities
            d_session.Subscribe(d_subscriptions);
            d_isSubscribed = true;
            setControlStates();
            toolStripStatusLabel1.Text = "Subscribed to securities.";
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
                        case Event.EventType.SUBSCRIPTION_DATA:
                            processRequestDataEvent(eventObj, session);
                            break;
                        case Event.EventType.SUBSCRIPTION_STATUS:
                            processRequestStatusEvent(eventObj, session);
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
            foreach (Message msg in eventObj)
            {
                DataGridViewRow dataRow = (DataGridViewRow)msg.CorrelationID.Object;
                if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("MarketDataEvents")))
                {
                    // check for initial paint
                    if (msg.HasElement("SES_START"))
                    {
                        if (msg.GetElementAsBool("IS_DELAYED_STREAM"))
                        {
                            // set to delay stream color
                            foreach (DataGridViewCell cell in dataRow.Cells)
                            {
                                cell.Style.BackColor = Color.LightSkyBlue;
                            }
                        }
                    }

                    // process tick data
                    for (int fieldIndex = 1; fieldIndex < dataGridViewData.ColumnCount; fieldIndex++)
                    {
                        string field = dataGridViewData.Columns[fieldIndex].Name;

                        if (msg.HasElement(field))
                            dataRow.Cells[field].Value = msg.GetElementAsString(field);
                    }
                    // allow application to update UI
                    Application.DoEvents();
                }
            }
        }

        /// <summary>
        /// Request status event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processRequestStatusEvent(Event eventObj, Session session)
        {
            List<string> dataList = new List<string>();

            foreach (Message msg in eventObj)
            {

                DataGridViewRow dataRow = (DataGridViewRow)msg.CorrelationID.Object;

                if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("SubscriptionStarted")))
                {
                    // set subscribed color
                    foreach (DataGridViewCell cell in dataRow.Cells)
                    {
                        cell.Style.BackColor = Color.LightGreen;
                    }
                    try
                    {
                        // check exceptions
                        if (msg.HasElement("exceptions"))
                        {
                            Element error = msg.GetElement("exceptions");
                            for (int errorIndex = 0; errorIndex < error.NumValues; errorIndex++)
                            {
                                Element errorException = error.GetValueAsElement(errorIndex);
                                string field = errorException.GetElementAsString(FIELD_ID);
                                Element reason = errorException.GetElement(REASON);
                                string message = reason.GetElementAsString(DESCRIPTION);
                                if (dataGridViewData.Columns.Contains(field))
                                    dataRow.Cells[field].Value = message;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        toolStripStatusLabel1.Text = "Error: " + e.Message;
                    }
                }
                else
                    if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("SubscriptionFailure")))
                    {
                        if (msg.HasElement(REASON))
                        {
                            Element reason = msg.GetElement(REASON);
                            string message = reason.GetElementAsString(DESCRIPTION);

                            dataRow.Cells[1].Value = message;
                        }
                    }
            }
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
                        // "Service Opened"
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