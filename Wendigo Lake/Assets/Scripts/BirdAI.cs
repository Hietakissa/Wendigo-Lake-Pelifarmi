using HietakissaUtils;
using UnityEngine;

public class BirdAI : MonoBehaviour
{
    [SerializeField] float speed;
    Vector3 target;
    bool moving;

    Vector3 startScale;

    const float dist = 35f;

    void Awake()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        if (moving)
        {

            Vector3 offset = new Vector3(Mathf.Sin(Time.time * 3f) * 1.2f, 0f, 0f);

            transform.Translate(Maf.Direction(transform.position, target) * speed * Time.deltaTime, Space.World);
            transform.Translate(offset * Time.deltaTime);

            float scaleMultiplier = Vector3.Distance(transform.position, target) / dist;
            transform.localScale = scaleMultiplier * startScale;

            if (scaleMultiplier < 0.1f || Vector3.Distance(transform.position, target) < 1f) Destroy(gameObject);
        }
    }


    public void Photographed(ImageParams imageParams)
    {
        if (moving || !imageParams.usedFlash) return;

        moving = true;
        target = transform.position + new Vector3(Random.Range(-dist, dist), Random.Range(dist * 0.5f, dist), Random.Range(-dist, dist));
    }
}
