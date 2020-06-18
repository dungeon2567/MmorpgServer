using System.Numerics;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using Priority_Queue;
using System.IO;
using System.Xml.Serialization;

namespace MmorpgServer
{
    public class Scene : TaskScheduler
    {
        public static Scene Instance;

        public static Scene Load(string path)
        {
            Scene scene = new Scene();

            FileStream stream = new FileStream(path, FileMode.Open);

            XmlSerializer serializer = new XmlSerializer(typeof(List<Entity>));

            List<Entity> entities = serializer.Deserialize(stream) as List<Entity>;

            foreach(var entity in entities){
                scene.Add(entity);
            }

            stream.Close();

            return scene;
        }

        private Int32 LastId = 0;

        private readonly Dictionary<Vector2i, Cluster> Clusters = new Dictionary<Vector2i, Cluster>();

        public readonly Queue<Integrable> Integrables = new Queue<Integrable>();
        public readonly HashSet<Updatable> Updateables = new HashSet<Updatable>();

        private readonly SimplePriorityQueue<TimerQueueNode, TimeSpan> TimerQueue = new SimplePriorityQueue<TimerQueueNode, TimeSpan>();

        private class TimerQueueNode : TaskCompletionSource<Object>
        {
            public readonly TimeSpan Time;
            public TimerQueueNode(TimeSpan time)
            {
                this.Time = time;
            }
        }

        private readonly Thread BoundThread;
        private readonly Queue<Task> TaskQueue = new Queue<Task>();

        private readonly TaskFactory TaskFactory;
        protected sealed override void QueueTask(Task task)
        {
            lock (TaskQueue)
            {
                TaskQueue.Enqueue(task);
            }
        }

        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected sealed override bool TryDequeue(Task task)
        {
            return false;
        }

        public sealed override int MaximumConcurrencyLevel { get { return 1; } }

        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(TaskQueue, ref lockTaken);
                if (lockTaken) return TaskQueue.ToArray();
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(TaskQueue);
            }
        }

        public TimeSpan CurrentTime;

        public Task Delay(TimeSpan delay)
        {
            TimeSpan time = CurrentTime + delay;

            TimerQueueNode tqn = new TimerQueueNode(time);

            TimerQueue.Enqueue(tqn, time);

            return tqn.Task;
        }

        public Task DelayFrame()
        {
            TimeSpan time = CurrentTime;

            TimerQueueNode tqn = new TimerQueueNode(time);

            TimerQueue.Enqueue(tqn, time);

            return tqn.Task;
        }

        public Scene()
        {
            BoundThread = Thread.CurrentThread;

            TaskFactory = new TaskFactory(
                CancellationToken.None, TaskCreationOptions.DenyChildAttach,
                TaskContinuationOptions.None, this);
        }

        public void Add(Entity entity)
        {
            entity.World = this;
            entity.Id = ++LastId;

            entity.Initialize();

            BoundingBox clusters = entity.BoundingBox;

            entity.BoundingBoxChanged += this.GameObjectBoundingBoxChanged;

            for (Int32 x = clusters.From.X; x <= clusters.To.X; ++x)
                for (Int32 y = clusters.From.Y; y <= clusters.To.Y; ++y)
                {
                    Vector2i position = new Vector2i(x, y);

                    Cluster cluster = GetCluster(position);

                    cluster.Enter(entity);
                }
        }

        public Entity Raycast(in Vector2 from, in Vector2 to, Predicate<Entity> filter)
        {
            int x0 = (int)(from.X <= to.X ? Math.Floor(from.X / 3) : Math.Ceiling(from.X / 3));
            int y0 = (int)(from.Y <= to.Y ? Math.Floor(from.Y / 3) : Math.Ceiling(from.Y / 3));
            int x1 = (int)(from.X >= to.X ? Math.Floor(to.X / 3) : Math.Ceiling(to.X / 3));
            int y1 = (int)(from.Y >= to.Y ? Math.Floor(to.Y / 3) : Math.Ceiling(to.Y / 3));

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int x = x0;
            int y = y0;
            int n = 1 + dx + dy;
            int x_inc = (x1 > x0) ? 1 : -1;
            int y_inc = (y1 > y0) ? 1 : -1;
            int error = dx - dy;
            dx *= 2;
            dy *= 2;

            double min = Double.PositiveInfinity;

            Entity target = null;

            Vector2 direction = (to - from);

            direction.Normalize();

            Ray ray = new Ray(from, direction);

            for (; n > 0; --n)
            {
                Cluster cluster = GetCluster(new Vector2i(x, y));

                foreach (Entity entity in cluster)
                {
                    if (filter(entity))
                    {
                        double rayDistance = entity.CollisionShape.Raycast(ray);

                        if (rayDistance < min)
                        {
                            min = rayDistance;
                            target = entity;
                        }
                    }
                }

                if (!Double.IsInfinity(min))
                {
                    return target;
                }

                if (error > 0)
                {
                    x += x_inc;
                    error -= dy;
                }
                else
                {
                    y += y_inc;
                    error += dx;
                }
            }

            return target;
        }

        private void GameObjectBoundingBoxChanged(Entity entity, in BoundingBox from, in BoundingBox to)
        {
            for (Int32 x = to.From.X; x <= to.To.X; ++x)
                for (Int32 y = to.From.Y; y <= to.To.Y; ++y)
                {
                    Vector2i position = new Vector2i(x, y);

                    if (!from.Contains(position))
                    {
                        Cluster cluster = GetCluster(position);

                        cluster.Enter(entity);
                    }
                }

            for (Int32 x = from.From.X; x <= from.To.X; ++x)
                for (Int32 y = from.From.Y; y <= from.To.Y; ++y)
                {
                    Vector2i position = new Vector2i(x, y);

                    if (!to.Contains(position))
                    {
                        Cluster cluster = GetCluster(position);

                        cluster.Leave(entity);
                    }
                }
        }

        public void Remove(Entity entity)
        {
            TaskFactory.StartNew(() =>
            {
                BoundingBox clusters = entity.BoundingBox;

                for (Int32 x = clusters.From.X; x <= clusters.To.X; ++x)
                    for (Int32 y = clusters.From.Y; y <= clusters.To.Y; ++y)
                    {
                        Vector2i position = new Vector2i(x, y);

                        Cluster cluster = GetCluster(position);

                        cluster.Leave(entity);
                    }

                entity.Destroy();

                entity.World = null;
                entity.Id = 0;
            });
        }

        public void Update(double delta)
        {
            while (TimerQueue.Count > 0 && TimerQueue.First.Time <= CurrentTime)
            {
                TimerQueueNode node = TimerQueue.Dequeue();

                node.SetResult(null);
            }

            while (TaskQueue.Count > 0)
            {
                Task task = TaskQueue.Dequeue();

                TryExecuteTask(task);
            }

            foreach (Updatable updatable in Updateables)
            {
                updatable.Update(delta);
            }

            this.Integrate();
        }

        private void Integrate()
        {
            while (Integrables.Count > 0)
            {
                Integrable integrable = Integrables.Dequeue();

                integrable.Integrate();
            }
        }

        public void ResolveCollision(Entity self, out CollisionResult result)
        {
            result = new CollisionResult(0, Vector2.Zero);

            BoundingBox bbox = self.BoundingBox;

            for (Int32 x = bbox.From.X; x <= bbox.To.X; ++x)
                for (Int32 y = bbox.From.Y; y <= bbox.To.Y; ++y)
                {
                    Vector2i position = new Vector2i(x, y);

                    Cluster cluster = GetCluster(position);

                    foreach (Entity entity in cluster)
                    {
                        if (entity != self)
                        {
                            CollisionResult gameObjectCollisionResult;

                            self.CollisionShape.ResolveCollision(entity.CollisionShape, out gameObjectCollisionResult);

                            if (gameObjectCollisionResult.Penetration > 0)
                            {
                                result = gameObjectCollisionResult;
                            }
                        }
                    }
                }
        }

        public void ResolveCollision(Entity self, Predicate<Entity> predicate, out CollisionResult result)
        {
            result = new CollisionResult(0, Vector2.Zero);

            BoundingBox bbox = self.BoundingBox;

            for (Int32 x = bbox.From.X; x <= bbox.To.X; ++x)
                for (Int32 y = bbox.From.Y; y <= bbox.To.Y; ++y)
                {
                    Vector2i position = new Vector2i(x, y);

                    Cluster cluster = GetCluster(position);

                    foreach (Entity entity in cluster)
                    {
                        if (entity != self && entity.Solid && !predicate(entity))
                        {
                            CollisionResult gameObjectCollisionResult;

                            self.CollisionShape.ResolveCollision(entity.CollisionShape, out gameObjectCollisionResult);

                            if (gameObjectCollisionResult.Penetration > 0)
                            {
                                result = gameObjectCollisionResult;
                            }
                        }
                    }
                }
        }

        public Cluster GetCluster(in Vector2i position)
        {
            Cluster cluster;

            if (!Clusters.TryGetValue(position, out cluster))
            {

                cluster = new Cluster(position);

                Clusters.Add(position, cluster);

            }

            return cluster;
        }
    }
}