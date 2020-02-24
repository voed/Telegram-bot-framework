using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;
using Xunit;

namespace BotFramework.Tests
{
    public class PipelineDriverTest
    {
        [Fact]
        public void PushPullRuns()
        {
            var packs = new List<UpdatePackage>();

            for(var i = 0; i < 50; i++)
            {
                packs.Add(new UpdatePackage(new Update { Id = i }, null));
            }

            var driver = new PipelineDriver(null);

            packs.AsParallel().ForAll(x => driver.Push(x));

            var result = driver.TakeFromQueues();

            Assert.True(packs.Count == result.Count);

            foreach(var package in result)
            {
                Assert.True(package != null);
                Assert.Contains(packs, x => x.Update.Id == package.Update.Id);
            }
        }

        [Fact]
        public void Takes()
        {
            var driver = new PipelineDriver(null);
            Assert.True(driver.TakeFromQueues().Count == 0);
        }
    }
}
