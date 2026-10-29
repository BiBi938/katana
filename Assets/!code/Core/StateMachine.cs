public interface IState<TContext>
{
    void Enter(TContext ctx);
    void Exit(TContext ctx);
    void Tick(TContext ctx, float dt);
}

public class StateMachine<TContext>
{
    public IState<TContext> Current { get; private set; }
    readonly TContext context;

    public StateMachine(TContext context)
    {
        this.context = context;
    }

    public void ChangeState(IState<TContext> next)
    {
        Current?.Exit(context);
        Current = next;
        Current?.Enter(context);
    }

    public void Tick(float dt) => Current?.Tick(context, dt);
}
