using strange.extensions.mediation.impl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ctac
{
    public class HistoryTileView : View, IPointerEnterHandler, IPointerExitHandler
    {
        private bool isOver = false;

        public HistoryItem item { get; set; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isOver = false;
        }

        private void CreateHoverCard()
        {

        }
    }
}

