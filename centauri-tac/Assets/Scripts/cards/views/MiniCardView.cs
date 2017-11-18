using ctac.util;
using strange.extensions.mediation.impl;
using SVGImporter;
using TMPro;
using UnityEngine;

namespace ctac {
    public class MiniCardView : View
    {
        public CardModel card { get; set; }
        public int quantity { get; set; }

        public RectTransform rectTransform { get; set; }

        public GameObject bgGO;
        public GameObject costGO;
        public GameObject nameGO;
        public GameObject quantityGO;

        public TextMeshProUGUI costText;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI quantityText;

        public SVGImage bgImage;

        protected override void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        protected override void Start()
        {
            bgGO = transform.Find("Bg").gameObject;
            costGO = transform.Find("Cost").gameObject;
            nameGO = transform.Find("Name").gameObject;
            quantityGO = transform.Find("Quantity").gameObject;

            bgImage = bgGO.GetComponent<SVGImage>();
            costText = costGO.GetComponent<TextMeshProUGUI>();
            quantityText = quantityGO.GetComponent<TextMeshProUGUI>();
            nameText = nameGO.GetComponent<TextMeshProUGUI>();
        
            UpdateText();
        }

        void Update()
        {
        }

        public void UpdateText()
        {
            //ResetTextColors();

            costText.text = card.cost.ToString();
            nameText.text = card.name;

            if (quantity <= 1)
            {
                quantityGO.SetActive(false);
            }
            else
            {
                quantityGO.SetActive(true);
                quantityText.text = quantity + "x";
            }

            switch (card.rarity)
            {
                case Rarities.Common:
                    bgImage.color = ColorExtensions.DesaturateColor(Colors.RarityCommon, 0.2f);
                    break;
                case Rarities.Rare:
                    bgImage.color = ColorExtensions.DesaturateColor(Colors.RarityRare, 0.2f);
                    break;
                case Rarities.Exotic:
                    bgImage.color = ColorExtensions.DesaturateColor(Colors.RarityExotic, 0.2f);
                    break;
                case Rarities.Mythical:
                    bgImage.color = ColorExtensions.DesaturateColor(Colors.RarityMythical, 0.3f);
                    break;
            }

        }

        //removes any buffed/debuffed colors on the numbers
        public void ResetTextColors()
        {
            costText.color = Color.white;
        }
    }
}
