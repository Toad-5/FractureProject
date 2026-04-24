using UnityEditor;
using UnityEngine;

namespace Hierarchy_Tools
{
    [InitializeOnLoad]
    public static class HierarchyColourizer
    {
        private struct HierarchyStyle
        {
            public string prefix;
            public Color backgroundColor;
            public Color textColor;
            public bool bold;
            public bool italic;
            public int fontSize;
        }

        private static readonly HierarchyStyle[] Styles = new[]
        {
            new HierarchyStyle
                {
                    prefix = "###", backgroundColor = new Color(0.0f,0.4f,0.2f),
                    textColor = new Color(0.67f,1f,0.80f),   bold=true,  italic=false, fontSize=13
                },
            new HierarchyStyle
                {
                    prefix = "##",  backgroundColor = new Color(0.67f,0.0f,0.0f),
                    textColor = new Color(1f,0.8f,0.8f),   bold=true, italic=false,  fontSize=12
                },
            new HierarchyStyle
                {
                    prefix = "#",   backgroundColor = new Color(0.8f,0.25f,0.0f),
                    textColor = new Color(1f,0.9f,0.8f),   bold=true,  italic=false, fontSize=11
                },
            new HierarchyStyle
                {
                    prefix = "*", backgroundColor = new Color(0.25f,0.25f,0.25f),
                    textColor = new Color(0.75f,0.75f,0.75f),bold=false, italic=true,  fontSize=10
                        
                }
        };

        static HierarchyColourizer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;

            string name = obj.name;

            foreach (var style in Styles)
            {
                if (!name.StartsWith(style.prefix) || !name.EndsWith(style.prefix))
                    continue;

                EditorGUI.DrawRect(selectionRect, style.backgroundColor);

                var labelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = style.bold && style.italic ? FontStyle.BoldAndItalic
                        : style.bold                 ? FontStyle.Bold
                        : style.italic               ? FontStyle.Italic
                        :                              FontStyle.Normal,
                    fontSize  = style.fontSize,
                    alignment = TextAnchor.MiddleCenter,
                };
                labelStyle.normal.textColor = style.textColor;

                string displayName = name.Substring(
                    style.prefix.Length,
                    name.Length - style.prefix.Length * 2
                ).Trim();

                EditorGUI.LabelField(selectionRect, displayName, labelStyle);
                break;
            }
        }
    }
}