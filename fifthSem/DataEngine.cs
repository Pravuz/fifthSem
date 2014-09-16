using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScadaCommunicationProtocol;

namespace fifthSem
{
    class DataEngine
    {
        public static string hostname;
        public int hostId, tempAlarmHigh, tempAlarmLow;
        public enum Type { REQUEST, RECIEVE, PUSH };
        public enum Mode { MASTER, SLAVE };

        private string logFile, logFolder = @"%USERPROFILE%\My Documents\Loggs\";
        private int sizeOfFile;
        private ScpHost mScpHost;
        //private rs485host mrs485host;
        //private alarmhost malarmhost;

        public DataEngine() {
            DateTime d = DateTime.Now;
            logFile = d.Month.ToString() + d.Year.ToString() + ".txt";

            mScpHost = new ScpHost(1);
            if (ComConnected()) mScpHost.CanBeMaster = true;
            mScpHost.ScpConnectionStatusEvent += ConnectionStatusHandler;
            mScpHost.PacketEvent += PacketHandler;
            hostname = ScpHost.Name;

            mScpHost.Start();
            //start rs485 og alarmsystem

        }

        private void ConnectionStatusHandler(object sender, ScpConnectionStatusEventArgs e) {
            string timeStamp = DateTime.Now.ToLongTimeString();

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
        private bool ComConnected() {
            return true;
        }
        
        /// <summary> Method for writing logg/alarm/information etc to file </summary>
        /// <returns> True if the operation was a success and vice/versa. </returns>
        private bool writeToFile(string s){
            bool success = true;
            try
            {
                File.AppendText(logFolder + logFile).Write(s);
            }
            catch (Exception e) {
                success = false;
            }
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

}
