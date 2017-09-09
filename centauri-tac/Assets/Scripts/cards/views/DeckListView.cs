using strange.extensions.mediation.impl;
using SVGImporter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ctac {
    public class DeckListView : View
    {
        public DeckModel deck { get; set; }

        public RectTransform rectTransform { get; set; }

        public GameObject bgGO;
        public GameObject nameGO;
        public Button deleteButton;

        public TextMeshProUGUI nameText;

        public SVGImage bgImage;

        private bool hovering = false;
        private float timeAccum = 0f;

        protected override void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        protected override void Start()
        {
            bgGO = transform.FindChild("Bg").gameObject;
            nameGO = transform.FindChild("Name").gameObject;
            deleteButton = transform.FindChild("DeleteButton").GetComponent<Button>();

            bgImage = bgGO.GetComponent<SVGImage>();
            nameText = nameGO.GetComponent<TextMeshProUGUI>();
        
            UpdateText();
        }

        void Update()
        {
            if (deck == null) return;
            if (hovering)
            {
                timeAccum += Time.deltaTime;
            }

            if (timeAccum >= Constants.hoverTime)
            {
                deleteButton.gameObject.SetActive(true);
            }

            nameText.text = deck.name;
            bgImage.color = Colors.RacePrimaries[deck.race];
        }

        void OnMouseOver()
        {
            hovering = true;
        }
        void OnMouseExit()
        {
            hovering = false;
            timeAccum = 0f;
            deleteButton.gameObject.SetActive(false);
        }

        public void UpdateText()
        {
            //ResetTextColors();


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
