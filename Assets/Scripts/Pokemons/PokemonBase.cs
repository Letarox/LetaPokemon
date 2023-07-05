using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] new string name;
    [TextArea][SerializeField] string description;
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] PokemonType primaryType;
    [SerializeField] PokemonType secondaryType;
    [SerializeField] AbilityID ability;

    //Base Stats/Attributes
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] MoveBase struggle;
    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name { get { return name; } }

    public string Description { get { return description; } }

    public Sprite FrontSprite { get { return frontSprite; } }

    public Sprite BackSprite { get { return backSprite; } }

    public PokemonType PrimaryType { get { return primaryType; } }

    public PokemonType SecondaryType { get { return secondaryType; } }

    public int MaxHp { get { return maxHp; } }

    public int Attack { get { return attack; } }

    public int Defense { get { return defense; } }

    public int SpecialAttack { get { return spAttack; } }

    public int SpecialDefense { get { return spDefense; } }

    public int Speed { get { return speed; } }
    public Ability Ability { get { return AbilityDB.Abilities[ability]; } }
    public List<LearnableMove> LearnableMoves { get { return learnableMoves; } }
    public MoveBase Struggle { get { return struggle; } }
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase MoveBase { get { return moveBase; } }
    public int Level { get { return level; } }
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    Accuracy,
    Evasiveness,
    Critical
}


public enum PokemonType
{
    None,
    Normal,
    Fighting,
    Flying,
    Poison,
    Ground,
    Rock,
    Bug,
    Ghost,
    Steel,
    Fire,
    Water,
    Grass,
    Electric,
    Psychic,
    Ice,
    Dragon,
    Dark,
    Fairy
}

public class TypeChart
{
    static float[][] chart =
    {
        //                            NORMAL      FIGHTING        FLYING      POISON      GROUND      ROCK        BUG     GHOST       STEEL       FIRE        WATER       GRASS       ELECTRIC    PSYCHIC     ICE     DRAGON      DARK        FAIRY
        /* NORMAL */    new float[] { 1f,         1f,             1f,         1f,         1f,         0.5f,       1f,     0f,         0.5f,       1f,         1f,         1f,         1f,         1f,         1f,     1f,         1f,         1f },
        /* FIGHTING */  new float[] { 2f,         1f,             0.5f,       0.5f,       1f,         2f,         0.5f,   0f,         2f,         1f,         1f,         1f,         1f,         0.5f,       2f,     1f,         2f,         0.5f },
        /* FLYING */    new float[] { 1f,         2f,             1f,         1f,         1f,         0.5f,       2f,     1f,         0.5f,       1f,         1f,         2f,         0.5f,       1f,         1f,     1f,         1f,         1f },
        /* POISON */    new float[] { 1f,         1f,             1f,         0.5f,       0.5f,       0.5f,       1f,     0.5f,       0f,         1f,         1f,         2f,         1f,         1f,         1f,     1f,         1f,         2f },
        /* GROUND */    new float[] { 1f,         1f,             0f,         2f,         1f,         2f,         0.5f,   1f,         2f,         2f,         1f,         0.5f,       2f,         1f,         1f,     1f,         1f,         1f },
        /* ROCK */      new float[] { 1f,         0.5f,           2f,         1f,         0.5f,       1f,         2f,     1f,         0.5f,       2f,         1f,         1f,         1f,         1f,         2f,     1f,         1f,         1f },
        /* BUG */       new float[] { 1f,         0.5f,           0.5f,       0.5f,       1f,         1f,         1f,     0.5f,       0.5f,       0.5f,       1f,         2f,         1f,         2f,         1f,     1f,         2f,         0.5f },
        /* GHOST */     new float[] { 0f,         1f,             1f,         1f,         1f,         1f,         1f,     2f,         1f,         1f,         1f,         1f,         1f,         2f,         1f,     1f,         0.5f,       1f },
        /* STEEL */     new float[] { 1f,         1f,             1f,         1f,         1f,         2f,         1f,     1f,         0.5f,       0.5f,       0.5f,       1f,         0.5f,       1f,         2f,     1f,         1f,         2f },
        /* FIRE */      new float[] { 1f,         1f,             1f,         1f,         1f,         0.5f,       2f,     1f,         2f,         0.5f,       0.5f,       2f,         1f,         1f,         2f,     0.5f,       1f,         1f },
        /* WATER */     new float[] { 1f,         1f,             1f,         1f,         2f,         2f,         1f,     1f,         1f,         2f,         0.5f,       0.5f,       1f,         1f,         1f,     0.5f,       1f,         1f },
        /* GRASS */     new float[] { 1f,         1f,             0.5f,       0.5f,       2f,         2f,         0.5f,   1f,         0.5f,       0.5f,       2f,         0.5f,       1f,         1f,         1f,     0.5f,       1f,         1f },
        /* ELECTRIC */  new float[] { 1f,         1f,             2f,         1f,         0f,         1f,         1f,     1f,         1f,         1f,         2f,         0.5f,       0.5f,       1f,         1f,     0.5f,       1f,         1f },
        /* PSYCHIC */   new float[] { 1f,         2f,             1f,         2f,         1f,         1f,         1f,     1f,         0.5f,       1f,         1f,         1f,         1f,         0.5f,       1f,     1f,         0f,         1f },
        /* ICE */       new float[] { 1f,         1f,             2f,         1f,         2f,         1f,         1f,     1f,         0.5f,       0.5f,       0.5f,       2f,         1f,         1f,         0.5f,   2f,         1f,         1f },
        /* DRAGON */    new float[] { 1f,         1f,             1f,         1f,         1f,         1f,         1f,     1f,         0.5f,       1f,         1f,         1f,         1f,         1f,         1f,     2f,         1f,         0f },
        /* DARK */      new float[] { 1f,         0.5f,           1f,         1f,         1f,         1f,         1f,     2f,         1f,         1f,         1f,         1f,         1f,         2f,         1f,     1f,         0.5f,       0.5f },
        /* FAIRY */     new float[] { 1f,         2f,             1f,         0.5f,       1f,         1f,         1f,     1f,         0.5f,       0.5f,       1f,         1f,         1f,         1f,         1f,     2f,         2f,         1f }
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1f;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }

    //                                         NORMAL              FIGHTING            FLYING              POISON              GROUND              ROCK                BUG                 GHOST               STEEL               FIRE                WATER               GRASS               ELECTRIC            PSYCHIC             ICE                 DRAGON              DARK                FAIRY
    static ConditionID[] conditionImmunity = { ConditionID.None,   ConditionID.None,   ConditionID.None,   ConditionID.PSN,    ConditionID.None,   ConditionID.None,   ConditionID.None,   ConditionID.None,   ConditionID.PSN,    ConditionID.None,   ConditionID.None,   ConditionID.None,   ConditionID.PAR,    ConditionID.None,   ConditionID.FRZ,    ConditionID.None,   ConditionID.None,   ConditionID.None };
    public static ConditionID GetConditionImmunity(PokemonType defenseType)
    {
        if (defenseType == PokemonType.None)
            return ConditionID.None;

        int col = (int)defenseType - 1;

        return conditionImmunity[col];
    }

    //                                           NORMAL          FIGHTING        FLYING          POISON          GROUND               ROCK                   BUG             GHOST           STEEL                   FIRE            WATER           GRASS           ELECTRIC        PSYCHIC         ICE             DRAGON          DARK            FAIRY
    static WeatherID[] weathersImmunityChart = { WeatherID.None, WeatherID.None, WeatherID.None, WeatherID.None, WeatherID.Sandstorm, WeatherID.Sandstorm,   WeatherID.None, WeatherID.None, WeatherID.Sandstorm,    WeatherID.None, WeatherID.None, WeatherID.None, WeatherID.None, WeatherID.None, WeatherID.Hail, WeatherID.None, WeatherID.None, WeatherID.None};
    public static WeatherID GetWeatherEffectiveness(PokemonType defenseType)
    {
        if (defenseType == PokemonType.None)
            return WeatherID.None;

        int col = (int)defenseType - 1;

        return weathersImmunityChart[col];
    }
}
    
