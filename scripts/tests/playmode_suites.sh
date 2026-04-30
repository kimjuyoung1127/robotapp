#!/usr/bin/env bash
# PlayMode suite registry for the grouped matrix runner.

to_lower() {
    printf '%s' "$1" | tr '[:upper:]' '[:lower:]'
}

playmode_group_label() {
    case "$(to_lower "$1")" in
        smoke) printf '%s\n' "Smoke" ;;
        flows) printf '%s\n' "Flows" ;;
        visuals) printf '%s\n' "Visuals" ;;
        *) return 1 ;;
    esac
}

playmode_group_suites() {
    case "$(to_lower "$1")" in
        smoke)
            cat <<'EOF'
KineTutor3D.Tests.PlayMode.AllButtonsSmokeTests
KineTutor3D.Tests.PlayMode.MathReadinessFlowSmokeTests
EOF
            ;;
        flows)
            cat <<'EOF'
KineTutor3D.Tests.PlayMode.FullSceneTransitionTests
KineTutor3D.Tests.PlayMode.RobotLibrarySandboxRoutingTests
KineTutor3D.Tests.PlayMode.SceneFlowSmokeTests
KineTutor3D.Tests.PlayMode.UxFlowSmokeTests
EOF
            ;;
        visuals)
            cat <<'EOF'
KineTutor3D.Tests.PlayMode.Phase5CommonVisualsSmokeTests
KineTutor3D.Tests.PlayMode.UIPanelDesignSystemSmokeTests
KineTutor3D.Tests.PlayMode.VisualizationSmokeTests
EOF
            ;;
        *)
            return 1
            ;;
    esac
}

playmode_all_groups() {
    printf '%s\n' smoke flows visuals
}
