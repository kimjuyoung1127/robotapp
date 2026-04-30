// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// 메인패널 헤더의 최소 상태칩과 보조패널 설명/선택 파츠 아코디언을 관리합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ViewportAuxInfoController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;

        private VisualElement root;
        private Label workPanelTitle;
        private Label workPanelSummary;
        private Label workPanelChipPrimary;
        private Label workPanelChipSecondary;
        private Button descriptionToggleButton;
        private VisualElement descriptionBody;
        private Label descriptionSummary;
        private Label descriptionDetail;
        private bool descriptionExpanded;
        private bool initialized;
        private bool isInitializing;

        private void OnEnable()
        {
            TryInitialize();
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string ToggleDescriptionForDebug()
        {
            ToggleDescription();
            return $"descriptionExpanded={descriptionExpanded}";
        }

        public string ToggleSelectionForDebug()
        {
            return "selectionInfo=removed";
        }

        internal void RefreshFromBinder(RobotControlV3RuntimeSnapshot snapshot, PendantV3LocalState shellState)
        {
            if (!initialized && !TryInitialize())
            {
                return;
            }

            ApplyWorkPanelHeader(snapshot, shellState);
            ApplyDescriptionAccordion(snapshot, shellState);
        }

        private bool TryInitialize()
        {
            if (initialized)
            {
                return true;
            }

            if (isInitializing)
            {
                return false;
            }

            isInitializing = true;
            try
            {
            document ??= GetComponent<UIDocument>();
            root = document?.rootVisualElement;
            if (root == null)
            {
                return false;
            }

            workPanelTitle = root.Q<Label>("WorkPanelTitle");
            workPanelSummary = root.Q<Label>("WorkPanelSummary");
            workPanelChipPrimary = root.Q<Label>("WorkPanelChipPrimary");
            workPanelChipSecondary = root.Q<Label>("WorkPanelChipSecondary");
            descriptionToggleButton = root.Q<Button>("BtnViewportDescriptionToggle");
            descriptionBody = root.Q<VisualElement>("ViewportDescriptionBody");
            descriptionSummary = root.Q<Label>("ViewportDescriptionSummary");
            descriptionDetail = root.Q<Label>("ViewportDescriptionDetail");

            if (descriptionToggleButton == null)
            {
                initialized = false;
                return false;
            }

            descriptionToggleButton.clicked -= ToggleDescription;
            descriptionToggleButton.clicked += ToggleDescription;
            descriptionExpanded = false;
            UpdateAccordionState();
            initialized = true;
            return true;
            }
            finally
            {
                isInitializing = false;
            }
        }

        private void ApplyWorkPanelHeader(RobotControlV3RuntimeSnapshot snapshot, PendantV3LocalState shellState)
        {
            if (workPanelTitle != null)
            {
                workPanelTitle.text = ResolvePanelTitle(shellState);
            }

            if (workPanelSummary != null)
            {
                workPanelSummary.text = string.Empty;
                workPanelSummary.EnableInClassList("rc-hidden", true);
            }

            if (workPanelChipPrimary != null)
            {
                workPanelChipPrimary.text = shellState.CoordSystem;
            }

            if (workPanelChipSecondary != null)
            {
                workPanelChipSecondary.text = ResolveCompactState(snapshot);
            }
        }

        private void ApplyDescriptionAccordion(RobotControlV3RuntimeSnapshot snapshot, PendantV3LocalState shellState)
        {
            if (descriptionSummary != null)
            {
                descriptionSummary.text = ResolveDescriptionSummary(shellState, snapshot);
            }

            if (descriptionDetail != null)
            {
                descriptionDetail.text = ResolveDescriptionDetail(shellState, snapshot);
            }
        }

        private void ToggleDescription()
        {
            descriptionExpanded = !descriptionExpanded;
            UpdateAccordionState();
        }

        private void UpdateAccordionState()
        {
            descriptionBody?.EnableInClassList("rc-hidden", !descriptionExpanded);

            if (descriptionToggleButton != null)
            {
                descriptionToggleButton.text = descriptionExpanded ? "설명 접기" : "설명 펼치기";
            }
        }

        private static string ResolvePanelTitle(PendantV3LocalState shellState)
        {
            if (shellState.ActiveNavSection == "NavPoints")
            {
                return "티칭";
            }

            return shellState.ActiveWorkTab switch
            {
                "TabJointJog" => "관절",
                "TabTcpJog" => "TCP",
                "TabPointMove" => "저장 위치",
                _ => "쉬운조작",
            };
        }

        private static string ResolveCompactState(RobotControlV3RuntimeSnapshot snapshot)
        {
            if (snapshot.HasPendingPreview)
            {
                return snapshot.PendingCommandSummary.Contains("MoveL") ? "직선" : "관절";
            }

            return snapshot.StatusMotion switch
            {
                "일시정지" => "일시정지",
                "미리보기" => "미리보기",
                _ => "대기",
            };
        }

        private static string ResolveDescriptionSummary(PendantV3LocalState shellState, RobotControlV3RuntimeSnapshot snapshot)
        {
            if (shellState.ActiveNavSection == "NavPoints")
            {
                return $"저장 위치 · {snapshot.PendingCommandSummary}";
            }

            return shellState.ActiveWorkTab switch
            {
                "TabJointJog" => $"관절 조그 · {snapshot.StatusMotion}",
                "TabTcpJog" => $"TCP 조그 · {FormatCoordSystemDisplay(shellState.CoordSystem)} · {snapshot.PendingCommandSummary}",
                "TabPointMove" => $"저장 위치 · {snapshot.PendingCommandSummary}",
                _ => $"쉬운 조작 · {snapshot.PendingCommandSummary}",
            };
        }

        private static string ResolveDescriptionDetail(PendantV3LocalState shellState, RobotControlV3RuntimeSnapshot snapshot)
        {
            if (shellState.ActiveNavSection == "NavPoints")
            {
                return "저장 위치, 실행 순서, 작업 묶음은 저장 메뉴에서 다룬다. 메인패널은 현재 로봇 위치와 미리보기 확인에 집중한다.";
            }

            return shellState.ActiveWorkTab switch
            {
                "TabJointJog" => "메인패널에서는 로봇만 보고, 관절값 입력/버튼은 보조패널에서만 만진다. 미리보기 후 적용 순서를 유지한다.",
                "TabTcpJog" => $"TCP는 {FormatCoordSystemDisplay(shellState.CoordSystem)}으로 조작합니다. Z/RX/RY/RZ 보조 조작은 로봇을 가리지 않게 여기서만 보여줍니다. {snapshot.ActionWhy}",
                "TabPointMove" => "목표 위치 입력과 실행 준비는 보조패널에서만 본다. 메인패널은 로봇과 미리보기 확인에 집중한다.",
                _ => "프리셋, 그리퍼, 작은 이동은 보조패널에서 다루고 메인패널은 로봇 상태 확인에 집중한다.",
            };
        }

        private static string FormatCoordSystemDisplay(string coordSystem)
        {
            return coordSystem switch
            {
                "Tool" => "툴 기준",
                "User" => "작업 기준",
                "Base" => "로봇 기준",
                _ => string.IsNullOrWhiteSpace(coordSystem) ? "--" : coordSystem,
            };
        }
    }
}
