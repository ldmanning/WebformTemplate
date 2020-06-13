using System;
using WebformTemplate.ViewModel;

namespace WebformTemplate.Utilities
{
    public static class EventAggregator
    {
        public static void BroadCast(object message)
        {
            if (OnMessageTransmitted != null)
                OnMessageTransmitted(message);
        }
        public static Action<object> OnMessageTransmitted;

        public static void BroadCast(BroadcastCommand cmd, ViewModelBase v)
        {
            if (OnCommandTransmitted != null)
                OnCommandTransmitted(cmd, v);
        }
        public static Action<BroadcastCommand, ViewModelBase> OnCommandTransmitted;

        public static void BroadCast(BroadcastCommand cmd)
        {
            if (CommandTransmitted != null)
                CommandTransmitted(cmd);
        }
        public static Action<BroadcastCommand> CommandTransmitted;
    }

    public enum BroadcastCommand
    {
        CloseTab,
        Refresh,
        ClosePopup
    }
}
