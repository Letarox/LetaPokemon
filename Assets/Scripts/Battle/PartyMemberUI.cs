using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _levelText;
    [SerializeField] HPBar _hpBar;

    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        _nameText.text = pokemon.Base.Name;
        _levelText.text = "Lvl: " + pokemon.Level;
        _hpBar.SetHP(_pokemon.HP, _pokemon.MaxHp);
    }
}
