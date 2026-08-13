using System;
using System.Collections.Generic;
using UtinComputerTest.Infrastructure.StateMachine.States.Base;
using UnityEngine;

namespace UtinComputerTest.Infrastructure.StateMachine
{
    public class StateMachine : IStateMachine
    {
        protected static bool LogNonErrors => false;

        protected readonly Dictionary<Type, IExitableState> _states = new();

        private IExitableState _activeState;

        public void EnterState<TState>() where TState : IState
        {
            if (!_states.ContainsKey(typeof(TState)))
            {
                Debug.LogError($"Unable to enter state. State: '{typeof(TState).Name}' not found in dictionary: '{nameof(_states)}'");
                return;
            }

            _activeState?.Exit();

            if (_states[typeof(TState)] is not TState state)
            {
                return;
            }

            _activeState = state;
            if (LogNonErrors)
            {
                Debug.Log($"Entered state: '{typeof(TState).Name}'");
            }
            state.Enter();
        }

        public void EnterState<TState, TArgs>(TArgs args) where TState : IStateWithArgument<TArgs>
        {
            if (!_states.ContainsKey(typeof(TState)))
            {
                Debug.LogError($"Unable to enter state. State: '{typeof(TState).Name}' not found in dictionary: '{nameof(_states)}'");
                return;
            }

            _activeState?.Exit();

            if (_states[typeof(TState)] is not TState state)
            {
                return;
            }

            if (LogNonErrors)
            {
                Debug.Log($"Entered state: '{typeof(TState).Name}'");
            }
            state.Enter(args);
        }

        public void AddState<TState>(TState state) where TState : IExitableState => _states.Add(typeof(TState), state);
    }
}
