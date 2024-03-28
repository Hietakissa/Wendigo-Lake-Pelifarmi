using System.Collections.Generic;
using HietakissaUtils;
using UnityEngine.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [field: SerializeField] public Transform PlayerCameraTransform { get; private set; }
    [field: SerializeField] public Transform PlayerTransform { get; private set; }
    public List<PhotographableObject> photographableObjects { get; private set; }

    public void SetPlayerVisibility(float visibility) => PlayerVisibility = visibility;
    public float PlayerVisibility { get; private set; }

    public bool Paused { get; private set; }


    [SerializeField] MaterialColor[] terrainMaterials;
    [SerializeField] Texture2D splatMap;
    [SerializeField] Image image;

    void Awake()
    {
        Instance = this;

        photographableObjects = new List<PhotographableObject>();

        Manager[] managers = GetComponents<Manager>();
        foreach (Manager manager in managers)
        {
            manager.Initialize();
        }
    }

    void Update()
    {
        Vector2Int texCoords = PositionToSplatMapCoords(PlayerTransform.position);
        Color color = splatMap.GetPixel(texCoords.x, texCoords.y);
        color.a = 1f;
        Debug.Log($"{color}");
        image.color = color;


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Paused) EventManager.UnPause();
            else EventManager.Pause();
        }
    }


    public TerrainMaterial GetTerrainMaterialForPosition(Vector3 position)
    {
        Vector2Int texCoords = PositionToSplatMapCoords(PlayerTransform.position);
        Color pixelColor = splatMap.GetPixel(texCoords.x, texCoords.y);

        int closestIndex = 0;
        float closestDistance = Mathf.Infinity;
        for (int i = 0; i < terrainMaterials.Length; i++)
        {
            float colorDistance = Vector3.Distance(new Vector3(pixelColor.r, pixelColor.g, pixelColor.b), new Vector3(terrainMaterials[i].Color.r, terrainMaterials[i].Color.g, terrainMaterials[i].Color.b));
            if (colorDistance < closestDistance)
            {
                closestDistance = colorDistance;
                closestIndex = i;
            }
        }

        return terrainMaterials[closestIndex].Material;
    }

    Vector2Int PositionToSplatMapCoords(Vector3 pos)
    {
        Vector3 texturePos = new Vector3(Maf.ReMap(-500, 500, 0, 512, pos.x), 0f, Maf.ReMap(-500, 500, 0, 512, pos.z));
        return new Vector2Int(texturePos.x.RoundToNearest(), texturePos.z.RoundToNearest());
    }


    public static void ShowMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public static void HideMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void EventManager_OnPause()
    {
        Time.timeScale = float.Epsilon;
        Paused = true;
        ShowMouse();
    }
    void EventManager_OnUnPause()
    {
        Time.timeScale = 1f;
        Paused = false;
        HideMouse();
    }


    public void RegisterPhotographableObject(PhotographableObject photographableObject) => photographableObjects.Add(photographableObject);
    //public void UnregisterPhotographableObject(int objectID) => photographableObjects.RemoveAt(objectID);

    void OnEnable()
    {
        EventManager.OnPause += EventManager_OnPause;
        EventManager.OnUnPause += EventManager_OnUnPause;
    }

    void OnDisable()
    {
        EventManager.OnPause -= EventManager_OnPause;
        EventManager.OnUnPause -= EventManager_OnUnPause;
    }
}

[System.Serializable]
class MaterialColor
{
    [field: SerializeField] public TerrainMaterial Material;
    [field: SerializeField] public Color Color;
}

public enum TerrainMaterial
{
    Grass,
    Dirt
}
