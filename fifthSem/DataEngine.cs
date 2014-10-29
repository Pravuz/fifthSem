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
    //These events are only used by the programs GUI
    #region DataEngine Events
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

    /// <summary>
    /// The DataEngine handles and manages datalogging, status, 
    /// synchronization of loggs and some alarms.
    /// Creates a folder for logging on C:/Loggs/ 
    /// Provides events for GUI updates. 
    /// </summary>
    class DataEngine
    {
        private string portNr, logFile, logFilePath, logFolder = @"\Loggs\"; //%USERPROFILE%\My Documents
        private long logFileSize = 0;
        private bool timerAlarmHigh, deStarted, comTrouble, comErr;
        private ScpHost mScpHost;
        private RS485.RS485 mRS485;
        private DateTime lastLog;
        private Timer mTimer;
        private AlarmManager mAlarmManager;
        //this AlarmManager object is used in the GUI. 
        //Since it's created here, it needs to be available for the GUI via this GET
        public AlarmManager deAlarmManager { get { return mAlarmManager; } }
        public ScpHost deScpHost { get { return mScpHost; } }

        public event DataEngineNewTempHandler mNewTempHandler;
        public event DataEngineNewComStatusHandler mNewComStatusHandler;
        public event DataEngineNewTcpStatusHandler mNewTcpStatusHandler;
        public event DataEngineMessageHandler mMessageHandler;

        public DataEngine()
        {
            //logfile init. uses month and year for filename
            DateTime d = DateTime.Now;
            logFile = d.Month.ToString() + d.Year.ToString() + ".txt";
            logFilePath = logFolder + logFile;
            logFileCheck();

            //timer init, used to detect missing temperaturereading
            mTimer = new Timer(3000);
            mTimer.Elapsed += mTimer_Elapsed;
            mTimer.Enabled = false;

            //creates objects of communication protocols
            mScpHost = new ScpHost(0); //default prio is 0, if this pc wants another prio, it'll be passed on later. 
            mAlarmManager = new AlarmManager(mScpHost);
            mRS485 = new RS485.RS485(); //passing prio later here aswell.

            //subscribe to events
            mScpHost.ScpConnectionStatusEvent += ConnectionStatusHandler;
            mScpHost.PacketEvent += PacketHandler;
            mRS485.ConnectionStatusHandler += ConnectionStatusRS485Handler;

            //initial booleans used to avoid multiple startups and masters.
            deStarted = false;
            comTrouble = false;
            comErr = false;
        }

        /// <summary>
        /// Starts the DataEngine WITHOUT com 
        /// </summary>
        public void Start()
        {
            if (!deStarted)
            {
                //starts protocols
                mScpHost.Start();
                deStarted = true;
                mScpHost.CanBeMaster = false;
            }
        }

        /// <summary>
        /// Starts the DataEngine WITH com
        /// </summary>
        /// <param name="portNr">Comport to use</param>
        /// <param name="ComputerPriority">Computers priority</param>
        public void Start(string portNr, int ComputerPriority)
        {
            if (!deStarted)
            {
                mAlarmManager.SetAlarmStatus(AlarmTypes.SerialPortError, AlarmCommand.Low, ScpHost.Name);
                mAlarmManager.SetAlarmStatus(AlarmTypes.RS485Error, AlarmCommand.Low, ScpHost.Name);

                //subscribe to additional events
                mScpHost.SlaveConnectionEvent += SlaveConnectionHandler;
                mRS485.TempHandler += TempEventHandler;
                mRS485.AlarmHandler += AlarmEventHandler;

                //starts protocols
                mScpHost.Start();
                this.portNr = portNr;
                //mRS485.startCom(portNr, 9600, 8, Parity.None, StopBits.One, Handshake.None);

                //passing priority to protocols. 
                mRS485.ComputerAddress = ComputerPriority;
                ScpHost.Priority = ComputerPriority;
                deStarted = true;
                mScpHost.CanBeMaster = true; //Is by default true, just making sure.
            }
        }

        /// <summary>
        /// Making sure everything stops when told. 
        /// </summary>
        public void stop()
        {
            if (deStarted)
            {
                mScpHost.SlaveConnectionEvent -= SlaveConnectionHandler;
                mRS485.TempHandler -= TempEventHandler;
                mRS485.AlarmHandler -= AlarmEventHandler;
            }
            deStarted = false;
            mTimer.Stop();
            mRS485.stopCom();
            mScpHost.Stop();
        }

        /// <summary>
        /// This event is triggered when DataEngine does not recieve a new temperaturereading after a given timeinterval. 
        /// Initiates a RequestSwitchToMaster, which will only be completed if the computer is currently a Slave.
        /// </summary>
        private async void mTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timerAlarmHigh = true;
            if (mScpHost.ScpConnectionStatus == ScpConnectionStatus.Master) await mAlarmManager.SetAlarmStatus(AlarmTypes.TempMissing, AlarmCommand.High);
            if (mScpHost.CanBeMaster && 
                mScpHost.ScpConnectionStatus != ScpConnectionStatus.Master && 
                (mRS485.connectionStatus_extern == RS485.ConnectionStatus.Slave || 
                mRS485.connectionStatus_extern == RS485.ConnectionStatus.Master)) 
                await mScpHost.RequestSwitchToMaster();
            Debug.WriteLine(this, "DataEngine: TempMissing!");
            mTimer.Stop();
        }

        #region ComEvents
        /// <summary>
        /// Handles new temperaturereadings from Com-port. 
        /// If this computer is master, a broadcast will be sent with the temperature
        /// Writes data to log.
        /// </summary>
        private async void TempEventHandler(object sender, RS485.TempEventArgs e)
        {
            if (mNewTempHandler != null) mNewTempHandler(this, new DataEngineNewTempArgs(e.temp));
            if (mScpHost.ScpConnectionStatus == ScpConnectionStatus.Master)
            {
                await mScpHost.SendBroadcastAsync(new ScpTempBroadcast(e.temp));
                mAlarmManager.SetTemp(e.temp);
                if (timerAlarmHigh)
                {
                    await mAlarmManager.SetAlarmStatus(AlarmTypes.TempMissing, AlarmCommand.Low);
                    timerAlarmHigh = false;
                }
            }
            if (mScpHost.ScpConnectionStatus == ScpConnectionStatus.Master)
            {
                mTimer.Stop();
                mTimer.Start();
            }
            comErr = false;
            writeTempToLog(e.temp);
        }

        /// <summary>
        /// Handles alarms sent from the RS485 protocol. 
        /// If this computer is master, comTrouble will be set to true.
        /// This allows for a new computer with com to assume role as Master.
        /// </summary>
        private async void AlarmEventHandler(object sender, RS485.AlarmEventArgs e)
        {
            comErr = false;
            switch (e.alarm)
            {
                case RS485.AlarmStatus.ComportFailure:
                    if (mScpHost.ScpConnectionStatus == ScpConnectionStatus.Master) comTrouble = true;
                    comErr = true;
                    await mAlarmManager.SetAlarmStatus(AlarmTypes.SerialPortError, AlarmCommand.High, ScpHost.Name);
                    break;
                case RS485.AlarmStatus.RS485Failure:
                    if (mScpHost.ScpConnectionStatus == ScpConnectionStatus.Master) comTrouble = true;
                    comErr = true;
                    await mAlarmManager.SetAlarmStatus(AlarmTypes.RS485Error, AlarmCommand.High, ScpHost.Name);
                    break;
                case RS485.AlarmStatus.None:
                    await mAlarmManager.SetAlarmStatus(AlarmTypes.SerialPortError, AlarmCommand.Low, ScpHost.Name);
                    await mAlarmManager.SetAlarmStatus(AlarmTypes.RS485Error, AlarmCommand.Low, ScpHost.Name);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ConnectionStatusRS485Handler(object sender, RS485.ConnectionStatusEventArgs e)
        {
            switch (e.status)
            {
                case RS485.ConnectionStatus.Master:
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Master"));
                    break;
                case RS485.ConnectionStatus.Slave:
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Slave"));
                    break;
                case RS485.ConnectionStatus.Waiting:
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Waiting"));
                    break;
                case RS485.ConnectionStatus.Stop:
                    if (mNewComStatusHandler != null) mNewComStatusHandler(this, new DataEngineNewComStatusArgs("Stop"));
                    break;
            }
        }
        #endregion
        #region ScpEvents
        /// <summary>
        /// 
        /// </summary>
        private void SlaveConnectionHandler(object sender, SlaveConnectionEventArgs e)
        {
            if (mMessageHandler != null) mMessageHandler(this, new DataEngineMessageArgs("Slave " + e.Name + " Disconnected."));
        }

        /// <summary>
        /// 
        /// </summary>
        private void ConnectionStatusHandler(object sender, ScpConnectionStatusEventArgs e)
        {
            switch (e.Status)
            {
                case ScpConnectionStatus.Master:
                    if (!mRS485.ComportEnabled && portNr != null) mRS485.startCom(portNr, 9600, 8, Parity.None, StopBits.One, Handshake.None);
                    if (mNewTcpStatusHandler != null) mNewTcpStatusHandler(this, new DataEngineNewTcpStatusArgs("Master"));
                    mTimer.Stop();
                    mTimer.Start();
                    break;
                case ScpConnectionStatus.Slave:
                    if (!mRS485.ComportEnabled && portNr != null) mRS485.startCom(portNr, 9600, 8, Parity.None, StopBits.One, Handshake.None);
                    if (mNewTcpStatusHandler != null) mNewTcpStatusHandler(this, new DataEngineNewTcpStatusArgs("Slave"));
                    if (mScpHost.CanBeMaster)
                    {
                        mTimer.Stop();
                        mTimer.Start();
                    }
                    logSync();
                    break;
                case ScpConnectionStatus.Waiting:
                    if (mNewTcpStatusHandler != null) mNewTcpStatusHandler(this, new DataEngineNewTcpStatusArgs("Waiting"));
                    break;
                case ScpConnectionStatus.Stopped:
                    if (mNewTcpStatusHandler != null) mNewTcpStatusHandler(this, new DataEngineNewTcpStatusArgs("Stopped"));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void PacketHandler(object sender, ScpPacketEventArgs e)
        {
            switch (mScpHost.ScpConnectionStatus)
            {
                case ScpConnectionStatus.Master:
                    if (e.Packet is ScpLogFileRequest)
                    {
                        logFileCheck(); //updates filesize.
                        if (logFileSize > ((ScpLogFileRequest)e.Packet).FileSize)
                            e.Response = new ScpLogFileResponse(File.ReadAllBytes(logFilePath));
                        else
                            e.Response = new ScpLogFileResponse(null);
                    }
                    else if (e.Packet is ScpMasterRequest && comTrouble)
                    {
                        e.Response = new ScpMasterResponse(true);
                        comTrouble = false;
                    }
                    break;
                case ScpConnectionStatus.Slave:
                    if (e.Packet is ScpTempBroadcast &&
                        (mRS485.connectionStatus_extern != RS485.ConnectionStatus.Master ||
                        mRS485.connectionStatus_extern != RS485.ConnectionStatus.Slave || comErr))
                    {
                        if (mNewTempHandler != null) mNewTempHandler(this, new DataEngineNewTempArgs(((ScpTempBroadcast)e.Packet).Temp));
                        if (mScpHost.CanBeMaster)
                        {
                            mTimer.Stop();
                            mTimer.Start();
                        }
                        writeTempToLog(((ScpTempBroadcast)e.Packet).Temp);
                    }
                    break;
                case ScpConnectionStatus.Waiting:
                    //kommer vel aldri til å skje? 
                    break;
            }
        }
        #endregion
        #region Logfile Methods
        /// <summary>
        /// 
        /// </summary>
        private async void logSync()
        {
            logFileCheck(); //updates filesize.
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        private async void writeTempToLog(double s)
        {
            DateTime now = DateTime.Now;
            if (lastLog.AddSeconds(10) < now)
            {
                Debug.WriteLine("DataEngine: 10 sekunder siden sist logging");
                writeToFile(now + ";" + s);
                lastLog = now;
            }
        }

        /// <summary>
        /// Appends the desired text to the logfile. 
        /// If the logfile does not exists, it will be created. 
        /// </summary>
        /// <param name="s">Desired text to append.</param>
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

        /// <summary>
        /// Checkings wether or not a log file allready exists
        /// If it exists the filesize will be fetched. 
        /// If the folder that will contain the logfile does not exists,
        /// it will be created. 
        /// </summary>
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
            //no need to create the file, 
            //it will be created in the writeToFile method if it does not allready exists.
        }
        #endregion
    }
}
