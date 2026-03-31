# UI Folder Guide

## Purpose
- HUD, tutorial interaction, glossary, gate, tooltip, onboarding presentation

## Page Folders
- `GuidedLesson/`: AppController-driven lesson HUD, DH table, step panels, matrix/why-moved, beginner helpers
- `Onboarding/`: onboarding modal and authored/runtime onboarding layout helpers
- `RobotLibrary/`: library cards, detail drawer, showroom, library scene coordinators
- `RobotControl/`: FR5/RobotControl panels, diagnostics, axis overlay, hand-input diagnostics
- `MathReadiness/`: math-readiness page panel and formatting helpers
- `Sandbox/`: sandbox-specific runtime UI builders
- `Shared/`: cross-page navigation, tooltip, toast, shared overlays/contracts
- `DesignSystem/`: tokens, typography, icon/style/component factories
- `Data/`: UI config/data assets and ScriptableObjects

## Allowed Here
- view creation/styling
- input parsing/formatting
- AppController event binding
- tutorial step visibility and HUD navigation

## Not Allowed Here
- FK algorithm math
- donor mesh traversal
- robot coordinate conversion

## Read First
1. root `AGENTS.md`
2. `docs/ref/architecture-mermaid.md`
3. `GuidedLesson/DHTableEditor.cs`
4. `GuidedLesson/StepNavigator.cs`
5. `Shared/SceneNavigationBar.cs`

## Refactor Rule
- keep UI files view-oriented
- organize by page first, and only keep true cross-page widgets in `Shared/` or `DesignSystem/`
- if one file both builds UI and parses/transforms data, extract helper classes first
