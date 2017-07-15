using strange.extensions.mediation.impl;
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

        protected override void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        protected override void Start()
        {
            bgGO = transform.FindChild("Bg").gameObject;
            costGO = transform.FindChild("Cost").gameObject;
            nameGO = transform.FindChild("Name").gameObject;
            quantityGO = transform.FindChild("Quantity").gameObject;

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
                quantityText.text = quantity.ToString();
            }

        }

        //removes any buffed/debuffed colors on the numbers
        public void ResetTextColors()
        {
            costText.color = Color.white;
        }
    }
}
