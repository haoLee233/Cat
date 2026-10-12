# 锦檐猫影：筑梦人（重制开发）

团结编辑器 1.8.4 工程。目标平台为 Windows，免费单机。当前已接通封面、故宫地图、午门、乾清宫、保和殿、太和殿和终章；改进次序见 `DEVELOPMENT_PLAN.md`，复色主线见 `STORY_DESIGN.md`。

## 工程与试玩

- 本地仓库：`E:\GitHubDeskTop\锦檐猫影续作`
- 在团结编辑器中打开 `Assets/Scenes/封面.unity`，按 Play。
- 若需要重新生成所有场景，使用菜单“锦檐猫影/生成首批场景”。这会刷新 `Assets/Scenes`，先保存正在编辑的场景。
- Windows 构建入口：`Assets/Editor/WindowsBuild.cs` 的 `WindowsBuild.Build`，输出到 `Builds/WindowsFirstPlayable`。构建目录不纳入 Git。

## 当前通关顺序

封面 → 地图 → 午门追逐 → 午门营造录 → 地图 → 乾清宫探索 → 乾清宫营造录 → 地图 → 保和殿营造图复原 → 地图 → 太和殿互动终章 → 结局。后续关卡需要完成前置关卡才能解锁；进度保存在本机 PlayerPrefs。

- 午门：二段跳、下蹲避障、收集三只脊兽。片头可以跳过。
- 乾清宫：穿过四个场景，先观察铜鹤、嘉量、日晷，再回答器物问题。连按同一方向可加速，片头可以跳过。
- 保和殿：对照原图，在七分钟内旋转 25 块殿宇画面拼块，找回屋檐与阶陛线索。场景文件仍沿用旧名 `保和殿水管.unity`。
- 太和殿：根据线索，从下到上选出建筑部位，完成终章；之后可以观看结局或返回已复色的地图。

## 素材与版本

前三关沿用并整理了已有项目素材。太和殿暂用旧工程 `TaiHeDian/placeR.jpg` 的水墨屋脊图。外部照片及资料授权请看 `CREDITS.md`，素材来源盘点与待核验项见 `ASSET_PROVENANCE.md`。参赛说明与录屏顺序见 `CONTEST_PRESENTATION.md`。

请通过 GitHub Desktop 检查并提交工程源码和素材，由项目负责人手动推送。不要将 `Library`、`Temp` 或 `Builds` 加入版本控制。
