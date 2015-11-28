using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(Perspective), true)]
public class PerspectiveEditor : GraphicEditor {

	SerializedProperty m_FieldOfView;
	GUIContent m_FieldOfViewContent;

	protected override void OnEnable() {
		base.OnEnable();

		m_FieldOfViewContent	= new GUIContent("Field of View");

		m_FieldOfView	= serializedObject.FindProperty("m_FieldOfView");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.PropertyField(m_FieldOfView, m_FieldOfViewContent);

		serializedObject.ApplyModifiedProperties();
	}
}
