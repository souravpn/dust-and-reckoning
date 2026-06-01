/// <summary>
/// Central home for all game-wide enumerations.
/// Add new values here rather than defining enums inline in other files.
/// </summary>

public enum GameState
{
    Initializing,
    MainMenu,
    Loading,
    Playing,
    Dialogue,
    Inventory,
    Journal,
    Map,
    Paused,
    Cutscene,
    GameOver
}

public enum Faction
{
    Law,
    Outlaws,
    Townsfolk,
    Shoshone
}

public enum ReputationTier
{
    Hostile,    // -100 to -60
    Cold,       // -59 to -20
    Neutral,    // -19 to +19
    Warm,       // +20 to +59
    Allied      // +60 to +100
}

public enum DialogueTone
{
    Neutral,
    Friendly,
    Suspicious,
    Confrontational
}

public enum WeatherType
{
    Clear,
    Overcast,
    Dusty,
    LightRain,
    Thunderstorm
}

public enum ClueType
{
    Document,
    Testimony,
    Observation,
    Deduction
}

public enum HorseState
{
    Idle,
    Walk,
    Trot,
    Gallop,
    Spooked,
    Dismounted
}

public enum CombatState
{
    None,
    Alert,       // Enemy suspicious
    Detected,    // Player spotted
    Active,      // Combat ongoing
    Fleeing
}

public enum InteractableType
{
    NPC,
    Item,
    Document,
    Door,
    Container,
    EnvironmentalDetail
}
