using System;
using UnityEngine;

public class PlayerWealth : MonoBehaviour
{
    [Serializable]
    private class Appearance
    {
        public string title;
        public int minimumValue;
        public SkinnedMeshRenderer[] renderers;
    }

    [Header("Balance")]
    [SerializeField] private int maxValue = 100;
    [SerializeField] private int startValue = 10;

    [Header("Appearances — ascending thresholds")]
    [SerializeField] private Appearance[] appearances;

    public int Value { get; private set; }
    public int MaxValue => maxValue;
    public string CurrentTitle { get; private set; }

    public event Action Changed;
    public event Action Depleted;

    public void ResetWealth()
    {
        Value = Mathf.Clamp(startValue, 1, maxValue);
        RefreshAppearance();
        Changed?.Invoke();
    }

    public void Add(int amount)
    {
        int previousValue = Value;
        Value = Mathf.Clamp(Value + amount, 0, maxValue);

        RefreshAppearance();
        Changed?.Invoke();

        if (previousValue > 0 && Value == 0)
        {
            Depleted?.Invoke();
        }
    }

    private void RefreshAppearance()
    {
        CurrentTitle = "";

        if (appearances == null || appearances.Length == 0)
        {
            return;
        }

        int selectedIndex = 0;

        for (int i = 0; i < appearances.Length; i++)
        {
            if (Value >= appearances[i].minimumValue)
            {
                selectedIndex = i;
            }
        }

        foreach (Appearance appearance in appearances)
        {
            foreach (SkinnedMeshRenderer renderer in appearance.renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }

        Appearance selected = appearances[selectedIndex];
        CurrentTitle = selected.title;

        foreach (SkinnedMeshRenderer renderer in selected.renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }

    private void OnValidate()
    {
        maxValue = Mathf.Max(1, maxValue);
        startValue = Mathf.Clamp(startValue, 1, maxValue);
    }
}