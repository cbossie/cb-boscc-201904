using SiteDynamoLambda.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteDynamoLambda.Services
{
    public interface ITranscribeDataService
    {
        Task<bool> AddDataToTranscribe(string text);
        Task<IEnumerable<TranscribeData>> GetLatestDataToTranscribe(DateTime? startDate = null);
    }
}
