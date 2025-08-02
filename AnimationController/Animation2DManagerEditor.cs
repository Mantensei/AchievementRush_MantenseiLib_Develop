#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MantenseiLib.Editor
{
    [CustomEditor(typeof(Animator2D))]
    public class Animation2DManagerEditor : UnityEditor.Editor
    {
        private Animator2D _manager;
        // AnimationClip �C���|�[�g�p
        private AnimationClip _clipToImport;

        private void OnEnable()
        {
            _manager = (Animator2D)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // AnimationClip �C���|�[�g�Z�N�V����
            EditorGUILayout.Space();
            DrawImportSection();


            // ���s���R���g���[��
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                DrawRuntimeControls();
            }
        }

        private void DrawImportSection()
        {
            EditorGUILayout.BeginHorizontal();

            // AnimationClip�I��
            _clipToImport = (AnimationClip)EditorGUILayout.ObjectField(
                "Clip Importer",
                _clipToImport,
                typeof(AnimationClip),
                false
            );

            if (_clipToImport != null)
            {
                // �X�v���C�g���̕\��
                var spriteBindings = GetSpriteBindings(_clipToImport);
                GUI.enabled = _clipToImport != null;
                if (GUILayout.Button("Import"))
                {
                    ImportAnimationClip();
                }
                GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawRuntimeControls()
        {
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play"))
            {
                if (_manager.GetRegisteredAnimationNames().Length > 0)
                {
                    _manager.Play(_manager.GetRegisteredAnimationNames()[0]);
                }
            }

            if (GUILayout.Button("Pause"))
            {
                _manager.Pause();
            }

            if (GUILayout.Button("Resume"))
            {
                _manager.Resume();
            }

            if (GUILayout.Button("Stop"))
            {
                _manager.Stop();
            }
            EditorGUILayout.EndHorizontal();

            // ���݂̏�ԕ\��
            if (_manager.IsPlaying || _manager.IsPaused)
            {
                EditorGUILayout.HelpBox(_manager.GetDebugInfo(), MessageType.Info);
            }
        }

        private void ImportAnimationClip()
        {
            if (_clipToImport == null)
                return;

            var spriteBindings = GetSpriteBindings(_clipToImport);

            try
            {
                // SpriteFrames�̍쐬
                var frames = CreateSpriteFrames(spriteBindings, _clipToImport.frameRate);

                // AnimationData2D�̍쐬
                var animData = _clipToImport.ToAnimationData2D();

                // Undo�T�|�[�g
                Undo.RecordObject(_manager, "Import AnimationClip");

                // �A�j���[�V������o�^�i����������Inspector��_animations���X�g�̗����ɒǉ��j
                _manager.RegisterAnimation(animData);

                // SerializedObject���g�p����Inspector�ɔ��f
                serializedObject.Update();
                EditorUtility.SetDirty(_manager);
                Undo.RecordObject(_manager, "Register Animation");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Import Error", $"Failed to import animation: {e.Message}", "OK");
                Debug.LogError($"Animation import failed: {e}");
            }
        }


        public static SpriteKeyframe[] GetSpriteBindings(AnimationClip clip)
        {
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            var spriteBindings = new List<SpriteKeyframe>();

            foreach (var binding in bindings)
            {
                if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
                {
                    var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);

                    foreach (var keyframe in keyframes)
                    {
                        spriteBindings.Add(new SpriteKeyframe
                        {
                            time = keyframe.time,
                            sprite = keyframe.value as Sprite
                        });
                    }
                }
            }

            // ���ԂŃ\�[�g
            spriteBindings.Sort((a, b) => a.time.CompareTo(b.time));
            return spriteBindings.ToArray();
        }

        private SpriteFrame[] CreateSpriteFrames(SpriteKeyframe[] spriteBindings, float frameRate)
        {
            if (spriteBindings.Count() == 0)
                return new SpriteFrame[0];

            var frames = new List<SpriteFrame>();

            for (int i = 0; i < spriteBindings.Count(); i++)
            {
                var currentBinding = spriteBindings[i];

                // �t���[���������Ԃ̌v�Z
                float duration;
                if (i < spriteBindings.Count() - 1)
                {
                    // ���̃L�[�t���[���܂ł̎���
                    duration = spriteBindings[i + 1].time - currentBinding.time;
                }
                else
                {
                    // �Ō�̃t���[���̏ꍇ�A�t���[�����[�g����v�Z
                    duration = 1f / frameRate;
                }

                // �ŏ��������Ԃ̕ۏ�
                duration = Mathf.Max(duration, 1f / 60f);

                frames.Add(new SpriteFrame(currentBinding.sprite, duration));
            }

            return frames.ToArray();
        }

        // �w���p�[�N���X
        public class SpriteKeyframe
        {
            public float time;
            public Sprite sprite;
        }
    }
}
#endif

#if UNITY_EDITOR
namespace MantenseiLib.Editor
{

    [CustomPropertyDrawer(typeof(AnimationData2D))]
    public class AnimationData2DPropertyDrawer : PropertyDrawer
    {
        private const float SPRITE_SIZE = 32f;
        private const float PADDING = 2f;
        private const float LINE_HEIGHT = 18f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = LINE_HEIGHT; // AnimationData2D foldout line

            if (property.isExpanded)
            {
                height += LINE_HEIGHT; // name

                var framesProperty = property.FindPropertyRelative("<frames>k__BackingField");
                if (framesProperty != null && framesProperty.arraySize > 0)
                {
                    // �t���[���\���͏��1�s���̍����̂�
                    height += SPRITE_SIZE + PADDING * 2;
                }

                height += LINE_HEIGHT * 3; // priority, speed, loop
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var nameProperty = property.FindPropertyRelative("<name>k__BackingField");
            var framesProperty = property.FindPropertyRelative("<frames>k__BackingField");
            var priorityProperty = property.FindPropertyRelative("<priority>k__BackingField");
            var speedProperty = property.FindPropertyRelative("<speed>k__BackingField");
            var loopProperty = property.FindPropertyRelative("<loop>k__BackingField");

            var rect = new Rect(position.x, position.y, position.width, LINE_HEIGHT);

            // Foldout for the entire AnimationData2D
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);
            rect.y += LINE_HEIGHT;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Name field
                EditorGUI.PropertyField(rect, nameProperty, new GUIContent("Name"));
                rect.y += LINE_HEIGHT;

                // Frames section (�����ŕ\���A���s�Ȃ�)
                if (framesProperty != null && framesProperty.arraySize > 0)
                {
                    float framesHeight = DrawFramesHorizontally(new Rect(position.x, rect.y, position.width, SPRITE_SIZE + PADDING * 2), framesProperty);
                    rect.y += framesHeight;
                }

                // Priority field
                EditorGUI.PropertyField(rect, priorityProperty, new GUIContent("Priority"));
                rect.y += LINE_HEIGHT;

                // Speed field
                EditorGUI.PropertyField(rect, speedProperty, new GUIContent("Speed"));
                rect.y += LINE_HEIGHT;

                // Loop field
                EditorGUI.PropertyField(rect, loopProperty, new GUIContent("Loop"));

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private float DrawFramesHorizontally(Rect position, SerializedProperty framesProperty)
        {
            if (framesProperty.arraySize == 0) return 0;

            var currentRect = new Rect(position.x + 15, position.y + PADDING, SPRITE_SIZE, SPRITE_SIZE);

            for (int i = 0; i < framesProperty.arraySize; i++)
            {
                var frameProperty = framesProperty.GetArrayElementAtIndex(i);
                var spriteProperty = frameProperty.FindPropertyRelative("<sprite>k__BackingField");

                if (spriteProperty != null)
                {
                    var currentSprite = spriteProperty.objectReferenceValue as Sprite;

                    if (currentSprite != null)
                    {
                        var texture = currentSprite.texture;
                        var textureRect = currentSprite.textureRect;
                        var normalizedRect = new Rect(
                            textureRect.x / texture.width,
                            textureRect.y / texture.height,
                            textureRect.width / texture.width,
                            textureRect.height / texture.height
                        );

                        GUI.DrawTextureWithTexCoords(currentRect, texture, normalizedRect);
                    }
                    else
                    {
                        // Empty sprite placeholder
                        EditorGUI.DrawRect(currentRect, Color.gray * 0.3f);
                        GUI.Label(currentRect, "Empty", EditorStyles.centeredGreyMiniLabel);
                    }

                    // Click to select sprite
                    if (Event.current.type == EventType.MouseDown && currentRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUIUtility.ShowObjectPicker<Sprite>(currentSprite, false, "", i);
                        Event.current.Use();
                    }

                    // Handle object picker selection
                    if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == i)
                    {
                        spriteProperty.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject();
                    }
                }

                // ���̈ʒu�ֈړ��i���s�Ȃ��A�����ő�����j
                currentRect.x += SPRITE_SIZE + PADDING;
            }

            // ���1�s���̍�����Ԃ�
            return SPRITE_SIZE + PADDING * 2;
        }
    }

    public static class AnimationData2DExtensions
    {
        public static AnimationData2D ToAnimationData2D(this AnimationClip clip)
        {
            var spriteBindings = GetSpriteBindings(clip);
            var name = clip.name;
            var frameRate = clip.frameRate;
            var loop = true;
            var speed = 1f;
            var priority = 0;

            try
            {
                // SpriteFrames�̍쐬
                var frames = CreateSpriteFrames(spriteBindings, frameRate);

                // AnimationData2D�̍쐬
                var animData = new AnimationData2D(name, frames)
                {
                    loop = loop,
                    speed = speed,
                    priority = priority
                };

                return animData;
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Import Error", $"Failed to import animation: {e.Message}", "OK");
                Debug.LogError($"Animation import failed: {e}");

                return null;
            }
        }


        private static SpriteKeyframe[] GetSpriteBindings(AnimationClip clip)
        {
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            var spriteBindings = new List<SpriteKeyframe>();

            foreach (var binding in bindings)
            {
                if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
                {
                    var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);

                    foreach (var keyframe in keyframes)
                    {
                        spriteBindings.Add(new SpriteKeyframe
                        {
                            time = keyframe.time,
                            sprite = keyframe.value as Sprite
                        });
                    }
                }
            }

            // ���ԂŃ\�[�g
            spriteBindings.Sort((a, b) => a.time.CompareTo(b.time));
            return spriteBindings.ToArray();
        }

        private static SpriteFrame[] CreateSpriteFrames(SpriteKeyframe[] spriteBindings, float frameRate)
        {
            if (spriteBindings.Count() == 0)
                return new SpriteFrame[0];

            var frames = new List<SpriteFrame>();

            for (int i = 0; i < spriteBindings.Count(); i++)
            {
                var currentBinding = spriteBindings[i];

                // �t���[���������Ԃ̌v�Z
                float duration;
                if (i < spriteBindings.Count() - 1)
                {
                    // ���̃L�[�t���[���܂ł̎���
                    duration = spriteBindings[i + 1].time - currentBinding.time;
                }
                else
                {
                    // �Ō�̃t���[���̏ꍇ�A�t���[�����[�g����v�Z
                    duration = 1f / frameRate;
                }

                // �ŏ��������Ԃ̕ۏ�
                duration = Mathf.Max(duration, 1f / 60f);

                frames.Add(new SpriteFrame(currentBinding.sprite, duration));
            }

            return frames.ToArray();
        }

        // �w���p�[�N���X
        public class SpriteKeyframe
        {
            public float time;
            public Sprite sprite;
        }
    }
}
#endif
