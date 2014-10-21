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
    #region DataEngine Events 
    //... Used by GUI
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
    #endregion

    class DataEngine
    {
        private string logFile, logFilePath, logFolder = @"\Loggs\"; //%USERPROFILE%\My Documents
        private long logFileSize = 0;
        private bool timerAlarmHigh, deStarted;
        private ScpHost mScpHost;
        private RS485.RS485 mRS485;
        private DateTime lastLog;
        private Timer mTimer;
        private AlarmManager mAlarmManager;
        //this AlarmManager object is used in the GUI. 
        //Since it's created here, it needs to be available for the GUI via this GET
        public AlarmManager deAlarmManager { get{return mAlarmManager; } }
        public ScpHost deScpHost { get { return mScpHost; } }

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
            logFileCheck();

            //timer init, used to detect missing temperaturereading
            mTimer = new Timer(3000);
            mTimer.Elapsed += mTimer_Elapsed;
            mTimer.Enabled = false;

            //creates objects of protocols
            mScpHost = new ScpHost(0); //default prio is 0, if this pc wants another prio, it'll be passed on later. 
            mAlarmManager = new AlarmManager(mScpHost);
            mRS485 = new RS485.RS485(); //passing prio later here aswell.
            //mScpHost.CanBeMaster = false;

            //Add hosts allowed to connect to the network. 
            //Hardcoded temporarily. 
            mScpHost.AddHost("OUROBORUS-PC");
            mScpHost.AddHost("WIN-9S0NTTTKCR6");
            mScpHost.AddHost("WALDEJER-PC");
            mScpHost.AddHost("ANDERS");
            mScpHost.AddHost("HILDE-PC");
            mScpHost.AddHost("FREDRIK");

            deStarted = false;
        }

        /// <summary>
        /// Starts the DataEngine WITHOUT com 
        /// </summary>
        public void Start()
        {
            if (!deStarted)
            {
                //subscribe to events
                mScpHost.ScpConnectionStatusEvent += ConnectionStatusHandler;
                mScpHost.PacketEvent += PacketHandler;
                mRS485.ConnectionStatusHandler += ConnectionStatusRS485Handler;

                //starts protocols
                mScpHost.Start();
                deStarted = true;
            }
        }

        /// <summary>
        /// Starts the DataEngine WITH com
        /// </summary>
        /// <param name="portNr">comport to use</param>
        public void Start(string portNr, int ComputerPriority)
        {
            if (!deStarted)
            {
                //subscribe to events
                mScpHost.ScpConnectionStatusEvent += ConnectionStatusHandler;
                mScpHost.PacketEvent += PacketHandler;
                mScpHost.SlaveConnectionEvent += SlaveConnectionHandler;
                mRS485.TempHandler += TempEventHandler;
                mRS485.AlarmHandler += AlarmEventHandler;
                mRS485.ConnectionStatusHandler += ConnectionStatusRS485Handler;

                mScpHost.CanBeMaster = true;

                //starts protocols
                mScpHost.Start();
                mRS485.startCom(portNr, 9600, 8, Parity.None, StopBits.One, Handshake.None);

                //passing priority to protocols. 
                mRS485.ComputerAddress = ComputerPriority;
                ScpHost.Priority = ComputerPriority;
                deStarted = true;
                mScpHost.CanBeMaster = true;
            }
        }

        /// <summary>
        /// Making sure everything stops when told. 
        /// </summary>
        private void stop()
        {
            deStarted = false;
            mTimer.Stop();
            mTimer.Dispose();
            mRS485.stopCom();
        }

        /// <summary>
        /// This event is triggered when DataEngine does not recieve a new temperaturereading after a given timeinterval. 
        /// </summary>
        private void mTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timerAlarmHigh = true;
            mAlarmManager.SetAlarmStatus(AlarmTypes.TempMissing, AlarmCommand.High);
            Debug.WriteLine(this, "DataEngine: TempMissing!");
        }

        #region ComEvents
        private void TempEventHandler(object sender, RS485.TempEventArgs e)
        {
            if (mNewTempHandler != null) mNewTempHandler(this, new DataEngineNewTempArgs(e.temp));
            if (mScpHost.ScpConnectionStatus == ScpConnectionStatus.Master)
            {
                mScpHost.SendBroadcastAsync(new ScpTempBroadcast(e.temp));
                mAlarmManager.SetTemp(e.temp);
            }
            writeTempToLog(e.temp);
        }

        private void AlarmEventHandler(object sender, RS485.AlarmEventArgs e)
        {
            bool err = false;
            switch (e.alarm)
            { 
                case RS485.AlarmStatus.ComportFailure:
                    err = true;
                    mAlarmManager.SetAlarmStatus(AlarmTypes.SerialPortError, AlarmCommand.High, ScpHost.Name);
                    break;
                case RS485.AlarmStatus.RS485Failure:
                    err = true;
                    mAlarmManager.SetAlarmStatus(AlarmTypes.RS485Error, AlarmCommand.High, ScpHost.Name);
                    break;
                case RS485.AlarmStatus.None:
                    mAlarmManager.SetAlarmStatus(AlarmTypes.SerialPortError, AlarmCommand.Low, ScpHost.Name);
                    mAlarmManager.SetAlarmStatus(AlarmTypes.RS485Error, AlarmCommand.Low, ScpHost.Name);
                    break;
            }
            //Here the master is in trouble and broadcast a request for someone to take over. 
            //If no one can take over, the network still needs a master, and this master will remain.
            if (err && mScpHost.ScpConnectionStatus == ScpConnectionStatus.Master)
            { 
                //send broadcast asking for new master candidate. 
                //response is recieved in packethandler, case master. if no response, this master will remain.
                //rs485 status event will also make sure that this master will no longer be a candidate.
            }
        }

        private void ConnectionStatusRS485Handler(object sender, RS485.ConnectionStatusEventArgs e)
        {
            switch (e.status)
            {
                case RS485.ConnectionStatus.Master:
                    mTimer.Start();
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Master"));
                    //mScpHost.CanBeMaster = true;
                    break;
                case RS485.ConnectionStatus.Slave:
                    mTimer.Stop();
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Slave"));
                    //mScpHost.CanBeMaster = true;
                    break;
                case RS485.ConnectionStatus.Waiting:
                    mTimer.Stop();
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Waiting"));
                    //mScpHost.CanBeMaster = false;
                    break;
                case RS485.ConnectionStatus.Stop:
                    mTimer.Stop();
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Stop"));
                    //mScpHost.CanBeMaster = false;
                    break;
            }
        }
        #endregion
        #region ScpEvents
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

        private void PacketHandler(object sender, ScpPacketEventArgs e)
        {
            switch (mScpHost.ScpConnectionStatus)
            {
                case ScpConnectionStatus.Master:
                    if(e.Packet is ScpLogFileRequest)
                    {
                        if (logFileSize > ((ScpLogFileRequest)e.Packet).FileSize)
                            e.Response = new ScpLogFileResponse(File.ReadAllBytes(logFilePath));
                        else
                            e.Response = new ScpLogFileResponse(null);
                    }
                    else if(e.Packet is ScpAlarmLimitBroadcast)
                    {
                    //endring av alarmgrenser
                    }
                    else if (e.Packet is ScpMasterRequest)
                    {
                        e.Response = new ScpMasterResponse(true);
                    }
                    break;
                case ScpConnectionStatus.Slave:
                    if (e.Packet is ScpTempBroadcast && 
                        (mRS485.connectionStatus_extern != RS485.ConnectionStatus.Master || 
                        mRS485.connectionStatus_extern != RS485.ConnectionStatus.Slave))
                    {
                        if (mNewTempHandler != null) mNewTempHandler(this, new DataEngineNewTempArgs(((ScpTempBroadcast)e.Packet).Temp)); 
                        writeTempToLog(((ScpTempBroadcast)e.Packet).Temp);
                    }
                    else if (e.Packet is ScpAlarmLimitBroadcast)
                    {
                        //endring av alarmgrenser
                    }
                    //else if (e.Packet is ) MASTER TA OVER LOGIKK HER
                    break;
                case ScpConnectionStatus.Waiting:
                    //kommer vel aldri til å skje? 
                    break;
            }
        }
        #endregion
        #region Logfile Methods
        private async void logSync()
        {
            ScpPacket response = null; 
            try
            {
                response = await mScpHost.SendRequestAsync(new ScpLogFileRequest(logFileSize));
            }
            catch (Exception e)
            {
                Debug.WriteLine("DataEngine: LogfileSync, timeout eller ikke nødvendig med sync.");
                Debug.WriteLine("DataEngine: " + e.ToString());
            }
            if ((response != null) && (response is ScpLogFileResponse))
            {
                if (((ScpLogFileResponse)response).File != null)
                {
                    try
                    {
                        //File.Move(logFilePath, logFilePath + "BACKUP");
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
                logFileSize = fi.Length;
            }
            else if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
        }
        #endregion
    }
}
