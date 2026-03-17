// Folder: UI - 3D 로봇 쇼룸 뷰포트 매니저.
using System;
using System.Collections.Generic;
using KineTutor3D.Templates;
using KineTutor3D.Visualization;
using UnityEngine;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 3D 로봇 쇼룸을 관리합니다.
    /// hero + 좌우 preview 2개를 기준으로 페이지/선택/정리를 담당합니다.
    /// </summary>
    public class RobotShowroomManager : MonoBehaviour
    {
        /// <summary>로봇 선택 시 발생합니다.</summary>
        public event Action<string> OnRobotSelected;

        /// <summary>CTA 버튼 클릭 시 발생합니다 (robotId, ctaType).</summary>
        public event Action<string, string> OnCtaClicked;

        /// <summary>페이지가 바뀔 때 발생합니다. (currentPage, totalPages)</summary>
        public event Action<int, int> OnPageChanged;

        private RobotShowroomContext context;
        private Transform podContainer;
        private readonly Dictionary<string, RobotPreviewPod> activePods = new Dictionary<string, RobotPreviewPod>();
        private readonly List<string> currentVisibleRobotIds = new List<string>();
        private string currentHeroId = string.Empty;
        private bool configured;

        public void Configure(RobotShowroomContext ctx)
        {
            context = ctx;
            configured = true;

            DisposeAllPods();
            EnsurePodContainer();

            string heroId = !string.IsNullOrEmpty(ctx.HeroRobotId)
                ? ctx.HeroRobotId
                : ResolveDefaultHeroId(ctx);

            if (!string.IsNullOrEmpty(heroId))
            {
                SelectRobot(heroId);
            }
        }

        public void SelectRobot(string robotId)
        {
            if (!configured || string.IsNullOrEmpty(robotId))
            {
                return;
            }

            if (currentHeroId == robotId && activePods.ContainsKey(robotId))
            {
                return;
            }

            currentHeroId = robotId;
            var visibleRobotIds = BuildVisibleRobotIds(robotId);
            currentVisibleRobotIds.Clear();
            currentVisibleRobotIds.AddRange(visibleRobotIds);
            SyncPods(visibleRobotIds);

            foreach (var kvp in activePods)
            {
                kvp.Value.SetSelected(kvp.Key == currentHeroId);
            }

            ArrangePods(visibleRobotIds);
            OnRobotSelected?.Invoke(robotId);
            OnPageChanged?.Invoke(GetCurrentPage(), GetTotalPages());
        }

        public void NextPage()
        {
            if (!configured || !context.EnablePaging || context.RobotIds.Length == 0)
            {
                return;
            }

            int pageSize = Mathf.Max(1, context.MaxVisiblePods);
            int nextPageStartIndex = Mathf.Min(GetCurrentPage() * pageSize, context.RobotIds.Length - 1);
            int nextHeroIndex = ResolveHeroIndexForPage(nextPageStartIndex, pageSize);
            SelectRobot(context.RobotIds[nextHeroIndex]);
        }

        public void PreviousPage()
        {
            if (!configured || !context.EnablePaging || context.RobotIds.Length == 0)
            {
                return;
            }

            int pageSize = Mathf.Max(1, context.MaxVisiblePods);
            int previousPageStartIndex = Mathf.Max(0, (GetCurrentPage() - 2) * pageSize);
            int previousHeroIndex = ResolveHeroIndexForPage(previousPageStartIndex, pageSize);
            SelectRobot(context.RobotIds[previousHeroIndex]);
        }

        public void NotifyCtaClicked(string robotId, string ctaType)
        {
            OnCtaClicked?.Invoke(robotId, ctaType);
        }

        public string GetCurrentHeroId()
        {
            return currentHeroId;
        }

        public string[] GetVisibleRobotIds()
        {
            return currentVisibleRobotIds.ToArray();
        }

        public bool TryGetPod(string robotId, out RobotPreviewPod pod)
        {
            if (string.IsNullOrWhiteSpace(robotId))
            {
                pod = null;
                return false;
            }

            return activePods.TryGetValue(robotId, out pod) && pod != null;
        }

        private void OnDestroy()
        {
            DisposeAllPods();
        }

        private void EnsurePodContainer()
        {
            if (podContainer != null)
            {
                return;
            }

            var containerGo = new GameObject("ShowroomPodContainer");
            containerGo.transform.SetParent(transform, false);
            podContainer = containerGo.transform;
        }

        private RobotPreviewPod CreatePodForRobot(string robotId)
        {
            if (!RobotCatalog.TryGet(robotId, out var entry))
            {
                return null;
            }

            EnsurePodContainer();
            return RobotPreviewFactory.CreatePod(podContainer, entry, context.ShowLabels);
        }

        private List<string> BuildVisibleRobotIds(string heroId)
        {
            var visible = new List<string>();
            if (context.RobotIds == null || context.RobotIds.Length == 0)
            {
                return visible;
            }

            int heroIndex = Array.IndexOf(context.RobotIds, heroId);
            if (heroIndex < 0)
            {
                heroIndex = 0;
            }

            int pageSize = Mathf.Max(1, context.MaxVisiblePods);
            int maxStartIndex = Mathf.Max(0, context.RobotIds.Length - pageSize);
            int startIndex = Mathf.Clamp((heroIndex / pageSize) * pageSize, 0, maxStartIndex);
            int endIndex = Mathf.Min(context.RobotIds.Length, startIndex + pageSize);
            for (int i = startIndex; i < endIndex; i++)
            {
                visible.Add(context.RobotIds[i]);
            }

            return visible;
        }

        private void SyncPods(List<string> visibleRobotIds)
        {
            var visibleSet = new HashSet<string>(visibleRobotIds);
            var staleRobotIds = new List<string>();
            foreach (var kvp in activePods)
            {
                if (!visibleSet.Contains(kvp.Key))
                {
                    staleRobotIds.Add(kvp.Key);
                }
            }

            foreach (var staleRobotId in staleRobotIds)
            {
                if (activePods.TryGetValue(staleRobotId, out var stalePod))
                {
                    RobotPreviewFactory.DisposePod(stalePod);
                    activePods.Remove(staleRobotId);
                }
            }

            foreach (var robotId in visibleRobotIds)
            {
                if (activePods.ContainsKey(robotId))
                {
                    continue;
                }

                var pod = CreatePodForRobot(robotId);
                if (pod != null)
                {
                    activePods[robotId] = pod;
                }
            }
        }

        private void ArrangePods(List<string> orderedRobotIds)
        {
            if (orderedRobotIds == null || orderedRobotIds.Count == 0)
            {
                return;
            }

            int heroIndex = orderedRobotIds.IndexOf(currentHeroId);
            if (heroIndex < 0)
            {
                heroIndex = Mathf.Clamp(orderedRobotIds.Count / 2, 0, orderedRobotIds.Count - 1);
            }

            float spacing = context.PodSpacing;
            for (int i = 0; i < orderedRobotIds.Count; i++)
            {
                if (!activePods.TryGetValue(orderedRobotIds[i], out var pod) || pod == null)
                {
                    continue;
                }

                float xOffset = (i - heroIndex) * spacing;
                pod.transform.localPosition = new Vector3(xOffset, 0f, 0f);
            }
        }

        private static string ResolveDefaultHeroId(RobotShowroomContext ctx)
        {
            if (ctx.RobotIds == null || ctx.RobotIds.Length == 0)
            {
                return string.Empty;
            }

            int pageSize = Mathf.Max(1, ctx.MaxVisiblePods);
            int defaultIndex = ResolveHeroIndexForPage(0, pageSize, ctx.RobotIds.Length);
            return ctx.RobotIds[defaultIndex];
        }

        private int ResolveHeroIndexForPage(int startIndex, int pageSize)
        {
            return ResolveHeroIndexForPage(startIndex, pageSize, context.RobotIds.Length);
        }

        private static int ResolveHeroIndexForPage(int startIndex, int pageSize, int totalCount)
        {
            int safeStartIndex = Mathf.Max(0, startIndex);
            int visibleCount = Mathf.Min(pageSize, Mathf.Max(0, totalCount - safeStartIndex));
            if (visibleCount <= 0)
            {
                return 0;
            }

            int heroOffset = visibleCount >= 3 ? visibleCount / 2 : 0;
            return safeStartIndex + heroOffset;
        }

        private int GetCurrentPage()
        {
            if (context.RobotIds == null || context.RobotIds.Length == 0)
            {
                return 1;
            }

            int heroIndex = Array.IndexOf(context.RobotIds, currentHeroId);
            if (heroIndex < 0)
            {
                heroIndex = 0;
            }

            int pageSize = Mathf.Max(1, context.MaxVisiblePods);
            return (heroIndex / pageSize) + 1;
        }

        private int GetTotalPages()
        {
            if (context.RobotIds == null || context.RobotIds.Length == 0)
            {
                return 1;
            }

            int pageSize = Mathf.Max(1, context.MaxVisiblePods);
            return Mathf.CeilToInt((float)context.RobotIds.Length / pageSize);
        }

        private void DisposeAllPods()
        {
            foreach (var kvp in activePods)
            {
                if (kvp.Value != null)
                {
                    RobotPreviewFactory.DisposePod(kvp.Value);
                }
            }

            activePods.Clear();
            currentVisibleRobotIds.Clear();
            currentHeroId = string.Empty;
        }
    }
}
