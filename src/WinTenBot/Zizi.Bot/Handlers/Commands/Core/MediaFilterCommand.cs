﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Commands.Core
{
    public class MediaFilterCommand : CommandBase
    {
        private readonly MediaFilterService _mediaFilterService;
        private readonly TelegramService _telegramService;

        public MediaFilterCommand(MediaFilterService mediaFilterService, TelegramService telegramService)
        {
            _mediaFilterService = mediaFilterService;
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;

            var sendText = "Saat ini hanya untuk Sudoer saja.";
            if (_telegramService.IsFromSudo)
            {
                sendText = "Reply pesan untuk menyaring..";
                if (msg.ReplyToMessage != null)
                {
                    var repMsg = msg.ReplyToMessage;
                    Log.Information("MessageType: {0}", msg.Type.ToJson(true));

                    var fileId = repMsg.GetFileId();

                    var isExist = await _mediaFilterService.IsExist("file_id", fileId);
                    if (!isExist)
                    {
                        var data = new Dictionary<string, object>()
                        {
                            {"file_id", fileId},
                            {"type_data", repMsg.Type.ToString().ToLower()},
                            {"blocked_by", msg.From.Id},
                            {"blocked_from", msg.Chat.Id}
                        };

                        await _mediaFilterService.SaveAsync(data);
                        sendText = "File ini berhasil di simpan";
                    }
                    else
                    {
                        sendText = "File ini sudah di simpan";
                    }
                }
            }
            else
            {
                sendText =
                    "Fitur ini membutuhkan akses Sudoer, namun file yang Anda laporkan sudah di teruskan ke Team, " +
                    "terima kasih atas laporan nya.";
            }

            await _telegramService.SendTextAsync(sendText);
        }
    }
}