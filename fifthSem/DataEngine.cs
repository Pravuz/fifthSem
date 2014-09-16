using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScadaCommunicationProtocol;

namespace fifthSem
{
    class DataEngine
    {
        public bool hasCom;
        public static string hostname, filename;
        public int hostId, tempAlarmHigh, tempAlarmLow;
        public const int REQUEST = 0, RECIEVE = 1, PUSH = 2;

        private string logFolder = @"%USERPROFILE%\My Documents\Loggs\";
        private string logFile;
        private int sizeOfFile, timeSinceLastPush;
        private ScpHost mScpHost;

        /// <summary>
        /// constructor
        /// </summary>
        public DataEngine() {
            DateTime d = DateTime.Now;
            logFile = d.Month.ToString() + d.Year.ToString();

            mScpHost = new ScpHost(1);
            mScpHost.ScpConnectionStatusEvent += ConnectionStatusHandler;
            mScpHost.PacketEvent += PacketHandler;
            hostname = ScpHost.Name;

            mScpHost.Start();

        }

        private void ConnectionStatusHandler(object sender, ScpConnectionStatusEventArgs e) {
            switch (e.Status) { 
                case ScpConnectionStatus.Master:
                    break;
                case ScpConnectionStatus.Slave:
                    break;
                case ScpConnectionStatus.Waiting:
                    break;
                default:
                    break;
            }
        }
        private void PacketHandler(object sender, ScpPacketEventArgs e) {
            
        }

        private void TimerTask() {
            
        }

        /// <summary>
        /// Endrer program/klassevariabler basert på om bruker har huket av boks for tilkobling til rs485 (com)
        /// </summary>
        private void isComConnected() { }
        
        /// <summary> Method for writing logg/alarm/information etc to file </summary>
        /// <returns> True if the operation was a success and vice/versa. </returns>
        private bool writeToFile(String[] s){
            bool success = true;
            return success;
        }

        //public metods:
    }

    /// <summary>
    /// Creates an object of a temperature reading. 
    /// </summary>
    private class temperatureReading {
        public DateTime dateTime { get; set; }
        public double temp { get; set; }

        /// <summary>
        /// An object representing a temperature reading.
        /// </summary>
        /// <param name="dateTime">date and time of the temperature reading. Must be DateTime format. </param>
        /// <param name="temp">A double value representing the temperature.</param>
        public temperatureReading(DateTime dateTime, double temp) {
            this.dateTime = dateTime;
            this.temp = temp;
        }
    }

    /// <summary>
    /// Creates an object of an alarm
    /// </summary>
    private class alarmEvent {

        public const int ALARM_INFO = 0;
        public const int ALARM_WARNING = 1;
        public const int ALARM_ERROR = 2;

        public int alarmId { get; set; }
        public int alarmSenderId { get; set; }
        public DateTime dateTime { get; set; }
        public string alarmMsg { get; set; }
        public int alarmSeverity { get; set; }

        /// <summary>
        /// An object representing an alarm-event.
        /// </summary>
        /// <param name="alarmSenderId">hostNameId</param>
        /// <param name="dateTime">datetime value of the DateTime format</param>
        /// <param name="alarmMsg">Alarmmessage</param>
        /// <param name="alarmSeverity">An integer of the following value:
        /// 0 = Information
        /// 1 = Warning 
        /// 2 = Error</param>
        public alarmEvent(int alarmSenderId, DateTime dateTime, string alarmMsg, int alarmSeverity) {
            alarmId = 0; //GET LAST ALARMID FROM LOG + 1
            this.alarmMsg = alarmMsg;
            this.alarmSenderId = alarmSenderId;
            this.alarmSeverity = alarmSeverity;
            this.dateTime = dateTime;
        }
    }

}
