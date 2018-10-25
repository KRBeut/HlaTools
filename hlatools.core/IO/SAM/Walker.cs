using hlatools.core.DataObjects;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{

    public class Walker<T> : IDisposable where T : ICoverRegion
    {

        T _lastRead;
        IEnumerator<T> _objSource;
        Action<T> _objSink;
        Pileup<T> _currentPileup;
        public StringComparer RNameComparer { get; set; }

        public Walker(IEnumerator<T> objSource, Action<T> objSink)
        {
            _objSink = objSink;
            _objSource = objSource;
            _lastRead = default(T);
            _currentPileup = new Pileup<T>();
            RNameComparer = StringComparer.InvariantCulture;
        }

        protected virtual int ComparePositions(int xs, int xe, int ys, int ye)
        {
            if (xe < ys)
            {
                return -1;
            }
            else if (xs > ye)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Skip over the reads that come between the current position, and 
        /// the position of the reads we have been asked to return.
        /// </summary>
        /// <param name="rName"></param>
        /// <param name="posStart"></param>
        /// <returns></returns>
        protected T FastForward(string rName, int posStart)
        {
            var nReads = 0;
            T read = default(T);
            while (_objSource.MoveNext())
            {
                nReads++;
                if (_objSource.Current.Rname == "chr2")
                {
                    var myRead = _objSource.Current;
                }

                var rNameCmp = RNameComparer.Compare(_objSource.Current.Rname, rName);
                var posCmp = ComparePositions(_objSource.Current.Pos, _objSource.Current.Pos + _objSource.Current.Length, posStart, posStart);
                
                if (rNameCmp == 0 && posCmp >= 0)
                {
                    read = _objSource.Current;
                    break;
                }
                else if(rNameCmp > 0)
                {
                    break;
                }
            }
            return read;
        }
        
        public IEnumerable<T> GetNextPileup(string rName, int posStart = int.MinValue, int posEnd = int.MaxValue)
        {
            if (_lastRead != null)
            {
                _currentPileup.Add(_lastRead);
            }
            var nextPileup = new Pileup<T>() { Rname = rName, Pos = posStart, Length = posEnd - posStart + 1 };
            foreach (var rd in _currentPileup)
            {
                _lastRead = rd;
                if (ReadIntersectsPos(_lastRead, rName, posStart, posEnd))
                {
                    nextPileup.Add(_lastRead);
                }
                else
                {
                    _objSink(_lastRead);
                }
            }

            var read = FastForward(rName, posStart);
            _lastRead = read;
            while (read != null && ReadIntersectsPos(read, rName,posStart,posEnd))
            {
                if (FilterFromReturn(read, rName, posStart, posEnd))
                {
                    _objSink(read);
                }
                else
                {
                    _lastRead = read;
                    nextPileup.Add(read);
                }
                
                if (!_objSource.MoveNext())
                {
                    break;
                }
                read = _objSource.Current;
            }
            _currentPileup = nextPileup;
            return nextPileup;
        }

        protected virtual bool FilterFromReturn(T rd, string rName, int posStart, int posEnd)
        {
            return false;
        }

        protected virtual bool ReadIntersectsPos(T rd, string rName, int posStart, int posEnd)
        {
            return RNameComparer.Compare(rd.Rname,rName) == 0 && ComparePositions(rd.Pos,rd.Pos+rd.Length,posStart,posEnd) == 0;
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
                foreach (var read in _currentPileup)
                {
                    _objSink(read);
                }
                _objSink = null;
                _objSource = null;
                _lastRead = default(T);
                _currentPileup.Clear();
                _currentPileup = null;
                RNameComparer = null;
            }
            disposed = true;
        }

    }
}
