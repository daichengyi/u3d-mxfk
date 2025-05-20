using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ZhiSe;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    public static SoundManager Ins
    {
        get
        {
            return instance;
        }
    }

    private Dictionary<string, AudioClip> sfxClips = new();
    public AudioSource MusicSounds;
    private Dictionary<string, AudioSource> source = new();
    private List<AudioSource> sendList = new();

    private const string AUDIO_PATH = "Assets/Sounds";
    private const string TEXTURE_PATH = "Texture";

    private void Awake()
    {
        instance = this;
        IsSfx = true;
        IsShake = true;
        _isMusic = true;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        // 释放加载的音频资源
        foreach (var clip in sfxClips.Values)
        {
            Addressables.Release(clip);
        }
        sfxClips.Clear();
    }

    private bool _isMusic;
    public bool IsMusic
    {
        get
        {
            return _isMusic;
        }
        set
        {
            _isMusic = value;
            if (value)
            {
                MusicSounds.Play();
            }
            else
            {
                MusicSounds.Stop();
            }
        }
    }

    public bool IsSfx { get; set; }
    public bool IsShake { get; set; }

    private string GetFullPath(string path, string type)
    {
        return $"{type}/{path}.mp3";
    }

    private async Task<T> LoadAssetAsync<T>(string path, string type) where T : UnityEngine.Object
    {
        try
        {
            string fullPath = GetFullPath(path, type);
            var handle = Addressables.LoadAssetAsync<T>(fullPath);
            return await handle.Task;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载资源失败: {path}, 类型: {typeof(T)}, 错误: {e.Message}");
            return null;
        }
    }

    public void StopMusic()
    {
        MusicSounds.Stop();
    }

    public async void PlayMusic(string path, float volume = 1f)
    {
        var clip = await LoadAssetAsync<AudioClip>(path, AUDIO_PATH);
        if (clip != null)
        {
            MusicSounds.clip = clip;
            MusicSounds.volume = volume;
            MusicSounds.Play();
        }
    }

    public async void PlaySfx(string path, float volume = 1f)
    {
        if (!IsSfx) return;

        try
        {
            AudioSource sound = null;
            AudioClip clip;

            if (!source.TryGetValue(path, out sound))
            {
                if (!sfxClips.TryGetValue(path, out clip))
                {
                    clip = await LoadAssetAsync<AudioClip>(path, AUDIO_PATH);
                    AudioClip clipTemp;
                    if (clip != null && !sfxClips.TryGetValue(path, out clipTemp))
                    {
                        sfxClips[path] = clip;
                    }
                }
                if (clip != null)
                {
                    sound = gameObject.AddComponent<AudioSource>();
                    AudioSource soundTemp = null;
                    if (!source.TryGetValue(path, out soundTemp))
                    {
                        source.Add(path, sound);
                    }
                    sound.clip = clip;
                }
            }
            if (sound != null)
            {
                sound.volume = volume;
                sound.Play();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"播放音效失败: {path}, 错误: {e.Message}");
        }
    }

    public void StopSfx(string path)
    {
        if (source.TryGetValue(path, out AudioSource sound))
        {
            sound.Stop();
        }
    }

    public void vibrate(int vibrateType = 1)
    {
        if (!IsShake) return;
        // Debug.Log("!!!震动!!!");
        PlatformManager.Instance.vibrate(vibrateType);
    }

    // 纹理加载相关
    public Dictionary<string, Texture2D> _latticeDic = new();

    public struct LatticeParam
    {
        public string name;
        public Action<Texture2D> callback;

        public LatticeParam(string name, Action<Texture2D> callback)
        {
            this.name = name;
            this.callback = callback;
        }
    }

    public async void LoadTexture(LatticeParam data)
    {
        var texture = await LoadAssetAsync<Texture2D>(data.name, TEXTURE_PATH);
        if (texture != null)
        {
            _latticeDic[data.name] = texture;
            data.callback?.Invoke(texture);
        }
        else
        {
            data.callback?.Invoke(default);
        }
    }

    private void OnApplicationQuit()
    {
        // 释放所有加载的资源
        // foreach (var clip in sfxClips.Values) {
        //     Addressables.Release(clip);
        // }
        // foreach (var texture in _latticeDic.Values) {
        //     Addressables.Release(texture);
        // }
    }
}
