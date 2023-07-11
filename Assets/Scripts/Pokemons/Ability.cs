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
    public Action<Pokemon> OnPokemonEnterBattle { get; set; }
    public Action<Pokemon> OnMakingContact { get; set; }
    public Action<Pokemon> OnReceivingContact { get; set; }
    public Func<Pokemon, float> OnAccuracyCheck { get; set; }
    public Func<Pokemon, float> OnEvasionCheck { get; set; }
    public Func<StatBoost, Pokemon, bool> OnStatsChange { get; set; }
    public Func<Pokemon, bool> OnFlinch { get; set; }
    public Action<Pokemon> OnAfterTurn { get; set; }
}