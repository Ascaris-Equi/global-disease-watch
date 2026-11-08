using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;

// disease 场景的七语文案表（zh / en / ja / es / fr / de / ru）。
// 要改词只动这一张表，别再到 WallCards.cs / WallView.cs 里散着写中文。
// 取词顺序：config.json 的 "lang" → 系统 UI 区域 → 不支持就回落 en；表里缺 key 也回落英文。
// full 场景不走这里，保持原来的中文版式。
static class Lang {
 static readonly string[] Codes={"zh","en","ja","es","fr","de","ru"};
 static int ix=1;                            // 当前语言在 Codes 里的下标，默认英文
 public static string Code="en";
 // 只有 disease 场景调用 Init；full 场景保持未初始化，病种/国名映射一律不生效。
 static bool Ready;
 static readonly Dictionary<string,string[]> Rows=new Dictionary<string,string[]>(StringComparer.Ordinal);
 static readonly Dictionary<string,string> Dis=new Dictionary<string,string>(StringComparer.Ordinal);
 static readonly Dictionary<string,string> CountryShort=new Dictionary<string,string>(StringComparer.Ordinal);
 // 数字按语言分组，但不受机器区域设置影响：中文/英文/日文逗号，西语/德语点号，法语/俄语不换行空格。
 static readonly string[] Group={",",",",",",".","\u00A0",".","\u00A0"};
 static readonly string[] Point={".",".",".",",",",",",",","};
 static readonly bool[] PctSpace={false,false,false,true,true,true,false};
 public static FontFamily Sans,Mono;

 static Lang(){
  // —— 遮幅 ——
  Add("title.global",
   "全球传染病监测","GLOBAL DISEASE WATCH","世界の感染症監視","VIGILANCIA EPIDEMIOLÓGICA","VEILLE ÉPIDÉMIOLOGIQUE","GLOBALE SEUCHENÜBERWACHUNG","ГЛОБАЛЬНЫЙ МОНИТОРИНГ");
  Add("footer.source","SOURCE","SOURCE","情報源","FUENTE","SOURCE","QUELLE","ИСТОЧНИК");
  Add("footer.sync","SYNC","SYNC","同期","SINCRO","SYNCHRO","SYNC","СИНХРО");
  Add("footer.lag","LAG {0}D","LAG {0}D","遅延 {0}D","RETRASO {0}D","RETARD {0}J","VERZUG {0}T","ЗАДЕРЖКА {0}Д");
  Add("footer.lagNone","LAG —","LAG —","遅延 —","RETRASO —","RETARD —","VERZUG —","ЗАДЕРЖКА —");

  // —— 监测焦点 ——
  Add("hero.kicker","监测焦点","FOCUS","監視対象","FOCO","FOCUS","FOKUS","ФОКУС");
  Add("metric.deaths","死亡","DEATHS","死亡","MUERTES","DÉCÈS","TODESFÄLLE","СМЕРТИ");
  Add("metric.cases","报告病例","REPORTED CASES","報告症例","CASOS","CAS","FÄLLE","СЛУЧАИ");
  Add("official","官方通报","OFFICIAL","公式発表","OFICIAL","OFFICIEL","OFFIZIELL","ОФИЦИАЛЬНО");
  Add("unit.deaths","例","","例","","","","");
  Add("unit.cases","例","","例","","","","");
  Add("metric.cfr","病死率","FATALITY","致死率","LETALIDAD","LÉTALITÉ","LETALITÄT","ЛЕТАЛЬНОСТЬ");
  Add("attention","关注度","ATTENTION","注目度","ATENCIÓN","ATTENTION","BEACHTUNG","ВНИМАНИЕ");
  Add("attention.note","估算指标 · 非官方评级","ESTIMATE · NOT OFFICIAL","推定値 · 非公式","ESTIMACIÓN · NO OFICIAL","ESTIMATION · NON OFFICIEL","SCHÄTZUNG · INOFFIZIELL","ОЦЕНКА · НЕ ОФИЦИАЛЬНО");
  Add("key.reportDate","通报日期","REPORTED","報告日","NOTIFICADO","NOTIFIÉ","GEMELDET","ОТЧЁТ");
  Add("key.asOf","数据截至","AS OF","基準日","DATOS AL","DONNÉES AU","STAND","ДАННЫЕ НА");
  Add("key.source","数据源","SOURCE","情報源","FUENTE","SOURCE","QUELLE","ИСТОЧНИК");
  Add("note.arcs","弧线为同病种关联示意，非溯源结论",
   "ARCS ARE SAME-DISEASE LINKS, NOT TRACE RESULTS",
   "弧線は同一疾患の関連を示すもので、感染経路の断定ではありません",
   "LOS ARCOS INDICAN VÍNCULOS POR ENFERMEDAD, NO TRAZABILIDAD",
   "LES ARCS INDIQUENT DES LIENS PAR MALADIE, PAS UNE TRAÇABILITÉ",
   "BÖGEN ZEIGEN GLEICHE KRANKHEIT, KEINE RÜCKVERFOLGUNG",
   "ДУГИ — СВЯЗЬ ПО БОЛЕЗНИ, НЕ ТРАССИРОВКА");
  Add("empty.reports","近期通报暂不可用","NO RECENT REPORTS","最近の報告なし","SIN DATOS RECIENTES","AUCUN RAPPORT RÉCENT","KEINE NEUEN MELDUNGEN","НЕТ СВЕЖИХ ДАННЫХ");
  Add("cdc.level","CDC {0} 级","CDC LEVEL {0}","CDC {0} 級","CDC NIVEL {0}","CDC NIVEAU {0}","CDC STUFE {0}","CDC УРОВЕНЬ {0}");

  // —— 关注度排行 ——
  Add("rank.title","关注度排行","ATTENTION RANKING","注目度ランキング","RANKING DE ATENCIÓN","CLASSEMENT PAR ATTENTION","AUFMERKSAMKEIT-RANG","РЕЙТИНГ ВНИМАНИЯ");
  Add("rank.sub","按本屏估算排序","BY ON-SCREEN ESTIMATE","本画面の推定順","POR ESTIMACIÓN EN PANTALLA","PAR ESTIMATION ÉCRAN","NACH SCHÄTZUNG","ПО ОЦЕНКЕ ЭКРАНА");
  Add("rank.foot","{0} REPORTS / {1} 国  ·  最近通报 {2}  ·  LAG {3}D",
   "{0} REPORTS / {1} COUNTRIES  ·  LATEST {2}  ·  LAG {3}D",
   "{0} 件 / {1} か国  ·  最新 {2}  ·  遅延 {3}D",
   "{0} INFORMES / {1} PAÍSES  ·  ÚLTIMO {2}  ·  RETRASO {3}D",
   "{0} RAPPORTS / {1} PAYS  ·  DERNIER {2}  ·  RETARD {3}J",
   "{0} MELDUNGEN / {1} LÄNDER  ·  NEUESTE {2}  ·  VERZUG {3}T",
   "{0} СООБЩЕНИЙ / {1} СТРАН  ·  ПОСЛЕДНЕЕ {2}  ·  ЗАДЕРЖКА {3}Д");
  Add("rank.none","NO REPORTS","NO REPORTS","報告なし","SIN INFORMES","AUCUN RAPPORT","KEINE MELDUNGEN","НЕТ СООБЩЕНИЙ");
  Add("col.country","国家","COUNTRY","国","PAÍS","PAYS","LAND","СТРАНА");
  Add("col.disease","病种","DISEASE","疾患","ENFERMEDAD","MALADIE","KRANKHEIT","БОЛЕЗНЬ");
  Add("col.cases","病例","CASES","症例","CASOS","CAS","FÄLLE","СЛУЧАИ");
  Add("col.deaths","死亡","DEATHS","死亡","MUERTES","DÉCÈS","TODESFÄLLE","СМЕРТИ");
  Add("col.date","通报","REPORT","報告","FECHA","DATE","MELDUNG","ОТЧЁТ");
  Add("col.attention","关注","ATTN.","注目","ATEN.","ATT.","FOKUS","ВНИМ.");

  // —— 等级 ——
  Add("level.low","低","LOW","低","BAJA","BASSE","NIEDRIG","НИЗК.");
  Add("level.mid","中","MID","中","MEDIA","MOYENNE","MITTEL","СРЕДН.");
  Add("level.high","高","HIGH","高","ALTA","HAUTE","HOCH","ВЫСОК.");

  // —— 地图图例 ——
  Add("legend.note","通报国 · 本屏关注度","REPORTING COUNTRIES · ON-SCREEN ATTENTION","報告国・本画面の注目度","PAÍSES NOTIFICANTES · ATENCIÓN EN PANTALLA","PAYS NOTIFIANTS · ATTENTION ÉCRAN","MELDUNGSLÄNDER · AUFMERKSAMKEIT","СТРАНЫ · ВНИМАНИЕ ЭКРАНА");

  // —— 传播链（WallChain.cs 不动，这里做盖章式覆盖）——
  Add("chain.title","传播链 / TRANSMISSION","TRANSMISSION CHAIN","感染経路 / TRANSMISSION","CADENA DE TRANSMISIÓN","CHAÎNE DE TRANSMISSION","ÜBERTRAGUNGSKETTE","ЦЕПОЧКА ПЕРЕДАЧИ");
  Add("chain.p2p","人传人","HUMAN-TO-HUMAN","人から人","PERSONA A PERSONA","D'HOMME À HOMME","MENSCH ZU MENSCH","МЕЖДУ ЛЮДЬМИ");
  Add("host.bat","蝙蝠","BATS","コウモリ","MURCIÉLAGO","CHAUVE-SOURIS","FLEDERMAUS","РУКОКРЫЛЫЕ");
  Add("host.rodent","鼠类","RODENTS","げっ歯類","ROEDORES","RONGEURS","NAGER","ГРЫЗУНЫ");
  Add("host.bird","禽鸟","BIRDS","鳥類","AVES","OISEAUX","VÖGEL","ПТИЦЫ");
  Add("host.livestock","牲畜","LIVESTOCK","家畜","GANADO","BÉTAIL","VIEH","СКОТ");
  Add("host.water","水与食物","WATER & FOOD","水・食品","AGUA/COMIDA","EAU/ALIMENTS","WASSER/ESSEN","ВОДА И ЕДА");
  Add("host.primate","猴类","PRIMATES","サル類","PRIMATES","PRIMATES","PRIMATEN","ПРИМАТЫ");
  Add("host.pig","猪","PIGS","ブタ","CERDOS","PORCS","SCHWEINE","СВИНЬИ");
  Add("host.mosquito","蚊虫","MOSQUITOES","蚊","MOSQUITOS","MOUSTIQUES","MÜCKEN","КОМАРЫ");
  Add("host.tick","蜱虫","TICKS","マダニ","GARRAPATAS","TIQUES","ZECKEN","КЛЕЩИ");
  Add("host.human","人","HUMAN","人","HUMANO","HUMAIN","MENSCH","ЧЕЛОВЕК");

  // —— 病种名 ——
  // science/transmission.json 和 outages 里存的是中文规范名，传播链 chips 与链底病种名直接拿它显示。
  // 只有英文界面需要译文；其余语言维持原样（数据里没有对应译文，不在这里编）。
  Dis["埃博拉"]="Ebola";Dis["马尔堡病"]="Marburg";Dis["猴痘"]="Mpox";Dis["霍乱"]="Cholera";
  Dis["登革热"]="Dengue";Dis["基孔肯雅热"]="Chikungunya";Dis["寨卡"]="Zika";Dis["黄热病"]="Yellow fever";
  Dis["疟疾"]="Malaria";Dis["西尼罗热"]="West Nile fever";Dis["裂谷热"]="Rift Valley fever";
  Dis["中东呼吸综合征"]="MERS";Dis["炭疽"]="Anthrax";Dis["克里米亚-刚果出血热"]="Crimean-Congo haemorrhagic fever";
  Dis["落基山斑点热"]="Rocky Mountain spotted fever";Dis["鼠疫"]="Plague";Dis["拉沙热"]="Lassa fever";
  Dis["汉坦病毒"]="Hantavirus";Dis["尼帕病毒病"]="Nipah virus";Dis["禽流感"]="Avian influenza";Dis["流感"]="Influenza";
  Dis["甲型肝炎"]="Hepatitis A";Dis["戊型肝炎"]="Hepatitis E";Dis["伤寒/副伤寒"]="Typhoid / paratyphoid";
  Dis["脊髓灰质炎"]="Polio";Dis["白喉"]="Diphtheria";Dis["麻疹"]="Measles";Dis["流脑"]="Meningococcal disease";
  Dis["奥罗普切热"]="Oropouche fever";Dis["不明原因肺炎"]="Undiagnosed pneumonia";Dis["不明原因疾病"]="Undiagnosed disease";

  // 英文国名比中文长得多，两个刚果的正式名放不进 hero 和排行表的列宽，这里只做显示用短名；
  // 中文界面和其他语言完全不受影响，数据文件不动。
  CountryShort["Democratic Republic of the Congo"]="DR Congo";
 }

 static void Add(string key,string zh,string en,string ja,string es,string fr,string de,string ru){
  Rows[key]=new[]{zh,en,ja,es,fr,de,ru};
 }
 static int Index(string code){
  for(int i=0;i<Codes.Length;i++)if(Codes[i]==code)return i;return -1;
 }
 // "en-US" 取前两位；不认识的原样返回 null。
 static string Resolve(string s){
  s=(s??"").Trim().ToLowerInvariant();
  if(s.Length>=2){string two=s.Substring(0,2);if(Index(two)>=0)return two;}
  return null;
 }
 // 只给 disease 场景调用：full 场景不初始化，保持中文和原字体。
 public static void Init(string cfgLang){
  // 顺序按任务卡：config 的 lang 优先；没写才看系统 UI 区域；两者都不支持就回落 en。
  string cfg=(cfgLang??"").Trim();
  string code;
  if(cfg.Length>0)code=Resolve(cfg)??"en";
  else code=Resolve(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)??"en";
  int i=Index(code);if(i<0)i=1;
  ix=i;Code=Codes[i];
  // 日文字体放在雅黑前面，避免汉字用中文字形；其余语言第一顺位 Segoe UI 覆盖拉丁/西里尔。
  if(Code=="ja"){
   Sans=new FontFamily("Segoe UI, Yu Gothic UI, Microsoft YaHei UI");
   Mono=new FontFamily("Consolas, Yu Gothic UI, Microsoft YaHei UI");
  }else{
   Sans=new FontFamily("Segoe UI, Microsoft YaHei UI, Yu Gothic UI");
   Mono=new FontFamily("Consolas, Microsoft YaHei UI, Yu Gothic UI");
  }
  Th.Sans=Sans;Th.Mono=Mono;Ready=true;
 }
 public static bool Has(string key){return key!=null&&Rows.ContainsKey(key);}
 // 病种名：只有英文模式做映射，中文和其他语言原样返回。
 public static string Disease(string name){
  if(String.IsNullOrEmpty(name)||!Ready||Code!="en")return name;
  string en;return Dis.TryGetValue(name,out en)?en:name;
 }
 // 英文国名的显示短名（只影响显示，数据文件里的 country_en 不动）。
 public static string Country(string name){
  if(String.IsNullOrEmpty(name)||!Ready||Code!="en")return name;
  string s;return CountryShort.TryGetValue(name,out s)?s:name;
 }
 public static string T(string key){
  string[] r;if(key==null||!Rows.TryGetValue(key,out r))return key??"";
  // 表里存在这个 key 就按表取；当前语言为空串则回落英文，英文也为空就返回空串（比如拉丁语系不需要"例"这种量词）。
  string s=r[ix];if(String.IsNullOrEmpty(s))s=r[1];return s;
 }
 public static string Fmt(string key,params object[] args){return String.Format(CultureInfo.InvariantCulture,T(key),args);}
 // 千分位：按语言分组；只处理整数，不改小数。
 public static string N(double v){
  long n=(long)Math.Round(v);string d=Math.Abs(n).ToString(CultureInfo.InvariantCulture),g="";
  for(int i=0;i<d.Length;i++){
   if(i>0&&(d.Length-i)%3==0)g+=Group[ix];
   g+=d[i];
  }
  return n<0?"-"+g:g;
 }
 public static string Dec(double v,int digits){
  string s=v.ToString("F"+digits,CultureInfo.InvariantCulture);
  return Point[ix]=="."?s:s.Replace('.',Point[ix][0]);
 }
 public static string Pct(double v){return Dec(v,1)+(PctSpace[ix]?" %":"%");}
 public static string Level(int lv){return T(lv>=2?"level.high":lv>=1?"level.mid":"level.low");}
}
