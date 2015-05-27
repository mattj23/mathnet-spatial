using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MathNet.Spatial.Euclidean
{
    public class PolyLine2D : IEnumerable<Point2D>
    {
        private List<Point2D> _points;

        public int Count
        {
            get { return this._points.Count; }
        }

        public double Length
        {
            get { return this.GetPolyLineLength(); }
        }

        // Constructors
        public PolyLine2D() : this(Enumerable.Empty<Point2D>())
        {
            
        }

        public PolyLine2D(IEnumerable<Point2D> points)
        {
            this._points = new List<Point2D>(points);
        }

        // Methods
        public Point2D this[int key]
        {
            get { return this._points[key]; }
            set { this._points[key] = value; }
        }

        public Polygon2D ConvexHull()
        {
            throw new NotImplementedException();
        }

        private double GetPolyLineLength()
        {
            double length = 0;
            for (int i = 0; i < this._points.Count - 1; ++i)
                length += this[i].DistanceTo(this[i + 1]);
            return length;
        }

        // Static methods
        public static Polygon2D GetConvexHull(PolyLine2D polyline)
        {
            var sortPoints = new List<Point2D>(polyline);
            sortPoints.Sort((a, b) => a.X == b.X ? a.Y.CompareTo(b.Y) : (a.X > b.X ? 1: -1));

            List<Point2D> hhull = new List<Point2D>();
            int lower = 0;
            int upper = 0;

            for (int i = sortPoints.Count - 1; i >= 0; --i)
            {
                ;

            }
            throw new NotImplementedException();
        }
        

        // IEnumerable<Point2D>
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