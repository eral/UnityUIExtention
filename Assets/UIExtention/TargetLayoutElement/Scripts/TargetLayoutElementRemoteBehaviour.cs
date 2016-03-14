/*
 * (C) 2014 ERAL
 * Distributed under the Boost Software License, Version 1.0.
 * (See copy at http://www.boost.org/LICENSE_1_0.txt)
 */

using UnityEngine;
using UnityEngine.UI;

namespace UIExtention {
	[ExecuteInEditMode]
	public class TargetLayoutElementRemoteBehaviour : MonoBehaviour, ILayoutController {
		public TargetLayoutElement Source {get{return _source;} set{_source = value;}}
		[SerializeField] private TargetLayoutElement _source;

		public void OnDestroy() {
			if (Source) {
				Source.OnDestroyOfRemoteBehaviour();
			}
		}

		public virtual void SetLayoutHorizontal() {
			if (Source) {
				Source.SetLayoutHorizontalOfRemoteBehaviour();
			}
		}

		public virtual void SetLayoutVertical() {
			if (Source) {
				Source.SetLayoutVerticalOfRemoteBehaviour();
			}
		}
	}
}
