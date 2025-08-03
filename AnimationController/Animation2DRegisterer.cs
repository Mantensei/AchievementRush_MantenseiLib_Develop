using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MantenseiLib
{
    public class Animation2DRegisterer : MonoBehaviour
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        public IAnimator2D IAnimator { get; private set; }
        public Animator2D Animator => IAnimator.Animator;

        [field:SerializeField] public AnimationData2D AnimationData2D { get; set; }


        public bool Play()
        {
            return Animator.Play(AnimationData2D);
        }

        public void Pause()
        {
            Animator.TryPause(AnimationData2D);
        }
    }
}

#if UNITY_EDITOR
namespace MantenseiLib.Editor
{
    [CustomEditor(typeof(Animation2DRegisterer))]
    public class Animation2DRegistererEditor : UnityEditor.Editor
    {
        private Animation2DRegisterer _registerer;
        private AnimationClip _clipToImport;

        private void OnEnable()
        {
            _registerer = (Animation2DRegisterer)target;
        }
        public override void OnInspectorGUI()
        {
            if(_registerer.AnimationData2D.FrameCount > 0)
                DrawDefaultInspector();
            else
                EditorGUILayout.LabelField("- No AnimationData2D found -");

            // AnimationClip インポートセクション
            EditorGUILayout.Space();
            DrawImportSection();
        }

        private void DrawImportSection()
        {
            EditorGUILayout.BeginHorizontal();

            // AnimationClip選択
            _clipToImport = (AnimationClip)EditorGUILayout.ObjectField(
                "Clip Importer",
                _clipToImport,
                typeof(AnimationClip),
                false
            );

            if (_clipToImport != null)
            {
                // スプライト情報の表示
                var spriteBindings = Animation2DManagerEditor.GetSpriteBindings(_clipToImport);
                GUI.enabled = _clipToImport != null;
                if (GUILayout.Button("Import"))
                {
                    _registerer.AnimationData2D = _clipToImport.ToAnimationData2D();
                }
                GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif