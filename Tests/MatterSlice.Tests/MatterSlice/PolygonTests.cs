﻿/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using System.Collections.Generic;
using MatterHackers.QuadTree;
using MSClipperLib;
using NUnit.Framework;

namespace MatterHackers.MatterSlice.Tests
{
	using System;
	using Pathfinding;
	using Polygon = List<IntPoint>;
	using Polygons = List<List<IntPoint>>;

	[TestFixture, Category("MatterSlice.PolygonHelpers")]
	public class PolygonHelperTests
	{
		[Test]
		public void GetPositionAllongPathTests()
		{
			// ______________2__ (20, 10) .5
			// |               |
			// 3               |
			// |               1
			// |__0____________|
			// (0, 0)
			Polygon closedPoly = new Polygon() { new IntPoint(0,0), new IntPoint(20,0), new IntPoint(20,10), new IntPoint(0,10) };
			Assert.AreEqual(new IntPoint(20, 10), closedPoly.GetPositionAllongPath(.5));
			Assert.AreEqual(new IntPoint(20, 0), closedPoly.GetPositionAllongPath(.333333));
			Assert.AreEqual(new IntPoint(5, 0), closedPoly.GetPositionAllongPath(.0833333));
			Assert.AreEqual(new IntPoint(0, 5), closedPoly.GetPositionAllongPath(.91666666));

			Assert.AreEqual(new IntPoint(20, 5), closedPoly.GetPositionAllongPath(.5, false));
			Assert.AreEqual(new IntPoint(10, 0), closedPoly.GetPositionAllongPath(.2, false));
			Assert.AreEqual(new IntPoint(20, 0), closedPoly.GetPositionAllongPath(.4, false));
			Assert.AreEqual(new IntPoint(10, 10), closedPoly.GetPositionAllongPath(.8, false));
		}

		[Test]
		public void CorrectDirectionMeasure()
		{
			{
				// ______________2__
				// |               |
				// 3               |
				// |               |
				// |               |
				// |               1
				// |__0____________|

				Polygon test = new Polygon();
				test.Add(new IntPoint(0, 0));
				test.Add(new IntPoint(40, 0));
				test.Add(new IntPoint(40, 40));
				test.Add(new IntPoint(0, 40));

				Assert.AreEqual(Intersection.Colinear, test.FindIntersection(test[0], test[2]));

				TestCorrectCrossings(test, new IntPoint(20, -1), new IntPoint(20, 41), 0, 2);
				TestCorrectCrossings(test, new IntPoint(-1, 20), new IntPoint(41, 20), 3, 1);
				TestCorrectCrossings(test, new IntPoint(19, 41), new IntPoint(20, -1), 2, 0);
				TestCorrectCrossings(test, new IntPoint(20, -1), new IntPoint(19, 41), 0, 2);

				TestDistance(test, 0, new IntPoint(10, 0), 0, new IntPoint(30, 0), 20);
				TestDistance(test, 0, new IntPoint(30, 0), 0, new IntPoint(10, 0), -20);
				TestDistance(test, 0, new IntPoint(30, 0), 1, new IntPoint(40, 10), 20);
				TestDistance(test, 1, new IntPoint(40, 10), 0, new IntPoint(30, 0), -20);
				TestDistance(test, 3, new IntPoint(0, 10), 1, new IntPoint(40, 10), 60);
				TestDistance(test, 1, new IntPoint(40, 10), 3, new IntPoint(0, 10), -60);
				TestDistance(test, 3, new IntPoint(0, 10), 0, new IntPoint(10, 0), 20);
				TestDistance(test, 0, new IntPoint(10, 0), 3, new IntPoint(0, 10), -20);
				TestDistance(test, 3, new IntPoint(0, 5), 1, new IntPoint(40, 5), 50);
				TestDistance(test, 1, new IntPoint(40, 5), 3, new IntPoint(0, 5), -50);
				TestDistance(test, 2, new IntPoint(21, 40), 0, new IntPoint(20, 0), -79);
				TestDistance(test, 2, new IntPoint(19, 40), 0, new IntPoint(20, 0), 79);
			}

			{
				// ___1_____________
				// |               |
				// |               2
				// |               |
				// |               |
				// 0               |
				// |_____________3_|

				Polygon test = new Polygon();
				test.Add(new IntPoint(0, 0));
				test.Add(new IntPoint(0, 40));
				test.Add(new IntPoint(40, 40));
				test.Add(new IntPoint(40, 0));

				TestDistance(test, 3, new IntPoint(10, 0), 3, new IntPoint(30, 0), -20);
				TestDistance(test, 3, new IntPoint(30, 0), 3, new IntPoint(10, 0), 20);
				TestDistance(test, 3, new IntPoint(30, 0), 2, new IntPoint(40, 10), -20);
				TestDistance(test, 2, new IntPoint(40, 10), 3, new IntPoint(30, 0), 20);
				TestDistance(test, 0, new IntPoint(0, 10), 2, new IntPoint(40, 10), -60);
				TestDistance(test, 2, new IntPoint(40, 10), 0, new IntPoint(0, 10), 60);
				TestDistance(test, 0, new IntPoint(0, 10), 3, new IntPoint(10, 0), -20);
				TestDistance(test, 3, new IntPoint(10, 0), 0, new IntPoint(0, 10), 20);
				TestDistance(test, 0, new IntPoint(0, 5), 2, new IntPoint(40, 5), -50);
				TestDistance(test, 2, new IntPoint(40, 5), 0, new IntPoint(0, 5), 50);
				TestDistance(test, 1, new IntPoint(21, 40), 3, new IntPoint(20, 0), 79);
				TestDistance(test, 1, new IntPoint(19, 40), 3, new IntPoint(20, 0), -79);
			}

			{
				// 404, 291   ___0____            S
				//            |       _________
				//            |               | 591, 236
				//            |               1
				//             |             |
				//             3             |
				//             |       ____2_| 587, 158
				//   406, 121  |_______
				//     E

				Polygon test = new Polygon();
				test.Add(new IntPoint(404, 291));
				test.Add(new IntPoint(591, 236));
				test.Add(new IntPoint(587, 158));
				test.Add(new IntPoint(406, 121));

				TestCorrectCrossings(test, new IntPoint(624, 251), new IntPoint(373, 142), 0, 3);
			}

			{
				Polygon test = new Polygon();
				test.Add(new IntPoint(154, 162));
				test.Add(new IntPoint(159, 235));
				test.Add(new IntPoint(243, 290));
				test.Add(new IntPoint(340, 114));
			}
		}

		private void TestCorrectCrossings(Polygon poly, IntPoint start, IntPoint end, int expectedStartIndex, int expectedEndIndex)
		{
			var polyCrossings = new List<(int pointIndex, IntPoint position)>(poly.FindCrossingPoints(start, end));
			polyCrossings.Sort(new IntPointDirectionSorter(start, end));
			Assert.AreEqual(2, polyCrossings.Count);
			Assert.IsTrue(polyCrossings[0].Item1 == expectedStartIndex);
			Assert.IsTrue(polyCrossings[1].Item1 == expectedEndIndex);
		}

		[Test]
		public void MergeLinesWorks()
		{
			// connected up
			{
				var lineSegments = new Polygons()
				{
					new Polygon() { new IntPoint(0, 0), new IntPoint(0, 200) },
					new Polygon() { new IntPoint(0, 205), new IntPoint(0, 300) },
				};

				var merged = lineSegments.MergeTouchingLineSegments();
				Assert.AreEqual(1, merged.Count);
				Assert.AreEqual(2, merged[0].Count);
				Assert.AreEqual(new IntPoint(0, 0), merged[0][0]);
				Assert.AreEqual(new IntPoint(0, 300), merged[0][1]);
			}

			// connected up swapped
			{
				var lineSegments = new Polygons()
				{
					new Polygon() { new IntPoint(0, 0), new IntPoint(0, 200) },
					new Polygon() { new IntPoint(0, 300), new IntPoint(0, 205) },
				};

				var merged = lineSegments.MergeTouchingLineSegments();
				Assert.AreEqual(1, merged.Count);
				Assert.AreEqual(2, merged[0].Count);
				Assert.AreEqual(new IntPoint(0, 0), merged[0][0]);
				Assert.AreEqual(new IntPoint(0, 300), merged[0][1]);
			}

			// connected down
			{
				var lineSegments = new Polygons()
				{
					new Polygon() { new IntPoint(0, 200), new IntPoint(0, 0) },
					new Polygon() { new IntPoint(0, 205), new IntPoint(0, 300) },
				};

				var merged = lineSegments.MergeTouchingLineSegments();
				Assert.AreEqual(1, merged.Count);
				Assert.AreEqual(2, merged[0].Count);
				Assert.AreEqual(new IntPoint(0, 300), merged[0][0]);
				Assert.AreEqual(new IntPoint(0, 0), merged[0][1]);
			}

			// connected down swapped
			{
				var lineSegments = new Polygons()
				{
					new Polygon() { new IntPoint(0, 200), new IntPoint(0, 0) },
					new Polygon() { new IntPoint(0, 300), new IntPoint(0, 205) },
				};

				var merged = lineSegments.MergeTouchingLineSegments();
				Assert.AreEqual(1, merged.Count);
				Assert.AreEqual(2, merged[0].Count);
				Assert.AreEqual(new IntPoint(0, 300), merged[0][0]);
				Assert.AreEqual(new IntPoint(0, 0), merged[0][1]);
			}

			// connected down swapped
			{
				var lineSegments = new Polygons()
				{
					new Polygon() { new IntPoint(0, 100), new IntPoint(0, 0) },
					new Polygon() { new IntPoint(0, 200), new IntPoint(0, 300) },
					new Polygon() { new IntPoint(0, 500), new IntPoint(0, 600) },
					new Polygon() { new IntPoint(0, 300), new IntPoint(0, 400) },
					new Polygon() { new IntPoint(0, 500), new IntPoint(0, 400) },
					new Polygon() { new IntPoint(0, 100), new IntPoint(0, 200) },
				};

				var merged = lineSegments.MergeTouchingLineSegments();
				Assert.AreEqual(1, merged.Count);
				Assert.AreEqual(2, merged[0].Count);
				Assert.AreEqual(new IntPoint(0, 600), merged[0][0]);
				Assert.AreEqual(new IntPoint(0, 0), merged[0][1]);
			}

			{
				var lineSegments = new Polygons()
				{
					new Polygon() { new IntPoint(0, 100), new IntPoint(0, 0) },
					new Polygon() { new IntPoint(0, 200), new IntPoint(0, 300) },
					new Polygon() { new IntPoint(0, 500), new IntPoint(0, 600) },
					new Polygon() { new IntPoint(0, 300), new IntPoint(0, 400) },
					new Polygon() { new IntPoint(0, 500), new IntPoint(0, 400) },
					new Polygon() { new IntPoint(0, 100), new IntPoint(0, 200) },
					new Polygon() { new IntPoint(30, 100), new IntPoint(30, 0) },
					new Polygon() { new IntPoint(30, 200), new IntPoint(30, 300) },
					new Polygon() { new IntPoint(30, 500), new IntPoint(30, 600) },
					new Polygon() { new IntPoint(30, 300), new IntPoint(30, 400) },
					new Polygon() { new IntPoint(30, 500), new IntPoint(30, 400) },
					new Polygon() { new IntPoint(30, 100), new IntPoint(30, 200) },
				};

				var merged = lineSegments.MergeTouchingLineSegments();
				Assert.AreEqual(2, merged.Count);
				Assert.AreEqual(2, merged[0].Count);
				Assert.AreEqual(new IntPoint(0, 600), merged[0][0]);
				Assert.AreEqual(new IntPoint(0, 0), merged[0][1]);
				Assert.AreEqual(2, merged[1].Count);
				Assert.AreEqual(new IntPoint(30, 600), merged[1][0]);
				Assert.AreEqual(new IntPoint(30, 0), merged[1][1]);
			}

			{
				var lineSegments = new Polygons()
				{
					new Polygon() { new IntPoint(0, 100), new IntPoint(0, 0) },
					new Polygon() { new IntPoint(30, 500), new IntPoint(30, 600) },
					new Polygon() { new IntPoint(0, 500), new IntPoint(0, 600) },
					new Polygon() { new IntPoint(30, 200), new IntPoint(30, 300) },
					new Polygon() { new IntPoint(0, 100), new IntPoint(0, 200) },
					new Polygon() { new IntPoint(30, 100), new IntPoint(30, 200) },
					new Polygon() { new IntPoint(0, 300), new IntPoint(0, 400) },
					new Polygon() { new IntPoint(0, 200), new IntPoint(0, 300) },
					new Polygon() { new IntPoint(30, 500), new IntPoint(30, 400) },
					new Polygon() { new IntPoint(0, 500), new IntPoint(0, 400) },
					new Polygon() { new IntPoint(30, 100), new IntPoint(30, 0) },
					new Polygon() { new IntPoint(30, 300), new IntPoint(30, 400) },
				};

				var merged = lineSegments.MergeTouchingLineSegments();
				Assert.AreEqual(2, merged.Count);
				Assert.AreEqual(2, merged[0].Count);
				Assert.AreEqual(new IntPoint(0, 600), merged[0][0]);
				Assert.AreEqual(new IntPoint(0, 0), merged[0][1]);
				Assert.AreEqual(2, merged[1].Count);
				Assert.AreEqual(new IntPoint(30, 0), merged[1][0]);
				Assert.AreEqual(new IntPoint(30, 600), merged[1][1]);
			}
		}

		private void TestDistance(Polygon test, int startEdgeIndex, IntPoint startPosition, int endEdgeIndex, IntPoint endPosition, int expectedDistance)
		{
			var network = new IntPointPathNetwork(test);

			IntPoint startLinkA = test[startEdgeIndex];
			IntPoint startLinkB = test[(startEdgeIndex + 1) % test.Count];

			IntPoint endLinkA = test[endEdgeIndex];
			IntPoint endLinkB = test[(endEdgeIndex + 1) % test.Count];

			Path<IntPointNode> path = network.FindPath(startPosition, startLinkA, startLinkB, endPosition, endLinkA, endLinkB);
			Assert.AreEqual(Math.Abs(expectedDistance), path.PathLength);
		}
	}
}