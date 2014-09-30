using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScadaCommunicationProtocol;
using System.IO.Ports;
using System.Diagnostics;

namespace fifthSem
{
    public delegate void DataEngineNewTempHandler(object sender, DataEngineNewTempArgs e);
    public delegate void DataEngineNewTcpStatusHandler(object sender, DataEngineNewTcpStatusArgs e);
    public delegate void DataEngineNewComStatusHandler(object sender, DataEngineNewComStatusArgs e);

    public class DataEngineNewTempArgs : EventArgs
    {
        public double temp;
        public DataEngineNewTempArgs(double temp)
        {
            this.temp = temp;
        }
    }

    public class DataEngineNewTcpStatusArgs : EventArgs
    {
        public string status;
        public DataEngineNewTcpStatusArgs(string status)
        {
            this.status = status;
        }
    }

    public class DataEngineNewComStatusArgs : EventArgs
    {
        public string status;
        public DataEngineNewComStatusArgs(string status)
        {
            this.status = status;
        }
    }

    class DataEngine
    {
        public static string hostname;
        //public int tempAlarmHigh, tempAlarmLow;

        private string logFile, logFilePath, logFolder = @"%USERPROFILE%\My Documents\Loggs\";
        private long sizeOfFile = 0;
        private ScpHost mScpHost;
        private RS485.RS485 mRS485;
        private FileInfo mFile;
        //private alarmhost malarmhost;

        public event DataEngineNewTempHandler mNewTempHandler;
        public event DataEngineNewComStatusHandler mNewComStatusHandler;
        public event DataEngineNewTcpStatusHandler mNewTcpStatusHandler;

        public DataEngine()
        {
            //waiting will be default mode until protocols are initialized.
            //ScpStatus = ScpMode.WAITING;
            //ComStatus = ComMode.WAITING;

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
        /// <summary>
        /// Starts the DataEngine WITHOUT com 
        /// </summary>
        public void Start() 
        {
            //subscribe to events
            mScpHost.ScpConnectionStatusEvent += ConnectionStatusHandler;
            mScpHost.PacketEvent += PacketHandler;

            //starts protocols
            mScpHost.Start();
            hostname = ScpHost.Name;

            if (mRS485.connectionStatus != RS485.ConnectionStatus.Waiting) mScpHost.CanBeMaster = true; //this if-test will most likely never be true, but event will handle this later.
        
        }

        /// <summary>
        /// Starts the DataEngine WITH com
        /// </summary>
        /// <param name="portNr">comport to use</param>
        public void Start(string portNr) 
        {
            //subscribe to events
            mScpHost.ScpConnectionStatusEvent += ConnectionStatusHandler;
            mScpHost.PacketEvent += PacketHandler;
            mScpHost.SlaveConnectionEvent += SlaveConnectionHandler;
            mRS485.TempHandler += TempEventHandler;
            mRS485.AlarmHandler += AlarmEventHandler;
            mRS485.ConnectionStatusHandler += ConnectionStatusRS485Handler;

            //starts protocols
            mScpHost.Start();
            mRS485.startCom(portNr, 9600, 8, Parity.None, StopBits.One, Handshake.None);
            //todo: start alarmsystem

            mRS485.ComputerAddress = 1; //need real prio from GUI
            hostname = ScpHost.Name;

            if (mRS485.connectionStatus != RS485.ConnectionStatus.Waiting) mScpHost.CanBeMaster = true; //this if-test will most likely never be true, but event will handle this later.
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
        {
            if(mNewTempHandler != null) mNewTempHandler(this, new DataEngineNewTempArgs(Convert.ToDouble(e.temp))); //upcoming update removes need to convert.
            writeToFile("Temperature reading " + DateTime.Now + ": " + e.temp);
            if (mScpHost.ScpConnectionStatus == ScpConnectionStatus.Master) mScpHost.SendBroadcastAsync(new ScpTempBroadcast(Convert.ToDouble(e.temp)));
            //skriv til logg
        }

        private void AlarmEventHandler(object sender, RS485.AlarmEventArgs e)
        {

        }

        private void ConnectionStatusRS485Handler(object sender, RS485.ConnectionStatusEventArgs e)
        {
            switch (e.status)
            {
                case RS485.ConnectionStatus.Master:
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Master"));
                    mScpHost.CanBeMaster = true;
                    break;
                case RS485.ConnectionStatus.Slave:
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Slave"));
                    mScpHost.CanBeMaster = true;
                    break;
                case RS485.ConnectionStatus.Waiting:
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Waiting"));
                    mScpHost.CanBeMaster = false;
                    break;
            }
        }

        //
        //ComEvents Stop
        //ScpEvents Start
        //

        private void SlaveConnectionHandler(object sender, SlaveConnectionEventArgs e)
        {
            
        }

        private void ConnectionStatusHandler(object sender, ScpConnectionStatusEventArgs e)
        {
            
            switch (e.Status)
            {
                case ScpConnectionStatus.Master:
                    if (mNewTcpStatusHandler != null) mNewTcpStatusHandler(this, new DataEngineNewTcpStatusArgs("Master"));
                    
                    break;
                case ScpConnectionStatus.Slave:
                    if (mNewTcpStatusHandler != null) mNewTcpStatusHandler(this, new DataEngineNewTcpStatusArgs("Slave"));
                    
                    break;
                case ScpConnectionStatus.Waiting:
                    if (mNewTcpStatusHandler != null) mNewTcpStatusHandler(this, new DataEngineNewTcpStatusArgs("Waiting"));
                    
                    break;
                default:
                    break;
            }
        }

        private void PacketHandler(object sender, ScpPacketEventArgs e)
        {
            switch (mScpHost.ScpConnectionStatus)
            { 
                case ScpConnectionStatus.Master:
                    break;
                case ScpConnectionStatus.Slave:
                    
                    break;
                case ScpConnectionStatus.Waiting:
                    break;
            }
        }

        //
        //ScpEvents Stop
        //

        /// <summary> Method for writing logg/alarm/information etc to file </summary>
        /// <returns> True if the operation was a success and vice/versa. </returns>
        private void writeToFile(string s)
        {
            try
            {
                mFile.AppendText().WriteLine(s);
            }
            catch (Exception e)
            {
                Debug.WriteLine(this, "DataEngine: " + e.ToString());
            }
        }

        private void logFileCheck()
        {
            //Debug.WriteLine(this, "DataEngine: " + mFile.FullName);
            mFile.Refresh();
            if (File.Exists(logFilePath))
                sizeOfFile = mFile.Length;
            else
            {
                Directory.CreateDirectory(logFolder);
                File.Create(logFilePath);
            }
        }
    }
}
