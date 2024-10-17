using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace SharkGame
{
    public class SceneLoader : MonoBehaviour
    {
        [Header("Video Player Reference")]
        [SerializeField] private VideoPlayer _videoPlayer;

        void Start()
        {
            _videoPlayer.loopPointReached += OnVideoEnd;
        }

        // This function will be called when the video finishes
        void OnVideoEnd(VideoPlayer vp)
        {
            // Load the next scene
            SceneManager.LoadScene("OurWaterScene");
        }
    }
}
