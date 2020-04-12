using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotFramework.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BotFramework
{
    public class BotInstanceManager: IHostedService
    {
        private static readonly List<string> configNames = new List<string>();
        private readonly PipelineDriver driver;
        private readonly List<Bot> instances = new List<Bot>();
        private readonly IServiceProvider serviceProvider;
        private IServiceScopeFactory scopeFactory;
        private List<WebhookListener> webhookListeners = new List<WebhookListener>();
        private WebHookDriver webHookDriver;
        private LocalWebhookListenerConfig _webhookListenerConfig = new LocalWebhookListenerConfig();

        public BotInstanceManager(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
        {
            this.serviceProvider = serviceProvider;
            this.scopeFactory = scopeFactory;
            driver = new PipelineDriver(scopeFactory);
            var cfg = serviceProvider.GetService<IConfiguration>();
            cfg.GetSection("WebhookListener").Bind(_webhookListenerConfig);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            CreateInstances();
            ConfigureModules();
            foreach(var instance in instances)
            {
                await instance.StartAsync(cancellationToken);
            }

            await driver.StartAsync(cancellationToken);

            if(webhookListeners.Any())
            {
                webHookDriver = new WebHookDriver(webhookListeners, driver, _webhookListenerConfig);
                webHookDriver.Start();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            webHookDriver.Stop();

            foreach(var instance in instances)
            {
                await instance.StopAsync(cancellationToken);
            }

            await driver.StopAsync(cancellationToken);
        }

        internal static void RegisterInstance(string configSectionName) { configNames.Add(configSectionName); }

        internal void CreateInstances()
        {
            foreach(var configName in configNames)
            {
                var instance = new Bot(getConfig(configName), driver, configName);

                if(instance.IsWebhook)
                {
                    var webhookListener = new WebhookListener(instance.WebhookListenURL, instance);
                    webhookListeners.Add(webhookListener);
                }

                instances.Add(instance);
            }
        }

        private BotConfig getConfig(string name)
        {
            var provider = serviceProvider.GetService<IConfiguration>();
            if(provider == null)
            {
                throw new ArgumentException();
            }

            var config = new BotConfig();
            provider.Bind(name, config);
            return config;
        }

        private void ConfigureModules()
        {
            using(var scope = scopeFactory.CreateScope())
            {
                var modules = scope.ServiceProvider.GetServices<BaseBotModule>();
                foreach(var baseBotModule in modules)
                {
                    baseBotModule.__Configure();
                }
            }
            
        }

        public ITelegramBot GetInstanceByName(string name) => instances.FirstOrDefault(x => x.InstanceName == name);
    }
}
