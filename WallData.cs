using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

// Loose JSON helpers over JavaScriptSerializer output (Dictionary / ArrayList / primitives).
static class J {
 public static Dictionary<string,object> Parse(string s){return new JavaScriptSerializer{MaxJsonLength=64*1024*1024}.Deserialize<Dictionary<string,object>>(s);}
 public static Dictionary<string,object> Obj(object v){return v as Dictionary<string,object> ?? new Dictionary<string,object>();}
 public static object Get(Dictionary<string,object> d,string k){object v;return d!=null && d.TryGetValue(k,out v)?v:null;}
 public static bool Has(Dictionary<string,object> d,string k){return Get(d,k)!=null;}
 public static Dictionary<string,object> D(Dictionary<string,object> d,string k){return Obj(Get(d,k));}
 public static List<Dictionary<string,object>> A(Dictionary<string,object> d,string k){var v=Get(d,k) as IEnumerable;if(v==null||v is string)return new List<Dictionary<string,object>>();return v.Cast<object>().Select(Obj).ToList();}
 public static List<double> Nums(Dictionary<string,object> d,string k){var v=Get(d,k) as IEnumerable;var r=new List<double>();if(v==null||v is string)return r;foreach(object o in v){try{r.Add(Convert.ToDouble(o,CultureInfo.InvariantCulture));}catch{}}return r;}
 public static double N(Dictionary<string,object> d,string k,double fallback=0){object v=Get(d,k);if(v==null)return fallback;try{return Convert.ToDouble(v,CultureInfo.InvariantCulture);}catch{return fallback;}}
 public static string S(Dictionary<string,object> d,string k,string fallback=""){object v=Get(d,k);return v==null?fallback:Convert.ToString(v,CultureInfo.InvariantCulture);}
 public static DateTime Time(string s){DateTime d;return DateTime.TryParse(s,CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind,out d)?d.ToUniversalTime():DateTime.MinValue;}
}

// Reads the files the collector and refresh-map tasks write. All loads are tolerant of partial or missing files.
class Store {
 public readonly string Root;
 public volatile Dictionary<string,object> Config=new Dictionary<string,object>();
 public volatile Dictionary<string,object> Latest=new Dictionary<string,object>();
 public volatile Dictionary<string,object> Outbreaks=new Dictionary<string,object>();
 public volatile Dictionary<string,object> World=new Dictionary<string,object>();
 public volatile Dictionary<string,object> Feeds=new Dictionary<string,object>();
 public volatile Dictionary<string,object> Transmission=new Dictionary<string,object>();
 public volatile Dictionary<string,object> ComputeNodes=new Dictionary<string,object>();
 public volatile Dictionary<string,object> ComputeQueue=new Dictionary<string,object>();
 public volatile List<Dictionary<string,object>> History=new List<Dictionary<string,object>>();
 public long OutbreakStamp,HistoryStamp,LatestStamp,TransmissionStamp;
 DateTime historyRead=DateTime.MinValue,outbreaksRead=DateTime.MinValue;

 public Store(string root){Root=root;}
 public string PathOf(params string[] parts){return Path.Combine(new[]{Root}.Concat(parts).ToArray());}

 static string ReadShared(string path){using(var f=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete))using(var r=new StreamReader(f,System.Text.Encoding.UTF8))return r.ReadToEnd();}

 public void LoadAll(){
  try{Config=J.Parse(ReadShared(PathOf("config.json")));}catch{}
  try{Feeds=J.Parse(ReadShared(PathOf("feeds.json")));}catch{}
  try{World=J.Parse(ReadShared(PathOf("science","world.json")));}catch{}
  Poll(true);
 }

 // Called from a worker thread every few seconds.
 public void Poll(bool force=false){
  try{ComputeNodes=J.Parse(ReadShared(PathOf("data","compute-nodes.json")));}catch{}
  try{ComputeQueue=J.Parse(ReadShared(PathOf("data","compute-queue.json")));}catch{}
  try{Latest=J.Parse(ReadShared(PathOf("data","latest.json")));LatestStamp++;}catch{}
  var now=DateTime.UtcNow;
  if(force||(now-historyRead).TotalSeconds>=30){LoadHistory();historyRead=now;}
  if(force||(now-outbreaksRead).TotalSeconds>=60){
   try{string p=PathOf("science","outbreaks.json");long stamp=File.GetLastWriteTimeUtc(p).Ticks;if(stamp!=OutbreakStamp){Outbreaks=J.Parse(ReadShared(p));OutbreakStamp=stamp;}}catch{}
   try{string p=PathOf("science","transmission.json");long stamp=File.GetLastWriteTimeUtc(p).Ticks;if(stamp!=TransmissionStamp){Transmission=J.Parse(ReadShared(p));TransmissionStamp=stamp;}}catch{}
   outbreaksRead=now;
  }
 }

 // Only the last ~25 hours are drawn, so only today's and yesterday's files are parsed.
 void LoadHistory(){
  try{
   var rows=new List<Dictionary<string,object>>();var cutoff=DateTime.UtcNow.AddHours(-25);var ser=new JavaScriptSerializer();
   var files=Directory.GetFiles(PathOf("data"),"history-*.jsonl").OrderBy(x=>x).ToList();
   foreach(string p in files.Skip(Math.Max(0,files.Count-2))){
    using(var f=new FileStream(p,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))using(var r=new StreamReader(f)){
     string line;while((line=r.ReadLine())!=null){
      if(line.Length<10)continue;
      try{var d=ser.Deserialize<Dictionary<string,object>>(line);var t=J.Time(J.S(d,"t"));if(t<cutoff)continue;d["_t"]=t;rows.Add(d);}catch{}
     }
    }
   }
   History=rows;HistoryStamp++;
  }catch{}
 }

 public List<Dictionary<string,object>> RecentEvents(){
  // Hard 90-day window (CEO, 2026-11-05): no exception for notices still marked active.
  return J.A(Outbreaks,"events").Where(e=>J.Time(J.S(e,"date"))>DateTime.UtcNow.AddDays(-90)).OrderByDescending(e=>J.Time(J.S(e,"date"))).ToList();
 }

 public Dictionary<string,object> Route(string disease){
  var all=J.A(Transmission,"diseases");
  return all.FirstOrDefault(r=>J.S(r,"name")==disease)??all.FirstOrDefault(r=>J.S(r,"name")!=""&&disease.Contains(J.S(r,"name")));
 }
 public string StructureFile(string role,string fallback){
  foreach(var s in J.A(Feeds,"structures"))if(J.S(s,"role")==role)return PathOf("science",Path.GetFileName(J.S(s,"file",fallback)));
  return PathOf("science",fallback);
 }
}

static class Names {
 static readonly Dictionary<string,string> zh=new Dictionary<string,string>();
 static Names(){
  const string table="AFG阿富汗|AGO安哥拉|ALB阿尔巴尼亚|ARE阿联酋|ARG阿根廷|AUS澳大利亚|AUT奥地利|BDI布隆迪|BEL比利时|BEN贝宁|BFA布基纳法索|BGD孟加拉国|BGR保加利亚|BOL玻利维亚|BRA巴西|BWA博茨瓦纳|CAF中非|CAN加拿大|CHE瑞士|CHL智利|CHN中国|CIV科特迪瓦|CMR喀麦隆|COD刚果（金）|COG刚果（布）|COL哥伦比亚|CRI哥斯达黎加|CUB古巴|CZE捷克|DEU德国|DJI吉布提|DNK丹麦|DOM多米尼加|DZA阿尔及利亚|ECU厄瓜多尔|EGY埃及|ERI厄立特里亚|ESP西班牙|ETH埃塞俄比亚|FIN芬兰|FRA法国|GAB加蓬|GBR英国|GHA加纳|GIN几内亚|GMB冈比亚|GNB几内亚比绍|GRC希腊|GTM危地马拉|HND洪都拉斯|HRV克罗地亚|HTI海地|HUN匈牙利|IDN印度尼西亚|IND印度|IRN伊朗|IRQ伊拉克|ISR以色列|ITA意大利|JPN日本|KEN肯尼亚|KHM柬埔寨|KOR韩国|LAO老挝|LBN黎巴嫩|LBR利比里亚|LKA斯里兰卡|MAR摩洛哥|MDG马达加斯加|MEX墨西哥|MLI马里|MMR缅甸|MOZ莫桑比克|MRT毛里塔尼亚|MWI马拉维|MYS马来西亚|NAM纳米比亚|NER尼日尔|NGA尼日利亚|NIC尼加拉瓜|NLD荷兰|NPL尼泊尔|PAK巴基斯坦|PAN巴拿马|PER秘鲁|PHL菲律宾|PNG巴布亚新几内亚|POL波兰|PRT葡萄牙|PRY巴拉圭|ROU罗马尼亚|RUS俄罗斯|RWA卢旺达|SAU沙特阿拉伯|SDN苏丹|SDS南苏丹|SSD南苏丹|SEN塞内加尔|SLE塞拉利昂|SLV萨尔瓦多|SOM索马里|SRB塞尔维亚|SWE瑞典|SYR叙利亚|TCD乍得|TGO多哥|THA泰国|TUR土耳其|TZA坦桑尼亚|UGA乌干达|UKR乌克兰|URY乌拉圭|USA美国|VEN委内瑞拉|VNM越南|YEM也门|ZAF南非|ZMB赞比亚|ZWE津巴布韦";
  foreach(string item in table.Split('|'))zh[item.Substring(0,3)]=item.Substring(3);
 }
 public static string Country(Dictionary<string,object> e){
  string name=J.S(e,"country"),iso=J.S(e,"iso");
  if(name.Any(c=>c>=0x4E00&&c<=0x9FFF))return name;
  string z;return zh.TryGetValue(iso,out z)?z:name;
 }
}
