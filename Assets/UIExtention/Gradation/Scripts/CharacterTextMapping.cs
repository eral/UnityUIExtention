using UnityEngine;
using System.Linq;
using RectangleIndex = UIExtention.Gradation.RectangleIndex;

namespace UIExtention {
	public class CharacterTextMapping : BaseTextMapping {
		private int index;
		private Vector2 normalizeWeight;

		protected override void Initialize() {
			index = 0;
			normalizeWeight = new Vector2(1.0f/ (localCorners[(int)RectangleIndex.UpperRight].x - localCorners[(int)RectangleIndex.LowerLeft].x)
										, 1.0f/ (localCorners[(int)RectangleIndex.UpperRight].y - localCorners[(int)RectangleIndex.LowerLeft].y)
										);
		}

		public override GradationMaterial.Grid  GetGridUnit(int[] rectangleIndices, Vector2[] rectangleNormalizePositions) {
			var ch = text.text[index];
			CharacterInfo info;
			if (!text.font.GetCharacterInfo(ch, out info)) {
				info = text.font.characterInfo.Where(x=>x.index == ch).FirstOrDefault();
			}
			var fontHeight = text.fontSize;
			var fontWidth = text.fontSize;
			var lineFontMaxY = text.font.ascent * text.fontSize / (float)text.font.fontSize;
			var charMinX = info.minX * text.fontSize / (float)info.size;
			var charMaxY = info.maxY * text.fontSize / (float)info.size;

			var rect = new Rect(rectangleNormalizePositions[(int)RectangleIndex.LowerLeft].x - charMinX * normalizeWeight.x
								, rectangleNormalizePositions[(int)RectangleIndex.LowerLeft].y - (lineFontMaxY - charMaxY) * normalizeWeight.y
								, fontWidth * normalizeWeight.x
								, fontHeight * normalizeWeight.y
								);
			var result = GradationMaterial.Grid.Resize(grid, rect);

			index++;
			return result;
		}
	}
}
