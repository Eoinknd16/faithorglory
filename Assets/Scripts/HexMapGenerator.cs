using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HexTileSettings
{
    public GameObject prefab;
    public int minCount;
    public int maxCount;
    public int minSpacing;
}

public class HexMapGenerator : MonoBehaviour
{
    [Header("Hex Settings")]
    public GameObject baseHexPrefab;
    public float hexRadius = 1f;
    public int width = 35;
    public int height = 35;

    [Header("Forest Settings")]
    public HexTileSettings forestTile = new HexTileSettings { minCount = 150, maxCount = 200, minSpacing = 3 };
    public int minForestClusterSize = 8;
    public int maxForestClusterSize = 20;
    public float forestDensity = 0.5f;

    [Header("Town Settings")]
    public HexTileSettings townTile = new HexTileSettings { minCount = 6, maxCount = 8, minSpacing = 6 };
    public int minTownSpacing = 6;
    public int maxTownSpacing = 10;
    public int minTownClusterSize = 3;
    public int maxTownClusterSize = 4;

    [Header("Village Settings")]
    public HexTileSettings villageTile = new HexTileSettings { minCount = 12, maxCount = 18, minSpacing = 6 };
    public int minVillageSpacing = 6;
    public int maxVillageSpacing = 8;

    [Header("Special Tile Settings")]
    public HexTileSettings goldMineTile = new HexTileSettings { minCount = 2, maxCount = 4, minSpacing = 6 };

    private float hexWidth;
    private float hexHeight;

    private Dictionary<Vector2Int, GameObject> spawnedTiles = new();
    private Transform tileContainer;
    private List<Vector2Int> townCenters = new();
    private HashSet<Vector2Int> villageCenters = new();

    private void Awake()
    {
        hexWidth = hexRadius * 2f;
        hexHeight = Mathf.Sqrt(3f) * hexRadius;
        tileContainer = new GameObject("HexTiles").transform;
        tileContainer.SetParent(transform);
    }

    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        ClearMap();

        PlaceTowns();
        PlaceVillages();
        PlaceForestClusters();
        PlaceSpecialTiles(goldMineTile);

        FillBaseHexes();
    }

    private void ClearMap()
    {
        foreach (var kvp in spawnedTiles)
        {
            if (kvp.Value != null)
                DestroyImmediate(kvp.Value);
        }
        spawnedTiles.Clear();
        townCenters.Clear();
        villageCenters.Clear();
    }

    private void FillBaseHexes()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int coord = new(x, y);
                if (!spawnedTiles.ContainsKey(coord))
                {
                    Vector3 pos = CalculateWorldPosition(x, y);
                    GameObject tile = Instantiate(baseHexPrefab, pos, Quaternion.identity, tileContainer);
                    spawnedTiles[coord] = tile;
                }
            }
        }
    }

    private void PlaceTowns()
    {
        int count = Random.Range(townTile.minCount, townTile.maxCount + 1);
        int attempts = 0;
        int maxAttempts = 1000;

        while (count > 0 && attempts < maxAttempts)
        {
            Vector2Int center = new(Random.Range(0, width), Random.Range(0, height));
            if (!IsTooClose(center, townCenters, minTownSpacing) && !spawnedTiles.ContainsKey(center))
            {
                List<Vector2Int> cluster = new() { center };
                Queue<Vector2Int> queue = new();
                queue.Enqueue(center);

                int clusterSize = Random.Range(minTownClusterSize, maxTownClusterSize + 1);
                HashSet<Vector2Int> visited = new() { center };

                while (queue.Count > 0 && cluster.Count < clusterSize)
                {
                    Vector2Int current = queue.Dequeue();
                    foreach (Vector2Int neighbor in GetHexNeighbors(current))
                    {
                        if (IsValidCoord(neighbor) && !spawnedTiles.ContainsKey(neighbor) && !visited.Contains(neighbor))
                        {
                            cluster.Add(neighbor);
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                            if (cluster.Count >= clusterSize) break;
                        }
                    }
                }

                if (cluster.Count >= minTownClusterSize)
                {
                    foreach (Vector2Int coord in cluster)
                    {
                        Vector3 pos = CalculateWorldPosition(coord.x, coord.y);
                        GameObject tile = Instantiate(townTile.prefab, pos, Quaternion.identity, tileContainer);
                        spawnedTiles[coord] = tile;
                    }
                    townCenters.Add(center);
                    count--;
                }
            }
            attempts++;
        }

        if (attempts >= maxAttempts)
            Debug.LogWarning("Stopped placing towns early due to max attempts.");
    }

    private void PlaceVillages()
    {
        int count = Random.Range(villageTile.minCount, villageTile.maxCount + 1);
        int attempts = 0;
        int maxAttempts = 2000;

        while (count > 0 && attempts < maxAttempts)
        {
            Vector2Int coord = new(Random.Range(0, width), Random.Range(0, height));

            if (!IsTooClose(coord, townCenters, minVillageSpacing)
                && !IsTooClose(coord, villageCenters, minVillageSpacing)
                && !spawnedTiles.ContainsKey(coord))
            {
                Vector3 pos = CalculateWorldPosition(coord.x, coord.y);
                GameObject tile = Instantiate(villageTile.prefab, pos, Quaternion.identity, tileContainer);
                spawnedTiles[coord] = tile;
                villageCenters.Add(coord);
                count--;
            }

            attempts++;
        }

        if (attempts >= maxAttempts)
            Debug.LogWarning("Stopped placing villages early due to max attempts.");
    }

    private void PlaceForestClusters()
    {
        int targetCount = Mathf.FloorToInt(forestTile.maxCount * forestDensity);
        int tries = 0;
        int maxTries = 1000;

        while (targetCount > 0 && tries < maxTries)
        {
            int clusterSize = Random.Range(minForestClusterSize, maxForestClusterSize);
            Vector2Int start = new(Random.Range(0, width), Random.Range(0, height));

            int placed = 0;
            HashSet<Vector2Int> cluster = new();
            Queue<Vector2Int> queue = new();
            queue.Enqueue(start);
            cluster.Add(start);

            while (queue.Count > 0 && placed < clusterSize)
            {
                Vector2Int current = queue.Dequeue();
                if (!IsValidCoord(current) || spawnedTiles.ContainsKey(current)) continue;

                Vector3 pos = CalculateWorldPosition(current.x, current.y);
                GameObject tile = Instantiate(forestTile.prefab, pos, Quaternion.identity, tileContainer);
                spawnedTiles[current] = tile;
                placed++;
                targetCount--;

                foreach (Vector2Int neighbor in GetHexNeighbors(current))
                {
                    if (cluster.Count >= clusterSize) break;
                    if (!cluster.Contains(neighbor))
                    {
                        cluster.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            tries++;
        }

        if (tries >= maxTries)
            Debug.LogWarning("Stopped placing forests early due to max attempts.");
    }

    private void PlaceSpecialTiles(HexTileSettings settings)
    {
        int count = Random.Range(settings.minCount, settings.maxCount + 1);
        int attempts = 0;

        while (count > 0 && attempts < 500)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Vector2Int coord = new(x, y);

            if (IsValidCoord(coord) && !spawnedTiles.ContainsKey(coord))
            {
                Vector3 pos = CalculateWorldPosition(x, y);
                GameObject tile = Instantiate(settings.prefab, pos, Quaternion.identity, tileContainer);
                spawnedTiles[coord] = tile;
                count--;
            }

            attempts++;
        }
    }

    private Vector3 CalculateWorldPosition(int x, int y)
    {
        float xOffset = x * hexWidth * 0.75f;
        float zOffset = y * hexHeight + ((x % 2 == 1) ? hexHeight / 2f : 0f);
        return new Vector3(xOffset, 0f, zOffset);
    }

    private bool IsValidCoord(Vector2Int coord) =>
        coord.x >= 0 && coord.y >= 0 && coord.x < width && coord.y < height;

    private bool IsTooClose(Vector2Int coord, IEnumerable<Vector2Int> otherCoords, int minSpacing)
    {
        foreach (var other in otherCoords)
        {
            if (Vector2Int.Distance(coord, other) < minSpacing)
                return true;
        }
        return false;
    }

    private IEnumerable<Vector2Int> GetHexNeighbors(Vector2Int coord)
    {
        int x = coord.x;
        int y = coord.y;
        bool odd = x % 2 == 1;

        yield return new Vector2Int(x - 1, y);
        yield return new Vector2Int(x + 1, y);
        yield return new Vector2Int(x, y - 1);
        yield return new Vector2Int(x, y + 1);
        yield return new Vector2Int(x - 1, y + (odd ? 0 : -1));
        yield return new Vector2Int(x + 1, y + (odd ? 0 : -1));
    }
}