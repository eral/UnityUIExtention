// (C) 2016 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace UIExtention {
	[CustomEditor(typeof(Perspective), true)]
	public class PerspectiveEditor : GraphicEditor {

		SerializedProperty m_FieldOfView;
		SerializedProperty m_PerspectiveCorrectShader;
		GUIContent m_FieldOfViewContent;
		GUIContent m_PerspectiveCorrectShaderContent;

		protected override void OnEnable() {
			base.OnEnable();

			m_FieldOfViewContent				= new GUIContent("Field of View");
			m_PerspectiveCorrectShaderContent	= new GUIContent("Perspective Correct Shader");

			m_FieldOfView				= serializedObject.FindProperty("m_FieldOfView");
			m_PerspectiveCorrectShader	= serializedObject.FindProperty("m_PerspectiveCorrectShader");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_FieldOfView, m_FieldOfViewContent);
			EditorGUILayout.PropertyField(m_PerspectiveCorrectShader, m_PerspectiveCorrectShaderContent);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
