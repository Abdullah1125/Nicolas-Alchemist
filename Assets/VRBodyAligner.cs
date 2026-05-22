using UnityEngine;

/// <summary>
/// Body Alignment Script (Gövde Hizalama Betiði)
/// Aligns the character's root body rotation with the VR headset's forward direction.
/// (Karakterin kök gövde rotasyonunu VR gözlüðünün bakýþ yönüyle hizalar.)
/// </summary>
public class VRBodyAligner : MonoBehaviour
{
    [Header("References (Referanslar)")]
    public Transform vrCamera;

    [Header("Settings (Ayarlar)")]
    public float turnSmoothness = 5f;

    /// <summary>
    /// LateUpdate is called after all Update functions have been called.
    /// (LateUpdate, tüm Update fonksiyonlarý çalýþtýktan sonra çaðrýlýr.)
    /// Used here to ensure the camera has finished its movement frame before aligning the body.
    /// (Kameranýn o karedeki hareketi bittikten sonra gövdeyi hizalamak için burada kullanýlýr.)
    /// </summary>
    void LateUpdate()
    {
        // Get the camera's forward direction but keep it flat on the ground.
        // (Kameranýn ileri yönünü al ama Z eksenini yerde düz tut.)
        Vector3 targetForward = vrCamera.forward;
        targetForward.y = 0f;

        // Prevent calculation errors if the player looks completely straight up or down.
        // (Oyuncu tam 90 derece yukarý veya aþaðý bakarsa hesaplama hatalarýný önle.)
        if (targetForward.sqrMagnitude < 0.001f) return;

        // Calculate the exact rotation needed to face the camera's forward direction.
        // (Kameranýn ileri yönüne bakmak için gereken kesin rotasyonu hesapla.)
        Quaternion targetRotation = Quaternion.LookRotation(targetForward);

        // Smoothly interpolate the body's current rotation towards the target rotation.
        // (Gövdenin mevcut rotasyonunu, hedef rotasyona doðru yumuþak bir þekilde eþitle.)
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSmoothness);
    }
}