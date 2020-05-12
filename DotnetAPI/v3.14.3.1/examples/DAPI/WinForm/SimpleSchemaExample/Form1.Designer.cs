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
            this.labelServices = new System.Windows.Forms.Label();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.listBoxServices = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainerSchema = new System.Windows.Forms.SplitContainer();
            this.treeViewSchema = new System.Windows.Forms.TreeView();
            this.splitContainerProperties = new System.Windows.Forms.SplitContainer();
            this.listViewProperties = new System.Windows.Forms.ListView();
            this.columnHeaderProperties = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderValues = new System.Windows.Forms.ColumnHeader();
            this.richTextBoxDescription = new System.Windows.Forms.RichTextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.textBoxService = new System.Windows.Forms.TextBox();
            this.buttonGetService = new System.Windows.Forms.Button();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.splitContainerSchema.Panel1.SuspendLayout();
            this.splitContainerSchema.Panel2.SuspendLayout();
            this.splitContainerSchema.SuspendLayout();
            this.splitContainerProperties.Panel1.SuspendLayout();
            this.splitContainerProperties.Panel2.SuspendLayout();
            this.splitContainerProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelServices
            // 
            this.labelServices.AutoSize = true;
            this.labelServices.Location = new System.Drawing.Point(6, 6);
            this.labelServices.Name = "labelServices";
            this.labelServices.Size = new System.Drawing.Size(46, 13);
            this.labelServices.TabIndex = 0;
            this.labelServices.Text = "Service:";
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerMain.Location = new System.Drawing.Point(4, 30);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.listBoxServices);
            this.splitContainerMain.Panel1.Controls.Add(this.label1);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainerSchema);
            this.splitContainerMain.Size = new System.Drawing.Size(811, 383);
            this.splitContainerMain.SplitterDistance = 202;
            this.splitContainerMain.TabIndex = 7;
            // 
            // listBoxServices
            // 
            this.listBoxServices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxServices.FormattingEnabled = true;
            this.listBoxServices.Location = new System.Drawing.Point(0, 15);
            this.listBoxServices.Name = "listBoxServices";
            this.listBoxServices.Size = new System.Drawing.Size(202, 368);
            this.listBoxServices.TabIndex = 4;
            this.listBoxServices.SelectedIndexChanged += new System.EventHandler(this.listBoxServices_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(202, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Services";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // splitContainerSchema
            // 
            this.splitContainerSchema.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerSchema.Location = new System.Drawing.Point(0, 0);
            this.splitContainerSchema.Name = "splitContainerSchema";
            // 
            // splitContainerSchema.Panel1
            // 
            this.splitContainerSchema.Panel1.Controls.Add(this.treeViewSchema);
            // 
            // splitContainerSchema.Panel2
            // 
            this.splitContainerSchema.Panel2.Controls.Add(this.splitContainerProperties);
            this.splitContainerSchema.Size = new System.Drawing.Size(605, 383);
            this.splitContainerSchema.SplitterDistance = 339;
            this.splitContainerSchema.TabIndex = 0;
            // 
            // treeViewSchema
            // 
            this.treeViewSchema.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewSchema.Location = new System.Drawing.Point(0, 0);
            this.treeViewSchema.Name = "treeViewSchema";
            this.treeViewSchema.Size = new System.Drawing.Size(339, 383);
            this.treeViewSchema.TabIndex = 5;
            this.treeViewSchema.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewSchema_AfterSelect);
            // 
            // splitContainerProperties
            // 
            this.splitContainerProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerProperties.Location = new System.Drawing.Point(0, 0);
            this.splitContainerProperties.Name = "splitContainerProperties";
            this.splitContainerProperties.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerProperties.Panel1
            // 
            this.splitContainerProperties.Panel1.Controls.Add(this.listViewProperties);
            // 
            // splitContainerProperties.Panel2
            // 
            this.splitContainerProperties.Panel2.Controls.Add(this.richTextBoxDescription);
            this.splitContainerProperties.Panel2.Controls.Add(this.labelDescription);
            this.splitContainerProperties.Size = new System.Drawing.Size(262, 383);
            this.splitContainerProperties.SplitterDistance = 311;
            this.splitContainerProperties.TabIndex = 0;
            // 
            // listViewProperties
            // 
            this.listViewProperties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderProperties,
            this.columnHeaderValues});
            this.listViewProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewProperties.Location = new System.Drawing.Point(0, 0);
            this.listViewProperties.Name = "listViewProperties";
            this.listViewProperties.Size = new System.Drawing.Size(262, 311);
            this.listViewProperties.TabIndex = 6;
            this.listViewProperties.UseCompatibleStateImageBehavior = false;
            this.listViewProperties.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderProperties
            // 
            this.columnHeaderProperties.Text = "Properties";
            this.columnHeaderProperties.Width = 116;
            // 
            // columnHeaderValues
            // 
            this.columnHeaderValues.Text = "Value";
            this.columnHeaderValues.Width = 140;
            // 
            // richTextBoxDescription
            // 
            this.richTextBoxDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxDescription.Location = new System.Drawing.Point(0, 13);
            this.richTextBoxDescription.Name = "richTextBoxDescription";
            this.richTextBoxDescription.Size = new System.Drawing.Size(262, 55);
            this.richTextBoxDescription.TabIndex = 7;
            this.richTextBoxDescription.Text = "";
            // 
            // labelDescription
            // 
            this.labelDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelDescription.Location = new System.Drawing.Point(0, 0);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(262, 13);
            this.labelDescription.TabIndex = 0;
            this.labelDescription.Text = "Description";
            this.labelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxService
            // 
            this.textBoxService.Location = new System.Drawing.Point(58, 3);
            this.textBoxService.Name = "textBoxService";
            this.textBoxService.Size = new System.Drawing.Size(224, 20);
            this.textBoxService.TabIndex = 1;
            this.textBoxService.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxService_KeyDown);
            // 
            // buttonGetService
            // 
            this.buttonGetService.Location = new System.Drawing.Point(288, 1);
            this.buttonGetService.Name = "buttonGetService";
            this.buttonGetService.Size = new System.Drawing.Size(75, 23);
            this.buttonGetService.TabIndex = 2;
            this.buttonGetService.Text = "Get Service";
            this.buttonGetService.UseVisualStyleBackColor = true;
            this.buttonGetService.Click += new System.EventHandler(this.buttonGetService_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(817, 418);
            this.Controls.Add(this.buttonGetService);
            this.Controls.Add(this.textBoxService);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.labelServices);
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "Form1";
            this.Text = "Simple Schema Example";
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainerSchema.Panel1.ResumeLayout(false);
            this.splitContainerSchema.Panel2.ResumeLayout(false);
            this.splitContainerSchema.ResumeLayout(false);
            this.splitContainerProperties.Panel1.ResumeLayout(false);
            this.splitContainerProperties.Panel2.ResumeLayout(false);
            this.splitContainerProperties.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelServices;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.SplitContainer splitContainerSchema;
        private System.Windows.Forms.TreeView treeViewSchema;
        private System.Windows.Forms.SplitContainer splitContainerProperties;
        private System.Windows.Forms.ListView listViewProperties;
        private System.Windows.Forms.ColumnHeader columnHeaderProperties;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.RichTextBox richTextBoxDescription;
        private System.Windows.Forms.ColumnHeader columnHeaderValues;
        private System.Windows.Forms.TextBox textBoxService;
        private System.Windows.Forms.Button buttonGetService;
        private System.Windows.Forms.ListBox listBoxServices;
        private System.Windows.Forms.Label label1;
    }
}

