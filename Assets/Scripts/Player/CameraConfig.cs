using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class CameraConfig
    {
        // Mouse
        [field: SerializeField, Range(1f, 100f)] public float MouseSensitivity { get; private set; } = 90f;
        [field: SerializeField, Range(1f, 500f)] public float MouseDamping { get; private set; } = 1f;
        
        // Pan
        [field: SerializeField] public float MouseEdgePanSpeed { get; private set; } = 10f;
        [field: SerializeField] public float KeyboardPanSpeed { get; private set; } = 10f;
        [field: SerializeField] public bool EnableEdgePan { get; private set; } = true;
        [field: SerializeField] public float EdgePanSize { get; private set; } = 50f;

        // Drag
        [field: SerializeField] public float DragDamping { get; private set; } = 10f;
        [field: SerializeField] public float DragSpeed { get; private set; } = 10f;
        
        // Zoom
        [field: SerializeField] public float MinZoomDistance { get; private set; } = 7.5f;
        [field: SerializeField] public float MaxZoomDistance { get; private set; } = 17.5f;
        [field: SerializeField] public float ZoomSpeed { get; private set; } = 1f;
        
        
        // Rotation
        [field: SerializeField] public float RotationSpeed { get; private set; } = 1f;
    }
}