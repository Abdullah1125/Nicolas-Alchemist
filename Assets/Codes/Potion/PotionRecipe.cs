using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ýksir tarifleri için etiket tabanlý veri modeli.
/// </summary>
[CreateAssetMenu(fileName = "NewPotionRecipe", menuName = "Alchemy/Potion Recipe")]
public class PotionRecipe : ScriptableObject
{
    [Header("Input Tags (Girdi Etiketleri)")]
    public List<string> requiredTags;

    [Header("Output (Çýktý)")]
    public GameObject resultPotionPrefab;
}