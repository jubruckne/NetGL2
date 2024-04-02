namespace NetGL;

public struct Finally: IDisposable {
    private Action? action;

    public Finally(in Action action) => this.action = action;

    private void reset() {
        action?.Invoke();
        action = null;
    }

    public void Dispose() {
        reset();
    }
}