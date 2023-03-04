namespace BlazorApp.Data;

public class CsvFileReaderService {

    public List<Task> ReadCsvFileToObject(string csvFilePath){

        List<List<string>> elements = new();
        using(var reader = new StreamReader(csvFilePath)){
            while(!reader.EndOfStream){
                var line = reader.ReadLine();
                var values = line?.Split(',');

                elements.Add(values.ToList<string>());
            }
        }

        List<Task> tasks = new();
        List<Activity> activities = new();
        List<Semaphore> semaphores = new();

        elements.FindAll(element => element.First() == "Task").ForEach(taskList => tasks.Add(CreateTaskFromList(taskList)));
        elements.FindAll(element => element.First() == "Activity").ForEach(activityList => {
            var activity =  CreateActivityFromList(activityList);
            tasks.Find(task => task.Name == activityList[1])?.AddActivity(activity);
            activities.Add(activity);
        });
        elements.FindAll(element => element.First() == "Semaphore").ForEach(semaphoreList => {
            var existingSemaphore = semaphores.Find(semaphore => semaphore.Name == semaphoreList[1]);
            if(existingSemaphore is null)
            {
                var semaphore = CreateSemaphoreFromList(semaphoreList);
                var inputActivity = activities.Find(activity => activity.Name == semaphoreList[3]);
                inputActivity?.AddInput(semaphore);
                var outputActivity = activities.Find(activitiy => activitiy.Name == semaphoreList[4]);
                outputActivity?.AddOutput(semaphore);

                if(inputActivity != null && outputActivity != null){
                    if(tasks.Find(task => task.Activities.Contains(inputActivity) && task.Activities.Contains(outputActivity)) != null) semaphore.SetActivitySemaphore();
                }

                semaphores.Add(semaphore);

            } else {
                existingSemaphore.IncrementNumberInputs();
            }

        });

        elements.FindAll(element => element.First() == "Mutex").ForEach(mutexList => {
            var mutex = CreateMutexFromList(mutexList);

            for(int i = 2; i < mutexList.Count; i++){
                var activity = activities.Find(activitiy => activitiy.Name == mutexList[i]);
                activity?.AddInput(mutex);
                activity?.AddOutput(mutex);
            }
        });
        
        return tasks;
        
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