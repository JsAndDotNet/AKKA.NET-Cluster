// See https://aka.ms/new-console-template for more information


using Akka.Actor;
using Akka.Configuration;
using SimpleClusterLib;
using System.Collections.Immutable;
using SimpleClusterCaller;

try
{

    var clusterPorts = new List<int>() { 2551, 2552, 2553 };

    var _baseUrl = "akka.tcp://ClusterSystem@localhost:2551";
    var _baseUrl2 = "akka.tcp://ClusterSystem@localhost:2552";

    var thisPort = 3000;
    var hoconFile = File.ReadAllText("akka-hocon.conf");
    var config = ConfigurationFactory.ParseString(hoconFile);

    var clusterNodeConfig =
                        ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + thisPort)
                            .WithFallback(config);

    //create an Akka system
    Console.WriteLine("Create actor system");
    var actorSystem = ActorSystem.Create("ClusterClientSystem", clusterNodeConfig);
    Console.WriteLine("Actor system created");

    var initialContacts = ImmutableHashSet<ActorPath>.Empty
                .Add(ActorPath.Parse(_baseUrl) / "system" / "receptionist")
                .Add(ActorPath.Parse(_baseUrl2) / "system" / "receptionist");

    var clusterClient = actorSystem.ActorOf(ClusterClientActor.Props(initialContacts), "cluster-client-actor");
    
    foreach (var clusterPort in clusterPorts)
    {
        var actorName = "ping-" + clusterPort;
        var resp = await clusterClient.Ask<string>(new ServiceMessage(actorName, "Hello port " + clusterPort));
        DisplayHelper.WriteLine("Cluster returned " + resp);
    }
}
catch (Exception ex)
{
    DisplayHelper.WriteLine(ex.ToString(), ConsoleColor.Red);
}




Console.ReadLine();
Console.WriteLine("Hit enter a few more times to exit.");
Console.ReadLine();
Console.ReadLine();
Console.ReadLine();


