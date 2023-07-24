using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherDB
{
    static readonly Dictionary<WeatherID, Weather> _weathers;
    public static IReadOnlyDictionary<WeatherID, Weather> Weathers => _weathers;
    static WeatherDB()
    {
        _weathers = new Dictionary<WeatherID, Weather>()
        {
            {
                WeatherID.None,
                new Weather()
                {
                    Name = "None"
                }
            },
            {
                WeatherID.Sunny,
                new Weather()
                {
                    Name = "Harsh Sunlight",
                    StartMessage = "The sunlight is harsh!",
                    CastMessage = "The sunlight turned harsh!",
                    EndMessage = "The harsh sunlight faded."
                }
            },
            {
                WeatherID.Rain,
                new Weather()
                {
                    Name = "Rain",
                    StartMessage = "It's raining!",
                    CastMessage = "It started to rain!",
                    EndMessage = "The rain stopped."
                }
            },
            {
                WeatherID.Sandstorm,
                new Weather()
                {
                    Name = "Sandstorm",
                    StartMessage = "The sandstorm is raging!",
                    CastMessage = "A sandstorm kicked up!",
                    EndMessage = "The sandstorm subsided.",
                    OnAfterTurn = (Pokemon pokemon) =>
                    {
                        if (TypeChart.GetWeatherEffectiveness(pokemon.Base.PrimaryType) != WeatherID.Sandstorm && TypeChart.GetWeatherEffectiveness(pokemon.Base.SecondaryType) != WeatherID.Sandstorm && pokemon.Base.Ability.Id != AbilityID.Overcoat)
                        {
                            pokemon.UpdateHP(Mathf.RoundToInt(Mathf.Clamp(pokemon.MaxHp / 16, 1, pokemon.MaxHp / 15)));
                            pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } is buffeted by the sandstorm!");
                            return true;
                        }
                        return false;
                    }
                }
            },
            {
                WeatherID.Hail,
                new Weather()
                {
                    Name = "Hail",
                    StartMessage = "It's hailing!",
                    CastMessage = "It started to hail!",
                    EndMessage = "The hail stopped.",
                    OnAfterTurn = (Pokemon pokemon) =>
                    {
                        if (TypeChart.GetWeatherEffectiveness(pokemon.Base.PrimaryType) != WeatherID.Hail && TypeChart.GetWeatherEffectiveness(pokemon.Base.SecondaryType) != WeatherID.Hail && pokemon.Base.Ability.Id != AbilityID.Overcoat)
                        {
                            pokemon.UpdateHP(Mathf.RoundToInt(Mathf.Clamp(pokemon.MaxHp / 16, 1, pokemon.MaxHp / 15)));
                            pokemon.StatusChanges.Enqueue($"{ pokemon.Base.Name } is buffeted by the hail!");
                            return true;
                        }
                        return false;
                    }
                }
            }
        };

        foreach (var conditionPair in _weathers)
        {
            conditionPair.Value.Id = conditionPair.Key;
        }
    }
}
public enum WeatherID
{
    None,
    Sunny,
    Rain,
    Sandstorm,
    Hail
}