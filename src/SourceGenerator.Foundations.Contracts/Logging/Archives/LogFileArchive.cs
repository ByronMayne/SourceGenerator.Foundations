using System.IO;
using System.Text;

namespace SGF.Logging.Archives
{
    /// <summary>
    /// An implemention of <see cref="ILogger"/> that logs to a file on disk
    /// 
    /// </summary>
    internal class LogFileArchive : ILogArchive
    {
        private readonly StreamWriter m_writer;

        public LogFileArchive(string archiveName)
        {
            string logDirectory = Path.GetTempPath();
            logDirectory = Path.Combine(logDirectory, "SourceGenerator.Foundations");
            if (!Directory.Exists(logDirectory))
            {
                _ = Directory.CreateDirectory(logDirectory);
            }
            string filePath = Path.Combine(logDirectory, $"{archiveName}.log");
            FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            m_writer = new StreamWriter(fileStream, Encoding.UTF8)
            {
                AutoFlush = true,
            };
        }

        /// <inheritdoc cref="ILogArchive"/>
        public void Write(string message)
        {
            m_writer.WriteLine(message);
        }
    }
}
