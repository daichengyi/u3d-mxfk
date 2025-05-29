using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class Board: MonoBehaviour
    {
        [SerializeField]
        private Image boardSpr;
        [SerializeField]
        public GameObject holesNode;
        [SerializeField]
        private GameObject objsNode;
        [HideInInspector]
        public int layerIndex = 1;
        [HideInInspector]
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
            boardSpr.color = value;
        }

        public void SetLock(bool value, float opacity = 1f)
        {
            GetComponent<CanvasGroup>().alpha = opacity;

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
                GetComponent<Image>().color = Color.gray;
                boardSpr.color = Color.gray;
                boardSpr.DOFade(0.1f, 0.1f);
            }
            else
            {
                // 边框
                GetComponent<Image>().color = Color.white;

                //里边
                boardSpr.color = boardColor;
                boardSpr.DOFade(0f, 0.0f);
                boardSpr.DOFade(0.7f,0.5f);
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
            List<Transform> holes = new List<Transform>();
            for (int i = 0; i < holesNode.transform.childCount; i++)
            {
                holes.Add(holesNode.transform.GetChild(i));
            }
            holes.Sort((a, b) => b.position.y.CompareTo(a.position.y));

            foreach (var hole in holes)
            {
                GameObject objNode = Instantiate(objPrefab, hole.position, Quaternion.identity, objsNode.transform);

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
