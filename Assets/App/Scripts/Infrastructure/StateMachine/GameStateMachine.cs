using System;
using System.Collections.Generic;
using UtinComputerTest.Infrastructure.StateMachine.States.Base;
using UnityEngine;

namespace UtinComputerTest.Infrastructure.StateMachine {
    public class GameStateMachine : StateMachine, IGameStateMachine {
        private readonly List<Type> _sceneStateTypes = new();

        public void AddSceneState<TState>(TState state) where TState : IExitableState {
            if (_states.ContainsKey(typeof(TState)))
            {
                Debug.LogWarning($"[GameStateMachine] State {typeof(TState).Name} already exists. Skipping.");
                return;
            }

            AddState(state);
            _sceneStateTypes.Add(typeof(TState));
        }

        public void ClearSceneStates() {
            foreach (var stateType in _sceneStateTypes) {
                if (_states.ContainsKey(stateType)) {
                    _states.Remove(stateType);
                }
            }
            
            _sceneStateTypes.Clear();
            if (LogNonErrors) {
                Debug.Log("Scene states have been cleared.");
            }
        }
    }
}