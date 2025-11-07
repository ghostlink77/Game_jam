using UnityEngine;

public enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public class Tile : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.5f); // 노란색 반투명

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public Tile GetNearTile(Direction direction)
    {
        Vector3 directionV = Vector3.zero;
        switch(direction)
        {
            case Direction.UP:
                directionV = Vector3.up;
                break;
            case Direction.DOWN:
                directionV = Vector3.down;
                break;
            case Direction.LEFT:
                directionV = Vector3.left;
                break;
            case Direction.RIGHT:
                directionV = Vector3.right;
                break;
        }

        RaycastHit hit;

        if(Physics.Raycast(transform.position, directionV, out hit, 1f, groundLayer))
        {
            return hit.transform.GetComponent<Tile>();
        }
        return null;
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isHighlighted ? highlightColor : originalColor;
        }
    }

    public void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}

