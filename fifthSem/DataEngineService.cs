using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScadaCommunicationProtocol;
using System.IO.Ports;

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
        private RS485.RS485 mRS485;
        //private alarmhost malarmhost;

        public DataEngineService()
        {
            this.ServiceName = "fifthSemDataEngineService";
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;

            ScpStatus = ScpMode.WAITING;

            DateTime d = DateTime.Now;
            logFile = d.Month.ToString() + d.Year.ToString() + ".txt";

            mScpHost = new ScpHost(1);
            mRS485 = new RS485.RS485();

        }

        public static void Main()
        {
            System.ServiceProcess.ServiceBase.Run(new DataEngineService());
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            //subscribe to events
            mScpHost.ScpConnectionStatusEvent += ConnectionStatusHandler;
            mScpHost.PacketEvent += PacketHandler;
            mRS485.TempHandler += TempEventHandler;
            mRS485.AlarmHandler += AlarmEventHandler;
            mRS485.ConnectionStatusHandler += ConnectionStatusRS485Handler;

            //starts protocols
            mScpHost.Start();
            mRS485.startCom("",9600,8, Parity.None, StopBits.None, Handshake.None);
            //todo: start alarmsystem

            if (ComConnected()) mScpHost.CanBeMaster = true;
            hostname = ScpHost.Name;
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

        private void TempEventHandler(object sender, RS485.TempEventArgs e)
        { }

        private void AlarmEventHandler(object sender, RS485.AlarmEventArgs e)
        { }

        private void ConnectionStatusRS485Handler(object sender, RS485.ConnectionStatusEventArgs e)
        { }

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
