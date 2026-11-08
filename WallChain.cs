using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

// How diseases reach people. Nodes, edges and per-disease routes come from science/transmission.json,
// whose disease names are the same canonical names the outbreak pipeline writes, so the route of the
// disease featured on the globe lights up and everything else stays dim.
partial class WallView {
 class ChainNode {public string Id;public Point At;public double R;public Border View;public TextBlock Label;}
 class ChainEdge {public ChainNode From,To;public Path Line,Head;public Point A,C,B;public Ellipse Dot;}
 readonly Dictionary<string,ChainNode> chainNodes=new Dictionary<string,ChainNode>();
 readonly Dictionary<string,ChainEdge> chainEdges=new Dictionary<string,ChainEdge>();
 readonly List<ChainEdge> chainHot=new List<ChainEdge>();
 Canvas chainGraph;StackPanel chainChips;TextBlock chainTitle;Path chainLoop;TextBlock chainLoopLabel;
 double chainW;string chainFor=null;long chainBuilt=-1;bool chainLoopHot;
 // disease 场景的直角发丝框版：颜色只用黑/骨白/血红，full 场景仍然是老配色。
 bool chainExhibit;

 void BuildChain(double x,double y,double w,double h){BuildChain(x,y,w,h,false);}

 void BuildChain(double x,double y,double w,double h,bool exhibit){
  chainExhibit=exhibit;chainW=exhibit?w-24:w;
  if(exhibit){
   // 与标本窗同规格：直角 1px 发丝边、纯黑一档底板；标注在框上方，不进画面、不压右边框。
   Put(stage,new Border{Width=w,Height=h,CornerRadius=new CornerRadius(0),Background=Th.Panel,BorderBrush=Th.Hair2,BorderThickness=new Thickness(1),ClipToBounds=true},x,y);
   Text(stage,"传播链 / TRANSMISSION",x+1,y-20,10,Th.Steel,family:Th.Mono);
   chainChips=Put(stage,new StackPanel{Orientation=Orientation.Horizontal},x+24,y+14);
   chainGraph=Put(stage,new Canvas{Width=chainW,Height=h},x+12,y-24);
   chainTitle=Text(stage,"",x+24,y+h-30,12,Th.T1,FontWeights.SemiBold,w-48);
  }else{
   Card(stage,x,y,w,h);
   Text(stage,"传播链",x+28,y+20,22,Th.T1,FontWeights.SemiBold);
   chainChips=Put(stage,new StackPanel{Orientation=Orientation.Horizontal},x+28,y+60);
   chainGraph=Put(stage,new Canvas{Width=w,Height=h},x,y);
   chainTitle=Text(stage,"",x+28,y+h-50,15,Th.T1,FontWeights.SemiBold,w-56);
  }
 }

 static Point Toward(Point p,Point q,double d){var v=q-p;if(v.Length<1e-6)return p;v.Normalize();return p+v*d;}
 static Point Bezier(Point a,Point c,Point b,double u){double v=1-u;return new Point(v*v*a.X+2*v*u*c.X+u*u*b.X,v*v*a.Y+2*v*u*c.Y+u*u*b.Y);}

 // Bend each edge just enough to clear the other nodes and their labels.
 Point RouteCurve(Point a,Point b,ChainNode from,ChainNode to){
  var mid=new Point((a.X+b.X)/2,(a.Y+b.Y)/2);var d=b-a;var n=new Vector(-d.Y,d.X);n.Normalize();
  Point best=mid;double bestGap=-1e9;
  foreach(double off in new[]{0.0,22,-22,40,-40,60,-60,82,-82}){
   var c=mid+n*off;double gap=1e9;
   for(int i=2;i<19;i++){var p=Bezier(a,c,b,i/20.0);foreach(var o in chainNodes.Values){if(o==from||o==to)continue;gap=Math.Min(gap,Math.Min((p-o.At).Length-o.R,(p-new Point(o.At.X,o.At.Y+o.R+10)).Length-14));}}
   if(gap>=8)return c;if(gap>bestGap){bestGap=gap;best=c;}
  }
  return best;
 }

 static string[] Pair(object step){return ((System.Collections.IEnumerable)step).Cast<object>().Select(o=>o.ToString()).ToArray();}
 static IEnumerable<object> Steps(Dictionary<string,object> route){var list=J.Get(route,"path") as System.Collections.IEnumerable;return list==null?new object[0]:list.Cast<object>();}

 void BuildChainGraph(){
  chainGraph.Children.Clear();chainNodes.Clear();chainEdges.Clear();chainHot.Clear();chainLoop=null;
  var nodes=J.A(store.Transmission,"nodes");if(nodes.Count==0)return;
  double w=chainW;double[] rowY={128,226,322};
  foreach(var row in nodes.GroupBy(n=>(int)J.N(n,"row")).OrderBy(g=>g.Key)){
   var list=row.ToList();int r=Math.Max(0,Math.Min(2,row.Key));double margin=r==0?40:72;
   for(int i=0;i<list.Count;i++){
    double px=list.Count==1?w/2:margin+i*(w-2*margin)/(list.Count-1);
    chainNodes[J.S(list[i],"id")]=new ChainNode{Id=J.S(list[i],"id"),At=new Point(px,rowY[r]),R=r==2?30:22};
   }
  }
  // Edges: the union of every disease route.
  foreach(var dis in J.A(store.Transmission,"diseases"))foreach(object step in Steps(dis)){
   var pair=Pair(step);
   if(pair.Length!=2||!chainNodes.ContainsKey(pair[0])||!chainNodes.ContainsKey(pair[1]))continue;
   string key=pair[0]+">"+pair[1];if(chainEdges.ContainsKey(key))continue;
   var e=new ChainEdge{From=chainNodes[pair[0]],To=chainNodes[pair[1]]};
   var c=RouteCurve(e.From.At,e.To.At,e.From,e.To);e.C=c;e.A=Toward(e.From.At,c,e.From.R+2);e.B=Toward(e.To.At,c,e.To.R+4);
   var geo=new PathGeometry{Figures={new PathFigure(e.A,new PathSegment[]{new QuadraticBezierSegment(c,e.B,true)},false)}};
   e.Line=Put(chainGraph,new Path{Data=geo,Stroke=Th.T4,StrokeThickness=1.1,StrokeDashArray=new DoubleCollection{4,4},Opacity=.55},0,0);
   var dir=e.B-c;dir.Normalize();var nn=new Vector(-dir.Y,dir.X);
   var head=new StreamGeometry();using(var s=head.Open()){s.BeginFigure(e.B,true,true);s.LineTo(e.B-dir*8+nn*4,true,false);s.LineTo(e.B-dir*8-nn*4,true,false);}head.Freeze();
   e.Head=Put(chainGraph,new Path{Data=head,Fill=Th.T4,Opacity=.55},0,0);
   e.Dot=Put(chainGraph,new Ellipse{Width=6,Height=6,Visibility=Visibility.Hidden,RenderTransform=new TranslateTransform()},0,0);
   chainEdges[key]=e;
  }
  ChainNode human;
  if(chainNodes.TryGetValue("human",out human)){
   var hp=human.At;
   var loop=new PathGeometry{Figures={new PathFigure(new Point(hp.X+human.R-4,hp.Y-14),new PathSegment[]{new ArcSegment(new Point(hp.X+human.R-4,hp.Y+14),new Size(22,18),0,true,SweepDirection.Clockwise,true)},false)}};
   chainLoop=Put(chainGraph,new Path{Data=loop,Stroke=Th.T4,StrokeThickness=1.1,StrokeDashArray=new DoubleCollection{4,4}},0,0);
   chainLoopLabel=Text(chainGraph,"人传人",hp.X+human.R+46,hp.Y-9,12,Th.T4);
  }
  foreach(var n in nodes){
   ChainNode cn;if(!chainNodes.TryGetValue(J.S(n,"id"),out cn))continue;
   var glyph=new TextBlock{Text=J.S(n,"glyph"),FontFamily=new FontFamily("Segoe UI Emoji, Microsoft YaHei UI"),FontSize=cn.R>25?30:21,Foreground=Th.T2,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,0,2)};
   cn.View=Put(chainGraph,new Border{Width=cn.R*2,Height=cn.R*2,CornerRadius=new CornerRadius(cn.R),Background=chainExhibit?Th.Cell:Th.B(0xFF0F1622),BorderBrush=Th.T4,BorderThickness=new Thickness(1.2),Child=glyph},cn.At.X-cn.R,cn.At.Y-cn.R);
   cn.Label=Text(chainGraph,J.S(n,"name"),cn.At.X-45,cn.At.Y+cn.R+3,12,Th.T3,null,90,TextAlignment.Center);
  }
 }

 void ApplyChain(){
  if(store.TransmissionStamp!=chainBuilt){chainBuilt=store.TransmissionStamp;BuildChainGraph();}
  // Chips: the featured disease first, then the others in attention order, including global notices.
  var names=new List<string>();if(FeaturedDisease!="")names.Add(FeaturedDisease);
  foreach(var e in ranked)names.Add(J.S(e,"disease"));
  foreach(var n in J.A(store.Outbreaks,"notices").OrderByDescending(n=>J.N(n,"level")))names.Add(J.S(n,"disease"));
  chainChips.Children.Clear();double used=0;
  foreach(string dname in names.Where(s=>s!="").Distinct()){
   bool on=dname==FeaturedDisease;
   // disease 场景不引入第四种颜色：焦点病种血红，其余钢灰；full 场景仍用各病种自己的色。
   var col=chainExhibit?(on?Th.C(0xFFC0392B):Th.C(0xFF79807F)):DiseaseColor(dname);
   var chip=new Border{CornerRadius=new CornerRadius(9),Padding=new Thickness(8,1,8,2),Margin=new Thickness(0,0,6,0),
    Background=new SolidColorBrush(Color.FromArgb((byte)(on?46:16),col.R,col.G,col.B)),BorderBrush=new SolidColorBrush(Color.FromArgb((byte)(on?200:60),col.R,col.G,col.B)),BorderThickness=new Thickness(1),
    Child=new TextBlock{Text=Lang.Disease(dname),FontSize=11,FontFamily=Th.Sans,Foreground=new SolidColorBrush(on?col:Color.FromArgb(170,col.R,col.G,col.B))}};
   chip.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));
   if(used+chip.DesiredSize.Width>chainW-56)break;used+=chip.DesiredSize.Width;chainChips.Children.Add(chip);
  }
  // Highlight the featured disease's route.
  var route=store.Route(FeaturedDisease);var color=chainExhibit?Th.C(0xFFC0392B):DiseaseColor(FeaturedDisease);var brush=new SolidColorBrush(color);brush.Freeze();
  chainHot.Clear();var lit=new HashSet<string>();chainLoopHot=route!=null&&J.S(route,"personToPerson")=="True";
  if(route!=null)foreach(object step in Steps(route)){
   var pair=Pair(step);ChainEdge e;
   if(pair.Length==2&&chainEdges.TryGetValue(pair[0]+">"+pair[1],out e)){chainHot.Add(e);lit.Add(pair[0]);lit.Add(pair[1]);}
  }
  if(chainLoopHot)lit.Add("human");
  foreach(var e in chainEdges.Values){
   bool on=chainHot.Contains(e);
   e.Line.Stroke=on?brush:Th.T4;e.Line.StrokeThickness=on?2.2:1.1;e.Line.Opacity=on?1:.55;e.Head.Fill=on?brush:Th.T4;e.Head.Opacity=e.Line.Opacity;
   e.Dot.Fill=brush;e.Dot.Visibility=on?Visibility.Visible:Visibility.Hidden;
  }
  if(chainLoop!=null){chainLoop.Stroke=chainLoopHot?brush:Th.T4;chainLoop.StrokeThickness=chainLoopHot?2:1.1;chainLoopLabel.Foreground=chainLoopHot?brush:Th.T4;}
  foreach(var n in chainNodes.Values){
   if(n.View==null)continue;bool on=lit.Contains(n.Id);
   n.View.BorderBrush=on?brush:Th.T4;n.View.Background=on?new SolidColorBrush(Color.FromArgb(40,color.R,color.G,color.B)):chainExhibit?Th.Cell:Th.B(0xFF0F1622);
   ((TextBlock)n.View.Child).Foreground=on?Th.T1:Th.T3;n.Label.Foreground=on?Th.T1:Th.T3;n.View.Opacity=on?1:.7;
  }
  chainTitle.Text=Lang.Disease(FeaturedDisease);chainTitle.Foreground=brush;
 }

 void TickChain(double t){
  if(chainFor!=FeaturedDisease||store.TransmissionStamp!=chainBuilt){chainFor=FeaturedDisease;try{ApplyChain();}catch{}}
  double off=-t*14;
  for(int i=0;i<chainHot.Count;i++){
   var e=chainHot[i];e.Line.StrokeDashOffset=off;
   var p=Bezier(e.A,e.C,e.B,((t*.45+i*.31)%1+1)%1);var tt=(TranslateTransform)e.Dot.RenderTransform;tt.X=p.X-3;tt.Y=p.Y-3;
  }
  if(chainLoopHot&&chainLoop!=null)chainLoop.StrokeDashOffset=off;
 }
}
