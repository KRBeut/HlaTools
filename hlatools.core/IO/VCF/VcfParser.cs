using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using hlatools.core.Utils;
using System.Diagnostics;

namespace hlatools.core.IO.VCF
{
    public class VcfParser : HeaderedRecordFileParser<Variant, VcfHeader>
    {

        public static IEnumerable<Variant> GetRecords(string filepath, Func<Variant> varFactory = null)
        {
            using (var vcfPrsr = FromFilepath(filepath,varFactory))
            {
                foreach (var v in vcfPrsr.GetRecords())
                {
                    yield return v;
                }
            }
        }

        public static VcfParser FromFilepath(string filepath, Func<Variant> varFactory = null)
        {
            if (varFactory == null)
            {
                varFactory = () => new Variant();
            }

            System.IO.Stream strm = File.OpenRead(filepath);
            if (filepath.EndsWith(".gz"))
            {
                strm = new BgzfReader(strm);
            }
            var txtRdr = new StreamReader(strm);
            var vcfPrsr = new VcfParser(txtRdr, varFactory);
            return vcfPrsr;
        }

        public VcfParser(TextReader txtRdr, Func<Variant> recFactory) 
            : base(txtRdr, recFactory)
        {
            
        }
        
        protected override Variant ParseFromLineTokens(string[] lineTokens)
        {
            var vcf = recFactory();
            vcf.Rname = lineTokens[0];
            vcf.Pos = int.Parse(lineTokens[1]);
            vcf.Id = lineTokens[2];

            vcf.Haplotypes.Clear();
            ParseAlleles(vcf.Haplotypes,lineTokens[3], lineTokens[4].Split(','));

            int qualVal;
            vcf.Qual = int.MinValue;
            if (int.TryParse(lineTokens[5],out qualVal))
            {
                vcf.Qual = qualVal;
            }
            vcf.Filter = lineTokens[6];

            vcf.Info.Clear();
            ParseVarInfo(lineTokens[7].Split(';'), vcf.Info);

            try
            {
                var toks = lineTokens[8].Split(':');
                if (toks == null || toks.Length < 1)
                {

                }
                vcf.Format = toks.Where(f=>Header.Format.ContainsKey(f))
                                 .Select(f => Header.Format[f]).ToArray();
                if (vcf.Format.Count() != toks.Length)
                {
                    var diffs = toks.Where(f => !Header.Format.ContainsKey(f)).ToList();
                }
            }
            catch (Exception)
            {
                throw;
                //return vcf;
            }
            

            vcf.SampleData.Clear();
            var gtTokens = lineTokens.Skip(9);
            ParseGenotypes(vcf.SampleData, vcf.Haplotypes, vcf.Format, gtTokens);
            return vcf;
        }

        protected void ParseAlleles(IList<VcfHaplotype> haps, string refAllele, IEnumerable<string> altAlleles)
        {
            var refHap = new VcfHaplotype(refAllele) { Type = VcfAlleleType.Reference };
            haps.Add(refHap);
            foreach (var allele in altAlleles)
            {
                VcfHeaderStructuralvar alt;
                if (Header.AltAlleles.TryGetValue(allele, out alt))
                {
                    var structHap =  new VcfHaplotype(string.Empty)
                    {
                        Type = VcfAlleleType.Structural,
                        StructVar = alt
                    };
                    haps.Add(structHap);
                }
                else
                {
                    var altHap = new VcfHaplotype(allele) { Type = VcfAlleleType.Alternative };
                    haps.Add(altHap);
                }
            }
        }

        protected void ParseVarInfo(string[] tokens, VariantInfo varInfo)
        {            
            foreach (var tok in tokens)
            {
                string key = null;
                string value = null;
                var kvp = tok.Split('=');
                if (kvp.Length < 2)
                {
                    key = tok;
                    value = true.ToString();
                }
                else
                {
                    key = kvp[0];
                    value = string.Join("=",kvp.Skip(1));
                }
                varInfo.Add(key, value);
            }
        }

        protected void ParseGenotypes(Dictionary<string,VcfGenotype> dict, IList<VcfHaplotype> haps, IEnumerable<VcfHeaderFormat> frmtToks, IEnumerable<string> genotypes)
        {
            int indx = 0;
            foreach (var smplGenotype in genotypes)
            {
                var smplId = Header.SampleByIndex[indx];
                var data = smplGenotype.Split(':');
                if (data.Length != frmtToks.Count())
                {
                    continue;
                }
                var genotype = ParseGenotype(haps, frmtToks, data);
                Dict.Upsert(dict, smplId, genotype);
                indx++;
            }
        }
        
        readonly char[] genotypeDelim = new char[]{'/','|','\\'};

        protected VcfGenotype ParseGenotype(IList<VcfHaplotype> haps, IEnumerable<VcfHeaderFormat> format, IList<string> tokens)
        {
            int indx = 0;
            var dict = new VcfGenotype(tokens.Count());
            foreach (var frmt in format)
            {
                switch (frmt.Id)
                {
                    case "GT":
                        foreach (var genotype in tokens[indx].Split(genotypeDelim))
                        {
                            int genoIndx;
                            if (genotype == ".")
                            {
                                //no genotype call was made
                                dict.Genotype.Clear();
                                break;
                            }
                            else if (int.TryParse(genotype, out genoIndx))
                            {
                                dict.Genotype.Add(haps[genoIndx]);
                            }
                        }
                        dict.IsPhased = dict.IsPhased || tokens[indx].Contains('|');
                        break;
                    default:
                        Dict.Upsert(dict, frmt.Id, tokens[indx]);
                        break;
                }
                indx++;
            }
            return dict;
        }
        
        protected override VcfHeader ParseHeader(TextReader txtRdr)
        {
            string fileLine;
            var hdr = new VcfHeader();            
            while ((fileLine = txtRdr.ReadLine()) != null)
            {
                if (fileLine.StartsWith("##"))
                {
                    ParseHeaderLine(hdr, fileLine.Substring(2));
                }
                else
                {
                    var tokens = fileLine.Substring(1).Split('\t');
                    hdr.SampleByIndex = ParseColumnHeaders(tokens);
                    break;
                }
            }
            return hdr;
        }

        protected void ParseHeaderLine(VcfHeader header, string fileLine)
        {
            if (fileLine.StartsWith("INDIVIDUAL"))
            {

            }
            IEnumerable<string> values = null;
            string tag = null;
            var mtches = hdrInfoRegEx.Match(fileLine);
            if (!mtches.Success)
            {
                var kvp = fileLine.Split('=');
                Dict.Upsert(header.MetaInfo, kvp[0], string.Join("=", kvp.Skip(1)));
            }

            tag = mtches.Groups["tag"].Value;
            values = mtches.Groups["val"].Value.Split(',');
            switch (tag)
            {
                case "INDIVIDUAL":
                    var indv = new VcfHeaderIndividual(Dict.FromTokens(values, '='));
                    Dict.Upsert(header.Individual, indv.Id, indv);
                    break;
                case "SAMPLE":
                    var smpl = new VcfHeaderSample(Dict.FromTokens(values, '='));
                    Dict.Upsert(header.Sample, smpl.Id, smpl);
                    break;
                case "gdcWorkflow":
                    var wrkFlw = new VcfHeaderWorkflow(Dict.FromTokens(values, '='));
                    Dict.Upsert(header.WorkFlow, wrkFlw.Id, wrkFlw);
                    break;
                case "INFO":
                    var info = new VcfHeaderInfo(Dict.FromTokens(values,'='));
                    Dict.Upsert(header.Info, info.Id, info);
                    break;
                case "FILTER":
                    var fltr = new VcfHeaderFilter(Dict.FromTokens(values, '='));
                    Dict.Upsert(header.Filter, fltr.Id, fltr);
                    break;
                case "FORMAT":
                    var frmt = new VcfHeaderFormat(Dict.FromTokens(values, '='));
                    Dict.Upsert(header.Format, frmt.Id, frmt);
                    break;
                case "ALT":
                    var alt = new VcfHeaderStructuralvar(Dict.FromTokens(values, '='));
                    Dict.Upsert(header.AltAlleles, string.Format("<{0}>",alt.Id), alt);
                    break;
                case "contig":
                    var cntg = new VcfHeaderContig(Dict.FromTokens(values, '='));
                    Dict.Upsert(header.Contigs, cntg.Id, cntg);
                    break;
                default:
                    break;
            }

        }

        readonly Regex hdrInfoRegEx = new Regex("(?<tag>[A-Za-z0-9]+)=<(?<val>.*)>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        
        protected Dictionary<int,string> ParseColumnHeaders(IEnumerable<string> colTokens)
        {
            int index = 0;
            var dict = new Dictionary<int, string>();
            foreach (var smplId in colTokens.Skip(9))
            {
                dict.Add(index, smplId);
                index++;
            }
            return dict;
        }
        
    }
}
