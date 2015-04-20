using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UICollider : Graphic, ICanvasRaycastFilter {

	protected override void Awake() {
		base.Awake();
		color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
	}

	protected override void OnFillVBO(List<UIVertex> vbo) {
	}

	public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
		return true;
	}
}
