namespace BlazorApp.Data;

public class Task
{
	public IList<Activity> Activities
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}


	public Task(string name) : this(new List<Activity>(), name) { }


	public Task(IList<Activity> activities, string name)
	{
		Activities = activities;
		Name = name;
	}

	public void AddActivity(Activity activity) => Activities.Add(activity);


}