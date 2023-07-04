using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] Slider _hpBarSlider;
    [SerializeField] Image _hpBarColor;

    public void SetHP(int currentHP, int maxHP)
    {
        _hpBarSlider.value = GetHP(currentHP, maxHP);
        UpdateHPBarColor(_hpBarSlider.value);
    }
    float GetHP(int currentHP, int maxHP)
    {
        return (float)currentHP / maxHP;
    }

    public IEnumerator LoseHPSmoothly(float current, float max)
    {
        //check the value that needs to be updated on the slider and does it smoothly
        float sliderFinalValue = current / max;
        float changeAmount = max - current;
        float currentHP = _hpBarSlider.value;
        while (_hpBarSlider.value > sliderFinalValue)
        {
            currentHP -= (changeAmount * Time.deltaTime) / 18;
            UpdateHPBarColor(currentHP);
            if (currentHP < sliderFinalValue)
                currentHP = sliderFinalValue;
            _hpBarSlider.value = currentHP;
            yield return null;
        }
    }
    public IEnumerator RegainHPSmoothly(float current, float max, float healthRegained)
    {
        //check the value that needs to be updated on the slider and does it smoothly
        float sliderFinalValue = current / max;
        float changeAmount = current - healthRegained;
        float currentHP = _hpBarSlider.value;
        while (_hpBarSlider.value < sliderFinalValue)
        {
            currentHP += (changeAmount * Time.deltaTime) / 18;
            UpdateHPBarColor(currentHP);
            if (currentHP > sliderFinalValue)
                currentHP = sliderFinalValue;
            _hpBarSlider.value = currentHP;
            yield return null;
        }
    }
    void UpdateHPBarColor(float currentHP)
    {
        if (currentHP >= 0.5f)
            _hpBarColor.color = Color.green;
        else
            if(currentHP > 0.21f)
                _hpBarColor.color = new Color(1.0f, 0.64f, 0.0f);
            else
                _hpBarColor.color = Color.red;
    }
}
