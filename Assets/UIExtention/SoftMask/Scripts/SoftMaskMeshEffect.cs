using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace UIExtention {
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public class SoftMaskMeshEffect : BaseMeshEffect {
		public RectTransform rectTransform {
			get {
				if (m_rectTransform == null) {
					m_rectTransform = transform as RectTransform;
				}
				return m_rectTransform;
			}
		}
		private RectTransform m_rectTransform;

		public OrientedRect2D orientedRect2D {
			get {
				if (!m_orientedRect2D.HasValue) {
					m_orientedRect2D = new OrientedRect2D(rectTransform);
				}
				return m_orientedRect2D.Value;
			}
		}
		private OrientedRect2D? m_orientedRect2D;

		public SoftMaskRect softMaskRect {
			get {
				if (m_softMaskRect == null) {
					m_softMaskRect = GetComponentInParent<SoftMaskRect>();
				}
				return m_softMaskRect;
			}
		}
		private SoftMaskRect m_softMaskRect;

		public void SetVerticesDirty() {
			graphic.SetVerticesDirty();
		}

		public override void ModifyMesh(VertexHelper vh) {
			if (!IsActive()) {
				return;
			}

			if (!softMaskRect.orientedRect2D.Overlaps(orientedRect2D)) {
				vh.Clear();
				return;
			}

			var vertices = new List<UIVertex>();
			vh.GetUIVertexStream(vertices);
			vh.Clear();
			var indices = Enumerable.Range(0, vertices.Count).ToList();
			for(int i = 0, iMax = vertices.Count; i < iMax; ++i) {
				vertices[i] = SetPosition(vertices[i], rectTransform.TransformPoint(vertices[i].position));
			}
			VertexUtility.Intersect(vertices, indices, softMaskRect.Vertices);
			for(int i = 0, iMax = vertices.Count; i < iMax; ++i) {
				vertices[i] = SetPosition(vertices[i], rectTransform.InverseTransformPoint(vertices[i].position));
			}
			vh.AddUIVertexStream(vertices, indices);
		}

		private static UIVertex SetPosition(UIVertex vertex, Vector3 position) {
			vertex.position = position;
			return vertex;
		}

		protected override void OnTransformParentChanged() {
			base.OnTransformParentChanged();
			SetVerticesDirty();
			transferToSoftMaskRect();
		}

		private void transferToSoftMaskRect() {
			m_softMaskRect = null;
			if (softMaskRect == null) {
#if UNITY_EDITOR
				if (!UnityEditor.EditorApplication.isPlaying) {
					DestroyImmediate(this);
				} else //fall through
#endif
				Destroy(this);
			}
		}

		protected virtual void LateUpdate() {
			var rectTransformHashCode = HashUtility.GetValueHashCode(rectTransform);
			if (lastRectTransformHashCode != rectTransformHashCode) {
				lastRectTransformHashCode = rectTransformHashCode;

				m_orientedRect2D = null;
				var verticesDirty = softMaskRect.orientedRect2D.Overlaps(orientedRect2D);
				if (verticesDirty || lastVerticesDirty) {
					SetVerticesDirty();
				}
				lastVerticesDirty = verticesDirty;
			}
		}

#if UNITY_EDITOR
		protected override void OnValidate() {
			base.OnValidate();
		}

		protected override void Reset() {
			base.Reset();
		}
#endif

		private int lastRectTransformHashCode;
		private bool lastVerticesDirty;
	}
}
