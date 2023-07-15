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

      Fill();
   }
   readonly GrayBMP mBmp;

   void Fill() {
      using var lines = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GrayBMP.Data.leaf-fill.txt"));
      var PolyPoints = lines.ReadToEnd().Replace("\r\n", " ").TrimEnd().Split(' ').Select(int.Parse).ToList();

      List<Point> mLines = new();
      for (int i = 0; i < PolyPoints.Count; i++) 
         mLines.Add(new Point(PolyPoints[i], PolyPoints[i++]));
     
      int ht = (int)Height;
      List<int> intersections = new();

      for (int y = 0; y < ht; y++) {
         var scan = y + 0.5;
         intersections.Clear();
         for (int i = 0; i < mLines.Count; i++) {
            var (j, P) = (i + 1, 0);
            var (x1, y1, x2, y2) = (mLines[i].X, mLines[i].Y, mLines[j].X, mLines[j].Y);
            if (y1 == y2) continue;
            if (x1 == x2) P = (int)x1;
            if ((scan >= Math.Min(y1, y2)) && (scan < Math.Max(y1, y2))) {
               P = (int)(x1 + (x2 - x1) * (y - y1) / (y2 - y1));
               intersections.Add(P);
            }
         }
         intersections.Sort();
         for (int n = 0; n < intersections.Count; n += 2)
            mBmp.DrawLine(intersections[n], ht - y, intersections[n + 1], ht - y, 255);
      }
   }
}