using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IResponse
    {
        [OperationContract]
        void Answer(Load l, Audit a);
    }
}
