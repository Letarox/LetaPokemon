using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public List<Pokemon> Pokemons { get { return pokemons; } }

    private void Start()
    {
        foreach(Pokemon pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    public Pokemon GetHealthyPokemon()
    {
        //Filter all pokemon in our party that are healthy, and returns the first found
        return pokemons.Where(pokemon => pokemon.HP > 0).FirstOrDefault();
    }
}
