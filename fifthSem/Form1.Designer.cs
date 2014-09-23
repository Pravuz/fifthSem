namespace fifthSem
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
            this.tabCCALEL = new System.Windows.Forms.TabControl();
            this.tabPageCC = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtRS485_MS = new System.Windows.Forms.TextBox();
            this.txtTCP_IP_MS = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbMPri = new System.Windows.Forms.ComboBox();
            this.btnSetPri = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtAlrmAck = new System.Windows.Forms.TextBox();
            this.btnAck = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLLLvl = new System.Windows.Forms.TextBox();
            this.txtLLvl = new System.Windows.Forms.TextBox();
            this.txtHLvl = new System.Windows.Forms.TextBox();
            this.txtHHLvl = new System.Windows.Forms.TextBox();
            this.btnSetLimits = new System.Windows.Forms.Button();
            this.tabPageAL = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lstAlarm = new System.Windows.Forms.ListView();
            this.tabPageEL = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lstEvent = new System.Windows.Forms.ListView();
            this.toolTipAck = new System.Windows.Forms.ToolTip(this.components);
            this.cmbCOM = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tabCCALEL.SuspendLayout();
            this.tabPageCC.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPageAL.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabPageEL.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabCCALEL
            // 
            this.tabCCALEL.Controls.Add(this.tabPageCC);
            this.tabCCALEL.Controls.Add(this.tabPageAL);
            this.tabCCALEL.Controls.Add(this.tabPageEL);
            this.tabCCALEL.Location = new System.Drawing.Point(0, 0);
            this.tabCCALEL.Name = "tabCCALEL";
            this.tabCCALEL.SelectedIndex = 0;
            this.tabCCALEL.Size = new System.Drawing.Size(1077, 494);
            this.tabCCALEL.TabIndex = 0;
            // 
            // tabPageCC
            // 
            this.tabPageCC.Controls.Add(this.groupBox5);
            this.tabPageCC.Controls.Add(this.groupBox2);
            this.tabPageCC.Controls.Add(this.groupBox1);
            this.tabPageCC.Location = new System.Drawing.Point(4, 22);
            this.tabPageCC.Name = "tabPageCC";
            this.tabPageCC.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCC.Size = new System.Drawing.Size(1069, 468);
            this.tabPageCC.TabIndex = 0;
            this.tabPageCC.Text = "ControlCenter";
            this.tabPageCC.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label8);
            this.groupBox5.Controls.Add(this.cmbCOM);
            this.groupBox5.Controls.Add(this.label7);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.txtRS485_MS);
            this.groupBox5.Controls.Add(this.txtTCP_IP_MS);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.cmbMPri);
            this.groupBox5.Controls.Add(this.btnSetPri);
            this.groupBox5.Location = new System.Drawing.Point(282, 6);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(302, 217);
            this.groupBox5.TabIndex = 19;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Setup";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(27, 106);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(107, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "RS485 Master/Slave";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(27, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(110, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "TCP/IP Master/Slave";
            // 
            // txtRS485_MS
            // 
            this.txtRS485_MS.Location = new System.Drawing.Point(30, 122);
            this.txtRS485_MS.Name = "txtRS485_MS";
            this.txtRS485_MS.ReadOnly = true;
            this.txtRS485_MS.Size = new System.Drawing.Size(100, 20);
            this.txtRS485_MS.TabIndex = 16;
            // 
            // txtTCP_IP_MS
            // 
            this.txtTCP_IP_MS.Location = new System.Drawing.Point(30, 83);
            this.txtTCP_IP_MS.Name = "txtTCP_IP_MS";
            this.txtTCP_IP_MS.ReadOnly = true;
            this.txtTCP_IP_MS.Size = new System.Drawing.Size(100, 20);
            this.txtTCP_IP_MS.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "MasterPriority";
            // 
            // cmbMPri
            // 
            this.cmbMPri.FormattingEnabled = true;
            this.cmbMPri.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4"});
            this.cmbMPri.Location = new System.Drawing.Point(30, 40);
            this.cmbMPri.Name = "cmbMPri";
            this.cmbMPri.Size = new System.Drawing.Size(121, 21);
            this.cmbMPri.TabIndex = 14;
            // 
            // btnSetPri
            // 
            this.btnSetPri.Location = new System.Drawing.Point(195, 38);
            this.btnSetPri.Name = "btnSetPri";
            this.btnSetPri.Size = new System.Drawing.Size(75, 23);
            this.btnSetPri.TabIndex = 13;
            this.btnSetPri.Text = "Set Priority";
            this.toolTipAck.SetToolTip(this.btnSetPri, "This button Set the priority of the PC. Check the other pc\'s so you don\'t get con" +
        "flicts.");
            this.btnSetPri.UseVisualStyleBackColor = true;
            this.btnSetPri.Click += new System.EventHandler(this.btnSetPri_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtAlrmAck);
            this.groupBox2.Controls.Add(this.btnAck);
            this.groupBox2.Location = new System.Drawing.Point(20, 251);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(565, 81);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Alarm";
            // 
            // txtAlrmAck
            // 
            this.txtAlrmAck.Location = new System.Drawing.Point(22, 33);
            this.txtAlrmAck.Name = "txtAlrmAck";
            this.txtAlrmAck.ReadOnly = true;
            this.txtAlrmAck.Size = new System.Drawing.Size(416, 20);
            this.txtAlrmAck.TabIndex = 11;
            // 
            // btnAck
            // 
            this.btnAck.Location = new System.Drawing.Point(444, 30);
            this.btnAck.Name = "btnAck";
            this.btnAck.Size = new System.Drawing.Size(99, 23);
            this.btnAck.TabIndex = 10;
            this.btnAck.Text = "Acknowledge";
            this.toolTipAck.SetToolTip(this.btnAck, "This button will acknowledge the alarm that is visible. If the problem has not be" +
        "en solved the alarm will persist. See AlarmList Tab for more information.");
            this.btnAck.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtLLLvl);
            this.groupBox1.Controls.Add(this.txtLLvl);
            this.groupBox1.Controls.Add(this.txtHLvl);
            this.groupBox1.Controls.Add(this.txtHHLvl);
            this.groupBox1.Controls.Add(this.btnSetLimits);
            this.groupBox1.Location = new System.Drawing.Point(20, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(235, 218);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Alarm Levels";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "LowLow Level";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Low Level";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "High Level";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "HighHigh Level";
            // 
            // txtLLLvl
            // 
            this.txtLLLvl.Location = new System.Drawing.Point(15, 170);
            this.txtLLLvl.Name = "txtLLLvl";
            this.txtLLLvl.Size = new System.Drawing.Size(100, 20);
            this.txtLLLvl.TabIndex = 4;
            // 
            // txtLLvl
            // 
            this.txtLLvl.Location = new System.Drawing.Point(15, 131);
            this.txtLLvl.Name = "txtLLvl";
            this.txtLLvl.Size = new System.Drawing.Size(100, 20);
            this.txtLLvl.TabIndex = 3;
            // 
            // txtHLvl
            // 
            this.txtHLvl.Location = new System.Drawing.Point(15, 92);
            this.txtHLvl.Name = "txtHLvl";
            this.txtHLvl.Size = new System.Drawing.Size(100, 20);
            this.txtHLvl.TabIndex = 2;
            // 
            // txtHHLvl
            // 
            this.txtHHLvl.Location = new System.Drawing.Point(15, 50);
            this.txtHHLvl.Name = "txtHHLvl";
            this.txtHHLvl.Size = new System.Drawing.Size(100, 20);
            this.txtHHLvl.TabIndex = 1;
            // 
            // btnSetLimits
            // 
            this.btnSetLimits.Location = new System.Drawing.Point(138, 110);
            this.btnSetLimits.Name = "btnSetLimits";
            this.btnSetLimits.Size = new System.Drawing.Size(75, 23);
            this.btnSetLimits.TabIndex = 0;
            this.btnSetLimits.Text = "Set Limits";
            this.toolTipAck.SetToolTip(this.btnSetLimits, "This button will set the alarm levels you have written in the textboxes.");
            this.btnSetLimits.UseVisualStyleBackColor = true;
            this.btnSetLimits.Click += new System.EventHandler(this.btnSetLimits_Click);
            // 
            // tabPageAL
            // 
            this.tabPageAL.Controls.Add(this.groupBox3);
            this.tabPageAL.Location = new System.Drawing.Point(4, 22);
            this.tabPageAL.Name = "tabPageAL";
            this.tabPageAL.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAL.Size = new System.Drawing.Size(1069, 468);
            this.tabPageAL.TabIndex = 1;
            this.tabPageAL.Text = "AlarmList";
            this.tabPageAL.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lstAlarm);
            this.groupBox3.Location = new System.Drawing.Point(20, 20);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(950, 430);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "AlarmList";
            // 
            // lstAlarm
            // 
            this.lstAlarm.Location = new System.Drawing.Point(15, 20);
            this.lstAlarm.Name = "lstAlarm";
            this.lstAlarm.Size = new System.Drawing.Size(920, 400);
            this.lstAlarm.TabIndex = 0;
            this.lstAlarm.UseCompatibleStateImageBehavior = false;
            // 
            // tabPageEL
            // 
            this.tabPageEL.Controls.Add(this.groupBox4);
            this.tabPageEL.Location = new System.Drawing.Point(4, 22);
            this.tabPageEL.Name = "tabPageEL";
            this.tabPageEL.Size = new System.Drawing.Size(1069, 468);
            this.tabPageEL.TabIndex = 2;
            this.tabPageEL.Text = "EventList";
            this.tabPageEL.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lstEvent);
            this.groupBox4.Location = new System.Drawing.Point(20, 20);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(950, 430);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "EventList";
            // 
            // lstEvent
            // 
            this.lstEvent.Location = new System.Drawing.Point(15, 20);
            this.lstEvent.Name = "lstEvent";
            this.lstEvent.Size = new System.Drawing.Size(920, 400);
            this.lstEvent.TabIndex = 0;
            this.lstEvent.UseCompatibleStateImageBehavior = false;
            // 
            // cmbCOM
            // 
            this.cmbCOM.FormattingEnabled = true;
            this.cmbCOM.Location = new System.Drawing.Point(30, 161);
            this.cmbCOM.Name = "cmbCOM";
            this.cmbCOM.Size = new System.Drawing.Size(121, 21);
            this.cmbCOM.TabIndex = 19;
            this.cmbCOM.SelectedIndexChanged += new System.EventHandler(this.cmbCOM_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(27, 145);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "COMPort";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1081, 501);
            this.Controls.Add(this.tabCCALEL);
            this.Name = "Form1";
            this.Text = "Skadd";
            this.tabCCALEL.ResumeLayout(false);
            this.tabPageCC.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPageAL.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tabPageEL.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabCCALEL;
        private System.Windows.Forms.TabPage tabPageCC;
        private System.Windows.Forms.TabPage tabPageAL;
        private System.Windows.Forms.TabPage tabPageEL;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLLLvl;
        private System.Windows.Forms.TextBox txtLLvl;
        private System.Windows.Forms.TextBox txtHLvl;
        private System.Windows.Forms.TextBox txtHHLvl;
        private System.Windows.Forms.Button btnSetLimits;
        private System.Windows.Forms.Button btnAck;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtAlrmAck;
        private System.Windows.Forms.ToolTip toolTipAck;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListView lstAlarm;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListView lstEvent;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtRS485_MS;
        private System.Windows.Forms.TextBox txtTCP_IP_MS;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbMPri;
        private System.Windows.Forms.Button btnSetPri;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbCOM;
    }
}

