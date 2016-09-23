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

		public enum RectangleIndex {
			LowerLeft,
			UpperLeft,
			UpperRight,
			LowerRight,
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

			var rectTransformRect = rectTransform.rect;
			var maskMesh = material.GetMesh();
			var mask = maskMesh.GetIndices(0).Select(x=>new UIVertex(){position = Vector2.Scale(maskMesh.vertices[x], rectTransformRect.size) + rectTransformRect.min
																		, color = maskMesh.colors[x]
																		}
													)
											.ToList();

			var blend = GetBlendUIVertex();

			var text = GetComponent<Text>();
			if (text == null) {
				RectTransformIntersect(vertices, indices, mask, blend);
			} else switch (material.textMapping) {
			case GradationMaterial.TextMapping.Line:
				LineIntersect(vertices, indices, mask, blend, text);
				break;
			case GradationMaterial.TextMapping.Character:
				CharacterIntersect(vertices, indices, mask, blend, text);
				break;
			case GradationMaterial.TextMapping.RectTransform:
			default:
				RectTransformIntersect(vertices, indices, mask, blend);
				break;
			}

			vh.AddUIVertexStream(vertices, indices);
		}

		private static System.Func<int, int, int> CreateBlendFunction(GradationMaterial.Blend blend) {
			switch (blend) {
			case GradationMaterial.Blend.Multiply:
			default:
				return (x,y)=>x * y / 0xFF;
			case GradationMaterial.Blend.Override:
				return (x,y)=>y;
			case GradationMaterial.Blend.Ignore:
				return (x,y)=>x;
			case GradationMaterial.Blend.Add:
				return (x,y)=>Mathf.Min(x + y, 0xFF);
			case GradationMaterial.Blend.Subtract:
				return (x,y)=>Mathf.Max(0, x - y);
			case GradationMaterial.Blend.Screen:
				return (x,y)=>0xFF - ((0xFF - x) * (0xFF - y) / 0xFF);
			case GradationMaterial.Blend.HardLight:
				return (x,y)=>(y < 0x80)? (x * y / 0xFF): (0xFF - ((0xFF - x) * (0xFF - y) / 0xFF));
			case GradationMaterial.Blend.HardLightFast:
				return (x,y)=>(y < 0x80)? (x * y / 0xFF): Mathf.Min(x + y, 0xFF);
			case GradationMaterial.Blend.Darken:
				return (x,y)=>Mathf.Min(x, y);
			case GradationMaterial.Blend.Lighten:
				return (x,y)=>Mathf.Max(x, y);
			}
		}

		private System.Func<UIVertex, UIVertex, UIVertex> GetBlendUIVertex() {
			if (weight <= 0.0f) {
				return BlendUIVertex0;
			} else if (1.0f <= weight) {
				return BlendUIVertex1;
			} else {
				return BlendUIVertex;
			}
		}

		private static UIVertex BlendUIVertex0(UIVertex vertex, UIVertex mask) {
			return vertex;
		}
		private UIVertex BlendUIVertex(UIVertex vertex, UIVertex mask) {
			Color32 color;
			var blendFunction = CreateBlendFunction(material.colorBlend);
			color.r = (byte)blendFunction(vertex.color.r, mask.color.r);
			color.g = (byte)blendFunction(vertex.color.g, mask.color.g);
			color.b = (byte)blendFunction(vertex.color.b, mask.color.b);
			blendFunction = CreateBlendFunction(material.alphaBlend);
			color.a = (byte)blendFunction(vertex.color.a, mask.color.a);

			vertex.color = Color32.Lerp(vertex.color, color, weight);
			return vertex;
		}
		private UIVertex BlendUIVertex1(UIVertex vertex, UIVertex mask) {
			Color32 color;
			var blendFunction = CreateBlendFunction(material.colorBlend);
			color.r = (byte)blendFunction(vertex.color.r, mask.color.r);
			color.g = (byte)blendFunction(vertex.color.g, mask.color.g);
			color.b = (byte)blendFunction(vertex.color.b, mask.color.b);
			blendFunction = CreateBlendFunction(material.alphaBlend);
			color.a = (byte)blendFunction(vertex.color.a, mask.color.a);

			vertex.color = color;
			return vertex;
		}

		private static void RectTransformIntersect(List<UIVertex> vertices, List<int> indices, List<UIVertex> mask, System.Func<UIVertex, UIVertex, UIVertex> blend) {
			VertexUtility.Intersect(vertices, indices, mask, blend);
		}

		private void LineIntersect(List<UIVertex> vertices, List<int> indices, List<UIVertex> mask, System.Func<UIVertex, UIVertex, UIVertex> blend, Text text) {
			var originalVerticesCount = vertices.Count;
			var originalIndicesCount = indices.Count;
			var rectVertices = new List<UIVertex>(mask.Count * 2);
			var rectIndices = new List<int>(mask.Count * 2);

			var rectTransformRect = rectTransform.rect;

			var srcIndex = 0;
			var dstIndex = 0;
			for (int i = 0, iMax = text.text.Length; i < iMax; ++i) {
				var ch = text.text[i];
				CharacterInfo info;
				if (!text.font.GetCharacterInfo(ch, out info)) {
					info = text.font.characterInfo.Where(x=>x.index == ch).FirstOrDefault();
				}

				var boundingBox = new Rect(0.0f
										, (text.font.ascent - ((text.font.lineHeight - text.font.fontSize) * 0.5f)) * text.fontSize / (float)text.font.fontSize - text.fontSize
										, text.fontSize
										, text.fontSize
										);
				var charGlyph = new Rect(info.minX * text.fontSize / (float)info.size
										, info.minY * text.fontSize / (float)info.size
										, (info.maxX - info.minX) * text.fontSize / (float)info.size
										, (info.maxY - info.minY) * text.fontSize / (float)info.size
										);
				var upperRightPosition = vertices[indices[srcIndex + 1]].position;
				var lowerLeftPosition = vertices[indices[srcIndex + 4]].position;
				var charDraw = new Rect(lowerLeftPosition.x
										, lowerLeftPosition.y
										, upperRightPosition.x - lowerLeftPosition.x
										, upperRightPosition.y - lowerLeftPosition.y
										);

				var scale = new Vector2(rectTransformRect.size.x / boundingBox.size.x
										, rectTransformRect.size.y / boundingBox.size.y
										);

				var srcStartIndex = indices[srcIndex];
				for (var k = 0; k < 4; ++k) {
					var vertex = vertices[srcStartIndex + k];
					var pos2d = Vector2.Scale((Vector2)vertex.position - charDraw.min + (charGlyph.min - boundingBox.min), scale) + rectTransformRect.min;
					vertex.position = new Vector3(vertex.position.x, pos2d.y, vertex.position.z);
					rectVertices.Add(vertex);
				}
				for (var k = 0; k < 6; ++k) {
					rectIndices.Add(indices[srcIndex] - srcStartIndex);
					++srcIndex;
				}

				VertexUtility.Intersect(rectVertices, rectIndices, mask, blend);
				
				var scaleInverse = new Vector2(boundingBox.size.x / rectTransformRect.size.x
										, boundingBox.size.y / rectTransformRect.size.y
										);

				vertices.AddRange(rectVertices.Select(x=>{
					var pos2d = Vector2.Scale((Vector2)x.position - rectTransformRect.min, scaleInverse) - (charGlyph.min - boundingBox.min) + charDraw.min;
					x.position = new Vector3(x.position.x, pos2d.y, x.position.z);
					return x;
				}));
				indices.AddRange(rectIndices.Select(x=>x+dstIndex));
				dstIndex += rectVertices.Count;

				rectVertices.RemoveRange(0, rectVertices.Count);
				rectIndices.RemoveRange(0, rectIndices.Count);
			}
			vertices.RemoveRange(0, originalVerticesCount);
			indices.RemoveRange(0, originalIndicesCount);
		}

		private void CharacterIntersect(List<UIVertex> vertices, List<int> indices, List<UIVertex> mask, System.Func<UIVertex, UIVertex, UIVertex> blend, Text text) {
			var originalVerticesCount = vertices.Count;
			var originalIndicesCount = indices.Count;
			var rectVertices = new List<UIVertex>(mask.Count * 2);
			var rectIndices = new List<int>(mask.Count * 2);

			var rectTransformRect = rectTransform.rect;

			var srcIndex = 0;
			var dstIndex = 0;
			for (int i = 0, iMax = text.text.Length; i < iMax; ++i) {
				var ch = text.text[i];
				CharacterInfo info;
				if (!text.font.GetCharacterInfo(ch, out info)) {
					info = text.font.characterInfo.Where(x=>x.index == ch).FirstOrDefault();
				}

				var boundingBox = new Rect(0.0f
										, (text.font.ascent - ((text.font.lineHeight - text.font.fontSize) * 0.5f)) * text.fontSize / (float)text.font.fontSize - text.fontSize
										, text.fontSize
										, text.fontSize
										);
				var charGlyph = new Rect(info.minX * text.fontSize / (float)info.size
										, info.minY * text.fontSize / (float)info.size
										, (info.maxX - info.minX) * text.fontSize / (float)info.size
										, (info.maxY - info.minY) * text.fontSize / (float)info.size
										);
				var upperRightPosition = vertices[indices[srcIndex + 1]].position;
				var lowerLeftPosition = vertices[indices[srcIndex + 4]].position;
				var charDraw = new Rect(lowerLeftPosition.x
										, lowerLeftPosition.y
										, upperRightPosition.x - lowerLeftPosition.x
										, upperRightPosition.y - lowerLeftPosition.y
										);

				var scale = new Vector2(rectTransformRect.size.x / boundingBox.size.x
										, rectTransformRect.size.y / boundingBox.size.y
										);

				var srcStartIndex = indices[srcIndex];
				for (var k = 0; k < 4; ++k) {
					var vertex = vertices[srcStartIndex + k];
					var pos2d = Vector2.Scale((Vector2)vertex.position - charDraw.min + (charGlyph.min - boundingBox.min), scale) + rectTransformRect.min;
					vertex.position = new Vector3(pos2d.x, pos2d.y, vertex.position.z);
					rectVertices.Add(vertex);
				}
				for (var k = 0; k < 6; ++k) {
					rectIndices.Add(indices[srcIndex] - srcStartIndex);
					++srcIndex;
				}

				VertexUtility.Intersect(rectVertices, rectIndices, mask, blend);
				
				var scaleInverse = new Vector2(boundingBox.size.x / rectTransformRect.size.x
										, boundingBox.size.y / rectTransformRect.size.y
										);

				vertices.AddRange(rectVertices.Select(x=>{
					var pos2d = Vector2.Scale((Vector2)x.position - rectTransformRect.min, scaleInverse) - (charGlyph.min - boundingBox.min) + charDraw.min;
					x.position = new Vector3(pos2d.x, pos2d.y, x.position.z);
					return x;
				}));
				indices.AddRange(rectIndices.Select(x=>x+dstIndex));
				dstIndex += rectVertices.Count;

				rectVertices.RemoveRange(0, rectVertices.Count);
				rectIndices.RemoveRange(0, rectIndices.Count);
			}
			vertices.RemoveRange(0, originalVerticesCount);
			indices.RemoveRange(0, originalIndicesCount);
		}
	}
}
