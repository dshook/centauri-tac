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

        private int itemCount = 0;
        private Dictionary<HistoryMediator.HistoryItemType, SVGAsset> iconMap;

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
            itemCount++;
            //animate everything down
            for (int t = 0; t < historyPanel.transform.childCount; t++)
            {
                var tile = historyPanel.transform.GetChild(t).gameObject;
                iTweenExtensions.MoveTo(
                    tile,
                    tile.transform.position.SetY(tile.transform.position.y - buttonHeight),
                    1f,
                    0f
                );
            }

            //pop in new button at top
            var newButton = GameObject.Instantiate(historyTilePrefab);
            newButton.transform.SetParent(historyPanel.transform, false);
            var buttonRect = newButton.GetComponent<RectTransform>();
            buttonRect.anchoredPosition3D = new Vector3(18.5f, panelTop, 0);

            var borderGo = newButton.transform.FindChild("Border").gameObject;
            var cardGo = newButton.transform.FindChild("Card").gameObject;

            var borderSvg = borderGo.GetComponent<SVGImage>();
            var cardSvg = cardGo.GetComponent<SVGImage>();

            cardSvg.vectorGraphics = iconMap[item.type];
            borderSvg.color = item.initiatingPlayerId == turns.currentPlayerId ? Colors.friendlyColor : Colors.enemyColor;
        }

    }
}

