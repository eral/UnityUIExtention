// (C) 2016 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using UnityEditor;

namespace UIExtention {
	[CustomEditor(typeof(CornerColors))]
	[CanEditMultipleObjects]
	public class CornerColorsEditor : Editor {
		SerializedProperty m_UpperLeftColor;
		SerializedProperty m_UpperRightColor;
		SerializedProperty m_LowerLeftColor;
		SerializedProperty m_LowerRightColor;
		GUIContent m_CornerColorsContent;

		protected virtual void OnEnable() {
			m_CornerColorsContent = new GUIContent("Corner Colors");
			m_UpperLeftColor = serializedObject.FindProperty("m_UpperLeftColor");
			m_UpperRightColor = serializedObject.FindProperty("m_UpperRightColor");
			m_LowerLeftColor = serializedObject.FindProperty("m_LowerLeftColor");
			m_LowerRightColor = serializedObject.FindProperty("m_LowerRightColor");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			var position = GUILayoutUtility.GetRect(m_CornerColorsContent, EditorStyles.label);

			var labelPosition = position;
			labelPosition.width = EditorGUIUtility.labelWidth;
			EditorGUI.BeginProperty(position, m_CornerColorsContent, m_UpperLeftColor);
			var prefabOverride = m_UpperLeftColor.prefabOverride || m_UpperRightColor.prefabOverride || m_LowerLeftColor.prefabOverride || m_LowerRightColor.prefabOverride;
			var style = ((!prefabOverride)? EditorStyles.label: EditorStyles.boldLabel);
			EditorGUI.LabelField(position, m_CornerColorsContent, style);
			EditorGUI.EndProperty();
			position.xMin += labelPosition.width;

			var indentLevel = EditorGUI.indentLevel++;
			const float c_spacing = 2.0f;
			var valuePosition = new Rect();
			if (EditorGUIUtility.wideMode) {
				valuePosition = position;
			} else {
				valuePosition = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight);
				valuePosition = EditorGUI.IndentedRect(valuePosition);
			}
			EditorGUI.indentLevel = 0;
			valuePosition.width = valuePosition.width * 0.5f - c_spacing;
			EditorGUI.PropertyField(valuePosition, m_UpperLeftColor, GUIContent.none);
			valuePosition.x += valuePosition.width + c_spacing;
			EditorGUI.PropertyField(valuePosition, m_UpperRightColor, GUIContent.none);
			EditorGUI.indentLevel = indentLevel + 1;
			if (EditorGUIUtility.wideMode) {
				valuePosition = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight);
				valuePosition.xMin += labelPosition.width;
			} else {
				valuePosition = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight);
				valuePosition = EditorGUI.IndentedRect(valuePosition);
			}
			EditorGUI.indentLevel = 0;
			valuePosition.width = valuePosition.width * 0.5f - c_spacing;
			EditorGUI.PropertyField(valuePosition, m_LowerLeftColor, GUIContent.none);
			valuePosition.x += valuePosition.width + c_spacing;
			EditorGUI.PropertyField(valuePosition, m_LowerRightColor, GUIContent.none);
			EditorGUI.indentLevel = indentLevel;

			serializedObject.ApplyModifiedProperties();
		}
	}
}
