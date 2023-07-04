using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> _wildPokemons;
    [SerializeField] WeatherID _environmentWeather;

    public WeatherID EnvironmentWeather { get { return _environmentWeather; } }

    public Pokemon GetRandomWildPokemon()
    {
        Pokemon wildPokemon = _wildPokemons[Random.Range(0, _wildPokemons.Count)];
        wildPokemon.Init();
        return wildPokemon;
    }
}
