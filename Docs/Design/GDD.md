# Game Design Document — Dust & Reckoning

**Version:** 0.1 (pre-production)
**Engine:** Unity 2023 LTS · Universal Render Pipeline (URP)
**Platform:** iOS 16+ / iPadOS 16+
**Genre:** Open-world narrative RPG / Western
**Target rating:** 17+ (violence, alcohol, mature themes)

> This is a living document. Update the version number and changelog at the bottom whenever a section changes. Every system described here should have a matching implementation note in `Docs/Tech/ARCHITECTURE.md`.

---

## Table of contents

1. [Vision statement](#1-vision-statement)
2. [Player experience goals](#2-player-experience-goals)
3. [Setting & tone](#3-setting--tone)
4. [Story overview](#4-story-overview)
5. [Characters](#5-characters)
6. [World design](#6-world-design)
7. [Core systems](#7-core-systems)
   - 7.1 [Movement & camera](#71-movement--camera)
   - 7.2 [Dialogue system](#72-dialogue-system)
   - 7.3 [Reputation system](#73-reputation-system)
   - 7.4 [Investigation & evidence journal](#74-investigation--evidence-journal)
   - 7.5 [Horse & riding](#75-horse--riding)
   - 7.6 [Combat & stealth](#76-combat--stealth)
   - 7.7 [Day/night & NPC schedules](#77-daynight--npc-schedules)
   - 7.8 [Save system](#78-save-system)
8. [Mobile UX & controls](#8-mobile-ux--controls)
9. [Audio design](#9-audio-design)
10. [Visual style](#10-visual-style)
11. [Acts & chapters](#11-acts--chapters)
    - 11.1 [Act I — Blackwood (MVP)](#111-act-i--blackwood-mvp)
    - 11.2 [Act II — Red Mesa Flats (post-MVP)](#112-act-ii--red-mesa-flats-post-mvp)
12. [Progression & economy](#12-progression--economy)
13. [Accessibility](#13-accessibility)
14. [Out of scope (MVP)](#14-out-of-scope-mvp)
15. [Open questions](#15-open-questions)
16. [Changelog](#16-changelog)

---

## 1. Vision statement

**Dust & Reckoning** is a slow-burn narrative western for mobile that treats the player's phone like a campfire — something you lean into alone, at night, for thirty minutes at a time.

The game is not about being a gunslinger. It is about being a man with a past and a sealed envelope, trying to figure out who he can trust in a town where everyone is hiding something. Violence is always an option but rarely the cleanest one. The world remembers every choice, and Blackwood talks.

The single clearest reference point is not a game — it is *No Country for Old Men*: the dread that builds before anything happens, the sense that forces larger than any one person are already in motion, and the question of whether the protagonist can stay clean.

In gameplay terms the closest reference is *Red Dead Redemption 2* compressed for mobile sessions: the same environmental density and NPC authenticity, but chapter-structured so a player can make meaningful progress in twenty minutes.

---

## 2. Player experience goals

These are the feelings we are designing toward — every system should serve at least one of them.

| # | Goal | What it means in practice |
|---|------|--------------------------|
| 1 | **Belonging** | Blackwood feels real before the player understands it. NPCs have routines. The saloon has regulars. Steam still rises from the train. |
| 2 | **Weight** | Choices stay made. Walt Pruitt remembers if you called him a liar. The town talks. |
| 3 | **Dread** | Something is wrong here and the player feels it before Elias names it. Pacing, music, and NPC behavior all contribute. |
| 4 | **Agency** | The player always has at least three meaningful options. No correct path — only tradeoffs. |
| 5 | **Immersion over interface** | The world is the UI. Health is visible in Elias's posture and breathing. The map is a physical journal page. |

---

## 3. Setting & tone

### Geography

Wyoming Territory, 1862. The Pacific Railroad survey has reached this far, bringing money, violence, and federal attention to land that was Shoshone territory a generation ago. The Civil War is happening far to the east and feels abstract here — but it shapes everything: where federal troops are, what government contracts are worth, why men like Harlan Dross operate with impunity.

The game map covers roughly 400 square miles of high-desert basin, bordered by the Granite Range to the northeast and the Wind Hills to the west. Three major locations in the MVP; two more in Act II. See `Docs/Story/Acts/Act1_Blackwood.md` for full location briefs.

### Tone references

- **Narrative:** Cormac McCarthy (*No Country for Old Men*, *Blood Meridian*) — economy of language, moral ambiguity, landscape as character
- **Dialogue:** *Deadwood* (HBO) — period-accurate cadence, profanity as texture, no character is purely good or evil
- **Visual:** *There Will Be Blood*, *True Grit* (2010) — dusty warm palette, practical lighting, faces that carry history
- **Gameplay feel:** *Red Dead Redemption 2* — every action has physical weight; the world is never backdrop

### What this game is NOT

- Not a power fantasy. Elias is good in a fight but not invincible, and most situations punish guns-first approaches.
- Not a morality meter. There is no karma score. Reputation with four factions is tracked separately and independently.
- Not a completionist checklist. Side content exists to deepen the world, not to fill a percentage bar.

---

## 4. Story overview

### Premise

Elias Cole, 34, is a disgraced Pinkerton detective travelling west on the Pacific Express in June 1862. In his coat pocket is a sealed envelope addressed to a man in Blackwood, Wyoming. The name on the envelope has been torn off.

When the train arrives at Blackwood Station, Elias learns the town's only notable recent event was the death of a man named **Caleb Marsh** — found at the bottom of the Harrow Mine two days prior. Ruled an accident. Nobody seems particularly troubled by this except a half-Shoshone scout named **June Whitehorse**, who knew Marsh and believes he was pushed.

Over the course of Act I, Elias discovers that Marsh was compiling a ledger of fraudulent land deeds — deeds signed by **Harlan Dross**, a railroad development agent who is systematically forging ownership documents to clear Shoshone territory for the rail line. The ledger is hidden in the mine. Dross's hired enforcer, a man called Two-Bit Terrence, is trying to find it first.

Act I ends with Elias recovering the ledger and defeating Terrence — but the ledger reveals a second hidden truth: Elias was not sent to Blackwood to deliver an envelope. He was sent to *find* it. By whom, and why, is the question Act II answers.

### The central mystery (Act II setup)

Before Act I's final scene, a second letter arrives at Elias's hotel room. It is addressed to him directly, in handwriting he doesn't recognize, and signed only "The Widow." It reads:

> *"You found what Marsh died for. Now ask yourself who sent you to find it. The answer is the same man who killed him."*

### Themes

- **Trust and complicity.** Every person Elias works with has their own agenda. Helping them helps him — but at what cost?
- **The cost of the railroad.** Progress as violence. The land survey is not a neutral act.
- **What men run from.** Elias's past is revealed in fragments. By Act II the player understands why he was disgraced — and whether it was deserved.

---

## 5. Characters

### Elias Cole — protagonist

- **Age:** 34
- **Background:** Former Pinkerton detective, discharged under circumstances not fully disclosed at game start. Traveled west after the discharge. Competent, taciturn, capable of genuine warmth but defaults to observation.
- **Player expression:** Elias's personality is partly shaped by player choices — a player who consistently chooses confrontational dialogue options will find NPCs respond to him differently than one who chooses empathy. But Elias always has a core: he is not cruel, and he is not dishonest. Those are guardrails, not a straitjacket.
- **Arc:** Elias came west to disappear. Blackwood won't let him.

### June Whitehorse — primary ally

- **Age:** 28
- **Background:** Half-Shoshone, half-Scottish. Grew up between two worlds and fully accepted by neither. Works as a scout and trail guide for the railroad survey — the only job that pays — while watching it destroy land she knows intimately.
- **Role:** Skills-based ally. She knows the terrain, reads tracks, speaks Shoshone. In Act I she is cautious with Elias but pragmatic — she needs his access to town, he needs her knowledge of the land.
- **Relationship tension:** June knows more than she tells Elias at first. She had her own reasons for watching Caleb Marsh. When those reasons surface, the player must decide whether the relationship survives.

### Harlan Dross — Act I antagonist

- **Age:** 52
- **Background:** Railroad development agent, technically an employee of the Pacific Survey Commission. In practice, a land speculator using the survey as cover. Educated, charming in formal settings, utterly ruthless in private. He does not get his hands dirty.
- **Design principle:** Dross is not a villain because he is evil. He is a villain because he is *efficient*. He has calculated that Caleb Marsh's life was worth less than the land the ledger threatened. He would make the same calculation again.
- **First appearance:** Chapter 2, The Sawdust & Rye. He is having dinner alone. He acknowledges Elias, asks one pointed question, and leaves. The player should feel watched.

### Walt Pruitt — sheriff, ambiguous

- **Age:** 47
- **Background:** Appointed sheriff of Blackwood eight months ago, after his predecessor died "of a fever." Pruitt is not corrupt in the simple sense — he does not take money from Dross. He takes *inaction*. He has a family in Cheyenne he is trying to keep alive.
- **Role:** Pruitt is the player's clearest indicator of how Blackwood's power structure works. He can be an ally if Elias earns his trust carefully. He will never move directly against Dross. Understanding why that is — and accepting it or not — is one of Act I's central emotional beats.

### Two-Bit Terrence — Act I boss

- **Age:** 38
- **Background:** Dross's enforcer. No last name anyone knows. Called Two-Bit because he once killed a man over a two-bit debt and seemed proud of it. Former Union Army irregular. Precise, unhurried, and genuinely dangerous.
- **Boss design:** See [Act I Chapter 3](#chapter-3--dead-mans-errand). Terrence is not a bullet-sponge. He is intelligent. The encounter is designed to be solvable without a direct gunfight if the player has paid attention.

### "The Widow" — Act II antagonist (introduced Act I end)

- **Age:** Unknown
- **Background:** Unknown at Act I close. The player knows only a name and a letter. She knows exactly who Elias is and why he came to Blackwood.
- **Design principle:** The Widow must feel genuinely dangerous before the player knows anything about her. The letter accomplishes this by demonstrating she knows things she should not.

### Supporting cast (Act I)

| Name | Role | Notes |
|------|------|-------|
| Rev. Solomon Voss | Tutorial NPC on the train | Nervous, sweating. Something in his bag he won't discuss. |
| Pearl Dancy | Saloon owner, The Sawdust & Rye | Sharp, fair, knows everything that happens in Blackwood |
| Looks-Twice | Shoshone elder (Act II foreshadowing) | Brief appearance in Ch. 3; expands in Act II |
| Doc Aldridge | Town physician | Signed off on Caleb Marsh's death certificate. Reluctantly. |
| Cora Marsh | Caleb's widow | Lives above the general store. Doesn't trust strangers. |

---

## 6. World design

### Design philosophy

The world is hand-crafted, not procedural. Every building in Blackwood has a reason to exist. Every NPC has a schedule because of who they are, not because a system assigned one. The open world is used for travel and atmosphere — the story beats happen in locations the player walks into, not random encounters.

**Density over scale.** Blackwood is a town of ~400 people. That is small enough to feel known — the player should recognize faces after a few hours. The terrain outside town is vast but not featureless: every half-hour ride has a landmark, a ruin, or a story.

### Locations (MVP — Act I)

#### Blackwood Station & Main Street

The entry point. The train platform runs east–west along the south edge of town. Main Street is 300 meters of false-fronted buildings: the saloon, the general store, the land office (Dross's base), the doctor's surgery, the sheriff's office, a livery stable, and a church under construction.

Key design intent: the player's first walk down Main Street should feel like every western town they have ever imagined — and then slightly wrong. The land office has new glass windows. The church has no congregation. Walt Pruitt is watching from his office door but not moving toward the player.

**Interactable objects (Ch. 1 tutorial):**
- Luggage rack on the train (newspaper — exposition)
- Notice board at the station (wanted poster, land auction notice, a missing person flyer for "C. Marsh")
- Water trough (Elias drinks, comments on the taste — world flavor)
- Blacksmith's sign (foreshadows a later clue)

#### The Sawdust & Rye (Saloon & Hotel)

Two-story wooden building, northeast corner of Main Street. Ground floor: bar, poker tables, a small stage (no performer tonight). Second floor: six hotel rooms, Pearl Dancy's office. The bar stays open until 2am — NPCs cycle through on schedule.

Key interactables: the bar (order a drink — opens Pearl dialogue), the notice board by the stairs (room rates, house rules, a handwritten note that will matter later), the poker table (optional mini-game, post-MVP).

#### Harrow Mine (outskirts, ~1.5hr ride northwest)

An abandoned silver mine that produced for eight years before the vein ran out in 1859. Caleb Marsh was using the foreman's office as a dead drop. The mine has three accessible areas: the entrance plaza (open), the main shaft (requires a lantern), and the foreman's office (locked — requires picking or a key from Doc Aldridge).

Design intent for Ch. 3: the mine should feel genuinely dangerous before Terrence appears. The environment does the work — unstable timbers, the sound of settling rock, the way the lantern light moves. Terrence's arrival should feel inevitable, not surprising.

### Locations (Act II — post-MVP)

#### Red Mesa Flats

~3hr ride east from Blackwood. High desert plateau, ochre and rust. The railroad survey camp is here — 40 men, a telegraph line under construction, Dross's surveying equipment. The Shoshone winter camp is a 45-minute ride further north.

#### Bitter Creek

~2hr ride from Red Mesa, following the abandoned Overland Mail route. A telegraph relay station that closed in 1861 when the Pony Express folded. The equipment is still here — including, it turns out, a functioning telegraph line that someone has been maintaining.

### Environmental storytelling rules

1. **Every interior tells its occupant's story.** Doc Aldridge's surgery has a half-empty whiskey bottle in the medicine cabinet. Pearl Dancy's office has a Wells Fargo lockbox and a letter from a sister in St. Louis. Walt Pruitt's desk has a child's drawing tucked under the blotter.
2. **The railroad's presence is felt before it is seen.** Survey stakes at the edge of town. A new-timber smell on buildings Dross owns. A Shoshone burial marker that has been moved.
3. **Night changes the town.** NPCs who are cautious by day become candid by night. The saloon's back room is accessible after 11pm. Dross's land office light is on at 3am.

---

## 7. Core systems

### 7.1 Movement & camera

#### On foot

Third-person follow camera, right-thumb drag to look. The camera sits slightly behind and above Elias's right shoulder — close enough to feel intimate, far enough to show the environment.

**Movement states:**

| State | Input | Transition |
|-------|-------|-----------|
| Walk | Joystick < 50% | Default |
| Jog | Joystick > 50% | Auto |
| Sprint | Double-tap joystick | 3s max, stamina cost |
| Crouch | Y button | Toggle; movement speed halved |
| Crouch-move | Y + joystick | Stealth state |

No jump. This is deliberate — the absence of a jump button forces the player to treat the environment as a space to navigate, not a platforming course.

**Interaction:** Any interactable within 2 meters shows a floating prompt (minimal — icon only, no button mashing). Single tap to interact. Hold to examine. The game never makes the player mash a button.

#### Camera behavior

- Auto-recenters behind Elias after 2 seconds of no camera input
- Collision detection prevents clipping into walls; camera pulls in smoothly
- In dialogue: cuts to a two-shot, then over-the-shoulder during player's reply selection
- In stealth: camera lowers and moves closer; FOV narrows slightly (tension cue)
- On horse: pulls back to show more terrain; responds to horse speed

### 7.2 Dialogue system

Built on **Yarn Spinner 3.x**. All dialogue lives in `.yarn` files in `Assets/Dialogue/`. C# logic is kept out of dialogue files except via Yarn commands.

#### Core design rules

1. **Show intent, not script.** Reply options display the player's *goal*, not exact words. "Push him on the debt" not "Did Caleb owe you money?" Elias improvises the actual phrasing — this keeps the performance feeling natural and prevents players from gaming "correct" answers.

2. **Three options, usually.** Most beats offer: a direct/confrontational option, a cautious/observational option, and an empathetic/deflecting option. Occasionally only two options make sense. Never more than four — cognitive load on mobile.

3. **Tone memory per NPC.** Each NPC tracks the dominant tone Elias has used with them (Suspicious / Neutral / Friendly). This is visible as a small colored bar in the dialogue UI. It affects what options are available in future conversations and how NPCs describe Elias to others.

4. **Silence is a choice.** "Say nothing. Watch his eyes." appears as an option whenever it is genuinely meaningful — not as filler, but as a way to learn something. Some NPCs react more honestly to silence than to questions.

5. **No dialogue is wasted.** Every conversation either advances plot knowledge, changes a reputation value, unlocks a clue, or reveals character. If a conversation does none of these things, it should not exist.

#### Yarn commands (bridge to game systems)

```yarn
// Reputation changes
<<reputation_change Law -10>>
<<reputation_change Shoshone +5>>

// Evidence journal
<<unlock_clue CalebMarshDeath>>
<<connect_clues CalebMarshDeath HarrowMineLedger>>

// World state flags
<<set_flag SheriffTrustEstablished true>>
<<set_flag DrssFirstMeeting true>>

// Audio
<<music_state Tension>>
<<play_sfx DoorSlam>>
```

#### Conversation flow

```
[Trigger zone entered]
    → Camera cuts to two-shot
    → NPC greeting (based on tone history + flags)
    → Player reply options (2–4)
    → NPC response + optional Yarn command
    → Branch or end
[End]
    → Camera returns to gameplay
    → Any reputation/clue changes applied
    → NPC memory updated
```

### 7.3 Reputation system

Four independent faction meters, each ranging from -100 to +100, starting at 0. They are **not** a morality system — high Law reputation and high Outlaw reputation can coexist, depending on how the player plays it.

#### Factions

| Faction | Represents | Visible indicator | Starts at |
|---------|-----------|------------------|-----------|
| **Law** | Sheriff Pruitt, federal presence | Badge icon, blue tint | 0 |
| **Outlaws** | Dross's men, criminal elements | Skull icon, red tint | 0 |
| **Townsfolk** | General Blackwood population | House icon, amber tint | 0 |
| **Shoshone** | June's people, tribal elder Looks-Twice | Feather icon, green tint | 0 (June starts +10) |

#### Threshold effects

| Range | Effect |
|-------|--------|
| +60 to +100 | NPCs proactively offer help, new dialogue unlocked, discounts at stores |
| +20 to +59 | Neutral-positive; standard interactions |
| -19 to +19 | True neutral |
| -20 to -59 | Cold reception, some doors closed, NPC gossip changes |
| -60 to -100 | Hostile; faction members may attack on sight or report player to enemies |

#### Key design rule

Reputation changes are **never announced**. No "+5 Townsfolk" popup. The player infers reputation from how people treat them. A player paying attention will notice Pearl is warmer after they sided with the rancher. A player who isn't paying attention will wonder why Walt Pruitt isn't returning their questions.

The tiny faction icons in the HUD show color shift only (green → amber → red), not numbers. Numbers are visible only in the journal under "People."

### 7.4 Investigation & evidence journal

The journal is Elias's handwritten notebook. When the player opens it, they see actual handwriting (a custom font that mimics period cursive), sketches Elias has made, and — critically — a **connection graph** where the player can link clues manually.

#### Clue types

| Type | Visual | Source |
|------|--------|--------|
| **Document** | Amber glow | Found in world |
| **Testimony** | Blue glow | Unlocked via dialogue |
| **Observation** | Gray | Environmental interaction |
| **Deduction** | Purple | Player connects two clues manually |

#### The connection mechanic

In the journal's "Evidence" tab, clues appear as index cards on a corkboard. The player can drag a thread between two cards. When two cards that *should* connect are linked, the game responds:

- Elias writes a deduction in his own handwriting in the margin
- A new dialogue option may unlock with a relevant NPC
- In some cases, a Yarn command fires (e.g., `<<unlock_clue DeductionMade>>`)

Wrong connections do nothing — no penalty, no mockery. The player can connect anything to anything. The game only responds to *correct* connections.

#### MVP clue chain (Act I)

```
[Caleb Marsh death notice]
    +
[Doc Aldridge's reluctant testimony]
    → DEDUCTION: "The death certificate was signed before the body was examined"
          +
    [Harrow Mine accident report (forged date)]
    → DEDUCTION: "Someone needed Marsh dead before the survey commission arrived"
          +
    [Cora Marsh's letter fragment]
    → DEDUCTION: "Marsh was hiding something at the mine — he told his wife"
          → Unlocks: Mine foreman's office key (from Doc Aldridge)
```

### 7.5 Horse & riding

Elias's horse is named by the player at the start of Chapter 3, when the livery stable attendant asks. The name is stored and used by NPCs and in Elias's journal entries throughout the game.

#### Horse stats

| Stat | Description | Mechanic |
|------|-------------|---------|
| **Stamina** | How long the horse can gallop | Bar depletes at gallop, recovers at walk. Full depletion forces walk. |
| **Loyalty** | How the horse responds under stress | Starts at 50/100. Increases with grooming, feeding, calm riding. Decreases if shot near, forced to gallop to exhaustion, or neglected. |
| **Condition** | Health | Damaged by combat near the horse. Recovered at stables. |

#### Loyalty effects

| Loyalty | Effect |
|---------|--------|
| 80–100 | Horse will hold its ground during combat; can be called with a whistle from 100m |
| 50–79 | Standard behavior |
| 20–49 | Horse may spook at gunfire; call distance reduced to 30m |
| 0–19 | Horse bolts from combat; may throw Elias |

#### Riding controls

| Input | Action |
|-------|--------|
| Joystick | Steer |
| Joystick forward > 80% | Gallop |
| Joystick < 30% | Slow to walk |
| Double-tap A | Dismount |
| Hold B | Emergency stop |
| Swipe right on horse | Pat (loyalty +1, plays animation) |
| Whistle (double-tap Y) | Call horse |

#### Fast travel

Available between discovered locations. Costs in-game time (day/night cycle advances). The player selects a destination on the world map, a brief riding animation plays (not a loading screen — 10–15 seconds of scenery), and they arrive.

### 7.6 Combat & stealth

Combat is designed to be visceral but rare. Most chapters have one or two mandatory combat moments; everything else can be resolved through dialogue, stealth, or avoidance. The game should never feel like a shooter.

#### Stealth system

Elias can crouch and move in shadow. NPCs have a detection cone visible in stealth mode — a faint arc in front of each enemy, transitioning from white (undetected) to amber (suspicious) to red (detected).

Detection factors:
- **Line of sight** — primary factor; shadows reduce detection range by 60%
- **Sound** — footsteps on gravel vs. dirt vs. wood have different radii
- **Light** — lanterns, open windows, and fire cast detection-extending light
- **Reputation** — high Outlaw reputation means some enemies already know who Elias is

When suspicious (amber): enemy pauses, looks around. Player has 3 seconds to break line of sight before the state escalates to red.

When detected (red): enemy calls out, combat state begins. Other enemies in range are alerted after 4 seconds.

#### Combat

Turn-based aiming is intentional — this is not a reflex shooter. The player enters **aim mode** (hold X) which slows time to 40% speed. In aim mode:

- Drag to aim (right thumb)
- Release to fire
- Time-slow drains an "Focus" meter (3 seconds at full drain); meter recovers out of aim mode

**Enemy types (MVP):**

| Enemy | Behavior | Notes |
|-------|----------|-------|
| Dross's hired hand | Advances on player position | Standard; warns before shooting |
| Two-Bit Terrence | Flanks, uses cover, does not warn | Boss; see Ch. 3 design |

**No health bars on enemies.** Hit reaction animations communicate damage (stumble, clutch wound, change in movement). A heavily wounded enemy will surrender if Elias doesn't close in — this opens a dialogue branch.

**Consequences:** Killing in town always has reputation consequences. Wounding and leaving an enemy alive has different consequences than killing. The game tracks these; NPCs discuss them.

#### Boss encounter: Two-Bit Terrence

Terrence is encountered in the Harrow Mine foreman's office (Act I Ch. 3). He has already found the ledger room and is waiting for Elias.

**Design intent:** The player should have three viable paths:

1. **Direct combat** — possible but difficult. Terrence uses the environment (flips the desk, moves between cover positions, throws a lantern to create fire). He does not have a health bar. He goes down after sustained hits and a finishing prompt.

2. **Stealth takedown** — if the player approaches from the upper shaft rather than the main entrance (discovered by exploring the mine), they can get behind Terrence before he is aware. One prompt, non-lethal option available.

3. **Dialogue bluff** — if the player has connected at least 3 clues correctly and found the forged land deed evidence before reaching the mine, a new dialogue option appears: Elias can claim he has already sent a copy of the ledger to the survey commission in Cheyenne. Terrence, who is logical, will hesitate. This does not end the encounter — but it changes its shape, and Terrence may be more useful alive than dead.

### 7.7 Day/night & NPC schedules

The world runs on a 24-minute real-time day (each in-game hour = 2 real minutes). Fast travel and sleeping advance the clock.

#### NPC schedule system

Every named NPC has a `DailySchedule` asset listing location and activity by hour block. The `NPCScheduler` moves them accordingly. If the player has significantly changed world state (e.g., confronted Walt Pruitt publicly), schedules can have conditional overrides.

**Example: Pearl Dancy**

| Time | Location | Activity |
|------|----------|---------|
| 06:00–10:00 | Saloon kitchen | Preparing for the day |
| 10:00–22:00 | Behind the bar | Working |
| 22:00–02:00 | Bar (busy) | Working (more talkative) |
| 02:00–06:00 | Her room (upstairs) | Sleeping |

**Secrets accessible only at specific times** (design examples):
- Doc Aldridge's surgery has a locked desk drawer that is only unlocked between 11pm–4am (he sleeps at the hotel on those nights)
- Walt Pruitt makes an unscheduled walk to the edge of town every night at 1am — if followed, this reveals a dead drop
- The back room of the Sawdust & Rye is only accessible during Pearl's working hours and only if Townsfolk reputation ≥ +30

### 7.8 Save system

**Auto-save triggers:**
- Chapter transitions
- Sleeping at the hotel (advances time to morning)
- Fast travel arrival
- After any significant reputation change (+/- 15 or more in a single event)

**Manual save:** Available any time from the pause menu. One manual save slot.

**Save data includes:**
- Current chapter and active objectives
- All reputation values (4 factions)
- NPC memory flags (dictionary per NPC)
- Discovered clues and connected deductions
- Player's journal notes (any text entered by player)
- World state flags
- Horse name, stats, loyalty
- In-game time and day
- Inventory

**No multiple save files in MVP.** Single continuous save. This is intentional — it reinforces that choices are permanent. A "chapter select" for replay is post-MVP.

---

## 8. Mobile UX & controls

### Design principles

1. **44pt minimum touch target** (Apple HIG standard). No exceptions.
2. **Thumb zones are law.** All interactive controls within natural thumb reach in landscape mode. No stretching to opposite corners for critical actions.
3. **The center of the screen is the world.** No persistent UI in the center 60% of the screen. The world should be unobstructed.
4. **Fade inactive UI.** All HUD elements fade to 20% opacity after 3 seconds of no interaction. Any input or game event (damage, dialogue trigger) restores full opacity for 3 seconds.
5. **Haptic feedback is part of the design.** Gunshots: heavy impact. Horse gallop: rhythmic medium pulse. Receiving a letter or unlocking a clue: single soft tap. The phone should feel like it is in the scene.

### HUD elements

| Element | Position | Visibility rule |
|---------|----------|----------------|
| Health indicator | Top-left | Only visible when below 75% or after taking damage |
| Stamina bar | Top-left (below health) | Only visible while sprinting or on horseback |
| Reputation icons (×4) | Top-left (below stamina) | Always visible, small; color-coded |
| Time/weather | Top-right | Always visible |
| Interaction prompt | Center-bottom edge | Appears when interactable is in range |
| Objective hint | Bottom-center | Fades after 8 seconds; tap to re-show |
| Joystick (left) | Bottom-left zone | Appears on first thumb contact; fades when still |
| Action buttons (right) | Bottom-right zone | Always visible at 40% opacity; full on contact |

### Gesture controls

| Gesture | Action |
|---------|--------|
| Swipe up (anywhere) | Open world map |
| Swipe down (anywhere) | Open evidence journal |
| Two-finger tap | Pause menu |
| Tap NPC | Initiate dialogue (if in range) |
| Tap interactable | Interact |
| Hold interactable | Examine (shows detailed description) |
| Pinch on map | Zoom |

### On-foot button layout

| Button | Action |
|--------|--------|
| A | Interact / Confirm |
| B | Cancel / Holster |
| X | Draw weapon / Aim mode (hold) |
| Y | Crouch toggle |
| Double-tap joystick | Sprint |
| Whistle (double-tap Y) | Call horse |
| MAP button | World map shortcut |
| JOURNAL button | Evidence journal shortcut |

### Accessibility accommodations

- All touch targets scalable +50% in accessibility settings
- Colorblind mode: replaces color-coded reputation indicators with shape icons
- Subtitles: on by default, size adjustable
- Aim assist: three levels (off / light / strong); default is light
- Reduce motion: disables camera shake and certain VFX transitions
- Text size: adjustable in settings, affects all in-game text

---

## 9. Audio design

Built entirely on **FMOD Studio 2.x**. Unity's built-in audio is disabled. All audio events are fired through `AudioManager.cs` which wraps FMOD's Unity integration.

### Philosophy

The score should feel like it grew out of the landscape. The primary instruments are acoustic guitar (fingerpicked, not strummed), upright bass, harmonica, and — in moments of real tension — a low orchestral string section that sounds like it is played in an empty church.

Silence is used aggressively. The world should sometimes be just wind and footsteps.

### FMOD bank structure

| Bank | Contents | Load trigger |
|------|---------|-------------|
| `Master` | All critical game events | Game start; never unloaded |
| `Music` | Adaptive score stems by intensity | Chapter load |
| `SFX` | Footsteps, weapons, ambient, horse, UI | Chapter load |
| `Dialogue` | All voiced lines (or text-to-speech placeholder in pre-production) | On demand |

### Adaptive music system

A single continuous FMOD parameter `GameIntensity` (0.0–1.0) drives the score in real time. `MusicStateController.cs` is the sole writer of this parameter.

| GameIntensity | Music state | Scene context |
|--------------|-------------|--------------|
| 0.0 – 0.25 | **Ambient** | Open exploration, daytime |
| 0.25 – 0.45 | **Unease** | Night, or approaching a known danger |
| 0.45 – 0.65 | **Tension** | Stealth mode, NPC suspicious, armed standoff |
| 0.65 – 0.85 | **Confrontation** | Combat initiated, chase |
| 0.85 – 1.0 | **Peak** | Boss encounter, Chapter climax |

Transitions between states are crossfades over 4–8 seconds. No hard cuts in the music.

### Environmental audio

Every location has a **base ambient layer** (always playing) and up to three **additive layers** triggered by proximity:

- **Blackwood Main Street (day):** Blacksmith hammer (distance), cart wheels, murmured conversation, flies
- **Blackwood Main Street (night):** Distant piano from saloon, coyotes, wind
- **Harrow Mine entrance:** Wind through shaft, settling wood, distant drip
- **Harrow Mine interior:** Much of the above amplified; own breathing audible; lantern flicker sound

### Dialogue audio

Pre-production: all dialogue is text-only with ambient audio continuing behind it.
Production target: key story characters (Elias, June, Dross, Pruitt, Terrence, Pearl) are fully voice-acted. Supporting NPCs have vocalization audio (reactions, grunts, short phrases) with text for full lines.

### Haptic map

| Event | Haptic type | iOS API |
|-------|------------|---------|
| Gunshot (fired by Elias) | Heavy impact | `UIImpactFeedbackGenerator.heavy` |
| Gunshot (received — hit) | Medium + notification | `UINotificationFeedbackGenerator.warning` |
| Horse gallop (per stride) | Light rhythmic | `UIImpactFeedbackGenerator.light` (looped) |
| Clue discovered | Soft tap | `UIImpactFeedbackGenerator.soft` |
| Letter received | Single medium | `UIImpactFeedbackGenerator.medium` |
| Reputation change (significant) | Double soft | `UIImpactFeedbackGenerator.soft` ×2 |

---

## 10. Visual style

### Palette

The game uses a warm, desaturated palette with controlled color temperature shifts:

| Time of day | Sky | Terrain | Accent |
|-------------|-----|---------|--------|
| Dawn | Pale amber, dusty rose | Long blue shadows | Gold rim light |
| Midday | Washed white-blue | Bleached ochre | None |
| Dusk | Deep amber, coral | Warm rust, long shadows | Red-orange |
| Night | Ink blue, star-dense | Near-black | Lantern amber pools |

### Rendering targets (URP mobile)

- Directional light (sun/moon): single real-time shadow-casting light
- Secondary lights (lanterns, fires): point lights, baked contribution only, 4 max per scene
- Global illumination: baked lightmaps; no real-time GI (too costly on mobile)
- Post-processing: subtle film grain, mild vignette, color grading LUT per time-of-day
- Anti-aliasing: SMAA (MSAA disabled for performance)
- Target: 60fps on iPhone 13 or newer; 30fps minimum on iPhone 11

### Character visual direction

- Faces carry history. No smooth hero faces. Elias has a scar above his left eyebrow. Dross's hands are too clean.
- Clothing shows wear. Dust accumulates on Elias during long rides; rain soaks clothing visually.
- Animations are physically grounded. Elias holds his gun hand near his holster when in an unfamiliar space. He sits with his back to walls.

### UI visual language

- All UI elements use a period-appropriate aesthetic: wood grain panels, brass fittings, paper-texture cards, sepia-toned photographs for character portraits
- Typography: primary — a period-appropriate serif (Playfair Display or similar); secondary — a worn letterpress sans for labels
- No modern UI conventions (no flat blue buttons, no Material Design)
- Evidence journal is literally a journal — torn edges, ink bleed, handwritten annotations

---

## 11. Acts & chapters

### 11.1 Act I — Blackwood (MVP)

#### Chapter 1 — Arrival at Blackwood

**Location:** Pacific Express train car → Blackwood Station → Main Street (first block)
**Goal:** Tutorial. Player learns movement, interaction, and dialogue. Elias arrives in Blackwood. The torn envelope sets the story in motion.
**Tone:** Quiet. Observational. The world is introduced before it becomes threatening.

**Beat sequence:**

1. Black screen — sound first. Rails, steam, voices. Camera opens on Elias's hands. The envelope is visible in his coat.
2. Player walks the train car (movement tutorial). Interactables: luggage rack (newspaper), window view (Blackwood approaching).
3. First dialogue: Rev. Solomon Voss. Three tone options introduced. No stakes — just character seeding.
4. Train arrives. Cinematic — Blackwood fills the screen.
5. Tutorial: interaction prompt system. Objective: "Find the hotel."
6. Three optional NPC conversations on the way to the hotel (each reveals a different town detail).
7. Midway down Main Street: a man running from an alley collides with Elias and drops a torn paper scrap. It matches the envelope's torn edge. The name fragment reads "...eb Marsh."
8. Chapter ends. Chapter 2 begins immediately.

**Systems introduced:** Movement, camera, tap-to-interact, dialogue (tone options), basic inventory (envelope examined)
**Reputation changes:** None yet (reputation starts at 0)
**Clues unlocked:** `CalebMarshNameFragment`

---

#### Chapter 2 — The Sawdust & Rye

**Location:** Main Street continuation → The Sawdust & Rye saloon and hotel
**Goal:** Elias checks in, gets the lay of the land, meets June Whitehorse, and opens the envelope. First moral choice. Harlan Dross makes his first appearance.
**Tone:** Watchful. Socially charged. Everyone in the saloon has an opinion about Caleb Marsh.

**Beat sequence:**

1. Elias checks in. Pearl Dancy is behind the bar. She sizes him up. Tone choice shapes how Pearl will interact for the rest of Act I.
2. Land dispute in progress at the bar: rancher Hennessey vs. Dross's surveyor Kincaid, argument over boundary stakes. Three options: side with Hennessey (Townsfolk +10, Law +5, Outlaw -5), side with Kincaid (Outlaw +5, Townsfolk -10), or watch silently and buy Hennessey a drink later (Townsfolk +5, no faction penalty).
3. June Whitehorse is eating alone in the corner. She acknowledges Elias but does not invite conversation. If the player approaches: brief, guarded exchange. She mentions she knew Caleb Marsh. This is the only way to learn this in Ch. 2 — if the player doesn't talk to her, they learn it differently in Ch. 3.
4. Harlan Dross enters with two men, eats dinner at a private table, catches Elias's eye across the room. One sentence: "You're new." Waits for a response. Two options: give your name, or say nothing. Dross nods either way and leaves. The player should feel watched.
5. In Elias's hotel room: the envelope is examined. Inside: a letter in a code Elias recognizes — a Pinkerton field cipher. He can only partially decode it. Decoded fragment: "Marsh has the ledger. Get it before —" The rest is water-damaged.
6. Chapter ends.

**Systems introduced:** Reputation (first changes), extended NPC dialogue, moral choice
**Reputation changes:** Per land dispute choice (above)
**Clues unlocked:** `CalebMarshEnvelopeLetter`, `PinkertonCipher`, `JuneMarshdConnection` (if player spoke to June)

---

#### Chapter 3 — Dead Man's Errand

**Location:** Blackwood outskirts → Harrow Mine (1.5hr ride northwest)
**Goal:** Elias and June investigate the mine. Terrence is found there. The ledger is recovered. Act I's central conflict is resolved — and immediately complicated by The Widow's letter.
**Tone:** Rising dread. Physical danger for the first time. The confrontation with Terrence is the Act I climax.

**Beat sequence:**

1. Morning. June meets Elias outside the livery stable. She has a horse arranged for him — this is the horse naming moment.
2. **Riding tutorial:** gallop, stamina, loyalty introduction. The trail to the mine is beautiful and should feel peaceful. This is intentional — the contrast with what comes next.
3. At the mine entrance: signs of recent activity (tire tracks from a cart, a broken lock). June reads the tracks (her skills system introduced here — she can identify things the player cannot).
4. Mine exploration (stealth optional). The mine has three sections; the foreman's office requires the key (obtained either from Doc Aldridge via earlier dialogue, or picked by June if her trust is high enough).
5. **Boss encounter: Two-Bit Terrence.** Three resolution paths (see [section 7.6](#76-combat--stealth)).
6. After Terrence: the ledger. June reads it with Elias. This is a long dialogue beat — the best in Act I. June is quiet for a moment, then: "My grandmother's family used to summer on Red Mesa. Their names aren't in that ledger. Nothing in there says what they lost."
7. Return to Blackwood. A letter is waiting at the hotel desk, addressed to Elias, in unfamiliar handwriting.
8. The letter: The Widow. Full text displayed slowly, on a paper-texture screen. Elias reads it once. Does not speak. End of Act I.

**Systems introduced:** Horse riding, stealth detection, combat/aim mode, boss encounter, evidence journal connection mechanic (ledger connects multiple existing clues)
**Reputation changes:** Terrence encounter (killing vs. subduing has different Townsfolk/Law consequences)
**Clues unlocked:** `HarrowMineLedger`, `ForgedLandDeeds`, `TheWidowLetter`

---

### 11.2 Act II — Red Mesa Flats (post-MVP)

Full chapter briefs in `Docs/Story/Acts/Act2_RedMesa.md`. Summary:

**Chapter 4 — Iron Rails & Blood Land:** Elias and June ride to Red Mesa to find the survey commission before Dross can destroy the ledger evidence. Looks-Twice is introduced. Walt Pruitt's loyalty is put to the test. Dross is confronted directly for the first time. The choice of how to use the ledger (legal channels, blackmail, or burning it for June's reasons) has lasting consequences.

**Chapter 5 — The Widow's Letter:** The telegraph at Bitter Creek is being used. Elias and June follow the line. The Widow's identity is not fully revealed — but Elias recognizes something in her second letter. The act ends on a revelation that reframes Elias's discharge from the Pinkertons.

---

## 12. Progression & economy

### No XP, no levels

Elias does not get stronger over time in a numerical sense. What progresses is:

- **World knowledge** — more clues connected, more options available
- **Reputation** — more or fewer doors open depending on faction standing
- **Relationship depth** — June, Pearl, and Pruitt have trust thresholds that unlock new dialogue and assistance
- **Tools** — new items found or purchased (better lantern, a second revolver, lockpicks) expand what is possible

This progression feels earned because it is tied to behavior and attention, not grinding.

### Economy

Currency: US dollars. Elias starts with $18. Money is found, earned from odd jobs (notice board in saloon), or won at cards (post-MVP).

**Purchases available (MVP):**
- Ammunition ($0.15/round — not scarce, but not free)
- Hotel room per night ($0.50)
- Meals at the saloon (restore stamina max temporarily; $0.25)
- Stable fee for horse care ($0.10/day; neglecting this reduces Loyalty)
- Lockpick set ($3.50 at the general store)

Economy is not a focus of the MVP. The player should never feel poor enough to be stuck. The goal is that money creates small, authentic decisions — not resource anxiety.

### Unlockables (MVP)

| Unlock | Trigger |
|--------|---------|
| Mine foreman's office key | Doc Aldridge dialogue (Townsfolk ≥ +20) OR June trust ≥ 60 |
| Pearl's back room | Townsfolk ≥ +30 AND nighttime |
| Walt Pruitt's 1am walk (observable) | Law ≥ +20 |
| Full Pinkerton cipher decode | Connect 3 specific clues in journal |
| Terrence dialogue bluff option | Connect 3+ clues AND find forged deed before Ch. 3 |

---

## 13. Accessibility

| Feature | Default | Option |
|---------|---------|--------|
| Subtitles | On | Off |
| Subtitle text size | Medium | Small / Large / X-Large |
| Colorblind mode | Off | Deuteranopia / Protanopia / Tritanopia |
| Aim assist | Light | Off / Strong |
| Touch target size | Standard | +25% / +50% |
| Camera shake | On | Off |
| Reduce motion | Off | On (disables VFX transitions, screen shake) |
| High contrast UI | Off | On |
| Auto-advance dialogue | Off | On (for players who prefer to observe) |

---

## 14. Out of scope (MVP)

The following are intentionally deferred to post-MVP:

- Poker / card mini-games
- Hunting system
- Multiple save slots / chapter select
- Voice acting (placeholder audio only)
- Acts III and beyond
- Localization (English only for MVP)
- Controller support (touch-only for MVP)
- Photo mode
- Achievements / Game Center integration
- iCloud save sync

---

## 15. Open questions

These require decisions before the listed milestone.

| # | Question | Needed by | Owner |
|---|----------|-----------|-------|
| 1 | Does Elias have a fixed voice for narration/journal entries, or is that post-MVP? | Milestone 2 | — |
| 2 | Can the player choose to *not* pursue the investigation in Ch. 1 and just explore Blackwood? If so, how long does the game wait before nudging? | Milestone 2 | — |
| 3 | Is Two-Bit Terrence killable or only incapacitatable? (Affects Act II — if he is alive, he could reappear) | Milestone 3 | — |
| 4 | What is the exact threshold for the Terrence dialogue bluff? Currently "3+ clues and forged deed" — needs playtesting | Milestone 3 | — |
| 5 | Does The Widow have a face in Act II, or does she remain physically absent? | Post-MVP design | — |

---

## 16. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 0.1 | 2024-06 | Initial draft — all systems, Act I chapters, full character bios |

---

*Next review: after Milestone 1 completion. Update version to 0.2 when horse system design is finalized.*
