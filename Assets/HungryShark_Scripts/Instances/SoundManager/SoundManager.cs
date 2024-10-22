using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class SoundManager : MonoBehaviour
    {
        #region Creating Instance
        public static SoundManager _instance;
        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SoundManager>();
                    if (_instance == null)
                    {
#if UNITY_EDITOR
                        Debug.LogError("There is no SpawnManager in the scene!");
#endif
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Private Variables

        [Header("Audio Files")]
        [SerializeField] private List<SharkGameDataModel.SoundModel> _soundsList;
        [Header("Audio Source")]
        [SerializeField] private AudioSource _gameAudioSource;

        [Header("Shark Sound Audio Source")]
        [SerializeField] private AudioSource _sharkSoundAudioSource;

        [Header("Game Audio Source")]
        [SerializeField] private AudioSource _inGameAudioSource;
        #endregion

        private void Start()
        {
            PlayGameAudioClip(SharkGameDataModel.Sound.MainThemeSound, true);
        }
        public void PlayAudioClip(SharkGameDataModel.Sound _sound)
        {
#if UNITY_EDITOR
            Debug.LogError("Watersplash" + _sound);
#endif

            _gameAudioSource.clip = _soundsList.Find(x => x._soundType == _sound)._audioClip;
            _gameAudioSource.Play();

            StartCoroutine(PlayTheAudioTillLength(_gameAudioSource.clip));
        }

        private IEnumerator PlayTheAudioTillLength(AudioClip _clip)
        {
            yield return new WaitForSeconds(_clip.length);
        }

        public void PlayGameAudioClip(SharkGameDataModel.Sound _sound, bool isLoop)
        {
#if UNITY_EDITOR
            Debug.LogError("Watersplash" + _sound);
#endif
            if (UIController.Instance.quitButtonClicked)
            {
                _inGameAudioSource.gameObject.GetComponent<AudioSource>().enabled = true;
            }
            _inGameAudioSource.clip = _soundsList.Find(x => x._soundType == _sound)._audioClip;
            _inGameAudioSource.Play();
            _inGameAudioSource.loop = isLoop;

            StartCoroutine(PlayTheGameAudioTillLength(_inGameAudioSource.clip));
        }

        private IEnumerator PlayTheGameAudioTillLength(AudioClip _clip)
        {
            yield return new WaitForSeconds(_clip.length);
        }

        internal void OnQuitGame()
        {
            PlayGameAudioClip(SharkGameDataModel.Sound.MainThemeSound, true);
        }

        public void PlaySharkAudioClip(SharkGameDataModel.Sound _sound)
        {
#if UNITY_EDITOR
            Debug.LogError("Watersplash" + _sound);
#endif

            _sharkSoundAudioSource.clip = _soundsList.Find(x => x._soundType == _sound)._audioClip;
            _sharkSoundAudioSource.Play();

            StartCoroutine(PlayTheSharkAudioTillLength(_sharkSoundAudioSource.clip));
        }

        private IEnumerator PlayTheSharkAudioTillLength(AudioClip _clip)
        {
            yield return new WaitForSeconds(_clip.length);
        }

        public void MuteSounds(bool isMuted)
        {
            _inGameAudioSource.mute = isMuted;
            _sharkSoundAudioSource.mute = isMuted;
            _gameAudioSource.mute = isMuted;
        }
    }
}
