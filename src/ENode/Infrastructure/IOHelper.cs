﻿using System;
using System.Threading;
using ECommon.Logging;
using ENode.Configurations;

namespace ENode.Infrastructure
{
    public class IOHelper
    {
        private readonly ILogger _logger;
        private readonly int _immediatelyRetryTimes;
        private readonly int _retryIntervalForIOException;

        public IOHelper(ILoggerFactory loggerFactory)
        {
            _immediatelyRetryTimes = ENodeConfiguration.Instance.Setting.ImmediatelyRetryTimes;
            _retryIntervalForIOException = ENodeConfiguration.Instance.Setting.RetryIntervalForIOException;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public bool TryIOActionRecursively(string actionName, string contextInfo, Func<bool> action, Action<string, Exception> failedAction)
        {
            return TryIOActionRecursively(actionName, contextInfo, (x, y, z) => action(), failedAction, 0);
        }

        public void TryIOAction(Action action, string actionName)
        {
            try
            {
                action();
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new IOException(string.Format("{0} failed.", actionName), ex);
            }
        }
        public T TryIOFunc<T>(Func<T> func, string funcName)
        {
            try
            {
                return func();
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new IOException(string.Format("{0} failed.", funcName), ex);
            }
        }

        private bool TryIOActionRecursively(string actionName, string contextInfo, Func<string, string, long, bool> action, Action<string, Exception> failedAction, long retryTimes)
        {
            try
            {
                return action(actionName, contextInfo, retryTimes);
            }
            catch (IOException ex)
            {
                _logger.Error(string.Format("IOException raised when executing action '{0}', current retryTimes:{1}, contextInfo:{2}", actionName, retryTimes, contextInfo), ex);
                if (retryTimes > _immediatelyRetryTimes)
                {
                    Thread.Sleep(_retryIntervalForIOException);
                }
                return TryIOActionRecursively(actionName, contextInfo, action, failedAction, retryTimes++);
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unknown exception raised when executing action '{0}', current retryTimes:{1}, contextInfo:{2}", actionName, retryTimes, contextInfo);
                _logger.Error(errorMessage, ex);
                if (failedAction != null)
                {
                    try
                    {
                        failedAction(errorMessage, ex);
                    }
                    catch (Exception e)
                    {
                        _logger.Error("Execute failedAction has exception.", e);
                    }
                }
                return false;
            }
        }
    }
}
