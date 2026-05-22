using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketCraftingManager : MonoBehaviour
{
    [Header("Database (Veritabaný)")]
    public List<CraftingRecipe> recipeDatabase;

    [Header("Sockets (Soketler)")]
    public XRSocketInteractor firstSocket;
    public XRSocketInteractor secondSocket;

    [Header("Spawn Settings (Üretim Ayarlarý)")]
    public Transform spawnPoint;

    /// <summary>
    /// Obje aktifleþtiðinde soket dinleyicilerini kaydeder.
    /// </summary>
    private void OnEnable()
    {
        // Soketlere obje girdiðinde tetiklenecek olaylarý baðla
        firstSocket.selectEntered.AddListener(OnItemPlaced);
        secondSocket.selectEntered.AddListener(OnItemPlaced);
    }

    /// <summary>
    /// Obje pasifleþtiðinde soket dinleyicilerini temizler.
    /// </summary>
    private void OnDisable()
    {
        // Bellek sýzýntýsýný önlemek için dinleyicileri kaldýr
        firstSocket.selectEntered.RemoveListener(OnItemPlaced);
        secondSocket.selectEntered.RemoveListener(OnItemPlaced);
    }

    /// <summary>
    /// Sokete obje konduðunda çalýþýr ve üretim þartlarýný denetler.
    /// </summary>
    private void OnItemPlaced(SelectEnterEventArgs args)
    {
        // Ýki soket de doluysa iþlemi baþlat
        if (firstSocket.hasSelection && secondSocket.hasSelection)
        {
            GameObject item1 = firstSocket.GetOldestInteractableSelected().transform.gameObject;
            GameObject item2 = secondSocket.GetOldestInteractableSelected().transform.gameObject;

            ProcessCrafting(item1, item2);
        }
    }

    /// <summary>
    /// Malzemeleri veritabanýndaki tariflerle karþýlaþtýrýr.
    /// </summary>
    private void ProcessCrafting(GameObject item1, GameObject item2)
    {
        foreach (CraftingRecipe recipe in recipeDatabase)
        {
            // Hangi objenin hangi sokete konduðunu baðýmsýz olarak kontrol et
            bool isMatchA = item1.CompareTag(recipe.firstItemTag) && item2.CompareTag(recipe.secondItemTag);
            bool isMatchB = item1.CompareTag(recipe.secondItemTag) && item2.CompareTag(recipe.firstItemTag);

            if (isMatchA || isMatchB)
            {
                ExecuteCraft(item1, item2, recipe.resultPrefab);
                return; // Doðru tarif bulundu, döngüden çýk
            }
        }
    }

    /// <summary>
    /// Malzemeleri yok eder ve yeni objeyi sahneye çýkarýr.
    /// </summary>
    private void ExecuteCraft(GameObject item1, GameObject item2, GameObject resultPrefab)
    {
        // Girdi malzemelerini bellekten sil
        Destroy(item1);
        Destroy(item2);

        // Yeni objeyi belirlenen noktada yarat
        Instantiate(resultPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}