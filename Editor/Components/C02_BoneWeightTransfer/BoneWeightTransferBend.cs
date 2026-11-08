using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Poyo.AvatarToolbox.BoneWeightTransfer.Editor
{
    internal sealed class BoneWeightTransferBend : IDisposable
    {
        private readonly List<Transform> _bones = new List<Transform>();
        private readonly List<Vector3> _angles = new List<Vector3>();
        private readonly List<AnimationClip> _clips = new List<AnimationClip>();
        private bool _started;
        internal float Angle { get; private set; }
        internal bool Blocked => !_started && AnimationMode.InAnimationMode();
        internal event Action Changed;

        internal BoneWeightTransferBend()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            Selection.selectionChanged += Stop;
        }

        internal void SetSources(BoneWeightTransferResult result)
        {
            float angle = Angle;
            Stop();
            _bones.Clear();
            if (result != null && result.ErrorKey == null)
            {
                foreach (BoneWeightTransferTargetRule rule in result.Rules)
                {
                    if (!rule.IsApplicable)
                    {
                        continue;
                    }
                    foreach (BoneWeightTransferSourceBoneInfo source in rule.SourceBones)
                    {
                        if (source.Bone != null && source.Bone != rule.Rule.Destination &&
                            !_bones.Contains(source.Bone))
                        {
                            _bones.Add(source.Bone);
                        }
                    }
                }
            }
            SetAngle(angle);
        }

        internal void SetAngle(float angle)
        {
            if (angle == 0f || _bones.Count == 0)
            {
                Stop();
                return;
            }
            if (Blocked || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            if (!_started)
            {
                foreach (Transform bone in _bones)
                {
                    _angles.Add(bone.localEulerAngles);
                    _clips.Add(new AnimationClip { hideFlags = HideFlags.HideAndDontSave });
                }
                AnimationMode.StartAnimationMode();
                _started = true;
            }
            Angle = angle;
            AnimationMode.BeginSampling();
            try
            {
                for (int i = 0; i < _bones.Count; i++)
                {
                    Transform bone = _bones[i];
                    if (bone == null)
                    {
                        continue;
                    }
                    Vector3 value = _angles[i] + new Vector3(0f, 0f, angle);
                    AnimationClip clip = _clips[i];
                    clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.x", AnimationCurve.Constant(0f, 0f, value.x));
                    clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.y", AnimationCurve.Constant(0f, 0f, value.y));
                    clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.z", AnimationCurve.Constant(0f, 0f, value.z));
                    // NOTE: ルートでのサンプリングは Humanoid 全体を解き直すため、各ボーンだけにかける。
                    AnimationMode.SampleAnimationClip(bone.gameObject, clip, 0f);
                }
            }
            finally
            {
                AnimationMode.EndSampling();
            }
            NotifyChanged();
        }

        internal void Stop()
        {
            // NOTE: Animation ウィンドウの編集を止めないよう、自分が始めたモードだけを終了する。
            if (_started)
            {
                AnimationMode.StopAnimationMode();
                _started = false;
            }
            Angle = 0f;
            foreach (AnimationClip clip in _clips)
            {
                UnityEngine.Object.DestroyImmediate(clip);
            }
            _clips.Clear();
            _angles.Clear();
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            if (Changed != null)
            {
                Changed();
            }
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            Stop();
        }

        public void Dispose()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Stop;
            Selection.selectionChanged -= Stop;
            Stop();
        }
    }
}
