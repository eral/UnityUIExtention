using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UIExtention {
	public class CornerColors : BaseMeshEffect {

		[SerializeField] private Color m_UpperLeftColor = Color.white;
		public Color upperLeftColor {get{return m_UpperLeftColor;} set{m_UpperLeftColor = value;}}

		[SerializeField] private Color m_UpperRightColor = Color.white;
		public Color upperRightColor {get{return m_UpperRightColor;} set{m_UpperRightColor = value;}}

		[SerializeField] private Color m_LowerLeftColor = Color.white;
		public Color lowerLeftColor {get{return m_LowerLeftColor;} set{m_LowerLeftColor = value;}}

		[SerializeField] private Color m_LowerRightColor = Color.white;
		public Color lowerRightColor {get{return m_LowerRightColor;} set{m_LowerRightColor = value;}}

		public RectTransform rectTransform {
			get {
				if (m_rectTransform == null) {
					m_rectTransform = transform as RectTransform;
				}
				return m_rectTransform;
			}
		}
		private RectTransform m_rectTransform;

		public override void ModifyMesh(VertexHelper vh) {
			if (!IsActive()) {
				return;
			}

			Vector3[] localCorners = new Vector3[4];
			rectTransform.GetLocalCorners(localCorners);

			var vertices = new List<UIVertex>();
			vh.GetUIVertexStream(vertices);
			localCorners[3] = localCorners[2] - localCorners[0];
			localCorners[1] = new Vector3(((0.0f < localCorners[3].x)? 1.0f / localCorners[3].x: 0.0f)
										, ((0.0f < localCorners[3].y)? 1.0f / localCorners[3].y: 0.0f)
										, ((0.0f < localCorners[3].z)? 1.0f / localCorners[3].z: 0.0f));

			var lowerLeftColor32 = (Color32)lowerLeftColor;
			var lowerRightColor32 = (Color32)lowerRightColor;
			var upperLeftColor32 = (Color32)upperLeftColor;
			var upperRightColor32 = (Color32)upperRightColor;

			UIVertex vertex = new UIVertex();
			for (int i = 0, i_max = vh.currentVertCount; i < i_max; ++i) {
				vh.PopulateUIVertex(ref vertex, i);
				var position = Vector3.Scale(vertex.position - localCorners[0], localCorners[1]);

				vertex.color = Color32.Lerp(Color32.Lerp(lowerLeftColor32, lowerRightColor32, position.x)
											, Color32.Lerp(upperLeftColor32, upperRightColor32, position.x)
											, position.y
											);
				vh.SetUIVertex(vertex, i);
			}
		}
	}
}
