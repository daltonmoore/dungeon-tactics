using System;
using _.Dalton.Utils;
using UnityEngine;
using UnityEngine.Serialization;

public class TestingHexGrid : MonoBehaviour
{
    [FormerlySerializedAs("pfSquare")] [SerializeField] private Transform pfHex;
    
    private GridHex<GridObject> _gridHex;
    private GridObject _lastGridObject;

    private class GridObject
    {
        private int x;
        private int y;
        public Transform VisualTransform;

        public GridObject(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void Show()
        {
            VisualTransform.Find("Selected").gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            VisualTransform.Find("Selected").gameObject.SetActive(false);
        }
        
        public override string ToString()
        {
            return x + ", " + y; // + "\n" + placedObject?.transform;
        }
    }

    private void Awake()
    {
        int width = 10;
        int height = 6;
        float cellSize = 1f;
        Vector3 originPosition = new Vector3(0, 0, 0);
        _gridHex = new GridHex<GridObject>(width, height, cellSize, originPosition, (g, x, y) => new GridObject(x, y));

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Transform visualTransform = Instantiate(pfHex, _gridHex.GetWorldPosition(x, z), Quaternion.identity);
                _gridHex.GetGridObject(x, z).VisualTransform = visualTransform;
                _gridHex.GetGridObject(x, z).Hide();
            }
        }
    }

    private void Update()
    {
        if (_lastGridObject != null)
        {
            _lastGridObject.Hide();
        }
        
        _lastGridObject = _gridHex.GetGridObject(Utils.GetMouseWorldPosition());
        if (_lastGridObject != null)
        {
            _lastGridObject.Show();
        }
    }
}
