// Mandelbrot.cs - Multithreaded Mandelbrot set plotter. 
// There are 2 implementations here - one using Threads directly, and the
// other using Tasks to get work done on background threads. The Task based
// approach is more efficient because it does not have the overhead of creating
// threads on each frame render
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Concurrent;
namespace GrayBMP;

class MandelWin : Window {
   // MandelWin constructor
   public MandelWin () {
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

      // Start a timer to repaint a new frame every 33 milliseconds
      DispatcherTimer timer = new () {
         Interval = TimeSpan.FromMilliseconds (20), IsEnabled = true,
      };
      timer.Tick += NextFrame;
   }
   readonly GrayBMP mBmp;

   // This is called on every frame, and paints zooms in a bit closer to the
   // Mandelbrot
   void NextFrame (object sender, EventArgs e) {
      DrawMandelbrotTask (-0.59990625, -0.42907020097, mScale);
      mScale *= 1.02;
   }
   double mScale = 1000;

   #region Common code used by the Task approach and Thread approach
   // Basic mandelbrot computation function, computes the 'escape' value
   // for each point on the Complex plane (used to decide the color)
   static byte Escape (Complex c) {
      Complex z = Complex.Zero;
      for (int i = 1; i < 255; i++) {
         if (z.NormSq > 4) return (byte)i;
         z = z * z + c;
      }
      return 0;
   }

   // Helper used to draw one row of the Mandelbrot set. This routine will
   // be called in parallel from many threads. However, since each will be using
   // a different value of y (and because of the padding added by the mStride
   // of the bitmap), the multiple threads will not step on each other - each
   // will be operating on a distinct different area of the bitmap
   void DrawMandelbrotRow (int y) {
      unsafe {
         byte* ptr = (byte*)(mBase + y * mStride);
         for (int x = 0; x < mDX; x++) {
            Complex c = new (mX1 + x * mStep, mY1 - y * mStep);
            *ptr++ = Escape (c);
         }
      }
   }

   // A common helper routine used to prepare for drawing the Mandelbrot
   // set, used by both DrawMandelbrotTask and DrawMandelbrotThread variants
   void PrepareMandelbrot (double xc, double yc, double zoom) {
      mBase = mBmp.Buffer;
      mStride = mBmp.Stride;
      mDX = mBmp.Width; mDY = mBmp.Height;
      mStep = 2.0 / mDY / zoom;
      mX1 = xc - mStep * mDX / 2; mY1 = yc + mStep * mDY / 2;

      // Add into the queue all the rows that need to be rendered. 
      // The Tasks (or Threads) will keep emptying this queue until they
      // are done
      mQueue.Clear ();
      for (int i = 0; i < mDY; i++) mQueue.Enqueue (i);
   }
   double mX1, mY1;  // Complex coordinates of the top-left corner being displayed
   double mStep;     // Each pixel corresponds to a step of this length in the complex plane
   nint mBase;       // Base pointer of the bitmap
   int mDX, mDY;     // Width, height of the bitmap (in pixels)
   int mStride;      // Spacing between each bitmap row (in bytes)
   ConcurrentQueue<int> mQueue = new ();  // Stores all the rows to be plotted
   #endregion

   #region Task approach - using background threads controlled by Task objects
   // A version to draw the mandelbrot set a set of background threads
   // (controlled using the Task API). 
   void DrawMandelbrotTask (double xc, double yc, double zoom) {
      mBmp.Begin ();
      PrepareMandelbrot (xc, yc, zoom);

      // Create an array of tasks (equal to the number of CPUs in the
      // system). Each task will fetch pending rows from the mQueue and 
      // render that row. When all tasks are done (the queue is empty),
      // the mandelbrot set is done 
      Task[] tasks = new Task[Environment.ProcessorCount];
      for (int i = 0; i < tasks.Length; i++)
         tasks[i] = Task.Run (TaskProc);
      Task.WaitAll (tasks);
      mBmp.Dirty (0, 0, mDX - 1, mDY - 1);
      mBmp.End ();
   }

   // This is the function being executed by each of the tasks
   void TaskProc () {
      while (mQueue.TryDequeue (out int y))
         DrawMandelbrotRow (y);
   }
   #endregion

   #region Thread approach - creating threads directly
   // Another verision to draw the mandelbrot set using a set of threads
   // we create directly. This is not as efficient as the DrawMandelbrotTask
   // method above, since we incur the penalty of creating a fresh set of 
   // threads each time we have to render a frame (the other routine just reuses
   // the same set of background threads)
   void DrawMandelbrotThread (double xc, double yc, double zoom) {
      mBmp.Begin ();

      // We create N threads, and use a CountdownEvent to track if they
      // are all completed. Each time a thread completes, it will signal
      // the event, and when all N threads are done, the event.Wait will
      // be done.
      int threads = Environment.ProcessorCount; ;
      mCEV = new CountdownEvent (threads);
      for (int i = 0; i < threads; i++)
         new Thread (ThreadProc).Start ();
      mCEV.Wait ();
      mBmp.Dirty (0, 0, mDX - 1, mDY - 1);
      mBmp.End ();
   }
   CountdownEvent mCEV;

   // This is the function executed by each thread
   void ThreadProc () {
      while (mQueue.TryDequeue (out int y))
         DrawMandelbrotRow (y);
      // Once this thread is done, raise a signal on the CountdownEvent.
      // When all N threads have signalled, the frame is done and the bitmap
      // is sent to the display
      mCEV.Signal ();
   }
   #endregion
}
