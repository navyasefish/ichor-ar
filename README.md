# ICHOR

An augmented reality city-building and strategy game set in ancient Greek mythology. Players construct and manage a kingdom by placing districts and structures on real-world flat surfaces using AR, while balancing resources and divine favor with the Greek pantheon.

**Platform:** Android / iOS  
**Engine:** Unity 2022.3.62f3  
**Genre:** AR City-Building / Strategy

---

## Overview

ICHOR merges traditional city-building mechanics with real-world spatial interaction. Using augmented reality, players place cities and districts on physical surfaces (tables, floors), transforming their environment into an interactive strategic map. The capital city serves as the population center, supported by specialized districts — farming, mining, ports, military camps, and hunting grounds — that form an interconnected resource economy.

Progression is driven by resource management, divine favor, and strategic decision-making. Each district is governed by a Greek god whose satisfaction directly affects efficiency, stability, and city conditions. A secondary multiplayer layer embeds a 2D hex-based board game within city structures, enabling competitive and cooperative play whose outcomes feed back into city resources.

---

## Core Systems

### Augmented Reality Placement
Cities and structures are placed on detected flat surfaces via AR plane detection. Players can rotate, reposition, and confirm placements through touch gestures and dedicated UI buttons. The system uses AR Foundation for cross-platform support (ARCore on Android, ARKit on iOS).

### Resource & Economy
Four primary resources sustain the city:

| Resource | Role |
|----------|------|
| Gold | Trade, upgrades, building upkeep |
| Food | Population sustenance |
| Stone | Construction and repairs |
| Ichor | Rare premium resource for high-tier upgrades and divine interventions |

Buildings must be connected to a district via a road network to produce resources. Disconnected buildings remain inactive but still consume resources from the nearest district, making road planning strategically important.

### Divine Favor System
Each district is governed by a Greek god. Favor is tracked on a 0–10 scale.

**Pantheon Gods** (primary districts — have both boons and banes):

| God | District | Boon (7+ Favor) | Bane (0–3 Favor) |
|-----|----------|-----------------|------------------|
| Zeus | Capital | Loyalty: boosts all building efficiency | Riots across production and trade |
| Poseidon | Port / Sea Trade | Increases NPC trade deal success | Earthquakes/cyclones; destroys buildings |
| Ares | Military Camp | Total protection from external wars | Frequent wars; forces resource drain |
| Hephaestus | Mining District | Boosts mining and extraction rates | Drastic drop in mining production |
| Demeter | Farming District | Increases farm output and harvest yields | Drastic decrease in farming efficiency |
| Artemis | Hunting Camps | Boosts hunting speed and early defense | Loss of hunting speed; hunters no longer act as soldiers |

**Bonus Gods** (utility/support — no bane at low favor, just no benefit):

| God | Role | Boon (7+) |
|-----|------|-----------|
| Hades | Population | Lowers mortality rates in war and disaster |
| Hermes | Supply Chain | Minimizes economic collapse when districts disconnect |
| Apollo | Prophecy | Predicts timing/location of next disaster or war |
| Hestia | Warmth | Negates winter efficiency dips and food consumption spikes |
| Dionysus | Versatility | Small additive bonus to all active boons |
| Aphrodite | Diplomacy | Boosts trade multipliers and inter-district morale |
| Athena | Research | Generates Research Points via milestones through her Temple |
| Hera | Guide | Governs friend visits and social interactions |

Resource production is dynamically multiplied by god favor: Zeus affects all resources universally; Demeter and Poseidon strongly boost food; Hephaestus boosts stone and gold; Artemis provides a smaller food bonus. Output is clamped to prevent negative production.

### Seasonal Mechanics
Winter cycles reduce work efficiency and increase food consumption. Players must prepare by stockpiling food or maintaining favor with Hestia (winter counter) and Demeter (farming boost). Seasonal pressure enforces long-term planning over short-term optimization.

### District Connectivity
Buildings connect to districts using a BFS (Breadth-First Search) algorithm that propagates a shared district value outward tile by tile from a source building or district center. Road connections take priority over proximity — a building routes to the district it is road-connected to, not necessarily the nearest one. Visual indicators signal resource shortages and full storage states.

### Multiplayer Board Game
A 2D hex-based board game is unlocked through specific city structures. To enter a match, both players stake a fixed number of soldiers and coins. The winner receives double the waged resources; the loser forfeits their contribution. Matches affect resources and progression without enabling direct city destruction.

---

## Storyline

Wandering settlers discover a crumbling nameless temple in an uninhabited wilderness and choose to restore it. Zeus claims the site as his own and sends Prometheus bearing the gift of fire — igniting human progress and the ability to build, farm, mine, and expand. Athena descends as a strategic guide. From this point, divine favor becomes the foundation of survival: the civilization's fate is intertwined with the Greek pantheon, and neglecting the gods has direct consequences.

---

## Technical Stack

| Component | Technology |
|-----------|-----------|
| Game Engine | Unity 2022.3.62f3 |
| Language | C# |
| AR Framework | AR Foundation 5.2 (ARCore 5.2 / ARKit 5.2) |
| Rendering | Universal Render Pipeline (URP) 14.0.12 |
| Input | Unity Input System 1.14.0 |
| UI | TextMesh Pro 3.0.9 |
| XR Interaction | XR Interaction Toolkit 3.1.2 |
| Location | Google Cloud Location API |
| Version Control | Git / Unity Version Control |
| 3D Modeling | Blender |
| 2D Art | Adobe Illustrator, Photoshop, Affinity, Ibis Paint, Procreate |

### Hardware Requirements

|  | Minimum | Recommended |
|--|---------|-------------|
| RAM | 4 GB | 6–8 GB |
| Processor | Quad-core | Octa-core |
| GPU | Integrated mobile GPU | High-performance mobile GPU |
| Display | 720p | 1080p+ |
| AR Support | ARCore / ARKit device | ARCore / ARKit device |
| Network | — | Stable connection for multiplayer |

---

## Current Progress

### Mid-Term
- **2D Strategic Game Prototype** — hex grid board game with player info panels, resource displays, and unit placement for testing interaction and tactical movement.
- **AR Plane Detection & Building Placement** — flat surface detection, virtual building placement with snapping, rotation, and confirmation via UI buttons.
- **Transportation System** — inter-city resource transfer prototype simulating logistics between a production city (e.g. mining) and a central city requiring those materials.

### End-Term
- **Inventory System** — players select from building types and place them into the world; buildings actively generate resources or trigger effects based on their type.
- **Crop Growth** — designated crop plots progress through growth phases; harvested crops contribute to both local and global inventory and sustain city needs.
- **District Connectivity (BFS)** — buildings propagate and share district values across the tile grid based on road network connections; buildings without road links remain inactive.
- **Road-Based District System** — road network determines district membership and resource routing; disconnected buildings consume resources without producing, reinforcing road planning.
- **God-Based Resource Production** — favor levels dynamically modify production multipliers per resource type per god, with output clamped to prevent negatives.

---

## Getting Started

### Prerequisites
- Unity 2022.3.62f3
- Android SDK / Xcode (depending on target platform)
- A physical device with ARCore (Android) or ARKit (iOS) support

### Setup
1. Clone the repository.
2. Open the project in Unity 2022.3.62f3.
3. In **Build Settings**, switch the platform to Android or iOS.
4. Connect a compatible device and build to it, or use Unity's Play Mode with the XR Device Simulator for desktop testing.

---

## References

1. J. Li, E. D. van der Spek, L. M. G. Feijs, F. Wang, & J. Hu — "Augmented reality games for learning: A literature review"
2. I. Paraschivoiu et al. — "Crafting Cities Together: Co-located Collaboration with Augmented Reality for Urban Design," CSCW 2025
3. "SARA: A Microservice-Based Architecture for Cross-Platform Collaborative Augmented Reality" — arXiv
4. David Kadish et al. — "Towards Situation Awareness and Attention Guidance in a Multiplayer Environment using Augmented Reality and Carcassonne," arXiv 2022
5. T. H. Laine — "Mobile Educational Augmented Reality Games: A Systematic Literature Review and Two Case Studies," MDPI Computers 2018
6. "Augmented Reality Games II: The Gamification of Education, Medicine and Art" — Springer
