using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using BostonCodeCampModels.Transcribe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BostonCodeCampServices.Service
{
    public class TranscribeDataService : ITranscribeDataService
    {
        IAmazonDynamoDB Client { get; }

        public TranscribeDataService(IAmazonDynamoDB cli)
        {
            Client = cli;
        }


        private DynamoDBContext GetContext()
        {
            return new DynamoDBContext(Client);
        }


        public async Task<bool> AddDataToTranscribe(string text)
        {
            var retval = true;
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            
            try
            {
                var data = new TranscribeData()
                {
                    TextToTranscribe = text
                };

                using (var ctx = GetContext())
                {
                    await ctx.SaveAsync(data);
                    return true;
                }
            }
            catch (Exception ex)
            {
                retval = false;
            }
            return retval;

        }

        public async Task<IEnumerable<TranscribeData>> GetLatestDataToTranscribe(DateTime? startDate = null)
        {
            List<TranscribeData> data = new List<TranscribeData>();
            startDate = startDate ?? DateTime.Now.AddDays(-1);
            try
            {
                using (var ctx = GetContext())
                {
                    var dateTimeCond = new ScanCondition("timestamp",
                        Amazon.DynamoDBv2.DocumentModel.ScanOperator.GreaterThan,
                        startDate.Value.Ticks);
                    

                    var search = ctx.ScanAsync<TranscribeData>(new[] { dateTimeCond });
                    var items = await search.GetRemainingAsync();

                    data.AddRange(items);
                }
            }
            catch(Exception ex)
            {
                // Log
            }
            return data;
        }

        public async Task<TranscribeData> GetTranscribeData(string id, long timestamp)
        {
            TranscribeData data = null;
            try
            {
                using (var ctx = GetContext())
                {
                    var item = await ctx.LoadAsync<TranscribeData>(id, timestamp);
                    data = item;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            return data;
        }

        public async Task<bool> SaveTranscribeData(TranscribeData data)
        {
            var retval = false;
            try
            {
                using (var ctx = GetContext())
                {
                    await ctx.SaveAsync(data);
                    retval = true;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            return retval;
        }
    }
}
