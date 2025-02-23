﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using Microsoft.Identity.Client.Core;

namespace Microsoft.Identity.Client.Internal.Logger
{
    internal class LegacyIdentityLoggerAdapter : ILoggerAdapter
    {
        LogLevel _minLogLevel = LogLevel.Always;
        LogCallback _logCallback;
        private string _clientInfo;
        private string _correlationId;

        public bool PiiLoggingEnabled { get; }
        public bool IsDefaultPlatformLoggingEnabled { get; }

        public string ClientName { get; }
        public string ClientVersion { get; }

        public bool IsLoggingEnabled(LogLevel logLevel)
        {
            return _logCallback != null && logLevel <= _minLogLevel;
        }

        internal LegacyIdentityLoggerAdapter(
            Guid correlationId,
            string clientName,
            string clientVersion,
            LogLevel logLevel,
            bool enablePiiLogging,
            bool isDefaultPlatformLoggingEnabled,
            LogCallback loggingCallback)
        {
            ClientName = clientName;
            ClientVersion = clientVersion;
            _correlationId = correlationId.Equals(Guid.Empty)
                    ? string.Empty
                    : " - " + correlationId;

            _clientInfo = LoggerHelper.GetClientInfo(clientName, clientVersion);

            PiiLoggingEnabled = enablePiiLogging;
            _logCallback = loggingCallback;
            _minLogLevel = logLevel;
            IsDefaultPlatformLoggingEnabled = isDefaultPlatformLoggingEnabled;
        }

        public void Log(LogLevel logLevel, string messageWithPii, string messageScrubbed)
        {
            if (IsLoggingEnabled(logLevel))
            {
                string message = LoggerHelper.FormatLogMessage(messageWithPii, messageScrubbed, PiiLoggingEnabled, _correlationId, _clientInfo);
                _logCallback.Invoke(logLevel, message, !string.IsNullOrEmpty(messageWithPii) ? true : false);
            }
        }

        public static ILoggerAdapter Create(
            Guid correlationId,
            ApplicationConfiguration config,
            bool isDefaultPlatformLoggingEnabled = false)
        {
            return new LegacyIdentityLoggerAdapter(
                correlationId,
                config?.ClientName ?? string.Empty,
                config?.ClientVersion ?? string.Empty,
                config?.LogLevel ?? LogLevel.Verbose,
                config?.EnablePiiLogging ?? false,
                config?.IsDefaultPlatformLoggingEnabled ?? isDefaultPlatformLoggingEnabled,
                config?.LoggingCallback);
        }

        public DurationLogHelper LogBlockDuration(string measuredBlockName, LogLevel logLevel = LogLevel.Verbose)
        {
            return new DurationLogHelper(this, measuredBlockName, logLevel);
        }

        public DurationLogHelper LogMethodDuration(LogLevel logLevel = LogLevel.Verbose, [CallerMemberName] string methodName = null, [CallerFilePath] string filePath = null)
        {
            return LoggerHelper.LogMethodDuration(this, logLevel, methodName, filePath);
        }
    }
}
