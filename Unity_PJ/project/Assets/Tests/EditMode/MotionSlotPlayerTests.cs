using System.Text.RegularExpressions;
using MascotDesktop.Runtime.Avatar;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MascotDesktop.Tests.EditMode
{
    public sealed class MotionSlotPlayerTests
    {
        private GameObject _gameObject;
        private MotionSlotPlayer _player;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("MotionSlotPlayerTests");
            _player = _gameObject.AddComponent<MotionSlotPlayer>();
            _player.RebuildSlotMap();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public void PlaySlot_UsesRequestedSlot_WhenExists()
        {
            var result = _player.PlaySlot("idle", "req-motion-001");

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResolvedSlot, Is.EqualTo("idle"));
            Assert.That(_player.CurrentSlot, Is.EqualTo("idle"));
        }

        [Test]
        public void PlaySlot_UsesFallback_WhenUnknownSlot()
        {
            var result = _player.PlaySlot("unknown_slot", "req-motion-002");

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResolvedSlot, Is.EqualTo("idle"));
            Assert.That(result.ErrorCode, Is.EqualTo("AVATAR.MOTION.SLOT_FALLBACK"));
            Assert.That(_player.CurrentSlot, Is.EqualTo("idle"));
        }

        [Test]
        public void PlaySlot_ReturnsFailure_WhenSlotEmpty()
        {
            LogAssert.Expect(LogType.Error, new Regex("\"error_code\":\"AVATAR.MOTION.SLOT_EMPTY\""));
            var result = _player.PlaySlot(string.Empty, "req-motion-003");

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("AVATAR.MOTION.SLOT_EMPTY"));
        }
    }
}
