using IPC.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPC.CacheServer
{
    public interface IFileCache
    {
        File GetFileById(int Id);

        List<File> GetFileByGuid(string Guid);

        void Add(File file);
        void Add(List<File> FileList);
    }
}
