using System.Linq;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLib;
using System;

namespace MmorpgServer
{
    public class PlayerController: Updatable
    {
        static readonly Dictionary<NetPeer, PlayerController> Controllers = new Dictionary<NetPeer, PlayerController>();

        public static PlayerController GetFromPeer(NetPeer peer)
        {
            return Controllers[peer];
        }
        public readonly Creature Player;
        public readonly AreaOfInterest AreaOfInterest;

        public readonly NetPeer Peer;

        bool AttackPressed = false;

        public PlayerController(Creature player, NetPeer peer)
        {
            Peer = peer;
            Player = player;

            AreaOfInterest = new AreaOfInterest(player.CollisionShape.Position, 10)
            {
                World = Player.World
            };
        }

        public void Start()
        {
            Controllers.Add(Peer, this);

            AreaOfInterest.Start();

            foreach (Entity entity in AreaOfInterest.Objects)
            {
                AreaOfInterestGameObjectAdded(AreaOfInterest, entity);
            }

            AreaOfInterest.GameObjectAdded += AreaOfInterestGameObjectAdded;
            AreaOfInterest.GameObjectRemoved += AreaOfInterestGameObjectRemoved;

            Player.PositionChanged += PlayerPositionChanged;

            this.Player.World.Updateables.Add(this);
        }

        public void Update(double deltaTime){
            if(this.AttackPressed){
                this.Player.Attack();
            }
        }

        public void PlayerPositionChanged(Entity player, in Vector2 from, in Vector2 to)
        {
            this.AreaOfInterest.Position = to;
        }

        public void Stop()
        {
            Console.WriteLine(Player.Id + " Disconnected ");

            Scene.Instance.Remove(Player);

            Controllers.Remove(Peer);

            AreaOfInterest.GameObjectAdded -= AreaOfInterestGameObjectAdded;
            AreaOfInterest.GameObjectRemoved -= AreaOfInterestGameObjectRemoved;

            foreach (Entity entity in AreaOfInterest.Objects)
            {
                entity.RemoveEvents(this);
            }

            AreaOfInterest.Stop();

            Player.PositionChanged -= PlayerPositionChanged;
        }

        public void AreaOfInterestGameObjectAdded(AreaOfInterest areaOfInterest, Entity entity)
        {
            if (entity == Player)
            {
                NetDataWriter writer = new NetDataWriter();

                writer.Put((byte)0);
                writer.Put((Int32)Player.Id);
                writer.Put((float)Player.CollisionShape.Rotation);
                writer.Put((float)Player.CollisionShape.Position.X);
                writer.Put((float)Player.CollisionShape.Position.Y);

                Peer.Send(writer, DeliveryMethod.ReliableOrdered);

                entity.AddEvents(this);
            }
            else
            {
                entity.SendEnterPacket(Peer);

                entity.AddEvents(this);
            }
        }

        public void AreaOfInterestGameObjectRemoved(AreaOfInterest areaOfInterest, Entity entity)
        {
            entity.RemoveEvents(this);

            entity.SendLeavePacket(Peer);
        }


        public void CreatureMoved(Creature creature, in Vector2 from, in Vector2 to)
        {
            NetDataWriter packetWriter = new NetDataWriter();

            packetWriter.Put((byte)2);
            packetWriter.Put((Int32)creature.Id);
            //packetWriter.Put((float)from.X);
            //packetWriter.Put((float)from.Y);

            packetWriter.Put((float)to.X);
            packetWriter.Put((float)to.Y);
            
            Peer.Send(packetWriter, DeliveryMethod.Sequenced);
        }

        public void CreatureRotationChanged(Creature creature, in double from, in double to)
        {
            NetDataWriter packetWriter = new NetDataWriter();

            packetWriter.Put((byte)12);
            packetWriter.Put((Int32)creature.Id);
            packetWriter.Put((float)to);

            Peer.Send(packetWriter, DeliveryMethod.Sequenced);
        }

        public void ProjectileMoved(Projectile projectile, in Vector2 from, in Vector2 to)
        {
            NetDataWriter packetWriter = new NetDataWriter();

            packetWriter.Put((byte)2);
            packetWriter.Put((Int32)projectile.Id);

            packetWriter.Put((float)to.X);
            packetWriter.Put((float)to.Y);

            Peer.Send(packetWriter, DeliveryMethod.Sequenced);
        }


        public void CreatureIsMovingChanged(Creature creature, bool from, bool to)
        {

            NetDataWriter packetWriter = new NetDataWriter();

            packetWriter.Put((byte)5);
            packetWriter.Put((Int32)creature.Id);
            packetWriter.Put((bool)to);

            Peer.Send(packetWriter, DeliveryMethod.ReliableSequenced);
        }


        public void CreatureStartedAttacking(Creature creature)
        {
            NetDataWriter packetWriter = new NetDataWriter();

            packetWriter.Put((byte)10);
            packetWriter.Put((int)creature.Id);
            packetWriter.Put((bool)true);

            Peer.Send(packetWriter, DeliveryMethod.ReliableSequenced);
        }

        public void CreatureStoppedAttacking(Creature creature)
        {
            NetDataWriter packetWriter = new NetDataWriter();

            packetWriter.Put((byte)10);
            packetWriter.Put((int)creature.Id);
            packetWriter.Put((bool)false);

            Peer.Send(packetWriter, DeliveryMethod.ReliableSequenced);
        }
        public void OnReceivePacket(NetPacketReader packet)
        {
            byte id = packet.GetByte();

            switch (id)
            {
                case 0:
                    Player.Move(packet.GetFloat());
                    break;
                case 1:
                    Player.Stop();
                    break;
                case 2:
                    {
                        double angle = (double)packet.GetFloat();

                        Player.SetRotation(angle);
                    }
                    break;
                case 3:
                    {
                       this.AttackPressed = packet.GetBool();
                    }
                    break;

            }
        }
    }
}