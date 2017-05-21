// (C) 2014 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(RestrictInterfaceAttribute))]
public class RestrictInterfacePropertyDrawer : PropertyDrawer {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		var restrictInterfaceAttribute = (RestrictInterfaceAttribute)attribute;

		if (SerializedPropertyType.ObjectReference == property.propertyType) {
			using (new EditorGUI.PropertyScope(position, label, property)) {
				var propertyHeight = base.GetPropertyHeight(property, label);
				position.height = propertyHeight;

				EditorGUI.BeginChangeCheck();
				var value = EditorGUI.ObjectField(position, label, property.objectReferenceValue, fieldInfo.FieldType, true);
				if (EditorGUI.EndChangeCheck()) {
					if (null == value) {
						property.objectReferenceValue = value;
					} else if (restrictInterfaceAttribute.RestrictType.IsAssignableFrom(value.GetType())) {
						property.objectReferenceValue = value;
					} else {
						var components = ((Component)value).GetComponents<Component>().Where(x=>0 == (HideFlags.HideInInspector & x.hideFlags))
																					.Where(x=>restrictInterfaceAttribute.RestrictType.IsAssignableFrom(x.GetType()))
																					.ToArray();
						if (0 == components.Length) {
							property.objectReferenceValue = null;
						} else {
							property.objectReferenceValue = components[0];
						}
					}
				}
			}
		} else {
			EditorGUI.LabelField(position, label, new GUIContent("This type has not supported."));
		}
	}
}
#endif

public class RestrictInterfaceAttribute : PropertyAttribute {
	public System.Type RestrictType {get; set;}

	public RestrictInterfaceAttribute(System.Type restrictType) {
		RestrictType = restrictType;
	}
}
