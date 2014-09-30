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
    class DataEngine
    {
        public static string hostname;
        public int hostId, tempAlarmHigh, tempAlarmLow;
        public enum ScpMode { MASTER, SLAVE, WAITING };
        public enum ComMode { MASTER, SLAVE, WAITING };
        public ScpMode ScpStatus;
        public ComMode ComStatus;

        private string logFile, logFilePath, logFolder = @"%USERPROFILE%\My Documents\Loggs\";
        private long sizeOfFile = 0;
        private ScpHost mScpHost;
        private RS485.RS485 mRS485;
        private FileInfo mFile;
        //private alarmhost malarmhost;

        public DataEngine()
        {
            //waiting will be default mode until protocols are initialized.
            ScpStatus = ScpMode.WAITING;
            ComStatus = ComMode.WAITING;
            
            //logfile init
            DateTime d = DateTime.Now;
            logFile = d.Month.ToString() + d.Year.ToString() + ".txt";
            logFilePath = logFolder + logFile;
            mFile = new FileInfo(logFilePath);
            logFileCheck();

            //creates objects of protocols
            mScpHost = new ScpHost(1);
            mRS485 = new RS485.RS485(); //passing prio later.
        }
        private void start() 
        {
            //subscribe to events
            mScpHost.ScpConnectionStatusEvent += ConnectionStatusHandler;
            mScpHost.PacketEvent += PacketHandler;
            mRS485.TempHandler += TempEventHandler;
            mRS485.AlarmHandler += AlarmEventHandler;
            mRS485.ConnectionStatusHandler += ConnectionStatusRS485Handler;

            //starts protocols
            mScpHost.Start();
            mRS485.startCom("", 9600, 8, Parity.None, StopBits.None, Handshake.None); //need real port from GUI
            //todo: start alarmsystem

            mRS485.ComputerAddress = 1; //need real prio from GUI
            hostname = ScpHost.Name;

            if (ComStatus != ComMode.WAITING) mScpHost.CanBeMaster = true; //this if-test will most likely never be true, but event will handle this later.
        
        }
        private void stop() 
        {
            //cleanup
            mRS485.stopCom();    
        }

        //
        //ComEvents Start
        //

        private void TempEventHandler(object sender, RS485.TempEventArgs e)
        { }

        private void AlarmEventHandler(object sender, RS485.AlarmEventArgs e)
        {

        }

        private void ConnectionStatusRS485Handler(object sender, RS485.ConnectionStatusEventArgs e)
        {
            switch (e.status)
            {
                case RS485.ConnectionStatus.Master:
                    mScpHost.CanBeMaster = true;
                    break;
                case RS485.ConnectionStatus.Slave:
                    mScpHost.CanBeMaster = true;
                    break;
                case RS485.ConnectionStatus.Waiting:
                    mScpHost.CanBeMaster = false;
                    break;
            }
        }

        //
        //ComEvents Stop
        //ScpEvents Start
        //

        private void ConnectionStatusHandler(object sender, ScpConnectionStatusEventArgs e)
        {
            string timeStamp = DateTime.Now.ToLongTimeString();

            switch (e.Status)
            {
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

        //
        //ScpEvents Stop
        //

        /// <summary> Method for writing logg/alarm/information etc to file </summary>
        /// <returns> True if the operation was a success and vice/versa. </returns>
        private bool writeToFile(string s)
        {
            bool success = true;
            try
            {
                File.AppendText(logFilePath).Write(s);
            }
            catch (Exception e)
            {
                success = false;
            }
            return success;
        }

        private void logFileCheck()
        {
            mFile.Refresh();
            if (File.Exists(logFilePath))
                sizeOfFile = mFile.Length;
            else
                File.Create(logFilePath);
        }
    }
}
