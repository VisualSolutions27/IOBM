using Gijima.IOBM.Infrastructure.Structs;
using Prism.Events;

namespace Gijima.IOBM.Infrastructure.Events
{
    public class MessageEvent : PubSubEvent<object> {}
    public class ApplicationInfoEvent : PubSubEvent<object> {}
    public class ProgressBarInfoEvent : PubSubEvent<ProgressBarInfo> {}
    public class IOBMSecurityEvent : PubSubEvent<object> {}
}
