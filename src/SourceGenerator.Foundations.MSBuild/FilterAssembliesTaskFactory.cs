using Microsoft.Build.Framework;
using SourceGenerator.Foundations.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGF
{
    public class FilterAssembliesTaskFactory : ITaskFactory
    {
        public string FactoryName { get; } = nameof(FilterAssembliesTaskFactory);

        public Type TaskType => typeof(FilterAssembliesTask);

        public void CleanupTask(ITask task)
        {
            if(task is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public ITask CreateTask(IBuildEngine taskFactoryLoggingHost)
        {
            return (ITask)Activator.CreateInstance(TaskType)!;

        }

        public TaskPropertyInfo[] GetTaskParameters()
        {
            return TaskType
            .GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(OutputAttribute), true).Any()
                     || p.CanWrite)
            .Select(p => new TaskPropertyInfo(
                p.Name,
                p.PropertyType,
                p.CanWrite,
                p.GetCustomAttributes(typeof(OutputAttribute), true).Any()))
            .ToArray();
        }

        public bool Initialize(string taskName, IDictionary<string, TaskPropertyInfo> parameterGroup, string taskBody, IBuildEngine taskFactoryLoggingHost)
        {
            return true;
        }
    }
}
