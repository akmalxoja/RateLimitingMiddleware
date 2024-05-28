using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _limit;
    private readonly TimeSpan _timeSpan;
    private readonly ConcurrentDictionary<string, RequestLog> _clients;

    public RateLimitingMiddleware(RequestDelegate next, int limit, TimeSpan timeSpan)
    {
        _next = next;
        _limit = limit;
        _timeSpan = timeSpan;
        _clients = new ConcurrentDictionary<string, RequestLog>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Get)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            if (clientIp == null)
            {
                await _next(context);
                return;
            }

            var now = DateTime.UtcNow;
            var requestLog = _clients.GetOrAdd(clientIp, new RequestLog());

            lock (requestLog)
            {
                requestLog.RequestTimes.RemoveAll(time => (now - time) > _timeSpan);

                if (requestLog.RequestTimes.Count >= _limit)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    return;
                }

                requestLog.RequestTimes.Add(now);
            }
        }

        await _next(context);
    }

    private class RequestLog
    {
        public List<DateTime> RequestTimes { get; set; } = new List<DateTime>();
    }
}
