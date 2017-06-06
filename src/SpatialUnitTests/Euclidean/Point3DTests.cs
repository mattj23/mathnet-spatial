using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using NUnit.Framework;

namespace MathNet.Spatial.UnitTests.Euclidean
{
    [TestFixture]
    public class Point3DTests
    {
        [Test]
        public void Ctor()
        {
            var actuals = new[]
            {
                new Point3D(1, 2, 3),
                new Point3D(new[] {1, 2, 3.0}),
            };
            foreach (var actual in actuals)
            {
                Assert.AreEqual(1, actual.X, 1e-6);
                Assert.AreEqual(2, actual.Y, 1e-6);
                Assert.AreEqual(3, actual.Z, 1e-6);
            }
            Assert.Throws<ArgumentException>(() => new Point3D(new[] { 1.0, 2, 3, 4 }));
        }

        [Test]
        public void ToDenseVector()
        {
            var p = new Point3D(1, 2, 3);
            var denseVector = p.ToVector();
            Assert.AreEqual(3, denseVector.Count);
            Assert.AreEqual(1, denseVector[0], 1e-6);
            Assert.AreEqual(2, denseVector[1], 1e-6);
            Assert.AreEqual(3, denseVector[2], 1e-6);
        }

        [TestCase("1, 2, 3", "1, 2, 3", 1e-4, true)]
        [TestCase("1, 2, 3", "4, 5, 6", 1e-4, false)]
        public void Equals(string p1s, string p2s, double tol, bool expected)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);
            Assert.AreEqual(expected, p1 == p2);
            Assert.AreEqual(expected, p1.Equals(p2));
            Assert.AreEqual(expected, p1.Equals((object)p2));
            Assert.AreEqual(expected, Equals(p1, p2));
            Assert.AreEqual(expected, p1.Equals(p2, tol));
            Assert.AreNotEqual(expected, p1 != p2);
        }

        [TestCase("0, 0, 0", "0, 0, 1", "0, 0, 0.5")]
        [TestCase("0, 0, 1", "0, 0, 0", "0, 0, 0.5")]
        [TestCase("0, 0, 0", "0, 0, 0", "0, 0, 0")]
        [TestCase("1, 1, 1", "3, 3, 3", "2, 2, 2")]
        [TestCase("-3, -3, -3", "3, 3, 3", "0, 0, 0")]
        public void MidPoint(string p1s, string p2s, string eps)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);
            var ep = Point3D.Parse(eps);
            Point3D mp = Point3D.MidPoint(p1, p2);
            AssertGeometry.AreEqual(ep, mp, 1e-9);
            var centroid = Point3D.Centroid(p1, p2);
            AssertGeometry.AreEqual(ep, centroid, 1e-9);
        }

        [TestCase("p:{0, 0, 0} v:{0, 0, 1}", "p:{0, 0, 0} v:{0, 1, 0}", "p:{0, 0, 0} v:{1, 0, 0}", "0, 0, 0")]
        [TestCase("p:{0, 0, 5} v:{0, 0, 1}", "p:{0, 4, 0} v:{0, 1, 0}", "p:{3, 0, 0} v:{1, 0, 0}", "3, 4, 5")]
        public void FromPlanes(string pl1s, string pl2s, string pl3s, string eps)
        {
            var plane1 = Plane.Parse(pl1s);
            var plane2 = Plane.Parse(pl2s);
            var plane3 = Plane.Parse(pl3s);
            var p1 = Point3D.IntersectionOf(plane1, plane2, plane3);
            var p2 = Point3D.IntersectionOf(plane2, plane1, plane3);
            var p3 = Point3D.IntersectionOf(plane2, plane3, plane1);
            var p4 = Point3D.IntersectionOf(plane3, plane1, plane2);
            var p5 = Point3D.IntersectionOf(plane3, plane2, plane1);
            var ep = Point3D.Parse(eps);
            foreach (var p in new[] { p1, p2, p3, p4, p5 })
            {
                AssertGeometry.AreEqual(ep, p);
            }
        }

        [TestCase("0, 0, 0", "p:{0, 0, 0} v:{0, 0, 1}", "0, 0, 0")]
        [TestCase("0, 0, 1", "p:{0, 0, 0} v:{0, 0, 1}", "0, 0, -1")]
        public void MirrorAbout(string ps, string pls, string eps)
        {
            var p = Point3D.Parse(ps);
            var p2 = Plane.Parse(pls);
            var actual = p.MirrorAbout(p2);

            var ep = Point3D.Parse(eps);
            AssertGeometry.AreEqual(ep, actual);
        }

        [TestCase("0, 0, 0", "p:{0, 0, 0} v:{0, 0, 1}", "0, 0, 0")]
        [TestCase("0, 0, 1", "p:{0, 0, 0} v:{0, 0, 1}", "0, 0, 0")]
        [TestCase("0, 0, 1", "p:{0, 10, 0} v:{0, 1, 0}", "0, 10, 1")]
        public void ProjectOnTests(string ps, string pls, string eps)
        {
            var p = Point3D.Parse(ps);
            var p2 = Plane.Parse(pls);
            var actual = p.ProjectOn(p2);

            var ep = Point3D.Parse(eps);
            AssertGeometry.AreEqual(ep, actual);
        }

        [TestCase("1, 2, 3", "1, 0, 0", "2, 2, 3")]
        [TestCase("1, 2, 3", "0, 1, 0", "1, 3, 3")]
        [TestCase("1, 2, 3", "0, 0, 1", "1, 2, 4")]
        public void AddVector(string ps, string vs, string eps)
        {
            Point3D p = Point3D.Parse(ps);
            var actuals = new[]
                          {
                              p + Vector3D.Parse(vs),
                              p + UnitVector3D.Parse(vs)
                          };
            var expected = Point3D.Parse(eps);
            foreach (var actual in actuals)
            {
                Assert.AreEqual(expected, actual);
            }
        }

        [TestCase("1, 2, 3", "1, 0, 0", "0, 2, 3")]
        [TestCase("1, 2, 3", "0, 1, 0", "1, 1, 3")]
        [TestCase("1, 2, 3", "0, 0, 1", "1, 2, 2")]
        public void SubtractVector(string ps, string vs, string eps)
        {
            Point3D p = Point3D.Parse(ps);
            var actuals = new[]
                          {
                              p - Vector3D.Parse(vs),
                              p - UnitVector3D.Parse(vs)
                          };
            var expected = Point3D.Parse(eps);
            foreach (var actual in actuals)
            {
                Assert.AreEqual(expected, actual);
            }
        }

        [TestCase("1, 2, 3", "4, 8, 16", "-3, -6, -13")]
        public void SubtractPoint(string p1s, string p2s, string evs)
        {
            Point3D p1 = Point3D.Parse(p1s);
            Point3D p2 = Point3D.Parse(p2s);

            var expected = Vector3D.Parse(evs);
            Assert.AreEqual(expected, p1 - p2);
        }

        [TestCase("0,0,0", "1,0,0", 1)]
        [TestCase("1,1,1", "2,1,1", 1)]
        public void DistanceTo(string p1s, string p2s, double d)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);

            Assert.AreEqual(d, p1.DistanceTo(p2), 1e-6);
            Assert.AreEqual(d, p2.DistanceTo(p1), 1e-6);
        }

        [TestCase("1.0 , 2.5,3.3", new double[] { 1, 2.5, 3.3 })]
        [TestCase("1,0 ; 2,5;3,3", new double[] { 1, 2.5, 3.3 })]
        [TestCase("-1.0 ; 2.5;3.3", new double[] { -1, 2.5, 3.3 })]
        [TestCase("-1 ; -2;-3", new double[] { -1, -2, -3 })]
        public void ParseTest(string pointAsString, double[] expectedPoint)
        {
            Point3D point3D = Point3D.Parse(pointAsString);
            Point3D expected = new Point3D(expectedPoint);
            AssertGeometry.AreEqual(expected, point3D, 1e-9);
        }

        [TestCase("-1 ; 2;-3")]
        public void ToVectorAndBack(string ps)
        {
            Point3D p = Point3D.Parse(ps);
            AssertGeometry.AreEqual(p, p.ToVector3D().ToPoint3D(), 1e-9);
        }

        [TestCase("-2, 0, 1e-4", null, "(-2, 0, 0.0001)", 1e-4)]
        [TestCase("-2, 0, 1e-4", "F2", "(-2.00, 0.00, 0.00)", 1e-4)]
        public void ToString(string vs, string format, string expected, double tolerance)
        {
            var p = Point3D.Parse(vs);
            string actual = p.ToString(format);
            Assert.AreEqual(expected, actual);
            AssertGeometry.AreEqual(p, Point3D.Parse(actual), tolerance);
        }

        [TestCase("1,0,0", "0,0,1", 90, "0,1,0")]
        [TestCase("1,0,0", "0,0,1", -90, "0,-1,0")]
        [TestCase("8.4910522,8.4844848,0.8012991", "4.1793042,4.0365428,8.9997942", -48.4550, "11.6840096,1.4335021,2.4810323")] // Generated in GOM Atos Professional v8
        [TestCase("5.7030502,5.8482169,2.2019595", "7.3457039,7.1033271,3.8819098", 339.5547, "5.9256585,5.6337029,2.1732491")] // Generated in GOM Atos Professional v8
        [TestCase("5.4854636,3.2175858,0.9740230", "1.2439597,1.1798032,5.5321128", 290.4793, "4.9984156,-3.2206924,2.4565975")] // Generated in GOM Atos Professional v8
        [TestCase("8.3882955,9.5841038,1.1293478", "3.5127812,0.3990365,4.7889790", 90.1552, "-3.8106602,6.4931200,10.3350005")] // Generated in GOM Atos Professional v8
        [TestCase("3.5778338,4.0321479,2.3887874", "3.6138080,4.3396801,1.7768308", -85.7481, "3.0357407,4.6569180,1.9654031")] // Generated in GOM Atos Professional v8
        [TestCase("2.8800706,8.0261508,1.4109566", "7.3891669,1.7581973,1.4806689", -184.9301, "6.2590496,-5.8679834,1.0467905")] // Generated in GOM Atos Professional v8
        public void RotationAboutOrigin(string p, string v, double a, string e)
        {
            var point = Point3D.Parse(p);
            var direction = Vector3D.Parse(v);
            var angle = Angle.FromDegrees(a);
            var expected = Point3D.Parse(e);

            var rotated = point.Rotate(direction, angle);
            AssertGeometry.AreEqual(expected, rotated, 1e-4);
        }

        [TestCase("1,0,0", "0,0,0", "0,0,1", 90, "0,1,0")]
        [TestCase("1,0,0", "2,0,0", "0,0,1", 90, "2,-1,0")]
        [TestCase("2.1898456,7.6772848,9.1588807", "0.7385921,9.8344071,2.7694078", "4.9828047,0.7545652,7.4076037", -200.3701, "6.5525553,12.0014456,5.7837822")] // Generated in GOM Atos Professional v8
        [TestCase("3.4921017,3.6809637,4.8383440", "6.3965512,1.2546020,5.8593084", "2.7668653,2.8547222,4.0588667", 51.2336, "2.6580651,1.3699948,7.0322671")] // Generated in GOM Atos Professional v8
        [TestCase("4.4350990,5.9221342,8.4652378", "8.6180110,7.1658996,3.7451098", "3.7115823,2.5381497,1.9027558", 53.6124, "7.8876918,1.8070680,7.2197135")] // Generated in GOM Atos Professional v8
        [TestCase("9.8284496,2.1588425,7.6273405", "6.3722199,7.6775151,4.4810891", "1.1380717,7.4185156,9.9227807", 53.0672, "13.4498394,6.2241181,4.1726927")] // Generated in GOM Atos Professional v8
        [TestCase("0.2346871,5.2757395,7.6410528", "4.0022896,2.6837793,4.1865969", "3.0557481,6.7346322,4.8437257", -267.1556, "6.3297790,1.3517789,9.2516655")] // Generated in GOM Atos Professional v8
        [TestCase("3.5875094,5.3870656,9.6765620", "3.5224911,3.5823199,3.3315112", "0.0895516,1.7439455,1.9349648", 136.2324, "5.8357615,8.9833905,6.3312145")] // Generated in GOM Atos Professional v8
        [TestCase("3.3263096,0.3594738,5.8479096", "9.6236571,2.7853836,3.8375412", "3.8332346,0.4346173,6.2294978", -120.0622, "9.5322881,9.3905864,1.3990678")] // Generated in GOM Atos Professional v8
        public void RotationAboutRay(string p, string v0, string vn, double a, string e)
        {
            var point = Point3D.Parse(p);
            var throughPoint = Point3D.Parse(v0);
            var direction = Vector3D.Parse(vn);
            var ray = new Ray3D(throughPoint, direction);
            var angle = Angle.FromDegrees(a);
            var expected = Point3D.Parse(e);

            var rotated = point.Rotate(ray, angle);
            AssertGeometry.AreEqual(expected, rotated, 1e-4);
        }



        [Test]
        public void XmlRoundtrip()
        {
            var p = new Point3D(1, -2, 3);
            const string Xml = @"<Point3D X=""1"" Y=""-2"" Z=""3"" />";
            const string ElementXml = @"<Point3D><X>1</X><Y>-2</Y><Z>3</Z></Point3D>";
            AssertXml.XmlRoundTrips(p, Xml, (expected, actual) => AssertGeometry.AreEqual(expected, actual));
            var serializer = new XmlSerializer(typeof (Point3D));

            var actuals = new[]
                          {
                              Point3D.ReadFrom(XmlReader.Create(new StringReader(Xml))),
                              Point3D.ReadFrom(XmlReader.Create(new StringReader(ElementXml))),
                              (Point3D)serializer.Deserialize(new StringReader(Xml)),
                              (Point3D)serializer.Deserialize(new StringReader(ElementXml))
                          };
            foreach (var actual in actuals)
            {
                AssertGeometry.AreEqual(p, actual);
            }
        }
        
        [Test]
        public void BinaryRountrip()
        {
            var v = new Point3D(1, 2, 3);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, v);
                ms.Flush();
                ms.Position = 0;
                var roundTrip = (Point3D)formatter.Deserialize(ms);
                AssertGeometry.AreEqual(v, roundTrip);
            }
        }
    }
}
