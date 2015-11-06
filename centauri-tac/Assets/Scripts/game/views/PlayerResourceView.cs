using strange.extensions.mediation.impl;
using TMPro;

namespace ctac
{
    public class PlayerResourceView : View
    {
        public TextMeshProUGUI energyText;

        internal void init()
        {
            energyText = GetComponentInChildren<TextMeshProUGUI>();
        }

        void Update()
        {
        }

        internal void updateText(int resource, int max)
        {
            if (energyText != null)
            {
                energyText.text = string.Format("Energy {0} / {1}", resource, max);
            }
        }
    }
}

