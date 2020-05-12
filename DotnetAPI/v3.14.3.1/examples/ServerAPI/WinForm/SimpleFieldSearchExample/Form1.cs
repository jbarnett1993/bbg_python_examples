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
///   using //blp/apiflds service.
/// ==========================================================
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Event = Bloomberglp.Blpapi.Event;
using Element = Bloomberglp.Blpapi.Element;
using InvalidRequestException = Bloomberglp.Blpapi.InvalidRequestException;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;

namespace Bloomberglp.Blpapi.Examples
{
    public partial class Form1 : Form
    {
        private static readonly Name REASON = new Name("reason");
        private static readonly Name CATEGORY = new Name("category");
        private static readonly Name DESCRIPTION = new Name("description");
        private static readonly Name ERROR_CODE = new Name("errorCode");
        private static readonly Name SOURCE = new Name("source");

        private SessionOptions d_sessionOptions;
        private Session d_session;
        private Service d_service;
        private DataSet d_data;
        private DataTable d_overrideDataTemp;
        private List<string> d_fieldIds = null;

        #region properties
        private DataTable overrideFieldsTempTable
        {
            get { return d_overrideDataTemp; }
            set { d_overrideDataTemp = value; }
        }

        private DataTable fieldTable
        {
            get { return d_data.Tables["fieldData"]; }
        }

        private DataTable propertyTable
        {
            get { return d_data.Tables["fieldPropertyData"]; }
        }

        private DataTable overrideTable
        {
            get { return d_data.Tables["fieldOverrideData"]; }
        }
        #endregion

        public Form1()
        {
            InitializeComponent();
            string serverHost = "localhost";
            int serverPort = 8194;

            // set sesson options
            d_sessionOptions = new SessionOptions();
            d_sessionOptions.ServerHost = serverHost;
            d_sessionOptions.ServerPort = serverPort;
            // init application
            initUI();
        }

        #region methods
        /// <summary>
        /// Initialize form controls
        /// </summary>
        private void initUI()
        {
            // init control
            comboBoxSearchType.SelectedIndex = 0;
            comboBoxIncludeFieldType.SelectedIndex = 0;
            comboBoxIncludeProductType.SelectedIndex = 0;
            comboBoxExcludeFieldType.SelectedIndex = 0;
            comboBoxExcludeProductType.SelectedIndex = 0;

            // add columns to grid
            if (d_data == null)
            {
                initDataTable();
            }
            dataGridViewDataView.DataSource = d_data;
            dataGridViewDataView.DataMember = "fieldData";
            dataGridViewDataView.Columns[2].Width = 300;
            dataGridViewDataView.Columns[3].Width = 500;

            // start connection
            if (createSession())
            {
                toolStripStatusLabel1.Text = "Session started";
            }
            else
            {
                buttonSubmitSearch.Enabled = false;
                toolStripStatusLabel1.Text = "Session failed to start. Please close example " +
                    "and try again.";
            }
        }

        /// <summary>
        /// Create data tables
        /// </summary>
        public void initDataTable()
        {
            if (d_data == null)
            {
                d_data = new DataSet();
            }

            d_data.Tables.Add("fieldData");
            DataColumn col = fieldTable.Columns.Add("Id");
            col = fieldTable.Columns.Add("mnemonic");
            col = fieldTable.Columns.Add("description");
            col = fieldTable.Columns.Add("categoryName");

            d_data.Tables.Add("fieldPropertyData");
            col = propertyTable.Columns.Add("id");
            col = propertyTable.Columns.Add("documentation");
            col.MaxLength = 10000;

            d_data.Tables.Add("fieldOverrideData");
            col = overrideTable.Columns.Add("parentId");
            col = overrideTable.Columns.Add("id");
            col = overrideTable.Columns.Add("mnemonic");
            col = overrideTable.Columns.Add("description");

            d_data.AcceptChanges();
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
            // create asynchronous session
            d_session = new Session(d_sessionOptions, new EventHandler(processEvent));
            return d_session.Start();
        }

        /// <summary>
        /// Clear security data
        /// </summary>
        private void clearData()
        {
            fieldTable.Clear();
            propertyTable.Clear();
            if (overrideFieldsTempTable != null)
            {
                overrideFieldsTempTable.Clear();
            }
            overrideTable.Clear();
            d_data.AcceptChanges();
        }

        /// <summary>
        /// Build and submit field search request
        /// </summary>
        /// <param name="searchSpec"></param>
        /// <param name="searchType"></param>
        /// <param name="includeOptions"></param>
        /// <param name="excludeOptions"></param>
        private void fieldSearch(string searchSpec, int searchType, FieldSearchOptions includeOptions, 
            FieldSearchOptions excludeOptions)
        {
            Request request = null;
            CorrelationID cId;

            // get auth service
            if (d_service == null)
            {
                if (d_session.OpenService(@"//blp/apiflds"))
                {
                    d_service = d_session.GetService(@"//blp/apiflds");
                }
            }

            // clear dataset
            clearData();

            if (searchType == 0)
            {
                // set field search correlationID to 1000
                cId = new CorrelationID(1000);
                request = d_service.CreateRequest("FieldSearchRequest");

                if (includeOptions != null)
                {
                    // include options
                    Element include = request.GetElement("include");
                    if (includeOptions.ProductType != "None")
                    {
                        include.SetElement("productType", includeOptions.ProductType);
                    }
                    if (includeOptions.FieldType != "None")
                    {
                        include.SetElement("fieldType", includeOptions.FieldType);
                    }
                }
            }
            else
            {
                // set category field search correlationID to 2000
                cId = new CorrelationID(2000);
                request = d_service.CreateRequest("CategorizedFieldSearchRequest");
            }
            // set search string
            request.Set("searchSpec", searchSpec);
            // return field documentation
            request.Set("returnFieldDocumentation", true);

            if (excludeOptions != null)
            {
                // exclude options
                Element exclude = request.GetElement("exclude");
                if (excludeOptions.ProductType != "None")
                {
                    exclude.SetElement("productType", excludeOptions.ProductType);
                }
                if (excludeOptions.FieldType != "None")
                {
                    exclude.SetElement("fieldType", excludeOptions.FieldType);
                }
            }

            // cancel previous pending request
            d_session.Cancel(cId);
            d_session.SendRequest(request, cId);
            toolStripStatusLabel1.Text = "Request sent.";
        }

        /// <summary>
        /// Get override field informations
        /// </summary>
        /// <param name="ids"></param>
        private void getFieldInformation(List<string> ids)
        {
            // get auth service
            if (d_service == null)
            {
                if (d_session.OpenService(@"//blp/apiflds"))
                {
                    d_service = d_session.GetService(@"//blp/apiflds");
                }
            }

            Request request = d_service.CreateRequest("FieldInfoRequest");
            request.Set("returnFieldDocumentation", true);

            foreach (string id in ids)
            {
                request.Append("id", id);
            }

            // set field info request correlationID to 3000
            CorrelationID cId = new CorrelationID(3000);
            // cancel previous pending request
            d_session.Cancel(cId); 
            d_session.SendRequest(request, cId);
        }

        /// <summary>
        /// Get field documentation information
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private String getFieldDocumentation(string id)
        {
            string doc = string.Empty;
            // get field documentation
            DataRow[] rows = propertyTable.Select("id = '" + id + "'");
            if (rows.Length > 0)
            {
                doc = rows[0]["documentation"].ToString();
            }
            return doc;
        }

        /// <summary>
        /// Get field overrides for field id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private DataTable getFieldOverrides(string id)
        {
            if (d_fieldIds == null)
            {
                d_fieldIds = new List<string>();
            }
            else
            {
                d_fieldIds.Clear();
            }

            if (overrideFieldsTempTable == null)
            {
                // create table
                overrideFieldsTempTable = fieldTable.Clone();
                overrideFieldsTempTable.Columns.Remove("categoryName");
            }
            else
            {
                // clear talbe
                overrideFieldsTempTable.Clear();
                overrideFieldsTempTable.AcceptChanges();
            }

            // get override fields
            DataRow[] rows = overrideTable.Select("parentId = '" + id + "'");
            foreach (DataRow row in rows)
            {
                string fieldId = row["id"].ToString();
                if (row["mnemonic"].ToString().Length > 0)
                {
                    // populate override field information
                    overrideFieldsTempTable.Rows.Add(new object[] { fieldId, row["mnemonic"].ToString(), row["description"].ToString() });
                }
                else
                {
                    // override field does not have information
                    d_fieldIds.Add(fieldId);
                    overrideFieldsTempTable.Rows.Add(new object[] { fieldId, DBNull.Value, DBNull.Value });
                }
            }

            // get override field informations
            if (d_fieldIds.Count > 0)
            {
                getFieldInformation(d_fieldIds);
            }

            overrideFieldsTempTable.AcceptChanges();
            return overrideFieldsTempTable;
        }
        #endregion end methods

        #region Control Events
        /// <summary>
        /// Field search event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSubmitSearch_Click(object sender, EventArgs e)
        {
            //List<string> category = null;
            FieldSearchOptions include = null; 
            FieldSearchOptions exclude = null; 

            if (comboBoxSearchType.SelectedIndex == 0)
            {
                // field search
                if (comboBoxIncludeProductType.SelectedIndex > 0 ||
                    comboBoxIncludeFieldType.SelectedIndex > 0)
                {
                    // field search, add include filters
                    include = new FieldSearchOptions();
                    include.ProductType = comboBoxIncludeProductType.Text;
                    include.FieldType = comboBoxIncludeFieldType.Text;
                }
            }

            if (comboBoxExcludeProductType.SelectedIndex > 0 ||
                comboBoxExcludeFieldType.SelectedIndex > 0)
            {
                // add exclude filters
                exclude = new FieldSearchOptions();
                exclude.ProductType = comboBoxExcludeProductType.Text;
                exclude.FieldType = comboBoxExcludeFieldType.Text;
            }
            // build field search request
            fieldSearch(textBoxSearchSpec.Text.Trim(), comboBoxSearchType.SelectedIndex, include, exclude);
            // clear data tables
            fieldTable.Rows.Clear();
            overrideTable.Rows.Clear();
            propertyTable.Rows.Clear();
            d_data.AcceptChanges();
            richTextBoxDocumentation.Text = string.Empty;
        }

        /// <summary>
        /// Field select change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewDataView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            // get field documentation
            string id = dataGridViewDataView.Rows[e.RowIndex].Cells["id"].Value.ToString();
            //Application.DoEvents();
            if (richTextBoxDocumentation.Tag.ToString() != id)
            {
                richTextBoxDocumentation.Tag = id;
                richTextBoxDocumentation.Text = getFieldDocumentation(id);
                dataGridViewOverrides.DataSource = getFieldOverrides(id);
                dataGridViewOverrides.Columns["description"].Width = 350;
            }
        }

        /// <summary>
        /// Field search type change 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxSearchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSearchType.SelectedIndex == 1)
            {
                // disable include option for category search
                groupBoxIncludOption.Enabled = false;
            }
            else
            {
                // enable include option for field search
                groupBoxIncludOption.Enabled = true;
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
                            processResponse(eventObj, session);
                            //setControlStates();
                            toolStripStatusLabel1.Text = "Completed";
                            break;
                        case Event.EventType.PARTIAL_RESPONSE:
                            // process partial data
                            processResponse(eventObj, session);
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
        /// Process API response event
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processResponse(Event eventObj, Session session)
        {
            foreach (Message msg in eventObj)
            {
                int cId = (int)msg.CorrelationID.Value;
                switch (cId)
                {
                    case 1000:
                        // process field search response
                        processFieldData(msg);
                        break;
                    case 2000:
                        // process categorized field search response
                        processCategorizedFieldData(msg);
                        break;
                    case 3000:
                        // process field info response
                        processOverrides(msg);
                        break;
                }
            }
        }

        /// <summary>
        /// Process field search response
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processFieldData(Bloomberglp.Blpapi.Message msg)
        {
            string message = string.Empty;
            string[] elementList = new string[] {"mnemonic", "description", 
                "categoryName", "documentation", "overrides"};
            object[] fieldDataValues = null;

            // process message
            toolStripStatusLabel1.Text = "Processing data...";

            if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("fieldResponse")))
            {
                if (msg.HasElement("fieldSearchError"))
                {
                    // process field search error
                    Element reason = msg.GetElement("fieldSearchError");
                    message = string.Concat("Error: Source-", reason.GetElementAsString("source"),
                        ", Code-", reason.GetElementAsString("code"), ", category-", 
                        reason.GetElementAsString("category"),
                        ", desc-", reason.GetElementAsString("message"));
                }
                else
                {
                    // process field data
                    Element fieldDataArray = msg.GetElement("fieldData");
                    int numberOfFields = fieldDataArray.NumValues;
                    // start table update
                    fieldTable.BeginLoadData();
                    propertyTable.BeginLoadData();
                    overrideTable.BeginLoadData();
                    for (int index = 0; index < numberOfFields; index++)
                    {
                        // get field element
                        fieldDataValues = new object[4];
                        Element fieldElement = fieldDataArray.GetValueAsElement(index);
                        Element fieldData = fieldElement.GetElement("fieldInfo");
                        // get field id
                        string fieldId = fieldElement.GetElementAsString("id");
                        fieldDataValues[0] = fieldId;
                        try
                        {
                            String dataValue = string.Empty;
                            int dataIndex = 1;
                            foreach (string item in elementList)
                            {
                                // get field property
                                Element dataElement = fieldData.GetElement(item);
                                if (dataElement.IsArray)
                                {
                                    // process array data
                                    switch (item)
                                    {
                                        case "categoryName":
                                            fieldDataValues[dataIndex] = dataElement.GetValueAsString().Trim();
                                            break;
                                        case "overrides":
                                            for (int overrideIndex = 0; overrideIndex < dataElement.NumValues; overrideIndex++)
                                            {
                                                overrideTable.Rows.Add(new object[] { fieldId, dataElement[overrideIndex].ToString(),
                                                    DBNull.Value, DBNull.Value });
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    // process element data
                                    switch (item)
                                    {
                                        case "documentation":
                                            // add documentation row
                                            propertyTable.Rows.Add(new object[] { fieldId, dataElement.GetValue() });
                                            break;
                                        default:
                                            fieldDataValues[dataIndex] = dataElement.GetValueAsString();
                                            break;
                                    }
                                }
                                dataIndex++;
                            }
                            // add field to table
                            fieldTable.Rows.Add(fieldDataValues);
                        }
                        catch
                        {
                            // field property not in response
                        }
                    }
                    // end of table update
                    fieldTable.EndLoadData();
                    propertyTable.EndLoadData();
                    overrideTable.EndLoadData();
                    d_data.AcceptChanges();
                    if (fieldTable.Rows.Count > 0)
                    {
                        richTextBoxDocumentation.Tag = string.Empty;
                        // trigger event to update override grid
                        dataGridViewDataView_RowEnter(this, new DataGridViewCellEventArgs(1, 0));
                    }
                }
            }
            if (message != string.Empty)
            {
                toolStripStatusLabel1.Text = "Request error: " + message;
            }
        }

        /// <summary>
        /// Process categorized field search response
        /// </summary>
        /// <param name="msg"></param>
        private void processCategorizedFieldData(Bloomberglp.Blpapi.Message msg)
        {
            string message = string.Empty;
            string[] elementList = new string[] {"mnemonic", "description", 
                "categoryName", "documentation", "overrides"};
            object[] fieldDataValues = null;

            // process message
            toolStripStatusLabel1.Text = "Processing data...";

            if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("categorizedFieldResponse")))
            {
                if (msg.HasElement("categorizedFieldSearchError"))
                {
                    // process field search error
                    Element reason = msg.GetElement("categorizedFieldSearchError");
                    message = string.Concat("Error: Source-", reason.GetElementAsString("source"),
                        ", Code-", reason.GetElementAsString("code"), ", category-", 
                        reason.GetElementAsString("category"),
                        ", desc-", reason.GetElementAsString("message"));
                }
                else
                {
                    if (msg.HasElement("category"))
                    {
                        // process category
                        Element categories = msg.GetElement("category");
                        int numberOfCategories = categories.NumValues;
                        for(int categoryIndex = 0; categoryIndex < numberOfCategories; categoryIndex++)
                        {
                            // process category data
                            Element category = categories.GetValueAsElement(categoryIndex);
                            Element fieldDataArray = category.GetElement("fieldData");
                            int numberOfFields = fieldDataArray.NumValues;
                            // start table update
                            fieldTable.BeginLoadData();
                            propertyTable.BeginLoadData();
                            overrideTable.BeginLoadData();
                            for (int index = 0; index < numberOfFields; index++)
                            {
                                // process field data
                                fieldDataValues = new object[4]; 
                                Element fieldElement = fieldDataArray.GetValueAsElement(index);
                                Element fieldData = fieldElement.GetElement("fieldInfo");
                                // get field id
                                string fieldId = fieldElement.GetElementAsString("id");
                                fieldDataValues[0] = fieldId;
                                try
                                {
                                    String dataValue = string.Empty;
                                    int dataIndex = 1;
                                    foreach (string item in elementList)
                                    {
                                        // get field property
                                        Element dataElement = fieldData.GetElement(item);
                                        if (dataElement.IsArray)
                                        {
                                            // process array data
                                            switch (item)
                                            {
                                                case "categoryName":
                                                    fieldDataValues[dataIndex] = category.GetElementAsString("categoryName");
                                                    break;
                                                case "overrides":
                                                    for (int overrideIndex = 0; overrideIndex < dataElement.NumValues; overrideIndex++)
                                                    {
                                                        if (overrideTable.Select("parentId = '" + fieldId + "' AND id = '" + 
                                                            dataElement[overrideIndex].ToString() + "'").Length == 0)
                                                        {
                                                            overrideTable.Rows.Add(new object[] { fieldId, dataElement[overrideIndex].ToString(),
                                                                DBNull.Value, DBNull.Value });
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            // process element data
                                            switch (item)
                                            {
                                                case "documentation":
                                                    // add documentation row
                                                    propertyTable.Rows.Add(new object[] { fieldId, dataElement.GetValue() });
                                                    break;
                                                default:
                                                    fieldDataValues[dataIndex] = dataElement.GetValueAsString();
                                                    break;
                                            }
                                        }
                                        dataIndex++;
                                    }
                                    // add field to table
                                    fieldTable.Rows.Add(fieldDataValues);
                                }
                                catch
                                {
                                    // field property not in response
                                }
                            }
                        }
                    }
                    // end of table update
                    fieldTable.EndLoadData();
                    propertyTable.EndLoadData();
                    overrideTable.EndLoadData();
                    d_data.AcceptChanges();
                    if (fieldTable.Rows.Count > 0)
                    {
                        richTextBoxDocumentation.Tag = string.Empty;
                        // trigger event to update override grid
                        dataGridViewDataView_RowEnter(this, new DataGridViewCellEventArgs(1, 0));
                    }
                }
            }
            if (message != string.Empty)
            {
                toolStripStatusLabel1.Text = "Request error: " + message;
            }
        }

        /// <summary>
        /// Process override field data returned
        /// </summary>
        /// <param name="msg"></param>
        private void processOverrides(Bloomberglp.Blpapi.Message msg)
        {
            string[] elementList = new string[] {"mnemonic", "description", "categoryName"};
            object[] fieldDataValues = null;

            if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName("fieldResponse")))
            {
                // process response
                Element fieldDataArray = msg.GetElement("fieldData");
                int numberOfFields = fieldDataArray.NumValues;
                // start table update
                overrideTable.BeginLoadData();
                overrideFieldsTempTable.BeginLoadData();
                for (int index = 0; index < numberOfFields; index++)
                {
                    // process field element
                    fieldDataValues = new object[4];
                    Element fieldElement = fieldDataArray.GetValueAsElement(index);
                    Element fieldData = fieldElement.GetElement("fieldInfo");
                    try
                    {
                        // get field id
                        string fieldId = d_fieldIds[index];
                        // get override for field id
                        string selectCriteria = "id = '" + fieldId + "'";
                        DataRow[] ovrRows = overrideTable.Select(selectCriteria);
                        DataRow[] fields = overrideFieldsTempTable.Select(selectCriteria);
                        if (fields.Length > 0)
                        {
                            // process override fields
                            int dataIndex = 1;
                            foreach (string item in elementList)
                            {
                                // field property
                                Element dataElement = fieldData.GetElement(item);
                                if (dataElement.IsArray)
                                {
                                    // process array data
                                    switch (item)
                                    {
                                        case "categoryName":
                                            // process categoryName here
                                            break;
                                        case "overrides":
                                            // process categoryName here
                                            break;
                                    }
                                }
                                else
                                {
                                    // process element data
                                    switch (item)
                                    {
                                        case "documentation":
                                            // process documentation here
                                            break;
                                        default:
                                            fields[0][dataIndex] = dataElement.GetValueAsString();
                                            break;
                                    }
                                }
                                dataIndex++;
                            }

                            if (ovrRows.Length > 0)
                            {
                                // update override field properties
                                foreach (DataRow ovrRow in ovrRows)
                                {
                                    ovrRow["mnemonic"] = fields[0]["mnemonic"];
                                    ovrRow["description"] = fields[0]["description"];
                                }
                            }
                        }
                    }
                    catch
                    {
                        // field property not in response
                    }
                }
                // end of table update
                overrideTable.BeginLoadData();
                overrideTable.AcceptChanges();
                overrideFieldsTempTable.BeginLoadData();
                overrideFieldsTempTable.AcceptChanges();
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
                            ", Code-", reason.GetElementAsString(ERROR_CODE), ", category-", 
                            reason.GetElementAsString(CATEGORY),
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

    #region "FieldSearchOptions Class"
    /// <summary>
    /// Field search option class
    /// </summary>
    class FieldSearchOptions
    {
        private string _productType = string.Empty;
        private string _fieldType = string.Empty;

        public string ProductType
        {
            get { return _productType; }
            set { _productType = value; }
        }

        public string FieldType
        {
            get { return _fieldType; }
            set { _fieldType = value; }
        }

        public FieldSearchOptions() { }
    }
    #endregion "FieldSearchOptions Class"

    #region "FieldInformation Class"
    /// <summary>
    /// Field information class
    /// </summary>
    class FieldInformation
    {
        private string _id = string.Empty;
        private string _mnemonic = string.Empty;
        private string _dataType = string.Empty;
        private string _description = string.Empty;
        private string _documentation = string.Empty;
        private string _categoryName = string.Empty;

        public string Id
        {
            get { return _id; }
            private set { _id = value; }
        }

        public string Mnemonic
        {
            get { return _mnemonic; }
            private set { _mnemonic = value; }
        }

        public string DataType
        {
            get { return _dataType; }
            private set { _dataType = value; }
        }

        public string Description
        {
            get { return _description; }
            private set { _description = value; }
        }

        public string Documentation
        {
            get { return _documentation; }
            private set { _documentation = value; }
        }

        public string CategoryName
        {
            get { return _categoryName; }
            private set { _categoryName = value; }
        }

        public FieldInformation(string fieldId, string fieldMnemonic, string fieldDataType,
            string fieldDescription, string fieldDocumentation, string fieldCategoryName)
        {
            Id = fieldId;
            Mnemonic = fieldMnemonic;
            DataType = fieldDataType;
            Description = fieldDescription;
            Documentation = fieldDocumentation;
            CategoryName = fieldCategoryName;
        }

        public object[] GetObjectArray()
        {
            object[] data = new object[] { Id, Mnemonic, DataType, Description, 
                Documentation, CategoryName };
            return data;
        }
    }
    #endregion "FieldInformation Class"

}