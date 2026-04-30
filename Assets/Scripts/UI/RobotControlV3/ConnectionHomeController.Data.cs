// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 연결 홈 패널 내부 참조를 분리합니다.
    /// </summary>
    public sealed partial class ConnectionHomeController
    {
        private sealed class PanelElements
        {
            public PanelElements(VisualElement root)
            {
                ConnectionRobot = root.Q<Label>("ConnectionRobot");
                ConnectionIp = root.Q<Label>("ConnectionIp");
                ConnectionStatus = root.Q<Label>("ConnectionStatus");
                BtnConnect = root.Q<Button>("BtnConnect");
                BtnDisconnect = root.Q<Button>("BtnDisconnect");
                QuickServo = root.Q<Label>("QuickServo");
                QuickMode = root.Q<Label>("QuickMode");
                QuickSync = root.Q<Label>("QuickSync");
                QuickControllerMode = root.Q<Label>("QuickControllerMode");
                QuickLiveArm = root.Q<Label>("QuickLiveArm");
                BtnModeAuto = root.Q<Button>("BtnModeAuto");
                BtnModeManual = root.Q<Button>("BtnModeManual");
                BtnMockMode = root.Q<Button>("BtnMockMode");
                BtnLiveMode = root.Q<Button>("BtnLiveMode");
                BtnArmLive = root.Q<Button>("BtnArmLive");
                BtnDisarmLive = root.Q<Button>("BtnDisarmLive");
                BtnQuickAction = root.Q<Button>("BtnQuickAction");
                ActionNow = root.Q<Label>("ActionNow");
                ActionPrimary = root.Q<Label>("ActionPrimary");
                ActionWhy = root.Q<Label>("ActionWhy");
                BtnPrimaryAction = root.Q<Button>("BtnPrimaryAction");
                BtnPresetDisconnected = root.Q<Button>("BtnPresetDisconnected");
                BtnPresetServoOff = root.Q<Button>("BtnPresetServoOff");
                BtnPresetUnsynced = root.Q<Button>("BtnPresetUnsynced");
                BtnPresetReady = root.Q<Button>("BtnPresetReady");
                BtnPresetFault = root.Q<Button>("BtnPresetFault");
                BtnPresetReconnect = root.Q<Button>("BtnPresetReconnect");
            }

            public Label ConnectionRobot { get; }
            public Label ConnectionIp { get; }
            public Label ConnectionStatus { get; }
            public Button BtnConnect { get; }
            public Button BtnDisconnect { get; }
            public Label QuickServo { get; }
            public Label QuickMode { get; }
            public Label QuickSync { get; }
            public Label QuickControllerMode { get; }
            public Label QuickLiveArm { get; }
            public Button BtnModeAuto { get; }
            public Button BtnModeManual { get; }
            public Button BtnMockMode { get; }
            public Button BtnLiveMode { get; }
            public Button BtnArmLive { get; }
            public Button BtnDisarmLive { get; }
            public Button BtnQuickAction { get; }
            public Label ActionNow { get; }
            public Label ActionPrimary { get; }
            public Label ActionWhy { get; }
            public Button BtnPrimaryAction { get; }
            public Button BtnPresetDisconnected { get; }
            public Button BtnPresetServoOff { get; }
            public Button BtnPresetUnsynced { get; }
            public Button BtnPresetReady { get; }
            public Button BtnPresetFault { get; }
            public Button BtnPresetReconnect { get; }
        }
    }
}
