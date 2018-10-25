using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core
{
    public class StockholmReader
    {
        string Header;
        TextReader _txtRdr;

        public StockholmReader(TextReader txtRdr)
        {
            _txtRdr = txtRdr;
            ParseHeader();
        }

        public static StockholmReader FromFilepath(string filepath)
        {
            StockholmReader stoRdr;
            var strm = File.OpenRead(filepath);
            var strmRdr = new StreamReader(strm);            
            stoRdr = new StockholmReader(strmRdr);
            return stoRdr;
        }

        protected void ParseHeader()
        {
            Header = string.Empty;
            while (_txtRdr.Peek() == '#' || _txtRdr.Peek() == '\r' || _txtRdr.Peek() == '\n')
            {
                var fileLine = _txtRdr.ReadLine();
                Header += fileLine;
            }
        }

        public Dictionary<string, string> GetRecords()
        {
            string fileLine;
            var seqs = new Dictionary<string, string>();
            while ((fileLine = _txtRdr.ReadLine()) != null)
            {
                if (fileLine.StartsWith("//"))
                {
                    break;
                }

                fileLine = fileLine.Trim();
                if (fileLine.StartsWith("#") || fileLine.Length == 0)
                {
                    continue;
                }

                var toks = fileLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (toks.Length < 2 || toks[1].Length < 1)
                {
                    continue;
                }
                string seqId = toks[0];
                string seq = toks[1];
                if (seqs.ContainsKey(seqId))
                {
                    seqs[seqId] += seq;
                }
                else
                {
                    seqs.Add(seqId, seq);
                }
            }
            return seqs;
        }

    }
}
