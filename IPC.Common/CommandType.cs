using System;
using System.Collections.Generic;
using System.Text;

namespace IPC.Common
{
    [Serializable]
    public enum CommandType:int
    {
        GetFileById=1,
        GetFileByGuid=2,


        GetCustomerById=3,
        GetCustomerByGUID=4,
        GetCustomerByPhone=5,

        AddFile=7,
        AddCustomer=8,
        GetCustomerFiles=9,

        Terminate=99
    }
}
