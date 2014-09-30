using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fifthSem
{
    static class Program
    {
        static DataEngine mDataEngine;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            mDataEngine = new DataEngine();
            
            //kommentar
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }

        //public static void setPrio(int p) 
        //{
        //    mDataEngine.prio = p;
        //}

        //public static void setPort(string p)
        //{
        //    mDataEngine.portNr = p;
        //}

        public static void startDataEngine()
        {
            mDataEngine.Start();
        }
        public static void startDataEngine(string s)
        {
            mDataEngine.Start(s);
        }

    }
}
