using BostonCodeCampServices.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System;
using System.Threading.Tasks;

namespace SiteDynamoLambda.Pages
{
    [Authorize]
    public class TranscribeModel : PageModel
    {
        ITranscribeDataService Svc { get; }

        public TranscribeModel(ITranscribeDataService svc)
        {
            Svc = svc;
        }


        public bool IsError { get; set; }
        public bool IsSuccess { get; set; }

        public async Task OnGet()
        {

        }

        public async Task OnPost(string TextToTranscribe)
        {
            if (string.IsNullOrEmpty(TextToTranscribe))
            {
                IsError = true;
                return;
            }

            try
            {
                var res = await Svc.AddDataToTranscribe(TextToTranscribe);
                IsSuccess = res;
                IsError = !res;
            }
            catch (Exception ex)
            {

                IsError = true;
            }

        }

    }
}