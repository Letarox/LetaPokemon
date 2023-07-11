using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBattleManager : MonoBehaviour
{
    [SerializeField] BattleSystem _battleSystem;
    [SerializeField] BattleUnit _playerUnit, _enemyUnit;
    [SerializeField] BattleDialogueBox _dialogueBox;
    [SerializeField] BattleAbilityBox _abilityBox;
    [SerializeField] PartyScreen _partyScreen;
    [SerializeField] SpriteRenderer _weatherImage;
    [SerializeField] List<Sprite> _weathers;
    WaitUntil _pressAnyKeyToContinue;
    WaitForSeconds _attackDelay, _faintDelay, _weatherDelay;
    public BattleUnit ActivePlayerUnit => _playerUnit;
    public BattleUnit ActiveEnemyUnit => _enemyUnit;
    public BattleDialogueBox DialogueBox => _dialogueBox;
    public BattleAbilityBox AbilityBox => _abilityBox;
    public PartyScreen PartyScreen => _partyScreen;
    public SpriteRenderer WeatherImage => _weatherImage;
    public List<Sprite> Weathers => _weathers;
    public WaitForSeconds AttackDelay => _attackDelay;
    public WaitForSeconds FaintDelay => _faintDelay;
    public WaitForSeconds WeatherDelay => _weatherDelay;
    public WaitUntil PressAnyKeyToContinue => _pressAnyKeyToContinue;
    public void SetupPlayerParty()
    {
        _playerUnit.Setup(_battleSystem.PlayerParty.GetHealthyPokemon());
        _dialogueBox.SetMoveNames(_playerUnit.Pokemon.Moves);
    }

    public void SetupEnemyPokemon(Pokemon wildPokemon)
    {
        _enemyUnit.Setup(wildPokemon);
    }
    public void UpdateWeatherImage(Weather weather)
    {
        _weatherImage.sprite = _weathers[(int)weather.Id];
    }

    void BattleStart()
    {
        _battleSystem.WeatherManager.OnWeatherChange += UpdateWeatherImage;
        _battleSystem.WeatherManager.OnWeatherStartFinish += WeatherStartFinishText;
        _battleSystem.WeatherManager.OnWeatherDamage += WeatherDamageText;
        _battleSystem.WeatherManager.OnWeatherMove += WeatherMoveText;
        _battleSystem.OnBattleOver += BattleOver;
    }

    private void Start()
    {        
        _pressAnyKeyToContinue = new WaitUntil(() => Input.GetKeyDown(KeyCode.X));
        _attackDelay = new WaitForSeconds(1.5f);
        _faintDelay = new WaitForSeconds(1f);
        _weatherDelay = new WaitForSeconds(0.25f);
    }
    private void OnEnable()
    {
        _battleSystem.OnBattleStart += BattleStart;
    }
    private void OnDisable()
    {
        _battleSystem.OnBattleStart -= BattleStart;
    }

    public IEnumerator WeatherStartFinishText(bool isStarting)
    {
        if (isStarting)
            yield return _dialogueBox.TypeDialogue(_battleSystem.WeatherManager.CurrentWeather.StartMessage);        
        else
            yield return _dialogueBox.TypeDialogue(_battleSystem.WeatherManager.CurrentWeather.EndMessage);        
    }

    public IEnumerator WeatherMoveText(bool castSuccess)
    {
        //if the weather is not the same as the move weather effect, the cast is successful and display the proper message. Otherwise, the cast fails
        yield return _faintDelay;
        if (castSuccess)
            yield return StartCoroutine(_dialogueBox.TypeDialogue(_battleSystem.WeatherManager.CurrentWeather.CastMessage));
        else
            yield return StartCoroutine(_dialogueBox.TypeDialogue("But it failed!"));
        yield return _faintDelay;
    }

    public IEnumerator WeatherDamageText(BattleUnit unit)
    {
        yield return StartCoroutine(ShowStatusChanges(unit.Pokemon));
        yield return WeatherDelay;
        unit.PlayHitAnimation();
        yield return WeatherDelay;
        yield return StartCoroutine(unit.Hud.UpdateLoseHP());
        yield return _battleSystem.CheckForFaint(unit);
    }

    public IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        // Dequeues the list of messages of status changes for the pokemon
        while (pokemon.StatusChanges.Count > 0)
        {
            string message = pokemon.StatusChanges.Dequeue();
            yield return StartCoroutine(_dialogueBox.TypeDialogue(message));
        }
    }

    public void BattleOver(bool won)
    {
        _battleSystem.WeatherManager.OnWeatherChange -= UpdateWeatherImage;
        _battleSystem.WeatherManager.OnWeatherStartFinish -= WeatherStartFinishText;
        _battleSystem.WeatherManager.OnWeatherDamage -= WeatherDamageText;
        _battleSystem.WeatherManager.OnWeatherMove -= WeatherMoveText;
        _battleSystem.OnBattleOver -= BattleOver;
    }
}
