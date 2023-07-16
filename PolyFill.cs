using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GrayBMP;
class PolyFill : Window {
   public PolyFill() {
      Width = 900; Height = 600;
      Left = 200; Top = 50;
      WindowStyle = WindowStyle.None;
      mBmp = new GrayBMP(Width, Height);

      Image image = new() {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
         Source = mBmp.Bitmap
      };
      RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode(image, EdgeMode.Aliased);
      Content = image;

      var LeafPoints = AddLine();
      Fill (LeafPoints, 255);
   }
   readonly GrayBMP mBmp;

   public static List<Point> AddLine() {
      using var lines = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GrayBMP.Data.leaf-fill.txt"));
      var PolyPoints = lines.ReadToEnd().Replace("\r\n", " ").TrimEnd().Split(' ').Select(int.Parse).ToList();
      List<Point> mLines = new();
      for (int i = 0; i < PolyPoints.Count; i += 2)
         mLines.Add(new Point(PolyPoints[i], PolyPoints[i + 1]));
      return mLines;
   }
   public void Fill(List<Point> mLines, int color) {
      List<int> intersections = new();
      for (double scan = 0.5; scan < Height; scan++) {
         intersections.Clear();
         for (int i = 0; i < mLines.Count; i += 2) {
            var j = i + 1;
            var (x1, y1, x2, y2) = (mLines[i].X, mLines[i].Y, mLines[j].X, mLines[j].Y);
            if ((y1 != y2) && (scan >= Math.Min(y1, y2)) && (scan < Math.Max(y1, y2))) {
               int P = (int)(x1 + (x2 - x1) * (scan - y1) / (y2 - y1));
               intersections.Add(P);
            }
         }
         intersections.Sort();
         int y = (int)(Height - scan - 0.5); // Since origin is at top left (y decreases with increasing x), we have to flip the y value.
         for (int n = 0; n < intersections.Count; n += 2)
            mBmp.DrawLine(intersections[n], y, intersections[n + 1], y, color);
      }
   }
}