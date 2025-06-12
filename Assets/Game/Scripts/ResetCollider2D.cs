using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class ResetCollider2D : MonoBehaviour
{
    PolygonCollider2D poly2D;
    Vector2[] pos_save;//存放PolygonCollider2D的Point点位
    public float identity = 100f;
    // Start is called before the first frame update

    public Vector2[] initPos;

    void OnEnable()
    {


        if (poly2D == null)
        {
            poly2D = GetComponent<PolygonCollider2D>();
            DestroyImmediate(poly2D);

            Image img = GetComponent<Image>();

            SpriteRenderer spRend = GetComponent<SpriteRenderer>();
            if (spRend == null)
            {
                spRend = transform.AddComponent<SpriteRenderer>();
            }
            spRend.sprite = img.sprite;

            //img.sprite = spRend.sprite;
            //img.SetNativeSize();
            //DestroyImmediate(spRend);

            poly2D = transform.AddComponent<PolygonCollider2D>();
            DestroyImmediate(spRend);

            initPos = (Vector2[])poly2D.GetPath(0).Clone();

        }
        pos_save = (Vector2[])initPos.Clone();
        for (int i = 0; i < pos_save.Length; i++)
        {
            pos_save[i] = pos_save[i] * identity;
        }
        poly2D.SetPath(0, pos_save);

        DestroyImmediate(this);
    }

}
#endif