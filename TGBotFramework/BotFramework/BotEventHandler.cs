﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BotFramework.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotFramework
{
    public abstract class BotEventHandler
    {
        private HandlerParams _params;
        protected ModuleCollection Modules { get; private set; }
        protected Chat Chat => _params.Chat;
        protected User From => _params.From;
        protected Update RawUpdate => _params.Update;
        protected ITelegramBotClient Bot => _params.Bot;
        protected bool HasChat => _params.HasChat;
        protected bool HasFrom => _params.HasFrom;
        protected bool IsCallbackQuery => CallbackQuery != null;
        protected CallbackQuery CallbackQuery => _params.CallbackQuery;

        internal void __Instantiate(HandlerParams param, IEnumerable<IBotFrameworkModule> modules)
        {
            _params = param;
            Modules = new ModuleCollection(modules);
        }
    }

    internal class EventHandler
    {
        public HandlerAttribute Attribute;
        public MethodInfo Method;
        public Type MethodOwner;
        public ParameterInfo[] Parameters;
        public bool Parametrized;
    }

    internal class EventHandlerFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly List<EventHandler> handlers = new List<EventHandler>();

        public EventHandlerFactory(IServiceScopeFactory scopeFactory) { _scopeFactory = scopeFactory; }

        public void Find()
        {
            var knowHandlers = new List<Type>();
            var asseblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in asseblies)
                try
                {
                    knowHandlers.AddRange(assembly.GetTypes()
                                                  .Where(x => !x.IsAbstract &&
                                                              x.IsSubclassOf(typeof(BotEventHandler))));
                } catch(ReflectionTypeLoadException) { }

            foreach(var handler in knowHandlers)
            {
                var methods = handler.GetMethods().Where(x => x.GetCustomAttributes<HandlerAttribute>().Any());

                foreach(var methodInfo in methods)
                {
                    var eHandler = new EventHandler
                        {
                            Attribute = methodInfo.GetCustomAttribute<HandlerAttribute>(),
                            Method = methodInfo,
                            MethodOwner = handler
                        };
                    eHandler.Parametrized = eHandler.Attribute is ParametrizedCommand;
                    eHandler.Parameters = methodInfo.GetParameters();
                    handlers.Add(eHandler);
                }
            }
        }

        public async Task ExecuteHandler(HandlerParams param)
        {
            bool executed;
            var modules = param.ServiceProvider.GetServices<IBotFrameworkModule>().ToArray();
            foreach(var telegramServiceHandler in modules) await telegramServiceHandler.PreHandler(param);

            var parametrized = handlers.Where(x => x.Parametrized)
                                       .Where(x => x.Attribute.CanHandleInternal(param))
                                       .ToList();
            foreach(var eventHandler in parametrized)
            {
                executed = await Exec(eventHandler, param, modules);
                if(executed) return;
            }

            foreach(var eventHandler in handlers.Where(x => !x.Parametrized)
                                                .Where(x => x.Attribute.CanHandleInternal(param)))
            {
                executed = await Exec(eventHandler, param, modules);
                if(executed) return;
            }
        }

        private async Task<bool> Exec(
                EventHandler handler, HandlerParams param, IEnumerable<IBotFrameworkModule> modules)
        {
            var provider = param.ServiceProvider;
            var method = handler.Method;

            try
            {
                var instance = (BotEventHandler)ActivatorUtilities.CreateInstance(provider, method.DeclaringType);
                instance.__Instantiate(param, modules);
                object[] paramObjects;
                if(handler.Parameters.Length > 0 && handler.Parametrized)
                {
                    var parseOk = param.TryParseParams(handler.Parameters);

                    var paramses = handler.Parameters;
                    if(parseOk)
                        paramObjects =
                                paramses.Select(x =>
                                             {
                                                 return param.CommandParameters
                                                             .First(p => p.Position == x.Position)
                                                             .TypedValue;
                                             })
                                        .ToArray();
                    else
                        return false;
                }
                else
                {
                    paramObjects = null;
                }

                if(method.ReturnParameter?.ParameterType == typeof(Task))
                {
                    var task = method.Invoke(instance, paramObjects);
                    await (Task)task;
                    return true;
                }

                method.Invoke(instance, paramObjects);
                return true;
            } catch(ArgumentException)
            {
                //debug
            }

            return false;
        }
    }
}
