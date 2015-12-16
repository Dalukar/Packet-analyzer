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
            this.buttonStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textHost = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBps = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // devicesList
            // 
            this.devicesList.FormattingEnabled = true;
            this.devicesList.Location = new System.Drawing.Point(65, 12);
            this.devicesList.Name = "devicesList";
            this.devicesList.Size = new System.Drawing.Size(531, 21);
            this.devicesList.TabIndex = 0;
            // 
            // logBox
            // 
            this.logBox.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.logBox.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.logBox.Location = new System.Drawing.Point(12, 65);
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            this.logBox.Size = new System.Drawing.Size(1208, 421);
            this.logBox.TabIndex = 1;
            this.logBox.Text = "";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(906, 3);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(164, 36);
            this.buttonStart.TabIndex = 2;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(1076, 3);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(144, 36);
            this.buttonStop.TabIndex = 3;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Adapter:";
            // 
            // textHost
            // 
            this.textHost.Location = new System.Drawing.Point(65, 39);
            this.textHost.Name = "textHost";
            this.textHost.Size = new System.Drawing.Size(531, 20);
            this.textHost.TabIndex = 5;
            this.textHost.Text = "192.168.1.1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Host IP:";
            // 
            // textBps
            // 
            this.textBps.Location = new System.Drawing.Point(635, 12);
            this.textBps.Name = "textBps";
            this.textBps.ReadOnly = true;
            this.textBps.Size = new System.Drawing.Size(159, 20);
            this.textBps.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(602, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "bps:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1225, 498);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBps);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textHost);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.devicesList);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox devicesList;
        private System.Windows.Forms.RichTextBox logBox;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textHost;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBps;
        private System.Windows.Forms.Label label3;
    }
}

