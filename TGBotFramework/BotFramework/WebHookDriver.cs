using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AleXr64.FastHttpPostReceiver;
using BotFramework.Config;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace BotFramework
{
    internal class WebHookDriver
    {
        private readonly PipelineDriver _driver;
        private readonly List<WebhookListener> _webhookListeners;
        private readonly HttpPostListener _listener;

        public WebHookDriver(List<WebhookListener> webhookListeners, PipelineDriver driver, LocalWebhookListenerConfig config)
        {
            _webhookListeners = webhookListeners;
            _driver = driver;
            _listener = new HttpPostListener(config.Host, config.Port);
            _listener.OnDataReceived += DataHandler;
        }

        private void DataHandler(HttpPostData data)
        {
            if(data.Message == null || data.Message.Length == 0)
            {
                return;
            }

            var path = data.Query.Split(' ')[1];
            var handler = _webhookListeners.FirstOrDefault(x => x.Uri == path);
            if(handler == null)
            {
                return;
            }

            try
            {
                var update = JsonConvert.DeserializeObject<Update>(Encoding.UTF8.GetString(data.Message));
                var pack = new UpdatePackage(update, handler.Instance);
                _driver.Push(pack);
            } catch
            {
                
            }
            

        }

        public void Start() => _listener.Start();
        public void Stop() => _listener.Stop();
    }

    internal class WebhookListener
    {
        public WebhookListener(string uri, Bot instance)
        {
            Uri = uri;
            Instance = instance;
        }

        public string Uri { get; }

        public Bot Instance { get; }
    }
}
