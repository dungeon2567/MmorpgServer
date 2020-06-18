using System;
using System.Collections;
using System.Collections.Generic;

namespace MmorpgServer
{
    public class Cluster : HashSet<Entity>
    {
        public readonly Vector2i Position;

        public delegate void GameObjectAddedHandler(Cluster cluster, Entity entity);
        public delegate void GameObjectRemovedHandler(Cluster cluster, Entity entity);

        public event GameObjectAddedHandler GameObjectAdded;
        public event GameObjectRemovedHandler GameObjectRemoved;

        public void Enter(Entity entity)
        {
            if (base.Add(entity))
            {
                GameObjectAdded?.Invoke(this, entity);
            }
        }

        public void Leave(Entity entity)
        {
            if (base.Remove(entity))
            {
                GameObjectRemoved?.Invoke(this, entity);
            }
        }

        public Cluster(in Vector2i position)
        {
            this.Position = position;
        }
    }
}