# Aristocrat

A [Unity Mod Manager](https://github.com/newman55/unity-mod-manager) mod for **Pathfinder: Wrath of the Righteous** that adds the **Aristocrat** inquisitor archetype.

Aristocrats are historians and scholars who believe civilization advances through knowledge, order, and truth. The right to rule is earned by wisdom, and they abhor the stupidity of the crowd.

## What it adds

Aristocrat follows the Faith Hunter, with these changes:

- **Judgment** is limited to Purity, Protection, and Smiting. Smiting begins as Smiting (Magic) and becomes Smiting (Alignment) at 6th level.
- The teamwork feat selections at **15th** and **18th** level are removed.
- **Martial Weapon Proficiency** at 1st level.
- **Weapon Focus** at 1st level, without the usual prerequisites.
- **Greater Weapon Focus** at 10th level, without the fighter level requirement.
- **Right to Rule** from 4th level. While one of your judgments is active:
  - **4th:** +2 insight bonus to AC against attacks from chaotic creatures.
  - **8th:** +2 insight bonus on saving throws against chaotic creatures and chaotic effects.
  - **12th:** +2 insight bonus on weapon attack rolls against chaotic creatures.
  - **16th:** add your Wisdom modifier to weapon damage rolls against chaotic creatures.

Create a new Inquisitor and choose Aristocrat.

## Requirements

- Pathfinder: Wrath of the Righteous **2.7.0**
- Unity Mod Manager **0.23** or later
- The Inquisitor class and the Faith Hunter archetype (the mod logs and stops if either blueprint is missing)

## Build

`Directory.Build.props` points at a local game install. A successful build copies `Aristocrat.dll` and `Info.json` into that install's `Mods\Aristocrat` folder.

```
dotnet build -c Release
```

Enable **Aristocrat** in Unity Mod Manager, then start a new Inquisitor.
