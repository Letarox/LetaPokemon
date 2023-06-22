using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _messageText;
    [SerializeField] List<PartyMemberUI> _memberSlots;
    [SerializeField] List<Image> _memberImages;

    List<Pokemon> _pokemons;

    public void SetPartyData(List<Pokemon> pokemons)
    {
        _pokemons = pokemons;

        for (int i = 0; i < _memberSlots.Count; i++)
        {
            if (i < pokemons.Count)
                _memberSlots[i].SetData(pokemons[i]);
            else
                _memberSlots[i].gameObject.SetActive(false);
        }

        _messageText.text = "Choose a Pokemon!";
    }

    public void UpdatePokemonSelection(int selectedPokemon)
    {
        for (int i = 0; i < _memberSlots.Count - 1; i++)
        {
            if (i == selectedPokemon)
                _memberImages[i].color = Color.cyan;
            else
                _memberImages[i].color = Color.white;
        }
    }

    public void SetMessageText(string message)
    {
        _messageText.text = message;
    }
}
