using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UIExtention {
	public class AroundLayoutGroup : LayoutGroup {

		public enum Path {
			Circle,
			RectangleCLV, //Constant Linear Velocity
			RectangleCAV, //Constant Angular Velocity
			RectangleExtensionCircle, //Extension Circle
		}
		[SerializeField] private Path m_Path = Path.Circle;
		public Path path {get{return m_Path;} set{if (m_Path != value) {m_Path = value; SetDirty();}}}

		[SerializeField] [MinMaxSlider(0.0f, 1.0f)] private Vector2 m_Range = new Vector2(0.0f, 1.0f);
		public Vector2 range {get{return m_Range;} set{if (m_Range != value) {m_Range = value; SetDirty();}}}

		[SerializeField] private bool m_Justify = false;
		public bool justify {get{return m_Justify;} set{if (m_Justify != value) {m_Justify = value; SetDirty();}}}

		[SerializeField] private float m_Offset = 0.0f;
		public float offset {get{return m_Offset;} set{if (m_Offset != value) {m_Offset = value; SetDirty();}}}

		public enum ChildAnchor {
			Nomodify,
			Center,
			Inscribe,
			Circumscribe,
		}
		public new ChildAnchor childAlignment {get{return (ChildAnchor)base.childAlignment;} set{if (base.childAlignment != (TextAnchor)value) {base.childAlignment = (TextAnchor)value; SetDirty();}}}

		protected AroundLayoutGroup() {
		}

		public override void CalculateLayoutInputVertical() {
			SetLayoutInputForAxis(-1.0f, -1.0f, -1.0f, 0);
			SetLayoutInputForAxis(-1.0f, -1.0f, -1.0f, 1);
		}

		public override void SetLayoutHorizontal() {
			setLayout();
		}

		public override void SetLayoutVertical() {
			setLayout();
		}

		private void setLayout() {
			switch (m_Path) {
			case Path.RectangleCLV:
				setLayoutRectangleCLV();
				break;
			case Path.RectangleCAV:
				setLayoutRectangleCAV();
				break;
			case Path.RectangleExtensionCircle:
				setLayoutRectangleExtensionCircle();
				break;
			case Path.Circle:
			default:
				setLayoutCircle();
				break;
			}
		}

		private void setLayoutCircle() {
			layoutBase((progress, sizeHalf)=>{
				var radian = progress * Mathf.PI * 2.0f;
				return new Vector2(Mathf.Cos(radian) * sizeHalf.x
								, Mathf.Sin(radian) * sizeHalf.y
								);
			});
		}

		private void setLayoutRectangleCLV() {
			layoutBase((progress, sizeHalf)=>{
				var total = (sizeHalf.x + sizeHalf.y) * 4.0f;
				progress = Mathf.Repeat(progress, 1.0f) * total;
				if (progress < sizeHalf.y) {
					return new Vector2(sizeHalf.x, progress);
				}
				progress -= sizeHalf.y;
				if (progress < (sizeHalf.x * 2.0f)) {
					return new Vector2(sizeHalf.x - progress, sizeHalf.y);
				}
				progress -= sizeHalf.x * 2.0f;
				if (progress < (sizeHalf.y * 2.0f)) {
					return new Vector2(-sizeHalf.x, sizeHalf.y - progress);
				}
				progress -= sizeHalf.y * 2.0f;
				if (progress < (sizeHalf.x * 2.0f)) {
					return new Vector2(progress - sizeHalf.x, -sizeHalf.y);
				}
				progress -= sizeHalf.x * 2.0f;
				return new Vector2(sizeHalf.x, progress - sizeHalf.y);
			});
		}

		private void setLayoutRectangleCAV() {
			layoutBase((progress, sizeHalf)=>{
				progress = Mathf.Repeat(progress, 1.0f);
				var pingPongProgress = Mathf.PingPong(progress, 0.25f);
				var pingPongRadian = pingPongProgress * Mathf.PI * 2.0f;
				var pingPongRay = new Vector2(Mathf.Cos(pingPongRadian), Mathf.Sin(pingPongRadian));
				var fitPoint = new Vector2(sizeHalf.y * pingPongRay.x / pingPongRay.y, sizeHalf.y);
				if (sizeHalf.x < fitPoint.x) {
					fitPoint = new Vector2(sizeHalf.x, sizeHalf.x * pingPongRay.y / pingPongRay.x);
				}
				return new Vector2((((progress <= 0.25f) || (0.75f <= progress))? fitPoint.x: -fitPoint.x)
									, ((progress <= 0.5f)? fitPoint.y: -fitPoint.y)
									);
			});
		}

		private void setLayoutRectangleExtensionCircle() {
			layoutBase((progress, sizeHalf)=>{
				var radian = progress * Mathf.PI * 2.0f;
				var distortion = 1.0f / Mathf.Cos(Mathf.PingPong(radian, Mathf.PI * 0.25f));
				return new Vector2(Mathf.Cos(radian) * sizeHalf.x * distortion
								, Mathf.Sin(radian) * sizeHalf.y * distortion
								);
			});
		}

		private void layoutBase(System.Func<float, Vector2, Vector2> layout) {
			var rectTransform = GetComponent<RectTransform>();
			var sizeHalf = new Vector2(rectTransform.rect.width * 0.5f, rectTransform.rect.height * 0.5f);
			var scribeSizeHalf = 0.0f;
			if (childAlignment == ChildAnchor.Inscribe) {
				scribeSizeHalf = 0.5f;
			} else if (childAlignment == ChildAnchor.Circumscribe) {
				scribeSizeHalf = -0.5f;
			}

			var rectChildrenCount = rectChildren.Count;
			if (0 < rectChildrenCount) {
				var countToProgress = (range.y - range.x) / (float)(rectChildrenCount + (((1 <= rectChildrenCount) && justify)? -1: 0));
				for (int i = 0, i_max = rectChildrenCount; i < i_max; ++i) {
					var child = rectChildren[i];

					var childRectTransform = child.GetComponent<RectTransform>();
					var progress = (float)i * countToProgress + (offset + range.x);
					childRectTransform.anchoredPosition = layout(progress, sizeHalf);

					if (childAlignment == ChildAnchor.Nomodify) {
						m_Tracker.Add(this, child, DrivenTransformProperties.AnchoredPosition);
					} else {
						if (0.0f < scribeSizeHalf) {
							childRectTransform.pivot = layout(progress, new Vector2(scribeSizeHalf, scribeSizeHalf)) + new Vector2(0.5f, 0.5f);
						} else if (scribeSizeHalf < 0.0f) {
							childRectTransform.pivot = new Vector2(0.5f, 0.5f) - layout(progress, new Vector2(-scribeSizeHalf, -scribeSizeHalf));
						} else {
							childRectTransform.pivot = new Vector2(0.5f, 0.5f);
						}

						m_Tracker.Add(this, child, DrivenTransformProperties.AnchoredPosition
												| DrivenTransformProperties.Pivot);
					}
				}
			}
		}
	}
}
