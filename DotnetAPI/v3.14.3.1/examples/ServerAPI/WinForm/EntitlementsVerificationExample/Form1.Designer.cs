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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBoxUserManagement = new System.Windows.Forms.GroupBox();
            this.buttonValidateUser = new System.Windows.Forms.Button();
            this.buttonRemoveUser = new System.Windows.Forms.Button();
            this.buttonLoadList = new System.Windows.Forms.Button();
            this.buttonClearList = new System.Windows.Forms.Button();
            this.buttonSaveList = new System.Windows.Forms.Button();
            this.buttonSubmitUsers = new System.Windows.Forms.Button();
            this.dataGridViewUsers = new System.Windows.Forms.DataGridView();
            this.UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UUID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Logon = new System.Windows.Forms.DataGridViewImageColumn();
            this.buttonAddUser = new System.Windows.Forms.Button();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.labelIP = new System.Windows.Forms.Label();
            this.textBoxUUID = new System.Windows.Forms.TextBox();
            this.labelUUID = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.labelUserName = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabControlUsers = new System.Windows.Forms.TabControl();
            this.tabPageSecurityEntitlements = new System.Windows.Forms.TabPage();
            this.CheckBoxAutoFitColumn = new System.Windows.Forms.CheckBox();
            this.buttonRemoveSec = new System.Windows.Forms.Button();
            this.checkBoxSecEIDs = new System.Windows.Forms.CheckBox();
            this.buttonAddSecurity = new System.Windows.Forms.Button();
            this.buttonClearAll = new System.Windows.Forms.Button();
            this.buttonUserEntitlementValidation = new System.Windows.Forms.Button();
            this.dataGridViewSecEntitlement = new System.Windows.Forms.DataGridView();
            this.textBoxSecurity = new System.Windows.Forms.TextBox();
            this.labelSecurity = new System.Windows.Forms.Label();
            this.tabPageUserMode = new System.Windows.Forms.TabPage();
            this.labelSubscriptionNote = new System.Windows.Forms.Label();
            this.buttonUserStopSubscription = new System.Windows.Forms.Button();
            this.buttonRemoveUserSecurity = new System.Windows.Forms.Button();
            this.buttonAddUserSecurity = new System.Windows.Forms.Button();
            this.buttonClearAllUserData = new System.Windows.Forms.Button();
            this.buttonUserSubscribe = new System.Windows.Forms.Button();
            this.dataGridViewUserData = new System.Windows.Forms.DataGridView();
            this.textBoxUserSecurity = new System.Windows.Forms.TextBox();
            this.labelUserSecurity = new System.Windows.Forms.Label();
            this.comboBoxSelectUser = new System.Windows.Forms.ComboBox();
            this.labelSelectUser = new System.Windows.Forms.Label();
            this.groupBoxUserManagement.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUsers)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.tabControlUsers.SuspendLayout();
            this.tabPageSecurityEntitlements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSecEntitlement)).BeginInit();
            this.tabPageUserMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUserData)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxUserManagement
            // 
            this.groupBoxUserManagement.Controls.Add(this.buttonValidateUser);
            this.groupBoxUserManagement.Controls.Add(this.buttonRemoveUser);
            this.groupBoxUserManagement.Controls.Add(this.buttonLoadList);
            this.groupBoxUserManagement.Controls.Add(this.buttonClearList);
            this.groupBoxUserManagement.Controls.Add(this.buttonSaveList);
            this.groupBoxUserManagement.Controls.Add(this.buttonSubmitUsers);
            this.groupBoxUserManagement.Controls.Add(this.dataGridViewUsers);
            this.groupBoxUserManagement.Controls.Add(this.buttonAddUser);
            this.groupBoxUserManagement.Controls.Add(this.textBoxIP);
            this.groupBoxUserManagement.Controls.Add(this.labelIP);
            this.groupBoxUserManagement.Controls.Add(this.textBoxUUID);
            this.groupBoxUserManagement.Controls.Add(this.labelUUID);
            this.groupBoxUserManagement.Controls.Add(this.textBoxName);
            this.groupBoxUserManagement.Controls.Add(this.labelUserName);
            this.groupBoxUserManagement.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxUserManagement.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxUserManagement.Location = new System.Drawing.Point(0, 0);
            this.groupBoxUserManagement.Name = "groupBoxUserManagement";
            this.groupBoxUserManagement.Size = new System.Drawing.Size(683, 262);
            this.groupBoxUserManagement.TabIndex = 1;
            this.groupBoxUserManagement.TabStop = false;
            this.groupBoxUserManagement.Text = "User Management";
            // 
            // buttonValidateUser
            // 
            this.buttonValidateUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonValidateUser.Enabled = false;
            this.buttonValidateUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonValidateUser.Location = new System.Drawing.Point(565, 72);
            this.buttonValidateUser.Name = "buttonValidateUser";
            this.buttonValidateUser.Size = new System.Drawing.Size(95, 23);
            this.buttonValidateUser.TabIndex = 20;
            this.buttonValidateUser.Text = "Validate User";
            this.buttonValidateUser.UseVisualStyleBackColor = true;
            this.buttonValidateUser.Click += new System.EventHandler(this.buttonValidateUser_Click);
            // 
            // buttonRemoveUser
            // 
            this.buttonRemoveUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveUser.Enabled = false;
            this.buttonRemoveUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRemoveUser.Location = new System.Drawing.Point(565, 129);
            this.buttonRemoveUser.Name = "buttonRemoveUser";
            this.buttonRemoveUser.Size = new System.Drawing.Size(95, 23);
            this.buttonRemoveUser.TabIndex = 14;
            this.buttonRemoveUser.Text = "Remove User";
            this.buttonRemoveUser.UseVisualStyleBackColor = true;
            this.buttonRemoveUser.Click += new System.EventHandler(this.buttonRemoveUser_Click);
            // 
            // buttonLoadList
            // 
            this.buttonLoadList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLoadList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonLoadList.Location = new System.Drawing.Point(565, 187);
            this.buttonLoadList.Name = "buttonLoadList";
            this.buttonLoadList.Size = new System.Drawing.Size(95, 23);
            this.buttonLoadList.TabIndex = 16;
            this.buttonLoadList.Text = "Load List";
            this.buttonLoadList.UseVisualStyleBackColor = true;
            this.buttonLoadList.Click += new System.EventHandler(this.buttonLoadList_Click);
            // 
            // buttonClearList
            // 
            this.buttonClearList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearList.Enabled = false;
            this.buttonClearList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearList.Location = new System.Drawing.Point(565, 216);
            this.buttonClearList.Name = "buttonClearList";
            this.buttonClearList.Size = new System.Drawing.Size(95, 23);
            this.buttonClearList.TabIndex = 17;
            this.buttonClearList.Text = "Clear List";
            this.buttonClearList.UseVisualStyleBackColor = true;
            this.buttonClearList.Click += new System.EventHandler(this.buttonClearList_Click);
            // 
            // buttonSaveList
            // 
            this.buttonSaveList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveList.Enabled = false;
            this.buttonSaveList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSaveList.Location = new System.Drawing.Point(565, 158);
            this.buttonSaveList.Name = "buttonSaveList";
            this.buttonSaveList.Size = new System.Drawing.Size(95, 23);
            this.buttonSaveList.TabIndex = 15;
            this.buttonSaveList.Text = "Save List";
            this.buttonSaveList.UseVisualStyleBackColor = true;
            this.buttonSaveList.Click += new System.EventHandler(this.buttonSaveList_Click);
            // 
            // buttonSubmitUsers
            // 
            this.buttonSubmitUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSubmitUsers.Enabled = false;
            this.buttonSubmitUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSubmitUsers.Location = new System.Drawing.Point(565, 100);
            this.buttonSubmitUsers.Name = "buttonSubmitUsers";
            this.buttonSubmitUsers.Size = new System.Drawing.Size(95, 23);
            this.buttonSubmitUsers.TabIndex = 13;
            this.buttonSubmitUsers.Text = "Submit User";
            this.buttonSubmitUsers.UseVisualStyleBackColor = true;
            this.buttonSubmitUsers.Click += new System.EventHandler(this.buttonSubmitUsers_Click);
            // 
            // dataGridViewUsers
            // 
            this.dataGridViewUsers.AllowUserToAddRows = false;
            this.dataGridViewUsers.AllowUserToDeleteRows = false;
            this.dataGridViewUsers.AllowUserToResizeRows = false;
            this.dataGridViewUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewUsers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewUsers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.UserName,
            this.UUID,
            this.IP,
            this.Logon});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewUsers.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewUsers.Location = new System.Drawing.Point(12, 45);
            this.dataGridViewUsers.Name = "dataGridViewUsers";
            this.dataGridViewUsers.ReadOnly = true;
            this.dataGridViewUsers.RowHeadersVisible = false;
            this.dataGridViewUsers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewUsers.Size = new System.Drawing.Size(529, 202);
            this.dataGridViewUsers.TabIndex = 18;
            // 
            // UserName
            // 
            this.UserName.HeaderText = "Name";
            this.UserName.Name = "UserName";
            this.UserName.ReadOnly = true;
            // 
            // UUID
            // 
            this.UUID.HeaderText = "UUID";
            this.UUID.Name = "UUID";
            this.UUID.ReadOnly = true;
            // 
            // IP
            // 
            this.IP.HeaderText = "IP";
            this.IP.Name = "IP";
            this.IP.ReadOnly = true;
            // 
            // Logon
            // 
            this.Logon.HeaderText = "Logon";
            this.Logon.Name = "Logon";
            this.Logon.ReadOnly = true;
            // 
            // buttonAddUser
            // 
            this.buttonAddUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddUser.Location = new System.Drawing.Point(565, 43);
            this.buttonAddUser.Name = "buttonAddUser";
            this.buttonAddUser.Size = new System.Drawing.Size(95, 23);
            this.buttonAddUser.TabIndex = 12;
            this.buttonAddUser.Text = "Add User";
            this.buttonAddUser.UseVisualStyleBackColor = true;
            this.buttonAddUser.Click += new System.EventHandler(this.buttonAddUser_Click);
            // 
            // textBoxIP
            // 
            this.textBoxIP.Location = new System.Drawing.Point(411, 19);
            this.textBoxIP.MaxLength = 15;
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.Size = new System.Drawing.Size(98, 20);
            this.textBoxIP.TabIndex = 7;
            // 
            // labelIP
            // 
            this.labelIP.AutoSize = true;
            this.labelIP.Location = new System.Drawing.Point(385, 22);
            this.labelIP.Name = "labelIP";
            this.labelIP.Size = new System.Drawing.Size(20, 13);
            this.labelIP.TabIndex = 6;
            this.labelIP.Text = "IP:";
            // 
            // textBoxUUID
            // 
            this.textBoxUUID.Location = new System.Drawing.Point(274, 19);
            this.textBoxUUID.MaxLength = 10;
            this.textBoxUUID.Name = "textBoxUUID";
            this.textBoxUUID.Size = new System.Drawing.Size(98, 20);
            this.textBoxUUID.TabIndex = 5;
            this.textBoxUUID.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxUUID_KeyDown);
            // 
            // labelUUID
            // 
            this.labelUUID.AutoSize = true;
            this.labelUUID.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUUID.Location = new System.Drawing.Point(227, 23);
            this.labelUUID.Name = "labelUUID";
            this.labelUUID.Size = new System.Drawing.Size(37, 13);
            this.labelUUID.TabIndex = 4;
            this.labelUUID.Text = "UUID:";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(60, 19);
            this.textBoxName.MaxLength = 40;
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(161, 20);
            this.textBoxName.TabIndex = 3;
            // 
            // labelUserName
            // 
            this.labelUserName.AutoSize = true;
            this.labelUserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUserName.Location = new System.Drawing.Point(11, 23);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(38, 13);
            this.labelUserName.TabIndex = 2;
            this.labelUserName.Text = "Name:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 490);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(683, 22);
            this.statusStrip1.TabIndex = 55;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "LoggedOn.ico");
            this.imageList1.Images.SetKeyName(1, "LoggedOff.ico");
            this.imageList1.Images.SetKeyName(2, "unknown.gif");
            this.imageList1.Images.SetKeyName(3, "LoggedOn1.ico");
            // 
            // tabControlUsers
            // 
            this.tabControlUsers.Controls.Add(this.tabPageSecurityEntitlements);
            this.tabControlUsers.Controls.Add(this.tabPageUserMode);
            this.tabControlUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlUsers.Location = new System.Drawing.Point(0, 262);
            this.tabControlUsers.Name = "tabControlUsers";
            this.tabControlUsers.SelectedIndex = 0;
            this.tabControlUsers.Size = new System.Drawing.Size(683, 228);
            this.tabControlUsers.TabIndex = 56;
            // 
            // tabPageSecurityEntitlements
            // 
            this.tabPageSecurityEntitlements.Controls.Add(this.CheckBoxAutoFitColumn);
            this.tabPageSecurityEntitlements.Controls.Add(this.buttonRemoveSec);
            this.tabPageSecurityEntitlements.Controls.Add(this.checkBoxSecEIDs);
            this.tabPageSecurityEntitlements.Controls.Add(this.buttonAddSecurity);
            this.tabPageSecurityEntitlements.Controls.Add(this.buttonClearAll);
            this.tabPageSecurityEntitlements.Controls.Add(this.buttonUserEntitlementValidation);
            this.tabPageSecurityEntitlements.Controls.Add(this.dataGridViewSecEntitlement);
            this.tabPageSecurityEntitlements.Controls.Add(this.textBoxSecurity);
            this.tabPageSecurityEntitlements.Controls.Add(this.labelSecurity);
            this.tabPageSecurityEntitlements.Location = new System.Drawing.Point(4, 22);
            this.tabPageSecurityEntitlements.Name = "tabPageSecurityEntitlements";
            this.tabPageSecurityEntitlements.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSecurityEntitlements.Size = new System.Drawing.Size(675, 202);
            this.tabPageSecurityEntitlements.TabIndex = 0;
            this.tabPageSecurityEntitlements.Text = "Security Entitlements";
            this.tabPageSecurityEntitlements.UseVisualStyleBackColor = true;
            // 
            // CheckBoxAutoFitColumn
            // 
            this.CheckBoxAutoFitColumn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CheckBoxAutoFitColumn.AutoSize = true;
            this.CheckBoxAutoFitColumn.Checked = true;
            this.CheckBoxAutoFitColumn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckBoxAutoFitColumn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckBoxAutoFitColumn.Location = new System.Drawing.Point(149, 179);
            this.CheckBoxAutoFitColumn.Name = "CheckBoxAutoFitColumn";
            this.CheckBoxAutoFitColumn.Size = new System.Drawing.Size(98, 17);
            this.CheckBoxAutoFitColumn.TabIndex = 28;
            this.CheckBoxAutoFitColumn.Text = "Auto fit collumn";
            this.CheckBoxAutoFitColumn.UseVisualStyleBackColor = true;
            this.CheckBoxAutoFitColumn.CheckedChanged += new System.EventHandler(this.CheckBoxAutoFitColumn_CheckedChanged);
            // 
            // buttonRemoveSec
            // 
            this.buttonRemoveSec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveSec.Enabled = false;
            this.buttonRemoveSec.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRemoveSec.Location = new System.Drawing.Point(561, 64);
            this.buttonRemoveSec.Name = "buttonRemoveSec";
            this.buttonRemoveSec.Size = new System.Drawing.Size(95, 23);
            this.buttonRemoveSec.TabIndex = 24;
            this.buttonRemoveSec.Text = "Remove Sec.";
            this.buttonRemoveSec.UseVisualStyleBackColor = true;
            this.buttonRemoveSec.Click += new System.EventHandler(this.buttonRemoveSec_Click);
            // 
            // checkBoxSecEIDs
            // 
            this.checkBoxSecEIDs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxSecEIDs.AutoSize = true;
            this.checkBoxSecEIDs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxSecEIDs.Location = new System.Drawing.Point(8, 179);
            this.checkBoxSecEIDs.Name = "checkBoxSecEIDs";
            this.checkBoxSecEIDs.Size = new System.Drawing.Size(104, 17);
            this.checkBoxSecEIDs.TabIndex = 27;
            this.checkBoxSecEIDs.Text = "Show Sec. EIDs";
            this.checkBoxSecEIDs.UseVisualStyleBackColor = true;
            this.checkBoxSecEIDs.CheckedChanged += new System.EventHandler(this.checkBoxSecEIDs_CheckedChanged);
            // 
            // buttonAddSecurity
            // 
            this.buttonAddSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddSecurity.Location = new System.Drawing.Point(379, 7);
            this.buttonAddSecurity.Name = "buttonAddSecurity";
            this.buttonAddSecurity.Size = new System.Drawing.Size(75, 23);
            this.buttonAddSecurity.TabIndex = 22;
            this.buttonAddSecurity.Text = "Add";
            this.buttonAddSecurity.UseVisualStyleBackColor = true;
            this.buttonAddSecurity.Click += new System.EventHandler(this.buttonAddSecurity_Click);
            // 
            // buttonClearAll
            // 
            this.buttonClearAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearAll.Enabled = false;
            this.buttonClearAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearAll.Location = new System.Drawing.Point(561, 93);
            this.buttonClearAll.Name = "buttonClearAll";
            this.buttonClearAll.Size = new System.Drawing.Size(95, 23);
            this.buttonClearAll.TabIndex = 25;
            this.buttonClearAll.Text = "Clear All";
            this.buttonClearAll.UseVisualStyleBackColor = true;
            this.buttonClearAll.Click += new System.EventHandler(this.buttonClearAll_Click);
            // 
            // buttonUserEntitlementValidation
            // 
            this.buttonUserEntitlementValidation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUserEntitlementValidation.Enabled = false;
            this.buttonUserEntitlementValidation.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUserEntitlementValidation.Location = new System.Drawing.Point(561, 35);
            this.buttonUserEntitlementValidation.Name = "buttonUserEntitlementValidation";
            this.buttonUserEntitlementValidation.Size = new System.Drawing.Size(95, 23);
            this.buttonUserEntitlementValidation.TabIndex = 23;
            this.buttonUserEntitlementValidation.Text = "Validate";
            this.buttonUserEntitlementValidation.UseVisualStyleBackColor = true;
            this.buttonUserEntitlementValidation.Click += new System.EventHandler(this.buttonUserEntitlementValidation_Click);
            // 
            // dataGridViewSecEntitlement
            // 
            this.dataGridViewSecEntitlement.AllowDrop = true;
            this.dataGridViewSecEntitlement.AllowUserToAddRows = false;
            this.dataGridViewSecEntitlement.AllowUserToDeleteRows = false;
            this.dataGridViewSecEntitlement.AllowUserToResizeRows = false;
            this.dataGridViewSecEntitlement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewSecEntitlement.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewSecEntitlement.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewSecEntitlement.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewSecEntitlement.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewSecEntitlement.Location = new System.Drawing.Point(8, 36);
            this.dataGridViewSecEntitlement.MultiSelect = false;
            this.dataGridViewSecEntitlement.Name = "dataGridViewSecEntitlement";
            this.dataGridViewSecEntitlement.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewSecEntitlement.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridViewSecEntitlement.RowHeadersVisible = false;
            this.dataGridViewSecEntitlement.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewSecEntitlement.Size = new System.Drawing.Size(529, 137);
            this.dataGridViewSecEntitlement.TabIndex = 26;
            this.dataGridViewSecEntitlement.DragEnter += new System.Windows.Forms.DragEventHandler(this.dataGridViewSecEntitlement_DragEnter);
            this.dataGridViewSecEntitlement.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGridViewSecEntitlement_DragDrop);
            // 
            // textBoxSecurity
            // 
            this.textBoxSecurity.Location = new System.Drawing.Point(68, 9);
            this.textBoxSecurity.Name = "textBoxSecurity";
            this.textBoxSecurity.Size = new System.Drawing.Size(294, 20);
            this.textBoxSecurity.TabIndex = 21;
            this.textBoxSecurity.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSecurity_KeyDown);
            // 
            // labelSecurity
            // 
            this.labelSecurity.AutoSize = true;
            this.labelSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSecurity.Location = new System.Drawing.Point(5, 12);
            this.labelSecurity.Name = "labelSecurity";
            this.labelSecurity.Size = new System.Drawing.Size(48, 13);
            this.labelSecurity.TabIndex = 20;
            this.labelSecurity.Text = "Security:";
            // 
            // tabPageUserMode
            // 
            this.tabPageUserMode.Controls.Add(this.labelSubscriptionNote);
            this.tabPageUserMode.Controls.Add(this.buttonUserStopSubscription);
            this.tabPageUserMode.Controls.Add(this.buttonRemoveUserSecurity);
            this.tabPageUserMode.Controls.Add(this.buttonAddUserSecurity);
            this.tabPageUserMode.Controls.Add(this.buttonClearAllUserData);
            this.tabPageUserMode.Controls.Add(this.buttonUserSubscribe);
            this.tabPageUserMode.Controls.Add(this.dataGridViewUserData);
            this.tabPageUserMode.Controls.Add(this.textBoxUserSecurity);
            this.tabPageUserMode.Controls.Add(this.labelUserSecurity);
            this.tabPageUserMode.Controls.Add(this.comboBoxSelectUser);
            this.tabPageUserMode.Controls.Add(this.labelSelectUser);
            this.tabPageUserMode.Location = new System.Drawing.Point(4, 22);
            this.tabPageUserMode.Name = "tabPageUserMode";
            this.tabPageUserMode.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUserMode.Size = new System.Drawing.Size(675, 202);
            this.tabPageUserMode.TabIndex = 1;
            this.tabPageUserMode.Text = "User Mode";
            this.tabPageUserMode.UseVisualStyleBackColor = true;
            // 
            // labelSubscriptionNote
            // 
            this.labelSubscriptionNote.AutoSize = true;
            this.labelSubscriptionNote.Location = new System.Drawing.Point(8, 186);
            this.labelSubscriptionNote.Name = "labelSubscriptionNote";
            this.labelSubscriptionNote.Size = new System.Drawing.Size(392, 13);
            this.labelSubscriptionNote.TabIndex = 34;
            this.labelSubscriptionNote.Text = "Note: White - Not Subscribed, Green - Subscribed, Yellow - Subscribed with delay";
            // 
            // buttonUserStopSubscription
            // 
            this.buttonUserStopSubscription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUserStopSubscription.Enabled = false;
            this.buttonUserStopSubscription.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUserStopSubscription.Location = new System.Drawing.Point(561, 87);
            this.buttonUserStopSubscription.Name = "buttonUserStopSubscription";
            this.buttonUserStopSubscription.Size = new System.Drawing.Size(95, 23);
            this.buttonUserStopSubscription.TabIndex = 31;
            this.buttonUserStopSubscription.Text = "Stop";
            this.buttonUserStopSubscription.UseVisualStyleBackColor = true;
            this.buttonUserStopSubscription.Click += new System.EventHandler(this.buttonUserStopSubscription_Click);
            // 
            // buttonRemoveUserSecurity
            // 
            this.buttonRemoveUserSecurity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveUserSecurity.Enabled = false;
            this.buttonRemoveUserSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRemoveUserSecurity.Location = new System.Drawing.Point(561, 116);
            this.buttonRemoveUserSecurity.Name = "buttonRemoveUserSecurity";
            this.buttonRemoveUserSecurity.Size = new System.Drawing.Size(95, 23);
            this.buttonRemoveUserSecurity.TabIndex = 32;
            this.buttonRemoveUserSecurity.Text = "Remove Sec.";
            this.buttonRemoveUserSecurity.UseVisualStyleBackColor = true;
            this.buttonRemoveUserSecurity.Click += new System.EventHandler(this.buttonRemoveUserSecurity_Click);
            // 
            // buttonAddUserSecurity
            // 
            this.buttonAddUserSecurity.Enabled = false;
            this.buttonAddUserSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddUserSecurity.Location = new System.Drawing.Point(388, 30);
            this.buttonAddUserSecurity.Name = "buttonAddUserSecurity";
            this.buttonAddUserSecurity.Size = new System.Drawing.Size(75, 23);
            this.buttonAddUserSecurity.TabIndex = 29;
            this.buttonAddUserSecurity.Text = "Add";
            this.buttonAddUserSecurity.UseVisualStyleBackColor = true;
            this.buttonAddUserSecurity.Click += new System.EventHandler(this.buttonAddUserSecurity_Click);
            // 
            // buttonClearAllUserData
            // 
            this.buttonClearAllUserData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearAllUserData.Enabled = false;
            this.buttonClearAllUserData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearAllUserData.Location = new System.Drawing.Point(561, 145);
            this.buttonClearAllUserData.Name = "buttonClearAllUserData";
            this.buttonClearAllUserData.Size = new System.Drawing.Size(95, 23);
            this.buttonClearAllUserData.TabIndex = 33;
            this.buttonClearAllUserData.Text = "Clear All";
            this.buttonClearAllUserData.UseVisualStyleBackColor = true;
            this.buttonClearAllUserData.Click += new System.EventHandler(this.buttonClearAllUserData_Click);
            // 
            // buttonUserSubscribe
            // 
            this.buttonUserSubscribe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUserSubscribe.Enabled = false;
            this.buttonUserSubscribe.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUserSubscribe.Location = new System.Drawing.Point(561, 58);
            this.buttonUserSubscribe.Name = "buttonUserSubscribe";
            this.buttonUserSubscribe.Size = new System.Drawing.Size(95, 23);
            this.buttonUserSubscribe.TabIndex = 30;
            this.buttonUserSubscribe.Text = "Subscribe";
            this.buttonUserSubscribe.UseVisualStyleBackColor = true;
            this.buttonUserSubscribe.Click += new System.EventHandler(this.buttonUserSubscribe_Click);
            // 
            // dataGridViewUserData
            // 
            this.dataGridViewUserData.AllowDrop = true;
            this.dataGridViewUserData.AllowUserToAddRows = false;
            this.dataGridViewUserData.AllowUserToDeleteRows = false;
            this.dataGridViewUserData.AllowUserToResizeRows = false;
            this.dataGridViewUserData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewUserData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewUserData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridViewUserData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewUserData.DefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridViewUserData.Enabled = false;
            this.dataGridViewUserData.Location = new System.Drawing.Point(8, 59);
            this.dataGridViewUserData.MultiSelect = false;
            this.dataGridViewUserData.Name = "dataGridViewUserData";
            this.dataGridViewUserData.ReadOnly = true;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewUserData.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridViewUserData.RowHeadersVisible = false;
            this.dataGridViewUserData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewUserData.Size = new System.Drawing.Size(529, 121);
            this.dataGridViewUserData.TabIndex = 33;
            this.dataGridViewUserData.DragEnter += new System.Windows.Forms.DragEventHandler(this.dataGridViewUserData_DragEnter);
            this.dataGridViewUserData.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGridViewUserData_DragDrop);
            // 
            // textBoxUserSecurity
            // 
            this.textBoxUserSecurity.Enabled = false;
            this.textBoxUserSecurity.Location = new System.Drawing.Point(77, 32);
            this.textBoxUserSecurity.Name = "textBoxUserSecurity";
            this.textBoxUserSecurity.Size = new System.Drawing.Size(294, 20);
            this.textBoxUserSecurity.TabIndex = 28;
            this.textBoxUserSecurity.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxUserSecurity_KeyDown);
            // 
            // labelUserSecurity
            // 
            this.labelUserSecurity.AutoSize = true;
            this.labelUserSecurity.Enabled = false;
            this.labelUserSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUserSecurity.Location = new System.Drawing.Point(23, 35);
            this.labelUserSecurity.Name = "labelUserSecurity";
            this.labelUserSecurity.Size = new System.Drawing.Size(48, 13);
            this.labelUserSecurity.TabIndex = 27;
            this.labelUserSecurity.Text = "Security:";
            // 
            // comboBoxSelectUser
            // 
            this.comboBoxSelectUser.Enabled = false;
            this.comboBoxSelectUser.FormattingEnabled = true;
            this.comboBoxSelectUser.Location = new System.Drawing.Point(77, 5);
            this.comboBoxSelectUser.Name = "comboBoxSelectUser";
            this.comboBoxSelectUser.Size = new System.Drawing.Size(294, 21);
            this.comboBoxSelectUser.TabIndex = 1;
            // 
            // labelSelectUser
            // 
            this.labelSelectUser.AutoSize = true;
            this.labelSelectUser.Enabled = false;
            this.labelSelectUser.Location = new System.Drawing.Point(6, 8);
            this.labelSelectUser.Name = "labelSelectUser";
            this.labelSelectUser.Size = new System.Drawing.Size(65, 13);
            this.labelSelectUser.TabIndex = 0;
            this.labelSelectUser.Text = "Select User:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 512);
            this.Controls.Add(this.tabControlUsers);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBoxUserManagement);
            this.MaximumSize = new System.Drawing.Size(699, 550);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(699, 550);
            this.Name = "Form1";
            this.Text = "Entitlements Verification Example";
            this.groupBoxUserManagement.ResumeLayout(false);
            this.groupBoxUserManagement.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUsers)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControlUsers.ResumeLayout(false);
            this.tabPageSecurityEntitlements.ResumeLayout(false);
            this.tabPageSecurityEntitlements.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSecEntitlement)).EndInit();
            this.tabPageUserMode.ResumeLayout(false);
            this.tabPageUserMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUserData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxUserManagement;
        private System.Windows.Forms.Button buttonRemoveUser;
        private System.Windows.Forms.Button buttonLoadList;
        private System.Windows.Forms.Button buttonClearList;
        private System.Windows.Forms.Button buttonSaveList;
        private System.Windows.Forms.Button buttonSubmitUsers;
        private System.Windows.Forms.DataGridView dataGridViewUsers;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn UUID;
        private System.Windows.Forms.DataGridViewTextBoxColumn IP;
        private System.Windows.Forms.DataGridViewImageColumn Logon;
        private System.Windows.Forms.Button buttonAddUser;
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.Label labelIP;
        private System.Windows.Forms.TextBox textBoxUUID;
        private System.Windows.Forms.Label labelUUID;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button buttonValidateUser;
        private System.Windows.Forms.TabControl tabControlUsers;
        private System.Windows.Forms.TabPage tabPageSecurityEntitlements;
        private System.Windows.Forms.TabPage tabPageUserMode;
        private System.Windows.Forms.CheckBox CheckBoxAutoFitColumn;
        private System.Windows.Forms.Button buttonRemoveSec;
        private System.Windows.Forms.CheckBox checkBoxSecEIDs;
        private System.Windows.Forms.Button buttonAddSecurity;
        private System.Windows.Forms.Button buttonClearAll;
        private System.Windows.Forms.Button buttonUserEntitlementValidation;
        private System.Windows.Forms.DataGridView dataGridViewSecEntitlement;
        private System.Windows.Forms.TextBox textBoxSecurity;
        private System.Windows.Forms.Label labelSecurity;
        private System.Windows.Forms.ComboBox comboBoxSelectUser;
        private System.Windows.Forms.Label labelSelectUser;
        private System.Windows.Forms.Button buttonRemoveUserSecurity;
        private System.Windows.Forms.Button buttonAddUserSecurity;
        private System.Windows.Forms.Button buttonClearAllUserData;
        private System.Windows.Forms.Button buttonUserSubscribe;
        private System.Windows.Forms.DataGridView dataGridViewUserData;
        private System.Windows.Forms.TextBox textBoxUserSecurity;
        private System.Windows.Forms.Label labelUserSecurity;
        private System.Windows.Forms.Button buttonUserStopSubscription;
        private System.Windows.Forms.Label labelSubscriptionNote;
    }
}

