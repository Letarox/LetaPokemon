using System.Collections;
using System.Collections.Generic;
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
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatsBoost { get; private set; }
    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();

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
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpecialAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpecialDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);
        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10;
    }

    void ResetStatBoost()
    {
        StatsBoost = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 },
            { Stat.Accuracy, 0 },
            { Stat.Evasion, 0 }
        };
    }

    int GetStat(Stat stat)
    {
        int statValue = Stats[stat];

        int boostCount = StatsBoost[stat];
        float[] positiveBoostValus = new float[] { 2/2f, 3/2f, 4/2f, 5/2f, 6/2f, 7/2f, 8/2f };
        float[] negativeBoostValues = new float[] { 2/2f, 2/3f, 2/4f, 2/5f, 2/6f, 2/7f, 2/8f };

        if (boostCount >= 0)
            statValue = Mathf.FloorToInt(statValue * positiveBoostValus[boostCount]);
        else
            statValue = Mathf.FloorToInt(statValue * negativeBoostValues[-boostCount]);

        return statValue;
    }

    float GetAccuracyEvasion(Stat stat)
    {
        int boostCount = StatsBoost[stat];
        float[] positiveBoostValus = new float[] { 3f/3f, 4f/3f, 5f/3f, 6f/3f, 7f/3f, 8f/3f, 9f/3f };
        float[] negativeBoostValues = new float[] { 3f/3f, 3f/4f, 3f/5f, 3f/6f, 3f/7f, 3f/8f, 3f/9f };

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

    public void ApplyBoost(List<StatBoost> statBoosts)
    {
        foreach(StatBoost statBoost in statBoosts)
        {
            Stat stat = statBoost.stat;
            int boost = statBoost.boost;

            if ((boost > 0 && StatsBoost[stat] < 6) || (boost < 0 && StatsBoost[stat] > -6))
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
            else
            {
                if (boost > 0)
                    StatusChanges.Enqueue($"{ Base.Name }'s { stat } cannot go any higher!");
                else
                    StatusChanges.Enqueue($"{ Base.Name }'s { stat } cannot go any lower!");
            }
            StatsBoost[stat] = Mathf.Clamp(StatsBoost[stat] + boost, -6, 6);
        }
    }

    public int MaxHp{ get; private set; }

    public int Attack { get { return GetStat(Stat.Attack); } }

    public int Defense { get { return GetStat(Stat.Defense); } }

    public int SpecialAttack { get { return GetStat(Stat.SpAttack); } }

    public int SpecialDefense { get { return GetStat(Stat.SpDefense); } }

    public int Speed { get { return GetStat(Stat.Speed); } }

    public float Accuracy { get { return GetAccuracyEvasion(Stat.Accuracy); } }
    public float Evasion { get { return GetAccuracyEvasion(Stat.Evasion); } }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        DamageDetails damageDetails = new DamageDetails()
        {
            Fainted = false,
            TypeEffectiveness = TypeChart.GetEffectiveness(move.Base.Type, Base.PrimaryType) * TypeChart.GetEffectiveness(move.Base.Type, Base.SecondaryType)
        };
        
        damageDetails.Critical = (Random.Range(1, 257) <= 16) ? 2f : 1f;
        float stab = (attacker.Base.PrimaryType == move.Base.Type || attacker.Base.SecondaryType == move.Base.Type) ? 1.5f : 1f;
        float modifiers = Random.Range(0.85f, 1f) * damageDetails.TypeEffectiveness * damageDetails.Critical * stab;
        int offense = (move.Base.Category == MoveCategory.Physical) ? attacker.Attack : attacker.SpecialAttack;
        int defense = (move.Base.Category == MoveCategory.Physical) ? Defense : SpecialDefense;
        int baseDamage = ((2 * attacker.Level / 5 + 2) * move.Base.Power * offense / defense) / 50 + 2;
        int damage = Mathf.FloorToInt(baseDamage * modifiers);
        HP -= damage;

        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }

    public Move GetRandomMove()
    {
        int random = Random.Range(0, Moves.Count);
        return Moves[random];
    }

    public void OnBattleOver()
    {
        ResetStatBoost();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}