using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Threading.Tasks;

namespace MmorpgServer
{
    public class Creature : Entity, Updatable, Integrable
    {
        Vector2 Velocity;

        Int32 Ticks;

        bool canWalk = true;

        bool canAttack = true;

        bool isMoving = false;

        TimeSpan canStopAt;
        TimeSpan? stopAt;

        protected double MovementDirection;

        public Creature()
        {
            CollisionShape = new Circle(Vector2.Zero, 0.5);

            Solid = true;
        }

        public delegate void MoveHandler(Creature creature, in Vector2 from, in Vector2 to);
        public delegate void StartedAttackingHandler(Creature creature);

        public delegate void StoppedAttackingHandler(Creature creature);
        public delegate void StartedMovingHandler(Creature creature);
        public delegate void StoppedMovingHandler(Creature creature);

        public delegate void RotationChangedHandler(Creature creature, in double from, in double to);
        public delegate void GetHitHandler(Creature creature);
        public event PropertyChangedHandlerFast<Creature, Vector2> Moved;
        public event PropertyChangedHandler<Creature, bool> IsMovingChanged;

        public event RotationChangedHandler RotationChanged;

        public event GetHitHandler GetHit;

        public event StartedAttackingHandler StartedAttacking;
        public event StoppedMovingHandler StoppedAttacking;

        public override void SendEnterPacket(NetPeer peer)
        {
            NetDataWriter writer = new NetDataWriter();

            writer.Put((byte)1);
            writer.Put((Int32)Id);
            writer.Put((float)CollisionShape.Rotation);
            writer.Put((float)CollisionShape.Position.X);
            writer.Put((float)CollisionShape.Position.Y);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public override bool HideOnFog
        {
            get
            {
                return true;
            }
        }


        public double Radius
        {
            get
            {
                return 0.5;
            }
        }

        public Double MovementSpeed;

        public override void Initialize()
        {
            base.Initialize();

            World.Updateables.Add(this);
        }

        public override void Destroy()
        {
            World.Updateables.Remove(this);

            base.Destroy();
        }


        public void Update(Double delta)
        {
            if (stopAt != null && World.CurrentTime >= stopAt)
            {
                stopAt = null;

                MovementSpeed = 0;
            }

            if (canWalk)
            {
                this.Velocity = new Vector2(Math.Cos(MovementDirection) * MovementSpeed * delta, Math.Sin(MovementDirection) * MovementSpeed * delta);

                if (this.Velocity.SqrLength > 0)
                {
                    World.Integrables.Enqueue(this);

                    this.Ticks = 0;
                }
                else
                {
                    if (isMoving)
                    {
                        isMoving = false;

                        IsMovingChanged?.Invoke(this, true, false);
                    }
                }
            }
            else
            {
                if (isMoving)
                {
                    isMoving = false;

                    IsMovingChanged?.Invoke(this, true, false);
                }
            }
        }

        public void Integrate()
        {
            ++this.Ticks;

            if (this.Ticks > 10)
                return;

            var from = CollisionShape.Position;

            CollisionShape.Position = from + Velocity;

            CollisionResult result;

            World.ResolveCollision(this, out result);

            if (result.Penetration > 0)
            {
                CollisionShape.Position += (result.Normal * (result.Penetration + 0.01));

                double velocityLength = Velocity.Length;

                double remainingLength = velocityLength - result.Penetration;

                double dot = (Velocity.X * result.Normal.Y + Velocity.Y * result.Normal.X) * remainingLength;

                Velocity.X = dot * result.Normal.Y;
                Velocity.Y = dot * result.Normal.X;

                World.Integrables.Enqueue(this);
            }

            if (from != Position)
            {
                OnPositionChanged(from, Position);

                if (!isMoving)
                {
                    isMoving = true;

                    IsMovingChanged?.Invoke(this, false, true);
                }

                Moved?.Invoke(this, from, Position);
            }
        }

        public async Task Attack()
        {
            if (canAttack)
            {
                canAttack = false;

                try
                {

                    Task canAttackDelay = World.Delay(TimeSpan.FromSeconds(1));

                    StartedAttacking?.Invoke(this);

                    await canAttackDelay;

                    StoppedAttacking?.Invoke(this);
                }
                finally
                {

                    canAttack = true;
                }
            }

        }

        public void SetRotation(double angle)
        {
            if (angle != CollisionShape.Rotation)
            {
                double from = CollisionShape.Rotation;

                CollisionShape.Rotation = angle;

                RotationChanged?.Invoke(this, from, angle);
            }
        }

        public override void AddEvents(PlayerController pc)
        {
            Moved += pc.CreatureMoved;
            StartedAttacking += pc.CreatureStartedAttacking;
            StoppedAttacking += pc.CreatureStoppedAttacking;
            IsMovingChanged += pc.CreatureIsMovingChanged;
            RotationChanged += pc.CreatureRotationChanged;

            base.AddEvents(pc);
        }

        public override void RemoveEvents(PlayerController pc)
        {
            Moved -= pc.CreatureMoved;
            StartedAttacking -= pc.CreatureStartedAttacking;
            StoppedAttacking -= pc.CreatureStoppedAttacking;
            IsMovingChanged += pc.CreatureIsMovingChanged;
            RotationChanged -= pc.CreatureRotationChanged;

            base.RemoveEvents(pc);
        }

        public void Move(double direction)
        {
            MovementSpeed = 6;
            MovementDirection = direction;

            canStopAt = World.CurrentTime + TimeSpan.FromSeconds(0.1);

            stopAt = null;
        }

        public void Stop()
        {
            stopAt = canStopAt;
        }
    }
}