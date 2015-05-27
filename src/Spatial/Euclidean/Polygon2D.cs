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