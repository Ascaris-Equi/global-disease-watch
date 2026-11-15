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
let EVENTS = [], RANKED = [], HERO = null, LATEST = '', LAG = 0, rankPage = 0;
let LEVEL_BY_ISO = new Map(), MARKERS = [], LINKS = [];
let CONN = new Map();
let globe = null, chainView = null;
let lastDraw = -1;
let lastClock = '';

/* ---------------------------------------------------------------------
   portrait / mobile mode
   The landscape stage is the fixed 1920x1080 design. A portrait viewport
   gets a separate 1080x1920 (9:16) design that is scaled to the viewport
   width; the two layouts never mix. The test is an aspect test, matching
   the `orientation: portrait` CSS breakpoint.
   --------------------------------------------------------------------- */
function isPortrait() {
  return window.innerWidth < window.innerHeight;
}
const MOBILE_BOOT = isPortrait();     /* phone texture size is fixed at boot */
const M_W = 1080;                     /* portrait design base, 9:16 */
const M_HERO_H = 446;                 /* portrait band heights, design px */
const M_WATCH_H = 300;
let pageRows = MOBILE_BOOT ? 3 : 6;   /* ranked rows per watchlist page */
let rowTop = MOBILE_BOOT ? 112 : 82;  /* first row y inside .watch */
let rowStep = MOBILE_BOOT ? 60 : 46;

/* hero rotation + globe focus easing, matching the WPF screen: one report every
   9 s, a 350 ms opacity fade each way, and a 2.4 s smoothstep turn of the globe. */
const HERO_ROWS = 6;        // rotation runs over the six rows this page shows
const HERO_ROT_MS = 9000;   // 9 s per report
const HERO_FADE_MS = 175;   // fade-out and fade-in each take 350 ms
const GLOBE_EASE_S = 2.4;   // seconds for the globe to reach the new focus
let heroFadeBusy = false;
let heroSwapTimer = null;
let heroTimer = null;

const $ = function (s) { return document.querySelector(s); };
const LV_COLOR = ['#79807F', '#C9A227', '#C0392B'];

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
/* snapshot v3 moved cases/deaths under "metrics"; keep reading the flat fields first
   so the screen still works with older snapshots. */
function metricNum(e, k) {
  if (!e) return null;
  const direct = num(e[k]);
  if (direct != null) return direct;
  return (e.metrics && e.metrics[k] != null) ? num(e.metrics[k]) : null;
}
function wrapLon(d) { d = d % 360; if (d > 180) d -= 360; else if (d < -180) d += 360; return d; }
function smoothstep(u) { u = u < 0 ? 0 : (u > 1 ? 1 : u); return u < 0.5 ? 4 * u * u * u : 1 - Math.pow(-2 * u + 2, 3) / 2; }
function pad2(n) { return n < 10 ? '0' + n : String(n); }
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
/* Step a single line down until it stops clipping its box. Only the portrait
   layout calls this: its type scale is about twice the landscape design's, so
   a long country name or data source would otherwise be cut with an ellipsis.
   In landscape the inline size is cleared again, leaving the stylesheet alone. */
function fitText(el, minPx, boxEl) {
  if (!el) return;
  el.style.fontSize = '';
  if (!isPortrait()) return;
  let size = parseFloat(getComputedStyle(el).fontSize);
  if (!isFinite(size)) return;
  let guard = 0;
  while (size > minPx && guard++ < 60) {
    const clipped = el.scrollWidth > el.clientWidth + 1;
    let outside = false;
    if (boxEl) {
      const r = el.getBoundingClientRect(), b = boxEl.getBoundingClientRect();
      outside = r.right > b.right + 1;
    }
    if (!clipped && !outside) break;
    size -= 2;
    el.style.fontSize = size + 'px';
  }
}
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
  for (let i = 0; i < pageRows; i++) {
    const row = document.createElement('div');
    row.className = 'row';
    row.style.top = (rowTop + i * rowStep) + 'px'; // landscape: 624 + 82 = 706
    if (i > 0) { const sep = document.createElement('div'); sep.className = 'sep'; row.appendChild(sep); }
    row.innerHTML = '<div class="dot"></div><div class="name"></div><div class="dis"></div>' +
      '<div class="cases"></div><div class="deaths"></div><div class="date"></div><div class="level"></div>';
    box.appendChild(row);
  }
}
function updateWatch() {
  const rows = document.querySelectorAll('#rows .row');
  for (let i = 0; i < rows.length; i++) {
    const row = rows[i], e = RANKED[rankPage * pageRows + i];
    if (!e) { row.style.display = 'none'; continue; }
    row.style.display = '';
    const s = attention(e), lv = levelOf(s);
    const dot = row.querySelector('.dot');
    dot.style.background = LV_COLOR[lv];
    const rowName = row.querySelector('.name'), rowDis = row.querySelector('.dis'), rowCases = row.querySelector('.cases');
    rowName.textContent = countryName(e);
    rowDis.textContent = diseaseName(e.disease, e.disease_en);
    const nCases = metricNum(e, 'cases'), nDeaths = metricNum(e, 'deaths');
    rowCases.textContent = nCases != null ? fmt(nCases) : (num(e.cdcLevel) ? 'CDC ' + num(e.cdcLevel) : '—');
    row.querySelector('.deaths').textContent = nDeaths != null ? fmt(nDeaths) : '—';
    row.querySelector('.date').textContent = mmdd(e.date);
    const lvEl = row.querySelector('.level');
    lvEl.textContent = t(['low', 'mid', 'high'][lv]);
    lvEl.style.color = LV_COLOR[lv];
    row.classList.toggle('on', !!(HERO && e === HERO));
    /* portrait type is larger relative to the screen, so a long country or
       disease name gets its size stepped down instead of being cut off */
    fitText(rowName, 18);
    fitText(rowDis, 16);
    fitText(rowCases, 20);
  }
    const firstRow = rankPage * pageRows + 1, lastRow = Math.min((rankPage + 1) * pageRows, RANKED.length);
    const kick = document.querySelector(".watch-kicker");
    if (kick) kick.textContent = "WATCHLIST\u00A0\u00A0" + pad2(firstRow) + "\u2013" + pad2(lastRow) + " / " + pad2(RANKED.length);
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
  const deaths = metricNum(HERO, 'deaths'), cases = metricNum(HERO, 'cases');
  const big = deaths != null ? deaths : (cases != null ? cases : null);
  const bigStr = big == null ? '—' : fmt(big);
  const ri = RANKED.indexOf(HERO);
  setText('#heroCountry', countryName(HERO));
  setText('#heroDisease', diseaseName(HERO.disease, HERO.disease_en));
  setText('#heroMeta', cases != null ? t('cumulative') : '');
  setText('#heroRank', ri >= 0 ? t('rank') + ' ' + pad2(ri + 1) + ' / ' + RANKED.length : '');
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
  const heroBox = $('.hero');
  fitText($('#heroCountry'), 44, heroBox);
  fitText($('#heroDisease'), 22, heroBox);
  fitText($('.big'), 56, heroBox);
}
/* ---------------------------------------------------------------
   hero rotation · every 9 s the focus moves to the next of the six
   ranked rows. The text fields fade out (350 ms), are swapped while
   fully transparent, then fade back in; the globe starts its 2.4 s
   ease at the same moment the new text lands.
   --------------------------------------------------------------- */
function heroRows() {
  /* Rotation picks entries that carry figures, then reorders greedily so two
     consecutive slides rarely show the same disease: four cholera slides in a
     row read like the map is stuck. Ranking order still drives the selection,
     only the tie-breaking does the spreading. */
  const pool = RANKED.filter(function (e) {
    return metricNum(e, "deaths") != null || metricNum(e, "cases") != null;
  });
  if (pool.length < 3) return RANKED.slice(0, HERO_ROWS);
  const rest = pool.slice(), list = [];
  let lastDisease = null;
  while (list.length < HERO_ROWS && rest.length) {
    let idx = 0;
    for (let i = 0; i < rest.length; i++) {
      if (rest[i].disease !== lastDisease) { idx = i; break; }
    }
    const pick = rest.splice(idx, 1)[0];
    list.push(pick);
    lastDisease = pick.disease;
  }
  return list;
}

function heroLonOf(e) { const v = e ? num(e.lon) : null; return v == null ? 7 : v; }

function showHero(entry) {
  HERO = entry || null;
  /* the table is `pageRows` rows tall, so it pages to whatever page holds the
     focus row; otherwise the highlight would point at a row that is not shown.
     landscape: 6 rows/page, portrait: 3 rows/page. */
  const heroIdx = HERO ? RANKED.indexOf(HERO) : 0;
  rankPage = heroIdx >= 0 ? Math.floor(heroIdx / pageRows) : 0;
  /* arcs, chips and the transmission chain all hang off the featured disease */
  LINKS = buildLinks();
  if (globe) {
    globe.setData(MARKERS, LINKS, LEVEL_BY_ISO, HERO);
    globe.focusTo(heroLonOf(HERO));
  }
  updateHero();
  updateWatch();
  if (chainView) chainView.apply();
}

function advanceHero() {
  const list = heroRows();
  if (list.length < 2 || heroFadeBusy) return;
  let i = list.indexOf(HERO);
  if (i < 0) i = 0;
  const next = list[(i + 1) % list.length];
  const hero = $('.hero');
  heroFadeBusy = true;
  if (!hero) { showHero(next); heroFadeBusy = false; return; }
  hero.classList.add('is-out');
  clearTimeout(heroSwapTimer);
  heroSwapTimer = setTimeout(function () {
    /* the fields are fully transparent at this point, so the swap cannot flash */
    showHero(next);
    void hero.offsetWidth;            /* flush the swap before the fade-in starts */
    hero.classList.remove('is-out');
    setTimeout(function () { heroFadeBusy = false; }, HERO_FADE_MS);
  }, HERO_FADE_MS);
}

function startHeroRotation() {
  clearInterval(heroTimer);
  heroTimer = setInterval(advanceHero, HERO_ROT_MS);
}
function updateClock() {
  const d = new Date();
  const p = function (v) { return String(v).padStart(2, '0'); };
  const s = d.getFullYear() + '-' + p(d.getMonth() + 1) + '-' + p(d.getDate()) + ' ' +
    p(d.getHours()) + ':' + p(d.getMinutes()) + ':' + p(d.getSeconds());
  if (s !== lastClock) { lastClock = s; setText('#clock', s); setText('#clockM', s); }
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
  fitText(el, 14);
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
   8 · the globe (webgl texture sphere + canvas-2d overlay)
   ---------------------------------------------------------------------
   The WPF screen paints one equirectangular texture and wraps it round a lit
   orthographic sphere; that is the approach here. The old canvas path
   projected every country outline by hand and clipped it against the horizon,
   which turned anything near the rim into a straight-edged wedge; a texture
   has no clipping to do, the sphere's own geometry decides what is visible.
   Palette and light rig are the constants the WPF build uses (src/WallGlobe.cs):
   ocean 13,27,41 · land 36,58,80 · coast 66,96,124 · heat 255,112,82/255,196,178,
   ambient 92,100,116 + directional 196,202,214 from (-.62,.42,.66).
   The 2d path is kept verbatim as the fallback for a browser without WebGL. */
const GLOBE_WEBGL = true;              /* master switch: false → canvas-2d globe */
const TEX_K = MOBILE_BOOT ? 0.5 : 1;   /* phone build halves the map */
const TEX_W = 4096 * TEX_K, TEX_H = 2048 * TEX_K;  /* 2048x1024 on phones */
const TEX_OCEAN = '#0A0C0D';
const TEX_GRID = '#2B2419', TEX_GRID_PX = 2.6 * TEX_K, TEX_GRID_DEG = 15;
const TEX_LAND = '#1B1F22';
const TEX_COAST = '#4B5257', TEX_COAST_PX = 3.0 * TEX_K;
const TEX_L1_FILL = 'rgba(240,208,112,0.55)', TEX_L1_STROKE = 'rgba(255,232,168,0.95)';
const TEX_L2_FILL = 'rgba(165,32,32,0.85)', TEX_L2_STROKE = 'rgba(224,58,42,0.95)';
const TEX_HOT_FILL = 'rgba(172,26,26,0.95)', TEX_HOT_STROKE = '#FF5A45', TEX_HOT_PX = 4.5 * TEX_K;
/* the wpf light rig, straight from WallGlobe.cs */
const GL_AMBIENT = [0.3608, 0.3922, 0.4549];   /* AmbientLight 92,100,116 */
const GL_DIFFUSE = [0.7686, 0.7922, 0.8392];   /* DirectionalLight 196,202,214 */
const GL_LIGHT = [-0.6211, 0.4208, 0.6612];    /* -(direction .62,-.42,-.66) normalised */
const GL_RIM = [0.045, 0.060, 0.085];          /* very faint fresnel edge */
const GL_ROWS = 48, GL_COLS = 96;              /* lat / lon segments on the sphere */
const GL_VSH = [
  'attribute vec3 aDir;',
  'attribute vec2 aUV;',
  'uniform mat3 uRot;',
  'uniform vec2 uScale;',
  'uniform vec2 uOffset;',
  'varying vec2 vUV;',
  'varying vec3 vDir;',
  'void main(){',
  '  vec3 d = uRot * aDir;',
  '  vDir = d;',
  '  vUV = aUV;',
  '  gl_Position = vec4(uOffset + uScale * d.xy, 0.0, 1.0);',
  '}'
].join('\n');
const GL_FSH = [
  'precision mediump float;',
  'uniform sampler2D uTex;',
  'uniform vec3 uLight;',
  'uniform vec3 uAmbient;',
  'uniform vec3 uDiffuse;',
  'uniform vec3 uRim;',
  'varying vec2 vUV;',
  'varying vec3 vDir;',
  'vec3 lin(vec3 c){ return pow(c, vec3(2.2)); }',
  'vec3 srgb(vec3 c){ return pow(c, vec3(1.0 / 2.2)); }',
  'void main(){',
  '  vec3 n = normalize(vDir);',
  '  vec3 c = lin(texture2D(uTex, vUV).rgb);',
  '  float d = max(dot(n, uLight), 0.0);',
  '  /* wpf lights its 3d materials in linear scRGB; do the same, then encode back */',
  '  vec3 lit = min(lin(uAmbient) + lin(uDiffuse) * d, vec3(1.0));',
  '  vec3 col = srgb(min(c * lit, vec3(1.0)));',
  '  float rim = pow(1.0 - clamp(n.z, 0.0, 1.0), 3.0);',
  '  col += uRim * rim * (0.30 + 0.70 * d);',
  '  gl_FragColor = vec4(col, 1.0);',
  '}'
].join('\n');

function GlobeView(canvas, glCanvas) {
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
  /* eased longitude: fromLon -> toLon over GLOBE_EASE_S, plus a 2 degree drift */
  this.baseLon = 7; this.fromLon = 7; this.toLon = 7; this.moveStart = -100;
  this.markers = [];
  this.links = [];
  this.hero = null;
  this.levels = new Map();
  this.gOcean = null; this.gLight = null;
  this.reticlePhase = 0;
  /* webgl sphere + offscreen texture; everything below is inert in 2d mode */
  this.glCanvas = glCanvas || null;
  this.gl = null; this.glOff = '';
  this.force2D = !GLOBE_WEBGL || window.__globeForce2D === true;
  try { if (/[?&](?:gl|webgl)=(?:0|off|no)\b/i.test(location.search)) this.force2D = true; } catch (e) {}
  this.txPaths = new Map(); this.txBBox = new Map(); this.txLand = null; this.hotIso = null;
  this.baseCanvas = null; this.baseCtx = null; this.scratch = null; this.hotBBox = null;
  this.tex = null;
  this.texW = TEX_W; this.texH = TEX_H;   /* actual equirect texture size */
  this.texStats = { baseMs: 0, upload: 0, hot: 0, rebuilds: 0 };
  if (!this.force2D) this.initGL();
}
GlobeView.prototype.initGL = function () {
  const cv = this.glCanvas;
  if (!cv) { this.glOff = 'no-canvas'; return; }
  let gl = null;
  try {
    const opt = {
      alpha: true, antialias: true, depth: false, stencil: false,
      premultipliedAlpha: true, preserveDrawingBuffer: true
    };
    gl = cv.getContext('webgl', opt) || cv.getContext('experimental-webgl', opt);
  } catch (e) { gl = null; }
  if (!gl) { this.glOff = 'no-webgl'; return; }
  const self = this;
  /* a lost context would otherwise leave an empty layer behind the overlay;
     dropping this.gl sends draw() back down the canvas-2d path */
  cv.addEventListener('webglcontextlost', function (e) {
    e.preventDefault();
    self.glOff = 'context-lost';
    self.gl = null;
    self.tex = null;
  }, false);
  try {
    const vs = this.compileGL(gl, gl.VERTEX_SHADER, GL_VSH);
    const fs = this.compileGL(gl, gl.FRAGMENT_SHADER, GL_FSH);
    const prog = gl.createProgram();
    gl.attachShader(prog, vs); gl.attachShader(prog, fs); gl.linkProgram(prog);
    if (!gl.getProgramParameter(prog, gl.LINK_STATUS)) throw new Error(gl.getProgramInfoLog(prog) || 'link');
    this.prog = prog;
    this.aDir = gl.getAttribLocation(prog, 'aDir');
    this.aUV = gl.getAttribLocation(prog, 'aUV');
    this.uRot = gl.getUniformLocation(prog, 'uRot');
    this.uScale = gl.getUniformLocation(prog, 'uScale');
    this.uOffset = gl.getUniformLocation(prog, 'uOffset');
    this.uTex = gl.getUniformLocation(prog, 'uTex');
    this.uLight = gl.getUniformLocation(prog, 'uLight');
    this.uAmbient = gl.getUniformLocation(prog, 'uAmbient');
    this.uDiffuse = gl.getUniformLocation(prog, 'uDiffuse');
    this.uRim = gl.getUniformLocation(prog, 'uRim');
    this.buildSphere(gl, GL_ROWS, GL_COLS);
    this.gl = gl;
  } catch (e) {
    this.glOff = 'init: ' + (e && e.message ? e.message : e);
    this.gl = null;
  }
};
GlobeView.prototype.compileGL = function (gl, type, src) {
  const sh = gl.createShader(type);
  gl.shaderSource(sh, src);
  gl.compileShader(sh);
  if (!gl.getShaderParameter(sh, gl.COMPILE_STATUS)) {
    throw new Error(gl.getShaderInfoLog(sh) || 'shader compile');
  }
  return sh;
};
/* One unit sphere. Direction (normal) and position are the same vector, so the
   vertex shader only needs the direction; uv runs lon -180..180 / lat 90..-90. */
GlobeView.prototype.buildSphere = function (gl, rows, cols) {
  const nv = (rows + 1) * (cols + 1);
  const dir = new Float32Array(nv * 3), uv = new Float32Array(nv * 2);
  const idx = new Uint16Array(rows * cols * 6);
  let p = 0, u = 0;
  for (let i = 0; i <= rows; i++) {
    const lat = (90 - 180 * i / rows) * Math.PI / 180;
    const cl = Math.cos(lat), sl = Math.sin(lat);
    for (let j = 0; j <= cols; j++) {
      const lon = (-180 + 360 * j / cols) * Math.PI / 180;
      dir[p++] = cl * Math.sin(lon); dir[p++] = sl; dir[p++] = cl * Math.cos(lon);
      uv[u++] = j / cols; uv[u++] = i / rows;
    }
  }
  let k = 0;
  for (let i = 0; i < rows; i++) {
    for (let j = 0; j < cols; j++) {
      const a = i * (cols + 1) + j, b = a + cols + 1;
      idx[k++] = a; idx[k++] = b; idx[k++] = a + 1;
      idx[k++] = a + 1; idx[k++] = b; idx[k++] = b + 1;
    }
  }
  this.bufDir = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, this.bufDir);
  gl.bufferData(gl.ARRAY_BUFFER, dir, gl.STATIC_DRAW);
  this.bufUV = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, this.bufUV);
  gl.bufferData(gl.ARRAY_BUFFER, uv, gl.STATIC_DRAW);
  this.bufIdx = gl.createBuffer();
  gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.bufIdx);
  gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, idx, gl.STATIC_DRAW);
  this.idxCount = idx.length;
  this.rot = new Float32Array(9);
};
GlobeView.prototype.setWorld = function (world) {
  this.countries = [];
  this.maxN = 0;
  this.txPaths = new Map();
  const txAll = new Path2D();
  for (const c of (world && world.countries ? world.countries : [])) {
    const rings = [];
    const txPath = new Path2D();
    let txUsed = false;
    let txMinX = TEX_W, txMinY = TEX_H, txMaxX = 0, txMaxY = 0;
    for (const ring of (c.rings || [])) {
      const n = ring.length;
      if (n < 3) continue;
      /* equirectangular copy of the same ring for the texture: lon→x, lat→y,
         nothing clipped and nothing cut. The world rings already arrive split
         at ±180 (Antarctica's pole ring runs along the bottom edge, which is
         exactly right), so the shape that used to fold into a wedge simply
         paints correctly here. */
      for (let ti = 0; ti < n; ti++) {
        const tx = (ring[ti][0] + 180) / 360 * TEX_W, ty = (90 - ring[ti][1]) / 180 * TEX_H;
        if (ti === 0) txPath.moveTo(tx, ty); else txPath.lineTo(tx, ty);
        if (tx < txMinX) txMinX = tx;
        if (tx > txMaxX) txMaxX = tx;
        if (ty < txMinY) txMinY = ty;
        if (ty > txMaxY) txMaxY = ty;
      }
      txPath.closePath();
      txUsed = true;
      /* Antimeridian: a ring that jumps 180 degrees would otherwise fill the
         sphere as a wedge. Cut it at +-180, inserting the crossing latitude on
         both edges so each half closes along the meridian. */
      const segs = [];
      let cur = [];
      for (let i = 0; i < n; i++) {
        const lon1 = ring[i][0], lat1 = ring[i][1];
        if (cur.length) {
          const lon0 = cur[cur.length - 1][0], lat0 = cur[cur.length - 1][1];
          if (Math.abs(lon1 - lon0) > 180) {
            const edge = lon1 > lon0 ? -180 : 180;
            const t = (edge - lon0) / (lon1 - lon0 + (lon1 > lon0 ? 360 : -360));
            const latC = lat0 + (lat1 - lat0) * t;
            /* overlap the halves by a hair so the +-180 seam does not read as a
               one-pixel dark line while the globe turns */
            const edgeOut = edge > 0 ? 180.05 : -180.05;
            cur.push([edgeOut, latC]);
            segs.push(cur);
            cur = [[-edgeOut, latC]];
          }
        }
        cur.push([lon1, lat1]);
      }
      if (cur.length >= 3) segs.push(cur);
      for (const seg of segs) {
        if (seg.length < 3) continue;
        const m = seg.length;
        const vx = new Float32Array(m), vy = new Float32Array(m), vz = new Float32Array(m), lon = new Float32Array(m);
        for (let k = 0; k < m; k++) {
          const lo = seg[k][0] * Math.PI / 180, la = seg[k][1] * Math.PI / 180;
          const cl = Math.cos(la);
          vx[k] = cl * Math.sin(lo); vy[k] = Math.sin(la); vz[k] = cl * Math.cos(lo); lon[k] = seg[k][0];
        }
        rings.push({ n: m, vx: vx, vy: vy, vz: vz, lon: lon });
        if (m > this.maxN) this.maxN = m;
      }
    }
    if (rings.length) this.countries.push({ iso: c.iso, rings: rings });
    if (txUsed) {
      this.txPaths.set(c.iso, txPath);
      this.txBBox.set(c.iso, [txMinX, txMinY, txMaxX, txMaxY]);
      txAll.addPath(txPath);
    }
  }
  this.txLand = txAll;
  this.X = new Float32Array(this.maxN);
  this.Y = new Float32Array(this.maxN);
  this.Z = new Float32Array(this.maxN);
  /* static half of the texture, painted once; the featured country is patched
     in later by applyHotTexture() */
  if (this.gl) {
    const base = document.createElement('canvas');
    base.width = TEX_W; base.height = TEX_H;
    this.baseCanvas = base;
    this.baseCtx = base.getContext('2d', { alpha: false });
    this.buildBaseTexture();
  }
};
GlobeView.prototype.setData = function (markers, links, levels, hero) {
  this.markers = markers; this.links = links; this.levels = levels; this.hero = hero;
  if (this.gl && !this.levelPainted && levels && levels.size) this.paintLevelTint();
  /* the texture carries one highlighted country; patch it only when the focus
     changes (once per carousel step, ~9 s — not per frame) */
  const iso = (hero && hero.iso) ? hero.iso : null;
  if (this.gl && iso !== this.hotIso) {
    this.hotIso = iso;
    this.applyHotTexture();
  }
};
/* Ease the view towards a longitude (degrees east). Starts where the sphere is
   right now, folds the delta into +/-180 so the turn is always the short way. */
GlobeView.prototype.focusTo = function (lonDeg, time) {
  const t = (time == null) ? (performance.now() / 1000) : time;
  const target = lonDeg == null || !isFinite(lonDeg) ? this.toLon : lonDeg;
  if (this.moveStart < 0 && Math.abs(wrapLon(target - this.baseLon)) < 0.01) return;
  this.fromLon = this.baseLon;
  this.toLon = target;
  this.moveStart = t;
};
/* First paint: land on the first report instead of flying in from longitude 0. */
GlobeView.prototype.snapTo = function (lonDeg) {
  const v = (lonDeg == null || !isFinite(lonDeg)) ? 7 : lonDeg;
  this.baseLon = v; this.lon0 = v; this.fromLon = v; this.toLon = v; this.moveStart = -1;
};
/* Portrait layout draws into its own box — a 1032-px-wide strip whose sphere
   is sized to the band between the bars — so the drawing coordinate system has
   to follow the layout. Landscape keeps the stock 1000x1080 / (478,540) / 430. */
GlobeView.prototype.setViewBox = function (W, H, cx, cy, R) {
  if (!(W > 0 && H > 0 && R > 0)) return;
  if (this.W === W && this.H === H && this.cx === cx && this.cy === cy && this.R === R) return;
  this.W = W; this.H = H; this.cx = cx; this.cy = cy; this.R = R;
  this.q = -1;                 /* force resize() to rebuild the backing store */
  this.buildGradients();
};
GlobeView.prototype.resize = function (q) {
  if (q === this.q && this.cv.width > 0 && (this.gl ? !!this.tex : !!this.gOcean)) return;
  this.q = q;
  this.cv.width = Math.round(this.W * q);
  this.cv.height = Math.round(this.H * q);
  if (this.glCanvas) {
    this.glCanvas.width = this.cv.width;
    this.glCanvas.height = this.cv.height;
  }
  if (this.gl) this.gl.viewport(0, 0, this.cv.width, this.cv.height);
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
/* View angle for this frame: eased focus plus a small residual drift so the
   sphere never freezes. Shared by both renderers so the 2d overlay always
   matches the sphere underneath. */
GlobeView.prototype.tickView = function (time) {
  /* longitude = eased focus + a small residual drift so the sphere never freezes */
  if (this.moveStart >= 0) {
    const u = (time - this.moveStart) / GLOBE_EASE_S;
    if (u >= 1) {
      this.baseLon = this.toLon;
      this.fromLon = this.toLon;
      this.moveStart = -1;
    } else {
      this.baseLon = this.fromLon + wrapLon(this.toLon - this.fromLon) * smoothstep(u);
    }
  }
  this.lon0 = this.baseLon + 2 * Math.sin(time * Math.PI * 2 / 210);
  this.tilt = 20 + 1.0 * Math.sin(time * Math.PI * 2 / 210);
  this.reticlePhase = (Math.sin(time * 2.1) + 1) / 2;
  /* test hook (only reachable through window.__globe with ?debug): freeze the
     view so the acceptance shots can be taken at exact yaw angles. */
  if (this.frozen) { this.lon0 = this.frozenLon; this.tilt = this.frozenTilt; }
};
/* Freeze the view at a yaw/pitch (degrees). Passing null resumes the drift. */
GlobeView.prototype.freezeAt = function (lonDeg, tiltDeg) {
  if (lonDeg == null || !isFinite(lonDeg)) { this.frozen = false; return; }
  this.frozen = true;
  this.frozenLon = lonDeg;
  this.frozenTilt = (tiltDeg == null || !isFinite(tiltDeg)) ? this.tilt : tiltDeg;
};
GlobeView.prototype.draw = function (time) {
  this.tickView(time);
  const ctx = this.ctx;
  ctx.setTransform(this.q, 0, 0, this.q, 0, 0);
  ctx.clearRect(0, 0, this.W, this.H);
  if (this.gl && this.tex) {
    this.drawSphereGL();
    this.drawOverlay(time);
  } else {
    this.draw2D(time);
  }
};
/* ---- canvas-2d fallback: the original per-country orthographic projection ---- */
GlobeView.prototype.draw2D = function (time) {
  const ctx = this.ctx;
  const q = this.q;
  ctx.setTransform(q, 0, 0, q, 0, 0);
  ctx.clearRect(0, 0, this.W, this.H);

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
  ctx.fillStyle = '#243A50';
  ctx.fill(pBase);
  ctx.fillStyle = '#2E4A63';
  ctx.fill(pMid);
  ctx.globalCompositeOperation = 'multiply';
  ctx.fillStyle = this.gLight;
  ctx.fillRect(cx - R, cy - R, R * 2, R * 2);
  ctx.globalCompositeOperation = 'source-over';
  ctx.fillStyle = '#4E1512';
  ctx.fill(pHigh);

  /* country borders: the wider low-alpha pass is the +0.5px compensation;
     the 0.65px pass keeps the hairline from turning into a smear. */
  ctx.strokeStyle = 'rgba(66,96,124,0.55)';
  ctx.lineWidth = 1.15;
  ctx.stroke(pStroke);
  ctx.strokeStyle = 'rgba(120,150,180,0.18)';
  ctx.lineWidth = 0.65;
  ctx.stroke(pStroke);

  /* featured countries: outer edge + an inner rim clipped to the fill, so the
     highlight keeps a sharp inner contour without any blur filter */
  ctx.strokeStyle = 'rgba(255,112,82,0.55)';
  ctx.lineWidth = 1;
  ctx.stroke(pHighStroke);
  ctx.save();
  ctx.clip(pHigh);
  ctx.strokeStyle = 'rgba(255,196,178,0.45)';
  ctx.lineWidth = 2.4;
  ctx.stroke(pHighStroke);
  ctx.restore();

  // graticule
  ctx.strokeStyle = 'rgba(24,44,64,0.85)';
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

/* ---- webgl texture sphere ---- */
/* The static half of the map (ocean, graticule, land, coastlines) is painted
   once into an offscreen canvas; when the carousel moves the focus, only the
   union of the old and the new country's texture box is repainted and pushed
   with texSubImage2D. A full 4096x2048 repaint every nine seconds would be a
   visible hitch — one rectangle is a couple of milliseconds. */
GlobeView.prototype.buildBaseTexture = function () {
  if (!this.baseCtx || !this.txLand) return;
  const t0 = performance.now();
  const ctx = this.baseCtx;
  ctx.setTransform(1, 0, 0, 1, 0, 0);
  ctx.globalAlpha = 1;
  ctx.globalCompositeOperation = 'source-over';
  ctx.fillStyle = TEX_OCEAN;
  ctx.fillRect(0, 0, TEX_W, TEX_H);
  /* graticule every 15°; the ±180 meridian lands on both texture edges so the
     wrap reads as one continuous line */
  ctx.strokeStyle = TEX_GRID;
  ctx.lineWidth = TEX_GRID_PX;
  ctx.beginPath();
  for (let lon = -180; lon <= 180; lon += TEX_GRID_DEG) {
    const x = (lon + 180) / 360 * TEX_W;
    ctx.moveTo(x, 0); ctx.lineTo(x, TEX_H);
  }
  for (let lat = -75; lat <= 75; lat += TEX_GRID_DEG) {
    const y = (90 - lat) / 180 * TEX_H;
    ctx.moveTo(0, y); ctx.lineTo(TEX_W, y);
  }
  ctx.stroke();
  /* land + coastline */
  ctx.fillStyle = TEX_LAND;
  ctx.fill(this.txLand);
  ctx.strokeStyle = TEX_COAST;
  ctx.lineWidth = TEX_COAST_PX;
  ctx.stroke(this.txLand);
  this.texStats.baseMs = +(performance.now() - t0).toFixed(1);
};
/* Alert tinting: every country that reports gets an amber (level 1) or red
   (level 2) wash baked into the base texture, the way the wall colours all
   reporting countries rather than only the featured one. Runs once, when the
   level map first arrives from the snapshot. */
GlobeView.prototype.paintLevelTint = function () {
  if (!this.gl || !this.baseCtx || !this.levels) return;
  const ctx = this.baseCtx;
  let painted = 0;
  for (const entry of this.levels) {
    const iso = entry[0], lv = entry[1];
    if (!lv) continue;
    const p = this.txPaths.get(iso);
    if (!p) continue;
    ctx.fillStyle = lv >= 2 ? TEX_L2_FILL : TEX_L1_FILL;
    ctx.fill(p);
    ctx.strokeStyle = lv >= 2 ? TEX_L2_STROKE : TEX_L1_STROKE;
    ctx.lineWidth = 2.2;
    ctx.stroke(p);
    painted++;
  }
  if (!painted) return;
  const gl = this.gl;
  gl.activeTexture(gl.TEXTURE0);
  gl.bindTexture(gl.TEXTURE_2D, this.tex);
  gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
  gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, this.baseCanvas);
  gl.generateMipmap(gl.TEXTURE_2D);
  this.levelPainted = painted;
};
/* Bake the featured country into the texture: fill 255,112,82 at 0.75 over the
   land with a 255,196,178 edge, the wall's heat colours. The light multiplies
   it down to the maroon-ish tone the WPF screen shows. */
GlobeView.prototype.applyHotTexture = function () {
  if (this.glOff) this.glOff = '';
  if (!this.gl || !this.baseCanvas) return;
  const t0 = performance.now();
  try {
    const gl = this.gl;
    if (!this.tex) {
      this.tex = gl.createTexture();
      gl.activeTexture(gl.TEXTURE0);
      gl.bindTexture(gl.TEXTURE_2D, this.tex);
      gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
      gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, this.baseCanvas);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR_MIPMAP_LINEAR);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
      this.texStats.upload = +(performance.now() - t0).toFixed(1);
    }
    const newBox = this.hotIso ? this.txBBox.get(this.hotIso) : null;
    const oldBox = this.hotBBox;
    if (newBox || oldBox) {
      const pad = 8;
      let x0 = TEX_W, y0 = TEX_H, x1 = 0, y1 = 0;
      for (const b of [oldBox, newBox]) {
        if (!b) continue;
        if (b[0] < x0) x0 = b[0];
        if (b[1] < y0) y0 = b[1];
        if (b[2] > x1) x1 = b[2];
        if (b[3] > y1) y1 = b[3];
      }
      x0 = Math.max(0, Math.floor(x0 - pad)); y0 = Math.max(0, Math.floor(y0 - pad));
      x1 = Math.min(TEX_W, Math.ceil(x1 + pad)); y1 = Math.min(TEX_H, Math.ceil(y1 + pad));
      const w = x1 - x0, h = y1 - y0;
      if (w > 0 && h > 0) {
        const sc = this.scratch || (this.scratch = document.createElement('canvas'));
        sc.width = w; sc.height = h;
        const sctx = sc.getContext('2d', { alpha: false });
        sctx.setTransform(1, 0, 0, 1, 0, 0);
        sctx.drawImage(this.baseCanvas, x0, y0, w, h, 0, 0, w, h);
        const hot = this.hotIso ? this.txPaths.get(this.hotIso) : null;
        if (hot) {
          sctx.save();
          sctx.translate(-x0, -y0);
          sctx.fillStyle = TEX_HOT_FILL;
          sctx.fill(hot);
          sctx.strokeStyle = TEX_HOT_STROKE;
          sctx.lineWidth = TEX_HOT_PX;
          sctx.stroke(hot);
          sctx.restore();
        }
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, this.tex);
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
        gl.texSubImage2D(gl.TEXTURE_2D, 0, x0, y0, gl.RGBA, gl.UNSIGNED_BYTE, sc);
        this.hotBBox = newBox;
      }
    }
    /* the texture is ~1.9x wider than its on-screen equator; without mips the
       coastlines would crawl and alias while the globe turns */
    gl.generateMipmap(gl.TEXTURE_2D);
    this.texStats.hot = +(performance.now() - t0).toFixed(1);
    this.texStats.rebuilds++;
  } catch (e) {
    /* out of memory / driver refusal: drop back to the 2d projection */
    this.glOff = 'texture: ' + (e && e.message ? e.message : e);
    try { this.gl.clearColor(0, 0, 0, 0); this.gl.clear(this.gl.COLOR_BUFFER_BIT); } catch (e2) {}
    this.gl = null;
  }
};
GlobeView.prototype.drawSphereGL = function () {
  const gl = this.gl, cv = this.glCanvas;
  if (!gl || !this.tex || !cv) return;
  if (cv.width !== this.cv.width || cv.height !== this.cv.height) {
    cv.width = this.cv.width; cv.height = this.cv.height;
  }
  gl.viewport(0, 0, cv.width, cv.height);
  gl.clearColor(0, 0, 0, 0);
  gl.clear(gl.COLOR_BUFFER_BIT);
  gl.disable(gl.DEPTH_TEST);
  gl.disable(gl.BLEND);
  gl.enable(gl.CULL_FACE);
  gl.cullFace(gl.BACK);
  gl.frontFace(gl.CCW);
  gl.useProgram(this.prog);
  const lon = this.lon0 * Math.PI / 180, tilt = this.tilt * Math.PI / 180;
  const c0 = Math.cos(lon), s0 = Math.sin(lon), ct = Math.cos(tilt), st = Math.sin(tilt);
  /* the same yaw-then-pitch rotation the 2d projection uses, as a column-major
     mat3: rot * v gives camera space (x right, y up, z towards the viewer) */
  const rot = this.rot;
  rot[0] = c0; rot[1] = -st * s0; rot[2] = ct * s0;
  rot[3] = 0; rot[4] = ct; rot[5] = st;
  rot[6] = -s0; rot[7] = -st * c0; rot[8] = ct * c0;
  gl.uniformMatrix3fv(this.uRot, false, rot);
  gl.uniform2f(this.uScale, 2 * this.R / this.W, 2 * this.R / this.H);
  gl.uniform2f(this.uOffset, 2 * this.cx / this.W - 1, 1 - 2 * this.cy / this.H);
  gl.uniform3f(this.uAmbient, GL_AMBIENT[0], GL_AMBIENT[1], GL_AMBIENT[2]);
  gl.uniform3f(this.uDiffuse, GL_DIFFUSE[0], GL_DIFFUSE[1], GL_DIFFUSE[2]);
  gl.uniform3f(this.uLight, GL_LIGHT[0], GL_LIGHT[1], GL_LIGHT[2]);
  gl.uniform3f(this.uRim, GL_RIM[0], GL_RIM[1], GL_RIM[2]);
  gl.activeTexture(gl.TEXTURE0);
  gl.bindTexture(gl.TEXTURE_2D, this.tex);
  gl.uniform1i(this.uTex, 0);
  gl.bindBuffer(gl.ARRAY_BUFFER, this.bufDir);
  gl.enableVertexAttribArray(this.aDir);
  gl.vertexAttribPointer(this.aDir, 3, gl.FLOAT, false, 0, 0);
  gl.bindBuffer(gl.ARRAY_BUFFER, this.bufUV);
  gl.enableVertexAttribArray(this.aUV);
  gl.vertexAttribPointer(this.aUV, 2, gl.FLOAT, false, 0, 0);
  gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.bufIdx);
  gl.drawElements(gl.TRIANGLES, this.idxCount, gl.UNSIGNED_SHORT, 0);
};
/* Everything that is not part of the sphere texture: rim + ticks, great-circle
   arcs, markers, the hero halo/reticle and the travelling dots. Projected with
   the same yaw/pitch as the webgl sphere, so it registers to the pixel. */
GlobeView.prototype.drawOverlay = function (time) {
  const ctx = this.ctx;
  const lon0 = this.lon0 * Math.PI / 180, tilt = this.tilt * Math.PI / 180;
  const c0 = Math.cos(lon0), s0 = Math.sin(lon0), ct = Math.cos(tilt), st = Math.sin(tilt);
  const cx = this.cx, cy = this.cy, R = this.R;

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

  // ---- hero halo and reticle ----
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
/* --sat/--sab are env(safe-area-inset-*) in *CSS* px (style.css :root); the
   stage is scaled, so the notch/home-indicator insets are converted to design
   px here. https://webkit.org/blog/7929/designing-websites-for-iphone-x/ */
function readSafeInsets(scale) {
  const out = { top: 0, bottom: 0 };
  try {
    const cs = getComputedStyle(document.documentElement);
    const t = parseFloat(cs.getPropertyValue('--sat')) || 0;
    const b = parseFloat(cs.getPropertyValue('--sab')) || 0;
    out.top = Math.round(t / (scale || 1));
    out.bottom = Math.round(b / (scale || 1));
  } catch (e) {}
  return out;
}
/* Keep the watchlist page size in sync with the live orientation (the boot
   texture size cannot change, but the DOM layout can). */
function ensureMode() {
  const p = isPortrait();
  document.documentElement.classList.toggle('portrait', p);
  document.body.classList.toggle('portrait', p);
  const next = p ? 3 : 6;
  if (next !== pageRows) {
    pageRows = next;
    rowTop = p ? 112 : 82;
    rowStep = p ? 60 : 46;
    buildWatchRows();
    /* the old page number was counted in rows of the other size — re-derive it
       from the focus so the table never points past the end of the list */
    const hi = HERO ? RANKED.indexOf(HERO) : -1;
    rankPage = hi >= 0 ? Math.floor(hi / pageRows) : 0;
    updateWatch();
    /* drop any portrait-only inline type sizes the fitter left behind */
    updateHero();
    updateSyncLine();
  }
  if (window.__app) { window.__app.portrait = p; window.__app.pageRows = pageRows; }
}
/* landscape: the stock 1920x1080 design, contain-scaled with black matte. */
function layoutLandscape(vw, vh) {
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
    stage.style.width = '';
    stage.style.height = '';
    stage.style.transform = 'translate(-50%,-50%) scale(' + s + ')';
    stage.style.setProperty('--padx', padXR + 'px');
    stage.style.setProperty('--pady', padYR + 'px');
  }
  if (globe) globe.setViewBox(1000, 1080, 478, 540, 430);
  return s;
}
/* portrait: a 1080-wide design whose height follows the device (1920 at 9:16).
   The stage is scaled to the viewport width, so the design height is whatever
   the device gives us; the extra space on tall phones is shared out as gaps
   between the four bands. CSS places every band from the variables set here. */
function layoutPortrait(vw, vh) {
  const stage = $('#stage');
  const s = vw / M_W;
  const stageH = Math.round(vh / s);
  if (!stage) return s;
  stage.style.width = M_W + 'px';
  stage.style.height = stageH + 'px';
  stage.style.transform = 'translate(-50%,-50%) scale(' + s + ')';
  stage.style.setProperty('--padx', '0px');
  stage.style.setProperty('--pady', '0px');

  const safe = readSafeInsets(s);
  const barH = Math.max(72, Math.min(140, Math.round(stageH * 0.052)));
  const gapMin = Math.max(14, Math.min(40, Math.round(stageH * 0.0115)));
  const usable = stageH - barH * 2 - safe.top - safe.bottom - M_HERO_H - M_WATCH_H - gapMin * 4;

  /* 24 px of air on both sides of the globe band. The sphere is as large as the
     width allows (the arcs bow out to 1.2R, so the drawing box needs that
     margin); the band itself stays close around the sphere so any spare height
     on tall phones lands in the gaps between bands, not around the globe. */
  const gW = M_W - 48;
  const maxR = Math.floor(gW / 2 / 1.2);
  const bandMargin = Math.max(10, Math.min(26, Math.round(stageH * 0.006)));
  let globeH = Math.min(Math.round(stageH * 0.466), usable);
  let R = maxR;
  const bandNeed = 2 * R + 2 * bandMargin;
  if (globeH >= bandNeed) {
    globeH = bandNeed;
  } else {
    R = Math.max(120, Math.floor((globeH - 2 * bandMargin) / 2));
    if (R > maxR) R = maxR;
    globeH = Math.min(usable, 2 * R + 2 * bandMargin);
  }
  const canvasH = Math.ceil(R * 2.4);

  const extra = usable - globeH;
  const gap = gapMin + (extra > 0 ? extra / 4 : 0);
  const globeTop = barH + safe.top + gap;
  const heroTop = globeTop + globeH + gap;
  const watchTop = heroTop + M_HERO_H + gap;
  const canvasTop = globeTop + Math.round((globeH - canvasH) / 2);

  stage.style.setProperty('--stage-h', stageH + 'px');
  stage.style.setProperty('--bar-h', barH + 'px');
  stage.style.setProperty('--safe-top', safe.top + 'px');
  stage.style.setProperty('--safe-bottom', safe.bottom + 'px');
  stage.style.setProperty('--globe-left', '24px');
  stage.style.setProperty('--globe-w', gW + 'px');
  stage.style.setProperty('--globe-canvas-top', canvasTop + 'px');
  stage.style.setProperty('--globe-canvas-h', canvasH + 'px');
  stage.style.setProperty('--hero-top', Math.round(heroTop) + 'px');
  stage.style.setProperty('--hero-h', M_HERO_H + 'px');
  stage.style.setProperty('--watch-top', Math.round(watchTop) + 'px');
  stage.style.setProperty('--watch-h', M_WATCH_H + 'px');
  if (globe) globe.setViewBox(gW, canvasH, gW / 2, canvasH / 2, R);
  return s;
}
function fitStage() {
  const vw = window.innerWidth, vh = window.innerHeight;
  ensureMode();
  const portrait = isPortrait();
  const s = portrait ? layoutPortrait(vw, vh) : layoutLandscape(vw, vh);
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
  /* the transmission chain is hidden in portrait — no point animating it */
  if (chainView && !isPortrait()) chainView.tick(time);
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

  globe = new GlobeView($('#globe'), $('#globe-gl'));
  globe.setWorld(WORLD);
  globe.resize(1);
  globe.setData(MARKERS, LINKS, LEVEL_BY_ISO, HERO);
  globe.snapTo(heroLonOf(HERO));   /* no fly-in on the first frame */

  chainView = new ChainView();
  chainView.build();
  chainView.apply();

  updateSyncLine();
  startClock();
  startHeroRotation();

  window.__perf = { frames: 0, total: 0, last: 0, max: 0, t: 0, start: performance.now() };
  window.__app = {
    lang: lang, schema: SNAP ? SNAP.schema : null, events: EVENTS.length,
    countries: new Set(EVENTS.map(function (e) { return e.iso; })).size,
    hero: HERO, ranked: RANKED.length,
    worldCountries: WORLD.countries.length, lag: LAG, latest: LATEST,
    portrait: isPortrait(), pageRows: pageRows,
    tex: { w: TEX_W, h: TEX_H, mobile: MOBILE_BOOT, k: TEX_K }
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
