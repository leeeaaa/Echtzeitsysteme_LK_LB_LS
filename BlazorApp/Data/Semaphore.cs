namespace BlazorApp.Data;

public class Semaphore : ISemaphoreOperations
{
    private int _state;
    private readonly bool _isActivitySemaphore;

    private readonly int _numberInputs;

    public Semaphore(int state, bool isActivitySemaphore, int numberInputs){
        _state = state;
        _isActivitySemaphore = isActivitySemaphore;
        _numberInputs = numberInputs;
    }

    public bool CanDecrement()
    {
        return _state > 0;
    }

    public void Decrement()
    {
        _state--;
    }

    public void Increment()
    {
        _state++;
    }
}