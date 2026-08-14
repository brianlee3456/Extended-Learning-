using UnityEngine;

/// <summary>
/// Change the color a meterial using a color, or Hue
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class SetMaterialColor : MonoBehaviour
{
    [Tooltip("The material that's used for the color change")]
    public Material material = null;

    public void SetColor(Color color)
    {
        material.color = color;
    }

    public void SetHue(float value)
    {
        Color.RGBToHSV(material.color, out _, out float s, out float v);

        value = Mathf.Clamp(value, 0, 1);
        Color newColor = Color.HSVToRGB(value, s, v);

        material.color = newColor;
    }

    [Tooltip("Color used when active")]
    public Color activeColor = Color.cyan;

    [Tooltip("Color used when inactive")]
    public Color inactiveColor = Color.white;

    public void SetActiveColor()
    {
        SetColor(activeColor);
    }

    public void SetInactiveColor()
    {
        SetColor(inactiveColor);
    }
}
