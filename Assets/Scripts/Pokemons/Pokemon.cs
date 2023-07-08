using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public PokemonBase Base { get { return _base; } }
    public int Level { get { return level; } }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatsBoost { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
    public bool HPChanged { get; set; }
    public bool CanAttack { get; set; }
    public event System.Action<Condition> OnStatusChanged;
    public bool TwoTurnMove { get; set; }
    public bool MustRecharge { get; set; }
    public int MaxHp { get; private set; }
    public int Attack { get { return GetStat(Stat.Attack); } }
    public int Defense { get { return GetStat(Stat.Defense); } }
    public int SpecialAttack { get { return GetStat(Stat.SpAttack); } }
    public int SpecialDefense { get { return GetStat(Stat.SpDefense); } }
    public int Speed { get { return GetStat(Stat.Speed); } }
    public float Accuracy { get { return GetAccuracyEvasion(Stat.Accuracy); } }
    public float Evasion { get { return GetAccuracyEvasion(Stat.Evasiveness); } }
    public float Critical { get { return GetCritChance(Stat.Critical); } }

    public void Init()
    {
        //Create a blank list for all moves and filter through all learnable moves from that pokemon. We add to the list up to the first 4 learnable moves from that pokemon, then we break
        Moves = new List<Move>();
        foreach(var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.MoveBase));

            if (Moves.Count >= 4)
                break;
        }
        CalculateStats();
        HP = MaxHp;
        ResetStatBoost();
        CureStatus();
        CureVolatileStatus();
        CanAttack = true;
        TwoTurnMove = false;
        MustRecharge = false;
    }

    void CalculateStats()
    {
        //calculate the stat for each of the following Stats and assign it to the Dictionary
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpecialAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpecialDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);
        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level;
    }

    public void ResetStatBoost()
    {
        //resets all stat changes
        StatsBoost = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 },
            { Stat.Accuracy, 0 },
            { Stat.Evasiveness, 0 },
            { Stat.Critical, 0 }
        };
    }

    int GetStat(Stat stat)
    {
        //Get the value of stat for Attack/Defense/SpAttack/SpDefense/Speed from the Dictionary. Gets the value for the amount of boosts for that stat. 
        //It creates the values for each stat change (including zero) for both positive and negative values. 
        //Returns the value according the the stat level
        int statValue = Stats[stat];
        int boostCount = StatsBoost[stat];
        float[] positiveBoostValus = new float[] { 2/2f, 3/2f, 4/2f, 5/2f, 6/2f, 7/2f, 8/2f };
        float[] negativeBoostValues = new float[] { 2/2f, 2/3f, 2/4f, 2/5f, 2/6f, 2/7f, 2/8f };

        if (boostCount >= 0)
            statValue = Mathf.FloorToInt(statValue * positiveBoostValus[boostCount]);
        else
            statValue = Mathf.FloorToInt(statValue * negativeBoostValues[-boostCount]);

        if (Status == ConditionDB.Conditions[ConditionID.PAR])
            return Mathf.FloorToInt(statValue * 0.5f);

        return statValue;
    }

    float GetAccuracyEvasion(Stat stat)
    {
        //Get the value of stat boosts for either Evasion or Accuracy. It creates the values for each stat change (including zero) for both positive and negative values. 
        //Returns the value according the the stat level
        int boostCount = StatsBoost[stat];
        float[] positiveBoostValus = new float[] { 3f/3f, 4f/3f, 5f/3f, 6f/3f, 7f/3f, 8f/3f, 9f/3f };
        float[] negativeBoostValues = new float[] { 3f/3f, 3f/4f, 3f/5f, 3f/6f, 3f/7f, 3f/8f, 3f/9f };


        //returns the normal values for Accuracy and the opposite for Evasion, since Evasion works for the Pokemon itself, forcing the enemy to have reduced chance through increased boosts
        if(stat == Stat.Accuracy)
        {
            if (boostCount >= 0)
                return positiveBoostValus[boostCount];
            else
                return negativeBoostValues[-boostCount];
        }
        else
        {
            if (boostCount >= 0)
                return negativeBoostValues[boostCount]; 
            else
                return positiveBoostValus[-boostCount];
        }
    }

    float GetCritChance(Stat stat)
    {
        //Get the value of the stat boost related to the Critical of the Pokemon. Returns the boost level of that value
        int boostCount = StatsBoost[stat];
        float[] positiveBoostValus = new float[] { 1f / 24f, 1f / 8f, 1f / 2f, 1f, 1f, 1f, 1f };

        return positiveBoostValus[boostCount];
    }

    public void ApplyBoost(StatBoost statBoost)
    {
        //check for all the possible stat boosts from a move. Individually it provides the message on the amount of stat changes the move made. 
        //If the affected stat is at its maximum stat (-6/6) returns the message that it couldn't update it. The stat is later clamped to make sure it will never be lower than -6 or higher than 6
        Stat stat = statBoost.stat;
        int boost = statBoost.boost;

        if ((boost > 0 && StatsBoost[stat] < 6) || (boost < 0 && StatsBoost[stat] > -6))
        {
            if(stat == Stat.Critical)
            {
                if (StatsBoost[stat] > 0)
                {
                    StatusChanges.Enqueue("But it failed!");
                    return;
                }
                else
                    StatusChanges.Enqueue($"{ Base.Name }'s is getting pumped!");
            }
            else
            {
                switch (boost)
                {
                    case 1:
                        StatusChanges.Enqueue($"{ Base.Name }'s { stat } rose!");
                        break;
                    case 2:
                        StatusChanges.Enqueue($"{ Base.Name }'s { stat } sharply rose!");
                        break;
                    case -1:
                        StatusChanges.Enqueue($"{ Base.Name }'s { stat } fell!");
                        break;
                    case -2:
                        StatusChanges.Enqueue($"{ Base.Name }'s { stat } sharply fell!");
                        break;
                    default:
                        if (boost > 0)
                            StatusChanges.Enqueue($"{ Base.Name }'s { stat } drastically rose!");
                        else
                            StatusChanges.Enqueue($"{ Base.Name }'s { stat } drastically fell!");
                        break;
                }
            }
        }
        else
        {
            if (boost > 0)
                StatusChanges.Enqueue($"{ Base.Name }'s { stat } cannot go any higher!");
            else
                StatusChanges.Enqueue($"{ Base.Name }'s { stat } cannot go any lower!");
        }
        StatsBoost[stat] = Mathf.Clamp(StatsBoost[stat] + boost, -6, 6);
    }

    public bool CanReceiveStatus(ConditionID conditionID)
    {
        //check if the target already has a condition
        if (Status != null)
            return false;

        //check if the pokemon is immune to the Status Condition. If it is, does not allow the target to receive it
        if(TypeChart.GetConditionImmunity(Base.PrimaryType) == conditionID || TypeChart.GetConditionImmunity(Base.SecondaryType) == conditionID)
            return false;

        return true;
    }
    

    public void SetStatus(ConditionID conditionID)
    {
        //Search on the Dictionary if either type can be affected by the current ConditionID
        if(CanReceiveStatus(conditionID) && conditionID != ConditionID.None)
        {
            Status = ConditionDB.Conditions[conditionID];
            Status?.OnStart?.Invoke(this);
            StatusChanges.Enqueue($"{ Base.Name } { Status.StartMessage }");
            OnStatusChanged?.Invoke(ConditionDB.Conditions[conditionID]);
        }
    }
    public void CureStatus()
    {
        //Removes the current Status Condition and invokes the OnStatusChanged for UI Update
        Status = null;
        OnStatusChanged?.Invoke(null);
    }
    public void SetVolatileStatus(ConditionID conditionID)
    {
        //Search on the Dictionary for the conditionID of the move that this was called from. It applies the effect to this pokemon and call its messages
        VolatileStatus = ConditionDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges?.Enqueue($"{ Base.Name } { VolatileStatus.StartMessage }");
    }
    public void CureVolatileStatus()
    {
        //Removes the current Volatile Status Condition
        VolatileStatus = null;
    }
    public void UpdateHP(int damage)
    {
        //reduces the HP based on received damage, the HP cannot be set to lower than 0
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        HPChanged = true;
    }
    public void RegainHP(int healthRestored)
    {
        //regains health based on the healthRestored value
        HP += Mathf.Clamp(healthRestored, 1, MaxHp - HP);
        HPChanged = true;
    }

    public int TakeConfusionDamage()
    {
        //Applies the whole formula of damage, calculating all instances that can impact the outcome of the damage
        float modifiers = Random.Range(0.85f, 1f);
        int baseDamage = ((2 * Level / 5 + 2) * 40 * Attack / Defense) / 50 + 2;
        int damage = Mathf.FloorToInt(baseDamage * modifiers);
        return damage;
    }
    

    public bool OnBeforeMove()
    {
        CanAttack = true;

        //if the status doesn't have the OnBeforeMove Func, it will return null
        if(Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
                CanAttack = false;
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
                CanAttack = false;
        }

        return CanAttack;
    }
    public void OnAfterTurn()
    {
        //if the status has the OnAfterTurn Action, it will Invoke it
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }
    public Move GetRandomMove()
    {
        //Creates a list of possible moves to use that still has PP. Generate a random number between 0 and the number of moves the enemy has learned. Returns a random move
        List<Move> movesWithPP = Moves.Where(m => m.PP > 0).ToList();
        
        if(movesWithPP.Count > 0)
        {
            int random = Random.Range(0, movesWithPP.Count);
            return movesWithPP[random];
        }
        else
        {
            return new Move(Base.Struggle);
        }
    }

    public void OnBattleOver()
    {
        //if the battle is over, resets both Volatile Status and all Stat Boosts
        CureVolatileStatus();
        ResetStatBoost();
    }
}