using System.Collections.Generic;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    [SerializeField] TerrainMaterialSoundCollectionPair[] materialStepSounds;
    [SerializeField] SoundCollectionSO defaultFootSteps;
    [SerializeField] float stepThreshold = 0.2f;

    [SerializeField] Transform groundCheckPos;
    [SerializeField] float groundCheckDistance;

    Dictionary<TerrainMaterial, SoundCollectionSO> materialSounds = new Dictionary<TerrainMaterial, SoundCollectionSO>();

    Vector3 lastPos;
    float distanceMoved;

    const float CONST_GROUNDCHECKOFFSET = 0.1f;

    void Awake()
    {
        materialSounds.Clear();
        foreach (TerrainMaterialSoundCollectionPair pair in materialStepSounds)
        {
            materialSounds[pair.Material] = pair.Sounds;
        }

        lastPos = transform.position;
    }

    void Update()
    {
        //Debug.Log($"lastpos: {lastPos}, currentpos: {transform.position}, moved: {Vector3.Distance(transform.position, lastPos)}, estimated speed: {1f / Time.deltaTime * Vector3.Distance(transform.position, lastPos)}");
        
        if (Physics.Raycast(groundCheckPos.position + Vector3.up * CONST_GROUNDCHECKOFFSET, Vector3.down, out RaycastHit hit, groundCheckDistance + CONST_GROUNDCHECKOFFSET))
        {
            distanceMoved += Vector3.Distance(transform.position, lastPos);
            if (distanceMoved >= stepThreshold)
            {
                distanceMoved -= stepThreshold;

                TerrainMaterial material;
                if (hit.collider.TryGetComponent(out SurfaceType surfaceType)) material = surfaceType.Material;
                else material = GameManager.Instance.GetTerrainMaterialForPosition(transform.position);

                SoundCollectionSO sound;
                if (materialSounds.TryGetValue(material, out sound)) ;
                else sound = defaultFootSteps;


                EventManager.PlaySoundAtPosition(sound, transform.position);

                Debug.Log($"step; hit: {hit.collider.name}, material: {material}");
            }
        }

        lastPos = transform.position;
    }
}

[System.Serializable]
class TerrainMaterialSoundCollectionPair
{
    [field: SerializeField] public TerrainMaterial Material { get; private set; }
    [field: SerializeField] public SoundCollectionSO Sounds { get; private set; }
}
