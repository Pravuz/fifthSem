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

        }
        private void startDataEngine()
        {
            mDataEngine.Start();
        }
        private void startDataEngine(string s)
        {
            mDataEngine.Start(s);
        }

        private void DataEngineNewTempHandler(object sender, DataEngineNewTempArgs e)
        {

        }
        private void DataEngineNewTcpStatusHandler(object sender, DataEngineNewTcpStatusArgs e)
        { 
        
        }

        private void DataEngineNewComStatusHandler(object sender, DataEngineNewComStatusArgs e)
        { 
        
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
            if (cmbCOM.SelectedItem != null) startDataEngine(cmbCOM.SelectedItem.ToString());
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
             
    }
}
