using MascotDesktop.Runtime.Avatar;
using MascotDesktop.Runtime.Core;
using NUnit.Framework;
using UnityEngine;

namespace MascotDesktop.Tests.EditMode
{
    public sealed class AvatarStateControllerTests
    {
        private GameObject _gameObject;
        private CoreOrchestrator _orchestrator;
        private MotionSlotPlayer _motionSlotPlayer;
        private AvatarStateController _stateController;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("AvatarStateControllerTests");
            _orchestrator = _gameObject.AddComponent<CoreOrchestrator>();
            _motionSlotPlayer = _gameObject.AddComponent<MotionSlotPlayer>();
            _stateController = _gameObject.AddComponent<AvatarStateController>();
            _motionSlotPlayer.RebuildSlotMap();
            _stateController.SetDependencies(_orchestrator, _motionSlotPlayer);
            _stateController.RebuildStateMap();
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
        public void ApplyState_UsesMappedSlot_ForHappy()
        {
            var result = _stateController.ApplyState("happy", "req-state-001");

            Assert.That(result.Success, Is.True);
            Assert.That(_stateController.CurrentState, Is.EqualTo("happy"));
            Assert.That(_motionSlotPlayer.CurrentSlot, Is.EqualTo("happy"));
        }

        [Test]
        public void ApplyState_UsesIdle_WhenUnknownState()
        {
            var result = _stateController.ApplyState("mystery_state", "req-state-002");

            Assert.That(result.Success, Is.True);
            Assert.That(_stateController.CurrentState, Is.EqualTo("mystery_state"));
            Assert.That(_motionSlotPlayer.CurrentSlot, Is.EqualTo("idle"));
        }

        [Test]
        public void CoreEvent_UpdatesStateController()
        {
            _orchestrator.ApplyAvatarState("sleepy", "req-state-003");

            Assert.That(_stateController.CurrentState, Is.EqualTo("sleepy"));
            Assert.That(_motionSlotPlayer.CurrentSlot, Is.EqualTo("sleepy"));
        }
    }
}
