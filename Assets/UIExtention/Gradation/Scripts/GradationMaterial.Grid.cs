using UnityEngine;
using System.Linq;
using Array = System.Array;

namespace UIExtention {
	public partial class GradationMaterial : ScriptableObject {
		public class Grid {
			[SerializeField] private GradationMaterial m_Material;
			private GradationMaterial material {get{return m_Material;} set{m_Material = value;}}

			[SerializeField] private float[] m_XThresholds;
			public float[] xThresholds {get{return m_XThresholds;} private set{m_XThresholds = value;}}
		
			[SerializeField] private float[] m_YThresholds;
			public float[] yThresholds {get{return m_YThresholds;} private set{m_YThresholds = value;}}
		
			[SerializeField] private Color[] m_Colors;
			public Color[] colors {get{return m_Colors;} private set{m_Colors = value;}}
		
			public int count {get{return m_Colors.Length;}}

			public Grid(GradationMaterial material) {
				this.material = material;
				xThresholds = material.keys.Select(x=>x.position.x)
											.Concat(new[]{0.0f, 1.0f})
											.OrderBy(x=>x)
											.Distinct()
											.ToArray();
				yThresholds = material.keys.Select(x=>x.position.y)
											.Concat(new[]{0.0f, 1.0f})
											.OrderBy(x=>x)
											.Distinct()
											.ToArray();
				colors = GetColors();
			}

			public int GetIndex(int x, int y) {
				if ((xThresholds.Length < x) || (yThresholds.Length < y)) {
					throw new System.ArgumentOutOfRangeException();
				}
				var result = x + y * xThresholds.Length;
				return result;
			}

			public Vector2 GetPosition(int index) {
				var x = index % xThresholds.Length;
				var y = index / xThresholds.Length;
				return GetPosition(x, y);
			}
			public Vector2 GetPosition(int x, int y) {
				var result = new Vector2(xThresholds[x], yThresholds[y]);
				return result;
			}

			public Color GetColor(int index) {
				var result = new Color();
				if (index < colors.Length) {
					result = colors[index];
				}
				return result;
			}
			public Color GetColor(int x, int y) {
				return GetColor(GetIndex(x, y));
			}

			private Color[] GetColors() {
				var keysCount = xThresholds.Length * yThresholds.Length;

				if (material.keys.Count == 0) {
					//有点キーが無いなら
					return Enumerable.Repeat(Color.white, keysCount).ToArray();
				}

				var validFlags = new bool[keysCount];
				var emptyCount = keysCount;
				var result = new Color[keysCount];

				//有点キー設定
				foreach (var key in material.keys) {
					var xIndex = Array.BinarySearch<float>(xThresholds, key.position.x);
					var yIndex = Array.BinarySearch<float>(yThresholds, key.position.y);
					var index = xIndex + yIndex * xThresholds.Length;
					result[index] = key.color;
					validFlags[index] = true;
					--emptyCount;
				}

				if (emptyCount == 0) {
					return result;
				}

				//角の無点キー補間
				emptyCount -= InterpolateCorner(new Vector2(0.0f, 0.0f), result, validFlags);
				emptyCount -= InterpolateCorner(new Vector2(1.0f, 0.0f), result, validFlags);
				emptyCount -= InterpolateCorner(new Vector2(1.0f, 1.0f), result, validFlags);
				emptyCount -= InterpolateCorner(new Vector2(0.0f, 1.0f), result, validFlags);

				if (emptyCount == 0) {
					return result;
				}

				//外周の無点キー補間
				emptyCount -= InterpolateLine(0
											, xThresholds.Length
											, 1
											, x=>xThresholds[x % xThresholds.Length]
											, result
											, validFlags
											);
				emptyCount -= InterpolateLine(xThresholds.Length * (yThresholds.Length - 1)
											, keysCount
											, 1
											, x=>xThresholds[x % xThresholds.Length]
											, result
											, validFlags
											);
				emptyCount -= InterpolateLine(0
											, keysCount
											, xThresholds.Length
											, y=>yThresholds[y / xThresholds.Length]
											, result
											, validFlags
											);
				emptyCount -= InterpolateLine(xThresholds.Length - 1
											, keysCount
											, xThresholds.Length
											, y=>yThresholds[y / xThresholds.Length]
											, result
											, validFlags
											);

				if (emptyCount == 0) {
					return result;
				}

				//無点キー補間
				for (int i = 0, iMax = keysCount; i < iMax; ++i) {
					emptyCount -= InterpolateCross(i, result, validFlags);
				}

				return result;
			}

			private int InterpolateCorner(Vector2 position, Color[] colors, bool[] validFlags) {
				var result = 0;

				var xIndex = Array.BinarySearch<float>(xThresholds, position.x);
				var yIndex = Array.BinarySearch<float>(yThresholds, position.y);
				var index = xIndex + yIndex * xThresholds.Length;
				if (!validFlags[index]) {
					var nearestKey = material.keys.OrderBy(x=>(x.position - position).sqrMagnitude)
										.First();
					var nearestXIndex = Array.BinarySearch<float>(xThresholds, nearestKey.position.x);
					var nearestYIndex = Array.BinarySearch<float>(yThresholds, nearestKey.position.y);

					for (int y = Mathf.Min(yIndex, nearestYIndex), yMax = Mathf.Max(yIndex, nearestYIndex) + 1; y < yMax; ++y) {
						for (int x = Mathf.Min(xIndex, nearestXIndex), xMax = Mathf.Max(xIndex, nearestXIndex) + 1; x < xMax; ++x) {
							var i = x + y * xThresholds.Length;
							if (!validFlags[i]) {
								colors[i] = nearestKey.color;
								validFlags[i] = true;
								++result;
							}
						}
					}

				}

				return result;
			}

			private static int InterpolateLine(int begin, int end, int slide, System.Func<int, float> getThreshold, Color[] colors, bool[] validFlags) {
				var result = 0;
				var prevIndex = begin - slide;
				for (int i = begin, iMax = end; i < iMax; i += slide) {
					if (validFlags[i]) {
						for (int k = prevIndex + slide, kMax = i; k < kMax; k += slide) {
							if (!validFlags[k]) {
								if (begin <= prevIndex) {
									var f = Mathf.InverseLerp(getThreshold(prevIndex), getThreshold(i), getThreshold(k));
									colors[k] = Color.Lerp(colors[prevIndex], colors[i], f);
								} else {
									colors[k] = colors[i];
								}
								validFlags[k] = true;
								++result;
							}
						}
						prevIndex = i;
					}
				}
				for (int x = prevIndex + slide, xMax = end; x < xMax; x += slide) {
					colors[x] = colors[prevIndex];
					validFlags[x] = true;
					++result;
				}
				return result;
			}

			private int InterpolateCross(int index, Color[] colors, bool[] validFlags) {
				var result = 0;
				if (!validFlags[index]) {
					var position = new Vector2(xThresholds[index % xThresholds.Length]
											, yThresholds[index / xThresholds.Length]
											);
					var upperLeftKey = new Key{position = new Vector2(0.0f, 0.0f), color = colors[0]};
					var upperRightKey = new Key{position = new Vector2(1.0f, 0.0f), color = colors[xThresholds.Length - 1]};
					var lowerRightKey = new Key{position = new Vector2(1.0f, 1.0f), color = colors[colors.Length - 1]};
					var lowerLeftKey = new Key{position = new Vector2(0.0f, 1.0f), color = colors[colors.Length - xThresholds.Length]};

					foreach (var key in material.keys) {
						if (key.position.y <= position.y) {
							if (key.position.x <= position.x) {
								//UpperLeft
								if ((key.position - position).sqrMagnitude < (upperLeftKey.position - position).sqrMagnitude) {
									upperLeftKey = key;
								}
							}
							if (position.x <= key.position.x) {
								//UpperRight
								if ((key.position - position).sqrMagnitude < (upperRightKey.position - position).sqrMagnitude) {
									upperRightKey = key;
								}
							}
						}
						if (position.y <= key.position.y) {
							if (key.position.x <= position.x) {
								//LowerLeft
								if ((key.position - position).sqrMagnitude < (lowerLeftKey.position - position).sqrMagnitude) {
									lowerLeftKey = key;
								}
							}
							if (position.x <= key.position.x) {
								//LowerRight
								if ((key.position - position).sqrMagnitude < (lowerRightKey.position - position).sqrMagnitude) {
									lowerRightKey = key;
								}
							}
						}
					}

					var upperLeftDistance = 1.0f / (upperLeftKey.position - position).magnitude;
					var upperRightDistance = 1.0f / (upperRightKey.position - position).magnitude;
					var lowerRightDistance = 1.0f / (lowerRightKey.position - position).magnitude;
					var lowerLeftDistance = 1.0f / (lowerLeftKey.position - position).magnitude;
					var distance = upperLeftDistance + upperRightDistance + lowerRightDistance + lowerLeftDistance;
					colors[index] = (upperLeftKey.color * upperLeftDistance / distance)
								+ (upperRightKey.color * upperRightDistance / distance)
								+ (lowerRightKey.color * lowerRightDistance / distance)
								+ (lowerLeftKey.color * lowerLeftDistance / distance)
								;
					validFlags[index] = true;
					++result;
				}
				return result;
			}
		}
	}
}
