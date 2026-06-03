using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Unity 6 XR objeleri için

/// <summary>
/// Plays an impact sound with spam-prevention (Cooldown) and XR Grab awareness.
/// (XR tutma kontrolü ve Cooldown sistemi ile spam yapmayan darbe sesi oynatýcýsý.)
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ImpactSoundPlayer : MonoBehaviour
{
    [Header("Audio Settings (Ses Ayarlarý)")]
    public AudioClip impactSound;
    [Range(0f, 1f)]
    public float maxVolume = 1f;

    [Header("Physics Settings (Fizik Ayarlarý)")]
    [Tooltip("Sadece bu hýzýn üzerindeki çarpmalarda ses çýkar (Yere yavaþça konduðunda susar).")]
    public float minImpactVelocity = 1.2f;

    [Tooltip("Sesin makineli tüfek gibi arka arkaya çalmasýný engellemek için bekleme süresi (Saniye).")]
    public float cooldownTime = 0.15f;

    private AudioSource _audioSource;
    private XRGrabInteractable _grabInteractable;
    private float _lastPlayTime;

    /// <summary>
    /// Bileþenleri ve VR ayarlarýný baþlatýr.
    /// </summary>
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1f;
        _audioSource.playOnAwake = false;

        // Üzerinde VR tutma kodu varsa onu hafýzaya al
        _grabInteractable = GetComponent<XRGrabInteractable>();
    }

    /// <summary>
    /// Fiziksel çarpýþmalarý algýlar, filtrelerden geçirip sesi çalar.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // 1. GÜVENLÝK FÝLTRESÝ: Obje þu an oyuncunun elinde mi? (Eðer elindeyse ses çýkarma)
        if (_grabInteractable != null && _grabInteractable.isSelected) return;

        // 2. GÜVENLÝK FÝLTRESÝ: Son sesin üzerinden yeterince zaman geçmediyse (Cooldown) iptal et
        if (Time.time - _lastPlayTime < cooldownTime) return;

        // Çarpýþmanýn hýzýný (þiddetini) al
        float impactForce = collision.relativeVelocity.magnitude;

        // 3. GÜVENLÝK FÝLTRESÝ: Çarpýþma yeterince sert mi?
        if (impactForce >= minImpactVelocity)
        {
            if (impactSound != null && _audioSource != null)
            {
                // Sesin çalýndýðý zamaný kaydet (Cooldown'u baþlat)
                _lastPlayTime = Time.time;

                float dynamicVolume = Mathf.Clamp01(impactForce / 5f) * maxVolume;
                _audioSource.PlayOneShot(impactSound, dynamicVolume);
            }
        }
    }
}