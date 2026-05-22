using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class ButtonLaunch : MonoBehaviour
{
    [Header("Launch Settings (Fýrlatma Ayarlarý)")]
    [SerializeField] private float launchForce = 25f; // Gucu biraz artirdim

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    /// <summary>
    /// Bileþen referanslarýný tanýmlar ve etkileþim olayýný dinlemeye baþlar.
    /// </summary>
    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // Tetik (activate) olayýna fýrlatma metodunu baðla
        grabInteractable.activated.AddListener(OnActivateLaunch);
    }

    /// <summary>
    /// Tetikleyiciye basýldýðýnda tutulan objeyi serbest býrakýr ve fýrlatma sürecini baþlatýr.
    /// </summary>
    private void OnActivateLaunch(ActivateEventArgs args)
    {
        // Ýtme yönünü elin baktýðý yön olarak belirle
        Vector3 shootDirection = args.interactorObject.transform.forward;

        // Etkileþim yöneticisi üzerinden objeyi zorla elden býrak (Drop)
        if (grabInteractable.interactionManager != null)
        {
            grabInteractable.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grabInteractable);
        }

        // XR sisteminin objeyi sýfýrlamasýný beklemek için Coroutine baþlat
        StartCoroutine(ApplyForceRoutine(shootDirection));
    }

    /// <summary>
    /// Fizik motorunun güncellenmesini bir kare bekler ve ardýndan itme kuvvetini uygular.
    /// </summary>
    private IEnumerator ApplyForceRoutine(Vector3 direction)
    {
        // XR sisteminin objeyi elden býrakma iþlemini bitirmesi için 1 fizik karesi bekle
        yield return new WaitForFixedUpdate();

        // Sýfýrlama bittikten sonra objeye anlýk (impulse) fiziksel itme kuvveti uygula
        rb.AddForce(direction * launchForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Bellek sýzýntýlarýný önlemek için olay dinleyicisini temizler.
    /// </summary>
    private void OnDestroy()
    {
        if (grabInteractable != null)
            grabInteractable.activated.RemoveListener(OnActivateLaunch);
    }
}