# HOLLER HORROR — Game Design Document
**Version 2.0 (Fresh Start) — July 2026**
**Engine: Unity 6 | Target: Steam Early Access | Price: £6.99**

---

## 1. High Concept

Holler Horror is a 1–4 player co-operative investigation horror game set in a remote Appalachian holler. Something is preying on the valley. Players arrive as outsiders and must work out *what* it is — interviewing the holler's residents, gathering physical evidence, and pinning it all to a shared clue board — then gather the components for and perform the ritual that banishes that specific entity. Perform the right ritual and the holler is saved. Perform the wrong one and you've just made it angry.

**Elevator pitch:** A co-op detective board wrapped in folk horror — Phasmophobia's deduction meets Return of the Obra Dinn's "commit to your answer" tension, in a valley full of people who may be lying to you.

## 2. Design Pillars

1. **Deduction is the game.** Combat and evasion exist, but the win condition is *knowing*. Every mechanic feeds the question: which entity is this?
2. **The residents are the content.** NPCs are the primary clue source — and they're scared, superstitious, grieving, or hiding things. Reading people matters as much as reading evidence.
3. **Commit under pressure.** The ritual is a bet. The entity escalates over the night, so waiting for certainty has a cost — but a wrong ritual has a bigger one.
4. **Folk horror, not gore.** Dread, ritual, superstition, isolation. The horror is in what the clues imply, and in the moment the board tells you something you didn't want to be true.

## 3. Core Gameplay Loop

**Session loop (30–50 min per run):**
1. **Arrive** — The team enters the holler at dusk. A resident has called for help; the case brief gives the surface story (disappearances, sick livestock, a haunted homestead).
2. **Investigate** — Split up or stick together:
   - **Talk to NPCs** — residents scattered across the holler's homesteads. Each knows fragments. Some are reliable, some mistaken, some lying.
   - **Find physical evidence** — tracks, carcasses, cold spots, scratched sigils, fogged mirrors. Collected evidence becomes clue cards.
3. **The Clue Board** — A shared, physical board (at the base camp, and via a portable field journal). Clues pin automatically; players connect them, mark contradictions, and see which entities the evidence supports or rules out.
4. **Deduce & Prepare** — Each entity has a unique banishing ritual requiring specific components found or earned in the world (some only obtainable from NPCs — including ones who won't hand them over easily). Committing to gathering one ritual's components IS committing to a theory.
5. **The Ritual** — Perform the ritual at the required site. A multi-step, interruptible co-op sequence (place components, hold positions, speak the words) while the entity actively tries to stop you.
   - **Correct ritual** → banishment. Full payout, dawn breaks.
   - **Wrong ritual** → backfire. The entity escalates hard; the team must survive, re-deduce, and attempt the true ritual with the night running out and the holler now actively hostile.
6. **Progress** — Earn money and journal entries. Money buys gear; the Journal permanently records entity knowledge learned first-hand.

**Escalation clock:** The entity grows more aggressive in phases through the night. Early night: signs and stalking. Mid: direct hunts. Late: NPCs start dying if unprotected — and dead NPCs take their clues with them.

**Meta loop:** Gear, cosmetics, and Journal lore across runs. No player stat upgrades — progression is knowledge and tools.

## 4. The Entities

Only one entity is active per run (EA scope). Each has overlapping-but-distinct evidence profiles, so single clues suggest and combinations confirm. Each has a unique ritual.

### 4.1 The Wendigo
- **Fantasy:** Starvation given legs. A person who did the unforgivable in a hard winter.
- **Behaviour:** Sound-driven pursuit predator. Drawn to noise and to *meat* — it raids smokehouses and livestock first, people later.
- **Evidence:** Stripped carcasses, claw-scored bark at head height, scream-calls echoing down the valley, an NPC who speaks of a neighbour who vanished "the winter the food ran out."
- **NPC angle:** Someone in the holler knows *who it was*. The name is a ritual component.
- **Ritual — The Burning of the Name:** Learn the entity's human name from a resident, carve it into ash wood, burn it at the site of its first kill. Requires keeping that NPC alive long enough to talk.

### 4.2 The Fetch
- **Fantasy:** The doppelganger. It wears faces — a resident's, or one of *yours*.
- **Behaviour:** Weak in confrontation, devastating through deception. Can appear as a living teammate (model + name tag) or pose as an NPC — meaning some "testimony" on your board may have come from the thing itself.
- **Evidence:** Duplicate footprints, fogged mirrors and still water, residents reporting conversations the other party denies having, your own voice from the treeline. It cannot cross running water.
- **NPC angle:** The signature twist — clue cards sourced from NPC interviews carry a "who told you this" tag, and a Fetch case is solved partly by finding the testimony that doesn't hold up.
- **Ritual — The Mirror Binding:** Lure or bait it before an unfogged mirror ringed with salt at a crossroads, forcing it to hold one face. Requires salt (traded from a resident) and a mirror carried — fragile — across the holler.

### 4.3 The Hollow
- **Fantasy:** The valley itself, awake. Not a creature — an absence that spreads.
- **Behaviour:** Manifests as drifting zones of unnatural dark and silence. Lights gutter, compasses spin, prolonged exposure drains sanity. It herds rather than chases — and it swallows homesteads, cutting you off from the NPCs inside.
- **Evidence:** Dead birdsong, cold spots, candle flames bending toward no wind, residents describing land that "moved" or paths that no longer lead where they did.
- **NPC angle:** The oldest residents remember when it woke before, and what was done. The chapel ledger — held by one of them — contains the rite.
- **Ritual — The Consecration:** Relight the chapel and walk a salt-and-fire boundary around the heart of the affected land while one player reads the rite aloud. Longest, most exposed ritual of the three.

## 5. The Clue Board (core system)

- **Shared and synced:** One board state for the whole team, physically present at base camp, mirrored in a portable field journal (reduced functionality — read/pin, no rearranging).
- **Clue cards:** Every piece of evidence and every notable NPC statement becomes a card automatically. Cards carry metadata: where found, when, and — for testimony — *who said it*.
- **Player-driven connections:** Players draw strings between cards, tag cards as supporting an entity theory, and flag contradictions. The board never auto-solves; it visualizes, players deduce.
- **Entity columns:** Optional assist mode where cards can be sorted under entity headers and the board greys out entities ruled out by hard evidence. (Toggleable — off for purists, on by default.)
- **Contradiction mechanics:** Two testimony cards can directly conflict. Resolving a contradiction (re-interviewing, finding physical proof) is a first-class action and often the case-cracker — especially against the Fetch.
- **Design goal:** The board should photograph well. It's the game's signature image and the thing streamers argue in front of.

## 6. NPCs & Dialogue

- **Cast per run:** 6–9 residents drawn from a larger pool, placed at fixed homesteads with randomized knowledge assignments per run — so "talk to the widow first" never becomes rote.
- **Dialogue system:** Node-based conversations with unlockable branches. Showing an NPC a relevant clue card opens new lines ("Show her the carved sigil"). Some NPCs only talk if approached alone, at certain hours, or after another resident vouches for you.
- **Reliability model:** Each NPC's statements are internally consistent with their knowledge + disposition:
  - **Honest** — accurate but partial.
  - **Mistaken** — sincerely wrong (superstition misattributes evidence).
  - **Withholding** — knows more; needs trust, trade, or leverage.
  - **Lying** — protecting someone or something (or, in a Fetch case, is not who they appear to be).
- **NPCs as stakes:** Late-night escalation threatens residents. Players can shelter or escort them — spending time and safety to protect clue sources and ritual-component holders.
- **Tone:** Grounded Appalachian voices — grief, stubbornness, faith, dark humour. No yokel caricature. This is where the folk horror lives or dies.

## 7. Player Systems

- **Movement:** Walk / crouch / sprint with stamina. Surface-based noise model (gravel, leaf litter, creek, floorboards) feeds entity AI.
- **Light:** Flashlights, lanterns, candles, braziers. Light attracts some attention; darkness drains sanity. Battery/fuel management.
- **Sanity:** Per-player meter drained by darkness, entity proximity, and events. Low sanity causes *client-side-only* hallucinations — including false clues that never pin to the board (the board only accepts what's real; the gap between "what I saw" and "what pinned" is itself information).
- **Voice:** Proximity voice chat as a core mechanic; radios for long range with static/interference. Fetch mimicry hooks into recorded player voice.
- **Health/Down state:** Downed and revivable rather than instakilled (until late-night escalation). Abandoned players can be dragged off.
- **Inventory:** Small slot-based inventory. Ritual components take slots — carrying the ritual is a team logistics problem.

## 8. The Map (EA: one map)

**"Bricker's Holler"** — a single handcrafted valley (~600m × 600m playable), fixed geography, randomized per run:
- **Base camp** (safe hub, main clue board), **the Creek** (running water — Fetch barrier), **the Farmstead**, **the Chapel**, **the Mine mouth**, **the Ridge cabins**, **the Crossroads**, dense laurel thickets between. Homesteads host the NPC cast.
- Landmark navigation, no minimap. Compass + landmarks + callouts.
- Randomized per run: entity, NPC knowledge assignments, evidence placement, ritual site variants, fog/weather.

## 9. Progression & Economy

- **Currency:** Payout scaled by outcome — full banishment > survived-a-backfire > fled. Buys gear, consumables, cosmetics.
- **The Journal:** Persistent codex. Entity evidence profiles, ritual details, and NPC lore unlock as entries when experienced first-hand. Diegetic tutorial + long-term completionist goal.
- **No stat upgrades.** Players get smarter, not stronger.

## 10. Art & Audio Direction

- **Visual:** Semi-stylized realism — grounded environments, exaggerated entity silhouettes. Volumetric fog; warm firelight vs cold moonlight. Kudzu, laurel, rusted tin, hand-painted signage. The clue board rendered lovingly — paper textures, pin shadows, string physics.
- **Audio:** Layered ambience (insects that *stop*), spatialized entity audio, dynamic mix ducking as threat rises. Banjo/dulcimer motifs twisted into drones. NPC VO is a major budget line — plan for it (or stylized non-verbal + text as fallback; decide early, it shapes the dialogue system).
- **Music:** Licensed track — *"Shepherd" by Plainride (instrumental)* — licensed for in-game use; promotional rights separate (confirm before trailer use).

## 11. Multiplayer & Tech

- **Players:** 1–4 co-op; solo supported with tuned aggression and a personal board.
- **Engine:** Unity 6, URP.
- **Netcode:** Netcode for GameObjects + Unity Relay/Lobby as default assumption; evaluate Steam networking in the tech spike. Clue board state sync is a first-class netcode object.
- **Voice:** Proximity VOIP with capture/replay — load-bearing for the Fetch. Evaluate Vivox vs Dissonance vs Steam Voice early.
- **Dialogue tooling:** Node-based dialogue needs an editor-side authoring tool (evaluate Yarn Spinner / Ink vs custom) — this is the second-biggest tech decision after VOIP.
- **Platform:** PC / Steam only for EA. KB+M primary, controller desirable.
- **Performance target:** 60fps on GTX 1660-class at 1080p.

## 12. Early Access Scope

**EA launch (v0.1) — £6.99:**
- 1 map (Bricker's Holler)
- 3 entities with 3 unique rituals
- NPC pool of ~12 residents (6–9 per run)
- Full clue board + field journal
- 1–4 co-op + solo
- Core gear (~10 items), Journal, basic cosmetics
- 8–12 hours before case patterns start repeating

**Explicitly NOT in EA launch:** second map, additional entities, PvP, mod support, localization beyond English, console, full VO if fallback is chosen.

**EA roadmap (public, vague dates):**
- Update 1: 4th entity + ritual, expanded NPC pool
- Update 2: second map (the Mine as its own case setting)
- Update 3: multi-entity cases (two active — evidence profiles interleave), hard mode
- 1.0: 2 maps, 5–6 entities, expanded dialogue, £9.99–£11.99 price raise

## 13. Success Criteria & Risks

- **EA gate:** A 4-stack of strangers must argue at the board, commit to a ritual, and *react* when it backfires — without a dev in the lobby.
- **Top risks:**
  1. **Deduction balance** — too easy and there's no game; too obscure and players brute-force rituals. Needs the most playtesting of anything.
  2. **NPC/dialogue content cost** — the randomized knowledge-assignment system is what keeps a small cast replayable; build the system before writing volume.
  3. **VOIP capture/replay** (Fetch) — highest pure-technical risk; prototype first.
  4. **Wrong-ritual backfire tuning** — must feel like a dramatic mid-session twist, not a 40-minute loss screen.

## 14. Prototype Plan (first milestones for Claude Code)

Build in this order — each milestone playable and testable:

1. **M0 — Project skeleton:** Unity 6 + URP, folder/asm structure, input system, first-person controller (walk/crouch/sprint/stamina), greybox test scene.
2. **M1 — Netcode spike:** 4-player lobby + synced movement; proximity VOIP proof-of-concept **including voice capture/replay** (Fetch feasibility gate).
3. **M2 — Clue board vertical slice:** Pin/inspect/connect/tag clue cards, board state fully synced across clients, field journal read view. Placeholder clues. *This is the signature system — get it feeling great in isolation.*
4. **M3 — Dialogue system:** Node-based conversations, show-clue-card-to-unlock-branch, testimony auto-generating clue cards with source tags. Authoring tool decision (Yarn/Ink/custom) made here.
5. **M4 — Evidence & deduction loop:** Physical evidence pickups → cards; entity evidence profiles; one full case (Wendigo) soluble end-to-end with placeholder art: interview → board → deduce.
6. **M5 — Ritual system:** Component gathering, multi-step interruptible ritual sequence, correct/backfire branches. Wendigo case now winnable and losable.
7. **M6 — Entity AI & escalation:** Perception model (sound/sight), Wendigo state machine, night-phase escalation clock, NPC threat/protection.
8. **M7 — The Fetch:** Mimicry (player + NPC impersonation) on M1's voice work; testimony-source contradiction mechanics.
9. **M8 — The Hollow + sanity system + Journal.**
10. **M9 — Vertical slice polish:** Bricker's Holler blockout → art pass on one homestead + base camp, audio pass, first external playtest.

## 15. Working Conventions

- A separate `CLAUDE.md` in the repo holds code style, architecture rules, and commit conventions — design (this doc) and engineering rules stay separate.
- This GDD is the source of truth for *what*; Claude Code proposes *how* and flags any design decision with disproportionate technical cost before building it.
- Open questions are the marked tech spikes (§11) and risks (§13) — surface options rather than silently picking answers.
