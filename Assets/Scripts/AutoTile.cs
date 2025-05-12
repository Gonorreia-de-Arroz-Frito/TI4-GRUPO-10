using UnityEngine;
using UnityEngine.Tilemaps;

public class AutoTile : MonoBehaviour
{
    public Tilemap tilemap;

    [Tooltip("Tiles organizados de acordo com a máscara de vizinhança (ordem: 0 a 15)")]
    public TileBase[] tileVariants; // 16 possíveis combinações

    private void Start()
    {
        UpdateAllTiles();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            UpdateAllTiles();
        }
    }

    void UpdateAllTiles()
    {
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);

                if (!tilemap.HasTile(position)) continue;

                int mask = GetNeighborMask(position);

                // 
                if (mask >= 0 && mask < tileVariants.Length)
                {
                    tilemap.SetTile(position, tileVariants[mask]);
                }
                else
                {
                    Debug.LogWarning($"Máscara {mask} fora do limite do array em {position}");
                }
            }
        }
    }

    // Bitmask: 0bNESW (N = bit 3, E = bit 2, S = bit 1, W = bit 0)
    int GetNeighborMask(Vector3Int position)
    {
        int mask = 0;
        if (tilemap.HasTile(position + Vector3Int.up)) mask |= 1 << 3;     // North
        if (tilemap.HasTile(position + Vector3Int.right)) mask |= 1 << 2;  // East
        if (tilemap.HasTile(position + Vector3Int.down)) mask |= 1 << 1;   // South
        if (tilemap.HasTile(position + Vector3Int.left)) mask |= 1 << 0;   // West
        return mask;
    }
}
