// (C) 2016 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UIExtention {
	public struct OrientedRect2D {
		public OrientedRect2D(OrientedRect2D source) {
			this.position = source.position;
			this.extents = source.extents;
			this.rotation = source.rotation;
		}
		public OrientedRect2D(Rect rect, Quaternion rotation) : this(rect.center, rect.size, rotation) {
		}
		public OrientedRect2D(Vector2 position, Vector2 size, Quaternion rotation) {
			this.position = position;
			this.extents = size * 0.5f;
			this.rotation = rotation;
		}
		public OrientedRect2D(float x, float y, float width, float height, Quaternion rotation) {
			this.position = new Vector2(x, y);
			this.extents = new Vector2(width * 0.5f, height * 0.5f);
			this.rotation = rotation;
		}
		public OrientedRect2D(RectTransform rectTransform) {
			var rect = rectTransform.rect;
			this.position = (Vector2)rectTransform.position + Vector2.Scale(rect.center, rectTransform.lossyScale);
			this.extents = Vector2.Scale(rect.size, rectTransform.lossyScale) * 0.5f;
			this.rotation = rectTransform.rotation;
		}

		public Vector2 position;
		public Vector2 extents;
		public Quaternion rotation;

		public Vector2 center { get{return position;} set{position = value;} }
		public float x { get{return position.x;} set{position = new Vector2(value, position.y);} }
		public float y { get{return position.y;} set{position = new Vector2(position.x, value);} }
		public Vector2 size { get{return extents * 2.0f;} set{extents = value * 0.5f;} }
		public float height { get{return extents.y * 2.0f;} set{extents = new Vector2(extents.x, value * 0.5f);} }
		public float width { get{return extents.x * 2.0f;} set{extents = new Vector2(value * 0.5f, extents.y);} }
		public Vector2 max { get{return position + extents;} set{var m = min; position = new Vector2((value.x + m.x) * 0.5f, (value.y - m.y) * 0.5f); size = new Vector2(Mathf.Abs(value.x - m.x), Mathf.Abs(value.y - m.y));} }
		public Vector2 min { get{return position - extents;} set{var m = max; position = new Vector2((m.x + value.x) * 0.5f, (m.y - value.y) * 0.5f); size = new Vector2(Mathf.Abs(m.x - value.x), Mathf.Abs(m.y - value.y));} }

		public float xMax { get{return position.x + extents.x;} }
		public float xMin { get{return position.x - extents.x;} }
		public float yMax { get{return position.y + extents.y;} }
		public float yMin { get{return position.y - extents.y;} }

		public static OrientedRect2D MinMaxOrientedRect(float xmin, float ymin, float xmax, float ymax, Quaternion rotation) {
			return new OrientedRect2D((xmax + xmin) * 0.5f, (ymax + ymin) * 0.5f, Mathf.Abs(xmax - xmin), Mathf.Abs(ymax - ymin), rotation);
		}

		public bool Contains(Vector3 point) {
			var localPoint = Quaternion.Inverse(rotation) * point;
			if (extents.x < Mathf.Abs(localPoint.x - position.x)) {
				return false;
			} else if (extents.y < Mathf.Abs(localPoint.y - position.y)) {
				return false;
			}
			return true;
		}

		public bool Contains(Vector2 point) {
			return Contains((Vector3)point);
		}

		public bool Overlaps(OrientedRect2D other) {
			var distanceAxis = position - other.position;
			var thisUnitAxis = new AxisPack3(rotation);
			var thisExtentsAxis = new AxisPack3(extents, rotation);
			var otherUnitAxis = new AxisPack3(other.rotation);
			var otherExtentsAxis = new AxisPack3(other.extents, other.rotation);

			//this
			for (int i = 0, iMax = 2; i < iMax; ++i) {
				var splitAxis = thisUnitAxis[i];
				var distance = GetVectorLengthOfProjection(distanceAxis, splitAxis);
				distance -= this.extents[i];
				distance -= GetVectorLengthOfProjection(otherExtentsAxis, splitAxis);
				if (0.0f < distance) {
					//NoHit
					return false;
				}
			}
			//other
			for (int i = 0, iMax = 2; i < iMax; ++i) {
				var splitAxis = otherUnitAxis[i];
				var distance = GetVectorLengthOfProjection(distanceAxis, splitAxis);
				distance -= GetVectorLengthOfProjection(thisExtentsAxis, splitAxis);
				distance -= other.extents[i];
				if (0.0f < distance) {
					//NoHit
					return false;
				}
			}
			//3rd split axis
			for (int i = 0, iMax = 2; i < iMax; ++i) {
				for (int k = 0, kMax = 2; k < kMax; ++k) {
					var splitAxis = Vector3.Cross(thisUnitAxis[i], otherUnitAxis[k]);
					var distance = GetVectorLengthOfProjection(distanceAxis, splitAxis);
					distance -= GetVectorLengthOfProjection(thisExtentsAxis, splitAxis);
					distance -= GetVectorLengthOfProjection(otherExtentsAxis, splitAxis);
					if (0.0f < distance) {
						//NoHit
						return false;
					}
				}
			}

			//Hit
			return true;
		}

		private class AxisPack3 {
			private Vector3[] axis;

			public Vector3 this[int i] {
				set{this.axis[i] = value;}
				get{return this.axis[i];}
			}
			public Vector3 right {
				set{this.axis[0] = value;}
				get{return this.axis[0];}
			}
			public Vector3 up {
				set{this.axis[1] = value;}
				get{return this.axis[1];}
			}
			public Vector3 forward {
				set{this.axis[2] = value;}
				get{return this.axis[2];}
			}
			public AxisPack3() : this(Vector3.one, Quaternion.identity) {}
			public AxisPack3(Vector3 scale) : this(scale, Quaternion.identity) {}
			public AxisPack3(Quaternion rotation) : this(Vector3.one, rotation) {}
			public AxisPack3(Vector3 scale, Quaternion rotation) {
				this.axis = new[]{Vector3.right * scale.x, Vector3.up * scale.y, Vector3.forward * scale.z}.Select(x=>rotation * x).ToArray();
			}
			public AxisPack3(Vector3 right, Vector3 up, Vector3 forward) {
				this.axis = new[]{right, up, forward}.ToArray();
			}
		}

		private static float GetVectorLengthOfProjection(Vector3 src, Vector3 projection) {
			return Mathf.Abs(Vector3.Dot(projection, src));
		}

		private static float GetVectorLengthOfProjection(AxisPack3 axis, Vector3 projection) {
			float result = Enumerable.Range(0, 3)
									.Select(x=>GetVectorLengthOfProjection(projection, axis[x]))
									.Sum();
			return result;
		}

	}
}