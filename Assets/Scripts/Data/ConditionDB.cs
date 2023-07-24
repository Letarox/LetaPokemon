using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionDB
{
    static readonly Dictionary<ConditionID, Condition> _conditions;
    public static IReadOnlyDictionary<ConditionID, Condition> Conditions => _conditions;
    static ConditionDB()
    {
        _conditions = new Dictionary<ConditionID, Condition>()
        {
            {
            ConditionID.None,
            new Condition()
            {
                Name = "None"
            }
        },
        {
            ConditionID.PSN,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "was poisoned.",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(Mathf.RoundToInt(Mathf.Clamp(pokemon.MaxHp / 8, 1, pokemon.MaxHp)));
                    pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } is hurt by poison!");
                }
            }
        },
        {
            ConditionID.BRN,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "was burned.",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(Mathf.RoundToInt(Mathf.Clamp(pokemon.MaxHp / 16, 1, pokemon.MaxHp / 15)));
                    pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name }'s hurt by its burn!");
                }
            }
        },
        {
            ConditionID.PAR,
            new Condition()
            {
                Name = "Paralysis",
                StartMessage = "was paralyzed! Maybe it can't attack!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1,101) <= 25)
                    {
                        pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } is paralyzed! It can't move!");
                        return pokemon.CanAttack = false;;
                    }
                    return pokemon.CanAttack = true;
                }
            }
        },
        {
            ConditionID.FRZ,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "was frozen solid!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    //The target has a 20% chance of thawing, otherwise it can't act
                    if(Random.Range(1,11) <= 2)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } is frozen no more!");
                        return pokemon.CanAttack = true;
                    }
                    pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } is frozen solid!");
                    return pokemon.CanAttack = false;;
                }
            }
        },
        {
            ConditionID.SLP,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "is fast asleep!",
                OnStart = (Pokemon pokemon) =>
                {
                    //Random amount of turns between 1 and 3
                    pokemon.StatusTime = Random.Range(1,4);
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    //If the counter is over, remove the target from sleep and alloww it to act
                    if(pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } woke up!");
                        return pokemon.CanAttack = true;;
                    }
                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } is fast asleep!");
                    return pokemon.CanAttack = false;
                }
            }
        },
        {
            ConditionID.Confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "is confused!",
                OnStart = (Pokemon pokemon) =>
                {

                    //Random amount of turns between 1 and 4
                    pokemon.VolatileStatusTime = Random.Range(1,5);
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    //null check for Status and if the target is asleep it will not try to damage itself. This also prevents from counting as a confusion turn
                    if(pokemon.Status != null && !pokemon.CanAttack)
                        return false;

                    if(pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } is confused no more!");
                        return true;
                    }
                    pokemon.VolatileStatusTime--;

                    pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } is confused!");

                    //30% chance of hitting itself
                    if(Random.Range(1,11) <= 7)
                        return true;

                    pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } it hurt itself in its confusion!");
                    pokemon.UpdateHP(pokemon.TakeConfusionDamage());
                    return false;
                }
            }
        },
        {
            ConditionID.Bound,
            new Condition()
            {
                Name = "Bound",
                StartMessage = "was trapped!"
            }
        },
        {
            ConditionID.Trapped,
            new Condition()
            {
                Name = "Trapped",
                StartMessage = "can't escape now!"
            }
        }
        };

        foreach (var conditionPair in _conditions)
        {
            conditionPair.Value.Id = conditionPair.Key;
        }
    }
}

public enum ConditionID 
{ 
    None,
    PSN, 
    BRN,
    PAR,
    FRZ,
    SLP,
    Confusion,
    Bound,
    Trapped
}
