using hlatools.core.DataObjects;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace hlatools.core.IO.GFF
{
    public class Gff3Parser<T> : HeaderedRecordFileParser<T,List<string>> where T : Gff3Feature
    {

        public static Gff3Parser<T> FromFilepath(string filepath, Func<T> recFactory)
        {
            System.IO.Stream strm = File.OpenRead(filepath);
            if (filepath.EndsWith(".gz"))
            {
                strm = new GZipStream(strm, CompressionMode.Decompress);
            }

            var txtRdr = new StreamReader(strm);
            return new Gff3Parser<T>(txtRdr, recFactory);
        }

        public Gff3Parser(TextReader txtRdr, Func<T> recFactory) 
            : base(txtRdr, recFactory)
        {

        }

        public override bool SkipLine(string fileLine)
        {
            return fileLine.StartsWith("##");
        }

        protected override T ParseFromLineTokens(string[] lineTokens)
        {
            var gff = recFactory();

            gff.Rname = lineTokens[0];
            gff.Source = lineTokens[1];
            gff.Feature = lineTokens[2];
            gff.Pos = int.Parse(lineTokens[3]);
            gff.End = int.Parse(lineTokens[4]);
            gff.Score = lineTokens[5] == "." ? int.MinValue : double.Parse(lineTokens[5]);

            switch (lineTokens[6])
            {
                case "+":
                    gff.Strand = DnaStrand.Forward;
                    break;
                case "-":
                    gff.Strand = DnaStrand.Reverse;
                    break;
                default:
                    gff.Strand = DnaStrand.Neither;
                    break;
            }

            switch (lineTokens[7])
            {
                case "0":
                    gff.Frame = ReadingFrame.Zero;
                    break;
                case "1":
                    gff.Frame = ReadingFrame.One;
                    break;
                case "2":
                    gff.Frame = ReadingFrame.Two;
                    break;
                default:
                    gff.Frame = ReadingFrame.None;
                    break;
            }

            gff.Attributes.Clear();
            if (lineTokens.Length >= 9)
            {
                ParseAttributes(lineTokens[8], gff.Attributes);
            }
            return gff;
        }

        private readonly Regex attribRegEx = new Regex("(?<key>[^=]+)=(?<val>[^;]+);*", RegexOptions.Compiled);
        protected virtual Regex AttribRegEx
        {
            get
            {
                return attribRegEx;
            }
        }

        protected virtual void ParseAttributes(string str, Dictionary<string,string> attributes)
        {
            var mtchs = AttribRegEx.Matches(str);
            for (int k = 0; k < mtchs.Count; k++)
            {
                var mtch = mtchs[k];
                var key = mtch.Groups["key"].Value;
                var val = mtch.Groups["val"].Value;
                if (key == "tag")
                {
                    Dict.Appendsert(attributes, key, val);
                }
                else
                {
                    Dict.Upsert(attributes, key, val);
                }
                
            }
        }

        protected override List<string> ParseHeader(TextReader txtRdr)
        {
            var headerStrs = new List<string>();
            while (txtRdr.Peek() == '#')
            {
                var hdrLine = txtRdr.ReadLine().Substring(2);
                headerStrs.Add(hdrLine);
            }
            return headerStrs;
        }
    }
}
