using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

// Which chains are the receptor (moves during the assembly animation) and how each chain is coloured.
class MolSpec {
 public string Receptor="",Antigen="";
 public char Peptide='\0';
 public Dictionary<char,Color> Colors=new Dictionary<char,Color>();
 public double CycleOffset;
 public double Separation=24;public char BottomAnchor='\0';public bool UpFromAnchor;public string TopAnchors="";
}

// Cartoon (helix ribbon / sheet arrow / coil tube) mesh built from the CA trace of a PDB file,
// plus a looping, purely illustrative separate -> dock -> bound -> separate animation.
class MoleculeScene {
 class Res {public char Chain;public string Key;public Vector3D CA,O;public bool HasCA,HasO;public char SS='C';public bool Iface;}

 public readonly Viewport3D View=new Viewport3D();
 public bool Loaded;
 readonly MolSpec spec;
 readonly AxisAngleRotation3D spin=new AxisAngleRotation3D(new Vector3D(0,1,0),0),twist=new AxisAngleRotation3D(new Vector3D(0,1,0),0);
 readonly TranslateTransform3D receptorShift=new TranslateTransform3D();
 readonly SolidColorBrush glow=new SolidColorBrush(Color.FromArgb(0,255,214,120));
 double SepMax=24;const double Cycle=34;

 const double Coil=.36,HelixW=1.35,SheetW=1.2,Flat=.32,ArrowW=2.0,PeptideR=.95;
 // Ribbon resolution: segments per residue and vertices per cross-section (coarser for software 3D).
 readonly int Sub=WallView.Lite("mesh")?2:4,Ring=WallView.Lite("mesh")?6:8;

 public MoleculeScene(string path,MolSpec s,double width,double height){
  spec=s;SepMax=s.Separation;
  WallView.LiteView(View,true);
  var lights=new Model3DGroup();
  lights.Children.Add(new AmbientLight(Color.FromRgb(46,52,66)));
  lights.Children.Add(new DirectionalLight(Color.FromRgb(222,226,236),new Vector3D(.55,-.5,-.7)));
  lights.Children.Add(new DirectionalLight(Color.FromRgb(58,70,98),new Vector3D(-.7,.35,-.3)));
  View.Children.Add(new ModelVisual3D{Content=lights});
  try{Build(path,width,height);Loaded=true;}catch{}
 }

 static double Num(string line,int start,int len){return Double.Parse(line.Substring(start,len),CultureInfo.InvariantCulture);}

 void Build(string path,double width,double height){
  var byKey=new Dictionary<string,Res>();var order=new List<Res>();var helices=new List<string[]>();var sheets=new List<string[]>();
  foreach(string line in File.ReadLines(path)){
   if(line.StartsWith("ENDMDL"))break;
   if(line.StartsWith("HELIX ")&&line.Length>=38){helices.Add(new[]{line[19]+line.Substring(21,5),line[31]+line.Substring(33,5)});continue;}
   if(line.StartsWith("SHEET ")&&line.Length>=38){sheets.Add(new[]{line[21]+line.Substring(22,5),line[32]+line.Substring(33,5)});continue;}
   if(!line.StartsWith("ATOM  ")||line.Length<54)continue;
   if(line[16]!=' '&&line[16]!='A')continue;
   string atom=line.Substring(12,4).Trim();if(atom!="CA"&&atom!="O")continue;
   char chain=line[21];if(spec.Receptor.IndexOf(chain)<0&&spec.Antigen.IndexOf(chain)<0)continue;
   string key=chain+line.Substring(22,5);Res r;
   if(!byKey.TryGetValue(key,out r)){r=new Res{Chain=chain,Key=key};byKey[key]=r;order.Add(r);}
   var p=new Vector3D(Num(line,30,8),Num(line,38,8),Num(line,46,8));
   if(atom=="CA"){r.CA=p;r.HasCA=true;}else{r.O=p;r.HasO=true;}
  }
  var res=order.Where(r=>r.HasCA).ToList();if(res.Count<10)throw new InvalidDataException("No CA trace");
  var index=new Dictionary<string,int>();for(int i=0;i<res.Count;i++)index[res[i].Key]=i;
  foreach(var range in helices)Mark(res,index,range,'H');
  foreach(var range in sheets)Mark(res,index,range,'E');

  // Binding interface: residues whose CA lies within 8 A of the partner.
  var rec=res.Where(r=>spec.Receptor.IndexOf(r.Chain)>=0).ToList();var ant=res.Where(r=>spec.Antigen.IndexOf(r.Chain)>=0).ToList();
  foreach(var a in rec)foreach(var b in ant){if((a.CA-b.CA).LengthSquared<64){a.Iface=true;b.Iface=true;}}

  // Orientation. With a membrane anchor, the anchor chain's C-terminus points down into the bottom cell;
  // otherwise the antigen -> receptor axis points up. The scene spins about the vertical axis through the antigen.
  Vector3D recC=Mean(rec),antC=Mean(ant);
  Res anchor=spec.BottomAnchor=='\0'?null:res.LastOrDefault(r=>r.Chain==spec.BottomAnchor);
  // Anchored scenes blend "C-terminus down" with "receptor up" so the partner approaches from above-side.
  Vector3D up=recC-antC;up.Normalize();if(spec.UpFromAnchor&&anchor!=null){var a=antC-anchor.CA;a.Normalize();up=a+up*.6;}up.Normalize();
  Matrix3D m=AlignToY(up);
  foreach(var r in res){r.CA=m.Transform(r.CA-antC);if(r.HasO)r.O=m.Transform(r.O-antC);}
  // Spin about the vertical axis through the whole complex so the side-bound partner does not sweep a wide circle.
  var mid=Mean(res);var off=new Vector3D(mid.X,0,mid.Z);foreach(var r in res){r.CA-=off;if(r.HasO)r.O-=off;}
  recC=Mean(rec);var antNow=Mean(ant);
  sepDir=recC-antNow;sepDir.Normalize();if(sepDir.Y<.6){sepDir.Y=.6;sepDir.Normalize();}

  var receptor=new Model3DGroup();var antigen=new Model3DGroup();
  foreach(var seg in Segments(res)){bool isRec=spec.Receptor.IndexOf(seg[0].Chain)>=0;BuildSegment(seg,isRec?receptor:antigen);}

  // Membranes (drawn in 2D by the card) and the linker stalks that tie the proteins to them.
  double antMin=ant.Min(r=>r.CA.Y),recMax=rec.Max(r=>r.CA.Y);
  double allMin=res.Min(r=>r.CA.Y);membraneBottom=anchor!=null?Math.Min(allMin-8,anchor.CA.Y-12):allMin-6;
  if(anchor!=null)antigen.Children.Add(Stalk(anchor.CA,new Vector3D(anchor.CA.X,membraneBottom-4,anchor.CA.Z),ChainColor(anchor.Chain)));
  HasTop=spec.TopAnchors!="";
  if(HasTop){
   var tops=spec.TopAnchors.Select(ch=>res.LastOrDefault(r=>r.Chain==ch)).Where(r=>r!=null).ToList();
   membraneTop=Math.Max(recMax+3,tops.Count>0?tops.Max(r=>r.CA.Y)+10:recMax+6);
   foreach(var r in tops)receptor.Children.Add(Stalk(r.CA,new Vector3D(r.CA.X,membraneTop+4,r.CA.Z),ChainColor(r.Chain)));
  }
  var tw=new Transform3DGroup();tw.Children.Add(new RotateTransform3D(twist,recC.X,recC.Y,recC.Z));tw.Children.Add(receptorShift);receptor.Transform=tw;
  var scene=new Model3DGroup();scene.Children.Add(receptor);scene.Children.Add(antigen);
  var tf=new Transform3DGroup();tf.Children.Add(new RotateTransform3D(spin));tf.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1,0,0),Tilt)));scene.Transform=tf;
  View.Children.Add(new ModelVisual3D{Content=scene});

  // Fixed camera framing the bottom membrane, the fully separated receptor and (if any) the top membrane.
  double rxz=res.Max(r=>Math.Sqrt(r.CA.X*r.CA.X+r.CA.Z*r.CA.Z))+SepMax*Math.Sqrt(sepDir.X*sepDir.X+sepDir.Z*sepDir.Z)+2;
  double lo=membraneBottom-4,hi=(HasTop?membraneTop:recMax)+SepMax*sepDir.Y+4;
  double hfov=24;fpx=width/2/Math.Tan(hfov/2*Math.PI/180);viewW=width;viewH=height;
  // With a top cell the content sits in the lower 70% so the header and cell label stay clear.
  double frac=HasTop?.70:.84,shift=HasTop?.025:.0;
  dist=Math.Max((hi-lo)*fpx/(height*frac),rxz*fpx/(width*.46)+rxz);
  camY=(hi+lo)/2+shift*height*dist/fpx;
  View.Camera=new PerspectiveCamera(new Point3D(0,camY,dist),new Vector3D(0,0,-1),new Vector3D(0,1,0),hfov){NearPlaneDistance=dist*.2,FarPlaneDistance=dist*3};
 }
 const double Tilt=7;
 Vector3D sepDir;double membraneBottom,membraneTop,fpx,dist,camY,viewW,viewH,sep;
 public bool HasTop;
 // Screen-space (viewport pixels) heights of the membrane surfaces, for the 2D cell artwork.
 double ScreenY(double y){double th=Tilt*Math.PI/180;return viewH/2-fpx*(y*Math.Cos(th)-camY)/(dist-y*Math.Sin(th));}
 public double BottomMembraneY{get{return ScreenY(membraneBottom);}}
 public double TopMembraneY{get{return ScreenY(membraneTop+sep*SepMax*sepDir.Y);}}

 GeometryModel3D Stalk(Vector3D a,Vector3D b,Color c){
  var m=new MeshGeometry3D();var axis=b-a;axis.Normalize();var u=Vector3D.CrossProduct(axis,new Vector3D(1,0,0));if(u.LengthSquared<1e-6)u=Vector3D.CrossProduct(axis,new Vector3D(0,0,1));u.Normalize();var v=Vector3D.CrossProduct(axis,u);
  const int n=10;const double rad=.75;
  for(int i=0;i<=n;i++){double th=2*Math.PI*i/n;var nn=u*Math.Cos(th)+v*Math.Sin(th);foreach(var p in new[]{a,b}){m.Positions.Add((Point3D)(p+nn*rad));m.Normals.Add(nn);}}
  for(int i=0;i<n;i++){int k=2*i;m.TriangleIndices.Add(k);m.TriangleIndices.Add(k+2);m.TriangleIndices.Add(k+1);m.TriangleIndices.Add(k+1);m.TriangleIndices.Add(k+2);m.TriangleIndices.Add(k+3);}
  m.Freeze();var g=new MaterialGroup();var br=new SolidColorBrush(Color.FromRgb((byte)(c.R*.8),(byte)(c.G*.8),(byte)(c.B*.8)));br.Freeze();g.Children.Add(new DiffuseMaterial(br));
  return new GeometryModel3D(m,g){BackMaterial=g};
 }
 static void Mark(List<Res> res,Dictionary<string,int> index,string[] range,char ss){
  int a,b;if(!index.TryGetValue(range[0],out a)||!index.TryGetValue(range[1],out b)||b<a||b-a>200)return;
  for(int i=a;i<=b;i++)if(res[i].Chain==res[a].Chain)res[i].SS=ss;
 }
 static Vector3D Mean(List<Res> list){var s=new Vector3D();foreach(var r in list)s+=r.CA;return s/Math.Max(1,list.Count);}
 static Matrix3D AlignToY(Vector3D a){
  var y=new Vector3D(0,1,0);var v=Vector3D.CrossProduct(a,y);double s=v.Length,c=Vector3D.DotProduct(a,y);var m=Matrix3D.Identity;
  if(s<1e-6){if(c<0)m.Rotate(new Quaternion(new Vector3D(1,0,0),180));return m;}
  m.Rotate(new Quaternion(v/s,Math.Atan2(s,c)*180/Math.PI));return m;
 }
 static IEnumerable<List<Res>> Segments(List<Res> res){
  var cur=new List<Res>();
  foreach(var r in res){if(cur.Count>0&&(cur[cur.Count-1].Chain!=r.Chain||(cur[cur.Count-1].CA-r.CA).Length>4.3)){if(cur.Count>1)yield return cur;cur=new List<Res>();}cur.Add(r);}
  if(cur.Count>1)yield return cur;
 }

 Color ChainColor(char c){Color k;return spec.Colors.TryGetValue(c,out k)?k:Color.FromRgb(150,160,180);}

 Material MakeMaterial(char chain,bool iface){
  var g=new MaterialGroup();var col=ChainColor(chain);
  var d=new SolidColorBrush(col);d.Freeze();g.Children.Add(new DiffuseMaterial(d));
  if(chain==spec.Peptide){var e=new SolidColorBrush(Color.FromArgb(70,col.R,col.G,col.B));e.Freeze();g.Children.Add(new EmissiveMaterial(e));}
  else if(iface)g.Children.Add(new EmissiveMaterial(glow));
  if(!WallView.Lite("nospec")){var sp=new SolidColorBrush(Color.FromArgb(90,255,255,255));sp.Freeze();g.Children.Add(new SpecularMaterial(sp,38));}
  return g;
 }

 void Dims(Res r,out double a,out double b){
  if(r.Chain==spec.Peptide){a=b=PeptideR;return;}
  if(r.SS=='H'){a=HelixW;b=Flat;return;}
  if(r.SS=='E'){a=SheetW;b=Flat;return;}
  a=b=Coil;
 }
 static double Smooth(double u){return u*u*(3-2*u);}
 static Vector3D Catmull(Vector3D p0,Vector3D p1,Vector3D p2,Vector3D p3,double u){
  double u2=u*u,u3=u2*u;return .5*((2*p1)+(-p0+p2)*u+(2*p0-5*p1+4*p2-p3)*u2+(-p0+3*p1-3*p2+p3)*u3);
 }

 void BuildSegment(List<Res> seg,Model3DGroup target){
  int n=seg.Count;var P=new Vector3D[n];var S=new Vector3D[n];
  for(int i=0;i<n;i++){
   P[i]=seg[i].CA;
   if(seg[i].SS=='E'&&i>0&&i<n-1&&seg[i-1].SS=='E'&&seg[i+1].SS=='E')P[i]=.25*seg[i-1].CA+.5*seg[i].CA+.25*seg[i+1].CA;
   Vector3D side=seg[i].HasO?seg[i].O-seg[i].CA:(i>0?S[i-1]:new Vector3D(0,1,0));
   if(side.LengthSquared<1e-6)side=i>0?S[i-1]:new Vector3D(0,1,0);side.Normalize();
   if(i>0&&Vector3D.DotProduct(side,S[i-1])<0)side=-side;S[i]=side;
  }
  var X=new List<Vector3D>();var SD=new List<Vector3D>();var A=new List<double>();var B=new List<double>();var owner=new List<int>();
  for(int i=0;i<n-1;i++){
   double a0,b0,a1,b1;Dims(seg[i],out a0,out b0);Dims(seg[i+1],out a1,out b1);
   bool arrow=seg[i].SS=='E'&&seg[i+1].SS!='E'&&seg[i].Chain!=spec.Peptide;
   int last=i==n-2?Sub:Sub-1;
   for(int k=0;k<=last;k++){
    double u=k/(double)Sub;
    X.Add(Catmull(P[Math.Max(0,i-1)],P[i],P[i+1],P[Math.Min(n-1,i+2)],u));
    SD.Add(S[i]*(1-u)+S[i+1]*u);
    if(arrow){A.Add(ArrowW*(1-u)+a1*u);B.Add(Flat*(1-u)+b1*u);}
    else{double f=Smooth(u);A.Add(a0+(a1-a0)*f);B.Add(b0+(b1-b0)*f);}
    owner.Add(u<.5?i:i+1);
   }
  }
  int m=X.Count;var W=new Vector3D[m];var N=new Vector3D[m];var T=new Vector3D[m];
  for(int j=0;j<m;j++){
   var t=X[Math.Min(m-1,j+1)]-X[Math.Max(0,j-1)];if(t.LengthSquared<1e-9)t=j>0?T[j-1]:new Vector3D(0,0,1);t.Normalize();T[j]=t;
   var w=SD[j]-Vector3D.DotProduct(SD[j],t)*t;if(w.LengthSquared<1e-9)w=j>0?W[j-1]:Vector3D.CrossProduct(t,new Vector3D(.3,.9,.1));w.Normalize();
   if(j>0&&Vector3D.DotProduct(w,W[j-1])<0)w=-w;W[j]=w;N[j]=Vector3D.CrossProduct(t,w);
  }
  // Split into runs that share a material, duplicating the boundary ring so runs join seamlessly.
  MeshGeometry3D mesh=null;int ringCount=0;string runKey=null;Material runMat=null;
  Action flush=()=>{if(mesh!=null&&ringCount>1){mesh.Freeze();target.Children.Add(new GeometryModel3D(mesh,runMat));}};
  for(int j=0;j<m;j++){
   var r=seg[owner[j]];bool hot=r.Iface&&r.Chain!=spec.Peptide;string key=r.Chain+(hot?"*":"");
   if(key!=runKey){
    if(mesh!=null){AddRing(mesh,X[j],W[j],N[j],A[j],B[j]);ringCount++;Stitch(mesh,ringCount);flush();}
    mesh=new MeshGeometry3D();ringCount=0;runKey=key;runMat=MakeMaterial(r.Chain,hot);
    if(j==0)Cap(mesh,X[0],-T[0],W[0],N[0],A[0],B[0],true);
   }
   AddRing(mesh,X[j],W[j],N[j],A[j],B[j]);ringCount++;Stitch(mesh,ringCount);
  }
  Cap(mesh,X[m-1],T[m-1],W[m-1],N[m-1],A[m-1],B[m-1],false);
  flush();
 }

 // Rings occupy Ring consecutive positions; caps are appended after and never stitched.
 readonly Dictionary<MeshGeometry3D,List<int>> ringStarts=new Dictionary<MeshGeometry3D,List<int>>();
 void AddRing(MeshGeometry3D mesh,Vector3D c,Vector3D w,Vector3D nrm,double a,double b){
  List<int> starts;if(!ringStarts.TryGetValue(mesh,out starts)){starts=new List<int>();ringStarts[mesh]=starts;}
  starts.Add(mesh.Positions.Count);
  for(int v=0;v<Ring;v++){
   double th=2*Math.PI*v/Ring,cs=Math.Cos(th),sn=Math.Sin(th);
   var p=c+w*(a*cs)+nrm*(b*sn);var nn=w*(cs/a)+nrm*(sn/b);nn.Normalize();
   mesh.Positions.Add((Point3D)p);mesh.Normals.Add(nn);
  }
 }
 void Stitch(MeshGeometry3D mesh,int ringCount){
  if(ringCount<2)return;var starts=ringStarts[mesh];int r0=starts[starts.Count-2],r1=starts[starts.Count-1];
  for(int v=0;v<Ring;v++){
   int v1=(v+1)%Ring,a=r0+v,b=r0+v1,c=r1+v1,d=r1+v;
   mesh.TriangleIndices.Add(a);mesh.TriangleIndices.Add(b);mesh.TriangleIndices.Add(c);
   mesh.TriangleIndices.Add(a);mesh.TriangleIndices.Add(c);mesh.TriangleIndices.Add(d);
  }
 }
 void Cap(MeshGeometry3D mesh,Vector3D c,Vector3D outward,Vector3D w,Vector3D nrm,double a,double b,bool start){
  int center=mesh.Positions.Count;mesh.Positions.Add((Point3D)c);mesh.Normals.Add(outward);int first=mesh.Positions.Count;
  for(int v=0;v<Ring;v++){double th=2*Math.PI*v/Ring;mesh.Positions.Add((Point3D)(c+w*(a*Math.Cos(th))+nrm*(b*Math.Sin(th))));mesh.Normals.Add(outward);}
  for(int v=0;v<Ring;v++){int v1=(v+1)%Ring;mesh.TriangleIndices.Add(center);if(start){mesh.TriangleIndices.Add(first+v1);mesh.TriangleIndices.Add(first+v);}else{mesh.TriangleIndices.Add(first+v);mesh.TriangleIndices.Add(first+v1);}}
 }

 static double Ease(double u){u=Math.Max(0,Math.Min(1,u));return u<.5?4*u*u*u:1-Math.Pow(-2*u+2,3)/2;}

 // Phase: 0 apart, 1 docking, 2 bound, 3 separating.
 public int Tick(double t){
  if(!Loaded)return 2;
  spin.Angle=(t*6.5+spec.CycleOffset*40)%360;
  double c=(t+spec.CycleOffset)%Cycle,sep,glowA;int phase;
  if(c<5){sep=1;glowA=0;phase=0;}
  else if(c<11){sep=1-Ease((c-5)/6);glowA=0;phase=1;}
  else if(c<29){sep=0;double since=c-11;glowA=since<1.2?since/1.2:.55+.45*Math.Exp(-(since-1.2)*1.4)+.08*Math.Sin(since*1.7);phase=2;}
  else{sep=Ease((c-29)/5);glowA=Math.Max(0,1-(c-29)/1.2)*.55;phase=3;}
  this.sep=sep;var d=sepDir*(sep*SepMax);receptorShift.OffsetX=d.X;receptorShift.OffsetY=d.Y;receptorShift.OffsetZ=d.Z;twist.Angle=sep*32;
  glow.Color=Color.FromArgb((byte)(Math.Max(0,Math.Min(1,glowA))*190),255,214,120);
  return phase;
 }
}
