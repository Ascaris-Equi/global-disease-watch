using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

// Small recognisable hardware models for a non-specialist audience. Every data-driven part samples one
// pixel of a shared 64x64 palette bitmap, so recolouring hundreds of parts is a single texture write.
class Rig {
 public readonly Viewport3D View=new Viewport3D();
 readonly Model3DGroup root=new Model3DGroup();
 readonly AxisAngleRotation3D sway=new AxisAngleRotation3D(new Vector3D(0,1,0),0);
 const int P=64;
 readonly WriteableBitmap palette=new WriteableBitmap(P,P,96,96,PixelFormats.Bgra32,null);
 readonly int[] pixels=new int[P*P];int slots;bool dirty;
 readonly Material cellMaterial,glowMaterial;
 readonly Dictionary<Color,Material> solids=new Dictionary<Color,Material>();
 Rect3D bounds=Rect3D.Empty;
 readonly double swayAmp,swayPhase;

 public static readonly Color Board=Color.FromRgb(17,24,32),Pcb=Color.FromRgb(18,44,36),Substrate=Color.FromRgb(26,70,52),Silver=Color.FromRgb(150,158,170),Gunmetal=Color.FromRgb(40,45,54),Plate=Color.FromRgb(52,58,68),Black=Color.FromRgb(12,14,18),Gold=Color.FromRgb(190,156,80),Part=Color.FromRgb(34,40,50),Blade=Color.FromRgb(86,94,108);

 public Rig(double amp,double phase){
  swayAmp=amp;swayPhase=phase;
  Func<double,ImageBrush> brush=o=>{var b=new ImageBrush(palette){ViewportUnits=BrushMappingMode.Absolute,Viewport=new Rect(0,0,1,1),Opacity=o};RenderOptions.SetBitmapScalingMode(b,BitmapScalingMode.NearestNeighbor);return b;};
  var g=new MaterialGroup();g.Children.Add(new DiffuseMaterial(brush(1)));g.Children.Add(new EmissiveMaterial(brush(.55)));cellMaterial=g;
  var h=new MaterialGroup();h.Children.Add(new DiffuseMaterial(brush(1)));h.Children.Add(new EmissiveMaterial(brush(1)));glowMaterial=h;
  var lights=new Model3DGroup();
  lights.Children.Add(new AmbientLight(Color.FromRgb(78,86,100)));
  lights.Children.Add(new DirectionalLight(Color.FromRgb(210,214,224),new Vector3D(.35,-1,-.5)));
  lights.Children.Add(new DirectionalLight(Color.FromRgb(70,82,108),new Vector3D(-.8,-.2,.3)));
  root.Transform=new RotateTransform3D(sway);
  View.Children.Add(new ModelVisual3D{Content=lights});View.Children.Add(new ModelVisual3D{Content=root});
  WallView.LiteView(View,false);
 }

 public int Slot(){return slots++;}
 public void Set(int slot,Color c){int v=(255<<24)|(c.R<<16)|(c.G<<8)|c.B;if(pixels[slot]!=v){pixels[slot]=v;dirty=true;}}
 public void Flush(){if(!dirty)return;palette.WritePixels(new Int32Rect(0,0,P,P),pixels,P*4,0);dirty=false;}
 static Point Uv(int slot){return new Point((slot%P+.5)/P,(slot/P+.5)/P);}
 void Grow(double x0,double y0,double z0,double x1,double y1,double z1){bounds.Union(new Rect3D(x0,y0,z0,x1-x0,y1-y0,z1-z0));}

 public Material Solid(Color c,double gloss=50){
  Material m;if(solids.TryGetValue(c,out m))return m;
  var b=new SolidColorBrush(c);b.Freeze();var sp=new SolidColorBrush(Color.FromArgb((byte)gloss,255,255,255));sp.Freeze();
  var g=new MaterialGroup();g.Children.Add(new DiffuseMaterial(b));if(!WallView.Lite("nospec"))g.Children.Add(new SpecularMaterial(sp,28));g.Freeze();solids[c]=g;return g;
 }
 public MeshGeometry3D Cells(){return new MeshGeometry3D();}
 public Model3DGroup Group(Transform3D t){var g=new Model3DGroup{Transform=t};root.Children.Add(g);return g;}
 void Add(Model3D m,Model3DGroup into){(into??root).Children.Add(m);}

 public void Box(double x0,double y0,double z0,double x1,double y1,double z1,Color c,Model3DGroup into=null){
  var m=new MeshGeometry3D();AddBox(m,x0,y0,z0,x1,y1,z1,new Point(0,0));m.Freeze();Add(new GeometryModel3D(m,Solid(c)),into);Grow(x0,y0,z0,x1,y1,z1);
 }
 // Static data parts share one mesh per material; each part's faces point at its palette pixel.
 public void Cell(MeshGeometry3D m,double x0,double y0,double z0,double x1,double y1,double z1,int slot){AddBox(m,x0,y0,z0,x1,y1,z1,Uv(slot));Grow(x0,y0,z0,x1,y1,z1);}
 public void Commit(MeshGeometry3D m,bool glow=false,Model3DGroup into=null){if(m.Positions.Count==0)return;m.Freeze();Add(new GeometryModel3D(m,glow?glowMaterial:cellMaterial),into);}
 // A data part that grows upward: returns its Y scale about the base.
 public ScaleTransform3D Pillar(double x0,double y0,double z0,double x1,double y1,double z1,int slot){
  var m=new MeshGeometry3D();AddBox(m,x0,y0,z0,x1,y1,z1,Uv(slot));m.Freeze();
  var s=new ScaleTransform3D(1,1,1,(x0+x1)/2,y0,(z0+z1)/2);root.Children.Add(new GeometryModel3D(m,cellMaterial){Transform=s});Grow(x0,y0,z0,x1,y1,z1);return s;
 }
 // Horizontal annulus facing +Y. Solid colour, palette slot, or a live brush.
 public void Disc(double cx,double y,double cz,double r0,double r1,Color c,Model3DGroup into=null){Add(new GeometryModel3D(DiscMesh(cx,y,cz,r0,r1,new Point(0,0)),Solid(c,20)),into);Grow(cx-r1,y,cz-r1,cx+r1,y,cz+r1);}
 public void DiscCell(double cx,double y,double cz,double r0,double r1,int slot,bool glow,Model3DGroup into=null){Add(new GeometryModel3D(DiscMesh(cx,y,cz,r0,r1,Uv(slot)),glow?glowMaterial:cellMaterial),into);}
 public void DiscBrush(double cx,double y,double cz,double r0,double r1,Brush b,Model3DGroup into=null){Add(new GeometryModel3D(DiscMesh(cx,y,cz,r0,r1,new Point(0,0)),new DiffuseMaterial(b)),into);}
 static MeshGeometry3D DiscMesh(double cx,double y,double cz,double r0,double r1,Point uv){
  var m=new MeshGeometry3D();const int seg=48;
  for(int k=0;k<=seg;k++){double a=2*Math.PI*k/seg;foreach(double r in new[]{r0,r1}){m.Positions.Add(new Point3D(cx+r*Math.Cos(a),y,cz+r*Math.Sin(a)));m.Normals.Add(new Vector3D(0,1,0));m.TextureCoordinates.Add(uv);}}
  for(int k=0;k<seg;k++){int i=2*k;m.TriangleIndices.Add(i);m.TriangleIndices.Add(i+3);m.TriangleIndices.Add(i+1);m.TriangleIndices.Add(i);m.TriangleIndices.Add(i+2);m.TriangleIndices.Add(i+3);}
  m.Freeze();return m;
 }
 // Flat swept fan blade between two radii, visible from both sides.
 public void BladeShape(double cx,double y,double cz,double r0,double r1,double a0,Color c,Model3DGroup into){
  Func<double,double,Point3D> p=(r,a)=>new Point3D(cx+r*Math.Cos(a),y+(r-r0)*.03,cz+r*Math.Sin(a));
  var m=new MeshGeometry3D();var pts=new[]{p(r0,a0),p(r0,a0+.34),p(r1,a0+.92),p(r1,a0+.42)};
  foreach(var q in pts){m.Positions.Add(q);m.Normals.Add(new Vector3D(0,1,0));}
  foreach(int k in new[]{0,2,1,0,3,2})m.TriangleIndices.Add(k);
  m.Freeze();var mat=Solid(c,60);Add(new GeometryModel3D(m,mat){BackMaterial=mat},into);
 }

 // Textures are painted once with WPF drawing and mapped onto quads.
 public static ImageBrush Paint(int w,int h,Action<DrawingContext> draw,bool transparent=false){
  var dv=new DrawingVisual();using(var dc=dv.RenderOpen())draw(dc);
  var bmp=new RenderTargetBitmap(w,h,96,96,PixelFormats.Pbgra32);bmp.Render(dv);bmp.Freeze();
  var b=new ImageBrush(bmp){ViewportUnits=BrushMappingMode.Absolute,Viewport=new Rect(0,0,1,1)};b.Freeze();return b;
 }
 public Material Textured(Brush brush,double gloss){
  var g=new MaterialGroup();g.Children.Add(new DiffuseMaterial(brush));
  if(!WallView.Lite("nospec")){var sp=new SolidColorBrush(Color.FromArgb((byte)gloss,255,255,255));sp.Freeze();g.Children.Add(new SpecularMaterial(sp,30));}return g;
 }
 // Quad with texture coordinates: bottom-left, bottom-right, top-right, top-left of the image.
 public void Face(Point3D bl,Point3D br,Point3D tr,Point3D tl,Vector3D n,Material m,Model3DGroup into=null){
  var mesh=new MeshGeometry3D();var uv=new[]{new Point(0,1),new Point(1,1),new Point(1,0),new Point(0,0)};var pts=new[]{bl,br,tr,tl};
  for(int i=0;i<4;i++){mesh.Positions.Add(pts[i]);mesh.Normals.Add(n);mesh.TextureCoordinates.Add(uv[i]);}
  bool ccw=Vector3D.DotProduct(Vector3D.CrossProduct(br-bl,tr-bl),n)>0;
  foreach(int k in ccw?new[]{0,1,2,0,2,3}:new[]{0,2,1,0,3,2})mesh.TriangleIndices.Add(k);
  mesh.Freeze();Add(new GeometryModel3D(mesh,m),into);
  foreach(var p in pts)Grow(p.X,p.Y,p.Z,p.X,p.Y,p.Z);
 }
 // Fan blade that sweeps forward and narrows toward the tip, slightly pitched; visible from both sides.
 public void CurvedBlade(double cx,double y,double cz,double r0,double r1,double a0,Color c,Model3DGroup into){
  var m=new MeshGeometry3D();const int n=6;
  for(int k=0;k<=n;k++){
   double f=(double)k/n,r=r0+(r1-r0)*f,lead=a0+.6*f,trail=lead+.44-.16*f;
   m.Positions.Add(new Point3D(cx+r*Math.Cos(lead),y,cz+r*Math.Sin(lead)));m.Positions.Add(new Point3D(cx+r*Math.Cos(trail),y+.03,cz+r*Math.Sin(trail)));
   m.Normals.Add(new Vector3D(0,1,0));m.Normals.Add(new Vector3D(0,1,0));
  }
  for(int k=0;k<n;k++){int i=2*k;foreach(int q in new[]{i,i+3,i+1,i,i+2,i+3})m.TriangleIndices.Add(q);}
  m.Freeze();var mat=Solid(c,70);Add(new GeometryModel3D(m,mat){BackMaterial=mat},into);
 }
 static void AddBox(MeshGeometry3D m,double x0,double y0,double z0,double x1,double y1,double z1,Point uv){
  Quad(m,new Point3D(x0,y1,z0),new Point3D(x1,y1,z0),new Point3D(x1,y1,z1),new Point3D(x0,y1,z1),new Vector3D(0,1,0),uv);
  Quad(m,new Point3D(x0,y0,z1),new Point3D(x1,y0,z1),new Point3D(x1,y1,z1),new Point3D(x0,y1,z1),new Vector3D(0,0,1),uv);
  Quad(m,new Point3D(x0,y0,z0),new Point3D(x1,y0,z0),new Point3D(x1,y1,z0),new Point3D(x0,y1,z0),new Vector3D(0,0,-1),uv);
  Quad(m,new Point3D(x1,y0,z0),new Point3D(x1,y0,z1),new Point3D(x1,y1,z1),new Point3D(x1,y1,z0),new Vector3D(1,0,0),uv);
  Quad(m,new Point3D(x0,y0,z0),new Point3D(x0,y0,z1),new Point3D(x0,y1,z1),new Point3D(x0,y1,z0),new Vector3D(-1,0,0),uv);
 }
 // Winding is chosen from the intended normal so every face is front-facing from outside.
 static void Quad(MeshGeometry3D m,Point3D a,Point3D b,Point3D c,Point3D d,Vector3D n,Point uv){
  int i=m.Positions.Count;foreach(var p in new[]{a,b,c,d}){m.Positions.Add(p);m.Normals.Add(n);m.TextureCoordinates.Add(uv);}
  bool ccw=Vector3D.DotProduct(Vector3D.CrossProduct(b-a,c-a),n)>0;
  if(ccw){foreach(int k in new[]{0,1,2,0,2,3})m.TriangleIndices.Add(i+k);}else{foreach(int k in new[]{0,2,1,0,3,2})m.TriangleIndices.Add(i+k);}
 }

 // Orthographic camera from the given azimuth/elevation, sized so the model (including sway) fills the viewport.
 public void Fit(double width,double height,double azimuth,double elevation,double margin){
  double az=azimuth*Math.PI/180,el=elevation*Math.PI/180;
  var dir=new Vector3D(-Math.Cos(el)*Math.Sin(az),-Math.Sin(el),-Math.Cos(el)*Math.Cos(az));
  var right=Vector3D.CrossProduct(dir,new Vector3D(0,1,0));right.Normalize();var up=Vector3D.CrossProduct(right,dir);up.Normalize();
  double umin=1e9,umax=-1e9,vmin=1e9,vmax=-1e9;
  foreach(double sw in new[]{-swayAmp,0,swayAmp}){
   double a=sw*Math.PI/180;
   for(int i=0;i<8;i++){
    double x=(i&1)==0?bounds.X:bounds.X+bounds.SizeX,y=(i&2)==0?bounds.Y:bounds.Y+bounds.SizeY,z=(i&4)==0?bounds.Z:bounds.Z+bounds.SizeZ;
    var p=new Vector3D(x*Math.Cos(a)+z*Math.Sin(a),y,-x*Math.Sin(a)+z*Math.Cos(a));
    double u=Vector3D.DotProduct(p,right),v=Vector3D.DotProduct(p,up);umin=Math.Min(umin,u);umax=Math.Max(umax,u);vmin=Math.Min(vmin,v);vmax=Math.Max(vmax,v);
   }
  }
  double w=Math.Max(umax-umin,(vmax-vmin)*width/height)*margin;
  var center=right*((umin+umax)/2)+up*((vmin+vmax)/2);
  View.Camera=new OrthographicCamera((Point3D)(center-dir*80),dir,up,w){NearPlaneDistance=1,FarPlaneDistance=300};
 }

 public void Tick(double t){if(!WallView.Off("sway"))sway.Angle=swayAmp*Math.Sin(t*.16+swayPhase);Flush();}

 // ---------- palettes ----------
 public static readonly double[] HeatAt={0,.12,.38,.62,.82,1};
 public static readonly Color[] HeatCol={Color.FromRgb(36,64,112),Color.FromRgb(40,110,190),Color.FromRgb(44,170,226),Color.FromRgb(70,220,170),Color.FromRgb(250,198,80),Color.FromRgb(255,84,70)};
 public static Color Heat(double v){
  v=Math.Max(0,Math.Min(1,v));int i=1;while(i<HeatAt.Length-1&&v>HeatAt[i])i++;
  double f=(v-HeatAt[i-1])/(HeatAt[i]-HeatAt[i-1]);return Mix(HeatCol[i-1],HeatCol[i],f);
 }
 public static Color Mix(Color a,Color b,double f){f=Math.Max(0,Math.Min(1,f));return Color.FromRgb((byte)(a.R+(b.R-a.R)*f),(byte)(a.G+(b.G-a.G)*f),(byte)(a.B+(b.B-a.B)*f));}
 public static Color Scale(Color a,double f){return Color.FromRgb((byte)Math.Min(255,a.R*f),(byte)Math.Min(255,a.G*f),(byte)Math.Min(255,a.B*f));}
 public static readonly Color Off=Color.FromRgb(28,36,50),Memory=Color.FromRgb(160,140,255),Used=Color.FromRgb(96,176,255),Led=Color.FromRgb(90,240,150);
}

enum GpuStyle {Twin,Blower,Passive}

// A graphics card in a product-shot pose, textured to read as the real thing: brushed shroud, fin stacks on the
// sides, curved fan blades (or a blower / passive grille), and a side logo that glows with the load colour.
class GpuRig {
 public readonly Rig Rig=new Rig(4,0);
 public readonly int Glow;public readonly List<AxisAngleRotation3D> Fans=new List<AxisAngleRotation3D>();public readonly List<SolidColorBrush> Blur=new List<SolidColorBrush>();
 public readonly int[] Vram;public readonly EmissiveMaterial Logo;public readonly double FanSpeedScale=1;

 public GpuRig(GpuStyle style,string logo,double w,double h){
  var r=Rig;Glow=r.Slot();
  double L=style==GpuStyle.Blower?4.7:5.1,D=2.15,top=style==GpuStyle.Blower?1.05:1.38;
  Color body=style==GpuStyle.Passive?Color.FromRgb(96,104,116):style==GpuStyle.Blower?Color.FromRgb(20,22,26):Color.FromRgb(44,48,56);
  r.Box(-L,-.07,-D,L,0,D,Color.FromRgb(30,34,40));
  r.Box(-L,0,-D,L,.1,D,Rig.Pcb);
  r.Box(-L,.1,-D,L,top,D,body);
  // Textured faces sit a hair outside the shroud box.
  r.Face(new Point3D(-L+.05,top+.002,D-.05),new Point3D(L-.05,top+.002,D-.05),new Point3D(L-.05,top+.002,-D+.05),new Point3D(-L+.05,top+.002,-D+.05),new Vector3D(0,1,0),r.Textured(TopTexture(style),40));
  var side=r.Textured(SideTexture(style),30);
  r.Face(new Point3D(-L+.08,.12,D+.003),new Point3D(L-.08,.12,D+.003),new Point3D(L-.08,top-.03,D+.003),new Point3D(-L+.08,top-.03,D+.003),new Vector3D(0,0,1),side);
  r.Face(new Point3D(L-.08,.12,-D-.003),new Point3D(-L+.08,.12,-D-.003),new Point3D(-L+.08,top-.03,-D-.003),new Point3D(L-.08,top-.03,-D-.003),new Vector3D(0,0,-1),side);
  // Side logo, lit in the load colour.
  Logo=new EmissiveMaterial(LogoTexture(logo)){Color=Color.FromRgb(60,120,200)};
  double lw=Math.Min(3.8,.36*logo.Length),ly=top*.55;
  r.Face(new Point3D(-lw/2,ly-.17,D+.006),new Point3D(lw/2,ly-.17,D+.006),new Point3D(lw/2,ly+.17,D+.006),new Point3D(-lw/2,ly+.17,D+.006),new Vector3D(0,0,1),Logo);
  // I/O bracket with ports, PCIe edge connector, power connector.
  r.Box(-L-.22,-.55,-D-.1,-L-.05,top+.1,D+.1,Rig.Silver);
  for(int i=0;i<3;i++)r.Box(-L-.26,.18+i*(top-.2)/3,-1.3,-L-.2,.18+i*(top-.2)/3+.22,-.25,Rig.Black);
  for(int i=0;i<2;i++)r.Box(-L-.26,.25+i*(top-.25)/2,.25,-L-.2,.25+i*(top-.25)/2+.26,1.35,Rig.Black);
  r.Box(-3.3,-.2,D-.35,1.4,.02,D-.1,Rig.Pcb);
  for(double x=-3.2;x<1.3;x+=.2)r.Box(x,-.21,D-.1,x+.12,.0,D-.08,Rig.Gold);
  r.Box(2.9,top,-D+.2,3.9,top+.16,-D+.55,Rig.Black);
  // Load-coloured light lines along the front edge, plus fans / blower / grille.
  var glow=r.Cells();
  r.Cell(glow,-L+.25,top+.002,D-.2,L-.25,top+.03,D-.1,Glow);
  if(style==GpuStyle.Twin){foreach(double cx in new[]{-2.42,2.28})TwinFan(cx,0,1.6,top);}
  else if(style==GpuStyle.Blower){BlowerFan(L-1.35,0,1.0,top);FanSpeedScale=1.6;}
  else{r.Cell(glow,-L+.25,top+.002,-D+.1,L-.25,top+.03,-D+.2,Glow);}
  r.Commit(glow,true);
  // VRAM fill gauge along the back edge of the top plate.
  Vram=new int[12];var cells=r.Cells();
  for(int i=0;i<12;i++){Vram[i]=r.Slot();double x=-L+.8+i*((2*L-1.6)/12);r.Cell(cells,x,top+.002,-D+.32,x+(2*L-1.6)/12-.1,top+.025,-D+.44,Vram[i]);}
  r.Commit(cells);
  r.Fit(w,h,-22,34,1.03);
 }

 // Dual axial fan: dark well, curved blades on a spinning hub, speed blur, frame ring, load-coloured rim.
 void TwinFan(double cx,double cz,double radius,double top){
  var r=Rig;
  r.Disc(cx,top+.004,cz,0,radius,Color.FromRgb(8,9,12));
  var spin=new AxisAngleRotation3D(new Vector3D(0,1,0),0);Fans.Add(spin);
  var g=r.Group(new RotateTransform3D(spin,cx,0,cz));
  for(int i=0;i<11;i++)r.CurvedBlade(cx,top+.012,cz,.46,radius-.04,i*2*Math.PI/11,Color.FromRgb(70,76,88),g);
  var blur=new SolidColorBrush(Color.FromArgb(0,80,88,102));Blur.Add(blur);
  r.DiscBrush(cx,top+.05,cz,.46,radius-.03,blur);
  r.Disc(cx,top+.058,cz,0,.46,Color.FromRgb(28,31,38));
  r.Disc(cx,top+.06,cz,.16,.2,Color.FromRgb(70,76,88));
  r.DiscCell(cx,top+.062,cz,.32,.4,Glow,true);
  r.Disc(cx,top+.064,cz,radius-.02,radius+.08,Color.FromRgb(86,92,104));
  r.DiscCell(cx,top+.068,cz,radius+.08,radius+.15,Glow,true);
 }

 // Blower (pro cards): circular intake over a ring of short radial blades.
 void BlowerFan(double cx,double cz,double radius,double top){
  var r=Rig;
  r.Disc(cx,top+.004,cz,0,radius,Color.FromRgb(6,7,9));
  var spin=new AxisAngleRotation3D(new Vector3D(0,1,0),0);Fans.Add(spin);
  var g=r.Group(new RotateTransform3D(spin,cx,0,cz));
  for(int i=0;i<30;i++){double a=i*2*Math.PI/30;r.BladeShape(cx,top+.01,cz,radius*.62,radius-.04,a,Color.FromRgb(64,70,80),g);}
  var blur=new SolidColorBrush(Color.FromArgb(0,70,76,88));Blur.Add(blur);
  r.DiscBrush(cx,top+.04,cz,radius*.6,radius-.03,blur);
  r.Disc(cx,top+.05,cz,0,radius*.6,Color.FromRgb(26,28,34));
  r.DiscCell(cx,top+.055,cz,radius*.18,radius*.26,Glow,true);
  r.Disc(cx,top+.06,cz,radius-.02,radius+.07,Color.FromRgb(60,64,72));
  r.DiscCell(cx,top+.064,cz,radius+.07,radius+.13,Glow,true);
 }

 static ImageBrush TopTexture(GpuStyle style){
  return Rig.Paint(1024,432,dc=>{
   var rnd=new Random(7);
   if(style==GpuStyle.Passive){
    dc.DrawRectangle(new LinearGradientBrush(Color.FromRgb(150,158,170),Color.FromRgb(104,112,124),90),null,new Rect(0,0,1024,432));
    var hole=new SolidColorBrush(Color.FromRgb(28,32,38));
    for(int yy=0;yy<15;yy++)for(int xx=0;xx<52;xx++){double px=60+xx*17.5+(yy%2)*8.7,py=70+yy*20;if(px<980)dc.DrawEllipse(hole,null,new Point(px,py),5.6,5.6);}
   }else{
    Color a=style==GpuStyle.Blower?Color.FromRgb(30,32,38):Color.FromRgb(70,76,86),b=style==GpuStyle.Blower?Color.FromRgb(16,17,20):Color.FromRgb(46,50,58);
    dc.DrawRectangle(new LinearGradientBrush(a,b,90),null,new Rect(0,0,1024,432));
    for(int i=0;i<520;i++){double y=rnd.NextDouble()*432;dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb((byte)rnd.Next(4,14),255,255,255)),1),new Point(0,y),new Point(1024,y));}
    if(style==GpuStyle.Blower){
     var rib=new Pen(new SolidColorBrush(Color.FromArgb(26,255,255,255)),2);
     for(int x=-432;x<1024;x+=22)dc.DrawLine(rib,new Point(x,432),new Point(x+432,0));
     dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(196,160,82)),null,new Rect(0,408,1024,10));
    }else{
     var edge=new Pen(new SolidColorBrush(Color.FromArgb(60,255,255,255)),3);
     dc.DrawLine(edge,new Point(486,10),new Point(486,422));dc.DrawLine(edge,new Point(538,10),new Point(538,422));
    }
   }
   dc.DrawRectangle(null,new Pen(new SolidColorBrush(Color.FromArgb(90,255,255,255)),4),new Rect(4,4,1016,424));
  });
 }
 static ImageBrush SideTexture(GpuStyle style){
  return Rig.Paint(1024,128,dc=>{
   if(style==GpuStyle.Blower){
    dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(18,19,22)),null,new Rect(0,0,1024,128));
    dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(196,160,82)),null,new Rect(0,8,1024,4));
    return;
   }
   dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(14,16,19)),null,new Rect(0,0,1024,128));
   // Fin stack: thin vertical fins catching light.
   for(int x=4;x<1020;x+=5){byte v=(byte)(x%10==4?120:70);dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(v,(byte)(v+6),(byte)(v+14))),null,new Rect(x,14,1.6,100));}
   dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(200,30,33,38)),null,new Rect(0,0,1024,14));
   dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(200,30,33,38)),null,new Rect(0,114,1024,14));
   if(style==GpuStyle.Twin)dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(40,44,52)),null,new Rect(300,30,424,68));
  });
 }
 static ImageBrush LogoTexture(string text){
  return Rig.Paint(1024,96,dc=>{
   var ft=new FormattedText(String.Join(" ",text.Select(ch=>ch.ToString()).ToArray()),System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface(new FontFamily("Bahnschrift, Segoe UI"),FontStyles.Normal,FontWeights.SemiBold,FontStretches.Normal),70,Brushes.White);
   ft.TextAlignment=TextAlignment.Center;dc.DrawText(ft,new Point(512,(96-ft.Height)/2));
  },true);
 }
}

// A CPU in its socket: green substrate and silver lid; the lid carries a thermal-camera style grid, one cell per thread.
class CpuRig {
 public readonly Rig Rig=new Rig(5,1.7);
 public readonly int[] Slots;public readonly ScaleTransform3D[] Columns;

 public CpuRig(int threads,double w,double h){
  var r=Rig;threads=Math.Max(1,threads);
  r.Box(-3.3,0,-3.0,3.3,.1,3.0,Rig.Board);
  r.Box(-2.65,.1,-2.65,2.65,.32,-2.35,Rig.Black);r.Box(-2.65,.1,2.35,2.65,.32,2.65,Rig.Black);
  r.Box(-2.65,.1,-2.35,-2.35,.32,2.35,Rig.Black);r.Box(2.35,.1,-2.35,2.65,.32,2.35,Rig.Black);
  r.Box(2.8,.1,-2.5,2.92,.22,2.5,Rig.Silver);r.Box(2.8,.1,2.5,3.2,.22,2.62,Rig.Silver);
  r.Box(-2.3,.1,-2.3,2.3,.24,2.3,Rig.Substrate);
  for(int i=0;i<6;i++){r.Box(-2.1+i*.25,.24,1.95,-1.98+i*.25,.29,2.12,Rig.Gold);r.Box(1.0+i*.18,.24,-2.15,1.1+i*.18,.29,-1.98,Rig.Gold);}
  r.Box(-1.85,.24,-1.85,1.85,.5,1.85,Rig.Silver);
  int cols=1;double best=1e9;
  for(int c=1;c<=threads;c++){int rows=(int)Math.Ceiling(threads/(double)c);double waste=c*rows-threads,shape=Math.Abs(Math.Log((double)c/rows));double score=waste*.6+shape*3;if(score<best){best=score;cols=c;}}
  int rowsN=(int)Math.Ceiling(threads/(double)cols);double area=3.3,gap=.08,cw=(area-(cols-1)*gap)/cols,cd=(area-(rowsN-1)*gap)/rowsN;
  Slots=new int[threads];Columns=new ScaleTransform3D[threads];
  for(int i=0;i<threads;i++){
   int row=i/cols,col=i%cols;double x0=-area/2+col*(cw+gap),z0=-area/2+row*(cd+gap);
   Slots[i]=r.Slot();Columns[i]=r.Pillar(x0,.5,z0,x0+cw,1.3,z0+cd,Slots[i]);
  }
  r.Fit(w,h,-24,42,1.02);
 }
}

// Memory modules standing in their slots; the light bar on top fills with used memory.
class RamRig {
 public readonly Rig Rig=new Rig(4,3.1);
 public readonly int[] Bars;public readonly int[] Chips;

 public RamRig(double totalGB,double w,double h){
  var r=Rig;int sticks=totalGB<=16?1:totalGB<=32?2:totalGB<=128?4:6;
  const double L=3.2,gap=.62;double z0=-(sticks-1)*gap/2;
  r.Box(-L-.4,0,z0-.6,L+.4,.1,z0+(sticks-1)*gap+.6,Rig.Board);
  var cells=r.Cells();var glow=r.Cells();var bars=new List<int>();var chips=new List<int>();
  for(int s=0;s<sticks;s++){
   double z=z0+s*gap;
   r.Box(-L-.15,.1,z-.13,L+.15,.36,z+.13,Rig.Black);
   r.Box(-L-.3,.1,z-.11,-L-.15,.5,z+.11,Rig.Silver);r.Box(L+.15,.1,z-.11,L+.3,.5,z+.11,Rig.Silver);
   r.Box(-L,.36,z-.035,L,1.62,z+.035,Rig.Pcb);
   r.Box(-L+.05,.5,z-.1,L-.05,1.58,z+.1,Rig.Gunmetal);
   for(int k=0;k<8;k++){int slot=r.Slot();chips.Add(slot);double x=-L+.3+k*.78;r.Cell(cells,x,.75,z+.1,x+.58,1.35,z+.115,slot);}
   for(int k=0;k<16;k++){int slot=r.Slot();bars.Add(slot);double x=-L+.12+k*.392;r.Cell(glow,x,1.58,z-.06,x+.34,1.68,z+.06,slot);}
  }
  r.Commit(cells);r.Commit(glow,true);Bars=bars.ToArray();Chips=chips.ToArray();
  r.Fit(w,h,-26,30,1.03);
 }
}

// 2.5-inch drives lying flat: a capacity gauge on the label and an activity LED.
class DriveRig {
 public readonly Rig Rig=new Rig(3,4.2);
 public readonly List<int[]> Gauges=new List<int[]>();public readonly List<int> Leds=new List<int>();
 public readonly List<double> Centers=new List<double>();

 public DriveRig(int count,double w,double h){
  var r=Rig;count=Math.Max(1,count);const double W=1.45,D=2.0,step=3.6;var cells=r.Cells();var glow=r.Cells();
  for(int i=0;i<count;i++){
   double cx=(i-(count-1)/2.0)*step;Centers.Add(cx);
   r.Box(cx-W,0,-D,cx+W,.42,D,Rig.Gunmetal);
   r.Box(cx-W+.15,.42,-D+.2,cx+W-.15,.45,D-.2,Color.FromRgb(26,30,38));
   r.Box(cx-1.0,.1,D,cx+.2,.3,D+.06,Rig.Black);
   var gauge=new int[10];for(int k=0;k<10;k++){gauge[k]=r.Slot();double x=cx-W+.35+k*.235;r.Cell(cells,x,.45,.5,x+.18,.47,1.3,gauge[k]);}
   Gauges.Add(gauge);
   int led=r.Slot();Leds.Add(led);r.Cell(glow,cx+W-.45,.45,D-.55,cx+W-.25,.5,D-.35,led);
  }
  r.Commit(cells);r.Commit(glow,true);
  r.Fit(w,h,-18,40,1.04);
 }
}

// A small Wi-Fi router whose LEDs blink with traffic.
class RouterRig {
 public readonly Rig Rig=new Rig(5,.6);
 public readonly int Down,Up,Power;

 public RouterRig(double w,double h){
  var r=Rig;
  r.Box(-2.0,0,-1.1,2.0,.5,1.1,Rig.Gunmetal);
  r.Box(-1.85,.5,-.95,1.85,.53,.95,Rig.Plate);
  r.Box(-1.7,.2,-1.0,-1.55,2.1,-.85,Rig.Black);r.Box(1.55,.2,-1.0,1.7,2.1,-.85,Rig.Black);
  var glow=r.Cells();Power=r.Slot();Down=r.Slot();Up=r.Slot();
  r.Cell(glow,-1.2,.53,.62,-.9,.57,.8,Power);r.Cell(glow,-.4,.53,.62,-.1,.57,.8,Down);r.Cell(glow,.4,.53,.62,.7,.57,.8,Up);
  r.Commit(glow,true);
  r.Fit(w,h,-24,30,1.04);
 }
}
