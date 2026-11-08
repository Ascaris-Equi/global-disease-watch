using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

// Procedural virus particle: capsid sphere with spikes on an icosahedral-ish lattice, slow rotation.
class VirusScene {
 public readonly Viewport3D View=new Viewport3D();
 readonly AxisAngleRotation3D spin=new AxisAngleRotation3D(new Vector3D(0,1,0),0);
 readonly AxisAngleRotation3D tilt=new AxisAngleRotation3D(new Vector3D(1,0,0),-12);
 public readonly SolidColorBrush Glow=new SolidColorBrush(Color.FromRgb(255,96,74));
 double size=1;

 public VirusScene(double diameter){
  size=diameter;
  WallView.LiteView(View,false);
  var lights=new Model3DGroup();
  lights.Children.Add(new AmbientLight(Color.FromRgb(70,74,86)));
  lights.Children.Add(new DirectionalLight(Color.FromRgb(226,214,210),new Vector3D(.4,-.6,-.7)));
  View.Children.Add(new ModelVisual3D{Content=lights});
  var root=new Model3DGroup();
  var tf=new Transform3DGroup();tf.Children.Add(new RotateTransform3D(tilt));tf.Children.Add(new RotateTransform3D(spin));
  root.Transform=tf;root.Children.Add(Capsid(1.0));
  foreach(var d in Lattice(58))root.Children.Add(Spike(d));
  View.Children.Add(new ModelVisual3D{Content=root});
  var cam=new OrthographicCamera(new Point3D(0,0,6),new Vector3D(0,0,-1),new Vector3D(0,1,0),2.6);
  View.Camera=cam;
  View.Width=diameter;View.Height=diameter;
 }

 static IEnumerable<Vector3D> Lattice(int n){
  // Fibonacci sphere: even coverage without clustering at the poles.
  double ga=Math.PI*(3-Math.Sqrt(5));
  for(int i=0;i<n;i++){
   double y=1-(i/(double)(n-1))*2;double r=Math.Sqrt(Math.Max(0,1-y*y));double th=ga*i;
   yield return new Vector3D(Math.Cos(th)*r,y,Math.Sin(th)*r);
  }
 }

 static MeshGeometry3D Sphere(double r,int rows,int cols){
  var m=new MeshGeometry3D();
  for(int i=0;i<=rows;i++){
   double lat=Math.PI*i/rows-Math.PI/2;
   for(int j=0;j<=cols;j++){double lon=2*Math.PI*j/cols;var n=new Vector3D(Math.Cos(lat)*Math.Cos(lon),Math.Sin(lat),Math.Cos(lat)*Math.Sin(lon));m.Positions.Add((Point3D)(n*r));m.Normals.Add(n);}
  }
  for(int i=0;i<rows;i++)for(int j=0;j<cols;j++){int a=i*(cols+1)+j,b=a+cols+1;m.TriangleIndices.Add(a);m.TriangleIndices.Add(b);m.TriangleIndices.Add(a+1);m.TriangleIndices.Add(a+1);m.TriangleIndices.Add(b);m.TriangleIndices.Add(b+1);}
  m.Freeze();return m;
 }

 GeometryModel3D Capsid(double r){
  var mat=new MaterialGroup();
  var body=new SolidColorBrush(Color.FromRgb(64,26,26));body.Freeze();
  mat.Children.Add(new DiffuseMaterial(body));
  var em=new SolidColorBrush(Color.FromArgb(90,120,40,34));em.Freeze();
  mat.Children.Add(new EmissiveMaterial(em));
  return new GeometryModel3D(Sphere(r,20,36),mat);
 }

 GeometryModel3D Spike(Vector3D n){
  // Cone approximated by a 4-sided pyramid with a narrow tip, standing on the capsid surface.
  double baseR=0.055,height=0.16;
  var a=new Vector3D(0,1,0);if(Math.Abs(Vector3D.DotProduct(a,n))>0.9)a=new Vector3D(1,0,0);
  var u=Vector3D.CrossProduct(a,n);u.Normalize();var v=Vector3D.CrossProduct(n,u);
  var tip=n*(1+height);
  var p0=n*1.0;
  var m=new MeshGeometry3D();
  for(int k=0;k<4;k++){
   double ang=Math.PI/2*k;
   var p=p0+u*(Math.Cos(ang)*baseR)+v*(Math.Sin(ang)*baseR);
   m.Positions.Add((Point3D)p);var nrm=p-tip;nrm.Normalize();m.Normals.Add(nrm);
  }
  m.Positions.Add((Point3D)tip);m.Normals.Add(n);
  for(int k=0;k<4;k++){int a1=k,b1=(k+1)%4;m.TriangleIndices.Add(b1);m.TriangleIndices.Add(a1);m.TriangleIndices.Add(4);}
  m.Freeze();
  var mat=new MaterialGroup();
  var brush=new SolidColorBrush(Color.FromRgb(178,52,40));brush.Freeze();
  mat.Children.Add(new DiffuseMaterial(brush));
  var em=new SolidColorBrush(Color.FromArgb(120,255,120,90));em.Freeze();
  mat.Children.Add(new EmissiveMaterial(em));
  return new GeometryModel3D(m,mat);
 }

 public void Tick(double t){
  spin.Angle=t*9%(360);
  tilt.Angle=-12+6*Math.Sin(t*0.21);
 }
}
