namespace BlazorApp.Data;

public class Activity 
{
    private readonly IList<ISemaphoreOperations> _inputs;

    private readonly IList<ISemaphoreOperations> _outputs;

    private readonly int _processDuration;

    private int _processTime;

    private bool _canProcess = false;

    private readonly string _name;

    public Activity(IList<ISemaphoreOperations> inputs, IList<ISemaphoreOperations> outputs,
        int processDuration, int processTime, string name)
    {
        _inputs = inputs;
        _outputs = outputs;
        _processDuration = processDuration;
        _processTime = processTime;
        _name = name;
    }


    public void Consume()
    {
        if(_processTime == 0){
            foreach(ISemaphoreOperations input in _inputs){
                _canProcess = input.CanDecrement();
                if(_canProcess == false) return;
            }
            foreach(ISemaphoreOperations input in _inputs) input.Decrement();
        }
    }


    public void Process()
    {
        if(_processTime > 0 || _canProcess) _processTime++;
        if(_processTime == _processDuration)
        {
            foreach(ISemaphoreOperations output in _outputs) output.Increment();
            _processTime = 0;
        }
        _canProcess = false;
    }

    public bool IsActive() => _processTime > 0;

}