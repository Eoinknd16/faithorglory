using UnityEngine;
using UnityEngine.InputSystem;

public class MouseFunctions : MonoBehaviour
{
    private GameObject currentTile;
    private GameObject currentUnit;
    private Material originalTileMaterial;
    private Material originalUnitMaterial;

    public Material highlightMaterial; // Set this in the Inspector

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Check if we hit a tile or a unit
            if (hitObject.CompareTag("Basic_tile") || hitObject.CompareTag("Forest_tile") || hitObject.CompareTag("GreekFactionTile") || hitObject.CompareTag("Town_tile") || hitObject.CompareTag("Village_tile"))
            {
                if (hitObject != currentTile)
                {
                    ClearTileHighlight(); // Clear previous tile highlight
                    currentTile = hitObject;

                    Renderer renderer = currentTile.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        originalTileMaterial = renderer.material;
                        renderer.material = highlightMaterial;
                    }
                }
            }
            else if (hitObject.CompareTag("Unit")) // If it's a unit (e.g., the Prophet)
            {
                if (hitObject != currentUnit)
                {
                    ClearUnitHighlight(); // Clear previous unit highlight
                    currentUnit = hitObject;

                    Renderer renderer = currentUnit.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        originalUnitMaterial = renderer.material;
                        renderer.material = highlightMaterial;
                    }
                }
            }
        }
        else
        {
            ClearTileHighlight();
            ClearUnitHighlight();
        }
    }

    void ClearTileHighlight()
    {
        if (currentTile != null)
        {
            Renderer renderer = currentTile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = originalTileMaterial;
            }
            currentTile = null;
            originalTileMaterial = null;
        }
    }

    void ClearUnitHighlight()
    {
        if (currentUnit != null)
        {
            Renderer renderer = currentUnit.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = originalUnitMaterial;
            }
            currentUnit = null;
            originalUnitMaterial = null;
        }
    }
}
