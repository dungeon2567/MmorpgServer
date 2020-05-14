using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Threading.Tasks;

namespace MmorpgServer
{
    public class Projectile : GameObject, Updatable, Integrable
    {
        Vector2 Velocity;

        public GameObject Owner;

        private TimeSpan CreatedAt;

        public Projectile()
        {
            CollisionShape = new Circle(Vector2.Zero, 0.3);

            MovementSpeed = 15;
        }

        public delegate void MoveHandler(Projectile projectile, in Vector2 from, in Vector2 to);

        public event MoveHandler Moved;

        public override void SendEnterPacket(NetPeer peer)
        {
            NetDataWriter writer = new NetDataWriter();

            writer.Put((byte)6);
            writer.Put((Int32)Id);
            writer.Put((float)CollisionShape.Rotation);
            writer.Put((float)CollisionShape.Position.X);
            writer.Put((float)CollisionShape.Position.Y);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public override bool HideOnFog
        {
            get { return true; }
        }


        public double Radius
        {
            get
            {
                return 0.3;
            }
        }

        public Double MovementSpeed;

        public override void Initialize()
        {
            base.Initialize();

            World.Updateables.Add(this);

            CreatedAt = World.CurrentTime;
        }

        public override void Destroy()
        {
            World.Updateables.Remove(this);

            base.Destroy();
        }


        public void Update(Double delta)
        {
            if ((World.CurrentTime - CreatedAt) >= TimeSpan.FromSeconds(2))
            {
                World.Remove(this);
            }
            else
            {
                this.Velocity = new Vector2(Math.Cos(CollisionShape.Rotation) * MovementSpeed * delta, -Math.Sin(CollisionShape.Rotation) * MovementSpeed * delta);

                World.Integrables.Enqueue(this);
            }
        }

        public void Integrate()
        {
            bool destroy = false;

            var from = CollisionShape.Position;

            CollisionShape.Position = from + Velocity;

            CollisionResult result;

            World.ResolveCollision(this, gameObject => gameObject == Owner, out result);

            if (result.Penetration > 0)
            {
                CollisionShape.Position += (result.Normal * (result.Penetration + 0.01));

                destroy = true;
            }

            if (from != Position)
            {
                OnPositionChanged(from, Position);

                Moved?.Invoke(this, from, Position);
            }

            if (destroy)
            {
                World.Remove(this);
            }
        }

        public override void AddEvents(PlayerController pc)
        {
            Moved += pc.ProjectileMoved;

            base.AddEvents(pc);
        }

        public override void RemoveEvents(PlayerController pc)
        {
            Moved -= pc.ProjectileMoved;

            base.RemoveEvents(pc);
        }
    }
}