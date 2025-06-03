using System;
using UnityEngine;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class Collider2DListener : MonoBehaviour
    {
        private Action<Collision2D> callback;

        // Use this for initialization
        public void addListener(Action<Collision2D> cb)
        {
            callback = cb;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            callback?.Invoke(collision);
        }
    }
}