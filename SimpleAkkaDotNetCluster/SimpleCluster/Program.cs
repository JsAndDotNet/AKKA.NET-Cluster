using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using SimpleCluster;
using SimpleClusterLib;
using System.Reflection;
using System.Xml.Linq;

namespace SimpleCluster
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Specify a port (2551,2552,2553), or leave blank for 'all'");
            var resp = Console.ReadLine();

            int enteredPort = 0;
            if(int.TryParse(resp, out enteredPort))
            {
                args = new string[] { enteredPort.ToString() };
            }

            StartUp(args.Length == 0 ? new String[] { "2551", "2552", "2553" /*"0"*/ } : args);
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        public static void StartUp(string[] ports)
        {
            var baseLocation = Assembly.GetAssembly(typeof(Program));
            var dirInfo = new DirectoryInfo(baseLocation.Location);
            var hoconFilePath = dirInfo.Parent + "\\akka-hocon.conf";
            Console.WriteLine(hoconFilePath);
            var hoconFile = XElement.Parse(File.ReadAllText(hoconFilePath));

            foreach (var port in ports)
            {
                var config = ConfigurationFactory.ParseString(hoconFile.Descendants("hocon").Single().Value);

                var clusterNodeConfig =
                                    ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                                        .WithFallback(config);

                //create an Akka system
                var system = ActorSystem.Create("ClusterSystem", clusterNodeConfig);

                var receptionist = ClusterClientReceptionist.Get(system);


                //Cluster cluster = Cluster.Get(actorSystem);

                //create an actor that handles cluster domain events
                system.ActorOf(Props.Create(typeof(SimpleClusterListenerActor)), "clusterListener");

                var actorName = "ping-" + port;
                DisplayHelper.WriteLine("Creating:" + actorName);

                var echoService = system.ActorOf(Props.Create(typeof(EchoActor)), actorName);

                var quickCheck = echoService.Ask("Ping from creator");

                DisplayHelper.WriteLine(quickCheck.Result?.ToString() ?? "");


                receptionist.RegisterService(echoService);
            }
        }
    }
}


    