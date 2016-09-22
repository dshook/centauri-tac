using strange.extensions.mediation.impl;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class AbilityView : View
    {
        private GameObject abilityPanel;
        private GameObject abilityButtonPrefab;

        private List<AbilityTarget> abilities = new List<AbilityTarget>();

        private float buttonHeight = 34f;
        private float buttonMargin = 2.5f;
        private Vector3 buttonFinalScale = new Vector3(2.65f, 3, 1);

        internal void init()
        {
            abilityPanel = this.gameObject;
            abilityButtonPrefab = Resources.Load("AbilityButton") as GameObject;

            abilityPanel.transform.DestroyChildren();
        }

        public void UpdateAbilities(int playerId, List<AbilityTarget> newAbilities)
        {
            //have to do a diff sort of algorithm... delete ability buttons for pieces no longer there, and create new ones
            var newPieceIds = newAbilities.Select(a => a.pieceId);
            var existingPieceIds = abilities.Select(a => a.pieceId);
            var pieceIdsToDelete = existingPieceIds.Except(newPieceIds).ToList();
            abilities.RemoveAll(a => pieceIdsToDelete.Contains(a.pieceId));

            for (int i = abilityPanel.transform.childCount - 1; i >= 0; i--)
            {
                var child = abilityPanel.transform.GetChild(i).gameObject;
                var abilityButtonView = child.GetComponent<AbilityButtonView>();
                if (pieceIdsToDelete.Contains(abilityButtonView.ability.pieceId))
                {
                    GameObject.Destroy(child);
                }
            }

            var toAdd = newPieceIds.Except(existingPieceIds);
            var toAddAbilities = newAbilities.Where(a => toAdd.Contains(a.pieceId)).ToList();
            AddButtons(toAddAbilities);
            abilities.AddRange(toAddAbilities);

            //need to position buttons next frame so that the destroy goes through first
            Invoke("PositionButtons", 0f);
        }

        private void AddButtons(List<AbilityTarget> toAdd)
        {
            var index = 0;
            foreach (var ability in toAdd)
            {
                var newButton = GameObject.Instantiate(abilityButtonPrefab);
                newButton.transform.SetParent(abilityPanel.transform, false);

                var buttonRect = newButton.GetComponent<RectTransform>();
                var abilityButtonView = newButton.GetComponent<AbilityButtonView>();

                abilityButtonView.ability = ability;
                buttonRect.anchorMin = Vector2.up;
                buttonRect.anchorMax = Vector2.up;
                buttonRect.pivot = Vector2.up;

                newButton.transform.localScale = Vector3.right;
                iTweenExtensions.ScaleTo(newButton, buttonFinalScale, 0.4f, 0.4f);

                var vertPosition = -((buttonHeight * index) + buttonMargin);
                buttonRect.anchoredPosition3D = new Vector3(14, vertPosition, 0);

                index++;
            }
        }

        private void PositionButtons()
        {
            for (int i = 0; i < abilityPanel.transform.childCount; i++)
            {
                var child = abilityPanel.transform.GetChild(i).gameObject;

                var vertPosition = ((buttonHeight * (i + 1)) + buttonMargin);

                iTweenExtensions.MoveToLocal(
                    child,
                    child.transform.localPosition.SetY(vertPosition),
                    0.4f,
                    0f
                );
            }
        }

    }
}

