// (C) 2016 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace UIExtention {
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

		protected override void OnPopulateMesh(VertexHelper toFill) {
			toFill.Clear();
		}

		public override bool Raycast(Vector2 sp, Camera eventCamera) {
			return false;
		}
	}
}
