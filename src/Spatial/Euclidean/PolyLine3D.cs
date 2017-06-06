using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Spatial.Units;

namespace MathNet.Spatial.Euclidean
{

    /// <summary>
    /// A PolyLine is an ordered series of line segments in space represented as list of connected Point3Ds.
    /// </summary>
    public class PolyLine3D : IEnumerable<Point3D>
    {
        /// <summary>
        /// An integer representing the number of Point3D objects in the polyline
        /// </summary>
        public int Count => this._points.Count;

        /// <summary>
        /// The length of the polyline, computed as the sum of the lengths of every segment
        /// </summary>
        public double Length => this.GetPolyLineLength();

        /// <summary>
        /// Indicates whether or not the collection of points in the polyline are planar within
        /// the floating point tolerance
        /// </summary>
        public bool IsPlanar
        {
            get { throw new NotImplementedException();}
        }

        private List<Point3D> _points;

        public PolyLine3D(IEnumerable<Point3D> points)
        {
            this._points = new List<Point3D>(points);
        }

        // Operators
        public Point3D this[int key] => this._points[key];

        // Methods

        /// <summary>
        /// Computes the length of the polyline by summing the lengths of the individual segments
        /// </summary>
        /// <returns></returns>
        private double GetPolyLineLength()
        {
            double length = 0;
            for (int i = 0; i < this._points.Count - 1; ++i)
                length += this[i].DistanceTo(this[i + 1]);
            return length;
        }

        /// <summary>
        /// Get the point at a fractional distance along the curve.  For instance, fraction=0.5 will return
        /// the point halfway along the length of the polyline.
        /// </summary>
        /// <param name="fraction">The fractional length at which to compute the point</param>
        /// <returns></returns>
        public Point3D GetPointAtFractionAlongCurve(double fraction)
        {
            if (fraction > 1 || fraction < 0)
                throw new ArgumentException("fraction must be between 0 and 1");
            return this.GetPointAtLengthFromStart(fraction * this.Length);
        }

        /// <summary>
        /// Get the point at a specified distance along the curve.  A negative argument will return the first point,
        /// an argument greater than the length of the curve will return the last point.
        /// </summary>
        /// <param name="lengthFromStart">The distance from the first point along the curve at which to return a point</param>
        /// <returns></returns>
        public Point3D GetPointAtLengthFromStart(double lengthFromStart)
        {
            double length = this.Length;
            if (lengthFromStart >= length)
                return this.Last();
            if (lengthFromStart <= 0)
                return this.First();

            double cumulativeLength = 0;
            int i = 0;
            while (true)
            {
                double nextLength = cumulativeLength + this[i].DistanceTo(this[i + 1]);
                if (cumulativeLength <= lengthFromStart && nextLength > lengthFromStart)
                {
                    double leftover = lengthFromStart - cumulativeLength;
                    var direction = this[i].VectorTo(this[i + 1]).Normalize();
                    return this[i] + (leftover * direction);
                }
                cumulativeLength = nextLength;
                i++;
            }
        }

        /// <summary>
        /// Splits a PolyLine3D at the projection of the splitpoint onto the polyline and returns the 
        /// two resulting PolyLine3Ds in a tuple
        /// </summary>
        /// <param name="splitPoint">the point to project onto the polyline and perform the split at</param>
        /// <returns></returns>
        public Tuple<PolyLine3D, PolyLine3D> SplitAtPoint(Point3D splitPoint)
        {
            var indexAndPoint = this.ClosestPointAndPreceedingIndex(splitPoint);

            var part1 = new List<Point3D>();
            for (int i = 0; i <= indexAndPoint.Item1; i++)
                part1.Add(this[i]);
            part1.Add(indexAndPoint.Item2);

            var part2 = new List<Point3D>{indexAndPoint.Item2};
            for (int i = indexAndPoint.Item1 + 1; i < this.Count; i++)
                part2.Add(this[i]);

            return Tuple.Create(new PolyLine3D(part1), new PolyLine3D(part2));
        }

        /// <summary>
        /// Returns the closest point on the polyline to the given point.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point3D ClosestPointTo(Point3D p)
        {
            return this.ClosestPointAndPreceedingIndex(p).Item2;
        }

        /// <summary>
        /// Rotate the entire polyline about the origin and the specified direction of rotation by
        /// the given angle.
        /// </summary>
        /// <param name="aboutVector"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public PolyLine3D Rotate(UnitVector3D aboutVector, Angle angle)
        {
            var newPoints = this.Select(x => x.Rotate(aboutVector, angle));
            return new PolyLine3D(newPoints);
        }

        /// <summary>
        /// Rotate the entire polyline about the origin and the specified direction of rotation by
        /// the given angle.
        /// </summary>
        /// <param name="aboutVector"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public PolyLine3D Rotate(Vector3D aboutVector, Angle angle)
        {
            return this.Rotate(aboutVector.Normalize(), angle);
        }

        /// <summary>
        /// Translate the entire polyline by a given vector
        /// </summary>
        /// <param name="shift"></param>
        /// <returns></returns>
        public PolyLine3D Translate(Vector3D shift)
        {
            return new PolyLine3D(this.Select(x => x + shift));
        }

        /// <summary>
        /// Rotate the entire polyline about the specified axis by the given angle.
        /// </summary>
        /// <param name="rotationAxis"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public PolyLine3D Rotate(Ray3D rotationAxis, Angle angle)
        {
            var newPoints = this.Select(x => x.Rotate(rotationAxis, angle));
            return new PolyLine3D(newPoints);
        }

        /// <summary>
        /// Scans through the polyline point by point and returns a new polyline by removing any adjacent points
        /// which are equal within the specified tolerance.
        /// </summary>
        /// <param name="tol"></param>
        /// <returns></returns>
        public PolyLine3D RemoveAdjacentDuplicates(double tol = 1e-6)
        {
            var newPoints = new List<Point3D> {this[0]};
            for (int i = 1; i < this._points.Count; i++)
            {
                if (!newPoints.Last().Equals(this._points[i], tol))
                {
                    newPoints.Add(this._points[i]);
                }
            }
            return new PolyLine3D(newPoints);
        }

        /// <summary>
        /// Find all of the intersections that this polyline has with a given plane
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public IEnumerable<Point3D> IntersectionsWith(Plane plane, double tolerance = Double.Epsilon)
        {
            var results = new List<Point3D>();
            foreach (var line3D in this.ToLine3Ds())
            {
                var result = line3D.IntersectionWith(plane, tolerance);
                if (result.HasValue)
                    results.Add(result.Value);
            }
            return results;
        }

        /// <summary>
        /// Resample the curve and return a new polyline with the given number of points
        /// </summary>
        /// <param name="numberOfPoints"></param>
        /// <returns></returns>
        public PolyLine3D Resample(int numberOfPoints)
        {
            var newPoints = new List<Point3D>();
            double fraction = 1.0 / numberOfPoints;
            for (int i = 0; i < numberOfPoints; i++)
            {
                newPoints.Add(this.GetPointAtFractionAlongCurve(i * fraction));
            }
            newPoints.Add(this.Last());

            return new PolyLine3D(newPoints);
        }

        /// <summary>
        /// Convert the PolyLine3D to an IEnumerable of Line3Ds representing the segments between the 
        /// points in the polyline. Remember that any zero-length line will raise an ArgumentException, so
        /// it is best to remove any duplicate adjacent points before performing this conversion.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Line3D> ToLine3Ds()
        {
            var lines = new List<Line3D>();
            for (int i = 0; i < this._points.Count - 1; i++)
            {
                lines.Add(new Line3D(this._points[i], this._points[i + 1]));
            }
            return lines;
        }

        public bool IsPlanarWithinTol(double tolerance)
        {
            throw new NotImplementedException();
        }

        // IEnumerable<Point3D>
        public IEnumerator<Point3D> GetEnumerator()
        {
            return this._points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Return the index of the point which preceeds the test point's projection onto the polyline.
        /// </summary>
        /// <param name="p"></param>
        protected int PreceedingPointIndex(Point3D p)
        {
            return this.ClosestPointAndPreceedingIndex(p).Item1;
        }

        protected Tuple<int, Point3D> ClosestPointAndPreceedingIndex(Point3D p)
        {
            var minError = double.MaxValue;
            var closest = new Point3D();
            int preceedingIndex = 0;

            for (int i = 0; i < this.Count - 1; i++)
            {
                var segment = new Line3D(this[i], this[i + 1]);
                var projected = segment.ClosestPointTo(p, true);
                double error = p.DistanceTo(projected);
                if (error < minError)
                {
                    minError = error;
                    closest = projected;
                    preceedingIndex = i;
                }
            }
            return Tuple.Create(preceedingIndex, closest);
        }
    }
}