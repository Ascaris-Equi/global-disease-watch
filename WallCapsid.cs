using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

// 衣壳：解析 capsid.pdb，按 (MODEL, chainID) 分组得到 Cα 轨迹，再扫成贴球的带状面片。
// 1HXS 的衣壳是 60 个原聚体，文件里的 12 个 MODEL 正好是它的 1/5；绕 5 次轴复制 5 份补全，
// 每条链轮换项目调色板 Heavy/Light/Antigen/Peptide。整体绕 Y 轴慢转，另加很轻的俯仰摆动。
// 球缘：相机沿 5 次轴看，把每个方位的最大柱半径统一收束到同一个 R，消掉原聚体凸起/缺口。
class CapsidScene {
 public readonly Viewport3D View=new Viewport3D();
 readonly AxisAngleRotation3D spin=new AxisAngleRotation3D(new Vector3D(0,1,0),0);
 readonly AxisAngleRotation3D tilt=new AxisAngleRotation3D(new Vector3D(1,0,0),0);
 const double BaseTilt=90,TiltAmp=2;
 int frames;

 public CapsidScene(string path,double size){
  WallView.LiteView(View,false);
  View.Width=size;View.Height=size;
  var lights=new Model3DGroup();
  lights.Children.Add(new AmbientLight(Color.FromRgb(58,64,78)));
  lights.Children.Add(new DirectionalLight(Color.FromRgb(230,234,244),new Vector3D(.45,-.55,-.7)));
  lights.Children.Add(new DirectionalLight(Color.FromRgb(64,78,108),new Vector3D(-.7,.25,-.35)));
  View.Children.Add(new ModelVisual3D{Content=lights});
  var root=new Model3DGroup();
  var tf=new Transform3DGroup();
  tf.Children.Add(new RotateTransform3D(spin));
  tf.Children.Add(new RotateTransform3D(tilt));
  root.Transform=tf;
  double radius=170;
  try{radius=Build(path,root);}catch{}
  View.Children.Add(new ModelVisual3D{Content=root});
  // 正交相机对准衣壳球心；取景按最大半径算，自转时不会出框。
  double width=radius*2*1.06;
  View.Camera=new OrthographicCamera(new Point3D(0,0,radius*5),new Vector3D(0,0,-1),new Vector3D(0,1,0),width){NearPlaneDistance=radius*2.2,FarPlaneDistance=radius*9};
 }

 public void Tick(double t){
  // 软件渲染时和 globe 一样三轮一画，省掉 2/3 的重绘。
  frames++;
  if(WallView.Lite("slice")&&frames%3!=0)return;
  spin.Angle=t*9%360;
  tilt.Angle=BaseTilt+TiltAmp*Math.Sin(t*.29);
 }

 class Trace {public char Chain;public List<Point3D> Points=new List<Point3D>();}

 static double Num(string line,int start,int len){return Double.Parse(line.Substring(start,len),CultureInfo.InvariantCulture);}

 static List<Trace> Load(string path){
  var list=new List<Trace>();var byKey=new Dictionary<string,Trace>();int model=0;
  foreach(string line in File.ReadLines(path)){
   if(line.StartsWith("MODEL")){
    string m=line.Length>5?line.Substring(5).Trim():"";
    Int32.TryParse(m,NumberStyles.Integer,CultureInfo.InvariantCulture,out model);
    continue;
   }
   if(!line.StartsWith("ATOM  ")||line.Length<54)continue;
   if(line.Substring(12,4).Trim()!="CA")continue;
   char chain=line[21];
   string key=model.ToString(CultureInfo.InvariantCulture)+":"+chain;
   Trace tr;if(!byKey.TryGetValue(key,out tr)){tr=new Trace{Chain=chain};byKey[key]=tr;list.Add(tr);}
   tr.Points.Add(new Point3D(Num(line,30,8),Num(line,38,8),Num(line,46,8)));
  }
  return list;
 }

 static Vector3D Mean(List<Point3D> pts){
  var s=new Vector3D();foreach(var p in pts)s+=new Vector3D(p.X,p.Y,p.Z);return s/Math.Max(1,pts.Count);
 }

 // 最小二乘球面拟合：12 个原聚体的质心精确落在衣壳球面上，用它定球心。
 static Vector3D FitSphere(List<Vector3D> pts){
  int n=pts.Count;if(n<4)return new Vector3D(Double.NaN,0,0);
  var a=new double[4,5];
  for(int i=0;i<n;i++){
   var p=pts[i];double row0=2*p.X,row1=2*p.Y,row2=2*p.Z;
   a[0,0]+=row0*row0;a[0,1]+=row0*row1;a[0,2]+=row0*row2;a[0,3]+=row0;
   a[1,1]+=row1*row1;a[1,2]+=row1*row2;a[1,3]+=row1;
   a[2,2]+=row2*row2;a[2,3]+=row2;a[3,3]+=1;
   double rhs=p.X*p.X+p.Y*p.Y+p.Z*p.Z;
   a[0,4]+=row0*rhs;a[1,4]+=row1*rhs;a[2,4]+=row2*rhs;a[3,4]+=rhs;
  }
  a[1,0]=a[0,1];a[2,0]=a[0,2];a[3,0]=a[0,3];a[2,1]=a[1,2];a[3,1]=a[1,3];a[3,2]=a[2,3];
  for(int col=0;col<4;col++){
   int piv=col;for(int r=col+1;r<4;r++)if(Math.Abs(a[r,col])>Math.Abs(a[piv,col]))piv=r;
   if(Math.Abs(a[piv,col])<1e-9)return new Vector3D(Double.NaN,0,0);
   if(piv!=col)for(int k=col;k<5;k++){double tmp=a[col,k];a[col,k]=a[piv,k];a[piv,k]=tmp;}
   for(int r=col+1;r<4;r++){
    double f=a[r,col]/a[col,col];if(f==0)continue;
    for(int k=col;k<5;k++)a[r,k]-=f*a[col,k];
   }
  }
  var x=new double[4];
  for(int r=3;r>=0;r--){double s=a[r,4];for(int k=r+1;k<4;k++)s-=a[r,k]*x[k];x[r]=s/a[r,r];}
  return new Vector3D(x[0],x[1],x[2]);
 }

 static Matrix3D AlignToY(Vector3D a){
  var y=new Vector3D(0,1,0);var v=Vector3D.CrossProduct(a,y);double s=v.Length,c=Vector3D.DotProduct(a,y);var m=Matrix3D.Identity;
  if(s<1e-6){if(c<0)m.Rotate(new Quaternion(new Vector3D(1,0,0),180));return m;}
  m.Rotate(new Quaternion(v/s,Math.Atan2(s,c)*180/Math.PI));return m;
 }

 static Color ChainColor(char c){
  switch(c){
   case '2':return Th.Light;
   case '3':return Th.Antigen;
   case '4':return Th.Peptide;
   default:return Th.Heavy;
  }
 }

 Material MakeMaterial(Color c,bool back){
  var g=new MaterialGroup();
  // 背面用暗一档的同色，透过缝隙看到的是壳的内壁而不是另一些亮面条，球体更有体积感。
  Color k=back?Color.FromRgb((byte)(c.R*.66),(byte)(c.G*.66),(byte)(c.B*.66)):c;
  var d=new SolidColorBrush(k);d.Freeze();g.Children.Add(new DiffuseMaterial(d));
  var e=new SolidColorBrush(Color.FromArgb((byte)(back?8:20),k.R,k.G,k.B));e.Freeze();g.Children.Add(new EmissiveMaterial(e));
  if(!back&&!WallView.Lite("nospec")){var sp=new SolidColorBrush(Color.FromArgb(22,255,255,255));sp.Freeze();g.Children.Add(new SpecularMaterial(sp,30));}
  g.Freeze();return g;
 }

 static int AzBin(Point3D p){
  const int bins=720;
  int b=(int)Math.Floor((Math.Atan2(p.Z,p.X)+Math.PI)/(2*Math.PI)*bins);
  if(b<0)b=0;else if(b>=bins)b=bins-1;
  return b;
 }

 // 相机沿 5 次轴看，球缘就是各方位最大柱半径。把每个方位整条剖面按同一比例缩放到
 // 统一的 R（取各方位最大柱半径的均值）：球缘成为正圆，只收外圈，蛋白带纹理还在。
 // 返回 R 与原最大柱半径之比，用于把相机取景拉回原来的视觉大小。
 static double WarpSilhouette(Dictionary<char,MeshGeometry3D> meshes,out double ratio){
  const int bins=720;var maxF=new double[bins];var maxAll=new double[bins];
  foreach(var kv in meshes)foreach(var p in kv.Value.Positions){
   int b=AzBin(p);double rho=Math.Sqrt(p.X*p.X+p.Z*p.Z);
   if(rho>maxAll[b])maxAll[b]=rho;
   if(p.Y>=0&&rho>maxF[b])maxF[b]=rho;
  }
  double oldMax=0,sum=0;int n=0;
  for(int i=0;i<bins;i++){if(maxF[i]<=1)maxF[i]=maxAll[i];double v=maxF[i];if(v>1){if(v>oldMax)oldMax=v;sum+=v;n++;}}
  ratio=1;
  if(n==0||oldMax<=0)return 0;
  double lim=sum/n-.35;
  var scale=new double[bins];
  for(int i=0;i<bins;i++)scale[i]=maxF[i]>1?lim/maxF[i]:1;
  foreach(var kv in meshes){
   var pos=kv.Value.Positions;
   for(int i=0;i<pos.Count;i++){
    var p=pos[i];double k=scale[AzBin(p)];
    double x=p.X*k,z=p.Z*k,rho=Math.Sqrt(x*x+z*z);
    // 背面几何不参与定义球缘，压到里面一档，避免深色背面在轮缘上突出成缺块。
    double cap=p.Y<0?lim-.8:lim;
    if(rho>cap&&rho>1e-6){double q=cap/rho;x*=q;z*=q;}
    pos[i]=new Point3D(x,p.Y,z);
   }
  }
  ratio=lim/oldMax;
  return lim;
 }

 // 返回包络半径（用于相机取景）。
 double Build(string path,Model3DGroup root){
  var traces=Load(path);
  if(traces.Count==0)throw new InvalidDataException("capsid.pdb 没有 CA 原子");
  var all=new List<Point3D>();var cents=new List<Vector3D>();
  foreach(var tr in traces){all.AddRange(tr.Points);cents.Add(Mean(tr.Points));}
  Vector3D c=FitSphere(cents);
  if(Double.IsNaN(c.X)){
   var mn=all[0];var mx=all[0];
   foreach(var p in all){mn=new Point3D(Math.Min(mn.X,p.X),Math.Min(mn.Y,p.Y),Math.Min(mn.Z,p.Z));mx=new Point3D(Math.Max(mx.X,p.X),Math.Max(mx.Y,p.Y),Math.Max(mx.Z,p.Z));}
   c=new Vector3D((mn.X+mx.X)/2,(mn.Y+mx.Y)/2,(mn.Z+mx.Z)/2);
  }
  // 衣壳的 5 次轴是 PDB 的 z 轴；转到 Y 轴后相机沿 5 次轴看，轮廓收成正圆。
  Matrix3D align=AlignToY(new Vector3D(0,0,1));
  var meshes=new Dictionary<char,MeshGeometry3D>();
  foreach(var tr in traces){
   var pts=new List<Point3D>();
   foreach(var p in tr.Points){var q=align.Transform(new Vector3D(p.X-c.X,p.Y-c.Y,p.Z-c.Z));pts.Add(new Point3D(q.X,q.Y,q.Z));}
   // 12 个 MODEL 是 60 个原聚体的 1/5；绕 5 次轴复制 5 份补成整球。
   for(int rep=0;rep<5;rep++){
    var rot=Matrix3D.Identity;if(rep>0)rot.Rotate(new Quaternion(new Vector3D(0,1,0),rep*72));
    var copy=new List<Point3D>(pts.Count);
    foreach(var p in pts){var q=rot.Transform(new Vector3D(p.X,p.Y,p.Z));copy.Add(new Point3D(q.X,q.Y,q.Z));}
    MeshGeometry3D mesh;if(!meshes.TryGetValue(tr.Chain,out mesh)){mesh=new MeshGeometry3D();meshes[tr.Chain]=mesh;}
    AddTrace(mesh,copy,WallView.Lite("mesh")?5:3,WallView.Lite("mesh")?9:6.0);
   }
  }
  double radius=0;
  foreach(var kv in meshes)foreach(var p in kv.Value.Positions)radius=Math.Max(radius,new Vector3D(p.X,p.Y,p.Z).Length);
  double ratio;WarpSilhouette(meshes,out ratio);
  foreach(var kv in meshes){
   var mesh=kv.Value;if(mesh.Positions.Count==0)continue;
   mesh.Freeze();
   var col=ChainColor(kv.Key);
   root.Children.Add(new GeometryModel3D(mesh,MakeMaterial(col,false)){BackMaterial=MakeMaterial(col,true)});
  }
  return Math.Max(radius*ratio,1);
 }

 // 轨迹在明显断链处（>8 Å）拆开，避免面片横穿缺失的 loop。
 static void AddTrace(MeshGeometry3D mesh,List<Point3D> raw,int stride,double width){
  var run=new List<Point3D>();
  for(int i=0;i<raw.Count;i++){
   if(i>0&&(raw[i]-raw[i-1]).Length>8){if(run.Count>1)AddRibbon(mesh,run,stride,width);run.Clear();}
   run.Add(raw[i]);
  }
  if(run.Count>1)AddRibbon(mesh,run,stride,width);
 }

 // 带状面片：宽度方向贴着球面、面法线朝外，所以从外面看是连续的蛋白带。
 static void AddRibbon(MeshGeometry3D mesh,List<Point3D> raw,int stride,double width){
  var pts=new List<Point3D>();
  for(int i=0;i<raw.Count;i+=stride)pts.Add(raw[i]);
  if((raw.Count-1)%stride!=0)pts.Add(raw[raw.Count-1]);
  if(pts.Count<2)return;
  // 轻度平滑，压掉 CA 折线的高频抖动。
  for(int i=1;i<pts.Count-1;i++)pts[i]=new Point3D(.25*pts[i-1].X+.5*pts[i].X+.25*pts[i+1].X,.25*pts[i-1].Y+.5*pts[i].Y+.25*pts[i+1].Y,.25*pts[i-1].Z+.5*pts[i].Z+.25*pts[i+1].Z);
  int n=pts.Count;
  var tan=new Vector3D[n];var side=new Vector3D[n];var nrm=new Vector3D[n];
  for(int i=0;i<n;i++){
   var d=pts[Math.Min(n-1,i+1)]-pts[Math.Max(0,i-1)];if(d.LengthSquared<1e-12)d=new Vector3D(0,0,1);d.Normalize();tan[i]=d;
  }
  for(int i=0;i<n;i++){
   var r=new Vector3D(pts[i].X,pts[i].Y,pts[i].Z);if(r.LengthSquared<1e-6)r=new Vector3D(0,1,0);r.Normalize();nrm[i]=r;
   var w=Vector3D.CrossProduct(tan[i],r);
   if(w.LengthSquared<1e-9)w=Vector3D.CrossProduct(tan[i],new Vector3D(0,0,1));
   if(w.LengthSquared<1e-9)w=new Vector3D(1,0,0);
   w.Normalize();side[i]=w;
  }
  int baseIdx=mesh.Positions.Count;
  for(int i=0;i<n;i++){
   double hw=width*.5;
   mesh.Positions.Add(new Point3D(pts[i].X-side[i].X*hw,pts[i].Y-side[i].Y*hw,pts[i].Z-side[i].Z*hw));
   mesh.Positions.Add(new Point3D(pts[i].X+side[i].X*hw,pts[i].Y+side[i].Y*hw,pts[i].Z+side[i].Z*hw));
   mesh.Normals.Add(nrm[i]);mesh.Normals.Add(nrm[i]);
  }
  for(int i=0;i<n-1;i++){
   int a=baseIdx+i*2,b=a+1,c=a+2,d=a+3;
   // 绕序让正面朝外；BackMaterial 保证转到背面时也可见。
   mesh.TriangleIndices.Add(a);mesh.TriangleIndices.Add(b);mesh.TriangleIndices.Add(c);
   mesh.TriangleIndices.Add(b);mesh.TriangleIndices.Add(d);mesh.TriangleIndices.Add(c);
  }
 }
}
