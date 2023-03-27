using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Console = Colorful.Console;

namespace ImageOSINT
{
    internal static class Program
    {
        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is Message message)
            {
                try
                {
                    if (message.Photo != null && message.Photo.Length != 0)
                    {
                        var photo = await botClient.GetFileAsync(message.Photo.LastOrDefault().FileId);
                        var url = "https://api.telegram.org/file/bot{token}/" + photo.FilePath;

                        try
                        {
                            var images = GetOtherImages(url)
                                .Take(5);

                            foreach (var image in images)
                            {
                                try
                                {
                                    await botClient.SendPhotoAsync(message.Chat.Id,
                                        new InputOnlineFile(image.SerpItem.Thumb.Url),
                                        $"{image.SerpItem.Snippet.Title}\n{image.SerpItem.Snippet.Url}");
                                }
                                catch
                                {

                                }
                            }
                        }
                        catch { }
                    }
                    if (message.Text == "/start")
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Hello, it's PhotoSINT by Temnij!\nPlease send link on image...");
                    if (message.Text.StartsWith("http"))
                    {
                        try
                        {
                            var images = GetOtherImages(message.Text)
                                .Take(5);

                            foreach (var image in images)
                            {
                                try
                                {
                                    await botClient.SendPhotoAsync(message.Chat.Id,
                                        new InputOnlineFile(image.SerpItem.Thumb.Url),
                                        $"{image.SerpItem.Snippet.Title}\n{image.SerpItem.Snippet.Url}");
                                }
                                catch
                                {

                                }
                            }
                        }
                        catch { }
                    }
                }
                catch
                {

                }
            }
        }

        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                await botClient.SendTextMessageAsync(1775514106, apiRequestException.ToString());
            }
        }

        static ITelegramBotClient bot = new TelegramBotClient("1859001518:AAEQ1Wer2AvfONoVvmT_IhbpO33_dDI-JNQ");
        private static async Task Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Console.WriteLine((await bot.GetMeAsync()).FirstName);
            await bot.ReceiveAsync(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync));
        }

        #region KAПOT

        private static readonly ScrapingBrowser browser = new ScrapingBrowser
        {
            UserAgent = FakeUserAgents.OperaGX,
            KeepAlive = true
        };
        public static List<Models.ImageOne> GetOtherImages(string imageurl)
        {
            var page = browser.NavigateToPage(new Uri("https://yandex.ru/images/search?rpt=imageview&from=tabbar&cbir_page=similar&url=" + imageurl));
            return page.Html
                .SelectNodes("/html/body/div[6]/div[1]/div[1]/div/div[2]/div[1]/div/div")
                .Select(x => Models.ImageOne.FromJson(x.Attributes["data-bem"].Value))
                .ToList();

            /* return nodes
                .Select(x => System.Web.HttpUtility.ParseQueryString(Uri.UnescapeDataString(x.Attributes["href"].Value).Replace("&amp;", "&")).Get("img_url"))
                .Distinct().ToList(); */
        }

        #endregion
    }
}
