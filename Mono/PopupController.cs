using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ECScape
{

    public class PopupController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI SystemText;
        [SerializeField] private Slider Slider;

        public void UpdateText(string text)
        {
            SystemText.text = text;
        }

        public void UpdateSlider(float value)
        {
            Slider.value = value;
        }
    }
}
