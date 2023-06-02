using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Electricity_server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost svc = new ServiceHost(typeof(Request)))
            {
                svc.Open();
                Console.WriteLine("Press [Enter] to exit");
                Request r = new Request();
                r.ItemExpired += DictionaryCleanup_ItemExpired;
                r.StartCleanup<List<Common.Load>>(Request.dic);
                r.StopCleanup();
                Request r1 = new Request();
                r1.ItemExpired += DictionaryCleanup_ItemExpired;
                r1.StartCleanup<Common.Audit>(Request.auditDic);
                r1.StopCleanup();
                Console.ReadKey();
                svc.Close();
            }
        }
        private static void DictionaryCleanup_ItemExpired(DateTime expiredKey)
        {
            // Perform deletion or other actions for the expired item
            Console.WriteLine($"Item expired: {expiredKey}");
        }
    }
}
