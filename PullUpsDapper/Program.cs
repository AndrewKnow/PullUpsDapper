
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

                

                if (update.Type == UpdateType.Message)
                {
                    var (level, count, program) = userRepository.GetUsersId(userId);
                    switch (message.Text.ToLower())
                    {
                        case "/start":
                            if (level != null && count == 1 && program == true)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    @$"{name}, твоя программа ""{level}"" можешь проверить свою программу тренирок и зписать результат"
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 3);
                            }

                            if (level != null && count == 1 && program == false)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    @$"{name}, твоя программа ""{level}"" нажми на ""🦾создать программу тренировок"" чтобы начать тренировки:"
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 1);
                            }

                            if (level == null && count == 1 && program == false)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    @$"{name}, тебе нужно выбрать уровень тернировок"
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 2);
                            }

                            else if (level == null && count == 0 && program == false)
                            {
                                User user = new User();
                                user.IdUser = userId;
                                user.Name = name;
                                userRepository.CreateUser(user);

                                await botClient.SendTextMessageAsync(message.Chat,
                                    $"Привет {name}, выбери программу тренировок:"
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 2);
                            }

                            break;
                        case "/menu":
                            await RemoveReplyKeboard(botClient, message);
                            await SendReplyKeboard(botClient, message, 0);

                            break;
                        case "🦾создать программу тренировок":
                            if (program)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    @$"{name}, у тебя уже сть программа тернировок ""{level}"""
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                userRepository.CreateTrainingProgram(level, userId);
                            }
                            break;
                        case "✅oтчёт о выполнении":

                            break;
                        case "💪моя задача на сегодня":

                            // вывод в список программы тернировок
                            userRepository.DayStatus(userId);

                            break;
                        case "📊график":
                            break;
                        case "❌удалить программу":

                            break;
                        case "новичок":
                            userRepository.UpdateUser("Новичок", userId);

                            (level, count, program) = userRepository.GetUsersId(userId);

                            userRepository.CreateTrainingProgram(level, userId);
                            
                            await botClient.SendTextMessageAsync(message.Chat,
                                @$"{name}, твоя программа ""{level}"" начинай тренироваться и зписывай результат"
                                + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                cancellationToken: cancellationToken);
                            await RemoveReplyKeboard(botClient, message);
                            await SendReplyKeboard(botClient, message, 3);
                            break;
                        case "профи":
                            userRepository.UpdateUser("Профи", userId);

                            (level, count, program) = userRepository.GetUsersId(userId);

                            userRepository.CreateTrainingProgram(level, userId);

                            await botClient.SendTextMessageAsync(message.Chat,
                                @$"{name}, твоя программа ""{level}"" начинай тренироваться и зписывай результат"
                                + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                cancellationToken: cancellationToken);
                            await RemoveReplyKeboard(botClient, message);
                            await SendReplyKeboard(botClient, message, 3);
                            break;
                        case "турникмэн":
                            userRepository.UpdateUser("Турникмен", userId);

                            (level, count, program) = userRepository.GetUsersId(userId);

                            userRepository.CreateTrainingProgram(level, userId);

                            await botClient.SendTextMessageAsync(message.Chat,
                                @$"{name}, твоя программа ""{level}"" начинай тренироваться и зписывай результат"
                                + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                cancellationToken: cancellationToken);
                            await RemoveReplyKeboard(botClient, message);
                            await SendReplyKeboard(botClient, message, 3);
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
                            new KeyboardButton [] { char.ConvertFromUtf32(0x1F4AA) + "Моя задача на сегодня" },
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

                case 2:
                    replyKeyboardMarkup = new(
                       new[]
                       {
                            new KeyboardButton [] { "Новичок" },
                            new KeyboardButton [] { "Профи" },
                            new KeyboardButton [] { "Турникмэн" }
                       })
                    {

                        ResizeKeyboard = true
                    };
                    break;
                case 3:
                    replyKeyboardMarkup = new(
                       new[]
                       {
                            new KeyboardButton [] { char.ConvertFromUtf32(0x2705) + "Отчёт о выполнении"},
                            new KeyboardButton [] { char.ConvertFromUtf32(0x1F4AA) + "Моя задача на сегодня" },
                            new KeyboardButton [] { char.ConvertFromUtf32(0x1F4CA) + "График" },
                            new KeyboardButton [] { char.ConvertFromUtf32(0x274C) + "Удалить программу" },
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
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "..."
                         , replyMarkup: new ReplyKeyboardRemove());

        }

    }
}
