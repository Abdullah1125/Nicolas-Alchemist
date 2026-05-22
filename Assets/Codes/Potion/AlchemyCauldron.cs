using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Manages cauldron tag reading, URP coloring, and XR dual-grip integrated crafting.
/// (Kazan içi etiket okuma, URP renk deðiþimi ve XR dual-grip entegreli üretimi yönetir.)
/// </summary>
public class AlchemyCauldron : MonoBehaviour
{
    [System.Serializable]
    public struct TagColorMap
    {
        public string itemTag;
        public Color color;
    }

    [Header("Database (Veritabaný)")]
    public List<PotionRecipe> allRecipes;
    public List<TagColorMap> tagColorMappings;

    [Header("Spawn Settings (Üretim Ayarlarý)")]
    [Tooltip("Dolu iksirin çýkacaðý nokta. Boþ býrakýlýrsa XR ile otomatik ele verilir.")]
    public Transform customSpawnPoint;

    [Header("Liquid Visuals (Sývý Görselleri)")]
    public MeshRenderer liquidRenderer;
    public Color defaultWaterColor = Color.cyan;
    private Color _currentLiquidColor;

    [Header("Feedback Effects (Geri Bildirim Efektleri)")]
    public ParticleSystem successParticles;
    public ParticleSystem failParticles;
    public GameObject ruinedPotionPrefab;

    private List<string> _addedTags = new List<string>();
    private bool _isRuined = false;

    /// <summary>
    /// Resets the cauldron state on awake.
    /// (Oyun baþladýðýnda kazan durumunu sýfýrlar.)
    /// </summary>
    private void Start()
    {
        ResetCauldron();
    }

    /// <summary>
    /// Checks tags of objects entering the liquid trigger.
    /// (Sývý alanýna giren objelerin etiketlerini kontrol eder.)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EmptyBottle"))
        {
            DipBottle(other.gameObject);
            return;
        }

        Color? itemColor = GetColorForTag(other.tag);
        if (itemColor.HasValue)
        {
            ProcessIngredient(other.gameObject, other.tag, itemColor.Value);
        }
    }

    /// <summary>
    /// Processes the ingredient and directly updates URP liquid color.
    /// (Atýlan malzemeyi iþler ve sývý URP rengini direkt günceller.)
    /// </summary>
    private void ProcessIngredient(GameObject ingredientObject, string itemTag, Color itemColor)
    {
        _addedTags.Add(itemTag);

        if (!_isRuined)
        {
            if (IsPathStillValid())
            {
                _currentLiquidColor = itemColor;

                if (liquidRenderer != null)
                {
                    liquidRenderer.material.SetColor("_BaseColor", _currentLiquidColor);
                    liquidRenderer.material.SetColor("_Color", _currentLiquidColor);
                }
            }
            else
            {
                _isRuined = true;
            }
        }

        Destroy(ingredientObject);
    }

    /// <summary>
    /// Evaluates mixture, spawns potion, and forces XR grip with DualGrip pre-warm.
    /// (Karýþýmý doðrular, iksiri üretir ve DualGrip hazýrlýðýyla XR eline zorla verir.)
    /// </summary>
    private void DipBottle(GameObject emptyBottle)
    {
        XRGrabInteractable oldGrab = emptyBottle.GetComponent<XRGrabInteractable>();
        IXRSelectInteractor holdingHand = null;

        if (oldGrab != null && oldGrab.isSelected)
        {
            holdingHand = oldGrab.interactorsSelecting[0];
        }

        Vector3 spawnPos = customSpawnPoint != null ? customSpawnPoint.position : emptyBottle.transform.position;
        Quaternion spawnRot = customSpawnPoint != null ? customSpawnPoint.rotation : emptyBottle.transform.rotation;

        Destroy(emptyBottle);

        PotionRecipe matchedRecipe = GetExactMatch();

        if (matchedRecipe != null && !_isRuined)
        {
            GameObject newPotion = Instantiate(matchedRecipe.resultPotionPrefab, spawnPos, spawnRot);

            if (newPotion.TryGetComponent<PotionColorController>(out PotionColorController colorController))
            {
                colorController.SetLiquidColor(_currentLiquidColor);
            }

            // XR DUAL-GRIP ENTEGRASYONU
            if (customSpawnPoint == null && holdingHand != null)
            {
                XRGrabInteractable newGrab = newPotion.GetComponent<XRGrabInteractable>();
                if (newGrab != null)
                {
                    // Þiþeyi ele vermeden HEMEN ÖNCE tutma noktasýný ilgili ele göre ayarla
                    if (newPotion.TryGetComponent<DualGripMover>(out DualGripMover dualGrip))
                    {
                        dualGrip.UpdateGripPosition(holdingHand.transform);
                    }

                    XRInteractionManager interactionManager = FindFirstObjectByType<XRInteractionManager>();
                    if (interactionManager != null)
                    {
                        // Tutma noktasý doðru ayarlandýðý için ele jilet gibi oturur
                        interactionManager.SelectEnter(holdingHand, (IXRSelectInteractable)newGrab);
                    }
                }
            }

            if (successParticles != null) successParticles.Play();
        }
        else
        {
            Instantiate(ruinedPotionPrefab, spawnPos, spawnRot);
            if (failParticles != null) failParticles.Play();
        }

        ResetCauldron();
    }

    /// <summary>
    /// Validates if current tags match any valid recipe path.
    /// (Mevcut etiketlerin herhangi bir tarif rotasýna uyup uymadýðýný sýnar.)
    /// </summary>
    private bool IsPathStillValid()
    {
        foreach (PotionRecipe recipe in allRecipes)
        {
            bool possible = true;
            List<string> tempReq = new List<string>(recipe.requiredTags);

            foreach (string added in _addedTags)
            {
                if (!tempReq.Remove(added))
                {
                    possible = false;
                    break;
                }
            }
            if (possible) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the exact recipe matching current ingredients.
    /// (Eklenen etiketlerle birebir eþleþen tarifi döndürür.)
    /// </summary>
    private PotionRecipe GetExactMatch()
    {
        foreach (PotionRecipe recipe in allRecipes)
        {
            if (recipe.requiredTags.Count == _addedTags.Count)
            {
                bool match = true;
                List<string> tempReq = new List<string>(recipe.requiredTags);
                foreach (string added in _addedTags)
                {
                    if (!tempReq.Remove(added)) match = false;
                }
                if (match) return recipe;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the mapped color for the given ingredient tag.
    /// (Etikete karþýlýk gelen rengi haritadan bulur.)
    /// </summary>
    private Color? GetColorForTag(string searchTag)
    {
        foreach (TagColorMap map in tagColorMappings)
        {
            if (map.itemTag == searchTag) return map.color;
        }
        return null;
    }

    /// <summary>
    /// Clears data and resets cauldron visuals.
    /// (Kazan verilerini ve rengini sýfýrlar.)
    /// </summary>
    private void ResetCauldron()
    {
        _addedTags.Clear();
        _isRuined = false;
        _currentLiquidColor = defaultWaterColor;

        if (liquidRenderer != null)
        {
            liquidRenderer.material.SetColor("_BaseColor", _currentLiquidColor);
            liquidRenderer.material.SetColor("_Color", _currentLiquidColor);
        }
    }
}