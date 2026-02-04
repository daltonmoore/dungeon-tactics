using System;
using Drawing;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWar : MonoBehaviour
{
    // Fog of War will only collide with the FogOfWar Layer, so we do not need to check anything here.
    // It will be assumed that the unit will have a collider that is on the FogOfWar layer.

    private void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}
