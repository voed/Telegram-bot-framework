using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Types;

namespace BotFramework
{
    public class UpdatePackage: IEquatable<UpdatePackage>
    {
        public UpdatePackage(Update update, ITelegramBot instance)
        {
            Update = update;
            Instance = instance;
        }

        public Update Update { get; }
        public ITelegramBot Instance { get; }

        public override bool Equals(object? obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }

            if(ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((UpdatePackage)obj);
        }

        public bool Equals(UpdatePackage other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return Update.Id == other.Update.Id && Equals(Instance, other.Instance);
        }

        public override int GetHashCode() => HashCode.Combine(Update, Instance);
    }

    public class PipelineDriver: IHostedService
    {
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Thread _workerThread;

        private readonly Timer _timer;

        private readonly List<Queue<UpdatePackage>> fastQueueList = new List<Queue<UpdatePackage>>
            {
                new Queue<UpdatePackage>(), new Queue<UpdatePackage>(), new Queue<UpdatePackage>()
            };

        private readonly Queue<UpdatePackage> slowQueue = new Queue<UpdatePackage>();

        private readonly EventHandlerFactory factory = new EventHandlerFactory();

        private bool isRun;

        public PipelineDriver(IServiceScopeFactory scopeFactory)
        {
            _workerThread = new Thread(WorkerLoop) { Name = "PipelineDriver" };
            _scopeFactory = scopeFactory;
            factory.Find();
            _timer = new Timer(TimerCallback);
            _timer.Change(0, 100);
        }

        private void TimerCallback(object state)
        {
            lock(_resetEvent)
            {
                _resetEvent.Set();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken) { return Task.Run(ServiceStart, cancellationToken); }

        private void ServiceStart()
        {
            isRun = true;
            _workerThread.Start();
        }

        private void ServiceStop()
        {
            isRun = false;
            _workerThread.Join();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.Run(ServiceStop, cancellationToken);

        private void WorkerLoop()
        {
            while(isRun)
            {
                _resetEvent.WaitOne();
                var updates = TakeFromQueues().ToArray();

                if(updates.Length > 1)
                {
                    updates = updates.Distinct().OrderBy(x => x.Update.Id).ToArray();
                }

                if(updates.Length > 0)
                {
                    Task.Run(async () =>
                        {
                            using(var scope = _scopeFactory.CreateScope())
                            {
                                foreach(var updatePackage in updates)
                                {
                                    await factory.ExecuteHandler(
                                        new HandlerParams(updatePackage.Instance, updatePackage.Update, scope.ServiceProvider));
                                }
                            }

                            updates = null;
                        });
                }
            }
        }

        public void Push(UpdatePackage package)
        {
            foreach(var fastQueue in fastQueueList)
            {
                if(Monitor.TryEnter(fastQueue))
                {
                    fastQueue.Enqueue(package);
                    Monitor.Exit(fastQueue);
                    return;
                }
            }

            lock(slowQueue)
            {
                slowQueue.Enqueue(package);
            }
        }

        internal List<UpdatePackage> TakeFromQueues()
        {
            var fastList = new List<UpdatePackage>();
            List<UpdatePackage> slowList = null;

            foreach(var fastQueue in fastQueueList)
            {
                lock(fastQueue)
                {
                    if(fastQueue.Count == 0)
                    {
                        continue;
                    }

                    var fastData = fastQueue.ToArray();
                    fastQueue.Clear();
                    fastList.AddRange(fastData);
                }
            }

            lock(slowQueue)
            {
                if(slowQueue.Count > 0)
                {
                    var slowData = slowQueue.ToArray();
                    slowQueue.Clear();
                    slowList = slowData.ToList();
                }
            }

            return slowList == null ? fastList : fastList.Union(slowList).ToList();
        }
    }
}
