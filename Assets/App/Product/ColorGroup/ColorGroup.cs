using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ColorGroup : Graphic {

	private CanvasRenderer[] m_CanvasRenderers = null;
	private CanvasRenderer[] canvasRenderers {get{
		if ((m_CanvasRenderers == null) || (m_CanvasRenderers.Length == 0)) {
			m_CanvasRenderers = GetComponentsInChildren<CanvasRenderer>();
		}
		return m_CanvasRenderers;
	}}

	protected virtual void Update() {
		var color = canvasRenderers[0].GetColor();
		foreach (var canvasRenderer in canvasRenderers.Skip(1)) {
			canvasRenderer.SetColor(color);
		}
	}

	protected virtual void OnTransformChildrenChanged() {
		m_CanvasRenderers = null;
	}

	protected override void OnFillVBO(List<UIVertex> vbo) {
	}

	public override bool Raycast(Vector2 sp, Camera eventCamera) {
		return false;
	}

}
