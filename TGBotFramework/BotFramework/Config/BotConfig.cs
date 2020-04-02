﻿namespace BotFramework.Config
{
    public class BotConfig
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string WebHookPublicURL { get; set; }
        public string WebHookLocalRelativePath { get; set; }
        public bool UseSertificate { get; set; }
        public string WebHookSertPath { get; set; }
        public bool EnableWebHook { get; set; }
        public string SOCKS5Address { get; set; }
        public int SOCKS5Port { get; set; }
        public string SOCKS5User { get; set; }
        public string SOCKS5Password { get; set; }
        public bool UseSOCKS5 { get; set; }
    }

    public class LocalWebhookListenerConfig
    {
        public string Host { get; set; }
        public ushort Port { get; set; }
    }
}
