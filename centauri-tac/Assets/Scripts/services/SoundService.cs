using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public interface ISoundService
    {
        void PlaySound(string clip);
    }

    public class SoundService : ISoundService
    {
        [Inject] public IDebugService debug { get; set; }

        Dictionary<string, AudioSource> sourceCache = new Dictionary<string, AudioSource>();

        public void PlaySound(string clip)
        {
            AudioSource source = null;
            if (sourceCache.ContainsKey(clip))
            {
                source = sourceCache[clip];
            }
            else
            {
                var soundsRoot = Camera.main.transform.FindChild("Sounds");
                if (soundsRoot == null)
                {
                    debug.LogWarning("No Sound root");
                    return;
                }
                var soundGO = soundsRoot.FindChild(clip);

                if (soundGO == null)
                {
                    debug.LogWarning("No sound setup for " + clip);
                    return;
                }
                source = soundGO.GetComponent<AudioSource>();
                if (source == null)
                {
                    debug.LogWarning("No audio source found for " + clip);
                    return;
                }
            }

            source.Play();
        }
    }
}

