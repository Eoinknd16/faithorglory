using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "MapSettings", menuName = "Game/Map Settings")]
public class MapSettings : ScriptableObject
{
    public int mapWidth = 100;
    public int mapHeight = 100;

    public float yOffset = 2.17f;

    public int minForestClusterSize = 8;
    public int maxForestClusterSize = 20;
    public float forestDensity = 0.5f;

    public int minTownSpacing = 6;
    public int maxTownSpacing = 10;
    public int minTownClusterSize = 3;
    public int maxTownClusterSize = 4;

    public int mapSeed = 1234;
}
