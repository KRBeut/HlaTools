![Image of HlaToolaLogo](https://github.com/KRBeut/HlaTools/blob/master/HlaTools.jpg)<br/>
Tools for analyzing DNA and RNA NGS data of HLA genes and pseudogenes

# Command line interface (CLI)

When complete, HlaTools will support the following command line interface (CLI) for the analsyis of HLA class I and class II NGS data

## Nomenclature update pipeline
	0.0) Build HlaTools database files from IMGT nomenclature update (outputs hmms, bam file of allele alignments, consensus for each locus, and gff3 annotations of exons).
	hlatools updateNom -i <nomInputRootDir> -o <nomOutputRootDir> -p <previousNomRootDir>
	
	0.1) Summarize contents of nomenclature version
	hlatools nomReport -n <hlatoolsNomRootDir> > nomReport.html
	
	0.2) Summarize diffs between two nomenclature versions
	hlatools nomDiffs -n <newerHlatoolsNomRootDir1> -o <olderHlatoolsNomRootDir2> > nomDiffsReport.html

## Mapping pipeline:
	1) Map the reads to HLA-specific HMMs.
	hlatools hmmerMap -r1 <.*\.(fa|fq|bam|sam)(.gz)?> -r2 <.*\.(fa|fq|bam|sam)(.gz)?> > my.sam
	
	2.1) Iteratively re-weight the mapping scores (irwms) and assign X locus reads to a single locus
	hlatools irwms my.sam -i 5 | hlatools assignXLocReads -r sam > myReWeightedXLocAssigned.sam
	
	2.2) Alternatively, assign reads that mapped to more than one locus to a single locus
	hlatools assignXLocReads -r my.sam > myReWeightedXLocAssigned.sam
	
	3) Call concensus for each locus
	hlatools callConsensus -r myReWeightedXLocAssigned.sam > consensus.fa

## Variant pipeline:
	4) Use your favorite variant caller(s).
	gatkHaplotypeCaller, TVC, etc myReWeightedXLocAssigned.sam consensus.fa > myReWeightedXLocAssigned.vcf
	
	5) Phase the variants.
	hlatools phaseVcf -v myReWeightedXLocAssigned.vcf -r myReWeightedXLocAssigned.sam > myReWeightedXLocAssignedPhased.vcf

## Contig building and genotype calling pipelines:
	6) Build contigs
	hlatools buildContigs -c consensus.fa -v myReWeightedXLocAssignedPhased.vcf > contigs.fa

	7) Call the HLA alleles (output should be Histoimmunogenetics Markup Language (HML) file)
	hlatools callAlleles -n ImgtNomenclature.xml -t contigs.fa > myHlaGenotypeCalls.xml
	
	8) Assign each read to one, or more, contig
	hlatools assignReadsToLoc -g myHlaGenotypeCalls.xml -r myReWeightedXLocAssigned.sam > myReWeightedXLocAssigned.sam
	
	9) QC Calls
	hlatools qcGenoCalls -g sampleHlaGenotypeCalls.xml -r myReWeightedXLocAssigned.sam -v myReWeightedXLocAssignedPhased.vcf -f haplotypeFrequencies.txt > genoCallQc.txt

## Gene expression pipeline:	
	Just feed myReWeightedXLocAssignedPhased.vcf and myReWeightedXLocAssigned.sam into your favorite existing tool(s)

## Somatics variant calling pipeline:
	Just feed myReWeightedXLocAssignedPhased.vcf and myReWeightedXLocAssigned.sam into mutect, strelka, freebayes, LOHLA, ot your favorite existing tool(s)

## Options convention across all commands

Option | Value
-------|------
-a,--alleles | aligned HLA allele sequences .bam file
-c,--consensus | consensus .fa file path, or "-" for standard in
-t,--contig | conting .fa file path, or "-" for standard in
-f,--freqs | allele frequencies file path
-g,--genos | HLA genotype calls filepath, or "-" for standard in, in HML format
-i,--iters | numner of iterations
-r,--reads | sam or bam filepath. if value is sam or bam, then input of the corresponding type is taken from standard in, or written to standard out
-s,--sto | stockholm alignment file path
-v,--vcf | vcf filepath, or "-" for standard in