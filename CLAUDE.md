# Holler Horror — Engineering Conventions

Design source of truth: [Docs/holler-horror-gdd.md](Docs/holler-horror-gdd.md). This file covers *how* we build; the GDD covers *what*. Flag any design decision that creates disproportionate technical cost before building it.

## Project

- Unity **6000.4.3f1**, URP, Input System (new), PC/Steam only for EA.
- Milestones M0–M7 are defined in GDD §13. Build in order; every milestone stays playable.
- Open tech-spike questions (GDD §10/§12) must be surfaced with options, never silently decided: netcode transport (NGO+Relay vs Steam networking), VOIP solution (Vivox vs Dissonance vs Steam Voice).

## Layout

- All project content lives under `Assets/_Project/` — never loose in `Assets/` (the template's `InputSystem_Actions.inputactions` at Assets root is the current exception; relocate when convenient and update `GreyboxSceneBuilder.InputActionsPath`).
- `Scripts/Runtime/` → `HollerHorror.Runtime` asmdef, namespace `HollerHorror.<Area>` (e.g. `HollerHorror.Player`, `HollerHorror.Entities`).
- `Scripts/Editor/` → `HollerHorror.Editor` asmdef, editor-only.
- Scenes in `Scenes/`, generated greybox content via `Holler Horror` menu (reproducible builders, not hand-placed greybox).

## Code style

- One public type per file, file named after the type. `sealed` unless designed for inheritance.
- Serialized fields: `[SerializeField] private`, camelCase, with `[Tooltip]` where the name alone is ambiguous. No public fields.
- Design-tunable numbers live in serialized fields (later: ScriptableObject configs), never hard-coded constants in logic.
- Comments explain constraints and intent, not restate code.
- Systems that later feed entity AI (noise, visibility, sanity) must expose clean read-only seams — see `FirstPersonController.CurrentSpeed` / `Locomotion` as the pattern.

## Multiplayer discipline (from M1 onward)

- Assume every gameplay-relevant state needs to sync. Keep sim state separate from presentation from day one.
- Client-side-only effects (sanity hallucinations) are a design feature — mark them explicitly in code.

## Commits

- Conventional prefixes: `feat:`, `fix:`, `chore:`, `refactor:`, `docs:`, scope optional (`feat(player): ...`).
- One milestone-relevant change per commit; never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
