using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using hlatools.core.IO;
using hlatools.core.DataObjects;
using System.Text.RegularExpressions;
using hlatools.core.Utils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace hlatools.core.IO.SAM
{

    public class SamParserCore : SamParserCore<SamSeq, SamHeader>
    {
        public SamParserCore(TextReader txtRdr, Func<SamSeq> recFactory) 
            : base(txtRdr, recFactory)
        {
        }
    }

    //public class SamParser<S,H> : HeaderedRecordFileParser<SamSeq, SamHeader>
    public class SamParserCore<S, H> : HeaderedRecordFileParser<S, H>, IDisposable where S : SamSeq where H : SamHeader, new()
    {

        protected static TextReader StringToTxtRdr(string filepath)
        {
            TextReader samFileRdr = null;
            if (filepath.EndsWith(".bam") || filepath.EndsWith(".cram"))
            {
                var samtoolsCmd = @"C:\wrk\bin\samtools.exe";
                var arguments = string.Format("view -h \"{0}\"", filepath);
                Process proc = new Process()
                {
                    StartInfo = new ProcessStartInfo(samtoolsCmd, arguments)
                    {
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                    }
                };
                proc.Start();
                samFileRdr = proc.StandardOutput;
            }
            else
            {
                samFileRdr = File.OpenText(filepath);
            }
            return samFileRdr;
        }

        public static SamParserCore<S,H> FromFilepath(string filepath, Func<S> fact)
        {
            var samFileRdr = StringToTxtRdr(filepath);
            var prsr = new SamParserCore<S, H>(samFileRdr, fact);
            return prsr;
        }

        public SamParserCore(TextReader txtRdr, Func<S> recFactory) 
            : base(txtRdr, recFactory)
        {
        }

        protected override S ParseFromLineTokens(string[] lineTokens)
        {
            var samSeq = this.recFactory();
            samSeq.Qname = lineTokens[0];
            samSeq.Flag = ParseSamFlag(lineTokens[1]);
            samSeq.Rname = lineTokens[2];
            samSeq.Pos = int.Parse(lineTokens[3]);
            samSeq.Mapq = int.Parse(lineTokens[4]);
            samSeq.Cigar = Cigar.FromString(lineTokens[5]);
            samSeq.Rnext = lineTokens[6];
            samSeq.Pnext = int.Parse(lineTokens[7]);
            samSeq.Length = int.Parse(lineTokens[8]);
            samSeq.Seq = lineTokens[9];
            samSeq.Qual = lineTokens[10];
            samSeq.Opts = samSeq.Opts ?? new Dictionary<string, SamSeqOpt>();
            PopulateMetadata(samSeq.Opts,lineTokens.Skip(11));

            var cigLen = samSeq.Cigar.ComputeTLen();
            if (cigLen > samSeq.Length)
            {
                samSeq.Length = cigLen;
            }
            return samSeq;
        }

        protected virtual void PopulateMetadata(Dictionary<string, SamSeqOpt>  dict, IEnumerable<string> strs)
        {
            
        }

        protected virtual SamSeqOpt ParseMetadata(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            var toks = str.Split(':');

            if (toks.Count() < 3)
            {
                return null;
            }

            var tag = toks[0];
            var valType = toks[1];
            var val = toks[2];
                        
            if (valType == "B")
            {
                var valToks = val.Split(',');
                var elementType = valToks[0];
                var arrayLength = valToks.Length;                
                if (elementType == "c")
                {
                    var vals = new sbyte[arrayLength-1];
                    for (int k = 1; k < arrayLength; k++)
                    {
                        vals[k] = sbyte.Parse(valToks[k]);
                    }
                    var opt = new SamSeqSbyteArrayOpt(tag) { Value = vals };
                    return opt;
                }
                else if (elementType == "C")
                {                    
                    var vals = new byte[arrayLength-1];
                    for (int k = 1; k < arrayLength; k++)
                    {
                        vals[k] = byte.Parse(valToks[k]);
                    }
                    var opt = new SamSeqByteArrayOpt(tag) { Value = vals };
                    return opt;
                }
                else if (elementType == "s")
                {
                    var vals = new Int16[arrayLength-1];
                    for (int k = 1; k < arrayLength; k++)
                    {
                        vals[k] = Int16.Parse(valToks[k]);
                    }
                    var opt = new SamSeqInt16ArrayOpt(tag) { Value = vals };
                    return opt;
                }
                else if (elementType == "S")
                {
                    var vals = new UInt16[arrayLength-1];
                    for (int k = 1; k < arrayLength; k++)
                    {
                        vals[k] = UInt16.Parse(valToks[k]);

                    }
                    var opt = new SamSeqUInt16ArrayOpt(tag) { Value = vals };
                    return opt;
                }
                else if (elementType == "i")
                {
                    var vals = new Int32[arrayLength-1];
                    for (int k = 1; k < arrayLength; k++)
                    {
                        vals[k] = Int32.Parse(valToks[k]);
                    }
                    var opt = new SamSeqInt32ArrayOpt(tag) { Value = vals };
                    return opt;
                }
                else if (elementType == "I")
                {
                    var vals = new UInt32[arrayLength-1];
                    for (int k = 1; k < arrayLength; k++)
                    {
                        vals[k] = UInt32.Parse(valToks[k]);
                    }
                    var opt = new SamSeqUInt32ArrayOpt(tag) { Value = vals };
                    return opt;
                }
                else if (elementType == "f")
                {
                    var vals = new float[arrayLength-1];
                    for (int k = 1; k < arrayLength; k++)
                    {
                        vals[k] = float.Parse(valToks[k]);
                    }
                    var opt = new SamSeqFloatArrayOpt(tag) { Value = vals };
                    return opt;
                }
            }
            else
            {
                if (valType == "A")
                {
                    var opt = new SamSeqCharOpt(tag) { Value = char.Parse(val) };
                    return opt;
                }
                else if (valType == "c")
                {
                    var opt = new SamSeqSbyteOpt(tag) { Value = sbyte.Parse(val) };
                    return opt;
                }
                else if (valType == "C")
                {
                    var opt = new SamSeqByteOpt(tag) { Value = byte.Parse(val) };
                    return opt;
                }
                else if (valType == "s")
                {
                    var opt = new SamSeqInt16Opt(tag) { Value = Int16.Parse(val) };
                    return opt;
                }
                else if (valType == "S")
                {
                    var opt = new SamSeqUInt16Opt(tag) { Value = UInt16.Parse(val) };
                    return opt;
                }
                else if (valType == "i")
                {
                    var opt = new SamSeqInt32Opt(tag) { Value = Int32.Parse(val) };
                    return opt;
                }
                else if (valType == "I")
                {
                    var opt = new SamSeqUInt32Opt(tag) { Value = UInt32.Parse(val) };
                    return opt;
                }
                else if (valType == "f")
                {
                    var opt = new SamSeqFloatOpt(tag) { Value = float.Parse(val) };
                    return opt;
                }
                else if (valType == "Z")
                {
                    var opt = new SamSeqStringOpt(tag) { Value = val };
                    return opt;
                }
                else if (valType == "H")
                {
                    throw new NotImplementedException("Sorry, I do not know how to parse hex-formated byte arrays in bam file optional fileds. I guess now will be my chance to learn!?!");
                }
            }
            return null;
        }

        private SamFlag ParseSamFlag(string v)
        {
            var samFlag = (SamFlag)Enum.Parse(typeof(SamFlag), v);
            return samFlag;
        }

        protected override H ParseHeader(TextReader txtRdr)
        {
            var samHdr = new H();
            while (txtRdr.Peek() == '@')
            {
                var hdrLine = txtRdr.ReadLine();
                ParseHeaderLine(samHdr, hdrLine);
            }
            var intToRefSa = samHdr.SQ.Values.ToDictionary(sq => sq.SortIndex, sq => sq);
            samHdr.IntToRefSeq = intToRefSa;
            return samHdr;
        }

        static readonly char[] hdrLineDelims = new char[] { '@', '\t' };

        public static void ParseHeaderLine(H samHdr, string hdrLine)
        {
            var mtch = hdrLine.Split(hdrLineDelims, StringSplitOptions.RemoveEmptyEntries);
            var code = mtch[0];
            if (code == "CO")
            {
                samHdr.CO = samHdr.CO ?? new List<string>();
                samHdr.CO.Add(mtch[1]);
            }
            else
            {
                var myDict = ParseHdrLine(mtch.Skip(1));
                switch (code)
                {
                    case "HD":
                        samHdr.HD = samHdr.HD ?? new SamHeaderLine();
                        Dict.Upsert(samHdr.HD, myDict);
                        break;
                    case "SQ":
                        samHdr.SQ = samHdr.SQ ?? new Dictionary<string, SamRefSeq>();
                        var sq = FromDictionary<SamRefSeq>(myDict);
                        sq.SortIndex = samHdr.SQ.Count;
                        Dict.Upsert(samHdr.SQ, myDict["SN"], sq);
                        break;
                    case "RG":
                        samHdr.RG = samHdr.RG ?? new Dictionary<string, SamReadGroup>();
                        Dict.Upsert(samHdr.RG, myDict["ID"], FromDictionary<SamReadGroup>(myDict));
                        break;
                    case "PG":
                        samHdr.PG = samHdr.PG ?? new Dictionary<string, SamProgram>();
                        Dict.Upsert(samHdr.PG, myDict["ID"], FromDictionary<SamProgram>(myDict));
                        break;
                    default:
                        samHdr.Uppend(code, myDict);
                        break;
                }
            }
        }

        protected static R FromDictionary<R>(Dictionary<string,string> kvps) where R : Dictionary<string,string>, new()
        {
            var samRefSeq = new R();
            Dict.Upsert(samRefSeq,kvps);
            return samRefSeq;
        }

        static readonly char[] tvpDelims = new char[] { ':' };

        public static Dictionary<string, string> ParseHdrLine(IEnumerable<string> tvps)
        {
            var dict = new Dictionary<string, string>();
            foreach (var tvp in tvps)
            {
                var toks = tvp.Split(tvpDelims, StringSplitOptions.RemoveEmptyEntries);
                dict.Add(toks[0], string.Join(":",toks.Skip(1)));
            }
            return dict;
        }

    }

    
}
