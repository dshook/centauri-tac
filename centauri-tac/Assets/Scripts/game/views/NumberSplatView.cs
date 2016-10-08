using UnityEngine;
using TMPro;
using System;

namespace ctac {
    public class NumberSplatView : MonoBehaviour
    {
        private GameObject damageSplatGo;
        private TextMeshPro damageSplatText;
        private GameObject damageSplatBonusGo;
        private TextMeshPro damageSplatBonusText;

        public int change { get; set; }
        public int? bonus { get; set; }
        public string bonusText { get; set; }
        public bool animate { get; set; }

        private Vector3 punchSize = new Vector3(1.5f, 1.5f, 1);
        private Color transparent = new Color(0,0,0,0);

        void Start()
        {
            damageSplatGo = transform.FindChild("Text").gameObject;
            damageSplatText = damageSplatGo.GetComponent<TextMeshPro>();
            damageSplatBonusGo = transform.FindChild("Bonus").gameObject;
            damageSplatBonusText = damageSplatBonusGo.GetComponent<TextMeshPro>();

            damageSplatText.text = change.ToString();

            if (bonus.HasValue && bonus != 0)
            {
                damageSplatBonusText.text = Math.Abs(bonus.Value).ToString(); //+ " " + bonusText;
            }
            else
            {
                damageSplatBonusText.text = "";
            }

            if (animate)
            {
                iTweenExtensions.PunchScale(gameObject, punchSize, 0.3f, 0);
                //iTweenExtensions.ScaleTo(gameObject, Vector3.zero, 1.0f, 0.5f);
                iTweenExtensions.ColorTo(damageSplatGo, transparent, 1f, 0.0f);
                iTweenExtensions.ColorTo(damageSplatBonusGo, transparent, 1.5f, 0.0f);
                iTweenExtensions.MoveToLocal(gameObject, Vector3.up, 3.5f, 0.0f);
            }
        }

        void Update()
        {
            //cleanup when punch scale is done
            if (transform.localScale == Vector3.zero)
            {
                Destroy(this);
            }
        }

    }
}