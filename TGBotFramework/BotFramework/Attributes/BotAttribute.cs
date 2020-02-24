using System;
using System.Collections.Generic;

namespace BotFramework.Attributes
{
    /// <summary>
    ///     Marks eventhandler class assigned to instance with specified config name
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Class)]
    public sealed class AssignHandlerToBot: Attribute
    {
        public AssignHandlerToBot(string configName, params string[] anotherBots)
        {
            var list = new List<string>();
            list.Add(configName);
            list.AddRange(anotherBots);
            ConfigNames = list;
        }

        public IEnumerable<string> ConfigNames { get; }
    }
}
