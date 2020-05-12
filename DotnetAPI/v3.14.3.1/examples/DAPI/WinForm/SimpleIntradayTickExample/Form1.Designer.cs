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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.labelIRSecurity = new System.Windows.Forms.Label();
            this.textBoxSecurity = new System.Windows.Forms.TextBox();
            this.labelIRStartDate = new System.Windows.Forms.Label();
            this.dateTimePickerStartDate = new System.Windows.Forms.DateTimePicker();
            this.labelIREndDate = new System.Windows.Forms.Label();
            this.dateTimePickerEndDate = new System.Windows.Forms.DateTimePicker();
            this.checkBoxIncludeConditionCode = new System.Windows.Forms.CheckBox();
            this.checkedListBoxEventTypes = new System.Windows.Forms.CheckedListBox();
            this.labelEventTypes = new System.Windows.Forms.Label();
            this.checkBoxIncludeExchangeCode = new System.Windows.Forms.CheckBox();
            this.buttonSendRequest = new System.Windows.Forms.Button();
            this.buttonClearAll = new System.Windows.Forms.Button();
            this.radioButtonAsynch = new System.Windows.Forms.RadioButton();
            this.radioButtonSynch = new System.Windows.Forms.RadioButton();
            this.panelIRTopView = new System.Windows.Forms.Panel();
            this.splitContainerIRView = new System.Windows.Forms.SplitContainer();
            this.listBoxSecurities = new System.Windows.Forms.ListBox();
            this.dataGridViewData = new System.Windows.Forms.DataGridView();
            this.checkBoxIncludeBrokerCode = new System.Windows.Forms.CheckBox();
            this.statusStrip1.SuspendLayout();
            this.panelIRTopView.SuspendLayout();
            this.splitContainerIRView.Panel1.SuspendLayout();
            this.splitContainerIRView.Panel2.SuspendLayout();
            this.splitContainerIRView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 448);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(806, 22);
            this.statusStrip1.TabIndex = 30;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // labelIRSecurity
            // 
            this.labelIRSecurity.AutoSize = true;
            this.labelIRSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelIRSecurity.Location = new System.Drawing.Point(28, 8);
            this.labelIRSecurity.Name = "labelIRSecurity";
            this.labelIRSecurity.Size = new System.Drawing.Size(48, 13);
            this.labelIRSecurity.TabIndex = 0;
            this.labelIRSecurity.Text = "Security:";
            // 
            // textBoxSecurity
            // 
            this.textBoxSecurity.Location = new System.Drawing.Point(78, 5);
            this.textBoxSecurity.Name = "textBoxSecurity";
            this.textBoxSecurity.Size = new System.Drawing.Size(294, 20);
            this.textBoxSecurity.TabIndex = 1;
            this.textBoxSecurity.Tag = "IBAR";
            this.textBoxSecurity.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSecurity_KeyDown);
            // 
            // labelIRStartDate
            // 
            this.labelIRStartDate.AutoSize = true;
            this.labelIRStartDate.Location = new System.Drawing.Point(18, 35);
            this.labelIRStartDate.Name = "labelIRStartDate";
            this.labelIRStartDate.Size = new System.Drawing.Size(58, 13);
            this.labelIRStartDate.TabIndex = 2;
            this.labelIRStartDate.Text = "Start Date:";
            // 
            // dateTimePickerStartDate
            // 
            this.dateTimePickerStartDate.CustomFormat = "dddd, MMMM dd, yyyy - HH:mm:ss";
            this.dateTimePickerStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerStartDate.Location = new System.Drawing.Point(78, 31);
            this.dateTimePickerStartDate.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerStartDate.Name = "dateTimePickerStartDate";
            this.dateTimePickerStartDate.Size = new System.Drawing.Size(294, 20);
            this.dateTimePickerStartDate.TabIndex = 3;
            this.dateTimePickerStartDate.Tag = "IBAR";
            this.dateTimePickerStartDate.ValueChanged += new System.EventHandler(this.dateTimePickerEndDate_ValueChanged);
            // 
            // labelIREndDate
            // 
            this.labelIREndDate.AutoSize = true;
            this.labelIREndDate.Location = new System.Drawing.Point(21, 63);
            this.labelIREndDate.Name = "labelIREndDate";
            this.labelIREndDate.Size = new System.Drawing.Size(55, 13);
            this.labelIREndDate.TabIndex = 4;
            this.labelIREndDate.Text = "End Date:";
            // 
            // dateTimePickerEndDate
            // 
            this.dateTimePickerEndDate.CustomFormat = "dddd, MMMM dd, yyyy - HH:mm:ss";
            this.dateTimePickerEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEndDate.Location = new System.Drawing.Point(78, 59);
            this.dateTimePickerEndDate.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerEndDate.Name = "dateTimePickerEndDate";
            this.dateTimePickerEndDate.Size = new System.Drawing.Size(294, 20);
            this.dateTimePickerEndDate.TabIndex = 5;
            this.dateTimePickerEndDate.Tag = "IBAR";
            this.dateTimePickerEndDate.ValueChanged += new System.EventHandler(this.dateTimePickerEndDate_ValueChanged);
            // 
            // checkBoxIncludeConditionCode
            // 
            this.checkBoxIncludeConditionCode.AutoSize = true;
            this.checkBoxIncludeConditionCode.Location = new System.Drawing.Point(462, 81);
            this.checkBoxIncludeConditionCode.Name = "checkBoxIncludeConditionCode";
            this.checkBoxIncludeConditionCode.Size = new System.Drawing.Size(141, 17);
            this.checkBoxIncludeConditionCode.TabIndex = 8;
            this.checkBoxIncludeConditionCode.Text = "Include Condition Codes";
            this.checkBoxIncludeConditionCode.UseVisualStyleBackColor = true;
            // 
            // checkedListBoxEventTypes
            // 
            this.checkedListBoxEventTypes.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBoxEventTypes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checkedListBoxEventTypes.CheckOnClick = true;
            this.checkedListBoxEventTypes.FormattingEnabled = true;
            this.checkedListBoxEventTypes.Items.AddRange(new object[] {
            "TRADE",
            "BID",
            "ASK",
            "BID_BEST",
            "ASK_BEST",
            "BID_YIELD",
            "ASK_YIELD",
            "MID_PRICE",
            "AT_TRADE"});
            this.checkedListBoxEventTypes.Location = new System.Drawing.Point(77, 85);
            this.checkedListBoxEventTypes.MultiColumn = true;
            this.checkedListBoxEventTypes.Name = "checkedListBoxEventTypes";
            this.checkedListBoxEventTypes.Size = new System.Drawing.Size(376, 47);
            this.checkedListBoxEventTypes.TabIndex = 7;
            this.checkedListBoxEventTypes.ThreeDCheckBoxes = true;
            this.checkedListBoxEventTypes.SelectedIndexChanged += new System.EventHandler(this.checkedListBoxEventTypes_SelectedIndexChanged);
            // 
            // labelEventTypes
            // 
            this.labelEventTypes.AutoSize = true;
            this.labelEventTypes.Location = new System.Drawing.Point(6, 85);
            this.labelEventTypes.Name = "labelEventTypes";
            this.labelEventTypes.Size = new System.Drawing.Size(70, 13);
            this.labelEventTypes.TabIndex = 6;
            this.labelEventTypes.Text = "Event Types:";
            // 
            // checkBoxIncludeExchangeCode
            // 
            this.checkBoxIncludeExchangeCode.AutoSize = true;
            this.checkBoxIncludeExchangeCode.Location = new System.Drawing.Point(462, 99);
            this.checkBoxIncludeExchangeCode.Name = "checkBoxIncludeExchangeCode";
            this.checkBoxIncludeExchangeCode.Size = new System.Drawing.Size(145, 17);
            this.checkBoxIncludeExchangeCode.TabIndex = 9;
            this.checkBoxIncludeExchangeCode.Text = "Include Exchange Codes";
            this.checkBoxIncludeExchangeCode.UseVisualStyleBackColor = true;
            // 
            // buttonSendRequest
            // 
            this.buttonSendRequest.Enabled = false;
            this.buttonSendRequest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSendRequest.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSendRequest.Location = new System.Drawing.Point(384, 3);
            this.buttonSendRequest.Name = "buttonSendRequest";
            this.buttonSendRequest.Size = new System.Drawing.Size(81, 23);
            this.buttonSendRequest.TabIndex = 10;
            this.buttonSendRequest.Tag = "RD";
            this.buttonSendRequest.Text = "Submit";
            this.buttonSendRequest.UseVisualStyleBackColor = true;
            this.buttonSendRequest.Click += new System.EventHandler(this.buttonSendRequest_Click);
            // 
            // buttonClearAll
            // 
            this.buttonClearAll.Enabled = false;
            this.buttonClearAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonClearAll.ImageIndex = 3;
            this.buttonClearAll.Location = new System.Drawing.Point(471, 3);
            this.buttonClearAll.Name = "buttonClearAll";
            this.buttonClearAll.Size = new System.Drawing.Size(81, 23);
            this.buttonClearAll.TabIndex = 11;
            this.buttonClearAll.Tag = "RD";
            this.buttonClearAll.Text = "Clear All";
            this.buttonClearAll.UseVisualStyleBackColor = true;
            this.buttonClearAll.Click += new System.EventHandler(this.buttonClearAll_Click);
            // 
            // radioButtonAsynch
            // 
            this.radioButtonAsynch.AutoSize = true;
            this.radioButtonAsynch.Checked = true;
            this.radioButtonAsynch.Location = new System.Drawing.Point(383, 32);
            this.radioButtonAsynch.Name = "radioButtonAsynch";
            this.radioButtonAsynch.Size = new System.Drawing.Size(92, 17);
            this.radioButtonAsynch.TabIndex = 12;
            this.radioButtonAsynch.TabStop = true;
            this.radioButtonAsynch.Text = "Asynchronous";
            this.radioButtonAsynch.UseVisualStyleBackColor = true;
            // 
            // radioButtonSynch
            // 
            this.radioButtonSynch.AutoSize = true;
            this.radioButtonSynch.Location = new System.Drawing.Point(481, 32);
            this.radioButtonSynch.Name = "radioButtonSynch";
            this.radioButtonSynch.Size = new System.Drawing.Size(87, 17);
            this.radioButtonSynch.TabIndex = 13;
            this.radioButtonSynch.Text = "Synchronous";
            this.radioButtonSynch.UseVisualStyleBackColor = true;
            // 
            // panelIRTopView
            // 
            this.panelIRTopView.Controls.Add(this.checkBoxIncludeBrokerCode);
            this.panelIRTopView.Controls.Add(this.radioButtonSynch);
            this.panelIRTopView.Controls.Add(this.radioButtonAsynch);
            this.panelIRTopView.Controls.Add(this.buttonClearAll);
            this.panelIRTopView.Controls.Add(this.buttonSendRequest);
            this.panelIRTopView.Controls.Add(this.checkBoxIncludeExchangeCode);
            this.panelIRTopView.Controls.Add(this.labelEventTypes);
            this.panelIRTopView.Controls.Add(this.checkedListBoxEventTypes);
            this.panelIRTopView.Controls.Add(this.checkBoxIncludeConditionCode);
            this.panelIRTopView.Controls.Add(this.dateTimePickerEndDate);
            this.panelIRTopView.Controls.Add(this.labelIREndDate);
            this.panelIRTopView.Controls.Add(this.dateTimePickerStartDate);
            this.panelIRTopView.Controls.Add(this.labelIRStartDate);
            this.panelIRTopView.Controls.Add(this.textBoxSecurity);
            this.panelIRTopView.Controls.Add(this.labelIRSecurity);
            this.panelIRTopView.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelIRTopView.Location = new System.Drawing.Point(0, 0);
            this.panelIRTopView.Name = "panelIRTopView";
            this.panelIRTopView.Size = new System.Drawing.Size(806, 139);
            this.panelIRTopView.TabIndex = 28;
            // 
            // splitContainerIRView
            // 
            this.splitContainerIRView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerIRView.Location = new System.Drawing.Point(0, 139);
            this.splitContainerIRView.Name = "splitContainerIRView";
            // 
            // splitContainerIRView.Panel1
            // 
            this.splitContainerIRView.Panel1.Controls.Add(this.listBoxSecurities);
            // 
            // splitContainerIRView.Panel2
            // 
            this.splitContainerIRView.Panel2.Controls.Add(this.dataGridViewData);
            this.splitContainerIRView.Size = new System.Drawing.Size(806, 309);
            this.splitContainerIRView.SplitterDistance = 190;
            this.splitContainerIRView.TabIndex = 31;
            // 
            // listBoxSecurities
            // 
            this.listBoxSecurities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxSecurities.FormattingEnabled = true;
            this.listBoxSecurities.Location = new System.Drawing.Point(0, 0);
            this.listBoxSecurities.Name = "listBoxSecurities";
            this.listBoxSecurities.Size = new System.Drawing.Size(190, 303);
            this.listBoxSecurities.TabIndex = 14;
            this.listBoxSecurities.SelectedIndexChanged += new System.EventHandler(this.listBoxSecurities_SelectedIndexChanged);
            // 
            // dataGridViewData
            // 
            this.dataGridViewData.AllowUserToAddRows = false;
            this.dataGridViewData.AllowUserToDeleteRows = false;
            this.dataGridViewData.AllowUserToResizeRows = false;
            this.dataGridViewData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewData.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewData.MultiSelect = false;
            this.dataGridViewData.Name = "dataGridViewData";
            this.dataGridViewData.ReadOnly = true;
            this.dataGridViewData.RowHeadersVisible = false;
            this.dataGridViewData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewData.Size = new System.Drawing.Size(612, 309);
            this.dataGridViewData.TabIndex = 18;
            this.dataGridViewData.TabStop = false;
            this.dataGridViewData.Tag = "HD";
            // 
            // checkBoxIncludeBrokerCode
            // 
            this.checkBoxIncludeBrokerCode.AutoSize = true;
            this.checkBoxIncludeBrokerCode.Location = new System.Drawing.Point(462, 118);
            this.checkBoxIncludeBrokerCode.Name = "checkBoxIncludeBrokerCode";
            this.checkBoxIncludeBrokerCode.Size = new System.Drawing.Size(128, 17);
            this.checkBoxIncludeBrokerCode.TabIndex = 14;
            this.checkBoxIncludeBrokerCode.Text = "Include Broker Codes";
            this.checkBoxIncludeBrokerCode.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 470);
            this.Controls.Add(this.splitContainerIRView);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panelIRTopView);
            this.MinimumSize = new System.Drawing.Size(814, 497);
            this.Name = "Form1";
            this.Text = "Simple Intraday Ticks Example";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelIRTopView.ResumeLayout(false);
            this.panelIRTopView.PerformLayout();
            this.splitContainerIRView.Panel1.ResumeLayout(false);
            this.splitContainerIRView.Panel2.ResumeLayout(false);
            this.splitContainerIRView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Label labelIRSecurity;
        private System.Windows.Forms.TextBox textBoxSecurity;
        private System.Windows.Forms.Label labelIRStartDate;
        private System.Windows.Forms.DateTimePicker dateTimePickerStartDate;
        private System.Windows.Forms.Label labelIREndDate;
        private System.Windows.Forms.DateTimePicker dateTimePickerEndDate;
        private System.Windows.Forms.CheckBox checkBoxIncludeConditionCode;
        private System.Windows.Forms.CheckedListBox checkedListBoxEventTypes;
        private System.Windows.Forms.Label labelEventTypes;
        private System.Windows.Forms.CheckBox checkBoxIncludeExchangeCode;
        private System.Windows.Forms.Button buttonSendRequest;
        private System.Windows.Forms.Button buttonClearAll;
        private System.Windows.Forms.RadioButton radioButtonAsynch;
        private System.Windows.Forms.RadioButton radioButtonSynch;
        private System.Windows.Forms.Panel panelIRTopView;
        private System.Windows.Forms.SplitContainer splitContainerIRView;
        private System.Windows.Forms.ListBox listBoxSecurities;
        private System.Windows.Forms.DataGridView dataGridViewData;
        private System.Windows.Forms.CheckBox checkBoxIncludeBrokerCode;

    }
}

