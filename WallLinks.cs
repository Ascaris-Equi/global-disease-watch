using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

// The data bus behind the hardware models: GPU (PCIe) at the top, CPU and RAM in the middle, drives and
// network at the bottom. Particles on each lane move with the live figure for that link.
partial class WallView {
 class Flow {public Point[] Pts;public double[] Cum;public double Len;public readonly List<Shape> Dots=new List<Shape>();public double Level;public double Offset;}
 Canvas linkLayer;readonly List<Flow> flows=new List<Flow>();
 Flow pcieUp,pcieDown,memRight,memLeft,pageOut,pageIn,diskRead,diskWrite,netIn,netOut;
 const double BusX=328,BusTop=360,MemY=590,IoY=792,CpuX=290,RamX=368,DriveX=292,NetX=366;

 static Point P(double x,double y){return new Point(x,y);}

 void BuildLinks(){
  linkLayer=Put(stage,new Canvas(),0,0);
  var bus=Th.B(0xFF141C29);var lane=Th.B(0xFF22304A);
  Action<Point[],double> trace=(pts,width)=>{
   var g=new StreamGeometry();using(var c=g.Open()){c.BeginFigure(pts[0],false,false);c.PolyLineTo(pts.Skip(1).ToList(),true,true);}g.Freeze();
   linkLayer.Children.Add(new Path{Data=g,Stroke=bus,StrokeThickness=width,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round,StrokeLineJoin=PenLineJoin.Round});
  };
  trace(new[]{P(BusX,BusTop),P(BusX,IoY+6)},16);
  trace(new[]{P(CpuX,MemY+4),P(RamX,MemY+4)},16);
  trace(new[]{P(DriveX,IoY),P(NetX,IoY)},14);
  for(double x=BusX-6;x<=BusX+6;x+=3)linkLayer.Children.Add(new Line{X1=x,Y1=BusTop,X2=x,Y2=IoY+4,Stroke=lane,StrokeThickness=1});
  foreach(var y in new[]{MemY,MemY+8})linkLayer.Children.Add(new Line{X1=CpuX,Y1=y,X2=RamX,Y2=y,Stroke=lane,StrokeThickness=1});
  foreach(var y in new[]{IoY-4,IoY+4})linkLayer.Children.Add(new Line{X1=DriveX,Y1=y,X2=NetX,Y2=y,Stroke=lane,StrokeThickness=1});
  var via=Th.B(0xFF2E4060);
  foreach(var p in new[]{P(BusX,MemY+4),P(BusX,IoY),P(BusX,BusTop)})Put(linkLayer,new Ellipse{Width=10,Height=10,Fill=via},p.X-5,p.Y-5);

  var blue=Th.Accent;var violet=Th.Violet;var amber=Th.Amber;var green=Th.Green;var cyan=Th.B(0xFF6FD3FF);
  pcieUp=MakeFlow(new[]{P(CpuX,MemY+2),P(BusX-4,MemY+2),P(BusX-4,BusTop)},blue,false);
  pcieDown=MakeFlow(new[]{P(BusX+4,BusTop),P(BusX+4,MemY+6),P(CpuX,MemY+6)},blue,false);
  memRight=MakeFlow(new[]{P(CpuX,MemY),P(RamX,MemY)},violet,false);
  memLeft=MakeFlow(new[]{P(RamX,MemY+8),P(CpuX,MemY+8)},violet,false);
  pageOut=MakeFlow(new[]{P(RamX,MemY+8),P(BusX,MemY+8),P(BusX,IoY+4),P(DriveX,IoY+4)},amber,true);
  pageIn=MakeFlow(new[]{P(DriveX,IoY-4),P(BusX,IoY-4),P(BusX,MemY),P(RamX,MemY)},amber,true);
  diskRead=MakeFlow(new[]{P(DriveX,IoY-4),P(BusX-6,IoY-4),P(BusX-6,MemY+4),P(CpuX,MemY+4)},green,false);
  diskWrite=MakeFlow(new[]{P(CpuX,MemY+4),P(BusX-6,MemY+4),P(BusX-6,IoY+4),P(DriveX,IoY+4)},green,false);
  netIn=MakeFlow(new[]{P(NetX,IoY-4),P(BusX+6,IoY-4),P(BusX+6,MemY+4),P(CpuX,MemY+4)},cyan,false);
  netOut=MakeFlow(new[]{P(CpuX,MemY+4),P(BusX+6,MemY+4),P(BusX+6,IoY+4),P(NetX,IoY+4)},cyan,false);
 }

 Flow MakeFlow(Point[] pts,Brush color,bool square){
  var f=new Flow{Pts=pts,Cum=new double[pts.Length],Offset=flows.Count*.37};
  for(int i=1;i<pts.Length;i++)f.Cum[i]=f.Cum[i-1]+(pts[i]-pts[i-1]).Length;f.Len=f.Cum[pts.Length-1];
  for(int i=0;i<7;i++){
   Shape s=square?(Shape)new Rectangle{Width=5,Height=5,Fill=color}:new Ellipse{Width=4.5,Height=4.5,Fill=color};
   s.Visibility=Visibility.Hidden;s.Effect=null;s.RenderTransform=new TranslateTransform();linkLayer.Children.Add(s);f.Dots.Add(s);
  }
  flows.Add(f);return f;
 }

 static double Log01(double x,double max){return Math.Max(0,Math.Min(1,Math.Log10(1+Math.Max(0,x))/Math.Log10(1+max)));}

 Point Along(Flow f,double s){
  for(int i=1;i<f.Pts.Length;i++)if(s<=f.Cum[i]){double u=(s-f.Cum[i-1])/Math.Max(1e-6,f.Cum[i]-f.Cum[i-1]);return f.Pts[i-1]+(f.Pts[i]-f.Pts[i-1])*u;}
  return f.Pts[f.Pts.Length-1];
 }

 void TickLinks(double t){
  foreach(var f in flows){
   int n=f.Level<.02?0:1+(int)Math.Round(f.Level*(f.Dots.Count-1));double speed=30+150*f.Level;
   for(int i=0;i<f.Dots.Count;i++){
    var d=f.Dots[i];if(i>=n){d.Visibility=Visibility.Hidden;continue;}
    double s=((t*speed+f.Offset*f.Len+i*f.Len/n)%f.Len+f.Len)%f.Len;var p=Along(f,s);
    // Moved by transform, not Canvas.Left/Top, so particles never trigger a layout pass.
    d.Visibility=Visibility.Visible;d.Opacity=.55+.45*f.Level;var tt=(TranslateTransform)d.RenderTransform;tt.X=p.X-d.Width/2;tt.Y=p.Y-d.Height/2;
   }
  }
 }
}
