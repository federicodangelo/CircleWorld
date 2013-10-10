﻿using UnityEngine;
using System.Collections;

public class Avatar : MonoBehaviour 
{
    public TilemapCircle tileMapCircle;

    public float jumpSpeed = 7.0f;
    public float speed = 3.0f;
    public float gravity = 10.0f;
    public float width = 1;
    public float height = 2;

    private float velocityY;

	// Use this for initialization
	void Start () 
    {
        //Snap to floor on start
        TileHitInfo hitInfo = tileMapCircle.GetHitInfo(transform.position);

        transform.position -= hitInfo.originNormal * (hitInfo.hitDistance - hitInfo.scale);
        transform.up = hitInfo.originNormal;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetMouseButton(0))
        {
            int tileX, tileY;
            if (GetTileCoordinateUnderMouse(out tileX, out tileY))
                tileMapCircle.SetTile(tileX, tileY, 0);
        }

        Vector3 position = transform.position;

        float scale = tileMapCircle.GetScaleFromPosition(position);
        Vector3 normal = tileMapCircle.GetNormalFromPosition(position); //doesn't change with vertical position
        Vector3 tangent = tileMapCircle.GetTangentFromPosition(position); //doesn't change with vertical position

        velocityY -= gravity * Time.deltaTime;

        float deltaY = velocityY * Time.deltaTime * scale;
        float deltaX = Input.GetAxis("Horizontal") * speed * Time.deltaTime * scale;

		TileHitInfo hitInfo;

        if (deltaY > 0)
        {
            //Check against ceiling
            if (tileMapCircle.Raycast(
                position + normal * (height * 0.5f * scale), 
                Vector3.up, 
                deltaY + (height * 0.5f * scale), 
                out hitInfo))
            {
                deltaY = -(hitInfo.hitDistance - (height * 0.5f * scale));

                velocityY = 0.0f;
            }
        }
        else if (deltaY < 0)
        {
            //Check against floor
            if (tileMapCircle.Raycast(
                position + normal * (height * 0.5f * scale), 
                Vector3.down, 
                -deltaY + (height * 0.5f * scale), 
                out hitInfo))
            {
                deltaY = -(hitInfo.hitDistance - (height * 0.5f * scale));

                velocityY = 0.0f;

                if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
                    velocityY = jumpSpeed;
            }
        }

        if (deltaY != 0)
        {
            position += normal * deltaY;
            scale = tileMapCircle.GetScaleFromPosition(position);
        }

        if (deltaX > 0)
        {
            //Check against right wall
            if (tileMapCircle.Raycast(
                position + normal * (height * 0.5f * scale), 
                Vector3.right, 
                deltaX + (width * 0.5f * scale), 
                out hitInfo))
            {
                deltaX = (hitInfo.hitDistance - (width * 0.5f * scale));
            }
        }
        else if (deltaX < 0)
        {
            //Check against left wall
            if (tileMapCircle.Raycast(
                position + normal * (height * 0.5f * scale), 
                Vector3.left, 
                -deltaX + (width * 0.5f * scale), 
                out hitInfo))
            {
                deltaX = -(hitInfo.hitDistance - (width * 0.5f * scale));
            }
        }

        if (deltaX != 0)
        {
            position += tangent * deltaX;
            normal = tileMapCircle.GetNormalFromPosition(position);
        }

        transform.position = position;
        transform.localScale = Vector3.one * scale;
        transform.up = normal;

        UpdateCamera(normal, scale);
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

    private bool GetTileCoordinateUnderMouse(out int tileX, out int tileY)
    {
        Camera cam = Camera.main;

        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        return tileMapCircle.GetTileCoordinatesFromPosition(worldPos, out tileX, out tileY);
    }

    private void UpdateCamera(Vector3 normal, float scale)
    {
        Camera cam = Camera.main;

        cam.transform.position = transform.position - Vector3.forward * 10.0f;
        cam.transform.up = normal;
        cam.orthographicSize = 10 * scale;
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
                    right * (width * 0.9f * x * 0.5f * scale) +
                        up * ((height * 0.9f / 2) * y * scale);

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
            float scale = tileMapCircle.GetScaleFromPosition(transform.position);
            Vector3 normal = tileMapCircle.GetNormalFromPosition(transform.position); //doesn't change with vertical position
            Vector3 tangent = tileMapCircle.GetTangentFromPosition(transform.position); //doesn't change with vertical position

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + normal * height * scale);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + tangent * width * 0.5f * scale);
        }
    }
}

