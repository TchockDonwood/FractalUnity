using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class FractalAppBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        if (FindAnyObjectByType<FractalController>() != null) return;
        if (FindAnyObjectByType<FractalAppBootstrap>() != null) return;
        new GameObject("FractalApp").AddComponent<FractalAppBootstrap>();
    }

    void Awake() => BuildApp();

    private void BuildApp()
    {
        SetupCamera();
        var renderer = SetupFractal();
        var canvas = SetupCanvas();
        var panel = SetupPanel(canvas);

        var iterSlider = CreateSliderRow(panel, 0, 6, 3, true, out var iterLbl);
        var sidesSlider = CreateSliderRow(panel, 3, 30, 3, true, out var sidesLbl);
        var angleSlider = CreateSliderRow(panel, 0, 180, 60, true, out var angleLbl);
        var lenSlider = CreateSliderRow(panel, 100, 800, 400, false, out var lenLbl);
        var thickSlider = CreateSliderRow(panel, 0.5f, 10f, 2f, false, out var thickLbl);

        CreateButtonRow(panel, new[] { "Сброс", "Центр", "Анимация" },
                        out var buttons, out var buttonLabels);

        var controller = gameObject.AddComponent<FractalController>();
        controller.curveRenderer = renderer;
        controller.iterationsSlider = iterSlider;
        controller.sidesSlider = sidesSlider;
        controller.angleSlider = angleSlider;
        controller.lengthSlider = lenSlider;
        controller.thicknessSlider = thickSlider;
        controller.iterationsLabel = iterLbl;
        controller.sidesLabel = sidesLbl;
        controller.angleLabel = angleLbl;
        controller.lengthLabel = lenLbl;
        controller.thicknessLabel = thickLbl;
        controller.resetButton = buttons[0];
        controller.centerButton = buttons[1];

        var animator = gameObject.AddComponent<FractalAnimator>();
        animator.controller = controller;
        animator.animationButton = buttons[2];
        animator.animationButtonLabel = buttonLabels[2];
    }

    private void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
    }

    private KochCurveRenderer SetupFractal()
    {
        var go = new GameObject("KochCurve");
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.sortingOrder = 1;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        return go.AddComponent<KochCurveRenderer>();
    }

    private Canvas SetupCanvas()
    {
        var go = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }
        return canvas;
    }

    private RectTransform SetupPanel(Canvas canvas)
    {
        var go = new GameObject("Panel");
        go.transform.SetParent(canvas.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = new Vector2(-20, 0);
        rt.sizeDelta = new Vector2(340, 540);

        go.AddComponent<Image>().color = new Color(0.15f, 0.17f, 0.2f, 0.95f);

        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(15, 15, 15, 15);
        layout.spacing = 10;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        return rt;
    }

    private Slider CreateSliderRow(RectTransform parent, float min, float max, float def,
                                   bool whole, out TMP_Text valueLabel)
    {
        var row = new GameObject("Row");
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 70);

        var vlg = row.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(row.transform, false);
        lblGO.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 24);
        valueLabel = lblGO.AddComponent<TextMeshProUGUI>();
        valueLabel.fontSize = 18;
        valueLabel.color = Color.white;
        valueLabel.alignment = TextAlignmentOptions.Left;

        var sliderGO = new GameObject("Slider");
        sliderGO.transform.SetParent(row.transform, false);
        sliderGO.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 30);
        var slider = sliderGO.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = def;
        slider.wholeNumbers = whole;

        var bg = new GameObject("Background");
        bg.transform.SetParent(sliderGO.transform, false);
        var bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0.25f);
        bgRT.anchorMax = new Vector2(1, 0.75f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        var faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0, 0.25f);
        faRT.anchorMax = new Vector2(1, 0.75f);
        faRT.offsetMin = Vector2.zero;
        faRT.offsetMax = Vector2.zero;

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillArea.transform, false);
        var fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        fillGO.AddComponent<Image>().color = new Color(0.3f, 0.7f, 0.9f);

        var hsArea = new GameObject("Handle Slide Area");
        hsArea.transform.SetParent(sliderGO.transform, false);
        var hsRT = hsArea.AddComponent<RectTransform>();
        hsRT.anchorMin = new Vector2(0, 0);
        hsRT.anchorMax = new Vector2(1, 1);
        hsRT.offsetMin = new Vector2(10, 0);
        hsRT.offsetMax = new Vector2(-10, 0);

        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(hsArea.transform, false);
        var handleRT = handleGO.AddComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0, 0);
        handleRT.anchorMax = new Vector2(0, 1);
        handleRT.pivot = new Vector2(0.5f, 0.5f);
        handleRT.sizeDelta = new Vector2(20, 0);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private void CreateButtonRow(RectTransform parent, string[] labels,
                                 out Button[] buttons, out TMP_Text[] labelTexts)
    {
        var row = new GameObject("ButtonRow");
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 44);

        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 6;
        hlg.padding = new RectOffset(0, 0, 0, 0);
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        buttons = new Button[labels.Length];
        labelTexts = new TMP_Text[labels.Length];
        for (int i = 0; i < labels.Length; i++)
            buttons[i] = CreateButton(row.transform, labels[i], out labelTexts[i]);
    }

    private Button CreateButton(Transform parent, string label, out TMP_Text labelText)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();

        var img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.5f, 0.7f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var lblGO = new GameObject("Text");
        lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero;
        lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = new Vector2(2, 0);
        lblRT.offsetMax = new Vector2(-2, 0);

        labelText = lblGO.AddComponent<TextMeshProUGUI>();
        labelText.fontSize = 15;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.text = label;
        return btn;
    }
}