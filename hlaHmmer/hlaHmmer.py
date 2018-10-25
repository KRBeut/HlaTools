import argparse
import os
import os.path
import sys
import subprocess

def file_path(string):
    if os.path.exists(string):
        return string
    else:
        raise NotADirectoryError(string)

def dir_path(string):
    if os.path.isdir(string):
        return string
    else:
        raise NotADirectoryError(string)

def filepath2Pipe(filepath):
    ext = os.path.splitext(filepath)[1]
    if ext == '.bam':
        return ['samtools fasta -F 0x900 {0} | '.format(filepath)]
    else:
        if ext == '.gz':
            faPipes = 'gzip -cdk {0} | '.format(filepath)
            ext = os.path.splitext(os.path.splitext(filepath)[0])[1]
        else:
            faPipes = 'cat {0} | '.format(filepath)        
        if ext == '.fq':
            faPipes = faPipes+'awk \'{if(NR%4==1) {split(substr($0,2),a," "); printf(">%s_2\\n",a[1]);} else if(NR%4==2) print;}\' | '        
        return faPipes

def nhmmer(readFiles,hmmFiles,workDir,hmmerOpts):
    if isinstance(readFiles,str):
        readFiles = [readFiles]
    
    k = 0
    faPipes = [filepath2Pipe(x) for x in set(readFiles)]
    hmmerCmdTemplate = 'nhmmer -o {0} {1} {2} -';
    multiFaPipes = len(faPipes) > 1
    hmmerGoiAlignments = []
    for fa in faPipes:
        k=k+1
        for hmmFilepath in hmmFiles:
            outFilepath = os.path.join(workDir,'{0}{1}.txt'.format(os.path.splitext(hmmFilepath)[0],'_{0}'.format(k) if multiFaPipes else ''))
            hmmerGoiAlignments.append(outFilepath)
            cmd = '{0}{1}'.format(fa,hmmerCmdTemplate.format(outFilepath,hmmerOpts,hmmFilepath))
            print(cmd)
            #os.system(cmd);
    return hmmerGoiAlignments

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("-g", "--gois", type=str, nargs='+', help=".hmm filepaths of genes to map onto", action="store", required=True)
    parser.add_argument("-d", "--decoys", type=str, nargs='+', help=".hmm filepath of genes that are homologous to one or more goi", action="store", default=[])
    parser.add_argument("-1", "--fq1", type=file_path, help="path to mate pair one fq(.gz)", action="store")
    parser.add_argument("-2", "--fq2", type=file_path, help="path to mate pair two fq(.gz)", action="store")
    parser.add_argument("-f", "--fq", type=str, help="path to unpaired fq(.gz)", action="store")
    parser.add_argument("-a", "--fa", type=str, help="path to unpaired fa(.gz)", action="store")
    parser.add_argument("-b", "--bam", type=str, help="path to input bam", action="store")
    parser.add_argument("-o", "--out", help="path to output bam", action="store", type=str)
    parser.add_argument("-p", "--hmmerOpts", nargs='+', help="hmmer options", action="store", type=str, default=['--notextw','--nonull2','--nobias','--dna','--tformat fasta'])
    parser.add_argument("-w", "--workDir", type=dir_path, help="path to dir where intermediate files can be written", action="store", default=os.getcwd())
    args = parser.parse_args()

    
    #check args and set defaults
    try:
        if not os.path.exists(args.workDir):
            os.makedirs(args.workDir)
    except OSError:
        print ('Error: Could not create work directory \'{0}\' '.format(args.workDir))

    if not args.out:
        args.out = os.path.join(args.workDir,'nhmmer.bam')

    hmmerOpts = ' '.join(args.hmmerOpts).replace('\'','')
    if args.fq1 or args.fq2:
        if args.fq or args.bam:
            raise ValueError('too many input types! Pleae use (--fq1 and --fq2), --fq, or --bam')
        if not (args.fq1 and args.fq2):
            raise ValueError('must specify both --fq1 and --fq2, or just --fq or --bam')
        if args.fq1 == args.fq2:
            raise ValueError('--fq1 cannot be the same file name as --fq2. Pleae use --fq for unpaired reads.')
        readFiles = [args.fq1,args.fq2]
    elif args.fq and args.bam:
            raise ValueError('too many input types! Pleae use (--fq1 and --fq2), --fq, or --bam')
    elif args.fq:
        readFiles = [args.fq]
    elif args.bam:
        readFiles = [args.bam]
    else:
        raise ValueError('No input reads were given to map! Please use (--fq1,fq2), --fq, or --bam')
    
    #map the reads onto the hmm from each gene of interest
    hmmerGoiAlignments = nhmmer(readFiles, args.gois, args.workDir, hmmerOpts)

    #build a new, smaller, fa file with only the reads that mapped onto one or more goi
    goiFaFilepath = os.path.join(args.workDir, 'goi.fa.gz')
    cmd = 'dotnet hlatools hmmerConvert -f fa -c on -o {0} -i {1}'.format(goiFaFilepath,' '.join(hmmerGoiAlignments))
    print(cmd)
    #os.system(cmd);

    #map the reads in goi.fa onto the decoys
    hmmerDecoyAlignments = nhmmer(goiFaFilepath, args.decoys, args.workDir, hmmerOpts)

    #output the final bam file
    cmd = 'dotnet hlatools hmmerConvert -f sam -c off -i {0} | samtools view -Sb - | samtools sort - {1}'.format(' '.join(hmmerGoiAlignments + hmmerDecoyAlignments),args.out)
    print(cmd)
    #os.system(cmd);
        
    #clean-up intermediate files
    #os.remove(goiFaFilepath)
    #for f in hmmerGoiAlignments:
    #    os.remove(f)
    #for f in hmmerDecoyAlignments:
    #    os.remove(f)

if __name__ == '__main__':
    main()



