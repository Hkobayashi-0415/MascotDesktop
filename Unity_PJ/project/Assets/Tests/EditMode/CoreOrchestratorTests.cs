using System;
using MascotDesktop.Runtime.Core;
using NUnit.Framework;
using UnityEngine;

namespace MascotDesktop.Tests.EditMode
{
    public sealed class CoreOrchestratorTests
    {
        private GameObject _gameObject;
        private CoreOrchestrator _orchestrator;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("CoreOrchestratorTests");
            _orchestrator = _gameObject.AddComponent<CoreOrchestrator>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public void ApplyAvatarState_NormalizesToIdle_WhenEmpty()
        {
            _orchestrator.ApplyAvatarState("   ", "req-core-001");
            Assert.That(_orchestrator.CurrentAvatarState, Is.EqualTo("idle"));
        }

        [Test]
        public void SendChat_RaisesStateChanged_ForHappyMessage()
        {
            var invoked = false;
            var capturedState = string.Empty;
            var capturedRequestId = string.Empty;
            _orchestrator.AvatarStateChanged += (rid, state) =>
            {
                invoked = true;
                capturedRequestId = rid;
                capturedState = state;
            };

            _orchestrator.SendChat("I am happy today", "req-core-002");

            Assert.That(invoked, Is.True);
            Assert.That(capturedRequestId, Is.EqualTo("req-core-002"));
            Assert.That(capturedState, Is.EqualTo("happy"));
        }

        [Test]
        public void ScheduleReminder_TriggersWhenDue()
        {
            var invoked = false;
            var capturedRequestId = string.Empty;
            var capturedMessage = string.Empty;
            _orchestrator.ReminderTriggered += (rid, message) =>
            {
                invoked = true;
                capturedRequestId = rid;
                capturedMessage = message;
            };

            _orchestrator.ScheduleReminder("ping", 0.0, "req-core-003");
            _orchestrator.ProcessDueRemindersForTests(DateTimeOffset.UtcNow.AddSeconds(1));

            Assert.That(invoked, Is.True);
            Assert.That(capturedRequestId, Is.EqualTo("req-core-003"));
            Assert.That(capturedMessage, Is.EqualTo("ping"));
        }
    }
}
