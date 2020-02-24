using System;
using System.Reflection;
using BotFramework.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotFramework
{
    public abstract class BotEventHandler
    {
        private HandlerParams _params;

        protected Chat Chat => _params.Chat;
        protected User From => _params.From;
        protected Update RawUpdate => _params.Update;
        protected ITelegramBotClient Bot => _params.Bot;
        protected bool HasChat => _params.HasChat;
        protected bool HasFrom => _params.HasFrom;
        protected bool IsCallbackQuery => CallbackQuery != null;
        protected CallbackQuery CallbackQuery => _params.CallbackQuery;

        internal void __Instantiate(HandlerParams param) { _params = param; }
    }

    internal struct EventHandlerDescriptor
    {
        public HandlerAttribute Attribute;
        public MethodInfo Method;
        public Type MethodOwner;
        public ParameterInfo[] Parameters;
        public bool Parametrized;
    }
}
