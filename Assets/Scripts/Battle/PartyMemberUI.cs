using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _levelText;
    [SerializeField] HPBar _hpBar;
    [SerializeField] Image _statusImage;
    [SerializeField] List<Sprite> _statusConditionImages;
    [SerializeField] Sprite _faintImage;

    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        _nameText.text = pokemon.Base.Name;
        _levelText.text = "Lvl: " + pokemon.Level;
        _hpBar.SetHP(_pokemon.HP, _pokemon.MaxHp);
    }
    public void SetStatusImage(Condition condition)
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
    public void SetFaintImage()
    {
        _statusImage.sprite = _faintImage;
        _statusImage.gameObject.SetActive(true);
    }
}
