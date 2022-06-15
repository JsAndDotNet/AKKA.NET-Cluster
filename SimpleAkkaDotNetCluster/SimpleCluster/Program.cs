using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using SimpleCluster;
using SimpleClusterLib;
using System.Reflection;
using System.Xml.Linq;
using Akka.Cluster.Tools.PublishSubscribe;

namespace SimpleCluster
{
    class Program
    {
        private static void Main(string[] args)
        {
            StartUp(new [] { 2551, 2552, 2553 });
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        public static void StartUp(int[] ports)
        {
            var hoconFile = File.ReadAllText("akka-hocon.conf");
            var config = ConfigurationFactory.ParseString(hoconFile);

            foreach (var port in ports)
            {
                var clusterNodeConfig = ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                        .WithFallback(config)
                        .WithFallback(DistributedPubSub.DefaultConfig())
                        .WithFallback(ClusterClientReceptionist.DefaultConfig());

                //create an Akka system
                var system = ActorSystem.Create("ClusterSystem", clusterNodeConfig);

                var receptionist = ClusterClientReceptionist.Get(system);

                //create an actor that handles cluster domain events
                system.ActorOf(Props.Create(typeof(SimpleClusterListenerActor)), "clusterListener");

                var actorName = "ping-" + port;
                DisplayHelper.WriteLine("Creating:" + actorName);

                var echoService = system.ActorOf(Props.Create(() => new EchoActor()), actorName);

                var quickCheck = echoService.Ask<string>("Ping from creator");

                DisplayHelper.WriteLine(quickCheck.Result ?? "");

                receptionist.RegisterService(echoService);
            }
        }
    }
}


    