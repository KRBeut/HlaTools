

import argparse
import os

def dir_path(string):
    if os.path.isdir(string):
        return string
    else:
        raise NotADirectoryError(string)

parser = argparse.ArgumentParser()
parser.add_argument("-i", "--imgtRootDir", type=dir_path, help="path to input IMGT nomenclature dir", action="store", required=True)
parser.add_argument("-o", "--outputRootDir", type=dir_path, help="path to dir where output should be written", action="store")
parser.add_argument("-p", "--prevRootDir", type=dir_path, help="path to dir where previous hlatools nomenclature was written", action="store")
args = parser.parse_args()


print(args.imgtRootDir)
