# HOLLER HORROR — Game Design Document
**Version 1.0 (Fresh Start) — July 2026**
**Engine: Unity 6 | Target: Steam Early Access | Price: £6.99**

---

## 1. High Concept

Holler Horror is a 1–4 player co-operative survival horror game set in a remote Appalachian holler. Players descend into the valley at dusk to complete objectives — recovering what was left behind, undoing what was done — while being hunted by folk horrors drawn from Appalachian legend. Each of the three monsters hunts differently, forcing teams to change how they move, communicate, and trust each other.

**Elevator pitch:** Phasmophobia's tension meets Lethal Company's co-op chaos, soaked in Appalachian folk horror — where one of the things hunting you might be wearing your friend's face.

## 2. Design Pillars

1. **The holler is hostile.** The environment itself — fog, terrain, darkness, sound carrying across the valley — is the first antagonist. Players should feel like intruders.
2. **Each monster changes the game.** The three entities aren't reskins. Knowing *which* one is out there fundamentally changes optimal play.
3. **Trust is a resource.** Co-op mechanics deliberately create doubt — mimicry, separation, unreliable information. The scariest moments come from other players.
4. **Folk horror, not gore.** Dread over jump scares (though jump scares have their place). Rooted in ritual, superstition, isolation, and things that were here long before you.

## 3. Core Gameplay Loop

**Session loop (20–40 min per run):**
1. **Prepare** — At the trailhead (safe hub), the team buys/equips gear, reviews the contract (objective), and reads the signs (clues hinting at which entity is active).
2. **Descend** — Enter the holler. Complete the run objective (e.g. recover heirlooms, relight the chapel candles, seal a ritual site) while gathering evidence about the entity.
3. **Survive** — The entity escalates over time. The longer the run, the more aggressive it becomes. Night deepens mechanically.
4. **Escape** — Return to the trailhead with objectives complete. Partial extraction is possible: whoever gets out keeps their share.
5. **Progress** — Earn money and journal entries. Money buys gear; journal entries permanently record entity knowledge (lore-as-progression).

**Meta loop:** Unlock gear, cosmetics, and journal lore across runs. No power creep on player stats — progression is knowledge and tools, keeping horror intact.

## 4. The Entities

Only one entity is active per run (EA scope). Players deduce which via environmental signs and behaviour, and deduction matters because counters differ.

### 4.1 The Wendigo
- **Fantasy:** The relentless pursuer. Starvation given legs.
- **Behaviour:** Patrols wide, drawn strongly to *sound* — footsteps on gravel, doors, voice chat proximity volume. Once it locks on, it does not stop; it is faster than a sprinting player in the open.
- **Counterplay:** Silence and terrain. Breaking line-of-sight in dense laurel thickets, crouch-moving on soft ground, and going quiet on voice. Sprinting is a death sentence in the open, salvation in a thicket.
- **Signs:** Stripped carcasses, claw-scored bark at head height, distant scream-calls that echo down the valley.
- **Tension knob:** Punishes loud, fast teams. Teaches the game's sound discipline.

### 4.2 The Fetch
- **Fantasy:** The doppelganger. It wears the face of someone you came in with.
- **Behaviour:** Weak in direct confrontation, devastating through deception. Periodically takes the appearance (model + name tag) of a *living* team member and appears where they shouldn't be. Can imitate proximity voice with garbled, delayed playback of things players actually said earlier in the run.
- **Counterplay:** Verification rituals — challenge phrases, checking equipment loadouts (the Fetch copies the body, not the belt), staying paired. It cannot cross running water.
- **Signs:** Duplicate footprints, mirrors and still water fogged over, your own voice heard from the treeline.
- **Tension knob:** Weaponizes the co-op itself. The signature entity — this is the one clips get made of.

### 4.3 The Hollow
- **Fantasy:** The valley itself, awake. Not a creature you see — an absence that spreads.
- **Behaviour:** Manifests as zones of unnatural dark and silence that grow and drift across the map. Inside a zone: light sources gutter, compasses spin, the HUD degrades, and prolonged exposure drains sanity/health. It herds players rather than chasing them.
- **Counterplay:** Fire and consecration. Lit braziers and salt lines repel zones temporarily; the run objective often doubles as the counter (relighting the chapel pushes it back).
- **Signs:** Dead birdsong, cold spots, candle flames bending toward no wind.
- **Tension knob:** The map-control entity. Turns runs into territory management under pressure.

## 5. Player Systems

- **Movement:** Walk / crouch / sprint with stamina. Noise model per surface (gravel, leaf litter, creek, floorboards) feeds entity AI.
- **Light:** Flashlights, lanterns, flares, brazier network. Light attracts some attention but darkness drains sanity. Battery/fuel management.
- **Sanity:** Per-player meter drained by darkness, entity proximity, and witnessing events. Low sanity = audio/visual hallucinations that are *client-side only* — so players can't fully trust their own senses or reports.
- **Voice:** Proximity voice chat is a core mechanic (feeds Wendigo aggro, fuels Fetch mimicry). Radios allow long-range comms with static/interference as a tension tool.
- **Health/Down state:** Entities down players rather than instakill (except late-run escalation). Downed players can be carried/revived; abandoned players can be dragged off by the entity.
- **Inventory:** Small slot-based inventory. Meaningful choices (extra battery vs salt vs medkit), no infinite hoarding.

## 6. The Map (EA: one map)

**"Bricker's Holler"** — a single, dense, handcrafted valley (~600m × 600m playable) with fixed geography and randomized interior/objective layouts per run:
- **Trailhead** (safe hub / lobby), **the Creek** (running water — Fetch barrier, noisy to cross), **the Farmstead**, **the Chapel**, **the Mine mouth**, **the Ridge cabins**, dense laurel thickets between.
- Landmark-based navigation, no minimap. Compass + landmarks + player callouts.
- Randomization: item spawns, objective locations, entity spawn/lair, weather (fog density), some door/blockage states.

## 7. Objectives (EA launch set)

Contract types, one per run, scaling with team size:
1. **Recovery** — find and extract 3–5 heirloom items scattered in POIs.
2. **The Vigil** — light and defend the chapel candles until dawn bell.
3. **Sealing** — locate ritual sites and complete a multi-step seal (carry components, place, chant/hold) at each.
Each contract has entity-agnostic steps but entity-specific complications.

## 8. Progression & Economy

- **Currency:** Payout per contract, split by extraction. Buys gear tiers, consumables, cosmetics.
- **The Journal:** Persistent codex. Entity behaviours, signs, and counters unlock as entries when experienced first-hand. Doubles as diegetic tutorial and long-term goal (completionism).
- **No stat upgrades.** Horror longevity depends on players never outscaling the threat.

## 9. Art & Audio Direction

- **Visual:** Semi-stylized realism — grounded environments, slightly exaggerated entity silhouettes. Heavy volumetric fog, warm firelight vs cold moonlight as the core palette. Kudzu, laurel, rusted tin, hand-painted signage.
- **Audio:** The star of the show. Layered ambience (insects that *stop*), spatialized entity audio, dynamic mix that ducks ambience as threat rises. Banjo/dulcimer motifs twisted into drones.
- **Music:** Licensed track — *"Shepherd" by Plainride (instrumental)* — licensed for in-game use; promotional rights are separate (confirm before using in trailers).

## 10. Multiplayer & Tech

- **Players:** 1–4 co-op, host-based or relay (decide in tech spike — see §13). Solo mode supported with tuned entity aggression.
- **Engine:** Unity 6 (URP recommended for perf headroom + volumetrics via custom solution).
- **Netcode:** Netcode for GameObjects + Unity Relay/Lobby as default assumption; evaluate against Steam networking (Facepunch/Heathen) in the tech spike.
- **Voice:** Proximity VOIP required — evaluate Vivox vs Dissonance vs Steam Voice early; this is load-bearing for the Fetch design.
- **Platform:** PC / Steam only for EA. Controller support desirable, KB+M primary.
- **Performance target:** 60fps on GTX 1660-class hardware at 1080p.

## 11. Early Access Scope (the whole point of this section: cut ruthlessly)

**EA launch (v0.1) — £6.99:**
- 1 map (Bricker's Holler)
- 3 entities (Wendigo, Fetch, The Hollow)
- 3 contract types
- 1–4 player co-op + solo
- Core gear set (~10 items), Journal, basic cosmetics
- 6–10 hours of content for a group before repetition sets in

**Explicitly NOT in EA launch:** second map, PvP, mod support, additional entities, localization beyond English, console.

**EA roadmap (public-facing, keep vague on dates):**
- Update 1: 4th entity + new contract type
- Update 2: second map (the Mine interior as its own map)
- Update 3: entity mutators / hard mode, Twitch integration
- 1.0: 2 maps, 5–6 entities, full progression, £9.99–£11.99 price raise

## 12. Success Criteria & Risks

- **EA gate:** Don't launch until a 4-stack of strangers has fun *and* gets scared in playtests without a dev in the lobby.
- **Top risks:**
  1. **VOIP-dependent design** — the Fetch needs reliable proximity voice + recorded playback; prototype this first, it's the highest technical risk.
  2. **Entity AI feel** — dumb AI kills horror instantly. Budget the most iteration time here.
  3. **Two-person team scope** — the map is the biggest asset cost; greybox everything, art-pass last.

## 13. Prototype Plan (first milestones for Claude Code)

Build in this order — each milestone is playable and testable:

1. **M0 — Project skeleton:** Unity 6 project, URP, folder/asm structure, input system, first-person controller (walk/crouch/sprint/stamina), greybox test scene.
2. **M1 — Netcode spike:** 4-player connect via lobby, synced movement, proximity VOIP proof-of-concept **including voice capture/replay** (Fetch feasibility gate).
3. **M2 — Sound & senses:** Surface-based noise emission system, entity-side hearing/vision perception model, debug visualization.
4. **M3 — First entity (Wendigo):** Patrol → investigate → chase → search state machine driven by the M2 perception model. Downed/revive states.
5. **M4 — Run loop:** Trailhead hub → contract select → Recovery objective → extraction → payout. Greybox Bricker's Holler blockout.
6. **M5 — The Fetch:** Mimicry systems on top of M1's voice work. Verification mechanics.
7. **M6 — The Hollow + Vigil/Sealing contracts, sanity system, Journal.**
8. **M7 — Vertical slice polish:** lighting, audio pass on one POI, first external playtest.

## 14. Working Conventions

- A separate `CLAUDE.md` in the repo will hold code style, architecture rules, and commit conventions — keep design (this doc) and engineering rules separate.
- This GDD is the source of truth for *what*; Claude Code proposes *how* and flags any design decision that creates disproportionate technical cost before building it.
- Open design questions to resolve during prototyping are marked by the tech spikes in §10 and the risks in §12 — do not silently pick answers to these; surface options.
