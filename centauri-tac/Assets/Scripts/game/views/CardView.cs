using ctac.signals;
using strange.extensions.mediation.impl;
using TMPro;
using UnityEngine;

namespace ctac {
    public class CardView : View
    {
        public CardModel card { get; set; }

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
            costGO = card.gameObject.transform.FindChild("Cost").gameObject;
            attackGO = card.gameObject.transform.FindChild("Attack").gameObject;
            healthGO = card.gameObject.transform.FindChild("Health").gameObject;
            nameGO = card.gameObject.transform.FindChild("Name").gameObject;
            descriptionGO = card.gameObject.transform.FindChild("Descrip").gameObject;
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
    }
}
