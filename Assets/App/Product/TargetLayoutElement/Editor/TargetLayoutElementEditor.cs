/*
 * (C) 2014 ERAL
 * Distributed under the Boost Software License, Version 1.0.
 * (See copy at http://www.boost.org/LICENSE_1_0.txt)
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(TargetLayoutElement))]
public class TargetLayoutElementEditor : Editor {

	public override void OnInspectorGUI() {
		var targetLayoutElement = (TargetLayoutElement)target;

		EditorGUIUtility.LookLikeControls();
		EditorGUI.BeginChangeCheck();
		var value = EditorGUILayout.ObjectField("Target", (Object)targetLayoutElement.Target, typeof(Component), true);
		if (EditorGUI.EndChangeCheck()) {
			if (null == value) {
				targetLayoutElement.Target = null;
			} else if (typeof(ILayoutElement).IsAssignableFrom(value.GetType())) {
				targetLayoutElement.Target = (ILayoutElement)value;
			} else {
				var components = ((Component)value).GetComponents<Component>().Where(x=>0 == (HideFlags.HideInInspector & x.hideFlags))
																			.Where(x=>typeof(ILayoutElement).IsAssignableFrom(x.GetType()))
																			.ToArray();
				if (0 == components.Length) {
					targetLayoutElement.Target = null;
				} else {
					targetLayoutElement.Target = (ILayoutElement)components[0];
				}
			}
		}
	}
}
