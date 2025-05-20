using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public MapSettings settings;
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public TMP_InputField forestMaxCluster;
    public TMP_InputField forestMinCluster;
    public TMP_InputField minTownSize;
    public TMP_InputField maxTownSize;
    public TMP_InputField seed;

    public GameObject settingsPanel;

    public void Apply()
    {
        if (int.TryParse(widthInput.text, out int width))
        {
            settings.mapWidth = width;
        }
        else
        {
            Debug.LogWarning("Invalid width input");
        }

        if (int.TryParse(heightInput.text, out int height))
        {
            settings.mapHeight = height;
        }
        else
        {
            Debug.LogWarning("Invalid height input");
        }

        if (int.TryParse(forestMaxCluster.text, out int maxClusterForest))
        {
            settings.maxForestClusterSize = maxClusterForest;
        }

        if (int.TryParse(forestMinCluster.text, out int minClusterForest))
        {
            settings.minForestClusterSize = minClusterForest;
        }

        if(int.TryParse(minTownSize.text, out int townminsize))
        {
            settings.minTownClusterSize = townminsize;
        }

        if(int.TryParse(maxTownSize.text, out int townmaxsize))
        {
            settings.maxTownClusterSize = townmaxsize;
        }

        if(int.TryParse(seed.text, out int seedInt))
        {
            settings.mapSeed = seedInt;
        }

        Debug.Log($"Map size updated to {settings.mapWidth} x {settings.mapHeight}, Map settings set");
    }

    public void StartBtn()
    {
        SceneManager.LoadScene("Map");
    }

    public void SettingsBtn()
    {
        settingsPanel.SetActive(true);
    }

    public void HideSettingsPanel()
    {
        settingsPanel.SetActive(false);
    }

}
