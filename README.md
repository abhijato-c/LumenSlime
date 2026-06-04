# LumenSlime

![Banner](https://github.com/abhijato-c/LumenSlime/blob/main/Banner.png)

> A physics based vertical tower climbing platformer.

---

## Gameplay
In LumenSlime, you control Squeak, a glowing, bouncy slime who has to reach the top of the tower. Game components inlcude Spikes, which kill you, and jump pads for a massive boost in jump power. Checkpoints are automatically saved when you reach them, and you respawn at the latest checkpoint after you die, or even restart the game.

---

## Controls

| Action | Key 1 | Key 2 | Key 3|
| :--- | :--- | :--- | :--- |
| Move Left | A | Left Arrow | - |
| Move Right | D | Right Arrow | - |
| Jump | W | Up Arrow | Space |
| Push Down | S | Down Arrow | - |

---

## Download & Play

Download from [itch.io](https://m-8000.itch.io/lumenslime) to play!
Builds available and tested for Linux and Windows.

---

## Technical

This game was built completely in Unity. The main script for slime movement is controlled by physics, and uses forces and acceleration to move the slime. Because of this physics based approach, this game may feel shightly different from other platformers with the controls. PlayerPrefs class in Unity is used to store checkpoints persistently. The level design is done with tilemaps.

---

## Motivation

I wanted to learn a bit of game design with a simple project, and basic animations like paralax and squish effects. This project didn't take too much effort or time, and I learnt the concepts of Unity that I wanted to.

---

## AI

AI used for debugging physics movements, and as autocomplete.