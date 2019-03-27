using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BostonCodeCampServices.Service
{
    public interface IFileService
    {
        Task<bool> SaveFile(byte[] data, string fileName);

        Task<byte[]> GetFile(string key);
        string GetFileName(string name);
    }
}
