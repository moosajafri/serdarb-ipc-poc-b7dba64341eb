using System;

namespace IpcPocV2.Common.Helpers
{
    public static class StringHelpers
    {
        public static string GetNewUid()
        {
            var uid = Guid.NewGuid().ToString("N").ToUpper();
            return uid;
        }
    }
}