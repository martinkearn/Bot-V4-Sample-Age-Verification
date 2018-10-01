// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using EchoBotWithCounter;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class EchoWithCounterBot : IBot
    {
        private readonly Accessors _accessors;
        private DialogSet _dialogs;

        private readonly BotServices _services;

        public EchoWithCounterBot(Accessors accessors)
        {
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
            _dialogs = new DialogSet(accessors.ConversationDialogState);

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                RequestPhotoStepAsync,
                RequestPhotoConfirmStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
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
                // Run the DialogSet - let the framework identify the current state of the dialog from
                // the dialog stack and figure out what (if any) is the active dialog.
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                // If the DialogTurnStatus is Empty we should start a new dialog.
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync("details", null, cancellationToken);
                }
            }

            // Processes ConversationUpdate Activities to welcome the user.
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded.Any())
                {
                    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            }

            // Save the dialog state into the conversation state.
            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = turnContext.Activity.CreateReply();
                    reply.Text = "Hi, how can I help you?";
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }

        private static async Task<DialogTurnResult> RequestPhotoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("requestPhoto", new PromptOptions { Prompt = MessageFactory.Text("Sorry, but I need to verify your age before we can continue. Please send me a selfie so I can confirm your age.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> RequestPhotoConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (stepContext.Context.Activity.Attachments == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I need you to send me a picture of yourself before we can continue. We'll start again now. Take a photo with your webcam or phone camera and come back to me to send it to me as an attachment."), cancellationToken);
            }

            if (stepContext.Context.Activity.Attachments.Count > 0)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks for sending me an attachement. Give me a few seconds while I check your age."), cancellationToken);

                // Get the source image
                var connector = new ConnectorClient(new Uri(stepContext.Context.Activity.ServiceUrl));
                var sourceImage = await connector.HttpClient.GetStreamAsync(stepContext.Context.Activity.Attachments.FirstOrDefault().ContentUrl);

                // Call Face Api
                var httpClient = new HttpClient();
                httpClient.BaseAddress = "https://uksouth.api.cognitive.microsoft.com/face/v1.0";
                _accessors.ConversationState.
            }

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
