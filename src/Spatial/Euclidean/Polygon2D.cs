using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Spatial.Euclidean
{
    /// <summary>
    /// Class to represent a closed polygon. If the 
    /// </summary>
    public class Polygon2D : IEnumerable<Point2D>
    {
        private List<Point2D> _points;

        public int Count
        {
            get { return this._points.Count; }
        }

        // Constructors
        public Polygon2D() : this(Enumerable.Empty<Point2D>())
        {
            
        }

        public Polygon2D(IEnumerable<Point2D> points)
        {
            this._points = new List<Point2D>(points);
            if (this._points.First().Equals(this._points.Last()))
                this._points.RemoveAt(0);
        }

        // Methods
        public Point2D this[int key]
        {
            get { return this._points[key]; }
            set { this._points[key] = value; }
        }

        public static bool IsPointInPolygon(Point2D p, Polygon2D poly)
        {
            // Algorithm from http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
            // translated into C#
            bool c = false;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if (((poly[i].Y > p.Y) != (poly[j].Y > p.Y)) &&
                    (p.X < (poly[j].X - poly[i].X)*(p.Y - poly[i].Y)/(poly[j].Y - poly[i].Y) + poly[i].X))
                    c = !c;
            }
            return c;
        }

        public static Polygon2D GetConvexHullFromPoints(IEnumerable<Point2D> pointList)
        {
            // Use the Quickhull algorithm to compute the convex hull of the given points, 
            // making the assumption that the points were delivered in no particular order.
            var points = new List<Point2D>(pointList);

            // Perform basic validation of the input point cloud for cases of less than
            // four points being given
            if (points.Count <= 1)
                return null;
            if (points.Count <= 3)
                return new Polygon2D(points);

            // Find the leftmost and rightmost points
            Point2D leftMost = points.First();
            Point2D rightMost = points.First();
            foreach (var point in points)
            {
                if (point.X < leftMost.X)
                    leftMost = point;
                if (point.X > rightMost.X)
                    rightMost = point;
            }

            // Remove the left and right points
            points.Remove(leftMost);
            points.Remove(rightMost);

            // Break the remaining cloud into upper and lower sets
            var upperPoints = new List<Point2D>();
            var lowerPoints = new List<Point2D>();
            Vector2D chord = leftMost.VectorTo(rightMost);
            foreach (var point2D in points)
            {
                Vector2D testVector = leftMost.VectorTo(point2D);
                if (chord.CrossProduct(testVector) > 0)
                    upperPoints.Add(point2D);
                else
                    lowerPoints.Add(point2D);
            }

            var hullPoints = new List<Point2D>{leftMost, rightMost};

            while (upperPoints.Any())
            {
                // Locate the furthest point 
            }


        }

        /// <summary>
        /// Recursive method to isolate the points from the working list which lie on the convex hull
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="workingList"></param>
        /// <param name="hullList"></param>
        private void RecursiveHullComputation(Point2D a, Point2D b, List<Point2D> workingList, List<Point2D> hullList)
        {
            if (!workingList.Any())
                return;
            if (workingList.Count == 1)
            {
                hullList.Add(workingList.First());
                workingList.Remove(workingList.First());
                return;
            }

            // Find the furthest point from the line
            var chord = a.VectorTo(b);
            foreach (var point2D in workingList)
            {
                var testVector = a.VectorTo(point2D);
                
            }
        }

        public IEnumerator<Point2D> GetEnumerator()
        {
            return this._points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}