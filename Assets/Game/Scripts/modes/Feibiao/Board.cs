using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class Board: MonoBehaviour
    {
        private GameObject boardSpr;

        public GameObject holesNode;

        private GameObject objsNode;

        public int layerIndex = 1;
        public bool isLocked = false;

        private Color boardColor = Color.white;

        void Start()
        {
            Rigidbody2D rigidBody = GetComponent<Rigidbody2D>();
            rigidBody.bodyType = RigidbodyType2D.Static;
            gameObject.SetActive(false);
        }

        public void SetBoardColor(Color value)
        {
            boardColor = value;
            boardSpr.GetComponent<SpriteRenderer>().color = value;
        }

        public void SetLock(bool value, float opacity = 255f)
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, opacity / 255f);

            Rigidbody2D rigidBody = GetComponent<Rigidbody2D>();
            if (opacity == 0)
            {
                gameObject.SetActive(false);
                if (rigidBody.bodyType != RigidbodyType2D.Static)
                {
                    Invoke("SetStaticBody", 0.1f);
                }
            }
            else
            {
                gameObject.SetActive(true);
                if (rigidBody.bodyType != RigidbodyType2D.Dynamic)
                {
                    Invoke("SetDynamicBody", 0.1f);
                }
            }

            if (isLocked == value)
            {
                return;
            }

            isLocked = value;
            if (value)
            {
                renderer.color = Color.gray;
                boardSpr.GetComponent<SpriteRenderer>().color = Color.gray;
                boardSpr.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.04f);
            }
            else
            {
                boardSpr.GetComponent<SpriteRenderer>().color = boardColor;
                // 使用 DOTween 或其他动画系统替代 cc.tintTo
                // TODO: 实现颜色渐变动画
                boardSpr.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
                // TODO: 实现透明度渐变动画
            }

            var comps = GetScrewComps();
            foreach (var comp in comps)
            {
                comp.SetLock(value);
            }
        }

        private void SetStaticBody()
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }

        private void SetDynamicBody()
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }

        public void Init(GameObject objPrefab)
        {
            List<Transform> holes = new List<Transform>(holesNode.transform.GetComponentsInChildren<Transform>());
            holes.Sort((a, b) => b.position.y.CompareTo(a.position.y));

            foreach (var hole in holes)
            {
                GameObject objNode = Instantiate(objPrefab, hole.position, Quaternion.identity);
                objNode.transform.SetParent(objsNode.transform);

                Rope screwComp = objNode.GetComponent<Rope>();
                Rigidbody2D rigidBody = screwComp.sprite.GetComponent<Rigidbody2D>();

                HingeJoint2D revoluteJoint = gameObject.AddComponent<HingeJoint2D>();
                revoluteJoint.connectedBody = rigidBody;
                revoluteJoint.anchor = new Vector2(objNode.transform.position.x, objNode.transform.position.y);
            }
        }

        public List<Rope> GetScrewComps()
        {
            List<Rope> tmp = new List<Rope>();
            foreach (Transform objNode in objsNode.transform)
            {
                Rope screwComp = objNode.GetComponent<Rope>();
                tmp.Add(screwComp);
            }
            return tmp;
        }
    }
}
