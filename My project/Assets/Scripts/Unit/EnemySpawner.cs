using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject enemyPrefab;
    public Vector2Int spawnPosition;

    private void Start()
    {
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

        if (gridManager == null)
        {
            Debug.LogError("EnemySpawner requires a GridManager.", this);
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner requires an enemy prefab.", this);
            return;
        }

        if (!gridManager.IsInsideGrid(spawnPosition))
        {
            Debug.LogError($"Enemy spawn position {spawnPosition} is outside the grid.", this);
            return;
        }

        if (gridManager.IsCellOccupied(spawnPosition))
        {
            Debug.LogWarning($"Cannot spawn enemy: grid cell {spawnPosition} is already occupied.", this);
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, gridManager.GetWorldPosition(spawnPosition), Quaternion.identity);
        if (!gridManager.TryOccupyCell(spawnPosition, enemy))
        {
            Destroy(enemy);
            Debug.LogError($"Failed to reserve grid cell {spawnPosition} for the spawned enemy.", this);
        }
    }
}
