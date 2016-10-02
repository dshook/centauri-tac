using strange.extensions.mediation.impl;
using System.Collections.Generic;
using UnityEngine;
using SVGImporter;
using UnityEngine.UI;
using strange.extensions.signal.impl;
using ctac.signals;

namespace ctac
{
    public class HistoryView : View
    {
        [Inject]
        public IResourceLoaderService loader { get; set; }

        [Inject] public GameTurnModel turns { get; set; }
        [Inject] public GamePlayersModel players { get; set; }


        private GameObject historyPanel;
        private GameObject historyTilePrefab;

        private float panelTop = 333f;

        private const int maxItems = 9;
        private Dictionary<HistoryItemType, SVGAsset> iconMap;
        private Vector3 scaleOutVec = new Vector3(1, 0, 1);
        private Canvas canvas { get; set; }

        private List<HistoryIcon> icons = new List<HistoryIcon>();
        private class HistoryIcon
        {
            public GameObject historyTile { get; set; }
            public SVGImage borderSvg { get; set; }
        }

        internal void init()
        {
            historyPanel = this.gameObject;
            historyTilePrefab = Resources.Load("UI/HistoryTile") as GameObject;
            canvas = GameObject.Find("Canvas").gameObject.GetComponent<Canvas>();

            iconMap = new Dictionary<HistoryItemType, SVGAsset>()
            {
                { HistoryItemType.Attack, loader.Load<SVGAsset>("UI/history icon attack")},
                { HistoryItemType.CardPlayed, loader.Load<SVGAsset>("UI/history icon card")},
                { HistoryItemType.Event, loader.Load<SVGAsset>("UI/history icon event")},
                { HistoryItemType.MinionPlayed, loader.Load<SVGAsset>("UI/history icon minion card")},
            };

            historyPanel.transform.DestroyChildren();
        }

        public void AddItem(HistoryItem item)
        {

            //pop in new button at top
            var newTile = GameObject.Instantiate(historyTilePrefab);
            newTile.transform.SetParent(historyPanel.transform, false);
            var buttonRect = newTile.GetComponent<RectTransform>();
            buttonRect.anchoredPosition3D = new Vector3(18.5f, panelTop, 0);
            var view = newTile.GetComponent<HistoryTileView>();
            view.item = item;
            //view.HistoryHoverSignal = HistoryHoverSignal;
            float buttonHeight = buttonRect.sizeDelta.y * 1.3f * canvas.scaleFactor; 
            newTile.transform.localScale = Vector3.zero;
            iTweenExtensions.ScaleTo(newTile, Vector3.one, 1f, 0f, EaseType.easeInOutBounce);

            //animate everything down
            foreach (var icon in icons)
            {
                iTweenExtensions.MoveToLocal(
                    icon.historyTile,
                    icon.historyTile.transform.localPosition.AddY(-buttonHeight),
                    1f,
                    0f
                );
            }

            var borderGo = newTile.transform.FindChild("Border").gameObject;
            var cardGo = newTile.transform.FindChild("Card").gameObject;

            var borderSvg = borderGo.GetComponent<SVGImage>();
            var cardSvg = cardGo.GetComponent<SVGImage>();

            cardSvg.vectorGraphics = iconMap[item.type];
            borderSvg.color = item.initiatingPlayerId != players.OpponentId(turns.currentPlayerId) ? Colors.friendlyColor : Colors.enemyColor;

            icons.Add(new HistoryIcon()
            {
                historyTile = newTile,
                borderSvg = borderSvg
            });

            //check for popping the end off
            if (icons.Count > maxItems)
            {
                var toRemove = icons[0];
                iTweenExtensions.ScaleTo(toRemove.historyTile, scaleOutVec, 1f, 0f, EaseType.easeInCirc);
                Destroy(toRemove.historyTile, 1f);
                icons.Remove(toRemove);
            }
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

