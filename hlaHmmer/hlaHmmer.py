import argparse
import os
import os.path
import sys
import subprocess
import shlex
from subprocess import call

def file_path(string):
    if os.path.exists(string):
        return string
    else:
        raise ValueError(string)

def dir_path(string):
    if os.path.isdir(string):
        return string
    else:
        raise ValueError(string)

def filepath2Pipe(filepath,flag):
    ext = os.path.splitext(filepath)[1]
    faPipe = 'gzip -cdk {0}'.format(filepath) if ext == '.gz' else 'cat {0}'.format(filepath)
    faPipe = faPipe + ' | awk \'BEGIN{{RS="@";FS="\\n"}}{{if(NF>3){{printf(">%s|%d|%s|%s\\n%s\\n",$1,{0},$2,$4,$2)}};}}\''.format(flag)
    return faPipe
        

def bam2fagz(bamFilepath, fagzFilepath):
    if isinstance(bamFilepath, str):
        bamFilepath = [bamFilepath]
    collatePrefix = os.path.join(os.path.dirname(fagzFilepath),os.path.splitext(os.path.basename(fagzFilepath))[0]+'tmpBam2FagzCollate')
    #cmd = 'samtools cat {0} | samtools view -h | gawk \'BEGIN{{OFS="\\t"}}{{if(!($1 ~ /^@/)){{$1=($1"|"or(3,and($2,704)));}}print($0);}}\' | samtools collate -O - {1} | samtools fastq -n -F 0x900 - | awk \'BEGIN{{RS="@";FS="\\n"}}{{if(NF>3){{printf(">%s|%s|%s\\n%s\\n",$1,$2,$4,$2)}};}}\' | gzip > {2}'.format(' '.join(bamFilepath),collatePrefix,fagzFilepath)
    cmd = 'samtools cat {0} | samtools view -h | gawk \'BEGIN{{OFS="\\t"}}{{if(!($1 ~ /^@/)){{$1=($1"|"or(3,and($2,704)));}}print($0);}}\' | samtools collate -O - {1} | samtools fastq -n -F 0x900 - | awk \'BEGIN{{RS="\\n";FS="\\n";ORS=""}}{{if(NR%4==1){{printf(">%s",substr($0,2))}}else if(NR%4==2){{printf("|%s",$0)}}else if(NR%4==0){{printf("|%s\\n",$0)}}}}\' | awk \'BEGIN{{RS="\\n"}}{{split($0,a,"|");printf("%s\\n%s\\n",$0,a[3])}}\' | gzip > {2}'.format(' '.join(bamFilepath),collatePrefix,fagzFilepath)
    print(cmd)
    subprocess.call(cmd, shell=True)

def nhmmer(faPipe,hmmFiles,workDir,hmmerOpts,nhmmerPath = 'nhmmer', hlatoolsPath='hlatools.dll'):
    
    hlatoolsCmdTemplate = 'dotnet {0}'.format(hlatoolsPath) +' hmmerConvert -f sam -c off -i {0} | samtools view -bh -q 90 - | samtools collate -O - {1} | samtools fixmate -cm - - | samtools view -bh -f 2 | samtools sort - | samtools markdup - {2}'
    
    alignments = []
    for hmmFilepath in hmmFiles:
        outBamFilepath = os.path.join(workDir,os.path.splitext(os.path.basename(hmmFilepath))[0]+'.bam')
        collatePrefix = os.path.join(os.path.dirname(outBamFilepath),os.path.splitext(os.path.basename(outBamFilepath))[0]+'tmpNhmmerCollate')
        alignments.append(outBamFilepath)
        cmd = '{0} | {1} {2} {3} - | {4}'.format(faPipe,nhmmerPath,hmmerOpts,hmmFilepath,hlatoolsCmdTemplate.format('-', collatePrefix, outBamFilepath))
        print(cmd)
        subprocess.call(cmd, shell=True)
        #outTxtFilename = os.path.splitext(os.path.basename(hmmFilepath))[0]+'.txt.gz'
        #outTxtFilepath = os.path.join(workDir,outTxtFilename)
        #cmd = '{0} | {1} {2} {3} - | gzip > {4}'.format(faPipe,nhmmerPath,hmmerOpts,hmmFilepath,outTxtFilepath)
        #print(cmd)
        #subprocess.call(cmd, shell=True)
        #cmd = hlatoolsCmdTemplate.format(outTxtFilepath, collatePrefix, outBamFilepath)
        #print(cmd)
        #subprocess.call(cmd, shell=True)
        #os.remove(outTxtFilepath)
    return alignments

def main():
    
    parser = argparse.ArgumentParser()
    parser.add_argument("-g", "--gois", type=str, nargs='+', help=".hmm filepaths of genes to map onto", action="store", required=True)
    parser.add_argument("-d", "--decoys", type=str, nargs='+', help=".hmm filepath of genes that are homologous to one or more goi", action="store", default=[])
    parser.add_argument("-b", "--bam", type=file_path, help="path to bam with input reads", action="store")
    parser.add_argument("-1", "--fq1", type=file_path, help="path to mate pair one fq(.gz)", action="store")
    parser.add_argument("-2", "--fq2", type=file_path, help="path to mate pair two fq(.gz)", action="store")
    parser.add_argument("-f", "--fq", type=str, help="path to unpaired fq(.gz)", action="store")
    parser.add_argument("-a", "--fa", type=str, help="path to fa(.gz) where read name is formated as readName|samFlags masked by 705, and read mapped in proper pair flag set|readSeq|readQuals", action="store")
    parser.add_argument("-o", "--out", help="path to output bam", action="store", type=str, required=True)
    parser.add_argument("-p", "--hmmerOpts", nargs='+', help="hmmer options", action="store", type=str, default=['--notextw','--nonull2','--nobias','--dna','--tformat fasta'])
    parser.add_argument("-w", "--workDir", type=str, help="path to dir where intermediate files can be written", action="store", default=os.getcwd())
    parser.add_argument("-t", "--hlatoolsPath", type=str, help="hlatools.dll filepath", action="store", default='hlatools.dll')
    parser.add_argument("-n", "--nhmmerPath", type=str, help="nhmmer filepath", action="store", default='nhmmer')
    args = parser.parse_args()
    
    
    #check args and set defaults
    try:
        if not os.path.exists(args.workDir):
            os.makedirs(args.workDir)
    except OSError:
        print ('Error: Could not create work directory \'{0}\' '.format(args.workDir))
    
    hmmerOpts = ' '.join(args.hmmerOpts).replace('\'','')
    fagzFilepath = None
    if args.bam:
        if args.fq or args.fq1 or args.fq2 or args.fa:
            raise ValueError('too many input types! Pleae use (--fq1 and --fq2), --fq, --bam, or --fa')
        fagzFilepath = os.path.join(args.workDir,os.path.splitext(os.path.basename(args.bam))[0]+'.fa.gz')
        bam2fagz(args.bam, fagzFilepath)
        faPipe = 'gzip -cdk {0}'.format(fagzFilepath)
    elif args.fq1 or args.fq2:
        if args.fq or args.fa:
            raise ValueError('too many input types! Pleae use (--fq1 and --fq2), --fq, --bam, or --fa')
        if not (args.fq1 and args.fq2):
            raise ValueError('must specify both --fq1 and --fq2, or just --fq or --bam')
        if args.fq1 == args.fq2:
            raise ValueError('--fq1 cannot be the same file name as --fq2. Pleae use --fq for unpaired reads.')
        fa1Pipe = filepath2Pipe(args.fq1,67)
        fa2Pipe = filepath2Pipe(args.fq2,131)
        faPipe = '{{({0}) ; ({1})}}'.format(fa1Pipe,fa2Pipe)
    elif args.fq:
        if args.fa:
            raise ValueError('too many input types! Pleae use (--fq1 and --fq2), --fq, --bam, or --fa')
        faPipe = filepath2Pipe(args.fq,0)
    elif args.fa:
        ext = os.path.splitext(args.fa)[1]
        faPipe = 'gzip -cdk {0}'.format(args.fa) if ext == '.gz' else 'cat {0}'.format(args.fa)
    else:
        raise ValueError('No input reads were given to map! Please use (--fq1,fq2), --fq, --bam, or --fa')
    
    
    #map the reads onto the hmm from each gene of interest
    hmmerGoiAlignments = nhmmer(faPipe, args.gois, args.workDir, hmmerOpts, args.nhmmerPath, args.hlatoolsPath)
    
    #build a new, smaller, fa file with only the reads that mapped onto one or more goi
    goiFagzFilepath = os.path.join(args.workDir,'goi.fa.gz')
    bam2fagz(' '.join(hmmerGoiAlignments),goiFagzFilepath)
    
    #map the reads in goi.fa.gz onto the decoys
    hmmerDecoyAlignments = nhmmer('gzip -cdk {0}'.format(goiFagzFilepath), args.decoys, args.workDir, hmmerOpts, args.nhmmerPath, args.hlatoolsPath)
    
    #output the final, merged, bam file (the merged file will be coordinate-sorted since each of the loci files were coordinate sorted before markdup)
    allBams = hmmerGoiAlignments + hmmerDecoyAlignments
    cmd = 'samtools merge {0} {1}'.format(args.out, ' '.join(allBams))
    print(cmd)
    subprocess.call(cmd, shell=True);
    
    #clean-up intermediate files
    #os.remove(goiFagzFilepath)
    for f in allBams:
        os.remove(f)
    #if fagzFilepath:
    #    os.remove(fagzFilepath)

if __name__ == '__main__':
    main()



