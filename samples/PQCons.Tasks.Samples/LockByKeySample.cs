using PQCons.Tasks.Samples.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PQCons.Tasks.Samples;

public class LockByKeySample(bool useLocking)
{
    private readonly LockByKey<string> _locks = new();
    private readonly List<Course> _courses = [];
    private readonly bool _useLocking = useLocking;
    public Stopwatch Stopwatch { get; set; } = new Stopwatch();

    public async Task TestParallel()
    {
        var allTasks = new List<Task>();

        var coursesPrefix = Enumerable.Range('a', 'z' - 'a' + 1).Select(c => (char)c).ToList();

        Stopwatch.Start();

        for (int i = 0; i < coursesPrefix.Count; i++)
        {
            for (int k = 0; k < 200; k++)
            {
                var course = new Course($"{coursesPrefix[i]}{k}");
                _courses.Add(course);
                var tasks = AddFourStudentTasks(course, 1);
                allTasks.AddRange(tasks);
            }
        }

        await Task.WhenAll(allTasks)
            .ContinueWith((x) =>
            {
                Stopwatch.Stop();
            });

        Console.WriteLine($"Tasks total: {allTasks.Count}");
        Console.WriteLine($"Courses total: {_courses.Count}");
        Console.WriteLine($"Remaining locks: {_locks.LockCount}");

        Console.WriteLine($"{_courses.Where(x => x.Seats < 0).Count()} courses overbooked.");
        Console.WriteLine($"{_courses.Where(x => x.Seats > 0).Count()} courses not fully booked.");
        Console.WriteLine($"{_courses.Where(x => x.Seats == 0).Count()} courses booked as intended.");
    }
    private Task[] AddFourStudentTasks(Course course, int firstPosition)
    {
        if (_useLocking)
        {
            return
            [
                AddStudentWithLockAsync(new Student("John", "Doe", firstPosition), course),
                AddStudentWithLockAsync(new Student("Jane", "Smith", firstPosition + 1), course),
                AddStudentWithLockAsync(new Student("Michael", "Johnson", firstPosition + 2), course),
                AddStudentWithLockAsync(new Student("Emily", "Brown", firstPosition + 3), course),
            ];
        }
        return
        [
            AddStudentWithoutLockAsync(new Student("John", "Doe", firstPosition), course),
            AddStudentWithoutLockAsync(new Student("Jane", "Smith", firstPosition + 1), course),
            AddStudentWithoutLockAsync(new Student("Michael", "Johnson", firstPosition + 2), course),
            AddStudentWithoutLockAsync(new Student("Emily", "Brown", firstPosition + 3), course),
        ];
    }

    private async Task AddStudentWithLockAsync(Student student, Course course)
    {
        using (await _locks.AcquireLockAsync(course.Id))
        {
            await course.AssignStudent(student);
        }
    }

    private async Task AddStudentWithoutLockAsync(Student student, Course course)
    {
        await course.AssignStudent(student);
    }
}