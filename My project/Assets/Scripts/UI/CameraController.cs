using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public GridManager gridManager;
    public float moveSpeed = 5f;

    private Camera cameraComponent;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();

        if (gridManager == null)
            Debug.LogError("CameraController에 GridManager를 연결해야 합니다.", this);
    }

    private void Update()
    {
        if (gridManager == null || Keyboard.current == null)
            return;

        float x = 0f;
        float y = 0f;
        Keyboard keyboard = Keyboard.current;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x--;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x++;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y--;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y++;

        Vector3 movement = new Vector3(x, y, 0f).normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;

        KeepCameraInsideGrid();
    }

    private void KeepCameraInsideGrid()
    {
        float cellSize = gridManager.cellSize;
        float minX = -cellSize / 2f;
        float maxX = (gridManager.width - 0.5f) * cellSize;
        float minY = -cellSize / 2f;
        float maxY = (gridManager.height - 0.5f) * cellSize;

        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        transform.position = position;
    }
}