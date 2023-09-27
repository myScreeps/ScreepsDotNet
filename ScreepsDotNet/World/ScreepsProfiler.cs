namespace ScreepsDotNet.World.profiler
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Linq;
    class ScreepsProfiler
    {
        private Stopwatch stopwatch = new Stopwatch();
        private List<(string, long)> methodTimings = new List<(string, long)>();
        
        public void Profile(string Name,Action method, bool enabled = true)
        {
            if (enabled)
            {
                stopwatch.Start();
                method();
                stopwatch.Stop();
                long elapsed = stopwatch.ElapsedMilliseconds;
              // MethodBase.GetCurrentMethod().Name
              //  string methodName = method.Method.Name;
                string methodName = Name;
                methodTimings.Add((methodName, elapsed));
                stopwatch.Reset();
            }
            else
            {
                // If profiling is disabled, just run the method without timing it
                method();
            }
        }

        public void LogElapsedTime(string sectionName, long elapsedMilliseconds, double percentageOfTotal)
        {
            var elapsed = elapsedMilliseconds.ToString("D2");
         //   var percent = percentageOfTotal.ToString("D2");
            //string formattedString = $"{sectionName,27}:{elapsedMilliseconds} ({percentageOfTotal}%)";
            string formattedString = $"{sectionName,27}: {elapsed} ({percentageOfTotal}%)";
            Console.WriteLine(formattedString);


          //  Console.WriteLine($"{sectionName} took {elapsedMilliseconds} ms ({percentageOfTotal}% of total).");
        }

        public void Reset()
        {
            methodTimings.Clear();
        }

        public void DisplayProfileResults()
        {

          //  Console.WriteLine("starting Screeps.Profiler.DisplayProfileResults()");
            // Calculate the total execution time
           // long totalElapsed = methodTimings.Sum(timing => timing.Item2);
           long totalElapsed = 0;
          //  Console.WriteLine($"methodTimings.Count: {methodTimings.Count}");

            foreach (var methodTiming in methodTimings)
            {
                //Console.WriteLine($"methodTiming.Item1 {methodTiming.Item1}");
                //Console.WriteLine($"methodTiming.Item2 {methodTiming.Item2}");

                totalElapsed = totalElapsed + methodTiming.Item2;
            }

           // Console.WriteLine("long totalElapsed = methodTimings.Sum(timing => timing.Item");

//            Console.WriteLine($"totalElapsed time: {totalElapsed}");
            //// Calculate and display the percentage of total runtime for each method
            foreach (var (methodName, elapsed) in methodTimings)
            {
        
                double percentage = (double)elapsed / totalElapsed * 100;

                if (elapsed == totalElapsed) 
                {
                    percentage = 100;
                }

                LogElapsedTime(methodName, elapsed, Math.Round(percentage,2));
            }

            Console.WriteLine($"totalElapsed: {totalElapsed}");
        }
    }

    class xProgram
    {
        static void MyMethod1()
        {
            // Code in your first method
            // Simulate some work with a delay
            System.Threading.Thread.Sleep(100);
        }

        static void MyMethod2()
        {
            // Code in your second method
            // Simulate some work with a delay
            System.Threading.Thread.Sleep(200);
        }

        static void xMain()
        {
            ScreepsProfiler profiler = new ScreepsProfiler();

            // Start profiling
           // profiler.Profile(() => MyMethod1(), enabled: true);
          /*  profiler.Profile(() => MyMethod2(), enabled: true)*/;

            // Display profile results
            profiler.DisplayProfileResults();
        }
    }
}
