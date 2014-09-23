using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScadaCommunicationProtocol;

namespace fifthSem
{
    class DataEngineService : System.ServiceProcess.ServiceBase
    {
        public static string hostname;
        public int hostId, tempAlarmHigh, tempAlarmLow;
        public enum ScpMode { MASTER, SLAVE, WAITING };
        public ScpMode ScpStatus;

        private string logFile, logFolder = @"%USERPROFILE%\My Documents\Loggs\";
        private int sizeOfFile;
        private ScpHost mScpHost;
        //private rs485host mrs485host;
        //private alarmhost malarmhost;

        public DataEngineService()
        {
            this.ServiceName = "fifthSemDEService";
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;

            ScpStatus = ScpMode.WAITING;

            DateTime d = DateTime.Now;
            logFile = d.Month.ToString() + d.Year.ToString() + ".txt";

            mScpHost = new ScpHost(1);

        }

        public static void Main()
        {
            System.ServiceProcess.ServiceBase.Run(new DataEngineService());
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            if (ComConnected()) mScpHost.CanBeMaster = true;
            mScpHost.ScpConnectionStatusEvent += ConnectionStatusHandler;
            mScpHost.PacketEvent += PacketHandler;
            hostname = ScpHost.Name;

            mScpHost.Start();
            //start rs485 og alarmsystem
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
            this.OnStop(); //?
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        private void ConnectionStatusHandler(object sender, ScpConnectionStatusEventArgs e) {
            string timeStamp = DateTime.Now.ToLongTimeString();

            switch (e.Status) { 
                case ScpConnectionStatus.Master:
                    ScpStatus = ScpMode.MASTER;
                    break;
                case ScpConnectionStatus.Slave:
                    ScpStatus = ScpMode.SLAVE;
                    break;
                case ScpConnectionStatus.Waiting:
                    ScpStatus = ScpMode.WAITING;
                    break;
                default:
                    break;
            }
        }

        private void PacketHandler(object sender, ScpPacketEventArgs e)
        {

        }

        private void TimerTask()
        {

        }

        /// <summary>
        /// Endrer program/klassevariabler basert på om bruker har huket av boks for tilkobling til rs485 (com)
        /// </summary>
        private bool ComConnected()
        {
            return true;
        }

        /// <summary> Method for writing logg/alarm/information etc to file </summary>
        /// <returns> True if the operation was a success and vice/versa. </returns>
        private bool writeToFile(string s)
        {
            bool success = true;
            try
            {
                File.AppendText(logFolder + logFile).Write(s);
            }
            catch (Exception e)
            {
                success = false;
            }
            return success;
        }        

    }
}
