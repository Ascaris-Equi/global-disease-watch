/* global window, document, performance, requestAnimationFrame, Intl, fetch, AbortController */
(function () {
'use strict';

/* =====================================================================
   1 · embedded fallback data (used when ./snapshot.json cannot load,
       e.g. opened straight from file://)
   ===================================================================== */
const FALLBACK_SNAPSHOT = {"schema":1,"generatedAt":"2026-11-06T06:18:02.537727Z","source":"WHO DON / ECDC / CDC","disclaimer":"关注度为本屏估算，不是官方风险评级。","events":[{"country":"意大利","iso":"ITA","disease":"基孔肯雅热","lat":44.732482,"lon":11.076907,"date":"2026-10-28","cases":1,"deaths":null,"level":"","active":"","source":"https://chik-weekly.ecdc.europa.eu/","evidence":"Italy (1 case)"},{"country":"法国","iso":"FRA","disease":"基孔肯雅热","lat":46.696113,"lon":2.552275,"date":"2026-10-28","cases":204,"deaths":null,"level":"","active":"","source":"https://chik-weekly.ecdc.europa.eu/","evidence":"France (204 cases)"},{"country":"意大利","iso":"ITA","disease":"登革热","lat":44.732482,"lon":11.076907,"date":"2026-10-28","cases":76,"deaths":null,"level":"","active":"","source":"https://dengue-weekly.ecdc.europa.eu/","evidence":"Italy (76 cases)"},{"country":"法国","iso":"FRA","disease":"登革热","lat":46.696113,"lon":2.552275,"date":"2026-10-28","cases":8,"deaths":null,"level":"","active":"","source":"https://dengue-weekly.ecdc.europa.eu/","evidence":"France (8 cases)"},{"country":"尼日利亚","iso":"NGA","disease":"埃博拉","lat":9.439799,"lon":7.50322,"date":"2026-10-23T23:03:45.0000000+08:00","cases":null,"deaths":null,"level":"","active":"","source":"https://www.who.int/emergencies/disease-outbreak-news/item/2026-DON622","evidence":""},{"country":"肯尼亚","iso":"KEN","disease":"埃博拉","lat":0.549043,"lon":37.907632,"date":"2026-10-23T23:03:45.0000000+08:00","cases":null,"deaths":null,"level":"","active":"True","source":"https://www.who.int/emergencies/disease-outbreak-news/item/2026-DON622","evidence":""},{"country":"哥伦比亚","iso":"COL","disease":"黄热病","lat":3.373111,"lon":-73.174347,"date":"2026-10-16T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level2/yellow-fever-colombia","evidence":"Level 2 - Yellow Fever in Colombia"},{"country":"加纳","iso":"GHA","disease":"黄热病","lat":7.717639,"lon":-1.036941,"date":"2026-10-16T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level2/yellow-fever-ghana","evidence":"Level 2 - Yellow Fever in Ghana"},{"country":"沙特阿拉伯","iso":"SAU","disease":"中东呼吸综合征","lat":23.806908,"lon":44.6996,"date":"2026-10-16T00:00:00.0000000+08:00","cases":null,"deaths":null,"level":"","active":"","source":"https://www.who.int/emergencies/disease-outbreak-news/item/middle-east-respiratory-syndrome-coronavirus---kingdom-of-saudi-arabia","evidence":""},{"country":"俄罗斯","iso":"RUS","disease":"不明原因肺炎","lat":58.249357,"lon":44.686469,"date":"2026-10-10T23:00:00.0000000+08:00","cases":null,"deaths":null,"level":"","active":"","source":"https://www.who.int/emergencies/disease-outbreak-news/item/2026-DON620","evidence":""},{"country":"刚果（金）","iso":"COD","disease":"埃博拉","lat":-1.858167,"lon":23.458829,"date":"2026-10-06","cases":8728,"deaths":4205,"level":"","active":"True","source":"https://www.who.int/emergencies/disease-outbreak-news/item/2026-DON619","evidence":"As of 6 October 2026, the country has recorded 8728 confirmed cases and 4205 confirmed deaths, corresponding to a crude case fatality ratio (CFR) of 48"},{"country":"哥斯达黎加","iso":"CRI","disease":"甲型肝炎","lat":10.0651,"lon":-84.077922,"date":"2026-09-18T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level1/hepatitis-a-costa-rica","evidence":"Level 1 - Hepatitis A in Costa Rica"},{"country":"阿富汗","iso":"AFG","disease":"霍乱","lat":34.164262,"lon":66.496586,"date":"2026-09-14","cases":115346,"deaths":55,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/all-topics-z/cholera/surveillance-and-disease-data/cholera-monthly","evidence":"Since 1 January 2026 and as of 14 September 2026, 115 346 cases, including 55 deaths have been reported"},{"country":"赞比亚","iso":"ZMB","disease":"霍乱","lat":-14.660804,"lon":26.395298,"date":"2026-09-13","cases":1074,"deaths":19,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/all-topics-z/cholera/surveillance-and-disease-data/cholera-monthly","evidence":"Since 1 January 2026 and as of 13 September 2026, 1 074 cases, including 19 deaths, have been reported"},{"country":"刚果（布）","iso":"COG","disease":"霍乱","lat":0.142331,"lon":15.9005,"date":"2026-09-13","cases":988,"deaths":65,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/all-topics-z/cholera/surveillance-and-disease-data/cholera-monthly","evidence":"Since 1 January 2026 and as of 13 September 2026, 988 cases, including 65 deaths, have been reported"},{"country":"莫桑比克","iso":"MOZ","disease":"霍乱","lat":-13.94323,"lon":37.83789,"date":"2026-09-13","cases":7814,"deaths":66,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/all-topics-z/cholera/surveillance-and-disease-data/cholera-monthly","evidence":"Since 1 January 2026 and as of 13 September 2026, 7 814 cases, including 66 deaths, have been reported"},{"country":"刚果（金）","iso":"COD","disease":"霍乱","lat":-1.858167,"lon":23.458829,"date":"2026-09-13","cases":41687,"deaths":1050,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/all-topics-z/cholera/surveillance-and-disease-data/cholera-monthly","evidence":"Since 1 January 2026 and as of 13 September 2026, 41 687 cases, including 1 050 deaths, have been reported"},{"country":"安哥拉","iso":"AGO","disease":"霍乱","lat":-12.182762,"lon":17.984249,"date":"2026-09-13","cases":6651,"deaths":127,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/all-topics-z/cholera/surveillance-and-disease-data/cholera-monthly","evidence":"Since 1 January 2026 and as of 13 September 2026, 6 651 cases, including 127 deaths, have been reported"},{"country":"南苏丹","iso":"SDS","disease":"霍乱","lat":7.230477,"lon":30.390151,"date":"2026-09-13","cases":457,"deaths":null,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/all-topics-z/cholera/surveillance-and-disease-data/cholera-monthly","evidence":"Since 1 January 2026 and as of 13 September 2026, 457 cases have been reported"},{"country":"索马里","iso":"SOM","disease":"霍乱","lat":3.568925,"lon":45.19238,"date":"2026-09-13","cases":2778,"deaths":1,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/all-topics-z/cholera/surveillance-and-disease-data/cholera-monthly","evidence":"Since 1 January 2026 and as of 13 September 2026, 2 778 cases, including 1 death, have been reported"},{"country":"也门","iso":"YEM","disease":"霍乱","lat":15.328226,"lon":45.874383,"date":"2026-09-07","cases":7328,"deaths":20,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/all-topics-z/cholera/surveillance-and-disease-data/cholera-monthly","evidence":"Since 1 January 2026 and as of 7 September 2026, 7 328 cases, including 20 deaths, have been reported"},{"country":"尼加拉瓜","iso":"NIC","disease":"基孔肯雅热","lat":12.670697,"lon":-85.069347,"date":"2026-09-03T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level2/chikungunya-nicaragua","evidence":"Level 2 - Chikungunya in Nicaragua"},{"country":"哥伦比亚","iso":"COL","disease":"登革热","lat":3.373111,"lon":-73.174347,"date":"2026-09-01","cases":79304,"deaths":null,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/dengue-monthly","evidence":"In 2026, and as of week 36, most cases have been reported by Brazil (1 294 963), Colombia (79 304), Bolivia (58 628), Peru (55 943), and Mexico (50 819) ( Dengue: analysis by country - PAHO/WHO | Pan American Health Organization; accessed 28 September 2026 )"},{"country":"巴西","iso":"BRA","disease":"登革热","lat":-12.098687,"lon":-49.55945,"date":"2026-09-01","cases":1294963,"deaths":null,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/dengue-monthly","evidence":"In 2026, and as of week 36, most cases have been reported by Brazil (1 294 963), Colombia (79 304), Bolivia (58 628), Peru (55 943), and Mexico (50 819) ( Dengue: analysis by country - PAHO/WHO | Pan American Health Organization; accessed 28 September 2026 )"},{"country":"玻利维亚","iso":"BOL","disease":"登革热","lat":-16.666015,"lon":-64.593433,"date":"2026-09-01","cases":58628,"deaths":null,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/dengue-monthly","evidence":"In 2026, and as of week 36, most cases have been reported by Brazil (1 294 963), Colombia (79 304), Bolivia (58 628), Peru (55 943), and Mexico (50 819) ( Dengue: analysis by country - PAHO/WHO | Pan American Health Organization; accessed 28 September 2026 )"},{"country":"墨西哥","iso":"MEX","disease":"登革热","lat":23.919988,"lon":-102.289448,"date":"2026-09-01","cases":50819,"deaths":null,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/dengue-monthly","evidence":"In 2026, and as of week 36, most cases have been reported by Brazil (1 294 963), Colombia (79 304), Bolivia (58 628), Peru (55 943), and Mexico (50 819) ( Dengue: analysis by country - PAHO/WHO | Pan American Health Organization; accessed 28 September 2026 )"},{"country":"秘鲁","iso":"PER","disease":"登革热","lat":-12.976679,"lon":-72.90016,"date":"2026-09-01","cases":55943,"deaths":null,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/dengue-monthly","evidence":"In 2026, and as of week 36, most cases have been reported by Brazil (1 294 963), Colombia (79 304), Bolivia (58 628), Peru (55 943), and Mexico (50 819) ( Dengue: analysis by country - PAHO/WHO | Pan American Health Organization; accessed 28 September 2026 )"},{"country":"孟加拉国","iso":"BGD","disease":"霍乱","lat":24.214956,"lon":89.684963,"date":"2026-08-31","cases":24,"deaths":null,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/all-topics-z/cholera/surveillance-and-disease-data/cholera-monthly","evidence":"Since 1 January 2026 and as of 31 August 2026, 24 cases have been reported"},{"country":"乌干达","iso":"UGA","disease":"埃博拉","lat":1.972589,"lon":32.948555,"date":"2026-08-28T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level2/ebola-drc-uganda","evidence":"Level 2 - Ebola Bundibugyo Virus Disease in the Democratic Republic of the Congo and Uganda"},{"country":"也门","iso":"YEM","disease":"伤寒/副伤寒","lat":15.328226,"lon":45.874383,"date":"2026-08-26T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level1/paratyphoid-fever-yemen","evidence":"Level 1 - Paratyphoid Fever in Yemen"},{"country":"巴基斯坦","iso":"PAK","disease":"霍乱","lat":29.328389,"lon":68.545632,"date":"2026-08-24","cases":6353,"deaths":null,"level":"","active":"","source":"https://www.ecdc.europa.eu/en/all-topics-z/cholera/surveillance-and-disease-data/cholera-monthly","evidence":"Since 1 January 2026 and as of 24 August 2026, 6 353 cases have been reported"},{"country":"印度尼西亚","iso":"IDN","disease":"寨卡","lat":-0.954404,"lon":101.892949,"date":"2026-08-07T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level2/zika-indonesia","evidence":"Level 2 - Zika in Indonesia"},{"country":"哥斯达黎加","iso":"CRI","disease":"基孔肯雅热","lat":10.0651,"lon":-84.077922,"date":"2026-07-30T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level2/chikungunya-costa-rica","evidence":"Level 2 - Chikungunya in Costa Rica"},{"country":"海地","iso":"HTI","disease":"白喉","lat":19.263784,"lon":-72.224051,"date":"2026-06-25T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level1/diphtheria-haiti","evidence":"Level 1 - Diphtheria in Haiti"},{"country":"也门","iso":"YEM","disease":"疟疾","lat":15.328226,"lon":45.874383,"date":"2026-06-11T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level2/malaria-yemen","evidence":"Level 2 - Malaria in Yemen"},{"country":"法属圭亚那","iso":"GUF","disease":"基孔肯雅热","lat":3.93,"lon":-53.13,"date":"2026-06-04T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level2/chikungunya-french-guiana","evidence":"Level 2 - Chikungunya in French Guiana"},{"country":"加拿大","iso":"CAN","disease":"甲型肝炎","lat":60.324287,"lon":-101.9107,"date":"2026-06-04T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level1/hepatitis-a-canada","evidence":"Level 1 - Hepatitis A in Canada"},{"country":"马约特","iso":"MYT","disease":"疟疾","lat":-12.83,"lon":45.17,"date":"2026-06-02T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level2/malaria-mayotte","evidence":"Level 2 - Malaria in Mayotte"},{"country":"刚果（金）","iso":"COD","disease":"流脑","lat":-1.858167,"lon":23.458829,"date":"2026-03-24T04:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level2/meningococcal-drc","evidence":"Level 2 - Meningococcal Disease in the Democratic Republic of the Congo"},{"country":"墨西哥","iso":"MEX","disease":"落基山斑点热","lat":23.919988,"lon":-102.289448,"date":"2025-12-23T05:00:00.0000000Z","cases":null,"deaths":null,"level":"","active":"True","source":"https://wwwnc.cdc.gov/travel/notices/level1/rmsf-mexico","evidence":"Level 1 - Rocky Mountain Spotted Fever in Mexico"}]};
const FALLBACK_TRANSMISSION = {
 "version": 1,
 "note": "How each disease reaches people, after the WHO fact sheet (CDC where WHO has none). Disease names match the values of diseaseNames in feeds.json.",
 "nodes": [
  {"id": "bat", "name": "蝙蝠", "glyph": "🦇", "row": 0},
  {"id": "rodent", "name": "鼠类", "glyph": "🐀", "row": 0},
  {"id": "bird", "name": "禽鸟", "glyph": "🐔", "row": 0},
  {"id": "livestock", "name": "牲畜", "glyph": "🐄", "row": 0},
  {"id": "water", "name": "水与食物", "glyph": "💧", "row": 0},
  {"id": "primate", "name": "猴类", "glyph": "🐒", "row": 1},
  {"id": "pig", "name": "猪", "glyph": "🐖", "row": 1},
  {"id": "mosquito", "name": "蚊虫", "glyph": "🦟", "row": 1},
  {"id": "tick", "name": "蜱虫", "glyph": "蜱", "row": 1},
  {"id": "human", "name": "人", "glyph": "🧍", "row": 2}
 ],
 "diseases": [
  {"name": "埃博拉", "color": "#FF6B6B", "path": [["bat","primate"],["primate","human"],["bat","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/ebola-disease"},
  {"name": "马尔堡病", "color": "#FF7B7B", "path": [["bat","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/marburg-virus-disease"},
  {"name": "猴痘", "color": "#C59BFF", "path": [["rodent","primate"],["rodent","human"],["primate","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/mpox"},
  {"name": "霍乱", "color": "#6FD3FF", "path": [["water","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/fact-sheets/detail/cholera"},
  {"name": "登革热", "color": "#FFB25C", "path": [["mosquito","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/fact-sheets/detail/dengue-and-severe-dengue"},
  {"name": "基孔肯雅热", "color": "#FF8FD0", "path": [["mosquito","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/fact-sheets/detail/chikungunya"},
  {"name": "寨卡", "color": "#FF9F80", "path": [["mosquito","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/zika-virus"},
  {"name": "黄热病", "color": "#FFE066", "path": [["primate","mosquito"],["mosquito","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/fact-sheets/detail/yellow-fever"},
  {"name": "疟疾", "color": "#F7C873", "path": [["mosquito","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/fact-sheets/detail/malaria"},
  {"name": "西尼罗热", "color": "#E6C27A", "path": [["bird","mosquito"],["mosquito","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/fact-sheets/detail/west-nile-virus"},
  {"name": "裂谷热", "color": "#E0A96D", "path": [["livestock","mosquito"],["mosquito","human"],["livestock","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/fact-sheets/detail/rift-valley-fever"},
  {"name": "中东呼吸综合征", "color": "#F2A65A", "path": [["livestock","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/middle-east-respiratory-syndrome-coronavirus-(mers-cov)"},
  {"name": "炭疽", "color": "#C9B38F", "path": [["livestock","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/questions-and-answers/item/anthrax"},
  {"name": "克里米亚-刚果出血热", "color": "#FF8A65", "path": [["livestock","tick"],["tick","human"],["livestock","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/crimean-congo-haemorrhagic-fever"},
  {"name": "落基山斑点热", "color": "#E3A06B", "path": [["tick","human"]], "personToPerson": false, "source": "https://www.cdc.gov/rocky-mountain-spotted-fever/about/index.html"},
  {"name": "鼠疫", "color": "#D98C8C", "path": [["rodent","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/plague"},
  {"name": "拉沙热", "color": "#E57373", "path": [["rodent","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/lassa-fever"},
  {"name": "汉坦病毒", "color": "#C4A484", "path": [["rodent","human"]], "personToPerson": false, "source": "https://www.cdc.gov/hantavirus/about/index.html"},
  {"name": "尼帕病毒病", "color": "#FF9E9E", "path": [["bat","pig"],["pig","human"],["bat","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/nipah-virus"},
  {"name": "禽流感", "color": "#9AD0FF", "path": [["bird","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/fact-sheets/detail/influenza-(avian-and-other-zoonotic)"},
  {"name": "流感", "color": "#8FB8FF", "path": [["bird","pig"],["pig","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/influenza-(avian-and-other-zoonotic)"},
  {"name": "甲型肝炎", "color": "#8FD3C8", "path": [["water","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/hepatitis-a"},
  {"name": "戊型肝炎", "color": "#7FC8B0", "path": [["water","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/fact-sheets/detail/hepatitis-e"},
  {"name": "伤寒/副伤寒", "color": "#9ACBFF", "path": [["water","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/fact-sheets/detail/typhoid"},
  {"name": "脊髓灰质炎", "color": "#C3E88D", "path": [["water","human"]], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/poliomyelitis"},
  {"name": "白喉", "color": "#B0E57C", "path": [], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/diphtheria"},
  {"name": "麻疹", "color": "#9BE38B", "path": [], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/measles"},
  {"name": "流脑", "color": "#A6B8FF", "path": [], "personToPerson": true, "source": "https://www.who.int/news-room/fact-sheets/detail/meningitis"},
  {"name": "奥罗普切热", "color": "#F0B27A", "path": [["mosquito","human"]], "personToPerson": false, "source": "https://www.who.int/news-room/fact-sheets/detail/oropouche-virus-disease"},
  {"name": "不明原因肺炎", "color": "#D0D7E2", "path": [], "personToPerson": false, "source": ""},
  {"name": "不明原因疾病", "color": "#D0D7E2", "path": [], "personToPerson": false, "source": ""}
 ]
}
;

/* =====================================================================
   2 · seven-language dictionary
   ===================================================================== */
const LANGS = ['zh', 'en', 'ja', 'es', 'fr', 'de', 'ru'];
const LOCALE = { zh: 'zh-CN', en: 'en-GB', ja: 'ja-JP', es: 'es-ES', fr: 'fr-FR', de: 'de-DE', ru: 'ru-RU' };

const UI = {
  zh: {
    brand: "全球传染病监测", brandTag: "GLOBAL WATCH", agencies: "WHO / ECDC / CDC",
    focus: "监测焦点", cumulative: "本轮疫情累计", deaths: "死亡", cases: "报告病例", unit: "例",
    reported: "报告病例", cfr: "病死率", attention: "关注度", estimate: "估算指标 · 非官方评级",
    reportDate: "通报日期", asOf: "数据截至", dataSource: "数据源", rank: "RANK",
    arcNote: "弧线为同病种关联示意，非溯源结论",
    watchKicker: "WATCHLIST\u00A0\u00A001–06", watchTitle: "关注度排行", rankedBy: "按本屏估算排序",
    colCountry: "国家", colDisease: "病种", colCases: "病例", colDeaths: "死亡", colReported: "通报", colAttention: "关注",
    high: "高", mid: "中", low: "低",
    transmission: "传播链 / TRANSMISSION", p2p: "人传人",
    reports: "REPORTS", countries: "国", latest: "最近通报", lag: "LAG", source: "SOURCE", sync: "SYNC",
    official: "官方通报", empty: "近期通报暂不可用"
  },
  en: {
    brand: "GLOBAL DISEASE WATCH", brandTag: "INFECTIOUS DISEASE", agencies: "WHO / ECDC / CDC",
    focus: "FOCUS", cumulative: "CUMULATIVE THIS OUTBREAK", deaths: "DEATHS", cases: "REPORTED CASES", unit: "",
    reported: "REPORTED CASES", cfr: "FATALITY", attention: "ATTENTION", estimate: "ESTIMATE · NOT OFFICIAL",
    reportDate: "REPORTED", asOf: "AS OF", dataSource: "SOURCE", rank: "RANK",
    arcNote: "ARCS ARE SAME-DISEASE LINKS, NOT TRACE RESULTS",
    watchKicker: "WATCHLIST\u00A0\u00A001–06", watchTitle: "ATTENTION RANKING", rankedBy: "BY ON-SCREEN ESTIMATE",
    colCountry: "COUNTRY", colDisease: "DISEASE", colCases: "CASES", colDeaths: "DEATHS", colReported: "REPORT", colAttention: "ATTN.",
    high: "HIGH", mid: "MID", low: "LOW",
    transmission: "TRANSMISSION CHAIN", p2p: "HUMAN-TO-HUMAN",
    reports: "REPORTS", countries: "COUNTRIES", latest: "LATEST", lag: "LAG", source: "SOURCE", sync: "SYNC",
    official: "OFFICIAL", empty: "NO RECENT REPORTS"
  },
  ja: {
    brand: "世界の感染症監視", brandTag: "GLOBAL WATCH", agencies: "WHO / ECDC / CDC",
    focus: "監視対象", cumulative: "今回の累計", deaths: "死亡", cases: "報告症例", unit: "例",
    reported: "報告症例", cfr: "致死率", attention: "注目度", estimate: "推定値 · 非公式",
    reportDate: "報告日", asOf: "基準日", dataSource: "情報源", rank: "RANK",
    arcNote: "弧線は同一疾患の関連を示すもので、感染経路の断定ではありません",
    watchKicker: "WATCHLIST\u00A0\u00A001–06", watchTitle: "注目度ランキング", rankedBy: "本画面の推定順",
    colCountry: "国", colDisease: "疾患", colCases: "症例", colDeaths: "死亡", colReported: "報告", colAttention: "注目",
    high: "高", mid: "中", low: "低",
    transmission: "感染経路 / TRANSMISSION", p2p: "人から人",
    reports: "件", countries: "か国", latest: "最新", lag: "遅延", source: "情報源", sync: "同期",
    official: "公式発表", empty: "最近の報告なし"
  },
  es: {
    brand: "VIGILANCIA EPIDEMIOLÓGICA", brandTag: "GLOBAL WATCH", agencies: "WHO / ECDC / CDC",
    focus: "FOCO", cumulative: "ACUMULADO DEL BROTE", deaths: "MUERTES", cases: "CASOS", unit: "",
    reported: "CASOS", cfr: "LETALIDAD", attention: "ATENCIÓN", estimate: "ESTIMACIÓN · NO OFICIAL",
    reportDate: "NOTIFICADO", asOf: "DATOS AL", dataSource: "FUENTE", rank: "RANK",
    arcNote: "LOS ARCOS INDICAN VÍNCULOS POR ENFERMEDAD, NO TRAZABILIDAD",
    watchKicker: "WATCHLIST\u00A0\u00A001–06", watchTitle: "RANKING DE ATENCIÓN", rankedBy: "POR ESTIMACIÓN EN PANTALLA",
    colCountry: "PAÍS", colDisease: "ENFERMEDAD", colCases: "CASOS", colDeaths: "MUERTES", colReported: "FECHA", colAttention: "ATEN.",
    high: "ALTA", mid: "MEDIA", low: "BAJA",
    transmission: "CADENA DE TRANSMISIÓN", p2p: "PERSONA A PERSONA",
    reports: "INFORMES", countries: "PAÍSES", latest: "ÚLTIMO", lag: "RETRASO", source: "FUENTE", sync: "SINCRO",
    official: "OFICIAL", empty: "SIN DATOS RECIENTES"
  },
  fr: {
    brand: "VEILLE ÉPIDÉMIOLOGIQUE", brandTag: "GLOBAL WATCH", agencies: "WHO / ECDC / CDC",
    focus: "FOCUS", cumulative: "CUMUL DE L'ÉPIDÉMIE", deaths: "DÉCÈS", cases: "CAS", unit: "",
    reported: "CAS", cfr: "LÉTALITÉ", attention: "ATTENTION", estimate: "ESTIMATION · NON OFFICIEL",
    reportDate: "NOTIFIÉ", asOf: "DONNÉES AU", dataSource: "SOURCE", rank: "RANG",
    arcNote: "LES ARCS INDIQUENT DES LIENS PAR MALADIE, PAS UNE TRAÇABILITÉ",
    watchKicker: "WATCHLIST\u00A0\u00A001–06", watchTitle: "CLASSEMENT PAR ATTENTION", rankedBy: "PAR ESTIMATION ÉCRAN",
    colCountry: "PAYS", colDisease: "MALADIE", colCases: "CAS", colDeaths: "DÉCÈS", colReported: "DATE", colAttention: "ATT.",
    high: "HAUTE", mid: "MOYENNE", low: "BASSE",
    transmission: "CHAÎNE DE TRANSMISSION", p2p: "D'HOMME À HOMME",
    reports: "RAPPORTS", countries: "PAYS", latest: "DERNIER", lag: "RETARD", source: "SOURCE", sync: "SYNCHRO",
    official: "OFFICIEL", empty: "AUCUN RAPPORT RÉCENT"
  },
  de: {
    brand: "GLOBALE SEUCHENÜBERWACHUNG", brandTag: "GLOBAL WATCH", agencies: "WHO / ECDC / CDC",
    focus: "FOKUS", cumulative: "KUMULIERT SEIT AUSBRUCH", deaths: "TODESFÄLLE", cases: "FÄLLE", unit: "",
    reported: "FÄLLE", cfr: "LETALITÄT", attention: "BEACHTUNG", estimate: "SCHÄTZUNG · INOFFIZIELL",
    reportDate: "GEMELDET", asOf: "STAND", dataSource: "QUELLE", rank: "RANG",
    arcNote: "BÖGEN ZEIGEN GLEICHE KRANKHEIT, KEINE RÜCKVERFOLGUNG",
    watchKicker: "WATCHLIST\u00A0\u00A001–06", watchTitle: "AUFMERKSAMKEIT-RANG", rankedBy: "NACH SCHÄTZUNG",
    colCountry: "LAND", colDisease: "KRANKHEIT", colCases: "FÄLLE", colDeaths: "TODESFÄLLE", colReported: "MELDUNG", colAttention: "FOKUS",
    high: "HOCH", mid: "MITTEL", low: "NIEDRIG",
    transmission: "ÜBERTRAGUNGSKETTE", p2p: "MENSCH ZU MENSCH",
    reports: "MELDUNGEN", countries: "LÄNDER", latest: "NEUESTE", lag: "VERZUG", source: "QUELLE", sync: "SYNC",
    official: "OFFIZIELL", empty: "KEINE NEUEN MELDUNGEN"
  },
  ru: {
    brand: "ГЛОБАЛЬНЫЙ МОНИТОРИНГ", brandTag: "GLOBAL WATCH", agencies: "WHO / ECDC / CDC",
    focus: "ФОКУС", cumulative: "ИТОГО ЗА ВСПЫШКУ", deaths: "СМЕРТИ", cases: "СЛУЧАИ", unit: "",
    reported: "СЛУЧАИ", cfr: "ЛЕТАЛЬНОСТЬ", attention: "ВНИМАНИЕ", estimate: "ОЦЕНКА · НЕ ОФИЦИАЛЬНО",
    reportDate: "СООБЩЕНО", asOf: "ДАННЫЕ НА", dataSource: "ИСТОЧНИК", rank: "МЕСТО",
    arcNote: "ДУГИ — СВЯЗЬ ПО БОЛЕЗНИ, НЕ ТРАССИРОВКА",
    watchKicker: "WATCHLIST\u00A0\u00A001–06", watchTitle: "РЕЙТИНГ ВНИМАНИЯ", rankedBy: "ПО ОЦЕНКЕ ЭКРАНА",
    colCountry: "СТРАНА", colDisease: "БОЛЕЗНЬ", colCases: "СЛУЧАИ", colDeaths: "СМЕРТИ", colReported: "СООБЩЕНО", colAttention: "ВНИМ.",
    high: "ВЫСОК.", mid: "СРЕДН.", low: "НИЗК.",
    transmission: "ЦЕПОЧКА ПЕРЕДАЧИ", p2p: "МЕЖДУ ЛЮДЬМИ",
    reports: "СООБЩЕНИЙ", countries: "СТРАН", latest: "ПОСЛЕДНЕЕ", lag: "ЗАДЕРЖКА", source: "ИСТОЧНИК", sync: "СИНХРО",
    official: "ОФИЦИАЛЬНО", empty: "НЕТ СВЕЖИХ ДАННЫХ"
  }
};

const COUNTRY = {
  ITA: { en: 'Italy', ja: 'イタリア', es: 'Italia', fr: 'Italie', de: 'Italien', ru: 'Италия' },
  FRA: { en: 'France', ja: 'フランス', es: 'Francia', fr: 'France', de: 'Frankreich', ru: 'Франция' },
  NGA: { en: 'Nigeria', ja: 'ナイジェリア', es: 'Nigeria', fr: 'Nigéria', de: 'Nigeria', ru: 'Нигерия' },
  KEN: { en: 'Kenya', ja: 'ケニア', es: 'Kenia', fr: 'Kenya', de: 'Kenia', ru: 'Кения' },
  COL: { en: 'Colombia', ja: 'コロンビア', es: 'Colombia', fr: 'Colombie', de: 'Kolumbien', ru: 'Колумбия' },
  GHA: { en: 'Ghana', ja: 'ガーナ', es: 'Ghana', fr: 'Ghana', de: 'Ghana', ru: 'Гана' },
  SAU: { en: 'Saudi Arabia', ja: 'サウジアラビア', es: 'Arabia Saudita', fr: 'Arabie saoudite', de: 'Saudi-Arabien', ru: 'Саудовская Аравия' },
  RUS: { en: 'Russia', ja: 'ロシア', es: 'Rusia', fr: 'Russie', de: 'Russland', ru: 'Россия' },
  COD: { en: 'DR Congo', ja: 'コンゴ民主共和国', es: 'RDC', fr: 'RD Congo', de: 'DR Kongo', ru: 'ДР Конго' },
  CRI: { en: 'Costa Rica', ja: 'コスタリカ', es: 'Costa Rica', fr: 'Costa Rica', de: 'Costa Rica', ru: 'Коста-Рика' },
  AFG: { en: 'Afghanistan', ja: 'アフガニスタン', es: 'Afganistán', fr: 'Afghanistan', de: 'Afghanistan', ru: 'Афганистан' },
  ZMB: { en: 'Zambia', ja: 'ザンビア', es: 'Zambia', fr: 'Zambie', de: 'Sambia', ru: 'Замбия' },
  COG: { en: 'Congo', ja: 'コンゴ共和国', es: 'Congo', fr: 'Congo', de: 'Kongo', ru: 'Конго' },
  MOZ: { en: 'Mozambique', ja: 'モザンビーク', es: 'Mozambique', fr: 'Mozambique', de: 'Mosambik', ru: 'Мозамбик' },
  AGO: { en: 'Angola', ja: 'アンゴラ', es: 'Angola', fr: 'Angola', de: 'Angola', ru: 'Ангола' },
  SDS: { en: 'South Sudan', ja: '南スーダン', es: 'Sudán del Sur', fr: 'Soudan du Sud', de: 'Südsudan', ru: 'Южный Судан' },
  SOM: { en: 'Somalia', ja: 'ソマリア', es: 'Somalia', fr: 'Somalie', de: 'Somalia', ru: 'Сомали' },
  YEM: { en: 'Yemen', ja: 'イエメン', es: 'Yemen', fr: 'Yémen', de: 'Jemen', ru: 'Йемен' },
  NIC: { en: 'Nicaragua', ja: 'ニカラグア', es: 'Nicaragua', fr: 'Nicaragua', de: 'Nicaragua', ru: 'Никарагуа' },
  BRA: { en: 'Brazil', ja: 'ブラジル', es: 'Brasil', fr: 'Brésil', de: 'Brasilien', ru: 'Бразилия' },
  BOL: { en: 'Bolivia', ja: 'ボリビア', es: 'Bolivia', fr: 'Bolivie', de: 'Bolivien', ru: 'Боливия' },
  MEX: { en: 'Mexico', ja: 'メキシコ', es: 'México', fr: 'Mexique', de: 'Mexiko', ru: 'Мексика' },
  PER: { en: 'Peru', ja: 'ペルー', es: 'Perú', fr: 'Pérou', de: 'Peru', ru: 'Перу' },
  BGD: { en: 'Bangladesh', ja: 'バングラデシュ', es: 'Bangladés', fr: 'Bangladesh', de: 'Bangladesch', ru: 'Бангладеш' },
  UGA: { en: 'Uganda', ja: 'ウガンダ', es: 'Uganda', fr: 'Ouganda', de: 'Uganda', ru: 'Уганда' },
  PAK: { en: 'Pakistan', ja: 'パキスタン', es: 'Pakistán', fr: 'Pakistan', de: 'Pakistan', ru: 'Пакистан' },
  IDN: { en: 'Indonesia', ja: 'インドネシア', es: 'Indonesia', fr: 'Indonésie', de: 'Indonesien', ru: 'Индонезия' },
  HTI: { en: 'Haiti', ja: 'ハイチ', es: 'Haití', fr: 'Haïti', de: 'Haiti', ru: 'Гаити' },
  GUF: { en: 'French Guiana', ja: '仏領ギアナ', es: 'Guayana Francesa', fr: 'Guyane', de: 'Französisch-Guayana', ru: 'Французская Гвиана' },
  CAN: { en: 'Canada', ja: 'カナダ', es: 'Canadá', fr: 'Canada', de: 'Kanada', ru: 'Канада' },
  MYT: { en: 'Mayotte', ja: 'マヨット', es: 'Mayotte', fr: 'Mayotte', de: 'Mayotte', ru: 'Майотта' }
};

const DISEASE = {
  '埃博拉': { en: 'Ebola', ja: 'エボラ出血熱', es: 'Ébola', fr: 'Ebola', de: 'Ebola', ru: 'Эбола' },
  '登革热': { en: 'Dengue', ja: 'デング熱', es: 'Dengue', fr: 'Dengue', de: 'Dengue', ru: 'Денге' },
  '基孔肯雅热': { en: 'Chikungunya', ja: 'チクングニア熱', es: 'Chikungunya', fr: 'Chikungunya', de: 'Chikungunya', ru: 'Чикунгунья' },
  '黄热病': { en: 'Yellow fever', ja: '黄熱', es: 'Fiebre amarilla', fr: 'Fièvre jaune', de: 'Gelbfieber', ru: 'Жёлтая лихорадка' },
  '霍乱': { en: 'Cholera', ja: 'コレラ', es: 'Cólera', fr: 'Choléra', de: 'Cholera', ru: 'Холера' },
  '中东呼吸综合征': { en: 'MERS', ja: '中東呼吸器症候群', es: 'MERS', fr: 'MERS', de: 'MERS', ru: 'БВРС' },
  '不明原因肺炎': { en: 'Undiagnosed pneumonia', ja: '原因不明の肺炎', es: 'Neumonía de causa desconocida', fr: 'Pneumonie inconnue', de: 'Lungenentzündung unklarer Ursache', ru: 'Пневмония неизвестной этиологии' },
  '甲型肝炎': { en: 'Hepatitis A', ja: 'A型肝炎', es: 'Hepatitis A', fr: 'Hépatite A', de: 'Hepatitis A', ru: 'Гепатит A' },
  '伤寒/副伤寒': { en: 'Typhoid / paratyphoid', ja: '腸チフス・パラチフス', es: 'Fiebre tifoidea / paratifoidea', fr: 'Typhoïde / paratyphoïde', de: 'Typhus / Paratyphus', ru: 'Брюшной тиф / паратиф' },
  '寨卡': { en: 'Zika', ja: 'ジカ熱', es: 'Zika', fr: 'Zika', de: 'Zika', ru: 'Зика' },
  '白喉': { en: 'Diphtheria', ja: 'ジフテリア', es: 'Difteria', fr: 'Diphtérie', de: 'Diphtherie', ru: 'Дифтерия' },
  '疟疾': { en: 'Malaria', ja: 'マラリア', es: 'Malaria', fr: 'Paludisme', de: 'Malaria', ru: 'Малярия' },
  '流脑': { en: 'Meningococcal disease', ja: '髄膜炎菌性髄膜炎', es: 'Enfermedad meningocócica', fr: 'Méningite à méningocoque', de: 'Meningokokken-Erkrankung', ru: 'Менингококковая инфекция' },
  '落基山斑点热': { en: 'Rocky Mountain spotted fever', ja: 'ロッキー山紅斑熱', es: 'Fiebre maculosa de las Montañas Rocosas', fr: 'Fièvre pourprée des montagnes Rocheuses', de: 'Rocky-Mountain-Fleckfieber', ru: 'Пятнистая лихорадка Скалистых гор' }
};

const HOST = {
  bat: { en: 'Bats', ja: 'コウモリ', es: 'Murciélagos', fr: 'Chauves-souris', de: 'Fledermäuse', ru: 'Летучие мыши' },
  rodent: { en: 'Rodents', ja: 'げっ歯類', es: 'Roedores', fr: 'Rongeurs', de: 'Nagetiere', ru: 'Грызуны' },
  bird: { en: 'Birds', ja: '鳥類', es: 'Aves', fr: 'Oiseaux', de: 'Vögel', ru: 'Птицы' },
  livestock: { en: 'Livestock', ja: '家畜', es: 'Ganado', fr: 'Bétail', de: 'Vieh', ru: 'Скот' },
  water: { en: 'Water & food', ja: '水・食べ物', es: 'Agua y alimentos', fr: 'Eau et aliments', de: 'Wasser & Essen', ru: 'Вода и пища' },
  primate: { en: 'Primates', ja: 'サル類', es: 'Primates', fr: 'Primates', de: 'Primaten', ru: 'Приматы' },
  pig: { en: 'Pigs', ja: 'ブタ', es: 'Cerdos', fr: 'Porcs', de: 'Schweine', ru: 'Свиньи' },
  mosquito: { en: 'Mosquitoes', ja: '蚊', es: 'Mosquitos', fr: 'Moustiques', de: 'Mücken', ru: 'Комары' },
  tick: { en: 'Ticks', ja: 'マダニ', es: 'Garrapatas', fr: 'Tiques', de: 'Zecken', ru: 'Клещи' },
  human: { en: 'Human', ja: '人', es: 'Personas', fr: 'Humains', de: 'Menschen', ru: 'Люди' }
};

/* =====================================================================
   3 · state & helpers
   ===================================================================== */
let lang = 'en', locale = 'en-GB', nf = null;
let SNAP = null, WORLD = null, TRANS = null;
let EVENTS = [], RANKED = [], HERO = null, LATEST = '', LAG = 0;
let LEVEL_BY_ISO = new Map(), MARKERS = [], LINKS = [];
let CONN = new Map();
let globe = null, chainView = null;
let lastDraw = -1;
let lastClock = '';

const $ = function (s) { return document.querySelector(s); };
const LV_COLOR = ['#79807F', '#D8D4C8', '#C0392B'];

function t(k) {
  const d = UI[lang] || UI.en;
  return d[k] != null ? d[k] : (UI.en[k] != null ? UI.en[k] : k);
}
function pickLang() {
  let q = null;
  try { q = new URLSearchParams(location.search).get('lang'); } catch (e) { q = null; }
  const cand = String(q || navigator.language || 'en').toLowerCase();
  const base = cand.split(/[-_]/)[0];
  return LANGS.indexOf(base) >= 0 ? base : 'en';
}
function esc(v) { return v == null ? '' : String(v); }
function num(v) { const n = Number(v); return (v == null || v === '' || !isFinite(n)) ? null : n; }
function fmt(n) { return n == null ? '—' : nf.format(n); }
function fmtPct(v) {
  if (v == null || !isFinite(v)) return '—';
  const sp = (lang === 'es' || lang === 'fr' || lang === 'de') ? ' ' : '';
  return new Intl.NumberFormat(locale, { minimumFractionDigits: 1, maximumFractionDigits: 1 }).format(v) + sp + '%';
}
function mmdd(s) {
  if (!s) return '—';
  s = String(s);
  return s.length >= 10 ? s.slice(5, 10) : s;
}
function countryName(e) {
  if (lang === 'zh') return e.country || e.country_en || e.iso;
  if (lang === 'en' && e.country_en) return e.country_en;
  const m = COUNTRY[e.iso];
  if (m && m[lang]) return m[lang];
  if (m && m.en) return m.en;
  if (e.country_en) return e.country_en;
  const w = WORLD && WORLD._byIso ? WORLD._byIso.get(e.iso) : null;
  return (w && (w.name || w.displayName)) || e.country || e.iso;
}
function diseaseName(d, en) {
  if (lang === 'zh') return d;
  if (lang === 'en' && en) return en;
  const m = DISEASE[d];
  if (m && m[lang]) return m[lang];
  if (m && m.en) return m.en;
  return en || d;
}
function hostName(id, fallback) {
  if (lang === 'zh') return fallback || id;
  const m = HOST[id];
  return (m && m[lang]) || (m && m.en) || fallback || id;
}
function sourceName(u) {
  if (!u) return t('agencies');
  if (/who\.int/i.test(u)) return 'WHO';
  if (/ecdc/i.test(u)) return 'ECDC';
  if (/cdc\.gov/i.test(u)) return 'CDC';
  return t('agencies');
}

const SEVERE = /埃博拉|马尔堡|拉沙|中东呼吸|尼帕|鼠疫|克里米亚|不明原因|Ebola|Marburg/i;
const SERIOUS = /霍乱|黄热|猴痘|白喉|脊髓灰质炎|炭疽|裂谷|疟疾|流脑|禽流感|Cholera|Mpox/i;

function attention(e) {
  if (e._a != null) return e._a;
  let s = SEVERE.test(e.disease) ? 0.55 : (SERIOUS.test(e.disease) ? 0.35 : 0.2);
  s += Math.min(1, Math.log10((num(e.cases) || 0) + 1) / 6) * 0.3;
  s += Math.min(1, Math.log10((num(e.deaths) || 0) + 1) / 4) * 0.2;
  const age = (Date.now() - Date.parse(e.date)) / 864e5;
  if (age >= 0 && age < 14) s += 0.04;
  const cdc = num(e.cdcLevel);
  if (cdc >= 1 && cdc <= 4) s = Math.max(s, [0, 0.40, 0.58, 0.78, 0.95][cdc]);
  e._a = Math.min(1, s);
  return e._a;
}
function levelOf(s) { return s >= 0.7 ? 2 : (s >= 0.45 ? 1 : 0); }
function gcDist(a, b) {
  const la1 = a.lat * Math.PI / 180, la2 = b.lat * Math.PI / 180;
  const dl = (a.lon - b.lon) * Math.PI / 180;
  return Math.acos(Math.max(-1, Math.min(1, Math.sin(la1) * Math.sin(la2) + Math.cos(la1) * Math.cos(la2) * Math.cos(dl))));
}

/* =====================================================================
   4 · data loading
   ===================================================================== */
const IS_FILE = location.protocol === 'file:';

function fetchJSON(url) {
  return new Promise(function (resolve) {
    let done = false;
    const ctl = typeof AbortController !== 'undefined' ? new AbortController() : null;
    const timer = setTimeout(function () { if (ctl) ctl.abort(); if (!done) { done = true; resolve(null); } }, 4500);
    fetch(url, { cache: 'no-store', signal: ctl ? ctl.signal : undefined })
      .then(function (r) { if (!r.ok) throw new Error('HTTP ' + r.status); return r.json(); })
      .then(function (j) { if (done) return; done = true; clearTimeout(timer); resolve(j); })
      .catch(function () { if (done) return; done = true; clearTimeout(timer); resolve(null); });
  });
}

async function loadScript(src) {
  return new Promise(function (resolve) {
    const s = document.createElement('script');
    s.src = src;
    s.onload = function () { resolve(true); };
    s.onerror = function () { resolve(false); };
    document.head.appendChild(s);
  });
}

async function loadAll() {
  let snap = null, world = null, trans = null;
  if (!IS_FILE) {
    const all = await Promise.all([
      fetchJSON('./snapshot.json'),
      fetchJSON('./world.json'),
      fetchJSON('./transmission.json')
    ]);
    snap = all[0]; world = all[1]; trans = all[2];
  }
  if (!world && !window.__WORLD__) await loadScript('world-data.js');
  SNAP = (snap && Array.isArray(snap.events) && snap.events.length) ? snap : FALLBACK_SNAPSHOT;
  TRANS = (trans && Array.isArray(trans.nodes)) ? trans : FALLBACK_TRANSMISSION;
  WORLD = (world && Array.isArray(world.countries)) ? world : (window.__WORLD__ || { countries: [] });
  WORLD._byIso = new Map();
  for (const c of WORLD.countries) WORLD._byIso.set(c.iso, c);
}

/* =====================================================================
   5 · derived numbers
   ===================================================================== */
function compute() {
  EVENTS = (SNAP && SNAP.events ? SNAP.events.slice() : []);
  EVENTS.forEach(function (e) { attention(e); });
  EVENTS.sort(function (a, b) { return String(b.date || '').localeCompare(String(a.date || '')); });
  RANKED = EVENTS.slice().sort(function (a, b) { return attention(b) - attention(a); });
  HERO = RANKED.length ? RANKED[0] : null;
  LATEST = EVENTS.reduce(function (m, e) { return String(e.date || '') > m ? String(e.date || '') : m; }, '');
  LAG = lagDays(LATEST);

  LEVEL_BY_ISO = new Map();
  MARKERS = [];
  const byIso = new Map();
  for (const e of EVENTS) {
    if (!byIso.has(e.iso)) byIso.set(e.iso, []);
    byIso.get(e.iso).push(e);
  }
  byIso.forEach(function (list, iso) {
    const top = list.slice().sort(function (a, b) { return attention(b) - attention(a); })[0];
    const lvl = levelOf(attention(top));
    LEVEL_BY_ISO.set(iso, lvl);
    let maxCases = 0;
    for (const e of list) maxCases = Math.max(maxCases, num(e.cases) || 0);
    MARKERS.push({
      lat: num(top.lat) || 0, lon: num(top.lon) || 0, lvl: lvl,
      size: Math.min(1, Math.log10(maxCases + 1) / 6), iso: iso
    });
  });
  LINKS = buildLinks();
}
function lagDays(s) {
  if (!s || s.length < 10) return 0;
  const d = new Date(s.slice(0, 10) + 'T00:00:00');
  if (isNaN(d.getTime())) return 0;
  const n = new Date();
  const today = new Date(n.getFullYear(), n.getMonth(), n.getDate());
  return Math.max(0, Math.round((today - d) / 864e5));
}
function buildLinks() {
  const out = [];
  const heroDis = HERO ? HERO.disease : '';
  const groups = new Map();
  for (const e of EVENTS) {
    if (!groups.has(e.disease)) groups.set(e.disease, []);
    groups.get(e.disease).push(e);
  }
  groups.forEach(function (list, dis) {
    const seen = new Set();
    const uniq = [];
    list.slice().sort(function (a, b) { return (num(b.cases) || 0) - (num(a.cases) || 0); }).forEach(function (e) {
      if (seen.has(e.iso)) return;
      seen.add(e.iso);
      uniq.push(e);
    });
    for (let i = 1; i < uniq.length; i++) {
      let best = 0, bd = 1e9;
      for (let j = 0; j < i; j++) {
        const d = gcDist(uniq[j], uniq[i]);
        if (d < bd) { bd = d; best = j; }
      }
      out.push({ a: uniq[best], b: uniq[i], hero: dis === heroDis });
    }
  });
  return out;
}

/* =====================================================================
   6 · DOM: hero + watchlist + chips
   ===================================================================== */
function setText(sel, v) { const e = $(sel); if (e) e.textContent = v; }
function applyLanguage() {
  document.documentElement.lang = lang === 'zh' ? 'zh-CN' : lang;
  nf = new Intl.NumberFormat(locale);
  document.title = t('brand') + ' · ' + t('brandTag');
  const nodes = document.querySelectorAll('[data-t]');
  for (let i = 0; i < nodes.length; i++) nodes[i].textContent = t(nodes[i].getAttribute('data-t'));
  const brand = $('.brand-name'); if (brand) brand.textContent = t('brand');
  const tag = $('.brand-tag'); if (tag) tag.textContent = t('brandTag');
}
function buildWatchRows() {
  const box = $('#rows');
  box.innerHTML = '';
  for (let i = 0; i < 6; i++) {
    const row = document.createElement('div');
    row.className = 'row';
    row.style.top = (82 + i * 46) + 'px'; // 624 + 82 = 706
    if (i > 0) { const sep = document.createElement('div'); sep.className = 'sep'; row.appendChild(sep); }
    row.innerHTML = '<div class="dot"></div><div class="name"></div><div class="dis"></div>' +
      '<div class="cases"></div><div class="deaths"></div><div class="date"></div><div class="level"></div>';
    box.appendChild(row);
  }
}
function updateWatch() {
  const rows = document.querySelectorAll('#rows .row');
  for (let i = 0; i < rows.length; i++) {
    const row = rows[i], e = RANKED[i];
    if (!e) { row.style.display = 'none'; continue; }
    row.style.display = '';
    const s = attention(e), lv = levelOf(s);
    const dot = row.querySelector('.dot');
    dot.style.background = LV_COLOR[lv];
    row.querySelector('.name').textContent = countryName(e);
    row.querySelector('.dis').textContent = diseaseName(e.disease, e.disease_en);
    row.querySelector('.cases').textContent = num(e.cases) != null ? fmt(num(e.cases)) : (num(e.cdcLevel) ? 'CDC ' + num(e.cdcLevel) : '—');
    row.querySelector('.deaths').textContent = num(e.deaths) != null ? fmt(num(e.deaths)) : '—';
    row.querySelector('.date').textContent = mmdd(e.date);
    const lvEl = row.querySelector('.level');
    lvEl.textContent = t(['low', 'mid', 'high'][lv]);
    lvEl.style.color = LV_COLOR[lv];
  }
  const countries = new Set(EVENTS.map(function (e) { return e.iso; })).size;
  setText('#watchFoot', EVENTS.length + ' ' + t('reports') + ' / ' + countries + ' ' + t('countries') +
    '  ·  ' + t('latest') + ' ' + mmdd(LATEST) + '  ·  LAG ' + LAG + 'D');
}
function updateHero() {
  if (!HERO) {
    setText('#heroCountry', '—'); setText('#heroDisease', ''); setText('#heroRank', '');
    setText('#bigValue', '—'); setText('#bigUnit', ''); setText('#bigLabel', '');
    setText('#valCases', '—'); setText('#valCfr', '—');
    return;
  }
  const deaths = num(HERO.deaths), cases = num(HERO.cases);
  const big = deaths != null ? deaths : (cases != null ? cases : null);
  const bigStr = big == null ? '—' : fmt(big);
  setText('#heroCountry', countryName(HERO));
  setText('#heroDisease', diseaseName(HERO.disease, HERO.disease_en));
  setText('#heroMeta', cases != null ? t('cumulative') : '');
  setText('#heroRank', t('rank') + ' 01 / ' + EVENTS.length);
  setText('#bigLabel', deaths != null ? t('deaths') : (cases != null ? t('cases') : t('official')));
  setText('#bigValue', bigStr);
  setText('#bigUnit', big != null ? t('unit') : '');
  const bigEl = $('.big');
  bigEl.className = 'big' + (bigStr.length <= 6 ? '' : bigStr.length <= 7 ? ' s7' : bigStr.length <= 9 ? ' s9' : ' sx');
  setText('#valCases', cases != null ? fmt(cases) : '—');
  setText('#valCfr', (deaths != null && cases) ? fmtPct(deaths / cases * 100) : '—');
  const lv = levelOf(attention(HERO));
  const bars = document.querySelectorAll('#meter i');
  for (let i = 0; i < bars.length; i++) bars[i].className = (i < lv * 2 + 1) ? 'on' : '';
  const att = $('#attLevel');
  att.textContent = t(['low', 'mid', 'high'][lv]);
  att.style.color = LV_COLOR[lv];
  setText('#factDate', mmdd(HERO.date));
  setText('#factAsOf', mmdd(HERO.date));
  setText('#factSource', sourceName(HERO.source));
}
function updateClock() {
  const d = new Date();
  const p = function (v) { return String(v).padStart(2, '0'); };
  const s = d.getFullYear() + '-' + p(d.getMonth() + 1) + '-' + p(d.getDate()) + ' ' +
    p(d.getHours()) + ':' + p(d.getMinutes()) + ':' + p(d.getSeconds());
  if (s !== lastClock) { lastClock = s; setText('#clock', s); }
}
function updateSyncLine() {
  let sync = '—';
  const g = SNAP ? SNAP.generatedAt : null;
  if (g) {
    const d = new Date(g);
    if (!isNaN(d.getTime())) {
      const p = function (v) { return String(v).padStart(2, '0'); };
      sync = p(d.getMonth() + 1) + '-' + p(d.getDate()) + ' ' + p(d.getHours()) + ':' + p(d.getMinutes());
    }
  }
  const line = t('source') + '  ' + t('agencies') + '    ·    ' + t('sync') + ' ' + sync +
    '    ·    ' + t('lag') + ' ' + LAG + 'D';
  const el = $('#syncLine');
  el.style.whiteSpace = 'pre';
  el.textContent = line;
}

/* =====================================================================
   7 · transmission chain (SVG edges + DOM nodes)
   ===================================================================== */
const SVGNS = 'http://www.w3.org/2000/svg';
function svg(tag, attrs) {
  const e = document.createElementNS(SVGNS, tag);
  for (const k in attrs) e.setAttribute(k, attrs[k]);
  return e;
}
function toward(a, b, d) {
  let vx = b.x - a.x, vy = b.y - a.y;
  const len = Math.hypot(vx, vy) || 1;
  return { x: a.x + vx / len * d, y: a.y + vy / len * d };
}
function bez(a, c, b, u) {
  const v = 1 - u;
  return { x: v * v * a.x + 2 * v * u * c.x + u * u * b.x, y: v * v * a.y + 2 * v * u * c.y + u * u * b.y };
}
function routeCurve(a, b, from, to, nodes) {
  const mid = { x: (a.x + b.x) / 2, y: (a.y + b.y) / 2 };
  let nx = -(b.y - a.y), ny = b.x - a.x;
  const nl = Math.hypot(nx, ny) || 1;
  nx /= nl; ny /= nl;
  let best = mid, bestGap = -1e9;
  const offs = [0, 22, -22, 40, -40, 60, -60, 82, -82];
  for (const off of offs) {
    const c = { x: mid.x + nx * off, y: mid.y + ny * off };
    let gap = 1e9;
    for (let i = 2; i < 19; i++) {
      const p = bez(a, c, b, i / 20);
      for (const id in nodes) {
        const o = nodes[id];
        if (o === from || o === to) continue;
        gap = Math.min(gap, Math.hypot(p.x - o.x, p.y - o.y) - o.r,
          Math.hypot(p.x - o.x, p.y - (o.y + o.r + 10)) - 14);
      }
    }
    if (gap >= 8) return c;
    if (gap > bestGap) { bestGap = gap; best = c; }
  }
  return best;
}

function ChainView() {
  this.nodes = {};
  this.edges = [];
  this.roots = [];
  this.hotDots = [];
  this.loopEl = null;
  this.p2pEl = null;
  this.hot = new Set();
  this.loopHot = false;
}
ChainView.prototype.build = function () {
  const self = this;
  const box = $('#chainNodes');
  box.innerHTML = '';
  const svgEl = $('#chainEdges');
  while (svgEl.firstChild) svgEl.removeChild(svgEl.firstChild);

  const defs = svg('defs');
  [['arw', '#343B3E'], ['arw-hot', '#C0392B']].forEach(function (m) {
    const mk = svg('marker', {
      id: m[0], viewBox: '0 0 8 8', refX: 7.4, refY: 4,
      markerWidth: 9, markerHeight: 9, markerUnits: 'userSpaceOnUse', orient: 'auto'
    });
    mk.appendChild(svg('path', { d: 'M0,0 L8,4 L0,8 Z', fill: m[1] }));
    defs.appendChild(mk);
  });
  svgEl.appendChild(defs);

  const raw = (TRANS && TRANS.nodes) ? TRANS.nodes : [];
  const W = 398;
  const rowY = [128, 226, 322];
  for (const n of raw) {
    const row = raw.filter(function (x) { return x.row === n.row; });
    const i = row.indexOf(n);
    const margin = n.row === 0 ? 40 : 72;
    const x = row.length === 1 ? W / 2 : margin + i * (W - 2 * margin) / (row.length - 1);
    this.nodes[n.id] = { id: n.id, name: n.name, glyph: n.glyph, row: n.row, x: x, y: rowY[n.row] || 322, r: n.row === 2 ? 30 : 22 };
  }

  const seen = new Set();
  const pairs = [];
  for (const d of (TRANS && TRANS.diseases) ? TRANS.diseases : []) {
    for (const step of (d.path || [])) {
      const a = step[0], b = step[1];
      if (!this.nodes[a] || !this.nodes[b]) continue;
      const key = a + '>' + b;
      if (seen.has(key)) continue;
      seen.add(key);
      pairs.push({ key: key, a: a, b: b, dis: d.name });
    }
  }
  for (const pr of pairs) {
    const A = this.nodes[pr.a], B = this.nodes[pr.b];
    const c = routeCurve(A, B, A, B, this.nodes);
    const pa = toward(A, c, A.r + 2), pb = toward(B, c, B.r + 4);
    const path = svg('path', {
      d: 'M' + pa.x.toFixed(2) + ',' + pa.y.toFixed(2) + ' Q' + c.x.toFixed(2) + ',' + c.y.toFixed(2) + ' ' + pb.x.toFixed(2) + ',' + pb.y.toFixed(2),
      class: 'edge', 'marker-end': 'url(#arw)'
    });
    svgEl.appendChild(path);
    this.edges.push({ key: pr.key, a: pr.a, b: pr.b, c: c, pa: pa, pb: pb, el: path, dot: null });
  }

  // person-to-person loop beside the human node
  const hp = this.nodes.human;
  if (hp) {
    const loop = svg('path', {
      d: 'M' + (hp.x + hp.r - 4) + ',' + (hp.y - 14) + ' A22,18 0 1 1 ' + (hp.x + hp.r - 4) + ',' + (hp.y + 14),
      class: 'loop'
    });
    svgEl.appendChild(loop);
    this.loopEl = loop;
    const label = svg('text', { x: hp.x + hp.r + 38, y: hp.y + 4, class: 'p2p' });
    this.p2pEl = label;
    svgEl.appendChild(label);
  }

  for (const n of raw) {
    const cn = this.nodes[n.id];
    if (!cn) continue;
    const el = document.createElement('div');
    el.className = 'node' + (n.id === 'human' ? ' human' : '');
    el.style.left = (cn.x - cn.r) + 'px';
    el.style.top = (cn.y - cn.r) + 'px';
    el.innerHTML = '<span class="glyph"></span><span class="label"></span>';
    el.querySelector('.glyph').textContent = n.glyph;
    el.querySelector('.label').textContent = hostName(n.id, n.name);
    box.appendChild(el);
    cn.el = el;
    cn.labelEl = el.querySelector('.label');
    cn.glyphEl = el.querySelector('.glyph');
  }
};
ChainView.prototype.apply = function () {
  const self = this;
  const route = (TRANS && TRANS.diseases) ? TRANS.diseases.filter(function (d) { return d.name === (HERO ? HERO.disease : ''); })[0] : null;
  const hot = new Set();
  if (route) for (const step of (route.path || [])) hot.add(step[0] + '>' + step[1]);
  const lit = new Set();
  for (const key of hot) { const p = key.split('>'); lit.add(p[0]); lit.add(p[1]); }
  this.loopHot = !!(route && String(route.personToPerson) === 'true');
  if (this.loopHot) lit.add('human');

  for (const e of this.edges) {
    const on = hot.has(e.key);
    e.el.className.baseVal = 'edge' + (on ? ' hot' : '');
    e.el.setAttribute('marker-end', on ? 'url(#arw-hot)' : 'url(#arw)');
    if (on && !e.dot) {
      e.dot = svg('circle', { r: 3, class: 'flow', cx: -20, cy: -20 });
      e.phase = this.hotDots.length * 0.34;
      $('#chainEdges').appendChild(e.dot);
      this.hotDots.push(e);
    } else if (!on && e.dot) {
      e.dot.remove(); e.dot = null;
      this.hotDots = this.hotDots.filter(function (x) { return x !== e; });
    }
  }
  if (this.loopEl) this.loopEl.className.baseVal = 'loop' + (this.loopHot ? ' hot' : '');
  if (this.p2pEl) {
    this.p2pEl.textContent = t('p2p');
    this.p2pEl.className.baseVal = 'p2p' + (this.loopHot ? ' hot' : '');
  }
  for (const id in this.nodes) {
    const n = this.nodes[id];
    if (!n.el) continue;
    const on = lit.has(id);
    n.el.className = 'node' + (id === 'human' ? ' human' : '') + (on ? ' on' : '');
    n.labelEl.textContent = hostName(id, n.name);
  }
  // chips
  const chips = $('#chips');
  chips.innerHTML = '';
  const names = [];
  if (HERO) names.push(HERO.disease);
  for (const e of RANKED) names.push(e.disease);
  const dEn = new Map();
  for (const e of EVENTS) { if (e.disease && e.disease_en && !dEn.has(e.disease)) dEn.set(e.disease, e.disease_en); }
  const added = new Set();
  for (const d of names) {
    if (!d || added.has(d)) continue;
    added.add(d);
    const c = document.createElement('span');
    c.className = 'chip' + (HERO && d === HERO.disease ? ' on' : '');
    c.textContent = diseaseName(d, dEn.get(d));
    chips.appendChild(c);
    if (chips.scrollWidth > 342) { c.remove(); break; }
  }
  const title = $('#chainTitle');
  title.textContent = HERO ? diseaseName(HERO.disease, HERO.disease_en) : '';
};
ChainView.prototype.tick = function (time) {
  const off = -time * 14;
  for (const e of this.hotDots) {
    const u = ((time * 0.32 + (e.phase || 0)) % 1 + 1) % 1;
    const p = bez(e.pa, e.c, e.pb, u);
    e.dot.setAttribute('cx', p.x.toFixed(2));
    e.dot.setAttribute('cy', p.y.toFixed(2));
    e.el.style.strokeDashoffset = off + 'px';
  }
  if (this.loopEl && this.loopHot) this.loopEl.style.strokeDashoffset = off + 'px';
};

/* =====================================================================
   8 · the globe (canvas 2D, orthographic, lit)
   ===================================================================== */
function GlobeView(canvas) {
  this.cv = canvas;
  this.ctx = canvas.getContext('2d', { alpha: true });
  this.ctx.imageSmoothingEnabled = true;
  this.ctx.imageSmoothingQuality = 'high';
  this.W = 1000; this.H = 1080;
  this.cx = 478; this.cy = 540; this.R = 430;
  this.q = 1;
  this.countries = [];
  this.maxN = 0;
  this.X = null; this.Y = null; this.Z = null;
  this.lon0 = 7; this.tilt = 20;
  this.markers = [];
  this.links = [];
  this.hero = null;
  this.levels = new Map();
  this.gOcean = null; this.gLight = null;
  this.reticlePhase = 0;
}
GlobeView.prototype.setWorld = function (world) {
  this.countries = [];
  this.maxN = 0;
  for (const c of (world && world.countries ? world.countries : [])) {
    const rings = [];
    for (const ring of (c.rings || [])) {
      const n = ring.length;
      if (n < 3) continue;
      const vx = new Float32Array(n), vy = new Float32Array(n), vz = new Float32Array(n), lon = new Float32Array(n);
      for (let i = 0; i < n; i++) {
        const lo = ring[i][0] * Math.PI / 180, la = ring[i][1] * Math.PI / 180;
        const cl = Math.cos(la);
        vx[i] = cl * Math.sin(lo); vy[i] = Math.sin(la); vz[i] = cl * Math.cos(lo); lon[i] = ring[i][0];
      }
      rings.push({ n: n, vx: vx, vy: vy, vz: vz, lon: lon });
      if (n > this.maxN) this.maxN = n;
    }
    if (rings.length) this.countries.push({ iso: c.iso, rings: rings });
  }
  this.X = new Float32Array(this.maxN);
  this.Y = new Float32Array(this.maxN);
  this.Z = new Float32Array(this.maxN);
};
GlobeView.prototype.setData = function (markers, links, levels, hero) {
  this.markers = markers; this.links = links; this.levels = levels; this.hero = hero;
};
GlobeView.prototype.resize = function (q) {
  if (q === this.q && this.cv.width > 0 && this.gOcean) return;
  this.q = q;
  this.cv.width = Math.round(this.W * q);
  this.cv.height = Math.round(this.H * q);
  /* resizing the backing store resets the context state */
  this.ctx.imageSmoothingEnabled = true;
  this.ctx.imageSmoothingQuality = 'high';
  this.buildGradients();
};
GlobeView.prototype.buildGradients = function () {
  const ctx = this.ctx, cx = this.cx, cy = this.cy, R = this.R;
  const og = ctx.createRadialGradient(cx - R * 0.32, cy - R * 0.34, 0, cx, cy, R);
  og.addColorStop(0, '#141719');
  og.addColorStop(0.42, '#0E1113');
  og.addColorStop(0.78, '#090B0C');
  og.addColorStop(1, '#060708');
  this.gOcean = og;
  const lg = ctx.createRadialGradient(cx - R * 0.26, cy - R * 0.32, 0, cx - R * 0.26, cy - R * 0.32, R * 1.18);
  lg.addColorStop(0, '#EDEFEE');
  lg.addColorStop(0.35, '#C2C5C4');
  lg.addColorStop(0.70, '#7B8182');
  lg.addColorStop(1, '#2C3134');
  this.gLight = lg;
};
GlobeView.prototype.project = function (vx, vy, vz, c0, s0, ct, st) {
  const x = vx * c0 - vz * s0;
  const zr = vx * s0 + vz * c0;
  const y2 = vy * ct - zr * st;
  const z2 = vy * st + zr * ct;
  return { x: this.cx + this.R * x, y: this.cy - this.R * y2, z: z2 };
};
GlobeView.prototype.draw = function (time) {
  const ctx = this.ctx;
  const q = this.q;
  ctx.setTransform(q, 0, 0, q, 0, 0);
  ctx.clearRect(0, 0, this.W, this.H);

  const slow = time * Math.PI * 2 / 300;
  this.lon0 = 7 + 10 * Math.sin(slow);
  this.tilt = 20 + 1.0 * Math.sin(time * Math.PI * 2 / 210);
  this.reticlePhase = (Math.sin(time * 2.1) + 1) / 2;

  const lon0 = this.lon0 * Math.PI / 180, tilt = this.tilt * Math.PI / 180;
  const c0 = Math.cos(lon0), s0 = Math.sin(lon0), ct = Math.cos(tilt), st = Math.sin(tilt);
  const cx = this.cx, cy = this.cy, R = this.R;
  const X = this.X, Y = this.Y, Z = this.Z;

  const pBase = new Path2D(), pMid = new Path2D(), pHigh = new Path2D();
  const pStroke = new Path2D(), pHighStroke = new Path2D();

  function projRing(ring) {
    const vx = ring.vx, vy = ring.vy, vz = ring.vz;
    for (let i = 0; i < ring.n; i++) {
      const a = vx[i], b = vy[i], c = vz[i];
      const x = a * c0 - c * s0;
      const zr = a * s0 + c * c0;
      X[i] = cx + R * x;
      Y[i] = cy - R * (b * ct - zr * st);
      Z[i] = b * st + zr * ct;
    }
  }
  function horizon(ring, i, prevZ, curZ) {
    const t = prevZ / (prevZ - curZ);
    let a = ring.vx[i - 1] + (ring.vx[i] - ring.vx[i - 1]) * t;
    let b = ring.vy[i - 1] + (ring.vy[i] - ring.vy[i - 1]) * t;
    let c = ring.vz[i - 1] + (ring.vz[i] - ring.vz[i - 1]) * t;
    const l = Math.hypot(a, b, c) || 1;
    a /= l; b /= l; c /= l;
    const x = a * c0 - c * s0;
    const zr = a * s0 + c * c0;
    return { x: cx + R * x, y: cy - R * (b * ct - zr * st) };
  }

  for (const country of this.countries) {
    const lvl = this.levels.has(country.iso) ? this.levels.get(country.iso) : -1;
    const isHigh = lvl === 2;
    const fp = isHigh ? pHigh : (lvl === 1 ? pMid : pBase);
    for (const ring of country.rings) {
      projRing(ring);
      let open = false, prevVis = false, prevZ = 0;
      let prevLon = ring.lon[0];
      for (let i = 0; i < ring.n; i++) {
        if (i > 0 && Math.abs(ring.lon[i] - prevLon) > 180) {
          if (open) { fp.closePath(); pStroke.closePath(); if (isHigh) pHighStroke.closePath(); open = false; }
        }
        prevLon = ring.lon[i];
        const z = Z[i], x = X[i], y = Y[i];
        if (z > 0) {
          if (!open || !prevVis) {
            fp.moveTo(x, y); pStroke.moveTo(x, y); if (isHigh) pHighStroke.moveTo(x, y);
            open = true;
          } else {
            fp.lineTo(x, y); pStroke.lineTo(x, y); if (isHigh) pHighStroke.lineTo(x, y);
          }
        } else if (prevVis && open) {
          const h = horizon(ring, i, prevZ, z);
          fp.lineTo(h.x, h.y); pStroke.lineTo(h.x, h.y); if (isHigh) pHighStroke.lineTo(h.x, h.y);
          fp.closePath(); pStroke.closePath(); if (isHigh) pHighStroke.closePath();
          open = false;
        }
        prevVis = z > 0;
        prevZ = z;
      }
      if (open) { fp.closePath(); pStroke.closePath(); if (isHigh) pHighStroke.closePath(); }
    }
  }

  // ---- sphere ----
  ctx.save();
  ctx.beginPath();
  ctx.arc(cx, cy, R, 0, Math.PI * 2);
  ctx.clip();
  ctx.fillStyle = this.gOcean;
  ctx.fillRect(cx - R, cy - R, R * 2, R * 2);
  ctx.fillStyle = '#4E575A';
  ctx.fill(pBase);
  ctx.fillStyle = '#576063';
  ctx.fill(pMid);
  ctx.globalCompositeOperation = 'multiply';
  ctx.fillStyle = this.gLight;
  ctx.fillRect(cx - R, cy - R, R * 2, R * 2);
  ctx.globalCompositeOperation = 'source-over';
  ctx.fillStyle = '#320D0E';
  ctx.fill(pHigh);

  /* country borders: the wider low-alpha pass is the +0.5px compensation;
     the 0.65px pass keeps the hairline from turning into a smear. */
  ctx.strokeStyle = 'rgba(216,212,200,0.06)';
  ctx.lineWidth = 1.15;
  ctx.stroke(pStroke);
  ctx.strokeStyle = 'rgba(216,212,200,0.16)';
  ctx.lineWidth = 0.65;
  ctx.stroke(pStroke);

  /* featured countries: outer edge + an inner rim clipped to the fill, so the
     highlight keeps a sharp inner contour without any blur filter */
  ctx.strokeStyle = 'rgba(192,57,43,0.62)';
  ctx.lineWidth = 1;
  ctx.stroke(pHighStroke);
  ctx.save();
  ctx.clip(pHigh);
  ctx.strokeStyle = 'rgba(214,72,56,0.55)';
  ctx.lineWidth = 2.4;
  ctx.stroke(pHighStroke);
  ctx.restore();

  // graticule
  ctx.strokeStyle = 'rgba(121,128,127,0.13)';
  ctx.lineWidth = 0.7;
  const grid = new Path2D();
  for (let lon = -180; lon < 180; lon += 30) {
    let started = false;
    for (let lat = -84; lat <= 84; lat += 4) {
      const la = lat * Math.PI / 180, lo = lon * Math.PI / 180;
      const p = this.project(Math.cos(la) * Math.sin(lo), Math.sin(la), Math.cos(la) * Math.cos(lo), c0, s0, ct, st);
      if (p.z > 0) { if (!started) { grid.moveTo(p.x, p.y); started = true; } else grid.lineTo(p.x, p.y); }
      else started = false;
    }
  }
  for (let lat = -60; lat <= 60; lat += 30) {
    let started = false;
    const la = lat * Math.PI / 180;
    for (let lon = -180; lon <= 180; lon += 4) {
      const lo = lon * Math.PI / 180;
      const p = this.project(Math.cos(la) * Math.sin(lo), Math.sin(la), Math.cos(la) * Math.cos(lo), c0, s0, ct, st);
      if (p.z > 0) { if (!started) { grid.moveTo(p.x, p.y); started = true; } else grid.lineTo(p.x, p.y); }
      else started = false;
    }
  }
  ctx.stroke(grid);
  ctx.restore();

  // rim + ticks
  ctx.beginPath();
  ctx.arc(cx, cy, R - 0.5, 0, Math.PI * 2);
  ctx.strokeStyle = '#23272A';
  ctx.lineWidth = 1;
  ctx.stroke();
  ctx.beginPath();
  for (let k = 0; k < 4; k++) {
    const a = k * Math.PI / 2;
    ctx.moveTo(cx + Math.cos(a) * (R + 3), cy + Math.sin(a) * (R + 3));
    ctx.lineTo(cx + Math.cos(a) * (R + 11), cy + Math.sin(a) * (R + 11));
  }
  ctx.strokeStyle = '#2E3538';
  ctx.stroke();

  // ---- arcs ----
  for (const l of this.links) {
    const a = l.a, b = l.b;
    const la1 = a.lat * Math.PI / 180, lo1 = a.lon * Math.PI / 180;
    const la2 = b.lat * Math.PI / 180, lo2 = b.lon * Math.PI / 180;
    const ax = Math.cos(la1) * Math.sin(lo1), ay = Math.sin(la1), az = Math.cos(la1) * Math.cos(lo1);
    const bx = Math.cos(la2) * Math.sin(lo2), by = Math.sin(la2), bz = Math.cos(la2) * Math.cos(lo2);
    let dot = Math.max(-1, Math.min(1, ax * bx + ay * by + az * bz));
    const ang = Math.acos(dot);
    const sinA = Math.sin(ang);
    ctx.beginPath();
    let started = false;
    let first = null, last = null;
    const N = 44;
    for (let i = 0; i <= N; i++) {
      const u = i / N;
      let vx, vy, vz;
      if (sinA < 1e-4) {
        vx = ax + (bx - ax) * u; vy = ay + (by - ay) * u; vz = az + (bz - az) * u;
      } else {
        const w1 = Math.sin((1 - u) * ang) / sinA, w2 = Math.sin(u * ang) / sinA;
        vx = ax * w1 + bx * w2; vy = ay * w1 + by * w2; vz = az * w1 + bz * w2;
      }
      const lift = 1 + 0.035 + 0.15 * Math.sin(Math.PI * u);
      const p = this.project(vx * lift, vy * lift, vz * lift, c0, s0, ct, st);
      if (p.z > 0) {
        if (!started) { ctx.moveTo(p.x, p.y); started = true; } else ctx.lineTo(p.x, p.y);
        if (i === 0) first = p;
        last = p;
      } else started = false;
    }
    ctx.strokeStyle = l.hero ? 'rgba(140,28,28,0.62)' : 'rgba(58,66,69,0.95)';
    ctx.lineWidth = l.hero ? 1.2 : 1;
    ctx.stroke();
  }

  // ---- markers ----
  for (const m of this.markers) {
    const la = m.lat * Math.PI / 180, lo = m.lon * Math.PI / 180;
    const p = this.project(Math.cos(la) * Math.sin(lo), Math.sin(la), Math.cos(la) * Math.cos(lo), c0, s0, ct, st);
    if (p.z <= 0.01) continue;
    const color = LV_COLOR[m.lvl];
    const r = 6 + 6.5 * m.size;
    ctx.beginPath();
    ctx.arc(p.x, p.y, r, 0, Math.PI * 2);
    ctx.strokeStyle = color; ctx.globalAlpha = 0.7; ctx.lineWidth = 1; ctx.stroke();
    ctx.globalAlpha = 1;
    ctx.beginPath();
    ctx.arc(p.x, p.y, 2.1, 0, Math.PI * 2);
    ctx.fillStyle = color; ctx.fill();
    m._x = p.x; m._y = p.y; m._z = p.z;
  }

  // ---- hero halo, reticle and flowing dots ----
  if (this.hero) {
    const h = EVENTS.indexOf(this.hero) >= 0 ? this.hero : null;
    if (h) {
      const la = h.lat * Math.PI / 180, lo = h.lon * Math.PI / 180;
      const p = this.project(Math.cos(la) * Math.sin(lo), Math.sin(la), Math.cos(la) * Math.cos(lo), c0, s0, ct, st);
      if (p.z > 0) {
        const halo = ctx.createRadialGradient(p.x, p.y, 0, p.x, p.y, 46);
        halo.addColorStop(0, 'rgba(140,28,28,0.20)');
        halo.addColorStop(1, 'rgba(140,28,28,0)');
        ctx.fillStyle = halo;
        ctx.fillRect(p.x - 46, p.y - 46, 92, 92);
        const pulse = 0.55 + 0.45 * this.reticlePhase;
        ctx.strokeStyle = 'rgba(140,28,28,' + pulse.toFixed(3) + ')';
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.arc(p.x, p.y, 8, 0, Math.PI * 2);
        ctx.stroke();
        ctx.beginPath();
        for (let k = 0; k < 4; k++) {
          const a = k * Math.PI / 2;
          ctx.moveTo(p.x + Math.cos(a) * 11, p.y + Math.sin(a) * 11);
          ctx.lineTo(p.x + Math.cos(a) * 16, p.y + Math.sin(a) * 16);
        }
        ctx.stroke();
        this.heroScreen = p;
      } else this.heroScreen = null;
    }
  }

  // flowing dots on the featured disease's arcs
  for (let k = 0; k < this.links.length; k++) {
    const l = this.links[k];
    if (!l.hero) continue;
    const a = l.a, b = l.b;
    const la1 = a.lat * Math.PI / 180, lo1 = a.lon * Math.PI / 180;
    const la2 = b.lat * Math.PI / 180, lo2 = b.lon * Math.PI / 180;
    const ax = Math.cos(la1) * Math.sin(lo1), ay = Math.sin(la1), az = Math.cos(la1) * Math.cos(lo1);
    const bx = Math.cos(la2) * Math.sin(lo2), by = Math.sin(la2), bz = Math.cos(la2) * Math.cos(lo2);
    const dot = Math.max(-1, Math.min(1, ax * bx + ay * by + az * bz));
    const ang = Math.acos(dot), sinA = Math.sin(ang);
    const u = ((time * 0.16 + k * 0.31) % 1 + 1) % 1;
    let vx, vy, vz;
    if (sinA < 1e-4) { vx = ax + (bx - ax) * u; vy = ay + (by - ay) * u; vz = az + (bz - az) * u; }
    else {
      const w1 = Math.sin((1 - u) * ang) / sinA, w2 = Math.sin(u * ang) / sinA;
      vx = ax * w1 + bx * w2; vy = ay * w1 + by * w2; vz = az * w1 + bz * w2;
    }
    const lift = 1 + 0.035 + 0.15 * Math.sin(Math.PI * u);
    const p = this.project(vx * lift, vy * lift, vz * lift, c0, s0, ct, st);
    if (p.z > 0) {
      ctx.save();
      ctx.shadowColor = 'rgba(192,57,43,0.9)';
      ctx.shadowBlur = 7;
      ctx.fillStyle = '#C0392B';
      ctx.beginPath();
      ctx.arc(p.x, p.y, 1.8, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();
    }
  }
};

/* =====================================================================
   9 · boot
   ===================================================================== */
function fitStage() {
  const vw = window.innerWidth, vh = window.innerHeight;
  const s = Math.min(vw / 1920, vh / 1080);
  /* contain-scale the 1920x1080 design; the leftover margin becomes pure-black
     matte that the letterbox bars absorb (see --padx/--pady in style.css). */
  const padX = s > 0 ? Math.max(0, (vw - 1920 * s) / (2 * s)) : 0;
  const padY = s > 0 ? Math.max(0, (vh - 1080 * s) / (2 * s)) : 0;
  const stage = $('#stage');
  if (stage) {
    /* integer design-pixel matte/furniture positions; pady is kept even so the
       bars' half-pady text offsets stay on whole pixels too */
    const padXR = Math.ceil(padX - 1e-6);
    const padYR = Math.ceil(padY / 2 - 1e-6) * 2;
    stage.style.transform = 'translate(-50%,-50%) scale(' + s + ')';
    stage.style.setProperty('--padx', padXR + 'px');
    stage.style.setProperty('--pady', padYR + 'px');
  }
  if (globe) {
    const dpr = window.devicePixelRatio || 1;
    /* 1.25x supersampling above the composited CSS size; 2.0 caps the cost on
       hidpi panels. ctx is scaled with setTransform(q), never by upscaling. */
    const q = Math.max(1, Math.min(2, s * dpr * 1.25));
    globe.resize(q);
  }
}

function startClock() {
  updateClock();
  updateSyncLine();
  setInterval(updateClock, 250);
}

function frame(ts) {
  requestAnimationFrame(frame);
  const time = ts / 1000;
  if (lastDraw < 0) lastDraw = time;
  if (time - lastDraw < 0.0233) return;
  const t0 = performance.now();
  lastDraw = time;
  if (globe) globe.draw(time);
  if (chainView) chainView.tick(time);
  const dur = performance.now() - t0;
  const p = window.__perf;
  p.frames++; p.total += dur; p.last = dur;
  if (dur > p.max) p.max = dur;
  p.t = time;
}

async function boot() {
  lang = pickLang();
  locale = LOCALE[lang] || 'en-GB';
  nf = new Intl.NumberFormat(locale);
  $('#stage').style.opacity = '0';
  await loadAll();
  compute();

  applyLanguage();
  buildWatchRows();
  updateWatch();
  updateHero();

  globe = new GlobeView($('#globe'));
  globe.setWorld(WORLD);
  globe.resize(1);
  globe.setData(MARKERS, LINKS, LEVEL_BY_ISO, HERO);

  chainView = new ChainView();
  chainView.build();
  chainView.apply();

  updateSyncLine();
  startClock();

  window.__perf = { frames: 0, total: 0, last: 0, max: 0, t: 0, start: performance.now() };
  window.__app = {
    lang: lang, schema: SNAP ? SNAP.schema : null, events: EVENTS.length,
    countries: new Set(EVENTS.map(function (e) { return e.iso; })).size,
    hero: HERO, ranked: RANKED.length,
    worldCountries: WORLD.countries.length, lag: LAG, latest: LATEST
  };
  if (/[?&]debug\b/.test(location.search)) { window.__globe = globe; window.__chain = chainView; }
  fitStage();
  window.addEventListener('resize', fitStage);
  requestAnimationFrame(frame);
  $('#stage').style.transition = 'opacity .9s ease';
  $('#stage').style.opacity = '1';
}

/* fullscreen: browsers need a gesture, so the first click/keypress arms it. */
function goFullscreen() {
  const el = document.documentElement;
  const fn = el.requestFullscreen || el.webkitRequestFullscreen;
  if (!fn) return;
  try { const p = fn.call(el); if (p && p.catch) p.catch(function () {}); } catch (e) {}
}
document.addEventListener('click', function () {
  if (!document.fullscreenElement) goFullscreen();
});
window.addEventListener('keydown', function (e) {
  if (e.key === 'f' || e.key === 'F') {
    if (!document.fullscreenElement) goFullscreen();
    else if (document.exitFullscreen) document.exitFullscreen();
  }
});
let idleTimer = null;
function wake() {
  document.body.classList.remove('idle');
  clearTimeout(idleTimer);
  idleTimer = setTimeout(function () { document.body.classList.add('idle'); }, 2600);
}
['mousemove', 'mousedown', 'keydown', 'touchstart'].forEach(function (ev) {
  window.addEventListener(ev, wake, { passive: true });
});
wake();

boot().catch(function (err) {
  if (window.console && console.error) console.error(err);
  const stage = $('#stage');
  if (stage) stage.style.opacity = '';
});
})();
