# 已知限制 · Known Limitations

## 中文

1. **关注度是本屏估算，不是官方风险评级。** 它由病种类别、病例数、死亡数和通报时间加权估算，仅用于本屏排序与着色，不构成任何风险评估或预警结论。
2. **数据取自公开通报**（WHO DON / ECDC / CDC），存在抓取延迟、源站改版导致字段缺失、以及缓存回灌。界面上 `STALE` 角标表示该条来自缓存。
3. **4096 地球贴图在 WPF 软件渲染路径下没有增益。** 实测 1080p 与 1440p 产物与 2048 贴图逐像素差约 0.27/255（4K 屏才有意义）。1440p 及以下建议改回 2048，省 32 MB 内存。
4. **macOS / Linux 产物需在对应平台打包。** 当前只有 Windows 产物经过验证，Electron 的 dmg / AppImage 未在 CI 上跑过。
5. **无代码签名。** 首次运行会有 SmartScreen（Windows）或 Gatekeeper（macOS）警告。
6. **国家与病种名只做了中英两种语言。** 其他语言回落到英文。
7. **排行末页不足 6 行时，格线按实际行数绘制**，这是有意设计，不是渲染缺陷。

## English

1. **Attention is an on-screen estimate, not an official risk rating.** It is a weighted blend of disease class, case counts, deaths and report recency, used only for ordering and colouring on this screen. It is not a risk assessment or an early-warning conclusion.
2. **Data comes from public notifications** (WHO DON / ECDC / CDC) and is subject to fetch delays, source-site changes that drop fields, and cache replay. The `STALE` badge marks entries served from cache.
3. **The 4096 globe texture gains nothing under WPF software rendering.** At 1080p and 1440p it differs from the 2048 texture by roughly 0.27/255 per pixel (only 4K displays benefit). On 1440p and below, 2048 saves 32 MB.
4. **macOS / Linux builds must be produced on those platforms.** Only the Windows build has been verified; the Electron dmg / AppImage targets have not run in CI.
5. **No code signing.** The first run triggers SmartScreen (Windows) or Gatekeeper (macOS).
6. **Country and disease names exist in Chinese and English only.** Other locales fall back to English.
7. **On the last ranking page, gridlines follow the actual row count** — intentional, not a rendering defect.
