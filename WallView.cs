using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

static class Th {
 public static Color C(uint argb){return Color.FromArgb((byte)(argb>>24),(byte)(argb>>16),(byte)(argb>>8),(byte)argb);}
 public static SolidColorBrush B(uint argb){var b=new SolidColorBrush(C(argb));b.Freeze();return b;}
 public static SolidColorBrush T1,T2,T3,T4,Hair;
 public static Color Bg1,Bg2;
 // 冷峻监测屏色板（只给 disease 场景用）：docs/生化危机视觉方案_20261106.md 附录 A。别处不引用这几个 token。
 public static readonly SolidColorBrush Bg=B(0xFF050607),Panel=B(0xFF0A0B0B),Hair2=B(0xFF23272A),GridLine=B(0xFF16191A),
  Bone=B(0xFFD8D4C8),Steel=B(0xFF79807F),SteelDim=B(0xB279807F),
  Signal=B(0xFF8C1C1C),SignalDim=B(0xFF4A1515),Alert=B(0xFFC0392B),Border=B(0xFF2E3538);
 public static void Use(string theme){
  string t=(theme??"").ToLowerInvariant();
  bool re=t.Contains("umbrella")||t.Contains("red")||t.Contains("bio");
  if(re){
   T1=B(0xFFF6E7E1);T2=B(0xFFC9A79F);T3=B(0xFF8A625D);T4=B(0xFF4A2B29);Hair=B(0xFF2B1517);
   Accent=B(0xFFE8452F);Warm=B(0xFFFF8A5C);Green=B(0xFF57D18E);Amber=B(0xFFF2B233);Violet=B(0xFFD98CFF);Cell=B(0xFF1C0E0F);
   Bg1=C(0xFF180A0B);Bg2=C(0xFF050203);
  }else{
   T1=B(0xFFEEF2F8);T2=B(0xFFB2BFCE);T3=B(0xFF7F8A9B);T4=B(0xFF44505F);Hair=B(0xFF243040);
   Accent=B(0xFF7CC4FF);Warm=B(0xFFFF7E5F);Green=B(0xFF5BE49B);Amber=B(0xFFF5B74E);Violet=B(0xFFB9A8FF);Cell=B(0xFF141B26);
   Bg1=C(0xFF0C121B);Bg2=C(0xFF05070B);
  }
 }
 // 只给 disease 场景换装：底色纯黑、三级冷灰、唯一血红；衣壳从糖果色改成骨白标本色。
 public static void UseDisease(){
  T1=B(0xFFD8D4C8);T2=B(0xFF79807F);T3=B(0xB879807F);T4=B(0xFF2E3538);Hair=B(0xFF23272A);
  Accent=Signal;Warm=Alert;Green=Steel;Amber=Steel;Violet=Steel;Cell=B(0xFF16191A);
  Bg1=C(0xFF050607);Bg2=C(0xFF050607);
  Heavy=C(0xFFD8D4C8);Light=C(0xFF79807F);Antigen=C(0xFF4A1515);B2m=C(0xFF101416);Peptide=C(0xFF23272A);
 }
 public static SolidColorBrush Accent,Warm,Green,Amber,Violet,Cell;
 // disease 场景会按语言换回退栈（见 WallLang.cs）；full 场景沿用缺省值。
 public static FontFamily Sans=new FontFamily("Segoe UI, Microsoft YaHei UI, Yu Gothic UI"),Label=new FontFamily("Bahnschrift, Segoe UI"),Mono=new FontFamily("Consolas, Microsoft YaHei UI, Yu Gothic UI");
 // Molecule chain palette: receptors cool, antigens warm, epitope peptide gold.
 public static Color Heavy=C(0xFF6F9BFF),Light=C(0xFFB9A8FF),Antigen=C(0xFFF2A07B),B2m=C(0xFF8FC7B8),Peptide=C(0xFFFFD24D);
}

// Exponential smoothing toward a target so numbers and bars glide instead of jumping.
class Tw {
 public double V,Target;bool init;
 public void Set(double v){Target=v;if(!init){V=v;init=true;}}
 public double Step(double k){V+=(Target-V)*k;return V;}
 public void Snap(){V=Target;}
}

partial class WallView:Grid {
 readonly Store store;
 readonly Canvas stage=new Canvas{Width=1920,Height=1080};
 readonly TranslateTransform drift=new TranslateTransform();
 double lastT=-1;bool snapped;

 public WallView(Store s){
  store=s;Th.Use(J.S(s.Config,"theme","science"));
  if(Scene=="disease"){
   Lang.Init(J.S(s.Config,"lang",""));
   Th.UseDisease();
   Background=Th.Bg;
   TextOptions.SetTextFormattingMode(this,TextFormattingMode.Display);
  }else{
   Background=new RadialGradientBrush(Th.Bg1,Th.Bg2){Center=new Point(.5,.42),GradientOrigin=new Point(.5,.42),RadiusX=.75,RadiusY=.85};
   TextOptions.SetTextFormattingMode(this,TextFormattingMode.Ideal);
  }
  // 文字渲染：ClearType + 面板线对齐到设备像素（面板元素继承这两个属性）。
  TextOptions.SetTextRenderingMode(this,TextRenderingMode.ClearType);
  // 位图缩放（emoji 字形、地球贴图）统一走高画质，避免视图缩放时被双线性抹糊。
  RenderOptions.SetBitmapScalingMode(this,BitmapScalingMode.HighQuality);
  UseLayoutRounding=true;SnapsToDevicePixels=true;
  var box=new Viewbox{Stretch=Stretch.Uniform,Child=stage};stage.RenderTransform=drift;
  Children.Add(box);
  if(Scene=="disease"){
   BuildDisease();
   // 传播链（WallChain.cs 同一个组件，直角发丝框版；全屏老布局不受影响）。换位后与 hero 一起在上半部分。
   BuildChain(1458,184,422,422,true);
   LocalizeChainChrome();
   BuildLetterbox();
  }else{
  BuildHardware(48,44,560,992);
  // Right side: outbreak globe across the top; TCR-pMHC, transmission chain and projects below.
  BuildOutbreak(648,44,1224,520);
  tcr=BuildMolecule(648,588,404,448,"TCR – pMHC","DMF5 TCR × MART-1 / HLA-A2 · PDB 3HG1",
   new MolSpec{Receptor="DE",Antigen="ABC",Peptide='C',BottomAnchor='A',TopAnchors="DE",Separation=17,Colors=new Dictionary<char,Color>{{'D',Th.Heavy},{'E',Th.Light},{'A',Th.Antigen},{'B',Th.B2m},{'C',Th.Peptide}},CycleOffset=17},
   store.StructureFile("tcr","3HG1.pdb"),new[]{Tuple.Create(Th.Heavy,"TCR α"),Tuple.Create(Th.Light,"TCR β"),Tuple.Create(Th.Antigen,"HLA-A2"),Tuple.Create(Th.B2m,"β2m"),Tuple.Create(Th.Peptide,"MART-1 肽")},"肿瘤细胞","T 细胞");
  BuildChain(1092,588,400,448);
  BuildProjects(1532,588,340,448);
  }
  Apply();
 }

 public void FadeIn(){stage.Opacity=0;stage.BeginAnimation(OpacityProperty,new DoubleAnimation(0,1,TimeSpan.FromSeconds(1.1)){EasingFunction=new CubicEase()});}

 // Called on the UI thread after the store has polled files.
 public void Apply(){
  if(Scene=="disease"){try{ApplyDisease();}catch{}return;}
  try{ApplyHardware();}catch{}
  try{ApplyOutbreak();}catch{}
  try{ApplyProjects();}catch{}
 }

 // Render mode: jump every tween to its target so a single frame shows real values.
 public void Snap(){snapped=true;}

 // Lighter scenes for software rendering (--lite clip,nospec,mesh,aliased or all); see WallApp.
 public static string LiteOpts="";
 public static bool Lite(string opt){return LiteOpts.Length>0&&(LiteOpts=="all"||(","+LiteOpts+",").Contains(","+opt+","));}
 // Software 3D: clip each viewport to its box and draw edges aliased (no extra coverage pass).
 public static void LiteView(System.Windows.Controls.Viewport3D v,bool clipAlways){v.ClipToBounds=clipAlways||Lite("clip");if(Lite("aliased"))RenderOptions.SetEdgeMode(v,EdgeMode.Aliased);if(Lite("cache"))v.CacheMode=new BitmapCache(.6);}
 // With "slice", the three 3D groups (hardware, globe, molecule) take turns: each redraws every third frame.
 int frameNo;
 bool Slice(int k){return snapped||!Lite("slice")||frameNo%3==k;}

 // Profiling hook (--skip hw,links,sway,mol,globe,chain,projects): parts named here stop animating.
 public static string Skip="";
 public static string Scene="full";
 public static bool Off(string part){return Skip.Length>0&&(","+Skip+",").Contains(","+part+",");}

 public void Tick(double t){
  double dt=lastT<0?0:Math.Min(.25,t-lastT);lastT=t;frameNo++;
  double k=snapped?1:1-Math.Exp(-dt*4.5);
  // 遮幅必须齐边，画面只留 ±1px 的防烧屏位移。
  if(Scene=="disease"){drift.X=Math.Round(Math.Sin(t/173));drift.Y=Math.Round(Math.Sin(t/241));}
  else{drift.X=Math.Round(3*Math.Sin(t/173));drift.Y=Math.Round(2*Math.Sin(t/241));}
  if(Scene!="disease"&&!Off("hw"))TickHardware(t,k);
  if(Scene!="disease"&&!Off("projects"))TickProjects(t);
  if(Scene=="disease"){if(!Off("globe"))TickDisease(t);}
  else if(!Off("globe"))TickOutbreak(t);
  // 传播链两个场景都有：disease 在右下窗、full 在老三栏，只在各自布局里建一次，这里统一 tick。
  if(!Off("chain"))TickChain(t);
 }

 // ---------- drawing helpers ----------
 static T Put<T>(Canvas c,T e,double x,double y) where T:UIElement{Canvas.SetLeft(e,x);Canvas.SetTop(e,y);c.Children.Add(e);return e;}
 static TextBlock Text(Canvas c,string s,double x,double y,double size,Brush fg,FontWeight? weight=null,double width=0,TextAlignment align=TextAlignment.Left,FontFamily family=null){
  var tb=new TextBlock{Text=s,FontSize=size,Foreground=fg,FontWeight=weight??FontWeights.Normal,FontFamily=family??Th.Sans,TextAlignment=align,TextTrimming=TextTrimming.CharacterEllipsis};
  Typography.SetNumeralAlignment(tb,FontNumeralAlignment.Tabular);
  if(width>0)tb.Width=width;
  return Put(c,tb,align==TextAlignment.Right&&width>0?x-width:x,y);
 }
 // A value with a smaller, dimmer unit on the same baseline.
 // Skips the rebuild when nothing changed: re-creating runs every frame forces a text re-layout.
 static void Value(TextBlock tb,string value,string unit,double unitSize){
  string key=value+"\u0001"+unit;if(key.Equals(tb.Tag))return;tb.Tag=key;
  tb.Inlines.Clear();tb.Inlines.Add(new Run(value));
  if(!String.IsNullOrEmpty(unit))tb.Inlines.Add(new Run(unit){FontSize=unitSize,Foreground=Th.T2,FontWeight=FontWeights.Normal});
 }
 // 1.15px：1px 被 Viewbox 缩到下采样时会被摊成两行 40% 的灰线；非整数宽度保住总覆盖率。
 // 尺寸不能被 UseLayoutRounding 圆回 1，否则等于没改。
 static Rectangle Hairline(Canvas c,double x,double y,double w){return Put(c,new Rectangle{Width=w,Height=1.15,Fill=Th.Hair,UseLayoutRounding=false},x,y);}
 static Border Card(Canvas c,double x,double y,double w,double h){
  var bg=new LinearGradientBrush(Th.C(0xF20F1620),Th.C(0xF00A0F17),90);bg.Freeze();
  return Put(c,new Border{Width=w,Height=h,CornerRadius=new CornerRadius(18),Background=bg,BorderBrush=Th.B(0xFF19222F),BorderThickness=new Thickness(1)},x,y);
 }
 static string Spaced(string s){return String.Join(" ",s.Select(ch=>ch.ToString()).ToArray());}
 static string F(double v,int decimals){return v.ToString("F"+decimals,CultureInfo.InvariantCulture);}
 static string Thousands(double v){return v.ToString("N0",CultureInfo.InvariantCulture);}
 static string Ago(DateTime utc){
  var d=DateTime.UtcNow-utc;
  if(utc==DateTime.MinValue)return "";
  if(d.TotalMinutes<1)return "刚刚";if(d.TotalHours<1)return (int)d.TotalMinutes+" 分钟前";if(d.TotalDays<1)return (int)d.TotalHours+" 小时前";return (int)d.TotalDays+" 天前";
 }
 static Geometry Ring(double cx,double cy,double r,double a0,double a1){
  var g=new StreamGeometry();using(var c=g.Open()){
   Func<double,Point> p=a=>new Point(cx+r*Math.Cos(a*Math.PI/180),cy+r*Math.Sin(a*Math.PI/180));
   c.BeginFigure(p(a0),false,false);c.ArcTo(p(a1),new Size(r,r),0,a1-a0>180,SweepDirection.Clockwise,true,false);
  }g.Freeze();return g;
 }

 // ---------- disease: 画幅 ----------
 // 上下 64px 纯黑遮幅（有效画面 1920×952）、四角取景器角标、画框发丝线、上下遮幅的机构行。
 // 右下那句免责声明已经删除；下沿只剩时钟 / 数据源 / 同步 / 延迟四段，均匀铺开避免右边空一大块。
 void BuildLetterbox(){
  const double W=1920,H=1080,bar=64;
  Put(stage,new Rectangle{Width=W,Height=bar,Fill=Brushes.Black},0,0);
  Put(stage,new Rectangle{Width=W,Height=bar,Fill=Brushes.Black},0,H-bar);
  Put(stage,new Rectangle{Width=W,Height=1,Fill=Th.Hair2},0,bar);
  Put(stage,new Rectangle{Width=W,Height=1,Fill=Th.Hair2},0,H-bar-1);
  CornerMark(stage,20,84,1,1);CornerMark(stage,1900,84,-1,1);CornerMark(stage,20,996,1,-1);CornerMark(stage,1900,996,-1,-1);
  var title=Text(stage,Lang.T("title.global"),32,25,12,Th.T1,FontWeights.SemiBold);
  title.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));
  // 英文标题本身就是 GLOBAL DISEASE WATCH，再跟一行 GLOBAL WATCH 会重复，英文下只留主标题。
  if(Lang.Code!="en")Text(stage,"GLOBAL WATCH",Math.Round(32+title.DesiredSize.Width+24),26,10,Th.Steel,family:Th.Mono);
  Text(stage,"WHO / ECDC / CDC",1888,26,10,Th.Steel,family:Th.Mono,width:220,align:TextAlignment.Right);
  dzClock=Text(stage,"",32,1041,10,Th.Steel,family:Th.Mono);
  dzSource=Text(stage,"",640,1041,10,Th.T3,family:Th.Mono);
  dzSync=Text(stage,"",1200,1041,10,Th.T3,family:Th.Mono);
  dzLag=Text(stage,"",1888,1041,10,Th.T3,family:Th.Mono,width:200,align:TextAlignment.Right);
 }
 // 取景器角标：两条 22px 细线，sx/sy 决定从角点向内画的方向。
 static void CornerMark(Canvas c,double x,double y,double sx,double sy){
  Put(c,new Rectangle{Width=22,Height=1,Fill=Th.Border},sx>0?x:x-22,y);
  Put(c,new Rectangle{Width=1,Height=22,Fill=Th.Border},x,sy>0?y:y-22);
 }
}
