using UnityEngine;

namespace Units
{
    public interface IBattler
    {
        public enum EActionType
        {
            Attack
        }
        
        bool IsMyTurn { get; set; }
        bool EndedTurn { get; set; }
        GameObject CurrentTurnHighlight { get; }
        
        public void HighlightForCurrentTurn();
        public void ResetHighlightForCurrentTurn();
        public void HighlightForHover();
        public void ResetHighlightForHover();
        public void DoAction();
    }
}