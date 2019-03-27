using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BostonCodeCampModels.Transcribe;
using System;
using System.Collections.Generic;
using System.IO;

using System.Text;
using System.Threading.Tasks;

namespace BostonCodeCampServices.Service
{
    
    public class FileService : IFileService
    {
        private IAmazonS3 S3Client { get; }
        private IGeneralConfig Config { get; }
      
        public FileService(IAmazonS3 s3, IGeneralConfig config)
        {
            Config = config;
            S3Client = s3;
        }

        public string GetFileName(string name)
        {
            return $"{name}{Config.FileExtension}";
        }

        public async Task<bool> SaveFile(byte[] data, string fileName)
        {
            var ms = new MemoryStream(data);
            var putRequest = new PutObjectRequest()
            {
                Key = fileName,
                ContentType = Config.AudioMimeType,
                InputStream = ms,
                BucketName = Config.BucketName

            };

            var res = await S3Client.PutObjectAsync(putRequest);

            return res.HttpStatusCode == System.Net.HttpStatusCode.OK;

        }

        public async Task<byte[]> GetFile(string key)
        {
            var getRequest = new GetObjectRequest()
            {
                Key = key,
                BucketName = Config.BucketName
            };

            var str = await S3Client.GetObjectAsync(getRequest);
            var arr = StreamUtilities.ReadFully(str.ResponseStream);


            return arr;
        }
    }
}
