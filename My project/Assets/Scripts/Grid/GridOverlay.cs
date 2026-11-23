using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridManager))]
public class GridOverlay : MonoBehaviour
{
    public GridManager gridManager;
    public Color lineColor = new Color(0.65f, 0.7f, 0.75f, 0.8f);
    public float lineWidth = 0.025f;
    public Color moveHighlightColor = new Color(0.1f, 0.4f, 1f, 0.35f);
    public Color attackHighlightColor = new Color(1f, 0.1f, 0.1f, 0.45f);

    private Material lineMaterial;
    private Texture2D highlightTexture;
    private Sprite highlightSprite;
    private readonly Dictionary<Vector2Int, SpriteRenderer> highlightTiles =
        new Dictionary<Vector2Int, SpriteRenderer>();

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
        highlightTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        highlightTexture.name = "Runtime Grid Highlight Texture";
        highlightTexture.SetPixel(0, 0, Color.white);
        highlightTexture.Apply();
        highlightTexture.filterMode = FilterMode.Point;
        highlightSprite = Sprite.Create(highlightTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        highlightSprite.name = "Runtime Grid Highlight Sprite";
        CreateGridLines();
    }

    public void SetHighlights(IEnumerable<Vector2Int> moveCells, IEnumerable<Vector2Int> attackCells)
    {
        HashSet<Vector2Int> activeCells = new HashSet<Vector2Int>();

        foreach (Vector2Int cell in moveCells)
        {
            if (gridManager.IsInsideGrid(cell))
            {
                activeCells.Add(cell);
                SetHighlight(cell, moveHighlightColor);
            }
        }

        foreach (Vector2Int cell in attackCells)
        {
            if (gridManager.IsInsideGrid(cell))
            {
                activeCells.Add(cell);
                SetHighlight(cell, attackHighlightColor);
            }
        }

        foreach (KeyValuePair<Vector2Int, SpriteRenderer> tile in highlightTiles)
            tile.Value.gameObject.SetActive(activeCells.Contains(tile.Key));
    }

    private void SetHighlight(Vector2Int cell, Color color)
    {
        if (!highlightTiles.TryGetValue(cell, out SpriteRenderer tile))
        {
            GameObject tileObject = new GameObject($"Grid Highlight {cell.x},{cell.y}");
            tileObject.transform.SetParent(transform, false);
            tileObject.transform.position = gridManager.GetWorldPosition(cell);
            tileObject.transform.localScale = Vector3.one * gridManager.cellSize;

            tile = tileObject.AddComponent<SpriteRenderer>();
            tile.sprite = highlightSprite;
            tile.sortingOrder = -2;
            highlightTiles.Add(cell, tile);
        }

        tile.color = color;
        tile.gameObject.SetActive(true);
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
            DestroyRuntimeObject(lineMaterial);
        if (highlightSprite != null)
            DestroyRuntimeObject(highlightSprite);
        if (highlightTexture != null)
            DestroyRuntimeObject(highlightTexture);
    }

    private void DestroyRuntimeObject(Object runtimeObject)
    {
        if (Application.isPlaying)
            Destroy(runtimeObject);
        else
            DestroyImmediate(runtimeObject);
    }
}
