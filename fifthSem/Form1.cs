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

        public string portNr { get; set; }
        public int prio { get; set; }

        public Form1()
        {
            InitializeComponent();
            serialPortNames();
        }

        private void btnSetLimits_Click(object sender, EventArgs e)
        {

        }

        private void btnSetPri_Click(object sender, EventArgs e)
        {
            prio = Convert.ToInt32(cmbMPri.SelectedItem.ToString());
        }

       
        public void serialPortNames()
        {
            // Get a list of serial port names. 
            string[] ports = SerialPort.GetPortNames();
            cmbCOM.DataSource = ports;
        }

        private void cmbCOM_SelectedIndexChanged(object sender, EventArgs e)
        {
            portNr = cmbCOM.SelectedItem.ToString();
        }


     
    }
}
