using System;
using System.Collections.Generic;

namespace Core.Foundation.StateMachine
{
    public class StateMachineManager
    {
        private IState _currentState;
        private Dictionary<Type, List<Transition>> _transitions = new();
        private List<Transition> _currentTransitions = new();
        private List<Transition> _fromAnyTransitions = new();
        private static List<Transition> _emptyTransitions = new(0);

        public void Tick()
        {
            var transition = GetTransition();
            if (transition != null) SetState(transition.NextState);

            _currentState?.Action();
        }

        public void FixedTick()
        {
            _currentState?.FixedAction();
        }

        public void SetState(IState state)
        {
            if (_currentState == state) return;

            _currentState?.Exit();
            _currentState = state;

            _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
            if (_currentTransitions == null) _currentTransitions = _emptyTransitions;

            _currentState.Enter();
        }

        public IState GetCurrentState()
        {
            return _currentState;
        }

        public void AddTransition(IState from, IState to, Func<bool> predicate)
        {
            if (!_transitions.TryGetValue(from.GetType(), out var transitions))
            {
                transitions = new List<Transition>();
                _transitions[from.GetType()] = transitions;
            }

            transitions.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(IState to, Func<bool> predicate)
        {
            _fromAnyTransitions.Add(new Transition(to, predicate));
        }


        private Transition GetTransition()
        {
            foreach (var transition in _fromAnyTransitions)
            {
                if (transition.ConditionSatisfied()) return transition;
            }

            foreach (var transition in _currentTransitions)
            {
                if (transition.ConditionSatisfied()) return transition;
            }

            return null;
        }

        private class Transition
        {
            public IState NextState { get; }
            public Func<bool> ConditionSatisfied { get; }

            public Transition(IState nextState, Func<bool> condition)
            {
                NextState = nextState;
                ConditionSatisfied = condition;
            }
        }
    }
}