using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotFramework.Tests.Primitives
{
    public class TelegramBot: ITelegramBot
    {
        public string UserName { get; set; }
        public ITelegramBotClient BotClient { get; set; }
        public User Me { get; set; }
    }
}
