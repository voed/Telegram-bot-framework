using BotFramework.Attributes;
using BotFramework.Setup;
using BotFramework.Tests.Primitives;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace BotFramework.Tests.Attributes
{
    public class CommandAttributeTest
    {
        [Fact]
        public void CanFilterUserName()
        {
            var bot = new TelegramBot { UserName = "testbot" };

            var commandInChat = new ParametrizedCommand(InChat.All, "test", CommandParseMode.Both);
            var command = new ParametrizedCommand("test", CommandParseMode.Both);

            var paramses = new HandlerParams(bot, new Update { Message = new Message { Text = "/test@testbot" } },
                                             null);

            Assert.True(commandInChat.CanHandleInternal(paramses));
            Assert.True(command.CanHandleInternal(paramses));

            commandInChat = new ParametrizedCommand(InChat.All, "test", CommandParseMode.WithUsername);
            command = new ParametrizedCommand("test", CommandParseMode.WithUsername);

            Assert.True(commandInChat.CanHandleInternal(paramses));
            Assert.True(command.CanHandleInternal(paramses));

            commandInChat = new ParametrizedCommand(InChat.All, "test", CommandParseMode.WithoutUsername);
            command = new ParametrizedCommand("test", CommandParseMode.WithoutUsername);

            Assert.False(commandInChat.CanHandleInternal(paramses));
            Assert.False(command.CanHandleInternal(paramses));

            paramses = new HandlerParams(bot, new Update { Message = new Message { Text = "/test" } },
                                         null);
            Assert.True(commandInChat.CanHandleInternal(paramses));
            Assert.True(command.CanHandleInternal(paramses));
        }

        [Fact]
        public void CanHandleByText()
        {
            var bot = new TelegramBot { UserName = string.Empty };

            var command = new ParametrizedCommand("test");
            var paramses = new HandlerParams(bot, new Update { Message = new Message { Text = "/test" } }, null);
            Assert.True(command.CanHandleInternal(paramses));
        }

        [Fact]
        public void CanHandleByTextWithUsername()
        {
            var bot = new TelegramBot { UserName = "testbot" };

            var command = new ParametrizedCommand("test");
            var paramses = new HandlerParams(bot, new Update { Message = new Message { Text = "/test@testbot" } },
                                             null);
            Assert.True(command.CanHandleInternal(paramses));
        }

        [Fact]
        public void CanHandleInChannel()
        {
            var bot = new TelegramBot { UserName = "testbot" };

            var command = new ParametrizedCommand(InChat.Channel, "test");
            var paramses =
                new HandlerParams(bot,
                                  new Update { Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Channel } } }, null);
            Assert.True(command.CanHandleInternal(paramses));

            paramses =
                new HandlerParams(bot,
                                  new Update { Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Group } } }, null);
            Assert.False(command.CanHandleInternal(paramses));

            paramses =
                new HandlerParams(bot,
                                  new Update { Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Supergroup } } },
                                  null);
            Assert.False(command.CanHandleInternal(paramses));

            paramses =
                new HandlerParams(bot,
                                  new Update { Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Private } } }, null);
            Assert.False(command.CanHandleInternal(paramses));
        }

        [Fact]
        public void CanHandleInPrivateChat()
        {
            var bot = new TelegramBot { UserName = "testbot" };

            var command = new ParametrizedCommand(InChat.Private, "test");
            var paramses =
                new HandlerParams(bot,
                                  new Update { Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Private } } }, null);
            Assert.True(command.CanHandleInternal(paramses));

            paramses = new HandlerParams(bot,
                                         new Update { Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Group } } },
                                         null);
            Assert.False(command.CanHandleInternal(paramses));

            paramses = new HandlerParams(bot,
                                         new Update
                                             {
                                                 Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Supergroup } }
                                             }, null);
            Assert.False(command.CanHandleInternal(paramses));

            paramses = new HandlerParams(bot,
                                         new Update { Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Channel } } },
                                         null);
            Assert.False(command.CanHandleInternal(paramses));
        }

        [Fact]
        public void CanHandleInPublicChat()
        {
            var bot = new TelegramBot { UserName = "testbot" };

            var command = new ParametrizedCommand(InChat.Public, "test");
            var paramses =
                new HandlerParams(bot,
                                  new Update { Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Private } } }, null);
            Assert.False(command.CanHandleInternal(paramses));

            paramses = new HandlerParams(bot,
                                         new Update { Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Group } } },
                                         null);
            Assert.True(command.CanHandleInternal(paramses));

            paramses = new HandlerParams(bot,
                                         new Update
                                             {
                                                 Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Supergroup } }
                                             }, null);
            Assert.True(command.CanHandleInternal(paramses));

            paramses = new HandlerParams(bot,
                                         new Update { Message = new Message { Text = "/test@testbot", Chat = new Chat { Type = ChatType.Channel } } },
                                         null);

            Assert.False(command.CanHandleInternal(paramses));
        }
    }
}
