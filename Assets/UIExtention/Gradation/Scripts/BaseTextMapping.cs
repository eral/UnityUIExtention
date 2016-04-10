using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UIExtention {
	public abstract class BaseTextMapping {
		public RectTransform rectTransform {get; private set;}
		public Text text {get; private set;}
		public List<UIVertex> vertices {get; private set;}
		public List<int> indices {get; private set;}
		public GradationMaterial.Grid grid {get; private set;}
		public Vector3[] localCorners {get; private set;}

		public void Initialize(RectTransform rectTransform, Text text, List<UIVertex> vertices, List<int> indices, GradationMaterial.Grid grid, Vector3[] localCorners) {
			this.rectTransform = rectTransform;
			this.text = text;
			this.vertices = vertices;
			this.indices = indices;
			this.grid = grid;
			this.localCorners = localCorners;
			Initialize();
		}

		protected virtual void Initialize() {}
		public virtual void PrePass(VertexHelper vh) {}
		public abstract GradationMaterial.Grid GetGridUnit(int[] rectangleIndices, Vector2[] rectangleNormalizePositions);
		public virtual void PostPass(VertexHelper vh) {}
	}
}
