// (C) 2016 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using UnityEditor;

namespace UIExtention {
	[CustomEditor(typeof(AroundLayoutGroup))]
	public class AroundLayoutGroupEditor : Editor {
		SerializedProperty m_Path;
		SerializedProperty m_Range;
		SerializedProperty m_Justify;
		SerializedProperty m_Offset;
		SerializedProperty m_ChildAlignment;
		GUIContent m_PathContent;
		GUIContent m_RangeContent;
		GUIContent m_JustifyContent;
		GUIContent m_OffsetContent;
		GUIContent m_ChildAlignmentContent;

		protected virtual void OnEnable() {
			m_PathContent			= new GUIContent("Path");
			m_RangeContent			= new GUIContent("Range");
			m_JustifyContent		= new GUIContent("Justify");
			m_OffsetContent			= new GUIContent("Offset");
			m_ChildAlignmentContent	= new GUIContent("Child Alignment");
			m_Path				= serializedObject.FindProperty("m_Path");
			m_Range				= serializedObject.FindProperty("m_Range");
			m_Justify			= serializedObject.FindProperty("m_Justify");
			m_Offset			= serializedObject.FindProperty("m_Offset");
			m_ChildAlignment	= serializedObject.FindProperty("m_ChildAlignment");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_Path, m_PathContent);
			EditorGUILayout.PropertyField(m_Range, m_RangeContent);
			EditorGUILayout.PropertyField(m_Justify, m_JustifyContent);
			EditorGUILayout.PropertyField(m_Offset, m_OffsetContent);
			{
				var position = GUILayoutUtility.GetRect(m_ChildAlignmentContent, EditorStyles.label);

				var labelPosition = position;
				labelPosition.width = EditorGUIUtility.labelWidth;
				EditorGUI.BeginProperty(position, m_ChildAlignmentContent, m_ChildAlignment);
				EditorGUI.LabelField(position, m_ChildAlignmentContent);
				EditorGUI.EndProperty();
				position.xMin += labelPosition.width;

				var valuePosition = position;
				m_ChildAlignment.enumValueIndex = (int)(AroundLayoutGroup.ChildAnchor)EditorGUI.EnumPopup(valuePosition, (AroundLayoutGroup.ChildAnchor)m_ChildAlignment.enumValueIndex);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
