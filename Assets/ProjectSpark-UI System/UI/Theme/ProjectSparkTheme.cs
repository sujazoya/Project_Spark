
using UnityEngine;


[CreateAssetMenu(
    menuName =
    "ProjectSpark/UI/Theme")]
public sealed class ProjectSparkTheme :
    ScriptableObject
{
    public Color BackgroundPrimary;
    public Color BackgroundSecondary;

    public Color Surface;
    public Color SurfaceElevated;

    public Color Accent;
    public Color AccentMuted;

    public Color TextPrimary;
    public Color TextSecondary;
    public Color TextMuted;

    public Color Success;
    public Color Warning;
    public Color Error;
}