using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Configuration;

namespace EchoBotWithCounter
{
    public class BotServices
    {
        public BotServices(BotConfiguration botConfiguration)
        {
            foreach (var service in botConfiguration.Services)
            {
                switch (service.Type)
                {
                    case ServiceTypes.Generic:
                        {
                            if (service.Name == "FaceApi")
                            {
                                var faceApi = service as GenericService;

                                if (!string.IsNullOrEmpty(faceApi.Url))
                                {
                                    FaceApiEndpoint = faceApi.Url;
                                }

                                if (!string.IsNullOrEmpty(faceApi.Configuration["key"]))
                                {
                                    FaceApiKey = faceApi.Configuration["key"];
                                }
                            }

                            break;
                        }
                }
            }
        }

        public string FaceApiEndpoint { get; }

        public string FaceApiKey { get; }
    }
}