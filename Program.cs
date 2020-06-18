using System;
using LiteNetLib;
using System.Threading;
using System.Diagnostics;

namespace MmorpgServer
{
    class Program
    {
        static void Main(string[] args)
        {

            EventBasedNetListener listener = new EventBasedNetListener();

            NetManager server = new NetManager(listener){
                ChannelsCount = 64
            };
            
            server.Start(8080);

            Scene.Instance = Scene.Load("SampleScene.xml");

            listener.ConnectionRequestEvent += request =>
            {
                request.AcceptIfKey("SomeConnectionKey");
            };

            listener.PeerConnectedEvent += peer =>
            {
                Creature player = new Creature()
                {
                    Position = new Vector2(5, 5)
                };

                Scene.Instance.Add(player);

                PlayerController ctrl = new PlayerController(player, peer);

                ctrl.Start();
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                PlayerController ctrl = PlayerController.GetFromPeer(peer);

                if (ctrl != null)
                {
                    ctrl.Stop();
                }
            };

            listener.NetworkReceiveEvent += (peer, packet, deliveryMethod) =>
            {
                try
                {
                    PlayerController ctrl = PlayerController.GetFromPeer(peer);

                    if (ctrl != null)
                    {
                        ctrl
                        .OnReceivePacket(packet);
                    }
                }
                finally
                {
                    packet.Recycle();
                }
            };

            Stopwatch stopwatch = new Stopwatch();

            TimeSpan frameTime = TimeSpan.FromSeconds(1.0 / 30.0);

            TimeSpan targetTime = frameTime;

            stopwatch.Start();

            while (true)
            {
                Scene.Instance.CurrentTime = stopwatch.Elapsed;

                server.PollEvents();

                Scene.Instance.Update(1.0 / 30.0);

                TimeSpan sleepTime = targetTime - stopwatch.Elapsed;

                if (sleepTime >= TimeSpan.FromMilliseconds(1))
                {
                    Thread.Sleep(sleepTime.Milliseconds);
                }

                targetTime += frameTime;
            }
        }
    }
}
