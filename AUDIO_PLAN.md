# 音效与配乐现状（2026-10-22）

旧版“尚无独立音频”的记录已过时。当前 `Assets/Resources/Audio/` 有 `.wav` 和 `.ogg` 文件，相关脚本已引用其中多项；`Assets/Videos/` 有封面开场、四关入场、结局 ED 共六段 `.m4v`。以下为**资源和代码接入情况**，不等同于已完成逐设备听感验收。

| 用途 | 当前接入或存在的资源 | 代码入口 |
| --- | --- | --- |
| 全局背景音乐 | `游戏通关前的背景音乐.ogg`、`游戏完成后的背景音乐.ogg` | `GameMusicManager` 按太和殿通关状态切换；有视频播放时暂停全局音乐。 |
| 午门跑酷 | `第一关跑酷.ogg`、`Cat_Run.wav`、`Jump_Takeoff.wav`、`Jump_Land.wav`、`Coin_Pickup.wav`、`Stone_Pickup.wav`、`Damage_Hit.wav` | `WuMenRunner`。 |
| 乾清宫与太和殿移动 | `Cat_Run.wav`、起跳与落地音、`UI_Click.ogg` | `QianQingExplorer`、`TaiHeFinale`。 |
| 保和殿对话 | `Cat_Meow.ogg`、`Cat_Meow_Reply.ogg` | `BaoHeEntranceController` 在墨宝台词处播放。 |
| 图鉴、按钮 | `GaoShanLiuShui_Game_BGM.ogg`、`UI_Click.ogg` | `CollectionGalleryController`、`UIButtonClickSound`。 |
| CG 补充音乐与氛围 | `QianQing_CG_Opening_BGM.wav`、`QianQing_CG_Clear_BGM.wav`、`QianQing_CG_Ambience.wav`、`TaiHe_CG_Ambience.wav` 等 | 乾清宫脚本和部分 `LevelIntroVideo` 场景配置；请以实际视频播放和场景引用复核。 |
| 结局 | `游戏完成后的背景音乐.ogg` | `EndingReveal` 播放结局视频时使用。 |

目录中还有 `Game_BGM.wav`、`Scene_Ending_BGM.wav` 等文件；文件存在不表示当前流程一定使用。若要删除、替换或对外声明音乐来源，先核对场景序列化引用、视频原声及团队音频制作记录。

## 下一轮听感检查

1. 从新进度播放六段视频，检查跳过或播完后的音乐恢复、是否与视频自带原声重叠，以及音量是否突然跳变。
2. 在午门分别试听跳跃、落地、脊兽、金币、伤害；脊兽收集声应比金币更容易辨认。
3. 在乾清宫查阅与答题、保和殿对话与拼图提交、太和殿搬运与嵌合时，确认重要反馈有清楚声音。当前部分交互借用 `UI_Click`，可后续换成更贴合材质的声音。
4. 用耳机和普通笔记本扬声器各听一轮，记录过响、过轻、循环接缝和长时间听感。

## 来源记录

`Tools/generate_cg_ambience.py` 与 `Tools/generate_qianqing_cg_music.py` 可追溯部分程序生成的 CG 氛围和音乐。其他音频的作者、录制或下载来源、编辑过程、参赛及再分发许可仍需团队逐项确认，见 [ASSET_PROVENANCE.md](ASSET_PROVENANCE.md)。不要仅凭文件名将其标为原创或已授权。
