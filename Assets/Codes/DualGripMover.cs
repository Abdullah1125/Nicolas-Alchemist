using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Production-ready dual grip manager. Optimized for zero garbage collection.
/// (Üretime hazýr çift tutma yöneticisi. Sýfýr çöp toplama için optimize edilmiþtir.)
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class DualGripMover : MonoBehaviour
{
    [Header("Grip Objects (Tutma Objeleri)")]
    public Transform mainAttachPoint;
    public Transform leftHandGrip;
    public Transform rightHandGrip;

    private XRGrabInteractable _grab;

    /// <summary>
    /// Initializes component references.
    /// (Bileþen referanslarýný baþlatýr.)
    /// </summary>
    private void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
    }

    /// <summary>
    /// Subscribes to interaction events.
    /// (Etkileþim olaylarýna abone olur.)
    /// </summary>
    private void OnEnable()
    {
        _grab.hoverEntered.AddListener(OnHandHover);
        _grab.selectEntered.AddListener(OnHandGrabbed);
    }

    /// <summary>
    /// Unsubscribes from events to prevent memory leaks.
    /// (Bellek sýzýntýlarýný önlemek için olay aboneliklerini kaldýrýr.)
    /// </summary>
    private void OnDisable()
    {
        _grab.hoverEntered.RemoveListener(OnHandHover);
        _grab.selectEntered.RemoveListener(OnHandGrabbed);
    }

    /// <summary>
    /// Triggers position update on hover.
    /// (Üzerine gelindiðinde pozisyon güncellemeyi tetikler.)
    /// </summary>
    private void OnHandHover(HoverEnterEventArgs args) => UpdateGripPosition(args.interactorObject.transform);

    /// <summary>
    /// Triggers position update on grab.
    /// (Tutma anýnda pozisyon güncellemeyi tetikler.)
    /// </summary>
    private void OnHandGrabbed(SelectEnterEventArgs args) => UpdateGripPosition(args.interactorObject.transform);

    /// <summary>
    /// Updates attach point using zero-allocation string checks.
    /// (Sýfýr bellek tahsisi yapan metin kontrolleriyle tutma noktasýný günceller. Kazan tarafýndan da tetiklenebilir.)
    /// </summary>
    public void UpdateGripPosition(Transform interactorTransform)
    {
        if (mainAttachPoint == null || leftHandGrip == null || rightHandGrip == null) return;

        Transform current = interactorTransform;

        while (current != null)
        {
            string objName = current.name;

            // Optimizasyon 1: ToLower() yerine OrdinalIgnoreCase kullanarak RAM'de yeni obje yaratýlmasýný engelliyoruz.
            if (objName.IndexOf("left", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Optimizasyon 2: Pozisyon ve açýyý tek seferde atayarak CPU maliyetini yarýya düþürüyoruz.
                mainAttachPoint.SetLocalPositionAndRotation(leftHandGrip.localPosition, leftHandGrip.localRotation);
                return; // Bulduðumuz an döngüyü kýrýp iþlemi sonlandýrýyoruz.
            }
            if (objName.IndexOf("right", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                mainAttachPoint.SetLocalPositionAndRotation(rightHandGrip.localPosition, rightHandGrip.localRotation);
                return;
            }

            current = current.parent;
        }
    }
}