using DG.Tweening;
using UnityEngine;

namespace Assets.Game.Scripts.modes
{
    public class TestRote : MonoBehaviour
    {
        public float rotationSpeed = 180f; // 每秒旋转角度
        public bool clockwise = true;
        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            // 计算每帧旋转角度
            float angle = rotationSpeed * Time.deltaTime;
            if (!clockwise)
            {
                angle = -angle;
            }

            // 执行旋转
            transform.Rotate(0, 0, angle);
        }
    }
}