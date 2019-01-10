using System;

namespace IpcPocV2.Common.Models.Command
{
    [Serializable]
    public enum CommandType
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