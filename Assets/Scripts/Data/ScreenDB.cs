using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenDB
{
    static readonly Dictionary<ScreenType, Screen> _screens;
    public static IReadOnlyDictionary<ScreenType, Screen> Screens => _screens;
    static ScreenDB()
    {
        _screens = new Dictionary<ScreenType, Screen>()
        {
            {
                ScreenType.None,
                new Screen()
                {
                    Name = "None"
                }
            },
            {
                ScreenType.LightScreen,
                new Screen()
                {
                    Name = "Light Screen",
                    PlayerStartMessage = "Light Screen raised your team's Special Defense!",
                    PlayerEndMessage = "Your team's Light Screen has faded.",
                    EnemyStartMessage = "Light Screen raised enemy's team Special Defense!",
                    EnemyEndMessage = "Enemy's Light Screen has faded."
                }
            },
            {
                ScreenType.Reflect,
                new Screen()
                {
                    Name = "Reflect",
                    PlayerStartMessage = "Reflect raised your team's Defense!",
                    PlayerEndMessage = "Your team's Reflect has faded.",
                    EnemyStartMessage = "Reflect raised enemy's team Defense!",
                    EnemyEndMessage = "Enemy's Reflect has faded."
                }
            },
            {
                ScreenType.AuroraVeil,
                new Screen()
                {
                    Name = "Aurora Veil",
                    PlayerStartMessage = "Aurora Veil raised your team's Defense and Special Defense!",
                    PlayerEndMessage = "Your team's Aurora Veil has faded.",
                    EnemyStartMessage = "Aurora Veil raised enemy's team Defense and Special Defense!",
                    EnemyEndMessage = "Enemy's Aurora Veil has faded."
                }
            }
        };

        // Loop through the screens and set their Id property
        foreach (var keyValuePair in _screens)
        {
            var screenType = keyValuePair.Key;
            var screen = keyValuePair.Value;

            screen.Id = screenType;
        }
    }
}

public enum ScreenType 
{ 
    None,
    LightScreen,
    Reflect,
    AuroraVeil
}
