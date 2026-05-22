using UnityEngine;

/// <summary>
/// Ýksir þiþesinin içindeki sývýnýn materyal rengini yönetir.
/// </summary>
public class PotionColorController : MonoBehaviour
{
    [Header("Liquid Reference (Sývý Referansý)")]
    public MeshRenderer liquidRenderer;

    /// <summary>
    /// Kazandan hesaplanan rengi modelin materyaline URP uyumlu olarak uygular.
    /// </summary>
    public void SetLiquidColor(Color newColor)
    {
        if (liquidRenderer != null)
        {
            liquidRenderer.material.SetColor("_BaseColor", newColor);
            liquidRenderer.material.SetColor("_Color", newColor);
        }
    }
}