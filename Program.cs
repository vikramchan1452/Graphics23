using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.Concurrent;

namespace A25;

class MyWindow : Window {
   public MyWindow () {
      Width = 800; Height = 600;
      Left = 50; Top = 50;
      WindowStyle = WindowStyle.None;
      mBmp = new GrayBMP (Width, Height);

      Image image = new () {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
         Source = mBmp.Bitmap
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);
      Content = image;

      DispatcherTimer timer = new DispatcherTimer () {
         Interval = TimeSpan.FromMilliseconds (30), IsEnabled = true,
      };
      timer.Tick += NextFrame;
   }

   void NextFrame (object? sender, EventArgs e) {
      DrawMandelbrotThread (-0.59990625, -0.42907020097, mScale);
      mScale *= 1.02;
   }
   double mScale = 1000;

   void DrawMandelbrotRow (int y) {
      unsafe {
         byte* ptr = (byte*)(mBase + y * mStride);
         for (int x = 0; x < mDX; x++) {
            Complex c = new (mX1 + x * mStep, mY1 - y * mStep);
            *ptr++ = Escape (c);
         }
      }
   }

   void DrawMandelbrot (double xc, double yc, double zoom) {
      try {
         mBmp.Begin ();
         mBase = mBmp.Buffer;
         mStride = mBmp.Stride;
         mDX = mBmp.Width; mDY = mBmp.Height;
         mStep = 2.0 / mDY / zoom;
         mX1 = xc - mStep * mDX / 2; mY1 = yc + mStep * mDY / 2;

         int threads = 50;
         mQueue.Clear ();
         for (int i = 0; i < mDY; i++) mQueue.Enqueue (i);
         List<Task> tasks = new List<Task> ();
         for (int i = 0; i < threads; i++)
            tasks.Add (Task.Run (TaskProc));
         Task.WaitAll (tasks.ToArray ());
         mBmp.Dirty (0, 0, mDX - 1, mDY - 1);

      } finally {
         mBmp.End ();
      }
   }

   void TaskProc () {
      while (mQueue.TryDequeue (out int y))
         DrawMandelbrotRow (y);
   }

   void DrawMandelbrotThread (double xc, double yc, double zoom) {
      try {
         mBmp.Begin ();
         mBase = mBmp.Buffer;
         mStride = mBmp.Stride;
         mDX = mBmp.Width; mDY = mBmp.Height;
         mStep = 2.0 / mDY / zoom;
         mX1 = xc - mStep * mDX / 2; mY1 = yc + mStep * mDY / 2;

         int threads = 50;
         mQueue.Clear ();
         for (int i = 0; i < mDY; i++) mQueue.Enqueue (i);
         mCEV = new CountdownEvent (threads);
         for (int i = 0; i < threads; i++) {
            Thread t1 = new Thread (ThreadProc);
            t1.Start ();
         }
         mCEV.Wait ();
         mBmp.Dirty (0, 0, mDX - 1, mDY - 1);

      } finally {
         mBmp.End ();
      }
   }

   double mX1, mY1, mStep;
   nint mBase;
   int mYNext, mStride, mDX, mDY;
   ConcurrentQueue<int> mQueue = new ConcurrentQueue<int> ();
   CountdownEvent mCEV;

   void ThreadProc () {
      while (mQueue.TryDequeue (out int y))
         DrawMandelbrotRow (y);
      mCEV.Signal ();
   }

   byte Escape (Complex c) {
      Complex z = Complex.Zero;
      for (int i = 1; i < 255; i++) {
         if (z.NormSq > 4) return (byte)i;
         z = z * z + c;
      }
      return 0;
   }

   GrayBMP mBmp;
}

internal class Program {
   [STAThread]
   static void Main (string[] args) {
      Window w = new MyWindow ();
      w.Show ();
      Application app = new Application ();
      app.Run ();
   }
}
