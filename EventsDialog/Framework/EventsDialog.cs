using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EventsBot.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Cognitive.LUIS;
using System.Configuration;
using EventsDialog.Extensions;
using EventsDialog.Framework;

namespace EventsBot.Dialogs.Framework
{
    [Serializable]
    public class EventsDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var msg = (await result as Activity).Text;
            var cmdMatch = new Regex("cmd:\\/\\/(?<commandName>.*)\\/(?<commandParam>.*)").Match(msg);

            if (cmdMatch.Success)
            {
                // explicit command
                var commandName = cmdMatch.Groups["commandName"].Value;
                var commandParam = cmdMatch.Groups["commandParam"].Value;

                if (commandName == "register")
                {
                    var dialog = ServiceLocator.EventRegistration;
                    dialog.EventId = commandParam;
                    context.Call(dialog, ResumeAfterRegisterDialogCompleted);
                }
                else
                {
                    await context.SayAsync($"unknown command {msg}");
                }
            }
            else
            {
                // delegate to luis
                LuisResult luisResult = null;
                await context.Activity.DoWithTyping(async () =>
                {
                    var luisId = ConfigurationManager.AppSettings["luisId"];
                    var luisKey = ConfigurationManager.AppSettings["luisKey"];

                    if (string.IsNullOrEmpty(luisId))
                    {
                        throw new Exception(@"populate appsetting <add key=""luisId"" value=""""/>");
                    }
                    if (string.IsNullOrEmpty(luisKey))
                    {
                        throw new Exception(@"populate appsetting <add key=""luisKey"" value=""""/>");
                    }

                    var luisClient = new LuisClient(luisId, luisKey);

                    luisResult = await luisClient.Predict(msg);
                });

                if (luisResult.TopScoringIntent.Name == "discover")
                {
                    var month = luisResult.Entities.FirstOrDefault(x => x.Key == "builtin.datetimeV2.daterange").Value?.FirstOrDefault();
                    context.Call(new DiscoverDialog(month?.Value), ResumeAfterDiscoverDialogCompleted);
                }
            }
        }

        private async Task ResumeAfterRegisterDialogCompleted(IDialogContext context, IAwaitable<object> serverName)
        {
        }

        private async Task ResumeAfterDiscoverDialogCompleted(IDialogContext context, IAwaitable<object> serverName)
        {
        }
    }
}