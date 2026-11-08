using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

// Cluster panel, two columns: nodes on the left, scheduler queue on the right.
// Data comes from data\compute-nodes.json (collector, ~10 s) and data\compute-queue.json (queue poller, ~1 min).
// The renderer never talks to the scheduler itself; a missing or stale file simply shows "没有数据".
partial class WallView {
 const int CRows=8,CQueue=8;
 readonly TextBlock[] ccName=new TextBlock[CRows],ccMeta=new TextBlock[CRows],ccUtil=new TextBlock[CRows];
 readonly Rectangle[] ccFill=new Rectangle[CRows],ccTrack=new Rectangle[CRows];
 readonly Ellipse[] ccDot=new Ellipse[CRows];
 readonly Ellipse[] cqDot=new Ellipse[CQueue];
 readonly TextBlock[] cqName=new TextBlock[CQueue],cqMeta=new TextBlock[CQueue];
 TextBlock ccSub,ccQHead,ccQEmpty,ccFoot;

 void BuildCompute(double x,double y,double w,double h){
  Card(stage,x,y,w,h);
  Text(stage,"集群算力",x+28,y+18,24,Th.T1,FontWeights.SemiBold);
  ccSub=Text(stage,"",x+28,y+54,13,Th.T2,null,w-56);
  double lr=x+356;
  for(int i=0;i<CRows;i++){
   double ry=y+100+i*60;
   ccDot[i]=Put(stage,new Ellipse{Width=7,Height=7,Fill=Th.T4},x+22,ry+5);
   ccName[i]=Text(stage,"",x+38,ry,14.5,Th.T1,FontWeights.SemiBold,92);
   ccTrack[i]=Put(stage,new Rectangle{Width=150,Height=7,RadiusX=3.5,RadiusY=3.5,Fill=Th.Cell},x+138,ry+5);
   ccFill[i]=Put(stage,new Rectangle{Width=0,Height=7,RadiusX=3.5,RadiusY=3.5,Fill=Th.Accent},x+138,ry+5);
   ccUtil[i]=Text(stage,"",lr,ry,13,Th.T2,null,46,TextAlignment.Right);
   ccMeta[i]=Text(stage,"",x+138,ry+22,11.5,Th.T3,null,lr-x-138);
  }
  Put(stage,new Rectangle{Width=1,Height=h-96,Fill=Th.Hair},x+372,y+80);
  double rx=x+400;
  ccQHead=Text(stage,"",rx,y+100,13,Th.T3);
  ccFoot=Text(stage,"",x+28,y+404,12,Th.T3,null,w-56);
  ccQEmpty=Text(stage,"近期没有排队或运行的作业",rx+16,y+126,12,Th.T3);
  for(int i=0;i<CQueue;i++){
   double qy=y+132+i*34;
   cqDot[i]=Put(stage,new Ellipse{Width=6,Height=6,Fill=Th.T4},rx,qy+4);
   cqName[i]=Text(stage,"",rx+16,qy-1,12.5,Th.T2,null,w-400-150);
   cqMeta[i]=Text(stage,"",x+w-28,qy-1,12,Th.T3,null,118,TextAlignment.Right);
  }
 }

 static string Short(string s,int n){return s.Length<=n?s:s.Substring(0,n)+"…";}
 static string NodeShort(string label){int i=label.IndexOf('（');return i>0?label.Substring(0,i):label;}
 static string NodeName(string id,string label){if(id=="master")return "RTX3000";return NodeShort(label);}
 static string Wait(double minutes){
  if(minutes<1)return "刚提交";
  if(minutes<60)return "等 "+((int)minutes).ToString(CultureInfo.InvariantCulture)+" 分";
  if(minutes<48*60)return "等 "+F(minutes/60,0)+" 小时";
  return "等 "+F(minutes/1440,0)+" 天";
 }

 // h800 / h800b / h800c are three containers on one physical host: one 8-GPU row.
 List<Dictionary<string,object>> ComputeRows(){
  var nodes=J.A(store.ComputeNodes,"nodes");
  var merged=new List<Dictionary<string,object>>();
  Dictionary<string,object> h8=null;
  foreach(var n in nodes){
   string nid=J.S(n,"id");
   if(!nid.StartsWith("h800",StringComparison.Ordinal)){
    n["name"]=NodeName(nid,J.S(n,"label"));merged.Add(n);continue;
   }
   double gd=J.N(n,"gpus");
   if(h8==null){
    h8=new Dictionary<string,object>{{"name","H800"},{"state",J.S(n,"state")},{"gpus",0.0},{"_util",0.0},{"cpuUsed",0.0},{"cpuCores",0.0},{"memUsedGB",0.0},{"memTotalGB",0.0}};
    merged.Add(h8);
   }
   if(J.S(n,"state")=="online")h8["state"]="online";
   h8["gpus"]=Convert.ToDouble(h8["gpus"])+gd;
   h8["_util"]=Convert.ToDouble(h8["_util"])+J.N(n,"util")*gd;
   h8["cpuUsed"]=Convert.ToDouble(h8["cpuUsed"])+J.N(n,"cpuUsed");
   h8["cpuCores"]=Convert.ToDouble(h8["cpuCores"])+J.N(n,"cpuCores");
   h8["memUsedGB"]=Convert.ToDouble(h8["memUsedGB"])+J.N(n,"memUsedGB");
   h8["memTotalGB"]=Convert.ToDouble(h8["memTotalGB"])+J.N(n,"memTotalGB");
  }
  if(h8!=null){double g=Convert.ToDouble(h8["gpus"]);h8["util"]=g>0?Convert.ToDouble(h8["_util"])/g:0.0;}
  return merged;
 }

 void ApplyCompute(){
  var d=store.ComputeNodes;
  var nodes=ComputeRows();
  var at=J.Time(J.S(d,"at"));
  bool stale=at==DateTime.MinValue||(DateTime.UtcNow-at).TotalMinutes>4;
  if(stale)ccSub.Text="调度器没有数据 · 显示为最后已知值";
  else ccSub.Text=((int)J.N(d,"online")).ToString(CultureInfo.InvariantCulture)+" / "+((int)J.N(d,"total")).ToString(CultureInfo.InvariantCulture)+" 节点在线 · 空闲卡 "+((int)J.N(d,"freeGpus")).ToString(CultureInfo.InvariantCulture)+" / "+((int)J.N(d,"gpuTotal")).ToString(CultureInfo.InvariantCulture)+" · v"+J.S(d,"version");
  for(int i=0;i<CRows;i++){
   bool has=i<nodes.Count;
   var vis=has?Visibility.Visible:Visibility.Collapsed;
   ccName[i].Visibility=ccMeta[i].Visibility=ccUtil[i].Visibility=vis;
   ccTrack[i].Visibility=ccFill[i].Visibility=ccDot[i].Visibility=vis;
   if(!has)continue;
   var n=nodes[i];
   ccName[i].Text=J.S(n,"name");
   string st=J.S(n,"state");
   ccDot[i].Fill=st=="online"?Th.Green:st=="degraded"?Th.Amber:Th.Warm;
   double util=J.N(n,"util");
   var fill=new SolidColorBrush(Rig.Heat(util/100));fill.Freeze();ccFill[i].Fill=fill;
   ccFill[i].Width=Math.Max(0,Math.Min(150,150*util/100));
   ccUtil[i].Text=util.ToString("F0",CultureInfo.InvariantCulture)+"%";
   string pre=J.N(n,"gpus")>1?(J.N(n,"gpus").ToString("F0",CultureInfo.InvariantCulture)+" 卡 · "):"";
   ccMeta[i].Text=pre+"CPU "+F(J.N(n,"cpuUsed"),1)+" / "+F(J.N(n,"cpuCores"),0)+" · 内存 "+Thousands(J.N(n,"memUsedGB"))+" / "+Thousands(J.N(n,"memTotalGB"))+" GB";
  }
  var q=store.ComputeQueue;
  var jobs=J.A(q,"jobs");
  double longest=0;foreach(var jb in jobs)longest=Math.Max(longest,J.N(jb,"waitMin"));
  ccFoot.Text="空闲卡 "+((int)J.N(d,"freeGpus")).ToString(CultureInfo.InvariantCulture)+" / "+((int)J.N(d,"gpuTotal")).ToString(CultureInfo.InvariantCulture)+" · 最久等待 "+F(longest/1440,1)+" 天 · v"+J.S(d,"version");
  int queued=(int)J.N(q,"queued"),running=(int)J.N(q,"running");
  ccQHead.Text="运行中 "+running.ToString(CultureInfo.InvariantCulture)+" 个 · 排队 "+queued.ToString(CultureInfo.InvariantCulture)+" 个";
  ccQEmpty.Visibility=jobs.Count==0?Visibility.Visible:Visibility.Collapsed;
  for(int i=0;i<CQueue;i++){
   bool has=i<jobs.Count;
   var vis=has?Visibility.Visible:Visibility.Collapsed;
   cqDot[i].Visibility=cqName[i].Visibility=cqMeta[i].Visibility=vis;
   if(!has)continue;
   var job=jobs[i];
   string state=J.S(job,"status");
   cqDot[i].Fill=state=="running"?Th.Accent:Th.Amber;
   cqName[i].Text=Short(J.S(job,"project")+" · "+J.S(job,"name"),44);
   double waitMin=J.N(job,"waitMin");
   cqMeta[i].Text=state=="running"?"运行 "+F(waitMin/60,0)+" 小时":Wait(waitMin);
  }
 }

 // Nothing animates here; values change when the poller refreshes them.
 void TickCompute(double t){}
}
