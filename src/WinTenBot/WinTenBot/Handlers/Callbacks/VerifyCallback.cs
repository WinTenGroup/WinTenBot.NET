using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using WinTenBot.Common;
using WinTenBot.Services;
using WinTenBot.Telegram;

namespace WinTenBot.Handlers.Callbacks
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
                    await Telegram.RestrictMemberAsync(fromId, true)
                        .ConfigureAwait(false);

                    var warnJson = await WarnUsernameUtil.GetWarnUsernameCollectionAsync()
                        .ConfigureAwait(false);
                    var isExist = warnJson.AsQueryable().Any(x => x.FromId == fromId);
                    Log.Debug("Is UserId: {0} exist? => {1}", fromId, isExist);
                    if (isExist)
                    {
                        var delete = await warnJson.DeleteManyAsync(x =>
                                x.FromId == fromId && x.ChatId == chatId)
                            .ConfigureAwait(false);
                        Log.Debug("Deleting {0} result {1}", fromId, delete);
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