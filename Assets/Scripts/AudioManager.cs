using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("---- Audio Source ----")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("---- Audio Clips ----")]
    public AudioClip backgroundMusic;
    public AudioClip lineClearSound;
    public AudioClip rotateSound;
    public AudioClip hardDropSound;
    public AudioClip lockSound;
    public AudioClip tetrisSound;

    private bool isMusicOn = true;
    private bool areSoundsOn = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (isMusicOn && musicSource.clip != backgroundMusic)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayLineClearSound()
    {
        if (areSoundsOn)
        {
            SFXSource.PlayOneShot(lineClearSound);
        }
    }

    public void PlayRotateSound()
    {
        if (areSoundsOn)
        {
            SFXSource.PlayOneShot(rotateSound);
        }
    }

    public void PlayHardDropSound()
    {
        if (areSoundsOn)
        {
            SFXSource.PlayOneShot(hardDropSound);
        }
    }

    public void PlayLockSound()
    {
        if (areSoundsOn)
        {
            SFXSource.PlayOneShot(lockSound);
        }
    }

    public void PlayTetrisSound()
    {
        if (areSoundsOn)
        {
            SFXSource.PlayOneShot(tetrisSound);
        }
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        if (isMusicOn)
        {
            musicSource.Play();
        }
        else
        {
            musicSource.Pause();
        }
    }

    public void ToggleSounds()
    {
        areSoundsOn = !areSoundsOn;
    }

    public bool IsMusicOn()
    {
        return isMusicOn;
    }

    public bool AreSoundsOn()
    {
        return areSoundsOn;
    }

}
