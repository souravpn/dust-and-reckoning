# Architecture overview

Dust & Reckoning is structured around a small set of long-lived **Manager** systems that communicate exclusively through an **EventBus** (publish/subscribe). No manager holds a direct reference to another. Scene-specific scripts communicate upward via events; managers push state changes back down via the same bus or through ScriptableObject data.

## System map

```
Bootstrap (scene)
  └── Initializes all managers in dependency order
        ├── EventBus           (global singleton, no MonoBehaviour)
        ├── SaveSystem         (load/write save file)
        ├── GameManager        (game state machine: MainMenu / Playing / Paused / Cutscene)
        ├── SceneLoader        (async additive scene loading)
        ├── AudioManager       (FMOD Studio integration, bank loading)
        ├── ReputationManager  (4-faction reputation state)
        ├── WorldStateManager  (day/night, weather, NPC schedule tick)
        └── EvidenceJournal    (clue graph, persistent across scenes)
```

## Key design rules

1. **EventBus only between managers.** `EventBus.Publish(new X())` and `EventBus.Subscribe<X>(handler)`. No `FindObjectOfType`, no singletons accessed via static property (except EventBus itself).

2. **ScriptableObjects for data.** Character stats, faction thresholds, item definitions, quest objectives — all live in `.asset` files. Scripts read them; they never write to them at runtime.

3. **Additive scene loading.** The world scene (Blackwood, etc.) is loaded additively on top of Bootstrap. UI lives in its own additive scene. This keeps load times fast and managers persistent without DontDestroyOnLoad gymnastics.

4. **NPC brain is a state machine.** Each NPC runs a lightweight FSM: `Idle → Scheduled → Talking → Alarmed`. The `NPCScheduler` advances scheduled states based on `WorldStateManager` time. Dialogue transitions happen via `DialogueTrigger` events.

5. **Dialogue drives story state.** Yarn Spinner dialogue files contain Yarn commands (`<<set_reputation Law -10>>`, `<<unlock_clue CalebMarshBody>>`) that fire into the EventBus. This keeps story logic out of C# and in the dialogue scripts where writers can edit it.

## Save system

Saves are JSON files written to `Application.persistentDataPath`. The schema is versioned — see `Docs/Tech/SaveFormat.md`. Save state includes:
- Current chapter and scene
- All reputation values
- Discovered clues (by ID)
- NPC memory flags (per-NPC dictionary of conversation outcomes)
- Player inventory
- World state (time of day, weather seed)

Auto-save triggers: chapter transitions, sleeping, fast travel. Manual save: any time from the pause menu.
