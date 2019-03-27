using Amazon.Polly;
using Amazon.Polly.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BostonCodeCampServices.Service
{
    public class PollyService : IPollyService
    {
        private IAmazonPolly Polly { get; }

        public PollyService(IAmazonPolly polly)
        {
            Polly = polly;
        }

        public async Task<byte[]> TranscribeText(string text)
        {
            if(string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            var req = new SynthesizeSpeechRequest()
            {
                VoiceId = VoiceId.Joanna,
                Text = text,
                OutputFormat = OutputFormat.Mp3
            };

            var response = await Polly.SynthesizeSpeechAsync(req);

            var bytes = StreamUtilities.ReadFully(response.AudioStream);
            return bytes;
        }
    }
}
