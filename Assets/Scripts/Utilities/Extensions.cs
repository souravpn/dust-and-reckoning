using UnityEngine;

/// <summary>
/// General-purpose C# and Unity extension methods.
/// </summary>
public static class Extensions
{
    // ── Transform ─────────────────────────────────────────────────────

    /// <summary>Distance to another Transform (XZ plane only — ignores height).</summary>
    public static float FlatDistanceTo(this Transform a, Transform b)
    {
        var diff = a.position - b.position;
        diff.y = 0f;
        return diff.magnitude;
    }

    /// <summary>Looks at target on the Y axis only (no tilting).</summary>
    public static void LookAtFlat(this Transform t, Vector3 target)
    {
        var dir = target - t.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            t.rotation = Quaternion.LookRotation(dir);
    }

    // ── Float ─────────────────────────────────────────────────────────

    /// <summary>Remap a value from one range to another.</summary>
    public static float Remap(this float value, float fromMin, float fromMax,
                                                float toMin,   float toMax)
        => Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));

    public static bool Approximately(this float a, float b, float tolerance = 0.001f)
        => Mathf.Abs(a - b) <= tolerance;

    // ── String ────────────────────────────────────────────────────────

    public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

    // ── MonoBehaviour ─────────────────────────────────────────────────

    /// <summary>Gets or adds a component — safe replacement for GetComponent + AddComponent pattern.</summary>
    public static T GetOrAdd<T>(this GameObject go) where T : Component
        => go.TryGetComponent<T>(out var c) ? c : go.AddComponent<T>();
}
