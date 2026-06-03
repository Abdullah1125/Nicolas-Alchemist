using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Kazan etkileþimlerini yönetir. Tarif tamamlandýðýnda kazan sývýsý otomatik olarak final iksir rengine dönüþür.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AlchemyCauldron : MonoBehaviour
{
    public static event Action OnNewPotionStarted;

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
    public Transform customSpawnPoint;

    [Header("Liquid Visuals (Sývý Görselleri)")]
    public MeshRenderer liquidRenderer;
    public Color defaultWaterColor = Color.cyan;
    public Color ruinedWaterColor = new Color(0.2f, 0.1f, 0.1f); // Çamur Rengi
    private Color _currentLiquidColor;

    [Header("Audio & Feedback (Ses ve Geri Bildirim)")]
    public ParticleSystem successParticles;
    public ParticleSystem failParticles;
    public GameObject ruinedPotionPrefab;

    [Space(10)]
    public AudioClip splashSound;
    public AudioClip fillSuccessSound;
    public AudioClip failSound;
    private AudioSource _audioSource;

    private List<string> _addedTags = new List<string>();
    private bool _isRuined = false;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource != null) _audioSource.spatialBlend = 1f;

        ResetCauldron();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EmptyBottle"))
        {
            DipBottle(other.gameObject);
            return;
        }

        if (IsTagInAnyRecipe(other.tag))
        {
            Color? itemColor = GetColorForTag(other.tag);
            if (itemColor.HasValue)
            {
                ProcessIngredient(other.gameObject, other.tag, itemColor.Value);
            }
        }
    }

    private void ProcessIngredient(GameObject ingredientObject, string itemTag, Color itemColor)
    {
        if (_addedTags.Count == 0)
        {
            OnNewPotionStarted?.Invoke();
        }

        _addedTags.Add(itemTag);

        if (!_isRuined)
        {
            // 1. KONTROL: Atýlan bu son malzeme ile iksir tamamen bitti mi?
            PotionRecipe completedRecipe = GetExactMatch();

            if (completedRecipe != null)
            {
                // ÝKSÝR PÝÞTÝ! Kazan direkt olarak tarifin final rengini (Target Color) alsýn.
                _currentLiquidColor = completedRecipe.targetColor;
                UpdateLiquidColor(_currentLiquidColor);

                // Ýstersen buraya sonradan bir "Ding!" baþarma sesi de ekleyebilirsin.
            }
            // 2. KONTROL: Henüz bitmedi, doðru yolda mý?
            else if (GetFirstValidRecipe() != null)
            {
                // Yolda, atýlan malzemenin rengini (TagColorMap) alsýn.
                _currentLiquidColor = itemColor;
                UpdateLiquidColor(_currentLiquidColor);
            }
            // 3. KONTROL: Hatalý malzeme mi atýldý?
            else
            {
                _isRuined = true;
                _currentLiquidColor = ruinedWaterColor;
                UpdateLiquidColor(_currentLiquidColor);
            }
        }

        PlaySound(splashSound);
        Destroy(ingredientObject);
    }

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
                colorController.SetLiquidColor(matchedRecipe.targetColor);
            }

            if (customSpawnPoint == null && holdingHand != null)
            {
                XRGrabInteractable newGrab = newPotion.GetComponent<XRGrabInteractable>();
                if (newGrab != null)
                {
                    if (newPotion.TryGetComponent<DualGripMover>(out DualGripMover dualGrip))
                    {
                        dualGrip.UpdateGripPosition(holdingHand.transform);
                    }

                    XRInteractionManager interactionManager = FindFirstObjectByType<XRInteractionManager>();
                    if (interactionManager != null)
                    {
                        interactionManager.SelectEnter(holdingHand, (IXRSelectInteractable)newGrab);
                    }
                }
            }

            if (successParticles != null) successParticles.Play();
            PlaySound(fillSuccessSound);
        }
        else
        {
            Instantiate(ruinedPotionPrefab, spawnPos, spawnRot);
            if (failParticles != null) failParticles.Play();
            PlaySound(failSound);
        }

        ResetCauldron();
    }

    private PotionRecipe GetFirstValidRecipe()
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
            if (possible) return recipe;
        }
        return null;
    }

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

    private bool IsTagInAnyRecipe(string tagToCheck)
    {
        foreach (PotionRecipe recipe in allRecipes)
        {
            if (recipe.requiredTags.Contains(tagToCheck)) return true;
        }
        return false;
    }

    private Color? GetColorForTag(string searchTag)
    {
        foreach (TagColorMap map in tagColorMappings)
        {
            if (map.itemTag == searchTag) return map.color;
        }
        return null;
    }

    private void UpdateLiquidColor(Color newColor)
    {
        if (liquidRenderer != null)
        {
            liquidRenderer.material.SetColor("_BaseColor", newColor);
            liquidRenderer.material.SetColor("_Color", newColor);
        }
    }

    private void ResetCauldron()
    {
        _addedTags.Clear();
        _isRuined = false;
        _currentLiquidColor = defaultWaterColor;
        UpdateLiquidColor(_currentLiquidColor);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}