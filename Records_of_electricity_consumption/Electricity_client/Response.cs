using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using CsvHelper;
using CsvHelper.Configuration;

namespace Electricity_client
{
    internal class Response : IResponse
    {
        private static string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static string parentDirectory = Directory.GetParent(currentDirectory).FullName;
        private static string parentDirectory1 = Directory.GetParent(parentDirectory).FullName;
        private static string parentDirectory2 = Directory.GetParent(parentDirectory1).FullName;
        private static string solutionDirectory = Directory.GetParent(parentDirectory2).FullName;
        private static string filePath = ConfigurationManager.AppSettings["filePath"];
        private static readonly string filePath1 = Path.Combine(solutionDirectory, filePath);
        public void Answer(Load l, Audit a)
        {
            
            bool fileExists = File.Exists($@"{filePath1}");
            //File.WriteAllText($@"{filePath}", string.Empty);
            // Create a new CSV writer
            bool headerDoesntExist = false;
            if (!FieldExists("Id", $@"{filePath1}") && !FieldExists("TimeStamp", $@"{filePath1}") && !FieldExists("ForecastedValue", $@"{filePath1}") && !FieldExists("MeasuredValue", $@"{filePath1}"))
            {
                headerDoesntExist = true;
            }
            bool DateExists = false;
            if (l != null)
            {
      
                if (FieldsExist(l.TimeStamp.Date.ToString("MM/dd/yyyy"), $@"{filePath1}") > 23)
                {
                    DateExists = true;
                }
            }


                using (var writer = new StreamWriter($@"{filePath1}", append: true))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    NewLine = "\r\n" // Enable escaping of newline characters
                };
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    // Write the header row if the header doesn't exist
                    if(headerDoesntExist == true)
                    {
                        csv.WriteField("Id");
                        csv.WriteField("TimeStamp");
                        csv.WriteField("ForecastedValue");
                        csv.WriteField("MeasuredValue");
                        csv.NextRecord();
                    }


                    // Write individual Load objects
                    if (DateExists == false)
                    {
                        csv.WriteRecord(l);
                        csv.NextRecord();
                    }

                }
            }
                if (l != null)
            {
                Console.WriteLine(l);
            }
            else
            {
                Console.WriteLine(a.Message);
            }
        }

        private bool FieldExists(string fieldToCheck, string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string line; //= reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split(',');
                    foreach (string field in fields)
                    {
                        if (field.Trim().Equals(fieldToCheck))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private int FieldsExist(string fieldToCheck, string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                int i = 0;
                string line; //= reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split(',');
                    foreach (string field in fields)
                    {
                        string[] parts = field.Split(' ');
                        if (parts.Length > 0 && parts[0].Equals(fieldToCheck))
                        {
                            i++;
                        }
                    }
                }
                return i;
            }
        }
    }
}
