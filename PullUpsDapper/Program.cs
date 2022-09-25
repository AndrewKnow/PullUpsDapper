
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Telegram.Bot.Types.ReplyMarkups;


namespace PullUpsDapper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string Key = Password.Bot();
            var bot = new TelegramBotClient(Key);

            Console.WriteLine("Включён бот " + bot.GetMeAsync().Result.FirstName);

            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };

            bot.StartReceiving(updateHandler: HandleUpdateAsync,
                   errorHandler: HandleErrorAsync,
                   receiverOptions: new ReceiverOptions()
                   {
                       AllowedUpdates = Array.Empty<UpdateType>()
                   },
                   cancellationToken: cts.Token);

            Console.ReadLine();
            cts.Cancel();
        }
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            var ErrorMessage = exception.ToString();

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                Message? message = update.Message;
                var userId = message.From.Id;
                var name = message.From.FirstName;


                UserRepository userRepository = new();
                var list = userRepository.GetUsers();

                var count = userRepository.GetUsersId(userId);


                if (update.Type == UpdateType.Message)
                {
                    switch (message.Text.ToLower())
                    {
                        case "/start":

                            if (count > 0)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    $"{name} подсчитал статус выполенния твоей программы тренировок"
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                User user = new User();
                                user.IdUser = userId;
                                user.Name = name;

                                userRepository.CreateUser(user);

                                await botClient.SendTextMessageAsync(message.Chat,
                                    $"Привет {name} я бот который создаст программу тернировок и запомнит твои достижения, жми:"
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 1);

                            }

                            break;
                        case "/menu":
                            await RemoveReplyKeboard(botClient, message);
                            await SendReplyKeboard(botClient, message, 0);
                            break;
                        case "🦾создать программу тренировок":
                            break;
                        case "✅oтчёт о выполнении":
                            break;
                        case "💪моя программа":
                            break;
                        case "📊график":
                            break;
                        case "❌удалить программу":
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        static async Task<Message> SendReplyKeboard(ITelegramBotClient botClient, Message message, int lvl)
        {
            ReplyKeyboardMarkup? replyKeyboardMarkup = null;
            switch (lvl)
            {
                case 0:
                    replyKeyboardMarkup = new(
                       new[]
                       {
                        new KeyboardButton [] { char.ConvertFromUtf32(0x1F9BE) + "Создать программу тренировок" },
                        new KeyboardButton [] { char.ConvertFromUtf32(0x2705) + "Отчёт о выполнении"},
                        new KeyboardButton [] { char.ConvertFromUtf32(0x1F4AA) + "Моя программа" },
                        new KeyboardButton [] { char.ConvertFromUtf32(0x1F4CA) + "График" },
                        new KeyboardButton [] { char.ConvertFromUtf32(0x274C) + "Удалить программу" },

                       })
                    {

                        ResizeKeyboard = true
                    };
                    break;
                case 1:
                    replyKeyboardMarkup = new(
                       new[]
                       {
                        new KeyboardButton [] { char.ConvertFromUtf32(0x1F9BE) + "Создать программу тренировок" }
                       })
                    {

                        ResizeKeyboard = true
                    };
                    break;
            }
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Выберите вариант:", replyMarkup: replyKeyboardMarkup);
        }

        static async Task<Message> RemoveReplyKeboard(ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "💪"
                         , replyMarkup: new ReplyKeyboardRemove());

        }

    }
}
