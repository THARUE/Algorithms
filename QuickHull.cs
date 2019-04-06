using System;
using System.Collections.Generic;
using System.IO;

namespace CSHARPPROJECT
{    
    /// <summary>
    /// This class performs a Divide and Conquer implementation
    /// of the QuickHull Algorithm which finds all the points that
    /// create the convex hull in a collection of X,Y coordinates on a
    /// 2d plot.
    /// </summary>
    class QuickHull
    {
        #region Properties
        /// <summary>List of all the Points<double> in the collection</summary>
        public List<Points<double>> AllPoints { get; set; }
        /// <summary>Linked List of all the points in the Convex Hull, begins with most -x and is linked in clockwise motion</summary>
        public LinkedList<Points<double>> ConvexHull { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Run an instance of the QuickHull Algorithm passing a filepath location.  Data in file must
        /// be in the following format: 
        /// X Y
        /// X Y 
        /// X Y (etc.)
        /// </summary>
        /// <param name="filePath"></param>
        /// <created>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</created>
        /// <changed>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</changed>
        public QuickHull(string filePath)
        {
            AllPoints = new List<Points<double>>(GetPoints(filePath));
            ConvexHull = new LinkedList<Points<double>>(GetConvexHullPoints());
        }

        /// <summary>
        /// Run an instance of the QuickHull Algorithm passing a List of Tuples where a single Tuple<double, double>
        /// represent an X and Y coordinate.</double>
        /// </summary>
        /// <param name="points"></param>
        /// <created>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</created>
        /// <changed>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</changed>
        public QuickHull(List<Tuple<double, double>> points)
        {
            AllPoints = new List<Points<double>>(GetPoints(points));
            ConvexHull = new LinkedList<Points<double>>(GetConvexHullPoints());
        }
        #endregion

        #region QuickHull Recursive Implementation
        /// <summary>
        /// Retrieve a LinkedList which represents all the X,Y coordinates
        /// in the Convex Hull.  First Node is the most -X coordinate and then they are placed in clockwise order.
        /// </summary>
        /// <returns>A Linked List with all the X,Y coordinates in the Convex Hull</returns>
        /// <created>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</created>
        /// <changed>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</changed>
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

        /// <summary>
        /// Recursive method which searches for a Node that is to the right of the oriented line.
        /// </summary>
        /// <param name="convexHull"></param>
        /// <param name="subset"></param>
        /// <param name="line"></param>
        /// <created>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</created>
        /// <changed>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</changed>
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
        #endregion

        #region Private Helper Methods
        /// <summary>
        /// Returns the subset of points which lay to the right of the oriented line.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="line"></param>
        /// <returns>A List of Points which Contain all the coordinates in the Subset of Points that are right of the given oriented line.</returns>
        /// <created>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</created>
        /// <changed>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</changed>
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

        /// <summary>
        /// Evaluates whether a given point is to the right of an oriented line.  
        /// If the returned value is > 0, then the point is to the right of the oriented line.
        /// If the returned value = 0, then the point falls on the oriented line.
        /// If the returned value < 0, then the point falls to the left of the oriented line.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns>A value containing the distance from the line to the point</returns>
        /// <created>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</created>
        /// <changed>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</changed>
        private double CompareAgainstLineEquation(Line<double> line, Points<double> point)
        {
            return (line.Point2.X - line.Point1.X) * (point.Y - line.Point1.Y) - (line.Point2.Y - line.Point1.Y) * (point.X - line.Point1.X);
        }

        /// <summary>
        /// Is the Point to the Right of the Oriented Line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns>True, if the point is to the right of the oriented line.</returns>
        /// <created>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</created>
        /// <changed>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</changed>
        private bool IsPointRightOfOrientedLine(Line<double> line, Points<double> point)
        {
            return CompareAgainstLineEquation(line, point) > 0;
        }

        /// <summary>
        /// Converts a List of Tuples<double, double> to a List of Points<double>
        /// </summary>
        /// <param name="points"></param>
        /// <returns>A List containing all the Points</returns>
        /// <created>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</created>
        /// <changed>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</changed>
        private List<Points<double>> GetPoints(List<Tuple<double, double>> points)
        {
            List<Points<double>> newPoints = new List<Points<double>>();
            foreach (var item in points)
            {
                newPoints.Add(new Points<double>(item.Item1, item.Item2));
            }
            return newPoints;
        }

        /// <summary>
        /// Retrieves all the X,Y coordinates from a formatted file.  Must be in following format:
        /// X Y
        /// X Y
        /// X Y (etc.)
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>A List of all the Points</returns>
        /// <created>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</created>
        /// <changed>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</changed>
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
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the Area of the Irregular Polygon
        /// </summary>
        /// <returns>A double containing the Area</returns>
        /// <created>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</created>
        /// <changed>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</changed>
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

        /// <summary>
        /// Displays All the Original Points, The Convex Hull Points, and the Area of the Irregular Polygon
        /// </summary>
        /// <returns></returns>
        /// <created>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</created>
        /// <changed>Andrew Haselden, andrewhaselden@gmail.com ,4/6/2019</changed>
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
        #endregion

        #region Internal Structs
        /// <summary>
        /// Represents a single X,Y Coordinate
        /// </summary>
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

        /// <summary>
        /// Represents a Line Segment (two points)
        /// </summary>
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
        #endregion
    }
}
