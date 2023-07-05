using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, InBattle, Dialogue }

public class GameManager : MonoBehaviour
{
    public static GameState gameState;
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
        DialogueManager.Instance.OnShowDialogue -= () =>
        {
            gameState = GameState.Dialogue;
        };
        DialogueManager.Instance.OnCloseDialogue -= () =>
        {
            if (gameState == GameState.Dialogue)
                gameState = GameState.FreeRoam;
        };
    }

    void Start()
    {
        DialogueManager.Instance.OnShowDialogue += () =>
        {
            gameState = GameState.Dialogue;
        };
        DialogueManager.Instance.OnCloseDialogue += () =>
        {
            if (gameState == GameState.Dialogue)
                gameState = GameState.FreeRoam;
        };
    }

    void Awake()
    {
        AbilityDB.Init();
        ConditionDB.Init();
        ScreenDB.Init();
        WeatherDB.Init();
    }

    void StartBattle()
    {
        gameState = GameState.InBattle;
        _battleSystem.gameObject.SetActive(true);
        _worldCamera.gameObject.SetActive(false);

        PokemonParty playerParty = _playerController.GetComponent<PokemonParty>();
        if (playerParty == null)
            Debug.LogError("PokemonParty is NULL on " + transform.name);
        MapArea mapArea = FindObjectOfType<MapArea>().GetComponent<MapArea>();
        if(mapArea == null)
            Debug.LogError("MapArea is NULL on " + transform.name);
        Pokemon wildPokemon = mapArea.GetRandomWildPokemon();
        Weather environmentWeather = WeatherDB.Weathers[mapArea.EnvironmentWeather];

        if (playerParty != null && wildPokemon != null)
            _battleSystem.StartBattle(playerParty, wildPokemon, environmentWeather);
    }

    void EndBattle(bool hasWon)
    {
        gameState = GameState.FreeRoam;
        _battleSystem.gameObject.SetActive(false);
        _worldCamera.gameObject.SetActive(true);
    }

    void Update()
    {
        switch (gameState)
        {
            case GameState.FreeRoam:
                _playerController.HandleUpdate();
                break;
            case GameState.InBattle:
                _battleSystem.HandleUpdate();
                break;
            case GameState.Dialogue:
                DialogueManager.Instance.HandleUpdate();
                break;
        }
    }
}
