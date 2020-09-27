using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Providers;
using Zizi.Bot.Common;
using Zizi.Bot.Enums;
using Zizi.Bot.Model;
using Zizi.Bot.Services;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Events
{
    public class NewChatMembersEvent : IUpdateHandler
    {
        private SettingsService _settingsService;
        private TelegramService _telegramService;
        private ChatSetting Settings { get; set; }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var msg = context.Update.Message;
            _telegramService = new TelegramService(context);
            _settingsService = new SettingsService(msg);
            await _telegramService.DeleteAsync(msg.MessageId)
                .ConfigureAwait(false);

            Log.Information("New Chat Members...");

            var chatSettings = _telegramService.CurrentSetting;
            Settings = chatSettings;

            if (!chatSettings.EnableWelcomeMessage)
            {
                Log.Information("Welcome message is disabled!");
                return;
            }

            var newMembers = msg.NewChatMembers;
            var isBootAdded = await newMembers.IsBotAdded().ConfigureAwait(false);
            if (isBootAdded)
            {
                var isRestricted = await _telegramService.EnsureChatRestrictionAsync()
                    .ConfigureAwait(false);
                if (isRestricted) return;

                var botName = BotSettings.GlobalConfiguration["Engines:ProductName"];
                var sendText = $"Hai, perkenalkan saya {botName}" +
                               $"\nFYI saya di bangun ulang menggunakan .NET." +
                               $"\n\nAku adalah bot pendebug dan grup manajemen yang di lengkapi dengan alat keamanan. " +
                               $"Agar saya berfungsi penuh, jadikan saya admin dengan level standard. " +
                               $"\n\nAku akan menerapkan konfigurasi standard jika aku baru pertama kali masuk kesini. " +
                               $"\n\nUntuk melihat daftar perintah bisa ketikkan /help";

                await _telegramService.SendTextAsync(sendText).ConfigureAwait(false);
                await _settingsService.SaveSettingsAsync(new Dictionary<string, object>()
                {
                    {"chat_id", msg.Chat.Id},
                    {"chat_title", msg.Chat.Title}
                }).ConfigureAwait(false);

                if (newMembers.Length == 1) return;
            }

            var parsedNewMember = await ParseMemberCategory(newMembers).ConfigureAwait(false);
            var allNewMember = parsedNewMember.AllNewMember;
            var allNoUsername = parsedNewMember.AllNoUsername;
            var allNewBot = parsedNewMember.AllNewBot;

            if (allNewMember.Length > 0)
            {
                var chatTitle = msg.Chat.Title;
                var greet = Time.GetTimeGreet();
                var memberCount = await _telegramService.GetMemberCount()
                    .ConfigureAwait(false);
                var newMemberCount = newMembers.Length;

                Log.Information("Preparing send Welcome..");

                if (chatSettings.WelcomeMessage.IsNullOrEmpty())
                {
                    chatSettings.WelcomeMessage = $"Hai {allNewMember}" +
                                                  $"\nSelamat datang di kontrakan {chatTitle}" +
                                                  $"\nKamu adalah anggota ke-{memberCount}";
                }

                var sendText = chatSettings.WelcomeMessage.ResolveVariable(new
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
                if (!chatSettings.WelcomeButton.IsNullOrEmpty())
                {
                    keyboard = chatSettings.WelcomeButton.ToReplyMarkup(2);
                }

                if (chatSettings.EnableHumanVerification)
                {
                    Log.Information("Human verification is enabled!");
                    Log.Information("Adding verify button..");

                    var userId = newMembers[0].Id;
                    var verifyButton = $"Saya Manusia!|verify {userId}";

                    var withVerifyArr = new string[] {chatSettings.WelcomeButton, verifyButton};
                    var withVerify = string.Join(",", withVerifyArr);

                    keyboard = withVerify.ToReplyMarkup(2);
                }

                var prevMsgId = chatSettings.LastWelcomeMessageId.ToInt();


                var sentMsgId = -1;

                if (chatSettings.WelcomeMediaType != MediaType.Unknown)
                {
                    var mediaType = (MediaType) chatSettings.WelcomeMediaType;
                    sentMsgId = (await _telegramService.SendMediaAsync(
                        chatSettings.WelcomeMedia,
                        mediaType,
                        sendText,
                        keyboard).ConfigureAwait(false)).MessageId;
                }
                else
                {
                    sentMsgId = (await _telegramService.SendTextAsync(sendText, keyboard)
                        .ConfigureAwait(false)).MessageId;
                }

                if (!chatSettings.EnableHumanVerification)
                {
                    await _telegramService.DeleteAsync(prevMsgId).ConfigureAwait(false);
                }

                await _settingsService.SaveSettingsAsync(new Dictionary<string, object>()
                {
                    {"chat_id", msg.Chat.Id},
                    {"chat_title", msg.Chat.Title},
                    {"chat_type", msg.Chat.Type},
                    {"members_count", memberCount},
                    {"last_welcome_message_id", sentMsgId}
                }).ConfigureAwait(false);

                await _settingsService.UpdateCache().ConfigureAwait(false);
            }
            else
            {
                Log.Information("Welcome Message ignored because User is Global Banned.");
            }
        }

        private async Task<NewMember> ParseMemberCategory(User[] users)
        {
            var lastMember = users.Last();
            var newMembers = new NewMember();
            var allNewMember = new StringBuilder();
            var allNoUsername = new StringBuilder();
            var allNewBot = new StringBuilder();

            Log.Information($"Parsing new {users.Length} members..");
            foreach (var newMember in users)
            {
                var newMemberId = newMember.Id;

                if (Settings.EnableHumanVerification)
                {
                    Log.Information($"Restricting {newMemberId}");
                    await _telegramService.RestrictMemberAsync(newMemberId)
                        .ConfigureAwait(false);
                }

                var isBan = await _telegramService.CheckGlobalBanAsync(newMember)
                    .ConfigureAwait(false);
                if (isBan) continue;

                if (BotSettings.IsProduction)
                {
                    // var isCasBan = await IsCasBan(newMember.Id);
                    await newMember.IsCasBanAsync().ConfigureAwait(false);
                }

                var fullName = (newMember.FirstName + " " + newMember.LastName).Trim();
                var nameLink = Members.GetNameLink(newMemberId, fullName);

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