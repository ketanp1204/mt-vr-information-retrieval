using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AudioGuide : MonoBehaviour
{

    /* Private Variables */
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private TextMeshPro displayText;



    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource.isPlaying)
        {
            displayText.text = "Pause Audio Guide";
        }
        else
        {
            displayText.text = "Play Audio Guide";
        }
    }

    public void PlayPauseAudioGuide()
    {
        if (audioSource != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.Pause();
            }
        }
    }
}
