#!/usr/bin/python3
"""
A helper utility for marshalling the 3es Unity support DLLs under the Assets/plugins folder of the 3rdEyeScene unity
project.
"""
import argparse
import os
import platform
import shutil
import sys

def validate_args(args):
    if args.output == '':
        # Set default output dir if not set.
        args.output = os.path.join(args.dir, '..')
        args.output = os.path.join(args.output, '3rdEyeScene')
        args.output = os.path.join(args.output, 'Assets')
        args.output = os.path.join(args.output, 'plugins')

    if not os.path.exists(args.output):
        raise Exception("Output path does not exist " + args.output)
    src_dir = args.dir
    if not os.path.exists(src_dir):
        raise Exception("Base directory does not exists " + src_dir)
    src_dir = os.path.join(src_dir, args.project)
    if not os.path.exists(src_dir):
        raise Exception("Project directory does not exists " + src_dir)
    src_dir = os.path.join(src_dir, 'bin')
    src_dir = os.path.join(src_dir, args.configuration)
    if not os.path.exists(src_dir):
        raise Exception("Configuration directory does not exists " + src_dir)
    src_dir = os.path.join(src_dir, args.framework)
    if not os.path.exists(src_dir):
        raise Exception("Framework directory does not exists " + src_dir)
    args.src = src_dir


def marshal_unity_runtime(args):
    for root, dirs, files in os.walk(os.path.join(args.src)):
        for filename in files:
            file_ext = os.path.splitext(filename)[1]
            if file_ext == '.dll' or file_ext == '.pdb':
                src = os.path.join(root, filename)
                dst = os.path.join(args.output, filename)
                print(src, '->', dst)
                shutil.copy2(src, dst)

if __name__ == "__main__":
    default_framework = "netstandard2.0"
    if platform.system() == 'Windows':
        default_framework = 'net461'

    help_description = 'A utility for helping marshal the 3es Unity support DLLs under the Assets/plugins folder of ' \
                       'the 3rdEyeScene unity project. General usage is to run this script from the 3esCore.sln ' \
                       'directory. This marshals the 3esRuntime and supporting DLLs into ' \
                       '../3rdEyeScene/Assets/plugins, which is the default plugins path for the 3rdEyeScene Unity ' \
                       'project. Various arguments may be used to override the default behaviour, selecting the ' \
                       'directory to marshal from (-d), the .Net framework (-f), the target output directory (-o) ' \
                       'and even the project to marshal (-p). The default framework under Windows is net461, for ' \
                       'other platforms it is netstandard2.0'

    parser = argparse.ArgumentParser(description = help_description)
    parser.add_argument('--framework', '-f', dest='framework', action='store',
                        default=default_framework,
                        help='The target .net framework version')
    parser.add_argument('--configuration', '-c', dest='configuration', action='store',
                        default='Release',
                        help='The configuration to marshal (case sensitive)')
    parser.add_argument('--dir', '-d', dest='dir', action='store',
                        default=os.getcwd(),
                        help='The base project directory. This is the parent directory of the project.')
    parser.add_argument('--output', '-o', dest='output', action='store',
                        default='',
                        help='The output destination directory. Defaults to ../3rdEyeScene/Assets/plugins relative to the --dir option.')
    parser.add_argument('--project', '-p', dest='project', action='store',
                        default='3esRuntime',
                        help='The source project name')

    args = parser.parse_args()

    validate_args(args)
    marshal_unity_runtime(args)
