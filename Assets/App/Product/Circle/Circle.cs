using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class Circle : Graphic, ICanvasRaycastFilter {

	[SerializeField] private Sprite m_Sprite = null;
	public Sprite sprite {get{return m_Sprite;} set{if (m_Sprite != value) {m_Sprite = value; SetMaterialDirty();}}}

	[SerializeField] [Range(3, 120)] private int m_VertexCount = 16;
	public int vertexCount {get{return m_VertexCount;} set{if (m_VertexCount != value) {m_VertexCount = value; SetVerticesDirty();}}}

	[SerializeField] private bool m_Distortion = false;
	public bool distortion {get{return m_Distortion;} set{if (m_Distortion!= value) {m_Distortion = value; SetVerticesDirty();}}}

	protected Circle() {
	}

	public override Texture mainTexture {get{
			return ((sprite == null)? s_WhiteTexture: sprite.texture);
	}}

	protected override void OnFillVBO(List<UIVertex> vbo) {
		var vert = UIVertex.simpleVert;
		vert.color = color;

		var pos = GetPixelAdjustedRect();
		var uvVector4 = ((sprite != null)? UnityEngine.Sprites.DataUtility.GetOuterUV(sprite): Vector4.zero);
		var uv = new Rect(uvVector4.x, uvVector4.y, uvVector4.z - uvVector4.x, uvVector4.w - uvVector4.y);

		System.Func<float, float> DistortionFunction;
		if (distortion) {
			DistortionFunction = x=>{
				x = Mathf.PingPong(x, Mathf.PI * 0.25f);
				return 1.0f / Mathf.Cos(x);
			};
		} else {
			DistortionFunction = x=>{
				return 1.0f;
			};
		}

		var halfSize = pos.size * 0.5f;
		var countToRad = Mathf.PI * 2.0f / vertexCount;
		var vertices = Enumerable.Range(0, vertexCount + 1)
								.Select(x=>x * countToRad)
								.Select(x=>new{Sin = Mathf.Sin(x), Cos = Mathf.Cos(x), Distortion = DistortionFunction(x)})
								.Select(x=>{
									vert.position = new Vector3(x.Sin * halfSize.x, x.Cos * halfSize.y);
									vert.uv0 = new Vector2(x.Sin * 0.5f * uv.width * x.Distortion + uv.center.x, x.Cos * 0.5f * uv.height * x.Distortion + uv.center.y);
									return vert;
								})
								.ToArray();

		var centerVertex = vert;
		centerVertex.position = pos.center;
		centerVertex.uv0 = uv.center;

		int vertexIndex;
		for (vertexIndex = 2; vertexIndex <= vertexCount; vertexIndex += 2) {
			vbo.Add(centerVertex);
			vbo.Add(vertices[vertexIndex - 2]);
			vbo.Add(vertices[vertexIndex - 1]);
			vbo.Add(vertices[vertexIndex - 0]);
		}
		if (vertexIndex-1 <= vertexCount) {
			vbo.Add(centerVertex);
			vbo.Add(vertices[vertexIndex - 2]);
			vbo.Add(vertices[vertexIndex - 1]);
			vbo.Add(vertices[vertexIndex - 1]);
		}
	}

	public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screenPoint, eventCamera, out localPoint);
		var pos = GetPixelAdjustedRect();
		localPoint = new Vector2(localPoint.x / pos.width, localPoint.y / pos.height);
		return localPoint.sqrMagnitude <= 0.25f;
	}
}
