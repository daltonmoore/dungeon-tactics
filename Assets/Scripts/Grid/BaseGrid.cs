using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Grid
{
    public class BaseGrid : MonoBehaviour
    {
        [field: SerializeField] public GridConfig gridConfig { get; private set; }
        [SerializeField] private GameObject gridCellPrefab;

        public void SetupGrid()
        {
            for (int x = 0; x < gridConfig.GridSize.x; x++)
            {
                for (int y = 0; y < gridConfig.GridSize.y; y++)
                {
                    GameObject square = Instantiate(gridCellPrefab, transform);
                    square.name = $"Grid Cell ({x}, {y})";
                    square.transform.position = new Vector3(x, y, -1);
                    // square.transform.localScale = new Vector3(.75f, .75f, .75f);
                    square.AddComponent<GridCell>().Configure(gridConfig);
                }
            }
        }
        
        public void ClearGrid()
        {
            while (transform.childCount > 0) {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
    }
}
