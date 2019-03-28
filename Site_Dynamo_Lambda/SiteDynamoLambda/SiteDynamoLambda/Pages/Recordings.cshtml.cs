using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BostonCodeCampModels.Transcribe;
using BostonCodeCampServices.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SiteDynamoLambda.Pages
{
    [Authorize]
    public class RecordingsModel : PageModel
    {

        public List<TranscribeData> Data = new List<TranscribeData>();
        IGeneralConfig Config;
        IFileService FileSvc { get; }
        ITranscribeDataService TranSvc { get; }

        public RecordingsModel(ITranscribeDataService svc, IFileService fsvc, IGeneralConfig cfg)
        {
            TranSvc = svc;
            FileSvc = fsvc;
            Config = cfg;
        }

        public async Task OnGet()
        {

            //Get data from DynamoDB
            var recordings = await TranSvc.GetLatestDataToTranscribe();
            Data.Clear();
            Data.AddRange(recordings.OrderByDescending(a => a.TimeStamp));

        }


        public async Task<ActionResult> OnGetDownloadAsync(string file)
        {
            var fileData = await FileSvc.GetFile(file);

            return File(fileData, Config.AudioMimeType, file);

        }


    }
}