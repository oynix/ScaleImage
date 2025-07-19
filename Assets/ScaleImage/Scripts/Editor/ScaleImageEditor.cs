using System;
using System.Linq.Expressions;
using Solitaire.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Editor.Inspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScaleImage))]
    public class ScaleImageEditor : ImageEditor
    {
        private MonoScript _monoScript;
        // private ScaleImage _target;
        // private RectTransform _rect;

        // public void OnEnable()
        // {
        //     base.OnEnable();
        //     _monoScript = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
        //     _target = (ScaleImage)target;
        // }

        [MenuItem("GameObject/UI/ScaleImage")]
        private static void CreateWithMenu()
        {
            var ps = Selection.objects;
            if (ps == null || ps.Length == 0) return;
            foreach (var p in ps)
            {
                if (p is GameObject pg)
                {
                    var go = new GameObject("ScaleImage");
                    go.AddComponent<ScaleImage>();
                    var rt = go.GetComponent<RectTransform>();
                    rt.SetParent(pg.transform);
                    rt.anchoredPosition = Vector2.zero;
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _monoScript = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
            // _target = (ScaleImage)target;
            // _rect = _target.GetComponent<RectTransform>();
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", _monoScript, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            SpriteGUI();
            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            NativeSizeButtonGUI();

            // TargetTypeAndSizeGUI();
            ScaleTypeGUI();
            BoundSizeGUI();
            RoundCornerGUI();

            serializedObject.ApplyModifiedProperties();
        }

        // private void TargetTypeAndSizeGUI()
        // {
        //     DrawProperty(GetFieldName(() => _target.targetType));
        //     if (_target.targetType == TargetType.Bound)
        //     {
        //         DrawProperty(GetFieldName(() => _target.boundSize));
        //         EditorGUILayout.BeginHorizontal();
        //         {
        //             GUILayout.Space(EditorGUIUtility.labelWidth);
        //             if (GUILayout.Button("Use Current Size", EditorStyles.miniButton))
        //             {
        //                 var r = _rect.rect;
        //                 _target.boundSize.x = (int)r.width;
        //                 _target.boundSize.y = (int)r.height;
        //             }
        //         }
        //         EditorGUILayout.EndHorizontal();
        //     }
        // }

        private void BoundSizeGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Use Parent Size", EditorStyles.miniButton))
                {
                    foreach (var t in targets)
                    {
                        if (t is ScaleImage si)
                        {
                            var rect = si.gameObject.GetComponent<RectTransform>();
                            var p = rect.parent.GetComponent<RectTransform>();
                            var pr = p.rect;
                            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pr.width);
                            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pr.height);
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ScaleTypeGUI()
        {
            var si = target as ScaleImage;
            if (si == null) return;
            DrawProperty(GetFieldName(() => si.scaleType));
            // EditorGUILayout.BeginHorizontal();
            // {
            //     GUILayout.Space(EditorGUIUtility.labelWidth);
            //     if (GUILayout.Button("Scale", EditorStyles.miniButton))
            //     {
            //         _target.ReviseSprite();
            //     }
            // }
            // EditorGUILayout.EndHorizontal();
        }

        private void RoundCornerGUI()
        {
            var image = target as ScaleImage;
            if (image == null) return;
            DrawProperty(GetFieldName(() => image.roundCorner));
            if (image.roundCorner)
            {
                DrawProperty(GetFieldName(() => image.radiusRatio));
                DrawProperty(GetFieldName(() => image.triangleNum));
            }
        }

        #region draw property

        private void DrawProperty(string fieldName)
        {
            var p = serializedObject.FindProperty(fieldName);
            EditorGUILayout.PropertyField(p);
        }

        private string GetFieldName<T>(Expression<Func<T>> e)
        {
            return ((MemberExpression)e.Body).Member.Name;
        }

        #endregion
    }
}