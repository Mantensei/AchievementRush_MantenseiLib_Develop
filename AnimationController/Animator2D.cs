using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MantenseiLib
{
    /// <summary>
    /// スプライトアニメーションの1フレーム情報
    /// </summary>
    [System.Serializable]
    public class SpriteFrame
    {
        [field: SerializeField] public Sprite sprite { get; set; }
        [field: SerializeField] public float duration { get; set; } = 0.1f;

        public SpriteFrame() { }

        public SpriteFrame(Sprite sprite, float duration = 0.1f)
        {
            this.sprite = sprite;
            this.duration = duration;
        }

        public SpriteFrame Clone()
        {
            return new SpriteFrame
            {
                sprite = this.sprite,
                duration = this.duration,
            };
        }
    }

    /// <summary>
    /// 2Dアニメーションデータ
    /// </summary>
    [System.Serializable]
    public class AnimationData2D
    {
        // セパレート文字定数
        private static readonly string[] SEPARATORS = { "_", "-", "." };

        [field: SerializeField] public string name { get; set; }
        [field: SerializeField] public SpriteFrame[] frames { get; set; }
        [field: SerializeField] public int priority { get; set; } = 0;
        [field: SerializeField] public float speed { get; set; } = 1f;
        [field: SerializeField] public bool loop { get; set; } = true;
        public static float frameRate => 60f;

        // 階層的管理用プロパティ
        public string characterName => nameParts.FirstOrDefault();

        public string motionName => nameParts.LastOrDefault();
        public string[] nameParts => GetNameParts();

        // 合計再生時間
        public float TotalDuration => frames?.Sum(f => f.duration) ?? 0f;
        public int FrameCount => frames?.Length ?? 0;
        public bool IsValid => !string.IsNullOrEmpty(name) && frames != null && frames.Length > 0;

        public AnimationData2D() { }

        public AnimationData2D(string name, SpriteFrame[] frames)
        {
            this.name = name;
            this.frames = frames;
        }

        public AnimationData2D Clone()
        {
            return new AnimationData2D
            {
                name = this.name,
                frames = this.frames?.Select(f => f.Clone()).ToArray(),
                priority = this.priority,
                speed = this.speed,
                loop = this.loop,
            };
        }

        private string[] GetNameParts()
        {
            foreach (var separator in SEPARATORS)
            {
                if (name.Contains(separator))
                {
                    return name.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            return new string[] { name };
        }
    }

    public interface IAnimator2D
    {
        Animator2D Animator { get; }
    }

    /// <summary>
    /// フレームイベント情報
    /// </summary>
    [System.Serializable]
    public class FrameEvent
    {
        public int frameIndex;
        public Action action;

        public FrameEvent(int frameIndex, Action action)
        {
            this.frameIndex = frameIndex;
            this.action = action;
        }
    }

    /// <summary>
    /// 汎用的に使える2Dアニメーション管理システム
    /// </summary>
    public class Animator2D : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private List<AnimationData2D> _animations = new List<AnimationData2D>();
        [SerializeField] private bool _playOnAwake = true;

        // コンポーネント参照
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)] 
        public SpriteRenderer sr { get; private set; }

        // アニメーション管理
        private readonly Dictionary<string, AnimationData2D> _animationDict = new Dictionary<string, AnimationData2D>();
        private readonly Dictionary<string, List<FrameEvent>> _frameEvents = new Dictionary<string, List<FrameEvent>>();

        // 再生状態の管理
        private AnimationData2D _currentAnimation;
        private Coroutine _playCoroutine;
        private bool _isPaused = false;
        private float _currentTime = 0f;
        private int _currentFrameIndex = 0;
        private int _previousFrameIndex = -1;

        // イベント
        public event Action<AnimationData2D> OnAnimationStart;
        public event Action<AnimationData2D> OnAnimationComplete;

        #region Properties

        public string CurrentAnimationName => _currentAnimation?.name ?? "";
        public bool IsPlaying => _playCoroutine != null && !_isPaused;
        public bool IsPaused => _isPaused;
        public float CurrentTime => _currentTime;
        public int CurrentFrameIndex => _currentFrameIndex;
        public float CurrentAnimationLength => _currentAnimation?.TotalDuration ?? 0f;
        public int CurrentAnimationFrameCount => _currentAnimation?.FrameCount ?? 0;
        public float Progress => CurrentAnimationLength > 0 ? _currentTime / CurrentAnimationLength : 0f;
        public float CurrentSpeed => _currentAnimation?.speed ?? 1f;
        public bool CurrentLoop => _currentAnimation?.loop ?? false;
        public SpriteFrame CurrentFrame => _currentAnimation?.frames?[_currentFrameIndex];

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeAnimations();

            if (_playOnAwake)
            {
                string animToPlay = _animations.FirstOrDefault()?.name;
                if (!string.IsNullOrEmpty(animToPlay))
                {
                    Play(animToPlay);
                }
            }
        }

        private void OnGUI()
        {
            var info = GetDebugInfo();
            var style = new GUIStyle()
            {
                fontSize = 32,
                normal = new GUIStyleState { textColor = Color.white },
            };

            GUI.Label(new Rect(10, 10, 400, 100), info, style);
        }

        #endregion

        #region Initialization

        private void InitializeAnimations()
        {
            _animationDict.Clear();
            foreach (var anim in _animations)
            {
                _animationDict[anim.name] = anim;
            }
        }

        #endregion


        #region Animation Control

        /// <summary>
        /// アニメーションを名前で再生
        /// </summary>
        public bool Play(string animationName, bool checkPriority = true)
        {
            if (string.IsNullOrEmpty(animationName))
            {
                Debug.LogWarning("Animation name is null or empty.");
                return false;
            }

            if (!_animationDict.TryGetValue(animationName, out var animData))
            {
                animData = FindMotionAnimation(animationName);

                if (animData == null) 
                {
                    Debug.LogWarning($"Animation '{animationName}' not found.");
                    return false;
                }
            }

            return Play(animData, checkPriority);
        }

        /// <summary>
        /// アニメーションデータで再生
        /// </summary>
        public bool Play(AnimationData2D animData, bool checkPriority = true)
        {
            if (animData?.IsValid != true)
            {
                Debug.LogWarning("Animation data is invalid.");
                return false;
            }

            RegisterAnimation(animData); // 動的に登録も行うようにする

            // 優先度チェック
            if (checkPriority && _currentAnimation != null)
            {
                if (animData.priority < _currentAnimation.priority)
                {
                    return false;
                }
            }

            // 同じアニメーションの場合はスキップ（オプション）
            if (_currentAnimation == animData && IsPlaying)
            {
                return true;
            }

            Stop();

            _currentAnimation = animData;
            _currentTime = 0f;
            _currentFrameIndex = 0;
            _previousFrameIndex = -1;
            _isPaused = false;

            _playCoroutine = StartCoroutine(PlayAnimationCoroutine());

            OnAnimationStart?.Invoke(animData);
            return true;
        }

        /// <summary>
        /// アニメーションを強制的に再生（優先度無視）
        /// </summary>
        public bool ForcePlay(string animationName)
        {
            return Play(animationName, false);
        }

        public bool TryPause(AnimationData2D animationData2D) => TryPause(animationData2D?.name);

        public bool TryPause(string animationName)
        {
            if(CurrentAnimationName == animationName)
            {
                Stop();
                return true;
            }

            return false;
        }

        public void Stop()
        {
            if (_playCoroutine != null)
            {
                StopCoroutine(_playCoroutine);
                _playCoroutine = null;
            }

            _isPaused = false;
            _currentTime = 0f;
            _currentFrameIndex = 0;
            _previousFrameIndex = -1;

            _currentAnimation = null;
        }

        public void Pause() => _isPaused = true;

        public void Resume() => _isPaused = false;

        public void SetTime(float time)
        {
            _currentTime = Mathf.Clamp(time, 0f, _currentAnimation.TotalDuration);
            UpdateFrameIndex();
            UpdateSprite();
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            SetTime(progress * _currentAnimation.TotalDuration);
        }

        public void SetFrame(int frameIndex)
        {
            frameIndex = Mathf.Clamp(frameIndex, 0, _currentAnimation.frames.Length - 1);

            // 指定フレームまでの経過時間を計算
            float targetTime = 0f;
            for (int i = 0; i < frameIndex; i++)
            {
                targetTime += _currentAnimation.frames[i].duration;
            }

            SetTime(targetTime);
        }

        #endregion

        #region Animation Search

        /// <summary>
        /// モーション名でアニメーションを検索（フォールバック付き）
        /// </summary>
        private AnimationData2D FindMotionAnimation(string motionName)
        {
            var candidates = _animationDict.Values.Where(anim => anim.motionName == motionName).ToList();
            return candidates.FirstOrDefault();
        }

        #endregion

        #region Frame Events

        public void AddFrameEvent(string animationName, int frameIndex, Action action)
        {
            if (!_frameEvents.ContainsKey(animationName))
            {
                _frameEvents[animationName] = new List<FrameEvent>();
            }

            var frameEvent = new FrameEvent(frameIndex, action);
            _frameEvents[animationName].Add(frameEvent);
        }

        public void ClearFrameEvents(string animationName)
        {
            _frameEvents.Remove(animationName);
        }

        #endregion

        #region Animation Registration

        public void RegisterAnimation(AnimationData2D animData)
        {
            if (!animData.IsValid)
            {
                Debug.LogWarning("Cannot register invalid animation data.");
                return;
            }

            _animationDict[animData.name] = animData;

            // インスペクター用リストにも追加
            if (!_animations.Exists(a => a.name == animData.name))
            {
                _animations.Add(animData);
            }

        }

        public void RegisterAnimation(string name, SpriteFrame[] frames, float speed = 1f, bool loop = true)
        {
            var animData = new AnimationData2D(name, frames)
            {
                speed = speed,
                loop = loop
            };

            RegisterAnimation(animData);
        }

        public void UnregisterAnimation(string name)
        {
            _animationDict.Remove(name);
            _animations.RemoveAll(a => a.name == name);
            ClearFrameEvents(name);
        }

        public string[] GetRegisteredAnimationNames()
        {
            return _animationDict.Keys.ToArray();
        }

        public bool HasAnimation(string name)
        {
            return _animationDict.ContainsKey(name);
        }

        public bool HasMotion(string motionName)
        {
            return _animationDict.Values.Any(anim => anim.motionName == motionName);
        }

        public bool HasCharacterMotion(string characterName, string motionName)
        {
            return _animationDict.Values.Any(anim =>
                anim.characterName == characterName && anim.motionName == motionName);
        }

        public AnimationData2D GetAnimation(string name)
        {
            return _animationDict.TryGetValue(name, out var anim) ? anim : null;
        }

        #endregion

        #region Private Methods

        private IEnumerator PlayAnimationCoroutine()
        {
            while (_currentAnimation?.IsValid == true)
            {
                if (!_isPaused)
                {
                    _currentTime += Time.deltaTime * _currentAnimation.speed;
                    UpdateFrameIndex();
                    UpdateSprite();
                    CheckFrameEvents();

                    // アニメーション終了判定
                    if (_currentTime >= _currentAnimation.TotalDuration)
                    {
                        if (_currentAnimation.loop)
                        {
                            _currentTime = 0f;
                            _currentFrameIndex = 0;
                            _previousFrameIndex = -1;
                        }
                        else
                        {
                            OnAnimationComplete?.Invoke(_currentAnimation);
                            break;
                        }
                    }
                }

                yield return null;
            }

            _playCoroutine = null;
        }

        private void UpdateFrameIndex()
        {
            if (_currentAnimation?.frames == null) return;

            _previousFrameIndex = _currentFrameIndex;

            float accumulatedTime = 0f;
            for (int i = 0; i < _currentAnimation.frames.Length; i++)
            {
                accumulatedTime += _currentAnimation.frames[i].duration;
                if (_currentTime <= accumulatedTime)
                {
                    _currentFrameIndex = i;
                    return;
                }
            }

            _currentFrameIndex = _currentAnimation.frames.Length - 1;
        }

        private void UpdateSprite()
        {
            if (_currentAnimation?.frames == null || sr == null) return;

            var frame = _currentAnimation.frames[_currentFrameIndex];
            if (frame == null) return;

            sr.sprite = frame.sprite;
        }

        private void CheckFrameEvents()
        {
            if (_currentAnimation == null) return;
            if (_currentFrameIndex == _previousFrameIndex) return;

            string animName = _currentAnimation.name;
            if (!_frameEvents.TryGetValue(animName, out var events)) return;

            foreach (var frameEvent in events)
            {
                if (_currentFrameIndex == frameEvent.frameIndex)
                {
                    frameEvent.action?.Invoke();
                }
            }
        }

        #endregion

        #region Debug

        public string GetDebugInfo()
        {
            if (_currentAnimation == null)
                return "No animation playing";

            return $"Animation: {CurrentAnimationName}\n" +
                   $"Character: {_currentAnimation.characterName} | Motion: {_currentAnimation.motionName}\n" +
                   $"Playing: {IsPlaying} | Paused: {IsPaused}\n" +
                   $"Time: {_currentTime:F2}/{CurrentAnimationLength:F2}\n" +
                   $"Frame: {_currentFrameIndex}/{CurrentAnimationFrameCount}\n" +
                   $"Progress: {Progress:P1}";
        }

        #endregion
    }
}