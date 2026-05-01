// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.Math;
using KineTutor3D.Visualization;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    public sealed partial class RobotControlV3RuntimeController
    {
        private void ApplyVisualState()
        {
            var displayJointAngles = ShouldPrioritizeLiveReadbackDisplay()
                ? currentState.JointPosDeg
                : previewUsesJointPose && previewJointAnglesDeg != null
                    ? previewJointAnglesDeg
                    : currentState.JointPosDeg;

            if (displayJointAngles != null && displayJointAngles.Length >= templateDefinition.JointCount)
            {
                jointDriver?.ApplyJointAngles(displayJointAngles);
                kinematicsFacade?.SetJointAnglesDegrees(displayJointAngles);
                if (showTrail && kinematicsFacade != null)
                {
                    eeTrailRenderer?.AddPoint(kinematicsFacade.EndEffectorTransform);
                }

                if (kinematicsFacade != null)
                {
                    displacementArrow?.UpdateFromFK(kinematicsFacade.EndEffectorTransform);
                }
            }

            ApplyBaseAndToolFrameState();
            ApplyJointHighlightState();

            ghostRobotVisual?.SetVisible(false);
            predictedPathRenderer?.ClearPath();

            if (previewUsesJointPose && previewJointAnglesDeg != null && previewJointAnglesDeg.Length >= templateDefinition.JointCount)
            {
                ghostRobotVisual?.ApplyJointAngles(previewJointAnglesDeg);
                ghostRobotVisual?.SetVisible(showGhost);
                predictedPathRenderer?.RenderPath(BuildJointPreviewPath(currentState.JointPosDeg, previewJointAnglesDeg));
            }
            else if (previewTcpVisualJointAnglesDeg != null && previewTcpVisualJointAnglesDeg.Length >= templateDefinition.JointCount && previewTcpPose != null)
            {
                ghostRobotVisual?.ApplyJointAngles(previewTcpVisualJointAnglesDeg);
                ghostRobotVisual?.SetVisible(showGhost);
                predictedPathRenderer?.RenderPath(BuildJointPreviewPath(currentState.JointPosDeg, previewTcpVisualJointAnglesDeg));
            }
            else if (previewTcpPose != null && !previewUsesJointPose)
            {
                predictedPathRenderer?.RenderPath(BuildCartesianPreviewPath(currentState.TcpPose, previewTcpPose));
            }

            if (previewTcpPose != null && previewTcpPose.Length >= 3 && !previewUsesJointPose)
            {
                targetMarkerVisual?.SetMarkersVisible(true);
                if (targetMarkerVisual.TargetMarker != null)
                {
                    var pos = CoordConverter.ToUnityPosition(new Vec3D(previewTcpPose[0] / 1000.0, previewTcpPose[1] / 1000.0, previewTcpPose[2] / 1000.0));
                    targetMarkerVisual.TargetMarker.transform.position = pos;
                }
            }
            else
            {
                targetMarkerVisual?.SetMarkersVisible(false);
            }

            eeTrailRenderer?.SetVisible(showTrail);

            if (requestStageRefocus)
            {
                requestStageRefocus = false;
                ResetStageCameraIfAutomatic();
            }
        }

    }
}
