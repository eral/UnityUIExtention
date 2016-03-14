// (C) 2014 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using UnityEditor;

namespace UIExtention {
	[CustomEditor(typeof(TargetLayoutElementRemoteBehaviour))]
	public class TargetLayoutElementRemoteBehaviourEditor : Editor {

		public override void OnInspectorGUI() {
			var targetLayoutElementRemoteBehaviour = (TargetLayoutElementRemoteBehaviour)target;

			var oldGuiEnabled = GUI.enabled;
			GUI.enabled = false;

			EditorGUILayout.ObjectField("Source", (Object)targetLayoutElementRemoteBehaviour.Source, typeof(TargetLayoutElement), true);

			GUI.enabled = oldGuiEnabled;
		}
	}
}
