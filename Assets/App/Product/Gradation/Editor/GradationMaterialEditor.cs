using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Array = System.Array;

[CustomEditor(typeof(GradationMaterial))]
public class GradationMaterialEditor : Editor {
	private GUIContent m_EditorWindowContent;
	private GUIContent m_PreviewTitleContent;
	private SerializedProperty m_ColorBlend;
	private GUIContent m_ColorBlendContent;
	private SerializedProperty m_AlphaBlend;
	private GUIContent m_AlphaBlendContent;
	private SerializedProperty m_Keys;
	private GUIContent m_KeysContent;

	protected virtual void OnEnable() {
		m_EditorWindowContent = new GUIContent("Gradation Editor");
		m_PreviewTitleContent = new GUIContent(target.name);
		m_ColorBlendContent = new GUIContent("Color Blend");
		m_ColorBlend = serializedObject.FindProperty("m_ColorBlend");
		m_AlphaBlendContent = new GUIContent("Alpha Blend");
		m_AlphaBlend = serializedObject.FindProperty("m_AlphaBlend");
		m_KeysContent = new GUIContent("Keys");
		m_Keys = serializedObject.FindProperty("m_Keys");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.PropertyField(m_ColorBlend, m_ColorBlendContent, true);
		EditorGUILayout.PropertyField(m_AlphaBlend, m_AlphaBlendContent, true);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(m_EditorWindowContent, GUILayout.MaxWidth(105))) {
			GradationWindow.Instantiate((GradationMaterial)target);
		}
		GUILayout.EndHorizontal();

		EditorGUILayout.PropertyField(m_Keys, m_KeysContent, true);

		serializedObject.ApplyModifiedProperties();
	}

	public override bool HasPreviewGUI() {
		return true;
	}

	public override GUIContent GetPreviewTitle() {
		return m_PreviewTitleContent;
	}

	public override void OnPreviewGUI(Rect r, GUIStyle background) {
		base.OnPreviewGUI(r, background);
		if (Event.current.type != EventType.Repaint) {
			return;
		}

		var drawShortLength = Mathf.Min(r.width, r.height);
		var drawSize = drawShortLength * ((40.0f < drawShortLength)? 1.0f: 4.0f) * Vector2.one;
		var previewTexture = GradationMaterialEditor.CreatePreviewTexture2D((GradationMaterial)target, drawSize, Color.white, Color.gray);

		var drawRect = new Rect(r.center - drawShortLength * 0.5f * Vector2.one
								, drawShortLength * Vector2.one
								);
		GUI.DrawTexture(drawRect, previewTexture);
	}

	public static Texture2D CreatePreviewTexture2D(GradationMaterial material, Vector2 size, Color lightColor, Color darkColor) {
		return CreatePreviewTexture2D(material, size, size, lightColor, darkColor);
	}
	public static Texture2D CreatePreviewTexture2D(GradationMaterial material, Vector2 textureSize, Vector2 drawSize, Color lightColor, Color darkColor) {
		var previewRender = new PreviewRenderUtility();
		previewRender.m_Camera.orthographic = true;
		previewRender.m_Camera.orthographicSize = 1.0f;
		previewRender.m_Camera.nearClipPlane = 0.0f;
		previewRender.m_Camera.farClipPlane = 2.0f;
		var previewMaterial = new Material(Shader.Find("Sprites/Default"));

		Rect r = new Rect(Vector2.zero, textureSize);
		previewRender.BeginStaticPreview(r);

		var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(previewRender.m_Camera.aspect, 1.0f, 1.0f));
		var mesh = CreateMesh(material, drawSize, lightColor, darkColor);
		previewRender.DrawMesh(mesh, matrix, previewMaterial, 0);

		previewRender.m_Camera.Render();
		var result = previewRender.EndStaticPreview();
		return result;
	}

	public static Mesh CreateMesh(GradationMaterial material) {
		var result = new Mesh();

		var grid = material.GetGrid();
		var vertexCount = grid.xThresholds.Length * grid.yThresholds.Length;
		
		var vertices = new Vector3[vertexCount];
		for (int y = 0, yMax = grid.yThresholds.Length; y < yMax; ++y) {
			for (int x = 0, xMax = grid.xThresholds.Length; x < xMax; ++x) {
				var index = x + y * grid.xThresholds.Length;
				vertices[index] = new Vector3(grid.xThresholds[x] * 2.0f - 1.0f
											, 1.0f - grid.yThresholds[y] * 2.0f
											, 0.0f
											);
			}
		}
		result.vertices = vertices;

		result.colors = grid.colors;

		var indices = new int[6 * (grid.yThresholds.Length - 1) * (grid.xThresholds.Length - 1)];
		for (int y = 0, yMax = grid.yThresholds.Length - 1; y < yMax; ++y) {
			for (int x = 0, xMax = grid.xThresholds.Length - 1; x < xMax; ++x) {
				var index = x + y * (grid.xThresholds.Length - 1);
				var upperLeftIndex = x + y * grid.xThresholds.Length;
				var upperRightIndex = upperLeftIndex + 1;
				var lowerLeftIndex = upperLeftIndex + grid.xThresholds.Length;
				var lowerRightIndex = lowerLeftIndex + 1;
				indices[index * 6 + 0] = upperLeftIndex;
				indices[index * 6 + 1] = upperRightIndex;
				indices[index * 6 + 2] = lowerRightIndex;
				indices[index * 6 + 3] = lowerRightIndex;
				indices[index * 6 + 4] = lowerLeftIndex;
				indices[index * 6 + 5] = upperLeftIndex;
			}
		}
		result.SetIndices(indices, MeshTopology.Triangles, 0);

		return result;
	}

	public static Mesh CreateMesh(GradationMaterial material, Vector2 drawSize, Color lightColor, Color darkColor) {
		var result = CreateMesh(material);
		if (material.keys.Any(x=>x.color.a < 1.0f)) {
			var cellSize = new Vector2(32.0f / drawSize.x, 32.0f / drawSize.y);
			var checkerMesh = CreateCheckerMesh(new Rect(-1.0f, -1.0f, 2.0f, 2.0f), cellSize, Color.white, Color.gray);

			var vertices = new Vector3[result.vertices.Length + checkerMesh.vertices.Length];
			Array.Copy(checkerMesh.vertices, 0, vertices, 0, checkerMesh.vertices.Length);
			Array.Copy(result.vertices, 0, vertices, checkerMesh.vertices.Length, result.vertices.Length);

			var colors = new Color[result.colors.Length + checkerMesh.colors.Length];
			Array.Copy(checkerMesh.colors, 0, colors, 0, checkerMesh.colors.Length);
			Array.Copy(result.colors, 0, colors, checkerMesh.colors.Length, result.colors.Length);

			var indices = checkerMesh.GetIndices(0)
									.Concat(result.GetIndices(0)
													.Select(x=>x + checkerMesh.vertices.Length)
											)
									.ToArray();

			result.Clear();
			result.vertices = vertices;
			result.colors = colors;
			result.SetIndices(indices, MeshTopology.Triangles, 0);
		}
		return result;
	}

	private static Mesh CreateCheckerMesh(Rect r, Vector2 cellSize, Color lightColor, Color darkColor) {
		var result = new Mesh();

		var vertexXCount = Mathf.CeilToInt(r.width / cellSize.x) + 1;
		var vertexYCount = Mathf.CeilToInt(r.height / cellSize.y) + 1;
		var vertexCount = vertexXCount * vertexYCount;

		var vertices = new Vector3[vertexCount + 4];
		vertices[0] = new Vector3(r.xMin, r.yMax, 0.0f);
		vertices[1] = new Vector3(r.xMax, r.yMax, 0.0f);
		vertices[2] = new Vector3(r.xMin, r.yMin, 0.0f);
		vertices[3] = new Vector3(r.xMax, r.yMin, 0.0f);
		for (int y = 0, yMax = vertexYCount; y < yMax; ++y) {
			for (int x = 0, xMax = vertexXCount; x < xMax; ++x) {
				var positon = new Vector3(r.xMax, r.yMin, 0.0f);
				if (x < (vertexXCount - 1)) {
					positon.x = cellSize.x * x + r.xMin;
				}
				if (y < (vertexYCount - 1)) {
					positon.y = r.yMax - cellSize.y * y;
				}
				var index = x + y * vertexXCount + 4;
				vertices[index] = positon;
			}
		}
		result.vertices = vertices;

		var colors = new Color[vertexCount + 4];
		for (int i = 0, iMax = 4; i < iMax; ++i) {
			colors[i] = lightColor;
		}
		for (int i = 4, iMax = vertexCount + 4; i < iMax; ++i) {
			colors[i] = darkColor;
		}
		result.colors = colors;

		var indices = new int[((vertexXCount - 1) * (vertexYCount - 1) / 2 + 1) * 6];
		{
			Array.Copy(new[]{0, 1, 2, 1, 3, 2}, indices, 6);
			var index = 6;
			for (int y = 0, yMax = vertexYCount - 1; y < yMax; ++y) {
				for (int x = 0, xMax = vertexXCount - 1; x < xMax; ++x) {
					if (((x + y) & 0x01) == 1) {
						var upperLeftIndex = x + y * vertexXCount + 4;
						var upperRightIndex = upperLeftIndex + 1;
						var lowerRightIndex = upperRightIndex + vertexXCount;
						var lowerLeftIndex = upperLeftIndex + vertexXCount;
						indices[index++] = upperLeftIndex;
						indices[index++] = upperRightIndex;
						indices[index++] = lowerLeftIndex;
						indices[index++] = upperRightIndex;
						indices[index++] = lowerRightIndex;
						indices[index++] = lowerLeftIndex;
					}
				}
			}
		}
		result.SetIndices(indices, MeshTopology.Triangles, 0);

		return result;
	}
}
