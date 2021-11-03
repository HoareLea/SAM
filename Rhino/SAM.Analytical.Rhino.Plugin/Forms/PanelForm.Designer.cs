
namespace SAM.Analytical.Rhino.Plugin
{
    partial class PanelForm
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
            this.Button_OK = new System.Windows.Forms.Button();
            this.Label_BucketSize = new System.Windows.Forms.Label();
            this.TextBox_BucketSize = new System.Windows.Forms.TextBox();
            this.TextBox_MaxExtend = new System.Windows.Forms.TextBox();
            this.Label_MaxExtend = new System.Windows.Forms.Label();
            this.TextBox_Weight = new System.Windows.Forms.TextBox();
            this.Label_Weight = new System.Windows.Forms.Label();
            this.Button_Reset = new System.Windows.Forms.Button();
            this.Button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Button_OK
            // 
            this.Button_OK.Location = new System.Drawing.Point(195, 182);
            this.Button_OK.Name = "Button_OK";
            this.Button_OK.Size = new System.Drawing.Size(75, 23);
            this.Button_OK.TabIndex = 0;
            this.Button_OK.Text = "OK";
            this.Button_OK.UseVisualStyleBackColor = true;
            this.Button_OK.Click += new System.EventHandler(this.Button_OK_Click);
            // 
            // Label_BucketSize
            // 
            this.Label_BucketSize.AutoSize = true;
            this.Label_BucketSize.Location = new System.Drawing.Point(12, 17);
            this.Label_BucketSize.Name = "Label_BucketSize";
            this.Label_BucketSize.Size = new System.Drawing.Size(82, 17);
            this.Label_BucketSize.TabIndex = 1;
            this.Label_BucketSize.Text = "Bucket Size";
            // 
            // TextBox_BucketSize
            // 
            this.TextBox_BucketSize.Location = new System.Drawing.Point(113, 14);
            this.TextBox_BucketSize.Name = "TextBox_BucketSize";
            this.TextBox_BucketSize.Size = new System.Drawing.Size(157, 22);
            this.TextBox_BucketSize.TabIndex = 2;
            this.TextBox_BucketSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            // 
            // TextBox_MaxExtend
            // 
            this.TextBox_MaxExtend.Location = new System.Drawing.Point(113, 42);
            this.TextBox_MaxExtend.Name = "TextBox_MaxExtend";
            this.TextBox_MaxExtend.Size = new System.Drawing.Size(157, 22);
            this.TextBox_MaxExtend.TabIndex = 4;
            this.TextBox_MaxExtend.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            // 
            // Label_MaxExtend
            // 
            this.Label_MaxExtend.AutoSize = true;
            this.Label_MaxExtend.Location = new System.Drawing.Point(12, 45);
            this.Label_MaxExtend.Name = "Label_MaxExtend";
            this.Label_MaxExtend.Size = new System.Drawing.Size(80, 17);
            this.Label_MaxExtend.TabIndex = 3;
            this.Label_MaxExtend.Text = "Max Extend";
            // 
            // TextBox_Weight
            // 
            this.TextBox_Weight.Location = new System.Drawing.Point(113, 70);
            this.TextBox_Weight.Name = "TextBox_Weight";
            this.TextBox_Weight.Size = new System.Drawing.Size(157, 22);
            this.TextBox_Weight.TabIndex = 6;
            this.TextBox_Weight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            // 
            // Label_Weight
            // 
            this.Label_Weight.AutoSize = true;
            this.Label_Weight.Location = new System.Drawing.Point(12, 73);
            this.Label_Weight.Name = "Label_Weight";
            this.Label_Weight.Size = new System.Drawing.Size(52, 17);
            this.Label_Weight.TabIndex = 5;
            this.Label_Weight.Text = "Weight";
            // 
            // Button_Reset
            // 
            this.Button_Reset.Location = new System.Drawing.Point(12, 113);
            this.Button_Reset.Name = "Button_Reset";
            this.Button_Reset.Size = new System.Drawing.Size(258, 23);
            this.Button_Reset.TabIndex = 7;
            this.Button_Reset.Text = "Reset";
            this.Button_Reset.UseVisualStyleBackColor = true;
            this.Button_Reset.Click += new System.EventHandler(this.Button_Reset_Click);
            // 
            // Button_Cancel
            // 
            this.Button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Button_Cancel.Location = new System.Drawing.Point(114, 182);
            this.Button_Cancel.Name = "Button_Cancel";
            this.Button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.Button_Cancel.TabIndex = 8;
            this.Button_Cancel.Text = "Cancel";
            this.Button_Cancel.UseVisualStyleBackColor = true;
            this.Button_Cancel.Click += new System.EventHandler(this.Button_Cancel_Click);
            // 
            // PanelForm
            // 
            this.AcceptButton = this.Button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Button_Cancel;
            this.ClientSize = new System.Drawing.Size(282, 217);
            this.ControlBox = false;
            this.Controls.Add(this.Button_Cancel);
            this.Controls.Add(this.Button_Reset);
            this.Controls.Add(this.TextBox_Weight);
            this.Controls.Add(this.Label_Weight);
            this.Controls.Add(this.TextBox_MaxExtend);
            this.Controls.Add(this.Label_MaxExtend);
            this.Controls.Add(this.TextBox_BucketSize);
            this.Controls.Add(this.Label_BucketSize);
            this.Controls.Add(this.Button_OK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "PanelForm";
            this.Text = "Panel Settings";
            this.Load += new System.EventHandler(this.PanelForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Button_OK;
        private System.Windows.Forms.Label Label_BucketSize;
        private System.Windows.Forms.TextBox TextBox_BucketSize;
        private System.Windows.Forms.TextBox TextBox_MaxExtend;
        private System.Windows.Forms.Label Label_MaxExtend;
        private System.Windows.Forms.TextBox TextBox_Weight;
        private System.Windows.Forms.Label Label_Weight;
        private System.Windows.Forms.Button Button_Reset;
        private System.Windows.Forms.Button Button_Cancel;
    }
}