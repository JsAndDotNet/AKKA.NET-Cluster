// See https://aka.ms/new-console-template for more information


using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Configuration;
using SimpleClusterLib;
using System.Collections.Immutable;
using System.Reflection;
using System.Xml.Linq;

try
{

    var clusterPorts = new List<int>() { 2551, 2552, 2553 };

    var _baseUrl = "akka.tcp://ClusterSystem@localhost:2551";
    var _baseUrl2 = "akka.tcp://ClusterSystem@localhost:2551";

    var port = 2550;
    var baseLocation = Assembly.GetAssembly(typeof(Program));
    var dirInfo = new DirectoryInfo(baseLocation.Location);
    var hoconFilePath = dirInfo.Parent + "\\akka-hocon.conf";
    Console.WriteLine(hoconFilePath);
    var hoconFile = XElement.Parse(File.ReadAllText(hoconFilePath));

    var config = ConfigurationFactory.ParseString(hoconFile.Descendants("hocon").Single().Value);

    var clusterNodeConfig =
                        ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                            .WithFallback(config);

    //create an Akka system
    Console.WriteLine("Create actor system");
    var actorSystem = ActorSystem.Create("ClusterSystem", clusterNodeConfig);
    Console.WriteLine("Actor system created");

    var intialContacts = ImmutableHashSet<ActorPath>.Empty
                .Add(ActorPath.Parse(_baseUrl + "/system/receptionist"))
                .Add(ActorPath.Parse(_baseUrl2 + "/system/receptionist"));

    var clusterClientSettings = ClusterClientSettings.Create(actorSystem)
        .WithInitialContacts(intialContacts);


    var clusterClientProps = ClusterClient.Props(clusterClientSettings);

    foreach (var clusterPort in clusterPorts)
    {
        var clusterClient = actorSystem.ActorOf(clusterClientProps, "ping-" + clusterPort);

        var foundSerializer = actorSystem.Serialization.FindSerializerFor("Test");


        var resp = await clusterClient.Ask("Hello port " + clusterPort);

        //var resp = await clusterClient.Ask
        //    (new ClusterClient.Send("ping-" + clusterPort, "Hello port " + clusterPort));

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


