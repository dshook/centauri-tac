using UnityEngine;
using TMPro;
using System;
using ctac.util;

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

        private Color damageColor = ColorExtensions.HexToColor("FD0000");
        private Color damageColorBot = ColorExtensions.HexToColor("470000");

        private Color healColor = ColorExtensions.HexToColor("00F00B");
        private Color healColorBot = ColorExtensions.HexToColor("006504");

        void Start()
        {
            damageSplatGo = transform.FindChild("Text").gameObject;
            damageSplatText = damageSplatGo.GetComponent<TextMeshPro>();
            damageSplatBonusGo = transform.FindChild("Bonus").gameObject;
            damageSplatBonusText = damageSplatBonusGo.GetComponent<TextMeshPro>();

            //damage vs heal
            if (change <= 0)
            {
                damageSplatText.color = damageColor;
                damageSplatBonusText.color = damageColor;
                damageSplatText.colorGradient = new VertexGradient(Color.white, Color.white, damageColorBot, damageColorBot);
                damageSplatBonusText.colorGradient = new VertexGradient(Color.white, Color.white, damageColorBot, damageColorBot);
            }
            else
            {
                damageSplatText.color = healColor;
                damageSplatBonusText.color = healColor;
                damageSplatText.colorGradient = new VertexGradient(Color.white, Color.white, healColorBot, healColorBot);
                damageSplatBonusText.colorGradient = new VertexGradient(Color.white, Color.white, healColorBot, healColorBot);
            }

            damageSplatText.text = string.Format("{0:+#;-#;0}", change);

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
                iTweenExtensions.ColorTo(damageSplatGo, Color.clear, 1f, 0.0f);
                iTweenExtensions.ColorTo(damageSplatBonusGo, Color.clear, 1.5f, 0.0f);
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