using LiteNetLib;
using System;
using LiteNetLib.Utils;

namespace MmorpgServer
{
    public class GameObject
    {

        public Int32 Id;

        public Shape CollisionShape;

        public World World;

        private BoundingBox bounds;

        private bool solid;
        private bool visible;

        public ref readonly BoundingBox BoundingBox
        {
            get
            {
                return ref bounds;
            }
        }

        public bool CanSee(GameObject other)
        {
            var direction = other.CollisionShape.Position - Position;

            direction.Normalize();

            var dir = direction * Vector2.Deg90;

            var p1 = CollisionShape.GetPointOnDirection(dir);
            var p2 = other.CollisionShape.GetPointOnDirection(dir);

            GameObject raycastResult = World.Raycast(p1, p2, gameObject => gameObject != this);

            if (raycastResult == null || raycastResult == other)
            {
                return true;
            }

            dir = direction * Vector2.Deg270;

            p1 = CollisionShape.GetPointOnDirection(dir);
            p2 = other.CollisionShape.GetPointOnDirection(dir);

            raycastResult = World.Raycast(p1, p2, gameObject => gameObject != this);

            if (raycastResult == null || raycastResult == other)
            {
                return true;
            }

            return false;
        }

        public virtual bool HideOnFog
        {
            get { return false; }
        }

        public GameObject()
        {
            Visible = true;
        }

        public bool Solid
        {
            get { return solid; }
            set
            {
                if (value != solid)
                {
                    var from = solid;

                    solid = value;

                    SolidChanged?.Invoke(this, from, value);
                }
            }
        }

        public bool Visible
        {
            get { return visible; }
            set
            {
                if (value != visible)
                {
                    var from = visible;

                    visible = value;

                    VisibleChanged?.Invoke(this, from, value);
                }
            }
        }

        protected void UpdateBoundingBox()
        {
            BoundingBox value = CollisionShape.GetClusters();

            if (value != bounds)
            {
                BoundingBox from = bounds;

                bounds = value;

                BoundingBoxChanged?.Invoke(this, from, value);
            }
        }
        public delegate void PositionChangedHandler(GameObject gameObject, in Vector2 from, in Vector2 to);

        public delegate void BoundingBoxChangedHandler(GameObject gameObject, in BoundingBox from, in BoundingBox to);

        public event PropertyChangedHandler<GameObject, bool> VisibleChanged;
        public event PropertyChangedHandler<GameObject, bool> SolidChanged;

        public event PositionChangedHandler PositionChanged;
        public event BoundingBoxChangedHandler BoundingBoxChanged;

        protected void OnPositionChanged(in Vector2 from, in Vector2 to)
        {
            PositionChanged?.Invoke(this, from, to);

            UpdateBoundingBox();
        }

        public virtual void Initialize()
        {
            bounds = CollisionShape.GetClusters();
        }

        public bool IsDestroyed
        {
            get { return Id == 0; }
        }

        public virtual void Destroy()
        {

        }

        public virtual void SendEnterPacket(NetPeer peer)
        {

        }

        public void SendLeavePacket(NetPeer peer)
        {
            NetDataWriter writer = new NetDataWriter();

            writer.Put((byte)4);
            writer.Put((Int32)Id);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public virtual void AddEvents(PlayerController pc)
        {

        }

        public virtual void RemoveEvents(PlayerController pc)
        {

        }

        public Vector2 Position
        {
            get
            {
                return CollisionShape.Position;
            }
            set
            {
                if (CollisionShape.Position != value)
                {
                    Vector2 from = CollisionShape.Position;

                    CollisionShape.Position = value;

                    OnPositionChanged(from, value);
                }
            }
        }
    }
}