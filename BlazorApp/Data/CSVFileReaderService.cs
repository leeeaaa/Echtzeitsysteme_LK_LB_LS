using System.Runtime.InteropServices.ComTypes;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorApp.Data;

public class CsvFileReaderService
{

	private List<Task> ConvertCsvToObjects(List<List<string>> elements)
	{
		List<Task> tasks = new();
		List<Activity> activities = new();
		List<Semaphore> semaphores = new();

		elements.FindAll(element => element.First() == "Task").ForEach(taskList => tasks.Add(CreateTaskFromList(taskList)));
		elements.FindAll(element => element.First() == "Activity").ForEach(activityList =>
		{
			var activity = CreateActivityFromList(activityList);
			tasks.Find(task => task.Name == activityList[1])?.AddActivity(activity);
			activities.Add(activity);
		});
		elements.FindAll(element => element.First() == "Semaphore").ForEach(semaphoreList =>
		{
			var existingSemaphore = semaphores.Find(semaphore => semaphore.Name == semaphoreList[1]);
			if (existingSemaphore is null)
			{
				existingSemaphore = CreateSemaphoreFromList(semaphoreList);
				semaphores.Add(existingSemaphore);
			}
			else
			{
				existingSemaphore.IncrementNumberInputs();
			}

			var outputActivity = activities.Find(activity => activity.Name == semaphoreList[3]);
			outputActivity?.AddOutput(existingSemaphore);
			var inputActivity = activities.Find(activitiy => activitiy.Name == semaphoreList[4]);
			inputActivity?.AddInput(existingSemaphore);

			if (inputActivity != null && outputActivity != null)
			{
				if (tasks.Find(task =>
					    task.Activities.Contains(inputActivity) && task.Activities.Contains(outputActivity)) !=
				    null) existingSemaphore.SetActivitySemaphore();
			}
		});

		elements.FindAll(element => element.First() == "Mutex").ForEach(mutexList =>
		{
			var mutex = CreateMutexFromList(mutexList);

			for (int i = 2; i < mutexList.Count; i++)
			{
				var activity = activities.Find(activitiy => activitiy.Name == mutexList[i]);
				activity?.AddInput(mutex);
				activity?.AddOutput(mutex);
			}
		});

		return tasks;
	}

	public System.Threading.Tasks.Task<List<Task>> ReadCsvFileToObjectAsync(System.IO.Stream iStream)
	{

		return System.Threading.Tasks.Task.Run(() =>
		{
			List<List<string>> elements = new();
			iStream.Seek(0, SeekOrigin.Begin);
			using (var reader = new StreamReader(iStream))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					var values = line?.Split(',');

					elements.Add(values.ToList<string>());
				}
			}


			return ConvertCsvToObjects(elements);
		});

	}

	private Activity CreateActivityFromList(List<string> activityItems)
		=> new Activity(Int32.Parse(activityItems[3]), activityItems[2]);
	private Task CreateTaskFromList(List<string> taskItems)
		=> new Task(taskItems[1]);

	private Semaphore CreateSemaphoreFromList(List<string> semaphoreItems)
		=> new Semaphore(Int32.Parse(semaphoreItems[2]), semaphoreItems[1]);

	private Mutex CreateMutexFromList(List<string> mutexItems)
		=> new Mutex(mutexItems[1]);


}