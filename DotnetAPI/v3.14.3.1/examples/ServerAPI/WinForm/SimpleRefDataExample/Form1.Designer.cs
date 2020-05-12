namespace Bloomberglp.Blpapi.Examples
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.splitContainerRDData = new System.Windows.Forms.SplitContainer();
            this.dataGridViewData = new System.Windows.Forms.DataGridView();
            this.listViewOverrides = new System.Windows.Forms.ListView();
            this.columnHeaderRDOvrFields = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderRDOvrValues = new System.Windows.Forms.ColumnHeader();
            this.buttonAddOverride = new System.Windows.Forms.Button();
            this.textBoxOverride = new System.Windows.Forms.TextBox();
            this.labelOverride = new System.Windows.Forms.Label();
            this.buttonClearData = new System.Windows.Forms.Button();
            this.buttonClearFields = new System.Windows.Forms.Button();
            this.buttonAddField = new System.Windows.Forms.Button();
            this.textBoxField = new System.Windows.Forms.TextBox();
            this.labelField = new System.Windows.Forms.Label();
            this.buttonSendRequest = new System.Windows.Forms.Button();
            this.buttonAddSecurity = new System.Windows.Forms.Button();
            this.textBoxSecurity = new System.Windows.Forms.TextBox();
            this.labelSecurity = new System.Windows.Forms.Label();
            this.labelOverrideNote = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.radioButtonSynch = new System.Windows.Forms.RadioButton();
            this.radioButtonAsynch = new System.Windows.Forms.RadioButton();
            this.buttonClearAll = new System.Windows.Forms.Button();
            this.labelUsageNote = new System.Windows.Forms.Label();
            this.splitContainerRDData.Panel1.SuspendLayout();
            this.splitContainerRDData.Panel2.SuspendLayout();
            this.splitContainerRDData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerRDData
            // 
            this.splitContainerRDData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerRDData.Location = new System.Drawing.Point(6, 123);
            this.splitContainerRDData.Name = "splitContainerRDData";
            // 
            // splitContainerRDData.Panel1
            // 
            this.splitContainerRDData.Panel1.Controls.Add(this.dataGridViewData);
            // 
            // splitContainerRDData.Panel2
            // 
            this.splitContainerRDData.Panel2.Controls.Add(this.listViewOverrides);
            this.splitContainerRDData.Size = new System.Drawing.Size(801, 348);
            this.splitContainerRDData.SplitterDistance = 559;
            this.splitContainerRDData.TabIndex = 40;
            // 
            // dataGridViewData
            // 
            this.dataGridViewData.AllowDrop = true;
            this.dataGridViewData.AllowUserToAddRows = false;
            this.dataGridViewData.AllowUserToDeleteRows = false;
            this.dataGridViewData.AllowUserToOrderColumns = true;
            this.dataGridViewData.AllowUserToResizeRows = false;
            this.dataGridViewData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewData.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewData.MultiSelect = false;
            this.dataGridViewData.Name = "dataGridViewData";
            this.dataGridViewData.ReadOnly = true;
            this.dataGridViewData.RowHeadersVisible = false;
            this.dataGridViewData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewData.Size = new System.Drawing.Size(559, 348);
            this.dataGridViewData.TabIndex = 14;
            this.dataGridViewData.Tag = "RD";
            this.dataGridViewData.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewData_CellMouseClick);
            this.dataGridViewData.DragEnter += new System.Windows.Forms.DragEventHandler(this.dataGridViewData_DragEnter);
            this.dataGridViewData.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridViewData_KeyDown);
            this.dataGridViewData.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGridViewData_DragDrop);
            // 
            // listViewOverrides
            // 
            this.listViewOverrides.AllowDrop = true;
            this.listViewOverrides.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderRDOvrFields,
            this.columnHeaderRDOvrValues});
            this.listViewOverrides.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewOverrides.FullRowSelect = true;
            this.listViewOverrides.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewOverrides.Location = new System.Drawing.Point(0, 0);
            this.listViewOverrides.Name = "listViewOverrides";
            this.listViewOverrides.Size = new System.Drawing.Size(238, 348);
            this.listViewOverrides.TabIndex = 15;
            this.listViewOverrides.Tag = "RD";
            this.listViewOverrides.UseCompatibleStateImageBehavior = false;
            this.listViewOverrides.View = System.Windows.Forms.View.Details;
            this.listViewOverrides.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewOverrides_DragDrop);
            this.listViewOverrides.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewOverrides_DragEnter);
            this.listViewOverrides.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewOverrides_KeyDown);
            // 
            // columnHeaderRDOvrFields
            // 
            this.columnHeaderRDOvrFields.Text = "Override Fields";
            this.columnHeaderRDOvrFields.Width = 149;
            // 
            // columnHeaderRDOvrValues
            // 
            this.columnHeaderRDOvrValues.Text = "Values";
            this.columnHeaderRDOvrValues.Width = 80;
            // 
            // buttonAddOverride
            // 
            this.buttonAddOverride.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddOverride.Location = new System.Drawing.Point(375, 61);
            this.buttonAddOverride.Name = "buttonAddOverride";
            this.buttonAddOverride.Size = new System.Drawing.Size(81, 23);
            this.buttonAddOverride.TabIndex = 31;
            this.buttonAddOverride.Tag = "RD";
            this.buttonAddOverride.Text = "Add";
            this.buttonAddOverride.UseVisualStyleBackColor = true;
            this.buttonAddOverride.Click += new System.EventHandler(this.buttonAddOverride_Click);
            // 
            // textBoxOverride
            // 
            this.textBoxOverride.Location = new System.Drawing.Point(75, 63);
            this.textBoxOverride.Name = "textBoxOverride";
            this.textBoxOverride.Size = new System.Drawing.Size(294, 20);
            this.textBoxOverride.TabIndex = 30;
            this.textBoxOverride.Tag = "RD";
            this.textBoxOverride.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxOverride_KeyDown);
            // 
            // labelOverride
            // 
            this.labelOverride.AutoSize = true;
            this.labelOverride.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOverride.Location = new System.Drawing.Point(23, 66);
            this.labelOverride.Name = "labelOverride";
            this.labelOverride.Size = new System.Drawing.Size(50, 13);
            this.labelOverride.TabIndex = 29;
            this.labelOverride.Tag = "RD";
            this.labelOverride.Text = "Override:";
            // 
            // buttonClearData
            // 
            this.buttonClearData.Enabled = false;
            this.buttonClearData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearData.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonClearData.ImageIndex = 3;
            this.buttonClearData.Location = new System.Drawing.Point(638, 4);
            this.buttonClearData.Name = "buttonClearData";
            this.buttonClearData.Size = new System.Drawing.Size(81, 23);
            this.buttonClearData.TabIndex = 34;
            this.buttonClearData.Tag = "RD";
            this.buttonClearData.Text = "Clear Data";
            this.buttonClearData.UseVisualStyleBackColor = true;
            this.buttonClearData.Click += new System.EventHandler(this.buttonClearData_Click);
            // 
            // buttonClearFields
            // 
            this.buttonClearFields.Enabled = false;
            this.buttonClearFields.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearFields.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonClearFields.ImageIndex = 2;
            this.buttonClearFields.Location = new System.Drawing.Point(551, 4);
            this.buttonClearFields.Name = "buttonClearFields";
            this.buttonClearFields.Size = new System.Drawing.Size(81, 23);
            this.buttonClearFields.TabIndex = 33;
            this.buttonClearFields.Tag = "RD";
            this.buttonClearFields.Text = "Clear Fields";
            this.buttonClearFields.UseVisualStyleBackColor = true;
            this.buttonClearFields.Click += new System.EventHandler(this.buttonClearFields_Click);
            // 
            // buttonAddField
            // 
            this.buttonAddField.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddField.Location = new System.Drawing.Point(375, 32);
            this.buttonAddField.Name = "buttonAddField";
            this.buttonAddField.Size = new System.Drawing.Size(81, 23);
            this.buttonAddField.TabIndex = 28;
            this.buttonAddField.Tag = "RD";
            this.buttonAddField.Text = "Add";
            this.buttonAddField.UseVisualStyleBackColor = true;
            this.buttonAddField.Click += new System.EventHandler(this.buttonAddField_Click);
            // 
            // textBoxField
            // 
            this.textBoxField.AllowDrop = true;
            this.textBoxField.Location = new System.Drawing.Point(75, 34);
            this.textBoxField.Name = "textBoxField";
            this.textBoxField.Size = new System.Drawing.Size(294, 20);
            this.textBoxField.TabIndex = 27;
            this.textBoxField.Tag = "RD";
            this.textBoxField.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxField_KeyDown);
            // 
            // labelField
            // 
            this.labelField.AutoSize = true;
            this.labelField.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelField.Location = new System.Drawing.Point(41, 37);
            this.labelField.Name = "labelField";
            this.labelField.Size = new System.Drawing.Size(32, 13);
            this.labelField.TabIndex = 26;
            this.labelField.Text = "Field:";
            // 
            // buttonSendRequest
            // 
            this.buttonSendRequest.Enabled = false;
            this.buttonSendRequest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSendRequest.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSendRequest.Location = new System.Drawing.Point(464, 4);
            this.buttonSendRequest.Name = "buttonSendRequest";
            this.buttonSendRequest.Size = new System.Drawing.Size(81, 23);
            this.buttonSendRequest.TabIndex = 32;
            this.buttonSendRequest.Tag = "RD";
            this.buttonSendRequest.Text = "Submit";
            this.buttonSendRequest.UseVisualStyleBackColor = true;
            this.buttonSendRequest.Click += new System.EventHandler(this.buttonSendRequest_Click);
            // 
            // buttonAddSecurity
            // 
            this.buttonAddSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddSecurity.Location = new System.Drawing.Point(375, 4);
            this.buttonAddSecurity.Name = "buttonAddSecurity";
            this.buttonAddSecurity.Size = new System.Drawing.Size(81, 23);
            this.buttonAddSecurity.TabIndex = 25;
            this.buttonAddSecurity.Tag = "RD";
            this.buttonAddSecurity.Text = "Add";
            this.buttonAddSecurity.UseVisualStyleBackColor = true;
            this.buttonAddSecurity.Click += new System.EventHandler(this.buttonAddSecurity_Click);
            // 
            // textBoxSecurity
            // 
            this.textBoxSecurity.AllowDrop = true;
            this.textBoxSecurity.Location = new System.Drawing.Point(75, 6);
            this.textBoxSecurity.Name = "textBoxSecurity";
            this.textBoxSecurity.Size = new System.Drawing.Size(294, 20);
            this.textBoxSecurity.TabIndex = 24;
            this.textBoxSecurity.Tag = "RD";
            this.textBoxSecurity.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSecurity_KeyDown);
            // 
            // labelSecurity
            // 
            this.labelSecurity.AutoSize = true;
            this.labelSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSecurity.Location = new System.Drawing.Point(25, 9);
            this.labelSecurity.Name = "labelSecurity";
            this.labelSecurity.Size = new System.Drawing.Size(48, 13);
            this.labelSecurity.TabIndex = 23;
            this.labelSecurity.Text = "Security:";
            // 
            // labelOverrideNote
            // 
            this.labelOverrideNote.AutoSize = true;
            this.labelOverrideNote.Location = new System.Drawing.Point(461, 66);
            this.labelOverrideNote.Name = "labelOverrideNote";
            this.labelOverrideNote.Size = new System.Drawing.Size(249, 13);
            this.labelOverrideNote.TabIndex = 37;
            this.labelOverrideNote.Text = "(Note: Override example:  SETTLE_DT=20081008)";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 481);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(811, 22);
            this.statusStrip1.TabIndex = 38;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // radioButtonSynch
            // 
            this.radioButtonSynch.AutoSize = true;
            this.radioButtonSynch.Location = new System.Drawing.Point(563, 35);
            this.radioButtonSynch.Name = "radioButtonSynch";
            this.radioButtonSynch.Size = new System.Drawing.Size(87, 17);
            this.radioButtonSynch.TabIndex = 37;
            this.radioButtonSynch.Text = "Synchronous";
            this.radioButtonSynch.UseVisualStyleBackColor = true;
            // 
            // radioButtonAsynch
            // 
            this.radioButtonAsynch.AutoSize = true;
            this.radioButtonAsynch.Checked = true;
            this.radioButtonAsynch.Location = new System.Drawing.Point(465, 35);
            this.radioButtonAsynch.Name = "radioButtonAsynch";
            this.radioButtonAsynch.Size = new System.Drawing.Size(92, 17);
            this.radioButtonAsynch.TabIndex = 36;
            this.radioButtonAsynch.TabStop = true;
            this.radioButtonAsynch.Text = "Asynchronous";
            this.radioButtonAsynch.UseVisualStyleBackColor = true;
            // 
            // buttonClearAll
            // 
            this.buttonClearAll.Enabled = false;
            this.buttonClearAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonClearAll.ImageIndex = 3;
            this.buttonClearAll.Location = new System.Drawing.Point(725, 4);
            this.buttonClearAll.Name = "buttonClearAll";
            this.buttonClearAll.Size = new System.Drawing.Size(81, 23);
            this.buttonClearAll.TabIndex = 35;
            this.buttonClearAll.Tag = "RD";
            this.buttonClearAll.Text = "Clear All";
            this.buttonClearAll.UseVisualStyleBackColor = true;
            this.buttonClearAll.Click += new System.EventHandler(this.buttonClearAll_Click);
            // 
            // labelUsageNote
            // 
            this.labelUsageNote.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.labelUsageNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelUsageNote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelUsageNote.Location = new System.Drawing.Point(6, 90);
            this.labelUsageNote.Name = "labelUsageNote";
            this.labelUsageNote.Size = new System.Drawing.Size(800, 29);
            this.labelUsageNote.TabIndex = 41;
            this.labelUsageNote.Text = resources.GetString("labelUsageNote.Text");
            this.labelUsageNote.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(811, 503);
            this.Controls.Add(this.labelUsageNote);
            this.Controls.Add(this.buttonClearAll);
            this.Controls.Add(this.radioButtonSynch);
            this.Controls.Add(this.radioButtonAsynch);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.labelOverrideNote);
            this.Controls.Add(this.splitContainerRDData);
            this.Controls.Add(this.buttonAddOverride);
            this.Controls.Add(this.textBoxOverride);
            this.Controls.Add(this.labelOverride);
            this.Controls.Add(this.buttonClearData);
            this.Controls.Add(this.buttonClearFields);
            this.Controls.Add(this.buttonAddField);
            this.Controls.Add(this.textBoxField);
            this.Controls.Add(this.labelField);
            this.Controls.Add(this.buttonSendRequest);
            this.Controls.Add(this.buttonAddSecurity);
            this.Controls.Add(this.textBoxSecurity);
            this.Controls.Add(this.labelSecurity);
            this.MinimumSize = new System.Drawing.Size(819, 530);
            this.Name = "Form1";
            this.Text = "Simple Reference Data with Override Example";
            this.splitContainerRDData.Panel1.ResumeLayout(false);
            this.splitContainerRDData.Panel2.ResumeLayout(false);
            this.splitContainerRDData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerRDData;
        private System.Windows.Forms.DataGridView dataGridViewData;
        private System.Windows.Forms.ListView listViewOverrides;
        private System.Windows.Forms.ColumnHeader columnHeaderRDOvrFields;
        private System.Windows.Forms.ColumnHeader columnHeaderRDOvrValues;
        private System.Windows.Forms.Button buttonAddOverride;
        private System.Windows.Forms.TextBox textBoxOverride;
        private System.Windows.Forms.Label labelOverride;
        private System.Windows.Forms.Button buttonClearData;
        private System.Windows.Forms.Button buttonClearFields;
        private System.Windows.Forms.Button buttonAddField;
        private System.Windows.Forms.TextBox textBoxField;
        private System.Windows.Forms.Label labelField;
        private System.Windows.Forms.Button buttonSendRequest;
        private System.Windows.Forms.Button buttonAddSecurity;
        private System.Windows.Forms.TextBox textBoxSecurity;
        private System.Windows.Forms.Label labelSecurity;
        private System.Windows.Forms.Label labelOverrideNote;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.RadioButton radioButtonSynch;
        private System.Windows.Forms.RadioButton radioButtonAsynch;
        private System.Windows.Forms.Button buttonClearAll;
        private System.Windows.Forms.Label labelUsageNote;
    }
}

