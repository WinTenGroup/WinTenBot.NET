using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Common;
using Zizi.Bot.Enums;
using Zizi.Bot.IO;
using Zizi.Bot.Models;
using Zizi.Bot.Models.Settings;
using Zizi.Bot.Telegram;
using File = System.IO.File;

namespace Zizi.Bot.Services
{
    public class TelegramService
    {
        public bool IsNoUsername { get; set; }
        public bool IsBotAdmin { get; set; }
        public bool IsFromAdmin { get; set; }
        public AppConfig AppConfig { get; set; }
        public ChatSetting CurrentSetting { get; set; }
        public IUpdateContext Context { get; set; }
        private string AppendText { get; set; }
        public ITelegramBotClient Client { get; set; }
        public Message Message { get; set; }
        public Message EditedMessage { get; set; }
        public Message MessageOrEdited { get; set; }
        public CallbackQuery CallbackQuery { get; set; }
        public int SentMessageId { get; internal set; }
        public Message SentMessage { get; set; }
        public int EditedMessageId { get; private set; }
        public int CallBackMessageId { get; set; }
        private string TimeInit { get; set; }
        private string TimeProc { get; set; }

        public TelegramService(IUpdateContext updateContext)
        {
            Context = updateContext;
            Client = updateContext.Bot.Client;
            EditedMessage = updateContext.Update.EditedMessage;

            Message = updateContext.Update.CallbackQuery != null ? updateContext.Update.CallbackQuery.Message : updateContext.Update.Message;

            if (updateContext.Update.CallbackQuery != null)
                CallbackQuery = updateContext.Update.CallbackQuery;

            MessageOrEdited = Message ?? EditedMessage;

            var settingService = new SettingsService(MessageOrEdited);
            CurrentSetting = settingService.ReadCache().Result;

            if (Message != null)
            {
                TimeInit = Message.Date.GetDelay();
            }

            IsNoUsername = MessageOrEdited.From.IsNoUsername();
        }

        public async Task<string> GetMentionAdminsStr()
        {
            var admins = await Client.GetChatAdministratorsAsync(Message.Chat.Id)
                .ConfigureAwait(false);
            var adminStr = string.Empty;
            foreach (var admin in admins)
            {
                var user = admin.User;
                var nameLink = Members.GetNameLink(user.Id, "&#8203;");

                adminStr += $"{nameLink}";
            }

            return adminStr;
        }

        public async Task LeaveChat(long chatId = 0)
        {
            try
            {
                var chatTarget = chatId;
                if (chatId == 0)
                {
                    chatTarget = Message.Chat.Id;
                }

                Log.Information($"Leaving from {chatTarget}");
                await Client.LeaveChatAsync(chatTarget)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error LeaveChat.");
            }
        }

        public async Task<long> GetMemberCount()
        {
            var chatId = Message.Chat.Id;
            var memberCount = await Client.GetChatMembersCountAsync(chatId)
                .ConfigureAwait(false);
            $"Member count on {chatId} is {memberCount}".LogInfo();

            return memberCount;
        }

        public async Task<Chat> GetChat()
        {
            var chat = await BotSettings.Client.GetChatAsync(Message.Chat.Id)
                .ConfigureAwait(false);
            return chat;
        }

        #region Message

        public async Task<Message> SendTextAsync(string sendText, InlineKeyboardMarkup replyMarkup = null,
            int replyToMsgId = -1, long customChatId = -1, bool disableWebPreview = false)
        {
            TimeProc = Message.Date.GetDelay();

            if (sendText.IsNotNullOrEmpty())
            {
                sendText += $"\n\n⏱ <code>{TimeInit} s</code> | ⌛️ <code>{TimeProc} s</code>";
            }

            var chatTarget = Message.Chat.Id;
            if (customChatId < -1)
            {
                chatTarget = customChatId;
            }

            if (replyToMsgId == -1)
            {
                replyToMsgId = Message.MessageId;
            }

            Message send = null;
            try
            {
                Log.Information($"Sending message to {chatTarget}");
                send = await Client.SendTextMessageAsync(
                    chatTarget,
                    sendText,
                    ParseMode.Html,
                    replyMarkup: replyMarkup,
                    replyToMessageId: replyToMsgId,
                    disableWebPagePreview: disableWebPreview
                ).ConfigureAwait(false);
            }
            catch (ApiRequestException apiRequestException)
            {
                Log.Error(apiRequestException, $"SendMessage Ex1");

                try
                {
                    Log.Information($"Try Sending message to {chatTarget} without reply to Msg Id.");
                    send = await Client.SendTextMessageAsync(
                        chatTarget,
                        sendText,
                        ParseMode.Html,
                        replyMarkup: replyMarkup
                    ).ConfigureAwait(false);
                }
                catch (ApiRequestException apiRequestException2)
                {
                    Log.Error(apiRequestException2, $"SendMessage Ex2");
                }
            }

            if (send != null) SentMessageId = send.MessageId;

            return send;
        }

        public async Task<Message> SendMediaAsync(string fileId, MediaType mediaType, string caption = "",
            IReplyMarkup replyMarkup = null, int replyToMsgId = -1)
        {
            Log.Information($"Sending media: {mediaType}, fileId: {fileId} to {Message.Chat.Id}");

            TimeProc = Message.Date.GetDelay();
            if (caption.IsNotNullOrEmpty())
            {
                caption += $"\n\n⏱ <code>{TimeInit} s</code> | ⌛️ <code>{TimeProc} s</code>";
            }

            switch (mediaType)
            {
                case MediaType.Document:
                    SentMessage = await Client.SendDocumentAsync(Message.Chat.Id, fileId, caption, ParseMode.Html,
                            replyMarkup: replyMarkup, replyToMessageId: replyToMsgId)
                        .ConfigureAwait(false);
                    break;

                case MediaType.LocalDocument:
                    var fileName = Path.GetFileName(fileId);
                    await using (var fs = File.OpenRead(fileId))
                    {
                        InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, fileName);
                        SentMessage = await Client.SendDocumentAsync(Message.Chat.Id, inputOnlineFile, caption,
                                ParseMode.Html,
                                replyMarkup: replyMarkup, replyToMessageId: replyToMsgId)
                            .ConfigureAwait(false);
                    }

                    break;

                case MediaType.Photo:
                    SentMessage = await Client.SendPhotoAsync(Message.Chat.Id, fileId, caption, ParseMode.Html,
                            replyMarkup: replyMarkup, replyToMessageId: replyToMsgId)
                        .ConfigureAwait(false);
                    break;

                case MediaType.Video:
                    SentMessage = await Client.SendVideoAsync(Message.Chat.Id, fileId, caption: caption,
                            parseMode: ParseMode.Html,
                            replyMarkup: replyMarkup, replyToMessageId: replyToMsgId)
                        .ConfigureAwait(false);
                    break;

                default:
                    Log.Information($"Media unknown: {mediaType}");
                    return null;
                    break;
            }

            Log.Information($"SendMedia: {SentMessage.MessageId}");

            return SentMessage;
        }

        public async Task EditAsync(string sendText, InlineKeyboardMarkup replyMarkup = null,
            bool disableWebPreview = true)
        {
            TimeProc = Message.Date.GetDelay();

            if (sendText.IsNotNullOrEmpty())
            {
                sendText += $"\n\n⏱ <code>{TimeInit} s</code> | ⌛️ <code>{TimeProc} s</code>";
            }

            var chatId = Message.Chat.Id;
            Log.Information($"Updating message {SentMessageId} on {chatId}");
            try
            {
                var edit = await Client.EditMessageTextAsync(
                    chatId,
                    SentMessageId,
                    sendText,
                    ParseMode.Html,
                    replyMarkup: replyMarkup,
                    disableWebPagePreview: disableWebPreview
                ).ConfigureAwait(false);
                EditedMessageId = edit.MessageId;
            }
            catch (MessageIsNotModifiedException exception)
            {
                Log.Warning(exception, "Message is not modified!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error edit message");
            }
        }

        public async Task EditMessageCallback(string sendText, InlineKeyboardMarkup replyMarkup = null,
            bool disableWebPreview = true)
        {
            try
            {
                Log.Information($"Editing {CallBackMessageId}");
                await Client.EditMessageTextAsync(
                    Message.Chat,
                    CallBackMessageId,
                    sendText,
                    ParseMode.Html,
                    replyMarkup: replyMarkup,
                    disableWebPagePreview: disableWebPreview
                ).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error EditMessage");
            }
        }

        public async Task AppendTextAsync(string sendText, InlineKeyboardMarkup replyMarkup = null)
        {
            if (string.IsNullOrEmpty(AppendText))
            {
                Log.Information("Sending new message");
                AppendText = sendText;
                await SendTextAsync(AppendText, replyMarkup)
                    .ConfigureAwait(false);
            }
            else
            {
                Log.Information("Next, edit existing message");
                AppendText += $"\n{sendText}";
                await EditAsync(AppendText, replyMarkup)
                    .ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync(int messageId = -1, int delay = 0)
        {
            Thread.Sleep(delay);

            try
            {
                var chatId = MessageOrEdited.Chat.Id;
                var msgId = messageId != -1 ? messageId : SentMessageId;

                Log.Information($"Delete MsgId: {msgId} on ChatId: {chatId}");
                await Client.DeleteMessageAsync(chatId, msgId)
                    .ConfigureAwait(false);
            }
            catch (ChatNotFoundException chatNotFoundException)
            {
                Log.Error(chatNotFoundException, $"Error Delete NotFound");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error Delete Message");
            }
        }

        public async Task ForwardMessageAsync(int messageId = -1)
        {
            var fromChatId = Message.Chat.Id;
            var msgId = Message.MessageId;
            if (messageId != -1) msgId = messageId;
            var chatId = BotSettings.BotChannelLogs;
            await Client.ForwardMessageAsync(chatId, fromChatId, msgId)
                .ConfigureAwait(false);
        }

        public async Task AnswerCallbackQueryAsync(string text)
        {
            try
            {
                var callbackQueryId = Context.Update.CallbackQuery.Id;
                await Client.AnswerCallbackQueryAsync(callbackQueryId, text)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error Answer Callback");
            }
        }

        public void ResetTime()
        {
            Log.Information("Resetting time..");

            var msgDate = Message.Date;
            var currentDate = DateTime.UtcNow;
            msgDate = msgDate.AddSeconds(-currentDate.Second);
            msgDate = msgDate.AddMilliseconds(-currentDate.Millisecond);
            TimeInit = msgDate.GetDelay();
        }

        #endregion Message

        public async Task<string> DownloadFileAsync(string fileName)
        {
            var fileId = Message.GetFileId();
            if (fileId.IsNullOrEmpty()) fileId = Message.ReplyToMessage.GetFileId();
            Log.Information($"Downloading file {fileId}");

            var file = await Client.GetFileAsync(fileId)
                .ConfigureAwait(false);

            Log.Information($"DownloadFile: {file.ToJson(true)}");

            fileName = $"Storage/Caches/{fileName}".EnsureDirectory();
            using (var fileStream = File.OpenWrite(fileName))
            {
                await Client.DownloadFileAsync(filePath: file.FilePath, destination: fileStream)
                    .ConfigureAwait(false);
                Log.Information($"File saved to {fileName}");
            }

            return fileName;
        }

        #region Member Exec

        public async Task<bool> KickMemberAsync(User user = null)
        {
            bool isKicked;
            var idTarget = user.Id;

            Log.Information($"Kick {idTarget} from {Message.Chat.Id}");
            try
            {
                await Client.KickChatMemberAsync(Message.Chat.Id, idTarget, DateTime.Now)
                    .ConfigureAwait(false);
                isKicked = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Kick Member");
                isKicked = false;
            }

            return isKicked;
        }

        public async Task UnbanMemberAsync(User user = null)
        {
            var idTarget = user.Id;
            Log.Information($"Unban {idTarget} from {Message.Chat.Id}");
            try
            {
                await Client.UnbanChatMemberAsync(Message.Chat.Id, idTarget)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UnBan Member");
                await SendTextAsync(ex.Message)
                    .ConfigureAwait(false);
            }
        }

        public async Task UnBanMemberAsync(int userId = -1)
        {
            Log.Information($"Unban {userId} from {Message.Chat}");
            try
            {
                await Client.UnbanChatMemberAsync(Message.Chat.Id, userId)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UnBan Member");
                await SendTextAsync(ex.Message)
                    .ConfigureAwait(false);
            }
        }

        public async Task<RequestResult> PromoteChatMemberAsync(int userId)
        {
            var requestResult = new RequestResult();
            try
            {
                await Client.PromoteChatMemberAsync(
                        Message.Chat.Id,
                        userId,
                        canChangeInfo: false,
                        canPostMessages: false,
                        canEditMessages: false,
                        canDeleteMessages: true,
                        canInviteUsers: true,
                        canRestrictMembers: true,
                        canPinMessages: true)
                    .ConfigureAwait(false);

                requestResult.IsSuccess = true;
            }
            catch (ApiRequestException apiRequestException)
            {
                Log.Error(apiRequestException, "Error Promote Member");
                requestResult.IsSuccess = false;
                requestResult.ErrorCode = apiRequestException.ErrorCode;
                requestResult.ErrorMessage = apiRequestException.Message;
            }

            return requestResult;
        }

        public async Task<RequestResult> DemoteChatMemberAsync(int userId)
        {
            var requestResult = new RequestResult();
            try
            {
                await Client.PromoteChatMemberAsync(
                        Message.Chat.Id,
                        userId,
                        canChangeInfo: false,
                        canPostMessages: false,
                        canEditMessages: false,
                        canDeleteMessages: false,
                        canInviteUsers: false,
                        canRestrictMembers: false,
                        canPinMessages: false)
                    .ConfigureAwait(false);

                requestResult.IsSuccess = true;
            }
            catch (ApiRequestException apiRequestException)
            {
                requestResult.IsSuccess = false;
                requestResult.ErrorCode = apiRequestException.ErrorCode;
                requestResult.ErrorMessage = apiRequestException.Message;

                Log.Error(apiRequestException, "Error Demote Member");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Demote Member Ex");
            }

            return requestResult;
        }

        public async Task<TelegramResult> RestrictMemberAsync(int userId, bool unMute = false, DateTime until = default)
        {
            var tgResult = new TelegramResult();

            try
            {
                var chatId = Message.Chat.Id;
                var untilDate = until;
                if (until == default)
                {
                    untilDate = DateTime.UtcNow.AddDays(366);
                }

                Log.Information($"Restricting member on {chatId} until {untilDate}");
                Log.Information($"UserId: {userId}, IsMute: {unMute}");

                var permission = new ChatPermissions
                {
                    CanSendMessages = unMute,
                    CanSendMediaMessages = unMute,
                    CanSendOtherMessages = unMute,
                    CanAddWebPagePreviews = unMute,
                    CanChangeInfo = unMute,
                    CanInviteUsers = unMute,
                    CanPinMessages = unMute,
                    CanSendPolls = unMute
                };

                Log.Debug($"ChatPermissions: {permission.ToJson(true)}");

                if (unMute) untilDate = DateTime.UtcNow;

                await Client.RestrictChatMemberAsync(chatId, userId, permission, untilDate)
                    .ConfigureAwait(false);

                tgResult.IsSuccess = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Demystify(), "Error restrict member");
                var exceptionMsg = ex.Message;
                if (exceptionMsg.Contains("CHAT_ADMIN_REQUIRED"))
                {
                    await SendTextAsync("Sepertinya saya harus menjadi Admin di Grup ini.")
                        .ConfigureAwait(false);
                }

                tgResult.IsSuccess = false;
                tgResult.Exception = ex;
            }

            return tgResult;
        }

        #endregion Member Exec
    }
}