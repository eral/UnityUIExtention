using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace UIExtention {
	public class GradationWindow : EditorWindow {

		[SerializeField]
		private int m_Target = -1;
		private GradationMaterial target {
			get {
				return ((m_Target != -1)? AssetDatabase.LoadAssetAtPath<GradationMaterial>(AssetDatabase.GetAssetPath(m_Target)): null);
			}
			set {
				if (value != null) {
					m_Target = value.GetInstanceID();
					m_Material = ScriptableObject.CreateInstance<GradationMaterial>();
					EditorUtility.CopySerialized(value, m_Material);
				} else {
					m_Target = -1;
					m_Material = null;
				}
				m_Focus = null;
				m_Dirty = false;
			}
		}

		[SerializeField]
		private GradationMaterial m_Material = null;
		private GradationMaterial material {get{return m_Material;}}

		[SerializeField]
		private bool m_Dirty = false;

		private int[] m_Focus;

		private bool m_Snap = false;
		private bool m_SnapInfo = false;
		private bool m_SnapCross = false;
		private int m_SnapDivideGrid = 4;

		private Vector2 m_Scroll;
		private float m_Scale;

		private PreviewRenderUtility m_PreviewRender = null;
		private PreviewRenderUtility previewRender {get{
			return m_PreviewRender ?? (m_PreviewRender = new PreviewRenderUtility());
		}}

		private const float k_NaturalScale = 0.5f;	//x1.0で表示させた時のウインドウに占めるグラデーションマップが占める割合
		private const float k_ScrollMargin = 0.8f;	//グラデーションマップ外側にスクロール出来る量(ウインドウサイズからの比率指定)
		private const float k_MakerRadius = 10.0f;	//マーカー半径
		private const float k_SnapRange = 6.0f;	//スナップ範囲

		public static GradationWindow Instantiate(GradationMaterial target) {
			var result = EditorWindow.GetWindow<GradationWindow>();
			result.target = target;
			result.Show();
			return result;
		}

		private void DisplayApplyDialog() {
			var assetPath = AssetDatabase.GetAssetPath(m_Target);
			if (EditorUtility.DisplayDialog("Unapplied import settings", "Unapplied import settings for '" + assetPath + "'", "Apply", "Revert")) {
				Apply();
			}
		}

		private void Apply() {
			var targetMaterial = this.target;

			Undo.RecordObjects(new Object[]{this, targetMaterial}, "apply gradation material");
			EditorUtility.CopySerialized(material, targetMaterial);
			EditorUtility.SetDirty(targetMaterial);
			m_Dirty = false;
			EditorUtility.SetDirty(this);
		}

		private void OnSelectionChange() {
			if (m_Target != Selection.activeInstanceID) {
				if ((material != null) && m_Dirty) {
					DisplayApplyDialog();
				}
			}
			target = Selection.activeObject as GradationMaterial;
			m_Focus = null;
			Repaint();
		}

		protected virtual void OnEnable() {
			Undo.undoRedoPerformed += OnUndoRedo;

			titleContent = new GUIContent("Gradation Editor");

			m_Scale = 1.0f;
			var roughlyTargetHalfSize = Mathf.Min(position.size.x, position.size.y) * m_Scale * k_NaturalScale * 0.5f * Vector2.one;
			m_Scroll = position.size * k_ScrollMargin + roughlyTargetHalfSize;
		}

		protected virtual void OnDisable() {
			if (m_Dirty) {
				DisplayApplyDialog();
			}
			Undo.undoRedoPerformed -= OnUndoRedo;
		}

		private void OnUndoRedo() {
			if ((m_Focus != null) && m_Focus.Any(x=>material.keys.Count <= x)) {
				m_Focus = null;
			}
			Repaint();
		}

		public void OnGUI() {
			if (material == null) {
				emptyMessage();
				return;
			}

			Rect r = Toolbar();

			var targetSize = Mathf.Min(r.size.x, r.size.y) * m_Scale * k_NaturalScale * Vector2.one;
			var scrollViewSize = r.size * k_ScrollMargin - new Vector2(GUI.skin.verticalScrollbar.fixedWidth, GUI.skin.horizontalScrollbar.fixedHeight);
			var scrollViewRect = new Rect(Vector2.zero, targetSize + scrollViewSize * 2.0f);

			var scrollAndScale = new Vector3(m_Scroll.x, m_Scroll.y, m_Scale);
			EditorGUI.BeginChangeCheck();
			scrollAndScale = BeginScrollScaleView(r, scrollAndScale, scrollViewRect);
			if (EditorGUI.EndChangeCheck()) {
				m_Scroll = scrollAndScale;
				m_Scale = scrollAndScale.z;
			}

			var mapRect = new Rect(scrollViewSize, targetSize);
			BeginUpdateFocus(mapRect);
			GradationMap(mapRect);

			EndScrollScaleView();

			r.size = r.size - new Vector2(GUI.skin.verticalScrollbar.fixedWidth, GUI.skin.horizontalScrollbar.fixedHeight);
			SnapInfo(r);
			FocusInfo(r);

			EndUpdateFocus(mapRect);
		}

		private void emptyMessage() {
			var label = new GUIContent("No gradation material selected");
			var oldGUIEnabled = GUI.enabled;
			GUI.enabled = false;
			GUILayout.Label(label);
			GUI.enabled = oldGUIEnabled;
		}

		private Rect Toolbar() {
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			AddRemoveToolbar();
			GUILayout.Space(EditorStyles.toolbar.fixedHeight);
			SnapToolbar();
			GUILayout.FlexibleSpace();
			ApplyRevertToolbar();
			ScaleToolbar();
			GUILayout.EndHorizontal();

			var toolbarHeight = EditorStyles.toolbar.fixedHeight - EditorStyles.toolbar.border.bottom;
			var result = new Rect(new Vector2(0.0f, toolbarHeight)
								, new Vector2(position.size.x, position.size.y - toolbarHeight)
								);
			return result;
		}

		private void AddRemoveToolbar() {
			var add = GUILayout.Button("Add", EditorStyles.toolbarButton);
			if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Insert)) {
				add = true;
				Event.current.Use();
			}
			if (add) {
				Undo.RecordObjects(new Object[]{this, material}, "add gradation key");
				material.keys.Add(new GradationMaterial.Key(){position = new Vector2(0.5f, 0.5f), color = Color.white});
				m_Focus = new[]{material.keys.Count - 1};
				m_Dirty = true;
				EditorUtility.SetDirty(this);
			}
			var oldGUIEnabled = GUI.enabled;
			GUI.enabled = (m_Focus != null) && (0 < m_Focus.Length);
			var remove = GUILayout.Button("Remove", EditorStyles.toolbarButton);
			if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Delete)) {
				remove = true;
				Event.current.Use();
			}
			if (remove) {
				Undo.RecordObjects(new Object[]{this, material}, "remove gradation key");
				foreach (var i in m_Focus.OrderByDescending(x=>x)) {
					material.keys.RemoveAt(i);
				}
				m_Focus = null;
				m_Dirty = true;
				EditorUtility.SetDirty(this);
			}
			GUI.enabled = oldGUIEnabled;
		}

		private void SnapToolbar() {
			EditorGUI.BeginChangeCheck();
			var snapStyle = new GUIStyle(EditorStyles.toolbarButton);
			snapStyle.padding.right /= 2;
			m_Snap = GUILayout.Toggle(m_Snap, "Snap", snapStyle);
			if (EditorGUI.EndChangeCheck()) {
				m_SnapInfo = m_Snap;
			}
			EditorGUI.BeginChangeCheck();
			var snapInfoStyle = new GUIStyle(EditorStyles.toolbarButton);
			snapInfoStyle.padding.left /= 2;
			m_SnapInfo = GUILayout.Toggle(m_SnapInfo, "▾", snapInfoStyle);
			if (EditorGUI.EndChangeCheck()) {
				if (m_SnapInfo) {
					m_Snap = true;
				}
			}
		}

		private void ApplyRevertToolbar() {
			var oldGUIEnabled = GUI.enabled;
			GUI.enabled = m_Dirty;
			if (GUILayout.Button("Revert", EditorStyles.toolbarButton)) {
				Undo.RecordObject(this, "revert gradation material");
				target = target;
			}
			if (GUILayout.Button("Apply", EditorStyles.toolbarButton)) {
				Apply();
			}
			GUI.enabled = oldGUIEnabled;
		}

		private void ScaleToolbar() {
			m_Scale = HorizontalPowSliderLayout(m_Scale, 0.125f, 8.0f, 0.25f, GUILayout.MinWidth(60.0f));
			if (GUILayout.Button(m_Scale.ToString("x0.0"), EditorStyles.toolbarButton)) {
				m_Scale = 1.0f;
			}
		}

		private static float HorizontalPowSliderLayout(float value, float leftValue, float rightValue, float pow, params GUILayoutOption[] options) {
			var powValue = Mathf.Pow(value, pow);
			EditorGUI.BeginChangeCheck();
			powValue = GUILayout.HorizontalSlider(powValue, Mathf.Pow(leftValue, pow), Mathf.Pow(rightValue, pow), options);
			if (EditorGUI.EndChangeCheck()) {
				value = Mathf.Pow(powValue, 1.0f / pow);
			}
			return value;
		}
		private static Vector3 BeginScrollScaleView(Rect position, Vector3 scrollPositionAndScale, Rect viewRect) {
			var scrollPosition = (Vector2)scrollPositionAndScale;
			var scale = scrollPositionAndScale.z;

			var scrollRect = new Rect(position.position, position.size - new Vector2(GUI.skin.verticalScrollbar.fixedWidth, GUI.skin.horizontalScrollbar.fixedHeight));
			switch (Event.current.type) {
			case EventType.MouseDrag:
				var isMove = false;
				isMove = isMove || (scrollRect.Contains(Event.current.mousePosition) && (Event.current.button == 2));
				isMove = isMove || (scrollRect.Contains(Event.current.mousePosition) && (Event.current.button == 0) && (Event.current.alt));
				if (isMove) {
					scrollPosition -= Event.current.delta;
					GUI.changed = true;
					Event.current.Use();
				}
				break;
			case EventType.ScrollWheel:
				if (scrollRect.Contains(Event.current.mousePosition)) {
					scale = Mathf.Clamp(scale - Event.current.delta.y / 64.0f, 0.125f, 8.0f);
					GUI.changed = true;
					Event.current.Use();
				}
				break;
			}
			scrollPosition = GUI.BeginScrollView(position, scrollPosition, viewRect);

			scrollPositionAndScale = new Vector3(scrollPosition.x, scrollPosition.y, scale);
			return scrollPositionAndScale;
		}

		private static void EndScrollScaleView() {
			GUI.EndScrollView();
		}

		private void BeginUpdateFocus(Rect r) {
			var isUpdate = (Event.current.type == EventType.MouseDown);
			isUpdate = isUpdate && ((Event.current.button == 0) && (!Event.current.alt));
			if (!isUpdate) {
				return;
			}

			int[] focus = null;
			for(int i = 0, i_max = material.keys.Count; i < i_max; ++i) {
				var key = material.keys[i];
				var position = new Vector2(Mathf.Lerp(r.xMin, r.xMax, key.position.x)
										, Mathf.Lerp(r.yMin, r.yMax, key.position.y)
										);
				if ((Event.current.mousePosition - position).sqrMagnitude < (k_MakerRadius * k_MakerRadius)) {
					focus = new[]{i};
					break;
				}
			}
			if (focus != null) {
				m_Focus = focus;
			}
		}

		private void EndUpdateFocus(Rect r) {
			var isUpdate = (Event.current.type == EventType.MouseDown);
			isUpdate = isUpdate && ((Event.current.button == 0) && (!Event.current.alt));
			if (!isUpdate) {
				return;
			}

			int[] focus = null;
			for(int i = 0, i_max = material.keys.Count; i < i_max; ++i) {
				var key = material.keys[i];
				var position = new Vector2(Mathf.Lerp(r.xMin, r.xMax, key.position.x)
										, Mathf.Lerp(r.yMin, r.yMax, key.position.y)
										);
				if ((Event.current.mousePosition - position).sqrMagnitude < (k_MakerRadius * k_MakerRadius)) {
					focus = new[]{i};
					break;
				}
			}
			if ((m_Focus != null) && (focus == null)) {
				m_Focus = focus;
				Event.current.Use();
			}
		}

		private void GradationMap(Rect r) {
			if (Event.current.type == EventType.Repaint) {
				DrawTarget(r);
				DrawSnapLines(r);
			}

			for(int i = 0, i_max = material.keys.Count; i < i_max; ++i) {
				var isFocus = (m_Focus != null) && m_Focus.Any(x=>x == i);
				EditorGUI.BeginChangeCheck();
				var key = GradationMaker(r, material.keys[i].position, material.keys[i].color, isFocus, k_MakerRadius, GetSnapPositions(r, i));
				if (EditorGUI.EndChangeCheck()) {
					Undo.RecordObjects(new Object[]{this, material}, "change gradation key");
					material.keys[i] = new GradationMaterial.Key(){position = key, color = material.keys[i].color};
					m_Dirty = true;
					EditorUtility.SetDirty(this);
				}
			}
		}

		private void DrawSnapLines(Rect r) {
			if (!m_Snap) {
				return;
			}

			var oldHandlesColor = Handles.color;

			Handles.color = new Color(0.0f, 0.0f, 0.0f, 1.0f / 3.0f);
			if (1 <= m_SnapDivideGrid) {
				var inverseSnapDivideGrid = 1.0f / m_SnapDivideGrid;
				for (int i = 0, iMax = m_SnapDivideGrid; i < iMax; ++i) {
					var f = i * inverseSnapDivideGrid;
					var position = new Vector2(Mathf.Lerp(r.xMin, r.xMax, f)
											, Mathf.Lerp(r.yMin, r.yMax, f)
											);
					Handles.DrawLine(new Vector3(r.xMin, position.y, 0.0f), new Vector3(r.xMax, position.y, 0.0f));
					Handles.DrawLine(new Vector3(position.x, r.yMin, 0.0f), new Vector3(position.x, r.yMax, 0.0f));
				}
				Handles.DrawLine(new Vector3(r.xMin, r.yMax, 0.0f), new Vector3(r.xMax, r.yMax, 0.0f));
				Handles.DrawLine(new Vector3(r.xMax, r.yMin, 0.0f), new Vector3(r.xMax, r.yMax, 0.0f));
			}
			if (m_SnapCross) {
				foreach (var xThreshold in material.keys.Select(x=>x.position.x).Distinct()) {
					var x = Mathf.Lerp(r.xMin, r.xMax, xThreshold);
					Handles.DrawLine(new Vector3(x, r.yMin, 0.0f), new Vector3(x, r.yMax, 0.0f));
				}
				foreach (var yThreshold in material.keys.Select(x=>x.position.y).Distinct()) {
					var y = Mathf.Lerp(r.yMin, r.yMax, yThreshold);
					Handles.DrawLine(new Vector3(r.xMin, y, 0.0f), new Vector3(r.xMax, y, 0.0f));
				}
			}

			Handles.color = oldHandlesColor;
		}

		private void DrawTarget(Rect r) {
			var textureSize = new Vector2(Mathf.Min(r.width, 1024.0f), Mathf.Min(r.height, 1024.0f));
			var previewTexture = GradationMaterialEditor.CreatePreviewTexture2D(previewRender, material, textureSize, r.size, Color.white, Color.gray);
			GUI.DrawTexture(r, previewTexture);
		}

		private static Vector2 GradationMaker(Rect r, Vector2 value, Color color, bool isFocus, float radius, IEnumerable<Vector2> snapPositions) {
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			return GradationMaker(controlID, r, value, color, isFocus, radius, snapPositions);
		}
		private static Vector2 GradationMaker(int controlID, Rect r, Vector2 value, Color color, bool isFocus, float radius, IEnumerable<Vector2> snapPositions) {
			var state = (Vector2)GUIUtility.GetStateObject(typeof(Vector2), controlID);

			var position = new Vector2(Mathf.Lerp(r.xMin, r.xMax, value.x)
									, Mathf.Lerp(r.yMin, r.yMax, value.y)
									);
			switch (Event.current.GetTypeForControl(controlID)) {
			case EventType.Repaint:
				var oldHandlesColor = Handles.color;
				{
					Handles.color = ((GUIUtility.hotControl == controlID)
										? Color.yellow
										: ((isFocus)
											? Color.blue
											: Color.black
										)
									);
					Handles.DrawSolidDisc(position, Vector3.back, radius);
				}
				Handles.color = new Color(color.r, color.g, color.b);
				Handles.DrawSolidArc(position, Vector3.back, Vector3.right, 180.0f, radius - 1.0f);
				if (color.a < 1.0f) {
					Handles.color = Color.white;
					Handles.DrawSolidArc(position, Vector3.back, Vector3.left, 90.0f, radius - 1.0f);
					Handles.color = Color.black;
					Handles.DrawSolidArc(position, Vector3.back, Vector3.up, 90.0f, radius - 1.0f);
				}
				Handles.color = color;
				Handles.DrawSolidDisc(position, Vector3.back, radius - 1.0f);

				Handles.color = oldHandlesColor;
				break;
			case EventType.MouseDown:
				if ((Event.current.mousePosition - position).sqrMagnitude < (radius * radius)) {
					if (Event.current.button == 0) {
						GUIUtility.hotControl = controlID;
						state = position - Event.current.mousePosition;
						Event.current.Use();
					}
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID) {
					GUIUtility.hotControl = 0;
					state = Vector2.zero;
					Event.current.Use();
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID) {
					var movedPosition = Event.current.mousePosition + state;
					foreach (var snapPosition in snapPositions) {
						if (Mathf.Abs(movedPosition.x - snapPosition.x) < k_SnapRange) {
							movedPosition.x = snapPosition.x;
							break;
						}
					}
					foreach (var snapPosition in snapPositions) {
						if (Mathf.Abs(movedPosition.y - snapPosition.y) < k_SnapRange) {
							movedPosition.y = snapPosition.y;
							break;
						}
					}
					value = new Vector2(Mathf.InverseLerp(r.xMin, r.xMax, movedPosition.x)
										, Mathf.InverseLerp(r.yMin, r.yMax, movedPosition.y)
										);
					GUI.changed = true;
					Event.current.Use();
				}
				break;
			}

			return value;
		}

		private IEnumerable<Vector2> GetSnapPositions(Rect r, params int[] excludeKeyIndices) {
			if (!m_Snap) {
				yield break;
			}

			if (1 <= m_SnapDivideGrid) {
				var inverseSnapDivideGrid = 1.0f / m_SnapDivideGrid;
				for (int i = 0, iMax = m_SnapDivideGrid; i < iMax; ++i) {
					var f = i * inverseSnapDivideGrid;
					var result = new Vector2(Mathf.Lerp(r.xMin, r.xMax, f)
											, Mathf.Lerp(r.yMin, r.yMax, f)
											);
					yield return result;
				}
				yield return new Vector2(r.xMax, r.yMax);
			}
			if (m_SnapCross) {
				var indices = Enumerable.Range(0, material.keys.Count)
										.Except(excludeKeyIndices)
										.ToArray();
				var xThresholds = indices.Select(x=>material.keys[x])
										.Select(x=>x.position.x)
										.Distinct()
										.Select(x=>Mathf.Lerp(r.xMin, r.xMax, x))
										.ToArray();
				var yThresholds = indices.Select(x=>material.keys[x])
										.Select(x=>x.position.y)
										.Distinct()
										.Select(x=>Mathf.Lerp(r.yMin, r.yMax, x))
										.ToArray();
				for (int i = 0, iMax = Mathf.Max(xThresholds.Length, yThresholds.Length); i < iMax; ++i) {
					var result = Vector2.zero;
					if (i < xThresholds.Length) {
						result.x = xThresholds[i];
					}
					if (i < yThresholds.Length) {
						result.y = yThresholds[i];
					}
					yield return result;
				}
			}

			yield break;
		}

		private void SnapInfo(Rect r) {
			if (!m_SnapInfo) {
				return;
			}

			var windowSize = new Vector2(200.0f, 60.0f);

			EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(200.0f));
			var oldGUIEnabled = GUI.enabled;
			var oldLabelWidth = EditorGUIUtility.labelWidth;
			var oldWideMode = EditorGUIUtility.wideMode;
			EditorGUIUtility.labelWidth = 60.0f;
			EditorGUIUtility.wideMode = true;
			SnapInfoWindow();
			EditorGUIUtility.labelWidth = oldLabelWidth;
			EditorGUIUtility.wideMode = oldWideMode;
			GUI.enabled = oldGUIEnabled;
			EditorGUILayout.EndVertical();
		}

		private void SnapInfoWindow() {
			var oldGUIEnabled = GUI.enabled;

			EditorGUI.BeginChangeCheck();
			m_SnapCross = EditorGUILayout.Toggle("Cross", m_SnapCross);
			var enableGrid = 0 < m_SnapDivideGrid;
			var divideGrid = Mathf.Abs(m_SnapDivideGrid);
			enableGrid = EditorGUILayout.Toggle("Grid", enableGrid);
			GUI.enabled = enableGrid;
			var editorGUIIndentLevel = EditorGUI.indentLevel++;
			divideGrid = EditorGUILayout.IntSlider("Divide", divideGrid, 1, 50);
			EditorGUI.indentLevel = editorGUIIndentLevel;
			if (EditorGUI.EndChangeCheck()) {
				if (!enableGrid) {
					divideGrid *= -1;
				}
				m_SnapDivideGrid = divideGrid;
			}

			GUI.enabled = oldGUIEnabled;
		}

		private void FocusInfo(Rect r) {
			if ((m_Focus == null) || (0 == m_Focus.Length)) {
				return;
			}

			var windowSize = new Vector2(200.0f, 60.0f);

			BeginWindows();
			var windowRect = new Rect(r.xMax - windowSize.x - 10.0f, r.yMax - windowSize.y - 10.0f, windowSize.x, windowSize.y);
			var windowTitle = new GUIContent("Key(" + m_Focus[0] + ")");
			windowRect = GUI.Window(0, windowRect, FocusInfoWindow, windowTitle);
			EndWindows();
			if (windowRect.Contains(Event.current.mousePosition)) {
				Event.current.Use();
			}
		}

		private void FocusInfoWindow(int id) {
			var oldLabelWidth = EditorGUIUtility.labelWidth;
			var oldWideMode = EditorGUIUtility.wideMode;
			EditorGUIUtility.labelWidth = 60.0f;
			EditorGUIUtility.wideMode = true;

			var currentIndex = m_Focus[0];
			var key = material.keys[currentIndex];
			EditorGUI.BeginChangeCheck();
			key.position = EditorGUILayout.Vector2Field("position", key.position);
			key.color = EditorGUILayout.ColorField("color", key.color);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObjects(new Object[]{this, material}, "change gradation key");
				material.keys[currentIndex] = key;
				m_Dirty = true;
				EditorUtility.SetDirty(this);
			}

			EditorGUIUtility.labelWidth = oldLabelWidth;
			EditorGUIUtility.wideMode = oldWideMode;
		}
	}
}
