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
        public Form1()
        {
            InitializeComponent();
            serialPortNames();
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
            if (cmbCOM.SelectedItem != null) Program.startDataEngine();
            else Program.startDataEngine(cmbCOM.SelectedItem.ToString());
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
