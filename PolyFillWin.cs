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
   readonly int mScale = 16;

   void NextFrame (object s, EventArgs e) {
      using (new BlockTimer ("Leaf")) {
         mBmp.Begin ();
         DrawLeaf ();
         mBmp.End ();
      }
   }

   void DrawLeaf () {
      mBmp.Begin ();
      mBmp.Clear (192);
      mPF.Reset ();
      foreach (var line in mDwg.Lines) {
         var ((x0, y0), (x1, y1)) = (line.A.Round (), line.B.Round ());
         mPF.AddLine (x0, y0, x1, y1);
      }
      mPF.Fill (mBmp, 255);

      foreach (var line in mDwg.Lines) {
         var ((x0, y0), (x1, y1)) = (line.A.Round (), line.B.Round ());
         mBmp.DrawThickLine (x0, y0, x1, y1, mScale, 0);
      }
      mBmp.End ();
   }
   PolyFillFast mPF = new ();

   Drawing LoadDrawing () {
      Drawing dwg = new ();
      using (var stm = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("GrayBMP.Data.leaf-fill.txt"))
      using (var sr = new StreamReader (stm)) {
         for (; ; ) {
            string line = sr.ReadLine (); if (line == null) break;
            double[] w = line.Split ().Select (double.Parse).Select (a => a * mScale).ToArray ();
            Point2 a = new (w[0], w[1]), b = new (w[2], w[3]);
            dwg.AddLine (new Line (a, b));
         }
      }
      return dwg;
   }
   Drawing mDwg;
}