using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class CameraConfig
    {
        // Mouse
        [field: SerializeField] public float MousePanSpeed { get; private set; } = 10f;
        
        // Pan
        [field: SerializeField] public float PanDamping { get; private set; } = 10f;
        [field: SerializeField] public bool EnableEdgePan { get; private set; } = true;
        [field: SerializeField] public float EdgePanSize { get; private set; } = 50f;
        [field: SerializeField] public float KeyboardPanSpeed { get; private set; } = 10f;
        
        // Zoom
        [field: SerializeField] public float MinZoomDistance { get; private set; } = 7.5f;
        [field: SerializeField] public float MaxZoomDistance { get; private set; } = 17.5f;
        [field: SerializeField] public float ZoomSpeed { get; private set; } = 1f;
        
        
        // Rotation
        [field: SerializeField] public float RotationSpeed { get; private set; } = 1f;
    }
}