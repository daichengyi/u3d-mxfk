using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Events
{
    class EventDispatcher
    {
        protected Dictionary<string,EventListener> eventListenerDict;

        public EventDispatcher()
        {
            this.eventListenerDict = new Dictionary<string,EventListener>();
        }

        /// 侦听事件
        public void addEventListener(string eventType, EventListener.EventListenerDelegate callback)
        {
            if (!this.eventListenerDict.ContainsKey(eventType))
            {
                this.eventListenerDict.Add(eventType, new EventListener());
            }
            this.eventListenerDict[eventType].OnEvent += callback;
        }

        /// <summary>
        /// 移除事件
        public void removeEventListener(string eventType, EventListener.EventListenerDelegate callback)
        {
            if (this.eventListenerDict.ContainsKey(eventType))
            {
                this.eventListenerDict[eventType].OnEvent -= callback;
            }
        }

        /// 发送事件
        public void dispatchEvent(string evtType, object gameObject=null)
        {
            //if (evt == null) return;
            //if (evt.eventType == null) return;
            EventStruct evt = new EventStruct(evtType);
            if (eventListenerDict.ContainsKey(evt.eventType) == false)
            {
                Debug.Log("dispatchEvent 事件" + evt.eventType);
                return;
            }
            EventListener eventListener = eventListenerDict[evt.eventType];
            if (eventListener == null) return;
            evt.target = gameObject;
            eventListener.Excute(evt);
        }

        /// <summary>
        public bool hasListener(string eventType)
        {
            return this.eventListenerDict.ContainsKey(eventType);
        }

        //end class
    }
}




   
