﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Settings.Configuration
{
    /// <summary>
    /// Contains "fake extension" methods for the Serilog configuration API.
    /// By default the settings know how to find extension methods, but some configuration
    /// are actually "regular" method calls and would not be found otherwise.
    ///
    /// This static class contains internal methods that can be used instead.
    ///
    /// </summary>
    static class SurrogateConfigurationMethods
    {
        static readonly Dictionary<Type, MethodInfo[]> SurrogateMethodCandidates = typeof(SurrogateConfigurationMethods)
            .GetTypeInfo().DeclaredMethods
            .GroupBy(m => m.GetParameters().First().ParameterType)
            .ToDictionary(g => g.Key, g => g.ToArray());


        internal static readonly MethodInfo[] WriteTo = SurrogateMethodCandidates[typeof(LoggerSinkConfiguration)];
        internal static readonly MethodInfo[] AuditTo = SurrogateMethodCandidates[typeof(LoggerAuditSinkConfiguration)];
        internal static readonly MethodInfo[] Enrich = SurrogateMethodCandidates[typeof(LoggerEnrichmentConfiguration)];
        internal static readonly MethodInfo[] Destructure = SurrogateMethodCandidates[typeof(LoggerDestructuringConfiguration)];
        internal static readonly MethodInfo[] Filter = SurrogateMethodCandidates[typeof(LoggerFilterConfiguration)];

        /*
        Pass-through calls to various Serilog config methods which are
        implemented as instance methods rather than extension methods. The
        FindXXXConfigurationMethods calls (above) use these to add method
        invocation expressions as surrogates so that SelectConfigurationMethod
        has a way to match and invoke these instance methods.
        */

        // ReSharper disable UnusedMember.Local
        // those methods are discovered through reflection by `SurrogateMethodCandidates`
        // ReSharper has no way to see that they are actually used ...

        // .WriteTo...
        // ========
        static LoggerConfiguration Sink(
            LoggerSinkConfiguration loggerSinkConfiguration,
            ILogEventSink sink,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null)
        {
            return loggerSinkConfiguration.Sink(sink, restrictedToMinimumLevel, levelSwitch);
        }

        static LoggerConfiguration Logger(
            LoggerSinkConfiguration loggerSinkConfiguration,
            Action<LoggerConfiguration> configureLogger,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null)
            => loggerSinkConfiguration.Logger(configureLogger, restrictedToMinimumLevel, levelSwitch);

        // .AuditTo...
        // ========
        static LoggerConfiguration Sink(
            LoggerAuditSinkConfiguration auditSinkConfiguration,
            ILogEventSink sink,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null)
        {
            return auditSinkConfiguration.Sink(sink, restrictedToMinimumLevel, levelSwitch);
        }

        // .Filter...
        // =======
        // TODO: add overload for array argument (ILogEventEnricher[])
        static LoggerConfiguration With(LoggerFilterConfiguration loggerFilterConfiguration, ILogEventFilter filter)
            => loggerFilterConfiguration.With(filter);

        // .Destructure...
        // ============
        // TODO: add overload for array argument (IDestructuringPolicy[])
        static LoggerConfiguration With(LoggerDestructuringConfiguration loggerDestructuringConfiguration, IDestructuringPolicy policy)
            => loggerDestructuringConfiguration.With(policy);

        static LoggerConfiguration ToMaximumDepth(LoggerDestructuringConfiguration loggerDestructuringConfiguration, int maximumDestructuringDepth)
            => loggerDestructuringConfiguration.ToMaximumDepth(maximumDestructuringDepth);

        static LoggerConfiguration ToMaximumStringLength(LoggerDestructuringConfiguration loggerDestructuringConfiguration, int maximumStringLength)
            => loggerDestructuringConfiguration.ToMaximumStringLength(maximumStringLength);

        static LoggerConfiguration ToMaximumCollectionCount(LoggerDestructuringConfiguration loggerDestructuringConfiguration, int maximumCollectionCount)
            => loggerDestructuringConfiguration.ToMaximumCollectionCount(maximumCollectionCount);

        static LoggerConfiguration AsScalar(LoggerDestructuringConfiguration loggerDestructuringConfiguration, Type scalarType)
            => loggerDestructuringConfiguration.AsScalar(scalarType);

        // .Enrich...
        // =======
        static LoggerConfiguration FromLogContext(LoggerEnrichmentConfiguration loggerEnrichmentConfiguration)
            => loggerEnrichmentConfiguration.FromLogContext();

        static LoggerConfiguration With(LoggerEnrichmentConfiguration loggerEnrichmentConfiguration, ILogEventEnricher enricher)
        {
            return loggerEnrichmentConfiguration.With(enricher);
        }

        // ReSharper restore UnusedMember.Local
    }
}
