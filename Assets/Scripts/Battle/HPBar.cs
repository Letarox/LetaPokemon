using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] Slider _hpBarSlider;
    [SerializeField] Image _hpBarColor;
    float _sliderFinalValue;
    float _changeAmount;
    float _currentHP;

    // Calculate duration based on remaining health
    float _duration;

    float _timer;

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
        _sliderFinalValue = current / max;
        _changeAmount = max - current;
        _currentHP = _hpBarSlider.value;

        // Calculate duration based on remaining health
        _duration = Mathf.Lerp(20f, 40f, current / max);

        _timer = 0f;

        while (_hpBarSlider.value > _sliderFinalValue)
        {
            _timer += Time.deltaTime;
            _currentHP -= (_changeAmount * Time.deltaTime) / _duration;
            UpdateHPBarColor(_currentHP);

            if (_currentHP < _sliderFinalValue)
                _currentHP = _sliderFinalValue;

            _hpBarSlider.value = _currentHP;

            yield return null;
        }
    }
    public IEnumerator RegainHPSmoothly(float current, float max, float healthRegained)
    {
        _sliderFinalValue = current / max;
        _changeAmount = current + healthRegained;
        _currentHP = _hpBarSlider.value;

        // Calculate duration based on remaining health
        _duration = Mathf.Lerp(20, 40f, current / max);

        _timer = 0f;

        while (_hpBarSlider.value < _sliderFinalValue)
        {
            _timer += Time.deltaTime;
            _currentHP += (_changeAmount * Time.deltaTime) / _duration;
            UpdateHPBarColor(_currentHP);

            if (_currentHP > _sliderFinalValue)
                _currentHP = _sliderFinalValue;

            _hpBarSlider.value = _currentHP;

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
