using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Timers;

//**********************INFO************************
//
// Klassen sørger for master/slave funksjon mot RS485.
// Lag et objekt av klassen. Nummerer datamaskinene fra 0, 1 ,2, osv, ved hjelp av property "ComputerAddress".
// Åpne comport ved å bruke funksjonen "startCom". Gi funksjonen alle comport innstillinger.
// Steng comport ved hjelp av funksjonen "stopCom".
// Tempdata mottas ved å opprette "TempEventHandler" event.
// Master/slave status mottas ved å opprette "ConnectionStatusEventHandler" event.
// Alarmer mottas ved å opprette "AlarmEventHandler" event.
//
// Laget av Alexander Waldejer
//**************************************************

namespace RS485
{
    public enum ConnectionStatus {Stop, Waiting, Master, Slave};
    public enum AlarmStatus {None, ComportFailure, RS485Failure};

    public delegate void TempEventHandler(object sender, TempEventArgs e);
    public delegate void ConnectionStatusEventHandler(object sender, ConnectionStatusEventArgs e);
    public delegate void AlarmEventHandler(object sender, AlarmEventArgs e);

    public class ConnectionStatusEventArgs : EventArgs
    {
        public ConnectionStatus status;
        public ConnectionStatusEventArgs(ConnectionStatus status)
        {
            this.status = status;
        }
    }

    public class AlarmEventArgs : EventArgs
    {
        public AlarmStatus alarm;
        public AlarmEventArgs(AlarmStatus alarm)

        {
            this.alarm = alarm;
        }
    }
    public class TempEventArgs : EventArgs
    {
        public double temp = 0.0;
        private string fortegn = "";
        public TempEventArgs(double temp, string fortegn)
        {
            this.temp = temp;
            this.fortegn = fortegn;
        }

        public override string ToString() //ToString() method gives complete formated temperature
        {
            return String.Format("{0}{1}°C", fortegn, temp);
        }
    }

    class RS485
    {
        // Serialport class, thread and timers
        private SerialPort serialPort;
        Thread getTempThread;
        System.Timers.Timer masterSlave;
        System.Timers.Timer timeout;

        // Private variables for class
        private int computerAddress;
        private int getTempTimeout = 0; // Flagg alarm when ConnectionStatus = Master and no temp has been received after 5 requests.
        private int getTempTimeoutCounter = 0; //
        private bool threadEnabled;
        private bool comportEnabled = false;
        private bool timeoutEnabled = false;
        private string tempData = "";

        // Property on Stop-Waiting-Master-Slave status
        public ConnectionStatus connectionStatus_extern;
        private ConnectionStatus connectionStatus_intern;
        
        // Property on AlarmStatus
        public AlarmStatus alarmStatus;
        public bool ComportEnabled { get { return comportEnabled; } }

        // Public eventhandlers for new temp, new connection status and new alarm
        public event TempEventHandler TempHandler;
        public event ConnectionStatusEventHandler ConnectionStatusHandler;
        public event AlarmEventHandler AlarmHandler;

        // Public property for ComputerAddress
        public int ComputerAddress { get { return computerAddress; } set { computerAddress = value; } }
        
        // Constructer for RS485 class
        public RS485()
        {
            // Default computer address
            computerAddress = 0;

            // Default com port connection status stop
            connectionStatus_extern = ConnectionStatus.Stop;
            if (null != ConnectionStatusHandler) ConnectionStatusHandler(this, new ConnectionStatusEventArgs(connectionStatus_extern));
        }
        
        // Private function ReadTimout. Depending on computeraddress, the time before before connectionstatus becomes master. 
        private int ReadTimeout(int ComputerAddress)
        {
            return (1 + ComputerAddress)*500;
        }

        // Private method for com-port connection
        public void startCom(string SetPortName, int SetBaudRate, int SetDataBits, Parity SetParity, StopBits SetStopBits, Handshake SetHandshake)
        {
            // Thread for datareading
            getTempThread = new Thread(new ThreadStart(GetTemp));
            getTempThread.IsBackground = true;

            try
            {
            // RS485 settings
            serialPort = new SerialPort();
            serialPort.PortName = SetPortName;
            serialPort.BaudRate = SetBaudRate;
            serialPort.Parity = SetParity;
            serialPort.DataBits = SetDataBits;
            serialPort.StopBits = SetStopBits;
            serialPort.Handshake = SetHandshake;

            // Serialport timout settings
            serialPort.ReadTimeout = 500;
            serialPort.WriteTimeout = 500;

            // Subscribe to event and open serial port for data
            serialPort.DataReceived +=
                new SerialDataReceivedEventHandler(serialPortDataReceived);
            serialPort.Open();
            comportEnabled = true;
            }
            catch
            {
                alarmStatus = AlarmStatus.ComportFailure;
                if (null != AlarmHandler) AlarmHandler(this, new AlarmEventArgs(alarmStatus));
            }
            // Set connectionStatus
            connectionStatus_intern = ConnectionStatus.Slave;
            connectionStatus_extern = ConnectionStatus.Waiting;
            if (null != ConnectionStatusHandler) ConnectionStatusHandler(this, new ConnectionStatusEventArgs(connectionStatus_extern));
            
            // Start thread
            threadEnabled = true;
            getTempThread.Start();

            // Start timout timer so that a new master is automatically chosen if the existing one is disconnected
            masterSlave = new System.Timers.Timer(ReadTimeout(computerAddress));
            masterSlave.Elapsed += MasterSlaveTimedEvent;
            masterSlave.Start();

            // Start timout timer for timeout if slave has not received temperature
            timeout = new System.Timers.Timer(4000);
            timeout.Elapsed += TimeoutTimedEvent;
            timeout.Start();
        }

        // Function to close down the com port and set connection status = Waiting. Flags a ConnectionStatusHandler event
        public void stopCom()
        {
            if (connectionStatus_extern != ConnectionStatus.Stop) // Com port must be opened before closed
            {
                masterSlave.Stop();
                threadEnabled = false;
                serialPort.Close();
                connectionStatus_intern = ConnectionStatus.Slave;
                connectionStatus_extern = ConnectionStatus.Stop;
                if (null != ConnectionStatusHandler) ConnectionStatusHandler(this, new ConnectionStatusEventArgs(connectionStatus_extern));
                comportEnabled = false;
            }
        }

        // Event for new serialport data received
        private void serialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (connectionStatus_intern == ConnectionStatus.Master)
            {
                connectionStatus_extern = ConnectionStatus.Master;
                if (null != ConnectionStatusHandler) ConnectionStatusHandler(this, new ConnectionStatusEventArgs(connectionStatus_extern));

                // Reset getTempTimeout and getTempTimeoutCounter
                getTempTimeout = 0;
                getTempTimeoutCounter = 0;
            }

            //  //Sets ConnectionStatus = Slave if not allready a Master
            //if (connectionStatus_intern != ConnectionStatus.Master)
            //{
            //    connectionStatus_intern = ConnectionStatus.Slave;
            //    connectionStatus_extern = ConnectionStatus.Slave;
            //    // Flags a ConnectionStatusHandler event
            //    if (null != ConnectionStatusHandler) ConnectionStatusHandler(this, new ConnectionStatusEventArgs(connectionStatus_extern));
            //}
           
            // Read data from port
            int dataLength = serialPort.BytesToRead;
            byte[] data = new byte[dataLength];
            serialPort.Read(data, 0, dataLength);

            // Read temp and remove unwanted data and make sure all of the data is received
            int indexStart, indexStop;
            string temp = "";
            tempData = tempData + Encoding.ASCII.GetString(data);
            //if(connectionStatus_intern == ConnectionStatus.Master & tempData.Contains("#"))
            //{
            if(tempData.Contains('#'))
            {
                if(connectionStatus_intern == ConnectionStatus.Master)
                {
                    connectionStatus_intern = ConnectionStatus.Slave;
                    connectionStatus_extern = ConnectionStatus.Slave;
                    if (null != ConnectionStatusHandler) ConnectionStatusHandler(this, new ConnectionStatusEventArgs(connectionStatus_extern));
                }

                // Restart master-slave timer
                masterSlave.Stop();
                masterSlave.Start();
            }

            if (tempData.Contains(">") & tempData.Contains("\r"))
            {
                indexStart = tempData.IndexOf(">");
                indexStop = tempData.IndexOf("\r",indexStart);
                if (indexStart < indexStop )
                {
                    string fortegn;
                    temp = tempData.Substring(2 + indexStart, indexStop - indexStart - 1);
                    fortegn = tempData.Substring(1 + indexStart,1);
                    temp = temp.TrimStart('0'); // Remove null in front of temp data
                    temp = temp.TrimEnd('\r');
                    temp = temp.Replace('.', ',');
                    try
                    {
                        if (null != TempHandler) TempHandler(this, new TempEventArgs(Convert.ToDouble(fortegn + temp), fortegn));
                    }
                    catch
                    {
                        if (null != TempHandler) TempHandler(this, new TempEventArgs(0,fortegn));
                    }
                    tempData = "";
                }

                //Sets ConnectionStatus = Slave if not allready a Master
                if (connectionStatus_intern != ConnectionStatus.Master)
                {
                    connectionStatus_intern = ConnectionStatus.Slave;
                    connectionStatus_extern = ConnectionStatus.Slave;
                    // Flags a ConnectionStatusHandler event
                    if (null != ConnectionStatusHandler) ConnectionStatusHandler(this, new ConnectionStatusEventArgs(connectionStatus_extern));
                }

                if (alarmStatus != AlarmStatus.None)
                {
                    alarmStatus = AlarmStatus.None;
                    if (null != AlarmHandler) AlarmHandler(this, new AlarmEventArgs(alarmStatus));
                }

                // Restart timeout timer
                timeout.Stop();
                timeout.Start();
                timeoutEnabled = false;

                //// Reset getTempTimeout and getTempTimeoutCounter
                //getTempTimeout = 0;
                //getTempTimeoutCounter = 0;
            }

            //// Restart master-slave timer
            //masterSlave.Stop();
            //masterSlave.Start();
        }

        // If a Slave has not received new temp, it automatically becomes Master. An event is flagged for connection status changed.
        private void MasterSlaveTimedEvent(Object source, ElapsedEventArgs e)
        {
            connectionStatus_intern = ConnectionStatus.Master;
        }

        // If a Slave has not received new temp, it automatically becomes Master. An event is flagged for connection status changed.
        private void TimeoutTimedEvent(Object source, ElapsedEventArgs e)
        {
            timeoutEnabled = true;
            connectionStatus_extern = ConnectionStatus.Waiting;
            if (null != ConnectionStatusHandler) ConnectionStatusHandler(this, new ConnectionStatusEventArgs(connectionStatus_extern));
            alarmStatus = AlarmStatus.RS485Failure;
            if (null != AlarmHandler) AlarmHandler(this, new AlarmEventArgs(alarmStatus));
        }

        // Threaded function. As a Master, request new temp from RS485.
        private void GetTemp()
        {
            while(threadEnabled == true)
            {
                if (connectionStatus_intern == ConnectionStatus.Master)
                {
                    // Com port surveillance. If com port is disconnected, an alarm event is flagged and a message i sent out of the class.
                    try
                    {
                        serialPort.WriteLine("#023\r");
                    }
                    catch(Exception e)
                    {
                        alarmStatus = AlarmStatus.ComportFailure;
                        if (null != AlarmHandler) AlarmHandler(this, new AlarmEventArgs(alarmStatus));
                        stopCom();
                    }
                    // Com port surveillance. If Master does not receive temp after 3 request, an alarm event is flagged.
                    if (getTempTimeout++ == 3)
                    {
                        if (connectionStatus_extern != ConnectionStatus.Waiting)
                        {
                            connectionStatus_extern = ConnectionStatus.Waiting;
                            if (null != ConnectionStatusHandler) ConnectionStatusHandler(this, new ConnectionStatusEventArgs(connectionStatus_extern));
                            getTempTimeoutCounter++;
                            alarmStatus = AlarmStatus.RS485Failure;
                            if (null != AlarmHandler) AlarmHandler(this, new AlarmEventArgs(alarmStatus));
                            getTempTimeout = 0; // Reset timeout, allowing new alarms be flagged
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
