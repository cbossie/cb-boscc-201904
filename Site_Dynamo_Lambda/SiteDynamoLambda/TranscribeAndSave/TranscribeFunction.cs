using System;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.DynamoDBv2.Model;
using BostonCodeCampModels.Transcribe;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace TranscribeAndSave
{
    public class TranscribeFunction
    {
        public void FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process {dynamoEvent.Records.Count} records...");

            foreach (var record in dynamoEvent.Records)
            {
                string id = record.Dynamodb.NewImage["id"].S;


                context.Logger.LogLine($"Event ID: {record.EventID}");
                context.Logger.LogLine($"Event Name: {record.EventName}");


                var text = record.Dynamodb.NewImage["TextToTranscribe"].S;
                context.Logger.LogLine($"Going to Transcribe \"{text}\"");

                // Only process if not complete
                if (!record.Dynamodb.NewImage["Complete"].BOOL)
                {
                    context.Logger.LogLine($"Processing Transcription for Item {id}");



                }




                // Polly Here













            }

            context.Logger.LogLine("Stream processing complete.");
        }
    }
}