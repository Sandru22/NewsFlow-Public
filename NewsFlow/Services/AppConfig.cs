using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsFlow.Services
{
    public static class AppConfig
    {
        public static string ApiBaseUrl { get; set; } 

        static AppConfig()
        {
            ApiBaseUrl = "https://api.newsflowapi.uk/api";
        }
    }
}
