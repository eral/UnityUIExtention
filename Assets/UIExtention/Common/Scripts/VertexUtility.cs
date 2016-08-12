using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UIExtention {
	public static class VertexUtility {
		public static UIVertex Lerp(UIVertex a, UIVertex b, float f) {
			return new UIVertex(){
				color = Color32.Lerp(a.color, b.color, f),
				normal = Vector3.Lerp(a.normal, b.normal, f),
				position = Vector3.Lerp(a.position, b.position, f),
				tangent = Vector4.Lerp(a.tangent, b.tangent, f),
				uv0 = Vector2.Lerp(a.uv0, b.uv0, f),
				uv1 = Vector2.Lerp(a.uv1, b.uv1, f),
			};
		}
		public static void Lerp(UIVertex a, UIVertex b, float f, out UIVertex result) {
			result.color = Color32.Lerp(a.color, b.color, f);
			result.normal = Vector3.Lerp(a.normal, b.normal, f);
			result.position = Vector3.Lerp(a.position, b.position, f);
			result.tangent = Vector4.Lerp(a.tangent, b.tangent, f);
			result.uv0 = Vector2.Lerp(a.uv0, b.uv0, f);
			result.uv1 = Vector2.Lerp(a.uv1, b.uv1, f);
		}

		public static UIVertex WeightedAverage(IEnumerable<UIVertex> vertices, IEnumerable<float> weights) {
			UIVertex result;
			WeightedAverage(vertices, weights, out result);
			return result;
		}
		public static void WeightedAverage(IEnumerable<UIVertex> vertices, IEnumerable<float> weights, out UIVertex result) {
			result.position = Vector3.zero;
			result.normal = Vector3.zero;
			var color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			result.uv0 = Vector2.zero;
			result.uv1 = Vector2.zero;
			result.tangent = Vector4.zero;

			var vEnumerator = vertices.GetEnumerator();
			var wEnumerator = weights.GetEnumerator();
			while (vEnumerator.MoveNext() && wEnumerator.MoveNext()) {
				var vertex = vEnumerator.Current;
				var weight = wEnumerator.Current;

				result.position += vertex.position * weight;
				result.normal += vertex.normal * weight;
				color += ((Color)vertex.color) * weight;
				result.uv0 += vertex.uv0 * weight;
				result.uv1 += vertex.uv1 * weight;
				result.tangent += vertex.tangent * weight;
			}
			result.color = color;
		}

		public static UIVertex MultiplyVertexColor(UIVertex a, UIVertex b) {
			return new UIVertex(){
				position = a.position,
				normal = a.normal,
				color = new Color32((byte)(a.color.r * b.color.r / 0xFF)
									, (byte)(a.color.g * b.color.g / 0xFF)
									, (byte)(a.color.b * b.color.b / 0xFF)
									, (byte)(a.color.a * b.color.a / 0xFF)
									),
				uv0 = a.uv0,
				uv1 = a.uv1,
				tangent = a.tangent,
			};
		}

		private static float GetSqrDistance(UIVertex aStart, UIVertex aEnd, UIVertex bStart, UIVertex bEnd) {
			return GetSqrDistance(aStart.position, aEnd.position, bStart.position, bEnd.position);
		}
		private static float GetSqrDistance(Vector3 aStart, Vector3 aEnd, Vector3 bStart, Vector3 bEnd) {
			var nearestProgress = GetProgressOfNearestPoint(aStart, aEnd, bStart, bEnd);
			var aPosition = Vector3.Lerp(aStart, aEnd, nearestProgress[0]);
			var bPosition = Vector3.Lerp(bStart, bEnd, nearestProgress[1]);
			var distance = aPosition  - bPosition;
			return distance.sqrMagnitude;
		}

		private static UIVertex? GetCrossPoint(UIVertex aStart, UIVertex aEnd, UIVertex bStart, UIVertex bEnd) {
			var nearestProgress = GetProgressOfNearestPoint(aStart.position, aEnd.position, bStart.position, bEnd.position);
			var a = Lerp(aStart, aEnd, nearestProgress[0]);
			var b = Lerp(bStart, bEnd, nearestProgress[1]);

			if (Vector2.SqrMagnitude((Vector2)a.position - (Vector2)b.position) < Vector2.kEpsilon) {
				return MultiplyVertexColor(a, b);
			} else {
				return null;
			}
		}
		private static UIVertex? GetCrossPoint(UIVertex aStart, UIVertex aEnd, Vector2 bStart, Vector2 bEnd) {
			var nearestProgress = GetProgressOfNearestPoint(aStart.position, aEnd.position, bStart, bEnd);
			var a = Lerp(aStart, aEnd, nearestProgress[0]);
			var b = Vector2.Lerp(bStart, bEnd, nearestProgress[1]);

			if (Vector2.SqrMagnitude((Vector2)a.position - b) < Vector2.kEpsilon) {
				return a;
			} else {
				return null;
			}
		}

		public static UIVertex[] GetNearestPoint(UIVertex aStart, UIVertex aEnd, UIVertex bStart, UIVertex bEnd) {
			var nearestProgress = GetProgressOfNearestPoint(aStart.position, aEnd.position, bStart.position, bEnd.position);
			return new[]{Lerp(aStart, aEnd, nearestProgress[0])
						, Lerp(aStart, aEnd, nearestProgress[1])
						};
		}
		public static Vector3[] GetNearestPoint(Vector3 aStart, Vector3 aEnd, Vector3 bStart, Vector3 bEnd) {
			var nearestProgress = GetProgressOfNearestPoint(aStart, aEnd, bStart, bEnd);
			return new[]{Vector3.Lerp(aStart, aEnd, nearestProgress[0])
						, Vector3.Lerp(aStart, aEnd, nearestProgress[1])
						};
		}

		public static float[] GetProgressOfNearestPoint(UIVertex aStart, UIVertex aEnd, UIVertex bStart, UIVertex bEnd) {
			return GetProgressOfNearestPoint(aStart.position, aEnd.position, bStart.position, bEnd.position);
		}
		public static float[] GetProgressOfNearestPoint(Vector3 aStart, Vector3 aEnd, Vector3 bStart, Vector3 bEnd) {
			var aDirection = aEnd - aStart;
			var bDirection = bEnd - bStart;

			var between = aStart - bStart;
			var aSqrLength = aDirection.sqrMagnitude;
			var bSqrLength = bDirection.sqrMagnitude;

			float aProgress, bProgress;
			do {
				if ((0.0f == aSqrLength) || (0.0f == bSqrLength)) {
					//Point & Point
					aProgress = 0.0f;
					bProgress = 0.0f;
					break;
				}

				var betweenOnBProjection = Vector3.Dot(bDirection, between);
				if (0.0f == aSqrLength) {
					//Point & Segment
					aProgress = 0.0f;
					bProgress = Mathf.Clamp01(betweenOnBProjection / bSqrLength);
					break;
				}
				var betweenOnAProjection = Vector3.Dot(aDirection, between);
				if (0.0f == bSqrLength) {
					//Segment & Point
					aProgress = Mathf.Clamp01(-betweenOnAProjection / aSqrLength);
					bProgress = 0.0f;
					break;
				}
				//Segment & Segment
				var bOnAProjection = Vector3.Dot(aDirection, bDirection);
				var denom = aSqrLength * bSqrLength - bOnAProjection * bOnAProjection;
				if (0.0f != denom) {
					aProgress = Mathf.Clamp01((bOnAProjection * betweenOnBProjection - betweenOnAProjection * bSqrLength) / denom);
				} else {
					aProgress = 0.0f;
				}
				var bProgressNom = bOnAProjection * aProgress + betweenOnBProjection;
				if (bProgressNom < 0.0f) {
					bProgress = 0.0f;
					aProgress = Mathf.Clamp01(-betweenOnAProjection / aSqrLength);
				} else if (bSqrLength < bProgressNom) {
					bProgress = 1.0f;
					aProgress = Mathf.Clamp01((bOnAProjection - betweenOnAProjection) / aSqrLength);
				} else {
					bProgress = bProgressNom / bSqrLength;
				}
			} while (false);
			return new[]{aProgress, bProgress};
		}

		public static void Intersect(List<UIVertex> vertices, VerticesView mask) {
			var indexBuffer = new List<int>();
			var vertexBuffer = new List<UIVertex>();
			for (var i = vertices.Count - 3; 0 <= i; i -= 3) {
				if (Vector3.kEpsilon < Vector3.SqrMagnitude(vertices[i].position - vertices[i + 1].position)) {
					indexBuffer.Clear();
					vertexBuffer.Clear();
					var vertexInput = new VerticesView(vertices, i, 3);
					MaskTriangles(vertexInput, mask, vertexBuffer, indexBuffer);
					var maskedVerticesCount = indexBuffer.Count;
					if (maskedVerticesCount == 0) {
						vertices.RemoveRange(i, 3);
						continue;
					}
					for(int k = 0, kMax = 3; k < kMax; ++k) {
						vertices[i + k] = vertexBuffer[indexBuffer[k]];
					}
					if (3 < maskedVerticesCount) {
						vertices.InsertRange(i + 3, indexBuffer.Skip(3).Select(x=>vertexBuffer[x]));
					}
				}
			}
		}

		private static void MaskTriangles(VerticesView vertices, VerticesView mask, List<UIVertex> vertexBuffer, List<int> indexBuffer) {
			var maskedCount = 0;
			for(int i = 0, iMax = mask.Count; i < iMax; i += 3) {
				var currentMask = mask.GetRange(i, 3);
				if (AddContainsVertex(vertices, currentMask, vertexBuffer) < 3) {
					AddContainsMask(vertices, currentMask, vertexBuffer);
					AddCrossPoint(vertices, currentMask, vertexBuffer);
				}
				var addCount = vertexBuffer.Count - maskedCount;
				if (3 <= addCount) {
					var triangulationIndex = TriangulationOfSmallConvexHull(vertexBuffer.Skip(maskedCount).Select(x=>(Vector2)x.position).ToList());
					indexBuffer.AddRange(triangulationIndex.Select(x=>x + maskedCount));
				}
				if (0 < addCount) {
					maskedCount = vertexBuffer.Count;
				}
			}
		}

		private static int AddContainsVertex(VerticesView vertices, VerticesView mask, List<UIVertex> vertexBuffer) {
			var result = 0;
			var mask2d = mask.Select(x=>(Vector2)x.position).ToArray();
			foreach (var vertex in vertices) {
				if (ContainsInConvexHull(mask2d, vertex.position)) {
					var maskVertex = PickupUIVertexFromTriangle(vertex.position, mask);
					var v = MultiplyVertexColor(vertex, maskVertex);
					vertexBuffer.Add(v);
					++result;
				}
			}
			return result;
		}

		private static int AddContainsMask(VerticesView vertices, VerticesView mask, List<UIVertex> vertexBuffer) {
			var result = 0;
			var vertices2d = vertices.Select(x=>(Vector2)x.position).ToArray();
			foreach (var maskVertex in mask) {
				if (ContainsInConvexHull(vertices2d, (Vector2)maskVertex.position)) {
					var vertex = PickupUIVertexFromTriangle(maskVertex.position, vertices);
					var v = MultiplyVertexColor(vertex, maskVertex);
					vertexBuffer.Add(v);
					++result;
				}
			}
			return result;
		}

		private static int AddCrossPoint(VerticesView vertices, VerticesView mask, List<UIVertex> vertexBuffer) {
			var result = 0;
			for (int i = 0, iMax = vertices.Count; i < iMax; ++i) {
				var iNext = i + 1;
				if (iMax <= iNext) iNext = 0;
				for (int k = 0, kMax = mask.Count; k < kMax; ++k) {
					var kNext = k + 1;
					if (kMax <= kNext) kNext = 0;

					var crossPoint = GetCrossPoint(vertices[i], vertices[iNext], mask[k], mask[kNext]);
					if (crossPoint.HasValue) {
						vertexBuffer.Add(crossPoint.Value);
						++result;
					}
				}
			}
			return result;
		}

		public static List<int> Triangulation(List<Vector2> vertices) {
			var outerVertices = GetVerticesOfOuterTriangle(vertices);
			var triangles = new List<Triangle>();
			triangles.Add(new Triangle(new[]{-1, -2, -3}, vertices, outerVertices));

			var doubtSides = new Stack<Side>();
			for (int i = 0, iMax = vertices.Count; i < iMax; ++i) {
				PlungeVertex(triangles, doubtSides, i, vertices, outerVertices);
			}

			RemoveOuterTriangle(triangles, doubtSides, vertices, outerVertices);

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
						triangles.Remove(suspectTriangles[0]);
						triangles.Remove(suspectTriangles[1]);
						triangles.Add(new Triangle(new[]{otherIndex, moreOtherIndex, removeSide.indices[0]}, vertices, outerVertices));
						triangles.Add(new Triangle(new[]{otherIndex, moreOtherIndex, removeSide.indices[1]}, vertices, outerVertices));
					}
				}
			}
			triangles.RemoveAll(x=>IgnoreTriangle(x));
		}

		public static bool ContainsInConvexHull(Vector2[] vertices, Vector2 point) {
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

		private static float Vector2Cross(Vector2 lhs, Vector2 rhs) {
			return lhs.x * rhs.y - lhs.y * rhs.x;
		}

		private static float GetAreaOfTriangle(Vector2 vertex0, Vector2 vertex1, Vector2 vertex2) {
			var vector01 = vertex1 - vertex0;
			var vector02 = vertex2 - vertex0;
			return 0.5f * Mathf.Abs(Vector2Cross(vector01, vector02));
		}

		public static bool IsDegeneracy(Vector2 vertex0, Vector2 vertex1, Vector2 vertex2) {
			var vector01 = vertex1 - vertex0;
			var vector02 = vertex2 - vertex0;
			var result = Vector2Cross(vector01, vector02) < float.Epsilon;
			return result;
		}

		private static UIVertex PickupUIVertexFromTriangle(Vector2 pickupPosition, VerticesView vertices) {
			var inverseArea = 1.0f / GetAreaOfTriangle(vertices.GetPosition(0), vertices.GetPosition(1), vertices.GetPosition(2));
			var weights = new[]{GetAreaOfTriangle(pickupPosition, vertices.GetPosition(1), vertices.GetPosition(2)) * inverseArea
								, GetAreaOfTriangle(vertices.GetPosition(0), pickupPosition, vertices.GetPosition(2)) * inverseArea
								, GetAreaOfTriangle(vertices.GetPosition(0), vertices.GetPosition(1), pickupPosition) * inverseArea
								};
			var result = WeightedAverage(vertices, weights);
			return result;
		}

		public static List<int> TriangulationOfSmallConvexHull(List<Vector2> vertices) {
			if (6 < vertices.Count) {
				throw new System.ArgumentOutOfRangeException();
			}

			var result = Enumerable.Range(0, vertices.Count).ToList();
			if (4 <= vertices.Count) {
				result.Sort(new CounterClockWiseVector2(vertices));
				result = k_SmallConvexHullVertexIndices[vertices.Count - 4].Select(x=>result[x]).ToList();
			}

			return result;
		}

		private class CounterClockWiseVector2 : IComparer<int> {
			public CounterClockWiseVector2 (List<Vector2> vertices) {
				m_vertices = vertices;
				m_center = Vector2.zero;
				vertices.ForEach(x=>{
					m_center.x += x.x;
					m_center.y += x.y;
				});
				m_center /= vertices.Count;
			}
			public int Compare(int x, int y) {
				var compare = Atan2(m_vertices[x] - m_center) - Atan2(m_vertices[y] - m_center);
				return ((0.0f < compare)
							? 1
						:(compare < 0.0f)
							? -1
							: 0
						);

			}
			private float Atan2(Vector2 x) {
				return Mathf.Atan2(x.y, x.x);
			}
			private List<Vector2> m_vertices;
			private Vector2 m_center;
		}

		private static readonly int[][] k_SmallConvexHullVertexIndices = new int[][]{
														new int[]{0, 1, 2, 0, 2, 3}
														, new int[]{0, 1, 2, 0, 2, 3, 0, 3, 4}
														, new int[]{0, 1, 2, 0, 2, 3, 0, 3, 5, 3, 4, 5}
														};

	}
}