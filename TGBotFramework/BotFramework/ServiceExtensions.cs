using System;
using System.Linq;
using BotFramework.ParameterResolvers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BotFramework
{
    public static class ServiceExtensions
    {
        private static bool isSettedUp;

        public static void AddTelegramBot(this IServiceCollection c, string configSection)
        {
            if(!c.Any(x => x.ServiceType == typeof(ITelegramBot) && ((Bot)x.ImplementationInstance).ConfigName == configSection))
            {
                c.AddTransient<ITelegramBot, Bot>(x => new Bot(x.GetService<IConfiguration>(), x.GetService<PipelineDriver>(), configSection));
                c.AddTransient<IHostedService>(x => x.GetService<PipelineDriver>());
                c.AddTransient<IHostedService>(x => (Bot)x.GetService<ITelegramBot>());
            }
        }

        public static IServiceCollection AddTelegramBotParameterParser<TParam, TParser>(this IServiceCollection collection)
            where TParser: class, IParameterParser<TParam>
        {
            collection.TryAddScoped<IParameterParser<TParam>, TParser>();
            return collection;
        }

        public static IServiceCollection AddTelegramBotRawUpdateParser<TParam, TParser>(this IServiceCollection collection)
            where TParser: class, IRawParameterParser<TParam>
        {
            collection.TryAddScoped<IRawParameterParser<TParam>, TParser>();
            return collection;
        }

        private static IServiceCollection AddDriver(this IServiceCollection c)
        {
            c.AddSingleton<PipelineDriver>();

            return c;
        }

        private static IServiceCollection Setup(this IServiceCollection c)
        {
            if(!isSettedUp)
            {
                c.AddTelegramBotParameterParser<long, LongParameter>()
                 .AddTelegramBotParameterParser<int, IntParameter>()
                 .AddTelegramBotParameterParser<string, StringParametr>()
                 .AddTelegramBotParameterParser<bool, BoolParameter>()
                 .AddTelegramBotParameterParser<float, FloatParameter>()
                 .AddTelegramBotParameterParser<double, DoubleParameter>()
                 .AddTelegramBotParameterParser<decimal, DecimalParameter>()
                 .AddTelegramBotParameterParser<DateTime, DateTimeParameter>()
                 .AddTelegramBotParameterParser<DateTimeOffset, DateTimeOffsetParameter>()
                 .AddTelegramBotParameterParser<TimeSpan, TimeSpanParameter>()
                 .AddDriver();
                isSettedUp = true;
            }

            return c;
        }
    }
}
