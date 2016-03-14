using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Array = System.Array;
using System.Reflection;

namespace UIExtention {
	public class Gradation : BaseMeshEffect {
		[SerializeField]
		private GradationMaterial m_Material;
		public GradationMaterial material {get{return m_Material;} set{m_Material = value;}}

		[SerializeField] [Range(0.0f, 1.0f)]
		private float m_Weight = 1.0f;
		public float weight {get{return m_Weight;} set{m_Weight = value;}}

		public RectTransform rectTransform {
			get {
				if (m_rectTransform == null) {
					m_rectTransform = transform as RectTransform;
				}
				return m_rectTransform;
			}
		}
		private RectTransform m_rectTransform;

		private enum RectangleIndex {
			UpperLeft,
			UpperRight,
			LowerRight,
			LowerLeft,
		}

		public override void ModifyMesh(VertexHelper vh) {
			if (!IsActive() || (material == null)) {
				return;
			}

			List<UIVertex> vertices;
			List<int> indices;
			{
				const BindingFlags k_BindingFlags = BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance;
				var positions = (List<Vector3>)typeof(VertexHelper).GetField("m_Positions", k_BindingFlags).GetValue(vh);
				var colors = (List<Color32>)typeof(VertexHelper).GetField("m_Colors", k_BindingFlags).GetValue(vh);
				var uv0s = (List<Vector2>)typeof(VertexHelper).GetField("m_Uv0S", k_BindingFlags).GetValue(vh);
				var uv1s = (List<Vector2>)typeof(VertexHelper).GetField("m_Uv1S", k_BindingFlags).GetValue(vh);
				var normals = (List<Vector3>)typeof(VertexHelper).GetField("m_Normals", k_BindingFlags).GetValue(vh);
				var tangents = (List<Vector4>)typeof(VertexHelper).GetField("m_Tangents", k_BindingFlags).GetValue(vh);
				vertices = Enumerable.Range(0, positions.Count)
									.Select(x=>new UIVertex(){position = positions[x], color = colors[x], uv0 = uv0s[x], uv1 = uv1s[x], normal = normals[x], tangent = tangents[x]})
									.ToList();

				indices = (List<int>)typeof(VertexHelper).GetField("m_Indices", k_BindingFlags).GetValue(vh);
				indices = new List<int>(indices);
			}
			vh.Clear();

			Vector3[] localCorners = new Vector3[System.Enum.GetValues(typeof(RectangleIndex)).Length];
			rectTransform.GetLocalCorners(localCorners);
			var grid = material.GetGrid();

			for (int i = 0, iMax = indices.Count - 3; i <= iMax; ) {
				int[] rectangleIndices = null;
				if (i <= indices.Count - 6) {
					//四角形の可能性がある
					var hexIndices = Enumerable.Range(i, 6)
												.Select(x=>indices[x])
												.ToArray();
					rectangleIndices = GetRectangleIndices(vertices, hexIndices);
				}
				if (rectangleIndices != null) {
					//四角形
					ModifyRectangle(vh, vertices, rectangleIndices, grid, localCorners);
					i += 6;
				} else {
					//三角形
					var triangleIndices = Enumerable.Range(i, 3)
													.Select(x=>indices[x])
													.ToArray();
					ModifyTriangle(vh, vertices, triangleIndices, grid, localCorners);
					i += 3;
				}
			}
		}

		private int[] GetRectangleIndices(List<UIVertex> vertices, int[] hexIndices) {
			if (hexIndices.Length != 6) {
				return null;
			}
			var result = hexIndices.Distinct().ToArray();
			if (result.Length != System.Enum.GetValues(typeof(RectangleIndex)).Length) {
				return null;
			}
			var upperLeftPosition = new Vector2(float.MaxValue, float.MinValue);
			var lowerRightPosition = new Vector2(float.MinValue, float.MaxValue);
			foreach (var position in hexIndices.Select(x=>vertices[x].position)) {
				upperLeftPosition.x = Mathf.Min(upperLeftPosition.x, position.x);
				upperLeftPosition.y = Mathf.Max(upperLeftPosition.y, position.y);
				lowerRightPosition.x = Mathf.Max(lowerRightPosition.x, position.x);
				lowerRightPosition.y = Mathf.Min(lowerRightPosition.y, position.y);
			}
			foreach (var index in hexIndices) {
				var position = vertices[index].position;
				var rectangleIndex = 0;
				if (position.x == upperLeftPosition.x) {
					rectangleIndex = 0x0;
				} else if (position.x == lowerRightPosition.x) {
					rectangleIndex = 0x1;
				} else {
					return null;
				}
				if (position.y == upperLeftPosition.y) {
					rectangleIndex ^= 0x0;
				} else if (position.y == lowerRightPosition.y) {
					rectangleIndex ^= 0x3;
				} else {
					return null;
				}
				result[rectangleIndex] = index;
			}
			return result;
		}

		private void ModifyRectangle(VertexHelper vh, List<UIVertex> vertices, int[] rectangleIndices, GradationMaterial.Grid grid, Vector3[] localCorners) {
			var rectangleVertices = rectangleIndices.Select(x=>vertices[x]).ToArray();
			var rectangleNormalizePositions = new Vector2[rectangleVertices.Length];
			for (int i = 0, iMax = rectangleVertices.Length; i < iMax; ++i) {
				rectangleNormalizePositions[i] = new Vector2(Mathf.InverseLerp(localCorners[(int)RectangleIndex.UpperLeft].x, localCorners[(int)RectangleIndex.LowerRight].x, rectangleVertices[i].position.x)
												, Mathf.InverseLerp(localCorners[(int)RectangleIndex.LowerRight].y, localCorners[(int)RectangleIndex.UpperLeft].y, rectangleVertices[i].position.y)
												);
			}

			var xMin = Array.BinarySearch<float>(grid.xThresholds, rectangleNormalizePositions[(int)RectangleIndex.UpperLeft].x);
			if (xMin < 0) xMin = ~xMin - 1;
			var xMax = Array.BinarySearch<float>(grid.xThresholds, rectangleNormalizePositions[(int)RectangleIndex.LowerRight].x);
			if (xMax < 0) xMax = ~xMax;
			var yMin = Array.BinarySearch<float>(grid.yThresholds, rectangleNormalizePositions[(int)RectangleIndex.UpperLeft].y);
			if (yMin < 0) yMin = ~yMin - 1;
			var yMax = Array.BinarySearch<float>(grid.yThresholds, rectangleNormalizePositions[(int)RectangleIndex.LowerRight].y);
			if (yMax < 0) yMax = ~yMax;

			for (int y = yMin; y < yMax; ++y) {
				for (int x = xMin; x < xMax; ++x) {
					ModifyRectangleGrid(vh, rectangleVertices, grid, x, y, rectangleNormalizePositions);
				}
			}
		}

		private void ModifyRectangleGrid(VertexHelper vh, UIVertex[] rectangleVertices, GradationMaterial.Grid grid, int x, int y, Vector2[] rectangleNormalizePositions) {
			var gridPositions = new Vector2[System.Enum.GetValues(typeof(RectangleIndex)).Length];
			var gridColors = new Color[System.Enum.GetValues(typeof(RectangleIndex)).Length];
			for (int i = 0, iMax = System.Enum.GetValues(typeof(RectangleIndex)).Length; i < iMax; ++i) {
				var xOffset = (i ^ (i >> 1)) & 0x01;
				var yOffest = i >> 1;
				var xIndex = x + xOffset;
				var yIndex = y + yOffest;
				gridPositions[i] = new Vector2(grid.xThresholds[xIndex], grid.yThresholds[yIndex]);
				gridColors[i] = grid.GetColor(xIndex, yIndex);
			}

			System.Func<Vector2, UIVertex> PickupUIVertex = (position)=>{
				var v = position;
				for (int i = 0, iMax = 2; i < iMax; ++i) {
					v[i] = Mathf.InverseLerp(rectangleNormalizePositions[(int)RectangleIndex.UpperLeft][i], rectangleNormalizePositions[(int)RectangleIndex.LowerRight][i], v[i]);
				}
				var result = Lerp2D<UIVertex>(rectangleVertices, v, LerpUIVertex);

				var c = position;
				for (int i = 0, iMax = 2; i < iMax; ++i) {
					c[i] = Mathf.InverseLerp(gridPositions[(int)RectangleIndex.UpperLeft][i], gridPositions[(int)RectangleIndex.LowerRight][i], c[i]);
				}
				var gridColor = Lerp2D<Color>(gridColors, c, Color.Lerp);
				result.color = BlendColor(result.color, gridColor);

				return result;
			};

			var maskedPositions = new Vector2[System.Enum.GetValues(typeof(RectangleIndex)).Length];
			for (int i = 0, iMax = maskedPositions.Length; i < iMax; ++i) {
				for (int k = 0, kMax = 2; k < kMax; ++k) {
					maskedPositions[i][k] = Mathf.Clamp(rectangleNormalizePositions[i][k], gridPositions[(int)RectangleIndex.UpperLeft][k], gridPositions[(int)RectangleIndex.LowerRight][k]);
				}
			}
			maskedPositions = maskedPositions.Distinct(new Vector2Approximately()).ToArray();

			if (maskedPositions.Length == 4) {
				var maskedVertices = maskedPositions.Select(z=>PickupUIVertex(z)).ToArray();
				vh.AddUIVertexQuad(maskedVertices);
			}
		}

		private void ModifyTriangle(VertexHelper vh, List<UIVertex> vertices, int[] triangleIndices, GradationMaterial.Grid grid, Vector3[] localCorners) {
			var triangleVertices = triangleIndices.Select(x=>vertices[x]).ToArray();
			var triangleNormalizePositions = new Vector2[triangleVertices.Length];
			for (int i = 0, iMax = triangleVertices.Length; i < iMax; ++i) {
				triangleNormalizePositions[i] = new Vector2(Mathf.InverseLerp(localCorners[(int)RectangleIndex.UpperLeft].x, localCorners[(int)RectangleIndex.LowerRight].x, triangleVertices[i].position.x)
												, Mathf.InverseLerp(localCorners[(int)RectangleIndex.LowerRight].y, localCorners[(int)RectangleIndex.UpperLeft].y, triangleVertices[i].position.y)
												);
			}

			var xMin = Array.BinarySearch<float>(grid.xThresholds, triangleNormalizePositions.Min(x=>x.x));
			if (xMin < 0) xMin = ~xMin - 1;
			var xMax = Array.BinarySearch<float>(grid.xThresholds, triangleNormalizePositions.Max(x=>x.x));
			if (xMax < 0) xMax = ~xMax;
			var yMin = Array.BinarySearch<float>(grid.yThresholds, triangleNormalizePositions.Min(x=>x.y));
			if (yMin < 0) yMin = ~yMin - 1;
			var yMax = Array.BinarySearch<float>(grid.yThresholds, triangleNormalizePositions.Max(x=>x.y));
			if (yMax < 0) yMax = ~yMax;

			for (int y = yMin; y < yMax; ++y) {
				for (int x = xMin; x < xMax; ++x) {
					ModifyTriangleGrid(vh, triangleVertices, grid, x, y, triangleNormalizePositions);
				}
			}
		}

		private void ModifyTriangleGrid(VertexHelper vh, UIVertex[] triangleVertices, GradationMaterial.Grid grid, int x, int y, Vector2[] triangleNormalizePositions) {
			var gridPositions = new Vector2[System.Enum.GetValues(typeof(RectangleIndex)).Length];
			var gridColors = new Color[System.Enum.GetValues(typeof(RectangleIndex)).Length];
			for (int i = 0, iMax = System.Enum.GetValues(typeof(RectangleIndex)).Length; i < iMax; ++i) {
				var xOffset = (i ^ (i >> 1)) & 0x01;
				var yOffest = i >> 1;
				var xIndex = x + xOffset;
				var yIndex = y + yOffest;
				gridPositions[i] = new Vector2(grid.xThresholds[xIndex], grid.yThresholds[yIndex]);
				gridColors[i] = grid.GetColor(xIndex, yIndex);
			}

			System.Func<Vector2, UIVertex> PickupUIVertex = (position)=>{
				var inverseArea = 1.0f / GetAreaOfTriangle(triangleNormalizePositions[0], triangleNormalizePositions[1], triangleNormalizePositions[2]);
				var result = UIVertexTriWeightedAverage(triangleVertices[0], GetAreaOfTriangle(position, triangleNormalizePositions[1], triangleNormalizePositions[2]) * inverseArea
													, triangleVertices[1], GetAreaOfTriangle(triangleNormalizePositions[0],position , triangleNormalizePositions[2]) * inverseArea
													, triangleVertices[2], GetAreaOfTriangle(triangleNormalizePositions[0], triangleNormalizePositions[1], position) * inverseArea
													);

				var c = position;
				for (int i = 0, iMax = 2; i < iMax; ++i) {
					c[i] = Mathf.InverseLerp(gridPositions[(int)RectangleIndex.UpperLeft][i], gridPositions[(int)RectangleIndex.LowerRight][i], c[i]);
				}
				var gridColor = Lerp2D<Color>(gridColors, c, Color.Lerp);
				result.color = BlendColor(result.color, gridColor);

				return result;
			};

			var maskedPositions = new List<Vector2>(6);
			{
				var overlap = OverlapPosition(triangleNormalizePositions, gridPositions[0]);
				for (int i = 0, iMax = gridPositions.Length; i < iMax; ++i) {
					if (overlap) {
						maskedPositions.Add(gridPositions[i]);
					}
					var nextIndex = (i + 1) % iMax;
					var nextOverlap = OverlapPosition(triangleNormalizePositions, gridPositions[nextIndex]);
					if (!overlap || !nextOverlap) {
						var crossPositions = GetCrossPositions(gridPositions[i], gridPositions[nextIndex], triangleNormalizePositions);
						maskedPositions.AddRange(crossPositions);
					}
					overlap = nextOverlap;
				}
			}
			for (int i = 0, iMax = triangleNormalizePositions.Length; i < iMax; ++i) {
				var overlap = OverlapPosition(gridPositions, triangleNormalizePositions[i]);
				if (overlap) {
					maskedPositions.Add(triangleNormalizePositions[i]);
				}
			}
			maskedPositions = maskedPositions.Distinct(new Vector2Approximately()).ToList();

			if (3 <= maskedPositions.Count) {
				var currentVertCount = vh.currentVertCount;
				var maskedVertices = maskedPositions.Select(z=>PickupUIVertex(z)).ToArray();
				foreach (var maskedvertex in maskedVertices) {
					vh.AddVert(maskedvertex);
				}
				var triangleIndices = GetTriangleIndices(maskedPositions);
				for (int i = 0, iMax = triangleIndices.Count; i < iMax; i += 3) {
					vh.AddTriangle(currentVertCount + triangleIndices[i]
									, currentVertCount + triangleIndices[i + 1]
									, currentVertCount + triangleIndices[i + 2]);
				}
			}
		}

		private static System.Func<float, float, float> CreateBlendFunction(GradationMaterial.Blend blend) {
			switch (blend) {
			case GradationMaterial.Blend.Multiply:
			default:
				return (x,y)=>x * y;
			case GradationMaterial.Blend.Override:
				return (x,y)=>y;
			case GradationMaterial.Blend.Ignore:
				return (x,y)=>x;
			case GradationMaterial.Blend.Add:
				return (x,y)=>x + y;
			case GradationMaterial.Blend.Subtract:
				return (x,y)=>x - y;
			case GradationMaterial.Blend.Screen:
				return (x,y)=>1.0f - (1.0f - x) * (1.0f - y);
			case GradationMaterial.Blend.HardLight:
				return (x,y)=>((y < 0.5f)? (x * y): (1.0f - (1.0f - x) * (1.0f - y)));
			case GradationMaterial.Blend.HardLightFast:
				return (x,y)=>((y < 0.5f)? (x * y): (x + y));
			case GradationMaterial.Blend.Darken:
				return (x,y)=>Mathf.Min(x, y);
			case GradationMaterial.Blend.Lighten:
				return (x,y)=>Mathf.Max(x, y);
			}
		}

		private Color BlendColor(Color vertexColor, Color gridColor) {
			Color blendColor;

			var blendFunction = CreateBlendFunction(material.colorBlend);
			blendColor.r = blendFunction(vertexColor.r, gridColor.r);
			blendColor.g = blendFunction(vertexColor.g, gridColor.g);
			blendColor.b = blendFunction(vertexColor.b, gridColor.b);
			blendFunction = CreateBlendFunction(material.alphaBlend);
			blendColor.a = blendFunction(vertexColor.a, gridColor.a);

			var result = Color.Lerp(vertexColor, blendColor, weight);
			return result;
		}

		private static List<int> GetTriangleIndices(List<Vector2> positions) {
			var result = new List<int>(){0, 1, 2};

			if (0 < positions.Count) {
				var pairs = new List<KeyValuePair<int, int>>((positions.Count - 3) * 2 + 5);
				pairs.Add(new KeyValuePair<int, int>(0, 1));
				pairs.Add(new KeyValuePair<int, int>(0, 2));
				pairs.Add(new KeyValuePair<int, int>(1, 2));

				for (int i = 3, iMax = positions.Count; i < iMax; ++i) {
					for(int k = 0, kMax = pairs.Count; k < kMax; ++k) {
						var currentPair = pairs[k];
						var isCross = pairs.Any(x=>{
													if ((currentPair.Key != x.Key) && (currentPair.Key != x.Value)) {
														var p = GetProgressOfCrossPosition(positions[i], positions[currentPair.Key], positions[x.Key], positions[x.Value]);
														if (p.HasValue) {
															return true;
														}
													}
													if ((currentPair.Value != x.Key) && (currentPair.Value != x.Value)) {
														var p = GetProgressOfCrossPosition(positions[i], positions[currentPair.Value], positions[x.Key], positions[x.Value]);
														if (p.HasValue) {
															return true;
														}
													}
													return false;
											});
						if (!isCross) {
							result.AddRange(new[]{currentPair.Key, currentPair.Value, i});
							pairs.Add(new KeyValuePair<int, int>(currentPair.Key, i));
							pairs.Add(new KeyValuePair<int, int>(currentPair.Value, i));
						}
					}
				}
			}
	
			return result;
		}

		private static float Vector2Cross(Vector2 lhs, Vector2 rhs) {
			return lhs.x * rhs.y - lhs.y * rhs.x;
		}

		private class Vector2Approximately : EqualityComparer<Vector2> {
			public override bool Equals(Vector2 x, Vector2 y) {
				return Mathf.Approximately(x.x, y.x) && Mathf.Approximately(x.y, y.y);
			}
			public override int GetHashCode(Vector2 obj) {
				return 0;
			}
		}

		private static T Lerp2D<T>(T[] rectangleValues, Vector2 f, System.Func<T, T, float, T> lerp) {
			return lerp(lerp(rectangleValues[(int)RectangleIndex.UpperLeft], rectangleValues[(int)RectangleIndex.UpperRight], f.x)
						, lerp(rectangleValues[(int)RectangleIndex.LowerLeft], rectangleValues[(int)RectangleIndex.LowerRight], f.x)
						, f.y
						);
		}

		private static UIVertex LerpUIVertex(UIVertex a, UIVertex b, float f) {
			return new UIVertex(){
				color = Color32.Lerp(a.color, b.color, f),
				normal = Vector3.Lerp(a.normal, b.normal, f),
				position = Vector3.Lerp(a.position, b.position, f),
				tangent = Vector4.Lerp(a.tangent, b.tangent, f),
				uv0 = Vector2.Lerp(a.uv0, b.uv0, f),
				uv1 = Vector2.Lerp(a.uv1, b.uv1, f),
			};
		}

		private static UIVertex UIVertexTriWeightedAverage(UIVertex a, float af, UIVertex b, float bf, UIVertex c, float cf) {
			return new UIVertex(){
				color = new Color32((byte)(a.color.r * af + b.color.r * bf + c.color.r * cf + 0.5f)
									, (byte)(a.color.g * af + b.color.g * bf + c.color.g * cf + 0.5f)
									, (byte)(a.color.b * af + b.color.b * bf + c.color.b * cf + 0.5f)
									, (byte)(a.color.a * af + b.color.a * bf + c.color.a * cf + 0.5f)
									),
				normal = a.normal * af + b.normal * bf + c.normal * cf,
				position = a.position * af + b.position * bf + c.position * cf,
				tangent = a.tangent * af + b.tangent * bf + c.tangent * cf,
				uv0 = a.uv0 * af + b.uv0 * bf + c.uv0 * cf,
				uv1 = a.uv1 * af + b.uv1 * bf + c.uv1 * cf,
			};
		}

		private static float GetAreaOfTriangle(Vector2 position0, Vector2 position1, Vector2 position2) {
			var vector01 = position1 - position0;
			var vector02 = position2 - position0;
			return 0.5f * Mathf.Abs(Vector2Cross(vector01, vector02));
		}

		private static bool OverlapPosition(Vector2[] polygonPositions, Vector2 position) {
			int polygonLength = polygonPositions.Length;
			var baseDot = Vector2Cross(polygonPositions[0] - polygonPositions[polygonLength - 1], position - polygonPositions[polygonLength - 1]);
			var result = true;
			for (int i = 0, iMax = polygonLength - 1; i < iMax; ++i) {
				var value = baseDot * Vector2Cross(polygonPositions[i + 1] - polygonPositions[i], position - polygonPositions[i]);
				if (value <= 0.0f) {
					result = false;
					break;
				}
			}
			return result;
		}

		private static bool OverlapPosition(Vector2 upperLeftMaskPosition, Vector2 lowerRightMaskPosition, Vector2 position) {
			bool result = true;
			if (position.x < upperLeftMaskPosition.x) {
				result = false;
			} else if (lowerRightMaskPosition.x < position.x) {
				result = false;
			} else if (position.y < upperLeftMaskPosition.y) {
				result = false;
			} else if (lowerRightMaskPosition.y < position.y) {
				result = false;
			}
			return result;
		}

		private static List<Vector2> GetCrossPositions(Vector2 position1, Vector2 position2, Vector2[] PolygonPositions) {
			var result = new List<Vector2>();
			for (int i = 0, iMax = PolygonPositions.Length; i < iMax; ++i) {
				var cross = GetProgressOfCrossPosition(position1
											, position2
											, PolygonPositions[i]
											, PolygonPositions[(i + 1) % iMax]
											);
				if (cross.HasValue) {
					result.Add(Vector2.Lerp(position1, position2, cross.Value));
				}
			}
			return result;
		}

		private static float? GetProgressOfCrossPosition(Vector2 positionA1, Vector2 positionA2, Vector2 positionB1, Vector2 positionB2) {
			var vectorA = positionA2 - positionA1;
			var vectorB = positionB2 - positionB1;
			var crossAB = Vector2Cross(vectorA, vectorB);
			if (crossAB == 0.0f) {
				return null;
			}
			var inverseCrossAB = 1.0f / crossAB;

			var vectorX = positionB1 - positionA1;
			var crossXA = Vector2Cross(vectorX, vectorA);
			var crossXB = Vector2Cross(vectorX, vectorB);
			var fA = crossXB * inverseCrossAB;
			var fB = crossXA * inverseCrossAB;
			if ((fA < 0.0f) || (1.0f < fA) || (fB < 0.0f) || (1.0f < fB)) {
				return null;
			}
			return fA;
		}
	}
}
