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
/// - Validate user logged on status using IP or Token method
///   using //blp/auth service.
/// - Check user's security entitlements using Identity and
///   //blp/refdata service.
/// - User mode security subscription
/// - Use of Name Enumeration.
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
using Identity = Bloomberglp.Blpapi.Identity;
using EventQueue = Bloomberglp.Blpapi.EventQueue;
using NameEnumeration = Bloomberglp.Blpapi.NameEnumeration;
using NameEnumerationTable = Bloomberglp.Blpapi.NameEnumerationTable;

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
        private Service d_authService;
        private Service d_refService;
        private Service d_mktdataService;
        private DataTable d_users;
        private DataTable d_submittedUserList;
        private DataTable d_securityEntitlements;
        private List<Subscription> d_subscriptions;
        private Dictionary<String, Identity> d_identityList = new Dictionary<String, Identity>();
        private Dictionary<String, Identity> d_submittedIdentityList = new Dictionary<String, Identity>();
        private NameEnumerationTable d_messageTypeTable;
        private int d_submittedRequests = 0;
        private Boolean d_isSubscribed = false;

        // Enumerate logon status
        private enum LogonStatus
        {
            Unknown,
            LoggedOn,
            LoggedOff
        }

        // Enumerate Message Types
        public class MessageType : NameEnumeration
        {
            public const int ENTITLEMENT_CHANGED = 1;
            public const int AUTHORIZATION_SUCCESS = 2;
            public const int AUTHORIZATION_REVOKED = 3;
            public const int TOKEN_RESPONSE = 4;
            public const int AUTHORIZATION_FAILURE = 5;
            public const int REFERENCE_DATA_RESPONSE = 6;
            public const int RESPONSE_ERROR = 7;

            public class NameBindings
            {
                public const string ENTITLEMENT_CHANGED = "EntitlementChanged";
                public const string AUTHORIZATION_SUCCESS = "AuthorizationSuccess";
                public const string AUTHORIZATION_REVOKED = "AuthorizationRevoked";
                public const string TOKEN_RESPONSE = "TokenResponse";
                public const string AUTHORIZATION_FAILURE = "AuthorizationFailure";
                public const string REFERENCE_DATA_RESPONSE = "ReferenceDataResponse";
                public const string RESPONSE_ERROR = "ResponseError";
            }
        }

        public Form1()
        {
            InitializeComponent();

            string serverHost = "localhost";
            int serverPort = 8294;

            // set sesson options
            d_sessionOptions = new SessionOptions();
            d_sessionOptions.ServerHost = serverHost;
            d_sessionOptions.ServerPort = serverPort;
            d_messageTypeTable = new NameEnumerationTable(new MessageType());

            // create session connection
            createSession();
            // initialize dataset
            initData();
            // initialize UI controls
            initUI();
        }

        #region methods
        /// <summary>
        /// Initialize form controls
        /// </summary>
        private void initUI()
        {
            setControlStates();
        }

        /// <summary>
        /// Initialize data tables
        /// </summary>
        private void initData()
        {
            DataColumn column;
            d_users = new DataTable();
            // setup user information table
            column = d_users.Columns.Add("Name", typeof(String));
            column.MaxLength = 40;
            column = d_users.Columns.Add("UUID", typeof(Int64));
            column = d_users.Columns.Add("IP", typeof(String));
            column.MaxLength = 15;
            d_users.AcceptChanges();

            // setup security information table
            d_securityEntitlements = new DataTable();
            column = d_securityEntitlements.Columns.Add("Security", typeof(String));
            column.MaxLength = 50;
            column = d_securityEntitlements.Columns.Add("Eids Info", typeof(String));
            column.MaxLength = 200;
            d_securityEntitlements.AcceptChanges();

            // set security entitlement data grid data source
            dataGridViewSecEntitlement.DataSource = d_securityEntitlements;
            dataGridViewSecEntitlement.Columns["Eids Info"].Visible = checkBoxSecEIDs.Checked;

            // set user mode data grid columns
            dataGridViewUserData.Columns.Add("Security", "Security");
            dataGridViewUserData.Columns.Add("LAST_PRICE", "LAST_PRICE");
            dataGridViewUserData.Columns.Add("TIME", "TIME");
        }

        /// <summary>
        /// Initialize users in security entitlement table
        /// </summary>
        private void initUserSecEntitlement()
        {
            // remove old users from table
            for (int index = d_securityEntitlements.Columns.Count - 1; index > 1; index--)
            {
                d_securityEntitlements.Columns.RemoveAt(index);
            }
            // add new users to table
            foreach (DataRow row in d_submittedUserList.Rows)
            {
                d_securityEntitlements.Columns.Add(row["Name"].ToString(), typeof(Boolean));
            }
            d_securityEntitlements.AcceptChanges();
        }

        /// <summary>
        /// Create session
        /// </summary>
        /// <returns></returns>
        private bool createSession()
        {
            bool isStarted = true;
            if (d_session == null)
            {
                // create session connection
                toolStripStatusLabel1.Text = "Connecting...";
                d_session = new Session(d_sessionOptions, new EventHandler(processEvent));
                isStarted = d_session.Start();
            }
            return isStarted;
        }

        /// <summary>
        /// Manage control states
        /// </summary>
        private void setControlStates()
        {
            bool userListState = (d_users.Rows.Count > 0);
            bool secEIDsState = false;
            if (d_submittedUserList != null)
                secEIDsState = (d_submittedUserList.Rows.Count > 0);
            // user list
            buttonValidateUser.Enabled = userListState;
            buttonClearList.Enabled = userListState;
            buttonSaveList.Enabled = userListState;
            buttonRemoveUser.Enabled = userListState;
            buttonSubmitUsers.Enabled = userListState;
            buttonAddSecurity.Enabled = secEIDsState;
            textBoxSecurity.Enabled = secEIDsState;
            labelSecurity.Enabled = secEIDsState;
            dataGridViewSecEntitlement.Enabled = secEIDsState;
            checkBoxSecEIDs.Enabled = secEIDsState;
            labelSelectUser.Enabled = secEIDsState && !d_isSubscribed;
            comboBoxSelectUser.Enabled = secEIDsState && !d_isSubscribed;
            labelUserSecurity.Enabled = secEIDsState && !d_isSubscribed;
            textBoxUserSecurity.Enabled = secEIDsState && !d_isSubscribed;
            buttonAddUserSecurity.Enabled = secEIDsState && !d_isSubscribed;
            dataGridViewUserData.Enabled = secEIDsState;

            // user security entitlement controls
            if (d_securityEntitlements.Rows.Count > 0)
            {
                buttonUserEntitlementValidation.Enabled = secEIDsState;
                buttonRemoveSec.Enabled = secEIDsState;
                buttonClearAll.Enabled = secEIDsState;
                checkBoxSecEIDs.Enabled = secEIDsState;
            }
            if (dataGridViewUserData.Rows.Count > 0)
            {
                buttonUserSubscribe.Enabled = secEIDsState && !d_isSubscribed;
                buttonRemoveUserSecurity.Enabled = secEIDsState && !d_isSubscribed;
                buttonClearAllUserData.Enabled = secEIDsState && !d_isSubscribed;
                buttonUserStopSubscription.Enabled = secEIDsState && d_isSubscribed;
            }
            else
            {
                buttonUserSubscribe.Enabled = false;
                buttonUserStopSubscription.Enabled = false;
                buttonRemoveUserSecurity.Enabled = false;
                buttonClearAllUserData.Enabled = false;
            }
        }

        /// <summary>
        /// Add user to grid
        /// </summary>
        /// <param name="name"></param>
        /// <param name="uuid"></param>
        /// <param name="ip"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private DataGridViewRow populateUserGrid(string name, string uuid, string ip, LogonStatus status)
        {
            Image logonStatusImage;
            // set user status image
            switch (status)
            {
                case LogonStatus.LoggedOn:
                    logonStatusImage = imageList1.Images[0];
                    break;
                case LogonStatus.LoggedOff:
                    logonStatusImage = imageList1.Images[1];
                    break;
                default:
                    logonStatusImage = imageList1.Images[2];
                    break;
            }
            // add user to grid
            dataGridViewUsers.Rows.Add(new object[] { name, uuid, ip, logonStatusImage });
            return dataGridViewUsers.Rows[dataGridViewUsers.Rows.Count - 1];
        }

        /// <summary>
        /// Add security to grid
        /// </summary>
        /// <param name="security"></param>
        private void addSecurity(String securities)
        {
            // Tokenize the string into what (we hope) are Security strings
            char[] sep = { '\r', '\n', '\t', ',' };
            string[] words = securities.Split(sep);
            foreach (string security in words)
            {
                if (security.Trim().Length > 0)
                {
                    // add security
                    d_securityEntitlements.Rows.Add(new Object[] { security.Trim() });
                }
            }
            setControlStates();
        }

        /// <summary>
        /// Add user mode security to grid
        /// </summary>
        /// <param name="securities"></param>
        private void addUserModeSecurity(String securities)
        {
            // Tokenize the string into what (we hope) are Security strings
            char[] sep = { '\r', '\n', '\t', ',' };
            string[] words = securities.Split(sep);
            foreach (string security in words)
            {
                if (security.Trim().Length > 0)
                {
                    // add security
                    dataGridViewUserData.Rows.Add(new Object[] { security.Trim() });
                }
            }
            setControlStates();
        }

        /// <summary>
        /// Clear user mode subscription data
        /// </summary>
        private void clearUserModeSubscriptionData()
        {
            foreach (DataGridViewRow row in dataGridViewUserData.Rows)
            {
                for (int index = 1; index < dataGridViewUserData.ColumnCount; index++)
                {
                    row.Cells[index].Value = string.Empty;
                }
            }
        }

        /// <summary>
        /// Request reference data for securities to get security EIDs
        /// </summary>
        private void submitRequest()
        {
            int correlationID = 1000;
            // get reference service
            if (d_refService == null)
                if (d_session.OpenService("//blp/refdata"))
                    d_refService = d_session.GetService("//blp/refdata");
                else
                    toolStripStatusLabel1.Text = "Unable to open refdata service.";

            // create request object
            Request refRequest = d_refService.CreateRequest("ReferenceDataRequest");
            // add securities to request
            Element securities = refRequest.GetElement("securities");
            foreach (DataRow row in d_securityEntitlements.Rows)
            {
                securities.AppendValue(row["Security"].ToString());
            }
            // add fields to request
            Element fields = refRequest.GetElement("fields");
            fields.AppendValue("PX_LAST");
            fields.AppendValue("API_MACHINE");
            // return security EIDs
            refRequest.Set("returnEids", true); 
            // create request correlation id
            CorrelationID correlator = new CorrelationID(correlationID);
            // make sure there is no outstanding for this correlation id
            d_session.Cancel(correlator); 
            // submit request
            d_session.SendRequest(refRequest, correlator);
            toolStripStatusLabel1.Text = "Submit security entitlement request";
            d_submittedRequests = 1;
        }

        /// <summary>
        /// Subscribe to securities
        /// </summary>
        /// <param name="user"></param>
        private void subscriptionRequest(Identity identity)
        {
            // clear security data only
            clearUserModeSubscriptionData();
            // get session
            if (!createSession())
            {
                toolStripStatusLabel1.Text = "Failed to start session.";
                return;
            }
            toolStripStatusLabel1.Text = "Connected sucessfully";
            List<string> fields = new List<string>();
            List<string> options = new List<string>();
            d_subscriptions = new List<Subscription>();
            // populate fields
            for (int fieldIndex = 1; fieldIndex < dataGridViewUserData.Columns.Count; fieldIndex++)
            {
                fields.Add(dataGridViewUserData.Columns[fieldIndex].Name);
            }
            // subscribe to security
            foreach (DataGridViewRow secRow in dataGridViewUserData.Rows)
            {
                d_subscriptions.Add(new Subscription(secRow.Cells["Security"].Value.ToString(), fields, options, new CorrelationID(secRow)));
            }
            // subscribe to securities using Identity credential
            d_session.Subscribe(d_subscriptions, identity);
            d_isSubscribed = true;
            setControlStates();
            toolStripStatusLabel1.Text = "Subscribed to securities.";
        }

        /// <summary>
        /// Stop all subscriptions
        /// </summary>
        public void StopSubscriptions()
        {
            if (d_subscriptions != null)
            {
                d_session.Unsubscribe(d_subscriptions);
                // set all securities to white color for unsubscribe
                foreach (DataGridViewRow row in dataGridViewUserData.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.BackColor = Color.White;
                    }
                }
                toolStripStatusLabel1.Text = "Subscription stopped";
            }
            d_isSubscribed = false;
            setControlStates();
        }

        /// <summary>
        /// Validate user entitlement against security EIDs
        /// </summary>
        /// <param name="security"></param>
        /// <param name="eids"></param>
        /// <param name="service"></param>
        private void checkUserEntitlement(String security, Element eids, Service service)
        {
            // check entitlment for each user in the submitted userhadle list 
            foreach (KeyValuePair<String, Identity> item in d_submittedIdentityList)
            {
                string msg = string.Empty;
                // get identity
                Identity identity = item.Value;
                String userName = d_submittedUserList.Select("UUID = '" + item.Key + "'")[0]["Name"].ToString();
                // get data row for security
                DataRow[] rows = d_securityEntitlements.Select("Security = '" + security + "'");
                if (rows.GetLength(0) > 0)
                {
                    // validate user entitlement
                    List<int> failedEids = new List<int>();
                    Boolean entitled = identity.HasEntitlements(eids, service, failedEids);
                    // get security eids
                    msg = eids.ToString().Replace("eidData[] = {\n ", "");
                    msg = msg.Replace("\n}\n", "");
                    // set user entitlement checkbox for security
                    foreach (DataRow row in rows)
                    {
                        // set entitlements
                        row[userName] = entitled;
                        rows[0]["Eids Info"] = msg;
                    }
                }
            }
        }

        #region Auth IP
        /// <summary>
        /// Check user logon status using IP
        /// </summary>
        /// <param name="UUID"></param>
        /// <param name="IP"></param>
        /// <param name="asynchronous"></param>
        /// <param name="correlation"></param>
        private void checkUserLoginStatus(string UUID, string IP, bool asynchronous)
        {
            try
            {
                // open auth service
                if (d_authService == null)
                {
                    if (d_session.OpenService("//blp/apiauth"))
                        d_authService = d_session.GetService("//blp/apiauth");
                }
                // get Identity
                Identity identity = null;
                if (d_identityList.ContainsKey(UUID))
                    identity = d_identityList[UUID];
                else
                {
                    // create user handle
                    identity = d_session.CreateIdentity();
                    d_identityList.Add(UUID, identity);
                }
                // create request object
                Request authRequest = d_authService.CreateAuthorizationRequest();
                authRequest.Set("uuid", UUID);
                authRequest.Set("ipAddress", IP);
                // create correlation id for request
                CorrelationID correlator = new CorrelationID(UUID);
                d_session.Cancel(correlator);
                toolStripStatusLabel1.Text = "Submit logged on status request";
                if (asynchronous)
                {
                    // asynchronous request
                    d_session.SendAuthorizationRequest(authRequest, identity, correlator);
                }
                else
                {
                    // synchronous request
                    EventQueue eventQueue = new EventQueue();
                    d_session.SendAuthorizationRequest(authRequest, identity, eventQueue, correlator);
                    // wait for data to return
                    Event eventObj = eventQueue.NextEvent();
                    while (true)
                    {
                        // process data
                        processEvent(eventObj, d_session);
                        // check if all data have been returned
                        if (eventObj.Type == Event.EventType.RESPONSE)
                            break;
                        eventObj = eventQueue.NextEvent();
                    }
                    toolStripStatusLabel1.Text = "Logged on status validation completed";
                }
            }
            catch (Exception ex)
            {
                // display message
                MessageBox.Show("Error: " + ex.Message, "Logon Status", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion Auth IP

        #endregion // end method region

        #region Control Events

        /// <summary>
        /// Add user and check user logon status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAddUser_Click(object sender, EventArgs e)
        {
            // open session
            if (!createSession())
            {
                toolStripStatusLabel1.Text = "Failed to start session.";
                return;
            }
            // validate user info
            if (textBoxName.Text.Length == 0)
            {
                MessageBox.Show("Please enter user name.", "User Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxName.Focus();
                return;
            }
            // validate UUID
            if (textBoxUUID.Text.Length == 0)
            {
                MessageBox.Show("Please enter user's UUID.", "User's UUID", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxUUID.Focus();
                return;
            }
            // check user in existing list
            if (d_users.Select("UUID = '" + textBoxUUID.Text.Trim() + "'").GetLength(0) > 0)
            {
                MessageBox.Show("User's UUID " + textBoxUUID.Text + " already exist. Delete existing user first.", "User UUID", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxUUID.Focus();
                return;
            }
            // check user in existing list
            if (d_users.Select("Name = '" + textBoxName.Text.Trim() + "'").GetLength(0) > 0)
            {
                MessageBox.Show("User's Name " + textBoxName.Text.Trim() + " already exist.", "User Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxName.Focus();
                return;
            }
            // check IP
            if (textBoxIP.Text.Length == 0 && textBoxIP.Visible)
            {
                MessageBox.Show("Please enter user's IP Address.", "User's IP Address", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxIP.Focus();
                return;
            }
            // trim beginning and ending spaces
            string uuid = textBoxUUID.Text.Trim();
            // add user
            d_users.Rows.Add(new object[] { textBoxName.Text.Trim(), uuid, textBoxIP.Text.Trim() });
            d_users.AcceptChanges();
            // populate grid
            DataGridViewRow row = populateUserGrid(textBoxName.Text.Trim(), uuid, textBoxIP.Text.Trim(), LogonStatus.Unknown);
            checkUserLoginStatus(uuid, textBoxIP.Text.Trim(), true);
            d_submittedRequests = 1;
            // update button state
            setControlStates();
        }

        /// <summary>
        /// Remove user from list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRemoveUser_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Delete current selected user?", "Delete User", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // delete selected users
                    foreach (DataGridViewRow row in dataGridViewUsers.SelectedRows)
                    {
                        int index = row.Index;
                        DataRow[] rows = d_users.Select("UUID = '" + dataGridViewUsers["UUID", index].Value + "'");
                        if (rows.GetLength(0) > 0)
                            rows[0].Delete();
                        d_users.AcceptChanges();
                        // remove user handle from list
                        d_identityList.Remove(dataGridViewUsers["UUID", index].Value.ToString());
                        dataGridViewUsers.Rows.RemoveAt(dataGridViewUsers.SelectedRows[0].Index);
                    }
                }
                toolStripStatusLabel1.Text = string.Empty;
            }
            else
            {
                MessageBox.Show("Please select a user to delete.", "Delete User");
            }
        }

        /// <summary>
        /// Submit user list for security entitlement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSubmitUsers_Click(object sender, EventArgs e)
        {
            // check for existing data
            if (d_submittedUserList != null)
                d_submittedUserList.Clear();
            // get new list of users
            d_submittedUserList = d_users.Copy();
            // clear old submitted userhandles
            if (d_submittedIdentityList != null)
                d_submittedIdentityList.Clear();
            // create new list
            d_submittedIdentityList = new Dictionary<string, Identity>(d_identityList);
            // add submitted users to security entitlment
            initUserSecEntitlement();
            toolStripStatusLabel1.Text = string.Empty;
            // submitted user data bind user to user list combo
            comboBoxSelectUser.DataSource = d_submittedUserList;
            comboBoxSelectUser.DisplayMember = "Name";
            comboBoxSelectUser.ValueMember = "UUID";
            comboBoxSelectUser.SelectedIndex = 0;

            // update button state
            setControlStates();
        }

        /// <summary>
        /// Validate selected user logon status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonValidateUser_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count == 1)
            {
                // get selected user
                DataGridViewRow row = dataGridViewUsers.SelectedRows[0];
                int index = row.Index;
                // remove old userhandle
                row.Cells["Logon"].Value = imageList1.Images[2];
                checkUserLoginStatus(row.Cells["UUID"].Value.ToString().Trim(), row.Cells["IP"].Value.ToString().Trim(), true);
                d_submittedRequests = 1;
            }
            else
            {
                MessageBox.Show("Please select one user to validate logon status.", "User Logon Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Save user list to xml file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSaveList_Click(object sender, EventArgs e)
        {
            SaveFileDialog file = new SaveFileDialog();
            file.Filter = "User List Files|*.xml";
            file.Title = "Save User List";
            if (file.ShowDialog() == DialogResult.OK)
            {
                DataSet data = new DataSet();
                data.Tables.Add(d_users.Copy());
                data.WriteXml(file.FileName, XmlWriteMode.WriteSchema);
                data.Tables.Clear();
                toolStripStatusLabel1.Text = "Saved user list.";
            }
        }

        /// <summary>
        /// Load users from xml file and validate IP user logon status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLoadList_Click(object sender, EventArgs e)
        {
            int requestCount = 0;
            // make sure user want to over write current users
            if (d_users.Rows.Count > 0)
            {
                if (MessageBox.Show("Over write current user list?", "Load User List", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }
            // get users from saved xml file
            DataSet newUserList = new DataSet();
            try
            {
                OpenFileDialog file = new OpenFileDialog();
                file.Filter = "User List Files|*.xml";
                file.Title = "Load User List";
                file.Multiselect = false;
                if (file.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        newUserList.ReadXml(file.FileName);
                        d_users.Clear();
                        d_identityList.Clear();
                        dataGridViewUsers.Rows.Clear();
                        // populate grid with users
                        foreach (DataRow row in newUserList.Tables[0].Rows)
                        {
                            // check user log in
                            populateUserGrid(row["Name"].ToString(), row["UUID"].ToString(), row["IP"].ToString(), LogonStatus.Unknown);
                        }
                        // submit IP logon verification for user
                        foreach (DataGridViewRow row in dataGridViewUsers.Rows)
                        {
                            // check user log in
                            checkUserLoginStatus(row.Cells["UUID"].Value.ToString(), row.Cells["IP"].Value.ToString(), true);
                            requestCount++;
                        }
                        d_submittedRequests = requestCount;
                        // remove all tables
                        d_users.Clear();
                        d_users = newUserList.Tables[0];
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading user list. " + ex.Message.ToString(), "Load User List", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    // update button state
                    setControlStates();
                }
            }
            catch
            {
                MessageBox.Show("Error loading user list", "Invalid User List");
            }
        }

        /// <summary>
        /// Clear users from list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearList_Click(object sender, EventArgs e)
        {
            // remove all user data
            d_users.Clear();
            dataGridViewUsers.Rows.Clear();
            d_identityList.Clear();
            toolStripStatusLabel1.Text = string.Empty;
            // update button state
            setControlStates();
        }

        /// <summary>
        /// Remove selected securities
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRemoveSec_Click(object sender, EventArgs e)
        {
            int numberSelectedRows = dataGridViewSecEntitlement.SelectedRows.Count;
            if (numberSelectedRows > 0)
            {
                for (int index = 0; index < numberSelectedRows; index++)
                {
                    ((DataRowView)dataGridViewSecEntitlement.SelectedRows[index].DataBoundItem).Delete();
                }
                toolStripStatusLabel1.Text = string.Empty;
                setControlStates();
            }
            else
            {
                MessageBox.Show("Please select a security to remove.", "Remove Security");
            }
        }

        /// <summary>
        /// Request securities EIDs and validate user entitlement.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonUserEntitlementValidation_Click(object sender, EventArgs e)
        {
            if (d_submittedUserList.Rows.Count > 0)
            {
                // submit refdata request
                submitRequest();
            }
            else
            {
                MessageBox.Show("Please enter a security.", "Security Entitlement", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxSecurity.Focus();
                return;
            }
        }

        /// <summary>
        /// Add security to security list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAddSecurity_Click(object sender, EventArgs e)
        {
            if (textBoxSecurity.Text.Length == 0)
            {
                MessageBox.Show("Missing security", "Add Security", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxSecurity.Focus();
            }
            else
            {
                addSecurity(textBoxSecurity.Text.Trim());
                textBoxSecurity.Text = string.Empty;
            }
            toolStripStatusLabel1.Text = string.Empty;
        }

        /// <summary>
        /// Add user mode security to security to grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAddUserSecurity_Click(object sender, EventArgs e)
        {
            if (textBoxUserSecurity.Text.Length == 0)
            {
                MessageBox.Show("Missing security", "Add Security", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxUserSecurity.Focus();
            }
            else
            {
                addUserModeSecurity(textBoxUserSecurity.Text.Trim());
                textBoxUserSecurity.Text = string.Empty;
            }
            toolStripStatusLabel1.Text = string.Empty;
        }

        /// <summary>
        /// Add security to list when return key is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxSecurity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && buttonAddSecurity.Enabled)
            {
                if (textBoxSecurity.Text.Length == 0)
                {
                    MessageBox.Show("Missing security", "Add Security", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBoxSecurity.Focus();
                }
                else
                {
                    addSecurity(textBoxSecurity.Text.Trim());
                    textBoxSecurity.Text = string.Empty;
                }
                toolStripStatusLabel1.Text = string.Empty;
            }
        }

        /// <summary>
        /// Add security to grid when return key is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxUserSecurity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && buttonAddSecurity.Enabled)
            {
                if (textBoxUserSecurity.Text.Length == 0)
                {
                    MessageBox.Show("Missing security", "Add Security", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBoxUserSecurity.Focus();
                }
                else
                {
                    addUserModeSecurity(textBoxUserSecurity.Text.Trim());
                    textBoxUserSecurity.Text = string.Empty;
                }
                toolStripStatusLabel1.Text = string.Empty;
            }
        }

        /// <summary>
        /// Only allow numeric keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxUUID_KeyDown(object sender, KeyEventArgs e)
        {
            // allow only numeric, backspace, left and right keys
            if (!((e.KeyValue >= 48 && e.KeyValue <= 57) ||
                e.KeyData == Keys.Back || e.KeyData == Keys.Left || e.KeyData == Keys.Right))
            {
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Clear all securities from security list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            d_securityEntitlements.Rows.Clear();
            toolStripStatusLabel1.Text = string.Empty;
            setControlStates();
        }

        /// <summary>
        /// Stop all active subscriptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonUserStopSubscription_Click(object sender, EventArgs e)
        {
            StopSubscriptions();
        }

        /// <summary>
        /// Subscribe using user credential
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonUserSubscribe_Click(object sender, EventArgs e)
        {
            // get user UUID
            string userUUID = comboBoxSelectUser.SelectedValue.ToString();
            // check if user exist in submitted list
            if (d_submittedIdentityList.ContainsKey(userUUID))
            {
                // get user handle
                Identity identity = d_submittedIdentityList[userUUID];
                // get mktdata service
                if (d_mktdataService == null)
                    if (d_session.OpenService("//blp/mktdata"))
                        d_mktdataService = d_session.GetService("//blp/mktdata");
                // check to see if user is authorized
                if (identity.IsAuthorized(d_mktdataService))
                {
                    // subscribe on as user
                    subscriptionRequest(identity);
                }
                else
                {
                    MessageBox.Show("User is not authorized to data.", "User Permission", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Remove selected user mode securities
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRemoveUserSecurity_Click(object sender, EventArgs e)
        {
            int numberSelectedRows = dataGridViewUserData.SelectedCells.Count;
            if (numberSelectedRows > 0)
            {
                for (int index = 0; index < numberSelectedRows; index++)
                {
                    int rowIndex = dataGridViewUserData.SelectedCells[index].RowIndex;
                    dataGridViewUserData.Rows.RemoveAt(rowIndex);
                }
                toolStripStatusLabel1.Text = string.Empty;
                setControlStates();
            }
            else
            {
                MessageBox.Show("Please select a security to remove.", "Remove Security");
            }
        }

        /// <summary>
        /// Clear all user mode securities and data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearAllUserData_Click(object sender, EventArgs e)
        {
            dataGridViewUserData.Rows.Clear();
            toolStripStatusLabel1.Text = string.Empty;
            setControlStates();
        }

        /// <summary>
        /// Display security EIDs on grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxSecEIDs_CheckedChanged(object sender, EventArgs e)
        {
            dataGridViewSecEntitlement.Columns["EIDs Info"].Visible = checkBoxSecEIDs.Checked;
        }

        /// <summary>
        /// Auto fit columns to grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxAutoFitColumn_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxAutoFitColumn.Checked)
                dataGridViewSecEntitlement.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            else
                dataGridViewSecEntitlement.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        }

        /// <summary>
        /// This handler will be called as the cursor enters our DataGrid's window.
        /// It gives us the opportunit to indicate the format of the data 
        /// we can handle. In this case we want to consume text data only.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewSecEntitlement_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (buttonAddSecurity.Enabled)
            {
                if (e.Data.GetDataPresent(DataFormats.Text))
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// This handler will be called when a Data object is dropped on our DataGrid's window.
        /// At this point we will have to parse the CR/LF delimited text that is dropped on us.
        /// In a real application we would probably want to do more validation of the input...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewSecEntitlement_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            // Get the entire text object that has been dropped on us.
            string tmp = e.Data.GetData(DataFormats.Text).ToString();

            // Tokenize the string into what (we hope) are Security strings
            char[] sep = { '\r', '\n', '\t' };
            string[] words = tmp.Split(sep);

            foreach (string sec in words)
            {
                if (sec.Trim().Length > 0)
                    addSecurity(sec.Trim());
            }
        }

        /// <summary>
        /// This handler will be called as the cursor enters our DataGrid's window.
        /// It gives us the opportunit to indicate the format of the data 
        /// we can handle. In this case we want to consume text data only.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewUserData_DragEnter(object sender, DragEventArgs e)
        {
            if (buttonAddUserSecurity.Enabled)
            {
                if (e.Data.GetDataPresent(DataFormats.Text))
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// This handler will be called when a Data object is dropped on our DataGrid's window.
        /// At this point we will have to parse the CR/LF delimited text that is dropped on us.
        /// In a real application we would probably want to do more validation of the input...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewUserData_DragDrop(object sender, DragEventArgs e)
        {
            // Get the entire text object that has been dropped on us.
            string tmp = e.Data.GetData(DataFormats.Text).ToString();
            // Tokenize the string into what (we hope) are Security strings
            char[] sep = { '\r', '\n', '\t' };
            string[] words = tmp.Split(sep);
            foreach (string sec in words)
            {
                if (sec.Trim().Length > 0)
                    addUserModeSecurity(sec.Trim());
            }
        }
        #endregion

        #region Bloomberg API Events and Data Processing
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
                    // process event type
                    switch (eventObj.Type)
                    {
                        case Event.EventType.AUTHORIZATION_STATUS:
                        case Event.EventType.PARTIAL_RESPONSE:
                        case Event.EventType.RESPONSE:
                            processRequestDataEvent(eventObj, session);
                            // only count response event type
                            if (eventObj.Type == Event.EventType.RESPONSE)
                                d_submittedRequests--;
                            // update process status
                            if (d_submittedRequests == 0)
                                toolStripStatusLabel1.Text = "Validation completed.";
                            else
                                toolStripStatusLabel1.Text = "Processing...";
                            break;
                        case Event.EventType.SUBSCRIPTION_DATA:
                            processSubscriptionDataEvent(eventObj, session);
                            break;
                        case Event.EventType.SUBSCRIPTION_STATUS:
                            processSubscriptionStatusEvent(eventObj, session);
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
        /// Process security data
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processRequestDataEvent(Event eventObj, Session session)
        {
            DataGridViewRow row = null;
            try
            {
                foreach (Message msg in eventObj)
                {
                    string uuid = string.Empty;
                    if (msg.CorrelationID.IsObject)
                    {
                        // get correlation id
                        uuid = msg.CorrelationID.Object.ToString();
                        // get grid row for uuid
                        foreach (DataGridViewRow user in dataGridViewUsers.Rows)
                        {
                            if (user.Cells["UUID"].Value.ToString().Trim() == uuid)
                            {
                                // found user
                                row = user;
                                break;
                            }
                        }
                    }
                    switch (d_messageTypeTable[msg.MessageType])
                    {
                        case MessageType.ENTITLEMENT_CHANGED:
                            // user entitlement changed
                            String name = row.Cells[0].Value.ToString();
                            MessageBox.Show("Entitlement has been changed for " + name + ".",
                                "Entitlement Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case MessageType.AUTHORIZATION_SUCCESS:
                            // success display user logged on
                            row.Cells["Logon"].Value = imageList1.Images[0];
                            toolStripStatusLabel1.Text = "User is logged on to the terminal.";
                            break;
                        case MessageType.AUTHORIZATION_REVOKED:
                            // Revoked display user logged off
                            row.Cells["Logon"].Value = imageList1.Images[1];
                            break;
                        case MessageType.AUTHORIZATION_FAILURE:
                            // Revoked display user logged off
                            row.Cells["Logon"].Value = imageList1.Images[1];
                            toolStripStatusLabel1.Text = msg.GetElement("reason").GetElement("message")[0].ToString();
                            break;
                        case MessageType.REFERENCE_DATA_RESPONSE:
                            // check user security entitlement
                            d_securityEntitlements.BeginLoadData();
                            foreach (Element secData in msg.Elements)
                            {
                                int numberData = secData.NumValues;
                                for (int dataIndex = 0; dataIndex < numberData; dataIndex++)
                                {
                                    Element data = (Element)secData[dataIndex];
                                    string security = data.GetElementAsString("security");
                                    toolStripStatusLabel1.Text = "Processing security: " + security;
                                    if (data.HasElement(SECURITY_ERROR))
                                    {
                                        Element error = data.GetElement(SECURITY_ERROR);
                                        String errorMessage = error.GetElementAsString(MESSAGE);
                                        d_securityEntitlements.Rows[dataIndex]["Eids Info"] = errorMessage;
                                    }
                                    else
                                        checkUserEntitlement(security, data.GetElement("eidData"), msg.Service);
                                }
                                Application.DoEvents();
                            }
                            d_securityEntitlements.EndLoadData();
                            break;
                        case MessageType.RESPONSE_ERROR:
                            // Revoked display user logged off
                            row.Cells["Logon"].Value = imageList1.Images[1];
                            toolStripStatusLabel1.Text = msg.GetElement("message")[0].ToString();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                // display error in status bar
                toolStripStatusLabel1.Text = e.Message;
            }
        }

        /// <summary>
        /// Process subscription data
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processSubscriptionDataEvent(Event eventObj, Session session)
        {
            foreach (Message msg in eventObj)
            {
                // get data grid row
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
                                cell.Style.BackColor = Color.Yellow;
                            }
                        }
                    }
                    // process tick data
                    for (int fieldIndex = 1; fieldIndex < dataGridViewUserData.ColumnCount; fieldIndex++)
                    {
                        string field = dataGridViewUserData.Columns[fieldIndex].Name;
                        if (msg.HasElement(field))
                        {
                            // check element to see if it has null value
                            if (!msg.GetElement(field).IsNull)
                            {
                                dataRow.Cells[field].Value = msg.GetElementAsString(field);
                            }
                        }
                    }
                    // allow application to update UI
                    Application.DoEvents();
                }
            }
        }


        /// <summary>
        /// Subscription status event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processSubscriptionStatusEvent(Event eventObj, Session session)
        {
            List<string> dataList = new List<string>();

            foreach (Message msg in eventObj)
            {
                // get data grid row
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
                        if (msg.HasElement("exceptions"))
                        {
                            // subscription has error
                            Element error = msg.GetElement("exceptions");
                            int searchIndex = 0;
                            for (int errorIndex = 0; errorIndex < error.NumValues; errorIndex++)
                            {
                                Element errorException = error.GetValueAsElement(errorIndex);
                                string field = errorException.GetElementAsString(FIELD_ID);
                                Element reason = errorException.GetElement(REASON);
                                string message = reason.GetElementAsString(DESCRIPTION);
                                for (; searchIndex < dataGridViewUserData.ColumnCount - 1; searchIndex++)
                                {
                                    if (field == dataGridViewUserData.Columns[searchIndex].Name)
                                    {
                                        dataRow.Cells[searchIndex].Value = message;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        toolStripStatusLabel1.Text = e.Message;
                    }
                }
                else
                {
                    // check for subscription failure
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
                        toolStripStatusLabel1.Text = "Session started";
                        break;
                    case "SessionTerminated":
                    case "SessionStopped":
                        // "Session Terminated"
                        toolStripStatusLabel1.Text = "Session stopped";
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