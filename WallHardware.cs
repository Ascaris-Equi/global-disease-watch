using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

// Hardware column for visitors: recognisable 3D parts whose colours, lights and fans follow live load,
// joined by a data bus whose particles follow PCIe, memory, paging, disk and network traffic.
partial class WallView {
 const double HX=72,HW=512,DriveW=246;
 TextBlock gpuModel,gpuSpec,gpuUtil,gpuState,cpuName,cpuUtil,ramSpec,ramUtil,pageLine,chartTitle,ioLine,netDown,netUp;
 readonly TextBlock[] statLabel=new TextBlock[4],statValue=new TextBlock[4],chartTicks=new TextBlock[3];
 readonly List<TextBlock> driveLabels=new List<TextBlock>();
 Path gpuArea,gpuLine,cpuLine,vramLine;
 Canvas gpuHost,cpuHost,ramHost,driveHost,netHost,driveText;
 GpuRig gpuRig;CpuRig cpuRig;RamRig ramRig;DriveRig driveRig;RouterRig routerRig;
 readonly Tw util=new Tw(),vram=new Tw(),fan=new Tw(),cpuAll=new Tw(),ram=new Tw(),diskIo=new Tw(),rx=new Tw(),tx=new Tw(),fracUse=new Tw(),fracStandby=new Tw();Tw[] coreLoad=new Tw[0];
 double[] diskUse=new double[0];
 string gpuFor="",cpuFor="",ramFor="",driveFor="";long chartStamp=-1;double lastRig=-1,lastFan=-1;

 static readonly double[] StatX={72,192,344,464};
 static readonly Regex Passive=new Regex("L20|L40|L4\\b|A100|A800|H100|H800|H200|A30\\b|A40\\b",RegexOptions.IgnoreCase);
 static readonly Regex Pro=new Regex("Quadro|RTX A\\d|RTX \\d{4} Ada|RTX PRO|\\bT[46]00\\b|\\bA[2-6]000\\b",RegexOptions.IgnoreCase);
 static readonly Color Standby=Color.FromRgb(72,120,190),FreeColor=Color.FromRgb(70,80,98);

 Canvas Host(double x,double y,double w,double h,double glowW){
  var halo=new RadialGradientBrush{GradientStops={new GradientStop(Color.FromArgb(30,90,140,220),0),new GradientStop(Color.FromArgb(0,90,140,220),1)}};halo.Freeze();
  Put(stage,new Ellipse{Width=glowW,Height=h*.8,Fill=halo},x+(w-glowW)/2,y+h*.15);
  return Put(stage,new Canvas{Width=w,Height=h},x,y);
 }
 static void Mount(Canvas host,Rig rig){host.Children.Clear();rig.View.Width=host.Width;rig.View.Height=host.Height;host.Children.Add(rig.View);}

 void BuildHardware(double x,double y,double w,double h){
  Card(stage,x,y,w,h);
  BuildLinks();
  // Colour key.
  Text(stage,"空闲",HX+HW-206,60,11,Th.T3);
  var heat=new LinearGradientBrush{StartPoint=new Point(0,0),EndPoint=new Point(1,0)};
  for(int i=1;i<Rig.HeatAt.Length;i++)heat.GradientStops.Add(new GradientStop(Rig.HeatCol[i],(Rig.HeatAt[i]-Rig.HeatAt[1])/(1-Rig.HeatAt[1])));
  heat.Freeze();Put(stage,new Rectangle{Width=140,Height=6,RadiusX=3,RadiusY=3,Fill=heat},HX+HW-176,66);
  Text(stage,"满载",HX+HW,60,11,Th.T3,null,30,TextAlignment.Right);

  // GPU: titled with the real model; memory size and state underneath.
  gpuModel=Text(stage,"",HX,90,20,Th.T1,FontWeights.SemiBold,340);
  gpuSpec=Text(stage,"",HX,120,12,Th.T2);
  gpuState=Text(stage,"",HX+90,120,12,Th.T3,null,120);
  gpuUtil=Text(stage,"",HX+HW,82,50,Th.T1,FontWeights.Light,170,TextAlignment.Right);
  Text(stage,"繁忙程度",HX+HW,142,12,Th.T3,null,170,TextAlignment.Right);
  gpuHost=Host(HX,148,HW,226,430);
  string[] labels={"显存","温度","功耗","PCIe"};
  for(int i=0;i<4;i++){statLabel[i]=Text(stage,labels[i],StatX[i],384,12,Th.T3,null,118);statValue[i]=Text(stage,"—",StatX[i],400,20,Th.T1,null,120);}
  Hairline(stage,HX,438,HW);

  // CPU (left) and memory (right), either side of the bus.
  double half=(HW-20)/2,rx0=HX+half+24;
  Text(stage,"处理器 CPU",HX,452,16,Th.T1,FontWeights.SemiBold);
  cpuUtil=Text(stage,"",HX+half,444,32,Th.T1,FontWeights.Light,100,TextAlignment.Right);
  cpuName=Text(stage,"",HX,476,11,Th.T2,null,half-70);
  cpuHost=Host(HX,494,half,180,200);
  Text(stage,"内存 RAM",rx0,452,16,Th.T1,FontWeights.SemiBold);
  ramUtil=Text(stage,"",HX+HW,444,32,Th.T1,FontWeights.Light,100,TextAlignment.Right);
  ramSpec=Text(stage,"",rx0,476,11,Th.T2,null,half-4);
  ramHost=Host(rx0,494,half-4,170,200);
  pageLine=Text(stage,"",rx0,666,11,Th.T3,null,half-4);
  Hairline(stage,HX,688,HW);

  // Drives (left) and network (right).
  double dw=DriveW,nx=HX+dw+24,nw=HW-dw-24;
  Text(stage,"硬盘",HX,702,16,Th.T1,FontWeights.SemiBold);
  driveHost=Host(HX,728,dw,104,240);
  driveText=Put(stage,new Canvas{Width=dw,Height=20},HX,836);
  ioLine=Text(stage,"",HX,858,11,Th.T3,null,dw);
  Text(stage,"网络",nx,702,16,Th.T1,FontWeights.SemiBold);
  netHost=Host(nx,728,nw,104,160);
  netDown=Text(stage,"",nx,836,12,Th.T2,null,nw,TextAlignment.Center);netUp=Text(stage,"",nx,858,11,Th.T3,null,nw,TextAlignment.Center);
  Canvas.SetLeft(netDown,nx);Canvas.SetLeft(netUp,nx);
  Hairline(stage,HX,886,HW);

  // Trend.
  chartTitle=Text(stage,"",HX,900,12,Th.T3);
  var legend=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};
  foreach(var item in new[]{Tuple.Create((Brush)Th.Accent,"显卡"),Tuple.Create((Brush)Th.Violet,"显存"),Tuple.Create((Brush)Th.T2,"处理器")}){
   legend.Children.Add(new Rectangle{Width=10,Height=2,Fill=item.Item1,Margin=new Thickness(14,0,6,0),VerticalAlignment=VerticalAlignment.Center});
   legend.Children.Add(new TextBlock{Text=item.Item2,FontSize=12,Foreground=Th.T3,FontFamily=Th.Sans});
  }
  Put(stage,new Grid{Width=320,Children={legend}},HX+HW-320,900);
  for(int i=0;i<3;i++)Put(stage,new Rectangle{Width=HW-26,Height=1,Fill=i==2?Th.T4:Th.Hair},HX,926+i*30);
  Text(stage,"100%",HX+HW,920,10,Th.T4,null,30,TextAlignment.Right,Th.Label);
  gpuArea=Put(stage,new Path{Fill=new LinearGradientBrush(Th.C(0x667CC4FF),Th.C(0x047CC4FF),90)},0,0);
  vramLine=Put(stage,new Path{Stroke=Th.Violet,StrokeThickness=1.2,StrokeLineJoin=PenLineJoin.Round,Opacity=.85},0,0);
  cpuLine=Put(stage,new Path{Stroke=Th.T2,StrokeThickness=1.1,StrokeLineJoin=PenLineJoin.Round,Opacity=.8},0,0);
  gpuLine=Put(stage,new Path{Stroke=Th.Accent,StrokeThickness=1.6,StrokeLineJoin=PenLineJoin.Round},0,0);
  chartTicks[0]=Text(stage,"",HX,992,11,Th.T4,null,120);
  chartTicks[1]=Text(stage,"",HX+(HW-26)/2-60,992,11,Th.T4,null,120,TextAlignment.Center);
  chartTicks[2]=Text(stage,"现在",HX+HW-26,992,11,Th.T4,null,80,TextAlignment.Right);
 }

 static string CleanCpu(string s){
  s=Regex.Replace(s,@"\(R\)|\(TM\)|\bCPU\b|@.*$|\bProcessor\b|\d+-Core","");
  return Regex.Replace(s,@"\s+"," ").Trim();
 }
 static string Size(double gb){return gb>=1000?F(gb/1024,1)+" TB":F(gb,0)+" GB";}

 void ApplyHardware(){
  var latest=store.Latest;var meta=J.D(latest,"meta");var cpu=J.D(latest,"cpu");var mem=J.D(latest,"memory");
  var gpu=J.A(latest,"gpus").FirstOrDefault()??new Dictionary<string,object>();
  bool stale=(DateTime.UtcNow-J.Time(J.S(latest,"timestamp"))).TotalSeconds>90;

  string model=J.S(gpu,"name");
  if(model==""){gpuModel.Text="未检测到 NVIDIA 显卡";gpuSpec.Text="";}
  else{
   if(!model.StartsWith("NVIDIA"))model="NVIDIA "+model;
   gpuModel.Text=model;gpuSpec.Text=F(J.N(gpu,"totalMB")/1024,0)+" GB 显存  ·";
   if(gpuFor!=model){
    gpuFor=model;var style=Passive.IsMatch(model)?GpuStyle.Passive:Pro.IsMatch(model)?GpuStyle.Blower:GpuStyle.Twin;
    string logo=model.IndexOf("GeForce",StringComparison.OrdinalIgnoreCase)>=0?"GEFORCE RTX":model.IndexOf("Quadro",StringComparison.OrdinalIgnoreCase)>=0?"QUADRO RTX":style==GpuStyle.Blower?"NVIDIA RTX":"NVIDIA";
    gpuRig=new GpuRig(style,logo,HW,226);Mount(gpuHost,gpuRig.Rig);
   }
  }
  double usage=J.N(gpu,"usage");
  util.Set(usage);vram.Set(J.N(gpu,"vramUsage"));
  fan.Set(J.Has(gpu,"fan")?J.N(gpu,"fan"):20+.8*usage);
  gpuState.Text=stale?"数据延迟":model==""?"":usage>=60?"满负荷工作中":usage>=8?"工作中":"空闲";
  gpuState.Foreground=stale?Th.Amber:usage>=8?Th.Green:Th.T3;
  gpuSpec.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(gpuState,HX+gpuSpec.DesiredSize.Width+6);

  statLabel[0].Text="显存  "+F(J.N(gpu,"usedMB")/1024,1)+" / "+F(J.N(gpu,"totalMB")/1024,0)+" GB";
  Value(statValue[0],F(J.N(gpu,"vramUsage"),0),"%",12);
  Value(statValue[1],J.Has(gpu,"temperature")?F(J.N(gpu,"temperature"),0):"—","°C",12);
  Value(statValue[2],J.Has(gpu,"power")?F(J.N(gpu,"power"),0):"—",J.Has(gpu,"powerLimit")?" / "+F(J.N(gpu,"powerLimit"),0)+" W":" W",12);
  statLabel[3].Text="PCIe"+(J.Has(gpu,"pcieGen")?"  Gen"+J.S(gpu,"pcieGen")+" ×"+J.S(gpu,"pcieWidth"):"");
  if(J.Has(gpu,"pcieRxMB"))Value(statValue[3],"↓"+F(J.N(gpu,"pcieRxMB"),0)+" ↑"+F(J.N(gpu,"pcieTxMB"),0)," MB/s",12);else Value(statValue[3],"—","",12);
  pcieUp.Level=Log01(J.N(gpu,"pcieRxMB"),2000);pcieDown.Level=Log01(J.N(gpu,"pcieTxMB"),2000);

  var loads=J.Nums(cpu,"cores");int threads=Math.Max(loads.Count,(int)J.N(meta,"threads",loads.Count));
  string cpuText=CleanCpu(J.S(meta,"cpuName"));
  cpuName.Text=cpuText+"  ·  "+threads+" 线程";
  if(cpuFor!=threads.ToString()&&threads>0){
   cpuFor=threads.ToString();cpuRig=new CpuRig(threads,(HW-20)/2,180);Mount(cpuHost,cpuRig.Rig);
   coreLoad=Enumerable.Range(0,threads).Select(i=>new Tw()).ToArray();
  }
  for(int i=0;i<coreLoad.Length;i++)coreLoad[i].Set(i<loads.Count?loads[i]:0);
  cpuAll.Set(J.N(cpu,"usage"));
  memRight.Level=memLeft.Level=Math.Min(1,J.N(cpu,"usage")/100*1.4);

  // Memory page map: in use (incl. modified), standby cache and free pages from the memory counters.
  double total=J.N(mem,"totalGB");ram.Set(J.N(mem,"usage"));
  double inUse=J.N(mem,"inUseGB",J.N(mem,"usedGB"))+J.N(mem,"modifiedGB"),standby=J.N(mem,"standbyGB"),free=J.Has(mem,"freeGB")?J.N(mem,"freeGB"):Math.Max(0,total-inUse);
  if(total>0){fracUse.Set(inUse/total);fracStandby.Set(standby/total);}
  ramSpec.Inlines.Clear();
  foreach(var part in new[]{Tuple.Create(Rig.Memory,"使用 ",inUse),Tuple.Create(Standby,"备用 ",standby),Tuple.Create(FreeColor,"可用 ",free)}){
   var b=new SolidColorBrush(part.Item1);b.Freeze();
   ramSpec.Inlines.Add(new Run("● "){Foreground=b});ramSpec.Inlines.Add(new Run(part.Item2+F(part.Item3,1)+"   "));
  }
  ramSpec.Inlines.Add(new Run("GB"));
  double pin=J.N(mem,"pagesInPerSec"),pout=J.N(mem,"pagesOutPerSec");
  pageLine.Text="换页  ↓"+Thousands(pin)+"  ↑"+Thousands(pout)+" 页/秒"+(J.Has(mem,"pageFileSizeGB")?"  ·  页面文件 "+J.S(mem,"pageFileDrive")+" "+F(J.N(mem,"pageFileUsedGB"),1)+" / "+F(J.N(mem,"pageFileSizeGB"),0)+" GB":"");
  pageIn.Level=Log01(pin,3000);pageOut.Level=Log01(pout,3000);
  if(ramFor!=F(total,0)&&total>0){ramFor=F(total,0);ramRig=new RamRig(total,(HW-20)/2-4,170);Mount(ramHost,ramRig.Rig);}

  var disks=J.A(latest,"disks").Take(3).ToList();
  string dk=String.Join(",",disks.Select(d=>J.S(d,"name")).ToArray());
  if(dk!=driveFor&&disks.Count>0){
   driveFor=dk;driveRig=new DriveRig(disks.Count,DriveW,104);Mount(driveHost,driveRig.Rig);
   driveText.Children.Clear();driveLabels.Clear();
   for(int i=0;i<disks.Count;i++)driveLabels.Add(Text(driveText,"",DriveW*(i+.5)/disks.Count-70,0,11,Th.T2,null,140,TextAlignment.Center));
  }
  diskUse=disks.Select(d=>J.N(d,"usage")).ToArray();
  for(int i=0;i<disks.Count&&i<driveLabels.Count;i++){Canvas.SetLeft(driveLabels[i],DriveW*(i+.5)/disks.Count-70);driveLabels[i].Text=J.S(disks[i],"name")+"  "+Size(J.N(disks[i],"freeGB"))+" 可用";}
  if(routerRig==null){routerRig=new RouterRig(HW-DriveW-24,104);Mount(netHost,routerRig.Rig);}

  var io=J.D(latest,"diskIO");var net=J.D(latest,"network");
  diskIo.Set(J.N(io,"readMB")+J.N(io,"writeMB"));rx.Set(J.N(net,"receiveMB"));tx.Set(J.N(net,"sendMB"));
  diskRead.Level=Log01(J.N(io,"readMB"),300);diskWrite.Level=Log01(J.N(io,"writeMB"),300);netIn.Level=Log01(J.N(net,"receiveMB"),50);netOut.Level=Log01(J.N(net,"sendMB"),50);
  ioLine.Text="读取 "+F(J.N(io,"readMB"),1)+" MB/s  ·  写入 "+F(J.N(io,"writeMB"),1)+" MB/s";
  netDown.Text="↓ 下载 "+F(J.N(net,"receiveMB"),2)+" MB/s";netUp.Text="↑ 上传 "+F(J.N(net,"sendMB"),2)+" MB/s";

  if(store.HistoryStamp!=chartStamp){chartStamp=store.HistoryStamp;BuildCharts();}
 }

 // Blink rate rises with throughput; returns 0..1 brightness.
 static double Blink(double mbps,double t,double seed){
  if(mbps<.02)return .08;double hz=Math.Min(9,1.2+2.2*Math.Log10(1+mbps*10));return .25+.75*(Math.Sin(t*hz*2*Math.PI+seed)>0?1:0);
 }

 void TickHardware(double t,double k){
  double u=util.Step(k),v=vram.Step(k),fs=fan.Step(k),ca=cpuAll.Step(k),rm=ram.Step(k),io=diskIo.Step(k),r=rx.Step(k),s=tx.Step(k),fu=fracUse.Step(k),fsb=fracStandby.Step(k);
  Value(gpuUtil,F(u,0),"%",22);Value(cpuUtil,F(ca,0),"%",16);Value(ramUtil,F(rm,0),"%",16);
  for(int i=0;i<coreLoad.Length;i++)coreLoad[i].Step(k);
  bool hw3d=Slice(0);double fdt=lastFan<0?0:t-lastFan;if(hw3d)lastFan=t;
  if(cpuRig!=null&&hw3d)for(int i=0;i<cpuRig.Columns.Length&&i<coreLoad.Length;i++)cpuRig.Columns[i].ScaleY=.06+.94*Math.Min(1,coreLoad[i].V/100);
  if(!Off("links"))TickLinks(t);
  // Colours are rewritten at ~15 Hz; the texture upload is the only per-update cost.
  bool paint=hw3d&&(snapped||t-lastRig>=1/15.0);if(paint)lastRig=t;
  if(gpuRig!=null&&hw3d){
   double speed=(40+6.2*Math.Max(0,Math.Min(100,fs)))*gpuRig.FanSpeedScale;
   if(Lite("slice"))speed=Math.Min(speed,90); // few redraws per second: keep each step small
   for(int i=0;i<gpuRig.Fans.Count;i++){gpuRig.Fans[i].Angle=(gpuRig.Fans[i].Angle+speed*fdt*(i%2==0?1:-1)+360)%360;gpuRig.Blur[i].Color=Color.FromArgb((byte)(20+110*Math.Min(1,fs/100)),90,98,112);}
   if(paint){
    gpuRig.Rig.Set(gpuRig.Glow,Rig.Scale(Rig.Heat(u/100),.9+.1*Math.Sin(t*2.2)));
    gpuRig.Logo.Color=Rig.Scale(Rig.Heat(.15+.85*u/100),1.3);
    Fill(gpuRig.Rig,gpuRig.Vram,v/100,Rig.Memory);
   }
   gpuRig.Rig.Tick(t);
  }
  if(cpuRig!=null&&hw3d){
   if(paint)for(int i=0;i<cpuRig.Slots.Length&&i<coreLoad.Length;i++)cpuRig.Rig.Set(cpuRig.Slots[i],Rig.Heat(coreLoad[i].V/100));
   cpuRig.Rig.Tick(t);
  }
  if(ramRig!=null&&hw3d){
   if(paint){int per=Math.Max(1,ramRig.Chips.Length/8);PageMap(ramRig.Rig,ramRig.Bars,fu,fsb,ramRig.Bars.Length/per);PageMap(ramRig.Rig,ramRig.Chips,fu,fsb,ramRig.Chips.Length);}
   ramRig.Rig.Tick(t);
  }
  if(driveRig!=null&&hw3d){
   if(paint)for(int i=0;i<driveRig.Gauges.Count;i++){
    Fill(driveRig.Rig,driveRig.Gauges[i],i<diskUse.Length?diskUse[i]/100:0,Rig.Used);
    driveRig.Rig.Set(driveRig.Leds[i],Rig.Mix(Rig.Off,Rig.Led,Blink(io/driveRig.Leds.Count,t,i*1.7)));
   }
   driveRig.Rig.Tick(t);
  }
  if(routerRig!=null&&hw3d){
   if(paint){routerRig.Rig.Set(routerRig.Power,Rig.Scale(Rig.Led,.7));routerRig.Rig.Set(routerRig.Down,Rig.Mix(Rig.Off,Rig.Used,Blink(r,t,0)));routerRig.Rig.Set(routerRig.Up,Rig.Mix(Rig.Off,Rig.Memory,Blink(s,t,1.3)));}
   routerRig.Rig.Tick(t);
  }
 }

 // Memory page map: parts coloured in order as in-use, standby cache, then free; `group` parts form one map.
 static void PageMap(Rig rig,int[] slots,double use,double standby,int group){
  for(int i=0;i<slots.Length;i++){double p=((i%group)+.5)/group;rig.Set(slots[i],p<use?Rig.Scale(Rig.Memory,.75+.25*p):p<use+standby?Standby:Rig.Off);}
 }

 // Parts fill in order like a level meter; with `group` > 0 each group of parts fills to the same level.
 static void Fill(Rig rig,int[] slots,double fraction,Color color,int group=0){
  int n=group>0?group:slots.Length;double lit=fraction*n;
  for(int i=0;i<slots.Length;i++){int j=group>0?i%group:i;double on=Math.Max(0,Math.Min(1,lit-j));rig.Set(slots[i],Rig.Mix(Rig.Off,Rig.Scale(color,.7+.3*(j+1.0)/n),on));}
 }

 // History: GPU area + VRAM + CPU over an adaptive window (1-24 h).
 void BuildCharts(){
  var rows=store.History;var now=DateTime.UtcNow;
  DateTime first=rows.Count>0?(DateTime)rows[0]["_t"]:now;double spanH=(now-first).TotalHours;
  double hours=new[]{1.0,3,6,12,24}.FirstOrDefault(hh=>hh>=spanH-.02);if(hours==0)hours=24;
  chartTitle.Text="过去 "+hours+" 小时";
  var start=now.AddHours(-hours);
  chartTicks[0].Text=start.ToLocalTime().ToString(hours>=24?"MM-dd HH:mm":"HH:mm");
  chartTicks[1].Text=start.AddHours(hours/2).ToLocalTime().ToString("HH:mm");
  var area=new Rect(HX,926,HW-26,60);
  gpuArea.Data=Series(rows,"usage",start,now,area,true);gpuLine.Data=Series(rows,"usage",start,now,area,false);
  vramLine.Data=Series(rows,"vramUsage",start,now,area,false);cpuLine.Data=Series(rows,"cpu",start,now,area,false);
 }

 static Geometry Series(List<Dictionary<string,object>> rows,string key,DateTime start,DateTime end,Rect r,bool fill){
  double span=(end-start).TotalSeconds;int buckets=(int)Math.Max(20,Math.Min(240,span/60));double bw=span/buckets;
  var sum=new double[buckets];var cnt=new int[buckets];
  foreach(var d in rows){
   object tv;if(!d.TryGetValue("_t",out tv)||!d.ContainsKey(key))continue;
   double s=((DateTime)tv-start).TotalSeconds;if(s<0||s>span)continue;int b=Math.Min(buckets-1,(int)(s/bw));
   sum[b]+=Math.Max(0,Math.Min(100,J.N(d,key)));cnt[b]++;
  }
  var g=new StreamGeometry();
  using(var c=g.Open()){
   var run=new List<Point>();int lastB=-100;int maxGap=Math.Max(2,(int)Math.Ceiling(150/bw));
   Action flush=()=>{
    if(run.Count>=2){
     if(fill){c.BeginFigure(new Point(run[0].X,r.Bottom),true,true);c.PolyLineTo(run,false,true);c.LineTo(new Point(run[run.Count-1].X,r.Bottom),false,true);}
     else{c.BeginFigure(run[0],false,false);c.PolyLineTo(run.Skip(1).ToList(),true,true);}
    }
    run.Clear();
   };
   for(int b=0;b<buckets;b++){
    if(cnt[b]==0)continue;
    if(b-lastB>maxGap)flush();
    run.Add(new Point(r.X+(b+.5)/buckets*r.Width,r.Bottom-sum[b]/cnt[b]/100*r.Height));lastB=b;
   }
   flush();
  }
  g.Freeze();return g;
 }
}
