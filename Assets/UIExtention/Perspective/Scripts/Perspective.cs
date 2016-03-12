using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Perspective : BaseMeshEffect {

	[SerializeField] [Range(0.0f, 179.0f)]
	private float m_FieldOfView = 60.0f;
	public float fieldOfView {
		get {
			return m_FieldOfView;
		}
		set {
			m_FieldOfView = value;
			m_Distance = float.NegativeInfinity;
			if (graphic != null) {
				graphic.SetVerticesDirty();
			}
		}
	}

	[SerializeField] [HideInInspector]
	private float m_Distance = float.NegativeInfinity;
	private float distance {
		get {
			if (float.IsNegativeInfinity(m_Distance)) {
				if (fieldOfView <= 0.0f) {
					m_Distance = 0.0f;
				} else {
					if (graphic.canvas != null) {
						var canvasRectTransform = graphic.canvas.GetComponent<RectTransform>();
						if (canvasRectTransform != null) {
							m_Distance = (canvasRectTransform.sizeDelta.y * 0.5f) / Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
						}
					}
				}
			}
			return m_Distance;
		}
	}

	[SerializeField]
	private bool m_PerspectiveCorrectShader = true;
	public bool perspectiveCorrectShader {
		get {
			return m_PerspectiveCorrectShader;
		}
		set {
			m_PerspectiveCorrectShader = value;
			if (graphic != null) {
				graphic.SetVerticesDirty();
			}
		}
	}

	[System.NonSerialized]
	private Quaternion m_lastLocalRotation = Quaternion.identity;

	private RectTransform m_rectTransform;
	public RectTransform rectTransform {
		get {
			if (m_rectTransform == null) {
				m_rectTransform = transform as RectTransform;
			}
			return m_rectTransform;
		}
	}

	public void LateUpdate() {
		var is_update = !m_lastLocalRotation.Equals(rectTransform.localRotation);
		if (is_update) {
			m_lastLocalRotation = rectTransform.localRotation;
			if (graphic != null) {
				graphic.SetVerticesDirty();
			}
		}
	}

	public override void ModifyMesh(VertexHelper vh) {
		if (!IsActive()) {
			return;
		}

		var distance = this.distance;
		if (distance <= 0.0f) {
			return;
		}

		var anchoredPosition3D = rectTransform.anchoredPosition3D;
		var localRotation = rectTransform.localRotation;

		UIVertex vertex = new UIVertex();
		for (int i = 0, i_max = vh.currentVertCount; i < i_max; ++i) {
			vh.PopulateUIVertex(ref vertex, i);
			var offset = localRotation * (vertex.position + anchoredPosition3D);
			var scale = 1.0e10f;
			if (-offset.z < distance) {
				scale = distance / (distance + offset.z);
			}
			vertex.position = new Vector3(vertex.position.x * scale, vertex.position.y * scale, vertex.position.z);
			if (perspectiveCorrectShader) {
				vertex.uv1 = new Vector2(scale, 0.0f);
			}
			vh.SetUIVertex(vertex, i);
		}
	}

	protected override void OnDidApplyAnimationProperties() {
		base.OnDidApplyAnimationProperties();
		fieldOfView = fieldOfView;
	}

#if UNITY_EDITOR
	protected override void OnValidate() {
		base.OnValidate();
		fieldOfView = fieldOfView;
	}
#endif
}
