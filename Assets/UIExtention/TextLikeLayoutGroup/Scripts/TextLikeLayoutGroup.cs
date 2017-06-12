// (C) 2017 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UIExtention {
	public class TextLikeLayoutGroup : LayoutGroup {

		[SerializeField] private float m_ElementSpacing = 0.0f;
		public float elementSpacing {get{return m_ElementSpacing;} set{if (m_ElementSpacing != value) {m_ElementSpacing = value; SetDirty();}}}

		[SerializeField] private float m_LineSpacing = 0.0f;
		public float LineSpacing {get{return m_LineSpacing;} set{if (m_LineSpacing != value) {m_LineSpacing = value; SetDirty();}}}

		public enum WritingMode {
			HorizontalLRTB,
			HorizontalRLTB,
			HorizontalLRBT,
			HorizontalRLBT,
			VerticalTBLR,
			VerticalTBRL,
			VerticalBTLR,
			VerticalBTRL,
		}
		[SerializeField] private WritingMode m_WritingMode = WritingMode.HorizontalLRTB;
		public WritingMode writingMode {get{return m_WritingMode;} set{if (m_WritingMode != value) {m_WritingMode = value; SetDirty();}}}

		public enum WritingDirection {
			Horizontal,
			Vertical,
		}
		private WritingDirection writingDirection {get{
			return (WritingDirection)((int)writingMode >> 2);
		}}

		public enum HorizontalDirection {
			LR,
			RL,
		}
		private HorizontalDirection horizontalDirection {get{
			return (HorizontalDirection)((int)writingMode & 0x01);
		}}

		public enum VerticalDirection {
			TB,
			BT,
		}
		private VerticalDirection verticalDirection {get{
			return (VerticalDirection)(((int)writingMode >> 1) & 0x01);
		}}

		public enum ChildAnchor {
			LeftTop,
			Center,
			RightBottom,
			Pivot,
		}
		public new ChildAnchor childAlignment {get{return (ChildAnchor)base.childAlignment;} set{if (base.childAlignment != (TextAnchor)value) {base.childAlignment = (TextAnchor)value; SetDirty();}}}

		protected TextLikeLayoutGroup() {
		}

		public override void CalculateLayoutInputVertical() {
			float minWidth;
			float preferredWidth;
			const float kFlexibleWidth = -1.0f;
			float minHeight;
			float preferredHeight;
			const float kFlexibleHeight = -1.0f;
			if (writingDirection == WritingDirection.Horizontal) {
				minWidth = rectTransform.rect.width;
				preferredWidth = rectTransform.rect.width;
				minHeight = GetMinSize().y;
				preferredHeight = GetPreferredHeightForHorizontalMode();
			} else {
				minWidth = GetMinSize().x;
				preferredWidth = GetPreferredWidthForVerticalMode();
				minHeight = rectTransform.rect.height;
				preferredHeight = rectTransform.rect.height;
			}
			SetLayoutInputForAxis(minWidth, preferredWidth, kFlexibleWidth, 0);
			SetLayoutInputForAxis(minHeight, preferredHeight, kFlexibleHeight, 1);
		}

		public override void SetLayoutHorizontal() {
			SetLayout();
		}

		public override void SetLayoutVertical() {
			SetLayout();
		}

#if UNITY_EDITOR
		protected override void OnValidate() {
			base.OnValidate();
		}

		protected override void Reset() {
			base.Reset();
			childAlignment = ChildAnchor.Center;
		}
#endif

		private Vector2 GetMinSize() {
			var result = new Vector2(-1.0f, -1.0f);
			foreach (var rectChild in rectChildren) {
				var rectChildSize = rectChild.rect.size;
				result.x = Mathf.Max(result.x, rectChildSize.x);
				result.y = Mathf.Max(result.y, rectChildSize.y);
			}
			return result;
		}

		private float GetPreferredHeightForHorizontalMode() {
			var rectTransformSize = rectTransform.rect.size;

			Vector2 lineOffset = new Vector2();
			if (horizontalDirection == HorizontalDirection.LR) {
				lineOffset.x -= rectTransformSize.x * rectTransform.pivot.x;
				lineOffset.x += padding.left;
			} else {
				lineOffset.x += rectTransformSize.x * (1.0f - rectTransform.pivot.x);
				lineOffset.x -= padding.right;
			}
			if (verticalDirection == VerticalDirection.TB) {
				lineOffset.y += rectTransformSize.y * (1.0f - rectTransform.pivot.y);
				lineOffset.y -= padding.top;
			} else {
				lineOffset.y -= rectTransformSize.y * rectTransform.pivot.y;
				lineOffset.y += padding.bottom;
			}
			var result = lineOffset.y;

			var childIndex = 0;
			var rectChildrenCount = rectChildren.Count;
			while (childIndex < rectChildrenCount) {
				//ライン矩形算出
				var lineRect = new Rect(0.0f, 0.0f, padding.horizontal, 0.0f);
				var childIndexLineMax = childIndex;
				while (childIndexLineMax < rectChildrenCount) {
					var child = rectChildren[childIndexLineMax];
					var childRect = child.rect;
					if (0.0f < lineRect.height) {
						//既にライン矩形に1つ以上入っている(垂直サイズが0以上なら既に入ってる)なら
						if ((rectTransformSize.x < (lineRect.width + childRect.width))) {
							//現在の子を含めると食み出すなら
							//次の行へ
							break;
						} else {
							//要素巻スペース空ける
							lineRect.width += elementSpacing;
						}
					}
					lineRect.width += childRect.width;
					if (childAlignment == ChildAnchor.Pivot) {
						lineRect.yMin = Mathf.Min(lineRect.yMin, childRect.yMin);
						lineRect.yMax = Mathf.Max(lineRect.yMax, childRect.yMax);
					} else {
						lineRect.height = Mathf.Max(lineRect.height, childRect.height);
					}
					childIndexLineMax++;
				}
				childIndex = childIndexLineMax;
				if (verticalDirection == VerticalDirection.TB) {
					lineOffset.y -= lineRect.height;
					lineOffset.y -= LineSpacing;
				} else {
					lineOffset.y += lineRect.height;
					lineOffset.y += LineSpacing;
				}
			}

			if (verticalDirection == VerticalDirection.TB) {
				lineOffset.y += LineSpacing;
			} else {
				lineOffset.y -= LineSpacing;
			}
			result = Mathf.Abs(result - lineOffset.y);
			result += padding.top;
			result += padding.bottom;
			return result;
		}

		private float GetPreferredWidthForVerticalMode() {
			var rectTransformSize = rectTransform.rect.size;

			Vector2 lineOffset = new Vector2();
			if (horizontalDirection == HorizontalDirection.LR) {
				lineOffset.x -= rectTransformSize.x * rectTransform.pivot.x;
				lineOffset.x += padding.left;
			} else {
				lineOffset.x += rectTransformSize.x * (1.0f - rectTransform.pivot.x);
				lineOffset.x -= padding.right;
			}
			if (verticalDirection == VerticalDirection.TB) {
				lineOffset.y += rectTransformSize.y * (1.0f - rectTransform.pivot.y);
				lineOffset.y -= padding.top;
			} else {
				lineOffset.y -= rectTransformSize.y * rectTransform.pivot.y;
				lineOffset.y += padding.bottom;
			}
			var result = lineOffset.x;

			var childIndex = 0;
			var rectChildrenCount = rectChildren.Count;
			while (childIndex < rectChildrenCount) {
				//ライン矩形算出
				var lineRect = new Rect(0.0f, 0.0f, 0.0f, padding.vertical);
				var childIndexLineMax = childIndex;
				while (childIndexLineMax < rectChildrenCount) {
					var child = rectChildren[childIndexLineMax];
					var childRect = child.rect;
					if (0.0f < lineRect.width) {
						//既にライン矩形に1つ以上入っている(垂直サイズが0以上なら既に入ってる)なら
						if ((rectTransformSize.y < (lineRect.height + childRect.height))) {
							//現在の子を含めると食み出すなら
							//次の行へ
							break;
						} else {
							//要素巻スペース空ける
							lineRect.height += elementSpacing;
						}
					}
					if (childAlignment == ChildAnchor.Pivot) {
						lineRect.xMin = Mathf.Min(lineRect.xMin, childRect.xMin);
						lineRect.xMax = Mathf.Max(lineRect.xMax, childRect.xMax);
					} else {
						lineRect.width = Mathf.Max(lineRect.width, childRect.width);
					}
					lineRect.height += childRect.height;
					childIndexLineMax++;
				}
				childIndex = childIndexLineMax;
				if (horizontalDirection != HorizontalDirection.LR) {
					lineOffset.x -= lineRect.width;
					lineOffset.x -= LineSpacing;
				} else {
					lineOffset.x += lineRect.width;
					lineOffset.x += LineSpacing;
				}
			}

			if (horizontalDirection != HorizontalDirection.LR) {
				lineOffset.x += LineSpacing;
			} else {
				lineOffset.x -= LineSpacing;
			}
			result = Mathf.Abs(result - lineOffset.x);
			result += padding.left;
			result += padding.right;
			return result;
		}

		private void SetLayout() {
			var rectChildrenCount = rectChildren.Count;
			if (0 < rectChildrenCount) {
				switch (writingDirection) {
				case WritingDirection.Horizontal:
					SetLayoutForHorizontalMode();
					break;
				case WritingDirection.Vertical:
					SetLayoutForVerticalMode();
					break;
				}
			}
		}

		private void SetLayoutForHorizontalMode() {
			var rectTransformSize = rectTransform.rect.size;

			Vector2 lineOffset = new Vector2();
			if (horizontalDirection == HorizontalDirection.LR) {
				lineOffset.x -= rectTransformSize.x * rectTransform.pivot.x;
				lineOffset.x += padding.left;
			} else {
				lineOffset.x += rectTransformSize.x * (1.0f - rectTransform.pivot.x);
				lineOffset.x -= padding.right;
			}
			if (verticalDirection == VerticalDirection.TB) {
				lineOffset.y += rectTransformSize.y * (1.0f - rectTransform.pivot.y);
				lineOffset.y -= padding.top;
			} else {
				lineOffset.y -= rectTransformSize.y * rectTransform.pivot.y;
				lineOffset.y += padding.bottom;
			}

			var childIndex = 0;
			var rectChildrenCount = rectChildren.Count;
			while (childIndex < rectChildrenCount) {
				//ライン矩形算出
				var lineRect = new Rect(0.0f, 0.0f, padding.horizontal, 0.0f);
				var childIndexLineMax = childIndex;
				while (childIndexLineMax < rectChildrenCount) {
					var child = rectChildren[childIndexLineMax];
					var childRect = child.rect;
					if (0.0f < lineRect.height) {
						//既にライン矩形に1つ以上入っている(垂直サイズが0以上なら既に入ってる)なら
						if ((rectTransformSize.x < (lineRect.width + childRect.width))) {
							//現在の子を含めると食み出すなら
							//次の行へ
							break;
						} else {
							//要素巻スペース空ける
							lineRect.width += elementSpacing;
						}
					}
					lineRect.width += childRect.width;
					if (childAlignment == ChildAnchor.Pivot) {
						lineRect.yMin = Mathf.Min(lineRect.yMin, childRect.yMin);
						lineRect.yMax = Mathf.Max(lineRect.yMax, childRect.yMax);
					} else {
						lineRect.height = Mathf.Max(lineRect.height, childRect.height);
					}
					childIndexLineMax++;
				}
				//位置配置
				if (verticalDirection != VerticalDirection.TB) {
					lineOffset.y += lineRect.height;
				}
				Vector2 elementOffset = lineOffset;
				if (childAlignment == ChildAnchor.Pivot) {
					elementOffset.y -= lineRect.yMax;
				}
				while (childIndex < childIndexLineMax) {
					var child = rectChildren[childIndex++];
					var childRect = child.rect;

					if (horizontalDirection != HorizontalDirection.LR) {
						elementOffset.x -= child.rect.width;
					}

					var childOffsetMin = elementOffset;
					childOffsetMin.x += child.pivot.x * child.rect.width;
					
					switch (childAlignment) {
					case ChildAnchor.LeftTop: childOffsetMin.y -= (1.0f - child.pivot.y) * child.rect.height; break;
					case ChildAnchor.Center: childOffsetMin.y -= (lineRect.height - child.rect.height) * 0.5f; goto case ChildAnchor.LeftTop;
					case ChildAnchor.RightBottom: childOffsetMin.y -= (lineRect.height - child.rect.height); goto case ChildAnchor.LeftTop;
					default: break;
					}
					child.localPosition = childOffsetMin;

					if (horizontalDirection == HorizontalDirection.LR) {
						elementOffset.x += child.rect.width;
						elementOffset.x += elementSpacing;
					} else {
						elementOffset.x -= elementSpacing;
					}

					m_Tracker.Add(this, child, DrivenTransformProperties.AnchoredPosition);
				}
				if (verticalDirection == VerticalDirection.TB) {
					lineOffset.y -= lineRect.height;
					lineOffset.y -= LineSpacing;
				} else {
					lineOffset.y += LineSpacing;
				}
			}
		}

		private void SetLayoutForVerticalMode() {
			var rectTransformSize = rectTransform.rect.size;

			Vector2 lineOffset = new Vector2();
			if (horizontalDirection == HorizontalDirection.LR) {
				lineOffset.x -= rectTransformSize.x * rectTransform.pivot.x;
				lineOffset.x += padding.left;
			} else {
				lineOffset.x += rectTransformSize.x * (1.0f - rectTransform.pivot.x);
				lineOffset.x -= padding.right;
			}
			if (verticalDirection == VerticalDirection.TB) {
				lineOffset.y += rectTransformSize.y * (1.0f - rectTransform.pivot.y);
				lineOffset.y -= padding.top;
			} else {
				lineOffset.y -= rectTransformSize.y * rectTransform.pivot.y;
				lineOffset.y += padding.bottom;
			}

			var childIndex = 0;
			var rectChildrenCount = rectChildren.Count;
			while (childIndex < rectChildrenCount) {
				//ライン矩形算出
				var lineRect = new Rect(0.0f, 0.0f, 0.0f, padding.vertical);
				var childIndexLineMax = childIndex;
				while (childIndexLineMax < rectChildrenCount) {
					var child = rectChildren[childIndexLineMax];
					var childRect = child.rect;
					if (0.0f < lineRect.width) {
						//既にライン矩形に1つ以上入っている(垂直サイズが0以上なら既に入ってる)なら
						if ((rectTransformSize.y < (lineRect.height + childRect.height))) {
							//現在の子を含めると食み出すなら
							//次の行へ
							break;
						} else {
							//要素巻スペース空ける
							lineRect.height += elementSpacing;
						}
					}
					if (childAlignment == ChildAnchor.Pivot) {
						lineRect.xMin = Mathf.Min(lineRect.xMin, childRect.xMin);
						lineRect.xMax = Mathf.Max(lineRect.xMax, childRect.xMax);
					} else {
						lineRect.width = Mathf.Max(lineRect.width, childRect.width);
					}
					lineRect.height += childRect.height;
					childIndexLineMax++;
				}
				//位置配置
				if (horizontalDirection == HorizontalDirection.LR) {
					lineOffset.x += lineRect.width;
				}
				Vector2 elementOffset = lineOffset;
				if (childAlignment == ChildAnchor.Pivot) {
					elementOffset.x -= lineRect.xMax;
				}
				while (childIndex < childIndexLineMax) {
					var child = rectChildren[childIndex++];
					var childRect = child.rect;

					if (verticalDirection != VerticalDirection.TB) {
						elementOffset.y += child.rect.height;
					}

					var childOffsetMin = elementOffset;
					childOffsetMin.y -= (1.0f - child.pivot.y) * child.rect.height;
					
					switch (childAlignment) {
					case ChildAnchor.LeftTop: childOffsetMin.x -= (lineRect.width - child.rect.width); goto case ChildAnchor.RightBottom;
					case ChildAnchor.Center: childOffsetMin.x -= (lineRect.width - child.rect.width) * 0.5f; goto case ChildAnchor.RightBottom;
					case ChildAnchor.RightBottom: childOffsetMin.x -= (1.0f - child.pivot.x) * child.rect.width; break;
					default: break;
					}
					child.localPosition = childOffsetMin;

					if (verticalDirection == VerticalDirection.TB) {
						elementOffset.y -= child.rect.height;
						elementOffset.y -= elementSpacing;
					} else {
						elementOffset.y += elementSpacing;
					}

					m_Tracker.Add(this, child, DrivenTransformProperties.AnchoredPosition);
				}
				if (horizontalDirection != HorizontalDirection.LR) {
					lineOffset.x -= lineRect.width;
					lineOffset.x -= LineSpacing;
				} else {
					lineOffset.x += LineSpacing;
				}
			}
		}

		[UnityEditor.MenuItem("test/test")]
		public static void test() {
			var textLikeLayoutGroup = GameObject.FindObjectOfType<TextLikeLayoutGroup>();
			if (textLikeLayoutGroup) {
				foreach (RectTransform child in textLikeLayoutGroup.transform) {
					child.pivot = new Vector2(Random.Range(0, 3)/2.0f, Random.Range(0, 3)/2.0f);
					child.sizeDelta = new Vector2(Random.Range(2,6)*30.0f, Random.Range(2,6)*30.0f);
					var image = child.GetComponent<Image>();
					image.color = Random.ColorHSV(0.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f);
				}
			}
		}

		[UnityEditor.MenuItem("test/test2")]
		public static void test2() {
			var textLikeLayoutGroup = GameObject.FindObjectOfType<TextLikeLayoutGroup>();
			if (textLikeLayoutGroup) {
				var i = 0;
				foreach (RectTransform child in textLikeLayoutGroup.transform) {
					child.GetComponentInChildren<Text>().text = (i++).ToString();
				}
			}
		}
	}
}
