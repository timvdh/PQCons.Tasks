namespace PQCons.Tasks.Samples.Data;

internal record Course(string Id)
{
    public List<Student> Students { get; set; } = [];
    public int Seats { get; private set; } = 3;

    private readonly Random _randomDelay = new();

    public async Task AssignStudent(Student student)
    {
        // this is the critical section that LockByKey must prevent
        if (Seats > 0)
        {
            // some work, e.g. a webservice call reserving a seat
            await Task.Delay(_randomDelay.Next(1, 20));
            Seats--;
            Students.Add(student);
        }
    }
}
