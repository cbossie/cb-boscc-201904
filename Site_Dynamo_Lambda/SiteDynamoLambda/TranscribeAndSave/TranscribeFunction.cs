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
using Amazon.Polly;
using Amazon.S3;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace TranscribeAndSave
{
    public class TranscribeFunction
    {
        public IConfigurationService ConfigService { get; }
        public IEnvironmentService Environment { get; }
        public ITranscribeDataService TransDataSvc { get; }
        public IPollyService PollySvc { get; }
        public IConfiguration Config { get; }
        public IFileService FileSvc { get; }

        public TranscribeFunction()
        {
            Environment = new EnvironmentService();
            ConfigService = new ConfigurationService(Environment);
            Config = ConfigService.GetConfiguration();


            // Set up Dependency Injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();

            TransDataSvc = services.GetService<ITranscribeDataService>();
            PollySvc = services.GetService<IPollyService>();
            FileSvc = services.GetService<IFileService>();

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

                        context.Logger.LogLine("Retrieved item. Beginning Synthesis");



                        // Polly Here
                        var stream = await PollySvc.TranscribeText(item.TextToTranscribe);

                        context.Logger.LogLine("Transcoded text");



                        // S3 Here
                        var fileName = FileSvc.GetFileName(item.Id);

                        context.Logger.LogLine($"Attempting to write file {fileName} to S3");

                        var fileRes = await FileSvc.SaveFile(stream, fileName);
                        if(fileRes)
                        {
                            item.Complete = true;
                            item.OutputFileData = fileName;
                        }
                        else
                        {
                            item.OutputFileData = "Error Synthesizing";
                        }


                        //Save Updated Record
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
            // Configuration
            var genConf = new GeneralConfig();
            Config.Bind("GeneralConfig", genConf);
            services.AddSingleton<IGeneralConfig>(genConf);
            
            //AWS Services
            services.AddSingleton(ConfigService);
            var awsOptions = Config.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddAWSService<IAmazonPolly>();
            services.AddAWSService<IAmazonS3>();


            services.AddTransient<ITranscribeDataService, TranscribeDataService>();
            services.AddTransient<IPollyService, PollyService>();
            services.AddTransient<IFileService, FileService>();
        }

    }
}