using LiteNetLib;
using System;
using LiteNetLib.Utils;
using System.Xml.Serialization;

namespace MmorpgServer
{
    [Serializable]
    [XmlInclude(typeof(Wall))]
    [XmlInclude(typeof(Pillar))]
    public class Entity
    {
        [XmlIgnore]
        public Int32 Id;

        [XmlIgnore]
        public Shape CollisionShape;

        [XmlIgnore]
        public Scene World;

        private BoundingBox bounds;
        private bool solid;

        [XmlIgnore]
        private bool visible;

        [XmlIgnore]
        public ref readonly BoundingBox BoundingBox
        {
            get
            {
                return ref bounds;
            }
        }

        public bool CanSee(Entity other)
        {
            var direction = other.CollisionShape.Position - Position;

            direction.Normalize();

            var dir = direction * Vector2.Deg90;

            var p1 = CollisionShape.GetPointOnDirection(dir);
            var p2 = other.CollisionShape.GetPointOnDirection(dir);

            Entity raycastResult = World.Raycast(p1, p2, entity => entity != this);

            if (raycastResult == null || raycastResult == other)
            {
                return true;
            }

            dir = direction * Vector2.Deg270;

            p1 = CollisionShape.GetPointOnDirection(dir);
            p2 = other.CollisionShape.GetPointOnDirection(dir);

            raycastResult = World.Raycast(p1, p2, entity => entity != this);

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

        public Entity()
        {
            Visible = true;
        }

        [XmlIgnore]
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

        [XmlIgnore]
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
        public delegate void PositionChangedHandler(Entity entity, in Vector2 from, in Vector2 to);

        public delegate void BoundingBoxChangedHandler(Entity entity, in BoundingBox from, in BoundingBox to);

        public event PropertyChangedHandler<Entity, bool> VisibleChanged;
        public event PropertyChangedHandler<Entity, bool> SolidChanged;

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

        [XmlIgnore]
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

        [XmlElement("Position")]
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