using System;

namespace IPC.Common.Helpers
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