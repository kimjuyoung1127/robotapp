// Folder: Types - Domain value types; no UnityEngine references.
using System;

namespace KineTutor3D.Types
{
    /// <summary>
    /// 카탈로그에 등록된 로봇 항목입니다. 메타데이터와 선택적 템플릿 팩토리를 포함합니다.
    /// </summary>
    public sealed class RobotCatalogEntry
    {
        /// <summary>
        /// 로봇 메타데이터입니다.
        /// </summary>
        public RobotMetadataInfo Metadata { get; }

        /// <summary>
        /// 로봇 템플릿 생성 팩토리입니다. 데모퍼스트 로봇은 null일 수 있습니다.
        /// </summary>
        public Func<RobotTemplate> TemplateFactory { get; }

        /// <summary>
        /// Robot Library에서 허용되는 상호작용 수준입니다.
        /// </summary>
        public LibraryInteractionMode LibraryInteractionMode { get; }

        public RobotCatalogEntry(
            RobotMetadataInfo metadata,
            Func<RobotTemplate> templateFactory = null,
            LibraryInteractionMode libraryInteractionMode = LibraryInteractionMode.Normal)
        {
            Metadata = metadata;
            TemplateFactory = templateFactory;
            LibraryInteractionMode = libraryInteractionMode;
        }
    }
}
