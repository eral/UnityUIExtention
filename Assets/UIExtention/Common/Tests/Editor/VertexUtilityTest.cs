// (C) 2016 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace UIExtention {
	public class VertexUtilityTest {

		[Test]
		public void T000_IntersectInTriangle()
		{
			var triangle = new List<UIVertex>(3);
			triangle.Add(CreateUIVertex(new Vector3(0.0f, 0.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(4.0f, 0.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(0.0f, 4.0f, 0.0f)));

			var mask = new List<UIVertex>(3);
			mask.Add(CreateUIVertex(new Vector3(1.0f, 1.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(3.0f, 1.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(1.0f, 3.0f, 0.0f)));

			VertexUtility.Intersect(triangle, mask);
			Assert.AreEqual(3, triangle.Count);
			Assert.AreEqual(3, triangle.Select(x=>x.position).Distinct().Count());

			var vertices = new List<Vector3>(3);
			vertices.Add(new Vector3(1.0f, 1.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 1.0f, 0.0f));
			vertices.Add(new Vector3(1.0f, 3.0f, 0.0f));
			Assert.AreEqual(3, triangle.Select(x=>x.position).Concat(vertices).Distinct().Count());
		}

		[Test]
		public void T001_IntersectOutTriangle()
		{
			var triangle = new List<UIVertex>(3);
			triangle.Add(CreateUIVertex(new Vector3(1.0f, 1.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(3.0f, 1.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(1.0f, 3.0f, 0.0f)));

			var mask = new List<UIVertex>(3);
			mask.Add(CreateUIVertex(new Vector3(0.0f, 0.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(0.0f, 4.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(4.0f, 0.0f, 0.0f)));

			VertexUtility.Intersect(triangle, mask);
			Assert.AreEqual(3, triangle.Count);
			Assert.AreEqual(3, triangle.Select(x=>x.position).Distinct().Count());

			var vertices = new List<Vector3>(3);
			vertices.Add(new Vector3(1.0f, 1.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 1.0f, 0.0f));
			vertices.Add(new Vector3(1.0f, 3.0f, 0.0f));
			Assert.AreEqual(3, triangle.Select(x=>x.position).Concat(vertices).Distinct().Count());
		}

		[Test]
		public void T002_IntersectCrossTriangle()
		{
			var triangle = new List<UIVertex>(3);
			triangle.Add(CreateUIVertex(new Vector3(0.0f, 0.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(4.0f, 0.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(0.0f, 4.0f, 0.0f)));

			var mask = new List<UIVertex>(3);
			mask.Add(CreateUIVertex(new Vector3( 3.0f,  3.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(-1.0f,  3.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3( 3.0f, -1.0f, 0.0f)));

			VertexUtility.Intersect(triangle, mask);
			Assert.AreEqual(12, triangle.Count);
			Assert.AreEqual(6, triangle.Select(x=>x.position).Distinct().Count());

			var vertices = new List<Vector3>(3);
			vertices.Add(new Vector3(2.0f, 0.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 0.0f, 0.0f));
			vertices.Add(new Vector3(0.0f, 2.0f, 0.0f));
			vertices.Add(new Vector3(0.0f, 3.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 1.0f, 0.0f));
			vertices.Add(new Vector3(1.0f, 3.0f, 0.0f));
			Assert.AreEqual(6, triangle.Select(x=>x.position).Concat(vertices).Distinct().Count());
		}

		[Test]
		public void T100_IntersectInSquare()
		{
			var triangle = new List<UIVertex>(3);
			triangle.Add(CreateUIVertex(new Vector3(0.0f, 0.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(4.0f, 0.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(4.0f, 4.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(0.0f, 4.0f, 0.0f)));
			triangle = (new[]{0, 1, 2, 2, 0, 3}).Select(x=>triangle[x]).ToList();

			var mask = new List<UIVertex>(3);
			mask.Add(CreateUIVertex(new Vector3(1.0f, 1.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(3.0f, 1.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(3.0f, 3.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(1.0f, 3.0f, 0.0f)));
			mask = (new[]{0, 1, 2, 2, 0, 3}).Select(x=>mask[x]).ToList();

			VertexUtility.Intersect(triangle, mask);
			Assert.AreEqual(6, triangle.Count);
			Assert.AreEqual(4, triangle.Select(x=>x.position).Distinct().Count());

			var vertices = new List<Vector3>(3);
			vertices.Add(new Vector3(1.0f, 1.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 1.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 3.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 1.0f, 0.0f));
			Assert.AreEqual(4, triangle.Select(x=>x.position).Concat(vertices).Distinct().Count());
		}

		[Test]
		public void T101_IntersectOutSquare()
		{
			var triangle = new List<UIVertex>(3);
			triangle.Add(CreateUIVertex(new Vector3(1.0f, 1.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(3.0f, 1.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(3.0f, 3.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(1.0f, 3.0f, 0.0f)));
			triangle = (new[]{0, 1, 2, 2, 0, 3}).Select(x=>triangle[x]).ToList();

			var mask = new List<UIVertex>(3);
			mask.Add(CreateUIVertex(new Vector3(0.0f, 0.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(4.0f, 0.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(4.0f, 4.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(0.0f, 4.0f, 0.0f)));
			mask = (new[]{0, 1, 2, 2, 0, 3}).Select(x=>mask[x]).ToList();

			VertexUtility.Intersect(triangle, mask);
			Assert.AreEqual(6, triangle.Count);
			Assert.AreEqual(4, triangle.Select(x=>x.position).Distinct().Count());

			var vertices = new List<Vector3>(3);
			vertices.Add(new Vector3(1.0f, 1.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 1.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 3.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 1.0f, 0.0f));
			Assert.AreEqual(4, triangle.Select(x=>x.position).Concat(vertices).Distinct().Count());
		}

		[Test]
		public void T102_IntersectCrossSquare()
		{
			var triangle = new List<UIVertex>(3);
			triangle.Add(CreateUIVertex(new Vector3(0.0f, 0.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(4.0f, 0.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(4.0f, 4.0f, 0.0f)));
			triangle.Add(CreateUIVertex(new Vector3(0.0f, 4.0f, 0.0f)));
			triangle = (new[]{0, 1, 2, 2, 0, 3}).Select(x=>triangle[x]).ToList();

			var mask = new List<UIVertex>(3);
			mask.Add(CreateUIVertex(new Vector3( 2.0f,-1.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3( 5.0f, 2.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3( 2.0f, 5.0f, 0.0f)));
			mask.Add(CreateUIVertex(new Vector3(-1.0f, 2.0f, 0.0f)));
			mask = (new[]{0, 1, 2, 2, 0, 3}).Select(x=>mask[x]).ToList();

			VertexUtility.Intersect(triangle, mask);
			Assert.AreEqual(30, triangle.Count);
			Assert.AreEqual(13, triangle.Select(x=>x.position).Distinct().Count());

			var vertices = new List<Vector3>(3);
			vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
			vertices.Add(new Vector3(2.0f, 0.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 0.0f, 0.0f));
			vertices.Add(new Vector3(0.5f, 0.5f, 0.0f));
			vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
			vertices.Add(new Vector3(4.0f, 1.0f, 0.0f));
			vertices.Add(new Vector3(2.0f, 2.0f, 0.0f));
			vertices.Add(new Vector3(0.0f, 3.0f, 0.0f));
			vertices.Add(new Vector3(4.0f, 3.0f, 0.0f));
			vertices.Add(new Vector3(3.5f, 3.5f, 0.0f));
			vertices.Add(new Vector3(1.0f, 4.0f, 0.0f));
			vertices.Add(new Vector3(2.0f, 4.0f, 0.0f));
			vertices.Add(new Vector3(3.0f, 4.0f, 0.0f));
			Assert.AreEqual(13, triangle.Select(x=>x.position).Concat(vertices).Distinct().Count());
		}

		private UIVertex CreateUIVertex(Vector3 position) {
			var result = new UIVertex();
			result.position = position;
			return result;
		}
	}
}

