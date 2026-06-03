using UnityEngine;

/// <summary>
/// Manages sequential background music playback.
/// (Sýralý arka plan müziði çalýnmasýný yönetir.)
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlaylistManager : MonoBehaviour
{
    [Header("Playlist Settings (Çalma Listesi Ayarlarý)")]
    public AudioClip[] musicTracks;

    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    private AudioSource _audioSource;
    private int _currentTrackIndex = 0;

    /// <summary>
    /// Initializes the audio source and starts the playlist.
    /// (Ses kaynaðýný baþlatýr ve çalma listesini oynatmaya baþlar.)
    /// </summary>
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        // Settings for global background music (Evrensel arka plan müziði ayarlarý)
        _audioSource.loop = false;
        _audioSource.spatialBlend = 0f;
        _audioSource.volume = musicVolume;
        _audioSource.playOnAwake = false;

        PlayNextTrack();
    }

    /// <summary>
    /// Checks if the current track has finished playing every frame.
    /// (Her karede mevcut parçanýn bitip bitmediðini kontrol eder.)
    /// </summary>
    private void Update()
    {
        if (musicTracks.Length == 0) return;

        // If audio stops playing, trigger the next track (Ses durduðunda sýradakini tetikle)
        if (!_audioSource.isPlaying)
        {
            PlayNextTrack();
        }
    }

    /// <summary>
    /// Plays the next track in the array and loops back to the start if necessary.
    /// (Dizideki bir sonraki parçayý çalar ve gerekirse baþa döner.)
    /// </summary>
    private void PlayNextTrack()
    {
        if (musicTracks.Length == 0) return;

        _audioSource.clip = musicTracks[_currentTrackIndex];
        _audioSource.Play();

        _currentTrackIndex++;

        // Loop back to the first track if the playlist is over (Liste bittiyse ilk parçaya dön)
        if (_currentTrackIndex >= musicTracks.Length)
        {
            _currentTrackIndex = 0;
        }
    }
}