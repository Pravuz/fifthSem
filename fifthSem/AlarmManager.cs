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
    public enum AlarmTypes { TempLoLo = 1, TempLo = 2, TempHi = 3, TempHiHi = 4, TempChangeFast = 5, HostMissing = 6, RS485Error = 7, SerialPortError = 8 }
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
            this.Source = Source;
            timestamp = DateTime.Now;
            this.High = true;
        }

        public double PV;
    }
    public class AlarmManager
    {
        private List<Alarm> alarms;
        private ScadaCommunicationProtocol.ScpHost scpHost;
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
            this.scpHost = scpHost;
            alarms = new List<Alarm>();
        }

        public async Task<bool> SetAlarmStatus(AlarmTypes Type, AlarmCommand Command, string alarmsource = "", double pv = 0.0)
        {
            bool result = false;
            if (scpHost.ScpConnectionStatus == ScpConnectionStatus.Master)
            {
                setMasterAlarmStatus(Type, Command, alarmsource, pv);
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
            byte[] bytes = new byte[0];
            MemoryStream ms = new MemoryStream();

            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(ms, alarms);
                bytes = ms.ToArray();
            }
            catch (SerializationException ex)
            {
/*                .WriteLine("Failed to serialize. Reason: " + ex.Message);
                throw;*/
            }
            finally
            {
                ms.Close();
            }
            return bytes;
        }

        private void deSerializeAlarms(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                alarms = (List<Alarm>)formatter.Deserialize(ms);
                OnAlarmsChanged();
            }
            catch (SerializationException ex)
            {
                /*Console.WriteLine("Failed to deserialize. Reason: " + ex.Message);
                throw;*/
            }
            finally
            {
                ms.Close();
            }

        }

        private void setMasterAlarmStatus(AlarmTypes Type, AlarmCommand Command, string hostname="", double pv=0.0)
        {
            Alarm alarm = alarms.FirstOrDefault(a => a.Type == Type && a.Source == hostname);
            if (hostname=="")
            {
                hostname = "Process";
            }
            switch (Command)
            {
                case AlarmCommand.High:
                    if (alarm == null)
                    {
                        alarm = new Alarm(Type, hostname);
                        alarm.PV = pv;
                        alarms.Add(alarm);
                    }
                    else
                    {
                        alarm.High = true;
                    }
                    break;
                case AlarmCommand.Low:
                    alarm.High = false;
                    if (alarm.Acked) // If Alarm already acked we can remove it
                    {
                        alarms.Remove(alarm);
                    }
                    break;
                case AlarmCommand.Ack:
                    alarm.Acked = true;
                    if (!alarm.High)
                    {
                        alarms.Remove(alarm);
                    }
                    break;
            }
            OnAlarmsChanged();
        }
        private void PacketHandler(object sender, ScpPacketEventArgs e)
        {
            if (e.Packet is ScpAlarmRequest && scpHost.ScpConnectionStatus == ScpConnectionStatus.Master)
            {
                ScpAlarmRequest scpRequest = (ScpAlarmRequest)e.Packet;
                setMasterAlarmStatus(scpRequest.AlarmType, scpRequest.AlarmCommand, scpRequest.Source);
                e.Response = new ScpAlarmResponse(true);
            }
            else if (e.Packet is ScpAlarmBroadcast && scpHost.ScpConnectionStatus == ScpConnectionStatus.Slave)
            {
            }
        }

    }
}
