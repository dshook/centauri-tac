﻿using ctac.util;
using strange.extensions.mediation.impl;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ctac {
    public class CardView : View
    {
        public CardModel card { get; set; }

        public GameObject displayWrapper;
        public GameObject costGO;
        public GameObject attackGO;
        public GameObject healthGO;
        public GameObject nameGO;
        public GameObject descriptionGO;
        public GameObject move;
        public GameObject range;
        public GameObject tribe;
        public GameObject moveRangeUnderline;

        public GameObject buffsPanel;
        public GameObject buffBg;
        public GameObject buffName;
        public GameObject buffAbility;

        public TextMeshPro costText;
        public TextMeshPro attackText;
        public TextMeshPro healthText;
        public TextMeshPro nameText;
        public TextMeshPro descriptionText;
        public TextMeshPro moveText;
        public TextMeshPro rangeText;
        public TextMeshPro tribeText;

        public TextMeshPro buffNameText;
        public TextMeshPro buffAbilityText;

        protected override void Start()
        {
            displayWrapper = card.gameObject.transform.FindChild("DisplayWrapper").gameObject;
            costGO = displayWrapper.transform.FindChild("Cost").gameObject;
            attackGO = displayWrapper.transform.FindChild("Attack").gameObject;
            healthGO = displayWrapper.transform.FindChild("Health").gameObject;
            nameGO = displayWrapper.transform.FindChild("Name").gameObject;
            descriptionGO = displayWrapper.transform.FindChild("Descrip").gameObject;
            move = displayWrapper.transform.FindChild("Movement").gameObject;
            range = displayWrapper.transform.FindChild("Range").gameObject;
            tribe = displayWrapper.transform.FindChild("Tribe").gameObject;
            moveRangeUnderline = displayWrapper.transform.FindChild("MoveRangeUnderline").gameObject;

            buffsPanel = displayWrapper.transform.FindChild("Buffs").gameObject;
            buffBg = buffsPanel.transform.FindChild("BuffBg").gameObject;
            buffName = buffsPanel.transform.FindChild("BuffName").gameObject;
            buffAbility = buffsPanel.transform.FindChild("BuffAbility").gameObject;

            costText = costGO.GetComponent<TextMeshPro>();
            attackText = attackGO.GetComponent<TextMeshPro>();
            healthText = healthGO.GetComponent<TextMeshPro>();
            descriptionText = descriptionGO.GetComponent<TextMeshPro>();
            nameText = nameGO.GetComponent<TextMeshPro>();
            moveText = move.GetComponent<TextMeshPro>();
            rangeText = range.GetComponent<TextMeshPro>();
            tribeText = tribe.GetComponent<TextMeshPro>();
            buffNameText = buffName.GetComponent<TextMeshPro>();
            buffAbilityText = buffAbility.GetComponent<TextMeshPro>();

        }

        void Update()
        {
            UpdateText();
        }

        public void UpdateText()
        {
            if (costText == null)
            {
                Start();
            }
            ResetTextColors();

            costText.text = card.cost.ToString();
            nameText.text = card.name;
            descriptionText.text = card.description;

            if (card.isMinion)
            {
                moveRangeUnderline.SetActive(true);

                //update number colors based on if piece is buffed
                var piece = card.linkedPiece;
                Color? attackColor = null, healthColor = null, movementColor = null, rangeColor = null;
                if (piece != null)
                {
                    if      (piece.attack > piece.baseAttack) { attackColor = Color.green; }
                    else if (piece.attack < piece.baseAttack) { attackColor = Color.red; }

                    if      (piece.health > piece.baseHealth) { healthColor = Color.green; }
                    else if (piece.health < piece.baseHealth) { healthColor = Color.red; }

                    if      (piece.movement > piece.baseMovement) { movementColor = Color.green; }
                    else if (piece.movement < piece.baseMovement) { movementColor = Color.red; }

                    if      (piece.range != null && piece.range > piece.baseRange) { rangeColor = Color.green; }
                    else if (piece.range != null && piece.range < piece.baseRange) { rangeColor = Color.red; }
                }

                attackText.text = colorWrap(card.attack, attackColor) + "<size=-90>atk</size>";
                healthText.text = "<size=-90>hp</size>" + colorWrap(card.health, healthColor);
                moveText.text = colorWrap(card.movement, movementColor) + " Movement";

                if (card.range.HasValue)
                {
                    rangeText.text = "Range " + colorWrap(card.range.Value, rangeColor);
                }

                //check for tribes
                //whitelist/blacklist/or new field for tribes?  blacklist tags for now
                var tribeTag = card.tags.Where(t => t != "Minion" && !Constants.eventTags.Contains(t)).FirstOrDefault();
                if (tribeTag != null)
                {
                    tribeText.text = tribeTag;
                }
                else
                {
                    tribeText.text = "";
                }
            }
            else
            {
                moveRangeUnderline.SetActive(false);
                moveText.text = "";
                rangeText.text = "";
                attackText.text = "";
                healthText.text = "";
                tribeText.text = "";
            }
        }

        private string colorWrap(int number, Color? c)
        {
            if (c != null)
            {
                return string.Format("<{0}>{1}</color>", ((Color32)c.Value).ToHex(), number);
            }
            return number.ToString();
        }

        //removes any buffed/debuffed colors on the numbers
        public void ResetTextColors()
        {
            attackText.color = Color.black;
            healthText.color = Color.black;
            moveText.color = Color.black;
            rangeText.color = Color.black;
        }

        public void UpdateBuffsDisplay()
        {
            if(buffsPanel == null) return;

            if (card.linkedPiece == null || card.linkedPiece.buffs == null || card.linkedPiece.buffs.Count == 0)
            {
                buffsPanel.SetActive(false);
                return;
            }

            buffsPanel.SetActive(true);

            //cleanup old duplicated buffs if any just by deleting ones with index more than the setup amount
            for(int t = 0; t < buffsPanel.transform.childCount; t++)
            {
                if (t > 2) {
                    Destroy(buffsPanel.transform.GetChild(t).gameObject);
                }
            }

            var buffTextHeight = 0f;
            var singleLineHeight = 30f;
            var doubleLineHeight = 50f;
            var bottomPadding = 5f;
            var buffs = card.linkedPiece.buffs;

            for (int i = 0; i < buffs.Count; i++)
            {
                var buff = buffs[i];
                if (buff.removed) {
                    continue;
                }
                var currentBuffNameText = buffNameText;
                var currentBuffAbilityText = buffAbilityText;
                if (i > 0)
                {
                    //for buffs more than the first buff copy buff name and abilities and move down
                    var newBuffName = Instantiate(buffName);
                    var newBuffAbility = Instantiate(buffAbility);

                    newBuffName.transform.SetParent(buffsPanel.transform, false);
                    newBuffAbility.transform.SetParent(buffsPanel.transform, false);

                    newBuffName.name = "BuffName " + i;
                    newBuffAbility.name = "BuffAbility " + i;

                    //move the new text down (by subtracting) since coords are based on middle of the card
                    newBuffName.transform.localPosition = buffName.transform.localPosition + new Vector3(0, -buffTextHeight, 0);
                    newBuffAbility.transform.localPosition = buffAbility.transform.localPosition + new Vector3(0, -buffTextHeight, 0);

                    currentBuffNameText = newBuffName.GetComponent<TextMeshPro>();
                    currentBuffAbilityText = newBuffAbility.GetComponent<TextMeshPro>();
                }

                currentBuffNameText.text = buff.name; 
                var numberFormat = "+0;-#";
                var twoLines = false;
                //js would actually be better at building this string
                string buffAttrTxt = "";
                if (buff.attack.HasValue)
                {
                    buffAttrTxt = buff.attack.Value.ToString(numberFormat) + " Attack ";
                }
                if (buff.health.HasValue)
                {
                    buffAttrTxt += buff.health.Value.ToString(numberFormat) + " Health ";
                }
                if (buff.movement.HasValue)
                {
                    twoLines = true;
                    buffAttrTxt += "\n" + buff.movement.Value.ToString(numberFormat) + " Movement ";
                }
                if (buff.range.HasValue)
                {
                    buffAttrTxt += buff.range.Value.ToString(numberFormat) + " Range ";
                    if (buff.attack.HasValue && buff.health.HasValue)
                    {
                        twoLines = true;
                    }
                }

                currentBuffAbilityText.text = buffAttrTxt;

                buffTextHeight += twoLines ? doubleLineHeight : singleLineHeight;
            }

            buffBg.transform.localScale = buffBg.transform.localScale.SetY(buffTextHeight * 2 + bottomPadding);
        }

        public static readonly float HOVER_DELAY = 0.5f;
    }
}
