using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ResolutionDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private Resolution[] resolutions;

    void Start()
    {
        // Get available resolutions
        resolutions = Screen.resolutions;

        // Clear existing options
        resolutionDropdown.ClearOptions();

        // Create a list of resolution strings
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            // Check if this resolution matches the current screen size
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        // Add options to the dropdown
        resolutionDropdown.AddOptions(options);

        // Set the default value to the current resolution
        resolutionDropdown.value = currentResolutionIndex;

        // Refresh the displayed value
        resolutionDropdown.RefreshShownValue();
    }

    // Call this method when the user selects a resolution from the dropdown
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
