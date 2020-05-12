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
/// - Make asynchronous and synchronous reference data
///   request using //blp/refdata service.
/// - Set request override fields.
/// - Retrieve bulk data.
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
        private DataSet d_bulkData;
        private DataTable d_data;
        private int d_numberOfReturnedSecurities = 0;

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
            // add columns to data table
            if (d_data == null)
                d_data = new DataTable();
            d_data.Columns.Add("security");
            d_data.AcceptChanges();
            // set grid data source
            dataGridViewData.DataSource = d_data;
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
                    // add fields
                    d_data.Rows.Add(security.Trim());
                }
            }
            setControlStates();
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
                // create asynchronous session
                d_session = new Session(d_sessionOptions);
            }
            return d_session.Start();
        }

        private void setControlStates()
        {
            buttonSendRequest.Enabled = d_data.Rows.Count > 0 && d_data.Columns.Count > 1;
            buttonClearFields.Enabled = d_data.Columns.Count > 1;
            buttonClearData.Enabled = buttonSendRequest.Enabled;
            buttonClearAll.Enabled = d_data.Rows.Count > 0 || d_data.Columns.Count > 1;
        }

        /// <summary>
        /// Clear security data
        /// </summary>
        private void clearData()
        {
            // clear security count
            d_numberOfReturnedSecurities = 0;

            // clear bulk data
            if (d_bulkData != null)
            {
                d_bulkData.Clear();
                d_bulkData.AcceptChanges();
            }

            if (d_data != null)
            {
                foreach (DataRow row in d_data.Rows)
                {
                    for (int index = 1; index < d_data.Columns.Count; index++)
                    {
                        row[index] = DBNull.Value;
                    }
                }
                d_data.AcceptChanges();
            }
            toolStripStatusLabel1.Text = string.Empty;
        }

        /// <summary>
        /// Remove all fields from grid
        /// </summary>
        private void clearFields()
        {
            for (int index = d_data.Columns.Count - 1; index > 0; index--)
            {
                d_data.Columns.RemoveAt(index);
            }
            d_data.AcceptChanges();
            toolStripStatusLabel1.Text = string.Empty;
        }

        /// <summary>
        /// Remove all securities and fields from grid
        /// </summary>
        private void clearAll()
        {
            if (d_bulkData != null)
            {
                d_bulkData.Clear();
                d_bulkData.AcceptChanges();
            }
            clearFields();
            d_data.Rows.Clear();
            d_data.AcceptChanges();
            listViewOverrides.Items.Clear();
            setControlStates();
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
        /// Enter key pressed to add override field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxOverride_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                buttonAddOverride_Click(this, new EventArgs());
            }
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
            foreach (DataRow row in d_data.Rows)
            {
                for (int index = 1; index < d_data.Columns.Count; index++)
                {
                    row[index] = DBNull.Value;
                }
            }
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
                    // only accept field with value
                    if (overrideItem.Trim().Length > 0 && overrideItem.Contains("="))
                    {
                        string[] ovr = overrideItem.Split(new char[] { '=' });
                        ListViewItem item = listViewOverrides.Items.Add(ovr[0].Trim());
                        item.SubItems.Add(ovr[1].Trim());
                    }
                }
                textBoxOverride.Text = string.Empty;
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
                        d_data.Rows.Add(new object[] {sec.Trim()});
                    }
                    else
                    {
                        // add fields
                        d_data.Columns.Add(sec.Trim());
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
        /// Display bulk data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewData_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                string security = dataGridViewData.Rows[e.RowIndex].Cells["security"].Value.ToString();
                string field = dataGridViewData.Columns[e.ColumnIndex].Name.ToString();
                string cellData = dataGridViewData.Rows[e.RowIndex].Cells[field].Value.ToString();
                if (cellData != "Bulk Data...")
                {
                    return;
                }
                // create bulk data table for display
                DataTable bulkTable = d_bulkData.Tables[field].Clone(); 
                bulkTable.TableName = "BulkData";
                // Get bulk data
                DataRow[] rows = d_bulkData.Tables[field].Select("security = '" + security + "'");
                foreach (DataRow row in rows)
                    bulkTable.ImportRow(row);
                // Display data
                FormBulkData bulkData = new FormBulkData(bulkTable);
                bulkData.ShowDialog(this);
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = ex.Message.ToString();
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
                if (columnIndex > 0)
                {
                    // remove field
                    d_data.Columns.RemoveAt(columnIndex);
                }
                else
                {
                    // remove security
                    d_data.Rows.RemoveAt(rowIndex);
                }
                // accept changes
                d_data.AcceptChanges();
                if (dataGrid.Columns.Count > columnIndex && columnIndex > 0)
                    dataGrid.Rows[rowIndex].Cells[columnIndex].Selected = true;
                else
                    if (dataGrid.Columns.Count > 1 && dataGrid.Columns.Count == columnIndex)
                        dataGrid.Rows[rowIndex].Cells[columnIndex - 1].Selected = true;
            }
        }

        /// <summary>
        /// Send reference data request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            clearData();
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
            // create reference data request
            Request request = refDataService.CreateRequest("ReferenceDataRequest");
            // set request parameters
            Element securities = request.GetElement("securities");
            Element fields = request.GetElement("fields");
            Element requestOverrides = request.GetElement("overrides");
            request.Set("returnEids", true);
            // populate security
            foreach (DataRow secRow in d_data.Rows)
            {
                securities.AppendValue(secRow["security"].ToString());
            }
            // populate fields
            for (int fieldIndex = 1; fieldIndex < d_data.Columns.Count; fieldIndex++)
                fields.AppendValue(d_data.Columns[fieldIndex].ColumnName);
            if (listViewOverrides.Items.Count > 0)
            {
                // populate overrides
                foreach (ListViewItem item in listViewOverrides.Items)
                {
                    Element ovr = requestOverrides.AppendElement();
                    ovr.SetElement(FIELD_ID, item.Text);
                    ovr.SetElement("value", item.SubItems[1].Text);
                }
            }
            // create correlation id            
            CorrelationID cID = new CorrelationID(1);
            d_session.Cancel(cID);
            // send request
            d_session.SendRequest(request, cID);
            toolStripStatusLabel1.Text = "Submitted request. Waiting for response...";
            if (radioButtonSynch.Checked)
            {
                // Allow UI to update
                Application.DoEvents();
                // Synchronous mode. Wait for reply before proceeding.
                while (true)
                {
                    Event eventObj = d_session.NextEvent();
                    toolStripStatusLabel1.Text = "Processing data...";
                    // process data
                    processEvent(eventObj, d_session);
                    if (eventObj.Type == Event.EventType.RESPONSE)
                    {
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
                            setControlStates();
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
            if (d_numberOfReturnedSecurities == 0)
                toolStripStatusLabel1.Text = "Processing data...";
            d_data.BeginLoadData();
            // process message
            foreach (Message msg in eventObj)
            {
                // get message correlation id
                int cId = (int)msg.CorrelationID.Value;
                if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("ReferenceDataResponse")))
                {
                    // process security data
                    Element secDataArray = msg.GetElement(SECURITY_DATA);
                    int numberOfSecurities = secDataArray.NumValues;
                    for (int index = 0; index < numberOfSecurities; index++)
                    {
                        Element secData = secDataArray.GetValueAsElement(index);
                        Element fieldData = secData.GetElement("fieldData");
                        d_numberOfReturnedSecurities++;
                        // get security index
                        int rowIndex = secData.GetElementAsInt32("sequenceNumber");
                        if (d_data.Rows.Count > rowIndex)
                        {
                            // get security record
                            DataRow row = d_data.Rows[rowIndex];
                            // check for field error
                            if (secData.HasElement(FIELD_EXCEPTIONS))
                            {
                                // process error
                                Element error = secData.GetElement(FIELD_EXCEPTIONS);
                                for (int errorIndex = 0; errorIndex < error.NumValues; errorIndex++)
                                {
                                    Element errorException = error.GetValueAsElement(errorIndex);
                                    string field = errorException.GetElementAsString(FIELD_ID);
                                    Element errorInfo = errorException.GetElement(ERROR_INFO);
                                    string message = errorInfo.GetElementAsString(MESSAGE);
                                    row[field] = message;
                                }
                            }
                            // check for security error
                            if (secData.HasElement(SECURITY_ERROR))
                            {
                                Element error = secData.GetElement(SECURITY_ERROR);
                                string errorMessage = error.GetElementAsString(MESSAGE);
                                row[1] = errorMessage;
                            }
                            // process data
                            foreach (DataColumn col in d_data.Columns)
                            {
                                String dataValue = string.Empty;
                                if (fieldData.HasElement(col.ColumnName))
                                {
                                    Element item = fieldData.GetElement(col.ColumnName);
                                    if (item.IsArray)
                                    {
                                        // bulk field
                                        dataValue = "Bulk Data...";
                                        processBulkData(secData.GetElementAsString("security"), item);
                                    }
                                    else
                                    {
                                        dataValue = item.GetValueAsString();
                                    }
                                    row[col.ColumnName] = dataValue;
                                }
                            }
                        }
                    }
                }
            }
            d_data.EndLoadData();
            // check if we are done
            if (d_numberOfReturnedSecurities >= d_data.Rows.Count)
                toolStripStatusLabel1.Text = "Completed";
        }

        /// <summary>
        /// Process bulk data
        /// </summary>
        /// <param name="security"></param>
        /// <param name="data"></param>
        private void processBulkData(string security, Element data)
        {
            DataTable bulkTable = null;
            // bulk data dataset
            if (d_bulkData == null)
                d_bulkData = new DataSet();
            // get bulk data
            Element bulk = data.GetValueAsElement(0);
            if (d_bulkData.Tables.Contains(bulk.Name.ToString()))
            {
                // get existing bulk data table
                bulkTable = d_bulkData.Tables[bulk.Name.ToString()];
            }
            else
            {
                // create new bulk data table
                bulkTable = d_bulkData.Tables.Add(bulk.Name.ToString());
            }
            // create column if not already exist
            if (!bulkTable.Columns.Contains("security"))
            {
                bulkTable.Columns.Add(new DataColumn("security", typeof(String)));
                bulkTable.Columns.Add(new DataColumn("Id", typeof(int)));
            }
            // create columns in data table for bulk data
            foreach (Element item in bulk.Elements)
            {
                if (!bulkTable.Columns.Contains(item.Name.ToString()))
                    bulkTable.Columns.Add(new DataColumn(item.Name.ToString(), typeof(String)));
            }
            // populate bulk
            int count = 0;
            for (int index = 0; index < data.NumValues; index++)
            {
                bulk = data.GetValueAsElement(index);
                object[] dataArray = new object[bulk.NumElements + 2];
                dataArray[0] = security;
                dataArray[1] = count;
                int dataIndex = 2;
                foreach (Element item in bulk.Elements)
                {
                    dataArray[dataIndex] = item.GetValueAsString();
                    dataIndex++;
                }
                bulkTable.Rows.Add(dataArray);
                count++;
            }
        }

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