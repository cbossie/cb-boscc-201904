﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SiteDynamoLambda.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            int a = 3;
        }
    }
}
