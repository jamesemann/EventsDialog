using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace EventsDialog.Extensions
{
    public static class ActivityExtensions
    {
        /// Runs a longish running Action and sends a Typing activity
        /// to the user until the action is completed.
        /// <summary>
        ///     Send a Typing activity to the user while a long running operation runs.
        /// </summary>
        /// <example>
        ///     <code>
        ///         await context.Activity.DoWithTyping(async () =>
        ///         {
        ///             await Task.Delay(10000);
        ///         });
        ///         
        ///         await context.SayAsync($"The long running operation has completed.");
        ///     </code>
        /// </example>
        public static async Task DoWithTyping(this IActivity activity, Func<Task> action)
        {
            var cts = new CancellationTokenSource();

            activity.SendTypingActivity(cts.Token);

            await action.Invoke().ContinueWith(task => { cts.Cancel(); });
        }

        public static async Task SendTypingActivity(this IActivity iactivity)
        {
            if (iactivity is Activity activity)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                var isTypingReply = activity.CreateReply();
                isTypingReply.Type = ActivityTypes.Typing;
                await connector.Conversations.ReplyToActivityAsync(isTypingReply);

                var delayTask = Task.Delay(1000);
                await delayTask.ContinueWith(task => { });
            }
        }

        private static async Task SendTypingActivity(this IActivity iactivity, CancellationToken cancellationToken)
        {
            if (iactivity is Activity activity)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                while (!cancellationToken.IsCancellationRequested)
                {
                    var isTypingReply = activity.CreateReply();
                    isTypingReply.Type = ActivityTypes.Typing;
                    await connector.Conversations.ReplyToActivityAsync(isTypingReply);

                    var delayTask = Task.Delay(3000, cancellationToken);
                    await delayTask.ContinueWith(task => { });
                }
            }
        }
    }
}