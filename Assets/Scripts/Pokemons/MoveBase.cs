using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] new string name;
    [TextArea] [SerializeField] string description;
    [SerializeField] PokemonType type;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveVariation variation;
    [SerializeField] MoveTarget target;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] int priority;
    [SerializeField] int recoil;
    [SerializeField] bool makesContact;
    [SerializeField] bool bypassAccuracy;    
    [SerializeField] bool twoTurnMove;    
    [SerializeField] bool mustRecharge;    
    [SerializeField] bool hpDrainingMove;    
    [SerializeField] string onCastMessage;    
    [SerializeField] MoveEffects effects;    
    [SerializeField] List<SecondaryEffects> secondaryEffects;

    public string Name => name;
    public string Description => description;
    public PokemonType Type => type;
    public MoveCategory Category => category;
    public MoveVariation Variation => variation;
    public MoveTarget Target => target;
    public int Power => power;
    public int Accuracy => accuracy;
    public int PP => pp;
    public int Priority => priority;
    public int Recoil => recoil;
    public bool MakesContact => makesContact;
    public bool BypassAccuracy => bypassAccuracy;
    public bool TwoTurnMove => twoTurnMove;
    public bool MustRecharge => mustRecharge;
    public bool HPDrainingMove => hpDrainingMove;
    public string OnCastMessage => onCastMessage;
    public MoveEffects Effects => effects;
    public List<SecondaryEffects> SecondaryEffects => secondaryEffects;
}

public enum MoveCategory { Physical, Special, Status }
public enum MoveVariation { None, Slicing, Punch, Spore, Claw, Biting }
public enum MoveTarget { Foe, Self }

[System.Serializable]
public class MoveEffects 
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;
    [SerializeField] WeatherID weatherEffect;
    [SerializeField] ScreenType screenType;
    [SerializeField] bool flinch;

    public List<StatBoost> Boosts => boosts;
    public ConditionID Status => status;
    public ConditionID VolatileStatus => volatileStatus;
    public WeatherID WeatherEffect => weatherEffect;
    public ScreenType ScreenType => screenType;
    public bool Flinch => flinch;
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int procChance;
    [SerializeField] MoveTarget target;

    public int ProcChance => procChance;
    public MoveTarget Target => target;
}

[System.Serializable]
public class StatBoost
{
    [SerializeField] private Stat stat;
    [SerializeField] private int boost;
    public Stat Stat { get { return stat; } set { stat = value; } }
    public int Boost { get { return boost; } set { boost = value; } }
}