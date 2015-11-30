namespace Packet_analyzer
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
            this.devicesList = new System.Windows.Forms.ComboBox();
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // devicesList
            // 
            this.devicesList.FormattingEnabled = true;
            this.devicesList.Location = new System.Drawing.Point(12, 12);
            this.devicesList.Name = "devicesList";
            this.devicesList.Size = new System.Drawing.Size(1177, 21);
            this.devicesList.TabIndex = 0;
            // 
            // logBox
            // 
            this.logBox.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.logBox.Location = new System.Drawing.Point(12, 105);
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            this.logBox.Size = new System.Drawing.Size(1208, 381);
            this.logBox.TabIndex = 1;
            this.logBox.Text = "";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(12, 39);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(164, 36);
            this.buttonStart.TabIndex = 2;
            this.buttonStart.Text = "Start scaner";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1225, 498);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.devicesList);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox devicesList;
        private System.Windows.Forms.RichTextBox logBox;
        private System.Windows.Forms.Button buttonStart;
    }
}

