using BotFramework.Attributes;
using BotFramework.Setup;
using BotFramework.Tests.Primitives;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace BotFramework.Tests.Attributes
{
    public class TextMessageAttributeTests
    {
        [Fact]
        public void CanHandleAllTextMessages()
        {
            var bot = new TelegramBot { UserName = "testbot" };

            var paramses = new HandlerParams(bot, new Update { Message = new Message { Text = "Blah" } }, null);
            var attribute = new TextMessage();

            Assert.True(attribute.CanHandleInternal(paramses));

            paramses = new HandlerParams(bot, new Update { Message = new Message { Animation = new Animation() } }, null);

            Assert.False(attribute.CanHandleInternal(paramses));

            attribute = new TextMessage(InChat.Public);
            paramses = new HandlerParams(bot, new Update { Message = new Message { Chat = new Chat { Type = ChatType.Channel }, Text = "asd" } },
                                         null);

            Assert.False(attribute.CanHandleInternal(paramses));

            attribute = new TextMessage(InChat.Channel);
            Assert.True(attribute.CanHandleInternal(paramses));

            attribute = new TextMessage(InChat.Private);
            Assert.False(attribute.CanHandleInternal(paramses));

            attribute = new TextMessage(InChat.Public);
            Assert.False(attribute.CanHandleInternal(paramses));
        }

        [Fact]
        public void CanHandleSomeEqualTextInMessage()
        {
            var bot = new TelegramBot { UserName = "testbot" };

            var paramses = new HandlerParams(bot, new Update { Message = new Message { Text = "Blah" } }, null);
            var attribute = new TextMessage("Blah");

            Assert.True(attribute.CanHandleInternal(paramses));

            attribute = new TextMessage("Foo");
            Assert.False(attribute.CanHandleInternal(paramses));

            attribute = new TextMessage("/test");
            Assert.False(attribute.CanHandleInternal(paramses));
        }

        [Fact]
        public void CanHandleSomeEqualTextInMessageByChat()
        {
            var bot = new TelegramBot { UserName = "testbot" };

            var paramses = new HandlerParams(bot, new Update { Message = new Message { Text = "Blah", Chat = new Chat { Type = ChatType.Private } } },
                                             null);
            var attribute = new TextMessage(InChat.All, "Blah");

            Assert.True(attribute.CanHandleInternal(paramses));

            attribute = new TextMessage(InChat.All, "Foo");
            Assert.False(attribute.CanHandleInternal(paramses));

            paramses = new HandlerParams(bot, new Update { Message = new Message { Chat = new Chat { Type = ChatType.Channel }, Text = "Blah" } },
                                         null);
            attribute = new TextMessage(InChat.Channel, "Blah");

            Assert.True(attribute.CanHandleInternal(paramses));

            attribute = new TextMessage(InChat.Public, "Blah");
            Assert.False(attribute.CanHandleInternal(paramses));

            attribute = new TextMessage(InChat.Channel, "asd");
            Assert.False(attribute.CanHandleInternal(paramses));
        }
    }
}
