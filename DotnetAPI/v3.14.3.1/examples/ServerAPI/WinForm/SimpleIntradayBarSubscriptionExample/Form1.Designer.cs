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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.listViewRTIBData = new System.Windows.Forms.ListView();
            this.columnHeaderSecurity = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderTime = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderOpen = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderHigh = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderLow = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderClose = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderVolume = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderNumberOfTicks = new System.Windows.Forms.ColumnHeader();
            this.panelRealtimeIB = new System.Windows.Forms.Panel();
            this.buttonClearAll = new System.Windows.Forms.Button();
            this.buttonClearData = new System.Windows.Forms.Button();
            this.buttonStopSubscribe = new System.Windows.Forms.Button();
            this.buttonSendRequest = new System.Windows.Forms.Button();
            this.buttonAddSecurity = new System.Windows.Forms.Button();
            this.labelNotes = new System.Windows.Forms.Label();
            this.textBoxOutputFile = new System.Windows.Forms.TextBox();
            this.checkBoxOutputFile = new System.Windows.Forms.CheckBox();
            this.labelMinutes = new System.Windows.Forms.Label();
            this.numericUpDownIntervalSize = new System.Windows.Forms.NumericUpDown();
            this.labelIntervalSize = new System.Windows.Forms.Label();
            this.dateTimePickerEndTime = new System.Windows.Forms.DateTimePicker();
            this.labelEndTime = new System.Windows.Forms.Label();
            this.dateTimePickerStartTime = new System.Windows.Forms.DateTimePicker();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.textBoxSecurity = new System.Windows.Forms.TextBox();
            this.labelSecurity = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panelTimeMessage = new System.Windows.Forms.Panel();
            this.labelTime = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.panelRealtimeIB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIntervalSize)).BeginInit();
            this.panelTimeMessage.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 479);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(788, 22);
            this.statusStrip1.TabIndex = 33;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // listViewRTIBData
            // 
            this.listViewRTIBData.AllowDrop = true;
            this.listViewRTIBData.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderSecurity,
            this.columnHeaderTime,
            this.columnHeaderOpen,
            this.columnHeaderHigh,
            this.columnHeaderLow,
            this.columnHeaderClose,
            this.columnHeaderVolume,
            this.columnHeaderNumberOfTicks});
            this.listViewRTIBData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewRTIBData.FullRowSelect = true;
            this.listViewRTIBData.Location = new System.Drawing.Point(0, 116);
            this.listViewRTIBData.MultiSelect = false;
            this.listViewRTIBData.Name = "listViewRTIBData";
            this.listViewRTIBData.Size = new System.Drawing.Size(788, 363);
            this.listViewRTIBData.TabIndex = 35;
            this.listViewRTIBData.TabStop = false;
            this.listViewRTIBData.UseCompatibleStateImageBehavior = false;
            this.listViewRTIBData.View = System.Windows.Forms.View.Details;
            this.listViewRTIBData.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewRTIBData_DragDrop);
            this.listViewRTIBData.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listViewRTIBData_MouseMove);
            this.listViewRTIBData.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewRTIBData_DragEnter);
            this.listViewRTIBData.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewRTIBData_KeyDown);
            // 
            // columnHeaderSecurity
            // 
            this.columnHeaderSecurity.Tag = "Security";
            this.columnHeaderSecurity.Text = "Security";
            this.columnHeaderSecurity.Width = 192;
            // 
            // columnHeaderTime
            // 
            this.columnHeaderTime.Tag = "TIME";
            this.columnHeaderTime.Text = "Time";
            this.columnHeaderTime.Width = 114;
            // 
            // columnHeaderOpen
            // 
            this.columnHeaderOpen.Tag = "OPEN";
            this.columnHeaderOpen.Text = "Open";
            this.columnHeaderOpen.Width = 65;
            // 
            // columnHeaderHigh
            // 
            this.columnHeaderHigh.Tag = "HIGH";
            this.columnHeaderHigh.Text = "High";
            this.columnHeaderHigh.Width = 65;
            // 
            // columnHeaderLow
            // 
            this.columnHeaderLow.Tag = "LOW";
            this.columnHeaderLow.Text = "Low";
            this.columnHeaderLow.Width = 65;
            // 
            // columnHeaderClose
            // 
            this.columnHeaderClose.Tag = "CLOSE";
            this.columnHeaderClose.Text = "Close";
            this.columnHeaderClose.Width = 65;
            // 
            // columnHeaderVolume
            // 
            this.columnHeaderVolume.Tag = "VOLUME";
            this.columnHeaderVolume.Text = "Volume";
            this.columnHeaderVolume.Width = 65;
            // 
            // columnHeaderNumberOfTicks
            // 
            this.columnHeaderNumberOfTicks.Tag = "NUMBER_OF_TICKS";
            this.columnHeaderNumberOfTicks.Text = "Number Of Ticks";
            this.columnHeaderNumberOfTicks.Width = 96;
            // 
            // panelRealtimeIB
            // 
            this.panelRealtimeIB.Controls.Add(this.buttonClearAll);
            this.panelRealtimeIB.Controls.Add(this.buttonClearData);
            this.panelRealtimeIB.Controls.Add(this.buttonStopSubscribe);
            this.panelRealtimeIB.Controls.Add(this.buttonSendRequest);
            this.panelRealtimeIB.Controls.Add(this.buttonAddSecurity);
            this.panelRealtimeIB.Controls.Add(this.labelNotes);
            this.panelRealtimeIB.Controls.Add(this.textBoxOutputFile);
            this.panelRealtimeIB.Controls.Add(this.checkBoxOutputFile);
            this.panelRealtimeIB.Controls.Add(this.labelMinutes);
            this.panelRealtimeIB.Controls.Add(this.numericUpDownIntervalSize);
            this.panelRealtimeIB.Controls.Add(this.labelIntervalSize);
            this.panelRealtimeIB.Controls.Add(this.dateTimePickerEndTime);
            this.panelRealtimeIB.Controls.Add(this.labelEndTime);
            this.panelRealtimeIB.Controls.Add(this.dateTimePickerStartTime);
            this.panelRealtimeIB.Controls.Add(this.labelStartTime);
            this.panelRealtimeIB.Controls.Add(this.textBoxSecurity);
            this.panelRealtimeIB.Controls.Add(this.labelSecurity);
            this.panelRealtimeIB.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelRealtimeIB.Location = new System.Drawing.Point(0, 0);
            this.panelRealtimeIB.Name = "panelRealtimeIB";
            this.panelRealtimeIB.Size = new System.Drawing.Size(788, 116);
            this.panelRealtimeIB.TabIndex = 34;
            // 
            // buttonClearAll
            // 
            this.buttonClearAll.Enabled = false;
            this.buttonClearAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonClearAll.ImageIndex = 3;
            this.buttonClearAll.Location = new System.Drawing.Point(674, 3);
            this.buttonClearAll.Name = "buttonClearAll";
            this.buttonClearAll.Size = new System.Drawing.Size(81, 23);
            this.buttonClearAll.TabIndex = 56;
            this.buttonClearAll.Tag = "RD";
            this.buttonClearAll.Text = "Clear All";
            this.buttonClearAll.UseVisualStyleBackColor = true;
            this.buttonClearAll.Click += new System.EventHandler(this.buttonClearAll_Click);
            // 
            // buttonClearData
            // 
            this.buttonClearData.Enabled = false;
            this.buttonClearData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearData.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonClearData.ImageIndex = 3;
            this.buttonClearData.Location = new System.Drawing.Point(587, 3);
            this.buttonClearData.Name = "buttonClearData";
            this.buttonClearData.Size = new System.Drawing.Size(81, 23);
            this.buttonClearData.TabIndex = 55;
            this.buttonClearData.Tag = "RD";
            this.buttonClearData.Text = "Clear Data";
            this.buttonClearData.UseVisualStyleBackColor = true;
            this.buttonClearData.Click += new System.EventHandler(this.buttonClearData_Click);
            // 
            // buttonStopSubscribe
            // 
            this.buttonStopSubscribe.Enabled = false;
            this.buttonStopSubscribe.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStopSubscribe.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonStopSubscribe.Location = new System.Drawing.Point(498, 3);
            this.buttonStopSubscribe.Name = "buttonStopSubscribe";
            this.buttonStopSubscribe.Size = new System.Drawing.Size(81, 23);
            this.buttonStopSubscribe.TabIndex = 54;
            this.buttonStopSubscribe.Tag = "RD";
            this.buttonStopSubscribe.Text = "Stop";
            this.buttonStopSubscribe.UseVisualStyleBackColor = true;
            this.buttonStopSubscribe.Click += new System.EventHandler(this.buttonStopSubscribe_Click);
            // 
            // buttonSendRequest
            // 
            this.buttonSendRequest.Enabled = false;
            this.buttonSendRequest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSendRequest.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSendRequest.Location = new System.Drawing.Point(411, 3);
            this.buttonSendRequest.Name = "buttonSendRequest";
            this.buttonSendRequest.Size = new System.Drawing.Size(81, 23);
            this.buttonSendRequest.TabIndex = 53;
            this.buttonSendRequest.Tag = "RD";
            this.buttonSendRequest.Text = "Subscribe";
            this.buttonSendRequest.UseVisualStyleBackColor = true;
            this.buttonSendRequest.Click += new System.EventHandler(this.buttonSendRequest_Click);
            // 
            // buttonAddSecurity
            // 
            this.buttonAddSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddSecurity.ImageKey = "Symbol-Add.ico";
            this.buttonAddSecurity.Location = new System.Drawing.Point(324, 3);
            this.buttonAddSecurity.Name = "buttonAddSecurity";
            this.buttonAddSecurity.Size = new System.Drawing.Size(81, 23);
            this.buttonAddSecurity.TabIndex = 52;
            this.buttonAddSecurity.Tag = "RT";
            this.buttonAddSecurity.Text = "Add";
            this.buttonAddSecurity.UseVisualStyleBackColor = true;
            this.buttonAddSecurity.Click += new System.EventHandler(this.buttonAddSecurity_Click);
            // 
            // labelNotes
            // 
            this.labelNotes.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.labelNotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelNotes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelNotes.Location = new System.Drawing.Point(9, 81);
            this.labelNotes.Name = "labelNotes";
            this.labelNotes.Size = new System.Drawing.Size(776, 29);
            this.labelNotes.TabIndex = 51;
            this.labelNotes.Text = resources.GetString("labelNotes.Text");
            this.labelNotes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxOutputFile
            // 
            this.textBoxOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutputFile.Location = new System.Drawing.Point(163, 55);
            this.textBoxOutputFile.Name = "textBoxOutputFile";
            this.textBoxOutputFile.ReadOnly = true;
            this.textBoxOutputFile.Size = new System.Drawing.Size(612, 20);
            this.textBoxOutputFile.TabIndex = 12;
            // 
            // checkBoxOutputFile
            // 
            this.checkBoxOutputFile.AutoSize = true;
            this.checkBoxOutputFile.Location = new System.Drawing.Point(68, 57);
            this.checkBoxOutputFile.Name = "checkBoxOutputFile";
            this.checkBoxOutputFile.Size = new System.Drawing.Size(89, 17);
            this.checkBoxOutputFile.TabIndex = 11;
            this.checkBoxOutputFile.Tag = "RT";
            this.checkBoxOutputFile.Text = "Output to File";
            this.checkBoxOutputFile.UseVisualStyleBackColor = true;
            this.checkBoxOutputFile.CheckedChanged += new System.EventHandler(this.checkBoxOutputFile_CheckedChanged);
            // 
            // labelMinutes
            // 
            this.labelMinutes.AutoSize = true;
            this.labelMinutes.Location = new System.Drawing.Point(449, 35);
            this.labelMinutes.Name = "labelMinutes";
            this.labelMinutes.Size = new System.Drawing.Size(43, 13);
            this.labelMinutes.TabIndex = 9;
            this.labelMinutes.Text = "minutes";
            // 
            // numericUpDownIntervalSize
            // 
            this.numericUpDownIntervalSize.Location = new System.Drawing.Point(399, 31);
            this.numericUpDownIntervalSize.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this.numericUpDownIntervalSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownIntervalSize.Name = "numericUpDownIntervalSize";
            this.numericUpDownIntervalSize.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownIntervalSize.TabIndex = 8;
            this.numericUpDownIntervalSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDownIntervalSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // labelIntervalSize
            // 
            this.labelIntervalSize.AutoSize = true;
            this.labelIntervalSize.Location = new System.Drawing.Point(280, 35);
            this.labelIntervalSize.Name = "labelIntervalSize";
            this.labelIntervalSize.Size = new System.Drawing.Size(113, 13);
            this.labelIntervalSize.TabIndex = 7;
            this.labelIntervalSize.Text = "Time Bar Interval Size:";
            // 
            // dateTimePickerEndTime
            // 
            this.dateTimePickerEndTime.CustomFormat = "HH:mm";
            this.dateTimePickerEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEndTime.Location = new System.Drawing.Point(202, 31);
            this.dateTimePickerEndTime.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerEndTime.Name = "dateTimePickerEndTime";
            this.dateTimePickerEndTime.ShowCheckBox = true;
            this.dateTimePickerEndTime.ShowUpDown = true;
            this.dateTimePickerEndTime.Size = new System.Drawing.Size(71, 20);
            this.dateTimePickerEndTime.TabIndex = 6;
            this.dateTimePickerEndTime.Tag = "IBAR";
            this.dateTimePickerEndTime.ValueChanged += new System.EventHandler(this.dateTimePickerStartTime_ValueChanged);
            // 
            // labelEndTime
            // 
            this.labelEndTime.AutoSize = true;
            this.labelEndTime.Location = new System.Drawing.Point(145, 35);
            this.labelEndTime.Name = "labelEndTime";
            this.labelEndTime.Size = new System.Drawing.Size(55, 13);
            this.labelEndTime.TabIndex = 5;
            this.labelEndTime.Text = "End Time:";
            // 
            // dateTimePickerStartTime
            // 
            this.dateTimePickerStartTime.CustomFormat = "HH:mm";
            this.dateTimePickerStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerStartTime.Location = new System.Drawing.Point(68, 31);
            this.dateTimePickerStartTime.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerStartTime.Name = "dateTimePickerStartTime";
            this.dateTimePickerStartTime.ShowCheckBox = true;
            this.dateTimePickerStartTime.ShowUpDown = true;
            this.dateTimePickerStartTime.Size = new System.Drawing.Size(71, 20);
            this.dateTimePickerStartTime.TabIndex = 4;
            this.dateTimePickerStartTime.Tag = "IBAR";
            this.dateTimePickerStartTime.ValueChanged += new System.EventHandler(this.dateTimePickerStartTime_ValueChanged);
            // 
            // labelStartTime
            // 
            this.labelStartTime.AutoSize = true;
            this.labelStartTime.Location = new System.Drawing.Point(8, 35);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(58, 13);
            this.labelStartTime.TabIndex = 3;
            this.labelStartTime.Text = "Start Time:";
            // 
            // textBoxSecurity
            // 
            this.textBoxSecurity.Location = new System.Drawing.Point(68, 5);
            this.textBoxSecurity.Name = "textBoxSecurity";
            this.textBoxSecurity.Size = new System.Drawing.Size(246, 20);
            this.textBoxSecurity.TabIndex = 1;
            this.textBoxSecurity.Tag = "";
            this.textBoxSecurity.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSecurity_KeyDown);
            // 
            // labelSecurity
            // 
            this.labelSecurity.AutoSize = true;
            this.labelSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSecurity.Location = new System.Drawing.Point(18, 8);
            this.labelSecurity.Name = "labelSecurity";
            this.labelSecurity.Size = new System.Drawing.Size(48, 13);
            this.labelSecurity.TabIndex = 0;
            this.labelSecurity.Text = "Security:";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Symbol-Stop.ico");
            this.imageList1.Images.SetKeyName(1, "Rename.ico");
            this.imageList1.Images.SetKeyName(2, "Document.ico");
            this.imageList1.Images.SetKeyName(3, "Symbol-Refresh.ico");
            this.imageList1.Images.SetKeyName(4, "Arrow-Right.ico");
            this.imageList1.Images.SetKeyName(5, "Arrow-Left.ico");
            this.imageList1.Images.SetKeyName(6, "FloppyDisk.ico");
            this.imageList1.Images.SetKeyName(7, "Find.ico");
            this.imageList1.Images.SetKeyName(8, "Symbol-Add.ico");
            this.imageList1.Images.SetKeyName(9, "PlayHS.png");
            // 
            // toolTip1
            // 
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.ToolTipTitle = "Subscription String";
            // 
            // panelTimeMessage
            // 
            this.panelTimeMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTimeMessage.Controls.Add(this.labelTitle);
            this.panelTimeMessage.Controls.Add(this.labelTime);
            this.panelTimeMessage.Location = new System.Drawing.Point(299, 162);
            this.panelTimeMessage.Name = "panelTimeMessage";
            this.panelTimeMessage.Size = new System.Drawing.Size(193, 85);
            this.panelTimeMessage.TabIndex = 36;
            // 
            // labelTime
            // 
            this.labelTime.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelTime.Location = new System.Drawing.Point(15, 24);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(162, 45);
            this.labelTime.TabIndex = 1;
            this.labelTime.Text = "End time is earlier than start time. Please adjust start or end time before proce" +
                "eding.";
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.Location = new System.Drawing.Point(15, 4);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(58, 13);
            this.labelTitle.TabIndex = 2;
            this.labelTitle.Text = "Warning:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 501);
            this.Controls.Add(this.panelTimeMessage);
            this.Controls.Add(this.listViewRTIBData);
            this.Controls.Add(this.panelRealtimeIB);
            this.Controls.Add(this.statusStrip1);
            this.MinimumSize = new System.Drawing.Size(796, 528);
            this.Name = "Form1";
            this.Text = "Simple Intraday Bar Subscription Example";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelRealtimeIB.ResumeLayout(false);
            this.panelRealtimeIB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIntervalSize)).EndInit();
            this.panelTimeMessage.ResumeLayout(false);
            this.panelTimeMessage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ListView listViewRTIBData;
        private System.Windows.Forms.ColumnHeader columnHeaderSecurity;
        private System.Windows.Forms.ColumnHeader columnHeaderTime;
        private System.Windows.Forms.ColumnHeader columnHeaderOpen;
        private System.Windows.Forms.ColumnHeader columnHeaderHigh;
        private System.Windows.Forms.ColumnHeader columnHeaderLow;
        private System.Windows.Forms.ColumnHeader columnHeaderClose;
        private System.Windows.Forms.ColumnHeader columnHeaderVolume;
        private System.Windows.Forms.ColumnHeader columnHeaderNumberOfTicks;
        private System.Windows.Forms.Panel panelRealtimeIB;
        private System.Windows.Forms.Label labelNotes;
        private System.Windows.Forms.TextBox textBoxOutputFile;
        private System.Windows.Forms.CheckBox checkBoxOutputFile;
        private System.Windows.Forms.Label labelMinutes;
        private System.Windows.Forms.NumericUpDown numericUpDownIntervalSize;
        private System.Windows.Forms.Label labelIntervalSize;
        private System.Windows.Forms.DateTimePicker dateTimePickerEndTime;
        private System.Windows.Forms.Label labelEndTime;
        private System.Windows.Forms.DateTimePicker dateTimePickerStartTime;
        private System.Windows.Forms.Label labelStartTime;
        private System.Windows.Forms.TextBox textBoxSecurity;
        private System.Windows.Forms.Label labelSecurity;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button buttonStopSubscribe;
        private System.Windows.Forms.Button buttonSendRequest;
        private System.Windows.Forms.Button buttonAddSecurity;
        private System.Windows.Forms.Button buttonClearAll;
        private System.Windows.Forms.Button buttonClearData;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panelTimeMessage;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.Label labelTitle;
    }
}

