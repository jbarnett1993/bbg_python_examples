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
/// - Show how to retrieve service schema
/// ==========================================================
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;

namespace Bloomberglp.Blpapi.Examples
{
    public partial class Form1 : Form
    {
        private Session d_session;
        private Dictionary<string, Service> d_serviceDictionary;
        private int d_currentServiceIndex = -1;

        public Form1()
        {
            InitializeComponent();
            // start session
            if (!createSession())
            {
                // error
                MessageBox.Show("Unable to start session.", "Session Error");
                textBoxService.Enabled = false;
                buttonGetService.Enabled = false;
            }
            // create service dictionary
            d_serviceDictionary = new Dictionary<string, Service>();
            // initialize UI controls
            initUI();
        }

        #region methods
        /// <summary>
        /// Initialize form controls
        /// </summary>
        private void initUI()
        {
            textBoxService.Text = "//blp/refdata";
        }

        /// <summary>
        /// Create session
        /// </summary>
        /// <returns></returns>
        private bool createSession()
        {
            // create session
            d_session = new Session(new SessionOptions());
            return d_session.Start();
        }

        /// <summary>
        /// Get service schema
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <returns></returns>
        private void decodeServiceSchema(string serviceName)
        {
            Service service = null;
            string data = string.Empty;

            // get service
            service = d_serviceDictionary[serviceName];
            // clear all child nodes
            treeViewSchema.Nodes.Clear();
            TreeNode node = null;

            if (service.NumOperations > 0)
            {
                // create Operation node
                node = treeViewSchema.Nodes.Add("Operations", "Operations", 0);
                // get service operations
                foreach (Operation op in service.Operations)
                {
                    if (op != null)
                    {
                        // Request name
                        TreeNode childNodeLvl1 = node.Nodes.Add(op.ToString(), op.Name.ToString());
                        // Request Type
                        TreeNode childNodeLvl2 = childNodeLvl1.Nodes.Add(op.RequestDefinition.Name.ToString(), 
                            op.RequestDefinition.Name.ToString() +
                            " (" + op.RequestDefinition.TypeDefinition.Datatype.ToString() + ")");
                        // store element definition
                        childNodeLvl2.Tag = op.RequestDefinition;
                        // process definitions
                        processOperation(childNodeLvl2, op.RequestDefinition.TypeDefinition);
                        // Response
                        if (op.NumResponseDefinition > 0)
                        {
                            childNodeLvl2 = null;
                            foreach (SchemaElementDefinition def in op.ResponseDefinitions)
                            {
                                if (def != null)
                                {
                                    if (childNodeLvl2 == null)
                                    {
                                        childNodeLvl2 = childNodeLvl1.Nodes.Add("Responses", "Responses", 0);
                                    }
                                    // process response definitions
                                    processOperation(childNodeLvl2, def.TypeDefinition);
                                }
                            }
                        }
                    }
                }
                if (node != null)
                {
                    // expand top level node
                    node.Expand();
                }
            }
            // get element definition
            if (service.NumEventDefinitions > 0)
            {
                // create Operation node
                node = treeViewSchema.Nodes.Add("Events", "Events", 0);
                // get service operations
                foreach (SchemaElementDefinition def in service.EventDefinitions)
                {
                    if (def != null)
                    {
                        // request name
                        TreeNode childNodeLvl1 = node.Nodes.Add(def.ToString(), def.Name.ToString(), 0);
                        childNodeLvl1.Tag = def;
                        TreeNode childNodeLvl2 = childNodeLvl1.Nodes.Add(def.TypeDefinition.Name.ToString(), 
                            def.TypeDefinition.Name.ToString() +
                            " (" + def.TypeDefinition.Datatype.ToString() + ")");
                        if (def.TypeDefinition.NumElementDefinitions > 0)
                        {
                            // process request definition
                            processOperation(childNodeLvl2, def.TypeDefinition);
                        }
                    }
                }
                if (node != null)
                {
                    // expand top level node
                    node.Expand();
                }
            }

            treeViewSchema.Sort();
            return;
        }

        /// <summary>
        /// Process schema operations
        /// </summary>
        /// <param name="node"></param>
        /// <param name="elementDef"></param>
        private void processOperation(TreeNode node, SchemaTypeDefinition elementDef)
        {
            foreach (SchemaElementDefinition def in elementDef.ElementDefinitions)
            {
                // add node
                TreeNode nextChild = node.Nodes.Add(def.Name.ToString(), def.Name.ToString() + " (" +
                    def.TypeDefinition.Datatype.ToString() + ")");
                // store node element definition
                nextChild.Tag = def;
                // check if definition had child definitions
                if (def.TypeDefinition.NumElementDefinitions > 0)
                {
                    // process child definition
                    processOperation(nextChild, def.TypeDefinition);
                }
            }
        }
        #endregion

        #region Control Event
        /// <summary>
        /// Get service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGetService_Click(object sender, EventArgs e)
        {
            Service service = null;
            string serviceName = textBoxService.Text.Trim();
            int itemIndex = -1;
            if (serviceName.Length > 0)
            {
                // check if service exist
                if (!d_serviceDictionary.ContainsKey(serviceName))
                {
                    // open services
                    if (d_session.OpenService(serviceName))
                    {
                        service = d_session.GetService(serviceName);
                        // add service to dictionary
                        d_serviceDictionary.Add(serviceName, service);
                        // create item for service
                        int index = listBoxServices.Items.Add(serviceName);
                        listBoxServices.SelectedIndex = index;
                    }
                    else
                    {
                        // unable to open service
                        MessageBox.Show("Unable to open service " + serviceName + ".", "Invalid Service");
                    }
                }
                else
                {
                    // select service
                    itemIndex = listBoxServices.Items.IndexOf(serviceName.ToLower());
                    listBoxServices.SelectedIndex = itemIndex;
                }
            }
        }

        /// <summary>
        /// User press enter in service textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxService_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // call get service button
                buttonGetService_Click(sender, new EventArgs());
            }
        }

        /// <summary>
        /// Process selected service schema
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxServices_SelectedIndexChanged(object sender, EventArgs e)
        {
            // only display service if it is selected and different from previous selection
            if (listBoxServices.SelectedIndex != -1 &&
                listBoxServices.SelectedIndex != d_currentServiceIndex)
            {
                // clear properties list
                listViewProperties.Items.Clear();
                richTextBoxDescription.Text = string.Empty;
                // get service name
                string service = listBoxServices.SelectedItem as string;
                // process service schema
                decodeServiceSchema(service);
                d_currentServiceIndex = listBoxServices.SelectedIndex;
            }
        }

        /// <summary>
        /// Display element properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewSchema_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            SchemaElementDefinition def = null;
            // clear properties list
            listViewProperties.Items.Clear();
            richTextBoxDescription.Text = string.Empty;

            if (node.Tag != null)
            {
                // get element definition
                def = node.Tag as SchemaElementDefinition;
                // get description
                if (def.Description != null)
                {
                    richTextBoxDescription.Text = def.Description;
                }
                // add properties to list
                ListViewItem property = listViewProperties.Items.Add("Name:");
                property.SubItems.Add(def.Name.ToString());
                property = listViewProperties.Items.Add("Status:");
                property.SubItems.Add(def.Status.ToString());
                property = listViewProperties.Items.Add("Type:");
                property.SubItems.Add(def.TypeDefinition.Datatype.ToString());
                if (def.TypeDefinition.IsEnumerationType)
                {
                    foreach (Constant item in def.TypeDefinition.Enumeration.Values)
                    {
                        ListViewItem enumDisplay = listViewProperties.Items.Add("");
                        enumDisplay.SubItems.Add(item.GetValueAsString());
                    }
                }
                property = listViewProperties.Items.Add("Minimal Occurence:");
                property.SubItems.Add(def.MinValues.ToString());
                property = listViewProperties.Items.Add("Maximal Occurence:");
                property.SubItems.Add(def.MaxValues.ToString());
                property = listViewProperties.Items.Add("Constraints:");
                if (def.Constraints != null)
                {
                    foreach (Constraint item in def.Constraints.Values)
                    {
                        ListViewItem enumDisplay = listViewProperties.Items.Add("");
                        enumDisplay.SubItems.Add(item.ConstraintType.ToString());
                    }
                }
            }
        }
        #endregion
    }
}