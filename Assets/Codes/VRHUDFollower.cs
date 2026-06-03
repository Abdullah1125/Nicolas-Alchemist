using UnityEngine;

/// <summary>
/// Smoothly follows the VR camera to create a comfortable HUD experience.
/// (VR kamerasýný yumuþak bir þekilde takip ederek rahat bir HUD deneyimi sunar.)
/// </summary>
public class VRHUDFollower : MonoBehaviour
{
    [Header("Target & Positioning (Hedef ve Konumlandýrma)")]
    public Transform vrCamera;
    public Vector3 offset = new Vector3(0f, -0.2f, 1.5f); // X: Sað/Sol, Y: Yukarý/Aþaðý, Z: Mesafe

    [Header("Smoothness (Yumuþaklýk)")]
    public float followSpeed = 6f;
    public float rotationSpeed = 8f;

    /// <summary>
    /// Updates the HUD position and rotation smoothly every frame.
    /// (HUD konumunu ve rotasyonunu her karede yumuþak bir þekilde günceller.)
    /// </summary>
    private void LateUpdate()
    {
        if (vrCamera == null) return;

        // Kameranýn baktýðý yöne ve belirlediðimiz ofsete göre hedef konumu hesapla
        Vector3 targetPosition = vrCamera.position + (vrCamera.rotation * offset);

        // Canvas'ýn her zaman kameraya dönük olmasýný saðlayacak açýyý hesapla
        Vector3 directionToCamera = transform.position - vrCamera.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);

        // Mevcut konum ve açýdan, hedef konum ve açýya doðru yumuþak geçiþ (Lerp/Slerp) yap
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}