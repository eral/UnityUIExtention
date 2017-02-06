// (C) 2016 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using UnityEditor;

namespace UIExtention {
	[CustomEditor(typeof(Gradation))]
	public class GradationEditor : Editor {
		private SerializedProperty m_Material;
		private GUIContent m_MaterialContent;
		private SerializedProperty m_Weight;
		private GUIContent m_WeightContent;
		private GUIContent m_PreviewTitleContent;

		protected virtual void OnEnable() {
			m_MaterialContent = new GUIContent("Material");
			m_Material = serializedObject.FindProperty("m_Material");
			m_WeightContent = new GUIContent("Weight");
			m_Weight = serializedObject.FindProperty("m_Weight");
			m_PreviewTitleContent = new GUIContent("Gradation");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_Material, m_MaterialContent);
			EditorGUILayout.PropertyField(m_Weight, m_WeightContent);

			serializedObject.ApplyModifiedProperties();
		}

		public override bool HasPreviewGUI() {
			return true;
		}

		public override GUIContent GetPreviewTitle() {
			return m_PreviewTitleContent;
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background) {
			base.OnPreviewGUI(r, background);
			if (Event.current.type != EventType.Repaint) {
				return;
			}
			var material = m_Material.objectReferenceValue as GradationMaterial;
			if (material == null) {
				return;
			}

			var drawShortLength = Mathf.Min(r.width, r.height);
			var drawSize = drawShortLength * ((40.0f < drawShortLength)? 1.0f: 4.0f) * Vector2.one;
			var previewTexture = GradationMaterialEditor.CreatePreviewTexture2D(material, drawSize, Color.white, Color.gray);

			var drawRect = new Rect(r.center - drawShortLength * 0.5f * Vector2.one
									, drawShortLength * Vector2.one
									);
			GUI.DrawTexture(drawRect, previewTexture);
		}
	}
}
