using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UIExtention {
	public static class Utility {
		public static List<int> Triangulation(List<Vector2> vertices) {
			var outerVertices = GetVerticesOfOuterTriangle(vertices);
			var triangles = new List<Triangle>();
			triangles.Add(new Triangle(new[]{-1, -2, -3}, vertices, outerVertices));

			var doubtSides = new Stack<Side>();
			for (int i = 0, iMax = vertices.Count; i < iMax; ++i) {
				PlungeVertex(triangles, doubtSides, i, vertices, outerVertices);
			}

#if UNITY_EDITOR
			var oldHandlesColor = UnityEditor.Handles.color;
			foreach (var triangle in triangles) {
				triangle.DrawLine(new Color(0.0f, 0.75f, 0.0f, 0.75f)
								, new Color(0.0f, 0.0f, 0.0f, 0.25f));
			}
			UnityEditor.Handles.color = oldHandlesColor;
			for (int i = 0, iMax = outerVertices.Length; i < iMax; ++i) {
				var vertex = outerVertices[i];
				UnityEditor.Handles.Label(vertex, (-i-1).ToString());
			}
#endif

#if UNITY_EDITOR
			UnityEditor.Handles.color = new Color(1.0f, 1.0f, 1.0f, 0.9f);
#endif
			RemoveOuterTriangle(triangles, doubtSides, vertices, outerVertices);
#if UNITY_EDITOR
			UnityEditor.Handles.color = oldHandlesColor;
#endif

			var result = triangles.SelectMany(x=>x.indices)
								.ToList();
			return result;
		}

		private class Triangle {
			public int[] indices; //<=0:Index of vertices, 0<: One's complement index of outerVertices
			public Vector2[] vertices;
			private Vector2 center;
			private float sqrRadius;

			public Triangle(int[] indices, List<Vector2> vertices, Vector2[] outerVertices) {
				this.indices = indices;
				this.vertices = indices.Select(x=>GetVertices(x, vertices, outerVertices)).ToArray();
				CacheCircumscribedCircle();
			}
			public bool Valid() {
				return !(float.IsInfinity(sqrRadius) || float.IsNaN(sqrRadius));
			}
			public bool Contains(Vector2 point) {
				return ContainsInConvexHull(vertices, point);
			}
			public bool ContainsInCircumscribedCircle(Vector2 point) {
				var result = (center - point).sqrMagnitude <= sqrRadius;
				return result;
			}
			public bool ContainsInCircumscribedCircleWithoutEdge(Vector2 point) {
				var result = (center - point).sqrMagnitude < sqrRadius;
				return result;
			}
			public bool HasSide(Side side) {
				return indices.Length == indices.Concat(side.indices).Distinct().Count();
			}
			public int GetOtherIndex(Side side) {
				return indices.Except(side.indices).First();
			}
			private void CacheCircumscribedCircle() {
				center.y = center.x = 1.0f / (2.0f * ((vertices[1].x - vertices[0].x) * (vertices[2].y - vertices[0].y) - (vertices[1].y - vertices[0].y) * (vertices[2].x - vertices[0].x)));
				center.x *= ((vertices[2].y - vertices[0].y) * (vertices[1].x * vertices[1].x - vertices[0].x * vertices[0].x + vertices[1].y * vertices[1].y - vertices[0].y * vertices[0].y) + (vertices[0].y - vertices[1].y) * (vertices[2].x * vertices[2].x - vertices[0].x * vertices[0].x + vertices[2].y * vertices[2].y - vertices[0].y * vertices[0].y));
				center.y *= ((vertices[0].x - vertices[2].x) * (vertices[1].x * vertices[1].x - vertices[0].x * vertices[0].x + vertices[1].y * vertices[1].y - vertices[0].y * vertices[0].y) + (vertices[1].x - vertices[0].x) * (vertices[2].x * vertices[2].x - vertices[0].x * vertices[0].x + vertices[2].y * vertices[2].y - vertices[0].y * vertices[0].y));
				sqrRadius = (vertices[0] - center).sqrMagnitude;
			}
#if UNITY_EDITOR
			public void DrawLine(Color lineColor, Color discColor) {
				UnityEditor.Handles.color = lineColor;
				UnityEditor.Handles.DrawLine(vertices[0], vertices[1]);
				UnityEditor.Handles.DrawLine(vertices[1], vertices[2]);
				UnityEditor.Handles.DrawLine(vertices[2], vertices[0]);
				if (0.0f < discColor.a) {
					UnityEditor.Handles.color = discColor;
					UnityEditor.Handles.DrawWireDisc(center, Vector3.back, Mathf.Sqrt(sqrRadius));
				}
			}
#endif
		}

		private class Side {
			public int[] indices; //<=0:Index of vertices, 0<: One's complement index of outerVertices
			public Vector2[] vertices;

			public Side(int[] indices, List<Vector2> vertices, Vector2[] outerVertices) {
				this.indices = indices;
				this.vertices = indices.Select(x=>GetVertices(x, vertices, outerVertices)).ToArray();
			}
		}

		private static Vector2 GetVertices(int index, List<Vector2> vertices, Vector2[] outerVertices) {
			return ((0 <= index)? vertices[index]: outerVertices[-index-1]);
		}

		private static Vector2[] GetVerticesOfOuterTriangle(List<Vector2> vertices) {
			var minX = vertices[0].x;
			var minY = vertices[0].y;
			var maxX = vertices[0].x;
			var maxY = vertices[0].y;
			foreach (var point in vertices.Skip(1)) {
				if (point.x < minX) minX = point.x;
				if (point.y < minY) minY = point.y;
				if (maxX < point.x) maxX = point.x;
				if (maxY < point.y) maxY = point.y;
			}
			var result = new Vector2[3];
			result[0].x = maxX;
			result[0].y = maxY;
			var diagonalLength = (result[0] - new Vector2(minX, minY)).magnitude;
			var sideLength = diagonalLength * 1.41422f;
			result[1].x = maxX;
			result[1].y = maxY - sideLength;
			result[2].x = maxX - sideLength;
			result[2].y = maxY;
			return result;
		}

		private static void PlungeVertex(List<Triangle> triangles, Stack<Side> doubtSides, int verticesIndex, List<Vector2> vertices, Vector2[] outerVertices) {
			var vertex = vertices[verticesIndex];
			var trianglesindices = triangles.Select((x,i)=>new{Element = x, Index = i})
											.Where(x=>x.Element.Contains(vertex))
											.Select(x=>x.Index)
											.Reverse()
											.ToArray();
			foreach (var trianglesIndex in trianglesindices) {
				var removeTriangle = triangles[trianglesIndex];
				triangles.RemoveAt(trianglesIndex);
				for (int i = 0, iMax = removeTriangle.indices.Length; i < iMax; ++i) {
					var indices = new[]{removeTriangle.indices[i]
														, removeTriangle.indices[((i < removeTriangle.indices.Length - 1)? i + 1: 0)]
														, verticesIndex
														};
					var addTriangle = new Triangle(indices, vertices, outerVertices);
					if (addTriangle.Valid()) {
						triangles.Add(addTriangle);
					}
					var doubtSide = new Side(new[]{indices[0], indices[1]}, vertices, outerVertices);
					if (!doubtSides.Contains(doubtSide, new DoubtSideEqualityComparer())) {
						doubtSides.Push(doubtSide);
					}
				}
			}
			while (0 < doubtSides.Count()) {
				var removeSide = doubtSides.Pop();
				var suspectTriangles = triangles.Where(x=>x.HasSide(removeSide)).ToArray();
				if (suspectTriangles.Length == 2) {
					var otherIndex = suspectTriangles[1].GetOtherIndex(removeSide);
					var otherVertex = GetVertices(otherIndex, vertices, outerVertices);
					var flip = suspectTriangles[0].ContainsInCircumscribedCircleWithoutEdge(otherVertex);
					if (flip) {
						triangles.Remove(suspectTriangles[0]);
						triangles.Remove(suspectTriangles[1]);
						var moreOtherIndex = suspectTriangles[0].GetOtherIndex(removeSide);
						triangles.Add(new Triangle(new[]{otherIndex, moreOtherIndex, removeSide.indices[0]}, vertices, outerVertices));
						triangles.Add(new Triangle(new[]{otherIndex, moreOtherIndex, removeSide.indices[1]}, vertices, outerVertices));
						doubtSides.Push(new Side(new[]{otherIndex, removeSide.indices[0]}, vertices, outerVertices));
						doubtSides.Push(new Side(new[]{otherIndex, removeSide.indices[1]}, vertices, outerVertices));
						doubtSides.Push(new Side(new[]{moreOtherIndex, removeSide.indices[0]}, vertices, outerVertices));
						doubtSides.Push(new Side(new[]{moreOtherIndex, removeSide.indices[1]}, vertices, outerVertices));
					}
				}
			}
		}

		private class DoubtSideEqualityComparer : IEqualityComparer<Side> {
			public bool Equals(Side x, Side y) {
				return x.indices.All(z=>y.indices.Contains(z));
			}
			public int GetHashCode(Side obj) {
				return obj.indices.Aggregate(0X01F11FE0, (x,y)=>x ^ y);
			}
		}

		private static void RemoveOuterTriangle(List<Triangle> triangles, Stack<Side> doubtSides, List<Vector2> vertices, Vector2[] outerVertices) {
			var removeSides = triangles.Where(x=>x.indices.Where(y=>y < 0).Take(2).Count() == 1)
										.Select(x=>x.indices.OrderBy(y=>y).ToArray())
										.SelectMany(x=>new[]{new[]{x[0], x[1]}, new[]{x[0], x[2]}})
										.Distinct()
										.Select(x=>new Side(x, vertices, outerVertices));
			foreach (var removeSide in removeSides) {
				doubtSides.Push(removeSide);
			}
			while (0 < doubtSides.Count()) {
				var removeSide = doubtSides.Pop();
				var suspectTriangles = triangles.Where(x=>x.HasSide(removeSide)).ToArray();
				if (suspectTriangles.Length == 2) {
					var otherIndex = suspectTriangles[0].GetOtherIndex(removeSide);
					var moreOtherIndex = suspectTriangles[1].GetOtherIndex(removeSide);
					var newTriangleVertices = new[]{removeSide.vertices[0]
													, GetVertices(otherIndex, vertices, outerVertices)
													, GetVertices(moreOtherIndex, vertices, outerVertices)
													};
					var flip = !ContainsInConvexHull(newTriangleVertices, removeSide.vertices[1]);
					if (flip) {
#if UNITY_EDITOR
						UnityEditor.Handles.DrawLine(removeSide.vertices[0], removeSide.vertices[1]);
#endif
						triangles.Remove(suspectTriangles[0]);
						triangles.Remove(suspectTriangles[1]);
						triangles.Add(new Triangle(new[]{otherIndex, moreOtherIndex, removeSide.indices[0]}, vertices, outerVertices));
						triangles.Add(new Triangle(new[]{otherIndex, moreOtherIndex, removeSide.indices[1]}, vertices, outerVertices));
					}
				}
			}
			triangles.RemoveAll(x=>IgnoreTriangle(x));
		}

		private static bool ContainsInConvexHull(Vector2[] vertices, Vector2 point) {
			System.Func<Vector2, Vector2, float> Vector2Cross = (Vector2 lhs, Vector2 rhs)=>{
				return lhs.x * rhs.y - lhs.y * rhs.x;
			};
			var cross= Vector2Cross(vertices[0] - vertices[vertices.Length - 1], point - vertices[vertices.Length - 1]);
			for (int i = 0, iMax = vertices.Length - 1; i < iMax; ++i) {
				var temp = Vector2Cross(vertices[i+1] - vertices[i], point - vertices[i]);
				if ((cross * temp) < 0.0f) {
					return false;
				}
				if ((cross == 0.0f) && (temp != 0.0f)) {
					cross = temp;
				}
			}
			return true;
		}

		private static bool IgnoreTriangle(Triangle triangle) {
			return triangle.indices.Any(x=>x<0);
		}
	}
}