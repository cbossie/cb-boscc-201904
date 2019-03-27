using System;
using System.Collections.Generic;
using System.Text;

namespace BostonCodeCampModels.Transcribe
{
    public class GeneralConfig : IGeneralConfig
    {
        public string BucketName { get; set; }

        public string FileExtension { get; set; }

        public string AudioMimeType { get; set; }
    }
}
