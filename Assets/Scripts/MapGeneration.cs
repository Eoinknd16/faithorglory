using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct TileSettings
{
    public GameObject prefab;
    public int minCount;
    public int maxCount;
    public int minSpacing;
    public float yOffset;
}

public class MapGeneration : MonoBehaviour
{
    public MapSettings settings;


    [Header("Tile Animation Settings")]
    public float spawnHeight = 20f;
    public float fallDuration = 1.5f;
    public AnimationCurve fallEasing;
    public float rippleDelayPerUnit = 0.05f;

    [Header("Hex Settings")]
    public GameObject defaultHexPrefab;
    public float hexRadius = 1f;
    //public int mapWidth = 35;
    //public int mapHeight = 35;

    [Header("Forest Settings")]
    public TileSettings forestTileSettings = new TileSettings { minCount = 150, maxCount = 200, minSpacing = 3 };
    //public int minForestClusterSize = 8;
    //public int settings.maxForestClusterSize = 20;
    public float forestDensity = 0.5f;

    [Header("Town Settings")]
    public TileSettings townTileSettings = new TileSettings { minCount = 6, maxCount = 8, minSpacing = 6 };
    public int minTownSpacing = 6;
    public int maxTownSpacing = 10;
    //public int settings.minTownClusterSize = 3;
    //public int settings.maxTownClusterSize = 4;

    [Header("Village Settings")]
    public TileSettings villageTileSettings = new TileSettings { minCount = 12, maxCount = 18, minSpacing = 6 };
    public int minVillageSpacing = 6;
    public int maxVillageSpacing = 8;

    [Header("Special Tile Settings")]
    public TileSettings goldMineTileSettings = new TileSettings { minCount = 2, maxCount = 4, minSpacing = 6 };
    

    [Header("Seed Settings")]
    //public TMP_InputField settings.mapSeed;
    public bool useRandomSeed = false;

    private float tileWidth;
    private float tileHeight;

    private Dictionary<Vector2Int, GameObject> generatedTiles = new();
    private Transform tileParent;
    private List<Vector2Int> townLocations = new();
    private HashSet<Vector2Int> villageLocations = new();

    private List<Coroutine> runningCoroutines = new();
    private int actualSeed;

    public bool isActive = false;

    // Object Pooling: Reusable Hex Tiles Pool
    private Stack<GameObject> hexTilePool = new Stack<GameObject>();


    public void Start ()
    {
        isActive = true;
        GenerateNewMap();
        //HexTile.GetWorldCenter();
    }

    private void Awake()
    {
        //if(isActive) return;

        //int seed = 12345;
        //settings.mapSeed = seed;

        tileWidth = hexRadius * 2f;
        tileHeight = Mathf.Sqrt(3f) * hexRadius;
        tileParent = new GameObject("HexTilesContainer").transform;
        tileParent.SetParent(transform);

        InitializeSeed();
    }

    private void InitializeSeed()
    {
        if (useRandomSeed)
        {
            actualSeed = Random.Range(int.MinValue, int.MaxValue);
            settings.mapSeed = actualSeed;
            Debug.Log($"[MapGeneration] Using random seed: {actualSeed}");
        }
        else
        {
            int actualSeed = settings.mapSeed;
            
            Debug.Log($"[MapGeneration] Using fixed seed: {actualSeed}");
        }

        Random.InitState(actualSeed);
    }

    [ContextMenu("Generate New Map")]
    public void GenerateNewMap()
    {
        
        if (!isActive) return;

        InitializeSeed();
        ClearExistingMap();
        PlaceTownsOnMap();
        PlaceVillagesOnMap();
        PlaceForestClustersOnMap();
        PlaceSpecialTiles(goldMineTileSettings);
        PopulateBaseHexTiles();

        isActive = false;
    }

    [ContextMenu("Generate With New Random Seed")]
    public void GenerateNewRandomSeedMap()
    {
        useRandomSeed = true;
        GenerateNewMap();
        useRandomSeed = false;
    }

    private void ClearExistingMap()
    {
        foreach (Coroutine c in runningCoroutines)
        {
            if (c != null) StopCoroutine(c);
        }
        runningCoroutines.Clear();

        foreach (var kvp in generatedTiles)
        {
            if (kvp.Value != null)
                ReturnTileToPool(kvp.Value);  // Recycle tile instead of destroying
        }
        generatedTiles.Clear();
        townLocations.Clear();
        villageLocations.Clear();
    }

    private void PopulateBaseHexTiles()
    {
        for (int x = 0; x < settings.mapWidth; x++)
        {
            for (int y = 0; y < settings.mapHeight; y++)
            {
                Vector2Int coordinates = new(x, y);
                if (!generatedTiles.ContainsKey(coordinates))
                {
                    Vector3 position = CalculateTileWorldPosition(x, y);
                    Vector3 spawnPos = position + Vector3.up * spawnHeight;
                    GameObject hexTile = GetPooledTile();
                    hexTile.transform.position = spawnPos;
                    hexTile.SetActive(true);

                    Coroutine routine = StartCoroutine(AnimateTileFall(hexTile, position, coordinates));
                    runningCoroutines.Add(routine);

                    HexTile hexTileScript = hexTile.GetComponent<HexTile>();
                    if (hexTileScript != null)
                    {
                        hexTileScript.gridPosition = coordinates;
                    }

                    generatedTiles[coordinates] = hexTile;
                }
            }
        }
    }

    private void PlaceTownsOnMap()
    {
        int townCount = Random.Range(townTileSettings.minCount, townTileSettings.maxCount + 1);
        int maxAttempts = 1000;
        int attempts = 0;

        while (townCount > 0 && attempts < maxAttempts)
        {
            Vector2Int center = new(Random.Range(0, settings.mapWidth), Random.Range(0, settings.mapHeight));
            if (IsValidCoordinates(center) &&
                !IsTileTooCloseToOthers(center, new List<Vector2Int>(townLocations), minTownSpacing) &&
                !generatedTiles.ContainsKey(center))
            {
                List<Vector2Int> cluster = new() { center };
                Queue<Vector2Int> queue = new();
                queue.Enqueue(center);

                int clusterSize = Random.Range(settings.minTownClusterSize, settings.maxTownClusterSize + 1);
                HashSet<Vector2Int> visited = new() { center };

                while (queue.Count > 0 && cluster.Count < clusterSize)
                {
                    Vector2Int current = queue.Dequeue();
                    foreach (Vector2Int neighbor in GetHexNeighbors(current))
                    {
                        if (IsValidCoordinates(neighbor) &&
                            !generatedTiles.ContainsKey(neighbor) &&
                            !visited.Contains(neighbor))
                        {
                            cluster.Add(neighbor);
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                            if (cluster.Count >= clusterSize) break;
                        }
                    }
                }

                if (cluster.Count >= settings.minTownClusterSize)
                {
                    foreach (Vector2Int coord in cluster)
                    {
                        Vector3 pos = CalculateTileWorldPosition(coord.x, coord.y);
                        GameObject tile = Instantiate(townTileSettings.prefab, tileParent);
                        tile.transform.position = pos + Vector3.up * spawnHeight;
                        Coroutine routine = StartCoroutine(AnimateTileFall(tile, pos, coord));
                        runningCoroutines.Add(routine);
                        generatedTiles[coord] = tile;
                    }
                    townLocations.Add(center);
                    townCount--;
                }
            }
            attempts++;
        }

        if (attempts >= maxAttempts)
            Debug.LogWarning("Max attempts reached while placing towns.");
    }

    private void PlaceVillagesOnMap()
    {
        int villageCount = Random.Range(villageTileSettings.minCount, villageTileSettings.maxCount + 1);
        int maxAttempts = 2000;
        int attempts = 0;

        while (villageCount > 0 && attempts < maxAttempts)
        {
            Vector2Int coord = new(Random.Range(0, settings.mapWidth), Random.Range(0, settings.mapHeight));
            if (IsValidCoordinates(coord) &&
                !IsTileTooCloseToOthers(coord, new List<Vector2Int>(townLocations), minVillageSpacing) &&
                !IsTileTooCloseToOthers(coord, new List<Vector2Int>(villageLocations), minVillageSpacing) &&
                !generatedTiles.ContainsKey(coord))
            {
                Vector3 pos = CalculateTileWorldPosition(coord.x, coord.y);
                GameObject tile = Instantiate(villageTileSettings.prefab, tileParent);
                tile.transform.position = pos + Vector3.up * spawnHeight;
                Coroutine routine = StartCoroutine(AnimateTileFall(tile, pos, coord));
                runningCoroutines.Add(routine);
                generatedTiles[coord] = tile;
                villageLocations.Add(coord);
                villageCount--;
            }
            attempts++;
        }

        if (attempts >= maxAttempts)
            Debug.LogWarning("Max attempts reached while placing villages.");
    }

    private void PlaceForestClustersOnMap()
    {
        int forestCount = Random.Range(forestTileSettings.minCount, forestTileSettings.maxCount + 1);
        int maxAttempts = 2000;
        int attempts = 0;

        while (forestCount > 0 && attempts < maxAttempts)
        {
            Vector2Int center = new(Random.Range(0, settings.mapWidth), Random.Range(0, settings.mapHeight));
            if (IsValidCoordinates(center) &&
                !IsTileTooCloseToOthers(center, new List<Vector2Int>(townLocations), forestTileSettings.minSpacing) &&
                !generatedTiles.ContainsKey(center))
            {
                List<Vector2Int> cluster = new() { center };
                Queue<Vector2Int> queue = new();
                queue.Enqueue(center);

                int clusterSize = Random.Range(settings.minForestClusterSize, settings.maxForestClusterSize + 1);
                HashSet<Vector2Int> visited = new() { center };

                while (queue.Count > 0 && cluster.Count < clusterSize)
                {
                    Vector2Int current = queue.Dequeue();
                    foreach (Vector2Int neighbor in GetHexNeighbors(current))
                    {
                        if (IsValidCoordinates(neighbor) &&
                            !generatedTiles.ContainsKey(neighbor) &&
                            !visited.Contains(neighbor))
                        {
                            cluster.Add(neighbor);
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                            if (cluster.Count >= clusterSize) break;
                        }
                    }
                }

                if (cluster.Count >= settings.minForestClusterSize)
                {
                    foreach (Vector2Int coord in cluster)
                    {
                        Vector3 pos = CalculateTileWorldPosition(coord.x, coord.y);
                        GameObject tile = Instantiate(forestTileSettings.prefab, tileParent);
                        tile.transform.position = pos + Vector3.up * spawnHeight;
                        Coroutine routine = StartCoroutine(AnimateTileFall(tile, pos, coord));
                        runningCoroutines.Add(routine);
                        generatedTiles[coord] = tile;
                    }
                    forestCount--;
                }
            }
            attempts++;
        }

        if (attempts >= maxAttempts)
            Debug.LogWarning("Max attempts reached while placing forests.");
    }

    private void PlaceSpecialTiles(TileSettings tileSettings)
    {
        int count = Random.Range(tileSettings.minCount, tileSettings.maxCount + 1);
        int maxAttempts = 500;
        int attempts = 0;

        while (count > 0 && attempts < maxAttempts)
        {
            Vector2Int coord = new(Random.Range(0, settings.mapWidth), Random.Range(0, settings.mapHeight));
            if (IsValidCoordinates(coord) &&
                !generatedTiles.ContainsKey(coord))
            {
                Vector3 pos = CalculateTileWorldPosition(coord.x, coord.y);
                GameObject tile = Instantiate(tileSettings.prefab, tileParent);
                tile.transform.position = pos + Vector3.up * spawnHeight;

                Coroutine routine = StartCoroutine(AnimateTileFall(tile, pos + Vector3.up * tileSettings.yOffset, coord));
                runningCoroutines.Add(routine);
                generatedTiles[coord] = tile;
                count--;
            }
            attempts++;
        }

        if (attempts >= maxAttempts)
            Debug.LogWarning("Max attempts reached while placing special tiles.");
    }

    private bool IsValidCoordinates(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < settings.mapWidth && coords.y >= 0 && coords.y < settings.mapHeight;
    }

    private bool IsTileTooCloseToOthers(Vector2Int coords, List<Vector2Int> otherLocations, int minSpacing)
    {
        foreach (var location in otherLocations)
        {
            if (Vector2Int.Distance(coords, location) < minSpacing)
            {
                return true;
            }
        }
        return false;
    }

    private List<Vector2Int> GetHexNeighbors(Vector2Int position)
    {
        List<Vector2Int> neighbors = new()
        {
            new Vector2Int(position.x + 1, position.y),
            new Vector2Int(position.x - 1, position.y),
            new Vector2Int(position.x, position.y + 1),
            new Vector2Int(position.x, position.y - 1)
        };
        return neighbors;
    }

    private Vector3 CalculateTileWorldPosition(int x, int y)
    {
        float xPos = x * tileWidth * 0.75f;
        float yPos = y * tileHeight;

        if (x % 2 != 0)
        {
            yPos += tileHeight / 2f;
        }

        return new Vector3(xPos, 0f, yPos);
    }

    private GameObject GetPooledTile()
    {
        if (hexTilePool.Count > 0)
        {
            return hexTilePool.Pop();
        }
        else
        {
            return Instantiate(defaultHexPrefab);
        }
    }

    private void ReturnTileToPool(GameObject tile)
    {
        tile.SetActive(false);
        hexTilePool.Push(tile);
    }

    private IEnumerator AnimateTileFall(GameObject tile, Vector3 targetPosition, Vector2Int coord)
    {
        if (tile == null) yield break;

        // Calculate the distance from the center to create a ripple effect
        Vector2 center = new Vector2(settings.mapWidth / 2f, settings.mapHeight / 2f);
        float distanceFromCenter = Vector2.Distance(new Vector2(coord.x, coord.y), center);

        // Stagger the start of the fall animation based on the distance (ripple effect)
        float delay = distanceFromCenter * rippleDelayPerUnit;
        float elapsedDelay = 0f;

        while (elapsedDelay < delay)
        {
            elapsedDelay += Time.deltaTime;
            yield return null;
        }

        // Now start the fall animation after the delay
        float elapsed = 0f;
        Vector3 startPos = tile.transform.position;

        // Animate the tile fall over the specified duration
        while (elapsed < fallDuration)
        {
            if (tile == null) yield break;
            tile.transform.position = Vector3.Lerp(startPos, targetPosition, fallEasing.Evaluate(elapsed / fallDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the tile is exactly at the target position after the animation
        if (tile != null)
        {
            tile.transform.position = targetPosition;
        }
    }


}
