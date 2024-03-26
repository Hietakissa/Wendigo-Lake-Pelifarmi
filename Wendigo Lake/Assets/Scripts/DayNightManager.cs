using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    [SerializeField] Material skyMaterial;

    [SerializeField] float timeMultiplier;

    float dayTime;
    float nightTime;

    void Update()
    {
        if (dayTime < 1f)
        {
            dayTime = Mathf.Min(1f, dayTime + timeMultiplier * Time.deltaTime);
            skyMaterial.SetFloat("_dayTime", dayTime);
        }
        else if (nightTime < 1f)
        {
            nightTime = Mathf.Min(1f, nightTime + timeMultiplier * Time.deltaTime);
            skyMaterial.SetFloat("_dayToNight", nightTime);
        }
        //else
        //{
        //    dayTime = 0f;
        //    nightTime = 0f;
        //}

        //skyMaterial.SetFloat("_dayTime", dayTime);
        //skyMaterial.SetFloat("_dayToNight", nightTime);
    }


    void OnDisable()
    {
        dayTime = 0f;
        nightTime = 0f;

        skyMaterial.SetFloat("_dayTime", dayTime);
        skyMaterial.SetFloat("_dayToNight", nightTime);
    }
}
