using System.Collections.Generic;
using System.Linq;
using MathNet.Spatial.Euclidean;
using NUnit.Framework;

namespace MathNet.Spatial.UnitTests.Euclidean
{
    [TestFixture]
    public class Polygon2DTests
    {
        
        private Polygon2D TestPolygon1()
        {
            var points = from x in new string[] {"0,0", "0.25,0.5", "1,1", "-1,1", "0.5,-0.5"} select Point2D.Parse(x);
            return new Polygon2D(points);
        }

        private Polygon2D TestPolygon2()
        {
            var points = from x in new string[] { "0,0", "0.25,0.5", "1,1", "-1,1", "0.5,-0.5", "0,0" } select Point2D.Parse(x);
            return new Polygon2D(points);
        }

        private Polygon2D TestPolygon3()
        {
            var points = from x in new string[] { "0.25,0", "0.5,1", "1,-1" } select Point2D.Parse(x);
            return new Polygon2D(points);
        }

        private Polygon2D TestPolygon4()
        {
            var points = from x in new string[] { "0.5,1", "1,-1", "0.25,0" } select Point2D.Parse(x);
            return new Polygon2D(points);
        }

        [Test]
        public void ConstructorTest()
        {
            var polygon = this.TestPolygon1();
            var checkList = new List<Point2D> { new Point2D(0, 0), new Point2D(0.25, 0.5), new Point2D(1, 1), new Point2D(-1, 1), new Point2D(0.5, -0.5) };
            CollectionAssert.AreEqual(checkList, polygon);
        }

        [Test]
        public void ConstructorTest_ClipsStartOnDuplicate()
        {
            // Test to make sure that if the constructor point list is given to the polygon constructor with the first and last points
            // being duplicates, the point at the beginning of the list is removed
            var polygon = this.TestPolygon2();
            var checkList = new List<Point2D> { new Point2D(0.25, 0.5), new Point2D(1, 1), new Point2D(-1, 1), new Point2D(0.5, -0.5), new Point2D(0, 0) };
            CollectionAssert.AreEqual(checkList, polygon);
        }

        [TestCase(0.5, 0, true)]
        [TestCase(0.35, 0, true)]
        [TestCase(0.5, 0.5, true)]
        [TestCase(0.75, 0.1, false)]
        [TestCase(0.75, -0.1, true)]
        [TestCase(0.5, -0.5, false)]
        [TestCase(0.25, 0.5, false)]
        [TestCase(0.25, -0.5, false)]
        [TestCase(0.0, 0, false)]
        [TestCase(1.5, 0, false)]
        public void IsPointInPolygonTest1(double x, double y, bool outcome)
        {
            var testPoint = new Point2D(x, y);
            var testPoly = this.TestPolygon3();
            
            Assert.AreEqual(outcome, Polygon2D.IsPointInPolygon(testPoint, testPoly));
        }

        [TestCase(0.5, 0, true)]
        [TestCase(0.35, 0, true)]
        [TestCase(0.5, 0.5, true)]
        [TestCase(0.75, 0.1, false)]
        [TestCase(0.75, -0.1, true)]
        [TestCase(0.5, -0.5, false)]
        [TestCase(0.25, 0.5, false)]
        [TestCase(0.25, -0.5, false)]
        [TestCase(0.0, 0, false)]
        [TestCase(1.5, 0, false)]
        public void IsPointInPolygonTest2(double x, double y, bool outcome)
        {
            var testPoint = new Point2D(x, y);
            var testPoly = this.TestPolygon4();

            Assert.AreEqual(outcome, Polygon2D.IsPointInPolygon(testPoint, testPoly));
        }

    }
}