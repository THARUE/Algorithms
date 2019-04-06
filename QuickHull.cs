using System;
using System.Collections.Generic;
using System.IO;

namespace CSHARPPROJECT
{    
    class QuickHull
    {
        public List<Points<double>> AllPoints { get; set; }
        public LinkedList<Points<double>> ConvexHull { get; set; }

        public QuickHull(string filePath)
        {
            AllPoints = new List<Points<double>>(GetPoints(filePath));
            ConvexHull = new LinkedList<Points<double>>(GetConvexHullPoints());
        }

        public QuickHull(List<Tuple<double, double>> points)
        {
            AllPoints = new List<Points<double>>(GetPoints(points));
            ConvexHull = new LinkedList<Points<double>>(GetConvexHullPoints());
        }

        private LinkedList<Points<double>> GetConvexHullPoints()
        {
            LinkedList<Points<double>> convexHull = new LinkedList<Points<double>>();
            Points<double> A = default, B = default;

            //retrieve the smallest and greatest X values in set
            foreach (var item in AllPoints)
            {
                //smallest X
                if (item.X < A.X)
                    A = item;
                //largest X
                if (item.X > B.X)
                    B = item;
            }

            //create subsets of all points above and below the given line
            List<Points<double>> subset1 = new List<Points<double>>(GetRightSubset(AllPoints, new Line<double>(A, B)));
            List<Points<double>> subset2 = new List<Points<double>>(GetRightSubset(AllPoints, new Line<double>(B, A)));

            //add first 2 points to the convex hull
            convexHull.AddFirst(A);
            convexHull.AddLast(B);

            //recursively search for points in between lines
            FindHull(ref convexHull, subset1, new Line<double>(A, B));
            FindHull(ref convexHull, subset2, new Line<double>(B, A));

            return convexHull;
        }

        private void FindHull(ref LinkedList<Points<double>> convexHull, List<Points<double>> subset, Line<double> line)
        {
            //find points that are on the right side of line segment
            List<Points<double>> rightSubset = new List<Points<double>>(GetRightSubset(subset, line));

            //if there are no points in the right subset then return
            if (rightSubset.Count == 0)
                return;

            //find the farthest point from the line segment
            Points<double> point = new Points<double>(rightSubset[0].X, rightSubset[0].Y);
            double distance = CompareAgainstLineEquation(line, rightSubset[0]);
            foreach (var item in rightSubset)
            {
                if (CompareAgainstLineEquation(line, item) > distance)
                {
                    point = new Points<double>(item.X, item.Y);
                }
            }

            //add the new point in between the two points of the line segment
            convexHull.AddAfter(convexHull.Find(line.Point1), point);

            //get subsets
            Line<double> line1 = new Line<double>(line.Point1, point);
            Line<double> line2 = new Line<double>(point, line.Point2);
            List<Points<double>> sub1 = new List<Points<double>>(GetRightSubset(rightSubset, line1));
            List<Points<double>> sub2 = new List<Points<double>>(GetRightSubset(rightSubset, line2));

            //search recursively for greater points between line segments
            FindHull(ref convexHull, sub1, line1);
            FindHull(ref convexHull, sub2, line2);
        }

        private List<Points<double>> GetRightSubset(List<Points<double>> set, Line<double> line)
        {
            //create 2 data sets of all points that are above and below the given line
            var s1 = new List<Points<double>>();
            foreach (var point in set)
            {
                if (point.X != line.Point1.X && point.X != line.Point2.X)
                    if (IsPointRightOfOrientedLine(line, point))
                        s1.Add(point);
            }

            return s1;
        }

        private double CompareAgainstLineEquation(Line<double> line, Points<double> point)
        {
            return (line.Point2.X - line.Point1.X) * (point.Y - line.Point1.Y) - (line.Point2.Y - line.Point1.Y) * (point.X - line.Point1.X);
        }
        private bool IsPointRightOfOrientedLine(Line<double> line, Points<double> point)
        {
            return CompareAgainstLineEquation(line, point) > 0;
        }

        private List<Points<double>> GetPoints(List<Tuple<double, double>> points)
        {
            List<Points<double>> newPoints = new List<Points<double>>();
            foreach (var item in points)
            {
                newPoints.Add(new Points<double>(item.Item1, item.Item2));
            }
            return newPoints;
        }

        private List<Points<double>> GetPoints(string filePath)
        {
            List<Points<double>> newPoints = new List<Points<double>>();

            using (StreamReader sw = new StreamReader(filePath))
            {
                string line;
                string[] values;
                double x, y;
                while (!sw.EndOfStream)
                {
                    line = sw.ReadLine();
                    values = line.Split(" ");

                    Double.TryParse(values[0].Trim(), out x);
                    Double.TryParse(values[1].Trim(), out y);

                    newPoints.Add(new Points<double>(x, y));
                }
                sw.Close();
            }

            return newPoints;
        }

        public double GetArea()
        {
            List<Points<double>> points = new List<Points<double>>(ConvexHull);
            double area = 0;
            int j = points.Count - 1;

            for (int i = 0; i < points.Count; i++)
            {
                area += (points[j].X + points[i].X) * (points[j].Y - points[i].Y);
                j = i;
            }
            return area / 2;
        }

        public override string ToString()
        {
            string display = String.Empty;

            display += "ALL POINTS:\n\n";
            foreach (var item in AllPoints)
            {
                display += $"X:{item.X} Y:{item.Y}\n";
            }

            display += "\n\nCONVEX HULL:\n\n";
            foreach (var item in ConvexHull)
            {
                display += $"X:{item.X} Y:{item.Y}\n";
            }

            display += $"\n\nArea:  {GetArea()}";

            return display;
        }

        internal struct Points<T>
        {
            public T X { get; set; }
            public T Y { get; set; }

            public Points(T x, T y)
            {
                X = x;
                Y = y;
            }
        }

        internal struct Line<T>
        {
            public Points<T> Point1 { get; set; }
            public Points<T> Point2 { get; set; }

            public Line(Points<T> p1, Points<T> p2)
            {
                Point1 = p1;
                Point2 = p2;
            }
        }
    }
}
