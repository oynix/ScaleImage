using System;
using System.Linq.Expressions;
using UnityEditor;
using UnityEditor.UI;

namespace UnityEngine.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BlendImage))]
    public class BlendImageEditor : ImageEditor
    {
        private MonoScript _monoScript;

        [MenuItem("GameObject/UI/Blend Image")]
        private static void CreateWithMenu()
        {
            var ps = Selection.objects;
            if (ps == null || ps.Length == 0) return;
            foreach (var p in ps)
            {
                if (p is GameObject pg)
                {
                    var go = new GameObject("BlendImage");
                    go.AddComponent<BlendImage>();
                    var rt = go.GetComponent<RectTransform>();
                    rt.SetParent(pg.transform);
                    rt.localScale = Vector3.one;
                    rt.anchoredPosition = Vector2.zero;
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _monoScript = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", _monoScript, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            // SpriteGUI();
            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            NativeSizeButtonGUI();

            // ScaleTypeGUI();
            // BoundSizeGUI();
            // RoundCornerGUI();

            DestElementGUI();
            SrcElementGUI();

            PaddingGUI();

            BoundSizeGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void DestElementGUI()
        {
            var image = target as BlendImage;
            if (image == null) return;
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Destination Image (Background)");
            DrawProperty(GetFieldName(() => image.destImage));
            
            // EditorGUI.indentLevel += 1;
            // var spriteProp = serializedObject.FindProperty("destImage.sprite");
            // EditorGUILayout.PropertyField(spriteProp);
            // DrawProperty(GetFieldName(() => image.destImage.scaleType));
            // DrawProperty(GetFieldName(() => image.destImage.roundCorner));
            // if (image.destImage.roundCorner)
            // {
            //     DrawProperty(GetFieldName(() => image.destImage.radiusRatio));
            //     DrawProperty(GetFieldName(() => image.destImage.triangleNum));
            // }
            //
            if (GUILayout.Button("print corner"))
            {
                Debug.Log($"corner:{image.destImage.roundCorner}");
            }
            //
            // EditorGUI.indentLevel -= 1;
        }

        private void SrcElementGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Source Image (Main Content)");
            var image = target as BlendImage;
            if (image == null) return;
            DrawProperty(GetFieldName(() => image.srcImage));
        }

        private void PaddingGUI()
        {
            var image = target as BlendImage;
            if (image == null) return;
            DrawProperty(GetFieldName(() => image.padding));
        }

        private void BoundSizeGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Use Parent Size", EditorStyles.miniButton))
                {
                    foreach (var t in targets)
                    {
                        if (t is BlendImage si)
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