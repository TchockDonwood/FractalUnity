using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public static class MouseHelper
{
    public static Vector2 Position
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var m = Mouse.current;
            return m != null ? m.position.ReadValue() : Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }
    }

    public static float Scroll
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var m = Mouse.current;
            if (m == null) return 0f;
            float raw = m.scroll.ReadValue().y;
            if (Mathf.Abs(raw) > 10f) raw /= 120f; // разные ОС дают ±120 за щелчок
            return raw;
#else
            return Input.mouseScrollDelta.y;
#endif
        }
    }

    public static bool LeftPressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var m = Mouse.current; return m != null && m.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }
    }

    public static bool LeftReleased
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var m = Mouse.current; return m != null && m.leftButton.wasReleasedThisFrame;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }
    }

    public static bool LeftHeld
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var m = Mouse.current; return m != null && m.leftButton.isPressed;
#else
            return Input.GetMouseButton(0);
#endif
        }
    }
}