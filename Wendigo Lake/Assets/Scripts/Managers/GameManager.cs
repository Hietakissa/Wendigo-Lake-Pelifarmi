using UnityEngine.SceneManagement;
using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [field: SerializeField] public Transform PlayerCameraTransform { get; private set; }
    [field: SerializeField] public Transform PlayerTransform { get; private set; }
    public List<PhotographableObject> photographableObjects { get; private set; }

    public void SetPlayerVisibility(float visibility) => PlayerVisibility = visibility;
    public float PlayerVisibility { get; private set; }
    public bool HasCollectedClues { get; private set; }
    public bool HasWonGame { get; private set; }
    public bool IsPlayerAlive { get; private set; } = true;
    public float Sensitivity = 1f;

    public bool Paused { get; private set; }


    [SerializeField] MaterialColor[] terrainMaterials;
    [SerializeField] Texture2D splatMap;

    [SerializeField] DeerAI deerPrefab;
    [SerializeField] float deerSpawnDelay = 30f;
    [SerializeField] int maxDeer = 10;
    List<DeerAI> deer = new List<DeerAI>();
    float deerSpawnTime;

    void Awake()
    {
        Instance = this;

        photographableObjects = new List<PhotographableObject>();

        Manager[] managers = GetComponents<Manager>();
        foreach (Manager manager in managers)
        {
            manager.Initialize();
        }

        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
    }

    void Start() => EventManager.Pause();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Paused) EventManager.UnPause();
            else EventManager.Pause();
        }

        if (deer.Count < maxDeer)
        {
            deerSpawnTime += Time.deltaTime;
            if (deerSpawnTime >= deerSpawnDelay)
            {
                deerSpawnTime -= deerSpawnDelay;
                deer.Add(Instantiate(deerPrefab, Vector3.zero, Quaternion.identity));
            }
        }
    }


    public TerrainMaterial GetTerrainMaterialForPosition(Vector3 position)
    {
        Vector2Int texCoords = PositionToSplatMapCoords(position);
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


    public void UnPause()
    {
        EventManager.UnPause();
    }

    public void Quit()
    {
        QOL.Quit();
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

    void EventManager_PlayerDied()
    {
        IsPlayerAlive = false;
        StartCoroutine(PlayerDiedCor());
    }

    IEnumerator PlayerDiedCor()
    {
        yield return QOL.GetWaitForSeconds(3);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    void Photography_OnDeerDied(DeerAI deer)
    {
        this.deer.Remove(deer);
    }

    void EventManager_OnCollectedAllClues()
    {
        HasCollectedClues = true;
    }

    void EventManager_OnWonGame()
    {
        EventManager.Pause();
        HasWonGame = true;
    }


    public void RegisterPhotographableObject(PhotographableObject photographableObject) => photographableObjects.Add(photographableObject);
    //public void UnregisterPhotographableObject(int objectID) => photographableObjects.RemoveAt(objectID);

    void OnEnable()
    {
        EventManager.OnPause += EventManager_OnPause;
        EventManager.OnUnPause += EventManager_OnUnPause;

        EventManager.OnPlayerDied += EventManager_PlayerDied;

        EventManager.Photography.OnDeerDied += Photography_OnDeerDied;

        EventManager.OnCollectedAllClues += EventManager_OnCollectedAllClues;

        EventManager.OnWonGame += EventManager_OnWonGame;
    }

    void OnDisable()
    {
        EventManager.OnPause -= EventManager_OnPause;
        EventManager.OnUnPause -= EventManager_OnUnPause;

        EventManager.OnPlayerDied -= EventManager_PlayerDied;

        EventManager.Photography.OnDeerDied -= Photography_OnDeerDied;

        EventManager.OnCollectedAllClues -= EventManager_OnCollectedAllClues;

        EventManager.OnWonGame -= EventManager_OnWonGame;
    }
}

[System.Serializable]
class MaterialColor
{
    [field: SerializeField] public TerrainMaterial Material { get; private set; }
    [field: SerializeField] public Color Color { get; private set; }
}

public enum TerrainMaterial
{
    Grass,
    Dirt,
    Wood,
    Sand
}
