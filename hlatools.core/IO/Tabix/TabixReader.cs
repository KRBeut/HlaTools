using hlatools.core.DataObjects;
using hlatools.core.IO.VCF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.Tabix
{
    public abstract class TabixReader<T> : AmgDataObject where T : ICoverRegion
    {

        protected TabixIndexFile Index { get; set; }

        public TabixReader(TabixIndexFile index) 
            : base()
        {
            Index = index;
        }

        public IEnumerable<T> GetRecords(ICoverRegion reg)
        {
            var mtchingRecs = GetRecords(reg.Rname, reg.Pos, reg.Pos + reg.Length);
            foreach (var rec in mtchingRecs)
            {
                yield return rec;
            }
        }

        public IEnumerable<T> GetRecords(TabixIndex index, ICoverRegion reg)
        {
            var mtchingRecs = GetRecords(index, reg.Rname, reg.Pos, reg.Pos + reg.Length);
            foreach (var rec in mtchingRecs)
            {
                yield return rec;
            }
        }

        public IEnumerable<T> GetRecords(string rName, int regStart = 0, int regEnd = (1 << 29))
        {
            TabixIndex rNameIndex = GetIndex(rName);
            if (rNameIndex == null)
            {
                //the request is for a rName that is not in the index
                yield break;
            }

            foreach (var rec in GetRecords(rNameIndex, rName ,regStart,regEnd))
            {
                yield return rec;
            }
            
        }

        public IEnumerable<T> GetRecords(TabixIndex index, string rName, int regStart = 0, int regEnd = (1 << 29))
        {
            //find the bin(s) that contain the requested region
            var rets = Query(index, regStart, regEnd);

            //iterate through the bins and return the records that
            //match the querry criteria
            //foreach (var ret in rets.Take(Math.Max(1, rets.Length - 1)))
            //{
                Seek((long)rets[0].cnk_beg);
                foreach (var rec in GetMatchingRecord(rName, regStart, regEnd))
                {
                    yield return rec;
                }
            //}
        }

        protected virtual TabixIndex GetIndex(string rName)
        {
            TabixIndex rNameIndex;
            if (!Index.indices.TryGetValue(rName, out rNameIndex))
            {
                rNameIndex = null;
            }
            return rNameIndex;
        }

        private static int MAX_BIN = 37450;
        //private static int MAX_BIN = (((1<<18)-1)/7);

        private static int TAD_LIDX_SHIFT = 14;

        private static bool less64(long u, long v)
        { // unsigned 64-bit comparison
            return (u < v) ^ (u < 0) ^ (v < 0);
        }

        public static TabixChunk[] Query(TabixIndex idx, int beg, int end)
        {
            if (beg < 0)
            {
                beg = 0;
            }
            long min_off;
            TabixChunk[] off;
            List<TabixChunk> chunks;
            int[] bins = new int[MAX_BIN];
            int i, l, n_off, n_bins = TabixIndexFile.reg2bins(beg, end, bins);
            if (idx.intervals.Count > 0)
                min_off = (long)(((beg >> TAD_LIDX_SHIFT) >= idx.intervals.Count) ? idx.intervals[idx.intervals.Count - 1].ioff : idx.intervals[beg >> TAD_LIDX_SHIFT].ioff);
            else
            {
                min_off = 0;
            }
            for (i = n_off = 0; i < n_bins; ++i)
            {
                TabixBin bn;
                if (idx.bins.TryGetValue(bins[i], out bn))
                {
                    chunks = bn.chunks;
                    n_off += chunks.Count();
                }
            }
            if (n_off == 0)
            {
                return new TabixChunk[0];
                //return EOF_ITERATOR;
            }
            off = new TabixChunk[n_off];
            for (i = n_off = 0; i < n_bins; ++i)
            {
                TabixBin bn;
                if (idx.bins.TryGetValue(bins[i], out bn))
                {
                    chunks = bn.chunks;
                    for (int j = 0; j < chunks.Count; ++j)
                    {
                        if (less64(min_off, (long)chunks[j].cnk_end))
                        {
                            off[n_off++] = new TabixChunk(chunks[j].cnk_beg, chunks[j].cnk_end);
                        }
                    }
                }
            }
            off = off.Where(o => o != null).OrderBy(c => c, TabixChunkComparer.Instance).ToArray();
            //n_off = off.Count();

            // resolve completely contained adjacent blocks
            for (i = 1, l = 0; i < n_off; ++i)
            {
                if (less64((long)off[l].cnk_end, (long)off[i].cnk_end))
                {
                    ++l;
                    off[l].cnk_beg = off[i].cnk_beg;
                    off[l].cnk_end = off[i].cnk_end;
                }
            }
            n_off = l + 1;
            // resolve overlaps between adjacent blocks; this may happen due to the merge in indexing
            for (i = 1; i < n_off; ++i)
                if (!less64((long)off[i - 1].cnk_end, (long)off[i].cnk_beg))
                {
                    off[i - 1].cnk_end = off[i].cnk_beg;
                }
            // merge adjacent blocks
            for (i = 1, l = 0; i < n_off; ++i)
            {
                if (off[l].cnk_end >> 16 == off[i].cnk_beg >> 16)
                {
                    off[l].cnk_end = off[i].cnk_end;
                }
                else
                {
                    ++l;
                    off[l].cnk_beg = off[i].cnk_beg;
                    off[l].cnk_end = off[i].cnk_end;
                }
            }
            n_off = l + 1;
            // return
            TabixChunk[] ret = new TabixChunk[n_off];
            for (i = 0; i < n_off; ++i)
            {
                if (off[i] != null)
                {
                    ret[i] = new TabixChunk(off[i].cnk_beg, off[i].cnk_end); // in C, this is inefficient
                }
            }
            if (ret.Length == 0 || (ret.Length == 1 && ret[0] == null))
            {
                return new TabixChunk[0];
                //return EOF_ITERATOR
            }
            return ret;
        }
        
        protected abstract void Seek(long offset);

        protected abstract IEnumerable<T> GetMatchingRecord(string id, int start, int end);
        
    }
}
