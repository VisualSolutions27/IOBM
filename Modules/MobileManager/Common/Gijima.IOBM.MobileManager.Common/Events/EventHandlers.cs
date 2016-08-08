using Gijima.IOBM.MobileManager.Common.Structs;
using Prism.Events;

namespace Gijima.IOBM.MobileManager.Common.Events
{
    public class NavigationEvent : PubSubEvent<int> { }
    public class ActionCompletedEvent : PubSubEvent<ActionCompleted> {}
    public class ReadInvoicesEvent : PubSubEvent<int> {}
    public class ReadDevicesEvent : PubSubEvent<int> {}
    public class ReadSimmCardsEvent : PubSubEvent<int> {}
    public class LinkDeviceSimmCardEvent : PubSubEvent<int> {}
    public class LinkSimmCardDeviceEvent : PubSubEvent<int> {}
    public class SearchResultEvent : PubSubEvent<int> {}
    public class SetActivityLogProcessEvent : PubSubEvent<object> {}
    public class ShowInvoiceReportEvent : PubSubEvent<object> {}
    public class MobileManagerSecurityEvent : PubSubEvent<object> {}
    public class BillingProcessEvent : PubSubEvent<BillingExecutionState> {}
    public class BillingProcessHistoryEvent : PubSubEvent<bool> { }
    public class BillingProcessStartedEvent : PubSubEvent<BillingExecutionState> { }
    public class BillingProcessCompletedEvent : PubSubEvent<BillingExecutionState> { }
    public class DataValiationResultEvent : PubSubEvent<object> {}
    public class DataImportUpdateResultEvent : PubSubEvent<object> { }
}
