using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public enum messageType { Error, Info, Warning }
    [DataContract]
    public class Audit
    {
        [DataMember]
        public int Id { get;set; }
        [DataMember]
        public DateTime TimeStamp { get; set; }
        [DataMember]
        public messageType Type { get; set; }
        [DataMember]
        public string Message { get; set; }
        public Audit(int id, DateTime timeStamp, messageType type, string message)
        {
            Id = id;
            TimeStamp = timeStamp;
            Type = type;
            Message = message;
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
