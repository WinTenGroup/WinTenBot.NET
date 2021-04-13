using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;

namespace Zizi.Bot.Handlers.Callbacks
{
    public class SettingsCallback
    {
        private readonly TelegramService _telegramService;
        private readonly SettingsService _settingsService;
        private readonly CallbackQuery _callbackQuery;

        public SettingsCallback(TelegramService telegramService, SettingsService settingsService)
        {
            _telegramService = telegramService;
            _settingsService = settingsService;

            _callbackQuery = telegramService.Context.Update.CallbackQuery;
        }

        public async Task<bool> ExecuteToggleAsync()
        {
            Log.Information("Processing Setting Callback.");

            var chatId = _callbackQuery.Message.Chat.Id;
            var fromId = _callbackQuery.From.Id;
            var msgId = _callbackQuery.Message.MessageId;

            var isAdmin = await _telegramService.IsAdminGroup(fromId);
            if (!isAdmin)
            {
                Log.Information("He is not admin.");
                return false;
            }

            var callbackData = _callbackQuery.Data;
            var partedData = callbackData.Split(" ");
            var callbackParam = partedData.ValueOfIndex(1);
            var partedParam = callbackParam.Split("_");
            var valueParamStr = partedParam.ValueOfIndex(0);
            var keyParamStr = callbackParam.Replace(valueParamStr, "");
            var currentVal = valueParamStr.ToBoolInt();

            Log.Information("Param : {KeyParamStr}", keyParamStr);
            Log.Information("CurrentVal : {CurrentVal}", currentVal);

            var columnTarget = "enable" + keyParamStr;
            var newValue = currentVal == 0 ? 1 : 0;
            Log.Information("Column: {ColumnTarget}, Value: {CurrentVal}, NewValue: {NewValue}",
                columnTarget, currentVal, newValue);

            var data = new Dictionary<string, object>()
            {
                ["chat_id"] = chatId,
                [columnTarget] = newValue
            };

            await _settingsService.SaveSettingsAsync(data);

            var settingBtn = await _settingsService.GetSettingButtonByGroup(chatId);
            var btnMarkup = await settingBtn.ToJson().JsonToButton(chunk: 2);
            Log.Debug("Settings: {Count}", settingBtn.Count);

            _telegramService.SentMessageId = msgId;

            var editText = $"Settings Toggles" +
                           $"\nParam: {columnTarget} to {newValue}";
            await _telegramService.EditMessageCallback(editText, btnMarkup);

            await _settingsService.UpdateCacheAsync(chatId);

            return true;
        }
    }
}