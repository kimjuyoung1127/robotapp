// Folder: Templates - Robot configuration templates; no UnityEngine references.
using System;
using System.Collections.Generic;
using KineTutor3D.Types;

namespace KineTutor3D.Templates
{
    /// <summary>
    /// 사용 가능한 로봇 카탈로그를 관리합니다.
    /// </summary>
    public static class RobotCatalog
    {
        private static readonly string[] RobotLibraryOrder =
        {
            "2DOF_RR",
            "SCARA_RV",
            "FAIRINO_FR5_TEMPLATE",
            "FAIRINO_FR5",
            "UR5e",
            "DOOSAN_M1013",
            "MECA500",
            "FANUC_CRX10",
            "IGUS_REBEL"
        };

        private static readonly Dictionary<string, RobotCatalogEntry> Entries =
            new Dictionary<string, RobotCatalogEntry>(StringComparer.Ordinal);

        static RobotCatalog()
        {
            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "2DOF_RR", "2DOF RR", 2, "RR", "Easy",
                    convention: "DH-Standard",
                    guidedLessonSupported: true,
                    sandboxSupported: true,
                    description: "2자유도 회전-회전 로봇. 기구학 입문에 적합합니다.",
                    supportedLessons: new[] { "L0", "L1", "L2", "L3", "S1", "S2", "S3" },
                    inputModes: new[] { "slider", "numeric" },
                    visualizationLevel: "Lesson",
                    importSource: "Assets/Scenes/Main.unity"),
                Template2DOF_RR.Create));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "SCARA_RV", "SCARA Robot", 4, "SCARA", "Medium",
                    convention: "DH",
                    guidedLessonSupported: true,
                    sandboxSupported: true,
                    instructorRecommended: true,
                    description: "4자유도 SCARA 로봇. 수평 다관절 구조의 첫 확장 모델입니다.",
                    supportedLessons: new[] { "SCARA Intro", "Sandbox" },
                    inputModes: new[] { "slider", "numeric" },
                    visualizationLevel: "DonorMesh",
                    zeroPoseDeg: new[] { 0d, 0d, 0d, 0d },
                    homePoseDeg: new[] { 0d, 25d, 0d, 0d },
                    demoPoseDeg: new[] { 35d, -20d, 40d, 10d },
                    importSource: "Assets/Runtime/Resources/Robots/ScaraRobot.prefab"),
                TemplateSCARA_RV.Create));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "FAIRINO_FR5", "FAIRINO FR5", 6, "Articulated", "Medium",
                    convention: "DH-Standard",
                    guidedLessonSupported: false,
                    sandboxSupported: true,
                    description: "6자유도 FAIRINO FR5 산업로봇. 실기 연동을 지원합니다.",
                    supportedLessons: new[] { "Sandbox", "RobotControl" },
                    inputModes: new[] { "slider", "numeric", "live" },
                    visualizationLevel: "DonorMesh",
                    zeroPoseDeg: new[] { 0d, 0d, 0d, 0d, 0d, 0d },
                    homePoseDeg: new[] { 0d, -90d, 0d, -90d, 0d, 0d },
                    demoPoseDeg: new[] { 30d, -45d, 60d, -30d, 45d, 0d },
                    importSource: "Assets/Runtime/Robots/FAIRINO_FR5/fairino5_v6.urdf"),
                TemplateFAIRINO_FR5.Create));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "FAIRINO_FR5_TEMPLATE", "템플릿 FR5", 6, "Articulated", "Medium",
                    convention: "DH-Standard",
                    guidedLessonSupported: false,
                    sandboxSupported: false,
                    description: "RobotControl 템플릿 구조를 보여주는 선택 전용 항목입니다.",
                    supportedLessons: Array.Empty<string>(),
                    inputModes: new[] { "select" },
                    visualizationLevel: "DonorMesh",
                    previewSourceRobotId: "FAIRINO_FR5",
                    zeroPoseDeg: new[] { 0d, 0d, 0d, 0d, 0d, 0d },
                    homePoseDeg: new[] { 0d, -90d, 0d, -90d, 0d, 0d },
                    demoPoseDeg: new[] { 30d, -45d, 60d, -30d, 45d, 0d },
                    importSource: "Assets/Runtime/Robots/FAIRINO_FR5/fairino5_v6.urdf"),
                libraryInteractionMode: LibraryInteractionMode.SelectOnly));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "UR5e", "Universal Robots UR5e", 6, "Collaborative", "Medium",
                    convention: "DH-Standard",
                    guidedLessonSupported: false,
                    sandboxSupported: true,
                    description: "6자유도 Universal Robots UR5e 협동로봇. 실기 연동을 지원합니다.",
                    supportedLessons: new[] { "Sandbox", "RobotControl" },
                    inputModes: new[] { "slider", "numeric", "live" },
                    visualizationLevel: "DonorMesh",
                    zeroPoseDeg: new[] { 0d, 0d, 0d, 0d, 0d, 0d },
                    homePoseDeg: new[] { 0d, -90d, 0d, -90d, 0d, 0d },
                    demoPoseDeg: new[] { 30d, -45d, -90d, -45d, 90d, 0d },
                    importSource: "Assets/Runtime/Resources/Robots/UR5e/"),
                TemplateUR5e.Create));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "DOOSAN_M1013", "Doosan M1013", 6, "Collaborative", "Medium",
                    convention: "DH-Standard",
                    guidedLessonSupported: false,
                    sandboxSupported: true,
                    description: "6자유도 Doosan Robotics M1013 협동로봇. 실기 연동을 지원합니다.",
                    supportedLessons: new[] { "Sandbox", "RobotControl" },
                    inputModes: new[] { "slider", "numeric", "live" },
                    visualizationLevel: "DonorMesh",
                    zeroPoseDeg: new[] { 0d, 0d, 0d, 0d, 0d, 0d },
                    homePoseDeg: new[] { 0d, 0d, 0d, 0d, 0d, 0d },
                    demoPoseDeg: new[] { 30d, -30d, 90d, 0d, 60d, 0d },
                    importSource: "Assets/Runtime/Resources/Robots/DoosanM1013/"),
                TemplateDoosanM1013.Create));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "MECA500", "Mecademic Meca500", 6, "Collaborative", "Easy",
                    convention: "DH-Standard",
                    guidedLessonSupported: false,
                    sandboxSupported: true,
                    description: "6자유도 Mecademic Meca500 협동로봇. 세계에서 가장 작은 6축 산업용 로봇입니다.",
                    supportedLessons: new[] { "Sandbox", "RobotControl" },
                    inputModes: new[] { "slider", "numeric", "live" },
                    visualizationLevel: "DonorMesh",
                    zeroPoseDeg: new[] { 0d, 0d, 0d, 0d, 0d, 0d },
                    homePoseDeg: new[] { 0d, 0d, 0d, 0d, 0d, 0d },
                    demoPoseDeg: new[] { 30d, -30d, 50d, 0d, -60d, 0d },
                    importSource: "Assets/Runtime/Robots/MECA500/meca500.urdf"),
                TemplateMeca500.Create));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "FANUC_CRX10", "Fanuc CRX-10iA/L", 6, "Collaborative", "Hard",
                    convention: "URDF-ready",
                    description: "6자유도 협동로봇. 실제 donor preview를 사용하는 Fanuc CRX 시리즈입니다.",
                    supportedLessons: new[] { "Demo" },
                    inputModes: new[] { "demo" },
                    visualizationLevel: "DonorMesh",
                    importSource: "Assets/Runtime/Resources/Robots/FanucCRX-10iA_L.prefab")));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "IGUS_REBEL", "igus REBEL", 6, "Educational", "Medium",
                    convention: "URDF-ready",
                    description: "6자유도 교육용 로봇. 실제 donor preview를 사용하는 igus REBEL 시리즈입니다.",
                    supportedLessons: new[] { "Demo" },
                    inputModes: new[] { "demo" },
                    visualizationLevel: "DonorMesh",
                    importSource: "Assets/Runtime/Resources/Robots/igusRebel.prefab")));
        }

        /// <summary>
        /// 카탈로그에 로봇을 등록합니다.
        /// </summary>
        public static void Register(RobotCatalogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            Entries[entry.Metadata.RobotId] = entry;
        }

        /// <summary>
        /// 로봇 ID로 카탈로그 항목을 조회합니다.
        /// </summary>
        public static bool TryGet(string robotId, out RobotCatalogEntry entry)
        {
            if (string.IsNullOrWhiteSpace(robotId))
            {
                entry = null;
                return false;
            }

            return Entries.TryGetValue(robotId, out entry);
        }

        /// <summary>
        /// 등록된 모든 로봇 ID를 반환합니다.
        /// </summary>
        public static string[] GetAllRobotIds()
        {
            var ids = new string[Entries.Count];
            Entries.Keys.CopyTo(ids, 0);
            return ids;
        }

        /// <summary>
        /// Robot Library 화면에서 사용되는 순서 고정 카탈로그 항목을 반환합니다.
        /// </summary>
        public static RobotCatalogEntry[] GetRobotLibraryEntries()
        {
            var ordered = new List<RobotCatalogEntry>(Entries.Count);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < RobotLibraryOrder.Length; i++)
            {
                if (!Entries.TryGetValue(RobotLibraryOrder[i], out var entry))
                {
                    continue;
                }

                ordered.Add(entry);
                seen.Add(entry.Metadata.RobotId);
            }

            foreach (var entry in Entries.Values)
            {
                if (seen.Add(entry.Metadata.RobotId))
                {
                    ordered.Add(entry);
                }
            }

            return ordered.ToArray();
        }

        /// <summary>
        /// Robot Library 화면에서 사용되는 순서 고정 로봇 ID를 반환합니다.
        /// </summary>
        public static string[] GetRobotLibraryIds()
        {
            var entries = GetRobotLibraryEntries();
            var ids = new string[entries.Length];
            for (var i = 0; i < entries.Length; i++)
            {
                ids[i] = entries[i].Metadata.RobotId;
            }

            return ids;
        }

        /// <summary>
        /// 템플릿 팩토리가 있는 로봇 ID만 반환합니다.
        /// </summary>
        public static string[] GetAvailableRobotIds()
        {
            var result = new List<string>();
            foreach (var kvp in Entries)
            {
                if (kvp.Value.TemplateFactory != null)
                {
                    result.Add(kvp.Key);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 등록된 모든 카탈로그 항목을 반환합니다.
        /// </summary>
        public static RobotCatalogEntry[] GetAll()
        {
            var entries = new RobotCatalogEntry[Entries.Count];
            Entries.Values.CopyTo(entries, 0);
            return entries;
        }

        /// <summary>
        /// 로봇 ID로 템플릿을 생성합니다. 팩토리가 없으면 null을 반환합니다.
        /// </summary>
        public static RobotTemplate CreateTemplate(string robotId)
        {
            if (!TryGet(robotId, out var entry) || entry.TemplateFactory == null)
            {
                return null;
            }

            return entry.TemplateFactory();
        }

        /// <summary>
        /// 로봇 ID에 대한 템플릿 팩토리가 존재하는지 확인합니다.
        /// </summary>
        public static bool HasTemplate(string robotId)
        {
            return TryGet(robotId, out var entry) && entry.TemplateFactory != null;
        }
    }
}
