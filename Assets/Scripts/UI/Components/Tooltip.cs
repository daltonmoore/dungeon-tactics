using TMPro;
using UnityEngine;

namespace UI.Components
{
    public class Tooltip : MonoBehaviour
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] [Range(0, 1)] public float HoverDelay { get; private set; } = 0.5f;
        [SerializeField] private TextMeshProUGUI toolTipText;
        
        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        public void SetText(string text)
        {
            toolTipText.SetText(text);
            Vector2 preferredSize = toolTipText.GetPreferredValues();
            RectTransform.sizeDelta = new Vector2(preferredSize.x + 50, RectTransform.sizeDelta.y);
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}