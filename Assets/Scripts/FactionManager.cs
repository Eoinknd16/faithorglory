using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // For Button

public class FactionManager : MonoBehaviour
{
    public enum Faction { None, Faction1, Faction2, Faction3, Faction4 }

    public Faction selectedFaction = Faction.None;  // This will store the currently selected faction
    public bool isPlacingFactionTile = false;

    // Prefabs for each faction's starting tile and Prophet unit
    public GameObject faction1TilePrefab;  // Greek faction tile prefab
    public GameObject faction2TilePrefab;
    public GameObject faction3TilePrefab;
    public GameObject faction4TilePrefab;
    public GameObject prophetPrefab;  // Prophet prefab

    // Button references
    public Button faction1Button;
    public Button faction2Button;
    public Button faction3Button;
    public Button faction4Button;
    public Button spawnUnitsButton;  // Button for spawning units

    

    // To track if a faction has placed its tile
    private bool isFaction1Placed = false;
    private bool isFaction2Placed = false;
    private bool isFaction3Placed = false;
    private bool isFaction4Placed = false;

    // Store the position of the placed faction tile for spawning the Prophet
    private Vector3 faction1TilePosition;

    void Start()
    {
        // Hook up button click listeners
        faction1Button.onClick.AddListener(() => SelectFaction(Faction.Faction1));
        faction2Button.onClick.AddListener(() => SelectFaction(Faction.Faction2));
        faction3Button.onClick.AddListener(() => SelectFaction(Faction.Faction3));
        faction4Button.onClick.AddListener(() => SelectFaction(Faction.Faction4));
        spawnUnitsButton.onClick.AddListener(SpawnUnits);  // Spawn units button listener

        // Disable the spawn button initially
        spawnUnitsButton.interactable = false;

        // Disable buttons if the faction tile has already been placed
        UpdateButtonState();
    }

    // This method is triggered when a faction button is clicked
    public void SelectFaction(Faction faction)
    {
        selectedFaction = faction;
        isPlacingFactionTile = true;
        spawnUnitsButton.interactable = false;  // Disable spawn button until tile is placed
        Debug.Log($"Selected Faction: {selectedFaction}. Click a tile to place your starting location.");
    }

    private bool hasPlacedThisClick = false;

    void Update()
    {
        if (isPlacingFactionTile)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && !hasPlacedThisClick)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.CompareTag("Basic_tile"))
                    {
                        Vector3 spawnPosition = hit.collider.transform.position;

                        Destroy(hit.collider.gameObject);

                        // Instantiate the faction tile
                        GameObject prefabToUse = GetFactionTilePrefab(selectedFaction);
                        if (prefabToUse != null)
                        {
                            GameObject placedTile = Instantiate(prefabToUse, spawnPosition, Quaternion.identity);
                            Debug.Log($"Faction {selectedFaction} tile placed at {spawnPosition}");

                            // If it's the Greek faction (Faction 1), store the position for Prophet spawning
                            if (selectedFaction == Faction.Faction1)
                            {
                                faction1TilePosition = spawnPosition;
                            }

                            // Enable spawn units button once the tile is placed
                            spawnUnitsButton.interactable = true;
                        }
                        else
                        {
                            Debug.LogError("Prefab is null for selected faction.");
                        }

                        isPlacingFactionTile = false;
                        hasPlacedThisClick = true;

                        MarkFactionAsPlaced(selectedFaction);
                        UpdateButtonState();
                    }
                    else
                    {
                        Debug.Log("Clicked on an invalid tile, not a valid Basic_tile.");
                    }
                }
            }

            // Reset for next click
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                hasPlacedThisClick = false;
            }
        }
    }

    // Spawn the Prophet unit when the spawn button is pressed
    void SpawnUnits()
    {
        if (selectedFaction == Faction.Faction1) // Greek faction
        {
            // Spawn Prophet at the stored position of Faction 1's tile
            Vector3 spawnPosition = faction1TilePosition;
            GameObject spawnedUnit = Instantiate(prophetPrefab, spawnPosition, Quaternion.identity);

            // Adjust the spawn position by adding the Y offset
            Unit unitScript = spawnedUnit.GetComponent<Unit>();
            if (unitScript != null)
            {
                spawnPosition.y += unitScript.spawnYOffset;
                spawnedUnit.transform.position = spawnPosition;
            }

            Debug.Log($"Greek Prophet spawned at {spawnPosition}");
        }
        else
        {
            // For other factions, spawn at the respective faction tile
            Vector3 spawnPosition = GetFactionTilePosition(selectedFaction);
            GameObject spawnedUnit = Instantiate(prophetPrefab, spawnPosition, Quaternion.identity);

            // Adjust the spawn position by adding the Y offset
            Unit unitScript = spawnedUnit.GetComponent<Unit>();
            if (unitScript != null)
            {
                spawnPosition.y += unitScript.spawnYOffset;
                spawnedUnit.transform.position = spawnPosition;
            }

            Debug.Log($"Prophet spawned at {spawnPosition}");
        }

        // Disable the spawn button once units are spawned
        spawnUnitsButton.interactable = false;
    }



    // Get the appropriate prefab based on selected faction
    GameObject GetFactionTilePrefab(Faction faction)
    {
        switch (faction)
        {
            case Faction.Faction1: // Greek faction
                return faction1TilePrefab;
            case Faction.Faction2:
                return faction2TilePrefab;
            case Faction.Faction3:
                return faction3TilePrefab;
            case Faction.Faction4:
                return faction4TilePrefab;
            default:
                return null;
        }
    }

    // Get the spawn position of the tile for the selected faction (for non-Greek factions)
    Vector3 GetFactionTilePosition(Faction faction)
    {
        // This method should return the correct position for the other factions
        // Placeholder logic to return a default position for now
        return new Vector3(0, 0, 0);
    }

    // Marks the selected faction as placed
    void MarkFactionAsPlaced(Faction faction)
    {
        switch (faction)
        {
            case Faction.Faction1:
                isFaction1Placed = true;
                break;
            case Faction.Faction2:
                isFaction2Placed = true;
                break;
            case Faction.Faction3:
                isFaction3Placed = true;
                break;
            case Faction.Faction4:
                isFaction4Placed = true;
                break;
        }
    }

    // Update the button state based on whether a faction's tile has been placed
    void UpdateButtonState()
    {
        faction1Button.interactable = !isFaction1Placed;
        faction2Button.interactable = !isFaction2Placed;
        faction3Button.interactable = !isFaction3Placed;
        faction4Button.interactable = !isFaction4Placed;
    }
}
