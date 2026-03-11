# Interactive Matrix Visualization -- Design Reference

> **Purpose**: Research document capturing the interaction pattern from ncase.me/matrix
> and related explorable explanations, adapted as a design reference for KineTutor3D's
> 4x4 DH transformation matrix visualization.
>
> **Status**: Research only -- not a spec. Created 2026-03-11.

---

## 1. Source Material

| Reference | URL | Relevance |
|---|---|---|
| The Magnificent 2D Matrix | https://ncase.me/matrix/ | Primary pattern source |
| The Matrix Arcade | https://yizhe-ang.github.io/matrix-explorable/ | Grid-warping variant, extends to 3D |
| Explorable Explanations: 4 More Design Patterns | https://blog.ncase.me/explorable-explanations-4-more-design-patterns/ | Reusable pedagogy patterns |
| How I Make Explorable Explanations | https://blog.ncase.me/how-i-make-an-explorable-explanation/ | Methodology and philosophy |
| Linear Transformations -- Visualize It | https://visualize-it.github.io/linear_transformations/simulation.html | Lightweight 2D sandbox |
| Matrix Transformations -- GeoGebra | https://www.geogebra.org/m/sqG26hQj | Classroom-oriented variant |
| MatVis (shad.io) | https://shad.io/MatVis/ | Canvas-based matrix explorer |
| Explorable Explanations hub | https://explorabl.es/math/ | Curated index of similar projects |

---

## 2. Core Interaction Pattern (ncase.me/matrix)

### What the user does

1. **Edits matrix cells directly.** The 2x2 transformation matrix is presented as four
   editable numeric fields. The user changes values by clicking/dragging or typing.
2. **Hovers over transformed elements.** Tooltips reveal the arithmetic behind each
   output component (e.g., `1*x + 0*y`), connecting numbers to operations.
3. **Observes a grid of input vectors** (dots arranged on a 2D plane) transform in
   real-time as matrix values change.

### What the user sees

- A **before/after vector field**: the original dot grid and the transformed dot grid
  are rendered simultaneously so the user can mentally diff them.
- **Real-time feedback**: every keystroke or drag immediately updates the visual output.
  There is zero lag between numeric change and geometric result.
- **Calculation breakdown on hover**: mousing over any output dot shows the exact
  multiply-and-add steps that produced its coordinates.

### Key insight

The matrix is never presented as an abstract block of numbers. It is always shown
**beside** the geometric effect it produces, creating a permanent bidirectional link
between symbolic and spatial understanding.

---

## 3. How Matrix Values Map to Visual Transformations

| Matrix cell(s) changed | Visual effect | Geometric concept |
|---|---|---|
| Diagonal (a, d) scaled equally | Uniform scaling of dot field | Isotropic scale |
| Diagonal (a, d) scaled unequally | Stretching along one axis | Anisotropic scale |
| Off-diagonal (b, c) | Shearing / skewing of grid | Shear transformation |
| Rotation pattern (cos/sin) | Rigid rotation of dot field | Rotation |
| Determinant approaches 0 | Dots collapse toward a line | Rank reduction / singularity |
| Negative determinant | Mirrored/flipped grid | Reflection |

The Matrix Arcade (Yi Zhe Ang) extends this by rendering the **full coordinate grid**
rather than discrete dots, making it visually obvious that linear transformations
preserve straight lines and fix the origin -- the two defining properties.

---

## 4. Pedagogical Approach -- Why It Works

### 4.1 Nicky Case's three-step framework

1. **Start with a question, not a definition.** The page opens with a relatable
   context (game graphics) rather than formal math. This makes the learner curious
   before introducing notation.

2. **Climb the Ladder of Abstraction.** Move from concrete manipulation (drag a
   number, see a shape change) to abstract understanding (the matrix encodes a
   linear map). Each step builds causally on the previous one ("therefore" / "but"),
   not as a disconnected list.

3. **End with a sandbox.** After the guided walkthrough, the user has an open-ended
   playground to test their own matrices. This shifts ownership from
   teacher-posed questions to student-generated exploration.

### 4.2 Four design patterns (from the blog)

| Pattern | Description | KineTutor3D applicability |
|---|---|---|
| **Puzzle It Out** | Gate progress behind a problem the learner must solve | Step-completion gates in Guided Lesson flow |
| **Place Your Bets** | Ask the learner to predict before revealing | "What will the robot do?" before applying FK |
| **Role Play** | Inhabit a perspective to build empathy | Less relevant for math-heavy content |
| **Sandbox Mode** | Open-ended exploration after guided intro | Existing Sandbox mode in KineTutor3D |

### 4.3 Principles that make interactive math visualization effective

1. **Immediate, continuous feedback.** Not "press Run" -- the output updates as the
   input changes, preserving the user's train of thought.
2. **Bidirectional mapping.** The user can start from numbers and see geometry, or
   start from a desired geometry and discover which numbers produce it.
3. **Progressive disclosure of complexity.** Start with identity matrix, add one
   degree of freedom at a time.
4. **Visible arithmetic.** Hover tooltips show the multiply-and-add steps, bridging
   symbolic and spatial understanding.
5. **Constrained initial state.** The opening state is simple (identity or near-identity),
   so the first change produces a clearly observable, non-overwhelming effect.

---

## 5. Adapting the Pattern for 4x4 DH Matrices in 3D

### 5.1 Challenges unique to KineTutor3D

| Challenge | ncase.me (2D, 2x2) | KineTutor3D (3D, 4x4 DH) |
|---|---|---|
| Matrix size | 4 cells, all editable | 16 cells, but DH constrains to 4 parameters (theta, d, a, alpha) |
| Visual output | 2D dot grid on screen | 3D robot arm in perspective viewport |
| Degrees of freedom | 4 independent | 4 per link, but chained via composition |
| Composition | Single matrix | Product of N link matrices |
| Learner prerequisite | Basic algebra | Trig, 3D spatial reasoning |

### 5.2 Recommended adaptation

#### A. Parameter-to-matrix-to-robot pipeline (single link)

```
  [DH Parameter Sliders]          [4x4 Matrix Display]         [3D Viewport]
  theta  ----+                    | cθ  -sθcα  sθsα  a·cθ |
  d      ----|--- live update --> | sθ   cθcα -cθsα  a·sθ |  ---> frame/joint
  a      ----+                    |  0    sα    cα     d   |       rendered in 3D
  alpha  ----+                    |  0     0     0     1   |
```

- **Sliders** are the primary input (analogous to ncase's editable cells), because
  DH parameters are the natural degrees of freedom -- not raw matrix entries.
- The **4x4 matrix display** updates in real-time as sliders move. Cells that change
  are highlighted (pulse or color shift) so the learner sees which parameter
  affects which cells.
- The **3D viewport** shows the resulting coordinate frame moving in space.

#### B. Hover/tap arithmetic reveal

When the user hovers or taps a matrix cell, a tooltip shows the formula:

```
cell [0,0] = cos(theta) = cos(45deg) = 0.707
```

This directly mirrors ncase's hover-to-reveal-arithmetic pattern but adapted to
trigonometric DH expressions.

#### C. Progressive disclosure across steps

| Step | What is shown | What is editable |
|---|---|---|
| Step 1: Single joint | One DH matrix, one frame | theta slider only |
| Step 2: Full single link | One DH matrix, all 4 params | All 4 sliders |
| Step 3: Two-link chain | Two matrices + product | Both links' sliders |
| Step 4: Full robot | N matrices, composed | All joints |
| Sandbox | Free-form DH table | Everything |

This follows the "climb the Ladder of Abstraction" principle exactly.

#### D. "Place Your Bets" integration

Before the learner adjusts a slider, prompt:

> "If you increase theta by 30 degrees, which direction will the end-effector move?"

Show a ghosted prediction target. After they commit, animate the actual result.
Mismatch creates productive cognitive dissonance.

#### E. Grid-warping option (Matrix Arcade style)

For advanced learners, offer a toggle that renders a 3D coordinate grid around the
current frame. As parameters change, the grid warps, making it visually obvious how
the transformation reshapes local space. This is the 3D equivalent of the Matrix
Arcade's full-grid rendering.

---

## 6. Key UI/UX Principles Extracted

1. **Numbers and geometry are always co-visible.** Never show the matrix on a
   separate screen from the 3D viewport. They must be side-by-side or overlaid.

2. **The input mechanism matches the natural degree of freedom.** For DH, that means
   sliders for (theta, d, a, alpha) -- not raw matrix cell editing, which would
   produce invalid (non-SE(3)) matrices.

3. **Highlight causality.** When a slider moves, the matrix cells that change should
   flash or pulse. When a matrix cell is hovered, the slider that controls it should
   highlight. Bidirectional linking.

4. **Constrain before you liberate.** Start with one slider unlocked. Only after the
   learner demonstrates understanding (or explicitly requests) do you unlock more.

5. **Zero-latency feedback.** The 3D viewport must update at interactive frame rates
   (>30 fps) during slider drag. Any perceptible lag breaks the direct-manipulation
   illusion.

6. **Tooltip = teaching moment.** Every cell, every slider, every frame axis should
   have a hover state that explains what it is and how it was computed.

7. **Identity as home base.** The default/reset state should be the identity matrix
   (all joints at zero). This gives the learner a known reference to return to.

8. **Determinant/validity indicator.** Show a small badge or color cue indicating
   whether the current matrix is a valid rotation (det = 1, orthonormal). In DH
   mode this is always true by construction, but in any future free-edit mode it
   becomes important.

---

## 7. Related Interactive Math Visualization References

| Project | What it does | Notable technique |
|---|---|---|
| **3Blue1Brown -- Essence of Linear Algebra** | Video series with animated matrix transformations | Grid-warping animation showing basis vectors stretching/rotating |
| **The Matrix Arcade** (Yi Zhe Ang) | Interactive grid-warping for 2D and 3D matrices | Scrub-through animation timeline; extends to 3D rotation |
| **GeoGebra Matrix Transformations** | Classroom applet for 2D matrix transforms | Familiar to educators; embeddable |
| **Visualize It -- Linear Transformations** | Lightweight browser simulation | Minimal UI, fast loading |
| **Immersive Linear Algebra** (Strang et al.) | Full interactive textbook | Chapter-level explorable figures embedded in narrative |
| **Red Blob Games** (Amit Patel) | Interactive tutorials on algorithms and math | Gold standard for scroll-driven interactive diagrams |
| **Better Explained** (Kalid Azad) | Intuition-first math articles | "Aha!" framing before formalism |
| **Mathigon** | Interactive math courses with manipulables | Step-gated progression with embedded exercises |

---

## 8. Summary: What KineTutor3D Should Take from This Pattern

1. **The slider-matrix-viewport pipeline** is the central interaction loop. Keep all
   three visible simultaneously with real-time linkage.

2. **Progressive disclosure** (one parameter -> one link -> chain -> sandbox) maps
   directly to the existing Guided Lesson / Sandbox architecture.

3. **Hover-to-reveal arithmetic** is low-cost to implement and high-value for
   bridging symbolic and spatial understanding of DH matrices.

4. **"Place Your Bets" prediction prompts** before each slider change can be
   integrated into the step-gated lesson flow.

5. **Grid-warping visualization** (optional advanced toggle) would differentiate
   KineTutor3D from static textbook DH presentations.

---

*End of design reference.*
