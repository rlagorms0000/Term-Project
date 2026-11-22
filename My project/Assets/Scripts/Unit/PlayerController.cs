using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UnitStats), typeof(UnitManagementUI))]
public class PlayerController : MonoBehaviour
{
    public GridManager gridManager;

    private Vector2Int currentPosition;
    private bool isSelected = false;
    private UnitStats unitStats;
    private UnitManagementUI unitManagementUI;
    private readonly HashSet<Vector2Int> reachablePositions = new HashSet<Vector2Int>();
    private readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private void Awake()
    {
        currentPosition = new Vector2Int(2, 2);
        unitStats = GetComponent<UnitStats>();
        unitManagementUI = GetComponent<UnitManagementUI>();

        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

        if (gridManager == null)
        {
            Debug.LogError("PlayerController requires a GridManager.", this);
            enabled = false;
            return;
        }

        if (!gridManager.TryOccupyCell(currentPosition, gameObject))
        {
            Debug.LogError($"Player cannot occupy grid cell {currentPosition} because it is outside the grid or already occupied.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        if (gridManager != null && enabled)
            transform.position = gridManager.GetWorldPosition(currentPosition);
    }

    private void OnDestroy()
    {
        if (gridManager != null)
            gridManager.ReleaseCell(currentPosition, gameObject);
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
            !unitManagementUI.IsPointerOverWindow())
            HandleMouseClick();
    }

    private void HandleMouseClick()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0;

        Vector2Int clickedPosition = new Vector2Int(
            Mathf.RoundToInt(mousePosition.x),
            Mathf.RoundToInt(mousePosition.y)
        );

        if (clickedPosition == currentPosition)
        {
            isSelected = true;
            CalculateMoveRange();
            Debug.Log("플레이어 선택!");
            return;
        }

        if (!isSelected)
            return;

        if (!reachablePositions.Contains(clickedPosition))
        {
            Debug.Log("이동할 수 없는 위치입니다.");
            return;
        }

        if (!gridManager.TryMoveOccupant(currentPosition, clickedPosition, gameObject))
        {
            Debug.Log("해당 칸은 이미 점유되어 있습니다.");
            return;
        }

        currentPosition = clickedPosition;
        transform.position = gridManager.GetWorldPosition(currentPosition);
        isSelected = false;
    }

    private void CalculateMoveRange()
    {
        reachablePositions.Clear();

        Queue<(Vector2Int position, int distance)> queue =
            new Queue<(Vector2Int, int)>();

        queue.Enqueue((currentPosition, 0));
        reachablePositions.Add(currentPosition);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.distance >= unitStats.EffectiveMoveRange)
                continue;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int nextPosition = current.position + direction;
                if (!gridManager.IsInsideGrid(nextPosition) ||
                    gridManager.IsCellOccupied(nextPosition) ||
                    reachablePositions.Contains(nextPosition))
                    continue;

                reachablePositions.Add(nextPosition);
                queue.Enqueue((nextPosition, current.distance + 1));
            }
        }
    }

}
