// Geometry.cs - Contains some basic Geometry structs (Complex numbers, Points, Vectors)
// ---------------------------------------------------------------------------------------
namespace GrayBMP;

/// <summary>A number in the complex plane of the form (X + iY)</summary>
readonly struct Complex {
   public Complex (double x, double y) => (X, Y) = (x, y);
   public override string ToString () => $"{X} + i{Y}";

   public double Norm => Math.Sqrt (X * X + Y * Y);
   public double NormSq => X * X + Y * Y;

   public static readonly Complex Zero = new (0, 0);

   public static Complex operator + (Complex a, Complex b)
      => new (a.X + b.X, a.Y + b.Y);
   public static Complex operator * (Complex a, Complex b)
      => new (a.X * b.X - a.Y * b.Y, a.X * b.Y + a.Y * b.X);

   public readonly double X, Y;
}

/// <summary>A point in 2D space, with double-precision coordinates (X, Y)</summary>
readonly struct Point2 {
   public Point2 (double x, double y) => (X, Y) = (x, y);
   public (int X, int Y) Round () => ((int)(X + 0.5), (int)(Y + 0.5));
   public override string ToString () => $"({Math.Round (X, 6)}, {Math.Round (Y, 6)})";

   public readonly double X, Y;
}

/// <summary>A Line in 2 dimensions (A -> B)</summary>
readonly struct Line {
   public Line (Point2 a, Point2 b) => (A, B) = (a, b);
   public override string ToString () => $"{A} -> {B}";

   public readonly Point2 A, B;
}

/// <summary>A drawing is a collection of lines</summary>
class Drawing {
   public void AddLine (Line line) => mLines.Add (line);

   public IReadOnlyList<Line> Lines => mLines;
   List<Line> mLines = new ();
}
