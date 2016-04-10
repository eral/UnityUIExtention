using UnityEngine;

namespace UIExtention {
	public class RectTransformTextMapping : BaseTextMapping {
		public override GradationMaterial.Grid  GetGridUnit(int[] rectangleIndices, Vector2[] rectangleNormalizePositions) {
			return grid;
		}
	}
}
