using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BostonCodeCampServices.Service
{
    public interface IPollyService
    {
        Task<byte[]> TranscribeText(string text);
    }
}
