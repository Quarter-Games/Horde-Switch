using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.SoundSystem
{
    public interface IEffectPlayer
    {
        public static Action<AudioClip> OnPlaySFX;
    }
}
