// PolyFillWin.cs - Demo window for testing the PolyFill class
// ---------------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Reflection;
using System.IO;
using System.Windows.Threading;

namespace GrayBMP;

class PolyFillWin : Window {
   public PolyFillWin () {
      Width = 900; Height = 600;
      Left = 200; Top = 50; WindowStyle = WindowStyle.None;

      mBmp = new GrayBMP (Width * mScale, Height * mScale);
      Image image = new () {
         Stretch = Stretch.Fill,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
         Source = mBmp.Bitmap
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.HighQuality);
      RenderOptions.SetEdgeMode (image, EdgeMode.Unspecified);
      Content = image;

      mDwg = LoadDrawing ();
      DispatcherTimer timer = new () {
         Interval = TimeSpan.FromMilliseconds (500), IsEnabled = true,
      };
      timer.Tick += NextFrame;
   }
   readonly GrayBMP mBmp;
   readonly int mScale = 1;

   void NextFrame (object s, EventArgs e) {
      using (new BlockTimer ("Leaf")) {
         mBmp.Begin ();
         DrawLeaf ();
         // DrawFatLines ();
         mBmp.End ();
      }
   }

   void DrawFatLines () {
      mBmp.Begin ();
      mBmp.Clear (192);
      int thick = 2;
      for (int x = 50; x < 850; x += 100, thick += 5) 
         mBmp.DrawThickLine (new Point2(50, 550), new (x, 50), thick, 0);
      mBmp.End ();
   }

   void DrawLeaf () {
      mBmp.Begin ();
      mBmp.Clear (192);
      mPF.Reset ();
      foreach (var (a, b) in EnumLines ())
         mPF.AddLine (a, b);
      mPF.Fill (mBmp, 255);
      foreach (var (a, b) in EnumLines ())
         mBmp.DrawLine (a, b, 0);
      mBmp.End ();

      // Enumerate all the lines from the Dwg
      IEnumerable<(Point2 A, Point2 B)> EnumLines () {
         foreach (var poly in mDwg.Polys) {
            for (int n = poly.Pts.Count, i = 0; i < n; i++) {
               int j = (i + 1) % n;
               yield return (poly.Pts[i], poly.Pts[j]);
            }
         }
      }
   }
   PolyFill mPF = new ();

   Drawing LoadDrawing () {
      Drawing dwg = new ();
      using (var stm = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("GrayBMP.Data.leaf-poly.txt"))
      using (var sr = new StreamReader (stm)) {
         List<Point2> pts = new ();
         int cPolys = int.Parse (sr.ReadLine ());
         for (int i = 0; i < cPolys; i++) {
            pts.Clear ();
            int cNodes = int.Parse (sr.ReadLine ());
            for (int j = 0; j < cNodes; j++) {
               double[] w = sr.ReadLine ().Trim ().Split ().Select (double.Parse).Select (a => a * 3.5).ToArray ();
               pts.Add (new (w[0] + 20, w[1] + 20));
            }
            dwg.Add (new Polygon (pts));
         }
      }
      return dwg;
   }
   Drawing mDwg;
}