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
            DisplayHelper.WriteLine("EchoActor Message in " + message);
            Sender.Tell("Server returns:" + message);
        }

    }
}