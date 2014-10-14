using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScadaCommunicationProtocol;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace fifthSem
{
    public delegate void AlarmsChangedEventHandler(object sender, AlarmsChangedEventArgs e);
    public class AlarmsChangedEventArgs : EventArgs
    {
        public List<Alarm> Alarms;
        public AlarmsChangedEventArgs(List<Alarm> alarms)
        {
            this.Alarms = alarms;
        }
    }
    public enum AlarmTypes { TempLoLo = 1, TempLo = 2, TempHi = 3, TempHiHi = 4, TempChangeFast = 5, HostMissing = 6, RS485Error = 7, SerialPortError = 8, TempMissing = 9 }
    public enum AlarmCommand { High=1, Low=2, Ack=3 }

    [Serializable]
    public class Alarm
    {
        private DateTime timestamp;
        public AlarmTypes Type { get; set; }
        public string Source { get; set; }
        public bool High { get; set; }
        public bool Acked { get; set; }
        public DateTime Timestamp { get { return timestamp; } }
        public Alarm(AlarmTypes Type, string Source)
        {
            this.Type = Type;
            timestamp = DateTime.Now;
            this.High = true;
            this.Source = Source;
        }
        public double PV;
    }
    /// <summary>
    /// Pass the SCP host object to the constructor when creating AlarmManager object.
    /// The AlarmManager communicates with other SCP hosts automatically to keep alarms updated
    /// Subscribe to AlarmsChangedEvent to be notified when there is a change in the alarm list
    /// Use SetAlarmStatus to set an alarm to High/Low or Acked state.
    /// </summary>
    public class AlarmManager
    {
        private List<Alarm> alarms;
        private ScadaCommunicationProtocol.ScpHost scpHost;
        private System.Timers.Timer timer;
        private bool updateNeeded = false;
        private void OnAlarmsChanged()
        {
            if (AlarmsChangedEvent != null)
            {
                AlarmsChangedEvent(this, new AlarmsChangedEventArgs(alarms));
            }
        }
        public event AlarmsChangedEventHandler AlarmsChangedEvent;

        public AlarmManager(ScadaCommunicationProtocol.ScpHost scpHost)
        {
            timer = new System.Timers.Timer(10000);
            timer.Elapsed += timer_Elapsed;
            this.scpHost = scpHost;
            scpHost.PacketEvent += PacketHandler;
            scpHost.SlaveConnectionEvent += SlaveConnectionEvent;
            scpHost.ScpConnectionStatusEvent += ScpConnectionStatusEvent;
            alarms = new List<Alarm>();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (string host in scpHost.Hosts)
            {
                if (!scpHost.IsHostConnected(host) && scpHost.ScpConnectionStatus == ScpConnectionStatus.Master)
                {
                    setMasterAlarmStatus(AlarmTypes.HostMissing, AlarmCommand.High, host);
                }
            }
            SendAlarmUpdate();
            timer.Stop();
        }

        void ScpConnectionStatusEvent(object sender, ScpConnectionStatusEventArgs e)
        {
            if (e.Status == ScpConnectionStatus.Master)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        void SlaveConnectionEvent(object sender, SlaveConnectionEventArgs e)
        {
            if (e.Connected)
            {
                setMasterAlarmStatus(AlarmTypes.HostMissing, AlarmCommand.Low, e.Name);
            }
            else
            {
                setMasterAlarmStatus(AlarmTypes.HostMissing, AlarmCommand.High, e.Name);
            }
            SendAlarmUpdate();
        }

        public async Task<bool> SetAlarmStatus(AlarmTypes Type, AlarmCommand Command, string alarmsource = "", double pv = 0.0)
        {
            bool result = false;
            if (Type.ToString().Contains("Temp"))
            {
                alarmsource = "Process";
            }
            if (scpHost.ScpConnectionStatus == ScpConnectionStatus.Master)
            {
                setMasterAlarmStatus(Type, Command, alarmsource, pv);
                SendAlarmUpdate();
                result = true;
            }
            else if (scpHost.ScpConnectionStatus == ScpConnectionStatus.Slave) // Send command to master
            {
                try
                {
                    ScpAlarmRequest scpPacket = new ScpAlarmRequest(Type, Command, alarmsource);
                    ScpPacket response = await scpHost.SendRequestAsync(scpPacket);
                    if (response != null && response is ScpAlarmResponse)
                    {
                        result = ((ScpAlarmResponse)response).Ok;
                    }
                }
                catch
                {
                }
            }
            return result;
        }
        private byte[] serializeAlarms()
        {
            byte[] bytes;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, alarms);
            bytes = ms.ToArray();
            ms.Close();
            return bytes;
        }

        private void deSerializeAlarms(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            BinaryFormatter formatter = new BinaryFormatter();
            alarms = (List<Alarm>)formatter.Deserialize(ms);
            OnAlarmsChanged();
            ms.Close();
        }

        private void setMasterAlarmStatus(AlarmTypes Type, AlarmCommand Command, string alarmsource="", double pv=0.0)
        {
            Alarm alarm = alarms.FirstOrDefault(a => a.Type == Type && a.Source == alarmsource);
            switch (Command)
            {
                case AlarmCommand.High:
                    if (alarm == null)
                    {
                        alarm = new Alarm(Type, alarmsource);
                        alarm.PV = pv;
                        alarms.Add(alarm);
                    }
                    else
                    {
                        alarm.High = true;
                    }
                    break;
                case AlarmCommand.Low:
                    if (alarm != null)
                    {
                        alarm.High = false;
                        if (alarm.Acked) // If Alarm already acked we can remove it
                        {
                            alarms.Remove(alarm);
                        }
                    }
                    break;
                case AlarmCommand.Ack:
                    if (alarm != null)
                    {
                        alarm.Acked = true;
                        if (!alarm.High)
                        {
                            alarms.Remove(alarm);
                        }
                    }
                    break;
            }
            updateNeeded = true;
            OnAlarmsChanged();
        }

        private void SendAlarmUpdate()
        {
            if (updateNeeded)
            {
                ScpAlarmBroadcast packet = new ScpAlarmBroadcast(serializeAlarms());
                scpHost.SendBroadcastAsync(packet).ConfigureAwait(false);
                updateNeeded = false;
            }
        }
        private void PacketHandler(object sender, ScpPacketEventArgs e)
        {
            if (e.Packet is ScpAlarmRequest && scpHost.ScpConnectionStatus == ScpConnectionStatus.Master)
            {
                ScpAlarmRequest scpRequest = (ScpAlarmRequest)e.Packet;
                SetAlarmStatus(scpRequest.AlarmType, scpRequest.AlarmCommand, scpRequest.AlarmSource).ConfigureAwait(false);
                e.Response = new ScpAlarmResponse(true);
            }
            else if (e.Packet is ScpAlarmBroadcast && scpHost.ScpConnectionStatus == ScpConnectionStatus.Slave)
            {
                // Receiving updated alarm list from master
                deSerializeAlarms(((ScpAlarmBroadcast)e.Packet).Alarm);
            }
        }

    }
}
