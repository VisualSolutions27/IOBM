using Prism.Events;

namespace Gijima.IOBM.Infrastructure.Events
{
    public class MessageEvent : PubSubEvent<object> {}

    public class ApplicationInfoEvent : PubSubEvent<object> { }

    public class IOBMSecurityEvent : PubSubEvent<object> { }
}
