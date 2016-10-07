using UnityEngine;
using TMPro;

namespace ctac {
    public class NumberSplatView : MonoBehaviour
    {
        public enum TextType
        {
            Damage,
            Heal
        }

        private GameObject damageSplatGo;
        private TextMeshPro damageSplatText;
        private GameObject damageSplatBonusGo;
        private TextMeshPro damageSplatBonusText;

        public string numberText { get; set; }
        public string bonusText { get; set; }
        public TextType type { get; set; }
        public bool animate { get; set; }

        private Vector3 punchSize = new Vector3(3.3f, 3.3f, 1);
        private Color transparent = new Color(0,0,0,0);

        void Start()
        {
            damageSplatGo = transform.FindChild("Text").gameObject;
            damageSplatText = damageSplatGo.GetComponent<TextMeshPro>();
            damageSplatBonusGo = transform.FindChild("Bonus").gameObject;
            damageSplatBonusText = damageSplatBonusGo.GetComponent<TextMeshPro>();

            damageSplatText.text = numberText;
            damageSplatBonusText.text = bonusText;

            if (animate)
            {
                iTweenExtensions.PunchScale(gameObject, punchSize, 0.8f, 0);
                //iTweenExtensions.ScaleTo(gameObject, Vector3.zero, 1.0f, 0.5f);
                iTweenExtensions.ColorTo(damageSplatGo, transparent, 1f, 0.0f);
                iTweenExtensions.ColorTo(damageSplatBonusGo, transparent, 1f, 0.0f);
                iTweenExtensions.MoveToLocal(gameObject, Vector3.up, 2.5f, 0.0f);
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