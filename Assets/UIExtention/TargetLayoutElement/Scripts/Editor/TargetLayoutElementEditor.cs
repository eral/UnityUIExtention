// (C) 2014 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;

namespace UIExtention {
	[CustomEditor(typeof(TargetLayoutElement))]
	[CanEditMultipleObjects]
	public class TargetLayoutElementEditor : Editor {
		SerializedProperty m_Target;
		GUIContent m_TargetContent;

		protected virtual void OnEnable() {
			m_TargetContent	= new GUIContent("Target");
			m_Target		= serializedObject.FindProperty("m_Target");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_Target, m_TargetContent);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
