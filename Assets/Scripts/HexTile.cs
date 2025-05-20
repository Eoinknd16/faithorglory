using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool isWalkable = true;
    public List<HexTile> neighbors = new();

    private void Start()
    {
        Debug.Log(gridPosition);
    }

    private void OnMouseDown()
    {
        if (Unit.selectedUnit != null && isWalkable)
        {
            Unit.selectedUnit.MoveToTile(this);
        }
    }

    public Vector3 GetWorldCenter()
    {
        return transform.position;
    }
}

