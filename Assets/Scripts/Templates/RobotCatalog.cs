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
        private static readonly Dictionary<string, RobotCatalogEntry> Entries =
            new Dictionary<string, RobotCatalogEntry>(StringComparer.Ordinal);

        static RobotCatalog()
        {
            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "2DOF_RR", "2DOF RR", 2, "RR", "Easy",
                    guidedLessonSupported: true,
                    sandboxSupported: true,
                    description: "2자유도 회전-회전 로봇. 기구학 입문에 적합합니다."),
                Template2DOF_RR.Create));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "SCARA_RV", "SCARA Robot", 3, "SCARA", "Medium",
                    guidedLessonSupported: true,
                    sandboxSupported: true,
                    description: "3자유도 SCARA 로봇. 수평 다관절 구조입니다."),
                TemplateSCARA_RV.Create));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "GENERIC_6DOF", "6축 산업로봇", 6, "Articulated", "Medium",
                    description: "6자유도 범용 산업로봇. 다양한 작업에 활용됩니다.")));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "FANUC_CRX10", "Fanuc CRX-10iA/L", 6, "Collaborative", "Hard",
                    description: "6자유도 협동로봇. Fanuc CRX 시리즈입니다.")));

            Register(new RobotCatalogEntry(
                new RobotMetadataInfo(
                    "IGUS_REBEL", "igus REBEL", 6, "Educational", "Medium",
                    description: "6자유도 교육용 로봇. igus REBEL 시리즈입니다.")));
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
