using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class Load
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public DateTime TimeStamp { get; set; }
        [DataMember]
        public double ForecastedValue { get; set; }
        [DataMember]
        public int MeasuredValue { get; set; }
        public override string ToString()
        {
            return $"Id: {Id}, Time stamp: {TimeStamp}, Forecasted value: {ForecastedValue}, Measured value: {MeasuredValue}";
        }
        public Load(int id, DateTime timeStamp, double forecastedValue, int measuredValue)
        {
            Id = id;
            TimeStamp = timeStamp;
            ForecastedValue = forecastedValue;
            MeasuredValue = measuredValue;
        }
    }
}
