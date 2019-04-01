using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCredentialsDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Instantiate AWS Service Client
            Amazon.S3.AmazonS3Client Cli = new Amazon.S3.AmazonS3Client();

            // Configure Request
            var blr = new ListBucketsRequest();

            // Issue Request
            var bucketResponse = Cli.ListBuckets(blr);

            //Process Response
            foreach(var b in bucketResponse.Buckets)
            {
                Console.WriteLine($"Bucket:{b.BucketName}, Created: {b.CreationDate.ToLongDateString()}");
            }

            Console.ReadKey();


        }
       
    }
}
