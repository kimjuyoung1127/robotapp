// Folder: Editor/CliTools - UI Toolkit 런타임 기능 가용성 검증
using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// Unity 6 UI Toolkit 런타임 기능 가용성을 검증합니다.
    /// V3 티칭패드 구현에 필요한 타입, 컨트롤, 바인딩, PanelSettings 지원 여부를 확인합니다.
    /// </summary>
    [UnityCliTool(Description = "Verify UI Toolkit runtime capabilities for V3 pendant")]
    public static class UIToolkitCapabilityTool
    {
        public static object HandleCommand(JObject @params)
        {
            var results = new Dictionary<string, object>();

            results["unityVersion"] = Application.unityVersion;
            results["coreTypes"] = CheckCoreTypes();
            results["controls"] = CheckControls();
            results["layoutFeatures"] = CheckLayoutFeatures();
            results["bindingSupport"] = CheckBindingSupport();
            results["panelSettings"] = CheckPanelSettings();
            results["ussVariables"] = CheckUssVariables();
            results["worldSpace"] = CheckWorldSpaceSupport();
            results["coexistence"] = CheckUguiCoexistence();

            return results;
        }

        static object CheckCoreTypes()
        {
            var types = new Dictionary<string, string>();
            CheckType<UIDocument>(types);
            CheckType<VisualElement>(types);
            CheckType<VisualTreeAsset>(types);
            CheckType<StyleSheet>(types);
            CheckType<PanelSettings>(types);
            return types;
        }

        static object CheckControls()
        {
            var controls = new Dictionary<string, string>();
            CheckType<Label>(controls);
            CheckType<Button>(controls);
            CheckType<Slider>(controls);
            CheckType<SliderInt>(controls);
            CheckType<TextField>(controls);
            CheckType<Toggle>(controls);
            CheckType<ScrollView>(controls);
            CheckType<ListView>(controls);
            CheckType<Foldout>(controls);
            CheckType<DropdownField>(controls);
            CheckType<ProgressBar>(controls);
            CheckType<RadioButton>(controls);
            CheckType<RadioButtonGroup>(controls);
            CheckType<MinMaxSlider>(controls);
            CheckType<GroupBox>(controls);
            CheckType<TabView>(controls);
            CheckType<Tab>(controls);
            return controls;
        }

        static object CheckLayoutFeatures()
        {
            var features = new Dictionary<string, object>();

            // Flexbox 테스트: VisualElement 생성 후 flex 속성 설정
            try
            {
                var root = new VisualElement();
                root.style.flexDirection = FlexDirection.Row;
                root.style.flexGrow = 1;
                root.style.flexShrink = 0;
                root.style.alignItems = Align.Center;
                root.style.justifyContent = Justify.SpaceBetween;
                root.style.flexWrap = Wrap.Wrap;

                features["flexDirection"] = "OK";
                features["flexGrow"] = "OK";
                features["flexShrink"] = "OK";
                features["alignItems"] = "OK";
                features["justifyContent"] = "OK";
                features["flexWrap"] = "OK";
            }
            catch (Exception e)
            {
                features["flexbox"] = "FAIL: " + e.Message;
            }

            // USS 크기 단위 테스트
            try
            {
                var el = new VisualElement();
                el.style.width = new Length(100, LengthUnit.Percent);
                el.style.height = new Length(48, LengthUnit.Pixel);
                el.style.minWidth = 72;
                el.style.maxWidth = 320;
                el.style.marginLeft = 8;
                el.style.paddingTop = 4;
                el.style.borderTopWidth = 1;
                el.style.borderTopLeftRadius = 4;

                features["percentUnit"] = "OK";
                features["pixelUnit"] = "OK";
                features["minMaxSize"] = "OK";
                features["margin"] = "OK";
                features["padding"] = "OK";
                features["border"] = "OK";
                features["borderRadius"] = "OK";
            }
            catch (Exception e)
            {
                features["sizing"] = "FAIL: " + e.Message;
            }

            return features;
        }

        static object CheckBindingSupport()
        {
            var binding = new Dictionary<string, string>();

            // Runtime Data Binding (Unity.Properties 기반)
            var dataBindingType = FindType("UnityEngine.UIElements.DataBinding");
            binding["DataBinding"] = dataBindingType != null ? "OK" : "NOT_FOUND";

            // SetBinding API 확인
            var setBindingMethod = typeof(VisualElement).GetMethod("SetBinding",
                BindingFlags.Public | BindingFlags.Instance);
            binding["SetBinding"] = setBindingMethod != null ? "OK" : "NOT_FOUND";

            // INotifyBindablePropertyChanged
            var notifyType = FindType("Unity.Properties.INotifyBindablePropertyChanged");
            binding["INotifyBindablePropertyChanged"] = notifyType != null ? "OK" : "NOT_FOUND";

            // 전통적 RegisterValueChangedCallback 확인
            try
            {
                var slider = new Slider();
                bool callbackFired = false;
                slider.RegisterValueChangedCallback(evt => callbackFired = true);
                binding["RegisterValueChangedCallback"] = "OK";
            }
            catch (Exception e)
            {
                binding["RegisterValueChangedCallback"] = "FAIL: " + e.Message;
            }

            return binding;
        }

        static object CheckPanelSettings()
        {
            var panel = new Dictionary<string, object>();

            // PanelSettings ScriptableObject 생성 가능 여부
            try
            {
                var ps = ScriptableObject.CreateInstance<PanelSettings>();
                panel["create"] = "OK";

                // Sort Order 속성 확인
                var sortOrderProp = typeof(PanelSettings).GetProperty("sortingOrder",
                    BindingFlags.Public | BindingFlags.Instance);
                panel["sortingOrder"] = sortOrderProp != null ? "OK" : "NOT_FOUND";

                // Scale Mode 확인
                var scaleModeProp = typeof(PanelSettings).GetProperty("scaleMode",
                    BindingFlags.Public | BindingFlags.Instance);
                panel["scaleMode"] = scaleModeProp != null ? "OK" : "NOT_FOUND";

                // ThemeStyleSheet 확인
                var themeProp = typeof(PanelSettings).GetProperty("themeStyleSheet",
                    BindingFlags.Public | BindingFlags.Instance);
                panel["themeStyleSheet"] = themeProp != null ? "OK" : "NOT_FOUND";

                UnityEngine.Object.DestroyImmediate(ps);
            }
            catch (Exception e)
            {
                panel["create"] = "FAIL: " + e.Message;
            }

            return panel;
        }

        static object CheckUssVariables()
        {
            var uss = new Dictionary<string, string>();

            // USS 커스텀 프로퍼티 (--variable) 지원 확인
            try
            {
                var el = new VisualElement();
                var customStyleProp = typeof(VisualElement).GetProperty("customStyle",
                    BindingFlags.Public | BindingFlags.Instance);
                uss["customStyle"] = customStyleProp != null ? "OK" : "NOT_FOUND";

                // ICustomStyle.TryGetValue 확인
                var customStyleType = FindType("UnityEngine.UIElements.ICustomStyle");
                if (customStyleType != null)
                {
                    var tryGetMethods = customStyleType.GetMethods();
                    int tryGetCount = 0;
                    foreach (var m in tryGetMethods)
                        if (m.Name == "TryGetValue") tryGetCount++;
                    uss["TryGetValue_overloads"] = tryGetCount > 0
                        ? "OK (" + tryGetCount + " overloads)"
                        : "NOT_FOUND";
                }
                else
                {
                    uss["ICustomStyle"] = "NOT_FOUND";
                }
            }
            catch (Exception e)
            {
                uss["customProperties"] = "FAIL: " + e.Message;
            }

            return uss;
        }

        static object CheckWorldSpaceSupport()
        {
            var ws = new Dictionary<string, string>();

            // UIDocument.worldCamera 또는 panelSettings.renderMode 확인
            var renderModeProp = typeof(UIDocument).GetProperty("worldCamera",
                BindingFlags.Public | BindingFlags.Instance);
            ws["worldCamera"] = renderModeProp != null ? "OK" : "NOT_FOUND";

            // UIDocument 의 sortingOrder 확인
            var sortProp = typeof(UIDocument).GetProperty("sortingOrder",
                BindingFlags.Public | BindingFlags.Instance);
            ws["sortingOrder"] = sortProp != null ? "OK" : "NOT_FOUND";

            // PanelSettings 에 RenderMode 관련 필드 확인
            var psMethods = typeof(PanelSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var renderProps = new List<string>();
            foreach (var p in psMethods)
                if (p.Name.ToLower().Contains("render") || p.Name.ToLower().Contains("world"))
                    renderProps.Add(p.Name);
            ws["renderRelatedProps"] = renderProps.Count > 0
                ? string.Join(", ", renderProps)
                : "NONE_FOUND";

            return ws;
        }

        static object CheckUguiCoexistence()
        {
            var coex = new Dictionary<string, string>();

            // UnityEngine.UI 어셈블리 로드 확인
            var uguiType = FindType("UnityEngine.UI.Canvas");
            coex["uguiLoaded"] = uguiType != null ? "OK" : "NOT_FOUND";

            // EventSystem 확인
            var eventSysType = FindType("UnityEngine.EventSystems.EventSystem");
            coex["eventSystem"] = eventSysType != null ? "OK" : "NOT_FOUND";

            // PanelEventHandler (UI Toolkit - Input System 연동)
            var panelEventType = FindType("UnityEngine.UIElements.PanelEventHandler");
            coex["PanelEventHandler"] = panelEventType != null ? "OK" : "NOT_FOUND";

            return coex;
        }

        static void CheckType<T>(Dictionary<string, string> dict)
        {
            dict[typeof(T).Name] = "OK";
        }

        static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }
    }
}
