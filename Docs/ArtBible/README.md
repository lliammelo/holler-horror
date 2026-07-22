# Holler Horror — Higgsfield Concept Art Guide

Everything needed to generate the game's art bible in one session. Prompts are
derived from GDD §10 (Art & Audio Direction).

**Goal:** lock the *visual direction* for the M9 art pass. These outputs are
**concept art / reference / marketing mood** — not final in-game assets.

**Important:** the free trial is **website-only**. Generation via API/MCP is
blocked (`only_website_usage_on_trial_is_available`), so all of this is done by
hand at higgsfield.ai. That's fine — the prompts are the valuable part.

---

## 1. Quick start (do this now)

1. Go to **higgsfield.ai** and sign in.
2. Click the **Create Image** feature.
3. Set model to **Higgsfield Soul Cinema**.
4. Set **16:9**, highest quality, **4 variations**.
5. Copy **Prompt 1** below, paste, generate.
6. Download keepers into this folder, then move to Prompt 2.

Work top-to-bottom through the prompts. They are ranked by value — stop
whenever you run out of time and you'll still have the most important pieces.

---

## 2. Which model to use

### Primary: **Higgsfield Soul Cinema**
*"Cinematic Film-Grade Aesthetic."* Use this for **all nine prompts** unless a
note says otherwise. It's the closest match to our fog / firelight / dread mood.

### Situational picks

| Model | Use it for | Why |
|---|---|---|
| **Nano Banana Pro** | Prompt 9 (Steam key art), any hero image | "Best 4K image model" — most detail/resolution |
| **Seedream 5.0 Pro** | Prompt 9 if the board looks incoherent | "Logically consistent… intelligent visual reasoning" — better at complex arrangements like pinned cards + yarn |
| **Recraft V4.1** | A photoreal alternate take on environments | Photorealistic and expressive |
| **FLUX.2** | Fast rough exploration | Speed-optimized |
| **Topaz** | **At the end**, on final keepers only | High-resolution upscaler |

### Avoid these (they'll waste your credits)

| Model | Why not |
|---|---|
| **Higgsfield Soul 2.0** | Tuned for *fashion visuals* — people and clothing, not eerie landscapes. Flagged TOP, still wrong for us. |
| **Z-Image** | Portraits only |
| **GPT Image 2 / 1.5** | Their edge is text rendering — we explicitly want *no text* |
| **Grok Imagine, Popcorn, Nano Banana 2 / Lite, Seedream Lite** | No advantage here; Soul Cinema beats them on mood |

---

## 3. Which features to use

| Feature | Use? | What for |
|---|---|---|
| **Create Image** | ✅ Main tool | All nine prompts |
| **Soul ID Character** | ✅ Important | After you like a Wendigo/Fetch silhouette, lock that identity and pull consistent multi-angle sheets. This is what turns one lucky image into a character sheet a modeller can use. |
| **Cinematic Cameras** | ✅ Try once | Camera controls (low angle, wide establishing) on Prompt 1. Composition is half of what makes concept art a usable paint-target. |
| **Relight** | ✅ If time | Take your best environment and push it warm-firelight vs cold-moonlight — that's the core palette decision from the GDD. |
| **Image Upscale / Topaz** | ✅ At the end | Upscale only the final keepers |
| **Inpaint** | 🔸 Optional | Fix one bad area instead of re-rolling the whole image |
| **Moodboard** | 🔸 At the end | Compile your keepers into one art-bible board |
| Canvas, AI Influencer, Photodump, Face Swap, Character Swap, Fashion Factory, Draw to Edit | ❌ Skip | Not useful for this |

---

## 3b. Keeping a consistent look across all nine

Generations are independent — the shared style preamble gets you most of the
way, but expect drift in grade, painterliness and realism. To lock a look:

1. **Nail the hero image first.** Iterate Prompt 1 (base camp) until one result
   has the palette and mood you want. Do not move on before this — everything
   else keys off it.
2. **Use Color Transfer** with that hero image as the reference on every
   subsequent generation. This is the strongest lever for a consistent grade.
3. **Attach the hero as a style reference** via the `+` next to the prompt box.
4. **Do not switch models between prompts 1–8.** Stay on Soul Cinema. Prompt 9
   (key art) is a standalone marketing piece, so switching there is fine.
5. Keep the style preamble text **byte-identical** across prompts — it already
   is in this doc; don't paraphrase it while editing.

---

## 4. Settings for every generation

- **Aspect ratio:** 16:9 (use 3840×1240-ish wide only for a Steam library hero)
- **Quality:** highest offered (2K+)
- **Variations:** 4 per prompt — batch, don't perfect
- **Negative prompt** (if there's a field):

```
text, watermark, logo, signature, cartoon, cute, bright daylight, oversaturated, gore, blood splatter, modern city, clean new surfaces, deformed hands
```

---

## 5. The prompts

Each block is complete — copy the whole paragraph, no assembly needed.

### 1. BASE CAMP — *M9 target, highest value*
Appalachian folk horror concept art, semi-stylized realism, grounded and weathered. Heavy volumetric fog, deep haze. Warm firelight amber and ember-orange against cold desaturated blue-grey moonlight. Kudzu vines, mountain laurel, rusted corrugated tin, hand-painted wooden signage, damp earth. Dusk into night. Cinematic, moody, painterly, restrained dread, no gore. A small dusk encampment at a wooded trailhead: a lantern-lit table, a cork clue board strung with red yarn under a canvas lean-to, a low fire. A warm pool of firelight swallowed by cold fog and dark treeline beyond. The feeling of a last safe place at the edge of something wrong.

### 2. HOMESTEAD — *M9 target*
Appalachian folk horror concept art, semi-stylized realism, grounded and weathered. Heavy volumetric fog, deep haze. Warm firelight amber against cold desaturated blue-grey moonlight. Kudzu, mountain laurel, rusted corrugated tin, damp earth. Night. Cinematic, moody, painterly, restrained dread, no gore. An abandoned Appalachian homestead at night: a sagging clapboard cabin, rusted tin roof, kudzu creeping up one wall, a torn smokehouse door hanging from its top hinge. A single guttering lantern in a window. Cold moonlight, thick ground fog, laurel closing in. Isolation and unease.

### 3. THE CHAPEL
Appalachian folk horror concept art, semi-stylized realism, weathered and grounded. Heavy volumetric fog. Warm candlelight against cold blue night. Cinematic, moody, painterly, restrained dread, no gore. A tiny abandoned mountain chapel in fog: peeling white clapboard, a leaning steeple, candles guttering on a stone altar, flames bending toward no wind. Cold blue night outside, faint warm candlelight within. Sacred and wrong.

### 4. THE CREEK
Appalachian folk horror concept art, semi-stylized realism, weathered. Heavy fog, cold desaturated blue-grey moonlight. Cinematic, moody, painterly, restrained dread, no gore. A shallow Appalachian creek at night cutting through mountain laurel, moonlight on black moving water, mist rising off the surface, mossed stones, a single set of muddy footprints walking into the water and stopping. Quiet and uncanny.

### 5. LAUREL THICKETS
Appalachian folk horror concept art, semi-stylized realism. Heavy volumetric fog, cold moonlight, deep shadow. Cinematic, moody, painterly, oppressive dread, no gore. A dense mountain laurel thicket at night, tangled twisted branches forming a claustrophobic corridor, thick fog between the trunks, a suggestion of movement just out of sight. Breath-held stillness.

### 6. THE WENDIGO
*(Skip Soul ID for this one — that tool is built for human facial identity, and
the Wendigo is inhuman and mostly glimpsed in fog. Plain generation is fine.)*

Appalachian folk horror creature concept art, semi-stylized realism, cinematic and moody, heavy fog, cold moonlight, restrained dread, not gory. An emaciated humanoid horror — starvation given legs. Unnaturally tall and gaunt, walks upright, elongated limbs, ribs like a broken cage, a suggestion of antlers, ash-grey hide. Seen at a distance through fog at a treeline, mostly hidden in darkness, claw-scored bark at head height nearby. Exaggerated unsettling silhouette. Wrong, not bloody.

#### 6b. Refinement pass — less stock, more starved
The above tends to produce a generic antlered forest cryptid (what every model
reaches for). The GDD wants *starvation* first, antlers only as a suggestion.
Also add scale — nothing in frame says "unnaturally tall" without a reference.

Appalachian folk horror creature, backlit through heavy fog so the silhouette reads black against pale mist, face lost in shadow. A starved humanoid nine feet tall seen from a low angle: ribs stark under drum-tight grey skin, hollow sunken belly, shoulders hunched forward, arms too long, joints wrong. Only small blunt antler stubs, not ornate branching antlers. A rotting fence post nearby for scale. Restrained, wrong, not gory.

> **Keep the backlit-through-fog staging — it's a lighting recipe, not just a
> picture.** Put the Wendigo between the player and a light source, in fog, and
> you get silhouette without detail. That's how to present it in-game too.

### 7. THE FETCH — three steps, do them in order

**Do NOT just prompt "two identical figures."** Tried it; the model fakes it with
copy-pasted cut-outs that are lit differently from the scene and look composited.
The Fetch needs a locked character identity. This is the best use of Soul ID in
the whole project — the Fetch *is* a duplicated person.

#### 7a. Generate the resident (portrait aspect)
Portrait of a woman in her sixties from rural 1930s Appalachia. Weathered sun-lined face, plain ordinary features, grey hair pulled back, worn cotton dress and knitted cardigan. Tired, wary, quietly dignified. Overcast natural light, documentary realism, photographic and grounded. Not glamorous, not a fashion model, no makeup, no retouching, an ordinary working face.

> The anti-glamour clause at the end matters — the Soul models drift toward
> fashion beauty, and Ruth has farmed a hillside for forty years.

#### 7b. Lock her
Take the best result into **Soul ID Character** and save her as a reusable
identity. Then attach her to the next prompt via the **+ CHARACTER** button.

#### 7c. The payoff shot (with Ruth attached)
Appalachian folk horror, night, cold blue moonlight, heavy fog on black still water. An older woman in a worn cotton dress stands waist-deep in the creek shallows in the foreground, her back mostly to us, looking down at the water. Far behind her on the misty bank, small and dim and half-lost in fog, the exact same woman stands motionless, watching her. The distant figure is barely visible. Both figures lit naturally by the same cold moonlight as the environment, fully integrated into the scene, documentary realism, no compositing look. Quiet dread.

> Three things doing the work: **asymmetric staging** (near/large vs far/small),
> the duplicate **half-hidden rather than presented**, and the explicit
> *"lit by the same moonlight, integrated, no compositing look"* to defeat the
> cut-out failure. Symmetry is fatal here — two equal figures reads as a portrait
> of twins, not a haunting. The horror is that something is where it shouldn't be.

### 8. THE HOLLOW — *show the EDGE, never the void*

> **Learned the hard way:** prompting "an absence, negative space, the dark as
> the subject" produces a literally black frame with nothing in it. You cannot
> depict nothing. Depict the **boundary** where the dark is eating a world you
> can still see, and make a guttering light sit right on that edge.

Appalachian folk horror concept art, cinematic, painterly, restrained dread. A lantern-lit homestead and yard at night on the left half of the frame, clearly visible and readable, warm amber light on the porch, fence posts, long grass. Advancing from the right, a wall of impossible absolute blackness - not shadow, an absence with no detail inside it - has already swallowed half the yard and the entire treeline, cutting a hard unnatural edge straight across the scene. A lantern right at the boundary is guttering out to an ember. The contrast between the warm lit world and the featureless black is the subject. No creature, no figure, no monster. Cold, wrong, silent.

### 9. CLUE BOARD KEY ART — *Steam capsule candidate; try Nano Banana Pro*
Cinematic hero shot, folk horror, painterly and tactile, warm lantern light against cold dark, shallow depth of field, no legible text, no watermark. A cork detective board in warm lantern light, pinned with weathered paper clue cards and old photographs connected by taut red yarn, a hand-drawn map of a mountain holler in one corner, contradictions circled in pencil. Paper texture, pin shadows, intimate and obsessive. A story half-solved.

### 10. MAIN MENU KEY ART — *the watching eye*

> Deliberately steps outside the folk-horror language of everything above —
> this is cosmic, symbolic, and that's allowed on a menu where it wouldn't be
> in-game. Reads as "the valley itself is awake and watching," which is the
> Hollow's idea taken to its most literal.
>
> Two clauses are load-bearing: **"half lost behind drifting cloud and haze so
> it reads as part of the sky itself"** stops the eye looking like a sticker,
> and **"empty dark sky in the upper left for a title"** reserves space for the
> logo and menu items. Menu art with no negative space is unusable.
>
> Good candidate for **Nano Banana Pro** rather than Soul Cinema: it's a
> standalone piece (consistency doesn't matter), it's displayed full-screen so
> 4K helps, and it has several elements that must relate correctly — which
> Gemini-family models follow more reliably. Keep the painterly clause or it
> will drift photoreal.

Painterly digital illustration, visible brushwork, muted limited palette, not photorealistic. Appalachian folk horror key art, cinematic and restrained. A lone old sedan drives away from the viewer down a narrow dirt road at night, red tail lights glowing, headlights cutting a cone of light through thin ground fog. Dense forest crowds in on both sides, tall bare hardwoods and dark pines silhouetted almost black. Above, filling the entire night sky, an enormous eye watches the road below: the iris vast and dim, the pupil a deep black well, half lost behind drifting cloud and haze so it reads as part of the sky itself rather than pasted onto it. Cold blue night against warm amber headlights. Quiet, oppressive, the feeling of being watched by something too large to run from. A large area of empty dark sky in the upper left, clear of detail, for a title to sit. No text, no watermark, no lettering.

---

## 6. Saving your work (do not skip)

The trial expires; the files don't. **This is the entire point of the exercise.**

Download every keeper into `Docs/ArtBible/` named by prompt number:

```
01-basecamp-a.png
02-homestead-c.png
06-wendigo-b.png
```

Then commit them to the repo so they're backed up on GitHub.

---

## 7. If a result misses

Paste the image back to Claude, or just describe the failure. Common fixes:

| Problem | Add to the prompt |
|---|---|
| Too clean / new | `derelict, rotting wood, peeling paint, rust streaks, overgrown` |
| Not scary enough | `ominous, oppressive, wrong, unsettling negative space` |
| Too bright | `night, near-darkness, single light source, deep shadow` |
| Creature reads as a person | `inhuman proportions, elongated limbs, wrong joint articulation, not human` |
| Too much monster on screen | `glimpsed at distance, obscured by fog, mostly hidden` |
| Fantasy/generic | `Appalachian, 1930s rural America, folk horror, grounded` |
| Evergreen/tropical foliage | `bare deciduous hardwoods, oak and hickory` |

### Known issue: everything comes out too dark

Using a dark night image as the Color Transfer reference drags exposure down on
every scene that lacks a strong practical light in frame (a lantern, a fire).
Observed on the homestead and creek prompts. Fix both ways:

1. Drop Color Transfer strength to ~50% — it should nudge hue, not set exposure.
2. Append to any prompt without a light source in it:

```
moonlight breaking through the fog, clear value separation between foreground midground and background, subject readable, not crushed to black
```
