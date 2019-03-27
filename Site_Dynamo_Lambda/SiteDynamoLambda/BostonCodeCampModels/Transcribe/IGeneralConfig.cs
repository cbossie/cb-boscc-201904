using System;
using System.Collections.Generic;
using System.Text;

namespace BostonCodeCampModels.Transcribe
{
    public interface IGeneralConfig
    {
        string BucketName { get; }
        string FileExtension { get; }
        string AudioMimeType { get; }
    }
}
