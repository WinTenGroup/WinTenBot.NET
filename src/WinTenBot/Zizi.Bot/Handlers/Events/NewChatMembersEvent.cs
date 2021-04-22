using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Common;
using Zizi.Bot.Enums;
using Zizi.Bot.Models;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Events
{
    public class NewChatMembersEvent : IUpdateHandler
    {
        private readonly SettingsService _settingsService;
        private readonly TelegramService _telegramService;
        private readonly AntiSpamService _antiSpamService;
        private ChatSetting Settings { get; set; }

        public NewChatMembersEvent(
            AntiSpamService antiSpamService,
            SettingsService settingsService,
            TelegramService telegramService
        )
        {
            _antiSpamService = antiSpamService;
            _settingsService = settingsService;
            _telegramService = telegramService;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;
            var chatId = _telegramService.ChatId;

            // await _telegramService.DeleteAsync(msg.MessageId);

            Log.Information("New Chat Members...");

            Settings = await _settingsService.GetSettingsByGroup(chatId);

            if (!Settings.EnableWelcomeMessage)
            {
                Log.Information("Welcome message is disabled!");
                return;
            }

            var welcomeMessage = Settings.WelcomeMessage;
            var welcomeButton = Settings.WelcomeButton;

            var newMembers = msg.NewChatMembers;

            var isBootAdded = await _telegramService.IsBotAdded(newMembers);
            if (isBootAdded)
            {
                var isRestricted = await _telegramService.EnsureChatRestrictionAsync();
                if (isRestricted) return;

                var getMe = await _telegramService.GetMeAsync();
                var greetMe = $"Hai, perkenalkan saya {getMe.FirstName}" +
                              $"\n\nSaya adalah bot pendebug dan grup manajemen yang di lengkapi dengan alat keamanan. " +
                              $"Agar saya berfungsi penuh, jadikan saya admin dengan level standard. " +
                              $"\n\nUntuk melihat daftar perintah bisa ketikkan /help";

                await _telegramService.SendTextAsync(greetMe, replyToMsgId: 0);
                await _settingsService.SaveSettingsAsync(new Dictionary<string, object>()
                {
                    {"chat_id", msg.Chat.Id},
                    {"chat_title", msg.Chat.Title}
                });

                if (newMembers.Length == 1) return;
            }

            var parsedNewMember = await ParseMemberCategory(newMembers);
            var allNewMember = parsedNewMember.AllNewMember;
            var allNoUsername = parsedNewMember.AllNoUsername;
            var allNewBot = parsedNewMember.AllNewBot;

            if (allNewMember.Length <= 0)
            {
                Log.Information("Welcome Message ignored because User is Global Banned..");
                return;
            }

            var chatTitle = msg.Chat.Title;
            var greet = Time.GetTimeGreet();
            var memberCount = await _telegramService.GetMemberCount();
            var newMemberCount = newMembers.Length;

            Log.Information("Preparing send Welcome..");

            if (welcomeMessage.IsNullOrEmpty())
            {
                welcomeMessage = $"Hai {allNewMember}" +
                                 $"\nSelamat datang di kontrakan {chatTitle}" +
                                 $"\nKamu adalah anggota ke-{memberCount}";
            }

            var sendText = welcomeMessage.ResolveVariable(new
            {
                allNewMember,
                allNoUsername,
                allNewBot,
                chatTitle,
                greet,
                newMemberCount,
                memberCount
            });

            InlineKeyboardMarkup keyboard = null;
            if (!welcomeButton.IsNullOrEmpty())
            {
                keyboard = welcomeButton.ToReplyMarkup(2);
            }

            if (Settings.EnableHumanVerification)
            {
                Log.Information("Human verification is enabled!");
                Log.Information("Adding verify button..");

                var userId = newMembers[0].Id;
                var verifyButton = $"Saya Manusia!|verify {userId}";

                var withVerifyArr = new[] {welcomeButton, verifyButton};
                var withVerify = string.Join(",", withVerifyArr);

                keyboard = withVerify.ToReplyMarkup(2);
            }

            var prevMsgId = Settings.LastWelcomeMessageId.ToInt();

            int sentMsgId;

            Log.Debug("New Member handler before send. Time: {Elapsed}", stopwatch.Elapsed);
            if (Settings.WelcomeMediaType != MediaType.Unknown)
            {
                var mediaType = Settings.WelcomeMediaType;
                sentMsgId = (await _telegramService.SendMediaAsync(
                    Settings.WelcomeMedia,
                    mediaType,
                    sendText,
                    keyboard,
                    0
                    )).MessageId;
            }
            else
            {
                sentMsgId = (await _telegramService.SendTextAsync(sendText, keyboard,0)).MessageId;
            }

            if (!Settings.EnableHumanVerification)
            {
                await _telegramService.DeleteAsync(prevMsgId);
            }

            await _settingsService.SaveSettingsAsync(new Dictionary<string, object>()
            {
                {"chat_id", msg.Chat.Id},
                {"chat_title", msg.Chat.Title},
                {"chat_type", msg.Chat.Type},
                {"members_count", memberCount},
                {"last_welcome_message_id", sentMsgId}
            });

            Log.Debug("New Member handler complete. Time: {Elapsed}", stopwatch.Elapsed);
            stopwatch.Stop();
        }

        private async Task<NewMember> ParseMemberCategory(User[] users)
        {
            var lastMember = users.Last();
            var newMembers = new NewMember();
            var allNewMember = new StringBuilder();
            var allNoUsername = new StringBuilder();
            var allNewBot = new StringBuilder();

            Log.Debug("Parsing new {Length} members..", users.Length);
            foreach (var newMember in users)
            {
                var newMemberId = newMember.Id;

                var isGBanTask = _antiSpamService.CheckEs2Ban(newMemberId);
                var isCasBanTask = _antiSpamService.IsCasBanAsync(newMemberId);
                var isSpamWatchTask = _antiSpamService.CheckSpamWatch(newMemberId);

                if (Settings.EnableHumanVerification)
                {
                    Log.Debug("Restricting {NewMemberId}", newMemberId);
                    await _telegramService.RestrictMemberAsync(newMemberId);
                }

                await Task.WhenAll(isCasBanTask, isGBanTask, isSpamWatchTask);

                var isBan = isGBanTask.Result;
                var isCasBan = isCasBanTask.Result;
                var isSpamWatchBan = isSpamWatchTask.Result;

                if (isBan || isCasBan || isSpamWatchBan)
                {
                    await _telegramService.KickMemberAsync(newMemberId, true);
                    continue;
                }

                var nameLink = newMember.GetNameLink();

                if (newMember != lastMember)
                {
                    allNewMember.Append(nameLink + ", ");
                }
                else
                {
                    allNewMember.Append(nameLink);
                }

                if (newMember.Username.IsNullOrEmpty())
                {
                    allNoUsername.Append(nameLink);
                }

                if (newMember.IsBot)
                {
                    allNewBot.Append(nameLink);
                }
            }

            newMembers.AllNewMember = allNewMember;
            newMembers.AllNoUsername = allNoUsername;
            newMembers.AllNewBot = allNewBot;

            return newMembers;
        }
    }
}