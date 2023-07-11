// LinesWin.cs - Demo window for testing the DrawLine and related functions
// ---------------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Reflection;
using System.IO;
namespace GrayBMP;

class PolyFillWin : Window {
   public PolyFillWin () {
      Width = 900; Height = 600;
      Left = 200; Top = 50; WindowStyle = WindowStyle.None;

      mBmp = new GrayBMP (Width, Height);
      Image image = new () {
         Stretch = Stretch.Fill,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
         Source = mBmp.Bitmap
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Unspecified);
      Content = image;

      mDwg = LoadDrawing ();
      DrawLeaf ();
   }
   readonly GrayBMP mBmp;

   void DrawLeaf () {
      mBmp.Begin ();
      mBmp.Clear (192);
      PolyFill pf = new ();
      foreach (var line in mDwg.Lines) {
         var ((x0, y0), (x1, y1)) = (line.A.Round (), line.B.Round ());
         pf.AddLine (x0, y0, x1, y1);
      }
      pf.Fill (mBmp, 255);

      foreach (var line in mDwg.Lines) {
         var ((x0, y0), (x1, y1)) = (line.A.Round (), line.B.Round ());
         mBmp.DrawLine (x0, y0, x1, y1, 0);
      }
      mBmp.End ();
   }

   Drawing LoadDrawing () {
      Drawing dwg = new ();
      using (var stm = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("GrayBMP.Data.leaf-fill.txt"))
      using (var sr = new StreamReader (stm)) {
         for (; ; ) {
            string line = sr.ReadLine (); if (line == null) break;
            double[] w = line.Split ().Select (double.Parse).ToArray ();
            Point2 a = new (w[0], w[1]), b = new (w[2], w[3]);
            dwg.AddLine (new Line (a, b));
         }
      }
      return dwg;
   }
   Drawing mDwg;
}