namespace BlazorApp.Data;

public class Task
{
    private readonly IList<Activity> _activities;

    private readonly string _name;

    public Task(IList<Activity> activities, string name){
        _activities = activities;
        _name = name;
    }

    public IList<Activity> GetActivities(){
        return _activities;
    }
}