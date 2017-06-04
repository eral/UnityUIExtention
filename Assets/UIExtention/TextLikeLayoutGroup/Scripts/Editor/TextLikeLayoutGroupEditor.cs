// (C) 2017 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UIExtention {
	[CustomEditor(typeof(TextLikeLayoutGroup))]
	[CanEditMultipleObjects]
	public class TextLikeLayoutGroupEditor : Editor {
		SerializedProperty m_Padding;
		SerializedProperty m_ElementSpacing;
		SerializedProperty m_LineSpacing;
		SerializedProperty m_WritingMode;
		SerializedProperty m_ChildAlignment;
		GUIContent m_ChildAlignmentContent;
		int m_ChildAlignmentDisplayIndex;
		GUIContent[] m_ChildAlignmentDisplayOptions;

		private static readonly string[] kWritingModeHorizontal = new[]{"Top (LeftTop)", "Center", "Bottom (RightBottom)", "Pivot"};
		private static readonly string[] kWritingModeVertical = new[]{"Left (LeftTop)", "Center", "Right (RightBottom)", "Pivot"};
		private static readonly string[][] kChildAlignmentSupplementaryDisplayOptions = new[]{
			kWritingModeHorizontal, //WritingMode.HorizontalLRTB
			kWritingModeHorizontal, //WritingMode.HorizontalRLTB
			kWritingModeHorizontal, //WritingMode.HorizontalLRBT
			kWritingModeHorizontal, //WritingMode.HorizontalRLBT
			kWritingModeVertical, //WritingMode.VerticalTBLR
			kWritingModeVertical, //WritingMode.VerticalTBRL
			kWritingModeVertical, //WritingMode.VerticalBTLR
			kWritingModeVertical, //WritingMode.VerticalBTRL
		};

		private GUIContent[] childAlignmentDisplayOptions {get{
			if (m_ChildAlignment.enumValueIndex != m_ChildAlignmentDisplayIndex) {
				if (m_ChildAlignmentDisplayOptions == null) {
					Debug.Assert(System.Enum.GetValues(typeof(TextLikeLayoutGroup.ChildAnchor)).Length == kWritingModeHorizontal.Length);
					m_ChildAlignmentDisplayOptions = new GUIContent[kWritingModeHorizontal.Length];
					for (int i = 0, iMax = m_ChildAlignmentDisplayOptions.Length; i < iMax; ++i) {
						m_ChildAlignmentDisplayOptions[i] = new GUIContent();
					}
				}
				for (int i = 0, iMax = m_ChildAlignmentDisplayOptions.Length; i < iMax; ++i) {
					m_ChildAlignmentDisplayOptions[i].text = kChildAlignmentSupplementaryDisplayOptions[m_WritingMode.enumValueIndex][i];
				}
			}
			return m_ChildAlignmentDisplayOptions;
		}}

		protected virtual void OnEnable() {
			m_ChildAlignmentContent	= new GUIContent("Child Alignment");
			m_Padding			= serializedObject.FindProperty("m_Padding");
			m_ElementSpacing	= serializedObject.FindProperty("m_ElementSpacing");
			m_LineSpacing		= serializedObject.FindProperty("m_LineSpacing");
			m_WritingMode		= serializedObject.FindProperty("m_WritingMode");
			m_ChildAlignment	= serializedObject.FindProperty("m_ChildAlignment");
			m_ChildAlignmentDisplayIndex = -1;
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_Padding, true);
			EditorGUILayout.PropertyField(m_ElementSpacing, true);
			EditorGUILayout.PropertyField(m_LineSpacing, true);
			EditorGUILayout.PropertyField(m_WritingMode, true);
			var position = GUILayoutUtility.GetRect(m_ChildAlignmentContent, EditorStyles.label);
			using (new EditorGUI.PropertyScope(position, m_ChildAlignmentContent, m_ChildAlignment)) {
				EditorGUI.BeginChangeCheck();
				var enumValueIndex = (int)EditorGUI.Popup(position, m_ChildAlignmentContent, m_ChildAlignment.enumValueIndex, childAlignmentDisplayOptions);
				if (EditorGUI.EndChangeCheck()) {
					m_ChildAlignment.enumValueIndex = enumValueIndex;
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
