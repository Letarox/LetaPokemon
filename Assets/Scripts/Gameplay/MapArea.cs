using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeightedPokemon
{
    [SerializeField] Pokemon pokemon;
    [SerializeField] int weight;
    public Pokemon Pokemon => pokemon;
    public int Weight => weight;
}

public class MapArea : MonoBehaviour
{
    [SerializeField] private List<WeightedPokemon> _wildPokemons;
    [SerializeField] private WeatherID _environmentWeather;

    private int[] _weights;

    public WeatherID EnvironmentWeather => _environmentWeather;

    private void Start()
    {
        InitializeWeights();
    }

    public Pokemon GetRandomWildPokemon()
    {
        int totalWeights = CalculateTotalWeights();

        int randomNumber = Random.Range(0, totalWeights);
        int accumulatedWeight = 0;

        for (int i = 0; i < _wildPokemons.Count; i++)
        {
            accumulatedWeight += _weights[i];
            if (randomNumber < accumulatedWeight)
            {
                Pokemon wildPokemon = _wildPokemons[i].Pokemon;
                wildPokemon.Init();
                return wildPokemon;
            }
        }

        return null;
    }

    private void InitializeWeights()
    {
        _weights = new int[_wildPokemons.Count];
        for (int i = 0; i < _wildPokemons.Count; i++)
        {
            _weights[i] = _wildPokemons[i].Weight;
        }
    }

    private int CalculateTotalWeights()
    {
        int totalWeights = 0;
        foreach (var weightedPokemon in _wildPokemons)
        {
            totalWeights += weightedPokemon.Weight;
        }
        return totalWeights;
    }
}