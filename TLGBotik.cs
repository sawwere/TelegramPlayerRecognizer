using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyNeuralNetwork
{
    class TLGBotik
    {
        readonly Dictionary<long, User> users = new Dictionary<long, User>();

        public Telegram.Bot.TelegramBotClient botik = null;
        AIMLBotik superbot;
        private UpdateTLGMessages formUpdater;
        MagicEye proc = new MagicEye();

        private BaseNetwork perseptron = null;
        // CancellationToken - инструмент для отмены задач, запущенных в отдельном потоке
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        public Bitmap processed;
        public TLGBotik(BaseNetwork net, UpdateTLGMessages updater)
        {
            superbot = new AIMLBotik();
            var botKey = System.IO.File.ReadAllText("..//..//token.txt");
            botik = new Telegram.Bot.TelegramBotClient(botKey);
            formUpdater = updater;
            perseptron = net;
        }

        public void SetNet(BaseNetwork net)
        {
            perseptron = net;
            formUpdater("Net updated!");
        }

        private async Task SendSticker(long chatId, string stickerId)
        {
            await botik.SendStickerAsync(chatId, InputFile.FromFileId(stickerId));
        }

        private async Task AnswerText(long chatId, string username, string text)
        {
            string answer = superbot.Talk(chatId, username, text);
            if (answer[0] == ';')
            {
                var splitted = answer.Split(';');
                string stickerCode = splitted[1];
                switch (stickerCode)
                {
                    case "HELLO1":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBuxlh2zRsPAQeTrPRvRBQcu6dMSm9gACbRUAAnFhAAFL63Ud3NjrFFUzBA");
                        break;
                    case "HELLO2":
                        await SendSticker(chatId, "CAACAgQAAxkBAAELBu5lh22Nu4AiwHOPuVYsL0dzL1BRxQACURAAAq8haFElmf1PBkm8BjME");
                        break;
                    case "HELLO3":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBvRlh23sdZpADDybzW0cMzFoht6TTQAC2hkAApkbAAFLpiCWZy6_haczBA");
                        break;
                    case "HELLO4":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBr5lh2DCZfO7ROqqhBcYPO0SdLQjTQAC4BwAAj06EUhEul5mn-zHqTME");
                        break;
                    case "HELLO5":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBsxlh2PM6gHaFlibYs5DhT4FyJZ_VwACPzkAAj8V-UqJSFZhpU_GGzME");
                        break;
                    case "SORRY01":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBthlh2T-436MqeIk-Y9Mim9HxsSJqwACJxwAAqlpeEhWnu_-myVDUjME");
                        break;
                    case "SORRY02":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBtZlh2T3XYyOyyajLukt8ndvsV6kiAACKB0AAvY1mUte9fJtXOvAqTME");
                        break;
                    case "SORRY03":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBtJlh2TjxKhtKNK2JSWH8HGomL1OqAACcSAAAg2j0UuKAvY0GQsy8DME");
                        break;
                    case "SORRY04":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBtRlh2TzHguhfoa9pXgm_sJS_ovU8gACDT8AAg6hWEvnrFtiRVKG4DME");
                        break;
                    case "SORRY05":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBuhlh2wv6Fad25pg16mjHj0WFH_g8gACYRsAAnbL-ErRklcD47HPHzME");
                        break;
                    case "KAK01":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBtxlh2d5IKJYfqLWJyMRAShV6fmFuQACBCQAAvzi2UmZgTU5N77N4TME");
                        break;
                    case "KAK02":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBt5lh2fg4jg0eob2ixxcAqM62It38QACBxwAAglUQUjzyy4hvWGnujME");
                        break;
                    case "BYE01":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBtplh2VK-zbZl5gYlXNtkIrQGc0LNAACVjIAAoblaEqOQ7yK_tGY8zME");
                        break;
                    case "BYE02":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBs5lh2TUqu7h5_TodzwbxS7KZOESIAACkxoAArqVGUjIvJix0dB9CTME");
                        break;
                    case "BYE03":
                        await SendSticker(chatId, "CAACAgIAAxkBAAELBvhlh29JquH5uRb9zZjmYkmmeEyEQQACRhwAAigtSEhKZjkxQIkOCDME");
                        break;
                    case "BYE04":
                        await SendSticker(chatId, "CAACAgQAAxkBAAELBvxlh2_DO5dZXGRCOFNsE0nPs__paAAChQ0AAls0aVEGmGV9TXEyjTME");
                        break;

                }
                await botik.SendTextMessageAsync(chatId, splitted[2]);
            }
            else
            {
                await botik.SendTextMessageAsync(chatId, answer);
            }
        }

        private async Task HandleUpdateMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //  Тут очень простое дело - банально отправляем назад сообщения
            var message = update.Message;
            var chatId = message.Chat.Id;
            var username = message.Chat.FirstName;
            if (!users.ContainsKey(chatId))
            {
                Console.WriteLine(superbot.Talk(chatId, message.Chat.FirstName, $"Меня зовут {message.Chat.FirstName}"));
                await botik.SendTextMessageAsync(message.Chat.Id, superbot.Talk(chatId, message.Chat.FirstName, $"Меня зовут {message.Chat.FirstName}"));
                //await botik.SendTextMessageAsync(message.Chat.Id, superbot.Talk(chatId, message.Chat.FirstName, $"Привет"));
                await AnswerText(chatId, message.Chat.FirstName, $"Привет");
                users.Add(chatId, message.From);
                return;
            }
            formUpdater("Тип сообщения : " + message.Type.ToString());

            //  Получение файла (картинки)
            if (message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
            {
                formUpdater("Picture loadining started");
				var photoId = message.Photo.Last().FileId;
				Telegram.Bot.Types.File fl = botik.GetFileAsync(photoId).Result;
				var imageStream = new MemoryStream();
				await botik.DownloadFileAsync(fl.FilePath, imageStream, cancellationToken: cancellationToken);
				var img = System.Drawing.Image.FromStream(imageStream);

				System.Drawing.Bitmap bm = new System.Drawing.Bitmap(img);
                AForge.Imaging.Filters.ResizeBilinear scaleFilter = new AForge.Imaging.Filters.ResizeBilinear(500, 500);
                var uProcessed = scaleFilter.Apply(AForge.Imaging.UnmanagedImage.FromManagedImage(bm));
                bm = uProcessed.ToManagedImage();
                proc.ProcessImage(bm);
				var img1 = AForge.Imaging.UnmanagedImage.FromManagedImage(bm);
                Sample fig = proc.CreateProcessedSample();
                //Sample sample = GenerateImage.GenerateFigure(uProcessed);
                //Console.WriteLine(bm.Width.ToString() +"=======" +bm.Height.ToString());
                switch (perseptron.Predict(fig))
                {
                    case FigureType.play: await SendSticker(chatId, "CAACAgIAAxkBAAELBm9lhyHrXGIeWu3q-YkAAXD25fki6CEAAiMUAALmOWlLsJSHzUZ3eqYzBA"); break;
                    case FigureType.pause: botik.SendTextMessageAsync(message.Chat.Id, "Это легко, это был pause!"); break;
                    case FigureType.Back: botik.SendTextMessageAsync(message.Chat.Id, "Это легко, это был back!"); break;
                    case FigureType.Break: botik.SendTextMessageAsync(message.Chat.Id, "Это легко, это был break!"); break;
                    case FigureType.forward: botik.SendTextMessageAsync(message.Chat.Id, "Это легко, это был forward!"); break;
                    case FigureType.previous: botik.SendTextMessageAsync(message.Chat.Id, "Это легко, это был prev!"); break;
                    case FigureType.next: botik.SendTextMessageAsync(message.Chat.Id, "Это легко, это был next!"); break;
                    default: botik.SendTextMessageAsync(message.Chat.Id, "Я такого не знаю!"); break;
                }
                await botik.SendTextMessageAsync(message.Chat.Id, "I am super bot");
                formUpdater("Picture recognized!");
                return;
            }
            else if (message.Type == MessageType.Text)
            {
                await AnswerText(chatId, message.Chat.FirstName, message.Text);
            }
            if (message == null || message.Type != MessageType.Text) return;
            
            formUpdater(message.Text);
            return;
        }
        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var apiRequestException = exception as ApiRequestException;
            if (apiRequestException != null)
                Console.WriteLine($"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}");
            else
                Console.WriteLine(exception.ToString());
            return Task.CompletedTask;
        }

        public bool Act()
        {
            try
            {
                botik.StartReceiving(HandleUpdateMessageAsync, HandleErrorAsync, new ReceiverOptions
                {   // Подписываемся только на сообщения
                    AllowedUpdates = new[] { UpdateType.Message }
                },
                cancellationToken: cts.Token);
                // Пробуем получить логин бота - тестируем соединение и токен
                Console.WriteLine($"Connected as {botik.GetMeAsync().Result}");
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public void Stop()
        {
            cts.Cancel();
        }

        public Settings settings = new Settings();

        private double[] ImageToArray2(AForge.Imaging.UnmanagedImage img)
        {
            double[] res = new double[img.Width];

            for (int x = 0; x < img.Width; x++)
            {
                int first = -1;
                int last = -1;
                for (int y = 0; y < img.Height; y++)
                {
                    var value = img.GetPixel(x, y).GetBrightness();
                    if (value < 0.001)
                    {
                        if (first < 0)
                        {
                            first = last = y;
                        }
                        else if (y > last)
                        {
                            last = y;
                        }
                    }
                }
                res[x] = first - last;
            }
            return res;
        }
    }
}
