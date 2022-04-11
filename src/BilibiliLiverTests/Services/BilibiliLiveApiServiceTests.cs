﻿using BilibiliLiver.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliLiverTests;
using BilibiliLiver.Services.Interface;

namespace BilibiliLiver.Services.Tests
{
    [TestClass()]
    public class BilibiliLiveApiServiceTests : BilibiliLiverTestsBase
    {
        private readonly IBilibiliLiveApiService _apiService;

        public BilibiliLiveApiServiceTests()
        {
            _apiService = (IBilibiliLiveApiService)ServiceProvider.GetService(typeof(IBilibiliLiveApiService));
            if (_apiService == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public async Task GetLiveRoomInfoTest()
        {
            var info = await _apiService.GetLiveRoomInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod()]
        public async Task UpdateLiveRoomNameTest()
        {
            var info = await _apiService.GetLiveRoomInfo();
            bool reslt = await _apiService.UpdateLiveRoomName(info.room_id, info.title);
            Assert.IsTrue(reslt);
        }

        [TestMethod()]
        public async Task GetLiveAreasTest()
        {
            var info = await _apiService.GetLiveAreas();
            Assert.IsNotNull(info);
        }

        [TestMethod()]
        public async Task StartLiveTest()
        {
            var info = await _apiService.GetLiveRoomInfo();
            var reslt = await _apiService.StartLive(info.room_id, "369");
            Assert.IsNotNull(reslt);
        }

        [TestMethod()]
        public async Task StopLiveTest()
        {
            var info = await _apiService.GetLiveRoomInfo();
            var reslt = await _apiService.StopLive(info.room_id);

            var liveRoomInfo = await _apiService.GetLiveRoomInfo();
            if(liveRoomInfo.live_status != 0)
            {
                Assert.Fail();
            }

            Assert.IsNotNull(reslt);
        }

        [TestMethod()]
        public async Task UpdateLiveRoomAreaTest()
        {
            var info = await _apiService.GetLiveRoomInfo();
            var reslt = await _apiService.UpdateLiveRoomArea(info.room_id, "369");
            Assert.IsTrue(reslt);
        }
    }
}