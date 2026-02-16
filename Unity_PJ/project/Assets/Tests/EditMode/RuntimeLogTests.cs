using System;
using System.Text.RegularExpressions;
using MascotDesktop.Runtime.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MascotDesktop.Tests.EditMode
{
    public sealed class RuntimeLogTests
    {
        [Test]
        public void NewRequestId_HasExpectedFormat()
        {
            var requestId = RuntimeLog.NewRequestId();
            Assert.That(requestId, Does.Match("^req-[0-9a-f]{32}$"));
        }

        [Test]
        public void NewRequestId_GeneratesUniqueIds()
        {
            var id1 = RuntimeLog.NewRequestId();
            var id2 = RuntimeLog.NewRequestId();

            Assert.That(id1, Is.Not.EqualTo(id2));
        }

        [Test]
        public void Warn_AssignsDefaultErrorCode_WhenMissing()
        {
            LogAssert.Expect(LogType.Warning, new Regex("\"error_code\":\"RUNTIME.WARN.UNSPECIFIED\""));
            RuntimeLog.Warn("tests", "tests.runtime.warn_missing_code", "req-test-warn");
        }

        [Test]
        public void Error_AssignsDefaultErrorCode_WhenMissing()
        {
            LogAssert.Expect(LogType.Error, new Regex("\"error_code\":\"RUNTIME.ERROR.UNSPECIFIED\""));
            RuntimeLog.Error("tests", "tests.runtime.error_missing_code", "req-test-error");
        }

        [Test]
        public void Info_NormalizesOptionalFieldsToEmptyString()
        {
            LogAssert.Expect(
                LogType.Log,
                new Regex("\"error_code\":\"\".*\"path\":\"\".*\"source_tier\":\"\""));
            RuntimeLog.Info("tests", "tests.runtime.info_defaults", "req-test-info");
        }

        [Test]
        public void Info_OutputsLogWithInfoLevel()
        {
            LogAssert.Expect(LogType.Log, new Regex("\"level\":\"INFO\""));
            RuntimeLog.Info("test", "test.event", "req-test-info-level", "test message");
        }

        [Test]
        public void Error_IncludesExceptionInfo()
        {
            var ex = new InvalidOperationException("Test exception message");
            LogAssert.Expect(LogType.Error, new Regex("\"exception_type\":\"System.InvalidOperationException\""));

            RuntimeLog.Error("test", "test.exception", "req-test-ex", "TEST.EX.CODE", "with exception", null, null, ex);
        }

        [Test]
        public void Write_FillsDefaultComponent_WhenEmpty()
        {
            LogAssert.Expect(LogType.Log, new Regex("\"component\":\"runtime\""));
            RuntimeLog.Info(null, "test.event", "req-test-default-component", "message");
        }

        [Test]
        public void Write_FillsDefaultEventName_WhenEmpty()
        {
            LogAssert.Expect(LogType.Log, new Regex("\"event_name\":\"runtime.event\""));
            RuntimeLog.Info("test", null, "req-test-default-event", "message");
        }
    }
}

