namespace NewApiBrowser
{
    partial class FormTransferProgress
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
            this.labelFilename = new System.Windows.Forms.Label();
            this.progressBarPercentComplete = new System.Windows.Forms.ProgressBar();
            this.labelProgressString = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelFilename
            // 
            this.labelFilename.AutoSize = true;
            this.labelFilename.Location = new System.Drawing.Point(13, 13);
            this.labelFilename.Name = "labelFilename";
            this.labelFilename.Size = new System.Drawing.Size(121, 13);
            this.labelFilename.TabIndex = 0;
            this.labelFilename.Text = "Uploading \"foobar.txt\"...";
            // 
            // progressBarPercentComplete
            // 
            this.progressBarPercentComplete.Location = new System.Drawing.Point(16, 39);
            this.progressBarPercentComplete.Name = "progressBarPercentComplete";
            this.progressBarPercentComplete.Size = new System.Drawing.Size(404, 23);
            this.progressBarPercentComplete.TabIndex = 1;
            // 
            // labelProgressString
            // 
            this.labelProgressString.AutoSize = true;
            this.labelProgressString.Location = new System.Drawing.Point(13, 74);
            this.labelProgressString.Name = "labelProgressString";
            this.labelProgressString.Size = new System.Drawing.Size(128, 13);
            this.labelProgressString.TabIndex = 2;
            this.labelProgressString.Text = "{0} of {1} bytes transfered";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(345, 69);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FormTransferProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 109);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelProgressString);
            this.Controls.Add(this.progressBarPercentComplete);
            this.Controls.Add(this.labelFilename);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FormTransferProgress";
            this.Text = "Upload Progress";
            this.Load += new System.EventHandler(this.FormTransferProgress_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelFilename;
        private System.Windows.Forms.ProgressBar progressBarPercentComplete;
        private System.Windows.Forms.Label labelProgressString;
        private System.Windows.Forms.Button buttonCancel;
    }
}