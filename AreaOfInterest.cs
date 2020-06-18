using System;
using System.Collections.Generic;

namespace MmorpgServer
{
    public class AreaOfInterest
    {
        Vector2 position;
        Double radius;

        BoundingBox bounds;

        public ref readonly BoundingBox BoundingBox
        {
            get
            {
                return ref bounds;
            }
        }

        readonly Dictionary<Entity, Int32> ReferenceManager = new Dictionary<Entity, Int32>();

        public IEnumerable<Entity> Objects
        {
            get { return ReferenceManager.Keys; }
        }

        public Scene World
        {
            get;
            set;
        }

        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                Vector2 from = position;

                position = value;

                PositionChanged(from, value);
            }
        }

        public Double Radius
        {
            get
            {
                return radius;
            }
            set
            {
                Double from = radius;

                this.radius = value;

                RadiusChanged(from, value);
            }
        }

        public AreaOfInterest(in Vector2 position, Double radius)
        {
            this.Position = position;
            this.Radius = radius;
        }

        public void Start()
        {
            Int32 x1 = (Int32)Math.Floor((Position.X - Radius) / 3);
            Int32 x2 = (Int32)Math.Ceiling((Position.X + Radius) / 3);

            Int32 y1 = (Int32)Math.Floor((Position.Y - Radius) / 3);
            Int32 y2 = (Int32)Math.Ceiling((Position.Y + Radius) / 3);

            for (Int32 x = x1; x <= x2; ++x)
                for (Int32 y = y1; y <= y2; ++y)
                {
                    Cluster cluster = World.GetCluster(new Vector2i(x, y));

                    cluster.GameObjectAdded += this.ClusterGameObjectAdded;
                    cluster.GameObjectRemoved += this.ClusterGameObjectRemoved;

                    foreach (Entity entity in cluster)
                    {
                        this.ClusterGameObjectAdded(cluster, entity);
                    }
                }
        }

        public void Stop()
        {
            Int32 x1 = (Int32)Math.Floor((Position.X - Radius) / 3);
            Int32 x2 = (Int32)Math.Ceiling((Position.X + Radius) / 3);

            Int32 y1 = (Int32)Math.Floor((Position.Y - Radius) / 3);
            Int32 y2 = (Int32)Math.Ceiling((Position.Y + Radius) / 3);

            for (Int32 x = x1; x <= x2; ++x)
                for (Int32 y = y1; y <= y2; ++y)
                {
                    Cluster cluster = World.GetCluster(new Vector2i(x, y));

                    cluster.GameObjectAdded -= this.ClusterGameObjectAdded;
                    cluster.GameObjectRemoved -= this.ClusterGameObjectRemoved;

                    foreach (Entity entity in cluster)
                    {
                        this.ClusterGameObjectRemoved(cluster, entity);
                    }
                }
        }

        private void ClusterGameObjectAdded(Cluster cluster, Entity entity)
        {
            int referenceCount;

            if (ReferenceManager.TryGetValue(entity, out referenceCount))
            {
                ReferenceManager[entity] = referenceCount + 1;
            }
            else
            {
                ReferenceManager.Add(entity, 1);

                this.GameObjectAdded?.Invoke(this, entity);
            }
        }

        private void ClusterGameObjectRemoved(Cluster cluster, Entity entity)
        {
            int referenceCount;

            if (ReferenceManager.TryGetValue(entity, out referenceCount))
            {
                if (referenceCount == 1)
                {
                    ReferenceManager.Remove(entity);

                    this.GameObjectRemoved?.Invoke(this, entity);
                }
                else
                {
                    ReferenceManager[entity] = referenceCount - 1;
                }
            }
        }

        private void RadiusChanged(double from, double to)
        {

        }

        private void BoundingBoxChanged(in BoundingBox from, in BoundingBox to)
        {
            if(World != null){
            for (Int32 x = to.From.X; x <= to.To.X; ++x)
                for (Int32 y = to.From.Y; y <= to.To.Y; ++y)
                {
                    Vector2i position = new Vector2i(x, y);

                    if (!from.Contains(position))
                    {
                        Cluster cluster = World.GetCluster(position);

                        cluster.GameObjectAdded += this.ClusterGameObjectAdded;
                        cluster.GameObjectRemoved += this.ClusterGameObjectRemoved;

                        foreach (Entity entity in cluster)
                        {
                            this.ClusterGameObjectAdded(cluster, entity);
                        }
                    }
                }

            for (Int32 x = from.From.X; x <= from.To.X; ++x)
                for (Int32 y = from.From.Y; y <= from.To.Y; ++y)
                {
                    Vector2i position = new Vector2i(x, y);

                    if (!to.Contains(position))
                    {
                        Cluster cluster = World.GetCluster(position);

                        cluster.GameObjectAdded -= this.ClusterGameObjectAdded;
                        cluster.GameObjectRemoved -= this.ClusterGameObjectRemoved;

                        foreach (Entity entity in cluster)
                        {
                            this.ClusterGameObjectRemoved(cluster, entity);
                        }
                    }
                }
            }
        }

        private void PositionChanged(in Vector2 from, in Vector2 to)
        {
            BoundingBox toBB = new BoundingBox(new Vector2i(
                (Int32)Math.Floor((to.X - Radius) / 3),
                (Int32)Math.Floor((to.Y - Radius) / 3)
            ), new Vector2i(
                 (Int32)Math.Ceiling((to.X + Radius) / 3),
                 (Int32)Math.Ceiling((to.Y + Radius) / 3)
            ));

            if (bounds != toBB)
            {
                BoundingBox fromBB = bounds;

                bounds = toBB;

                BoundingBoxChanged(fromBB, toBB);
            }
        }

        public delegate void GameObjectAddedHandler(AreaOfInterest areaOfInterest, Entity entity);
        public delegate void GameObjectRemovedHandler(AreaOfInterest areaOfInterest, Entity entity);

        public event GameObjectAddedHandler GameObjectAdded;
        public event GameObjectRemovedHandler GameObjectRemoved;

    }
}