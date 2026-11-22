using UnityEngine;

[RequireComponent(typeof(GridManager))]
public class GridOverlay : MonoBehaviour
{
    public GridManager gridManager;
    public Color lineColor = new Color(0.65f, 0.7f, 0.75f, 0.8f);
    public float lineWidth = 0.025f;

    private Material lineMaterial;

    private void Awake()
    {
        if (gridManager == null)
            gridManager = GetComponent<GridManager>();

        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogError("Could not find a shader for rendering the grid overlay.", this);
            enabled = false;
            return;
        }

        lineMaterial = new Material(shader);
        lineMaterial.name = "Runtime Grid Line Material";
        CreateGridLines();
    }

    private void CreateGridLines()
    {
        float cellSize = gridManager.cellSize;
        float left = -cellSize / 2f;
        float bottom = -cellSize / 2f;
        float right = (gridManager.width - 0.5f) * cellSize;
        float top = (gridManager.height - 0.5f) * cellSize;

        for (int x = 0; x <= gridManager.width; x++)
        {
            float worldX = x * cellSize - cellSize / 2f;
            CreateLine($"Grid Vertical Line {x}", new Vector3(worldX, bottom, 0.1f), new Vector3(worldX, top, 0.1f));
        }

        for (int y = 0; y <= gridManager.height; y++)
        {
            float worldY = y * cellSize - cellSize / 2f;
            CreateLine($"Grid Horizontal Line {y}", new Vector3(left, worldY, 0.1f), new Vector3(right, worldY, 0.1f));
        }
    }

    private void CreateLine(string lineName, Vector3 start, Vector3 end)
    {
        GameObject lineObject = new GameObject(lineName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.sharedMaterial = lineMaterial;
        line.sortingOrder = -1;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(lineMaterial);
            else
                DestroyImmediate(lineMaterial);
        }
    }
}
