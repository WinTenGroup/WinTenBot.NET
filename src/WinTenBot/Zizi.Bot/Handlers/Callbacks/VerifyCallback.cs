using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Services;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Callbacks
{
    public class VerifyCallback
    {
        private TelegramService Telegram { get; set; }
        private CallbackQuery CallbackQuery { get; set; }

        public VerifyCallback(TelegramService telegramService)
        {
            Telegram = telegramService;
            CallbackQuery = telegramService.Context.Update.CallbackQuery;
            Log.Information("Receiving Verify Callback");

            Parallel.Invoke(async () =>
                await ExecuteVerifyAsync().ConfigureAwait(false));
        }

        private async Task ExecuteVerifyAsync()
        {
            Log.Information("Executing Verify Callback");

            var callbackData = CallbackQuery.Data;
            var message = Telegram.Message;
            var fromId = CallbackQuery.From.Id;
            var chatId = CallbackQuery.Message.Chat.Id;

            Log.Debug($"CallbackData: {callbackData} from {fromId}");

            var partCallbackData = callbackData.Split(" ");
            var callBackParam1 = partCallbackData.ValueOfIndex(1);
            var answer = "Tombol ini bukan untukmu Bep!";

            Log.Debug("Verify Param1: {0}", callBackParam1);

            if (callBackParam1 == "username")
            {
                Log.Debug("Checking username");
                if (CallbackQuery.From.IsNoUsername())
                {
                    answer = "Sepertinya Anda belum memasang Username, silakan di periksa kembali.";
                }
                else
                {
                    var warnJson = await WarnUsernameUtil.GetWarnUsernameCollectionAsync()
                        .ConfigureAwait(false);
                    var listWarns = warnJson.AsQueryable().ToList();

                    foreach (var warn in listWarns)
                    {
                        var userId = warn.FromId;
                        var isExist = listWarns.Any(x => x.FromId == userId);
                        Log.Debug("Is UserId: {0} exist? => {1}", userId, isExist);

                        try
                        {
                            var chatMember = await Telegram.Client.GetChatMemberAsync(chatId, userId);

                            if (chatMember.User.IsNoUsername())
                            {
                                Log.Debug("User {0} does not set an Username", userId);
                            }
                            else
                            {
                                await Telegram.RestrictMemberAsync(fromId, true)
                                    .ConfigureAwait(false);


                                if (isExist)
                                {
                                    var delete = await warnJson.DeleteManyAsync(x =>
                                            x.FromId == userId && x.ChatId == chatId)
                                        .ConfigureAwait(false);
                                    Log.Debug("Deleting {0} result {1}", userId, delete);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error Occured.");

                            if (ex.Message.Contains("user not found"))
                            {
                                Log.Debug("Maybe UserID '{0}' has leave from ChatID '{1}' ", userId, chatId);
                                if (isExist)
                                {
                                    var delete = await warnJson.DeleteManyAsync(x =>
                                            x.FromId == userId && x.ChatId == chatId)
                                        .ConfigureAwait(false);
                                    Log.Debug("Deleting {0} result {1}", userId, delete);
                                }
                            }
                        }
                    }

                    answer = "Terima kasih sudah verifikasi Username!";
                }

                await Telegram.UpdateWarnMessageAsync().ConfigureAwait(false);
            }
            else if (fromId == callBackParam1.ToInt64())
            {
                await Telegram.RestrictMemberAsync(callBackParam1.ToInt(), true)
                    .ConfigureAwait(false);
                answer = "Terima kasih sudah verifikasi!";
            }

            await Telegram.AnswerCallbackQueryAsync(answer)
                .ConfigureAwait(false);
        }
    }
}