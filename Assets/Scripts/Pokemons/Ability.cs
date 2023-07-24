using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
    public AbilityID Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Action<Pokemon> OnStart { get; set; }
    public Func<Pokemon, Move, float> OnDamageCheck { get; set; }
    public Action<Pokemon, Pokemon> OnPokemonEnterBattle { get; set; }
    public Func<Pokemon, bool> OnMakingContact { get; set; }
    public Func<Pokemon, bool> OnReceivingContact { get; set; }
    public Func<Pokemon, float> OnAccuracyCheck { get; set; }
    public Func<Pokemon, Pokemon, WeatherID, float> OnEvasionCheck { get; set; }
    public Func<StatBoost, Pokemon, Pokemon, bool> OnStatsChange { get; set; }
    public Func<Pokemon, Pokemon, bool> OnFlinch { get; set; }
    public Action<Pokemon> OnAfterTurn { get; set; }
}