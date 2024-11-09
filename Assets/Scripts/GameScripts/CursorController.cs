using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public float centerX = 0f;
    public float centerY = 0f;
    public float minX = -5f;
    public float maxX = 5f; 
    public float minY = -3f;
    public float maxY = 3f; 

    private void Start()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        // Limit position.
        var size = new Vector3(maxX - minX, maxY - minY, 0);
        var oldPosition = mousePosition;
        mousePosition.x = Mathf.Clamp(mousePosition.x, centerX - size.x / 2, centerX + size.x / 2);
        mousePosition.y = Mathf.Clamp(mousePosition.y, centerY - size.y / 2, centerY + size.y / 2);

        if (mousePosition == oldPosition)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }

        transform.position = mousePosition;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Vector3 center = new Vector3(centerX, centerY, 0);
        Gizmos.DrawWireCube(center, new Vector3(maxX - minX, maxY - minY, 0));
    }
}
