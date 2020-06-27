public class StateMachine
{
    public delegate void Callback();

    State currentState;
    State[] states;

    public StateMachine(int numStates)
    {
        states = new State[numStates];
    }

    public void AddState(int state, Callback update, Callback begin, Callback end)
    {
        State s = new State();
        s.update = update;
        s.begin = begin;
        s.end = end;

        states[state] = s;
    }

    public void SetState(int state)
    {
        if (currentState.end != null) 
        {
            currentState.end();
        }

        currentState = states[state];

        if (currentState.begin != null) 
        {
            currentState.begin();
        }
    }

    public void Update()
    {
        if (currentState.update != null)
        {
           currentState.update(); 
        } 
    }

    struct State
    {
        public Callback update, begin, end;
    }
}
