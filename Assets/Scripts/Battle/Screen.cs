using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screen
{
    public ScreenType Id { get; set; }
    public string Name { get; set; }    
    public int Duration { get; set; }
    public string PlayerStartMessage { get; set; }
    public string EnemyStartMessage { get; set; }
    public string PlayerEndMessage { get; set; }
    public string EnemyEndMessage { get; set; }
}
