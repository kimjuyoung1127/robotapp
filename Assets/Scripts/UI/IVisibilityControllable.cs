// Folder: UI - HUD/view components only; no kinematics logic.
namespace KineTutor3D.UI
{
    /// <summary>
    /// 패널/컨트롤러의 가시성 제어 계약을 정의합니다.
    /// coordinator가 패널 목록을 일괄 제어할 때 사용합니다.
    /// </summary>
    public interface IVisibilityControllable
    {
        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        void SetVisible(bool visible);
    }
}
