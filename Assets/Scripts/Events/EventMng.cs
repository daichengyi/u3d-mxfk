

namespace Assets.Scripts.Events
{
    class EventMng
    {
        /** 单例*/
        private volatile static EventMng _instance = null;
        public static EventMng Instance()
        {
            if (_instance == null)
            {
                _instance = new EventMng();
            }
            return _instance;
        }

        public static readonly EventDispatcher dispatcher = new EventDispatcher();

        public static void addEventListener(string eventType, EventListener.EventListenerDelegate callback = null)
        {
            dispatcher.addEventListener(eventType, callback);
        }

        public static void dispatchEvent(EventStruct evt, object gameObject)
        {
            dispatcher.dispatchEvent(evt, gameObject);
        }

        public static void removeEventListener(string eventType, EventListener.EventListenerDelegate callback = null)
        {
            dispatcher.removeEventListener(eventType, callback);
        }

        public bool hasListener(string eventType)
        {
            return dispatcher.hasListener(eventType);
        }

    }
}
