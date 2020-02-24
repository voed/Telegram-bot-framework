using System;
using System.Threading;
using System.Threading.Tasks;
using BotFramework.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MihaZupan;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace BotFramework
{
    public interface ITelegramBot
    {
        string UserName { get; }
        ITelegramBotClient BotClient { get; }
        User Me { get; }
    }
    public class Bot: IHostedService, ITelegramBot
    {
        private readonly BotConfig _config = new BotConfig();
        private TelegramBotClient client;
        public User Me { get; private set; }
        public string ConfigName { get; private set; }
        private readonly PipelineDriver _driver;
        internal Bot(IConfiguration configProvider, PipelineDriver driver, string configSection)
        {
            ConfigName = configSection;
            var section = configProvider.GetSection(configSection);
            if(section != null)
            {
                section.Bind(_config);
                _driver = driver;
            }
            else
            {
                throw new ArgumentException($"Section {configSection} not fount");
            }
        }

        public string UserName => Me.Username;
        public ITelegramBotClient BotClient => client;

        public async Task StartAsync(CancellationToken cancellationToken) { await StartListen(); }

        public async Task StopAsync(CancellationToken cancellationToken) { await Task.Run(client.StopReceiving, cancellationToken); }

        private async Task StartListen()
        {
            try
            {
                if(_config.UseSOCKS5)
                {
                    var proxy = new HttpToSocks5Proxy(_config.SOCKS5Address, _config.SOCKS5Port, _config.SOCKS5User,
                                                      _config.SOCKS5Password);
                    client = new TelegramBotClient(_config.Token, proxy);
                }
                else
                {
                    client = new TelegramBotClient(_config.Token);
                }

                client.OnUpdate += Client_OnUpdate;
                client.OnReceiveError += Client_OnReceiveError;
                client.OnReceiveGeneralError += Client_OnReceiveGeneralError;
                Me  = await client.GetMeAsync();

            } catch(ArgumentException e)
            {
                Console.WriteLine(e);
            }

            if(_config.EnableWebHook)
            {
                if(_config.UseSertificate)
                {
                    //TODO серт
                    // await client.SetWebhookAsync(_config.WebHookURL, new InputFileStream(new FileStream(_config.WebHookSertPath)))
                }
                else
                {
                    //TODO подготовить и заспавнить контроллер
                    await client.SetWebhookAsync(_config.WebHookPublicURL);
                }
            }
            else
            {
                client.StartReceiving();
            }
        }

        private void Client_OnUpdate(object sender, UpdateEventArgs e)
        {
            _driver.Push(new UpdatePackage(e.Update, this));
        }

        private void Client_OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
        {
           // throw new NotImplementedException();
        }

        private void Client_OnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
           // throw new NotImplementedException();
        }
    }
}
