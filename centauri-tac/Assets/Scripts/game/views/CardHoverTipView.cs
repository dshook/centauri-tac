using strange.extensions.mediation.impl;
using TMPro;
using UnityEngine;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ctac
{
    public class CardHoverTipView : View
    {
        GameObject hoverTipPrefab = null;
        GameObject hoverTipHolder = null;

        protected override void Awake()
        {
        }

        public void init(IResourceLoaderService loader)
        {
            hoverTipPrefab = loader.Load<GameObject>("HoverTip");
            hoverTipHolder = new GameObject("HoverTipHolder");
            hoverTipHolder.SetActive(false);
        }


        void Update()
        {
        }

        public void EnableHoverTips(CardView cardView)
        {
            DisableHoverTips();
            var cardTop = new Vector2(151.6f, 97.8f);
            var positionDelta = new Vector2(0, -93f);
            var card = cardView.card;

            if(String.IsNullOrEmpty(card.description)) return;

            //find description words to make tips on
            var descriptionMatches = Regex.Matches(card.description, @"<b>(.*?)<\/b>", RegexOptions.Multiline);
            var descriptionWords = new List<string>();
            foreach (Match match in descriptionMatches)
            {
                //second group should be the real match
                if (match.Groups.Count > 1)
                {
                    var descripWord = match.Groups[1].Value;
                    var parenIndex = descripWord.IndexOf('(');
                    //if the word has parens in it like Ability(1) does, strip off everything from the paren on
                    if (parenIndex >= 0)
                    {
                        descripWord = descripWord.Substring(0, parenIndex);
                    }
                    descriptionWords.Add(descripWord);
                }
            }

            //if there's a linked piece with status effects, add those in too
            if (card.linkedPiece != null)
            {
                var statuses = card.linkedPiece.statuses;
                if ((statuses & Statuses.Silence) != 0) { descriptionWords.Add("Silence"); }
                if ((statuses & Statuses.Shield) != 0) { descriptionWords.Add("Holtz Shield"); }
                if ((statuses & Statuses.Paralyze) != 0) { descriptionWords.Add("Paralyze"); }
                if ((statuses & Statuses.Taunt) != 0) { descriptionWords.Add("Taunt"); }
                if ((statuses & Statuses.Cloak) != 0) { descriptionWords.Add("Cloak"); }
                if ((statuses & Statuses.Elusive) != 0) { descriptionWords.Add("Elusive"); }
                if ((statuses & Statuses.Root) != 0) { descriptionWords.Add("Root"); }
                if ((statuses & Statuses.Charge) != 0) { descriptionWords.Add("Charge"); }
                if ((statuses & Statuses.DyadStrike) != 0) { descriptionWords.Add("Dyad Strike"); }
                if ((statuses & Statuses.Flying) != 0) { descriptionWords.Add("Flying"); }
                if ((statuses & Statuses.Airdrop) != 0) { descriptionWords.Add("Airdrop"); }
                if ((statuses & Statuses.Cleave) != 0) { descriptionWords.Add("Cleave"); }
                if ((statuses & Statuses.Piercing) != 0) { descriptionWords.Add("Piercing"); }
            }

            descriptionWords = descriptionWords.Distinct().ToList();

            if(descriptionWords.Count == 0) return;


            int tips = 0;
            foreach (var descriptionWord in descriptionWords)
            {
                if (!Constants.keywordDescrips.ContainsKey(descriptionWord)) { continue; }

                var description = Constants.keywordDescrips[descriptionWord];

                var newHoverTip = Instantiate(hoverTipPrefab);
                newHoverTip.transform.SetParent(hoverTipHolder.transform, false);
                hoverTipHolder.transform.SetParent(cardView.displayWrapper.transform, false);
                hoverTipHolder.transform.localPosition = hoverTipHolder.transform.localPosition.SetZ(-0.5f);

                newHoverTip.name = "HoverTip " + descriptionWord;

                //move the new text down (by subtracting) since coords are based on middle of the card
                newHoverTip.transform.localPosition = cardTop + (tips * positionDelta);

                var title = newHoverTip.transform.Find("Title").gameObject;
                var descrip = newHoverTip.transform.Find("Descrip").gameObject;

                var titleText = title.GetComponent<TextMeshPro>();
                var descripText = descrip.GetComponent<TextMeshPro>();

                titleText.text = descriptionWord;
                descripText.text = description;

                tips++;
            }

            hoverTipHolder.SetActive(true);
        }

        public void DisableHoverTips()
        {
            if(hoverTipHolder == null) return; //can't disable before init

            hoverTipHolder.transform.DestroyChildren();
            hoverTipHolder.SetActive(false);
        }

    }
}

