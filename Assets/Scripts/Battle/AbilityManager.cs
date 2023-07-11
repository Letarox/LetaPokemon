using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoSingleton<AbilityManager>
{
    private WeatherManager _weatherManager;
    public WeatherManager WeatherManager => _weatherManager;
    public void SetWeatherManager(WeatherManager weatherManager)
    {
        _weatherManager = weatherManager;
    }
    public bool CanReceiveBoost(StatBoost statBoost, Pokemon pokemon)
    {
        //check if the target has an ability with OnStatsChange and if it does, allows the ability to check if the current stat can be changed
        if (pokemon.Base?.Ability?.OnStatsChange != null)
        {
            return pokemon.Base.Ability.OnStatsChange(statBoost, pokemon);
        }

        return true;
    }
    public float OnDamageCheck(Pokemon pokemon, Move move)
    {
        //if the status doesn't have the OnBeforeMove Func, it will return null
        if (pokemon.Base?.Ability?.OnDamageCheck != null)
        {
            return pokemon.Base.Ability.OnDamageCheck(pokemon, move);
        }
        return 1f;
    }

    public float OnAccuracyCheck(Pokemon pokemon)
    {
        //if the pokemon ability has OnAccuracyCheck, runs it, otherwise returns 1f
        if (pokemon.Base?.Ability?.OnAccuracyCheck != null)
        {
            return pokemon.Base.Ability.OnAccuracyCheck(pokemon);
        }
        return 1f;
    }
    public float OnEvasionCheck(Pokemon pokemon)
    {
        //if the pokemon ability has OnAccuracyCheck, runs it, otherwise returns 1f
        if (pokemon.Base?.Ability?.OnEvasionCheck != null)
        {
            return pokemon.Base.Ability.OnEvasionCheck(pokemon);
        }
        return 1f;
    }

    public bool OnPokemonEnterBattle(Pokemon source, Pokemon target)
    {
        //if the pokemon ability has OnAccuracyCheck, runs it, otherwise returns 1f
        if (source.Base?.Ability?.OnPokemonEnterBattle != null)
        {
            source.Base.Ability.OnPokemonEnterBattle(target);
            return true;
        }
        return false;
    }

    public bool OnFlinch(Pokemon pokemon)
    {
        if(pokemon.Base?.Ability?.OnFlinch != null)
        {
            return pokemon.Base.Ability.OnFlinch(pokemon);
        }
        return true;
    }

    public void OnContactCheck(Pokemon attacker, Pokemon defender)
    {
        //checks if the attacking pokemon has OnMakingContact to apply its effects
        if (attacker.Base?.Ability?.OnMakingContact != null)
        {
            attacker.Base.Ability.OnMakingContact(defender);
        }

        //Check if the defending pokemon has OnReceivingContact to apply its effects
        if (defender.Base?.Ability?.OnReceivingContact != null)
        {
            defender.Base.Ability.OnReceivingContact(attacker);
        }
    }
}
