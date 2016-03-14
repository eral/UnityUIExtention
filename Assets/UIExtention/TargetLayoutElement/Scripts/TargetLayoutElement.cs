// (C) 2014 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace UIExtention {
	[AddComponentMenu("Layout/Target Layout Element")]
	[RequireComponent(typeof(RectTransform))]
	[ExecuteInEditMode]
	public class TargetLayoutElement : MonoBehaviour, ILayoutElement {
		public ILayoutElement Target {
			get{
				return null!=_target? (ILayoutElement)_target: null;
			}
			set{
				_target = null!=value? (Component)value: null;
				updateRemoteBehaviour();
				SetDirty();
			}
		}
		[SerializeField] [RestrictInterface(typeof(ILayoutElement))] private Component _target;

		private RectTransform rectTransform {get{return _rectTransform ?? GetComponent<RectTransform>();}}
		[System.NonSerialized] private RectTransform _rectTransform = null;
		[SerializeField] private TargetLayoutElementRemoteBehaviour _remoteBehaviour = null;

		public virtual float minWidth {get{return null!=Target? Target.minWidth: 0.0f;}}
		public virtual float minHeight {get{return null!=Target? Target.minHeight: 0.0f;}}
		public virtual float preferredWidth {get{return null!=Target? Target.preferredWidth: 0.0f;}}
		public virtual float preferredHeight {get{return null!=Target? Target.preferredHeight: 0.0f;}}
		public virtual float flexibleWidth {get{return null!=Target? Target.flexibleWidth: 0.0f;}}
		public virtual float flexibleHeight {get{return null!=Target? Target.flexibleHeight: 0.0f;}}
		public virtual int layoutPriority {get{return null!=Target? Target.layoutPriority: 1;}}

		public virtual void CalculateLayoutInputHorizontal() {
		}

		public virtual void CalculateLayoutInputVertical() {
		}

		public void Awake() {
		}

		public void OnDestroy() {
			Target = null;
		}

		public void OnDestroyOfRemoteBehaviour() {
			_target = null;
			_remoteBehaviour = null;
		}

		public void SetDirtyOfRemoteBehaviour() {
			SetDirty();
		}

		public void OnBeforeTransformParentChanged() {
			SetDirty();
		}

		public void OnDidApplyAnimationProperties() {
			SetDirty();
		}

		public void OnDisable() {
			SetDirty();
		}

		public void OnEnable() {
			SetDirty();
		}

		public void OnTransformParentChanged() {
			SetDirty();
		}

		public void SetLayoutHorizontalOfRemoteBehaviour() {
			foreach (var component in GetComponents<Component>().Select(x=>x as ILayoutController)
																.Where(x=>null != x)) {
				component.SetLayoutHorizontal();
			}
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
		}

		public void SetLayoutVerticalOfRemoteBehaviour() {
			foreach (var component in GetComponents<Component>().Select(x=>x as ILayoutController)
																.Where(x=>null != x)) {
				component.SetLayoutVertical();
			}
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
		}

		private void SetDirty() {
			if (IsActive()) {
				LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
#if UNITY_EDITOR
				//UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
			}
		}

		private bool IsActive() {
			return enabled && gameObject.activeInHierarchy;
		}

		private void updateRemoteBehaviour() {
			if (null != _remoteBehaviour) {
				_remoteBehaviour.Source = null;
				DestroyImmediate(_remoteBehaviour);
				_remoteBehaviour = null;
			}
			if (_target) {
				_remoteBehaviour = _target.gameObject.AddComponent<TargetLayoutElementRemoteBehaviour>();
				_remoteBehaviour.Source = this;
				//_remoteBehaviour.hideFlags |= HideFlags.HideInInspector;
			}
		}

#if UNITY_EDITOR
		public void OnValidate() {
			SetDirty();
		}
#endif
	}
}
