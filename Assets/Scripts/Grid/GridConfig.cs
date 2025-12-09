using UnityEngine;

namespace Grid
{
    [CreateAssetMenu(fileName = "Grid Config", menuName = "Grid/Grid Config", order = 0)]
    public class GridConfig : ScriptableObject
    {
        [field: SerializeField] public Vector2Int GridSize { get; private set; } = new Vector2Int(10, 10);
        [field: SerializeField] public float DefaultScale { get; private set; } = 1f;
        [field: SerializeField] public float HighlightScale { get; private set; } = 10f;
        [field: SerializeField] public float HighlightScaleLerpDuration { get; private set; } = .25f;
    }
}