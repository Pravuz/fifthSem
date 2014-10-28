using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace fifthSem
{
    public partial class Form1 : Form
    {
        private DataEngine mDataEngine;
        public Form1()
        {            
            InitializeComponent();
            serialPortNames();

            mDataEngine = new DataEngine();
            mDataEngine.mNewTempHandler += DataEngineNewTempHandler;
            mDataEngine.mNewTcpStatusHandler += DataEngineNewTcpStatusHandler;
            mDataEngine.mNewComStatusHandler += DataEngineNewComStatusHandler;
            mDataEngine.deAlarmManager.AlarmsChangedEvent += deAlarmManager_AlarmsChangedEvent;
            mDataEngine.deAlarmManager.TempLimitsChangedEvent += deAlarmManager_TempLimitsChangedEvent;
            mDataEngine.deAlarmManager.AlarmFiltersChangedEvent += deAlarmManager_AlarmFiltersChangedEvent;
            mDataEngine.deScpHost.MessageEvent += scpHost_MessageEvent;
        }

        void deAlarmManager_AlarmFiltersChangedEvent(object sender, AlarmFiltersChangedEventArgs e)
        {
            if (this.InvokeRequired) // InvokeRequired is true if event is triggered from another thread
            {
                // In this case trigger the event using BeginInvoke which makes sure the event is handled by the main thread
                this.BeginInvoke((MethodInvoker)delegate
                {
                    deAlarmManager_AlarmFiltersChangedEvent(sender, e);
                });
                return;
            }

            // This is needed to make sure CheckedChanged is not triggered when .Checked is updated here.
            // CheckedChange should only be triggered when changed from GUI
            foreach (CheckBox chk in tabCCALEL.TabPages["tabPageCC"].Controls["groupBox2"].Controls.OfType<CheckBox>())
            {
                chk.CheckedChanged -= chkAlarmFilter_CheckedChanged;
            }

            chkTempAlarms.Checked = e.Filters.Contains(AlarmTypes.TempHi);
            chkHostMissingAlarms.Checked = e.Filters.Contains(AlarmTypes.HostMissing);
            chkRS485Alarms.Checked = e.Filters.Contains(AlarmTypes.RS485Error);
            chkCOMAlarms.Checked = e.Filters.Contains(AlarmTypes.SerialPortError);
            chkTempMissingAlarm.Checked = e.Filters.Contains(AlarmTypes.TempMissing);

            foreach (CheckBox chk in tabCCALEL.TabPages["tabPageCC"].Controls["groupBox2"].Controls.OfType<CheckBox>())
            {
                chk.CheckedChanged += chkAlarmFilter_CheckedChanged;
            }
        }

        void LoadConfig()
        {
            for (int i = 0; i < cmbCOM.Items.Count;i++ )
            {
                if (Properties.Settings.Default.COMPort == ((string)cmbCOM.Items[i]))
                {
                    cmbCOM.SelectedIndex = i;
                }
            }
            for (int i = 0; i < cmbMPri.Items.Count; i++)
            {
                if (Properties.Settings.Default.Priority.ToString() == ((string)cmbMPri.Items[i])
                    || ((Properties.Settings.Default.Priority == 0) && (((string)cmbMPri.Items[i]) == "None")))
                {
                    cmbMPri.SelectedIndex = i;
                }
            }
            mDataEngine.deAlarmManager.TempLimitLoLo = Properties.Settings.Default.TempLimitLoLo;
            mDataEngine.deAlarmManager.TempLimitLo = Properties.Settings.Default.TempLimitLo;
            mDataEngine.deAlarmManager.TempLimitHi = Properties.Settings.Default.TempLimitHi;
            mDataEngine.deAlarmManager.TempLimitHiHi = Properties.Settings.Default.TempLimitHiHi;
            txtLLLvl.Text = Properties.Settings.Default.TempLimitLoLo.ToString();
            txtLLvl.Text = Properties.Settings.Default.TempLimitLo.ToString();
            txtHLvl.Text = Properties.Settings.Default.TempLimitHi.ToString();
            txtHHLvl.Text = Properties.Settings.Default.TempLimitHiHi.ToString();
            txtHosts.Text = Properties.Settings.Default.Hosts.Replace(",", System.Environment.NewLine);

            startDataEngine();
        }

        void SaveConfig()
        {
            if (cmbCOM.SelectedItem != null)
                Properties.Settings.Default.COMPort = (string)cmbCOM.SelectedItem;
            if (cmbMPri.SelectedItem != null && ((string)cmbMPri.SelectedItem) != "None")
                Properties.Settings.Default.Priority = Convert.ToInt16((string)cmbMPri.SelectedItem);
            Properties.Settings.Default.Hosts = txtHosts.Text.Replace(System.Environment.NewLine, ",").Trim();
            Properties.Settings.Default.Save();
        }

        void SaveTempLimits()
        {
            double hi, hihi, lo, lolo;
            if (Double.TryParse(txtHLvl.Text, out hi) && Double.TryParse(txtHHLvl.Text, out hihi) && Double.TryParse(txtLLvl.Text, out lo) && Double.TryParse(txtLLLvl.Text, out lolo))
            {
                Properties.Settings.Default.TempLimitLoLo = lolo;
                Properties.Settings.Default.TempLimitLo = lo;
                Properties.Settings.Default.TempLimitHi = hi;
                Properties.Settings.Default.TempLimitHiHi = hihi;
            }
            Properties.Settings.Default.Save();
        }

        void deAlarmManager_TempLimitsChangedEvent(object sender, TempLimitsChangedEventArgs e)
        {
            if (this.InvokeRequired) // InvokeRequired is true if event is triggered from another thread
            {
                // In this case trigger the event using BeginInvoke which makes sure the event is handled by the main thread
                this.BeginInvoke((MethodInvoker)delegate
                {
                    deAlarmManager_TempLimitsChangedEvent(sender, e);
                });
                return;
            }
            txtLLLvl.Text = e.LoLoLimit.ToString();
            txtLLvl.Text = e.LoLimit.ToString();
            txtHLvl.Text = e.HiLimit.ToString();
            txtHHLvl.Text = e.HiHiLimit.ToString();

            SaveTempLimits();
        }

        void scpHost_MessageEvent(object sender, ScadaCommunicationProtocol.MessageEventArgs e)
        {
            if (this.InvokeRequired) // InvokeRequired is true if event is triggered from another thread
            {
                // In this case trigger the event using BeginInvoke which makes sure the event is handled by the main thread
                this.BeginInvoke((MethodInvoker)delegate
                {
                    scpHost_MessageEvent(sender, e);
                });
                return;
            }

            txtDebug.AppendText(e.Message + System.Environment.NewLine);
        }

        void updateAlarmgrid(DataGridView dg, List<Alarm> alarmList)
        {
            List<Alarm> selectedAlarms = new List<Alarm>();
            foreach (DataGridViewRow row in dg.SelectedRows)
            {
                selectedAlarms.Add((Alarm)row.DataBoundItem);
            }
            dg.DataSource = alarmList;
            dg.ClearSelection();
            foreach (DataGridViewRow row in dg.Rows)
            {
                Alarm alarm = (Alarm)row.DataBoundItem;
                if (selectedAlarms.Exists(al => al.Type == alarm.Type & al.Source == alarm.Source))
                {
                    row.Selected = true;
                }
            }
        }

        void deAlarmManager_AlarmsChangedEvent(object sender, AlarmsChangedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    deAlarmManager_AlarmsChangedEvent(sender, e);
                });
                return;
            }
            updateAlarmgrid(dGAllAlarms, e.Alarms);
            updateAlarmgrid(dGFilteredAlarms, e.FilteredAlarms);
        }
        private void startDataEngine()
        {
            mDataEngine.deScpHost.ClearHosts();
            foreach (string host in Properties.Settings.Default.Hosts.Split(new Char[] { ',' }))
            {
                mDataEngine.deScpHost.AddHost(host);
            }
            if (cmbCOM.SelectedItem != null && ((string)cmbCOM.SelectedItem) != "None"
                && cmbMPri.SelectedItem != null && ((string)cmbMPri.SelectedItem) != "None")
            {
                mDataEngine.Start(cmbCOM.SelectedItem.ToString(), Convert.ToInt32(cmbMPri.SelectedItem.ToString()));
            }
            else
            {
                mDataEngine.Start();
            }
        }

        private void DataEngineNewTempHandler(object sender, DataEngineNewTempArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    DataEngineNewTempHandler(sender, e);
                });
                return;
            }
            txtTemperature.Text = e.temp.ToString();
            chrtTemp.Series["SerTemp"].Points.AddXY(DateTime.Now,e.temp);
            //System.Diagnostics.Debug.WriteLine("form1: " + e.temp);
        }
        private void DataEngineNewTcpStatusHandler(object sender, DataEngineNewTcpStatusArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    DataEngineNewTcpStatusHandler(sender, e);
                });
                return;
            }
            txtTCP_IP_MS.Text = e.status;
        }

        private void DataEngineNewComStatusHandler(object sender, DataEngineNewComStatusArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    DataEngineNewComStatusHandler(sender, e);
                });
                return;
            }
            txtRS485_MS.Text = e.status;
        }

        public void serialPortNames()
        {
            // Get a list of serial port names. 
            List<string> ports;
            ports = SerialPort.GetPortNames().ToList();
            if (!ports.Contains(Properties.Settings.Default.COMPort))
            {
                ports.Insert(0,Properties.Settings.Default.COMPort);
            }
            ports.Insert(0, "None");
            
            cmbCOM.DataSource = ports;
        }
        private void dGAllAlarms_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            DataGridView dg = (DataGridView)sender;
            if (e.RowIndex < dg.RowCount)
            {
                if (e.RowIndex < dg.RowCount)
                {
                    fifthSem.Alarm alarm = (fifthSem.Alarm)dg.Rows[e.RowIndex].DataBoundItem;
                    if (alarm.Type == fifthSem.AlarmTypes.TempHiHi || alarm.Type == fifthSem.AlarmTypes.TempLoLo)
                    {
                        dg.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
                        dg.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.White;
                    }
                    else if (alarm.Type == fifthSem.AlarmTypes.TempHi || alarm.Type == fifthSem.AlarmTypes.TempLo)
                    {
                        dg.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                        dg.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
                    }
                    else
                    {
                        dg.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Black;
                        dg.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.White;
                    }
                } 
            }
        }

        private void chkAlarmFilter_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            switch (chk.Name)
            {
                case "chkTempAlarms":
                    mDataEngine.deAlarmManager.SetAlarmFilter(new AlarmTypes[] { AlarmTypes.TempHi,
                                                                                 AlarmTypes.TempHiHi,
                                                                                 AlarmTypes.TempLo,
                                                                                 AlarmTypes.TempLoLo }, chkTempAlarms.Checked);
                    break;
                case "chkRS485Alarms":
                    mDataEngine.deAlarmManager.SetAlarmFilter(new AlarmTypes[] { AlarmTypes.RS485Error }, chkRS485Alarms.Checked);
                    break;
                case "chkCOMAlarms":
                    mDataEngine.deAlarmManager.SetAlarmFilter(new AlarmTypes[] { AlarmTypes.SerialPortError }, chkCOMAlarms.Checked);
                    break;
                case "chkHostMissingAlarms":
                    mDataEngine.deAlarmManager.SetAlarmFilter(new AlarmTypes[] { AlarmTypes.HostMissing }, chkHostMissingAlarms.Checked);
                    break;
                case "chkTempMissingAlarm":
                    mDataEngine.deAlarmManager.SetAlarmFilter(new AlarmTypes[] { AlarmTypes.TempMissing }, chkTempMissingAlarm.Checked);
                    break;
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            SaveConfig();
            mDataEngine.stop();
            startDataEngine();
        }

        private void btnSetLimits_Click_1(object sender, EventArgs e)
        {
            double hi,hihi,lo,lolo;
            if (Double.TryParse(txtHLvl.Text, out hi) && Double.TryParse(txtHHLvl.Text, out hihi) && Double.TryParse(txtLLvl.Text, out lo) && Double.TryParse(txtLLLvl.Text, out lolo))
            {
                if (hi >= hihi || lo >= hi || lolo >= lo)
                {
                    MessageBox.Show("Limits must be: HiHi>Hi>Lo>LoLo!");
                    return;
                }
                mDataEngine.deAlarmManager.TempLimitHi = hi;
                mDataEngine.deAlarmManager.TempLimitHiHi = hihi;
                mDataEngine.deAlarmManager.TempLimitLo = lo;
                mDataEngine.deAlarmManager.TempLimitLoLo = lolo;
                SaveTempLimits();
            }
        }

        private void ackAlarms(DataGridView dg)
        {
            List<Alarm> alarms = new List<Alarm>();
            foreach (DataGridViewRow row in dg.SelectedRows)
            {
                if (row.Index != -1)
                    alarms.Add((Alarm)row.DataBoundItem);
            }
            foreach (Alarm alarm in alarms)
            {
                mDataEngine.deAlarmManager.SetAlarmStatus(alarm.Type, AlarmCommand.Ack, alarm.Source);
            }
        }

        private void btnAck_Click(object sender, EventArgs e)
        {
            ackAlarms(dGFilteredAlarms);
        }

        private void Acknowledge2_Click(object sender, EventArgs e)
        {
            ackAlarms(dGAllAlarms);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mDataEngine.deScpHost.RequestSwitchToMaster();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }
             
    }
}
