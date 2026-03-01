# ⚔️ Unity 2D Hardcore Action Kit



## 👥 Authors

| Role | Name |
| :--- | :--- |
| **Leader** | **Lu Qi** |
| Member | YanWenXin |
| Member | LiYanZe |
| Member | LiuYiXun |

> 🎓 **Note**: This project was developed as a university course assignment and is now open-sourced for **educational and reference purposes**.
>
> 🌟 **Support**: If this project helps you in your game development journey, please **Star** this repository! Your support means a lot to us.

---

## 📖 Introduction

A **Cyberpunk-style 2D side-scrolling action game framework**. The project features **6 distinct levels**, each offering a unique gameplay experience. It includes a robust **Finite State Machine (FSM)** framework, a multi-phase Boss behavior tree, a dynamic trap system, and a comprehensive status effect (Debuff) mechanism.

> ⚠️ **IMPORTANT**: To ensure the Singleton Managers initialize correctly, please start the game from the **`Start`** scene located in `Assets/Project/Scenes/`.

---

## 📸 Core Features

<!-- You can replace the paths below with your actual GIF paths -->
| Boss Fight | Dynamic Traps |
| :---: | :---: |
| ![Boss Fight](Docs/Gifs/boss_preview.gif) | ![Traps](Docs/Gifs/trap_preview.gif) |

### 1. 💀 Advanced Trap System
A highly configurable trap system where hazards are more than just static colliders.
*   **Dynamic Laser**: Supports horizontal/vertical axis toggling. Operates on a *"Warning (Thin Line) → Damage (Wide Beam) → Cooldown"* cycle.
*   **Frame-Synced Fire Trap**: Hitboxes are strictly synchronized with the flame animation frames. Supports adjustable damage delay and duration.
*   **Frost Arrow Tower**: Supports 4-directional firing. Inflicts damage and applies a temporary **Slow Debuff** on hit.
*   **Pendulums & Patrolling Spikes**: Features physics-based harmonic swinging and customizable patrol paths (Linear, Rectangular, or Diamond trajectories).

### 2. 👹 Multi-Phase Boss AI
Showcasing complex Boss logic design using behavior trees and state machines.

#### **The Demon (Three-Phase Boss)**
*   **Phase 1 (Normal)**: Fan-shaped range detection & predictive jump attacks.
*   **Phase 2 (Enraged)**:
    *   **Roar**: Knocks back the player and enters an invulnerable state.
    *   **Teleport**: Uses raycasting to find a safe spot behind the player for a backstab.
    *   **Wave**: Unleashes multi-stage long-range projectile attacks.
*   **Phase 3 (Ultimate)**:
    *   **Screen Dash**: Vanishes and locks onto screen edges, generating warning lines before executing high-speed dashes.
    *   **Summon**: Spawns tracking missiles and ground spikes.
    *   **Afterimage**: Generates visual trails during movement.

#### **Storm Lord**
*   Features **Thunder Slam** (with ground detection), **Laser Array Sweeps**, and an auto-healing mechanic when health is critically low.

### 3. 🤖 Enemy AI (FSM)
Diverse enemy behaviors implemented via Finite State Machines:
*   **Archer**: Line-of-sight detection with parabolic projectile trajectory calculation.
*   **Patroller**: Automatic edge detection and wall reversal.
*   **Elite Guard**: Built-in hit counter; triggers **Super Armor (Iron Body)** counter-attacks after taking consecutive hits.

### 4. 🎮 Combat & Game Feel
*   **Juice**: Implements **Hit Stop** (freeze frames), **Screen Shake** (Cinemachine Impulse), and dynamic **Knockback Vectors**.
*   **Mobility**: Double Jump, **Dash** (with i-frames), and Dash Jump mechanics.
*   **Skills**: Chargeable **Heavy Attack** & **Burn Aura** (AoE damage over time).

### 5. 💰 Systems & Progression
*   **Shop System**: A lightweight economy system allowing players to purchase upgrades and consumables using collected currency.
*   **Storyline System**: An integrated dialogue and narrative framework that drives the game's plot across the 6 levels.

### 6. 🛠️ Built-in Developer Tools
Press **`Ctrl + ~`** to toggle the debug console:
*   **God Mode**: Invincibility.
*   **No Clip**: Disable gravity and collision to fly through walls.
*   **One Hit Kill**: Instantly defeat enemies.
*   **Teleport**: Teleport to the mouse cursor position.

---

## 🚀 Quick Start

1.  Clone the repository:
    ```bash
    git clone https://github.com/YourUsername/YourRepo.git
    ```
2.  Open the project with **Unity 2022.3** or higher.
3.  Open the scene: `Assets/Project/Scenes/Start.unity`.
4.  Click **Play**.

---

**Thanks for checking out our project! Don't forget to leave a Star ⭐ if you like it!**
