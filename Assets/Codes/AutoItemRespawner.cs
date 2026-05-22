using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks items by tag and respawns them at their original locations when destroyed.
/// (Eþyalarý etiketlerine göre takip eder ve silindiklerinde ilk konumlarýnda yeniden üretir.)
/// </summary>
public class GlobalItemRespawner : MonoBehaviour
{
    /// <summary>
    /// Data structure for items that need to be respawned.
    /// (Yeniden üretilmesi gereken eþyalar için veri yapýsý.)
    /// </summary>
    [System.Serializable]
    public struct ItemData
    {
        public string targetTag;
        public GameObject prefabToSpawn;
        public float respawnDelay;
    }

    [Header("Item Database (Eþya Veritabaný)")]
    public List<ItemData> itemsToTrack;

    /// <summary>
    /// Internal class to track the runtime state of each object in the scene.
    /// (Sahnedeki her objenin çalýþma zamaný durumunu takip eden dahili sýnýf.)
    /// </summary>
    private class TrackedObject
    {
        public GameObject instance;
        public Vector3 startPosition;
        public Quaternion startRotation;
        public ItemData data;
        public bool isRespawning;
    }

    private List<TrackedObject> _allTrackedObjects = new List<TrackedObject>();

    /// <summary>
    /// Finds all objects with the specified tags at startup and records their initial transforms.
    /// (Oyun baþladýðýnda belirtilen etiketlere sahip tüm objeleri bulur ve ilk konumlarýný kaydeder.)
    /// </summary>
    private void Start()
    {
        foreach (ItemData itemData in itemsToTrack)
        {
            // Sahnede bu etikete sahip tüm objeleri bul
            GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(itemData.targetTag);

            foreach (GameObject obj in foundObjects)
            {
                TrackedObject newTracker = new TrackedObject
                {
                    instance = obj,
                    startPosition = obj.transform.position,
                    startRotation = obj.transform.rotation,
                    data = itemData,
                    isRespawning = false
                };
                _allTrackedObjects.Add(newTracker);
            }
        }
    }

    /// <summary>
    /// Checks the lifecycle of tracked objects every frame.
    /// (Her karede takip edilen objelerin yaþam döngüsünü kontrol eder.)
    /// </summary>
    private void Update()
    {
        foreach (TrackedObject tracker in _allTrackedObjects)
        {
            // Obje silinmiþse (null) ve henüz üretim sürecinde deðilse
            if (tracker.instance == null && !tracker.isRespawning)
            {
                StartCoroutine(RespawnRoutine(tracker));
            }
        }
    }

    /// <summary>
    /// Waits for the specified delay and instantiates a new object at the original location.
    /// (Belirtilen süre kadar bekler ve objeyi ilk konumunda yeniden üretir.)
    /// </summary>
    private IEnumerator RespawnRoutine(TrackedObject tracker)
    {
        tracker.isRespawning = true;
        yield return new WaitForSeconds(tracker.data.respawnDelay);

        // Yeni objeyi hafýzadaki ilk konumunda üret
        GameObject newObj = Instantiate(tracker.data.prefabToSpawn, tracker.startPosition, tracker.startRotation);
        newObj.tag = tracker.data.targetTag;

        // Referansý güncelle ve üretim durumunu kapat
        tracker.instance = newObj;
        tracker.isRespawning = false;
    }
}