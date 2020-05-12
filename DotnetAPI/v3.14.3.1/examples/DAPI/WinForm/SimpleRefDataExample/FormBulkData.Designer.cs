namespace Bloomberglp.Blpapi.Examples
{
    partial class FormBulkData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBulkData));
            this.toolStripBDTool = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonBDClose = new System.Windows.Forms.ToolStripButton();
            this.dataGridViewRDBulkData = new System.Windows.Forms.DataGridView();
            this.toolStripBDTool.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRDBulkData)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStripBDTool
            // 
            this.toolStripBDTool.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSave,
            this.toolStripButtonBDClose});
            this.toolStripBDTool.Location = new System.Drawing.Point(0, 0);
            this.toolStripBDTool.Name = "toolStripBDTool";
            this.toolStripBDTool.Size = new System.Drawing.Size(674, 25);
            this.toolStripBDTool.TabIndex = 4;
            this.toolStripBDTool.Text = "toolStrip1";
            // 
            // toolStripButtonSave
            // 
            this.toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSave.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSave.Image")));
            this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSave.Name = "toolStripButtonSave";
            this.toolStripButtonSave.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonSave.Text = "Save data to text file";
            this.toolStripButtonSave.ToolTipText = "Save data to text file";
            this.toolStripButtonSave.Click += new System.EventHandler(this.toolStripButtonSave_Click);
            // 
            // toolStripButtonBDClose
            // 
            this.toolStripButtonBDClose.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonBDClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonBDClose.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonBDClose.Image")));
            this.toolStripButtonBDClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonBDClose.Name = "toolStripButtonBDClose";
            this.toolStripButtonBDClose.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonBDClose.Text = "Close";
            this.toolStripButtonBDClose.Click += new System.EventHandler(this.toolStripButtonBDClose_Click);
            // 
            // dataGridViewRDBulkData
            // 
            this.dataGridViewRDBulkData.AllowUserToAddRows = false;
            this.dataGridViewRDBulkData.AllowUserToDeleteRows = false;
            this.dataGridViewRDBulkData.AllowUserToResizeRows = false;
            this.dataGridViewRDBulkData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewRDBulkData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewRDBulkData.Location = new System.Drawing.Point(0, 25);
            this.dataGridViewRDBulkData.MultiSelect = false;
            this.dataGridViewRDBulkData.Name = "dataGridViewRDBulkData";
            this.dataGridViewRDBulkData.ReadOnly = true;
            this.dataGridViewRDBulkData.RowHeadersVisible = false;
            this.dataGridViewRDBulkData.Size = new System.Drawing.Size(674, 471);
            this.dataGridViewRDBulkData.TabIndex = 5;
            // 
            // FormBulkData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 496);
            this.ControlBox = false;
            this.Controls.Add(this.dataGridViewRDBulkData);
            this.Controls.Add(this.toolStripBDTool);
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "FormBulkData";
            this.Text = "Bulk Data";
            this.toolStripBDTool.ResumeLayout(false);
            this.toolStripBDTool.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRDBulkData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripBDTool;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonBDClose;
        private System.Windows.Forms.DataGridView dataGridViewRDBulkData;
    }
}