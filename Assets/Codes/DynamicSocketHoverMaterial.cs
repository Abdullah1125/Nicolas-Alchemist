using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// XR Soketinin varsayilan onizleme materyalini, sokete yaklasan objenin gercek materyali ile anlik degistirir.
/// </summary>
[RequireComponent(typeof(XRSocketInteractor))]
public class DynamicSocketHoverMaterial : MonoBehaviour
{
    private XRSocketInteractor _socket;

    /// <summary>
    /// Bilesen referansini alir ve varsayilan ayarlari yapar.
    /// </summary>
    private void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();

        // Unity'nin kendi gorsel cizim sistemini aktif et
        _socket.showInteractableHoverMeshes = true;
    }

    /// <summary>
    /// Soket etkilesim olaylarini dinlemeye baslar.
    /// </summary>
    private void OnEnable()
    {
        _socket.hoverEntered.AddListener(OnHoverEnter);
    }

    /// <summary>
    /// Bellek sizintilarini onlemek icin olay dinleyicilerini temizler.
    /// </summary>
    private void OnDisable()
    {
        _socket.hoverEntered.RemoveListener(OnHoverEnter);
    }

    /// <summary>
    /// Sokete obje girdiginde materyalini kopyalar ve soket sistemine aktarir.
    /// </summary>
    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        // Yaklasan asil objeyi al
        GameObject hoverObj = args.interactableObject.transform.gameObject;

        // Objenin uzerindeki ilk gecerli MeshRenderer bilesenini bul
        MeshRenderer renderer = hoverObj.GetComponentInChildren<MeshRenderer>();

        // Eger objenin bir gorseli ve materyali varsa, soketin materyalini guncelle
        if (renderer != null && renderer.sharedMaterial != null)
        {
            _socket.interactableHoverMeshMaterial = renderer.sharedMaterial;
        }
    }
}