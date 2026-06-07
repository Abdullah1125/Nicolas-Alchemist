using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Çarpýþma anýnda ses çalar. Objeyi býrakma anýndaki el sürtünmesi kaynaklý sahte sesleri önler.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CollisionSoundController : MonoBehaviour
{
    [Header("Audio Settings (Ses Ayarlarý)")]
    public AudioClip impactSound;
    [Range(0f, 1f)]
    public float maxVolume = 1f;

    [Header("Physics Settings (Fizik Ayarlarý)")]
    public float minImpactForce = 1.0f;
    public float cooldown = 0.15f;

    [Header("VR Settings (VR Ayarlarý)")]
    [Tooltip("Elinden býraktýktan sonra ele sürtünme sesini engellemek için gereken sessizlik süresi (Saniye)")]
    public float muteDurationAfterRelease = 0.2f;

    private AudioSource _audioSource;
    private XRGrabInteractable _grabInteractable;
    private float _lastPlayTime;
    private float _lastReleaseTime;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1f;
        _audioSource.playOnAwake = false;

        _grabInteractable = GetComponent<XRGrabInteractable>();
    }

    /// <summary>
    /// Obje aktifken XR býrakma (Release) olayýna abone olur.
    /// </summary>
    private void OnEnable()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectExited.AddListener(OnReleasedFromHand);
        }
    }

    /// <summary>
    /// Obje kapandýðýnda bellek sýzýntýsýný önler.
    /// </summary>
    private void OnDisable()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectExited.RemoveListener(OnReleasedFromHand);
        }
    }

    /// <summary>
    /// Oyuncu objeyi elinden býraktýðý an tetiklenir ve zamaný kaydeder.
    /// </summary>
    private void OnReleasedFromHand(SelectExitEventArgs args)
    {
        _lastReleaseTime = Time.time;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. GÜVENLÝK: Obje þu an elde mi tutuluyor?
        if (_grabInteractable != null && _grabInteractable.isSelected) return;

        // 2. YENÝ GÜVENLÝK: Obje elden henüz yeni mi býrakýldý? (Milisaniyelik el sürtünmesini iptal et)
        if (Time.time - _lastReleaseTime < muteDurationAfterRelease) return;

        // 3. GÜVENLÝK: Makinalý tüfek gibi spam yapýyor mu?
        if (Time.time - _lastPlayTime < cooldown) return;

        float impactForce = collision.relativeVelocity.magnitude;

        // 4. GÜVENLÝK: Çarpma yeterince sert mi?
        if (impactForce >= minImpactForce)
        {
            _lastPlayTime = Time.time;
            float dynamicVolume = Mathf.Clamp01(impactForce / 5f) * maxVolume;
            _audioSource.PlayOneShot(impactSound, dynamicVolume);
        }
    }
}