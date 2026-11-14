
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GridManager gridManager;

    private Vector2Int currentPosition;
    private bool isSelected = false;

    void Start()
    {
        // 현재 위치를 격자 좌표로 초기화
        currentPosition = new Vector2Int(2, 2);

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
            Debug.Log("플레이어 선택!");
            return;
        }

        if (!isSelected)
            return;

        // 현재 위치와 클릭 위치의 거리 계산
        int distance =
            Mathf.Abs(clickedPosition.x - currentPosition.x) +
            Mathf.Abs(clickedPosition.y - currentPosition.y);

        // 상하좌우 한 칸만 이동 허용
        if (distance != 1)
            return;

        // 전장 밖으로 이동할 수 없음
        if (!gridManager.IsInsideGrid(clickedPosition))
            return;

        // 위치 갱신
        currentPosition = clickedPosition;

        transform.position =
            gridManager.GetWorldPosition(currentPosition);

        isSelected = false;
    }
}
