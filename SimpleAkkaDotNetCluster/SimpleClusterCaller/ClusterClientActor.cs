using System.Collections.Immutable;
using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Event;
using SimpleClusterLib;

namespace SimpleClusterCaller;

public class ClusterClientActor: ReceiveActor
{
    public static Props Props(ImmutableHashSet<ActorPath> contactPoints)
        => Akka.Actor.Props.Create(() => new ClusterClientActor(contactPoints));
    
    private readonly ImmutableHashSet<ActorPath> _contactPoints;
    private readonly ILoggingAdapter _log;
    private IActorRef _clusterClient;

    public ClusterClientActor(ImmutableHashSet<ActorPath> contactPoints)
    {
        _contactPoints = contactPoints;
        _log = Context.GetLogger();

        Receive<ServiceMessage>(message =>
        {
            _clusterClient.Tell(new ClusterClient.Send(
                "/user/" + message.ServiceName, new PayloadEnvelope(Sender, message.Payload)));
        });
    }

    protected override void PreStart()
    {
        base.PreStart();
        _log.Info($"Actor started. Contact points: [{string.Join(", ", _contactPoints)}]");
        _clusterClient = Context.ActorOf(ClusterClient.Props(ClusterClientSettings.Create(Context.System)
            .WithInitialContacts(_contactPoints)));
    }
}