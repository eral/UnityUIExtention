// (C) 2015 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderPropertyDrawer : PropertyDrawer {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		var minMaxSliderAttribute = (MinMaxSliderAttribute)attribute;

		if (SerializedPropertyType.Vector2 == property.propertyType) {
			const float c_spacing = 5.0f;
			EditorGUIUtility.LookLikeControls();

			var value = property.vector2Value;
			EditorGUI.BeginChangeCheck();

			position = EditorGUI.PrefixLabel(position, label);

			var minFloatFieldPosition = position;
			minFloatFieldPosition.width = EditorGUIUtility.fieldWidth;
			value.x = EditorGUI.FloatField(minFloatFieldPosition, value.x);
			position.xMin += minFloatFieldPosition.width + c_spacing;

			var minMaxSliderPosition = position;
			minMaxSliderPosition.width -= EditorGUIUtility.fieldWidth + c_spacing;
			EditorGUI.MinMaxSlider(minMaxSliderPosition, ref value.x, ref value.y, minMaxSliderAttribute.Min, minMaxSliderAttribute.Max);
			position.xMin += minMaxSliderPosition.width + c_spacing;

			var maxFloatFieldPosition = position;
			maxFloatFieldPosition.width = EditorGUIUtility.fieldWidth;
			value.y = EditorGUI.FloatField(maxFloatFieldPosition, value.y);
			position.xMin += maxFloatFieldPosition.width + c_spacing;

			if (EditorGUI.EndChangeCheck()) {
				property.vector2Value = value;
			}
		} else {
			EditorGUI.LabelField(position, label, new GUIContent("Use MinMaxSlider with Vector2."));
		}
	}
}
#endif

public class MinMaxSliderAttribute : PropertyAttribute {
	public float Min {get; set;}
	public float Max {get; set;}

	public MinMaxSliderAttribute(float min, float max) {
		Min = min;
		Max = max;
	}
}
