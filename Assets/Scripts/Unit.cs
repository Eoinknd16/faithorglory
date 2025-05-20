using UnityEngine;

public class Unit : MonoBehaviour
{
    public static Unit selectedUnit;
    public float spawnYOffset = 1f;

    [SerializeField] private float moveSpeed = 5f;
    private bool isMoving = false;
    private Vector3 targetPosition;

    

    private void Update()
    {
        HandleSelection();

        if (isMoving)
            MoveToTarget();
            
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    selectedUnit = this;
                    Debug.Log($"Selected unit: {gameObject.name}");
                }
            }
        }
    }

    public void MoveToTile(HexTile tile)
    {
        targetPosition = tile.GetWorldCenter();
        targetPosition.y += spawnYOffset;  // Apply the offset to the Y position
        isMoving = true;
    }


    private void MoveToTarget()
    {
        // Move only along X and Z axes; keep Y fixed
        Vector3 targetPositionWithYOffset = targetPosition;
        targetPositionWithYOffset.y = transform.position.y;  // Keep the current Y position

        transform.position = Vector3.MoveTowards(transform.position, targetPositionWithYOffset, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPositionWithYOffset) < 0.05f)
        {
            isMoving = false;
            selectedUnit = null;
            Debug.Log($"{gameObject.name} arrived at tile.");
        }
    }

}
