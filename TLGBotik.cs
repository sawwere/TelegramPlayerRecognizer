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
        private async Task SendPhoto(long chatId, string a)
        {
            Message message = await botik.SendPhotoAsync(
                chatId: chatId,
                photo: InputFile.FromUri(a));
        }

        private async Task AnswerText(long chatId, string username, string text)
        {
            Console.WriteLine(text);
            string answer = superbot.Talk(chatId, username, text);
            if (answer.Length < 1)
            {
                return;
            }
            Console.WriteLine(answer);
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

                    case "PHOTO1":
                        await SendPhoto(chatId, "https://upload.wikimedia.org/wikipedia/commons/6/65/MPlayer.png");
                        break;
                    case "PHOTO2":
                        await SendPhoto(chatId, "http://ae01.alicdn.com/kf/HTB1K3CUNFXXXXckXFXXq6xXFXXXP/228120074/HTB1K3CUNFXXXXckXFXXq6xXFXXXP.jpg");
                        break;
                    case "PHOTO3":
                        await SendPhoto(chatId, "https://images.philips.com/is/image/PhilipsConsumer/SA3CNT16K_37-RTP-global-001?$jpglarge$&wid=1250");
                        break;
                    case "PHOTO4":
                        await SendPhoto(chatId, "https://nowatermark.ozone.ru/s3/multimedia-o/6040580196.jpg");
                        break;
                    case "PHOTO5":
                        await SendPhoto(chatId, "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Sony_D50_Discman_Open.JPG/1200px-Sony_D50_Discman_Open.JPG");
                        break;
                    case "PHOTO6":
                        await SendPhoto(chatId, "https://cdn.mos.cms.futurecdn.net/1ae99ad7093f8e3003b5dc17f0bab394-1200-80.jpg");
                        break;
                    case "PHOTO7":
                        await SendPhoto(chatId, "https://ae01.alicdn.com/kf/HTB1MdasXrr1gK0jSZFDq6z9yVXaC.jpg");
                        break;
                    case "PHOTO8":
                        await SendPhoto(chatId, "https://static2.nordic.pictures/3329610-thickbox_default/apple-ipod-shuffle-space-gray-6-generation-2gb.jpg");
                        break;
                    case "PHOTO9":
                        await SendPhoto(chatId, "https://maiso.ru/files/user/312707/board/texet-t-795-garantija-god.jpg");
                        break;
                    case "PHOTO10":
                        await SendPhoto(chatId, "https://img.audiomania.ru/images/content/mp3-aac-wav-flac-all-the-audio-file-formats-explained.jpg");
                        break;
                    case "PHOTO11":
                        await SendPhoto(chatId, "https://u.9111s.ru/uploads/202009/30/cf645a085a8b53dfb3cbb711ff1a8e0a.jpg");
                        break;
                    case "PHOTO12":
                        await SendPhoto(chatId, "https://a.d-cd.net/mHVydkAZvg2dH3-468st-wUs1aI-960.jpg");
                        break;
                    case "PHOTO13":
                        await SendPhoto(chatId, "https://wp-s.ru/wallpapers/15/0/314378068711966/devushka-otdyxayushhaya-na-trave-i-slushayushhaya-muzyku.jpg");
                        break;
                    case "PHOTO14":
                        await SendPhoto(chatId, "https://cdn.mos.cms.futurecdn.net/437db76d1a17ed462864495ca03ba635.jpg");
                        break;
                    case "PHOTO15":
                        await SendPhoto(chatId, "https://i.pinimg.com/originals/d3/3a/e2/d33ae2d07a7d1ad4be3b0adc462b9fd9.jpg");
                        break;
                    case "PHOTO16":
                        await SendPhoto(chatId, "https://www.nippon.com/ja/ncommon/contents/japan-topics/139393/139393.jpg");
                        break;
                    case "PHOTO17":
                        await SendPhoto(chatId, "https://www.hi-fi.ru/upload/medialibrary/f17/f17878229c58fc32be5bc7ba4fa1057c.jpg");
                        break;
                    case "PHOTO18":
                        await SendPhoto(chatId, "https://hifi-wiki.com/images/7/7b/Sony_D-50-1984.jpg");
                        break;
                    case "PHOTO19":
                        await SendPhoto(chatId, "https://www.zdnet.com/a/img/2017/05/25/7150efe6-27c6-4302-90cc-aa8c51826cfc/mighty-audio-spotify-streaming-mp3.jpg");
                        break;
                    case "PHOTO20":
                        await SendPhoto(chatId, "https://blog.barnsly.ru/wp-content/uploads/2017/10/CA_DacMagic_XS2_inside_1_1200.jpg");
                        break;
                    case "PHOTO21":
                        await SendPhoto(chatId, "https://img.myipadbox.com/sec/product_l/EDA0017953.jpg");
                        break;
                    case "PHOTO22":
                        await SendPhoto(chatId, "https://www.hifinext.com/wp-content/uploads/2021/10/Sony_CDP-101-Prospekt-1.jpg");
                        break;
                    case "PHOTO23":
                        await SendPhoto(chatId, "https://i.pinimg.com/736x/94/ac/2f/94ac2fe1308445d7a0cba7fc04bcced2--cool-inventions-andreas.jpg");
                        break;
                    case "PHOTO24":
                        await SendPhoto(chatId, "https://remont.zakazdj.ru/wp-content/uploads/2018/10/debjut2_4.jpg");
                        break;
                    case "PHOTO25":
                        await SendPhoto(chatId, "https://www.minidisc.org/images/sony_mzg750_huge.jpg");
                        break;
                    case "PHOTO26":
                        await SendPhoto(chatId, "https://img.audiomania.ru/images/content/mp4-aac-wav-flac-all-the-audio-file-formats-explained.jpg");
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
                //proc.ProcessImage(bm);
                Sample fig = proc.CreateSample(new Bitmap(img));
                switch (perseptron.Predict(fig))
                {
                    case FigureType.Break: await AnswerText(chatId, username, "Загадываю Стоп"); break;
                    case FigureType.pause: await AnswerText(chatId, username, "Загадываю пауза"); break;
                    case FigureType.play: await AnswerText(chatId, username, "Загадываю воспроизвести"); break;
                    case FigureType.next: await AnswerText(chatId, username, "Загадываю Перейти к следующему треку"); break;
                    case FigureType.previous: await AnswerText(chatId, username, "Загадываю Перейти к предыдущему треку"); break;
                    case FigureType.forward: await AnswerText(chatId, username, "Загадываю перемотка вперед"); break;
                    case FigureType.Back: await AnswerText(chatId, username, "Загадываю перемотка назад"); break;
                    default: await botik.SendTextMessageAsync(message.Chat.Id, "Я такого не знаю!"); break;
                }
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
