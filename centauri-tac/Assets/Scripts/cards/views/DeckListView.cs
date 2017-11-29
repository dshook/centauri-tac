using strange.extensions.mediation.impl;
using SVGImporter;
using TMPro;
using UnityEngine;

namespace ctac {
    public class DeckListView : View
    {
        [Inject] public IResourceLoaderService loader { get; set; }

        public DeckModel deck { get; set; }

        public RectTransform rectTransform { get; set; }

        public GameObject bgGO;
        public GameObject nameGO;
        public GameObject iconGO;

        public TextMeshProUGUI nameText;
        public SVGImage bgImage;
        public SVGImage iconImage;

        public bool isSelected = false;
        bool isHovered = false;

        protected override void Awake()
        {
            base.Awake();
            rectTransform = GetComponent<RectTransform>();
        }

        protected override void Start()
        {
            bgGO = transform.Find("Bg").gameObject;
            nameGO = transform.Find("Name").gameObject;
            iconGO = transform.Find("Icon").gameObject;

            bgImage = bgGO.GetComponent<SVGImage>();
            nameText = nameGO.GetComponent<TextMeshProUGUI>();
            iconImage = iconGO.GetComponent<SVGImage>();

            if (deck != null)
            {
                iconImage.vectorGraphics = loader.Load<SVGAsset>(Constants.RaceIconPaths[deck.race]);
            }
        }

        void Update()
        {
            if (deck == null) return;

            nameText.text = deck.name;
            //bgImage.color = Colors.RacePrimaries[deck.race];

            if (isSelected && isHovered)
            {
                bgImage.color = Colors.darkerGray;
            }
            else if (isHovered)
            {
                bgImage.color = Colors.lightGray;
            }
            else if (isSelected)
            {
                bgImage.color = Colors.darkGray;
            }
            else
            {
                bgImage.color = Colors.white;
            }
        }

        void OnMouseOver()
        {
            isHovered = true;
        }

        void OnMouseExit()
        {
            isHovered = false;
        }
    }
}
