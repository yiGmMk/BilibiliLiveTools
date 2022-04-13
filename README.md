# BilibiliLiveTools

Bilibili直播工具。自动登录并获取推流地址，可以用于电脑、树莓派等设备无人值守直播。

### 注意
因为B站随时在更新API，所以工具有随时挂掉的风险。当发现工具非配置原因导致不可用时，请提交issue。API也是本人参考github其他项目来的，未深入了解过B站APP，所以在未来遇到无法解决问题且无人接收情况下，此项目将会被废弃。

### 前提条件
- 网络良好，能正常访问B站，上传速度过得去。    
- 一台连接了摄像头的Linux/Windows电脑，要求不高，树莓派zero 2w都可以。  
- 在Bilibili中通过实名认证，并开通了直播间。[点击连接](https://link.bilibili.com/p/center/index "点击连接")开通直播间（很简单的，实名认证通过后直接就可以开通了）  
- 推流默认使用FFmpeg，树莓派官方系统默认安装了的，我就不再赘述，其它系统请自行安装。  

### 项目说明
1. BilibiliLiveAreaTool  
直播分区获取获取工具，可以通过此工具获取直播分区。  
2. BilibiliLiver  
一键开启直播工具。  
3. BilibiliLiverTests
单元测试神马的。

![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/1.png)

### 教程

#### 如何获取Bilibili Cookie
由于B站登录接口越来越难搞，无奈之下只有用Cookie了。幸运的是，B站的Cookie很容易就能获取到（理论上网站Cookie都能很容易拿到）。

1. 浏览器打开B站，并登陆  
一定要先登录，一定要先登录，一定要先登录！

2. 开启开发者选项  
登录后在浏览页面按F12（或者Ctrl+Shift+I），打开开发者选项。如图所示：  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/2.png)
选择“Network”，然后在地址栏打开：[https://api.bilibili.com/x/web-interface/nav](https://api.bilibili.com/x/web-interface/nav "https://api.bilibili.com/x/web-interface/nav")。  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/3.png)
打开之后，会看到一大串Json，不用管内容。然后点击右侧的nav（序号3），将图中序号4中cookie中的值拷贝出来，粘贴到程序目录下面的`cookie.txt`文件即可。  

#### 在Linux上面推流（这里以树莓派为例）
1. 获取程序  
```shell
wget https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliLiver_Linux_ARM.zip --no-check-certificate
```

2. 解压并授权
```shell
unzip BilibiliLiver_Linux_ARM.zip && chmod -R 755 BilibiliLiver_Linux_ARM && chmod +x BilibiliLiver_Linux_ARM/BilibiliLiver
```

3. 设置Cookie
```shell
cd BilibiliLiver_Linux_ARM
nano cookie.txt
```
然后编辑cookie.txt，并将上面获取到cookie粘贴进去。

4. 编辑直播设置  
编辑配置文件appsettings.json  
```shell
cd BilibiliLiver_Linux_ARM/
nano appsettings.json
```
配置文件如下所示，按照提示修改为自己的分区和直播间名称。
```json
{
  "LiveSetting": {
    //直播间分类
    "LiveAreaId": 369,
    //直播间名称
    "LiveRoomName": "【24H】小金鱼啦~",
    //FFmpeg推流命令，请自行填写对应操作系统和设备的推流命令
    //填写到此处时，请注意将命令中‘"’用‘\’进行转义，将推流的rtmp连接替换为[[URL]]，[[URL]]不需要双引号。
    //下面推流指令默认适配设备树莓派，使用USB摄像头，设备为/dev/video0
    "FFmpegCmd": "ffmpeg -thread_queue_size 1024 -f v4l2 -s 1280*720 -input_format mjpeg -i \"/dev/video0\" -stream_loop -1 -i \"Data/demo_music.m4a\" -vcodec h264_omx -pix_fmt yuv420p -r 30 -s 1280*720 -g 60 -b:v 10M -bufsize 10M -acodec aac -ac 2 -ar 44100 -ab 128k -f flv [[URL]]",
    //ffmpeg异常退出后，是否自动重新启动
    "AutoRestart": true
  }
}
```
由于推流方式不同以及FFmpeg配置的多样性，不同的平台、不同的硬件的参数都不相同（主要是懒，懒得去写FFmpeg的适配了，直接调用多巴适）。这里采用直接填写推流命令的方式。  
建议填写之前先测试推流命令能否正确执行。默认的推流命令设配树莓派官方系统，并且使用USB摄像头，设备Id为'/dev/video0'，其它系统可能不适用，需要自己修改。详情可以访问下发的博客连接。  
推流命令（FFmpegCmd）中的“[[URL]]”，是一个配置符号，将在程序中被替换为获取到的Bilibili推流地址，所以一定要在最终命令中，把测试文件或者地址修改为 “[[URL]]”（URL大写） ，否则程序将抛出错误。推流命令中注意半角双引号需要用符号‘\’来进行转义。  

5. 安装FFmpeg（可选）  
为什么是可选？因为树莓派官方系统已经默认内置了ffmpeg，不用自行安装。但是对于一些其他的linux发行版。可能没有安装ffmpeg，所以需要用户自行安装ffmpeg。这里只讨论debian系的linux，即使用apt作为包管理的发行版。  
```shell
# 安装，就这一行命令
sudo apt install ffmpeg
# 测试是否安装，有输出表示安装完成
ffmpeg -version
```

6. 跑起来  
```shell
sudo ./BilibiliLiver
```

**配置系统服务等，可以查看：https://www.quarkbook.com/?p=733**

#### 在Windows系统上面推流

1. 获取程序  
点击链接：[https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliLiver_Windows_x64.zip](https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliLiver_Windows_x64.zip "https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliLiver_Windows_x64.zip")
下载最新的适用于Windows系统的发布包。  

2. 设置Cookie  
```shell
cd BilibiliLiver_Linux_ARM
nano cookie.txt
```
然后编辑cookie.txt，并将上面获取到cookie粘贴进去。  

3. 编辑直播设置  
编辑配置文件appsettings.json  
```shell
cd BilibiliLiver_Linux_ARM/
nano appsettings.json
```
配置文件如下所示，按照提示修改为自己的分区和直播间名称。  
```json
{
  "LiveSetting": {
    //直播间分类
    "LiveAreaId": 369,
    //直播间名称
    "LiveRoomName": "【24H】小金鱼啦~",
    //FFmpeg推流命令，请自行填写对应操作系统和设备的推流命令
    //填写到此处时，请注意将命令中‘"’用‘\’进行转义，将推流的rtmp连接替换为[[URL]]，[[URL]]不需要双引号。
    //下面推流指令默认适配设备树莓派，使用USB摄像头，设备为/dev/video0
    "FFmpegCmd": "ffmpeg -thread_queue_size 1024 -f v4l2 -s 1280*720 -input_format mjpeg -i \"/dev/video0\" -stream_loop -1 -i \"Data/demo_music.m4a\" -vcodec h264_omx -pix_fmt yuv420p -r 30 -s 1280*720 -g 60 -b:v 10M -bufsize 10M -acodec aac -ac 2 -ar 44100 -ab 128k -f flv [[URL]]",
    //ffmpeg异常退出后，是否自动重新启动
    "AutoRestart": true
  }
}
```

4. 安装FFmpeg（可选）  
Windows版本随程序包发布有一个ffmpeg（解压后程序根目录），可以不用单独安装ffmpeg。  

5. 跑起来  
在地址栏输入cmd，如图所示：  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/4.png)
打开命令行之后，输入`BilibiliLiver.exe`。Enjoy it!

#### 常见问题

1. cookie.txt未正确配置  
巧妇难为无米之炊，请按照教程，配置cookie.txt  

2. FFmpeg报错  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/5.png)
肯定是你的ffmpeg指令有问题。这个工具严格意义上来说就是一个ffmpeg调用工具，ffmpeg的使用，全靠你自己。  

3. 通过Cookie登录失败  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/6.png)
cookie过期了，重新获取吧。  

#### 直播分区  
开播时需要将ID填写到LiveSetting中的LiveAreaId中。  
**请注意正确填写分区ID，不然会有被封的风险。**

|  AreaId | 分类名称  | 分区名称  |
| :------------ | :------------ | :------------ |
 | 86 | 英雄联盟 | 网游 | 
 | 88 | 穿越火线 | 网游 | 
 | 89 | CS:GO | 网游 | 
 | 240 | APEX英雄 | 网游 | 
 | 92 | DOTA2 | 网游 | 
 | 87 | 守望先锋 | 网游 | 
 | 80 | 吃鸡行动 | 网游 | 
 | 599 | 洛奇英雄传 | 网游 | 
 | 601 | 综合射击 | 网游 | 
 | 610 | QQ飞车 | 网游 | 
 | 252 | 逃离塔科夫 | 网游 | 
 | 102 | 最终幻想14 | 网游 | 
 | 329 | VALORANT | 网游 | 
 | 575 | 生死狙击2 | 网游 | 
 | 590 | 失落的方舟 | 网游 | 
 | 600 | 猎杀对决 | 网游 | 
 | 472 | CFHD  | 网游 | 
 | 84 | 300英雄 | 网游 | 
 | 91 | 炉石传说 | 网游 | 
 | 82 | 剑网3 | 网游 | 
 | 505 | 剑灵 | 网游 | 
 | 596 | 天涯明月刀 | 网游 | 
 | 519 | 超激斗梦境 | 网游 | 
 | 574 | 冒险岛 | 网游 | 
 | 487 | 逆战 | 网游 | 
 | 181 | 魔兽争霸3 | 网游 | 
 | 78 | DNF | 网游 | 
 | 499 | 剑网3怀旧服 | 网游 | 
 | 83 | 魔兽世界 | 网游 | 
 | 388 | FIFA ONLINE 4 | 网游 | 
 | 581 | NBA2KOL2 | 网游 | 
 | 318 | 使命召唤:战区 | 网游 | 
 | 249 | 星际战甲 | 网游 | 
 | 115 | 坦克世界 | 网游 | 
 | 248 | 战舰世界 | 网游 | 
 | 316 | 战争雷霆 | 网游 | 
 | 383 | 战意 | 网游 | 
 | 196 | 无限法则 | 网游 | 
 | 114 | 风暴英雄 | 网游 | 
 | 93 | 星际争霸2 | 网游 | 
 | 239 | 刀塔自走棋 | 网游 | 
 | 164 | 堡垒之夜 | 网游 | 
 | 251 | 枪神纪 | 网游 | 
 | 81 | 三国杀 | 网游 | 
 | 112 | 龙之谷 | 网游 | 
 | 173 | 古剑奇谭OL | 网游 | 
 | 176 | 幻想全明星 | 网游 | 
 | 300 | 封印者 | 网游 | 
 | 288 | 怀旧网游 | 网游 | 
 | 298 | 新游前瞻 | 网游 | 
 | 331 | 星战前夜：晨曦 | 网游 | 
 | 350 | 梦幻西游端游 | 网游 | 
 | 551 | 流放之路 | 网游 | 
 | 459 | 永恒轮回：黑色幸存者 | 网游 | 
 | 558 | 泰亚史诗 | 网游 | 
 | 607 | 激战2 | 网游 | 
 | 107 | 其他游戏 | 网游 | 
 | 35 | 王者荣耀 | 手游 | 
 | 256 | 和平精英 | 手游 | 
 | 395 | LOL手游 | 手游 | 
 | 321 | 原神 | 手游 | 
 | 550 | 幻塔 | 手游 | 
 | 506 | APEX英雄手游 | 手游 | 
 | 474 | 哈利波特：魔法觉醒  | 手游 | 
 | 514 | 金铲铲之战 | 手游 | 
 | 163 | 第五人格 | 手游 | 
 | 255 | 明日方舟 | 手游 | 
 | 613 | 重返帝国 | 手游 | 
 | 386 | 使命召唤手游 | 手游 | 
 | 37 | Fate/GO | 手游 | 
 | 525 | 少女前线：云图计划 | 手游 | 
 | 538 |  东方归言录 | 手游 | 
 | 36 | 阴阳师 | 手游 | 
 | 549 | 崩坏：星穹铁道 | 手游 | 
 | 504 | 航海王热血航线 | 手游 | 
 | 493 | 宝可梦大集结 | 手游 | 
 | 448 | 天地劫：幽城再临 | 手游 | 
 | 464 | 摩尔庄园手游 | 手游 | 
 | 442 | 坎公骑冠剑 | 手游 | 
 | 502 | 暗区突围 | 手游 | 
 | 354 | 综合棋牌 | 手游 | 
 | 140 | 决战！平安京 | 手游 | 
 | 407 | 游戏王：决斗链接 | 手游 | 
 | 408 | 天谕手游 | 手游 | 
 | 389 | 天涯明月刀手游 | 手游 | 
 | 293 | 战双帕弥什 | 手游 | 
 | 40 | 崩坏3 | 手游 | 
 | 330 | 公主连结Re:Dive | 手游 | 
 | 41 | 狼人杀 | 手游 | 
 | 286 | 百闻牌 | 手游 | 
 | 292 | 火影忍者手游 | 手游 | 
 | 511 | 漫威对决 | 手游 | 
 | 333 | CF手游 | 手游 | 
 | 154 | QQ飞车手游 | 手游 | 
 | 113 | 碧蓝航线 | 手游 | 
 | 352 | 三国杀移动版 | 手游 | 
 | 269 | 猫和老鼠手游 | 手游 | 
 | 156 | 影之诗 | 手游 | 
 | 206 | 剑网3指尖江湖 | 手游 | 
 | 608 | 文明与征服 | 手游 | 
 | 343 | DNF手游 | 手游 | 
 | 342 | 梦幻西游手游 | 手游 | 
 | 189 | 明日之后 | 手游 | 
 | 50 | 部落冲突:皇室战争 | 手游 | 
 | 39 | 少女前线 | 手游 | 
 | 42 | 解密游戏 | 手游 | 
 | 178 | 梦幻模拟战 | 手游 | 
 | 203 | 忍者必须死3 | 手游 | 
 | 258 | BanG Dream | 手游 | 
 | 212 | 非人学园 | 手游 | 
 | 214 | 雀姬 | 手游 | 
 | 265 | 跑跑卡丁车手游 | 手游 | 
 | 274 | 新游评测 | 手游 | 
 | 98 | 其他手游 | 手游 | 
 | 473 | 小动物之星 | 手游 | 
 | 492 | 暗黑破坏神：不朽 | 手游 | 
 | 469 | 荒野乱斗 | 手游 | 
 | 478 | 漫威超级战争 | 手游 | 
 | 576 | 恋爱养成游戏 | 手游 | 
 | 615 | 黑色沙漠手游 | 手游 | 
 | 236 | 主机游戏 | 单机游戏 | 
 | 555 | 艾尔登法环 | 单机游戏 | 
 | 443 | 永劫无间 | 单机游戏 | 
 | 216 | 我的世界 | 单机游戏 | 
 | 283 | 独立游戏 | 单机游戏 | 
 | 277 | 命运2 | 单机游戏 | 
 | 237 | 怀旧游戏 | 单机游戏 | 
 | 276 | 恐怖游戏 | 单机游戏 | 
 | 597 | 战地风云 | 单机游戏 | 
 | 612 | 幽灵线：东京 | 单机游戏 | 
 | 611 | 小缇娜的奇幻之地 | 单机游戏 | 
 | 586 | 消逝的光芒2 | 单机游戏 | 
 | 609 | 纪元：变异 | 单机游戏 | 
 | 591 | Dread Hunger | 单机游戏 | 
 | 587 | SIFU | 单机游戏 | 
 | 245 | 只狼 | 单机游戏 | 
 | 578 | 怪物猎人 | 单机游戏 | 
 | 218 | 饥荒 | 单机游戏 | 
 | 588 | 地平线 西之绝境 | 单机游戏 | 
 | 228 | 精灵宝可梦 | 单机游戏 | 
 | 582 | 暖雪 | 单机游戏 | 
 | 594 | 全面战争：战锤3 | 单机游戏 | 
 | 580 | 彩虹六号：异种 | 单机游戏 | 
 | 579 | 战神 | 单机游戏 | 
 | 585 | 神秘海域：盗贼传奇合辑 | 单机游戏 | 
 | 302 | FORZA 极限竞速 | 单机游戏 | 
 | 362 | NBA2K | 单机游戏 | 
 | 556 |  帝国神话 | 单机游戏 | 
 | 548 | 帝国时代4 | 单机游戏 | 
 | 559 | 光环：无限 | 单机游戏 | 
 | 537 | 孤岛惊魂6 | 单机游戏 | 
 | 460 | 弹幕互动游戏 | 单机游戏 | 
 | 309 | 植物大战僵尸 | 单机游戏 | 
 | 282 | 使命召唤 | 单机游戏 | 
 | 540 | 仙剑奇侠传七 | 单机游戏 | 
 | 357 | 糖豆人 | 单机游戏 | 
 | 223 | 灵魂筹码 | 单机游戏 | 
 | 433 | 格斗游戏 | 单机游戏 | 
 | 226 | 荒野大镖客2 | 单机游戏 | 
 | 426 | 重生细胞 | 单机游戏 | 
 | 227 | 刺客信条 | 单机游戏 | 
 | 387 | 恐鬼症 | 单机游戏 | 
 | 219 | 以撒 | 单机游戏 | 
 | 446 | 双人成行 | 单机游戏 | 
 | 295 | 方舟 | 单机游戏 | 
 | 313 | 仁王2 | 单机游戏 | 
 | 244 | 鬼泣5 | 单机游戏 | 
 | 364 | 枪火重生 | 单机游戏 | 
 | 570 | 策略游戏 | 单机游戏 | 
 | 341 | 盗贼之海 | 单机游戏 | 
 | 507 | 胡闹厨房 | 单机游戏 | 
 | 496 | 人间地狱 | 单机游戏 | 
 | 500 | 体育游戏 | 单机游戏 | 
 | 439 | 恐惧之间 | 单机游戏 | 
 | 308 | 塞尔达 | 单机游戏 | 
 | 261 | 马里奥制造2 | 单机游戏 | 
 | 243 | 全境封锁2 | 单机游戏 | 
 | 326 | 骑马与砍杀 | 单机游戏 | 
 | 270 | 人类一败涂地 | 单机游戏 | 
 | 424 | 鬼谷八荒 | 单机游戏 | 
 | 273 | 无主之地3 | 单机游戏 | 
 | 220 | 辐射76 | 单机游戏 | 
 | 257 | 全面战争 | 单机游戏 | 
 | 463 | 亿万僵尸 | 单机游戏 | 
 | 535 | 暗黑破坏神2 | 单机游戏 | 
 | 583 | 文字游戏 | 单机游戏 | 
 | 592 | 恋爱模拟游戏 | 单机游戏 | 
 | 593 | 泰拉瑞亚 | 单机游戏 | 
 | 441 | 雨中冒险2 | 单机游戏 | 
 | 604 | 星之卡比：探索发现 | 单机游戏 | 
 | 605 | 从军 | 单机游戏 | 
 | 614 | 赏金游戏 | 单机游戏 | 
 | 616 | 午夜猎魂 | 单机游戏 | 
 | 235 | 其他单机 | 单机游戏 | 
 | 21 | 视频唱见 | 娱乐 | 
 | 530 | 萌宅领域 | 娱乐 | 
 | 145 | 视频聊天 | 娱乐 | 
 | 207 | 舞见 | 娱乐 | 
 | 123 | 户外 | 娱乐 | 
 | 399 | 日常 | 娱乐 | 
 | 602 | 运动 | 娱乐 | 
 | 339 | 放松电台 | 电台 | 
 | 190 | 唱见电台 | 电台 | 
 | 192 | 聊天电台 | 电台 | 
 | 193 | 配音 | 电台 | 
 | 371 | 虚拟主播 | 虚拟主播 | 
 | 367 | 美食 | 生活 | 
 | 369 | 萌宠 | 生活 | 
 | 378 | 时尚 | 生活 | 
 | 33 | 影音馆 | 生活 | 
 | 376 | 人文社科 | 学习 | 
 | 375 | 科技科普 | 学习 | 
 | 377 | 职业技能 | 学习 | 
 | 372 | 陪伴学习 | 学习 | 
 | 373 | 绘画 | 学习 | 
 | 561 | 游戏赛事 | 赛事 | 
 | 562 | 体育赛事 | 赛事 | 
 | 563 | 赛事综合 | 赛事 | 
 
 ## Stargazers over time
[![Stargazers over time](https://starchart.cc/withsalt/BilibiliLiveTools.svg)](https://starchart.cc/withsalt/BilibiliLiveTools)
