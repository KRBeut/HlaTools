using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.Utils
{
    public class ObjPool<T> : IDisposable
    {

        Func<T> _objFact;
        ConcurrentBag<T> _objs;

        public ObjPool(Func<T> objFact )
        {
            _objFact = objFact;
            _objs = new ConcurrentBag<T>();
        }

        public void SetSize(int instCnt)
        {
            var instDif = _objs.Count - instCnt;
            if (instDif > 0)
            {
                _objs.Take(instDif);
            }
            else if (instDif < 0)
            {
                for (int k = 0; k < Math.Abs(instDif); k++)
                {
                    _objs.Add(_objFact());
                }
            }
        } 

        public int Count
        {
            get
            {
                return _objs.Count;
            }
        }

        public virtual T Pop()
        {
            T obj;
            if (!_objs.TryTake(out obj))
            {
                obj = _objFact();
            }
            return obj;
        }

        public virtual void Push(T obj)
        {
            _objs.Add(obj);
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Flag: Has Dispose already been called?
        bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                _objs = null;
                _objFact = null;
            }
            disposed = true;
        }

    }
}
