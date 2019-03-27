﻿using System;
using System.Collections.Generic;
using System.Text;
using static BostonCodeCampServices.Constants;

namespace TranscribeAndSave.Service
{
    public class EnvironmentService : IEnvironmentService
    {
        public EnvironmentService()
        {
            EnvironmentName = Environment.GetEnvironmentVariable(EnvironmentVariables.AspnetCoreEnvironment) ?? Environments.Production;
        }

        public string EnvironmentName { get; set; }
    }
}
