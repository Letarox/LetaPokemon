using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] Slider _hpBarSlider;

    public void SetHP(int currentHP, int maxHP)
    {
        _hpBarSlider.value = GetHP(currentHP, maxHP);
    }
    float GetHP(int currentHP, int maxHP)
    {
        return (float)currentHP / maxHP;
    }

    public IEnumerator ChangeHPSmoothly(float current, float max)
    {
        float sliderFinalValue = current / max;
        float changeAmount = max - current;
        float currentHP = _hpBarSlider.value;

        while (_hpBarSlider.value > sliderFinalValue)
        {
            currentHP -= (changeAmount * Time.deltaTime)/10;
            if (currentHP < sliderFinalValue)
                currentHP = sliderFinalValue;
            _hpBarSlider.value = currentHP;
            yield return null;
        }
    }
}
