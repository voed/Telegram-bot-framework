using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BotFramework
{
    public delegate void BotMiddleware(HandlerParams eventParams, Action next);

    public abstract class BaseBotModule
    {
        private readonly List<BotMiddleware> __middleWares = new List<BotMiddleware>();
        private ModuleProvider __provider;
        internal IReadOnlyList<BotMiddleware> __GetMiddleWares() => __middleWares;

        protected void RegisterMiddleware(BotMiddleware middleware) => __middleWares.Add(middleware);

        protected TModule Module<TModule>() where TModule: BaseBotModule => __provider.Module<TModule>();

        protected abstract void ConfigureMiddlewares();
        protected virtual Task OnIncomingUpdate(HandlerParams handlerParams) => Task.CompletedTask;

        internal async Task RunInvomingUpdateEvent(HandlerParams handlerParams)
        {
            var task = OnIncomingUpdate(handlerParams);
            if(task.Status == TaskStatus.Created)
            {
                task.Start();
                await task;
            }
        }
        internal void __Configure() { ConfigureMiddlewares(); }

        internal void __PushModuleProvider(ModuleProvider provider) => __provider = provider;
    }

    internal class ModuleProvider
    {
        private readonly BaseBotModule[] modules;

        public ModuleProvider(IServiceProvider provider)
        {
            modules = provider.GetServices<BaseBotModule>().ToArray();
            foreach(var module in modules)
            {
                module.__PushModuleProvider(this);
            }
        }


        public async Task ExecuteEvents(HandlerParams handlerParams)
        {
            foreach(var module in modules)
            {
                await module.RunInvomingUpdateEvent(handlerParams);
            }
        }

        internal TModule Module<TModule>() where TModule: BaseBotModule
        {
            var module = modules.FirstOrDefault(x => x is TModule);
            return module as TModule;
        }

        internal IReadOnlyList<BotMiddleware> GetMiddlewares()
        {
            var wares = new List<BotMiddleware>();
            foreach(var botModule in modules)
            {
                botModule.__Configure();
                wares.AddRange(botModule.__GetMiddleWares());
            }

            return wares.AsReadOnly();
        }
    }
}
