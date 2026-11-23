
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;

    private readonly Dictionary<Vector2Int, GameObject> occupiedCells =
        new Dictionary<Vector2Int, GameObject>();

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * cellSize,
            gridPosition.y * cellSize,
            0
        );
    }

    public bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 &&
               position.x < width &&
               position.y >= 0 &&
               position.y < height;
    }

    public bool IsCellOccupied(Vector2Int position)
    {
        return TryGetOccupant(position, out _);
    }

    public bool TryGetOccupant(Vector2Int position, out GameObject occupant)
    {
        if (occupiedCells.TryGetValue(position, out occupant))
        {
            if (occupant != null)
                return true;

            occupiedCells.Remove(position);
        }

        occupant = null;
        return false;
    }

    public bool TryOccupyCell(Vector2Int position, GameObject occupant)
    {
        if (!IsInsideGrid(position) || occupant == null)
            return false;

        if (IsCellOccupied(position) && occupiedCells[position] != occupant)
            return false;

        occupiedCells[position] = occupant;
        return true;
    }

    public bool TryMoveOccupant(Vector2Int from, Vector2Int to, GameObject occupant)
    {
        if (occupant == null || !IsInsideGrid(to))
            return false;

        if (IsCellOccupied(to) && occupiedCells[to] != occupant)
            return false;

        occupiedCells[to] = occupant;
        if (from != to && occupiedCells.TryGetValue(from, out GameObject currentOccupant) && currentOccupant == occupant)
            occupiedCells.Remove(from);

        return true;
    }

    public void ReleaseCell(Vector2Int position, GameObject occupant)
    {
        if (occupiedCells.TryGetValue(position, out GameObject currentOccupant) && currentOccupant == occupant)
            occupiedCells.Remove(position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        for (int x = 0; x <= width; x++)
        {
            Vector3 start = new Vector3(
                x * cellSize - cellSize / 2,
                -cellSize / 2,
                0
            );

            Vector3 end = new Vector3(
                x * cellSize - cellSize / 2,
                height * cellSize - cellSize / 2,
                0
            );

            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= height; y++)
        {
            Vector3 start = new Vector3(
                -cellSize / 2,
                y * cellSize - cellSize / 2,
                0
            );

            Vector3 end = new Vector3(
                width * cellSize - cellSize / 2,
                y * cellSize - cellSize / 2,
                0
            );

            Gizmos.DrawLine(start, end);
        }
    }
}
