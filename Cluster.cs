using System;
using System.Collections;
using System.Collections.Generic;

namespace MmorpgServer
{
    public class Cluster : HashSet<GameObject>
    {
        public readonly Vector2i Position;

        public delegate void GameObjectAddedHandler(Cluster cluster, GameObject gameObject);
        public delegate void GameObjectRemovedHandler(Cluster cluster, GameObject gameObject);

        public event GameObjectAddedHandler GameObjectAdded;
        public event GameObjectRemovedHandler GameObjectRemoved;

        public void Enter(GameObject gameObject)
        {
            if (base.Add(gameObject))
            {
                GameObjectAdded?.Invoke(this, gameObject);
            }
        }

        public void Leave(GameObject gameObject)
        {
            if (base.Remove(gameObject))
            {
                GameObjectRemoved?.Invoke(this, gameObject);
            }
        }

        public Cluster(in Vector2i position)
        {
            this.Position = position;
        }
    }
}