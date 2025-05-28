using System;
using UnityEngine;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class RopeBase : MonoBehaviour
    {
        public new int type;

        public virtual void RemoveFromBoard() { }

        public virtual  void SetType(int type, bool isAnimated = true) { }

        public virtual void MoveStart(Action onComplete) { }
    }
}
