using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ýksir tarifleri için veri modelini oluþturur. Artýk kazanýn alacaðý nihai rengi de tutar.
/// </summary>
[CreateAssetMenu(fileName = "NewPotionRecipe", menuName = "Alchemy/Potion Recipe")]
public class PotionRecipe : ScriptableObject
{
    [Header("Recipe Settings (Tarif Ayarlarý)")]
    public List<string> requiredTags;
    public GameObject resultPotionPrefab;

    [Header("Visuals (Görseller)")]
    [Tooltip("Bu tarif doðru yolda olduðunda veya tamamlandýðýnda kazanýn alacaðý renk.")]
    public Color targetColor = Color.green;
}