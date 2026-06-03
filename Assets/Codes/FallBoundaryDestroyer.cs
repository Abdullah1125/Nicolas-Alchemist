using UnityEngine;

/// <summary>
/// Destroys the object if it falls below a specified Y coordinate.
/// (Obje belirtilen Y koordinatýnýn altýna düþerse onu yok eder.)
/// </summary>
public class FallBoundaryDestroyer : MonoBehaviour
{
    [Header("Boundary Settings (Sýnýr Ayarlarý)")]
    public float killYLevel = -5.86f;

    /// <summary>
    /// Checks the object's Y position every frame.
    /// (Her karede objenin Y konumunu kontrol eder.)
    /// </summary>
    private void Update()
    {
        // Eðer objenin o anki yüksekliði (Y) belirlediðimiz sýnýrýn altýndaysa
        if (transform.position.y < killYLevel)
        {
            Destroy(gameObject);
        }
    }
}