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
            this.checkBoxForceDelay = new System.Windows.Forms.CheckBox();
            this.checkBoxOutputFile = new System.Windows.Forms.CheckBox();
            this.textBoxInterval = new System.Windows.Forms.TextBox();
            this.labelInterval = new System.Windows.Forms.Label();
            this.dataGridViewData = new System.Windows.Forms.DataGridView();
            this.buttonAddField = new System.Windows.Forms.Button();
            this.textBoxField = new System.Windows.Forms.TextBox();
            this.labelField = new System.Windows.Forms.Label();
            this.buttonAddSecurity = new System.Windows.Forms.Button();
            this.textBoxSecurity = new System.Windows.Forms.TextBox();
            this.labelSecurity = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonClearAll = new System.Windows.Forms.Button();
            this.buttonClearData = new System.Windows.Forms.Button();
            this.buttonClearFields = new System.Windows.Forms.Button();
            this.buttonSendRequest = new System.Windows.Forms.Button();
            this.buttonStopSubscribe = new System.Windows.Forms.Button();
            this.textBoxOutputFile = new System.Windows.Forms.TextBox();
            this.labelUsageNote = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxForceDelay
            // 
            this.checkBoxForceDelay.AutoSize = true;
            this.checkBoxForceDelay.Location = new System.Drawing.Point(132, 62);
            this.checkBoxForceDelay.Name = "checkBoxForceDelay";
            this.checkBoxForceDelay.Size = new System.Drawing.Size(119, 17);
            this.checkBoxForceDelay.TabIndex = 8;
            this.checkBoxForceDelay.Tag = "RT";
            this.checkBoxForceDelay.Text = "Force Delay Stream";
            this.checkBoxForceDelay.UseVisualStyleBackColor = true;
            // 
            // checkBoxOutputFile
            // 
            this.checkBoxOutputFile.AutoSize = true;
            this.checkBoxOutputFile.Location = new System.Drawing.Point(257, 62);
            this.checkBoxOutputFile.Name = "checkBoxOutputFile";
            this.checkBoxOutputFile.Size = new System.Drawing.Size(89, 17);
            this.checkBoxOutputFile.TabIndex = 9;
            this.checkBoxOutputFile.Tag = "RT";
            this.checkBoxOutputFile.Text = "Output to File";
            this.checkBoxOutputFile.UseVisualStyleBackColor = true;
            this.checkBoxOutputFile.CheckedChanged += new System.EventHandler(this.checkBoxOutputFile_CheckedChanged);
            // 
            // textBoxInterval
            // 
            this.textBoxInterval.Location = new System.Drawing.Point(72, 60);
            this.textBoxInterval.MaxLength = 5;
            this.textBoxInterval.Name = "textBoxInterval";
            this.textBoxInterval.Size = new System.Drawing.Size(54, 20);
            this.textBoxInterval.TabIndex = 7;
            this.textBoxInterval.Tag = "RT";
            this.textBoxInterval.Text = "0";
            this.textBoxInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxInterval_KeyDown);
            // 
            // labelInterval
            // 
            this.labelInterval.AutoSize = true;
            this.labelInterval.Location = new System.Drawing.Point(0, 62);
            this.labelInterval.Name = "labelInterval";
            this.labelInterval.Size = new System.Drawing.Size(71, 13);
            this.labelInterval.TabIndex = 6;
            this.labelInterval.Text = "Interval (sec):";
            // 
            // dataGridViewData
            // 
            this.dataGridViewData.AllowDrop = true;
            this.dataGridViewData.AllowUserToAddRows = false;
            this.dataGridViewData.AllowUserToDeleteRows = false;
            this.dataGridViewData.AllowUserToOrderColumns = true;
            this.dataGridViewData.AllowUserToResizeRows = false;
            this.dataGridViewData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewData.CausesValidation = false;
            this.dataGridViewData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewData.EnableHeadersVisualStyles = false;
            this.dataGridViewData.Location = new System.Drawing.Point(3, 117);
            this.dataGridViewData.MultiSelect = false;
            this.dataGridViewData.Name = "dataGridViewData";
            this.dataGridViewData.ReadOnly = true;
            this.dataGridViewData.RowHeadersVisible = false;
            this.dataGridViewData.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridViewData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewData.ShowCellErrors = false;
            this.dataGridViewData.ShowCellToolTips = false;
            this.dataGridViewData.ShowEditingIcon = false;
            this.dataGridViewData.ShowRowErrors = false;
            this.dataGridViewData.Size = new System.Drawing.Size(742, 405);
            this.dataGridViewData.TabIndex = 27;
            this.dataGridViewData.Tag = "RT";
            this.dataGridViewData.DragEnter += new System.Windows.Forms.DragEventHandler(this.dataGridViewData_DragEnter);
            this.dataGridViewData.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridViewData_KeyDown);
            this.dataGridViewData.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGridViewData_DragDrop);
            // 
            // buttonAddField
            // 
            this.buttonAddField.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddField.ImageKey = "Symbol-Add.ico";
            this.buttonAddField.Location = new System.Drawing.Point(373, 32);
            this.buttonAddField.Name = "buttonAddField";
            this.buttonAddField.Size = new System.Drawing.Size(81, 23);
            this.buttonAddField.TabIndex = 5;
            this.buttonAddField.Tag = "RT";
            this.buttonAddField.Text = "Add";
            this.buttonAddField.UseVisualStyleBackColor = true;
            this.buttonAddField.Click += new System.EventHandler(this.buttonAddField_Click);
            // 
            // textBoxField
            // 
            this.textBoxField.AllowDrop = true;
            this.textBoxField.Location = new System.Drawing.Point(72, 34);
            this.textBoxField.Name = "textBoxField";
            this.textBoxField.Size = new System.Drawing.Size(294, 20);
            this.textBoxField.TabIndex = 4;
            this.textBoxField.Tag = "RT";
            this.textBoxField.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxField_KeyDown);
            // 
            // labelField
            // 
            this.labelField.AutoSize = true;
            this.labelField.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelField.Location = new System.Drawing.Point(38, 37);
            this.labelField.Name = "labelField";
            this.labelField.Size = new System.Drawing.Size(32, 13);
            this.labelField.TabIndex = 3;
            this.labelField.Text = "Field:";
            // 
            // buttonAddSecurity
            // 
            this.buttonAddSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddSecurity.ImageKey = "Symbol-Add.ico";
            this.buttonAddSecurity.Location = new System.Drawing.Point(373, 4);
            this.buttonAddSecurity.Name = "buttonAddSecurity";
            this.buttonAddSecurity.Size = new System.Drawing.Size(81, 23);
            this.buttonAddSecurity.TabIndex = 2;
            this.buttonAddSecurity.Tag = "RT";
            this.buttonAddSecurity.Text = "Add";
            this.buttonAddSecurity.UseVisualStyleBackColor = true;
            this.buttonAddSecurity.Click += new System.EventHandler(this.buttonAddSecurity_Click);
            // 
            // textBoxSecurity
            // 
            this.textBoxSecurity.AllowDrop = true;
            this.textBoxSecurity.Location = new System.Drawing.Point(72, 6);
            this.textBoxSecurity.Name = "textBoxSecurity";
            this.textBoxSecurity.Size = new System.Drawing.Size(294, 20);
            this.textBoxSecurity.TabIndex = 1;
            this.textBoxSecurity.Tag = "RT";
            this.textBoxSecurity.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSecurity_KeyDown);
            // 
            // labelSecurity
            // 
            this.labelSecurity.AutoSize = true;
            this.labelSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSecurity.Location = new System.Drawing.Point(22, 9);
            this.labelSecurity.Name = "labelSecurity";
            this.labelSecurity.Size = new System.Drawing.Size(48, 13);
            this.labelSecurity.TabIndex = 0;
            this.labelSecurity.Text = "Security:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 525);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(747, 22);
            this.statusStrip1.TabIndex = 28;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // buttonClearAll
            // 
            this.buttonClearAll.Enabled = false;
            this.buttonClearAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonClearAll.ImageIndex = 3;
            this.buttonClearAll.Location = new System.Drawing.Point(634, 32);
            this.buttonClearAll.Name = "buttonClearAll";
            this.buttonClearAll.Size = new System.Drawing.Size(81, 23);
            this.buttonClearAll.TabIndex = 16;
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
            this.buttonClearData.Location = new System.Drawing.Point(547, 32);
            this.buttonClearData.Name = "buttonClearData";
            this.buttonClearData.Size = new System.Drawing.Size(81, 23);
            this.buttonClearData.TabIndex = 15;
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
            this.buttonClearFields.Location = new System.Drawing.Point(460, 32);
            this.buttonClearFields.Name = "buttonClearFields";
            this.buttonClearFields.Size = new System.Drawing.Size(81, 23);
            this.buttonClearFields.TabIndex = 14;
            this.buttonClearFields.Tag = "RD";
            this.buttonClearFields.Text = "Clear Fields";
            this.buttonClearFields.UseVisualStyleBackColor = true;
            this.buttonClearFields.Click += new System.EventHandler(this.buttonClearFields_Click);
            // 
            // buttonSendRequest
            // 
            this.buttonSendRequest.Enabled = false;
            this.buttonSendRequest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSendRequest.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSendRequest.Location = new System.Drawing.Point(460, 4);
            this.buttonSendRequest.Name = "buttonSendRequest";
            this.buttonSendRequest.Size = new System.Drawing.Size(81, 23);
            this.buttonSendRequest.TabIndex = 12;
            this.buttonSendRequest.Tag = "RD";
            this.buttonSendRequest.Text = "Subscribe";
            this.buttonSendRequest.UseVisualStyleBackColor = true;
            this.buttonSendRequest.Click += new System.EventHandler(this.buttonSendRequest_Click);
            // 
            // buttonStopSubscribe
            // 
            this.buttonStopSubscribe.Enabled = false;
            this.buttonStopSubscribe.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStopSubscribe.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonStopSubscribe.Location = new System.Drawing.Point(547, 4);
            this.buttonStopSubscribe.Name = "buttonStopSubscribe";
            this.buttonStopSubscribe.Size = new System.Drawing.Size(81, 23);
            this.buttonStopSubscribe.TabIndex = 13;
            this.buttonStopSubscribe.Tag = "RD";
            this.buttonStopSubscribe.Text = "Stop";
            this.buttonStopSubscribe.UseVisualStyleBackColor = true;
            this.buttonStopSubscribe.Click += new System.EventHandler(this.buttonStopSubscribe_Click);
            // 
            // textBoxOutputFile
            // 
            this.textBoxOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutputFile.Location = new System.Drawing.Point(352, 60);
            this.textBoxOutputFile.Name = "textBoxOutputFile";
            this.textBoxOutputFile.ReadOnly = true;
            this.textBoxOutputFile.Size = new System.Drawing.Size(393, 20);
            this.textBoxOutputFile.TabIndex = 29;
            this.textBoxOutputFile.Visible = false;
            this.textBoxOutputFile.WordWrap = false;
            // 
            // labelUsageNote
            // 
            this.labelUsageNote.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.labelUsageNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelUsageNote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelUsageNote.Location = new System.Drawing.Point(3, 85);
            this.labelUsageNote.Name = "labelUsageNote";
            this.labelUsageNote.Size = new System.Drawing.Size(742, 29);
            this.labelUsageNote.TabIndex = 49;
            this.labelUsageNote.Text = resources.GetString("labelUsageNote.Text");
            this.labelUsageNote.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(747, 547);
            this.Controls.Add(this.labelUsageNote);
            this.Controls.Add(this.textBoxOutputFile);
            this.Controls.Add(this.buttonStopSubscribe);
            this.Controls.Add(this.buttonClearAll);
            this.Controls.Add(this.buttonClearData);
            this.Controls.Add(this.buttonClearFields);
            this.Controls.Add(this.buttonSendRequest);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.checkBoxForceDelay);
            this.Controls.Add(this.checkBoxOutputFile);
            this.Controls.Add(this.textBoxInterval);
            this.Controls.Add(this.labelInterval);
            this.Controls.Add(this.dataGridViewData);
            this.Controls.Add(this.buttonAddField);
            this.Controls.Add(this.textBoxField);
            this.Controls.Add(this.labelField);
            this.Controls.Add(this.buttonAddSecurity);
            this.Controls.Add(this.textBoxSecurity);
            this.Controls.Add(this.labelSecurity);
            this.MinimumSize = new System.Drawing.Size(755, 574);
            this.Name = "Form1";
            this.Text = "Simple Subscription Example";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxForceDelay;
        private System.Windows.Forms.CheckBox checkBoxOutputFile;
        private System.Windows.Forms.TextBox textBoxInterval;
        private System.Windows.Forms.Label labelInterval;
        private System.Windows.Forms.DataGridView dataGridViewData;
        private System.Windows.Forms.Button buttonAddField;
        private System.Windows.Forms.TextBox textBoxField;
        private System.Windows.Forms.Label labelField;
        private System.Windows.Forms.Button buttonAddSecurity;
        private System.Windows.Forms.TextBox textBoxSecurity;
        private System.Windows.Forms.Label labelSecurity;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button buttonClearAll;
        private System.Windows.Forms.Button buttonClearData;
        private System.Windows.Forms.Button buttonClearFields;
        private System.Windows.Forms.Button buttonSendRequest;
        private System.Windows.Forms.Button buttonStopSubscribe;
        private System.Windows.Forms.TextBox textBoxOutputFile;
        private System.Windows.Forms.Label labelUsageNote;
    }
}

