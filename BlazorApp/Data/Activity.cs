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

    public bool getCanProcess() => _canProcess;


    void consume()
    {
        if(_processTime == 0){
            foreach(ISemaphoreOperations input in _inputs){
                _canProcess = input.canDecrement();
                if(_canProcess == false) return;
            }
            foreach(ISemaphoreOperations input in _inputs) input.decrement();
        }
    }


    void process()
    {
        if(_processTime > 0 || _canProcess) _processTime++;
        if(_processTime == _processDuration)
        {
            foreach(ISemaphoreOperations output in _outputs) output.decrement();
        }
        _canProcess = false;
    }

    bool isActive() => _processTime > 0;

}