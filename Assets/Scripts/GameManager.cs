using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, InBattle }

public class GameManager : MonoBehaviour
{
    GameState _state;
    [SerializeField] PlayerController _playerController;
    [SerializeField] BattleSystem _battleSystem;
    [SerializeField] Camera _worldCamera;

    void OnEnable()
    {
        _playerController.OnEncounter += StartBattle;
        _battleSystem.OnBattleOver += EndBattle;
    }

    void OnDisable()
    {
        _playerController.OnEncounter -= StartBattle;
        _battleSystem.OnBattleOver -= EndBattle;
    }

    void StartBattle()
    {
        _state = GameState.InBattle;
        _battleSystem.gameObject.SetActive(true);
        _worldCamera.gameObject.SetActive(false);

        PokemonParty playerParty = _playerController.GetComponent<PokemonParty>();
        Pokemon wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        if(playerParty != null && wildPokemon != null)
            _battleSystem.StartBattle(playerParty, wildPokemon);
    }

    void EndBattle(bool hasWon)
    {
        _state = GameState.FreeRoam;
        _battleSystem.gameObject.SetActive(false);
        _worldCamera.gameObject.SetActive(true);
    }

    void Update()
    {
        if(_state == GameState.FreeRoam)
        {
            _playerController.HandleUpdate();
        }
        else if(_state == GameState.InBattle)
        {
            _battleSystem.HandleUpdate();
        }
    }
}
