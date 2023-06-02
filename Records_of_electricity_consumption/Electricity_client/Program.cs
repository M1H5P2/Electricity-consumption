using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Common;

namespace Electricity_client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (ChannelFactory<IRequest> channelFactory = new ChannelFactory<IRequest>("client"))
            {
                using(ServiceHost svc = new ServiceHost(typeof(Response)))
                {
                    svc.Open();
                    IRequest request = channelFactory.CreateChannel();
                    while(true)
                    {
                        Console.WriteLine("Enter a date you want to see forecasted and measured values of elctricity consumption (if you want to exit press [Enter]): ");
                        string date = Console.ReadLine();
                        if (date.Equals(""))
                        {
                            break;
                        }
                        DateTime dateTime;
                        if(DateTime.TryParse(date, out dateTime))
                        {
                            Console.WriteLine(dateTime);
                            request.Search(dateTime);
                        }
                        
                    }
                    
                    channelFactory.Close();
                    svc.Close();
                }
                
            }
        }
    }
}
