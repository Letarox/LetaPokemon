using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCalculator
{
    int RunCriticalCalculation(Stat stat, Pokemon pokemon)
    {
        //check which value is higher for Attack/SpAttack and which is lower for Defense/SpDefense when a crit occurs
        switch (stat)
        {
            case Stat.Attack:
                return pokemon.Stats[Stat.Attack] > pokemon.Attack ? pokemon.Stats[Stat.Attack] : pokemon.Attack;
            case Stat.Defense:
                return pokemon.Stats[Stat.Defense] < pokemon.Defense ? pokemon.Stats[Stat.Defense] : pokemon.Defense;
            case Stat.SpAttack:
                return pokemon.Stats[Stat.SpAttack] > pokemon.SpecialAttack ? pokemon.Stats[Stat.SpAttack] : pokemon.SpecialAttack;
            case Stat.SpDefense:
                return pokemon.Stats[Stat.SpDefense] < pokemon.SpecialDefense ? pokemon.Stats[Stat.SpDefense] : pokemon.SpecialDefense;
            default:
                return 0;
        }
    }
    public bool AccuracyCheck(Move move, Pokemon source, Pokemon target, WeatherID weatherID)
    {
        //check if the move is being used on self
        if (move.Base.Category == MoveCategory.Status && move.Base.Target == MoveTarget.Self)
            return true;
        //check if the target is currently using Dig/Fly
        if (target.TwoTurnMove)
            return false;
        //check if the move should always hit
        if (move.Base.BypassAccuracy)
            return true;
        //Generates a random float number between 1 and 100, multiplying by the accuracy of the move and accuracy of the pokemon, applying the stat changes.
        //Clamps the accuracy to at least 33% accuracy, and maximum of 3x its accuracy
        return (UnityEngine.Random.Range(1.00f, 100.00f) <= Math.Clamp(move.Base.Accuracy * source.Accuracy * target.Evasion * AbilityManager.Instance.OnAccuracyCheck(source) * AbilityManager.Instance.OnEvasionCheck(source, target, weatherID), 33f, move.Base.Accuracy * 3f)) ? true : false;
    }
    public int DamageCalculation(Move move, Pokemon attacker, Pokemon defender, WeatherID weatherID, DamageDetails damageDetails, List<Screen> screens = null)
    {
        //Applies the whole formula of damage, calculating all instances that can impact the outcome of the damage
        float stab = (attacker.Base.PrimaryType == move.Base.Type || attacker.Base.SecondaryType == move.Base.Type) ? 1.5f : 1f;
        float burn = (move.Base.Category == MoveCategory.Physical && attacker.Status == ConditionDB.Conditions[ConditionID.BRN] && attacker.Base.Ability.Id != AbilityID.Guts) ? 0.5f : 1f;
        float abilities = AbilityManager.Instance.OnDamageCheck(attacker, move);
        float weather = ((move.Base.Type == PokemonType.Fire && weatherID == WeatherID.Sunny) || (move.Base.Type == PokemonType.Water && weatherID == WeatherID.Rain)) ? 1.5f :
            ((move.Base.Type == PokemonType.Fire && weatherID == WeatherID.Rain) || (move.Base.Type == PokemonType.Water && weatherID == WeatherID.Sunny)) ? 0.5f : 1f;
        float screen = (move.Base.Category == MoveCategory.Physical) ?
            screens.Exists(obj => obj.Id == ScreenType.Reflect || obj.Id == ScreenType.AuroraVeil && damageDetails.Critical == 1f) ?
                0.5f : 1f :
            screens.Exists(obj => obj.Id == ScreenType.LightScreen || obj.Id == ScreenType.AuroraVeil && damageDetails.Critical == 1f) ?
                0.5f : 1f;
        float modifiers = UnityEngine.Random.Range(0.85f, 1f) * damageDetails.TypeEffectiveness * damageDetails.Critical * stab * burn * abilities * weather * screen;
        int offense = (move.Base.Category == MoveCategory.Physical) ?
            (damageDetails.Critical > 1f) ?
                RunCriticalCalculation(Stat.Attack, attacker) : attacker.Attack :
            (damageDetails.Critical > 1f) ?
                RunCriticalCalculation(Stat.SpAttack, attacker) : attacker.SpecialAttack;
        int defense = (move.Base.Category == MoveCategory.Physical) ?
            (damageDetails.Critical > 1f) ?
                RunCriticalCalculation(Stat.Defense, defender) : defender.Defense :
            (damageDetails.Critical > 1f) ?
                RunCriticalCalculation(Stat.SpDefense, defender) : defender.SpecialDefense;
        defense *= Mathf.FloorToInt((move.Base.Category == MoveCategory.Physical) ?
            ((defender.Base.PrimaryType == PokemonType.Ice || defender.Base.SecondaryType == PokemonType.Ice) && weatherID == WeatherID.Hail) ? 1.5f : 1f :
            ((defender.Base.PrimaryType == PokemonType.Rock || defender.Base.SecondaryType == PokemonType.Rock) && weatherID == WeatherID.Sandstorm) ? 1.5f : 1f);
        int baseDamage = (((2 * attacker.Level / 5) + 2) * move.Base.Power * offense / defense) / 50 + 2;
        int damage = Math.Clamp(Mathf.FloorToInt(baseDamage * modifiers), 1, defender.MaxHp);
        return damage;
    }
    public DamageDetails ApplyDamage(Move move, Pokemon attacker, Pokemon defender, WeatherID weatherId, List<Screen> screens = null)
    {
        DamageDetails damageDetails = new DamageDetails()
        {
            TypeEffectiveness = TypeChart.GetEffectiveness(move.Base.Type, defender.Base.PrimaryType) * TypeChart.GetEffectiveness(move.Base.Type, defender.Base.SecondaryType),
            Critical = (UnityEngine.Random.Range(1.00f, 100.00f) <= (attacker.Critical * 100f)) ? 1.5f : 1f
        };
        int damage = DamageCalculation(move, attacker, defender, weatherId, damageDetails, screens);

        //if struggle damages the attacker for a 1/4 of their health
        if (move.Base.Name == "Struggle")
        {
            attacker.UpdateHP(Mathf.RoundToInt(attacker.MaxHp / 4));
            attacker.StatusChanges.Enqueue($"{ attacker.Base.Name } is damaged by recoil!");
        }

        //if the move is a HP Draining Move, apply the amount of health to be restored and adds the message to be displayed
        if (move.Base.HPDrainingMove)
        {
            damageDetails.HealthRestored = (defender.HP - damage) > 0 ? Mathf.FloorToInt(damage / 2) : Mathf.Clamp(Mathf.FloorToInt(defender.HP / 2), 1, defender.HP);
            attacker.RegainHP(Mathf.FloorToInt(damageDetails.HealthRestored));
            attacker.StatusChanges.Enqueue($"{ defender.Base.Name } had its energy drained!");
        }

        //if the move contains recoil, apply the recoil based on the damage about to be dealt, being capped at the target HP in case of death
        if (move.Base.Recoil > 0)
        {
            attacker.UpdateHP((attacker.HP - damage) > 0 ? Mathf.FloorToInt(damage / move.Base.Recoil) : Mathf.Clamp(Mathf.FloorToInt(attacker.HP / move.Base.Recoil), 1, attacker.HP));
            attacker.StatusChanges.Enqueue($"{ attacker.Base.Name } is damage by recoil.");
        }

        defender.UpdateHP(damage);
        damageDetails.Fainted = (defender.HP <= 0) ? true : false;
        return damageDetails;
    }
}
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
    public int HealthRestored { get; set; }
}