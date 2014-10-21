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
    public delegate void TempLimitsChangedEventHandler(object sender, TempLimitsChangedEventArgs e);
    public class AlarmsChangedEventArgs : EventArgs
    {
        public List<Alarm> Alarms;
        public List<Alarm> FilteredAlarms;
        public AlarmsChangedEventArgs(List<Alarm> Alarms, List<Alarm> FilteredAlarms)
        {
            this.Alarms = Alarms;
            this.FilteredAlarms = FilteredAlarms;
        }
    }
    public class TempLimitsChangedEventArgs : EventArgs
    {
        public double LoLoLimit { get; set; }
        public double LoLimit { get; set; }
        public double HiLimit { get; set; }
        public double HiHiLimit { get; set; }
        public TempLimitsChangedEventArgs(double LoLo, double Lo, double Hi, double HiHi)
        {
            this.LoLoLimit = LoLo;
            this.LoLimit = Lo;
            this.HiLimit = Hi;
            this.HiHiLimit = HiHi;
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
        public bool Filtered { get; set; }
        public Alarm(AlarmTypes Type, string Source)
        {
            this.Type = Type;
            timestamp = DateTime.Now;
            this.High = true;
            this.Source = Source;
        }
    }
    /// <summary>
    /// Pass the SCP host object to the constructor when creating AlarmManager object.
    /// The AlarmManager communicates with other SCP hosts automatically to keep alarms updated
    /// Subscribe to AlarmsChangedEvent to be notified when there is a change in the alarm list
    /// Use SetAlarmStatus to set an alarm to High/Low or Acked state.
    /// 
    /// </summary>
    public class AlarmManager
    {
        private List<Alarm> alarms;
        private ScadaCommunicationProtocol.ScpHost scpHost;
        private System.Timers.Timer timer;
        private bool updateNeeded = false;
        private List<AlarmTypes> filteredAlarms;
        private double tempLimitLoLo, tempLimitLo, tempLimitHi, tempLimitHiHi;
        public double TempLimitLoLo 
        {
            get
            {
                return tempLimitLoLo;
            }
            set
            {
                if (value != tempLimitLoLo)
                {
                    tempLimitLoLo = value;
                    SendTempUpdate();
                }
            }
        }
        public double TempLimitLo
        {
            get
            {
                return tempLimitLo;
            }
            set
            {
                if (value != tempLimitLo)
                {
                    tempLimitLo = value;
                    SendTempUpdate();
                }
            }
        }
        public double TempLimitHi
        {
            get
            {
                return tempLimitHi;
            }
            set
            {
                if (value != tempLimitHi)
                {
                    tempLimitHi = value;
                    SendTempUpdate();
                }
            }
        }
        public double TempLimitHiHi
        {
            get
            {
                return tempLimitHiHi;
            }
            set
            {
                if (value != tempLimitHiHi)
                {
                    tempLimitHiHi = value;
                    SendTempUpdate();
                }
            }
        }
        public List<Alarm> AllAlarms
        {
            get
            {
                return alarms.ToList();
            }
        }
        public List<Alarm> FilteredAlarms
        {
            get
            {
                return alarms.Where(al => al.Filtered == false).ToList();
            }
        }
        private void OnAlarmsChanged()
        {
            if (AlarmsChangedEvent != null)
            {
                AlarmsChangedEvent(this, new AlarmsChangedEventArgs(AllAlarms, FilteredAlarms));
            }
        }
        private void OnTempLimitsChanged()
        {
            if (TempLimitsChangedEvent != null)
            {
                TempLimitsChangedEvent(this, new TempLimitsChangedEventArgs(TempLimitLoLo, TempLimitLo, TempLimitHi, TempLimitHiHi));
            }
        }
        private void SendTempUpdate()
        {
            ScpAlarmLimitBroadcast scpPacket = new ScpAlarmLimitBroadcast(tempLimitLoLo, tempLimitLo, tempLimitHi, tempLimitHiHi);
            scpHost.SendBroadcastAsync(scpPacket);
        }
        public event AlarmsChangedEventHandler AlarmsChangedEvent;
        public event TempLimitsChangedEventHandler TempLimitsChangedEvent;

        public AlarmManager(ScadaCommunicationProtocol.ScpHost scpHost)
        {
            timer = new System.Timers.Timer(10000);
            timer.Elapsed += timer_Elapsed;
            this.scpHost = scpHost;
            scpHost.PacketEvent += PacketHandler;
            scpHost.SlaveConnectionEvent += SlaveConnectionEvent;
            scpHost.ScpConnectionStatusEvent += ScpConnectionStatusEvent;
            alarms = new List<Alarm>();
            filteredAlarms = new List<AlarmTypes>();
            tempLimitLoLo = -10000;
            tempLimitLo = -10000;
            tempLimitHi = 10000;
            tempLimitHiHi = 10000;
        }

        public void SetAlarmFilter(AlarmTypes type, bool filter)
        {
            if (filter)
            {
                if (!filteredAlarms.Exists(at => at == type))
                {
                    filteredAlarms.Add(type);
                }
            }
            else
            {
                if (filteredAlarms.Exists(at => at == type))
                {
                    filteredAlarms.Remove(type);
                }
            }
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
                SendTempUpdate();
                updateNeeded = true;
            }
            else
            {
                setMasterAlarmStatus(AlarmTypes.HostMissing, AlarmCommand.High, e.Name);
            }
            SendAlarmUpdate();
        }

        public void SetTemp(double temp)
        {
            if (scpHost.ScpConnectionStatus == ScpConnectionStatus.Master)
            {
                if (temp > TempLimitHiHi)
                {
                    setMasterAlarmStatus(AlarmTypes.TempHiHi, AlarmCommand.High);
                }
                else if (temp > TempLimitHi)
                {
                    setMasterAlarmStatus(AlarmTypes.TempHiHi, AlarmCommand.Low);

                    setMasterAlarmStatus(AlarmTypes.TempHi, AlarmCommand.High);
                }
                else if (temp < TempLimitLoLo)
                {
                    setMasterAlarmStatus(AlarmTypes.TempLoLo, AlarmCommand.High);
                }
                else if (temp < TempLimitLo)
                {
                    setMasterAlarmStatus(AlarmTypes.TempLoLo, AlarmCommand.Low);

                    setMasterAlarmStatus(AlarmTypes.TempLo, AlarmCommand.High);
                }
                else
                {
                    setMasterAlarmStatus(AlarmTypes.TempLoLo, AlarmCommand.Low);
                    setMasterAlarmStatus(AlarmTypes.TempLo, AlarmCommand.Low);
                    setMasterAlarmStatus(AlarmTypes.TempHi, AlarmCommand.Low);
                    setMasterAlarmStatus(AlarmTypes.TempHiHi, AlarmCommand.Low);
                }
                SendAlarmUpdate();
            }
        }

        public async Task<bool> SetAlarmStatus(AlarmTypes Type, AlarmCommand Command, string alarmsource = "")
        {
            bool result = false;
            if (scpHost.ScpConnectionStatus == ScpConnectionStatus.Master)
            {
                setMasterAlarmStatus(Type, Command, alarmsource);
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
        public byte[] serializeAlarms()
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

        private void setMasterAlarmStatus(AlarmTypes Type, AlarmCommand Command, string alarmsource="")
        {
            // Failsafe to makesure only alarms are set when master
            if (scpHost.ScpConnectionStatus != ScpConnectionStatus.Master)
            {
                return;
            }
            if (Type.ToString().Contains("Temp"))
            {
                alarmsource = "Process";
            }
            Alarm alarm = alarms.FirstOrDefault(a => a.Type == Type && a.Source == alarmsource);
            bool changed = false;
            switch (Command)
            {
                case AlarmCommand.High:
                    if (alarm == null)
                    {
                        alarm = new Alarm(Type, alarmsource);
                        alarm.Filtered = filteredAlarms.Contains(Type);
                        alarms.Add(alarm);
                        changed = true;
                    }
                    else
                    {
                        if (!alarm.High)
                        {
                            alarm.Filtered = filteredAlarms.Contains(Type);
                            alarm.High = true;
                            changed = true;
                        }
                    }
                    break;
                case AlarmCommand.Low:
                    if (alarm != null)
                    {
                        if (alarm.High)
                        {
                            alarm.High = false;
                            if (alarm.Acked) // If Alarm already acked we can remove it
                            {
                                alarms.Remove(alarm);
                            }
                            changed = true;
                        }
                    }
                    break;
                case AlarmCommand.Ack:
                    if (alarm != null)
                    {
                        if (!alarm.Acked)
                        {
                            alarm.Acked = true;
                            if (!alarm.High)
                            {
                                alarms.Remove(alarm);
                            }
                            changed = true;
                        }
                    }
                    break;
            }
            if (changed)
            {
                updateNeeded = true;
                OnAlarmsChanged();
            }
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
            else if (e.Packet is ScpAlarmLimitBroadcast)
            {
                ScpAlarmLimitBroadcast packet = (ScpAlarmLimitBroadcast)e.Packet;
                tempLimitLoLo = packet.LoLoLimit;
                tempLimitLo = packet.LoLimit;
                tempLimitHi = packet.HiLimit;
                tempLimitHiHi = packet.HiHiLimit;
                OnTempLimitsChanged();
            }
        }

    }
}
