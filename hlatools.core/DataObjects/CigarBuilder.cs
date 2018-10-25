using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class CigarBuilder
    {

        public bool UseXAndEquals { get; set; }
        
        public CigarBuilder(bool useXAndEquals = true)
        {
            UseXAndEquals = useXAndEquals;
            cigToks = new Queue<CigTok>();
            Clear();
        }

        Queue<CigTok> cigToks;

        protected int currentCnt;

        protected char currentState;

        const char emptyState = '\0';

        public void Clear()
        {
            currentCnt = 0;
            _currentCig = null;
            currentState = emptyState;
            cigToks.Clear();
        }

        public void Append(char state, int length = 1)
        {
            if (length < 1)
            {
                return;
            }
            if (currentState != emptyState && state != currentState)
            {
                cigToks.Enqueue(new CigTok(currentState.ToString(), currentCnt));
                currentCnt = 0;
            }
            currentState = state;
            currentCnt += length;
        }

        Cigar _currentCig = null;

        public Cigar GetCigar()
        {
            if (_currentCig == null)
            {
                cigToks.Enqueue(new CigTok(currentState.ToString(), currentCnt));
                _currentCig = new Cigar(cigToks);
            }
            return _currentCig;
        }

    }
}
