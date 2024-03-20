using HietakissaUtils;
using UnityEngine;

public class Randomize : MonoBehaviour
{
    [SerializeField] int objectCount;
    //[SerializeField] GameObject[] objects;
    [SerializeField] LootTable<GameObject> objects;

    [SerializeField][Button(nameof(CreateObjects))] bool _CreateObjectsButton;
    [SerializeField][Button(nameof(DestroyChildren))] bool _DestroyChildrenButton;

    [SerializeField] Vector3 min;
    [SerializeField] Vector3 max;


    void Awake()
    {
        //CreateObjects();
    }

    public void CreateObjects()
    {
        for (int i = 0; i < objectCount; i++)
        {
            //Transform t = Instantiate(objects.RandomElement(), transform, false).transform;
            Transform t = Instantiate(objects.GetItem(), transform, false).transform;
            Setup(t);
        }


        void Setup(Transform t)
        {
            Vector3 randomPos = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit) && hit.point.y < max.y)
            {
                t.AlignToHit(hit);
                t.localRotation = Quaternion.Euler(Random.Range(-3f, 3f), Random.Range(0f, 360f), Random.Range(-3f, 3f));
                t.localScale = t.localScale * Random.Range(0.8f, 1.2f);
            }
            else Setup(t);

            //t.localPosition = new Vector3(Random.Range(-10f, 10f), Random.Range(0f, 5f), Random.Range(-10f, 10f));
            //t.localRotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            //t.localScale = Vector3.Scale(t.localScale, new Vector3(Random.Range(0.8f, 1.2f), Random.Range(0.8f, 1.2f), Random.Range(0.8f, 1.2f)));
        }
    }

    public void DestroyChildren()
    {
        transform.DestroyChildrenImmediate();
    }
}
