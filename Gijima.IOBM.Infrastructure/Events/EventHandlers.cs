using Gijima.IOBM.Infrastructure.Structs;
using Prism.Events;

namespace Gijima.IOBM.Infrastructure.Events
{
    public class ApplicationMessageEvent : PubSubEvent<ApplicationMessage> {}
    public class ApplicationInfoEvent : PubSubEvent<object> {}
    public class ProgressBarInfoEvent : PubSubEvent<ProgressBarInfo> {}
    public class IOBMSecurityEvent : PubSubEvent<object> {}
}
