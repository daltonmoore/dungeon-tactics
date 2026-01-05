using System;
using Drawing;
using HexGrid;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWar : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log($"{name} collided with {other.gameObject.name} which is on layer {LayerMask.LayerToName(other.gameObject.layer)}");
        
        // if (other.gameObject.layer == LayerMask.NameToLayer("FogOfWar"))
        Vector2 hitPoint = other.contacts[0].point;
        
        using (Draw.WithDuration(2f))
        {
            Draw.ingame.Circle(
                new Vector3(hitPoint.x, hitPoint.y, 0),
                Vector3.forward, 
                PathfindingHex.CellSize / 20,
                Color.red);
                
        }
        Debug.Log($"Hit point is {hitPoint}");
            
       Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Units"))
            Destroy(gameObject);
    }
}
