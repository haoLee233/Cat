# 锦檐猫影：筑梦人

使用团结编辑器 1.8.4 开发的 Windows 单机游戏。玩家操控黑猫墨宝，沿故宫地图依次完成午门、乾清宫、保和殿和太和殿的任务，让失色的宫殿逐处复色。

## 给测试人员

请先阅读 [测试与试玩指南](TESTER_GUIDE.md)。其中列有运行方式、封面按钮的区别、每关操作、预期流程、存档规则和问题反馈格式。**只下载 Git 仓库并不会得到可执行版**：`Builds/` 不纳入版本控制，发行版需由项目负责人另行提供。

## 工程入口

- 工程根目录是本 README 所在文件夹；用团结编辑器 1.8.4 打开此文件夹。
- 编辑器内从 `Assets/Scenes/封面.unity` 开始 Play，按游戏完整流程进入其他场景。
- Windows 构建入口为 `Assets/Editor/WindowsBuild.cs` 的 `WindowsBuild.Build`，本地输出到 `Builds/WindowsFirstPlayable/`。构建目录及其日志不提交 Git。
- 场景文件 `保和殿水管.unity` 沿用旧文件名；游戏中呈现的是保和殿营造图旋转拼合，不是水管题。

## 文档索引

| 文档 | 用途 |
| --- | --- |
| [TESTER_GUIDE.md](TESTER_GUIDE.md) | 测试人员的运行、操作、流程和问题记录指南 |
| [STORY_DESIGN.md](STORY_DESIGN.md) | 玩家主线、各关行动与史实/创作边界 |
| [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md) | 分层改进计划与历史进度记录，非最新测试结论 |
| [CONTEST_PRESENTATION.md](CONTEST_PRESENTATION.md) | 参赛演示与说明稿 |
| [CREDITS.md](CREDITS.md) | 外部资料和照片的来源、署名与许可 |
| [ASSET_PROVENANCE.md](ASSET_PROVENANCE.md) | 素材来源盘点及团队尚待核验的授权事项 |

本仓库的开发分支名为 `remake-2026-10-12`。测试时请在反馈中附上实际构建日期或提交号，避免把不同版本的现象混在一起。
