using Akka.Actor;

namespace SimpleClusterLib;

public class ServiceMessage
{
    public ServiceMessage(string serviceName, string payload)
    {
        ServiceName = serviceName;
        Payload = payload;
    }

    public string ServiceName { get; }
    public string Payload { get; }
}

public class PayloadEnvelope
{
    public PayloadEnvelope(IActorRef sender, string payload)
    {
        Sender = sender;
        Payload = payload;
    }

    public IActorRef Sender { get; }
    public string Payload { get; }
}

public class Response
{
    public Response(string payload)
    {
        Payload = payload;
    }

    public string Payload { get; }
}