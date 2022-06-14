using Akka.Actor;

namespace SimpleClusterLib
{
    public class EchoActor : UntypedActor
    {
        public EchoActor()
        {
            DisplayHelper.WriteLine("Echo actor created @ " + this.Self.Path.Address);
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case string str:
                    DisplayHelper.WriteLine("EchoActor Message in " + str);
                    Sender.Tell("Server returns:" + str);
                    break;
                case PayloadEnvelope envelope:
                    DisplayHelper.WriteLine("EchoActor Message in " + envelope.Payload);
                    envelope.Sender.Tell("Server returns:" + envelope.Payload);
                    break;
            }
        }

    }
}