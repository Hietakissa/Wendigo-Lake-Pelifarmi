using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    [SerializeField] Material skyMaterial;
    [SerializeField] Light directionalLight;
    [SerializeField] float timeMultiplier = 0.1f;

    [SerializeField] float[] fogIntensity;
    [SerializeField] Color[] fogColor;
    [SerializeField] Color[] lightColor;

    [SerializeField][Range(0f, 1f)] float dayTime;
    [SerializeField][Range(0f, 1f)] float nightTime;

    bool progressDay;
    bool progressNight;


    void Awake()
    {
        dayTime = 0f;
        nightTime = 0f;

        skyMaterial.SetFloat("_dayTime", 0f);
        skyMaterial.SetFloat("_dayToNight", 0f);
        RenderSettings.fogColor = fogColor[0];
    }

    void Update()
    {
        if (progressDay && dayTime < 1f)
        {
            dayTime = Mathf.Min(1f, dayTime + timeMultiplier * Time.deltaTime);
            UpdateValues();
        }
        else if (progressNight && nightTime < 1f)
        {
            nightTime = Mathf.Min(1f, nightTime + timeMultiplier * Time.deltaTime);
            UpdateValues();
        }
    }

    void UpdateValues()
    {
        Color fogColor;
        Color lightColor;

        if (dayTime < 1f)
        {
            fogColor = Color.Lerp(this.fogColor[0], this.fogColor[1], dayTime);
            lightColor = Color.Lerp(this.lightColor[0], this.lightColor[1], dayTime);
            RenderSettings.fogDensity = Mathf.Lerp(fogIntensity[0], fogIntensity[1], dayTime);
        }
        else
        {
            fogColor = Color.Lerp(this.fogColor[1], this.fogColor[2], nightTime);
            lightColor = Color.Lerp(this.lightColor[1], this.lightColor[2], nightTime);
            RenderSettings.fogDensity = Mathf.Lerp(fogIntensity[1], fogIntensity[2], nightTime);
        }


        skyMaterial.SetFloat("_dayTime", dayTime);
        skyMaterial.SetFloat("_dayToNight", nightTime);


        RenderSettings.fogColor = fogColor;
        directionalLight.color = lightColor;
    }


    [ContextMenu("Progress Day (1 of 2)")]
    public void BeginDayProgress()
    {
        progressDay = true;
    }

    [ContextMenu("Progress Night (2 of 2)")]
    public void BeginNightProgress()
    {
        progressNight = true;
    }


    void OnValidate()
    {
        UpdateValues();
    }


    void OnDisable()
    {
        skyMaterial.SetFloat("_dayTime", 1f);
        skyMaterial.SetFloat("_dayToNight", 1f);
    }
}
