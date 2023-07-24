using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDB
{
    static readonly Dictionary<AbilityID, Ability> _abilities;
    public static IReadOnlyDictionary<AbilityID, Ability> Abilities => _abilities;
    static AbilityDB()
    {
        _abilities = new Dictionary<AbilityID, Ability>()
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
                        //30% chance of proc
                        if (Random.Range(1,11) <= 3)
                        {
                            if (pokemon.CanReceiveStatus(ConditionID.PSN))
                            {
                                pokemon.SetStatus(ConditionID.PSN);
                                return true;
                            }
                        }
                        return false;
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
                        //30% chance of proc
                        if (Random.Range(1,11) <= 3)
                        {
                            if (pokemon.CanReceiveStatus(ConditionID.PAR))
                            {
                                pokemon.SetStatus(ConditionID.PAR);
                                return true;
                            }
                        }
                        return false;
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
                    OnStatsChange = (StatBoost statBoost, Pokemon source, Pokemon target) =>
                    {
                        if(statBoost.Stat == Stat.Accuracy && statBoost.Boost < 0 && source.Base.Ability.Id != AbilityID.MoldBreaker)
                        {
                            target.StatusChanges.Enqueue("Keen Eye prevents the loss of accuracy!");
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
                    OnPokemonEnterBattle = (Pokemon source, Pokemon target) =>
                    {
                        StatBoost intimidate = new StatBoost{
                            Stat = Stat.Attack,
                            Boost = -1
                        };

                        if(AbilityManager.Instance.CanReceiveBoost(intimidate, source, target))
                        {
                            target.ApplyBoost(intimidate);
                        }
                        else
                        {
                            target.StatusChanges.Enqueue($"{ target.Base.Name } cannot have its Attack reduced!");
                        }
                    }
                }
            },
            {
                AbilityID.Drought,
                new Ability()
                {
                    Name = "Drought",
                    OnPokemonEnterBattle = (Pokemon source, Pokemon target) =>
                    {
                        AbilityManager.Instance.StartCoroutine(AbilityManager.Instance.WeatherManager.ChangeWeather(WeatherDB.Weathers[WeatherID.Sunny]));
                    }
                }
            },
            {
                AbilityID.InnerFocus,
                new Ability()
                {
                    Name = "Inner Focus",
                    OnFlinch = (Pokemon source, Pokemon target) =>
                    {
                        if(source.Base.Ability.Id != AbilityID.MoldBreaker)
                            return false;
                        return true;
                    }
                }
            },
            {
                AbilityID.SnowCloak,
                new Ability()
                {
                    Name = "Snow Cloak",
                    OnEvasionCheck = (Pokemon source, Pokemon target, WeatherID weatherID) =>
                    {
                        if(weatherID == WeatherID.Hail && source.Base.Ability.Id != AbilityID.MoldBreaker)
                            return 0.8f;
                        return 1f;
                    }
                }
            },
            {
                AbilityID.SandVeil,
                new Ability()
                {
                    Name = "Sand Veil",
                    OnEvasionCheck = (Pokemon source, Pokemon target, WeatherID weatherID) =>
                    {
                        if(weatherID == WeatherID.Sandstorm && source.Base.Ability.Id != AbilityID.MoldBreaker)
                            return 0.8f;
                        return 1f;
                    }
                }
            },
            {
                AbilityID.MoldBreaker,
                new Ability()
                {
                    Name = "Mold Breaker",
                    OnPokemonEnterBattle= (Pokemon source, Pokemon target) =>
                    {
                        source.StatusChanges.Enqueue($"{ source.Base.Name } breaks the mold!");
                    }
                }
            }
        };

        foreach (var keyValuePair in _abilities)
        {
            var abilityId = keyValuePair.Key;
            var ability = keyValuePair.Value;

            ability.Id = abilityId;
        }
    }
}

public enum AbilityID
{
    Blaze,
    Overgrow,
    Torrent,
    PoisonPoint,    
    Static, 
    CompoundEyes,
    KeenEye,
    Guts,
    Overcoat,
    Intimidate,
    Drought,
    InnerFocus,
    SnowCloak,
    SandVeil,
    MoldBreaker
}