using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScadaCommunicationProtocol;
using System.IO.Ports;
using System.Diagnostics;
using System.Timers;

namespace fifthSem
{
    public delegate void DataEngineNewTempHandler(object sender, DataEngineNewTempArgs e);
    public delegate void DataEngineNewTcpStatusHandler(object sender, DataEngineNewTcpStatusArgs e);
    public delegate void DataEngineNewComStatusHandler(object sender, DataEngineNewComStatusArgs e);
    public delegate void DataEngineMessageHandler(object sender, DataEngineMessageArgs e);

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

    public class DataEngineMessageArgs : EventArgs
    {
        public string message;
        public DataEngineMessageArgs(string message)
        {
            this.message = message;
        }
    }

    class DataEngine
    {
        public static string hostname;
        //public int tempAlarmHigh, tempAlarmLow;

        private string logFile, logFilePath, logFolder = @"\Loggs\"; //%USERPROFILE%\My Documents
        private long sizeOfFile = 0;
        private ScpHost mScpHost;
        private RS485.RS485 mRS485;
        private DateTime lastLog;
        private Timer mTimer;
        private bool timerAlarmHigh;
        private AlarmManager mAlarmManager;
        //private alarmhost malarmhost;

        public event DataEngineNewTempHandler mNewTempHandler;
        public event DataEngineNewComStatusHandler mNewComStatusHandler;
        public event DataEngineNewTcpStatusHandler mNewTcpStatusHandler;
        public event DataEngineMessageHandler mMessageHandler;

        public DataEngine()
        {
            //logfile init
            DateTime d = DateTime.Now;
            logFile = d.Month.ToString() + d.Year.ToString() + ".txt";
            logFilePath = logFolder + logFile;
            //mFile = new FileInfo(logFilePath);
            logFileCheck();

            mTimer = new Timer(3000);
            mTimer.Elapsed += mTimer_Elapsed;
            mTimer.Enabled = false;

            //creates objects of protocols
            mScpHost = new ScpHost(1);
            mAlarmManager = new AlarmManager(mScpHost);
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

            //if (mRS485.connectionStatus_extern != RS485.ConnectionStatus.Waiting) mScpHost.CanBeMaster = true; //this if-test will most likely never be true, but event will handle this later.

        }

        /// <summary>
        /// Starts the DataEngine WITH com
        /// </summary>
        /// <param name="portNr">comport to use</param>
        public void Start(string portNr, int ComputerAdress)
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

            mRS485.ComputerAddress = ComputerAdress; //need real prio from GUI
            hostname = ScpHost.Name;

            //if (mRS485.connectionStatus != RS485.ConnectionStatus.Waiting) mScpHost.CanBeMaster = true; //this if-test will most likely never be true, but event will handle this later.
        }

        private void stop()
        {
            //cleanup
            mTimer.Stop();
            mTimer.Dispose();
            mRS485.stopCom();
        }

        private void mTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timerAlarmHigh = true;
            mAlarmManager.SetAlarmStatus(AlarmTypes.TempMissing, AlarmCommand.High);
            Debug.WriteLine(this, "DataEngine: TempMissing!");
        }

        //
        //ComEvents Start
        //

        private void TempEventHandler(object sender, RS485.TempEventArgs e)
        {
            //Debug.WriteLine("DataEngine: lest av temp, skriver til event. temp: " + e.temp);
            if (mNewTempHandler != null) mNewTempHandler(this, new DataEngineNewTempArgs(e.temp)); 
            if (mScpHost.ScpConnectionStatus == ScpConnectionStatus.Master) mScpHost.SendBroadcastAsync(new ScpTempBroadcast(e.temp));
            writeTempToLog(e.temp);
        }

        private void AlarmEventHandler(object sender, RS485.AlarmEventArgs e)
        {
            switch (e.alarm)
            { 
                case RS485.AlarmStatus.ComportFailure:
                    mAlarmManager.SetAlarmStatus(AlarmTypes.SerialPortError, AlarmCommand.High, ScpHost.Name); 
                    break;
                case RS485.AlarmStatus.RS485Failure:
                    mAlarmManager.SetAlarmStatus(AlarmTypes.RS485Error, AlarmCommand.High, ScpHost.Name); 
                    break;
                case RS485.AlarmStatus.None:
                    mAlarmManager.SetAlarmStatus(AlarmTypes.SerialPortError, AlarmCommand.Low, ScpHost.Name);
                    mAlarmManager.SetAlarmStatus(AlarmTypes.RS485Error, AlarmCommand.Low, ScpHost.Name);
                    break;
            }  
        }

        private void ConnectionStatusRS485Handler(object sender, RS485.ConnectionStatusEventArgs e)
        {
            switch (e.status)
            {
                case RS485.ConnectionStatus.Master:
                    mTimer.Start();
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Master"));
                    mScpHost.CanBeMaster = true;
                    break;
                case RS485.ConnectionStatus.Slave:
                    mTimer.Stop();
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Slave"));
                    mScpHost.CanBeMaster = true;
                    break;
                case RS485.ConnectionStatus.Waiting:
                    mTimer.Stop();
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
            if (mMessageHandler != null) mMessageHandler(this, new DataEngineMessageArgs("Slave " + e.Name + " Disconnected."));
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
                    logSync();
                    break;
                case ScpConnectionStatus.Waiting:
                    if (mNewTcpStatusHandler != null) mNewTcpStatusHandler(this, new DataEngineNewTcpStatusArgs("Waiting"));
                    break;
                default:
                    break;
            }
        }

        private async void logSync()
        {
            ScpPacket response = await mScpHost.SendRequestAsync(new ScpLogFileRequest(sizeOfFile));
            if ((response != null) && (response is ScpLogFileResponse)) 
            {
                if (((ScpLogFileResponse)response).File != null)
                {
                    try
                    {
                        File.Move(logFilePath, logFilePath + "BACKUP");
                        File.WriteAllBytes(logFilePath, ((ScpLogFileResponse)response).File);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(this, "DataEngine: " + ex.ToString());
                        //fikk ikke utført synkronisering av logg. 
                        //messagebox med prøv igjen kanskje?
                    }
                }   
            }   
        }

        private void PacketHandler(object sender, ScpPacketEventArgs e)
        {
            switch (mScpHost.ScpConnectionStatus)
            {
                case ScpConnectionStatus.Master:
                    if(e.Packet is ScpLogFileRequest)
                    {
                        if (sizeOfFile > ((ScpLogFileRequest)e.Packet).FileSize)
                            e.Response = new ScpLogFileResponse(File.ReadAllBytes(logFilePath));
                    }
                    else if(e.Packet is ScpAlarmLimitBroadcast)
                    {
                    //endring av alarmgrenser
                    }
                    break;
                case ScpConnectionStatus.Slave:
                    if (e.Packet is ScpTempBroadcast && 
                        (mRS485.connectionStatus_extern != RS485.ConnectionStatus.Master || 
                        mRS485.connectionStatus_extern != RS485.ConnectionStatus.Slave))
                    {
                        writeTempToLog(((ScpTempBroadcast)e.Packet).Temp);
                    }
                    else if (e.Packet is ScpAlarmLimitBroadcast)
                    {
                        //endring av alarmgrenser
                    }
                    break;
                case ScpConnectionStatus.Waiting:
                    //kommer vel aldri til å skje? 
                    break;
            }
        }

        //
        //ScpEvents Stop
        //

        private void writeTempToLog(double s)
        {
            DateTime now = DateTime.Now;
            mTimer.Stop();
            mTimer.Start();
            if (timerAlarmHigh)
            {
                mAlarmManager.SetAlarmStatus(AlarmTypes.TempMissing, AlarmCommand.Low);
                timerAlarmHigh = false;
            }
            
            if (lastLog != null)
            {
                if (lastLog.AddSeconds(10) < now)
                {
                    Debug.WriteLine("DataEngine: 10 sekunder siden sist logging");
                    writeToFile("Temperature reading " + DateTime.Now + ": " + s);
                }
            }
            else writeToFile("Temperature reading " + DateTime.Now + ": " + s);
            lastLog = now;
        }

        private void writeToFile(string s)
        {
            try
            {
                using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(s);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(this, "DataEngine: " + e.ToString());
            }
        }

        private void logFileCheck()
        {
            FileInfo fi;
            if (File.Exists(logFilePath))
            {
                fi = new FileInfo(logFilePath);
                sizeOfFile = fi.Length;
            }
            else if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

        }
    }
}
