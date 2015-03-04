namespace heckdarn
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
            this.lblProcessStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblProcessStatus
            // 
            this.lblProcessStatus.AutoSize = true;
            this.lblProcessStatus.Location = new System.Drawing.Point(13, 13);
            this.lblProcessStatus.Name = "lblProcessStatus";
            this.lblProcessStatus.Size = new System.Drawing.Size(85, 13);
            this.lblProcessStatus.TabIndex = 0;
            this.lblProcessStatus.Text = "lblProcessStatus";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(493, 144);
            this.Controls.Add(this.lblProcessStatus);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblProcessStatus;
    }
}

