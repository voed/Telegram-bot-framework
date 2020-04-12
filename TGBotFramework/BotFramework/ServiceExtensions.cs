using System;
using BotFramework.ParameterResolvers;
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
            if(!isSettedUp)
            {
                c.Setup();
            }

            BotInstanceManager.RegisterInstance(configSection);
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

        private static void Setup(this IServiceCollection c)
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
             .AddDriver()
             .AddSingleton<BotInstanceManager>().AddScoped<ModuleProvider>()
             .AddTransient<IHostedService, BotInstanceManager>(s => s.GetService<BotInstanceManager>());
            isSettedUp = true;
        }
    }
}
