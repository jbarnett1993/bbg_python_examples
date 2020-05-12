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
            this.panelFieldSearchTop = new System.Windows.Forms.Panel();
            this.labelSearchType = new System.Windows.Forms.Label();
            this.comboBoxSearchType = new System.Windows.Forms.ComboBox();
            this.buttonSubmitSearch = new System.Windows.Forms.Button();
            this.groupBoxExcludeOptions = new System.Windows.Forms.GroupBox();
            this.comboBoxExcludeFieldType = new System.Windows.Forms.ComboBox();
            this.labelFieldType = new System.Windows.Forms.Label();
            this.comboBoxExcludeProductType = new System.Windows.Forms.ComboBox();
            this.labelExcludeProductType = new System.Windows.Forms.Label();
            this.groupBoxIncludOption = new System.Windows.Forms.GroupBox();
            this.comboBoxIncludeFieldType = new System.Windows.Forms.ComboBox();
            this.labelIncludeFieldType = new System.Windows.Forms.Label();
            this.comboBoxIncludeProductType = new System.Windows.Forms.ComboBox();
            this.labelIncludeProductType = new System.Windows.Forms.Label();
            this.textBoxSearchSpec = new System.Windows.Forms.TextBox();
            this.labelSearchSpec = new System.Windows.Forms.Label();
            this.dataGridViewDataView = new System.Windows.Forms.DataGridView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainerInfo = new System.Windows.Forms.SplitContainer();
            this.labelDocumentation = new System.Windows.Forms.Label();
            this.richTextBoxDocumentation = new System.Windows.Forms.RichTextBox();
            this.labelOverrides = new System.Windows.Forms.Label();
            this.dataGridViewOverrides = new System.Windows.Forms.DataGridView();
            this.panelFieldSearchTop.SuspendLayout();
            this.groupBoxExcludeOptions.SuspendLayout();
            this.groupBoxIncludOption.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDataView)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.splitContainerInfo.Panel1.SuspendLayout();
            this.splitContainerInfo.Panel2.SuspendLayout();
            this.splitContainerInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOverrides)).BeginInit();
            this.SuspendLayout();
            // 
            // panelFieldSearchTop
            // 
            this.panelFieldSearchTop.Controls.Add(this.labelSearchType);
            this.panelFieldSearchTop.Controls.Add(this.comboBoxSearchType);
            this.panelFieldSearchTop.Controls.Add(this.buttonSubmitSearch);
            this.panelFieldSearchTop.Controls.Add(this.groupBoxExcludeOptions);
            this.panelFieldSearchTop.Controls.Add(this.groupBoxIncludOption);
            this.panelFieldSearchTop.Controls.Add(this.textBoxSearchSpec);
            this.panelFieldSearchTop.Controls.Add(this.labelSearchSpec);
            this.panelFieldSearchTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFieldSearchTop.Location = new System.Drawing.Point(0, 0);
            this.panelFieldSearchTop.Name = "panelFieldSearchTop";
            this.panelFieldSearchTop.Size = new System.Drawing.Size(870, 93);
            this.panelFieldSearchTop.TabIndex = 1;
            // 
            // labelSearchType
            // 
            this.labelSearchType.AutoSize = true;
            this.labelSearchType.Location = new System.Drawing.Point(414, 9);
            this.labelSearchType.Name = "labelSearchType";
            this.labelSearchType.Size = new System.Drawing.Size(71, 13);
            this.labelSearchType.TabIndex = 2;
            this.labelSearchType.Text = "Search Type:";
            // 
            // comboBoxSearchType
            // 
            this.comboBoxSearchType.FormattingEnabled = true;
            this.comboBoxSearchType.ItemHeight = 13;
            this.comboBoxSearchType.Items.AddRange(new object[] {
            "Field Search",
            "Category Field Search"});
            this.comboBoxSearchType.Location = new System.Drawing.Point(487, 6);
            this.comboBoxSearchType.Name = "comboBoxSearchType";
            this.comboBoxSearchType.Size = new System.Drawing.Size(229, 21);
            this.comboBoxSearchType.TabIndex = 3;
            this.comboBoxSearchType.SelectedIndexChanged += new System.EventHandler(this.comboBoxSearchType_SelectedIndexChanged);
            // 
            // buttonSubmitSearch
            // 
            this.buttonSubmitSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSubmitSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSubmitSearch.ImageIndex = 7;
            this.buttonSubmitSearch.Location = new System.Drawing.Point(722, 4);
            this.buttonSubmitSearch.Name = "buttonSubmitSearch";
            this.buttonSubmitSearch.Size = new System.Drawing.Size(73, 23);
            this.buttonSubmitSearch.TabIndex = 4;
            this.buttonSubmitSearch.Tag = "FS";
            this.buttonSubmitSearch.Text = "Search";
            this.buttonSubmitSearch.UseVisualStyleBackColor = true;
            this.buttonSubmitSearch.Click += new System.EventHandler(this.buttonSubmitSearch_Click);
            // 
            // groupBoxExcludeOptions
            // 
            this.groupBoxExcludeOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxExcludeOptions.Controls.Add(this.comboBoxExcludeFieldType);
            this.groupBoxExcludeOptions.Controls.Add(this.labelFieldType);
            this.groupBoxExcludeOptions.Controls.Add(this.comboBoxExcludeProductType);
            this.groupBoxExcludeOptions.Controls.Add(this.labelExcludeProductType);
            this.groupBoxExcludeOptions.Location = new System.Drawing.Point(385, 30);
            this.groupBoxExcludeOptions.MinimumSize = new System.Drawing.Size(262, 55);
            this.groupBoxExcludeOptions.Name = "groupBoxExcludeOptions";
            this.groupBoxExcludeOptions.Size = new System.Drawing.Size(482, 55);
            this.groupBoxExcludeOptions.TabIndex = 6;
            this.groupBoxExcludeOptions.TabStop = false;
            this.groupBoxExcludeOptions.Text = "Exclude Options";
            // 
            // comboBoxExcludeFieldType
            // 
            this.comboBoxExcludeFieldType.FormattingEnabled = true;
            this.comboBoxExcludeFieldType.Items.AddRange(new object[] {
            "None",
            "All",
            "RealTime",
            "Static"});
            this.comboBoxExcludeFieldType.Location = new System.Drawing.Point(254, 19);
            this.comboBoxExcludeFieldType.Name = "comboBoxExcludeFieldType";
            this.comboBoxExcludeFieldType.Size = new System.Drawing.Size(107, 21);
            this.comboBoxExcludeFieldType.TabIndex = 11;
            // 
            // labelFieldType
            // 
            this.labelFieldType.AutoSize = true;
            this.labelFieldType.Location = new System.Drawing.Point(193, 22);
            this.labelFieldType.Name = "labelFieldType";
            this.labelFieldType.Size = new System.Drawing.Size(59, 13);
            this.labelFieldType.TabIndex = 10;
            this.labelFieldType.Text = "Field Type:";
            // 
            // comboBoxExcludeProductType
            // 
            this.comboBoxExcludeProductType.FormattingEnabled = true;
            this.comboBoxExcludeProductType.Items.AddRange(new object[] {
            "None",
            "All",
            "Govt",
            "Corp",
            "Mtge",
            "M-Mkt",
            "Muni",
            "Pfd",
            "Equity",
            "Cmdty",
            "Index",
            "Curncy"});
            this.comboBoxExcludeProductType.Location = new System.Drawing.Point(82, 19);
            this.comboBoxExcludeProductType.Name = "comboBoxExcludeProductType";
            this.comboBoxExcludeProductType.Size = new System.Drawing.Size(107, 21);
            this.comboBoxExcludeProductType.TabIndex = 9;
            // 
            // labelExcludeProductType
            // 
            this.labelExcludeProductType.AutoSize = true;
            this.labelExcludeProductType.Location = new System.Drawing.Point(6, 22);
            this.labelExcludeProductType.Name = "labelExcludeProductType";
            this.labelExcludeProductType.Size = new System.Drawing.Size(74, 13);
            this.labelExcludeProductType.TabIndex = 8;
            this.labelExcludeProductType.Text = "Product Type:";
            // 
            // groupBoxIncludOption
            // 
            this.groupBoxIncludOption.Controls.Add(this.comboBoxIncludeFieldType);
            this.groupBoxIncludOption.Controls.Add(this.labelIncludeFieldType);
            this.groupBoxIncludOption.Controls.Add(this.comboBoxIncludeProductType);
            this.groupBoxIncludOption.Controls.Add(this.labelIncludeProductType);
            this.groupBoxIncludOption.Location = new System.Drawing.Point(6, 30);
            this.groupBoxIncludOption.Name = "groupBoxIncludOption";
            this.groupBoxIncludOption.Size = new System.Drawing.Size(373, 55);
            this.groupBoxIncludOption.TabIndex = 5;
            this.groupBoxIncludOption.TabStop = false;
            this.groupBoxIncludOption.Text = "Include Options";
            // 
            // comboBoxIncludeFieldType
            // 
            this.comboBoxIncludeFieldType.FormattingEnabled = true;
            this.comboBoxIncludeFieldType.Items.AddRange(new object[] {
            "None",
            "All",
            "RealTime",
            "Static"});
            this.comboBoxIncludeFieldType.Location = new System.Drawing.Point(254, 19);
            this.comboBoxIncludeFieldType.Name = "comboBoxIncludeFieldType";
            this.comboBoxIncludeFieldType.Size = new System.Drawing.Size(107, 21);
            this.comboBoxIncludeFieldType.TabIndex = 8;
            // 
            // labelIncludeFieldType
            // 
            this.labelIncludeFieldType.AutoSize = true;
            this.labelIncludeFieldType.Location = new System.Drawing.Point(193, 22);
            this.labelIncludeFieldType.Name = "labelIncludeFieldType";
            this.labelIncludeFieldType.Size = new System.Drawing.Size(59, 13);
            this.labelIncludeFieldType.TabIndex = 7;
            this.labelIncludeFieldType.Text = "Field Type:";
            // 
            // comboBoxIncludeProductType
            // 
            this.comboBoxIncludeProductType.FormattingEnabled = true;
            this.comboBoxIncludeProductType.Items.AddRange(new object[] {
            "None",
            "All",
            "Govt",
            "Corp",
            "Mtge",
            "M-Mkt",
            "Muni",
            "Pfd",
            "Equity",
            "Cmdty",
            "Index",
            "Curncy"});
            this.comboBoxIncludeProductType.Location = new System.Drawing.Point(83, 19);
            this.comboBoxIncludeProductType.Name = "comboBoxIncludeProductType";
            this.comboBoxIncludeProductType.Size = new System.Drawing.Size(107, 21);
            this.comboBoxIncludeProductType.TabIndex = 6;
            // 
            // labelIncludeProductType
            // 
            this.labelIncludeProductType.AutoSize = true;
            this.labelIncludeProductType.Location = new System.Drawing.Point(7, 22);
            this.labelIncludeProductType.Name = "labelIncludeProductType";
            this.labelIncludeProductType.Size = new System.Drawing.Size(74, 13);
            this.labelIncludeProductType.TabIndex = 5;
            this.labelIncludeProductType.Text = "Product Type:";
            // 
            // textBoxSearchSpec
            // 
            this.textBoxSearchSpec.Location = new System.Drawing.Point(77, 6);
            this.textBoxSearchSpec.Name = "textBoxSearchSpec";
            this.textBoxSearchSpec.Size = new System.Drawing.Size(333, 20);
            this.textBoxSearchSpec.TabIndex = 1;
            // 
            // labelSearchSpec
            // 
            this.labelSearchSpec.AutoSize = true;
            this.labelSearchSpec.Location = new System.Drawing.Point(4, 9);
            this.labelSearchSpec.Name = "labelSearchSpec";
            this.labelSearchSpec.Size = new System.Drawing.Size(72, 13);
            this.labelSearchSpec.TabIndex = 0;
            this.labelSearchSpec.Text = "Search Spec:";
            // 
            // dataGridViewDataView
            // 
            this.dataGridViewDataView.AllowUserToAddRows = false;
            this.dataGridViewDataView.AllowUserToDeleteRows = false;
            this.dataGridViewDataView.AllowUserToResizeRows = false;
            this.dataGridViewDataView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDataView.Dock = System.Windows.Forms.DockStyle.Top;
            this.dataGridViewDataView.Location = new System.Drawing.Point(0, 93);
            this.dataGridViewDataView.Name = "dataGridViewDataView";
            this.dataGridViewDataView.ReadOnly = true;
            this.dataGridViewDataView.RowHeadersVisible = false;
            this.dataGridViewDataView.Size = new System.Drawing.Size(870, 265);
            this.dataGridViewDataView.TabIndex = 2;
            this.dataGridViewDataView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewDataView_RowEnter);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 504);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(870, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(109, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // splitContainerInfo
            // 
            this.splitContainerInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerInfo.Location = new System.Drawing.Point(0, 358);
            this.splitContainerInfo.Name = "splitContainerInfo";
            // 
            // splitContainerInfo.Panel1
            // 
            this.splitContainerInfo.Panel1.Controls.Add(this.labelDocumentation);
            this.splitContainerInfo.Panel1.Controls.Add(this.richTextBoxDocumentation);
            // 
            // splitContainerInfo.Panel2
            // 
            this.splitContainerInfo.Panel2.Controls.Add(this.labelOverrides);
            this.splitContainerInfo.Panel2.Controls.Add(this.dataGridViewOverrides);
            this.splitContainerInfo.Size = new System.Drawing.Size(870, 146);
            this.splitContainerInfo.SplitterDistance = 393;
            this.splitContainerInfo.TabIndex = 5;
            // 
            // labelDocumentation
            // 
            this.labelDocumentation.AutoSize = true;
            this.labelDocumentation.Location = new System.Drawing.Point(0, 3);
            this.labelDocumentation.Name = "labelDocumentation";
            this.labelDocumentation.Size = new System.Drawing.Size(82, 13);
            this.labelDocumentation.TabIndex = 1;
            this.labelDocumentation.Text = "Documentation:";
            // 
            // richTextBoxDocumentation
            // 
            this.richTextBoxDocumentation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxDocumentation.Location = new System.Drawing.Point(3, 18);
            this.richTextBoxDocumentation.Name = "richTextBoxDocumentation";
            this.richTextBoxDocumentation.Size = new System.Drawing.Size(387, 125);
            this.richTextBoxDocumentation.TabIndex = 0;
            this.richTextBoxDocumentation.Tag = "0";
            this.richTextBoxDocumentation.Text = "";
            // 
            // labelOverrides
            // 
            this.labelOverrides.AutoSize = true;
            this.labelOverrides.Location = new System.Drawing.Point(3, 3);
            this.labelOverrides.Name = "labelOverrides";
            this.labelOverrides.Size = new System.Drawing.Size(55, 13);
            this.labelOverrides.TabIndex = 2;
            this.labelOverrides.Text = "Overrides:";
            // 
            // dataGridViewOverrides
            // 
            this.dataGridViewOverrides.AllowUserToAddRows = false;
            this.dataGridViewOverrides.AllowUserToDeleteRows = false;
            this.dataGridViewOverrides.AllowUserToOrderColumns = true;
            this.dataGridViewOverrides.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewOverrides.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOverrides.Location = new System.Drawing.Point(6, 18);
            this.dataGridViewOverrides.Name = "dataGridViewOverrides";
            this.dataGridViewOverrides.ReadOnly = true;
            this.dataGridViewOverrides.RowHeadersVisible = false;
            this.dataGridViewOverrides.Size = new System.Drawing.Size(464, 125);
            this.dataGridViewOverrides.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(870, 526);
            this.Controls.Add(this.splitContainerInfo);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.dataGridViewDataView);
            this.Controls.Add(this.panelFieldSearchTop);
            this.MinimumSize = new System.Drawing.Size(878, 553);
            this.Name = "Form1";
            this.Text = "Simple Field Search Example";
            this.panelFieldSearchTop.ResumeLayout(false);
            this.panelFieldSearchTop.PerformLayout();
            this.groupBoxExcludeOptions.ResumeLayout(false);
            this.groupBoxExcludeOptions.PerformLayout();
            this.groupBoxIncludOption.ResumeLayout(false);
            this.groupBoxIncludOption.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDataView)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainerInfo.Panel1.ResumeLayout(false);
            this.splitContainerInfo.Panel1.PerformLayout();
            this.splitContainerInfo.Panel2.ResumeLayout(false);
            this.splitContainerInfo.Panel2.PerformLayout();
            this.splitContainerInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOverrides)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelFieldSearchTop;
        private System.Windows.Forms.Button buttonSubmitSearch;
        private System.Windows.Forms.GroupBox groupBoxExcludeOptions;
        private System.Windows.Forms.ComboBox comboBoxExcludeFieldType;
        private System.Windows.Forms.Label labelFieldType;
        private System.Windows.Forms.ComboBox comboBoxExcludeProductType;
        private System.Windows.Forms.Label labelExcludeProductType;
        private System.Windows.Forms.GroupBox groupBoxIncludOption;
        private System.Windows.Forms.ComboBox comboBoxIncludeFieldType;
        private System.Windows.Forms.Label labelIncludeFieldType;
        private System.Windows.Forms.ComboBox comboBoxIncludeProductType;
        private System.Windows.Forms.Label labelIncludeProductType;
        private System.Windows.Forms.TextBox textBoxSearchSpec;
        private System.Windows.Forms.Label labelSearchSpec;
        private System.Windows.Forms.DataGridView dataGridViewDataView;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer splitContainerInfo;
        private System.Windows.Forms.Label labelDocumentation;
        private System.Windows.Forms.RichTextBox richTextBoxDocumentation;
        private System.Windows.Forms.Label labelOverrides;
        private System.Windows.Forms.DataGridView dataGridViewOverrides;
        private System.Windows.Forms.ComboBox comboBoxSearchType;
        private System.Windows.Forms.Label labelSearchType;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}

