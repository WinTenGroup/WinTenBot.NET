using EasyCaching.Core;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Telegram;
using File = System.IO.File;

namespace Zizi.Bot.Services
{
    public class TelegramService
    {
        private readonly SettingsService _settingsService;
        private readonly IEasyCachingProvider _cachingProvider;
        private readonly CommonConfig _commonConfig;
        private readonly AppConfig _appConfig;

        public bool IsNoUsername { get; private set; }
        public bool IsBotAdmin { get; private set; }
        public bool IsFromSudo { get; private set; }
        public bool IsFromAdmin { get; private set; }
        public bool IsPrivateChat { get; set; }
        public bool IsChatRestricted { get; set; }

        public int FromId { get; set; }
        public long ChatId { get; set; }

        public int SentMessageId { get; internal set; }
        public int EditedMessageId { get; private set; }
        public int CallBackMessageId { get; set; }

        private string AppendText { get; set; }
        private string TimeInit { get; set; }
        private string TimeProc { get; set; }

        public string[] MessageTextParts { get; set; }

        public Message AnyMessage { get; set; }
        public Message Message { get; set; }
        public Message EditedMessage { get; set; }
        public Message MessageOrEdited { get; set; }
        public Message SentMessage { get; set; }
        public Message CallbackMessage { get; set; }
        public CallbackQuery CallbackQuery { get; set; }

        public AppConfig AppConfig { get; set; }
        public ChatSetting CurrentSetting { get; private set; }
        public IUpdateContext Context { get; private set; }
        public ITelegramBotClient Client { get; private set; }

        public TelegramService(
            AppConfig appConfig,
            IEasyCachingProvider cachingProvider,
            CommonConfig commonConfig,
            SettingsService settingsService
        )
        {
            _appConfig = appConfig;
            _cachingProvider = cachingProvider;
            _commonConfig = commonConfig;
            _settingsService = settingsService;
        }

        public async Task AddUpdateContext(IUpdateContext updateContext)
        {
            Log.Debug("Adding UpdateContext..");
            var sw = Stopwatch.StartNew();

            Context = updateContext;
            Client = updateContext.Bot.Client;
            var update = updateContext.Update;

            EditedMessage = updateContext.Update.EditedMessage;

            AnyMessage = update.Message;
            if (update.EditedMessage != null)
                AnyMessage = update.EditedMessage;

            if (update.CallbackQuery?.Message != null)
                AnyMessage = update.CallbackQuery.Message;

            Message = updateContext.Update.CallbackQuery != null ? updateContext.Update.CallbackQuery.Message : updateContext.Update.Message;

            if (updateContext.Update.CallbackQuery != null) CallbackQuery = updateContext.Update.CallbackQuery;

            MessageOrEdited = Message ?? EditedMessage;

            if (Message != null)
            {
                TimeInit = Message.Date.GetDelay();
            }

            if (AnyMessage != null)
            {
                FromId = AnyMessage.From.Id;
                ChatId = AnyMessage.Chat.Id;
                IsNoUsername = AnyMessage.From.IsNoUsername();
                IsChatRestricted = CheckRestriction();
                IsFromSudo = FromId.IsSudoer();

                if (AnyMessage.Text != null) MessageTextParts = AnyMessage.Text.SplitText(" ").ToArray();

                CheckIsPrivateChat();

                var getSettingsTask = _settingsService.GetSettingsByGroup(ChatId);

                await Task.WhenAll(CheckBotAdmin(), CheckFromAdmin(), getSettingsTask);

                // CurrentSetting = await _settingsService.GetSettingsByGroup(ChatId);
                CurrentSetting = getSettingsTask.Result;
            }

            // var settingService = new SettingsService(AnyMessage);
            // CurrentSetting = await settingService.ReadCache();

            Log.Debug("Adding Update context complete in {0}", sw.Elapsed);
            sw.Stop();
        }

        #region Chat

        public bool IsRestricted()
        {
            var isRestricted = _commonConfig.IsRestricted;
            Log.Debug("Global Restriction: {0}", isRestricted);

            return isRestricted;
        }

        public bool CheckRestriction()
        {
            try
            {
                var isRestricted = false;
                var restrictArea = _appConfig.RestrictArea;
                var match = restrictArea.FirstOrDefault(x => x == ChatId.ToString());

                if (match == null)
                {
                    isRestricted = true;
                }

                Log.Information("ChatId: {0} IsRestricted: {1}", ChatId, isRestricted);
                return isRestricted;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Chat Restriction.");
                return false;
            }
        }

        public async Task<string> GetMentionAdminsStr()
        {
            var admins = await Client.GetChatAdministratorsAsync(ChatId);

            var strBuild = new StringBuilder();
            foreach (var admin in admins)
            {
                var user = admin.User;
                var nameLink = user.Id.GetNameLink("&#8203;");

                strBuild.Append(nameLink);
            }

            return strBuild.ToString();
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

                Log.Information("Leaving from {ChatTarget}", chatTarget);
                await Client.LeaveChatAsync(chatTarget);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error LeaveChat.");
            }
        }

        public async Task<long> GetMemberCount()
        {
            var chatId = Message.Chat.Id;
            var cacheKey = $"member-count_{chatId.ReduceChatId()}";
            if (!await _cachingProvider.ExistsAsync(cacheKey))
            {
                var memberCount = await Client.GetChatMembersCountAsync(chatId);

                Log.Debug("Member count on {0} is {1}", chatId, memberCount);

                await _cachingProvider.SetAsync(cacheKey, memberCount, TimeSpan.FromMinutes(10));
                // memberCount.AddCache(cacheKey, 60);
            }

            var cache = _cachingProvider.Get<int>(cacheKey);
            return cache.Value;
        }

        public async Task<Chat> GetChat()
        {
            var chat = await Client.GetChatAsync(ChatId);
            return chat;
        }

        public async Task<ChatMember[]> GetChatAdmin()
        {
            try
            {
                var keyCache = $"list-admin_{ChatId.ReduceChatId()}";

                var cacheExist = await _cachingProvider.ExistsAsync(keyCache);
                if (!cacheExist)
                {
                    Log.Information("Updating list Admin Cache with key: {0}", keyCache);
                    var admins = await Client.GetChatAdministratorsAsync(ChatId);

                    await _cachingProvider.SetAsync(keyCache, admins, TimeSpan.FromMinutes(10));
                }

                var chatMembers = await _cachingProvider.GetAsync<ChatMember[]>(keyCache);
                // Log.Debug("ChatMemberAdmin: {0}", chatMembers.ToJson(true));

                return chatMembers.Value;
            }
            catch
            {
                return null;
            }
        }

        public bool IsGroupChat()
        {
            var chat = AnyMessage.Chat;
            var isGroupChat = chat.Type == ChatType.Group || chat.Type == ChatType.Supergroup;

            Log.Debug("Chat with ID {0} IsGroupChat? {1}", chat.Id, isGroupChat);
            return isGroupChat;
        }

        #endregion Chat

        // public async Task<ChatMember[]> GetChatAdmin()
        // {
        //     // var message = telegramService.Message;
        //     // var client = telegramService.Client;
        //     var chatId = AnyMessage.Chat.Id;
        //
        //
        //     // var cacheExist = telegramService.IsChatCacheExist(CacheKey);
        //     var cacheExist = MonkeyCacheUtil.IsCacheExist("");
        //     if (!cacheExist)
        //     {
        //         await Client.UpdateCacheAdminAsync(chatId);
        //         // var admins = await client.GetChatAdministratorsAsync(chatId)
        //         // ;
        //
        //         // telegramService.SetChatCache(CacheKey, admins);
        //     }
        //
        //     var chatMembers = telegramService.GetChatCache<ChatMember[]>(CacheKey);
        //     // Log.Debug("ChatMemberAdmin: {0}", chatMembers.ToJson(true));
        //
        //     return chatMembers;
        // }

        #region Privilege

        public bool IsFromSudoer()
        {
            bool isSudoer = false;
            var sudoers = _appConfig.Sudoers;
            var match = sudoers.FirstOrDefault(x => x == FromId.ToString());

            if (match != null)
            {
                isSudoer = true;
            }

            Log.Information("UserId: {0} IsSudoer: {1}", FromId, isSudoer);
            return isSudoer;
        }

        public bool IsAdminOrPrivateChat()
        {
            var isAdmin = IsFromAdmin;
            var isPrivateChat = IsPrivateChat;

            return isAdmin || isPrivateChat;
        }

        private async Task CheckBotAdmin()
        {
            Log.Debug("Starting check is Bot Admin.");

            var chat = AnyMessage.Chat;
            var chatId = chat.Id;

            var me = await GetMeAsync();

            if (IsPrivateChat) return;

            var isBotAdmin = await IsAdminChat(me.Id);

            IsBotAdmin = isBotAdmin;
        }

        private async Task CheckFromAdmin()
        {
            Log.Debug("Starting check is From Admin.");

            if (IsPrivateChat) return;

            var isFromAdmin = await IsAdminChat(FromId);

            IsFromAdmin = isFromAdmin;
        }

        private void CheckIsPrivateChat()
        {
            var chat = AnyMessage.Chat;
            var isPrivate = chat.Type == ChatType.Private;

            Log.Debug("Chat {0} IsPrivateChat => {1}", chat.Id, isPrivate);
            IsPrivateChat = isPrivate;
        }

        public async Task<bool> IsAdminChat(int userId = -1)
        {
            var sw = Stopwatch.StartNew();

            if (userId == -1) userId = FromId;

            var chatMembers = await GetChatAdmin();

            var isAdmin = chatMembers.Any(admin => userId == admin.User.Id);

            Log.Debug("Check UserID {0} Admin on Chat {1}? {2}. Time: {3}", userId, ChatId, isAdmin, sw.Elapsed);

            sw.Stop();

            return isAdmin;
        }

        #endregion Privilege

        #region Bot

        public async Task<bool> IsBotAdded(User[] users)
        {
            Log.Information("Checking is added me?");

            var me = await GetMeAsync();

            var isMe = (from user in users where user.Id == me.Id select user.Id == me.Id).FirstOrDefault();
            Log.Information("Is added me? {0}", isMe);

            return isMe;
        }

        public async Task<User> GetMeAsync()
        {
            Log.Debug("Getting Me");

            var GetMeCacheKey = "get-me";

            var isCacheExist = await _cachingProvider.ExistsAsync(GetMeCacheKey);
            if (!isCacheExist)
            {
                Log.Debug("Request GetMe API");
                var getMe = await Client.GetMeAsync();

                Log.Debug("Updating cache");
                await _cachingProvider.SetAsync(GetMeCacheKey, getMe, TimeSpan.FromMinutes(10));
            }

            var fromCache = await _cachingProvider.GetAsync<User>(GetMeCacheKey);
            return fromCache.Value;
        }

        public async Task<bool> IsBeta()
        {
            var me = await GetMeAsync();
            var isBeta = me.Username.ToLower().Contains("beta");
            Log.Information("Is Bot {0} IsBeta: {1}", me, isBeta);
            return isBeta;
        }

        public async Task<string> GetUrlStart(string param)
        {
            var getMe = await Client.GetMeAsync();
            var username = getMe.Username;
            return $"https://t.me/{username}?{param}";
        }

        #endregion Bot

        #region Message

        public bool IsTooOld(int offset = -10)
        {
            var isOld = false;
            if (Message == null) isOld = false;

            if (Message != null)
            {
                var date = Message.Date;

                // Skip older than 10 minutes
                var prev10M = DateTime.UtcNow.AddMinutes(offset);
                Log.Debug("Msg Date: {0}", date.ToString("yyyy-MM-dd hh:mm:ss tt zz"));
                Log.Debug("Prev 10m: {0}", prev10M.ToString("yyyy-MM-dd hh:mm:ss tt zz"));

                isOld = prev10M > date;
            }

            Log.Debug("Is MessageId {0} too old? => {1}", Message?.MessageId, isOld);

            return isOld;
        }

        public async Task<string> DownloadFileAsync(string prefixName)
        {
            var fileId = Message.GetFileId();
            if (fileId.IsNullOrEmpty()) fileId = Message.ReplyToMessage.GetFileId();

            Log.Information("Getting file by FileId {0}", fileId.ToJson(true));
            var file = await Client.GetFileAsync(fileId);

            var filePath = file.FilePath;
            Log.Debug("DownloadFile: {0}", file.ToJson(true));
            var fileName = $"{prefixName}_{filePath}";

            fileName = $"Storage/Caches/{fileName}".EnsureDirectory();

            await using var fileStream = File.OpenWrite(fileName);
            await Client.DownloadFileAsync(filePath: file.FilePath, destination: fileStream);
            Log.Information("File saved to {0}", fileName);

            return fileName;
        }

        public async Task<Message> SendTextAsync(string sendText, IReplyMarkup replyMarkup = null,
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
                Log.Information("Sending message to {ChatTarget}", chatTarget);
                send = await Client.SendTextMessageAsync(
                    chatTarget,
                    sendText,
                    ParseMode.Html,
                    replyMarkup: replyMarkup,
                    replyToMessageId: replyToMsgId,
                    disableWebPagePreview: disableWebPreview
                );
            }
            catch (Exception apiRequestException)
            {
                if (apiRequestException.Message.ToLower().Contains("forbidden"))
                {
                    Log.Warning("Bot Forbidden. {0}", apiRequestException.Message);
                    return null;
                }

                Log.Error(apiRequestException, "SendMessage Ex1");

                try
                {
                    Log.Information("Try Sending message to {ChatTarget} without reply to Msg Id.", chatTarget);
                    send = await Client.SendTextMessageAsync(
                        chatTarget,
                        sendText,
                        ParseMode.Html,
                        replyMarkup: replyMarkup
                    );
                }
                catch (Exception apiRequestException2)
                {
                    Log.Error(apiRequestException2, "SendMessage Ex2");
                }
            }

            if (send != null) SentMessageId = send.MessageId;

            Log.Information("Sent Message Text: {0}", SentMessageId);

            return send;
        }

        public async Task<Message> SendMediaAsync(string fileId, MediaType mediaType, string caption = "",
            IReplyMarkup replyMarkup = null, int replyToMsgId = -1)
        {
            Log.Information("Sending media: {MediaType}, fileId: {FileId} to {Id}", mediaType, fileId, Message.Chat.Id);

            TimeProc = Message.Date.GetDelay();
            if (caption.IsNotNullOrEmpty())
            {
                caption += $"\n\n⏱ <code>{TimeInit} s</code> | ⌛️ <code>{TimeProc} s</code>";
            }

            switch (mediaType)
            {
                case MediaType.Document:
                    SentMessage = await Client.SendDocumentAsync(Message.Chat.Id, fileId, caption, ParseMode.Html,
                        replyMarkup: replyMarkup, replyToMessageId: replyToMsgId);
                    break;

                case MediaType.LocalDocument:
                    var fileName = Path.GetFileName(fileId);
                    await using (var fs = File.OpenRead(fileId))
                    {
                        var inputOnlineFile = new InputOnlineFile(fs, fileName);
                        SentMessage = await Client.SendDocumentAsync(Message.Chat.Id, inputOnlineFile, caption,
                            ParseMode.Html,
                            replyMarkup: replyMarkup, replyToMessageId: replyToMsgId);
                    }

                    break;

                case MediaType.Photo:
                    SentMessage = await Client.SendPhotoAsync(Message.Chat.Id, fileId, caption, ParseMode.Html,
                        replyMarkup: replyMarkup, replyToMessageId: replyToMsgId);
                    break;

                case MediaType.Video:
                    SentMessage = await Client.SendVideoAsync(Message.Chat.Id, fileId, caption: caption,
                        parseMode: ParseMode.Html,
                        replyMarkup: replyMarkup, replyToMessageId: replyToMsgId);
                    break;

                default:
                    Log.Information("Media unknown: {MediaType}", mediaType);
                    return null;
            }

            Log.Information("SendMedia: {MessageId}", SentMessage.MessageId);

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
            Log.Information("Updating message {SentMessageId} on {ChatId}", SentMessageId, chatId);
            try
            {
                var edit = await Client.EditMessageTextAsync(
                    chatId,
                    SentMessageId,
                    sendText,
                    ParseMode.Html,
                    replyMarkup: replyMarkup,
                    disableWebPagePreview: disableWebPreview
                );
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
                Log.Information("Editing {CallBackMessageId}", CallBackMessageId);
                await Client.EditMessageTextAsync(
                    Message.Chat,
                    CallBackMessageId,
                    sendText,
                    ParseMode.Html,
                    replyMarkup: replyMarkup,
                    disableWebPagePreview: disableWebPreview
                );
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
                await SendTextAsync(AppendText, replyMarkup);
            }
            else
            {
                Log.Information("Next, edit existing message");
                AppendText += $"\n{sendText}";
                await EditAsync(AppendText, replyMarkup);
            }
        }

        public async Task DeleteAsync(int messageId = -1, int delay = 0)
        {
            Thread.Sleep(delay);

            try
            {
                var chatId = MessageOrEdited.Chat.Id;
                var msgId = messageId != -1 ? messageId : SentMessageId;

                Log.Information("Delete MsgId: {MsgId} on ChatId: {ChatId}", msgId, chatId);
                await Client.DeleteMessageAsync(chatId, msgId);
            }
            catch (ChatNotFoundException chatNotFoundException)
            {
                Log.Error(chatNotFoundException, "Error Delete NotFound");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Delete Message");
            }
        }

        public async Task ForwardMessageAsync(int messageId = -1)
        {
            var fromChatId = Message.Chat.Id;
            var msgId = Message.MessageId;
            if (messageId != -1) msgId = messageId;
            var chatId = _commonConfig.ChannelLogs;
            await Client.ForwardMessageAsync(chatId, fromChatId, msgId);
        }

        public async Task AnswerCallbackQueryAsync(string text, bool showAlert = false)
        {
            try
            {
                var callbackQueryId = Context.Update.CallbackQuery.Id;
                await Client.AnswerCallbackQueryAsync(callbackQueryId, text, showAlert: showAlert);
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

        #region Member

        public async Task<bool> KickMemberAsync(User user = null)
        {
            bool isKicked;
            var idTarget = user.Id;

            Log.Information("Kick {IdTarget} from {Id}", idTarget, Message.Chat.Id);
            try
            {
                await Client.KickChatMemberAsync(ChatId, idTarget, DateTime.Now);
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
            Log.Information("Unban {IdTarget} from {Id}", idTarget, Message.Chat.Id);
            try
            {
                await Client.UnbanChatMemberAsync(Message.Chat.Id, idTarget);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UnBan Member");
                await SendTextAsync(ex.Message);
            }
        }

        public async Task UnBanMemberAsync(int userId = -1)
        {
            Log.Information("Unban {UserId} from {Chat}", userId, Message.Chat);
            try
            {
                await Client.UnbanChatMemberAsync(Message.Chat.Id, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UnBan Member");
                await SendTextAsync(ex.Message);
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
                    canPinMessages: true);

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
                    canPinMessages: false);

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

                Log.Information("Restricting member on {0} until {1}", chatId, untilDate);
                Log.Information("UserId: {0}, IsMute: {1}", userId, unMute);

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

                Log.Debug("ChatPermissions: {0}", permission.ToJson(true));

                if (unMute) untilDate = DateTime.UtcNow;

                await Client.RestrictChatMemberAsync(chatId, userId, permission, untilDate);

                tgResult.IsSuccess = true;

                Log.Debug("MemberID {0} muted until {1}", userId, untilDate);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Demystify(), "Error restrict member");
                var exceptionMsg = ex.Message;
                if (exceptionMsg.Contains("CHAT_ADMIN_REQUIRED"))
                {
                    Log.Debug("I'm must Admin on this Group!");
                }

                tgResult.IsSuccess = false;
                tgResult.Exception = ex;
            }

            return tgResult;
        }

        #endregion Member
    }
}