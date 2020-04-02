using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BotFramework.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace BotFramework
{
    internal class EventHandlerFactory
    {
        private readonly List<EventHandlerDescriptor> _handlers = new List<EventHandlerDescriptor>();

        public void Find()
        {
            var knowHandlers = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(BotEventHandler)));
                    knowHandlers.AddRange(types);
                } catch(ReflectionTypeLoadException) { }
            }

            foreach(var handler in knowHandlers)
            {
                var assignation = handler.GetCustomAttribute<AssignHandlerToBot>();
                var methods = handler.GetMethods().Where(x => x.GetCustomAttributes<HandlerAttribute>().Any());

                foreach(var methodInfo in methods)
                {
                    var eHandler = new EventHandlerDescriptor
                        {
                            Attribute = methodInfo.GetCustomAttribute<HandlerAttribute>(), Method = methodInfo, MethodOwner = handler
                        };
                    eHandler.TargetInstances = assignation != null ? assignation.ConfigNames.ToArray() : new string[0];
                    eHandler.Parametrized = eHandler.Attribute is ParametrizedCommand;
                    eHandler.Parameters = methodInfo.GetParameters();
                    _handlers.Add(eHandler);
                }
            }
        }

        public async Task ExecuteHandler(HandlerParams param)
        {
            bool executed;

            bool MatchedInstance(EventHandlerDescriptor x)
            {
                return x.TargetInstances != null && x.TargetInstances.Contains(param.InstanceName) ||
                       x.TargetInstances == null ||
                       x.TargetInstances?.Length == 0;
            }

            var handlers = _handlers.Where(MatchedInstance).ToArray();

            var parametrized = handlers.Where(x => x.Parametrized).Where(x => x.Attribute.CanHandleInternal(param))
                                       .ToList();

            foreach(var eventHandler in parametrized)
            {
                executed = await Exec(eventHandler, param);
                if(executed)
                {
                    return;
                }
            }

            foreach(var eventHandler in handlers.Where(x => !x.Parametrized)
                                                .Where(x => x.Attribute.CanHandleInternal(param)))
            {
                executed = await Exec(eventHandler, param);
                if(executed)
                {
                    return;
                }
            }
        }

        private async Task<bool> Exec(EventHandlerDescriptor handler, HandlerParams param)
        {
            var provider = param.ServiceProvider;
            var method = handler.Method;

            try
            {
                var instance = (BotEventHandler)ActivatorUtilities.CreateInstance(provider, method.DeclaringType);
                instance.__Instantiate(param);
                object[] paramObjects;
                if(handler.Parameters.Length > 0 && handler.Parametrized)
                {
                    var parseOk = param.TryParseParams(handler.Parameters);

                    var paramses = handler.Parameters;
                    if(parseOk)
                    {
                        paramObjects = paramses.Select(x => param.CommandParameters.First(p => p.Position == x.Position).TypedValue)
                                               .ToArray();
                    }
                    else
                    {
                        return false;
                    }
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
