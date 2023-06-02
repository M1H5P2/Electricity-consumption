using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Common;

namespace Electricity_server
{
    public class Request : IRequest
    {
        //In memory 'baza podataka'
        public static Dictionary<DateTime,List<Load>> dic = new Dictionary<DateTime, List<Load>>();
        public static Dictionary<DateTime,Audit> auditDic = new Dictionary<DateTime, Audit>();
        //private static readonly string folder = @"C:\Users\Milko\Desktop\predmeti\virtualizacija_procesa\vp\Records_of_electricity_consumption\load_tables";
        //private static readonly string auditFile = @"C:\Users\Milko\Desktop\predmeti\virtualizacija_procesa\vp\Records_of_electricity_consumption\audit_folder\TBL_AUDIT.xml";
        TimeSpan timeSpanValue = TimeSpan.Parse(ConfigurationManager.AppSettings["TimeSpanValue"]);
        private static string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static string parentDirectory = Directory.GetParent(currentDirectory).FullName;
        private static string parentDirectory1 = Directory.GetParent(parentDirectory).FullName;
        private static string parentDirectory2 = Directory.GetParent(parentDirectory1).FullName;
        private static string solutionDirectory = Directory.GetParent(parentDirectory2).FullName;
        private static readonly string folder = Path.Combine(solutionDirectory, "load_tables");
        private static readonly string auditFile = Path.Combine(solutionDirectory, "audit_folder\\TBL_AUDIT.xml");
        private Thread cleanupThread;
        private readonly object lockObject = new object();
        private bool isRunning;

        public delegate void ItemExpiredHandler(DateTime expiredKey);
        public event ItemExpiredHandler ItemExpired;
        public void Search(DateTime date)
        {
            List<Load> list = new List<Load>();
            using (ChannelFactory<IResponse> cf = new ChannelFactory<IResponse>("client1"))
            {
                IResponse response = cf.CreateChannel();
                int i = 0;
                
                if (dic.Count == 0)
                {
                    SearchThroughDatabase(folder, list, date, response,ref i);
                    if (i != 0)
                    {
                        CreateAudit(date, auditFile, response, messageType.Info);
                    }
                }
                else
                {
                    SearchThroughDictionary(dic, date, response, ref i);
                    if(i==0)
                    {
                        SearchThroughDatabase(folder, list, date, response,ref i);
                        if (i != 0)
                        {
                            CreateAudit(date, auditFile, response, messageType.Info);
                        }
                    }
                    else
                    {
                        CreateAudit(date, auditFile, response, messageType.Info);
                    }
                }
                if (i == 0)
                {
                    ForecastTheElectricity(date,folder);
                    Console.WriteLine("Another XML table created successfully");
                    CreateAudit(date, auditFile, response, messageType.Error);
                }
                cf.Close();
            }
            
        }
        private string[] LoadThese(string directory)
        {
            string[] xmlFiles = Directory.GetFiles(directory, "*.xml");
            return xmlFiles;
        }

        private void ForecastTheElectricity(DateTime date, string directory)
        {
            List<Load> list = new List<Load>();
            int i = 1;
            int number_for_table = 1;
            Random r = new Random();
            DateTime startTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            DateTime endTime = new DateTime(date.Year, date.Month, date.Day, 23, 0, 0);
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t" // Use tab for indentation (optional)
            };
            foreach (string file in LoadThese(directory))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                Match match = Regex.Match(fileName, @"\d+");
                if (match.Success)
                {
                    int number;
                    if (int.TryParse(match.Value, out number))
                    {
                        while (number >= number_for_table)
                        {
                            number_for_table++;
                        }
                    }
                }
            }
            //using (XmlWriter writer = XmlWriter.Create($@"C:\Users\Milko\Desktop\predmeti\virtualizacija_procesa\vp\Records_of_electricity_consumption\load_tables\TB_LOAD{number_for_table}.xml", settings))
            using (XmlWriter writer = XmlWriter.Create($"{folder}\\TB_LOAD{number_for_table}.xml", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("rows");
                while (startTime <= endTime)
                {
                    string start = startTime.ToString("yyyy-MM-dd HH:mm");
                    Console.WriteLine(start);
                    int measuredValue = r.Next(4800, 7000);
                    double deviation = r.NextDouble() * 2 - 1;
                    double forecastedValue = Math.Round(((double)measuredValue + deviation),2);
                    Load l = new Load(i, startTime, forecastedValue, measuredValue);
                    list.Add(l);

                    


                    
                    writer.WriteStartElement("row");
                    writer.WriteElementString("ID", $"{i}");
                    writer.WriteElementString("TIME_STAMP", start);
                    writer.WriteElementString("FORECAST_VALUE", $"{forecastedValue}");
                    writer.WriteElementString("MEASURED_VALUE", $"{measuredValue}");
                    writer.WriteEndElement();
                    


                    startTime = startTime.AddHours(1);
                    i++;
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            AddToDictionary(list);
        }

        private void SearchThroughDatabase(string directory, List<Load> loads, DateTime date, IResponse response,ref int i)
        {
            foreach (string file in LoadThese(directory))
            {
                string xml = File.ReadAllText(file);
                System.Diagnostics.Debug.WriteLine(file);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                XmlNodeList rowNodes = xmlDoc.SelectNodes("/rows/row");

                foreach (XmlNode rowNode in rowNodes)
                {
                    string id = rowNode.SelectSingleNode("ID").InnerText;
                    string timeStamp = rowNode.SelectSingleNode("TIME_STAMP").InnerText;
                    DateTime time = DateTime.Parse(timeStamp);
                    string forecastValue = rowNode.SelectSingleNode("FORECAST_VALUE").InnerText;
                    string measuredValue = rowNode.SelectSingleNode("MEASURED_VALUE").InnerText;
                    double forecastedValue = double.Parse(forecastValue);
                    int mValue = int.Parse(measuredValue);
                    int Id = int.Parse(id);
                    if (date.Date.Equals(time.Date))
                    {


                        i++;
                        Console.WriteLine("ID: " + id);
                        Console.WriteLine("Time Stamp: " + timeStamp);
                        Console.WriteLine("Forecast Value: " + forecastValue);
                        Console.WriteLine("Measured Value: " + measuredValue);
                        Load l = new Load(Id, time, forecastedValue, mValue);
                        loads.Add(l);
                        
                        response.Answer(l, null);
                        if (mValue >= 6800)
                        {
                            CreateAudit(date, auditFile, response, messageType.Warning);
                        }
                    }
                    else
                    {
                        goto here;
                    }
                }
            here:;
            }
            AddToDictionary(loads);
        }
        
        private void CreateAudit(DateTime date, string existingAudit, IResponse response, messageType typ)
        {
            int i = 103;
            
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t" // Use tab for indentation (optional)
            };
            
            DateTime now = DateTime.Now;
            string messag = "";
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(existingAudit);
            XmlNodeList rowNodes = xmlDocument.SelectNodes("//row");
            int elementId=0;
            foreach (XmlNode rowNode in rowNodes)
            {
                XmlNodeList childNodes = rowNode.ChildNodes;
                foreach (XmlNode childNode in childNodes)
                {
                    string elementName = childNode.Name;
                    if (elementName.Equals("ID"))
                    {
                        string elementValue = childNode.InnerText;
                        elementId = Convert.ToInt32(elementValue);
                                                
                    }
                }
            }
            while(i <= elementId)
            {
                i++;
            }
            XmlNode rows = xmlDocument.SelectSingleNode("rows");
            XmlElement row = xmlDocument.CreateElement("row");
            XmlElement rowId = xmlDocument.CreateElement("ID");
            rowId.InnerText = $"{i}";
            row.AppendChild(rowId);
            XmlElement time_stamp = xmlDocument.CreateElement("TIME_STAMP");
            time_stamp.InnerText = $"{now.ToString("yyyy-MM-dd HH:mm:ss.fff")}";
            row.AppendChild(time_stamp);
            XmlElement messageTyp = xmlDocument.CreateElement("MESSAGE_TYPE");
            messageTyp.InnerText = $"{typ}";
            row.AppendChild(messageTyp);
            if(typ == messageType.Error) {
                messag = $"There is no data for date {date.Date.ToString("yyyy-MM-dd")} in the database";
            }
            else if(typ == messageType.Info)
            {
                messag = "Data is successfully read and forwarded";
            }
            else
            {
                messag = "Measured value for electricity consumption went over red zone of 6800";
            }
            XmlElement m = xmlDocument.CreateElement("MESSAGE");
            m.InnerText = messag;
            row.AppendChild(m);
            rows.AppendChild(row);
            xmlDocument.Save(existingAudit);
            Audit a = new Audit(i, date, typ, messag);
            response.Answer(null, a);
            DateTime dateKey = DateTime.Now.AddMinutes(-30);
            auditDic.Add(dateKey, a);
        }

        private void SearchThroughDictionary(Dictionary<DateTime,List<Load>> dict,DateTime date,IResponse response, ref int k)
        {
            foreach(KeyValuePair<DateTime,List<Load>> keyValuePair in dict)
            {
                foreach(Load load in keyValuePair.Value)
                {
                    if (load.TimeStamp.Date.Equals(date))
                    {
                        response.Answer(load, null);
                        k++;
                    }
                    else
                    {
                        goto there;
                    }
                }
            there:;
            }
        }












        public void StartCleanup<T>(Dictionary<DateTime,T> things)
        {
            lock (lockObject)
            {
                if (!isRunning)
                {
                    isRunning = true;
                    cleanupThread = new Thread(() => CleanupThreadWorker(things));
                    cleanupThread.Start();
                }
            }
        }

        public void StopCleanup()
        {
            lock (lockObject)
            {
                if (isRunning)
                {
                    isRunning = false;
                    cleanupThread.Join();
                }
            }
        }

        public void AddToDictionary(List<Load> item)
        {
            if (item.Count != 0)
            {
                DateTime key = DateTime.Now.AddMinutes(-30);
                dic.Add(key, item);
            }
            
        }

        private void CleanupThreadWorker<T>(Dictionary<DateTime,T> dictionary)
        {
            while (true)
            {
                DateTime currentTime = DateTime.Now;
                List<DateTime> expiredKeys = new List<DateTime>();

                lock (lockObject)
                {
                    foreach (var kvp in dictionary)
                    {
                        if (currentTime - kvp.Key > timeSpanValue)
                        {
                            expiredKeys.Add(kvp.Key);
                            OnItemExpired(kvp.Key);
                        }
                    }

                    foreach (DateTime key in expiredKeys)
                    {
                        dictionary.Remove(key);
                    }
                }

                // Sleep for a specified interval before checking again
                Thread.Sleep(TimeSpan.FromMinutes(5)); // Adjust the interval as needed

                lock (lockObject)
                {
                    if (!isRunning)
                    {
                        break;
                    }
                }
            }
        }



        public Request()
        {

        }

        protected virtual void OnItemExpired(DateTime expiredKey)
        {
            ItemExpired?.Invoke(expiredKey);
        }
    }
}
