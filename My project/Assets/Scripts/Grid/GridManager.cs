
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;

    // 격자 좌표를 실제 월드 좌표로 변환
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * cellSize,
            gridPosition.y * cellSize,
            0
        );
    }

    // 격자 좌표가 전장 내부에 있는지 확인
    public bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 &&
               position.x < width &&
               position.y >= 0 &&
               position.y < height;
    }

    // Scene 창에 격자 선 표시
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
