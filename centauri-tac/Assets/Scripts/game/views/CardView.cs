using ctac.signals;
using strange.extensions.mediation.impl;
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
        public TextMeshPro costText;
        public TextMeshPro attackText;
        public TextMeshPro healthText;
        public TextMeshPro nameText;
        public TextMeshPro descriptionText;

        protected override void Start()
        {
            displayWrapper = card.gameObject.transform.FindChild("DisplayWrapper").gameObject;
            costGO = displayWrapper.transform.FindChild("Cost").gameObject;
            attackGO = displayWrapper.transform.FindChild("Attack").gameObject;
            healthGO = displayWrapper.transform.FindChild("Health").gameObject;
            nameGO = displayWrapper.transform.FindChild("Name").gameObject;
            descriptionGO = displayWrapper.transform.FindChild("Descrip").gameObject;
            costText = costGO.GetComponent<TextMeshPro>();
            attackText = attackGO.GetComponent<TextMeshPro>();
            healthText = healthGO.GetComponent<TextMeshPro>();
            descriptionText = descriptionGO.GetComponent<TextMeshPro>();
            nameText = nameGO.GetComponent<TextMeshPro>();
        }

        void Update()
        {
            costText.text = card.cost.ToString();
            attackText.text = card.attack.ToString();
            healthText.text = card.health.ToString();
            nameText.text = card.name;
            descriptionText.text = card.description;
        }

        public static readonly float HOVER_DELAY = 0.5f;
    }
}
