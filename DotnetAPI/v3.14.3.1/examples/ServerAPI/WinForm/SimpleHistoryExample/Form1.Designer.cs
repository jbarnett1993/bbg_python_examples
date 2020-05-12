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
            this.buttonAddFields = new System.Windows.Forms.Button();
            this.textBoxField = new System.Windows.Forms.TextBox();
            this.labelField = new System.Windows.Forms.Label();
            this.textBoxSecurity = new System.Windows.Forms.TextBox();
            this.labelSecurity = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxNonTradingDayValue = new System.Windows.Forms.ComboBox();
            this.labelNonTradingDayValue = new System.Windows.Forms.Label();
            this.comboBoxOverrideOption = new System.Windows.Forms.ComboBox();
            this.labelOverrideOption = new System.Windows.Forms.Label();
            this.comboBoxPricing = new System.Windows.Forms.ComboBox();
            this.labelPricing = new System.Windows.Forms.Label();
            this.tabControlDates = new System.Windows.Forms.TabControl();
            this.tabPageDate = new System.Windows.Forms.TabPage();
            this.dateTimePickerEndDate = new System.Windows.Forms.DateTimePicker();
            this.labelEndDate = new System.Windows.Forms.Label();
            this.dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this.labelStartDate = new System.Windows.Forms.Label();
            this.tabPageRelativeDate = new System.Windows.Forms.TabPage();
            this.textBoxRelEndDate = new System.Windows.Forms.TextBox();
            this.textBoxRelStartDate = new System.Windows.Forms.TextBox();
            this.labelRelEndDate = new System.Windows.Forms.Label();
            this.labelRelStartDate = new System.Windows.Forms.Label();
            this.comboBoxPeriodicitySelection = new System.Windows.Forms.ComboBox();
            this.labelPeriodicitySelection = new System.Windows.Forms.Label();
            this.comboBoxNonTradingDayMethod = new System.Windows.Forms.ComboBox();
            this.labelNonTradingDayMethod = new System.Windows.Forms.Label();
            this.comboBoxPeriodicityAdjustment = new System.Windows.Forms.ComboBox();
            this.labelPeridicityAdjustment = new System.Windows.Forms.Label();
            this.textBoxMaxPoints = new System.Windows.Forms.TextBox();
            this.labelMaxPoint = new System.Windows.Forms.Label();
            this.textBoxCurrencyCode = new System.Windows.Forms.TextBox();
            this.labelCurrencyCode = new System.Windows.Forms.Label();
            this.dataGridViewData = new System.Windows.Forms.DataGridView();
            this.buttonClearFields = new System.Windows.Forms.Button();
            this.buttonSendRequest = new System.Windows.Forms.Button();
            this.radioButtonAsynch = new System.Windows.Forms.RadioButton();
            this.radioButtonSynch = new System.Windows.Forms.RadioButton();
            this.buttonClearData = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.labelUsageNote = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.tabControlDates.SuspendLayout();
            this.tabPageDate.SuspendLayout();
            this.tabPageRelativeDate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonAddFields
            // 
            this.buttonAddFields.Enabled = false;
            this.buttonAddFields.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddFields.Location = new System.Drawing.Point(360, 35);
            this.buttonAddFields.Name = "buttonAddFields";
            this.buttonAddFields.Size = new System.Drawing.Size(81, 23);
            this.buttonAddFields.TabIndex = 12;
            this.buttonAddFields.Tag = "HD";
            this.buttonAddFields.Text = "Add";
            this.buttonAddFields.UseVisualStyleBackColor = true;
            this.buttonAddFields.Click += new System.EventHandler(this.buttonAddFields_Click);
            // 
            // textBoxField
            // 
            this.textBoxField.Enabled = false;
            this.textBoxField.Location = new System.Drawing.Point(60, 37);
            this.textBoxField.Name = "textBoxField";
            this.textBoxField.Size = new System.Drawing.Size(294, 20);
            this.textBoxField.TabIndex = 11;
            this.textBoxField.Tag = "HD";
            this.textBoxField.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxField_KeyDown);
            // 
            // labelField
            // 
            this.labelField.AutoSize = true;
            this.labelField.Enabled = false;
            this.labelField.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelField.Location = new System.Drawing.Point(26, 40);
            this.labelField.Name = "labelField";
            this.labelField.Size = new System.Drawing.Size(32, 13);
            this.labelField.TabIndex = 10;
            this.labelField.Text = "Field:";
            // 
            // textBoxSecurity
            // 
            this.textBoxSecurity.AllowDrop = true;
            this.textBoxSecurity.Location = new System.Drawing.Point(60, 9);
            this.textBoxSecurity.Name = "textBoxSecurity";
            this.textBoxSecurity.Size = new System.Drawing.Size(294, 20);
            this.textBoxSecurity.TabIndex = 8;
            this.textBoxSecurity.Tag = "HD";
            this.textBoxSecurity.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSecurity_KeyDown);
            // 
            // labelSecurity
            // 
            this.labelSecurity.AutoSize = true;
            this.labelSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSecurity.Location = new System.Drawing.Point(10, 12);
            this.labelSecurity.Name = "labelSecurity";
            this.labelSecurity.Size = new System.Drawing.Size(48, 13);
            this.labelSecurity.TabIndex = 7;
            this.labelSecurity.Text = "Security:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBoxNonTradingDayValue);
            this.groupBox1.Controls.Add(this.labelNonTradingDayValue);
            this.groupBox1.Controls.Add(this.comboBoxOverrideOption);
            this.groupBox1.Controls.Add(this.labelOverrideOption);
            this.groupBox1.Controls.Add(this.comboBoxPricing);
            this.groupBox1.Controls.Add(this.labelPricing);
            this.groupBox1.Controls.Add(this.tabControlDates);
            this.groupBox1.Controls.Add(this.comboBoxPeriodicitySelection);
            this.groupBox1.Controls.Add(this.labelPeriodicitySelection);
            this.groupBox1.Controls.Add(this.comboBoxNonTradingDayMethod);
            this.groupBox1.Controls.Add(this.labelNonTradingDayMethod);
            this.groupBox1.Controls.Add(this.comboBoxPeriodicityAdjustment);
            this.groupBox1.Controls.Add(this.labelPeridicityAdjustment);
            this.groupBox1.Controls.Add(this.textBoxMaxPoints);
            this.groupBox1.Controls.Add(this.labelMaxPoint);
            this.groupBox1.Controls.Add(this.textBoxCurrencyCode);
            this.groupBox1.Controls.Add(this.labelCurrencyCode);
            this.groupBox1.Location = new System.Drawing.Point(12, 63);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(231, 378);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // comboBoxNonTradingDayValue
            // 
            this.comboBoxNonTradingDayValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxNonTradingDayValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNonTradingDayValue.FormattingEnabled = true;
            this.comboBoxNonTradingDayValue.Items.AddRange(new object[] {
            "Weekdays",
            "ALL Calender Days",
            "Active Days Only"});
            this.comboBoxNonTradingDayValue.Location = new System.Drawing.Point(97, 115);
            this.comboBoxNonTradingDayValue.Name = "comboBoxNonTradingDayValue";
            this.comboBoxNonTradingDayValue.Size = new System.Drawing.Size(125, 21);
            this.comboBoxNonTradingDayValue.TabIndex = 37;
            this.comboBoxNonTradingDayValue.Tag = "HD";
            // 
            // labelNonTradingDayValue
            // 
            this.labelNonTradingDayValue.Location = new System.Drawing.Point(21, 111);
            this.labelNonTradingDayValue.Name = "labelNonTradingDayValue";
            this.labelNonTradingDayValue.Size = new System.Drawing.Size(67, 28);
            this.labelNonTradingDayValue.TabIndex = 36;
            this.labelNonTradingDayValue.Text = "Non Trading Day Value:";
            // 
            // comboBoxOverrideOption
            // 
            this.comboBoxOverrideOption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxOverrideOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOverrideOption.FormattingEnabled = true;
            this.comboBoxOverrideOption.Items.AddRange(new object[] {
            "CLOSE",
            "GPA"});
            this.comboBoxOverrideOption.Location = new System.Drawing.Point(97, 196);
            this.comboBoxOverrideOption.Name = "comboBoxOverrideOption";
            this.comboBoxOverrideOption.Size = new System.Drawing.Size(125, 21);
            this.comboBoxOverrideOption.TabIndex = 43;
            this.comboBoxOverrideOption.Tag = "HD";
            // 
            // labelOverrideOption
            // 
            this.labelOverrideOption.AutoSize = true;
            this.labelOverrideOption.Location = new System.Drawing.Point(7, 199);
            this.labelOverrideOption.Name = "labelOverrideOption";
            this.labelOverrideOption.Size = new System.Drawing.Size(84, 13);
            this.labelOverrideOption.TabIndex = 42;
            this.labelOverrideOption.Text = "Override Option:";
            // 
            // comboBoxPricing
            // 
            this.comboBoxPricing.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPricing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPricing.FormattingEnabled = true;
            this.comboBoxPricing.Items.AddRange(new object[] {
            "PRICE",
            "YIELD"});
            this.comboBoxPricing.Location = new System.Drawing.Point(97, 169);
            this.comboBoxPricing.Name = "comboBoxPricing";
            this.comboBoxPricing.Size = new System.Drawing.Size(125, 21);
            this.comboBoxPricing.TabIndex = 41;
            this.comboBoxPricing.Tag = "HD";
            // 
            // labelPricing
            // 
            this.labelPricing.AutoSize = true;
            this.labelPricing.Location = new System.Drawing.Point(49, 172);
            this.labelPricing.Name = "labelPricing";
            this.labelPricing.Size = new System.Drawing.Size(42, 13);
            this.labelPricing.TabIndex = 40;
            this.labelPricing.Text = "Pricing:";
            // 
            // tabControlDates
            // 
            this.tabControlDates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlDates.Controls.Add(this.tabPageDate);
            this.tabControlDates.Controls.Add(this.tabPageRelativeDate);
            this.tabControlDates.Location = new System.Drawing.Point(11, 227);
            this.tabControlDates.Name = "tabControlDates";
            this.tabControlDates.SelectedIndex = 0;
            this.tabControlDates.Size = new System.Drawing.Size(212, 140);
            this.tabControlDates.TabIndex = 44;
            // 
            // tabPageDate
            // 
            this.tabPageDate.Controls.Add(this.dateTimePickerEndDate);
            this.tabPageDate.Controls.Add(this.labelEndDate);
            this.tabPageDate.Controls.Add(this.dateTimePickerStart);
            this.tabPageDate.Controls.Add(this.labelStartDate);
            this.tabPageDate.Location = new System.Drawing.Point(4, 22);
            this.tabPageDate.Name = "tabPageDate";
            this.tabPageDate.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDate.Size = new System.Drawing.Size(204, 114);
            this.tabPageDate.TabIndex = 0;
            this.tabPageDate.Text = "Actual Dates";
            this.tabPageDate.UseVisualStyleBackColor = true;
            // 
            // dateTimePickerEndDate
            // 
            this.dateTimePickerEndDate.Location = new System.Drawing.Point(9, 70);
            this.dateTimePickerEndDate.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerEndDate.Name = "dateTimePickerEndDate";
            this.dateTimePickerEndDate.Size = new System.Drawing.Size(187, 20);
            this.dateTimePickerEndDate.TabIndex = 31;
            this.dateTimePickerEndDate.Tag = "HD";
            // 
            // labelEndDate
            // 
            this.labelEndDate.AutoSize = true;
            this.labelEndDate.Location = new System.Drawing.Point(6, 54);
            this.labelEndDate.Name = "labelEndDate";
            this.labelEndDate.Size = new System.Drawing.Size(55, 13);
            this.labelEndDate.TabIndex = 30;
            this.labelEndDate.Text = "End Date:";
            // 
            // dateTimePickerStart
            // 
            this.dateTimePickerStart.Location = new System.Drawing.Point(9, 28);
            this.dateTimePickerStart.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            this.dateTimePickerStart.Size = new System.Drawing.Size(187, 20);
            this.dateTimePickerStart.TabIndex = 29;
            this.dateTimePickerStart.Tag = "HD";
            // 
            // labelStartDate
            // 
            this.labelStartDate.AutoSize = true;
            this.labelStartDate.Location = new System.Drawing.Point(6, 12);
            this.labelStartDate.Name = "labelStartDate";
            this.labelStartDate.Size = new System.Drawing.Size(58, 13);
            this.labelStartDate.TabIndex = 28;
            this.labelStartDate.Text = "Start Date:";
            // 
            // tabPageRelativeDate
            // 
            this.tabPageRelativeDate.Controls.Add(this.textBoxRelEndDate);
            this.tabPageRelativeDate.Controls.Add(this.textBoxRelStartDate);
            this.tabPageRelativeDate.Controls.Add(this.labelRelEndDate);
            this.tabPageRelativeDate.Controls.Add(this.labelRelStartDate);
            this.tabPageRelativeDate.Location = new System.Drawing.Point(4, 22);
            this.tabPageRelativeDate.Name = "tabPageRelativeDate";
            this.tabPageRelativeDate.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRelativeDate.Size = new System.Drawing.Size(204, 114);
            this.tabPageRelativeDate.TabIndex = 1;
            this.tabPageRelativeDate.Text = "Relative Dates";
            this.tabPageRelativeDate.UseVisualStyleBackColor = true;
            // 
            // textBoxRelEndDate
            // 
            this.textBoxRelEndDate.Location = new System.Drawing.Point(12, 70);
            this.textBoxRelEndDate.Name = "textBoxRelEndDate";
            this.textBoxRelEndDate.Size = new System.Drawing.Size(184, 20);
            this.textBoxRelEndDate.TabIndex = 35;
            this.textBoxRelEndDate.Tag = "HD";
            this.textBoxRelEndDate.Text = "-1CQ";
            // 
            // textBoxRelStartDate
            // 
            this.textBoxRelStartDate.Location = new System.Drawing.Point(9, 31);
            this.textBoxRelStartDate.Name = "textBoxRelStartDate";
            this.textBoxRelStartDate.Size = new System.Drawing.Size(187, 20);
            this.textBoxRelStartDate.TabIndex = 33;
            this.textBoxRelStartDate.Tag = "HD";
            this.textBoxRelStartDate.Text = "ED-6CQ";
            // 
            // labelRelEndDate
            // 
            this.labelRelEndDate.AutoSize = true;
            this.labelRelEndDate.Location = new System.Drawing.Point(6, 54);
            this.labelRelEndDate.Name = "labelRelEndDate";
            this.labelRelEndDate.Size = new System.Drawing.Size(55, 13);
            this.labelRelEndDate.TabIndex = 34;
            this.labelRelEndDate.Text = "End Date:";
            // 
            // labelRelStartDate
            // 
            this.labelRelStartDate.AutoSize = true;
            this.labelRelStartDate.Location = new System.Drawing.Point(6, 12);
            this.labelRelStartDate.Name = "labelRelStartDate";
            this.labelRelStartDate.Size = new System.Drawing.Size(58, 13);
            this.labelRelStartDate.TabIndex = 32;
            this.labelRelStartDate.Text = "Start Date:";
            // 
            // comboBoxPeriodicitySelection
            // 
            this.comboBoxPeriodicitySelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPeriodicitySelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPeriodicitySelection.FormattingEnabled = true;
            this.comboBoxPeriodicitySelection.Location = new System.Drawing.Point(97, 88);
            this.comboBoxPeriodicitySelection.Name = "comboBoxPeriodicitySelection";
            this.comboBoxPeriodicitySelection.Size = new System.Drawing.Size(125, 21);
            this.comboBoxPeriodicitySelection.TabIndex = 35;
            this.comboBoxPeriodicitySelection.Tag = "HD";
            // 
            // labelPeriodicitySelection
            // 
            this.labelPeriodicitySelection.AutoSize = true;
            this.labelPeriodicitySelection.Location = new System.Drawing.Point(13, 91);
            this.labelPeriodicitySelection.Name = "labelPeriodicitySelection";
            this.labelPeriodicitySelection.Size = new System.Drawing.Size(79, 13);
            this.labelPeriodicitySelection.TabIndex = 34;
            this.labelPeriodicitySelection.Text = "Periodicity Sel.:";
            // 
            // comboBoxNonTradingDayMethod
            // 
            this.comboBoxNonTradingDayMethod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxNonTradingDayMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNonTradingDayMethod.FormattingEnabled = true;
            this.comboBoxNonTradingDayMethod.Items.AddRange(new object[] {
            "Blank",
            "Previous"});
            this.comboBoxNonTradingDayMethod.Location = new System.Drawing.Point(97, 142);
            this.comboBoxNonTradingDayMethod.Name = "comboBoxNonTradingDayMethod";
            this.comboBoxNonTradingDayMethod.Size = new System.Drawing.Size(125, 21);
            this.comboBoxNonTradingDayMethod.TabIndex = 39;
            this.comboBoxNonTradingDayMethod.Tag = "HD";
            // 
            // labelNonTradingDayMethod
            // 
            this.labelNonTradingDayMethod.Location = new System.Drawing.Point(21, 139);
            this.labelNonTradingDayMethod.Name = "labelNonTradingDayMethod";
            this.labelNonTradingDayMethod.Size = new System.Drawing.Size(75, 28);
            this.labelNonTradingDayMethod.TabIndex = 38;
            this.labelNonTradingDayMethod.Text = "Non Trading Day Method:";
            // 
            // comboBoxPeriodicityAdjustment
            // 
            this.comboBoxPeriodicityAdjustment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPeriodicityAdjustment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPeriodicityAdjustment.FormattingEnabled = true;
            this.comboBoxPeriodicityAdjustment.Items.AddRange(new object[] {
            "ACTUAL",
            "CALENDAR",
            "FISCAL"});
            this.comboBoxPeriodicityAdjustment.Location = new System.Drawing.Point(97, 61);
            this.comboBoxPeriodicityAdjustment.Name = "comboBoxPeriodicityAdjustment";
            this.comboBoxPeriodicityAdjustment.Size = new System.Drawing.Size(125, 21);
            this.comboBoxPeriodicityAdjustment.TabIndex = 33;
            this.comboBoxPeriodicityAdjustment.Tag = "HD";
            this.comboBoxPeriodicityAdjustment.SelectedIndexChanged += new System.EventHandler(this.comboBoxPeriodicityAdjustment_SelectedIndexChanged);
            // 
            // labelPeridicityAdjustment
            // 
            this.labelPeridicityAdjustment.AutoSize = true;
            this.labelPeridicityAdjustment.Location = new System.Drawing.Point(13, 64);
            this.labelPeridicityAdjustment.Name = "labelPeridicityAdjustment";
            this.labelPeridicityAdjustment.Size = new System.Drawing.Size(79, 13);
            this.labelPeridicityAdjustment.TabIndex = 32;
            this.labelPeridicityAdjustment.Text = "Periodicity Adj.:";
            // 
            // textBoxMaxPoints
            // 
            this.textBoxMaxPoints.Location = new System.Drawing.Point(97, 36);
            this.textBoxMaxPoints.MaxLength = 5;
            this.textBoxMaxPoints.Name = "textBoxMaxPoints";
            this.textBoxMaxPoints.Size = new System.Drawing.Size(48, 20);
            this.textBoxMaxPoints.TabIndex = 31;
            this.textBoxMaxPoints.Tag = "HD";
            this.textBoxMaxPoints.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxMaxPoints_KeyDown);
            // 
            // labelMaxPoint
            // 
            this.labelMaxPoint.AutoSize = true;
            this.labelMaxPoint.Location = new System.Drawing.Point(27, 39);
            this.labelMaxPoint.Name = "labelMaxPoint";
            this.labelMaxPoint.Size = new System.Drawing.Size(65, 13);
            this.labelMaxPoint.TabIndex = 29;
            this.labelMaxPoint.Text = "Max. Points:";
            // 
            // textBoxCurrencyCode
            // 
            this.textBoxCurrencyCode.Location = new System.Drawing.Point(97, 11);
            this.textBoxCurrencyCode.MaxLength = 3;
            this.textBoxCurrencyCode.Name = "textBoxCurrencyCode";
            this.textBoxCurrencyCode.Size = new System.Drawing.Size(48, 20);
            this.textBoxCurrencyCode.TabIndex = 30;
            this.textBoxCurrencyCode.Tag = "HD";
            this.textBoxCurrencyCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxCurrencyCode_KeyDown);
            // 
            // labelCurrencyCode
            // 
            this.labelCurrencyCode.AutoSize = true;
            this.labelCurrencyCode.Location = new System.Drawing.Point(12, 14);
            this.labelCurrencyCode.Name = "labelCurrencyCode";
            this.labelCurrencyCode.Size = new System.Drawing.Size(80, 13);
            this.labelCurrencyCode.TabIndex = 28;
            this.labelCurrencyCode.Text = "Currency Code:";
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
            this.dataGridViewData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewData.Location = new System.Drawing.Point(249, 93);
            this.dataGridViewData.MultiSelect = false;
            this.dataGridViewData.Name = "dataGridViewData";
            this.dataGridViewData.ReadOnly = true;
            this.dataGridViewData.RowHeadersVisible = false;
            this.dataGridViewData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewData.Size = new System.Drawing.Size(498, 348);
            this.dataGridViewData.TabIndex = 40;
            this.dataGridViewData.Tag = "HD";
            this.dataGridViewData.DragEnter += new System.Windows.Forms.DragEventHandler(this.dataGridViewView_DragEnter);
            this.dataGridViewData.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridViewData_KeyDown);
            this.dataGridViewData.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGridViewView_DragDrop);
            // 
            // buttonClearFields
            // 
            this.buttonClearFields.Enabled = false;
            this.buttonClearFields.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearFields.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonClearFields.ImageIndex = 2;
            this.buttonClearFields.Location = new System.Drawing.Point(447, 7);
            this.buttonClearFields.Name = "buttonClearFields";
            this.buttonClearFields.Size = new System.Drawing.Size(81, 23);
            this.buttonClearFields.TabIndex = 37;
            this.buttonClearFields.Tag = "HD";
            this.buttonClearFields.Text = "Clear Fields";
            this.buttonClearFields.UseVisualStyleBackColor = true;
            this.buttonClearFields.Click += new System.EventHandler(this.buttonClearFields_Click);
            // 
            // buttonSendRequest
            // 
            this.buttonSendRequest.Enabled = false;
            this.buttonSendRequest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSendRequest.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSendRequest.Location = new System.Drawing.Point(360, 7);
            this.buttonSendRequest.Name = "buttonSendRequest";
            this.buttonSendRequest.Size = new System.Drawing.Size(81, 23);
            this.buttonSendRequest.TabIndex = 36;
            this.buttonSendRequest.Tag = "HD";
            this.buttonSendRequest.Text = "Submit";
            this.buttonSendRequest.UseVisualStyleBackColor = true;
            this.buttonSendRequest.Click += new System.EventHandler(this.buttonSendRequest_Click);
            // 
            // radioButtonAsynch
            // 
            this.radioButtonAsynch.AutoSize = true;
            this.radioButtonAsynch.Checked = true;
            this.radioButtonAsynch.Location = new System.Drawing.Point(449, 38);
            this.radioButtonAsynch.Name = "radioButtonAsynch";
            this.radioButtonAsynch.Size = new System.Drawing.Size(92, 17);
            this.radioButtonAsynch.TabIndex = 39;
            this.radioButtonAsynch.TabStop = true;
            this.radioButtonAsynch.Text = "Asynchronous";
            this.radioButtonAsynch.UseVisualStyleBackColor = true;
            // 
            // radioButtonSynch
            // 
            this.radioButtonSynch.AutoSize = true;
            this.radioButtonSynch.Location = new System.Drawing.Point(547, 38);
            this.radioButtonSynch.Name = "radioButtonSynch";
            this.radioButtonSynch.Size = new System.Drawing.Size(87, 17);
            this.radioButtonSynch.TabIndex = 40;
            this.radioButtonSynch.Text = "Synchronous";
            this.radioButtonSynch.UseVisualStyleBackColor = true;
            // 
            // buttonClearData
            // 
            this.buttonClearData.Enabled = false;
            this.buttonClearData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearData.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonClearData.ImageIndex = 2;
            this.buttonClearData.Location = new System.Drawing.Point(534, 7);
            this.buttonClearData.Name = "buttonClearData";
            this.buttonClearData.Size = new System.Drawing.Size(81, 23);
            this.buttonClearData.TabIndex = 38;
            this.buttonClearData.Tag = "HD";
            this.buttonClearData.Text = "Clear Data";
            this.buttonClearData.UseVisualStyleBackColor = true;
            this.buttonClearData.Click += new System.EventHandler(this.buttonClearData_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 448);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(759, 22);
            this.statusStrip1.TabIndex = 25;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // labelUsageNote
            // 
            this.labelUsageNote.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.labelUsageNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelUsageNote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelUsageNote.Location = new System.Drawing.Point(249, 67);
            this.labelUsageNote.Name = "labelUsageNote";
            this.labelUsageNote.Size = new System.Drawing.Size(498, 23);
            this.labelUsageNote.TabIndex = 55;
            this.labelUsageNote.Text = "Note: User can delete field by selecting a cell within the field column and press" +
                " the delete key. ";
            this.labelUsageNote.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(759, 470);
            this.Controls.Add(this.labelUsageNote);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.buttonClearData);
            this.Controls.Add(this.radioButtonSynch);
            this.Controls.Add(this.radioButtonAsynch);
            this.Controls.Add(this.buttonClearFields);
            this.Controls.Add(this.buttonSendRequest);
            this.Controls.Add(this.dataGridViewData);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonAddFields);
            this.Controls.Add(this.textBoxField);
            this.Controls.Add(this.labelField);
            this.Controls.Add(this.textBoxSecurity);
            this.Controls.Add(this.labelSecurity);
            this.MinimumSize = new System.Drawing.Size(767, 497);
            this.Name = "Form1";
            this.Text = "Simple History Example";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControlDates.ResumeLayout(false);
            this.tabPageDate.ResumeLayout(false);
            this.tabPageDate.PerformLayout();
            this.tabPageRelativeDate.ResumeLayout(false);
            this.tabPageRelativeDate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewData)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonAddFields;
        private System.Windows.Forms.TextBox textBoxField;
        private System.Windows.Forms.Label labelField;
        private System.Windows.Forms.TextBox textBoxSecurity;
        private System.Windows.Forms.Label labelSecurity;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBoxNonTradingDayValue;
        private System.Windows.Forms.Label labelNonTradingDayValue;
        private System.Windows.Forms.ComboBox comboBoxOverrideOption;
        private System.Windows.Forms.Label labelOverrideOption;
        private System.Windows.Forms.ComboBox comboBoxPricing;
        private System.Windows.Forms.Label labelPricing;
        private System.Windows.Forms.TabControl tabControlDates;
        private System.Windows.Forms.TabPage tabPageDate;
        private System.Windows.Forms.DateTimePicker dateTimePickerEndDate;
        private System.Windows.Forms.Label labelEndDate;
        private System.Windows.Forms.DateTimePicker dateTimePickerStart;
        private System.Windows.Forms.Label labelStartDate;
        private System.Windows.Forms.TabPage tabPageRelativeDate;
        private System.Windows.Forms.TextBox textBoxRelEndDate;
        private System.Windows.Forms.TextBox textBoxRelStartDate;
        private System.Windows.Forms.Label labelRelEndDate;
        private System.Windows.Forms.Label labelRelStartDate;
        private System.Windows.Forms.ComboBox comboBoxPeriodicitySelection;
        private System.Windows.Forms.Label labelPeriodicitySelection;
        private System.Windows.Forms.ComboBox comboBoxNonTradingDayMethod;
        private System.Windows.Forms.Label labelNonTradingDayMethod;
        private System.Windows.Forms.ComboBox comboBoxPeriodicityAdjustment;
        private System.Windows.Forms.Label labelPeridicityAdjustment;
        private System.Windows.Forms.TextBox textBoxMaxPoints;
        private System.Windows.Forms.Label labelMaxPoint;
        private System.Windows.Forms.TextBox textBoxCurrencyCode;
        private System.Windows.Forms.Label labelCurrencyCode;
        private System.Windows.Forms.DataGridView dataGridViewData;
        private System.Windows.Forms.Button buttonClearFields;
        private System.Windows.Forms.Button buttonSendRequest;
        private System.Windows.Forms.RadioButton radioButtonAsynch;
        private System.Windows.Forms.RadioButton radioButtonSynch;
        private System.Windows.Forms.Button buttonClearData;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Label labelUsageNote;
    }
}

