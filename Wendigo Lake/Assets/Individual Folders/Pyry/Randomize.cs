using HietakissaUtils;

using UnityEngine;

public class Randomize : MonoBehaviour
{
    [SerializeField] int objectCount;
    [SerializeField] GameObject[] objects;

    void Awake()
    {
        for (int i = 0; i < objectCount; i++)
        {
            Transform t = Instantiate(objects.RandomElement(), transform, false).transform;
            Setup(t);
        }

        //foreach (Transform t in transform)
        //{
        //    Setup(t);
        //}



        void Setup(Transform t)
        {
            t.localPosition = new Vector3(Random.Range(-10f, 10f), Random.Range(0f, 5f), Random.Range(-10f, 10f));
            t.localRotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            t.localScale = new Vector3(Random.Range(0.8f, 1.2f), Random.Range(0.8f, 1.2f), Random.Range(0.8f, 1.2f));
        }
    }
}
