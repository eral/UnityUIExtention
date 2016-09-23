using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UIExtention {
	[CreateAssetMenu]
	public class GradationMaterial : ScriptableObject {
		public enum Blend {
			Multiply,
			Override,
			Ignore,
			Add,
			Subtract,
			Screen,
			HardLight,  //(Multiply + Screen)
			HardLightFast, //(Multiply + Add)
			Darken,
			Lighten,
		}

		public enum TextMapping {
			RectTransform,
			Line,
			Character,
		}

		[System.Serializable]
		public struct Key {
			public Vector2 position;
			public Color color;

			public static Key identity {get{
				return new Key {position = Vector2.zero, color = Color.white};
			}}
		}

		[SerializeField]
		private Blend m_ColorBlend = Blend.Multiply;
		public Blend colorBlend {get{return m_ColorBlend;} set{m_ColorBlend = value;}}

		[SerializeField]
		private Blend m_AlphaBlend = Blend.Multiply;
		public Blend alphaBlend {get{return m_AlphaBlend;} set{m_AlphaBlend = value;}}

		[SerializeField]
		private TextMapping m_TextMapping = TextMapping.RectTransform;
		public TextMapping textMapping {get{return m_TextMapping;} set{m_TextMapping = value;}}

		[SerializeField]
		private List<Key> m_Keys = new List<Key>{new Key {position = new Vector2(0.0f, 0.0f), color = Color.white}
												, new Key {position = new Vector2(0.0f, 1.0f), color = Color.white}
												, new Key {position = new Vector2(1.0f, 1.0f), color = Color.white}
												, new Key {position = new Vector2(1.0f, 0.0f), color = Color.white}
												};
		public List<Key> keys {get{return m_Keys;}}

		public Mesh GetMesh() {
			var supportCornerKeys = GetSupportCornerKeys();
			var verticesCount = keys.Count + ((supportCornerKeys != null)? supportCornerKeys.Count: 0);
			var vertices = new Vector3[verticesCount];
			var colors = new Color32[verticesCount];
			
			var verticesIndex = 0;
			foreach (var key in keys) {
				vertices[verticesIndex] = key.position;
				colors[verticesIndex] = key.color;
				++verticesIndex;
			}
			if (supportCornerKeys != null) {
				foreach (var key in supportCornerKeys) {
					vertices[verticesIndex] = key.position;
					colors[verticesIndex] = key.color;
					++verticesIndex;
				}
			}

			var index = VertexUtility.Triangulation(vertices.Select(x=>(Vector2)x).ToList());

			var result = new Mesh();
			result.vertices = vertices;
			result.colors32 = colors;
			result.SetTriangles(index, 0);
			return result;
		}

		private List<Key> GetSupportCornerKeys() {
			const int lowerLeft = 0, upperLeft = 1, upperRight = 2, lowerRight = 3;

			List<Key> result = null;
			var existKey = 0;
			foreach (var key in keys) {
				var position = key.position;
				if (position.x == 0.0f) {
					if (position.y == 0.0f) {
						existKey |= 1 << lowerLeft;
					} else if (position.y == 1.0f) {
						existKey |= 1 << upperLeft;
					}
				} else if (position.x == 1.0f) {
					if (position.y == 0.0f) {
						existKey |= 1 << lowerRight;
					} else if (position.y == 1.0f) {
						existKey |= 1 << upperRight;
					}
				}
			}
			var existKeyCount = (existKey & 0x5) + ((existKey & 0xA) >> 1);
			existKeyCount = (existKeyCount >> 2) + (existKeyCount & 0x3);
			if (existKeyCount < 4) {
				Vector2 lowerLeftPosition = new Vector2(0.0f, 0.0f), upperLeftPosition = new Vector2(0.0f, 1.0f), upperRightPosition = new Vector2(1.0f, 1.0f), lowerRightPosition = new Vector2(1.0f, 0.0f);
				int lowerLeftKeyIndex = 0, upperLeftKeyIndex = 0, upperRightKeyIndex = 0, lowerRightKeyIndex = 0;
				float lowerLeftSqrMagnitude = float.PositiveInfinity, upperLeftSqrMagnitude = float.PositiveInfinity, upperRightSqrMagnitude = float.PositiveInfinity, lowerRightSqrMagnitude = float.PositiveInfinity;
				for (int i = 0, iMax = keys.Count; i < iMax; ++i) {
					var position = keys[i].position;
					if ((existKey & (1 << lowerLeft)) == 0) {
						var sqrMagnitude = Vector2.SqrMagnitude(position - lowerLeftPosition);
						if (sqrMagnitude < lowerLeftSqrMagnitude) {
							lowerLeftSqrMagnitude = sqrMagnitude;
							lowerLeftKeyIndex = i;
						} 
					}
					if ((existKey & (1 << upperLeft)) == 0) {
						var sqrMagnitude = Vector2.SqrMagnitude(position - upperLeftPosition);
						if (sqrMagnitude < upperLeftSqrMagnitude) {
							upperLeftSqrMagnitude = sqrMagnitude;
							upperLeftKeyIndex = i;
						} 
					}
					if ((existKey & (1 << upperRight)) == 0) {
						var sqrMagnitude = Vector2.SqrMagnitude(position - upperRightPosition);
						if (sqrMagnitude < upperRightSqrMagnitude) {
							upperRightSqrMagnitude = sqrMagnitude;
							upperRightKeyIndex = i;
						} 
					}
					if ((existKey & (1 << lowerRight)) == 0) {
						var sqrMagnitude = Vector2.SqrMagnitude(position - lowerRightPosition);
						if (sqrMagnitude < lowerRightSqrMagnitude) {
							lowerRightSqrMagnitude = sqrMagnitude;
							lowerRightKeyIndex = i;
						} 
					}
				}
				result = new List<Key>(4 - existKeyCount);
				if ((existKey & (1 << lowerLeft)) == 0) {
					result.Add(new Key(){position = lowerLeftPosition, color = keys[lowerLeftKeyIndex].color});
				}
				if ((existKey & (1 << upperLeft)) == 0) {
					result.Add(new Key(){position = upperLeftPosition, color = keys[upperLeftKeyIndex].color});
				}
				if ((existKey & (1 << upperRight)) == 0) {
					result.Add(new Key(){position = upperRightPosition, color = keys[upperRightKeyIndex].color});
				}
				if ((existKey & (1 << lowerRight)) == 0) {
					result.Add(new Key(){position = lowerRightPosition, color = keys[lowerRightKeyIndex].color});
				}
			}
			return result;
		}
	}
}
