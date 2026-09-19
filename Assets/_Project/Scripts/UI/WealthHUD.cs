using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WealthHUD : MonoBehaviour
{
    [SerializeField] private PlayerWealth wealth;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text label;

    private void OnEnable()
    {
        wealth.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        wealth.Changed -= Refresh;
    }

    private void Refresh()
    {
        slider.normalizedValue = (float)wealth.Value / wealth.MaxValue;

        label.text = $"{wealth.CurrentTitle}  {wealth.Value}/{wealth.MaxValue}";
    }
}