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
            //mDataEngine.mAlarmEventHandler += DataEngineAlarmEventHandler;
            mDataEngine.deAlarmManager.AlarmsChangedEvent += deAlarmManager_AlarmsChangedEvent;
            mDataEngine.deScpHost.MessageEvent += scpHost_MessageEvent;

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
            dGAllAlarms.DataSource = e.Alarms;
            dGFilteredAlarms.DataSource = e.FilteredAlarms;
        }
        private void startDataEngine()
        {
            mDataEngine.Start();
        }
        private void startDataEngine(string s,int p)
        {
            mDataEngine.Start(s,p);
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

        //private void DataEngineAlarmEventHandler(object sender, DataEngineAlarmEventArgs e)
        //{
        //    if (this.InvokeRequired)
        //    {
        //        this.BeginInvoke((MethodInvoker)delegate
        //        {
        //            DataEngineAlarmEventHandler(sender, e);
        //        });
        //        return;
        //    }
        //    txtAlarmList.AppendText(DateTime.Now + e.alarmType + e.alarmCommand + e.alarmSender + "\n");
        //}

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



        private void btnSetLimits_Click(object sender, EventArgs e)
        {
            double hHTemp, hTemp, lTemp, lLTemp;
            if ((Double.TryParse(txtHHLvl.Text, out hHTemp)) & (Double.TryParse(txtHLvl.Text, out hTemp)) & (Double.TryParse(txtLLvl.Text, out lTemp)) & (Double.TryParse(txtLLLvl.Text, out lLTemp)))
            { }
            else
            {
                MessageBox.Show("Incorrect Values!","" ,MessageBoxButtons.OK);
            }
        }

        private void btnSetPri_Click(object sender, EventArgs e)
        {
            //Program.setPrio(Convert.ToInt32(cmbMPri.SelectedItem.ToString()));
            if (cmbCOM.SelectedItem != null) startDataEngine(cmbCOM.SelectedItem.ToString(),Convert.ToInt32(cmbMPri.SelectedItem.ToString()));
            else startDataEngine();
        }

    
        public void serialPortNames()
        {
            // Get a list of serial port names. 

            string[] ports = SerialPort.GetPortNames();
            cmbCOM.DataSource = ports;
        }

        private void cmbCOM_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Program.setPort(cmbCOM.SelectedItem.ToString());
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

        private void chkTempAlarms_CheckedChanged(object sender, EventArgs e)
        {
            mDataEngine.deAlarmManager.SetAlarmFilter(AlarmTypes.TempHi, !chkTempAlarms.Checked);
            mDataEngine.deAlarmManager.SetAlarmFilter(AlarmTypes.TempHiHi, !chkTempAlarms.Checked);
            mDataEngine.deAlarmManager.SetAlarmFilter(AlarmTypes.TempLo, !chkTempAlarms.Checked);
            mDataEngine.deAlarmManager.SetAlarmFilter(AlarmTypes.TempLoLo, !chkTempAlarms.Checked);
        }

        private void chkRS485Alarms_CheckedChanged(object sender, EventArgs e)
        {
            mDataEngine.deAlarmManager.SetAlarmFilter(AlarmTypes.RS485Error, !chkRS485Alarms.Checked);
        }

        private void chkHostMissingAlarms_CheckedChanged(object sender, EventArgs e)
        {
            mDataEngine.deAlarmManager.SetAlarmFilter(AlarmTypes.HostMissing, !chkHostMissingAlarms.Checked);
        }

        private void chkCOMAlarms_CheckedChanged(object sender, EventArgs e)
        {
            mDataEngine.deAlarmManager.SetAlarmFilter(AlarmTypes.SerialPortError, !chkCOMAlarms.Checked);
        }

        private void chkTempMissingAlarm_CheckedChanged(object sender, EventArgs e)
        {
            mDataEngine.deAlarmManager.SetAlarmFilter(AlarmTypes.TempMissing, !chkTempMissingAlarm.Checked);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (cmbCOM.SelectedItem != null) startDataEngine(cmbCOM.SelectedItem.ToString(), Convert.ToInt32(cmbMPri.SelectedItem.ToString()));
            else startDataEngine();
        }
             
    }
}
