using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace TranscribeAndSave.Service
{
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}
