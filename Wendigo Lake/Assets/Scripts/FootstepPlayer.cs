using System.Collections.Generic;
using HietakissaUtils;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    [SerializeField] TerrainMaterialSoundCollectionPair[] materialStepSounds;
    [SerializeField] SoundCollectionSO defaultFootSteps;
    [SerializeField] float stepThreshold = 0.2f;

    Dictionary<TerrainMaterial, SoundCollectionSO> materialSounds = new Dictionary<TerrainMaterial, SoundCollectionSO>();

    Vector3 lastPos;
    float distanceMoved;

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
        distanceMoved += Vector3.Distance(transform.position, lastPos);
        if (distanceMoved >= stepThreshold)
        {
            distanceMoved -= stepThreshold;
            //EventManager.PlaySoundAtPosition(footstepSounds, transform.position);
            TerrainMaterial material = GameManager.Instance.GetTerrainMaterialForPosition(transform.position);
            SoundCollectionSO sound;
            if (materialSounds.TryGetValue(material, out sound)) ;
            else sound = defaultFootSteps;

            Debug.Log($"material: {material}");

            EventManager.PlaySoundAtPosition(sound, transform.position);
        }

        lastPos = transform.position;
    }
}

[System.Serializable]
class TerrainMaterialSoundCollectionPair
{
    [field: SerializeField] public TerrainMaterial Material;
    [field: SerializeField] public SoundCollectionSO Sounds;
}
