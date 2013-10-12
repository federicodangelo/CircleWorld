using UnityEngine;
using System.Collections;

public class TilemapObject : MonoBehaviour 
{
    public TilemapCircle tileMapCircle;

    public bool useGravity = true;

    public float gravity = 10.0f;
    public Vector2 size = new Vector2(1, 1);
    public Vector2 velocity;

    public TileHitFlags hitFlags;

    public virtual void Start () 
    {
        velocity = Vector2.zero;
    }

    public virtual void Update ()
    {
        UpdatePosition();
    }

    protected void UpdatePosition()
    {
        Vector3 position = transform.position;

        float scale = tileMapCircle.GetScaleFromPosition(position);
        Vector3 normal = tileMapCircle.GetNormalFromPosition(position); //doesn't change with vertical position
        Vector3 tangent = tileMapCircle.GetTangentFromPosition(position); //doesn't change with vertical position

        if (useGravity)
            velocity.y -= gravity * Time.deltaTime;

        Vector2 delta = velocity * Time.deltaTime * scale;

        TileHitInfo hitInfo;

        hitFlags = TileHitFlags.None;

        if (delta.y > 0)
        {
            //Check against ceiling
            if (tileMapCircle.RaycastSquare(
                position + normal * (size.y * 0.5f * scale), 
                size.x * scale,
                TileDirection.Up, 
                delta.y + (size.y * 0.5f * scale), 
                out hitInfo))
            {
                delta.y = -(hitInfo.hitDistance - (size.y * 0.5f * scale));
                velocity.y = 0.0f;
                hitFlags |= TileHitFlags.Up;
            }
        }
        else if (delta.y < 0)
        {
            //Check against floor
            if (tileMapCircle.RaycastSquare(
                position + normal * (size.y * 0.5f * scale), 
                size.x * scale,
                TileDirection.Down, 
                -delta.y + (size.y * 0.5f * scale), 
                out hitInfo))
            {
                delta.y = -(hitInfo.hitDistance - (size.y * 0.5f * scale));
                velocity.y = 0.0f;
                hitFlags |= TileHitFlags.Down;
            }
        }

        if (delta.y != 0)
        {
            position += normal * delta.y;
            scale = tileMapCircle.GetScaleFromPosition(position);
        }

        if (delta.x > 0)
        {
            //Check against right wall
            if (tileMapCircle.RaycastSquare(
                position + normal * (size.y * 0.5f * scale), 
                size.y * scale,
                TileDirection.Right, 
                delta.x + (size.x * 0.5f * scale), 
                out hitInfo))
            {
                delta.x = (hitInfo.hitDistance - (size.x * 0.5f * scale));
                velocity.x = 0.0f;
                hitFlags |= TileHitFlags.Right;
            }
        }
        else if (delta.x < 0)
        {
            //Check against left wall
            if (tileMapCircle.RaycastSquare(
                position + normal * (size.y * 0.5f * scale), 
                size.y * scale,
                TileDirection.Left, 
                -delta.x + (size.x * 0.5f * scale), 
                out hitInfo))
            {
                delta.x = -(hitInfo.hitDistance - (size.x * 0.5f * scale));
                velocity.x = 0.0f;
                hitFlags |= TileHitFlags.Left;
            }
        }

        if (delta.x != 0)
        {
            position += tangent * delta.x;
            normal = tileMapCircle.GetNormalFromPosition(position);
        }

        transform.position = position;
        transform.localScale = Vector3.one * scale;
        transform.rotation = Quaternion.AngleAxis(-tileMapCircle.GetAngleFromPosition(position), Vector3.forward);
    }

    public bool MoveTo(Vector3 position)
    {
        if (CanMoveTo(position))
        {
            transform.position = position;
            return true;
        }

        return false;
    }

    public bool CanMoveTo(Vector3 position)
    {
        float scale = tileMapCircle.GetScaleFromPosition(position);

        int tileX, tileY;

        Vector3 right = transform.right;
        Vector3 up = transform.up;

        position += up * 0.05f;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = 0; y <= 2; y++)
            {
                Vector3 pos = position + 
                    right * (size.x * 0.9f * x * 0.5f * scale) +
                        up * ((size.y * 0.9f / 2) * y * scale);

                if (tileMapCircle.GetTileCoordinatesFromPosition(pos, out tileX, out tileY))
                    if (tileMapCircle.GetTile(tileX, tileY) != 0)
                        return false;
            }
        }

        return true;
    }

    public void OnDrawGizmosSelected()
    {
        OnDrawGizmos();
    }

    public void OnDrawGizmos()
    {
        if (tileMapCircle)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * size.y);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position + transform.up * size.y * 0.5f, transform.position + transform.up * size.y * 0.5f + transform.right * size.x * 0.5f);
        }
    }
}
