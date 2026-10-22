# 锦檐猫影：筑梦人

Windows 64 位单机水墨风游戏，使用团结编辑器 1.8.4 开发。玩家操控黑猫墨宝，沿故宫画卷依次完成午门、乾清宫、保和殿和太和殿的任务，让失色的宫殿逐处复色。当前开发分支为 `remake-2026-10-12`。

## 试玩与构建

- **试玩发行版**：获取完整的 `WindowsFirstPlayable` 文件夹，运行其中的 `JinYanMaoYing.exe`。保留同级的 `JinYanMaoYing_Data`、`MonoBleedingEdge` 和 DLL 文件。`Builds/` 未纳入 Git，仓库本身不包含可执行发行版。
- **编辑器试玩**：用团结编辑器 1.8.4 打开本目录，从 `Assets/Scenes/封面.unity` 开始 Play。封面上的“从头开始”会重置本机通关进度；“开始游戏”会保留现有进度。
- **Windows 构建**：入口为 `Assets/Editor/WindowsBuild.cs` 的 `WindowsBuild.Build`，目标为 Windows 64 位，输出到 `Builds/WindowsFirstPlayable/`。该脚本会检查关键场景和资源，并将 `CREDITS.md` 复制到发行版目录。构建前应确认视频和音频素材已完整导入。
- `Assets/Scenes/保和殿水管.unity` 沿用旧文件名，现为保和殿营造图旋转拼合；进入它之前先经过 `保和殿前庭.unity` 的匠人对话。

当前工程包含封面开场 CG、四关入场 CG、结局 ED，共六段视频；另有独立背景音乐和操作音效。文件存在及代码引用不等于已完成全流程实机验收。提交测试反馈时请记录构建日期或 Git 提交号。

## 文档索引

| 文档 | 内容 |
| --- | --- |
| [TESTER_GUIDE.md](TESTER_GUIDE.md) | 试玩入口、各关操作、预期流程及问题反馈格式 |
| [STORY_DESIGN.md](STORY_DESIGN.md) | 复色主线、玩法与故事的关系、史实和虚构边界 |
| [CONTEST_PRESENTATION.md](CONTEST_PRESENTATION.md) | 参赛说明和实机录制建议 |
| [AUDIO_PLAN.md](AUDIO_PLAN.md) | 当前音频接入情况与后续听感检查 |
| [CREDITS.md](CREDITS.md) | 已核对的建筑资料和照片署名 |
| [ASSET_PROVENANCE.md](ASSET_PROVENANCE.md) | 美术、视频、音频来源盘点与待核验授权 |
| [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md) | 分层目标、历史记录与当前待验事项 |

文档依据截至 2026-10-22 的工程源码与资产更新；实际操作手感、视频内容和发行版稳定性仍以同版本实机测试为准。
