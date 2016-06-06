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
        public GameObject moveRange;
        public GameObject tribe;
        public TextMeshPro costText;
        public TextMeshPro attackText;
        public TextMeshPro healthText;
        public TextMeshPro nameText;
        public TextMeshPro descriptionText;
        public TextMeshPro moveRangeText;
        public TextMeshPro tribeText;

        protected override void Start()
        {
            displayWrapper = card.gameObject.transform.FindChild("DisplayWrapper").gameObject;
            costGO = displayWrapper.transform.FindChild("Cost").gameObject;
            attackGO = displayWrapper.transform.FindChild("Attack").gameObject;
            healthGO = displayWrapper.transform.FindChild("Health").gameObject;
            nameGO = displayWrapper.transform.FindChild("Name").gameObject;
            descriptionGO = displayWrapper.transform.FindChild("Descrip").gameObject;
            moveRange = displayWrapper.transform.FindChild("MoveRange").gameObject;
            tribe = displayWrapper.transform.FindChild("Tribe").gameObject;
            costText = costGO.GetComponent<TextMeshPro>();
            attackText = attackGO.GetComponent<TextMeshPro>();
            healthText = healthGO.GetComponent<TextMeshPro>();
            descriptionText = descriptionGO.GetComponent<TextMeshPro>();
            nameText = nameGO.GetComponent<TextMeshPro>();
            moveRangeText = moveRange.GetComponent<TextMeshPro>();
            tribeText = tribe.GetComponent<TextMeshPro>();
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
            costText.text = card.cost.ToString();
            attackText.text = card.attack + "<size=-90>atk</size>";
            healthText.text = "<size=-90>hp</size>" + card.health;
            nameText.text = card.name;
            descriptionText.text = card.description;

            if (card.isMinion)
            {
                //assemble movement and range line
                var totalLength = 27;
                //var moveLength = card.movement.ToString().Length + 9;
                var rangeLength = 0;
                if (card.range.HasValue)
                {
                    rangeLength = card.range.ToString().Length + 10;
                }
                var spacesNeeded = Math.Max(0, totalLength - rangeLength);
                var moveString = (card.movement + " Movement").PadRight(spacesNeeded, ' ');
                if (card.range.HasValue)
                {
                    moveString += "Range " + card.range;
                }
                else
                {
                    moveString += "_";
                }
                moveRangeText.text = moveString;

                //check for tribes
                //whitelist/blacklist/or new field for tribes?  blacklist tags for now
                var tribeTag = card.tags.Where(t => t != "Minion" && !Constants.eventTags.Contains(t)).FirstOrDefault();
                if (tribeTag != null)
                {
                    tribeText.text = tribeTag;
                }
            }
            else
            {
                moveRangeText.text = "";
                attackText.text = "";
                healthText.text = "";
                tribeText.text = "";
            }
        }

        public static readonly float HOVER_DELAY = 0.5f;
    }
}
