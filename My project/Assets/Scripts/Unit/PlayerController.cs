using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class PlayerController : MonoBehaviour
{
    public GridManager gridManager;

    private Vector2Int currentPosition;
    private bool isSelected = false;

    void Start()
    {
        // 현재 위치를 격자 좌표로 초기화
        currentPosition = new Vector2Int(2, 2);

        unitStats = GetComponent<UnitStats>();

        transform.position =
            gridManager.GetWorldPosition(currentPosition);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    void HandleMouseClick()
    {
        Vector3 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mousePosition.z = 0;

        // 클릭한 위치를 격자 좌표로 변환
        Vector2Int clickedPosition = new Vector2Int(
            Mathf.RoundToInt(mousePosition.x),
            Mathf.RoundToInt(mousePosition.y)
        );

        // 플레이어 선택
        if (clickedPosition == currentPosition)
        {
            isSelected = true;
            CalculateMoveRange();
            Debug.Log("플레이어 선택!");
            return;
        }

        if (!isSelected)
            return;

        // 현재 위치와 클릭 위치의 거리 계산
        int distance =
            Mathf.Abs(clickedPosition.x - currentPosition.x) +
            Mathf.Abs(clickedPosition.y - currentPosition.y);

        if (!reachablePositions.Contains(clickedPosition))
        {
            Debug.Log("이동할 수 없는 위치입니다.");
            return;
        }

        // 위치 갱신
        currentPosition = clickedPosition;

        transform.position =
            gridManager.GetWorldPosition(currentPosition);

        isSelected = false;
    }

    private UnitStats unitStats;

    private HashSet<Vector2Int> reachablePositions = new HashSet<Vector2Int>();

    private Vector2Int[] diraction =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

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

            if(current.distance >= unitStats.moveRange)
                continue;

            foreach(Vector2Int direction in diraction)
            {
                Vector2Int nextPosition = current.position + direction;
                if (!gridManager.IsInsideGrid(nextPosition))
                    continue;
                if (reachablePositions.Contains(nextPosition))
                    continue;
                reachablePositions.Add(nextPosition);
                queue.Enqueue((nextPosition, current.distance + 1));
            }
        }
    }



}
