using UnityEngine;

/// <summary>
/// Objeyi fizik kullanmadan doğrudan hedefin pozisyonuna ve açısına kilitler.
/// Test ve hizalama süreçleri için tasarlanmıştır.
/// </summary>
public class SimpleHandFollower : MonoBehaviour
{
    [Header("Target Controller (Hedef Kontrolcü)")]
    public Transform target;

    /// <summary>
    /// Her karede objenin koordinatlarını hedefin koordinatlarıyla eşitler.
    /// </summary>
    private void Update()
    {
        if (target == null) return;

        transform.position = target.position;
        transform.rotation = target.rotation;
    }

}