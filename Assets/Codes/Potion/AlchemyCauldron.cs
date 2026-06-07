using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Kazan etkileþimlerini yönetir. Karýþýmý doðrular, sývý rengini günceller ve her iki sonuçta da (baþarýlý/baþarýsýz) iksiri oyuncunun eline zorla verir.
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

    /// <summary>
    /// Baþlangýç ayarlarýný yapar ve ses kaynaðýný hazýrlar.
    /// </summary>
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource != null) _audioSource.spatialBlend = 1f;

        ResetCauldron();
    }

    /// <summary>
    /// Kazana giren objeleri (þiþe veya malzeme) algýlar.
    /// </summary>
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

    /// <summary>
    /// Kazana atýlan malzemeyi iþler ve tarife göre sývý rengini günceller.
    /// </summary>
    private void ProcessIngredient(GameObject ingredientObject, string itemTag, Color itemColor)
    {
        if (_addedTags.Count == 0)
        {
            OnNewPotionStarted?.Invoke();
        }

        _addedTags.Add(itemTag);

        if (!_isRuined)
        {
            PotionRecipe completedRecipe = GetExactMatch();

            if (completedRecipe != null)
            {
                _currentLiquidColor = completedRecipe.targetColor;
                UpdateLiquidColor(_currentLiquidColor);
            }
            else if (GetFirstValidRecipe() != null)
            {
                _currentLiquidColor = itemColor;
                UpdateLiquidColor(_currentLiquidColor);
            }
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

    /// <summary>
    /// Boþ þiþe daldýrýldýðýnda sonucu hesaplar, objeyi üretir ve koþulsuz þartsýz ele verir.
    /// </summary>
    private void DipBottle(GameObject emptyBottle)
    {
        // 1. Oyuncunun elini bul (Hangi el boþ þiþeyi tutuyor?)
        XRGrabInteractable oldGrab = emptyBottle.GetComponent<XRGrabInteractable>();
        IXRSelectInteractor holdingHand = null;

        if (oldGrab != null && oldGrab.isSelected)
        {
            holdingHand = oldGrab.interactorsSelecting[0];
        }

        // 2. Doðma noktasýný belirle ve eski þiþeyi sil
        Vector3 spawnPos = customSpawnPoint != null ? customSpawnPoint.position : emptyBottle.transform.position;
        Quaternion spawnRot = customSpawnPoint != null ? customSpawnPoint.rotation : emptyBottle.transform.rotation;

        Destroy(emptyBottle);

        // 3. Karýþým sonucuna göre doðru prefab'ý üret
        PotionRecipe matchedRecipe = GetExactMatch();
        GameObject newPotion = null;

        if (matchedRecipe != null && !_isRuined)
        {
            // BAÞARILI DURUM
            newPotion = Instantiate(matchedRecipe.resultPotionPrefab, spawnPos, spawnRot);

            if (newPotion.TryGetComponent<PotionColorController>(out PotionColorController colorController))
            {
                colorController.SetLiquidColor(matchedRecipe.targetColor);
            }

            if (successParticles != null) successParticles.Play();
            PlaySound(fillSuccessSound);
        }
        else
        {
            // BAÞARISIZ / ÇÖP DURUM
            newPotion = Instantiate(ruinedPotionPrefab, spawnPos, spawnRot);

            if (failParticles != null) failParticles.Play();
            PlaySound(failSound);
        }

        // 4. KÜRESEL XR TUTMA (Baþarýlý da olsa çöp de olsa elin içine ýþýnla ve tuttur)
        if (newPotion != null && holdingHand != null)
        {
            XRGrabInteractable newGrab = newPotion.GetComponent<XRGrabInteractable>();
            if (newGrab != null)
            {
                Transform handTransform = holdingHand.transform;

                // Offset (uzakta kalma) hatasýný önlemek için direkt elin pozisyonuna ýþýnla
                newPotion.transform.position = handTransform.position;
                newPotion.transform.rotation = handTransform.rotation;

                // Eðer çift el tutma ayarý varsa onu da güncelle
                if (newPotion.TryGetComponent<DualGripMover>(out DualGripMover dualGrip))
                {
                    dualGrip.UpdateGripPosition(handTransform);
                }

                // XR Yöneticisine objeyi zorla tutturmasý emrini ver
                XRInteractionManager interactionManager = FindFirstObjectByType<XRInteractionManager>();
                if (interactionManager != null)
                {
                    interactionManager.SelectEnter(holdingHand, (IXRSelectInteractable)newGrab);
                }
            }
        }

        // 5. Kazaný sýfýrla
        ResetCauldron();
    }

    /// <summary>
    /// Atýlan malzemelerle eþleþen ilk geçerli tarifi bulur.
    /// </summary>
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

    /// <summary>
    /// Kazandaki malzemelerle birebir eþleþen bitmiþ tarifi döndürür.
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
    /// Objenin herhangi bir tarifte kullanýlýp kullanýlmadýðýný kontrol eder.
    /// </summary>
    private bool IsTagInAnyRecipe(string tagToCheck)
    {
        foreach (PotionRecipe recipe in allRecipes)
        {
            if (recipe.requiredTags.Contains(tagToCheck)) return true;
        }
        return false;
    }

    /// <summary>
    /// Etikete karþýlýk gelen özel sývý rengini döndürür.
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
    /// Kazan sývýsýnýn (Material) rengini günceller.
    /// </summary>
    private void UpdateLiquidColor(Color newColor)
    {
        if (liquidRenderer != null)
        {
            liquidRenderer.material.SetColor("_BaseColor", newColor);
            liquidRenderer.material.SetColor("_Color", newColor);
        }
    }

    /// <summary>
    /// Kazan verilerini ve rengini varsayýlan (temiz) durumuna sýfýrlar.
    /// </summary>
    private void ResetCauldron()
    {
        _addedTags.Clear();
        _isRuined = false;
        _currentLiquidColor = defaultWaterColor;
        UpdateLiquidColor(_currentLiquidColor);
    }

    /// <summary>
    /// Belirtilen ses dosyasýný bir kerelik çalar.
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}