﻿using BilibiliLiver.Config;
using BilibiliLiver.Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace BilibiliLiver.Services
{
    public class BilibiliCookieService : IBilibiliCookieService
    {
        private readonly ILogger<BilibiliCookieService> _logger;
        private readonly IMemoryCache _cache;

        private string _cookiePath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "cookie.txt");

        public BilibiliCookieService(ILogger<BilibiliCookieService> logger
            , IMemoryCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public string Get(bool force = false)
        {
            if (force)
            {
                _cache.Remove(CacheKeyConstant.COOKIE_KEY);
            }

            return _cache.GetOrCreate<string>(CacheKeyConstant.COOKIE_KEY, entry =>
            {
                if (!File.Exists(_cookiePath))
                {
                    throw new FileNotFoundException("File 'cookie.txt' not fount.");
                }
                string result = File.ReadAllText(_cookiePath);
                if (!CookieHeaderValue.TryParse(result, out _))
                {
                    throw new Exception("Parse cookie failed.");
                }
                return result;
            });
        }

        public void Init()
        {
            if (!File.Exists(_cookiePath))
            {
                File.Create(_cookiePath);
            }
        }

        public CookieHeaderValue CookieDeserialize(string cookieText)
        {
            if (string.IsNullOrEmpty(cookieText))
            {
                throw new ArgumentNullException(nameof(cookieText));
            }
            if (!CookieHeaderValue.TryParse(cookieText, out CookieHeaderValue value))
            {
                throw new Exception("Parse cookie failed.");
            }
            return value;
        }

        public string GetCsrf()
        {
            string jct = GetValueFromCookie("bili_jct");
            if (string.IsNullOrWhiteSpace(jct))
            {
                throw new Exception("Get csrf from cookie failed.");
            }
            return jct;
        }

        public string GetUserId()
        {
            string userId = GetValueFromCookie("DedeUserID");
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new Exception("Get userid from cookie failed.");
            }
            return userId;
        }

        private string GetValueFromCookie(string key)
        {
            CookieHeaderValue values = CookieDeserialize(Get());
            if (!values.Cookies.Any(p => p.Name == key))
            {
                return null;
            }
            string result = values[key].Value;
            if (string.IsNullOrWhiteSpace(result))
            {
                return null;
            }
            return result.Trim();
        }
    }
}
