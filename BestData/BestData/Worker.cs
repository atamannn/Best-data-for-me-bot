using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BestData
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private static TelegramBotClient Bot;
        private static User me;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Bot = new TelegramBotClient(Configuration.BotToken);

                
                me = await Bot.GetMeAsync();
                Console.Title = me.Username;

                var cts = new CancellationTokenSource();

                //await Bot.SendTextMessageAsync(new ChatId(me.Username), "test", cancellationToken: cts.Token);

                // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
                Bot.StartReceiving(
                    new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                    cts.Token
                );

                Console.WriteLine($"Start listening for @{me.Username}");
                Console.ReadLine();

                // Send cancellation request to stop bot
                cts.Cancel();
            }
        }

        public static async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            await Bot.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Choose", cancellationToken: cancellationToken);
        }

        

        public static async Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
        }
    }
}
