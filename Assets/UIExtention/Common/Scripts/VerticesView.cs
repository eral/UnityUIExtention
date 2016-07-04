using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace UIExtention {
	public interface IIndexer<T> : IEnumerable<T>, IEnumerable {
		int Count {
			get;
		}

		T this[int index] {
			get;
			set;
		}

		int IndexOf(T item);
	}

	public class VerticesView : IIndexer<UIVertex>, IEnumerable<UIVertex>, IEnumerable {
		public interface VerticesViewBridge : IIndexer<UIVertex>, IEnumerable<UIVertex>, IEnumerable {
			Vector3 GetPosition(int index);
			void SetPosition(int index, Vector3 position);
			IEnumerable<Vector3> PositionEnumerable {get;}
			VerticesViewBridge GetRange(int index, int count);
			VerticesViewBridge Skip(int index);
			VerticesViewBridge Take(int count);
		}

		public VerticesView()
			: this((VerticesViewBridge)null) {
		}
		public VerticesView(UIVertex[] src)
			: this (new VerticesViewUIVertexArray<UIVertex>(src, toUIVertexFrom, toUIVertexFrom, toVector3From, toUIVertexFrom)) {
		}
		public VerticesView(UIVertex[] src, int index)
			: this (new VerticesViewUIVertexArray<UIVertex>(src, index, toUIVertexFrom, toUIVertexFrom, toVector3From, toUIVertexFrom)) {
		}
		public VerticesView(UIVertex[] src, int index, int count)
			: this (new VerticesViewUIVertexArray<UIVertex>(src, index, count, toUIVertexFrom, toUIVertexFrom, toVector3From, toUIVertexFrom)) {
		}
		public VerticesView(List<UIVertex> src)
			: this (new VerticesViewUIVertexList<UIVertex>(src, toUIVertexFrom, toUIVertexFrom, toVector3From, toUIVertexFrom)) {
		}
		public VerticesView(List<UIVertex> src, int index)
			: this (new VerticesViewUIVertexList<UIVertex>(src, index, toUIVertexFrom, toUIVertexFrom, toVector3From, toUIVertexFrom)) {
		}
		public VerticesView(List<UIVertex> src, int index, int count)
			: this (new VerticesViewUIVertexList<UIVertex>(src, index, count, toUIVertexFrom, toUIVertexFrom, toVector3From, toUIVertexFrom)) {
		}
		public VerticesView(Vector3[] src)
			: this (new VerticesViewUIVertexArray<Vector3>(src, toUIVertexFrom, toVector3From, toVector3From, toVector3From)) {
		}
		public VerticesView(Vector3[] src, int index)
			: this (new VerticesViewUIVertexArray<Vector3>(src, index, toUIVertexFrom, toVector3From, toVector3From, toVector3From)) {
		}
		public VerticesView(Vector3[] src, int index, int count)
			: this (new VerticesViewUIVertexArray<Vector3>(src, index, count, toUIVertexFrom, toVector3From, toVector3From, toVector3From)) {
		}
		public VerticesView(List<Vector3> src)
			: this (new VerticesViewUIVertexList<Vector3>(src, toUIVertexFrom, toVector3From, toVector3From, toVector3From)) {
		}
		public VerticesView(List<Vector3> src, int index)
			: this (new VerticesViewUIVertexList<Vector3>(src, index, toUIVertexFrom, toVector3From, toVector3From, toVector3From)) {
		}
		public VerticesView(List<Vector3> src, int index, int count)
			: this (new VerticesViewUIVertexList<Vector3>(src, index, count, toUIVertexFrom, toVector3From, toVector3From, toVector3From)) {
		}
		public VerticesView(Vector2[] src)
			: this (new VerticesViewUIVertexArray<Vector2>(src, toUIVertexFrom, toVector2From, toVector3From, toVector2From)) {
		}
		public VerticesView(Vector2[] src, int index)
			: this (new VerticesViewUIVertexArray<Vector2>(src, index, toUIVertexFrom, toVector2From, toVector3From, toVector2From)) {
		}
		public VerticesView(Vector2[] src, int index, int count)
			: this (new VerticesViewUIVertexArray<Vector2>(src, index, count, toUIVertexFrom, toVector2From, toVector3From, toVector2From)) {
		}
		public VerticesView(List<Vector2> src)
			: this (new VerticesViewUIVertexList<Vector2>(src, toUIVertexFrom, toVector2From, toVector3From, toVector2From)) {
		}
		public VerticesView(List<Vector2> src, int index)
			: this (new VerticesViewUIVertexList<Vector2>(src, index, toUIVertexFrom, toVector2From, toVector3From, toVector2From)) {
		}
		public VerticesView(List<Vector2> src, int index, int count)
			: this (new VerticesViewUIVertexList<Vector2>(src, index, count, toUIVertexFrom, toVector2From, toVector3From, toVector2From)) {
		}

		public static implicit operator VerticesView(UIVertex[] src) {
			return new VerticesView(src);
		}
		public static implicit operator VerticesView(List<UIVertex> src) {
			return new VerticesView(src);
		}
		public static implicit operator VerticesView(Vector3[] src) {
			return new VerticesView(src);
		}
		public static implicit operator VerticesView(List<Vector3> src) {
			return new VerticesView(src);
		}
		public static implicit operator VerticesView(Vector2[] src) {
			return new VerticesView(src);
		}
		public static implicit operator VerticesView(List<Vector2> src) {
			return new VerticesView(src);
		}

		public int Count {
			get {return mImpl.Count;}
		}

		public UIVertex this[int index] {
			get {return mImpl[index];}
			set {mImpl[index] = value;}
		}

		public int IndexOf(UIVertex item) {
			return mImpl.IndexOf(item);
		}

		public IEnumerator<UIVertex> GetEnumerator() {
			return mImpl.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return mImpl.GetEnumerator();
		}

		public Vector3 GetPosition(int index) {
			return mImpl.GetPosition(index);
		}

		public void SetPosition(int index, Vector3 position) {
			mImpl.SetPosition(index, position);
		}

		public IEnumerable<Vector3> PositionEnumerable {
			get{return mImpl.PositionEnumerable;}
		}

		public VerticesView GetRange(int index, int count) {
			return new VerticesView(mImpl.GetRange(index, count));
		}

		public VerticesView Skip(int index) {
			return new VerticesView(mImpl.Skip(index));
		}

		public VerticesView Take(int count) {
			return new VerticesView(mImpl.Take(count));
		}

		private static UIVertex toUIVertexFrom(UIVertex src) {
			return src;
		}
		private static UIVertex toUIVertexFrom(Vector3 src) {
			var result = UIVertex.simpleVert;
			result.position = src;
			return result;
		}
		private static UIVertex toUIVertexFrom(Vector2 src) {
			var result = UIVertex.simpleVert;
			result.position = src;
			return result;
		}

		private static Vector3 toVector3From(UIVertex src) {
			return src.position;
		}
		private static Vector3 toVector3From(Vector3 src) {
			return src;
		}
		private static Vector3 toVector3From(Vector2 src) {
			return src;
		}

		private static Vector2 toVector2From(UIVertex src) {
			return src.position;
		}
		private static Vector2 toVector2From(Vector3 src) {
			return src;
		}
		private static Vector2 toVector2From(Vector2 src) {
			return src;
		}

		private VerticesView(VerticesViewBridge impl) {
			mImpl = impl;
		}
		private VerticesViewBridge mImpl;
	}

	public class VerticesViewUIVertexArray<T> : VerticesView.VerticesViewBridge where T : struct {
		public VerticesViewUIVertexArray(T[] src, System.Func<T, UIVertex> toUIVertexFromT, System.Func<UIVertex, T> toTFromUIVertex, System.Func<T, Vector3> toVector3FromT, System.Func<Vector3, T> toTFromVector3)
			: this(src, 0, src.Length, toUIVertexFromT, toTFromUIVertex, toVector3FromT, toTFromVector3) {
		}
		public VerticesViewUIVertexArray(T[] src, int index, System.Func<T, UIVertex> toUIVertexFromT, System.Func<UIVertex, T> toTFromUIVertex, System.Func<T, Vector3> toVector3FromT, System.Func<Vector3, T> toTFromVector3)
			: this(src, index, src.Length - index, toUIVertexFromT, toTFromUIVertex, toVector3FromT, toTFromVector3) {
		}
		public VerticesViewUIVertexArray(T[] src, int index, int count, System.Func<T, UIVertex> toUIVertexFromT, System.Func<UIVertex, T> toTFromUIVertex, System.Func<T, Vector3> toVector3FromT, System.Func<Vector3, T> toTFromVector3) {
			if (src == null) throw new System.ArgumentNullException();
			if ((index < 0) || (src.Length <= index)) throw new System.ArgumentOutOfRangeException();
			if ((count < 0) || ((src.Length - index) < count)) throw new System.ArgumentOutOfRangeException();
			mSrc = src;
			mIndex = index;
			mCount = count;
			mToUIVertexFromT = toUIVertexFromT;
			mToTFromUIVertex = toTFromUIVertex;
			mToVector3FromT = toVector3FromT;
			mToTFromVector3 = toTFromVector3;
		}

		public int Count {
			get {return mCount;}
		}

		public UIVertex this[int index] {
			get {return mToUIVertexFromT(mSrc[mIndex + index]);}
			set {mSrc[mIndex + index] = mToTFromUIVertex(value);}
		}

		public int IndexOf(UIVertex item) {
			return System.Array.IndexOf<T>(mSrc, mToTFromUIVertex(item), mIndex, mCount);
		}

		public IEnumerator<UIVertex> GetEnumerator() {
			return Enumerable.Range(mIndex, mCount).Select(x=>mToUIVertexFromT(mSrc[x])).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return Enumerable.Range(mIndex, mCount).Select(x=>mToUIVertexFromT(mSrc[x])).GetEnumerator();
		}

		public Vector3 GetPosition(int index) {
			return mToVector3FromT(mSrc[mIndex + index]);
		}

		public void SetPosition(int index, Vector3 position) {
			mSrc[mIndex + index] = mToTFromVector3(position);
		}

		public IEnumerable<Vector3> PositionEnumerable {
			get{return Enumerable.Range(mIndex, mCount).Select(x=>mToVector3FromT(mSrc[x]));}
		}

		public VerticesView.VerticesViewBridge GetRange(int index, int count) {
			return new VerticesViewUIVertexArray<T>(mSrc, mIndex + index, count, mToUIVertexFromT, mToTFromUIVertex, mToVector3FromT, mToTFromVector3);
		}

		public VerticesView.VerticesViewBridge Skip(int index) {
			return new VerticesViewUIVertexArray<T>(mSrc, mIndex + index, mCount - index, mToUIVertexFromT, mToTFromUIVertex, mToVector3FromT, mToTFromVector3);
		}

		public VerticesView.VerticesViewBridge Take(int count) {
			return new VerticesViewUIVertexArray<T>(mSrc, mIndex, count, mToUIVertexFromT, mToTFromUIVertex, mToVector3FromT, mToTFromVector3);
		}

		private T[] mSrc;
		private int mIndex;
		private int mCount;
		private System.Func<T, UIVertex> mToUIVertexFromT;
		private System.Func<UIVertex, T> mToTFromUIVertex;
		private System.Func<T, Vector3> mToVector3FromT;
		private System.Func<Vector3, T> mToTFromVector3;
	}

	public class VerticesViewUIVertexList<T> : VerticesView.VerticesViewBridge where T : struct {
		public VerticesViewUIVertexList(List<T> src, System.Func<T, UIVertex> toUIVertexFromT, System.Func<UIVertex, T> toTFromUIVertex, System.Func<T, Vector3> toVector3FromT, System.Func<Vector3, T> toTFromVector3) 
			: this(src, 0, src.Count, toUIVertexFromT, toTFromUIVertex, toVector3FromT, toTFromVector3) {
		}
		public VerticesViewUIVertexList(List<T> src, int index, System.Func<T, UIVertex> toUIVertexFromT, System.Func<UIVertex, T> toTFromUIVertex, System.Func<T, Vector3> toVector3FromT, System.Func<Vector3, T> toTFromVector3)
			: this(src, index, src.Count - index, toUIVertexFromT, toTFromUIVertex, toVector3FromT, toTFromVector3) {
		}
		public VerticesViewUIVertexList(List<T> src, int index, int count, System.Func<T, UIVertex> toUIVertexFromT, System.Func<UIVertex, T> toTFromUIVertex, System.Func<T, Vector3> toVector3FromT, System.Func<Vector3, T> toTFromVector3) {
			if (src == null) throw new System.ArgumentNullException();
			if ((index < 0) || (src.Count <= index)) throw new System.ArgumentOutOfRangeException();
			if ((count < 0) || ((src.Count - index) < count)) throw new System.ArgumentOutOfRangeException();
			mSrc = src;
			mIndex = index;
			mCount = count;
			mToUIVertexFromT = toUIVertexFromT;
			mToTFromUIVertex = toTFromUIVertex;
			mToVector3FromT = toVector3FromT;
			mToTFromVector3 = toTFromVector3;
		}

		public int Count {
			get {return mCount;}
		}

		public UIVertex this[int index] {
			get {return mToUIVertexFromT(mSrc[mIndex + index]);}
			set {mSrc[mIndex + index] = mToTFromUIVertex(value);}
		}

		public int IndexOf(UIVertex item) {
			return mSrc.IndexOf(mToTFromUIVertex(item), mIndex, mCount);
		}

		public IEnumerator<UIVertex> GetEnumerator() {
			return Enumerable.Range(mIndex, mCount).Select(x=>mToUIVertexFromT(mSrc[x])).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return Enumerable.Range(mIndex, mCount).Select(x=>mToUIVertexFromT(mSrc[x])).GetEnumerator();
		}

		public Vector3 GetPosition(int index) {
			return mToVector3FromT(mSrc[mIndex + index]);
		}

		public void SetPosition(int index, Vector3 position) {
			mSrc[mIndex + index] = mToTFromVector3(position);
		}

		public IEnumerable<Vector3> PositionEnumerable {
			get{return Enumerable.Range(mIndex, mCount).Select(x=>mToVector3FromT(mSrc[x]));}
		}

		public VerticesView.VerticesViewBridge GetRange(int index, int count) {
			return new VerticesViewUIVertexList<T>(mSrc, mIndex + index, count, mToUIVertexFromT, mToTFromUIVertex, mToVector3FromT, mToTFromVector3);
		}

		public VerticesView.VerticesViewBridge Skip(int index) {
			return new VerticesViewUIVertexList<T>(mSrc, mIndex + index, mCount - index, mToUIVertexFromT, mToTFromUIVertex, mToVector3FromT, mToTFromVector3);
		}

		public VerticesView.VerticesViewBridge Take(int count) {
			return new VerticesViewUIVertexList<T>(mSrc, mIndex, count, mToUIVertexFromT, mToTFromUIVertex, mToVector3FromT, mToTFromVector3);
		}

		private List<T> mSrc;
		private int mIndex;
		private int mCount;
		private System.Func<T, UIVertex> mToUIVertexFromT;
		private System.Func<UIVertex, T> mToTFromUIVertex;
		private System.Func<T, Vector3> mToVector3FromT;
		private System.Func<Vector3, T> mToTFromVector3;
	}

}