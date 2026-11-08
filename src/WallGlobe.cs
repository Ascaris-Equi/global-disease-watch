using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

// Textured, lit globe. Countries with recent reports are painted into the texture; the globe
// eases from one reporting country to the next so the featured country always sits at the centre.
class GlobeScene {
 public readonly Viewport3D View=new Viewport3D();
 readonly AxisAngleRotation3D yaw=new AxisAngleRotation3D(new Vector3D(0,1,0),0),pitch=new AxisAngleRotation3D(new Vector3D(1,0,0),0);
 readonly DiffuseMaterial surface=new DiffuseMaterial();
 readonly Model3DGroup markers=new Model3DGroup();
 readonly List<Tuple<ScaleTransform3D,SolidColorBrush,double>> pulses=new List<Tuple<ScaleTransform3D,SolidColorBrush,double>>();
 double fromLon=10,fromLat=20,toLon=10,toLat=20,moveStart=-100;

 public static readonly Color Ocean=Color.FromRgb(13,27,41),Land=Color.FromRgb(36,58,80),Coast=Color.FromRgb(66,96,124),Hot=Color.FromRgb(255,112,82),HotEdge=Color.FromRgb(255,196,178);

 public GlobeScene(){
  WallView.LiteView(View,false);
  // 3D 视口按最终分辨率合成：Viewbox 放大到 4K 时不要退回低质量双线性。
  RenderOptions.SetBitmapScalingMode(View,BitmapScalingMode.HighQuality);
  var lights=new Model3DGroup();
  lights.Children.Add(new AmbientLight(Color.FromRgb(92,100,116)));
  lights.Children.Add(new DirectionalLight(Color.FromRgb(196,202,214),new Vector3D(.62,-.42,-.66)));
  View.Children.Add(new ModelVisual3D{Content=lights});
  var globe=new Model3DGroup();
  var sp=new SolidColorBrush(Color.FromArgb(40,200,225,255));sp.Freeze();
  var mat=new MaterialGroup();mat.Children.Add(surface);if(!WallView.Lite("nospec"))mat.Children.Add(new SpecularMaterial(sp,24));
  globe.Children.Add(new GeometryModel3D(WallView.Lite("mesh")?Sphere(1,32,64):Sphere(1,48,96),mat));
  globe.Children.Add(markers);globe.Children.Add(arcs);
  var tf=new Transform3DGroup();tf.Children.Add(new RotateTransform3D(yaw));tf.Children.Add(new RotateTransform3D(pitch));globe.Transform=tf;
  View.Children.Add(new ModelVisual3D{Content=globe});
  View.Camera=new OrthographicCamera(new Point3D(0,0,5),new Vector3D(0,0,-1),new Vector3D(0,1,0),2.0);
 }

 // The viewport is square; the sphere of radius 1 fills `fill` of its width.
 public void Frame(double fill){((OrthographicCamera)View.Camera).Width=2/fill;}

 static Vector3D Unit(double lat,double lon){double a=lat*Math.PI/180,b=lon*Math.PI/180;return new Vector3D(Math.Cos(a)*Math.Sin(b),Math.Sin(a),Math.Cos(a)*Math.Cos(b));}

 static MeshGeometry3D Sphere(double r,int rows,int cols){
  var m=new MeshGeometry3D();
  for(int i=0;i<=rows;i++){
   double lat=90-180.0*i/rows;
   for(int j=0;j<=cols;j++){double lon=-180+360.0*j/cols;var n=Unit(lat,lon);m.Positions.Add((Point3D)(n*r));m.Normals.Add(n);m.TextureCoordinates.Add(new Point((double)j/cols,(double)i/rows));}
  }
  for(int i=0;i<rows;i++)for(int j=0;j<cols;j++){
   int a=i*(cols+1)+j,b=a+cols+1;
   m.TriangleIndices.Add(a);m.TriangleIndices.Add(b);m.TriangleIndices.Add(a+1);
   m.TriangleIndices.Add(a+1);m.TriangleIndices.Add(b);m.TriangleIndices.Add(b+1);
  }
  m.Freeze();return m;
 }

 // Equirectangular texture: ocean, graticule, land, highlighted reporting countries.
 // 4096×2048 (~32 MB) so the sphere never magnifies a low-res map; stroke widths scale ×2 with
 // the canvas and keep the same on-screen weight as the old 2048 paint.
 public void Paint(Dictionary<string,object> world,Dictionary<string,Color> hot){
  const int W=4096,H=2048;
  var dv=new DrawingVisual();
  using(var dc=dv.RenderOpen()){
   dc.DrawRectangle(new SolidColorBrush(Ocean),null,new Rect(0,0,W,H));
   var grid=new Pen(new SolidColorBrush(Color.FromRgb(24,44,64)),2.8);
   for(int lon=-180;lon<=180;lon+=15)dc.DrawLine(grid,new Point((lon+180)/360.0*W,0),new Point((lon+180)/360.0*W,H));
   for(int lat=-75;lat<=75;lat+=15)dc.DrawLine(grid,new Point(0,(90-lat)/180.0*H),new Point(W,(90-lat)/180.0*H));
   var landFill=new SolidColorBrush(Land);var coast=new Pen(new SolidColorBrush(Coast),2.6);
   foreach(bool drawHot in new[]{false,true})
   foreach(var c in J.A(world,"countries")){
    Color hc;bool isHot=hot.TryGetValue(J.S(c,"iso"),out hc);if(isHot!=drawHot)continue;
    var rings=J.Get(c,"rings") as IEnumerable;if(rings==null)continue;
    var geo=new StreamGeometry();
    using(var g=geo.Open()){
     foreach(IEnumerable ring in rings){
      var pts=new List<Point>();
      foreach(IEnumerable pair in ring){var v=pair.Cast<object>().Select(o=>Convert.ToDouble(o)).ToArray();if(v.Length>=2)pts.Add(new Point((v[0]+180)/360*W,(90-v[1])/180*H));}
      if(pts.Count<3)continue;g.BeginFigure(pts[0],true,true);g.PolyLineTo(pts.Skip(1).ToList(),true,true);
     }
    }
    geo.Freeze();dc.DrawGeometry(isHot?new SolidColorBrush(Color.FromArgb(225,hc.R,hc.G,hc.B)):landFill,isHot?new Pen(new SolidColorBrush(Color.FromArgb(255,(byte)(hc.R/2+128),(byte)(hc.G/2+128),(byte)(hc.B/2+128))),3.6):coast,geo);
   }
  }
  var bmp=new RenderTargetBitmap(W,H,96,96,PixelFormats.Pbgra32);bmp.Render(dv);bmp.Freeze();
  var brush=new ImageBrush(bmp){ViewportUnits=BrushMappingMode.Absolute,Viewport=new Rect(0,0,1,1)};
  RenderOptions.SetBitmapScalingMode(brush,BitmapScalingMode.HighQuality);
  brush.Freeze();
  surface.Brush=brush;
 }

 public void SetMarkers(IEnumerable<Tuple<double,double,Color,double>> points){
  markers.Children.Clear();pulses.Clear();int i=0;
  foreach(var p in points){
   var n=Unit(p.Item1,p.Item2);var col=p.Item3;double size=p.Item4;
   var g=new Model3DGroup();
   var dot=new SolidColorBrush(col);dot.Freeze();
   var core=new MaterialGroup();core.Children.Add(new DiffuseMaterial(Brushes.Black));core.Children.Add(new EmissiveMaterial(dot));
   g.Children.Add(new GeometryModel3D(Disc(0,.012+.012*size,20),core));
   var ringBrush=new SolidColorBrush(Color.FromArgb(0,col.R,col.G,col.B));var scale=new ScaleTransform3D(1,1,1);
   var ring=new GeometryModel3D(Disc(.03+.012*size,.038+.012*size,32),new EmissiveMaterial(ringBrush)){Transform=scale};
   g.Children.Add(ring);pulses.Add(Tuple.Create(scale,ringBrush,i*.37));
   g.Transform=new MatrixTransform3D(Tangent(n,1.004));
   markers.Children.Add(g);i++;
  }
 }
 // Orientation that lays a +Z-facing disc tangent to the sphere at n, lifted to radius `lift`.
 static Matrix3D Tangent(Vector3D n,double lift){
  var m=Matrix3D.Identity;var z=new Vector3D(0,0,1);var axis=Vector3D.CrossProduct(z,n);double s=axis.Length,c=Vector3D.DotProduct(z,n);
  if(s>1e-6)m.Rotate(new Quaternion(axis/s,Math.Atan2(s,c)*180/Math.PI));else if(c<0)m.Rotate(new Quaternion(new Vector3D(1,0,0),180));
  m.Translate(n*lift);return m;
 }

 // Great-circle arcs lifted off the surface, with a particle travelling from the first point to the second.
 readonly Model3DGroup arcs=new Model3DGroup();
 readonly List<Tuple<Point3D[],TranslateTransform3D,double>> travellers=new List<Tuple<Point3D[],TranslateTransform3D,double>>();
 public void SetArcs(IEnumerable<Tuple<double,double,double,double,Color>> links){
  arcs.Children.Clear();travellers.Clear();int k=0;
  foreach(var l in links){
   var a=Unit(l.Item1,l.Item2);var b=Unit(l.Item3,l.Item4);double ang=Math.Acos(Math.Max(-1,Math.Min(1,Vector3D.DotProduct(a,b))));
   if(ang<.02)continue;const int N=48;var pts=new Point3D[N+1];
   for(int i=0;i<=N;i++){double u=(double)i/N;var v=(a*Math.Sin((1-u)*ang)+b*Math.Sin(u*ang))/Math.Sin(ang);pts[i]=(Point3D)(v*(1.006+(.05+.2*ang/Math.PI)*Math.Sin(Math.PI*u)));}
   var col=l.Item5;var line=new SolidColorBrush(Color.FromArgb(150,col.R,col.G,col.B));line.Freeze();
   arcs.Children.Add(new GeometryModel3D(Tube(pts,.0045),new EmissiveMaterial(line)));
   var head=new SolidColorBrush(Color.FromArgb(255,(byte)(col.R/2+128),(byte)(col.G/2+128),(byte)(col.B/2+128)));head.Freeze();
   var tr=new TranslateTransform3D();arcs.Children.Add(new GeometryModel3D(Ball(.016),new EmissiveMaterial(head)){Transform=tr});
   travellers.Add(Tuple.Create(pts,tr,k*.29));k++;
  }
 }
 static MeshGeometry3D Tube(Point3D[] pts,double r){
  var m=new MeshGeometry3D();const int sides=6;
  for(int i=0;i<pts.Length;i++){
   var t=pts[Math.Min(pts.Length-1,i+1)]-pts[Math.Max(0,i-1)];t.Normalize();var u=Vector3D.CrossProduct(t,(Vector3D)pts[i]);u.Normalize();var v=Vector3D.CrossProduct(t,u);
   for(int s=0;s<sides;s++){double th=2*Math.PI*s/sides;var nn=u*Math.Cos(th)+v*Math.Sin(th);m.Positions.Add(pts[i]+nn*r);m.Normals.Add(nn);}
  }
  for(int i=0;i<pts.Length-1;i++)for(int s=0;s<sides;s++){int a=i*sides+s,b=i*sides+(s+1)%sides,c=a+sides,d=b+sides;m.TriangleIndices.Add(a);m.TriangleIndices.Add(b);m.TriangleIndices.Add(c);m.TriangleIndices.Add(b);m.TriangleIndices.Add(d);m.TriangleIndices.Add(c);}
  m.Freeze();return m;
 }
 static MeshGeometry3D Ball(double r){
  var m=new MeshGeometry3D();const int rows=6,cols=10;
  for(int i=0;i<=rows;i++){double lat=Math.PI*i/rows-Math.PI/2;for(int j=0;j<=cols;j++){double lon=2*Math.PI*j/cols;var n=new Vector3D(Math.Cos(lat)*Math.Cos(lon),Math.Sin(lat),Math.Cos(lat)*Math.Sin(lon));m.Positions.Add((Point3D)(n*r));m.Normals.Add(n);}}
  for(int i=0;i<rows;i++)for(int j=0;j<cols;j++){int a=i*(cols+1)+j,b=a+cols+1;m.TriangleIndices.Add(a);m.TriangleIndices.Add(b);m.TriangleIndices.Add(a+1);m.TriangleIndices.Add(a+1);m.TriangleIndices.Add(b);m.TriangleIndices.Add(b+1);}
  m.Freeze();return m;
 } static MeshGeometry3D Disc(double r0,double r1,int seg){
  var m=new MeshGeometry3D();
  for(int k=0;k<=seg;k++){double a=2*Math.PI*k/seg;m.Positions.Add(new Point3D(Math.Cos(a)*r0,Math.Sin(a)*r0,0));m.Positions.Add(new Point3D(Math.Cos(a)*r1,Math.Sin(a)*r1,0));m.Normals.Add(new Vector3D(0,0,1));m.Normals.Add(new Vector3D(0,0,1));}
  for(int k=0;k<seg;k++){int a=2*k;m.TriangleIndices.Add(a);m.TriangleIndices.Add(a+1);m.TriangleIndices.Add(a+3);m.TriangleIndices.Add(a);m.TriangleIndices.Add(a+3);m.TriangleIndices.Add(a+2);}
  m.Freeze();return m;
 }

 double Lon(double t){double u=Ease((t-moveStart)/2.4);return fromLon+Wrap(toLon-fromLon)*u;}
 double Lat(double t){double u=Ease((t-moveStart)/2.4);return fromLat+(toLat-fromLat)*u;}
 static double Wrap(double d){while(d>180)d-=360;while(d<-180)d+=360;return d;}
 static double Ease(double u){u=Math.Max(0,Math.Min(1,u));return u<.5?4*u*u*u:1-Math.Pow(-2*u+2,3)/2;}

 public void FocusOn(double lat,double lon,double t,bool instant){
  if(instant){fromLon=toLon=lon;fromLat=toLat=lat;moveStart=t-10;return;}
  fromLon=Lon(t);fromLat=Lat(t);toLon=lon;toLat=Math.Max(-50,Math.Min(55,lat));moveStart=t;
 }

 // Where the focused country currently projects, in sphere radii from the centre (y up).
 public Vector FocusOffset(){
  var m=Matrix3D.Identity;m.Rotate(new Quaternion(yaw.Axis,yaw.Angle));m.Rotate(new Quaternion(pitch.Axis,pitch.Angle));
  var p=m.Transform(Unit(toLat,toLon));return new Vector(p.X,p.Y);
 }

 public void Tick(double t){
  double drift=4*Math.Sin(t*.13);
  yaw.Angle=-(Lon(t)+drift);pitch.Angle=Lat(t);
  foreach(var p in pulses){
   double u=((t*.38+p.Item3)%1+1)%1;double s=.8+u*1.5;p.Item1.ScaleX=s;p.Item1.ScaleY=s;
   var pc=p.Item2.Color;p.Item2.Color=Color.FromArgb((byte)(160*(1-u)*(1-u)),pc.R,pc.G,pc.B);
  }
  foreach(var tv in travellers){double u=((t*.22+tv.Item3)%1+1)%1;double f=u*(tv.Item1.Length-1);int i0=(int)f;int i1=Math.Min(tv.Item1.Length-1,i0+1);var q=tv.Item1[i0]+(tv.Item1[i1]-tv.Item1[i0])*(f-i0);tv.Item2.OffsetX=q.X;tv.Item2.OffsetY=q.Y;tv.Item2.OffsetZ=q.Z;}
 }
}
