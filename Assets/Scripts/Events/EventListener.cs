using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Events
{
    class EventListener
    {
        public EventListener() { }
        public delegate void EventListenerDelegate(EventStruct evt = null);
        public event EventListenerDelegate OnEvent;
        public void Excute(EventStruct evt)
        {
            if (OnEvent != null)
            {
                this.OnEvent(evt);
            }
        }

        //end class
    }
}