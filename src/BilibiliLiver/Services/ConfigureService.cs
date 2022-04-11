﻿using BilibiliLiver.Config.Models;
using BilibiliLiver.Extensions;
using BilibiliLiver.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
{
    public class ConfigureService : IHostedService
    {
        private readonly ILogger<ConfigureService> _logger;
        private readonly IBilibiliAccountService _account;
        private readonly IBilibiliLiveApiService _api;
        private readonly IBilibiliCookieService _cookie;
        private readonly IPushStreamService _push;
        private readonly LiveSetting _liveSetting;

        public ConfigureService(ILogger<ConfigureService> logger
            , IBilibiliAccountService account
            , IBilibiliLiveApiService api
            , IBilibiliCookieService cookie
            , IPushStreamService push
            , IOptions<LiveSetting> liveSettingOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _cookie = cookie ?? throw new ArgumentNullException(nameof(cookie));
            _push = push ?? throw new ArgumentNullException(nameof(push));
            _liveSetting = liveSettingOptions.Value ?? throw new ArgumentNullException(nameof(liveSettingOptions));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //验证配置
            CheckLiveSetting();
            //检查Cookie
            CheckCookie();
            //登录
            var userInfo = await _account.Login();
            if (userInfo == null || !userInfo.IsLogin)
            {
                _logger.ThrowLogError("登录失败，Cookie无效或已过期，请重新配置Cookie！");
            }
            _logger.LogInformation($"用户{userInfo.Uname}登录成功！");
            //获取直播间信息
            var liveRoomInfo = await _api.GetLiveRoomInfo();
            if (liveRoomInfo == null)
            {
                _logger.ThrowLogError("获取直播间信息失败！");
            }
            if (liveRoomInfo.room_id == 0 || liveRoomInfo.have_live == 0)
            {
                _logger.ThrowLogError("当前用户未开启直播间！");
            }
            _logger.LogInformation($"获取直播间信息成功，当前直播间名称：{liveRoomInfo.title}，分区：{liveRoomInfo.parent_name}·{liveRoomInfo.area_v2_name}，直播状态：{(liveRoomInfo.live_status == 1 ? "直播中" : "未开启")}");
            //检查FFMpeg
            if(!await _push.FFmpegTest())
            {
                _logger.ThrowLogError("未找到FFmpeg，请先安装FFmpeg！");
            }
            //开始推流
            await _push.StartPush();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #region private

        /// <summary>
        /// 检查配置文件
        /// </summary>
        private void CheckLiveSetting()
        {
            if (_liveSetting.LiveAreaId <= 0)
            {
                _logger.ThrowLogError("配置文件appsettings.json中，LiveSetting.LiveAreaId填写错误！");
            }
            if (string.IsNullOrWhiteSpace(_liveSetting.LiveRoomName))
            {
                _logger.ThrowLogError("配置文件appsettings.json中，LiveSetting.LiveRoomName不能为空！");
            }
            if (string.IsNullOrWhiteSpace(_liveSetting.FFmpegCmd))
            {
                _logger.ThrowLogError("配置文件appsettings.json中，LiveSetting.FFmpegCmd不能为空！");
            }
            int markIndex = _liveSetting.FFmpegCmd.IndexOf("[[URL]]");
            if (markIndex < 5)
            {
                throw new Exception("配置文件appsettings.json中，LiveSetting.FFmpegCmd不正确，命令中未找到 '[[URL]]'标记。");
            }
            if (_liveSetting.FFmpegCmd[markIndex - 1] == '\"')
            {
                throw new Exception("配置文件appsettings.json中，LiveSetting.FFmpegCmd不正确， '[[URL]]'标记前后无需“\"”。");
            }
        }

        /// <summary>
        /// 检查COOKIE
        /// </summary>
        private void CheckCookie()
        {
            try
            {
                string cookie = _cookie.Get();
                if (string.IsNullOrWhiteSpace(cookie))
                {
                    _cookie.Init();
                    _logger.ThrowLogError("cookie.txt文件为空，请按照教程获取Bilibili Cookie之后放入程序目录下面的cookie.txt中！");
                }
                var cookieValues = _cookie.CookieDeserialize(cookie);
                if (cookieValues == null || cookieValues.Cookies.Count == 0)
                {
                    _logger.ThrowLogError("cookie.txt文件为空，请按照教程获取Bilibili Cookie之后放入程序目录下面的cookie.txt中！");
                }
                string userid = _cookie.GetUserId();
                if (string.IsNullOrEmpty(userid))
                {
                    _logger.ThrowLogError("从Cookie中获取用户Id失败，请按照教程获取Bilibili Cookie之后放入程序目录下面的cookie.txt中！");
                }
            }
            catch (Exception ex)
            {
                _cookie.Init();
                _logger.ThrowLogError($"cookie.txt未正确配置，错误：{ex.Message}", ex);
            }
        }

        #endregion
    }
}
