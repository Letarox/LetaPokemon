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
    public bool CanReceiveBoost(StatBoost statBoost, Pokemon source, Pokemon target)
    {
        //check if the target has an ability with OnStatsChange and if it does, allows the ability to check if the current stat can be changed
        if (target.Base?.Ability?.OnStatsChange != null)
        {
            return target.Base.Ability.OnStatsChange(statBoost, source, target);
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
    public float OnEvasionCheck(Pokemon source, Pokemon target, WeatherID weatherID)
    {
        //if the pokemon ability has OnAccuracyCheck, runs it, otherwise returns 1f
        if (target.Base?.Ability?.OnEvasionCheck != null)
        {
            return target.Base.Ability.OnEvasionCheck(source, target, weatherID);
        }
        return 1f;
    }

    public bool OnPokemonEnterBattle(Pokemon source, Pokemon target)
    {
        //if the pokemon ability has OnAccuracyCheck, runs it, otherwise returns 1f
        if (source.Base?.Ability?.OnPokemonEnterBattle != null)
        {
            source.Base.Ability.OnPokemonEnterBattle(source, target);
            return true;
        }
        return false;
    }

    public bool OnFlinch(Pokemon source, Pokemon target)
    {
        if(target.Base?.Ability?.OnFlinch != null)
        {
            return target.Base.Ability.OnFlinch(source, target);
        }
        return true;
    }

    public bool OnMakingContact(Pokemon attacker, Pokemon defender)
    {
        //checks if the attacking pokemon has OnMakingContact to apply its effects
        if (attacker.Base?.Ability?.OnMakingContact != null)
        {
            return attacker.Base.Ability.OnMakingContact(defender);
        }
        return false;
    }

    public bool OnReceivingContact(Pokemon attacker, Pokemon defender)
    {       
        //Check if the defending pokemon has OnReceivingContact to apply its effects
        if (defender.Base?.Ability?.OnReceivingContact != null)
        {
            return defender.Base.Ability.OnReceivingContact(attacker);
        }
        return false;
    }
}
