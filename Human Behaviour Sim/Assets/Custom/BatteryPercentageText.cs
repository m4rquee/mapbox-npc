using UnityEngine;
using UnityEngine.UI;

namespace Custom
{
    public class BatteryPercentageText : MonoBehaviour
    {
        private Text _selfText;
        private Slider _parentSlider;

        private void Awake()
        {
            _selfText = GetComponent<Text>();
            _parentSlider = GetComponentInParent<Slider>();
        }

        private void Update()
        {
            var batteryLevel = _parentSlider.value;
            _selfText.text = $"{100 * batteryLevel}%";
            _selfText.color = batteryLevel < 0.1 ? Color.red : Color.white;
        }
    }
}