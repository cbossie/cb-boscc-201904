using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteDynamoLambda.Model
{
    [DynamoDBTable("cbnug-transcribedata")]
    public class TranscribeData
    {
        public TranscribeData()
        {
            Id = Guid.NewGuid().ToString();
            TimeStamp = DateTime.Now.Ticks;
        }

        [DynamoDBHashKey("id")]
        public string Id { get; set; }

        [DynamoDBRangeKey("timestamp")]
        public long TimeStamp { get; set; }

        [DynamoDBProperty("TextToTranscribe")]
        public string TextToTranscribe { get; set; }

        [DynamoDBProperty("Complete")]
        public bool Complete { get; set; }

        [DynamoDBProperty("OutputFileData")]
        public string OutputFileData { get; set; }
    }
}
