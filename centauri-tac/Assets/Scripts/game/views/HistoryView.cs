using strange.extensions.mediation.impl;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ctac
{
    public class HistoryView : View
    {
        private GameObject historyPanel;
        private GameObject historyTilePrefab;

        private List<AbilityTarget> abilities;

        private float buttonHeight = 34f;
        private float buttonMargin = 2.5f;
        private float panelHeightPerButton = 36.5f;

        internal void init()
        {
            historyPanel = this.gameObject;
            historyTilePrefab = Resources.Load("UI/HistoryTile") as GameObject;

            historyPanel.transform.DestroyChildren();
        }

        public void UpdateAbilities(int playerId, List<AbilityTarget> newAbilities)
        {
            abilities = newAbilities;
            historyPanel.transform.DestroyChildren();
            BuildPanel();
        }

        private void BuildPanel()
        {
            var abilityRect = historyPanel.GetComponent<RectTransform>();
            abilityRect.sizeDelta = new Vector2(abilityRect.sizeDelta.x, (panelHeightPerButton * abilities.Count) + buttonMargin);

            var index = 0;
            foreach (var ability in abilities)
            {
                var newButton = GameObject.Instantiate(historyTilePrefab);
                newButton.transform.SetParent(historyPanel.transform, false);

                var buttonRect = newButton.GetComponent<RectTransform>();
                var abilityButtonView = newButton.GetComponent<AbilityButtonView>();

                abilityButtonView.ability = ability;
                buttonRect.anchorMin = Vector2.up;
                buttonRect.anchorMax = Vector2.up;
                buttonRect.pivot = Vector2.up;

                var vertPosition = -((buttonHeight * index) + buttonMargin);
                buttonRect.anchoredPosition3D = new Vector3(14, vertPosition, 0);

                index++;
            }
        }

    }
}

