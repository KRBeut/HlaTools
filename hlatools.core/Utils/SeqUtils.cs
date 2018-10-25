using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.Utils
{
    public class SeqUtils
    {

        public static Dictionary<string, char> codon2Aa = new Dictionary<string, char>()
        {
            /*"Isoleucine"   */{"att",'I'},{"atc",'I'},{"ata",'I'},{"auu",'I'},{"auc",'I'},{"aua",'I'},                                                                        {"ATT",'I'},{"ATC",'I'},{"ATA",'I'},{"AUU",'I'},{"AUC",'I'},{"AUA",'I'},                                                                        
            /*"Leucine"      */{"ctt",'L'},{"ctc",'L'},{"cta",'L'},{"ctg",'L'},{"tta",'L'},{"ttg",'L'},{"cuu",'L'},{"cuc",'L'},{"cua",'L'},{"cug",'L'},{"uua",'L'},{"uug",'L'},{"CTT",'L'},{"CTC",'L'},{"CTA",'L'},{"CTG",'L'},{"TTA",'L'},{"TTG",'L'},{"CUU",'L'},{"CUC",'L'},{"CUA",'L'},{"CUG",'L'},{"UUA",'L'},{"UUG",'L'}, 
            /*"Valine"       */{"gtt",'V'},{"gtc",'V'},{"gta",'V'},{"gtg",'V'},{"guu",'V'},{"guC",'V'},{"gua",'V'},{"gug",'V'},                                                {"GTT",'V'},{"GTC",'V'},{"GTA",'V'},{"GTG",'V'},{"GUU",'V'},{"GUC",'V'},{"GUA",'V'},{"GUG",'V'},                                                
            /*"Phenylalanine"*/{"ttt",'F'},{"ttc",'F'},{"uuu",'F'},{"uuc",'F'},                                                                                                {"TTT",'F'},{"TTC",'F'},{"UUU",'F'},{"UUC",'F'},                                                                                                
            /*"Methionine"   */{"atg",'M'},{"aug",'M'},                                                                                                                        {"ATG",'M'},{"AUG",'M'},                                                                                                                        
            /*"Cysteine"     */{"tgt",'C'},{"tgc",'C'},{"ugu",'C'},{"ugc",'C'},                                                                                                {"TGT",'C'},{"TGC",'C'},{"UGU",'C'},{"UGC",'C'},                                                                                                
            /*"Alanine"      */{"gct",'A'},{"gcc",'A'},{"gca",'A'},{"gcg",'A'},{"gcu",'A'},                                                                                    {"GCT",'A'},{"GCC",'A'},{"GCA",'A'},{"GCG",'A'},{"GCU",'A'},                                                                                    
            /*"Glycine"      */{"ggt",'G'},{"ggc",'G'},{"gga",'G'},{"ggg",'G'},{"ggu",'G'},                                                                                    {"GGT",'G'},{"GGC",'G'},{"GGA",'G'},{"GGG",'G'},{"GGU",'G'},                                                                                    
            /*"Proline"      */{"cct",'P'},{"ccc",'P'},{"cca",'P'},{"ccg",'P'},{"ccu",'P'},                                                                                    {"CCT",'P'},{"CCC",'P'},{"CCA",'P'},{"CCG",'P'},{"CCU",'P'},                                                                                    
            /*"Threonine"    */{"act",'T'},{"acc",'T'},{"aca",'T'},{"acg",'T'},{"acu",'T'},                                                                                    {"ACT",'T'},{"ACC",'T'},{"ACA",'T'},{"ACG",'T'},{"ACU",'T'},                                                                                    
            /*"Serine"       */{"tct",'S'},{"tcc",'S'},{"tca",'S'},{"tcg",'S'},{"agt",'S'},{"agC",'S'},{"ucu",'S'},{"ucc",'S'},{"uca",'S'},{"ucg",'S'},{"agu",'S'},            {"TCT",'S'},{"TCC",'S'},{"TCA",'S'},{"TCG",'S'},{"AGT",'S'},{"AGC",'S'},{"UCU",'S'},{"UCC",'S'},{"UCA",'S'},{"UCG",'S'},{"AGU",'S'},            
            /*"Tyrosine"     */{"tat",'Y'},{"tac",'Y'},{"uau",'Y'},{"uac",'Y'},                                                                                                {"TAT",'Y'},{"TAC",'Y'},{"UAU",'Y'},{"UAC",'Y'},                                                                                                
            /*"Tryptophan"   */{"tgg",'W'},{"ugg",'W'},                                                                                                                        {"TGG",'W'},{"UGG",'W'},                                                                                                                        
            /*"Glutamine"    */{"caa",'Q'},{"cag",'Q'},                                                                                                                        {"CAA",'Q'},{"CAG",'Q'},                                                                                                                        
            /*"Asparagine"   */{"aat",'N'},{"aac",'N'},{"aau",'N'},                                                                                                            {"AAT",'N'},{"AAC",'N'},{"AAU",'N'},                                                                                                            
            /*"Histidine"    */{"cat",'H'},{"cac",'H'},{"cau",'H'},                                                                                                            {"CAT",'H'},{"CAC",'H'},{"CAU",'H'},                                                                                                            
            /*"Glutamic acid"*/{"gaa",'E'},{"gag",'E'},                                                                                                                        {"GAA",'E'},{"GAG",'E'},                                                                                                                        
            /*"Aspartic acid"*/{"gat",'D'},{"gac",'D'},{"gau",'D'},                                                                                                            {"GAT",'D'},{"GAC",'D'},{"GAU",'D'},                                                                                                            
            /*"Lysine"       */{"aaa",'K'},{"aag",'K'},                                                                                                                        {"AAA",'K'},{"AAG",'K'},                                                                                                                        
            /*"Arginine"     */{"cgt",'R'},{"cgc",'R'},{"cga",'R'},{"cgg",'R'},{"aga",'R'},{"agg",'R'},{"cgu",'R'},                                                            {"CGT",'R'},{"CGC",'R'},{"CGA",'R'},{"CGG",'R'},{"AGA",'R'},{"AGG",'R'},{"CGU",'R'},                                                            
            /*"Stop codons"  */{"taa",'*'},{"tag",'*'},{"tga",'*'},{"uaa",'*'},{"uag",'*'},{"uga",'*'},                                                                        {"TAA",'*'},{"TAG",'*'},{"TGA",'*'},{"UAA",'*'},{"UAG",'*'},{"UGA",'*'}
        };

        public static IEnumerable<char> Transcribe(IEnumerable<char> nucSeq)
        {
            return nucSeq.Select(n => n == 'T' ? 'U' : n);
            //foreach (var n in nucSeq)
            //{
            //    yield return n == 'T' ? 'U' : n;                
            //}
        }

        public static IEnumerable<char> Translate(string nucSeq, bool untilFirstStopCodon = false)
        {
            for (int k = 0; k < nucSeq.Length - 2; k += 3)
            {
                var codonSeq = nucSeq.Substring(k, 3);
                var prot = codon2Aa[codonSeq];
                if (untilFirstStopCodon && prot == '*')
                {
                    yield break;
                }
                yield return prot;
            }
        }

        public static IEnumerable<char> RevComp(IEnumerable<char> nucSeq, bool isRna = false)
        {
            return Compliment(nucSeq.Reverse(), isRna);
        }

        public static IEnumerable<char> Compliment(IEnumerable<char> nucSeq, bool isRna = false)
        {
            foreach (var n in nucSeq)
            {
                switch (n)
                {
                    case 'A':
                        if (isRna)
                        {
                            yield return 'U';
                        }
                        else
                        {
                            yield return 'T';
                        }
                        break;
                    case 'T':
                        yield return 'A';
                        break;
                    case 'G':
                        yield return 'C';
                        break;
                    case 'C':
                        yield return 'G';
                        break;
                    case 'U':
                        yield return 'A';
                        break;
                    default:
                        yield return '?';
                        break;
                }
            }
        }

    }
}
