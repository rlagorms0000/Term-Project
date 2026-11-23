using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UnitStats), typeof(UnitManagementUI))]
public class PlayerController : MonoBehaviour
{
    public GridManager gridManager;
    public TurnManager TurnManager;

    private Vector2Int currentPosition;
    private bool isSelected = false;
    private UnitStats unitStats;
    private UnitManagementUI unitManagementUI;
    private GridOverlay gridOverlay;
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

        if (TurnManager == null)
            TurnManager = FindFirstObjectByType<global::TurnManager>();

        if (TurnManager == null)
        {
            Debug.LogError("PlayerController requires a TurnManager.", this);
            enabled = false;
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("PlayerController requires a GridManager.", this);
            enabled = false;
            return;
        }

        gridOverlay = gridManager.GetComponent<GridOverlay>();
        if (gridOverlay == null)
            Debug.LogWarning("PlayerController could not find a GridOverlay; range highlights will be hidden.", this);

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
        if (TurnManager == null || !TurnManager.PlayerTurn)
            return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
            !unitManagementUI.IsPointerOverWindow())
            HandleMouseClick();
    }

    private void HandleMouseClick()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("PlayerController requires a Main Camera.", this);
            return;
        }

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0;

        Vector2Int clickedPosition = new Vector2Int(
            Mathf.RoundToInt(mousePosition.x / gridManager.cellSize),
            Mathf.RoundToInt(mousePosition.y / gridManager.cellSize)
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

        if (gridManager.TryGetOccupant(clickedPosition, out GameObject occupant))
        {
            UnitStats targetStats = occupant.GetComponent<UnitStats>();
            if (targetStats != null && targetStats.team == Team.Enemy)
                TryAttack(targetStats, clickedPosition);
            else
                Debug.Log("해당 칸은 이미 점유되어 있습니다.");
            return;
        }

        if (unitStats.hasMove)
        {
            Debug.Log("이번 턴에는 이미 이동했습니다.");
            return;
        }

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
        unitStats.hasMove = true;
        CalculateMoveRange();
    }

    private void TryAttack(UnitStats target, Vector2Int targetPosition)
    {
        if (unitStats.hasActed)
        {
            Debug.Log("이번 턴에는 이미 공격했습니다.");
            return;
        }

        int distance = Mathf.Abs(targetPosition.x - currentPosition.x) +
                       Mathf.Abs(targetPosition.y - currentPosition.y);
        if (distance == 0 || distance > unitStats.attackRange)
        {
            Debug.Log("적이 공격 범위 밖에 있습니다.");
            return;
        }

        if (CombetSystem.Instance == null)
        {
            Debug.LogError("PlayerController requires a CombetSystem.", this);
            return;
        }

        if (CombetSystem.Instance.ResolveCombat(unitStats, target))
        {
            unitStats.hasActed = true;
            CalculateMoveRange();
        }
    }

    private void CalculateMoveRange()
    {
        reachablePositions.Clear();

        if (unitStats.hasMove)
        {
            UpdateHighlights();
            return;
        }

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

        reachablePositions.Remove(currentPosition);
        UpdateHighlights();
    }

    private void UpdateHighlights()
    {
        if (gridOverlay == null)
            return;

        HashSet<Vector2Int> attackRangeCells = new HashSet<Vector2Int>();
        if (!unitStats.hasActed)
        {
            HashSet<Vector2Int> attackOrigins = new HashSet<Vector2Int>(reachablePositions);
            attackOrigins.Add(currentPosition);

            foreach (Vector2Int attackOrigin in attackOrigins)
            {
                for (int x = attackOrigin.x - unitStats.attackRange;
                     x <= attackOrigin.x + unitStats.attackRange;
                     x++)
                {
                    for (int y = attackOrigin.y - unitStats.attackRange;
                         y <= attackOrigin.y + unitStats.attackRange;
                         y++)
                    {
                        Vector2Int cell = new Vector2Int(x, y);
                        int distance = Mathf.Abs(x - attackOrigin.x) + Mathf.Abs(y - attackOrigin.y);
                        if (distance == 0 || distance > unitStats.attackRange ||
                            !gridManager.IsInsideGrid(cell) || reachablePositions.Contains(cell))
                            continue;

                        if (gridManager.TryGetOccupant(cell, out GameObject occupant))
                        {
                            UnitStats target = occupant.GetComponent<UnitStats>();
                            if (target == null || target.team == unitStats.team || target.currentHealth <= 0)
                                continue;
                        }

                        attackRangeCells.Add(cell);
                    }
                }
            }
        }

        gridOverlay.SetHighlights(reachablePositions, attackRangeCells);
    }

}
