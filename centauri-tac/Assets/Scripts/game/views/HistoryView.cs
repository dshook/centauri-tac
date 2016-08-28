using strange.extensions.mediation.impl;
using System.Collections.Generic;
using UnityEngine;
using SVGImporter;

namespace ctac
{
    public class HistoryView : View
    {
        [Inject]
        public IResourceLoaderService loader { get; set; }

        [Inject]
        public GameTurnModel turns { get; set; }

        private GameObject historyPanel;
        private GameObject historyTilePrefab;

        private List<AbilityTarget> abilities;

        private float panelTop = 333f;
        private float buttonHeight = 33f;

        private const int maxItems = 9;
        private Dictionary<HistoryMediator.HistoryItemType, SVGAsset> iconMap;

        private List<HistoryIcon> icons = new List<HistoryIcon>();
        private class HistoryIcon
        {
            public GameObject historyTile { get; set; }
            public RectTransform rectT { get; set; }
            public GameObject border { get; set; }
            public GameObject card { get; set; }
            public SVGImage borderSvg { get; set; }
            public SVGImage cardSvg { get; set; }
        }

        internal void init()
        {
            historyPanel = this.gameObject;
            historyTilePrefab = Resources.Load("UI/HistoryTile") as GameObject;

            iconMap = new Dictionary<HistoryMediator.HistoryItemType, SVGAsset>()
            {
                { HistoryMediator.HistoryItemType.Attack, loader.Load<SVGAsset>("UI/history icon attack")},
                { HistoryMediator.HistoryItemType.CardPlayed, loader.Load<SVGAsset>("UI/history icon card")},
                { HistoryMediator.HistoryItemType.Event, loader.Load<SVGAsset>("UI/history icon event")},
                { HistoryMediator.HistoryItemType.MinionPlayed, loader.Load<SVGAsset>("UI/history icon minion card")},
            };

            historyPanel.transform.DestroyChildren();
        }

        public void AddItem(HistoryMediator.HistoryItem item)
        {
            //animate everything down
            foreach (var icon in icons)
            {
                iTweenExtensions.MoveTo(
                    icon.historyTile,
                    icon.historyTile.transform.position.SetY(icon.historyTile.transform.position.y - buttonHeight),
                    1f,
                    0f
                );
            }

            //check for popping the end off
            if (icons.Count > maxItems)
            {
                var toRemove = icons[0];
                iTweenExtensions.ScaleTo(toRemove.card, Vector3.zero, 1f, 0f, EaseType.easeOutQuad);
                Destroy(toRemove.card, 1f);
                icons.Remove(toRemove);
            }

            //pop in new button at top
            var newButton = GameObject.Instantiate(historyTilePrefab);
            newButton.transform.SetParent(historyPanel.transform, false);
            var buttonRect = newButton.GetComponent<RectTransform>();
            buttonRect.anchoredPosition3D = new Vector3(18.5f, panelTop, 0);

            newButton.transform.localScale = Vector3.zero;
            iTweenExtensions.ScaleTo(newButton, Vector3.one, 1f, 0f, EaseType.easeInOutBounce);

            var borderGo = newButton.transform.FindChild("Border").gameObject;
            var cardGo = newButton.transform.FindChild("Card").gameObject;

            var borderSvg = borderGo.GetComponent<SVGImage>();
            var cardSvg = cardGo.GetComponent<SVGImage>();

            cardSvg.vectorGraphics = iconMap[item.type];
            borderSvg.color = item.initiatingPlayerId == turns.currentPlayerId ? Colors.friendlyColor : Colors.enemyColor;

            icons.Add(new HistoryIcon()
            {
                historyTile = newButton,
                rectT = buttonRect,
                border = borderGo,
                borderSvg = borderSvg,
                card = cardGo,
                cardSvg = cardSvg
            });
        }

        //Swip swap all the colors
        public void UpdatePlayerColors()
        {
            foreach (var icon in icons)
            {
                icon.borderSvg.color = icon.borderSvg.color == Colors.friendlyColor ? Colors.enemyColor : Colors.friendlyColor;
            }
        }


    }
}

