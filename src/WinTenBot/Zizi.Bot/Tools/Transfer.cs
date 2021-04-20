﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Tools.GoogleCloud;

namespace Zizi.Bot.Tools
{
    public static class Transfer
    {
        public static void AddQueue()
        {
        }

        public static async Task<string> DownloadFile(this TelegramService telegramService, string address)
        {
            var message = telegramService.Message;
            var messageId = message.MessageId;
            var chatId = message.Chat.Id;
            try
            {
                var totalSize = "";
                var wc = new WebClient();
                var swUpdater = new Stopwatch();
                var swProgress = new Stopwatch();
                var uri = new Uri(address);

                var cachePath = BotSettings.PathCache;
                // var url = new Url(address.GetAutoRedirectedUrl());
                var url = address.GetAutoRedirectedUrl();
                var fileName = Path.GetFileName(url.AbsoluteUri);
                var saveFilePath = Path.Combine(cachePath, chatId.ToString(), fileName);

                swUpdater.Start();
                swProgress.Start();

                wc.DownloadFileAsync(uri, saveFilePath);


                wc.DownloadProgressChanged += async (sender, args) =>
                {
                    if (swUpdater.Elapsed.Seconds < 5) return;

                    swUpdater.Reset();
                    var progressPercentage = args.ProgressPercentage;
                    var downloadSpeed = (args.BytesReceived / swProgress.Elapsed.TotalSeconds).SizeFormat("/s");
                    totalSize = args.TotalBytesToReceive.ToDouble().SizeFormat();
                    var progressText = $"Downloading file to Temp" +
                                       $"\n<b>Url:</b> {url.AbsoluteUri}" +
                                       $"\n<b>Referrer:</b> {address}" +
                                       $"\n<b>Name:</b> {fileName}" +
                                       $"\n<b>Progress:</b> {progressPercentage} %" +
                                       $"\n<b>Size:</b> {totalSize}" +
                                       $"\n<b>Speed:</b> {downloadSpeed}";

                    await telegramService.EditAsync(progressText);

                    swUpdater.Start();
                };

                wc.DownloadFileCompleted += async (sender, args) =>
                {
                    var completeText = $"Download  complete" +
                                       $"\n<b>Url:</b> {address}" +
                                       $"\n<b>Size:</b> {totalSize}" +
                                       $"\n<b>Success:</b> {!args.Cancelled}";

                    await telegramService.EditAsync(completeText);
                    Log.Information("Download file complete {Cancelled}", args.Cancelled);

                    var preparingUpload = $"Preparing upload file to Google Drive." +
                                          $"\nFile: {fileName}";
                    await telegramService.EditAsync(preparingUpload);
                    telegramService.UploadFile(saveFilePath);
                };

                wc.Dispose();
                return saveFilePath;
            }

            catch (Exception e)
            {
                Log.Error(e.Demystify(), "Error when starting download");
                await telegramService.EditAsync($"⛔️ Error when download file. \n{e.Message}");
            }

            return null;
        }
    }
}