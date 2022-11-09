
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text;
using PullUpsDapper.Users;
using PullUpsDapper.DBrepository;

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
                Users.User user = new ();

                if (update.Type == UpdateType.CallbackQuery)
                {
                    CallbackQuery callbackQuery = update.CallbackQuery;
                    var messageCB = callbackQuery.Data;
                    switch (messageCB)
                    {
                        case "Удалить":
                            userRepository.DeleteUserProgram(User);
                            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat, "🤖 Программа удалена",
                                cancellationToken: cancellationToken);
                            break;
                        case "Отмена":
                            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat, "🤖 Действие отменено",
                                cancellationToken: cancellationToken);
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
                            // Для асинхронного вызова метода
                            // public async Task<string> DayResult(long userId, int pulls)
                            // string checkResult = await userRepository.DayResult(userId, result); 
                            string checkResult = userRepository.DayResult(userId, result);

                            if (result >= 1 && result <= 4)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    $"🤖 Записал {result} повторения за сегодня, ты {checkResult}",
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    $"🤖 Записал {result} повторений за сегодня, ты {checkResult}",
                                    cancellationToken: cancellationToken);
                            }

                            UserDayProgram.DayReport = false;
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat,
                                "🤖 Введено не число, ввод данных отменён!",
                                cancellationToken: cancellationToken);
                            UserDayProgram.DayReport = false;
                        }
                    }

                    if (message.Text != null)
                    {
                        if (message.Text.Substring(0, 1) == "+")
                        {
                            bool pullsCheck = int.TryParse(message.Text.Substring(1, message.Text.Length - 1), out int result);
                            if (pullsCheck)
                            {
                                if (level != null && count == 1)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat,
                                        "🤖 Сохранение данных о выполненных повторениях...",
                                        cancellationToken: cancellationToken);

                                    string checkResult = userRepository.DayResultPlus(userId, result);

                                    if (result == 1)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat,
                                            $"🤖 Добавил {result} повторение к сегодняшнему результату, ты {checkResult}",
                                            cancellationToken: cancellationToken);
                                    }
                                    else if (result >= 2 && result <= 4)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat,
                                            $"🤖 Добавил {result} повторения к сегодняшнему результату, ты {checkResult}",
                                            cancellationToken: cancellationToken);
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat,
                                            $"🤖 Записал {result} повторений к сегодняшнему результату, ты {checkResult}",
                                            cancellationToken: cancellationToken);
                                    }
                                }
                            }
                        }
                    }

                    switch (message.Text)
                    {
                        case "/start":
                            if (level != null && count == 1 )
                            {
                                await RemoveReplyKeboard(botClient, message);
                                await botClient.SendTextMessageAsync(message.Chat,
                                    @$"🤖 {name}, твоя программа ""{level}"" можешь проверить свою программу тренировок и зписать результат. " +
                                    @"Для ввода рузультата можно выбрать кнопку ""✔️Отчёт о выполнении за сегодня"" и записать общее количество выполнненых повторенией" + 
                                    @" или набери ""+"" и количество повторений, тогда резултат будет сумироваться."
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                                
                                await SendReplyKeboard(botClient, message, 3);
                            }

                            if (level == null && count == 1)
                            {
                                await RemoveReplyKeboard(botClient, message);
                                await botClient.SendTextMessageAsync(message.Chat,
                                    @$"🤖 {name}, у тебя не выбрана программа тренировок"
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                                
                                await SendReplyKeboard(botClient, message, 2);
                            }
                            else if (level == null && count == 0)
                            {
                                
                                user.IdUser = userId;
                                user.Name = name;
                                userRepository.CreateUser(user);
                                await RemoveReplyKeboard(botClient, message);
                                await botClient.SendTextMessageAsync(message.Chat,
                                    $"🤖 Привет {name}, завёл тебя в свою базу, выбери программу тренировок"
                                    + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                    cancellationToken: cancellationToken);
                                await SendReplyKeboard(botClient, message, 2);
                            }
                            break;

                        case "/menu":
                            if (userId == 1209629878)
                            {
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 4);
                            }
                            else
                            {
                                await RemoveReplyKeboard(botClient, message);
                                await SendReplyKeboard(botClient, message, 3);
                            }
                            break;

                        case "✔️Отчёт о выполнении за сегодня":

                            if (level != null && count == 1)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                "Введи общее количество выполненных повторений:",
                                cancellationToken: cancellationToken);
                                UserDayProgram.DayReport = true;
                            }
                            else
                            {
                                await RemoveReplyKeboard(botClient, message);
                                await botClient.SendTextMessageAsync(message.Chat,
                                "Не создана программ тренировок!\nВыбирайте:",
                                cancellationToken: cancellationToken);
                                await SendReplyKeboard(botClient, message, 2);
                            }
                            break;

                        case "💪Моя задача на сегодня":
                            if (level != null && count == 1)
                            {
                                var userDayProgram = await userRepository.DayStatus(userId, level);
                                await botClient.SendTextMessageAsync(message.Chat,
                                 $"Дата: {DateTime.Today.ToShortDateString()}",
                                cancellationToken: cancellationToken);

                                StringBuilder sb = new StringBuilder();
                                var i = 0;

                                Func<int, string> whiteSpace = x => new string('\t', x);

                                foreach (var item in userDayProgram)
                                {
                                    if (i == 0)
                                    {
                                        sb.Append($"{whiteSpace(6)}Подход{whiteSpace(4)}Повторения \n");
                                    }    

                                    if (item.Pulls.ToString().Length == 1)
                                    {
                                        sb.Append(
                                            $"|{whiteSpace(7)}{item.Approach}{whiteSpace(6)}" +
                                            $"|" +
                                            $"{whiteSpace(11)}{item.Pulls}" +
                                            $"{whiteSpace(12)}" +
                                            $"|\n");
                                        i++;
                                    }
                                    else
                                    {
                                        sb.Append(
                                            $"|{whiteSpace(7)}{item.Approach}{whiteSpace(6)}" +
                                            $"|" +
                                            $"{whiteSpace(11)}{item.Pulls}" +
                                            $"{whiteSpace(12)}" +
                                            $"|\n");
                                        i++;
                                    }
                                }

                                var (fact, plan) = userRepository.FactPlanToday(userId, level);
                                sb.Append($"\nВыполнено (повторений): {fact} / {plan}");

                                await botClient.SendTextMessageAsync(message.Chat,
                                    sb.ToString(),
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await RemoveReplyKeboard(botClient, message);
                                await botClient.SendTextMessageAsync(message.Chat,
                                "🤖 Не создана программ тренировок!\nВыбирайте:",
                                cancellationToken: cancellationToken);
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
                            await RemoveReplyKeboard(botClient, message);
                            await botClient.SendTextMessageAsync(message.Chat,
                                @$"{name}, твоя программа ""{level}"" начинай тренироваться и зписывай результат"
                                + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                cancellationToken: cancellationToken);
                            await SendReplyKeboard(botClient, message, 3);
                            break;

                        case "Профи":
                            userRepository.UpdateUser("Профи", userId);

                            (level, count) = userRepository.GetUsersId(userId);
                            userRepository.CreateTrainingProgram(level, userId);

                            await RemoveReplyKeboard(botClient, message);
                            await botClient.SendTextMessageAsync(message.Chat,
                                @$"{name}, твоя программа ""{level}"" начинай тренироваться и зписывай результат"
                                + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                cancellationToken: cancellationToken);
                            await SendReplyKeboard(botClient, message, 3);
                            break;

                        case "Турникмэн":
                            userRepository.UpdateUser("Турникмен", userId);

                            (level, count) = userRepository.GetUsersId(userId);
                            userRepository.CreateTrainingProgram(level, userId);
                            await RemoveReplyKeboard(botClient, message);
                            await botClient.SendTextMessageAsync(message.Chat,
                                @$"{name}, твоя программа ""{level}"" начинай тренироваться и зписывай результат"
                                + char.ConvertFromUtf32(0x1F4AA) + char.ConvertFromUtf32(0x1F609),
                                cancellationToken: cancellationToken);
                            await SendReplyKeboard(botClient, message, 3);
                            break;

                        case "Администратор":
                            userRepository.CreateLevelProgram();
                            await botClient.SendTextMessageAsync(message.Chat,
                            "Я всё сделал хозяин",
                            cancellationToken: cancellationToken);
                            break;

                        case "📊График (план/факт)":

                            if (level != null && count == 1)
                            {
                                StringBuilder sb = new StringBuilder();
                                var userReport = userRepository.UserReport(userId, level);
                                var i = 0;
                                string tabs = "";

                                Func<int, string> whiteSpace = x => new string('\t', x);
                   
                                sb.Append($"{whiteSpace(2)}Нед.{whiteSpace(18)}Период{whiteSpace(13)}{whiteSpace(3)}План{whiteSpace(1)}{whiteSpace(3)}Факт{whiteSpace(6)}\n");
    
                                foreach (var item in userReport)
                                {
                                    string emoji="";
                                    if (item.Fact < item.Plan) emoji = "😤";
                                    if (item.Fact == item.Plan) emoji = "💪🏻";
                                    if (item.Fact > item.Plan) emoji = "🦾";
                                    if (item.Fact >= 100) tabs = whiteSpace(2);
                                    if (item.Fact >= 10 && item.Fact <=99) tabs = whiteSpace(4);
                                    if (item.Fact < 10 ) tabs = whiteSpace(6);

                                    i++;
                                    if (i == 1 || i < 10)
                                    {
                                        if (item.Plan > 99 && item.Fact > 99)
                                            sb.Append($"|{whiteSpace(3)}{item.Week}{whiteSpace(4)}|{item.DateBegin}-{item.DateEnd}|{whiteSpace(2)}{item.Plan}{whiteSpace(3)}|{whiteSpace(1)}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan <= 99 && item.Fact <= 99)        
                                            sb.Append($"|{whiteSpace(3)}{item.Week}{whiteSpace(4)}|{item.DateBegin}-{item.DateEnd}|{whiteSpace(4)}{item.Plan}{whiteSpace(3)}|{whiteSpace(2)}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan > 99 && item.Fact <= 99)         
                                            sb.Append($"|{whiteSpace(3)}{item.Week}{whiteSpace(4)}|{item.DateBegin}-{item.DateEnd}|{whiteSpace(2)}{item.Plan}{whiteSpace(3)}|{whiteSpace(2)}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan <= 99 && item.Fact > 99)        
                                            sb.Append($"|{whiteSpace(3)}{item.Week}{whiteSpace(4)}|{item.DateBegin}-{item.DateEnd}|{whiteSpace(4)}{item.Plan}{whiteSpace(3)}|{whiteSpace(2)}{item.Fact}{tabs}{emoji}\n");
                                    }
                                    else
                                    {
                                        if (item.Plan > 99 && item.Fact > 99)
                                            sb.Append($"|{whiteSpace(2)}{item.Week}{whiteSpace(3)}|{item.DateBegin}-{item.DateEnd}|{whiteSpace(2)}{item.Plan}{whiteSpace(3)}|{whiteSpace(2)}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan <= 99 && item.Fact <= 99)
                                            sb.Append($"|{whiteSpace(2)}{item.Week}{whiteSpace(3)}|{item.DateBegin}-{item.DateEnd}|{whiteSpace(4)}{item.Plan}{whiteSpace(3)}|{whiteSpace(2)}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan > 99 && item.Fact <= 99)
                                            sb.Append($"|{whiteSpace(2)}{item.Week}{whiteSpace(3)}|{item.DateBegin}-{item.DateEnd}|{whiteSpace(2)}{item.Plan}{whiteSpace(3)}|{whiteSpace(2)}{item.Fact}{tabs}{emoji}\n");
                                        if (item.Plan <= 99 && item.Fact > 99)
                                            sb.Append($"|{whiteSpace(2)}{item.Week}{whiteSpace(3)}|{item.DateBegin}-{item.DateEnd}|{whiteSpace(4)}{item.Plan}{whiteSpace(3)}|{whiteSpace(2)}{item.Fact}{tabs}{emoji}\n");
                                    }
                                }

                                await botClient.SendTextMessageAsync(message.Chat,
                                sb.ToString(),
                                cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await RemoveReplyKeboard(botClient, message);
                                await botClient.SendTextMessageAsync(message.Chat,
                                "🤖 Не создана программ тренировок!\nВыбирайте:",
                                cancellationToken: cancellationToken);
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
                            new KeyboardButton [] { "✔️Отчёт о выполнении за сегодня"},
                            new KeyboardButton [] { "💪Моя задача на сегодня" },
                            new KeyboardButton [] { "📊График (план/факт)" },
                            new KeyboardButton [] { "❌Удалить программу" },
                       })
                    {

                        ResizeKeyboard = true
                    };
                    break;

                case 4:
                    replyKeyboardMarkup = new(
                       new[]
                       {
                            new KeyboardButton [] { "Администратор" },
                            new KeyboardButton [] { $"✔️Отчёт о выполнении за сегодня" },
                            new KeyboardButton [] { "💪Моя задача на сегодня" },
                            new KeyboardButton [] { "📊График (план/факт)" },
                            new KeyboardButton [] { "❌Удалить программу" },
                       })
                    {

                        ResizeKeyboard = true
                    };
                    break;
            }
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "🤖 готов", replyMarkup: replyKeyboardMarkup);
        }

        static async Task<Message> RemoveReplyKeboard(ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "🤖 настраиваюсь ..."
                         , replyMarkup: new ReplyKeyboardRemove());
        }
    }
}
