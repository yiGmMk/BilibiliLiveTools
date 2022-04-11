﻿using BilibiliLiver.Model;
using BilibiliLiver.Model.Base;
using BilibiliLiver.Model.Enums;
using BilibiliLiver.Model.Exceptions;
using BilibiliLiver.Services.Interface;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
{
    public class BilibiliLiveApiService : IBilibiliLiveApiService
    {
        /// <summary>
        /// 获取直播间信息
        /// </summary>
        private const string _getLiveRoomInfoApi = "https://api.live.bilibili.com/xlive/app-blink/v1/room/GetInfo?platform=pc";

        /// <summary>
        /// 更新直播间名称
        /// </summary>
        private const string _updateLiveRoomNameApi = "https://api.live.bilibili.com/room/v1/Room/update";

        /// <summary>
        /// 获取直播种类
        /// </summary>
        private const string _getLiveCategoryApi = "https://api.live.bilibili.com/room/v1/Area/getList";

        /// <summary>
        /// 开启直播
        /// </summary>
        private const string _startLiveApi = "https://api.live.bilibili.com/room/v1/Room/startLive";

        /// <summary>
        /// 停止直播
        /// </summary>
        private const string _stopLiveApi = "https://api.live.bilibili.com/room/v1/Room/stopLive";

        private readonly ILogger<BilibiliLiveApiService> _logger;
        private readonly IHttpClientService _httpClient;
        private readonly IBilibiliCookieService _cookie;

        public BilibiliLiveApiService(ILogger<BilibiliLiveApiService> logger
            , IHttpClientService httpClient
            , IBilibiliCookieService cookie)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cookie = cookie ?? throw new ArgumentNullException(nameof(cookie));
        }

        public async Task<LiveRoomInfo> GetLiveRoomInfo()
        {
            var result = await _httpClient.Execute<ResultModel<LiveRoomInfo>>(_getLiveRoomInfoApi, Method.Get);
            if (result == null)
            {
                throw new ApiRequestException(_getLiveRoomInfoApi, Method.Get, "返回内容为空");
            }
            if (result.Code != 0)
            {
                throw new ApiRequestException(_getLiveRoomInfoApi, Method.Get, result.Message);
            }
            return result.Data;
        }

        public async Task<List<LiveAreaItem>> GetLiveAreas()
        {
            var result = await _httpClient.Execute<ResultModel<List<LiveAreaItem>>>(_getLiveCategoryApi, Method.Get);
            if (result == null)
            {
                throw new ApiRequestException(_getLiveCategoryApi, Method.Get, "返回内容为空");
            }
            if (result.Code != 0)
            {
                throw new ApiRequestException(_getLiveCategoryApi, Method.Get, result.Message);
            }
            return result.Data;
        }

        public async Task<bool> UpdateLiveRoomName(int roomId, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title), "直播间名称不能为空！");
            }
            var postData = new
            {
                room_id = roomId,
                title = title,
                csrf_token = _cookie.GetCsrf(),
                csrf = _cookie.GetCsrf(),
            };
            var result = await _httpClient.Execute<ResultModel<object>>(_updateLiveRoomNameApi, Method.Post, postData, BodyFormat.Form);
            if (result == null)
            {
                throw new ApiRequestException(_updateLiveRoomNameApi, Method.Post, "返回内容为空");
            }
            if (result.Code != 0)
            {
                throw new ApiRequestException(_updateLiveRoomNameApi, Method.Post, result.Message);
            }
            return result.Code == 0;
        }

        public async Task<bool> UpdateLiveRoomArea(int roomId, int areaId)
        {
            _ = await CheckArea(areaId);
            var postData = new
            {
                room_id = roomId,
                area_id = areaId,
                csrf_token = _cookie.GetCsrf(),
                csrf = _cookie.GetCsrf(),
            };
            var result = await _httpClient.Execute<ResultModel<object>>(_updateLiveRoomNameApi, Method.Post, postData, BodyFormat.Form);
            if (result == null)
            {
                throw new ApiRequestException(_updateLiveRoomNameApi, Method.Post, "返回内容为空");
            }
            if (result.Code != 0)
            {
                throw new ApiRequestException(_updateLiveRoomNameApi, Method.Post, result.Message);
            }
            return result.Code == 0;
        }

        public async Task<StartLiveInfo> StartLive(int roomId, int areaId)
        {
            var areaItem = await CheckArea(areaId);
            var postData = new
            {
                room_id = roomId,
                platform = "pc",
                area_v2 = areaItem.id,
                csrf_token = _cookie.GetCsrf(),
                csrf = _cookie.GetCsrf(),
            };
            var result = await _httpClient.Execute<ResultModel<StartLiveInfo>>(_startLiveApi, Method.Post, postData, BodyFormat.Form);
            if (result == null)
            {
                throw new ApiRequestException(_startLiveApi, Method.Post, "返回内容为空");
            }
            if (result.Code != 0)
            {
                throw new ApiRequestException(_startLiveApi, Method.Post, result.Message);
            }
            if (result.Data.need_face_auth)
            {
                throw new Exception("开启直播失败，需要进行人脸识别！");
            }
            return result.Data;
        }

        public async Task<StopLiveInfo> StopLive(int roomId)
        {
            var postData = new
            {
                room_id = roomId,
                platform = "pc",
                csrf_token = _cookie.GetCsrf(),
                csrf = _cookie.GetCsrf(),
            };
            var result = await _httpClient.Execute<ResultModel<StopLiveInfo>>(_stopLiveApi, Method.Post, postData, BodyFormat.Form);
            if (result == null)
            {
                throw new ApiRequestException(_stopLiveApi, Method.Post, "返回内容为空");
            }
            if (result.Code != 0)
            {
                throw new ApiRequestException(_stopLiveApi, Method.Post, result.Message);
            }
            return result.Data;
        }

        #region private

        /// <summary>
        /// 根据Id获取分区
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private LiveAreaItem FindItemFromLiveAreaTree(List<LiveAreaItem> source, int id)
        {
            foreach (var item in source)
            {
                if (item.id == id)
                {
                    return item;
                }
                if (item.list != null && item.list.Any())
                {
                    var result = FindItemFromLiveAreaTree(item.list, id);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 验证直播分区是否正确
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task<LiveAreaItem> CheckArea(int areaId)
        {
            if (areaId <= 0)
            {
                throw new ArgumentNullException(nameof(areaId), "直播间分类不能为空！");
            }
            List<LiveAreaItem> liveAreas = await GetLiveAreas();
            if (liveAreas == null || !liveAreas.Any())
            {
                throw new Exception("获取直播间分区信息失败！");
            }
            LiveAreaItem areaItem = FindItemFromLiveAreaTree(liveAreas, areaId);
            if (areaItem == null)
            {
                throw new Exception($"根据Id[{areaItem.id}]获取直播间分区信息失败！");
            }
            return areaItem;
        }

        #endregion
    }
}
