using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
    /// <summary>
    /// 音频事件处理示例
    /// </summary>
	public class HandleEventWithAudioExample : MonoBehaviour {

		public SkeletonAnimation skeletonAnimation;
		[SpineEvent(dataField: "skeletonAnimation", fallbackToTextField: true)]
		public string eventName;

		[Space]
		public AudioSource audioSource;
		public AudioClip audioClip;
		public float basePitch = 1f;
		public float randomPitchOffset = 0.1f;

		[Space]
		public bool logDebugMessage = false;

		Spine.EventData eventData;

		void OnValidate () {
			if (skeletonAnimation == null) GetComponent<SkeletonAnimation>();
			if (audioSource == null) GetComponent<AudioSource>();
		}

		void Start () {
			if (audioSource == null) return;
			if (skeletonAnimation == null) return;
			skeletonAnimation.Initialize(false);
			if (!skeletonAnimation.valid) return;

			eventData = skeletonAnimation.Skeleton.Data.FindEvent(eventName);
			skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
		}

		private void HandleAnimationStateEvent (TrackEntry trackEntry, Event e) {
			if (logDebugMessage) Debug.Log("Event fired! " + e.Data.Name);
			//bool eventMatch = string.Equals(e.Data.Name, eventName, System.StringComparison.Ordinal); // Testing recommendation: String compare.
			bool eventMatch = (eventData == e.Data); // Performance recommendation: Match cached reference instead of string.
			if (eventMatch) {
				Play();
			}
		}

		public void Play () {
			audioSource.pitch = basePitch + Random.Range(-randomPitchOffset, randomPitchOffset);
			audioSource.clip = audioClip;
			audioSource.Play();
		}
	}

}
