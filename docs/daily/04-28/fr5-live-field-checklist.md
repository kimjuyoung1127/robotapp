# FR5 Live Field Checklist Log

Date: 2026-04-28 (KST)

## Summary

- Added `docs/ref/product/roadmap/fr5-live-field-checklist.md`.
- Captured the field checklist for MacBook Ethernet + FAIRINO FR5 live readback preparation.
- Locked the immediate policy: V3 can move toward main only after verification, V1/V2 are not deleted yet, and the first hardware session is readback-only.
- Updated handoff flow so a MacBook field session reads `docs/ref/product/ux/robotcontrol-next-session-handoff.md` first.
- Added clone/pull guidance for `codex/robotcontrol-v3-toolkit` and expected implementation commit `d8c0726 Add FR5 readback-only live monitor`.
- Added direct SDK probe vs `FAIRINO_BRIDGE_URL` bridge fallback guidance for macOS.
- Added field evidence requirements for `latest-state.json`, `latest-drift.json`, and session NDJSON logs.

## Decisions

- Do not remove legacy V1/V2 before V3 is merged, field-readback verified, and rollback evidence exists.
- Treat `서보ON` as robot enable/motor-ready, not as a normal beginner action.
- Treat `현재 위치 읽기` as the plain-language replacement for sync/readback.
- Do not use one ever-growing JSON file for live telemetry.
- Use `latest-state.json` for current state, `latest-drift.json` for comparison, and session `*.ndjson` for history.
- AI agent may read/compare live values, but must not directly send live robot movement.

## Sources Checked

- `docs/ref/product/robots/fairino-fr5-integration-reference.md`
- `Assets/Scripts/App/Fairino/LiveFairinoClient.cs`
- `Assets/Editor/KineTutor3D/FairinoLiveSmokeTools.cs`
- FAIRINO SDK docs: `https://fairino-doc-en.readthedocs.io/latest/SDKManual/index.html`
- FAIRINO C# intro: `https://fairino-doc-en.readthedocs.io/3.7.2/SDKManual/c%23_intro.html`
- FAIRINO support/download surface: `https://fairino.support/`

## Follow-Up

- Pull `codex/robotcontrol-v3-toolkit` on the MacBook before testing hardware.
- Run field smoke only after Ethernet/ping check.
- Do not merge V3 to `main` or remove V1/V2 until readback evidence is captured.
