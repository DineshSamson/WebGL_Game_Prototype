using UnityEngine;

public class AudioController : MonoBehaviour
{

    public static AudioController instance;
    public AudioSource backgroundAudio;
    public AudioSource alarmAudio;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }

    public void OnGameStart()
    {
        backgroundAudio.Play();
    }

    public void DoAlarm()
    {
        alarmAudio.Play();
    }

    public void StopAudo()
    {
        backgroundAudio.Stop();
        alarmAudio.Stop();
    }
}
