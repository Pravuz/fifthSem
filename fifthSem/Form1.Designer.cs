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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.tabCCALEL = new System.Windows.Forms.TabControl();
            this.tabPageCC = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.txtDebug = new System.Windows.Forms.TextBox();
            this.chrtTemp = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkTempMissingAlarm = new System.Windows.Forms.CheckBox();
            this.chkCOMAlarms = new System.Windows.Forms.CheckBox();
            this.chkHostMissingAlarms = new System.Windows.Forms.CheckBox();
            this.chkRS485Alarms = new System.Windows.Forms.CheckBox();
            this.chkTempAlarms = new System.Windows.Forms.CheckBox();
            this.dGFilteredAlarms = new System.Windows.Forms.DataGridView();
            this.btnAck = new System.Windows.Forms.Button();
            this.tabPageAL = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.Acknowledge2 = new System.Windows.Forms.Button();
            this.dGAllAlarms = new System.Windows.Forms.DataGridView();
            this.tabPageCF = new System.Windows.Forms.TabPage();
            this.Temperature = new System.Windows.Forms.GroupBox();
            this.txtTemperature = new System.Windows.Forms.TextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbCOM = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtRS485_MS = new System.Windows.Forms.TextBox();
            this.txtTCP_IP_MS = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbMPri = new System.Windows.Forms.ComboBox();
            this.btnApply = new System.Windows.Forms.Button();
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
            this.toolTipAck = new System.Windows.Forms.ToolTip(this.components);
            this.tabCCALEL.SuspendLayout();
            this.tabPageCC.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chrtTemp)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGFilteredAlarms)).BeginInit();
            this.tabPageAL.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGAllAlarms)).BeginInit();
            this.tabPageCF.SuspendLayout();
            this.Temperature.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabCCALEL
            // 
            this.tabCCALEL.Controls.Add(this.tabPageCC);
            this.tabCCALEL.Controls.Add(this.tabPageAL);
            this.tabCCALEL.Controls.Add(this.tabPageCF);
            this.tabCCALEL.Location = new System.Drawing.Point(0, 0);
            this.tabCCALEL.Name = "tabCCALEL";
            this.tabCCALEL.SelectedIndex = 0;
            this.tabCCALEL.Size = new System.Drawing.Size(1077, 494);
            this.tabCCALEL.TabIndex = 0;
            // 
            // tabPageCC
            // 
            this.tabPageCC.Controls.Add(this.button1);
            this.tabPageCC.Controls.Add(this.txtDebug);
            this.tabPageCC.Controls.Add(this.chrtTemp);
            this.tabPageCC.Controls.Add(this.groupBox2);
            this.tabPageCC.Location = new System.Drawing.Point(4, 22);
            this.tabPageCC.Name = "tabPageCC";
            this.tabPageCC.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCC.Size = new System.Drawing.Size(1069, 468);
            this.tabPageCC.TabIndex = 0;
            this.tabPageCC.Text = "ControlCenter";
            this.tabPageCC.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(622, 296);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(99, 23);
            this.button1.TabIndex = 15;
            this.button1.Text = "Switch to master";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtDebug
            // 
            this.txtDebug.Location = new System.Drawing.Point(760, 7);
            this.txtDebug.Multiline = true;
            this.txtDebug.Name = "txtDebug";
            this.txtDebug.Size = new System.Drawing.Size(303, 455);
            this.txtDebug.TabIndex = 14;
            // 
            // chrtTemp
            // 
            chartArea2.AxisX.LabelStyle.Format = "HH:mm:ss";
            chartArea2.Name = "ChartArea1";
            this.chrtTemp.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.chrtTemp.Legends.Add(legend2);
            this.chrtTemp.Location = new System.Drawing.Point(8, 244);
            this.chrtTemp.Name = "chrtTemp";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "Legend1";
            series2.Name = "SerTemp";
            series2.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            this.chrtTemp.Series.Add(series2);
            this.chrtTemp.Size = new System.Drawing.Size(745, 218);
            this.chrtTemp.TabIndex = 13;
            this.chrtTemp.Text = "chrtTemp";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkTempMissingAlarm);
            this.groupBox2.Controls.Add(this.chkCOMAlarms);
            this.groupBox2.Controls.Add(this.chkHostMissingAlarms);
            this.groupBox2.Controls.Add(this.chkRS485Alarms);
            this.groupBox2.Controls.Add(this.chkTempAlarms);
            this.groupBox2.Controls.Add(this.dGFilteredAlarms);
            this.groupBox2.Controls.Add(this.btnAck);
            this.groupBox2.Location = new System.Drawing.Point(8, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(745, 232);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Alarm";
            // 
            // chkTempMissingAlarm
            // 
            this.chkTempMissingAlarm.AutoSize = true;
            this.chkTempMissingAlarm.Checked = true;
            this.chkTempMissingAlarm.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTempMissingAlarm.Location = new System.Drawing.Point(614, 151);
            this.chkTempMissingAlarm.Name = "chkTempMissingAlarm";
            this.chkTempMissingAlarm.Size = new System.Drawing.Size(114, 17);
            this.chkTempMissingAlarm.TabIndex = 16;
            this.chkTempMissingAlarm.Text = "TempMissingAlarm";
            this.chkTempMissingAlarm.UseVisualStyleBackColor = true;
            this.chkTempMissingAlarm.CheckedChanged += new System.EventHandler(this.chkTempMissingAlarm_CheckedChanged);
            // 
            // chkCOMAlarms
            // 
            this.chkCOMAlarms.AutoSize = true;
            this.chkCOMAlarms.Checked = true;
            this.chkCOMAlarms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCOMAlarms.Location = new System.Drawing.Point(614, 128);
            this.chkCOMAlarms.Name = "chkCOMAlarms";
            this.chkCOMAlarms.Size = new System.Drawing.Size(76, 17);
            this.chkCOMAlarms.TabIndex = 15;
            this.chkCOMAlarms.Text = "COMAlarm";
            this.chkCOMAlarms.UseVisualStyleBackColor = true;
            this.chkCOMAlarms.CheckedChanged += new System.EventHandler(this.chkCOMAlarms_CheckedChanged);
            // 
            // chkHostMissingAlarms
            // 
            this.chkHostMissingAlarms.AutoSize = true;
            this.chkHostMissingAlarms.Checked = true;
            this.chkHostMissingAlarms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHostMissingAlarms.Location = new System.Drawing.Point(614, 105);
            this.chkHostMissingAlarms.Name = "chkHostMissingAlarms";
            this.chkHostMissingAlarms.Size = new System.Drawing.Size(109, 17);
            this.chkHostMissingAlarms.TabIndex = 14;
            this.chkHostMissingAlarms.Text = "HostMissingAlarm";
            this.chkHostMissingAlarms.UseVisualStyleBackColor = true;
            this.chkHostMissingAlarms.CheckedChanged += new System.EventHandler(this.chkHostMissingAlarms_CheckedChanged);
            // 
            // chkRS485Alarms
            // 
            this.chkRS485Alarms.AutoSize = true;
            this.chkRS485Alarms.Checked = true;
            this.chkRS485Alarms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRS485Alarms.Location = new System.Drawing.Point(614, 82);
            this.chkRS485Alarms.Name = "chkRS485Alarms";
            this.chkRS485Alarms.Size = new System.Drawing.Size(85, 17);
            this.chkRS485Alarms.TabIndex = 13;
            this.chkRS485Alarms.Text = "RS485Alarm";
            this.chkRS485Alarms.UseVisualStyleBackColor = true;
            this.chkRS485Alarms.CheckedChanged += new System.EventHandler(this.chkRS485Alarms_CheckedChanged);
            // 
            // chkTempAlarms
            // 
            this.chkTempAlarms.AutoSize = true;
            this.chkTempAlarms.Checked = true;
            this.chkTempAlarms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTempAlarms.Location = new System.Drawing.Point(614, 59);
            this.chkTempAlarms.Name = "chkTempAlarms";
            this.chkTempAlarms.Size = new System.Drawing.Size(112, 17);
            this.chkTempAlarms.TabIndex = 12;
            this.chkTempAlarms.Text = "TemperatureAlarm";
            this.chkTempAlarms.UseVisualStyleBackColor = true;
            this.chkTempAlarms.CheckedChanged += new System.EventHandler(this.chkTempAlarms_CheckedChanged);
            // 
            // dGFilteredAlarms
            // 
            this.dGFilteredAlarms.AllowUserToAddRows = false;
            this.dGFilteredAlarms.AllowUserToDeleteRows = false;
            this.dGFilteredAlarms.AllowUserToResizeRows = false;
            this.dGFilteredAlarms.Location = new System.Drawing.Point(18, 30);
            this.dGFilteredAlarms.Name = "dGFilteredAlarms";
            this.dGFilteredAlarms.ReadOnly = true;
            this.dGFilteredAlarms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dGFilteredAlarms.Size = new System.Drawing.Size(590, 196);
            this.dGFilteredAlarms.TabIndex = 0;
            this.dGFilteredAlarms.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dGAllAlarms_RowPrePaint);
            // 
            // btnAck
            // 
            this.btnAck.Location = new System.Drawing.Point(614, 30);
            this.btnAck.Name = "btnAck";
            this.btnAck.Size = new System.Drawing.Size(99, 23);
            this.btnAck.TabIndex = 10;
            this.btnAck.Text = "Acknowledge";
            this.toolTipAck.SetToolTip(this.btnAck, "This button will acknowledge the alarm that is visible. If the problem has not be" +
        "en solved the alarm will persist. See AlarmList Tab for more information.");
            this.btnAck.UseVisualStyleBackColor = true;
            this.btnAck.Click += new System.EventHandler(this.btnAck_Click);
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
            this.groupBox3.Controls.Add(this.Acknowledge2);
            this.groupBox3.Controls.Add(this.dGAllAlarms);
            this.groupBox3.Location = new System.Drawing.Point(20, 20);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(1043, 430);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "AlarmList";
            // 
            // Acknowledge2
            // 
            this.Acknowledge2.Location = new System.Drawing.Point(930, 19);
            this.Acknowledge2.Name = "Acknowledge2";
            this.Acknowledge2.Size = new System.Drawing.Size(107, 23);
            this.Acknowledge2.TabIndex = 1;
            this.Acknowledge2.Text = "Acknowledge";
            this.Acknowledge2.UseVisualStyleBackColor = true;
            this.Acknowledge2.Click += new System.EventHandler(this.Acknowledge2_Click);
            // 
            // dGAllAlarms
            // 
            this.dGAllAlarms.AllowUserToAddRows = false;
            this.dGAllAlarms.AllowUserToDeleteRows = false;
            this.dGAllAlarms.AllowUserToResizeRows = false;
            this.dGAllAlarms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGAllAlarms.Location = new System.Drawing.Point(6, 19);
            this.dGAllAlarms.Name = "dGAllAlarms";
            this.dGAllAlarms.ReadOnly = true;
            this.dGAllAlarms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dGAllAlarms.Size = new System.Drawing.Size(918, 405);
            this.dGAllAlarms.TabIndex = 0;
            this.dGAllAlarms.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dGAllAlarms_RowPrePaint);
            // 
            // tabPageCF
            // 
            this.tabPageCF.Controls.Add(this.Temperature);
            this.tabPageCF.Controls.Add(this.groupBox5);
            this.tabPageCF.Controls.Add(this.groupBox1);
            this.tabPageCF.Location = new System.Drawing.Point(4, 22);
            this.tabPageCF.Name = "tabPageCF";
            this.tabPageCF.Size = new System.Drawing.Size(1069, 468);
            this.tabPageCF.TabIndex = 2;
            this.tabPageCF.Text = "Config";
            this.tabPageCF.UseVisualStyleBackColor = true;
            // 
            // Temperature
            // 
            this.Temperature.Controls.Add(this.txtTemperature);
            this.Temperature.Location = new System.Drawing.Point(725, 112);
            this.Temperature.Name = "Temperature";
            this.Temperature.Size = new System.Drawing.Size(156, 217);
            this.Temperature.TabIndex = 20;
            this.Temperature.TabStop = false;
            this.Temperature.Text = "Temperature";
            // 
            // txtTemperature
            // 
            this.txtTemperature.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTemperature.Location = new System.Drawing.Point(24, 34);
            this.txtTemperature.Name = "txtTemperature";
            this.txtTemperature.Size = new System.Drawing.Size(100, 62);
            this.txtTemperature.TabIndex = 0;
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
            this.groupBox5.Controls.Add(this.btnApply);
            this.groupBox5.Location = new System.Drawing.Point(398, 112);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(302, 217);
            this.groupBox5.TabIndex = 22;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Setup";
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
            // cmbCOM
            // 
            this.cmbCOM.FormattingEnabled = true;
            this.cmbCOM.Location = new System.Drawing.Point(30, 161);
            this.cmbCOM.Name = "cmbCOM";
            this.cmbCOM.Size = new System.Drawing.Size(121, 21);
            this.cmbCOM.TabIndex = 19;
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
            "None",
            "1",
            "2",
            "3",
            "4"});
            this.cmbMPri.Location = new System.Drawing.Point(30, 40);
            this.cmbMPri.Name = "cmbMPri";
            this.cmbMPri.Size = new System.Drawing.Size(121, 21);
            this.cmbMPri.TabIndex = 14;
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(193, 161);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 23);
            this.btnApply.TabIndex = 13;
            this.btnApply.Text = "Apply";
            this.toolTipAck.SetToolTip(this.btnApply, "This button Set the priority of the PC. Check the other pc\'s so you don\'t get con" +
        "flicts.");
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
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
            this.groupBox1.Location = new System.Drawing.Point(136, 112);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(235, 218);
            this.groupBox1.TabIndex = 21;
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
            this.btnSetLimits.Click += new System.EventHandler(this.btnSetLimits_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1081, 501);
            this.Controls.Add(this.tabCCALEL);
            this.Name = "Form1";
            this.Text = "Skadd";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabCCALEL.ResumeLayout(false);
            this.tabPageCC.ResumeLayout(false);
            this.tabPageCC.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chrtTemp)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGFilteredAlarms)).EndInit();
            this.tabPageAL.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dGAllAlarms)).EndInit();
            this.tabPageCF.ResumeLayout(false);
            this.Temperature.ResumeLayout(false);
            this.Temperature.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabCCALEL;
        private System.Windows.Forms.TabPage tabPageCC;
        private System.Windows.Forms.TabPage tabPageAL;
        private System.Windows.Forms.TabPage tabPageCF;
        private System.Windows.Forms.Button btnAck;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ToolTip toolTipAck;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtTemp;
        private System.Windows.Forms.DataGridView dGFilteredAlarms;
        private System.Windows.Forms.DataGridView dGAllAlarms;
        private System.Windows.Forms.CheckBox chkTempMissingAlarm;
        private System.Windows.Forms.CheckBox chkCOMAlarms;
        private System.Windows.Forms.CheckBox chkHostMissingAlarms;
        private System.Windows.Forms.CheckBox chkRS485Alarms;
        private System.Windows.Forms.CheckBox chkTempAlarms;
        private System.Windows.Forms.DataVisualization.Charting.Chart chrtTemp;
        private System.Windows.Forms.Button Acknowledge2;
        private System.Windows.Forms.GroupBox Temperature;
        private System.Windows.Forms.TextBox txtTemperature;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbCOM;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtRS485_MS;
        private System.Windows.Forms.TextBox txtTCP_IP_MS;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbMPri;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLLLvl;
        private System.Windows.Forms.TextBox txtLLvl;
        private System.Windows.Forms.TextBox txtHLvl;
        private System.Windows.Forms.TextBox txtHHLvl;
        private System.Windows.Forms.Button btnSetLimits;
        private System.Windows.Forms.TextBox txtDebug;
        private System.Windows.Forms.Button button1;
    }
}

