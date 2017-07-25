using strange.extensions.mediation.impl;
using SVGImporter;
using TMPro;
using UnityEngine;

namespace ctac {
    public class DeckListView : View
    {
        public DeckModel deck { get; set; }

        public RectTransform rectTransform { get; set; }

        public GameObject bgGO;
        public GameObject nameGO;

        public TextMeshProUGUI nameText;

        public SVGImage bgImage;

        protected override void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        protected override void Start()
        {
            bgGO = transform.FindChild("Bg").gameObject;
            nameGO = transform.FindChild("Name").gameObject;

            bgImage = bgGO.GetComponent<SVGImage>();
            nameText = nameGO.GetComponent<TextMeshProUGUI>();
        
            UpdateText();
        }

        void Update()
        {
        }

        public void UpdateText()
        {
            //ResetTextColors();

            nameText.text = deck.name;

            //switch (card.rarity)
            //{
            //    case Rarities.Common:
            //        bgImage.color = ColorExtensions.DesaturateColor(Colors.RarityCommon, 0.2f);
            //        break;
            //    case Rarities.Rare:
            //        bgImage.color = ColorExtensions.DesaturateColor(Colors.RarityRare, 0.2f);
            //        break;
            //    case Rarities.Exotic:
            //        bgImage.color = ColorExtensions.DesaturateColor(Colors.RarityExotic, 0.2f);
            //        break;
            //    case Rarities.Mythical:
            //        bgImage.color = ColorExtensions.DesaturateColor(Colors.RarityMythical, 0.3f);
            //        break;
            //}

        }
    }
}
