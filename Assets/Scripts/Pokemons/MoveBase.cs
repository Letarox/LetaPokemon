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

    public string Name { get { return name; } }
    public string Description { get { return description; } }
    public PokemonType Type { get { return type; } }
    public MoveCategory Category { get { return category; } }
    public MoveTarget Target { get { return target; } }    
    public int Power { get { return power; } }
    public int Accuracy { get { return accuracy; } }
    public int PP { get { return pp; } }
    public int Priority { get { return priority; } }
    public int Recoil { get { return recoil; } }
    public bool MakesContact { get { return makesContact; } }
    public bool BypassAccuracy { get { return bypassAccuracy; } }
    public bool TwoTurnMove { get { return twoTurnMove; } }
    public bool MustRecharge { get { return mustRecharge; } }
    public bool HPDrainingMove { get { return hpDrainingMove; } }
    public string OnCastMessage { get { return onCastMessage; } }
    public MoveEffects Effects { get { return effects; } }    
    public List<SecondaryEffects> SecondaryEffects { get { return secondaryEffects; } }
   
}

public enum MoveCategory { Physical, Special, Status }
public enum MoveTarget { Foe, Self }

[System.Serializable]
public class MoveEffects 
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;
    [SerializeField] WeatherID weatherEffect;

    public List<StatBoost> Boosts { get { return boosts; } }
    public ConditionID Status { get { return status; } }
    public ConditionID VolatileStatus { get { return volatileStatus; } }
    public WeatherID WeatherEffect { get { return weatherEffect; } }
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int procChance;
    [SerializeField] MoveTarget target;

    public int ProcChance{ get { return procChance; } }
    public MoveTarget Target{ get { return target; } }
}

[System.Serializable]
public class StatBoost 
{
    public Stat stat;
    public int boost;
}
