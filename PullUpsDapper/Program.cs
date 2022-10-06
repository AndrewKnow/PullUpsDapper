
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text;

namespace PullUpsDapper
{
    internal class Program
    {
        public static long User { get; set; }

        static void Main(string[] args)
        {
            string Key = Password.Bot();
            var bot = new TelegramBotClient(Key);
            UserDayProgram.DayReport = false;
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
                Message message = update.Message;

                UserRepository userRepository = new();
                User user = new User();

                if (update.Type == UpdateType.CallbackQuery)
                {
                    CallbackQuery callbackQuery = update.CallbackQuery;
                    var messageCB = callbackQuery.Data;
                    switch (messageCB)
                    {
                        case "Удалить":
                            userRepository.DeleteUserProgram(User);
                            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat, "Программа удалена",
                                cancellationToken: cancellationToken);
                            break;
                        case "Отмена":
                            break;
                    }
                }
                if (update.Type == UpdateType.Message)
                {
                    var userId = message.From.Id;
                    var name = message.From.FirstName;
                    var list = userRepository.GetUsers();
                    var (level, count) = userRepository.GetUsersId(userId);

                    if (UserDayProgram.DayReport)
                    {
                        bool pullsCheck = int.TryParse(message.Text, out int result);
                        if (pullsCheck)
                        {
                            string checkResult = userRepository.DayResult(userId, result);

                            
                            if (result >= 1 && result <= 4)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    $"Записал {result} повторения за сегодня, ты {checkResult} программу на сегодня",
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    $"Записал {result} повторений за сегодня, ты {checkResult} программу на сегодня",
                                    cancellationToken: cancellationToken);
                            }

                            UserDayProgram.DayReport = false;
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat,
                                "Введено не число, ввод данных отменён!",
                                cancellationToken: cancellationToken);
                            UserDayProgram.DayReport = false;
                        }
                    }

                    switch (message.Text)
                    {
                        case "/start":
                            if (level != null && count == 1 )
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    @$"{name}, твоя программа ""{level}"" можешь проверить свою программу тренировок и зписать результат"
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 3);
                            }

                            if (level == null && count == 1)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    @$"{name}, у тебя не выбрана программа тренировок"
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 2);
                            }
                            else if (level == null && count == 0)
                            {
                                
                                user.IdUser = userId;
                                user.Name = name;
                                userRepository.CreateUser(user);

                                await botClient.SendTextMessageAsync(message.Chat,
                                    $"Привет {name}, завёл тебя в свою базу, выбери программу тренировок"
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 2);
                            }
                            break;

                        case "/menu":
                            if (userId == 1209629878)
                            {
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 5);
                            }
                            else
                            {
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 0);
                            }
                            break;

                        case "✔️Отчёт о выполнении":

                            if (level != null && count == 1)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                "Введи общее количество выполненных повторений:",
                                cancellationToken: cancellationToken);
                                UserDayProgram.DayReport = true;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                "Не создана программ тренировок!\nВыбирайте:",
                                cancellationToken: cancellationToken);
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 2);
                            }
                            break;

                        case "💪Моя задача на сегодня":
                            if (level != null && count == 1)
                            {
                                var userDayProgram = userRepository.DayStatus(userId, level);
                                await botClient.SendTextMessageAsync(message.Chat,
                                 $"Дата: {DateTime.Today.ToShortDateString()}",
                                cancellationToken: cancellationToken);

                                StringBuilder sb = new StringBuilder();
                                var i = 0;
                                string tabs7 = new string('\t', 7);
                                string tabs6 = new string('\t', 6);
                                string tabs11 = new string('\t', 11);
                                string tabs12 = new string('\t', 12);
                                foreach (var item in userDayProgram)
                                {
                                    if (i == 0)
                                    {
                                        sb.Append($"\u007CПодход\u007CПовторения\u007C\n");
                                    }    

                                    if (item.Pulls.ToString().Length == 1)
                                    {
                                        sb.Append(
                                            $"\u007C{tabs7}{item.Approach}{tabs6}" +
                                            $"\u007C" +
                                            $"{tabs11}{item.Pulls}" +
                                            $"{tabs12}" +
                                            $"\u007C\n");
                                        i++;
                                    }
                                    else
                                    {
                                        sb.Append(
                                            $"\u007C{tabs7}{item.Approach}{tabs6}" +
                                            $"\u007C" +
                                            $"{tabs11}{item.Pulls}" +
                                            $"{tabs11}" +
                                            $"\u007C\n");
                                        i++;
                                    }
                                }

                                await botClient.SendTextMessageAsync(message.Chat,
                                    sb.ToString(),
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                "Не создана программ тренировок!\nВыбирайте:",
                                cancellationToken: cancellationToken);
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 2);
                            }
                            break;

                        case "❌Удалить программу":

                            var keyboard = new InlineKeyboardMarkup(new[]
                            {
                                    new []
                                    {
                                        InlineKeyboardButton.WithCallbackData("Удалить", "Удалить"),
                                    },
                                    new []
                                    {
                                        InlineKeyboardButton.WithCallbackData("Отмена", "Отмена"),
                                    },

                                    }
                            );
                            User = userId;
                            await botClient.SendTextMessageAsync(message.Chat, "Подтверждение удаления:" + char.ConvertFromUtf32(0x1F447),
                                replyMarkup: keyboard, cancellationToken: cancellationToken);

                            //userRepository.DeleteUserProgram(userId);

                            break;

                        case "Новичок":
                            userRepository.UpdateUser("Новичок", userId);


                            var s = userRepository.GetUsersLevel(userId).Level;

                            (level, count) = userRepository.GetUsersId(userId);
                            userRepository.CreateTrainingProgram(level, userId);
                            
                            await botClient.SendTextMessageAsync(message.Chat,
                                @$"{name}, твоя программа ""{level}"" начинай тренироваться и зписывай результат"
                                + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                cancellationToken: cancellationToken);
                            await RemoveReplyKeboard(botClient, message);
                            await SendReplyKeboard(botClient, message, 3);
                            break;

                        case "Профи":
                            userRepository.UpdateUser("Профи", userId);

                            (level, count) = userRepository.GetUsersId(userId);
                            userRepository.CreateTrainingProgram(level, userId);

                            await botClient.SendTextMessageAsync(message.Chat,
                                @$"{name}, твоя программа ""{level}"" начинай тренироваться и зписывай результат"
                                + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                cancellationToken: cancellationToken);
                            await RemoveReplyKeboard(botClient, message);
                            await SendReplyKeboard(botClient, message, 3);
                            break;

                        case "Турникмэн":
                            userRepository.UpdateUser("Турникмен", userId);

                            (level, count) = userRepository.GetUsersId(userId);
                            userRepository.CreateTrainingProgram(level, userId);

                            await botClient.SendTextMessageAsync(message.Chat,
                                @$"{name}, твоя программа ""{level}"" начинай тренироваться и зписывай результат"
                                + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                cancellationToken: cancellationToken);
                            await RemoveReplyKeboard(botClient, message);
                            await SendReplyKeboard(botClient, message, 3);
                            break;

                        case "Администратор":
                            userRepository.CreateLevelProgram();
                            await botClient.SendTextMessageAsync(message.Chat,
                            "Я всё сделал хозяин",
                            cancellationToken: cancellationToken);
                            break;

                        case "📊График":

                            if (level != null && count == 1)
                            {
                                StringBuilder sb = new StringBuilder();
                                var userReport = userRepository.UserReport(userId, level);
                                var i = 0;
                                string tabs = "";
                                string tabs4 = new string('\t', 4);
                                string tabs5 = new string('\t', 5);
                                string tabs7 = new string('\t', 7);
                                string tabs6 = new string('\t', 6);
                                string tabs3 = new string('\t', 3);
                                string tabs2 = new string('\t', 2);
                                string tabs1 = new string('\t', 1);
                                string tabs13 = new string('\t', 13);
                                string tabs12 = new string('\t', 12);
                                sb.Append("\u007CНеделя\u007CПлан\u007CФакт\n");
                                foreach (var item in userReport)
                                {
                                    string emoji="";
                                    if (item.Fact < item.Plan) emoji = "😤";
                                    if (item.Fact == item.Plan) emoji = "💪🏻";
                                    if (item.Fact > item.Plan) emoji = "🦾";
                                    //tabs = item.Fact == 0 ? tabs4 : tabs1;
                                    if (item.Fact >= 100) tabs = tabs2;
                                    if (item.Fact >= 10 && item.Fact <=99) tabs = tabs6;
                                    if (item.Fact < 10 ) tabs = tabs7;

                                    i++;
                                    if (i == 1 || i < 10)
                                    {
                                        if (item.Plan > 99 && item.Fact > 99)
                                            sb.Append($"\u007C{tabs6}{item.Week}{tabs7}\u007C{item.Plan}{tabs3}\u007C{tabs2}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan <= 99 && item.Fact <= 99)
                                            sb.Append($"\u007C{tabs6}{item.Week}{tabs7}\u007C {tabs1}{item.Plan}{tabs3}\u007C{tabs2}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan > 99 && item.Fact <= 99)
                                            sb.Append($"\u007C{tabs6}{item.Week}{tabs7}\u007C{item.Plan}{tabs3}\u007C{tabs2}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan <= 99 && item.Fact > 99)
                                            sb.Append($"\u007C{tabs6}{item.Week}{tabs7}\u007C {tabs1}{item.Plan}{tabs3}\u007C{tabs2}{item.Fact}{tabs}{emoji}\n");
                                    }
                                    else
                                    {
                                        if (item.Plan > 99 && item.Fact > 99)
                                            sb.Append($"\u007C{tabs5}{item.Week}{tabs6}\u007C{item.Plan}{tabs3}\u007C{tabs2}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan <= 99 && item.Fact <= 99)
                                            sb.Append($"\u007C{tabs5}{item.Week}{tabs6}\u007C {tabs1}{item.Plan}{tabs3}\u007C{tabs2}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan > 99 && item.Fact <= 99)
                                            sb.Append($"\u007C{tabs5}{item.Week}{tabs6}\u007C{item.Plan}{tabs3}\u007C{tabs2}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan <= 99 && item.Fact > 99)
                                            sb.Append($"\u007C{tabs5}{item.Week}{tabs6}\u007C {tabs1}{item.Plan}{tabs3}\u007C{tabs2}{item.Fact}{tabs}{emoji}\n");
                                    }
                                }

                                await botClient.SendTextMessageAsync(message.Chat,
                                sb.ToString(),
                                cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                "Не создана программ тренировок!\nВыбирайте:",
                                cancellationToken: cancellationToken);
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 2);
                            }

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
                case 5:
                    replyKeyboardMarkup = new(
                       new[]
                       {
                            new KeyboardButton [] { "Администратор" },
                            //new KeyboardButton [] { "🦾Создать программу тренировок" },
                            new KeyboardButton [] { "✔️Отчёт о выполнении"},
                            new KeyboardButton [] { "💪Моя задача на сегодня" },
                            new KeyboardButton [] { "📊График" },
                            new KeyboardButton [] { "❌Удалить программу" },
                       })
                    {

                        ResizeKeyboard = true
                    };
                    break;

                case 0:
                    replyKeyboardMarkup = new(
                       new[]
                       {
                            new KeyboardButton [] { "🦾Создать программу тренировок" },
                            new KeyboardButton [] { "✔️Отчёт о выполнении"},
                            new KeyboardButton [] { "💪Моя задача на сегодня" },
                            new KeyboardButton [] { "📊График" },
                            new KeyboardButton [] { "❌Удалить программу" },
                       })
                    {

                        ResizeKeyboard = true
                    };
                    break;
                case 1:
                    replyKeyboardMarkup = new(
                       new[]
                       {
                            new KeyboardButton [] { "🦾Создать программу тренировок" },
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
                            new KeyboardButton [] { "✔️Отчёт о выполнении"},
                            new KeyboardButton [] { "💪Моя задача на сегодня" },
                            new KeyboardButton [] { "📊График" },
                            new KeyboardButton [] { "❌Удалить программу" },
                       })
                    {

                        ResizeKeyboard = true
                    };
                    break;
            }
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "👇🏻", replyMarkup: replyKeyboardMarkup);
        }

        static async Task<Message> RemoveReplyKeboard(ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "..."
                         , replyMarkup: new ReplyKeyboardRemove());
        }

    }
}
