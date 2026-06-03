using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Handles potion validation, forces strict alignment inside the socket, and manages the destruction lifecycle.
/// (Ýksir doðrulamasýný yönetir, soket içinde kesin hizalamayý zorlar ve yok edilme yaþam döngüsünü kontrol eder.)
/// </summary>
public class PotionCheckerSocket : MonoBehaviour
{
    [Header("References (Referanslar)")]
    public XRSocketInteractor socketInteractor;
    public TextMeshProUGUI resultText;

    [Header("Tags (Etiket Ayarlarý)")]
    public string successTag = "GoodPotion";
    public string failTag = "RuinedPotion";

    [Header("Timer Settings (Zamanlayýcý)")]
    public float destroyDelay = 5f;

    private bool _isProcessing = false;

    /// <summary>
    /// Subscribes to interaction and global cauldron events.
    /// (Etkileþim ve küresel kazan olaylarýna abone olur.)
    /// </summary>
    private void OnEnable()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnPotionPlaced);
            socketInteractor.selectExited.AddListener(OnPotionRemoved);
        }

        AlchemyCauldron.OnNewPotionStarted += ResetScreen;
    }

    /// <summary>
    /// Unsubscribes from events to safeguard against memory leaks.
    /// (Bellek sýzýntýlarýna karþý korunmak için olay aboneliklerini iptal eder.)
    /// </summary>
    private void OnDisable()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnPotionPlaced);
            socketInteractor.selectExited.RemoveListener(OnPotionRemoved);
        }

        AlchemyCauldron.OnNewPotionStarted -= ResetScreen;
    }

    /// <summary>
    /// Processes the potion tag and initiates the lock-and-align sequence.
    /// (Ýksir etiketini iþler ve kilitleme-hizalama dizisini baþlatýr.)
    /// </summary>
    private void OnPotionPlaced(SelectEnterEventArgs args)
    {
        if (_isProcessing) return;

        GameObject placedObject = args.interactableObject.transform.gameObject;

        if (placedObject.CompareTag(successTag))
        {
            resultText.text = "BAÞARILI!";
            resultText.color = Color.green;
        }
        else if (placedObject.CompareTag(failTag))
        {
            resultText.text = "BAÞARISIZ!";
            resultText.color = Color.red;
        }
        else
        {
            resultText.text = "GECERSIZ IKSIR!";
            resultText.color = Color.yellow;
        }

        StartCoroutine(ProcessAndDestroyPotion(placedObject));
    }

    /// <summary>
    /// Resets the interface when an object is removed prematurely.
    /// (Bir obje vaktinden önce çýkarýldýðýnda arayüzü sýfýrlar.)
    /// </summary>
    private void OnPotionRemoved(SelectExitEventArgs args)
    {
        if (!_isProcessing)
        {
            ResetScreen();
        }
    }

    /// <summary>
    /// Freezes physics, forces perfect rotation/position alignment, and safely disposes of the object.
    /// (Fiziði dondurur, kusursuz rotasyon/konum hizalamasýný zorlar ve objeyi güvenli bir þekilde imha eder.)
    /// </summary>
    private IEnumerator ProcessAndDestroyPotion(GameObject potion)
    {
        _isProcessing = true;

        // Oynatýcýnýn objeyi geri almasýný engelle
        XRGrabInteractable grab = potion.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.enabled = false;
        }

        // Yerçekimini ve fiziksel savrulmayý devre dýþý býrak
        Rigidbody rb = potion.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // AÇIYI VE KONUMU DÜZELT: Soketin hedef referans noktasýný al ve þiþeyi oraya kusursuzca eþitle
        Transform targetAttach = socketInteractor.attachTransform != null ? socketInteractor.attachTransform : socketInteractor.transform;
        potion.transform.SetPositionAndRotation(targetAttach.position, targetAttach.rotation);

        yield return new WaitForSeconds(destroyDelay);

        if (potion != null)
        {
            Destroy(potion);
        }

        _isProcessing = false;
        ResetScreen();
    }

    /// <summary>
    /// Reverts the text display to the default state.
    /// (Metin ekranýný varsayýlan duruma geri döndürür.)
    /// </summary>
    private void ResetScreen()
    {
        if (resultText != null && !_isProcessing)
        {
            resultText.text = "Sise Bekleniyor...";
            resultText.color = Color.white;
        }
    }
}