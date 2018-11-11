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
    if ext == '.gz':
        faPipe = 'gzip -cdk {0}'.format(filepath)
    else:
        faPipe = 'cat {0}'.format(filepath)
    faPipe = faPipe + ' | awk \'BEGIN{{RS="@";FS="\\n"}}{{if(NF>3){{printf(">%s|%d|%s|%s\\n%s\\n",$1,{0},$2,$4,$2)}};}}\''.format(flag)
    return faPipe
        

def nhmmer(faPipe,hmmFiles,workDir,hmmerOpts,nhmmerPath = 'nhmmer', hlatoolsPath='hlatools.dll'):
    
    hlatoolsCmdTemplate = 'dotnet {0}'.format(hlatoolsPath) +' hmmerConvert -f sam -c off -i {0} | gawk \'BEGIN{{OFS="\\t"}}{{if(!($1 ~ /^@/)){{$2=or($2,2);}}print($0);}}\' | samtools sort -n - | samtools fixmate -cm - - | samtools sort - | samtools markdup - {1}'
    
    alignments = []
    for hmmFilepath in hmmFiles:
        outBamFilename = os.path.splitext(os.path.basename(hmmFilepath))[0]+'.bam'
        outTxtFilename = os.path.splitext(os.path.basename(hmmFilepath))[0]+'.txt.gz'
        outBamFilepath = os.path.join(workDir,outBamFilename)
        outTxtFilepath = os.path.join(workDir,outTxtFilename)
        alignments.append(outBamFilepath)
        cmd = '{0} | {1} {2} {3} - | {4}'.format(faPipe,nhmmerPath,hmmerOpts,hmmFilepath,hlatoolsCmdTemplate.format('-', outBamFilepath))
        print(cmd)
        subprocess.call(cmd, shell=True)
        #cmd = '{0} | {1} {2} {3} - | gzip > {4}'.format(faPipe,nhmmerPath,hmmerOpts,hmmFilepath,outTxtFilepath)
        #print(cmd)
        #subprocess.call(cmd, shell=True)
        #cmd = hlatoolsCmdTemplate.format(outTxtFilepath, outBamFilepath)
        #print(cmd)
        #subprocess.call(cmd, shell=True)
        #os.remove(outTxtFilepath)
    return alignments

def main():
    
    parser = argparse.ArgumentParser()
    parser.add_argument("-g", "--gois", type=str, nargs='+', help=".hmm filepaths of genes to map onto", action="store", required=True)
    parser.add_argument("-d", "--decoys", type=str, nargs='+', help=".hmm filepath of genes that are homologous to one or more goi", action="store", default=[])
    parser.add_argument("-1", "--fq1", type=file_path, help="path to mate pair one fq(.gz)", action="store")
    parser.add_argument("-2", "--fq2", type=file_path, help="path to mate pair two fq(.gz)", action="store")
    parser.add_argument("-f", "--fq", type=str, help="path to unpaired fq(.gz)", action="store")
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
    if args.fq1 or args.fq2:
        if args.fq:
            raise ValueError('too many input types! Pleae use (--fq1 and --fq2), --fq, or --bam')
        if not (args.fq1 and args.fq2):
            raise ValueError('must specify both --fq1 and --fq2, or just --fq or --bam')
        if args.fq1 == args.fq2:
            raise ValueError('--fq1 cannot be the same file name as --fq2. Pleae use --fq for unpaired reads.')
        fa1Pipe = filepath2Pipe(args.fq1,65)
        fa2Pipe = filepath2Pipe(args.fq2,129)
        faPipe = '{{({0}) ; ({1})}}'.format(fa1Pipe,fa2Pipe)
    elif args.fq:
        faPipe = filepath2Pipe(args.fq,0)
    else:
        raise ValueError('No input reads were given to map! Please use (--fq1,fq2), or --fq')
    
    
    #map the reads onto the hmm from each gene of interest
    hmmerGoiAlignments = nhmmer(faPipe, args.gois, args.workDir, hmmerOpts, args.nhmmerPath, args.hlatoolsPath)
    
    #build a new, smaller, fa file with only the reads that mapped onto one or more goi
    goiFaFilepath = os.path.join(args.workDir,'goi.fa.gz')
    cmd = 'samtools cat {0} | samtools view -h | gawk \'BEGIN{{OFS="\\t"}}{{if(!($1 ~ /^@/)){{$1=($1"|"and($2,193));}}print($0);}}\' | samtools sort -n - | samtools fastq -n -F 0x900 - | awk \'BEGIN{{RS="@";FS="\\n"}}{{if(NF>3){{printf(">%s|%s|%s\\n%s\\n",$1,$2,$4,$2)}};}}\' | gzip > {1}'.format(' '.join(hmmerGoiAlignments),goiFaFilepath);
    print(cmd)
    subprocess.call(cmd, shell=True);
    
    #map the reads in goi.fa.gz onto the decoys
    hmmerDecoyAlignments = nhmmer('gzip -cdk {0}'.format(goiFaFilepath), args.decoys, args.workDir, hmmerOpts, args.nhmmerPath, args.hlatoolsPath)
    
    #output the final, merged, bam file
    allBams = hmmerGoiAlignments + hmmerDecoyAlignments
    cmd = 'samtools merge {0} {1}'.format(args.out, ' '.join(allBams))
    print(cmd)
    subprocess.call(cmd, shell=True);
    
    #clean-up intermediate files
    os.remove(goiFaFilepath)
    for f in allBams:
        os.remove(f)

if __name__ == '__main__':
    main()



