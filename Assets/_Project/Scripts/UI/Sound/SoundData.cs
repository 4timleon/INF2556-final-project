using System;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    [Serializable]
    public class SoundData
    {
        public AudioClip[] clips;
        public AudioMixerGroup mixerGroup;
        public bool loop;
        public bool playOnAwake;
        public float volume;
        public bool frequentSound;
        public float spatialBlend;
    }
}

