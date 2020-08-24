﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// Add this to an AudioSource to shake its pitch remapped along a curve 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MMAudioSourcePitchShaker")]
    [RequireComponent(typeof(AudioSource))]
    public class MMAudioSourcePitchShaker : MMShaker
    {
        [Header("Pitch")]
        /// whether or not to add to the initial value
        public bool RelativePitch = false;
        /// the curve used to animate the intensity value on
        public AnimationCurve ShakePitch = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(0.5f, 0f), new Keyframe(1, 1f));
        /// the value to remap the curve's 0 to
        [Range(-3f, 3f)]
        public float RemapPitchZero = 0f;
        /// the value to remap the curve's 1 to
        [Range(-3f, 3f)]
        public float RemapPitchOne = 1f;

        /// the audio source to pilot
        protected AudioSource _targetAudioSource;
        protected float _initialPitch;
        protected float _originalShakeDuration;
        protected bool _originalRelativePitch;
        protected AnimationCurve _originalShakePitch;
        protected float _originalRemapPitchZero;
        protected float _originalRemapPitchOne;

        /// <summary>
        /// On init we initialize our values
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
            _targetAudioSource = this.gameObject.GetComponent<AudioSource>();
        }
               
        /// <summary>
        /// When that shaker gets added, we initialize its shake duration
        /// </summary>
        protected virtual void Reset()
        {
            ShakeDuration = 2f;
        }

        /// <summary>
        /// Shakes values over time
        /// </summary>
        protected override void Shake()
        {
            float newPitch = ShakeFloat(ShakePitch, RemapPitchZero, RemapPitchOne, RelativePitch, _initialPitch);
            _targetAudioSource.pitch = newPitch;
        }

        /// <summary>
        /// Collects initial values on the target
        /// </summary>
        protected override void GrabInitialValues()
        {
            _initialPitch = _targetAudioSource.pitch;
        }

        /// <summary>
        /// When we get the appropriate event, we trigger a shake
        /// </summary>
        /// <param name="pitchCurve"></param>
        /// <param name="duration"></param>
        /// <param name="amplitude"></param>
        /// <param name="relativePitch"></param>
        /// <param name="attenuation"></param>
        /// <param name="channel"></param>
        public virtual void OnMMAudioSourcePitchShakeEvent(AnimationCurve pitchCurve, float duration, float remapMin, float remapMax, bool relativePitch = false,
            float attenuation = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true)
        {
            if (!CheckEventAllowed(channel) || Shaking)
            {
                return;
            }
            
            _resetShakerValuesAfterShake = resetShakerValuesAfterShake;
            _resetTargetValuesAfterShake = resetTargetValuesAfterShake;

            if (resetShakerValuesAfterShake)
            {
                _originalShakeDuration = ShakeDuration;
                _originalShakePitch = ShakePitch;
                _originalRemapPitchZero = RemapPitchZero;
                _originalRemapPitchOne = RemapPitchOne;
                _originalRelativePitch = RelativePitch;
            }

            ShakeDuration = duration;
            ShakePitch = pitchCurve;
            RemapPitchZero = remapMin * attenuation;
            RemapPitchOne = remapMax * attenuation;
            RelativePitch = relativePitch;

            Play();
        }

        /// <summary>
        /// Resets the target's values
        /// </summary>
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            _targetAudioSource.pitch = _initialPitch;
        }

        /// <summary>
        /// Resets the shaker's values
        /// </summary>
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ShakeDuration = _originalShakeDuration;
            ShakePitch = _originalShakePitch;
            RemapPitchZero = _originalRemapPitchZero;
            RemapPitchOne = _originalRemapPitchOne;
            RelativePitch = _originalRelativePitch;
        }

        /// <summary>
        /// Starts listening for events
        /// </summary>
        public override void StartListening()
        {
            base.StartListening();
            MMAudioSourcePitchShakeEvent.Register(OnMMAudioSourcePitchShakeEvent);
        }

        /// <summary>
        /// Stops listening for events
        /// </summary>
        public override void StopListening()
        {
            base.StopListening();
            MMAudioSourcePitchShakeEvent.Unregister(OnMMAudioSourcePitchShakeEvent);
        }
    }

    /// <summary>
    /// An event used to trigger vignette shakes
    /// </summary>
    public struct MMAudioSourcePitchShakeEvent
    {
        public delegate void Delegate(AnimationCurve pitchCurve, float duration, float remapMin, float remapMax, bool relativePitch = false,
            float attenuation = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(AnimationCurve pitchCurve, float duration, float remapMin, float remapMax, bool relativePitch = false,
            float attenuation = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true)
        {
            OnEvent?.Invoke(pitchCurve, duration, remapMin, remapMax, relativePitch,
                attenuation, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake);
        }
    }
}
