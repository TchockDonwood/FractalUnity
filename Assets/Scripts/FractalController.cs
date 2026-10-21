using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class FractalController : MonoBehaviour
{
    [Header("Ссылки")]
    public KochCurveRenderer curveRenderer;
    public Slider iterationsSlider;
    public Slider sidesSlider;
    public Slider angleSlider;
    public Slider lengthSlider;
    public Slider thicknessSlider;
    public TMP_Text iterationsLabel;
    public TMP_Text sidesLabel;
    public TMP_Text angleLabel;
    public TMP_Text lengthLabel;
    public TMP_Text thicknessLabel;
    public Button resetButton;
    public Button centerButton;

    [Header("Значения по умолчанию")]
    public int defaultIterations = 0;
    public int defaultSides = 3;
    public float defaultAngle = 60f;
    public float defaultLength = 400f;
    public float defaultThickness = 2f;

    [Header("Масштаб/зум")]
    public float pixelsToUnits = 0.01f;
    public float zoomSensitivity = 0.15f;
    public float minViewScale = 0.05f;
    public float maxViewScale = 50f;

    public int MinIterations => (int)iterationsSlider.minValue;
    public int MaxIterations => (int)iterationsSlider.maxValue;
    public int CurrentIterations => (int)iterationsSlider.value;

    private List<Vector2> _basePoints;
    private readonly List<Vector2> _transformed = new List<Vector2>();
    private float _viewScale = 1f;
    private Vector2 _viewOffset = Vector2.zero;

    private bool _isDragging;
    private Vector3 _lastMousePos;

    void Start()
    {
        iterationsSlider.wholeNumbers = true;
        sidesSlider.wholeNumbers = true;
        angleSlider.wholeNumbers = true;

        iterationsSlider.onValueChanged.AddListener(_ => Rebuild());
        sidesSlider.onValueChanged.AddListener(_ => Rebuild());
        angleSlider.onValueChanged.AddListener(OnAngleChanged);
        lengthSlider.onValueChanged.AddListener(_ => Rebuild());
        thicknessSlider.onValueChanged.AddListener(_ => Rebuild());
        resetButton.onClick.AddListener(ResetToDefaults);
        if (centerButton != null) centerButton.onClick.AddListener(ResetView);

        ResetToDefaults();
    }

    void Update()
    {
        HandleZoom();
        HandleDrag();
    }

    private void HandleZoom()
    {
        float scroll = MouseHelper.Scroll;
        if (Mathf.Abs(scroll) < 0.001f) return;

        Vector2 mouseWorld = GetMouseWorld();
        float oldScale = _viewScale;
        _viewScale *= (1f + scroll * zoomSensitivity);
        _viewScale = Mathf.Clamp(_viewScale, minViewScale, maxViewScale);
        float ratio = _viewScale / oldScale;
        // фиксируем точку под курсором
        _viewOffset = mouseWorld - (mouseWorld - _viewOffset) * ratio;
        ApplyTransform();
    }

    private void HandleDrag()
    {
        if (MouseHelper.LeftPressed && !IsPointerOverUI())
        {
            _isDragging = true;
            _lastMousePos = MouseHelper.Position;
        }
        if (MouseHelper.LeftReleased) _isDragging = false;

        if (_isDragging && MouseHelper.LeftHeld)
        {
            Vector3 cur = MouseHelper.Position;
            Vector3 delta = cur - _lastMousePos;
            var cam = Camera.main;
            if (cam != null)
            {
                float worldPerPixel = cam.orthographicSize * 2f / Screen.height;
                _viewOffset += new Vector2(delta.x, delta.y) * worldPerPixel;
                ApplyTransform();
            }
            _lastMousePos = cur;
        }
    }

    private Vector2 GetMouseWorld()
    {
        var cam = Camera.main;
        if (cam == null) return Vector2.zero;
        Vector3 screen = MouseHelper.Position;
        screen.z = -cam.transform.position.z;
        return cam.ScreenToWorldPoint(screen);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        var data = new PointerEventData(EventSystem.current) { position = MouseHelper.Position };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        return results.Count > 0;
    }

    public void ResetToDefaults()
    {
        iterationsSlider.SetValueWithoutNotify(defaultIterations);
        sidesSlider.SetValueWithoutNotify(defaultSides);
        angleSlider.SetValueWithoutNotify(defaultAngle);
        lengthSlider.SetValueWithoutNotify(defaultLength);
        thicknessSlider.SetValueWithoutNotify(defaultThickness);
        ResetView();
        Rebuild();
    }

    public void ResetView()
    {
        _viewScale = 1f;
        _viewOffset = Vector2.zero;
        ApplyTransform();
    }

    public void SetIterations(int value)
    {
        iterationsSlider.SetValueWithoutNotify(value);
        Rebuild();
    }

    private static float SnapAngle(float angle)
    {
        if (angle > 60f && angle < 120f)
            return angle < 90f ? 60f : 120f;
        return angle;
    }

    private void OnAngleChanged(float value)
    {
        float snapped = SnapAngle(value);
        if (!Mathf.Approximately(snapped, value))
            angleSlider.SetValueWithoutNotify(snapped);   // без рекурсии и без события
        Rebuild();
    }

    private void Rebuild()
    {
        int it = (int)iterationsSlider.value;
        int sides = (int)sidesSlider.value;
        float angle = SnapAngle(angleSlider.value);
        float len = lengthSlider.value * pixelsToUnits;

        if (iterationsLabel) iterationsLabel.text = $"Итерации: {it}";
        if (sidesLabel) sidesLabel.text = $"Сторон: {sides}";
        if (angleLabel) angleLabel.text = $"Угол: {angle:F0}°";
        if (lengthLabel) lengthLabel.text = $"Длина: {lengthSlider.value:F0}";
        if (thicknessLabel) thicknessLabel.text = $"Толщина: {thicknessSlider.value:F1}";

        _basePoints = KochCurveGenerator.GenerateSnowflake(sides, len, it, angle);
        ApplyTransform();
    }

    private void ApplyTransform()
    {
        if (_basePoints == null || _basePoints.Count < 2)
        {
            curveRenderer.Render(new List<Vector2>(), 0f);
            return;
        }

        _transformed.Clear();
        int n = _basePoints.Count;
        for (int i = 0; i < n; i++)
            _transformed.Add(_basePoints[i] * _viewScale + _viewOffset);

        curveRenderer.Render(_transformed, thicknessSlider.value * pixelsToUnits);
    }
}