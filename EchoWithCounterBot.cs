// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EchoBotWithCounter;
using EchoBotWithCounter.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{
    public class EchoWithCounterBot : IBot
    {
        private readonly BotServices _botServices;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private DialogSet _dialogs;

        public EchoWithCounterBot(BotServices botServices, ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _botServices = botServices ?? throw new ArgumentNullException(nameof(botServices));

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                RequestPhotoStepAsync,
                RequestPhotoConfirmStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            _dialogs = new DialogSet(_conversationState.CreateProperty<DialogState>(nameof(EchoWithCounterBot)));
            _dialogs.Add(new WaterfallDialog("details", waterfallSteps));
            _dialogs.Add(new AttachmentPrompt("requestPhoto"));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                // If the DialogTurnStatus is Empty we should start a new dialog.
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync("details", null, cancellationToken);
                }
            }
        }

        private static async Task<DialogTurnResult> RequestPhotoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("requestPhoto", new PromptOptions { Prompt = MessageFactory.Text("I need to verify your age before we can continue. Please send me a picture of yourself so I can confirm your age.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> RequestPhotoConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Attachments == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I need you to send me a picture of yourself before we can continue. We'll start again now. Take a photo with your webcam or phone camera and come back to me to send it to me as an attachment."), cancellationToken);
            }

            if (stepContext.Context.Activity.Attachments.Count > 0)
            {
                // Get the source image
                var connector = new ConnectorClient(new Uri(stepContext.Context.Activity.ServiceUrl));
                var sourceImage = await connector.HttpClient.GetStreamAsync(stepContext.Context.Activity.Attachments.FirstOrDefault().ContentUrl);

                // Call Face Api
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(_botServices.FaceApiEndpoint),
                };
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _botServices.FaceApiKey);

                // Setup data object
                HttpContent content = new StreamContent(sourceImage);
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/octet-stream");

                // Request parameters
                var uri = $"{_botServices.FaceApiEndpoint}/detect?returnFaceId=false&returnFaceLandmarks=false&returnFaceAttributes=age";

                // Make request
                var responseMessage = await httpClient.PostAsync(uri, content);

                // get age
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                var faces = JsonConvert.DeserializeObject<IEnumerable<FaceResponseDto>>(responseString.ToString());
                var firstFaceAge = faces.FirstOrDefault().FaceAttributes.Age;

                // Respond to user
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You appear to be {firstFaceAge.ToString()} years old."), cancellationToken);

                if (firstFaceAge > 25)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You are old enough to receive information about our products, how can I help?"), cancellationToken);
                }
                else if (firstFaceAge > 16)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I cannot provide information to people younger than 16 but also operate the 'Challenge 25' principle. You appear to be between 16 and 25 and so I cannot help you via this channel, sorry."), cancellationToken);
                }
                else {

                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I cannot provide information to people younger than 16 so cannot help you via this channel, sorry."), cancellationToken);
                }
            }

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
