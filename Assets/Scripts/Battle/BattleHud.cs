using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _levelText;
    [SerializeField] HPBar _hpBar;
    [SerializeField] Image _statusImage;
    [SerializeField] List<Sprite> _statusConditionImages;

    Pokemon _pokemon;

    void OnDisable()
    {
        _pokemon.OnStatusChanged -= SetStatusImage;
    }
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        _nameText.text = pokemon.Base.Name;
        _levelText.text = "Lvl: " + pokemon.Level;
        SetStatusImage(_pokemon.Status);
        _hpBar.SetHP(_pokemon.HP, _pokemon.MaxHp);
        _pokemon.OnStatusChanged += SetStatusImage;
    }
    void SetStatusImage(Condition condition)
    {
        if (condition == null || _pokemon.Status == null)
            _statusImage.gameObject.SetActive(false);
        else
        {
            if ((int)condition.Id > _statusConditionImages.Count)
                return;
            _statusImage.gameObject.SetActive(true);
            _statusImage.sprite = _statusConditionImages[(int)_pokemon.Status.Id - 1];
        }
    }

    public IEnumerator UpdateLoseHP()
    {
        //check if the pokemon HP was already changed, and update its HP bar
        if (_pokemon.HPChanged)
        {
            yield return _hpBar.LoseHPSmoothly((float)_pokemon.HP, (float)_pokemon.MaxHp);
            _pokemon.HPChanged = false;
        }
    }
    public IEnumerator UpdateRegainHP(float healthRegained)
    {
        //check if the pokemon HP was already changed, and update its HP bar
        if (_pokemon.HPChanged)
        {
            yield return _hpBar.RegainHPSmoothly((float)_pokemon.HP, (float)_pokemon.MaxHp, healthRegained);
            _pokemon.HPChanged = false;
        }
    }
}
