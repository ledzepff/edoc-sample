namespace eDocTest
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
            this.btnTest1 = new System.Windows.Forms.Button();
            this.btnLoadDoc = new System.Windows.Forms.Button();
            this.dgvProperties = new System.Windows.Forms.DataGridView();
            this.Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProperties)).BeginInit();
            this.SuspendLayout();
            // 
            // btnTest1
            // 
            this.btnTest1.Location = new System.Drawing.Point(579, 12);
            this.btnTest1.Name = "btnTest1";
            this.btnTest1.Size = new System.Drawing.Size(152, 57);
            this.btnTest1.TabIndex = 0;
            this.btnTest1.Text = "Sign Document";
            this.btnTest1.UseVisualStyleBackColor = true;
            this.btnTest1.Click += new System.EventHandler(this.btnTest1_Click);
            // 
            // btnLoadDoc
            // 
            this.btnLoadDoc.Location = new System.Drawing.Point(12, 12);
            this.btnLoadDoc.Name = "btnLoadDoc";
            this.btnLoadDoc.Size = new System.Drawing.Size(152, 57);
            this.btnLoadDoc.TabIndex = 1;
            this.btnLoadDoc.Text = "Load eDoc";
            this.btnLoadDoc.UseVisualStyleBackColor = true;
            this.btnLoadDoc.Click += new System.EventHandler(this.btnLoadDoc_Click);
            // 
            // dgvProperties
            // 
            this.dgvProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProperties.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Property,
            this.Value});
            this.dgvProperties.Location = new System.Drawing.Point(12, 75);
            this.dgvProperties.Name = "dgvProperties";
            this.dgvProperties.Size = new System.Drawing.Size(554, 532);
            this.dgvProperties.TabIndex = 2;
            // 
            // Property
            // 
            this.Property.HeaderText = "Property";
            this.Property.Name = "Property";
            this.Property.Width = 200;
            // 
            // Value
            // 
            this.Value.HeaderText = "Value";
            this.Value.Name = "Value";
            this.Value.Width = 300;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(579, 84);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(319, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 613);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.dgvProperties);
            this.Controls.Add(this.btnLoadDoc);
            this.Controls.Add(this.btnTest1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dgvProperties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTest1;
        private System.Windows.Forms.Button btnLoadDoc;
        private System.Windows.Forms.DataGridView dgvProperties;
        private System.Windows.Forms.DataGridViewTextBoxColumn Property;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}

