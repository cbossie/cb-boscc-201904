using System;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.DynamoDBv2.Model;
using BostonCodeCampModels.Transcribe;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using TranscribeAndSave.Service;
using BostonCodeCampServices.Service;
using Microsoft.Extensions.Configuration;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace TranscribeAndSave
{
    public class TranscribeFunction
    {
        public IConfigurationService ConfigService { get; }
        public IEnvironmentService Environment { get; }
        public ITranscribeDataService TransDataSvc { get; }

        public TranscribeFunction()
        {
            Environment = new EnvironmentService();
            ConfigService = new ConfigurationService(Environment);

            // Set up Dependency Injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();

            TransDataSvc = services.GetService<ITranscribeDataService>();

        }


        public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            try
            {
                context.Logger.LogLine($"Beginning to process {dynamoEvent.Records.Count} records...");

                foreach (var record in dynamoEvent.Records)
                {
                    string id = record.Dynamodb.NewImage["id"].S;
                    long ts = Convert.ToInt64(record.Dynamodb.NewImage["timestamp"].N);

                    context.Logger.LogLine($"Event ID: {record.EventID}");
                    context.Logger.LogLine($"Event Name: {record.EventName}");


                    var text = record.Dynamodb.NewImage["TextToTranscribe"].S;
                    context.Logger.LogLine($"Going to Transcribe \"{text}\"");

                    // Only process if not complete
                    if (!record.Dynamodb.NewImage["Complete"].BOOL)
                    {
                        context.Logger.LogLine($"Processing Transcription for Item {id}");

                        var item = await TransDataSvc.GetTranscribeData(id, ts);

                        context.Logger.LogLine("Retrieved item");



                        // Polly Here



                        // Save it
                        item.Complete = true;
                        item.OutputFileData = "This is a file";
                        var res = await TransDataSvc.SaveTranscribeData(item);
                        if (res)
                        {
                            context.Logger.LogLine("Successfully Updated");
                        }


                    }


















                }
            }
            catch(Exception ex)
            {
                context.Logger.LogLine($"Exception: {ex.Message}");
            }
            context.Logger.LogLine("Stream processing complete.");
        }


        private void ConfigureServices(IServiceCollection services)
        {
            //AWS Services
            services.AddSingleton(ConfigService);
            var awsOptions = ConfigService.GetConfiguration().GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddTransient<ITranscribeDataService, TranscribeDataService>();
            services.AddTransient<IPollyService, PollyService>();
        }

    }
}