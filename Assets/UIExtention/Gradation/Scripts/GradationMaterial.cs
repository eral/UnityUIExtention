using UnityEngine;
using System.Collections.Generic;

namespace UIExtention {
	[CreateAssetMenu]
	public partial class GradationMaterial : ScriptableObject {
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
												, new Key {position = new Vector2(1.0f, 0.0f), color = Color.white}
												, new Key {position = new Vector2(1.0f, 1.0f), color = Color.white}
												, new Key {position = new Vector2(0.0f, 1.0f), color = Color.white}
												};
		public List<Key> keys {get{return m_Keys;}}

		public Grid GetGrid() {
			return new Grid(this);
		}
	}
}
