# UI Folder Guide

## Purpose
- HUD, tutorial interaction, glossary, gate, tooltip, onboarding presentation

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
3. `DHTableEditor.cs`
4. `StepNavigator.cs`
5. `SceneNavigationBar.cs`

## Refactor Rule
- keep UI files view-oriented
- if one file both builds UI and parses/transforms data, extract helper classes first
