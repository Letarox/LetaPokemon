using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDB
{
    public static void Init()
    {
        foreach (KeyValuePair<AbilityID, Ability> keyValuePair in Abilities)
        {
            AbilityID abilityId = keyValuePair.Key;
            Ability ability = keyValuePair.Value;

            ability.Id = abilityId;
        }
    }
    public static Dictionary<AbilityID, Ability> Abilities { get; set; } = new Dictionary<AbilityID, Ability>()
    {
        {
            AbilityID.Blaze,
            new Ability()
            {
                Name = "Blaze",
                OnDamageCheck = (Pokemon pokemon, Move move) => 
                {
                    if (move.Base.Type == PokemonType.Fire && (pokemon.Base.PrimaryType == PokemonType.Fire || pokemon.Base.SecondaryType == PokemonType.Fire) && (pokemon.HP <= (pokemon.MaxHp / 3f)))
                        return 1.5f;
                    
                    return 1f;
                }
            }
        },
        {
            AbilityID.Overgrow,
            new Ability()
            {
                Name = "Overgrow",
                OnDamageCheck = (Pokemon pokemon, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Grass && (pokemon.Base.PrimaryType == PokemonType.Grass || pokemon.Base.SecondaryType == PokemonType.Grass) && (pokemon.HP <= (pokemon.MaxHp / 3f)))
                        return 1.5f;
                    
                    return 1f;
                }
            }
        },
        {
            AbilityID.Torrent,
            new Ability()
            {
                Name = "Torrent",
                OnDamageCheck = (Pokemon pokemon, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Water && (pokemon.Base.PrimaryType == PokemonType.Water || pokemon.Base.SecondaryType == PokemonType.Water) && (pokemon.HP <= (pokemon.MaxHp / 3f)))
                        return 1.5f;
                    
                    return 1f;
                }
            }
        },
        {
            AbilityID.PoisonPoint,
            new Ability()
            {
                Name = "Poison Point",
                OnReceivingContact = (Pokemon pokemon) =>
                {
                    if (Random.Range(1,11) <= 3)
                    {
                        if (pokemon.CanReceiveStatus(ConditionID.PSN))
                        {
                            pokemon.SetStatus(ConditionID.PSN);
                        }
                    }
                }
            }
        },
        {
            AbilityID.Static,
            new Ability()
            {
                Name = "Static",
                OnReceivingContact = (Pokemon pokemon) =>
                {
                    if (Random.Range(1,11) <= 3)
                    {
                        if (pokemon.CanReceiveStatus(ConditionID.PAR))
                        {
                            pokemon.SetStatus(ConditionID.PAR);
                        }
                    }
                }
            }
        },
        {
            AbilityID.CompoundEyes,
            new Ability()
            {
                Name = "Compound Eyes",
                OnAccuracyCheck = (Pokemon pokemon) =>
                {
                    return 1.3f;
                }
            }
        },
        {
            AbilityID.KeenEye,
            new Ability()
            {
                Name = "Keen Eye",
                OnStatsChange = (StatBoost statBoost, Pokemon pokemon) =>
                {
                    if(statBoost.stat == Stat.Accuracy && statBoost.boost < 0)
                    {
                        pokemon.StatusChanges.Enqueue("Keen Eye prevents the loss of accuracy!");
                        return false;
                    }                    
                    return true;
                }
            }
        },
        {
            AbilityID.Guts,
            new Ability()
            {
                Name = "Guts",
                OnDamageCheck = (Pokemon pokemon, Move move) =>
                {
                    if (pokemon.Status != null)
                        return 1.5f;

                    return 1f;
                }
            }
        },
        {
            AbilityID.Overcoat,
            new Ability()
            {
                Name = "Overcoat"
            }
        },
        {
            AbilityID.Intimidate,
            new Ability()
            {
                Name = "Intimidate",
                OnPokemonEnterBattle = (Pokemon pokemon) =>
                {
                    StatBoost intimidate = new StatBoost{
                        stat = Stat.Attack,
                        boost = -1
                    };
                    
                    if(AbilityManager.Instance.CanReceiveBoost(intimidate, pokemon))
                    {
                        pokemon.ApplyBoost(intimidate);
                    }
                    else
                    {
                        pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } cannot have its Attack reduced!");
                    }
                }
            }
        }
    };
}

public enum AbilityID
{
    Blaze,
    Overgrow,
    Torrent,
    PoisonPoint,
    CompoundEyes,
    Static,
    KeenEye,
    Guts,
    Overcoat,
    Intimidate
}