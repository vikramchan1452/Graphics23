// Program.cs - Entry point into the GrayBMP application   
// ---------------------------------------------------------------------------------------
using System.Windows;
namespace GrayBMP;

class Program {
   [STAThread]
   static void Main () {
      // Create a PolyFillWin that demonstrates polygon filling
      new PolyFillWin ().Show ();
      new Application ().Run ();
   }

   [STAThread]
   static void Main1 () {
      // Create a MandelWin that shows an animated Mandelbrot set,
      // and create an Application object to do message-pumping and keep
      // the window alive
      new MandelWin ().Show ();
      new Application ().Run ();
   }

   [STAThread]
   static void Main2 () {
      // Create a LinesWin that demonstrates the Line Drawing
      new LinesWin ().Show ();
      new Application ().Run ();
   }
}
