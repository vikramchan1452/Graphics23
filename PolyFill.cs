// PolyFill.cs - Polygon filler
// ---------------------------------------------------------------------------------------
namespace GrayBMP;

class PolyFill {
   public void AddLine (int x0, int y0, int x1, int y1) {
      if (y0 == y1) return;
      if (y0 > y1) (x0, y0, x1, y1) = (x1, y1, x0, y0);
      mYMin = Math.Min (mYMin, y0);
      mYMax = Math.Max (mYMax, y1);
      mEvents.Add (new Event (y0, mEdges.Count));
      double dx = (double)(x1 - x0) / (y1 - y0);
      mEdges.Add (new Edge (x0, dx, y1 - y0));
   }
   int mYMin = int.MaxValue, mYMax = int.MinValue;

   public void AddLine (Point2 a, Point2 b) {
      var ((x0, y0), (x1, y1)) = (a.Round (), b.Round ());
      AddLine (x0, y0, x1, y1);
   }

   public void Fill (GrayBMP bmp, int color) {
      bmp.Begin ();

      mEvents.Sort ();
      List<Edge> active = new ();
      List<int> ints = new ();
      int nEvent = 0;   // Next event to process
      for (int y = mYMin; y <= mYMax; y++) {
         // Remove dead edges from the AEL
         active.RemoveAll (a => a.Life == 0);

         // Figure out all the edges that need to come in here
         while (nEvent < mEvents.Count && mEvents[nEvent].Y == y) 
            active.Add (mEdges[mEvents[nEvent++].Index]);

         ints.Clear ();
         for (int i = 0; i < active.Count; i++) {
            var e = active[i];
            ints.Add ((int)(e.X + 0.5));
            e.X += e.DX; e.Life--;
            active[i] = e;
         }
         ints.Sort ();
         for (int i = 0; i < ints.Count; i += 2)
            bmp.DrawHorizontalLine (ints[i], ints[i + 1], y, color);
      }

      bmp.Dirty (0, mYMin, bmp.Width - 1, mYMax);
      bmp.End ();
   }

   public void Reset () {
      mEvents.Clear (); mEdges.Clear ();
      mYMin = int.MaxValue; mYMax = int.MinValue;
   }

   readonly record struct Event (int Y, int Index) : IComparable<Event> {
      public int CompareTo (Event other) => Y.CompareTo (other.Y);
   }
   List<Event> mEvents = new ();
   record struct Edge (double X, double DX, int Life) : IComparable<Edge> {
      public int CompareTo (Edge other) {
         int n = X.CompareTo (other.X); if (n != 0) return n;
         return DX.CompareTo (other.DX);
      }
   }
   List<Edge> mEdges = new ();
}
