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
/// - Subscribe and de-subscribe intraday bar data for securities
///   using //blp/mktbar service.
/// ==========================================================
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
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
        private static readonly Name MARKET_BAR_START = new Name("MarketBarStart");
        private static readonly Name MARKET_BAR_END = new Name("MarketBarEnd");
        private static readonly Name MARKET_BAR_UPDATE = new Name("MarketBarUpdate");
        private static readonly Name TIME = new Name("TIME");
        private static readonly Name OPEN = new Name("OPEN");
        private static readonly Name HIGH = new Name("HIGH");
        private static readonly Name LOW = new Name("LOW");
        private static readonly Name CLOSE = new Name("CLOSE");
        private static readonly Name VOLUME = new Name("VOLUME");
        private static readonly Name NUMBER_OF_TICKS = new Name("NUMBER_OF_TICKS");
        
        private const string MKTBAR_SERVICE = "//blp/mktbar";

        private SessionOptions d_sessionOptions;
        private Session d_session;
        private List<Subscription> d_subscriptions;
        private TextWriter d_realtimeOutputFile;
        private String d_outputFileName;
        private Boolean d_isSubscribed = false;
        private List<Name> d_fields;

        public Form1()
        {
            InitializeComponent();

            string serverHost = "localhost";
            int serverPort = 8194;

            // set bar fields
            d_fields = new List<Name>();
            Name[] fields = {TIME, OPEN, HIGH, LOW, CLOSE, VOLUME, NUMBER_OF_TICKS};
            d_fields.AddRange(fields);

            d_subscriptions = new List<Subscription>();
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
            dateTimePickerStartTime.Value = DateTime.Now.ToUniversalTime().AddMinutes(2);
            dateTimePickerEndTime.Value = DateTime.Now.ToUniversalTime().AddMinutes(12);
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
                    int groupIndex = listViewRTIBData.Groups.Count + 1;
                    ListViewGroup group = null;
                    group = listViewRTIBData.Groups.Add(groupIndex.ToString(), security.Trim());
                    // add 1st item to group
                    ListViewItem listItem = CreateGroupItem(group, Color.White);
                    listViewRTIBData.Items.Add(listItem);
                }
            }
            setControlStates();
        }

        private ListViewItem CreateGroupItem(ListViewGroup group, Color backgroundColor)
        {
            ListViewItem listItem = new ListViewItem("", group);
            for (int index = 0; index < d_fields.Count; index++)
            {
                listItem.SubItems.Add("");
                listItem.SubItems[index].BackColor = backgroundColor;
            }
            // set tool tip
            listItem.ToolTipText = string.Empty;
            return listItem;
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
                // create new session
                d_session = new Session(d_sessionOptions, new EventHandler(processEvent));
            }
            return d_session.Start();
        }

        /// <summary>
        /// Manage control states
        /// </summary>
        private void setControlStates()
        {
            bool isTimeValid = !(dateTimePickerStartTime.Value >= dateTimePickerEndTime.Value &&
                (dateTimePickerEndTime.Checked && dateTimePickerStartTime.Checked));
            buttonSendRequest.Enabled = listViewRTIBData.Groups.Count > 0 && isTimeValid;
            buttonClearData.Enabled = (listViewRTIBData.Groups.Count > 0 && !d_isSubscribed);
            buttonClearAll.Enabled = (listViewRTIBData.Groups.Count > 0 && !d_isSubscribed);
            buttonStopSubscribe.Enabled = d_isSubscribed;
            panelTimeMessage.Visible = !isTimeValid;
            panelTimeMessage.BringToFront();
        }

        /// <summary>
        /// Set row color
        /// </summary>
        /// <param name="group"></param>
        /// <param name="cellColor"></param>
        private void setGroupColor(ListViewGroup group, Color cellColor)
        {
            foreach (ListViewItem item in group.Items)
            {
                for (int index = 0; index < item.SubItems.Count; index++)
                {
                    item.SubItems[index].BackColor = cellColor;
                }
            }
        }

        /// <summary>
        /// Open output file
        /// </summary>
        private void openOutputFile()
        {
            if (checkBoxOutputFile.Checked)
            {
                if (d_realtimeOutputFile == null)
                {
                    d_outputFileName = Application.StartupPath + "\\realtimeBarOutput" + DateTime.Now.ToString("MMddyyyy_HHmmss") + ".txt";
                    d_realtimeOutputFile = new StreamWriter("realtimeBarOutput" + DateTime.Now.ToString("MMddyyyy_HHmmss") + ".txt");
                }
                textBoxOutputFile.Text = d_outputFileName;
                textBoxOutputFile.Visible = checkBoxOutputFile.Checked;
            }
        }

        /// <summary>
        /// Clear security data rows
        /// </summary>
        private void clearData()
        {
            int count = 0;
            foreach (ListViewGroup group in listViewRTIBData.Groups)
            {
                // check for unsubscribed securities
                if (group.Items.Count > 0 && (group.Items[0].BackColor.IsEmpty ||
                    (group.Items[0].BackColor != Color.LightGreen &&
                     group.Items[0].BackColor != Color.Yellow &&
                     group.Items[0].BackColor != Color.Red)))
                {
                    // clear item in group
                    for (count = group.Items.Count - 1; count >= 1; count--)
                    {
                        listViewRTIBData.Items.Remove(group.Items[count]);
                    }
                    // clear sub-item data for last bar
                    for (count = 0; count < group.Items[0].SubItems.Count; count++)
                    {
                        group.Items[0].SubItems[count].Text = string.Empty;
                    }
                }
            }
            toolStripStatusLabel1.Text = string.Empty;
        }

        /// <summary>
        /// Remove all securities and fields from grid
        /// </summary>
        private void clearAll()
        {
            d_isSubscribed = false;
            listViewRTIBData.Items.Clear();
            listViewRTIBData.Groups.Clear();
            setControlStates();
        }

        /// <summary>
        /// Stop all subscriptions
        /// </summary>
        public void Stop()
        {
            if (d_subscriptions != null && d_isSubscribed)
            {
                // unsubscribe all securities
                d_session.Unsubscribe(d_subscriptions);
                d_subscriptions.Clear();
                // set all securities to white color for unsubscribe
                foreach (ListViewGroup group in listViewRTIBData.Groups)
                {
                    foreach (ListViewItem item in group.Items)
                    {
                        for (int index = 0; index < item.SubItems.Count; index++)
                        {
                            item.SubItems[index].BackColor = Color.White;
                        }
                    }
                }
                toolStripStatusLabel1.Text = "Subscription stopped";
            }
            d_isSubscribed = false;
        }
        #endregion end methods

        #region Control Events
        /// <summary>
        /// Allow only numeric keys for subscription interval
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxInterval_KeyDown(object sender, KeyEventArgs e)
        {
            // only allow 0 to 9, backspace, left and right keys
            if (!((e.KeyValue >= 48 && e.KeyValue <= 57) || e.KeyData == Keys.Back ||
                e.KeyData == Keys.Left || e.KeyData == Keys.Right))
            {
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Output subscription data to file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxOutputFile_CheckedChanged(object sender, EventArgs e)
        {
            if (d_isSubscribed && checkBoxOutputFile.Checked)
                openOutputFile();
            else
                if (!checkBoxOutputFile.Checked)
                {
                    // close output file
                    if (d_realtimeOutputFile != null)
                    {
                        d_realtimeOutputFile.Flush();
                        d_realtimeOutputFile.Close();
                        d_realtimeOutputFile = null;
                    }
                    textBoxOutputFile.Visible = false;
                }
        }

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
        private void listViewRTIBData_DragDrop(object sender, DragEventArgs e)
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
                    ListViewGroup  group = null;
                    // add security
                    group = listViewRTIBData.Groups.Add(listViewRTIBData.Groups.Count.ToString(), sec.Trim());
                    // add 1st item to group
                    ListViewItem listItem = CreateGroupItem(group, Color.White);
                    listViewRTIBData.Items.Add(listItem);
                }
            }
            setControlStates();
        }

        /// <summary>
        /// Mouse drag over grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewRTIBData_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }


        /// <summary>
        /// Subscribe to securities
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            double interval;
            List<string> options = new List<string>();
            List<Subscription> tempSubscriptions = new List<Subscription>();

            clearData();
            // create session
            if (!createSession())
            {
                toolStripStatusLabel1.Text = "Failed to start session.";
                return;
            }
            // open market data service
            if (!d_session.OpenService(MKTBAR_SERVICE))
            {
                toolStripStatusLabel1.Text = "Failed to open " + MKTBAR_SERVICE;
                return;
            }
            toolStripStatusLabel1.Text = "Connected sucessfully";
            if (numericUpDownIntervalSize.Value > 0)
            {
                List<string> securities = new List<string>();
                string sec;
                foreach (ListViewGroup row in listViewRTIBData.Groups)
                {
                    // check for unsubscribed securities
                    if (row.Items.Count > 0 && (row.Items[0].BackColor.IsEmpty ||
                        (row.Items[0].BackColor != Color.LightGreen && 
                         row.Items[0].BackColor != Color.Yellow && 
                         row.Items[0].BackColor != Color.Red )))
                    {
                        sec = string.Empty;
                        if (row.Header.StartsWith("/"))
                        {
                            if (!row.Header.StartsWith(MKTBAR_SERVICE))
                            {
                                // add //blp/mktbar in front of security identifier
                                sec = MKTBAR_SERVICE + row.Header;
                            }
                        }
                        else
                        {
                            // add //blp/mktbar/ticker/ in front of security identifier
                            sec = MKTBAR_SERVICE + "/ticker/" + row.Header;
                        }

                        // set main bar field
                        List<string> fields = new List<string>();
                        fields.Add("LAST_PRICE");

                        // get bar interval
                        interval = (double)numericUpDownIntervalSize.Value;
                        // set bar interval, start time and end time
                        options.Clear();
                        interval = (double)numericUpDownIntervalSize.Value;
                        options.Add("interval=" + interval.ToString());
                        if (dateTimePickerStartTime.Checked)
                        {
                            // start time format HH:mm in GMT
                            options.Add("start_time=" + dateTimePickerStartTime.Value.ToString("HH:mm"));
                        }
                        if (dateTimePickerEndTime.Checked)
                        {
                            // end time format HH:mm in GMT
                            options.Add("end_time=" + dateTimePickerEndTime.Value.ToString("HH:mm"));
                        }
                        // create subscription object
                        Subscription subscription = new Subscription(sec, fields, options, new CorrelationID(row));
                        // add subscription to temp subscription list
                        tempSubscriptions.Add(subscription);
                        // add to application subscription list
                        d_subscriptions.Add(subscription);
                        // store subscription string
                        row.Tag = subscription.SubscriptionString;
                    }
                }
            }
            if (tempSubscriptions.Count > 0)
            {
                // open output file
                openOutputFile();
                // subscribe to securities
                d_session.Subscribe(tempSubscriptions);
                d_isSubscribed = true;
                setControlStates();
                toolStripStatusLabel1.Text = "Subscribed to securities.";
            }
        }

        /// <summary>
        /// Realtime Intraday Bar delete security
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewRTIBData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (!d_isSubscribed)
                {
                    if (listViewRTIBData.SelectedItems.Count > 0)
                    {
                        ListViewItem selectedItem = listViewRTIBData.SelectedItems[0];
                        ListViewGroup group = selectedItem.Group;
                        for (int count = group.Items.Count - 1; count >= 0; count--)
                        {
                            listViewRTIBData.Items.Remove(group.Items[count]);
                        }
                        listViewRTIBData.Groups.Remove(group);
                    }
                }
            }
        }

        private void dateTimePickerStartTime_ValueChanged(object sender, EventArgs e)
        {
            setControlStates();
        }

        /// <summary>
        /// Show subscription string in tooltip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewRTIBData_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewItem item = listViewRTIBData.GetItemAt(e.X, e.Y);
            if (item == null)
            {
                // clear tool tip
                toolTip1.SetToolTip(listViewRTIBData, "");
            }
            else
            {
                if (item.Group.Tag == null)
                {
                    // clear tool tip
                    toolTip1.SetToolTip(listViewRTIBData, "");
                }
                else
                {
                    // display subscription string
                    toolTip1.SetToolTip(listViewRTIBData, item.Group.Tag.ToString());
                }
            }
        }
        #endregion

        #region Bloomberg API Events
        /// <summary>
        /// close output file on form close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (d_realtimeOutputFile != null)
            {
                d_realtimeOutputFile.Flush();
                d_realtimeOutputFile.Close();
                d_realtimeOutputFile = null;
            }
        }

        /// <summary>
        /// Data Event
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
                            // process subscription data
                            processRequestDataEvent(eventObj, session);
                            break;
                        case Event.EventType.SUBSCRIPTION_STATUS:
                            // process subscription status
                            processRequestStatusEvent(eventObj, session);
                            break;
                        default:
                            // Other status events
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
            ListViewGroup group = null;
            ListViewItem item = null;

            // process message
            foreach (Message msg in eventObj)
            {
                // get correlation id
                group = (ListViewGroup)msg.CorrelationID.Object;
                // output to file
                if (checkBoxOutputFile.Checked)
                {
                    d_realtimeOutputFile.WriteLine(msg.TopicName + ":\n" + msg.ToString());
                    d_realtimeOutputFile.Flush();
                }
                // Get security item to update
                //if (msg.MessageType == MARKET_BAR_UPDATE)
                if (msg.MessageType.Equals(MARKET_BAR_UPDATE))
                {
                    if (group.Items[0].BackColor == Color.Yellow)
                    {
                        // set waiting for subscription to start color
                        setGroupColor(group, Color.LightGreen);
                    }
                    // get last group item to update
                    item = group.Items[group.Items.Count - 1];
                }
                else
                {
                    if (msg.MessageType.Equals(MARKET_BAR_START))
                    {
                        if (group.Items.Count == 1 && group.Items[0].SubItems[1].Text.Length == 0)
                        {
                            // use empty row
                            item = group.Items[group.Items.Count - 1];
                        }
                        else
                        {
                            // create new group item
                            item = CreateGroupItem(group, Color.LightGreen);
                            listViewRTIBData.Items.Add(item);
                        }
                    }
                    else
                    {
                        // MarketBarEnd, create new group seperator item
                        item = CreateGroupItem(group, Color.White);
                        listViewRTIBData.Items.Add(item);
                        // set waiting for subscription to start color
                        setGroupColor(group, Color.Yellow);
                        continue;
                    }
                }
                item.Text = group.Header;
                int index = 0;
                foreach (Element field in msg.Elements)
                {
                    index = d_fields.IndexOf(field.Name);
                    item.SubItems[index + 1].Text = field.GetValueAsString();
                }
            }
            // allow application to update UI
            Application.DoEvents();
        }


        /// <summary>
        /// Request status event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processRequestStatusEvent(Event eventObj, Session session)
        {
            List<string> dataList = new List<string>();
            // process status message
            foreach (Message msg in eventObj)
            {
                ListViewGroup group = (ListViewGroup)msg.CorrelationID.Object;
                if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("SubscriptionStarted")))
                {
                    // set waiting for subscription to start color
                    setGroupColor(group, Color.Yellow);
                }
                else
                {
                    // check for subscription failure
                    if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("SubscriptionFailure")))
                    {
                        // set exception color
                        setGroupColor(group, Color.Red);
                        if (msg.HasElement(REASON))
                        {
                            Element reason = msg.GetElement(REASON);
                            string message = reason.GetElementAsString(DESCRIPTION);
                            group.Items[0].SubItems[1].Text = message;
                        }
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
                        toolStripStatusLabel1.Text = msg.MessageType.ToString();
                        break;
                    case "SessionTerminated":
                    case "SessionStopped":
                        // "Session Terminated"
                        toolStripStatusLabel1.Text = msg.MessageType.ToString();
                        break;
                    case "SessionStartupFailure":
                        // Failed to start session
                        toolStripStatusLabel1.Text = msg.MessageType.ToString();
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