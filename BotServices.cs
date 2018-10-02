using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Configuration;

namespace EchoBotWithCounter
{
    /// <summary>
    /// Represents references to external services.
    ///
    /// For example, LUIS services are kept here as a singleton.  This external service is configured
    /// using the <see cref="BotConfiguration"/> class.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    /// <seealso cref="https://www.luis.ai/home"/>
    public class BotServices
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotServices"/> class.
        /// </summary>
        /// <param name="botConfiguration">A dictionary of named <see cref="BotConfiguration"/> instances for usage within the bot.</param>
        public BotServices(BotConfiguration botConfiguration)
        {
            foreach (var service in botConfiguration.Services)
            {
                switch (service.Type)
                {
                    case ServiceTypes.Generic:
                        {
                            //if (service.Name == "FaceApi")
                            //{
                            //    var faceApi = service as GenericService;

                            //    if (!string.IsNullOrEmpty(faceApi.Url))
                            //    {
                            //        FaceApiEndpoint = faceApi.Url;
                            //    }

                            //    if (!string.IsNullOrEmpty(faceApi.Configuration["key"]))
                            //    {
                            //        FaceApiKey = faceApi.Configuration["key"];
                            //    }
                            //}

                            break;
                        }
                }
            }
        }

        //public string FaceApiEndpoint { get; }

        //public string FaceApiKey { get; }
    }
}