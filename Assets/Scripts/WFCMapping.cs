using System.Collections.Generic;
using UnityEngine;

public class WFCMapGenerator : MonoBehaviour
{
    public int mapWidth = 20;
    public int mapHeight = 20;

    public GameObject grassPrefab;
    public GameObject forestPrefab;
    public GameObject townPrefab;
    public GameObject villagePrefab;
    public GameObject minePrefab;
    public GameObject roadPrefab;

    [Header("Forest Settings")]
    public int forestTileCount = 30;
    public int forestClusterRadius = 3;

    [Header("Town Settings")]
    public int townClusterSize = 3;
    public float townMinDistance = 5f;

    [Header("Village Settings")]
    public int villageClusterSize = 2;
    public float villageMinDistance = 3f;

    [Header("Mine Settings")]
    public float mineMinDistance = 4f;

    private enum TileType { Grass, Forest, Town, Village, Mine, Road }

    private class Tile
    {
        public Vector2Int position;
        public TileType type;
        public GameObject instance;
    }

    private Tile[,] tiles;
    private List<Tile> towns = new();
    private List<Tile> villages = new();
    private List<Tile> mines = new();

    public void GenerateMap()
    {
        ClearMap();
        tiles = new Tile[mapWidth, mapHeight];

        // Step 1: Fill with grass
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float xOffset = 0.75f; // Width scale factor for pointy-top hexes (depends on your prefab size)
                float yOffset = Mathf.Sqrt(3) / 2f; // ≈ 0.866 for correct vertical offset

                float xPos = x * xOffset;
                float zPos = y * yOffset;
                if (x % 2 == 1) zPos += yOffset / 2f; // offset every other column

                Vector3 pos = new Vector3(xPos, 0, zPos);

                GameObject instance = Instantiate(grassPrefab, pos, Quaternion.identity, transform);
                tiles[x, y] = new Tile { position = new Vector2Int(x, y), type = TileType.Grass, instance = instance };
            }
        }

        // Step 2: Forest clusters
        PlaceClusters(TileType.Forest, forestTileCount, forestClusterRadius, forestPrefab);

        // Step 3: Town clusters
        PlaceClusters(TileType.Town, townClusterSize, (int)townMinDistance, townPrefab, towns);

        // Step 4: Village clusters
        PlaceClusters(TileType.Village, villageClusterSize, (int)villageMinDistance, villagePrefab, villages);

        // Step 5: Mines (single-tile with distance)
        PlaceSingleTiles(TileType.Mine, 5, mineMinDistance, minePrefab, mines);

        // Step 6: Roads between villages and towns
        GenerateRoads();
    }

    private void PlaceClusters(TileType type, int clusterSize, int minDistance, GameObject prefab, List<Tile> tileList = null)
    {
        int attempts = 0, placed = 0;
        while (placed < 5 && attempts < 500)
        {
            int x = Random.Range(0, mapWidth);
            int y = Random.Range(0, mapHeight);
            if (IsValidLocation(x, y, type, minDistance))
            {
                for (int i = 0; i < clusterSize; i++)
                {
                    int offsetX = x + Random.Range(-1, 2);
                    int offsetY = y + Random.Range(-1, 2);
                    if (IsInBounds(offsetX, offsetY) && tiles[offsetX, offsetY].type == TileType.Grass)
                    {
                        ReplaceTile(offsetX, offsetY, type, prefab, tileList);
                    }
                }
                placed++;
            }
            attempts++;
        }
    }

    private void PlaceSingleTiles(TileType type, int count, float minDistance, GameObject prefab, List<Tile> tileList)
    {
        int placed = 0, attempts = 0;
        while (placed < count && attempts < 1000)
        {
            int x = Random.Range(0, mapWidth);
            int y = Random.Range(0, mapHeight);
            if (IsValidLocation(x, y, type, minDistance))
            {
                ReplaceTile(x, y, type, prefab, tileList);
                placed++;
            }
            attempts++;
        }
    }

    private bool IsValidLocation(int x, int y, TileType type, float minDistance)
    {
        if (!IsInBounds(x, y) || tiles[x, y].type != TileType.Grass)
            return false;

        List<Tile> compareList = type switch
        {
            TileType.Town => towns,
            TileType.Village => villages,
            TileType.Mine => mines,
            _ => null,
        };

        if (compareList == null) return true;

        foreach (var tile in compareList)
        {
            if (Vector2Int.Distance(tile.position, new Vector2Int(x, y)) < minDistance)
                return false;
        }

        return true;
    }

    private void ReplaceTile(int x, int y, TileType newType, GameObject prefab, List<Tile> tileList = null)
    {
        Destroy(tiles[x, y].instance);
        Vector3 pos = new(x, 0, y);
        GameObject instance = Instantiate(prefab, pos, Quaternion.identity, transform);
        Tile tile = new Tile { position = new Vector2Int(x, y), type = newType, instance = instance };
        tiles[x, y] = tile;
        tileList?.Add(tile);
    }

    private void GenerateRoads()
    {
        List<Tile> connections = new();
        connections.AddRange(towns);
        connections.AddRange(villages);

        foreach (Tile from in connections)
        {
            Tile to = FindClosestTarget(from, connections);
            if (to != null && from != to)
            {
                List<Vector2Int> path = AStar(from.position, to.position);
                foreach (var pos in path)
                {
                    if (tiles[pos.x, pos.y].type == TileType.Grass || tiles[pos.x, pos.y].type == TileType.Forest)
                        ReplaceTile(pos.x, pos.y, TileType.Road, roadPrefab);
                }
            }
        }
    }

    private Tile FindClosestTarget(Tile from, List<Tile> all)
    {
        Tile closest = null;
        float minDist = float.MaxValue;
        foreach (Tile tile in all)
        {
            float dist = Vector2Int.Distance(from.position, tile.position);
            if (tile != from && dist < minDist)
            {
                closest = tile;
                minDist = dist;
            }
        }
        return closest;
    }

    private List<Vector2Int> AStar(Vector2Int start, Vector2Int goal)
    {
        HashSet<Vector2Int> closed = new();
        PriorityQueue<Vector2Int> open = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, float> gScore = new();

        open.Enqueue(start, 0);
        gScore[start] = 0;

        while (open.Count > 0)
        {
            Vector2Int current = open.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            closed.Add(current);
            foreach (Vector2Int dir in Directions())
            {
                Vector2Int neighbor = current + dir;
                if (!IsInBounds(neighbor.x, neighbor.y) || closed.Contains(neighbor))
                    continue;

                float tentative = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentative < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative;
                    float fScore = tentative + Vector2Int.Distance(neighbor, goal);
                    open.Enqueue(neighbor, fScore);
                }
            }
        }
        return new();
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new() { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    private bool IsInBounds(int x, int y) => x >= 0 && y >= 0 && x < mapWidth && y < mapHeight;

    private void ClearMap()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        towns.Clear();
        villages.Clear();
        mines.Clear();
    }

    private List<Vector2Int> Directions() => new() {
        Vector2Int.up, Vector2Int.down,
        Vector2Int.left, Vector2Int.right
    };

    private class PriorityQueue<T>
    {
        private readonly List<(T item, float priority)> elements = new();

        public int Count => elements.Count;

        public void Enqueue(T item, float priority) => elements.Add((item, priority));

        public T Dequeue()
        {
            int best = 0;
            for (int i = 1; i < elements.Count; i++)
                if (elements[i].priority < elements[best].priority)
                    best = i;
            T item = elements[best].item;
            elements.RemoveAt(best);
            return item;
        }
    }
}
