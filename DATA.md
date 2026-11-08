# 接入更多数据源 · Adding data sources

屏保不绑定任何一家数据源。它只认一种东西：**符合下面 schema 的 JSON**。谁产出的都行——WHO、ECDC、各国疾控、你自己写的爬虫、手工整理的表格。

The screensaver is not tied to any provider. It only understands one thing: **JSON in the schema below**. Anything that produces it works — WHO, ECDC, a national agency, your own scraper, a hand-maintained spreadsheet.

---

## 快照格式 · Snapshot schema

`schema: 2`，单个对象：

```json
{
  "schema": 2,
  "generatedAt": "2026-11-08T12:00:00Z",
  "source": "WHO DON / ECDC / CDC",
  "events": [
    {
      "country": "刚果（金）",
      "country_en": "DR Congo",
      "iso": "COD",
      "disease": "埃博拉",
      "disease_en": "Ebola",
      "lat": -2.5,
      "lon": 23.0,
      "date": "2026-11-04",
      "cases": 8728,
      "deaths": 4205,
      "level": "高",
      "active": "True",
      "source": "WHO",
      "evidence": "https://...",
      "metrics": {
        "period": "本轮疫情累计",
        "period_en": "This outbreak",
        "asOf": "2026-11-04",
        "caseLabel": "报告病例"
      }
    }
  ]
}
```

必填：`country`、`disease`、`date`。其余缺了界面会留空，不会崩。

Required: `country`, `disease`, `date`. Everything else degrades gracefully.

| 字段 | 作用 |
|---|---|
| `iso` | 三位国家码，用来在地球上定位和着色。没有就按 `lat`/`lon` 打点 |
| `country_en` / `disease_en` / `metrics.period_en` | 英文界面用；缺了就显示中文原值 |
| `cases` / `deaths` | 主数字、病例数、病死率；二者都缺时该条不参与关注度排序 |
| `level` | 关注度等级（高/中/低），缺了按病例数和时间自动估算 |
| `active` | `True` 表示进行中；`False` 表示已结束 |
| `date` | 通报日期 `YYYY-MM-DD`，参与排序和 `LAG` 计算 |
| `evidence` | 原文链接，界面不显示，供核对 |

---

## 三种接入方式 · Three ways in

### 1. 直接放快照文件（最简单）

把符合 schema 的 JSON 放到：

- 网页版：`web/snapshot.json`
- Electron：同上（读的是同一份）
- WPF 屏保：`science/outbreaks.json`（同一 schema；WPF 还读 `science/transmission.json` 做传播链）

刷新频率见 README。文件变了，界面下一次重读就生效。

### 2. 自己写采集脚本，定时覆盖

任何语言都行，只要能产出上表 JSON。典型流程：

1. 抓源站页面或 API
2. 解析成 events 数组
3. 补 `iso`（用 `science/world.json` 里 177 国的 `iso` 做匹配）
4. 写文件（UTF-8 无 BOM）

仓库里的 `feeds.json` + `SourceParsing.ps1` 就是这种脚本的一个实例（未随仓库发布，因为是内部采集链路）。

### 3. 挂在已有采集器后面

已经有自己的数据库或看板，加一层导出即可——按 schema 生成一份 JSON，放到上面的路径。

---

## 加一个源要改哪里 · What to touch

| 想做的事 | 改哪里 |
|---|---|
| 加一个新源 | 写一个抓取脚本，产出快照 JSON，覆盖到上面的路径 |
| 只加一个国家/地区 | 不用改代码，事件里写对 `iso` 即可；`science/world.json` 没有的属地（如 MYT / GUF）用手工经纬度 |
| 加一种新病种 | 事件里写 `disease` 原文；传播链若要有它，在 `science/transmission.json` 加一条对应关系，否则该病种显示但不画链路 |
| 改排序权重 | `WallCards.cs` 里关注度计算（病种权重、病例数、死亡数、通报时间衰减） |
| 改刷新节奏 | `feeds.json` 的 `refreshSeconds`；屏内重读间隔是 `WallView.cs` 的 `RefreshSecs` |

---

## 注意 · Caveats

- `lat`/`lon` 是世界坐标（经度 -180~180，纬度 -90~90），不是屏幕坐标
- 同一个国家同一种病的多条通报会合并显示为一条，`date` 取最新
- 没有 `iso` 且没有 `lat`/`lon` 的事件不会出现在地球上，但仍会出现在排行里
- 快照体积建议控制在 200 KB 以内（当前 40 条事件约 16 KB）
