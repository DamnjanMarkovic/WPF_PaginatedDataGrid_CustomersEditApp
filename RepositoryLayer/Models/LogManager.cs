using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Models
{
    public static class LogManager
    {
        static readonly ILog NullLogInstance = new NullLog();

        public static Func<Type, ILog> GetLog = type => NullLogInstance;

        private class NullLog : ILog
        {
            public void Info(string format, params object[] args) { }
            public void Warn(string format, params object[] args) { }
            public void Error(Exception exception) { }
        }
    }
}
