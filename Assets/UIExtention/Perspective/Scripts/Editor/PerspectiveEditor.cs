using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

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
