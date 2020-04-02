using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotFramework.Tests.Primitives
{
    public class TelegramBot: ITelegramBot
    {
        private string _instanceName;
        public string UserName { get; set; }
        public ITelegramBotClient BotClient { get; set; }
        public User Me { get; set; }
        public string InstanceName { get => _instanceName; set => _instanceName = value; }
    }
}
