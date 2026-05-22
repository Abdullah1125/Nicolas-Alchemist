using UnityEngine;

/// <summary>
/// Üretim tarifleri için veri modeli oluþturur.
/// </summary>
[CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Input (Girdiler)")]
    public string firstItemTag;
    public string secondItemTag;

    [Header("Output (Çýktý)")]
    public GameObject resultPrefab;
}