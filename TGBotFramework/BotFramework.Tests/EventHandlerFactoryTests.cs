using System.Threading.Tasks;
using BotFramework.Attributes;
using BotFramework.Tests.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Xunit;

namespace BotFramework.Tests
{
    public class EventHandlerFactoryTests
    {
        private EventHandlerFactory _factory;
        private ServiceCollection _services;
        private TestProxy _proxy;

        private void Setup()
        {
            _services = new ServiceCollection();
            _proxy = new TestProxy();
            _services.AddSingleton(s => _proxy);

            _factory = new EventHandlerFactory();
        }

        private class TestProxy
        {
            public bool Acessed;
            public bool Executed;
        }

        private class TestHandlerAttribute: HandlerAttribute
        {
            protected override bool CanHandle(HandlerParams param) => true;
        }

        private class TestHandler: BotEventHandler
        {
            private readonly TestProxy _proxy;

            public TestHandler(TestProxy proxy)
            {
                _proxy = proxy;
                _proxy.Acessed = true;
            }

            [TestHandler]
            public void Fact() { _proxy.Executed = true; }
        }

        [Fact]
        public async Task Test()
        {
            Setup();
            _factory.Find();
            var bot = new TelegramBot { UserName = "test" };
            var hParams = new HandlerParams(bot, new Update(), _services.BuildServiceProvider());
            await _factory.ExecuteHandler(hParams);
            Assert.True(_proxy.Acessed);
            Assert.True(_proxy.Executed);
        }
    }
}
