using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

class MolCard {public MoleculeScene Scene;public TextBlock[] Steps;public int Phase=-1;public Canvas Bottom,Top;public double ViewTop;}

partial class WallView {
 MolCard tcr;

 // ---------- molecule cards ----------
 MolCard BuildMolecule(double x,double y,double w,double h,string title,string subtitle,MolSpec spec,string path,Tuple<Color,string>[] legend,string bottomCell,string topCell){
  Card(stage,x,y,w,h);
  var card=new MolCard();
  double vy=y+70,vh=h-70-46;card.ViewTop=vy-y;
  // Cell artwork sits behind the 3D view inside a clip matching the card's rounded corners.
  var cells=Put(stage,new Canvas{Width=w,Height=h,Clip=new RectangleGeometry(new Rect(0,0,w,h),18,18)},x,y);
  card.Bottom=Membrane(w,true,Th.Antigen,bottomCell,7);cells.Children.Add(card.Bottom);
  if(topCell!=null){card.Top=Membrane(w,false,Th.Light,topCell,3);cells.Children.Add(card.Top);}
  var tint=new RadialGradientBrush{GradientStops={new GradientStop(Color.FromArgb(26,111,155,255),0),new GradientStop(Color.FromArgb(0,0,0,0),1)}};tint.Freeze();
  Put(stage,new Ellipse{Width=w*.7,Height=vh*.8,Fill=tint},x+w*.15,vy+vh*.08);
  card.Scene=new MoleculeScene(path,spec,w-2,vh);card.Scene.View.Width=w-2;card.Scene.View.Height=vh;
  Put(stage,card.Scene.View,x+1,vy);
  if(!card.Scene.Loaded)Text(stage,"结构文件暂不可读",x+28,vy+vh/2-10,14,Th.T3);

  Text(stage,title,x+28,y+22,24,Th.T1,FontWeights.SemiBold);
  Text(stage,subtitle,x+28,y+58,13,Th.T2);
  var steps=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};
  string[] names={"解离","对接","结合"};card.Steps=new TextBlock[3];
  for(int i=0;i<3;i++){
   if(i>0)steps.Children.Add(new Rectangle{Width=14,Height=1,Fill=Th.T4,Margin=new Thickness(7,0,7,0),VerticalAlignment=VerticalAlignment.Center});
   card.Steps[i]=new TextBlock{Text=names[i],FontSize=12,FontFamily=Th.Sans,Foreground=Th.T4};steps.Children.Add(card.Steps[i]);
  }
  Put(stage,new Grid{Width=200,Children={steps}},x+w-28-200,y+30);

  var row=new StackPanel{Orientation=Orientation.Horizontal};
  foreach(var item in legend){
   var b=new SolidColorBrush(item.Item1);b.Freeze();
   row.Children.Add(new Ellipse{Width=8,Height=8,Fill=b,Margin=new Thickness(0,0,7,0),VerticalAlignment=VerticalAlignment.Center});
   row.Children.Add(new TextBlock{Text=item.Item2,FontSize=12,FontFamily=Th.Sans,Foreground=Th.T2,Margin=new Thickness(0,0,13,0)});
  }
  Put(stage,row,x+26,y+h-36);
  return card;
 }

 // A stretch of cell surface: lipid bilayer (heads + tails), a faint cytoplasm gradient and a few other
 // surface proteins. Drawn around y=0; the card moves it to the projected membrane height every frame.
 static Canvas Membrane(double w,bool cellBelow,Color tone,string label,int seed){
  var c=new Canvas{Width=w,Height=0,RenderTransform=new TranslateTransform()};
  double sag=16,sgn=cellBelow?1:-1;Func<double,double> curve=x=>sgn*sag*Math.Pow((x-w/2)/(w/2),2);
  // Cytoplasm
  var fill=new StreamGeometry();using(var g=fill.Open()){
   var pts=new List<Point>();for(double x=0;x<=w;x+=8)pts.Add(new Point(x,curve(x)+sgn*16));
   g.BeginFigure(new Point(0,sgn*900),true,true);g.PolyLineTo(pts,true,true);g.LineTo(new Point(w,sgn*900),true,true);
  }fill.Freeze();
  var grad=new LinearGradientBrush{MappingMode=BrushMappingMode.Absolute,StartPoint=new Point(0,0),EndPoint=new Point(0,sgn*220),GradientStops={new GradientStop(Color.FromArgb(36,tone.R,tone.G,tone.B),0),new GradientStop(Color.FromArgb(8,tone.R,tone.G,tone.B),1)}};
  c.Children.Add(new Path{Data=fill,Fill=grad});
  // Bilayer: outer leaflet heads on the surface, inner leaflet heads 15 px into the cell, tails meeting between.
  var heads=new GeometryGroup();var tails=new StreamGeometry();
  using(var g=tails.Open()){
   for(double x=3;x<w;x+=6.4){
    double y0=curve(x),y1=y0+sgn*15;
    heads.Children.Add(new EllipseGeometry(new Point(x,y0),2.7,2.7));heads.Children.Add(new EllipseGeometry(new Point(x+3.2,y1),2.7,2.7));
    foreach(double dx in new[]{-.9,.9}){g.BeginFigure(new Point(x+dx,y0+sgn*2.5),false,false);g.LineTo(new Point(x+dx,y0+sgn*7),true,false);g.BeginFigure(new Point(x+3.2+dx,y1-sgn*2.5),false,false);g.LineTo(new Point(x+3.2+dx,y1-sgn*7.5),true,false);}
   }
  }
  heads.Freeze();tails.Freeze();
  c.Children.Add(new Path{Data=tails,Stroke=new SolidColorBrush(Color.FromArgb(70,tone.R,tone.G,tone.B)),StrokeThickness=.9});
  c.Children.Add(new Path{Data=heads,Fill=new SolidColorBrush(Color.FromArgb(185,tone.R,tone.G,tone.B))});
  // Other surface proteins (glycoproteins) for texture, kept away from the centre where the complex sits.
  var rnd=new Random(seed);var other=new GeometryGroup();var stems=new StreamGeometry();
  using(var g=stems.Open()){
   for(int i=0;i<9;i++){
    double x=24+rnd.NextDouble()*(w-48);if(Math.Abs(x-w/2)<120)continue;double len=12+rnd.NextDouble()*16,y0=curve(x)-sgn*2,y1=y0-sgn*len;
    g.BeginFigure(new Point(x,y0),false,false);g.LineTo(new Point(x+(rnd.NextDouble()-.5)*6,y1),true,false);
    other.Children.Add(new EllipseGeometry(new Point(x,y1-sgn*3),4.5+rnd.NextDouble()*2.5,3.6));
   }
  }
  other.Freeze();stems.Freeze();
  c.Children.Add(new Path{Data=stems,Stroke=new SolidColorBrush(Color.FromArgb(90,tone.R,tone.G,tone.B)),StrokeThickness=1.4});
  c.Children.Add(new Path{Data=other,Fill=new SolidColorBrush(Color.FromArgb(70,tone.R,tone.G,tone.B))});
  // Cell name, inside the cell near the right edge.
  var tb=new TextBlock{Text=label,FontSize=12,FontFamily=Th.Sans,Foreground=new SolidColorBrush(Color.FromArgb(210,tone.R,tone.G,tone.B)),Width=260,TextAlignment=cellBelow?TextAlignment.Right:TextAlignment.Left};
  Canvas.SetLeft(tb,cellBelow?w-28-260:28);Canvas.SetTop(tb,cellBelow?curve(w-40)+22:curve(40)-40);c.Children.Add(tb);
  return c;
 }

 void TickMolecule(MolCard m,double t){
  int phase=m.Scene.Tick(t),step=phase==1?1:phase==2?2:0;
  if(m.Scene.Loaded){
   ((TranslateTransform)m.Bottom.RenderTransform).Y=m.ViewTop+m.Scene.BottomMembraneY;
   if(m.Top!=null)((TranslateTransform)m.Top.RenderTransform).Y=m.ViewTop+m.Scene.TopMembraneY;
  }else{m.Bottom.Visibility=Visibility.Collapsed;if(m.Top!=null)m.Top.Visibility=Visibility.Collapsed;}
  if(step==m.Phase)return;m.Phase=step;
  for(int i=0;i<3;i++)m.Steps[i].Foreground=i!=step?Th.T4:step==2?Th.B(0xFFFFD678):step==1?Th.Accent:Th.T2;
 }

 // ---------- outbreak card ----------
 GlobeScene globe;
 double GR=140,gcx,gcy,infoX,infoW;public string FeaturedDisease="";
 TextBlock obSub,obLine,obCountry,obDisease,obLevel,obLabel1,obValue1,obLabel2,obValue2,obEmpty,obListNote;
 Border obBadge;Rectangle[] obMeter=new Rectangle[5];
 const int Ranks=6;readonly TextBlock[] rankName=new TextBlock[Ranks],rankDisease=new TextBlock[Ranks],rankCases=new TextBlock[Ranks],rankDeaths=new TextBlock[Ranks],rankDate=new TextBlock[Ranks],rankLevel=new TextBlock[Ranks];readonly Ellipse[] rankDot=new Ellipse[Ranks];Rectangle rankMark;TextBlock obDate;
 Canvas reticle,sweep;Polyline leader;Ellipse leaderEnd;Rect leaderKey=Rect.Empty;
 List<Dictionary<string,object>> events=new List<Dictionary<string,object>>(),ranked=new List<Dictionary<string,object>>();
 long paintedStamp=-1;int featured=-1;double globeStart=-1;

 static readonly string[] LevelName={"低","中","高"};
 static readonly Color[] LevelColor={Th.C(0xFFE6CF6A),Th.C(0xFFF7A24B),Th.C(0xFFFF5D4D)};
 // On-screen attention estimate: disease severity class plus log-scaled cases and deaths, nudged by recency.
 // Shown with a caption that it is this display's estimate, not an official risk rating.
 static double Attention(Dictionary<string,object> e){
  string d=J.S(e,"disease");var m=J.D(e,"metrics");
  double s=Regex.IsMatch(d,"埃博拉|马尔堡|拉沙|中东呼吸|尼帕|鼠疫|克里米亚|不明原因|Ebola|Marburg",RegexOptions.IgnoreCase)?.55:Regex.IsMatch(d,"霍乱|黄热|猴痘|白喉|脊髓灰质炎|炭疽|裂谷|疟疾|流脑|禽流感|Cholera|Mpox",RegexOptions.IgnoreCase)?.35:.2;
  s+=Math.Min(1,Math.Log10(J.N(m,"cases")+1)/6)*.3+Math.Min(1,Math.Log10(J.N(m,"deaths")+1)/4)*.2;
  if((DateTime.UtcNow-J.Time(J.S(e,"date"))).TotalDays<14)s+=.04;
  int cdc=(int)J.N(e,"cdcLevel");if(cdc>=1&&cdc<=4)s=Math.Max(s,new[]{0,.40,.58,.78,.95}[cdc]);
  return Math.Min(1,s);
 }
 static int Level(double s){return s>=.7?2:s>=.45?1:0;}
 Color DiseaseColor(string d){
  var r=store.Route(d);string hex=r==null?"":J.S(r,"color");
  try{if(hex.Length==7)return (Color)ColorConverter.ConvertFromString(hex);}catch{}
  return Th.C(0xFFCFE3FF);
 }
 static double Distance(Dictionary<string,object> a,Dictionary<string,object> b){
  double la1=J.N(a,"lat")*Math.PI/180,la2=J.N(b,"lat")*Math.PI/180,dl=(J.N(a,"lon")-J.N(b,"lon"))*Math.PI/180;
  return Math.Acos(Math.Max(-1,Math.Min(1,Math.Sin(la1)*Math.Sin(la2)+Math.Cos(la1)*Math.Cos(la2)*Math.Cos(dl))));
 }

 void BuildOutbreak(double x,double y,double w,double h){
  Card(stage,x,y,w,h);
  GR=Math.Min(Scene=="disease"?430:200,(h-130)/2);gcx=x+48+GR;gcy=y+(h+60)/2;infoX=gcx+GR+70;infoW=x+w-28-infoX;
  Text(stage,"全球传染病监测",x+28,y+20,22,Th.T1,FontWeights.SemiBold);
  obSub=Text(stage,"",x+28,y+53,12,Th.T2,null,520);
  // Legend: attention levels and arcs.
  var lg=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};
  lg.Children.Add(new TextBlock{Text="关注度",FontSize=12,FontFamily=Th.Sans,Foreground=Th.T3,Margin=new Thickness(0,0,8,0)});
  for(int i=2;i>=0;i--){var b=new SolidColorBrush(LevelColor[i]);b.Freeze();lg.Children.Add(new Ellipse{Width=8,Height=8,Fill=b,Margin=new Thickness(0,0,5,0),VerticalAlignment=VerticalAlignment.Center});lg.Children.Add(new TextBlock{Text=LevelName[i],FontSize=12,FontFamily=Th.Sans,Foreground=Th.T2,Margin=new Thickness(0,0,12,0)});}
  Put(stage,new Grid{Width=300,Children={lg}},x+w-28-300+12,y+24);
  var halo=new RadialGradientBrush{GradientStops={new GradientStop(Color.FromArgb(0,90,150,230),0),new GradientStop(Color.FromArgb(0,90,150,230),.80),new GradientStop(Color.FromArgb(70,90,150,230),.835),new GradientStop(Color.FromArgb(18,90,150,230),.9),new GradientStop(Color.FromArgb(0,90,150,230),1)}};halo.Freeze();
  double hr=GR/.835;Put(stage,new Ellipse{Width=hr*2,Height=hr*2,Fill=halo},gcx-hr,gcy-hr);
  var bezel=new StreamGeometry();
  using(var c=bezel.Open())for(int i=0;i<72;i++){double a=i*5*Math.PI/180,r0=GR+11,r1=GR+(i%6==0?17:14);c.BeginFigure(new Point(gcx+r0*Math.Cos(a),gcy+r0*Math.Sin(a)),false,false);c.LineTo(new Point(gcx+r1*Math.Cos(a),gcy+r1*Math.Sin(a)),true,false);}
  bezel.Freeze();Put(stage,new Path{Data=bezel,Stroke=Th.T4,StrokeThickness=1},0,0);
  sweep=Put(stage,new Canvas{Width=0,Height=0},gcx,gcy);
  for(int i=0;i<16;i++)sweep.Children.Add(new Path{Data=Ring(0,0,GR+7,-i*4-4,-i*4),Stroke=Th.B(0xFF9CC8FF),StrokeThickness=1.4,Opacity=.5*(1-i/16.0)});
  sweep.RenderTransform=new RotateTransform(0);

  globe=new GlobeScene();double side=GR*2/.8;globe.Frame(.8);globe.View.Width=side;globe.View.Height=side;
  Put(stage,globe.View,gcx-side/2,gcy-side/2);
  Put(stage,new Ellipse{Width=GR*2,Height=GR*2,Stroke=Th.B(0x305F9BE8),StrokeThickness=1},gcx-GR,gcy-GR);

  leader=Put(stage,new Polyline{Stroke=Th.B(0x66FFFFFF),StrokeThickness=1,StrokeLineJoin=PenLineJoin.Round},0,0);
  leaderEnd=Put(stage,new Ellipse{Width=5,Height=5,Fill=Th.B(0xAAFFFFFF)},infoX-14.5,0);
  reticle=Put(stage,new Canvas(),gcx,gcy);
  Put(reticle,new Ellipse{Width=26,Height=26,Stroke=Th.B(0xCCFFFFFF),StrokeThickness=1.2},-13,-13);
  for(int i=0;i<4;i++)reticle.Children.Add(new Line{X1=17,X2=22,Y1=0,Y2=0,Stroke=Th.B(0xCCFFFFFF),StrokeThickness=1.2,RenderTransform=new RotateTransform(i*90+45)});
  reticle.RenderTransform=new TransformGroup{Children={new RotateTransform(0),new TranslateTransform(0,0)}};

  // Featured report.
  double ix=infoX,iy=y+100;
  obLine=Text(stage,"",ix,iy,12,Th.T3,null,infoW-200);
  obCountry=Text(stage,"",ix,iy+18,34,Th.T1,FontWeights.SemiBold,infoW-200);
  obDisease=Text(stage,"",ix,iy+66,17,Th.Warm,null,infoW-200);
  obBadge=Put(stage,new Border{CornerRadius=new CornerRadius(12),Padding=new Thickness(12,3,12,4)},0,iy+22);
  obLevel=new TextBlock{FontSize=14,FontFamily=Th.Sans,FontWeight=FontWeights.SemiBold};obBadge.Child=obLevel;
  for(int i=0;i<5;i++)obMeter[i]=Put(stage,new Rectangle{Width=22,Height=6,RadiusX=2,RadiusY=2,Fill=Th.Cell},ix+infoW-5*27+i*27+5,iy+64);
  obLabel1=Text(stage,"",ix,iy+104,12,Th.T3,null,220);obValue1=Text(stage,"",ix,iy+120,38,Th.T1,FontWeights.Light,240);
  obLabel2=Text(stage,"",ix+250,iy+104,12,Th.T3,null,200);obValue2=Text(stage,"",ix+250,iy+120,38,Th.T1,FontWeights.Light,200);
  Text(stage,"通报日期",ix+470,iy+104,12,Th.T3,null,infoW-470);obDate=Text(stage,"",ix+470,iy+120,38,Th.T1,FontWeights.Light,infoW-470);
  obEmpty=Text(stage,"近期通报暂不可用",ix,iy+40,15,Th.T3);obEmpty.Visibility=Visibility.Collapsed;
  Hairline(stage,ix,y+286,infoW);

  // Ranking by attention.
  Text(stage,"关注度排行",ix,y+300,12,Th.T3);
  rankMark=Put(stage,new Rectangle{Width=infoW+12,Height=26,RadiusX=6,RadiusY=6,Fill=Th.B(0x14FFFFFF),Visibility=Visibility.Collapsed},ix-6,0);
  for(int i=0;i<Ranks;i++){
   double ry=y+326+i*28;
   rankDot[i]=Put(stage,new Ellipse{Width=9,Height=9,Fill=Th.T4},ix,ry+6);
   rankName[i]=Text(stage,"",ix+18,ry,14,Th.T1,null,130);
   rankDisease[i]=Text(stage,"",ix+156,ry+1,13,Th.T2,null,110);
   rankCases[i]=Text(stage,"",ix+420,ry+1,13,Th.T2,null,150,TextAlignment.Right);
   rankDeaths[i]=Text(stage,"",ix+548,ry+1,13,Th.T3,null,120,TextAlignment.Right);
   rankDate[i]=Text(stage,"",ix+infoW-40,ry+1,12,Th.T3,null,70,TextAlignment.Right);
   rankLevel[i]=Text(stage,"",ix+infoW,ry,14,Th.T1,FontWeights.SemiBold,24,TextAlignment.Right);
  }
 }
 void ApplyOutbreak(){
  events=store.RecentEvents();
  var countries=new HashSet<string>(events.Select(e=>J.S(e,"iso")));
  var fetched=events.Select(e=>J.Time(J.S(e,"fetchedAt"))).DefaultIfEmpty(DateTime.MinValue).Max();
  obSub.Text="WHO / ECDC / CDC 官方通报  ·  "+events.Count+" 条 / "+countries.Count+" 国"+(fetched==DateTime.MinValue?"":"  ·  同步 "+fetched.ToLocalTime().ToString("MM-dd HH:mm"));
  ranked=events.OrderByDescending(e=>Attention(e)).ToList();
  if(store.OutbreakStamp!=paintedStamp){
   paintedStamp=store.OutbreakStamp;
   var byIso=events.GroupBy(e=>J.S(e,"iso")).ToDictionary(g=>g.Key,g=>g.Max(e=>Attention(e)));
   globe.Paint(store.World,byIso.ToDictionary(p=>p.Key,p=>LevelColor[Level(p.Value)]));
   globe.SetMarkers(events.GroupBy(e=>J.S(e,"iso")).Select(g=>{var top=g.OrderByDescending(e=>Attention(e)).First();double s=Attention(top);return Tuple.Create(J.N(top,"lat"),J.N(top,"lon"),LevelColor[Level(s)],Math.Min(1,Math.Log10(1+g.Max(e=>J.N(J.D(e,"metrics"),"cases")))/6));}));
   // Each reporting country links to the nearest country with a larger outbreak of the same disease.
   var links=new List<Tuple<double,double,double,double,Color>>();
   foreach(var g in events.GroupBy(e=>J.S(e,"disease"))){
    var list=g.GroupBy(e=>J.S(e,"iso")).Select(c=>c.OrderByDescending(e=>J.N(J.D(e,"metrics"),"cases")).First()).OrderByDescending(e=>J.N(J.D(e,"metrics"),"cases")).ToList();
    for(int i=1;i<list.Count;i++){var from=list.Take(i).OrderBy(p=>Distance(p,list[i])).First();links.Add(Tuple.Create(J.N(from,"lat"),J.N(from,"lon"),J.N(list[i],"lat"),J.N(list[i],"lon"),DiseaseColor(g.Key)));}
   }
   globe.SetArcs(links);
   featured=-1;
  }
  bool none=events.Count==0;
  foreach(var tb in new[]{obLine,obCountry,obDisease,obLabel1,obValue1,obLabel2,obValue2,obDate})tb.Visibility=none?Visibility.Collapsed:Visibility.Visible;
  obBadge.Visibility=none?Visibility.Collapsed:Visibility.Visible;
  obEmpty.Visibility=none?Visibility.Visible:Visibility.Collapsed;
  reticle.Visibility=leader.Visibility=leaderEnd.Visibility=none?Visibility.Collapsed:Visibility.Visible;
  for(int i=0;i<Ranks;i++){
   if(i>=ranked.Count){rankName[i].Text=rankDisease[i].Text=rankCases[i].Text=rankDeaths[i].Text=rankDate[i].Text=rankLevel[i].Text="";rankDot[i].Visibility=Visibility.Hidden;continue;}
   var e=ranked[i];double s=Attention(e);var lc=new SolidColorBrush(LevelColor[Level(s)]);lc.Freeze();
   rankDot[i].Visibility=Visibility.Visible;rankDot[i].Fill=lc;
   rankName[i].Text=(Lang.Code=="en"&&!String.IsNullOrEmpty(J.S(e,"country_en"))?J.S(e,"country_en"):Names.Country(e));rankDisease[i].Text=(Lang.Code=="en"&&!String.IsNullOrEmpty(J.S(e,"disease_en"))?J.S(e,"disease_en"):J.S(e,"disease"));rankDisease[i].Foreground=new SolidColorBrush(DiseaseColor(J.S(e,"disease")));
   var m=J.D(e,"metrics");int lvl=(int)J.N(e,"cdcLevel");rankCases[i].Text=J.Has(m,"cases")?Thousands(J.N(m,"cases"))+" 例":lvl>0?"CDC "+lvl+" 级":"";rankDeaths[i].Text=J.Has(m,"deaths")?"死亡 "+Thousands(J.N(m,"deaths")):"";
   string dt=J.S(e,"date");rankDate[i].Text=dt.Length>=10?dt.Substring(5,5):dt;
   rankLevel[i].Text=LevelName[Level(s)];rankLevel[i].Foreground=lc;
  }
 }

 static IEnumerable<string> SourceNames(Dictionary<string,object> e){
  var ids=J.A(e,"sources").Select(s=>J.S(s,"feedId")).ToList();if(ids.Count==0)ids.Add(J.S(e,"feedId"));
  var names=new List<string>();int cdc=(int)J.N(e,"cdcLevel");
  if(ids.Any(s=>s.StartsWith("who")))names.Add("WHO");if(ids.Any(s=>s.StartsWith("ecdc")))names.Add("ECDC");if(ids.Any(s=>s.StartsWith("cdc")))names.Add(cdc>0?"CDC "+cdc+" 级":"CDC");
  return names;
 }

 void ShowEvent(int i,double t,bool instant){
  var e=events[i];var m=J.D(e,"metrics");
  string asOf=J.S(m,"asOf"),period=J.S(m,"period");int cdc=(int)J.N(e,"cdcLevel");
  obLine.Text=String.Join("  ·  ",SourceNames(e).Concat(period!=""?new[]{period}:new string[0]).ToArray())+(asOf!=""?"  ·  截至 "+asOf.Substring(Math.Max(0,asOf.Length-5)):"")+(J.S(e,"stale")=="True"?"  ·  缓存":"");
  obCountry.Text=(Lang.Code=="en"&&!String.IsNullOrEmpty(J.S(e,"country_en"))?J.S(e,"country_en"):Names.Country(e));obDisease.Text=(Lang.Code=="en"&&!String.IsNullOrEmpty(J.S(e,"disease_en"))?J.S(e,"disease_en"):J.S(e,"disease"));obDisease.Foreground=new SolidColorBrush(DiseaseColor(J.S(e,"disease")));
  double s=Attention(e);int lv=Level(s);var lc=LevelColor[lv];
  obLevel.Text="关注度 "+LevelName[lv];obLevel.Foreground=new SolidColorBrush(lc);
  obBadge.Background=new SolidColorBrush(Color.FromArgb(34,lc.R,lc.G,lc.B));obBadge.BorderBrush=new SolidColorBrush(Color.FromArgb(120,lc.R,lc.G,lc.B));obBadge.BorderThickness=new Thickness(1);
  obBadge.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(obBadge,infoX+infoW-obBadge.DesiredSize.Width);
  for(int k=0;k<5;k++)obMeter[k].Fill=s*5>k+.15?new SolidColorBrush(LevelColor[Math.Min(2,k*3/5)]):Th.Cell;
  var metrics=new List<Tuple<string,string>>();
  if(J.Has(m,"cases"))metrics.Add(Tuple.Create(J.S(m,"caseLabel","报告病例"),Thousands(J.N(m,"cases"))));
  if(J.Has(m,"deaths"))metrics.Add(Tuple.Create("死亡",Thousands(J.N(m,"deaths"))));
  if(J.Has(m,"r0"))metrics.Add(Tuple.Create("R₀",F(J.N(m,"r0"),2)));
  if(cdc>0)metrics.Add(Tuple.Create("CDC 旅行健康提示",cdc+" 级"));
  obLabel1.Text=metrics.Count>0?metrics[0].Item1:"官方通报";obValue1.Text=metrics.Count>0?metrics[0].Item2:"—";
  obLabel2.Text=metrics.Count>1?metrics[1].Item1:"";obValue2.Text=metrics.Count>1?metrics[1].Item2:"";
  string date=J.S(e,"date");obDate.Text=date.Length>=10?date.Substring(5,5):date;FeaturedDisease=J.S(e,"disease");
  int rank=ranked.IndexOf(e);
  if(rank>=0&&rank<Ranks){rankMark.Visibility=Visibility.Visible;Canvas.SetTop(rankMark,Canvas.GetTop(rankName[rank])-4);}else rankMark.Visibility=Visibility.Collapsed;
  globe.FocusOn(J.N(e,"lat"),J.N(e,"lon"),t,instant);
 }

 void TickOutbreak(double t){
  if(globeStart<0)globeStart=t;
  if(events.Count>0){int i=(int)((t-globeStart)/9)%events.Count;if(i!=featured){bool first=featured<0;featured=i;ShowEvent(i,t,first);}}
  if(Slice(1))globe.Tick(t);
  ((RotateTransform)sweep.RenderTransform).Angle=(t*14)%360;
  var off=globe.FocusOffset();double rx=gcx+off.X*GR,ry=gcy-off.Y*GR;
  var tg=(TransformGroup)reticle.RenderTransform;((RotateTransform)tg.Children[0]).Angle=(t*9)%90;
  ((TranslateTransform)tg.Children[1]).X=rx-gcx;((TranslateTransform)tg.Children[1]).Y=ry-gcy;
  double ly=Canvas.GetTop(obCountry)+24;
  // Elbow callout: out of the reticle horizontally, then a short diagonal to the country name.
  double ex=Math.Max(rx+40,gcx+GR+22),turn=infoX-14-Math.Min(30,Math.Abs(ly-ry));
  // Rebuilt only when the reticle or the label moved (a new geometry every frame costs a re-render and garbage).
  var key=new Rect(Math.Round(rx*2)/2,Math.Round(ry*2)/2,Math.Round(Math.Max(ex,turn)),Math.Round(ly));if(key==leaderKey)return;leaderKey=key;
  leader.Points=new PointCollection{new Point(rx+16,ry),new Point(Math.Max(ex,turn),ry),new Point(infoX-14,ly)};Canvas.SetTop(leaderEnd,ly-2.5);
 }

 // ---------- projects card ----------
 // Real work first: jobs running on this machine (named after their project folder or the Codex/OpenCode
 // session that worked there), then jobs that recently finished, then recent sessions as a fallback.
 readonly TextBlock[] pjTitle=new TextBlock[3],pjStatus=new TextBlock[3],pjMeta=new TextBlock[3],pjAgo=new TextBlock[3];
 readonly Ellipse[] pjDot=new Ellipse[3],pjPulse=new Ellipse[3];
 readonly bool[] pjBusy=new bool[3];
 TextBlock pjCount;
 static readonly Regex TagPattern=new Regex(@"^\s*[（(【\[]\s*(暂缓|暂停|完成|已完成)\s*[)）】\]]\s*");

 void BuildProjects(double x,double y,double w,double h){
  Card(stage,x,y,w,h);
  Text(stage,"项目进展",x+28,y+20,22,Th.T1,FontWeights.SemiBold);
  pjCount=Text(stage,"",x+w-28,y+28,12,Th.Green,FontWeights.SemiBold,150,TextAlignment.Right);
  Put(stage,new Rectangle{Width=1,Height=230,Fill=Th.T4},x+40,y+104);
  for(int i=0;i<3;i++){
   double ry=y+92+i*112;
   pjPulse[i]=Put(stage,new Ellipse{Width=12,Height=12,Stroke=Th.Green,StrokeThickness=1.2,Opacity=0,RenderTransformOrigin=new Point(.5,.5),RenderTransform=new ScaleTransform(1,1)},x+40.5-6,ry+12-6);
   pjDot[i]=Put(stage,new Ellipse{Width=11,Height=11,Fill=Th.T4,Stroke=Th.B(0xFF0B1119),StrokeThickness=2.5},x+40.5-5.5,ry+12-5.5);
   pjTitle[i]=Text(stage,"",x+66,ry,18,Th.T1,null,w-66-24);
   pjStatus[i]=Text(stage,"",x+66,ry+32,13,Th.T2,FontWeights.SemiBold);
   pjMeta[i]=Text(stage,"",x+66+70,ry+32,13,Th.T3,null,w-66-24-70);
   pjAgo[i]=Text(stage,"",x+66,ry+56,12,Th.T4,null,w-66-24);
  }
 }

 static string Span(double seconds){
  var s=TimeSpan.FromSeconds(Math.Max(0,seconds));
  if(s.TotalMinutes<1)return "不到 1 分钟";if(s.TotalHours<1)return (int)s.TotalMinutes+" 分钟";
  if(s.TotalDays<1)return (int)s.TotalHours+" 小时 "+s.Minutes+" 分";return (int)s.TotalDays+" 天 "+s.Hours+" 小时";
 }
 static string JobName(Dictionary<string,object> j){return TagPattern.Replace(J.S(j,"name"),"");}
 // Detail line: completed / failed runs for pipelines first, then program, GPU, CPU and memory.
 static string Details(Dictionary<string,object> j,bool running){
  var parts=new List<string>();
  int done=(int)J.N(j,"runsDone"),failed=(int)J.N(j,"runsFailed");
  if(done>0||failed>0)parts.Add("已完成 "+done+" 轮"+(failed>0?" · 失败 "+failed:""));
  string program=J.S(j,"program");if(program!="")parts.Add(program);
  if(J.S(j,"gpu")=="True")parts.Add("GPU");
  if(running){parts.Add("CPU "+F(J.N(j,"cpu"),0)+"%");parts.Add("内存 "+F(J.N(j,"ramGB"),1)+" GB");}
  return String.Join("  ·  ",parts.ToArray());
 }

 void ApplyProjects(){
  var running=J.A(store.Latest,"jobs");var finished=J.A(store.Latest,"recentJobs");
  var sessions=J.A(store.Latest,"projects").OrderByDescending(p=>J.Time(J.S(p,"updated"))).ToList();
  DateTime jobsAt=J.Time(J.S(store.Latest,"jobsUpdated"));bool stale=jobsAt!=DateTime.MinValue&&(DateTime.UtcNow-jobsAt).TotalSeconds>120;
  pjCount.Text=stale?"数据延迟":running.Count>0?"运行中 "+running.Count:"";pjCount.Foreground=stale?Th.Amber:Th.Green;
  var rows=new List<Action<int>>();
  if(!stale)foreach(var j in running)rows.Add(i=>{
   pjTitle[i].Text=JobName(j);pjTitle[i].Foreground=Th.T1;
   pjStatus[i].Text="运行中";pjStatus[i].Foreground=Th.Green;pjDot[i].Fill=Th.Green;pjBusy[i]=true;
   pjMeta[i].Text="·   "+Span((DateTime.UtcNow-J.Time(J.S(j,"started"))).TotalSeconds)+(J.S(j,"wsl")=="True"?"  ·  WSL":"");
   pjAgo[i].Text=Details(j,true);
  });
  foreach(var j in finished)rows.Add(i=>{
   string st=J.S(j,"status");Brush color=st=="done"?Th.Accent:st=="failed"?Th.Warm:Th.T3;
   pjTitle[i].Text=JobName(j);pjTitle[i].Foreground=Th.T1;
   pjStatus[i].Text=st=="done"?"已完成":st=="failed"?"异常退出":"已结束";pjStatus[i].Foreground=color;pjDot[i].Fill=color;pjBusy[i]=false;
   pjMeta[i].Text="·   "+J.Time(J.S(j,"ended")).ToLocalTime().ToString("MM-dd HH:mm")+"  ·  用时 "+Span(J.N(j,"durationSec"));
   pjAgo[i].Text=Details(j,false)+(st=="failed"&&J.S(j,"exitCode")!=""?"  ·  退出码 "+J.S(j,"exitCode"):"");
  });
  foreach(var p in sessions)rows.Add(i=>{
   string raw=J.S(p,"title"),state=J.S(p,"status");DateTime updated=J.Time(J.S(p,"updated"));
   string status;Brush color;
   if(raw.Contains("暂缓")||raw.Contains("暂停")){status="暂缓";color=Th.Amber;}
   else if(raw.Contains("完成")){status="已完成";color=Th.T2;}
   else if(state=="busy"){status="对话中";color=Th.Accent;}
   else{status="最近会话";color=Th.T3;}
   pjTitle[i].Text=TagPattern.Replace(raw,"");pjTitle[i].Foreground=Th.T2;
   pjStatus[i].Text=status;pjStatus[i].Foreground=color;pjDot[i].Fill=color;pjBusy[i]=false;
   pjMeta[i].Text="·   "+J.S(p,"source");pjAgo[i].Text=Ago(updated);
  });
  for(int i=0;i<3;i++){
   if(i<rows.Count)rows[i](i);
   else{pjTitle[i].Text=i==0?"暂无运行中的任务":"";pjTitle[i].Foreground=Th.T3;pjStatus[i].Text=pjMeta[i].Text=pjAgo[i].Text="";pjDot[i].Fill=Th.T4;pjBusy[i]=false;}
   pjStatus[i].Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));
   Canvas.SetLeft(pjMeta[i],Canvas.GetLeft(pjStatus[i])+pjStatus[i].DesiredSize.Width+10);
  }
 }
 void TickProjects(double t){
  for(int i=0;i<3;i++){
   if(!pjBusy[i]){pjPulse[i].Opacity=0;continue;}
   double u=(t*.55+i*.3)%1;pjPulse[i].Opacity=.9*(1-u);var s=(ScaleTransform)pjPulse[i].RenderTransform;s.ScaleX=s.ScaleY=1+u*1.6;
  }
 }

 // ---------- disease: 观察窗版式 ----------
 // 栅格按 docs/生化危机视觉方案_20261106.md：左区地球；右栏上部监测焦点 + 传播链，下部关注度排行。
 // 两块纵向区间：hero/链 y=94–606，排行 y=624–1012（中间留 18px，与原版两块间距一致）。
 // 颜色只取报告附录 A 的色板；这个场景的版式/配色和 full 场景完全分开，full 仍走 BuildOutbreak。
 double dLX=1000,dRX=1880,dHRX=1440;
 TextBlock dzClock,dzSource,dzSync,dzLag,dzRankFoot,dzHeroKicker,dzRankRange;
 TextBlock dzVal1Label,dzVal2Label,dzEmpty;
 TextBlock[] dzCol=new TextBlock[6],dzKey=new TextBlock[3];
 Canvas dzReticle,dzRankPage,dzRankGrid;
 int dzClockSec=-1;
 // 关注度排行分页：轮播走全部通报（31 条），排行区固定 Ranks 行，跟着 hero 名次翻页。
 // 换页 = 140ms 淡出 + 140ms 淡入（约 280ms），Snap（--render）直接落页不淡。
 int dzPage=-1,dzPageWant=-1,dzPageDir,dzMarkRow=-1;
 double dzPageFade=1;
 // 末页不满 Ranks 行时整页行下移 (Ranks-n)*46/2，让 n 行在 6 行的格线区间里垂直居中；满页为 0。
 double dzRankShift;
 const double PageFade=0.14;
 // hero 轮播：两层同版式文字叠放，切换时 350ms 交叉淡入；渲染模式（Snap）直接落到目标层。
 class HeroLayer {
  public Canvas Root;public TextBlock Rank,Country,Disease,Meta,BigLabel,Big,Val1,Val2,Att;
  public TextBlock[] Val=new TextBlock[3];public Rectangle[] Meter=new Rectangle[5];public string Key="";
 }
 readonly HeroLayer[] dzHeroLayer=new HeroLayer[2];
 int dzLayer,dzHeroIdx=-1,dzFadeFrom,dzFadeTo;
 double dzFade=1,dzT,dzLastT=-1,dzEpoch=-1;
 string dzHeroId="",dzFocusId="";
 static readonly Color[] MapFill={Th.C(0xFF16191A),Th.C(0xFF23272A),Th.C(0xFF4A1515)};
 static readonly Color[] MapDot={Th.C(0xFF79807F),Th.C(0xFFD8D4C8),Th.C(0xFFC0392B)};
 static readonly Brush[] LvBrush={Th.B(0xFF79807F),Th.B(0xFFD8D4C8),Th.B(0xFFC0392B)};

 void BuildDisease(){
  double lx=dLX,cx=dRX,cw=cx-lx;
  // 地球：删辉光、轨道环、扫掠弧。只剩 1px 圈和四个方位刻度——保留的每根线都是测量。
  GR=430;gcx=478;gcy=540;
  globe=new GlobeScene();double side=GR*2/.8;globe.Frame(.8);globe.View.Width=side;globe.View.Height=side;
  Put(stage,globe.View,gcx-side/2,gcy-side/2);
  Put(stage,new Ellipse{Width=GR*2,Height=GR*2,Stroke=Th.Hair2,StrokeThickness=1},gcx-GR,gcy-GR);
  var ticks=new StreamGeometry();
  using(var c=ticks.Open())for(int i=0;i<4;i++){
   double a=i*Math.PI/2;
   c.BeginFigure(new Point(gcx+Math.Cos(a)*(GR+3),gcy+Math.Sin(a)*(GR+3)),false,false);
   c.LineTo(new Point(gcx+Math.Cos(a)*(GR+11),gcy+Math.Sin(a)*(GR+11)),true,false);
  }
  ticks.Freeze();Put(stage,new Path{Data=ticks,Stroke=Th.Border,StrokeThickness=1},0,0);
  // 菲涅尔边缘：只在上左受光侧把球缘提亮 1px，渐变收尾，不向外扩、不加模糊（不是辉光）。
  var rim=new LinearGradientBrush{StartPoint=new Point(0,0),EndPoint=new Point(1,1)};
  rim.GradientStops.Add(new GradientStop(Color.FromArgb(0x00,0xD8,0xD4,0xC8),0));
  rim.GradientStops.Add(new GradientStop(Color.FromArgb(0x3A,0xD8,0xD4,0xC8),.22));
  rim.GradientStops.Add(new GradientStop(Color.FromArgb(0x08,0xD8,0xD4,0xC8),.60));
  rim.GradientStops.Add(new GradientStop(Color.FromArgb(0x00,0xD8,0xD4,0xC8),1));
  rim.Freeze();
  Put(stage,new Path{Data=Ring(gcx,gcy,GR-.7,-160,-20),Stroke=rim,StrokeThickness=1.2},0,0);
  // 焦点准星：1px 血红，钉在地球中心那个国家上。
  dzReticle=Put(stage,new Canvas(),gcx,gcy);
  dzReticle.RenderTransform=new TranslateTransform();
  Put(dzReticle,new Ellipse{Width=16,Height=16,Stroke=Th.Signal,StrokeThickness=1,Opacity=.95},-8,-8);
  for(int i=0;i<4;i++)dzReticle.Children.Add(new Line{X1=11,X2=16,Y1=0,Y2=0,Stroke=Th.Signal,StrokeThickness=1,Opacity=.95,RenderTransform=new RotateTransform(i*90)});
  // 球缘到 hero 左缘的测量线，两端带刻度（跟着 hero 一起上移）。
  Put(stage,new Rectangle{Width=88,Height=1,Fill=Th.Border},908,127);
  Put(stage,new Rectangle{Width=1,Height=9,Fill=Th.Border},908,123);
  Put(stage,new Rectangle{Width=1,Height=9,Fill=Th.Border},995,123);
  // 关注度排行：6 行，表头带单位，行内不再重复"例/死亡"。换位后落在下半部分 y=624–1012。
  dzRankRange=Text(stage,"WATCHLIST  01–06",lx,624,10,Th.Steel,family:Th.Mono);
  Text(stage,Lang.T("rank.title"),lx,636,20,Th.T1,FontWeights.SemiBold);
  Text(stage,Lang.T("rank.sub"),cx,642,10,Th.T3,family:Th.Mono,width:220,align:TextAlignment.Right);
  Hairline(stage,lx,676,cw);
  string[] heads={Lang.T("col.country"),Lang.T("col.disease"),Lang.T("col.cases"),Lang.T("col.deaths"),Lang.T("col.date"),Lang.T("col.attention")};
  // 英文国名比中文长：国名列 124→180，病种列整体右移，病例/死亡/通报列位置不动（不压病种列）。
  bool wideName=Lang.Code=="en";
  double[] hx={lx+16,wideName?lx+204:lx+148,1440,1590,1620,cx};
  double[] hw={wideName?180:120,130,150,140,80,50};
  TextAlignment[] ha={TextAlignment.Left,TextAlignment.Left,TextAlignment.Right,TextAlignment.Right,TextAlignment.Left,TextAlignment.Right};
  for(int i=0;i<6;i++)dzCol[i]=Text(stage,heads[i],hx[i],678,10,Th.T3,family:Th.Mono,width:hw[i],align:ha[i]);
  // 行内容按页重画，整页放在一个 Canvas 里，换页时只动这一层的 Opacity（格线在另一层，不跟着淡）。
  dzRankPage=Put(stage,new Canvas{Width=1920,Height=1080,IsHitTestVisible=false},0,0);
  rankMark=Put(dzRankPage,new Rectangle{Width=cw+16,Height=34,Fill=new SolidColorBrush(Color.FromArgb(22,0xC0,0x39,0x2B)),Stroke=new SolidColorBrush(Color.FromArgb(86,0xC0,0x39,0x2B)),StrokeThickness=1,Visibility=Visibility.Collapsed},lx-8,0);
  for(int i=0;i<Ranks;i++){
   double ry=706+i*46;
   rankDot[i]=Put(dzRankPage,new Ellipse{Width=5,Height=5,Fill=Th.Steel},lx,ry+7);
   rankName[i]=Text(dzRankPage,"",lx+16,ry,15,Th.T1,null,wideName?180:124);
   rankDisease[i]=Text(dzRankPage,"",wideName?lx+204:lx+148,ry+1,13,Th.T2,null,132);
   rankCases[i]=Text(dzRankPage,"",1440,ry+1,15,Th.T1,null,150,TextAlignment.Right);
   rankDeaths[i]=Text(dzRankPage,"",1590,ry+1,15,Th.T2,null,140,TextAlignment.Right);
   rankDate[i]=Text(dzRankPage,"",1620,ry+3,11,Th.T3,null,80);
   rankLevel[i]=Text(dzRankPage,"",cx,ry+1,12,Th.T2,null,60,TextAlignment.Right);
  }
  // 排行格线不再固定在 stage 上：每页按行数重画（PaintRankGrid），末页跟着 dzRankShift 居中；
  // 表头下那条发丝线（676）仍固定在 stage 上，不参与分页。
  dzRankGrid=Put(stage,new Canvas{Width=1920,Height=1080,IsHitTestVisible=false},0,0);
  PaintRankGrid(Ranks,0);
  dzRankFoot=Text(stage,"",lx,984,10,Th.Steel,family:Th.Mono);
  Hairline(stage,lx,1012,cw);

  // 监测焦点：换到上半部分 y=94–606（原排行所占区间）。主数字 96px，只有死亡数/病例数配得上这个字号。
  // 会随轮播变化的文字都放进两个 HeroLayer（交叉淡入用），固定标签留在版面上。
  dzHeroKicker=Text(stage,Lang.T("hero.kicker"),lx,94,11,Th.Steel);
  Put(stage,new Rectangle{Width=2,Height=20,Fill=Th.Signal},lx,198);
  Hairline(stage,lx,324,440);
  dzVal1Label=Text(stage,Lang.T("metric.cases"),lx,338,10,Th.T3,family:Th.Mono);
  dzVal2Label=Text(stage,Lang.T("metric.cfr"),lx+190,338,10,Th.T3,family:Th.Mono);
  Hairline(stage,lx,394,440);
  Text(stage,Lang.T("attention"),lx,406,11,Th.Steel);
  for(int i=0;i<3;i++)dzKey[i]=Text(stage,Lang.T(new[]{"key.reportDate","key.asOf","key.source"}[i]),lx,460+i*22,11,Th.T3);
  Text(stage,Lang.T("note.arcs"),lx,538,10,Th.T3,null,430);
  for(int i=0;i<2;i++)BuildDiseaseHeroLayer(i);
  dzEmpty=Text(stage,Lang.T("empty.reports"),lx,106,26,Th.T3);dzEmpty.Visibility=Visibility.Collapsed;
 }

 // One copy of every hero value. Both copies use the exact coordinates the single copy used before,
 // so the carousel does not change the layout by a pixel.
 void BuildDiseaseHeroLayer(int i){
  var l=new HeroLayer();
  var c=Put(stage,new Canvas{Width=1920,Height=1080,Visibility=i==0?Visibility.Visible:Visibility.Collapsed,Opacity=i==0?1:0},0,0);
  c.IsHitTestVisible=false;l.Root=c;
  l.Rank=Text(c,"",dRX,94,10,Th.T3,family:Th.Mono,width:120,align:TextAlignment.Right);
  l.Country=Text(c,"",dLX,106,40,Th.T1,FontWeights.SemiBold,440);
  l.Disease=Text(c,"",dLX,156,20,Th.T2,null,230);
  l.Meta=Text(c,"",dHRX,114,11,Th.T3,null,240,TextAlignment.Right);
  l.BigLabel=Text(c,"",dLX+10,202,11,Th.Steel);
  l.Big=Text(c,"",dLX,194,96,Th.T1,FontWeights.Light);
  l.Val1=Text(c,"",dLX,350,26,Th.T1,FontWeights.Light,150);
  l.Val2=Text(c,"",dLX+190,350,26,Th.T1,FontWeights.Light,150);
  l.Att=Text(c,"",dLX+220,406,11,Th.Steel);
  for(int k=0;k<5;k++)l.Meter[k]=Put(c,new Rectangle{Width=24,Height=5,Fill=Th.GridLine},dLX+72+k*28,411);
  for(int k=0;k<3;k++)l.Val[k]=Text(c,"",dHRX,460+k*22,11,Th.T2,null,260,TextAlignment.Right);
  dzHeroLayer[i]=l;
 }

 void ApplyDisease(){
  events=store.RecentEvents();
  var countries=new HashSet<string>(events.Select(e=>J.S(e,"iso")));
  var fetched=events.Select(e=>J.Time(J.S(e,"fetchedAt"))).DefaultIfEmpty(DateTime.MinValue).Max();
  ranked=events.OrderByDescending(e=>Attention(e)).ToList();
  if(store.OutbreakStamp!=paintedStamp){
   paintedStamp=store.OutbreakStamp;
   var byIso=events.GroupBy(e=>J.S(e,"iso")).ToDictionary(g=>g.Key,g=>g.Max(e=>Attention(e)));
   globe.Paint(store.World,byIso.ToDictionary(p=>p.Key,p=>MapFill[Level(p.Value)]));
   globe.SetMarkers(events.GroupBy(e=>J.S(e,"iso")).Select(g=>{var top=g.OrderByDescending(e=>Attention(e)).First();double s=Attention(top);return Tuple.Create(J.N(top,"lat"),J.N(top,"lon"),MapDot[Level(s)],Math.Min(1,Math.Log10(1+g.Max(e=>J.N(J.D(e,"metrics"),"cases")))/6));}));
   // 同病种关联弧线：全部冷灰，只有当前焦点病种那一组是暗血红。
   string hero=ranked.Count>0?J.S(ranked[0],"disease"):"";
   var links=new List<Tuple<double,double,double,double,Color>>();
   foreach(var g in events.GroupBy(e=>J.S(e,"disease"))){
    var list=g.GroupBy(e=>J.S(e,"iso")).Select(c=>c.OrderByDescending(e=>J.N(J.D(e,"metrics"),"cases")).First()).OrderByDescending(e=>J.N(J.D(e,"metrics"),"cases")).ToList();
    Color arcColor=g.Key==hero?Th.C(0xFF4A1515):Th.C(0xFF2E3538);
    for(int i=1;i<list.Count;i++){var from=list.Take(i).OrderBy(p=>Distance(p,list[i])).First();links.Add(Tuple.Create(J.N(from,"lat"),J.N(from,"lon"),J.N(list[i],"lat"),J.N(list[i],"lon"),arcColor));}
   }
   globe.SetArcs(links);
   RecolorGlobe();
   if(dzHeroLayer[0]!=null)dzHeroLayer[0].Key=dzHeroLayer[1].Key="";
  }
  int lag=0;
  if(events.Count>0){
   var last=events.Max(e=>J.Time(J.S(e,"date")));
   if(last!=DateTime.MinValue)lag=Math.Max(0,(int)(DateTime.Now.Date-last.ToLocalTime().Date).TotalDays);
   dzRankFoot.Text=Lang.Fmt("rank.foot",Lang.N(events.Count),Lang.N(countries.Count),(last==DateTime.MinValue?"—":last.ToLocalTime().ToString("yyyy-MM-dd")),lag);
   dzSource.Text=Lang.T("footer.source")+"  WHO / ECDC / CDC";
   dzSync.Text=Lang.T("footer.sync")+"  "+(fetched==DateTime.MinValue?"—":fetched.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
   dzLag.Text=Lang.Fmt("footer.lag",lag);
  }else{
   dzRankFoot.Text=Lang.T("rank.none");
   dzSource.Text=Lang.T("footer.source")+"  WHO / ECDC / CDC";
   dzSync.Text=Lang.T("footer.sync")+"  —";
   dzLag.Text=Lang.T("footer.lagNone");
  }
  if(ranked.Count>0){
   int i=dzHeroIdx>=0&&dzHeroIdx<ranked.Count?dzHeroIdx:0;
   // Ranking data changed under the same slot (event identity differs) → fade like a carousel step.
   bool animate=dzHeroIdx>=0&&HeroId(ranked[i])!=dzHeroId;
   dzHeroIdx=i;
   ShowDiseaseHero(ranked[i],animate);
   // 3 s 数据刷新重画当前页（数字变了但页码不动）；正在翻页时留给翻页结束那次重画。
   if(dzPageDir==0)PaintRankPage(dzPage<0?i/Ranks:dzPage);
  }else ShowDiseaseEmpty();
 }

 // 把某一页（0 起）画进排行区：页内固定 Ranks 行，行号 = 全局名次 - 页首。
 void PaintRankPage(int page){
  int pages=Math.Max(1,(ranked.Count+Ranks-1)/Ranks);
  if(page<0)page=0;if(page>=pages)page=pages-1;
  dzPage=page;
  int start=page*Ranks,end=Math.Min(start+Ranks,ranked.Count);
  // 末页只有 n<Ranks 条：n 行在 6*46=276px 的格线区间里垂直居中，偏移 (Ranks-n)*46/2；
  // 满页 n=Ranks 偏移为 0，行位与旧版完全一致。格线由 PaintRankGrid 按同一偏移重画。
  int n=end-start;
  dzRankShift=(Ranks-n)*46/2.0;
  PaintRankGrid(n,dzRankShift);
  for(int i=0;i<Ranks;i++){
   int ri=start+i;
   PlaceRankRow(i,dzRankShift);
   if(ri>=ranked.Count){rankName[i].Text=rankDisease[i].Text=rankCases[i].Text=rankDeaths[i].Text=rankDate[i].Text=rankLevel[i].Text="";rankDot[i].Visibility=Visibility.Hidden;continue;}
   var e=ranked[ri];double s=Attention(e);int lv=Level(s);var m=J.D(e,"metrics");
   rankDot[i].Visibility=Visibility.Visible;rankDot[i].Fill=LvBrush[lv];
   rankName[i].Text=Lang.Country(Lang.Code=="en"&&!String.IsNullOrEmpty(J.S(e,"country_en"))?J.S(e,"country_en"):Names.Country(e));
   rankDisease[i].Text=(Lang.Code=="en"&&!String.IsNullOrEmpty(J.S(e,"disease_en"))?J.S(e,"disease_en"):J.S(e,"disease"));
   rankCases[i].Text=J.Has(m,"cases")?Lang.N(J.N(m,"cases")):(J.N(e,"cdcLevel")>0?Lang.Fmt("cdc.level",(int)J.N(e,"cdcLevel")):"—");
   rankDeaths[i].Text=J.Has(m,"deaths")?Lang.N(J.N(m,"deaths")):"—";
   string dt=J.S(e,"date");rankDate[i].Text=dt.Length>=10?dt.Substring(0,10):dt;
   rankLevel[i].Text=Lang.Level(lv);rankLevel[i].Foreground=LvBrush[lv];
  }
  // 页码提示占用原来固定写死的 "WATCHLIST 01–06" 那一格：显示本页名次区间 / 总条数。
  dzRankRange.Text=ranked.Count==0?"WATCHLIST":String.Format(CultureInfo.InvariantCulture,"WATCHLIST  {0:D2}–{1:D2} / {2}",start+1,end,ranked.Count);
  dzMarkRow=-1;SetRankMark();
 }

 // 排行格线：每页重画。满页 n=Ranks 时为 5 条行间线 + 底边，与旧版固定 6 条完全同位；
 // 不足 Ranks 行只画 n 条（n-1 条行间线 + 最后一行下的底边），整体跟着 off 居中，不会穿过文字或高亮带。
 // 表头下那条发丝线（676）仍固定在 stage 上，不在这里。
 void PaintRankGrid(int n,double off){
  if(dzRankGrid==null)return;
  if(n<1)n=1;if(n>Ranks)n=Ranks;
  dzRankGrid.Children.Clear();
  for(int i=1;i<n;i++)Put(dzRankGrid,new Rectangle{Width=dRX-dLX,Height=1,Fill=Th.GridLine},dLX,706+i*46-9+off);
  Hairline(dzRankGrid,dLX,706+n*46-6+off,dRX-dLX);
 }

 // 第 i 行的基准 top 是 706+i*46，off 为末页居中下移量（满页 0）。行内的点/文字按原相对位置一起走。
 void PlaceRankRow(int i,double off){
  double ry=706+i*46+off;
  Canvas.SetTop(rankDot[i],ry+7);
  Canvas.SetTop(rankName[i],ry);
  Canvas.SetTop(rankDisease[i],ry+1);
  Canvas.SetTop(rankCases[i],ry+1);
  Canvas.SetTop(rankDeaths[i],ry+1);
  Canvas.SetTop(rankDate[i],ry+3);
  Canvas.SetTop(rankLevel[i],ry+1);
 }

 // 高亮带只指向当前显示页里的那一行；hero 落在别的页时先收起，等翻页落地再定位。末页跟着行一起下移。
 void SetRankMark(){
  int row=dzHeroIdx>=0&&dzPage>=0&&dzHeroIdx/Ranks==dzPage?dzHeroIdx%Ranks:-1;
  if(row==dzMarkRow)return;
  dzMarkRow=row;
  if(row<0){rankMark.Visibility=Visibility.Collapsed;return;}
  rankMark.Visibility=Visibility.Visible;Canvas.SetTop(rankMark,706+row*46-7+dzRankShift);
 }

 // 翻页淡入淡出：先整页淡出，落地换页，再淡入；Snap 模式下不经过这里（Tick 里直接落页）。
 void AdvancePageFade(double dt){
  double step=snapped?1:dt/PageFade;
  if(dzPageDir<0){
   dzPageFade-=step;
   if(dzPageFade<=0){
    dzPageFade=0;dzRankPage.Opacity=0;
    PaintRankPage(dzPageWant>=0?dzPageWant:0);
    dzPageDir=1;
    dzPageFade=Math.Min(1,step);      // 同一帧就开始淡入，不留全黑帧
   }
  }else{
   dzPageFade+=step;
   if(dzPageFade>=1){dzPageFade=1;dzPageDir=0;}
  }
  dzRankPage.Opacity=dzPageFade;
 }

 static string HeroId(Dictionary<string,object> e){return J.S(e,"iso")+"|"+J.S(e,"disease")+"|"+J.S(e,"date");}

 void ShowDiseaseEmpty(){
  dzHeroIdx=-1;dzHeroId=dzFocusId="";dzFade=1;
  dzPage=-1;dzPageWant=-1;dzPageDir=0;dzPageFade=1;dzMarkRow=-1;dzRankPage.Opacity=1;
  // 空数据恢复原版满页 6 条格线，避免残留上一页的末页格线。
  PaintRankGrid(Ranks,0);dzRankShift=0;
  foreach(var l in dzHeroLayer){if(l==null)continue;l.Key="";l.Root.Visibility=Visibility.Collapsed;l.Root.Opacity=1;}
  rankMark.Visibility=Visibility.Collapsed;dzEmpty.Visibility=Visibility.Visible;
 }

 // Render mode and a switch arriving inside the 350 ms window both need the fade settled now.
 void FinishHeroFade(){
  if(dzFade>=1)return;
  var a=dzHeroLayer[dzFadeFrom];var b=dzHeroLayer[dzFadeTo];
  a.Root.Opacity=0;a.Root.Visibility=Visibility.Collapsed;
  b.Root.Opacity=1;b.Root.Visibility=Visibility.Visible;
  dzLayer=dzFadeTo;dzFade=1;
 }

 // 350 ms crossfade stepped from the real frame delta; Snap() jumps straight to the target.
 void AdvanceHeroFade(double dt){
  if(dzFade>=1)return;
  var a=dzHeroLayer[dzFadeFrom];var b=dzHeroLayer[dzFadeTo];
  dzFade=snapped?1:Math.Min(1,dzFade+dt/.35);
  double u=dzFade*dzFade*(3-2*dzFade);
  a.Root.Opacity=1-u;b.Root.Opacity=u;
  if(dzFade>=1){a.Root.Opacity=0;a.Root.Visibility=Visibility.Collapsed;b.Root.Opacity=1;b.Root.Visibility=Visibility.Visible;dzLayer=dzFadeTo;}
 }

 // 地球贴图重映射：WallGlobe.cs 不动，只在它画完贴图后把水面/陆地/高亮映射到报告色板（冷灰 + 干血）。
 // 拿不到贴图就原样保留，异常一律吞掉——3D 本身不受影响。
 static DiffuseMaterial GlobeMaterial(Viewport3D view){
  foreach(var v in view.Children){
   var mv=v as ModelVisual3D;if(mv==null)continue;
   var grp=mv.Content as Model3DGroup;if(grp==null)continue;
   foreach(var m in grp.Children){
    var g=m as GeometryModel3D;if(g==null)continue;
    var mg=g.Material as MaterialGroup;if(mg==null)continue;
    foreach(var mm in mg.Children){var dm=mm as DiffuseMaterial;if(dm!=null)return dm;}
   }
  }
  return null;
 }

 void RecolorGlobe(){
  try{
   var dm=GlobeMaterial(globe.View);if(dm==null)return;
   var ib=dm.Brush as ImageBrush;if(ib==null)return;
   var src=ib.ImageSource as BitmapSource;if(src==null)return;
   int w=src.PixelWidth,h=src.PixelHeight,stride=w*4;var buf=new byte[stride*h];
   src.CopyPixels(buf,stride,0);
   for(int i=0;i<buf.Length;i+=4){
    int b=buf[i],g=buf[i+1],r=buf[i+2];
    int maxc=Math.Max(r,Math.Max(g,b)),minc=Math.Min(r,Math.Min(g,b));
    int oR,oG,oB;
    if(maxc-minc>30&&r>g&&r>b){
     // 高亮国：暗血红 → 血红一档
     double u=Math.Max(0,Math.Min(1,(r-40)/100.0));
     oR=(int)(74+u*66);oG=(int)(21+u*7);oB=(int)(21+u*7);
    }else{
     int l=(r*299+g*587+b*114)/1000;
     double u=Math.Max(0,Math.Min(1,(l-8)/100.0));double v=u*u*(3-2*u);
     oR=(int)(5+v*116);oG=(int)(6+v*122);oB=(int)(7+v*120);
    }
    buf[i]=(byte)oB;buf[i+1]=(byte)oG;buf[i+2]=(byte)oR;
   }
   var bmp=BitmapSource.Create(w,h,96,96,PixelFormats.Pbgra32,null,buf,stride);bmp.Freeze();
   var nb=new ImageBrush(bmp){ViewportUnits=ib.ViewportUnits,Viewport=ib.Viewport,Stretch=ib.Stretch,TileMode=ib.TileMode};nb.Freeze();
   dm.Brush=nb;
  }catch{}
 }

 static string FeedNames(Dictionary<string,object> e){
  var ids=J.A(e,"sources").Select(s=>J.S(s,"feedId")).ToList();
  if(ids.Count==0)ids.Add(J.S(e,"feedId"));
  var names=new List<string>();
  foreach(string id in ids){string n=id.StartsWith("who")?"WHO":id.StartsWith("ecdc")?"ECDC":id.StartsWith("cdc")?"CDC":"";if(n!=""&&!names.Contains(n))names.Add(n);}
  return String.Join(" / ",names.ToArray());
 }

 static double BigSize(string s){return s.Length<=6?96:s.Length<=7?84:s.Length<=9?64:52;}

 void ShowDiseaseHero(Dictionary<string,object> e,bool crossfade){
  var m=J.D(e,"metrics");double s=Attention(e);int lv=Level(s);
  string deaths=J.Has(m,"deaths")?Lang.N(J.N(m,"deaths")):"";
  string cases=J.Has(m,"cases")?Lang.N(J.N(m,"cases")):"";
  string big=deaths!=""?deaths:cases!=""?cases:"—";
  string label=deaths!=""?Lang.T("metric.deaths"):cases!=""?Lang.T("metric.cases"):Lang.T("official");
  string cfr="—";
  if(deaths!=""&&cases!=""&&J.N(m,"cases")>0)cfr=Lang.Pct(J.N(m,"deaths")/J.N(m,"cases")*100);
  string key=J.S(e,"iso")+"|"+J.S(e,"disease")+"|"+J.S(e,"date")+"|"+big+"|"+cases+"|"+lv+"|"+cfr+"|"+Lang.Code;
  // A 3 s data refresh must not cut a running fade short: keep writing into the layer that is fading in.
  bool fading=dzFade<1;
  if(crossfade&&!snapped&&fading){FinishHeroFade();fading=false;}
  int target=crossfade&&!snapped?1-dzLayer:fading?dzFadeTo:dzLayer;
  var l=dzHeroLayer[target];
  if(l.Key!=key){
   l.Key=key;
   l.Country.Text=Lang.Country(Lang.Code=="en"&&!String.IsNullOrEmpty(J.S(e,"country_en"))?J.S(e,"country_en"):Names.Country(e));
   l.Disease.Text=(Lang.Code=="en"&&!String.IsNullOrEmpty(J.S(e,"disease_en"))?J.S(e,"disease_en"):J.S(e,"disease"));
   string period=Lang.Code=="en"&&!String.IsNullOrEmpty(J.S(m,"period_en"))?J.S(m,"period_en"):J.S(m,"period");int cdc=(int)J.N(e,"cdcLevel");
   l.Meta.Text=(period!=""?period:"")+(cdc>0?(period!=""?"  ·  ":"")+Lang.Fmt("cdc.level",cdc):"");
   l.BigLabel.Text=label;
   l.Big.Inlines.Clear();
   l.Big.Inlines.Add(new Run(big){FontSize=BigSize(big)});
   string unit=deaths!=""?Lang.T("unit.deaths"):Lang.T("unit.cases");
   if(big!="—"&&unit!="")l.Big.Inlines.Add(new Run("  "+unit){FontSize=20,Foreground=Th.Steel});
   l.Val1.Text=cases!=""?cases:"—";
   l.Val2.Text=cfr;
   for(int k=0;k<5;k++)l.Meter[k].Fill=k<lv*2+1?LvBrush[lv]:Th.GridLine;
   l.Att.Text=Lang.Level(lv);l.Att.Foreground=LvBrush[lv];
   string dt=J.S(e,"date"),asOf=J.S(m,"asOf");
   l.Val[0].Text=dt.Length>=10?dt.Substring(0,10):dt;
   l.Val[1].Text=asOf.Length>=10?asOf.Substring(0,10):asOf!=""?asOf:"—";
   string src=FeedNames(e);
   l.Val[2].Text=src!=""?src:"—";
  }
  dzEmpty.Visibility=Visibility.Collapsed;
  int ri=ranked.IndexOf(e);
  l.Rank.Text=ri>=0?"RANK "+(ri+1).ToString("D2",CultureInfo.InvariantCulture)+" / "+ranked.Count:"";
  FeaturedDisease=J.S(e,"disease");
  string id=HeroId(e);
  if(id!=dzFocusId){
   // Ease to the new epicentre like the old full-screen scene; the first selection (and --render) snap.
   globe.FocusOn(J.N(e,"lat"),J.N(e,"lon"),dzT,dzFocusId==""||snapped);
   dzFocusId=id;
  }
  dzHeroId=id;
  if(crossfade&&!snapped){
   var oldL=dzHeroLayer[dzLayer];
   Panel.SetZIndex(l.Root,1);Panel.SetZIndex(oldL.Root,0);
   l.Root.Visibility=Visibility.Visible;l.Root.Opacity=0;
   oldL.Root.Visibility=Visibility.Visible;oldL.Root.Opacity=1;
   dzFadeFrom=dzLayer;dzFadeTo=target;dzFade=0;
  }else if(!fading){
   var other=dzHeroLayer[1-target];
   Panel.SetZIndex(l.Root,1);Panel.SetZIndex(other.Root,0);
   l.Root.Visibility=Visibility.Visible;l.Root.Opacity=1;
   foreach(var o in dzHeroLayer)if(o!=l){o.Root.Visibility=Visibility.Collapsed;o.Root.Opacity=1;}
   dzLayer=target;dzFade=1;
  }
 }

 // 传播链卡片是 WallChain.cs 建的（本次不改那个文件），所以在这里做一次盖章式本地化：
 // 表头/人传人直接改文字；宿主名按节点 id 覆盖，不碰 science/transmission.json。
 long chainLangStamp=-1;
 void LocalizeChainChrome(){
  foreach(var child in stage.Children){
   var tb=child as TextBlock;if(tb==null)continue;
   if(tb.Text=="传播链 / TRANSMISSION")tb.Text=Lang.T("chain.title");
  }
  if(chainLoopLabel!=null)chainLoopLabel.Text=Lang.T("chain.p2p");
 }
 void LocalizeChainNodes(){
  if(chainNodes.Count==0)return;
  if(store.TransmissionStamp==chainLangStamp)return;
  chainLangStamp=store.TransmissionStamp;
  foreach(var n in chainNodes.Values){if(Lang.Has("host."+n.Id))n.Label.Text=Lang.T("host."+n.Id);}
  if(chainLoopLabel!=null)chainLoopLabel.Text=Lang.T("chain.p2p");
 }

 void TickDisease(double t){
  double dt=dzLastT<0?0:Math.Min(.25,t-dzLastT);dzLastT=t;dzT=t;
  LocalizeChainNodes();
  // 9 s rotation over the whole watchlist (31 reports), not only the six rows that fit on screen.
  // In --render the phase is absolute, which makes a frame at t deterministic.
  if(ranked.Count>0){
   if(dzEpoch<0)dzEpoch=snapped?0:t;
   int idx=(int)((t-dzEpoch)/9.0)%ranked.Count;
   int page=idx/Ranks;
   if(idx!=dzHeroIdx){
    dzHeroIdx=idx;
    ShowDiseaseHero(ranked[idx],true);
   }
   if(snapped){
    // 单帧渲染没有过渡：直接落到 hero 所在页。
    if(page!=dzPage){PaintRankPage(page);dzPageDir=0;dzPageFade=1;dzRankPage.Opacity=1;}
   }else if(dzPage<0){
    PaintRankPage(page);
   }else if(page!=dzPage){
    dzPageWant=page;if(dzPageDir==0)dzPageDir=-1;
   }else if(dzPageDir<0){
    dzPageDir=0;dzPageFade=1;dzRankPage.Opacity=1;   // 翻页途中 hero 又回到本页：撤销淡出
   }
  }
  if(dzPageDir!=0)AdvancePageFade(dt);else SetRankMark();
  if(dzFade<1)AdvanceHeroFade(dt);
  if(Slice(1))globe.Tick(t);
  var off=globe.FocusOffset();double rx=gcx+off.X*GR,ry=gcy-off.Y*GR;
  var tr=(TranslateTransform)dzReticle.RenderTransform;tr.X=rx-gcx;tr.Y=ry-gcy;
  dzReticle.Visibility=events.Count>0?Visibility.Visible:Visibility.Collapsed;
  var now=DateTime.Now;int sec=now.Hour*3600+now.Minute*60+now.Second;
  if(sec!=dzClockSec){dzClockSec=sec;dzClock.Text=now.ToString("yyyy-MM-dd HH:mm:ss");}
 }
}