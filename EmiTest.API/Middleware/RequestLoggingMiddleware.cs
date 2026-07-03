using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EmiTest.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> _logger)
        {
            _next = next;
            this._logger = _logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Capturar detalles de la petición entrante (Sección 2.3)
            context.Request.EnableBuffering(); // Permite leer el Body múltiples veces sin consumirlo por completo

            var requestTime = DateTime.UtcNow;
            var method = context.Request.Method;
            var path = context.Request.Path;
            var queryString = context.Request.QueryString.ToUriComponent();

            // Leer el cuerpo de la petición de forma segura
            var bodyAsText = string.Empty;
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                bodyAsText = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reiniciar la posición para que los controladores puedan leerlo
            }

            // Loguear la petición HTTP entrante
            _logger.LogInformation("HTTP Request Information:\n" +
                                   "Time: {Time}\n" +
                                   "Method: {Method}\n" +
                                   "Path: {Path}{Query}\n" +
                                   "Body: {Body}",
                                   requestTime, method, path, queryString, string.IsNullOrEmpty(bodyAsText) ? "(Empty)" : bodyAsText);

            // 2. Invocar al siguiente middleware en el pipeline
            await _next(context);
        }
    }
}