using ctac.util;
using strange.extensions.mediation.impl;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace ctac {
    public class CardView : View
    {
        public CardModel card { get; set; }
        public RectTransform rectTransform { get; set; }

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

        protected override void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

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
        
            //hopefully this won't cause any issues updating the text with 0 spell damage
            //it should get updated shortly after on queue process completion
            UpdateText(0);
        }

        void Update()
        {
        }

        public void UpdateText(int currentSpellDamage)
        {
            if (costText == null)
            {
                Start();
            }
            ResetTextColors();

            costText.text = card.cost.ToString();
            nameText.text = card.name;
            descriptionText.text = card.description;

            if      (card.cost > card.baseCost) { costText.color = Color.red; }
            else if (card.cost < card.baseCost) { costText.color = Color.green; }

            if (card.metCondition) { descriptionText.color = ColorExtensions.HexToColor("740000"); }
            else { descriptionText.color = Color.black; }

            if (card.isMinion || card.isHero)
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
                else
                {
                    rangeText.text = "";
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
                //must be a spell
                moveRangeUnderline.SetActive(false);
                moveText.text = "";
                rangeText.text = "";
                attackText.text = "";
                healthText.text = "";
                tribeText.text = "";

                //update spell damage text
                string descrip = card.description;
                var numberMatches = Regex.Matches(descrip, @"\{\d+\}");
                for(int m = numberMatches.Count - 1; m >= 0; m--) 
                {
                    Match match = numberMatches[m];
                    for(int c = 0; c < match.Captures.Count; c++)
                    {
                        Capture capture = match.Captures[c];
                        int number = Int32.Parse(capture.Value.Replace("{", "").Replace("}", ""));
                        string replacement = number.ToString();
                        if (currentSpellDamage > 0)
                        {
                            replacement = string.Format("*{0}*", number + currentSpellDamage);
                        }
                        
                        descrip = descrip.ReplaceAt(capture.Index, capture.Length, replacement);
                    }
                }
                descriptionText.text = descrip;
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
            costText.color = Color.white;
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
            var singleLineHeight = 35f;
            var doubleLineHeight = 50f;
            var bottomPadding = 10f;

            //since buff name is unique, group by the name and only show totals
            var groupedBuffs = card.linkedPiece.buffs
                .Where(b => !b.removed)
                .GroupBy(b => b.name)
                .Select(b => new
                {
                    name = b.Key,
                    attack = b.Sum(s => s.attack),
                    health = b.Sum(s => s.health),
                    movement = b.Sum(s => s.movement),
                    range = b.Sum(s => s.range),
                }).ToArray();

            for (int i = 0; i < groupedBuffs.Length; i++)
            {
                var buff = groupedBuffs[i];
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
                if (buff.attack.HasValue && buff.attack != 0)
                {
                    buffAttrTxt = buff.attack.Value.ToString(numberFormat) + " Attack ";
                }
                if (buff.health.HasValue && buff.health != 0)
                {
                    buffAttrTxt += buff.health.Value.ToString(numberFormat) + " Health ";
                }
                if (buff.movement.HasValue && buff.movement != 0)
                {
                    twoLines = true;
                    buffAttrTxt += "\n" + buff.movement.Value.ToString(numberFormat) + " Movement ";
                }
                if (buff.range.HasValue && buff.range != 0)
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

        public class UpdateTextAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public GameObject textGO { get; set; }
            public TextMeshPro text { get; set; }
            public int current { get; set; }
            public int original { get; set; }
            public int change { get; set; }
            private Vector3 punchSize = new Vector3(1.5f, 1.5f, 1.5f);

            public void Init() { }
            public void Update()
            {
                if (text == null) return;

                text.text = current.ToString();
                if (change != 0)
                {
                    iTweenExtensions.PunchScale(textGO, punchSize, 1.5f, 0);
                }
                if (current > original)
                {
                    text.color = Color.red;
                }
                else if (current < original)
                {
                    text.color = Color.green;
                }
                else
                {
                    text.color = Color.white;
                }
                Complete = true;
            }
        }
    }
}
