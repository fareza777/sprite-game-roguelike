"""Compile-check the Unity C# assemblies without the editor, using Unity's bundled Roslyn.
Reads references and defines from the generated .csproj files, compiles every .cs under each assembly's folder.
usage: python tools/csc-check.py            (prints errors, exit code 1 if any)"""
import glob
import os
import re
import subprocess
import sys
import tempfile

ROOT = os.path.join(os.path.dirname(__file__), "..", "Oathdeep")
UNITY = r"C:/Program Files/Unity/Hub/Editor/6000.3.20f1/Editor/Data"
DOTNET = UNITY + "/NetCoreRuntime/dotnet.exe"
CSC = UNITY + "/DotNetSdkRoslyn/csc.dll"
SCRIPTS = os.path.join(ROOT, "Assets", "_Game", "Scripts")
TESTS = os.path.join(ROOT, "Assets", "_Game", "Tests", "EditMode")

ASSEMBLIES = [
    ("Oathdeep.Runtime", [f for f in glob.glob(SCRIPTS + "/**/*.cs", recursive=True) if os.sep + "Editor" + os.sep not in f and os.sep + "Monetization" + os.sep not in f], []),
    ("Oathdeep.Monetization", glob.glob(SCRIPTS + "/Monetization/**/*.cs", recursive=True), ["Oathdeep.Runtime"]),
    ("Oathdeep.Editor", glob.glob(SCRIPTS + "/Editor/**/*.cs", recursive=True), ["Oathdeep.Runtime"]),
    ("Oathdeep.Tests.EditMode", glob.glob(TESTS + "/**/*.cs", recursive=True), ["Oathdeep.Runtime"]),
]


def project(name):
    text = open(os.path.join(ROOT, name + ".csproj"), encoding="utf-8").read()
    refs = [r if os.path.isabs(r) else os.path.join(ROOT, r) for r in re.findall(r"<HintPath>(.*?)</HintPath>", text)]
    defines = re.search(r"<DefineConstants>(.*?)</DefineConstants>", text)
    return refs, defines.group(1) if defines else ""


out_dir = tempfile.mkdtemp(prefix="csc_")
failed = False
for name, files, deps in ASSEMBLIES:
    refs, defines = project(name)
    refs = [r for r in refs if not any(os.path.basename(r).startswith(d + ".") for d in [a[0] for a in ASSEMBLIES])]
    refs += [os.path.join(out_dir, d + ".dll") for d in deps]
    rsp = os.path.join(out_dir, name + ".rsp")
    with open(rsp, "w", encoding="utf-8") as f:
        f.write("-target:library -nologo -nowarn:1591,0169,0414,0649,0067 -langversion:9 -unsafe\n")
        f.write(f'-out:"{os.path.join(out_dir, name + ".dll")}"\n')
        f.write(f"-define:{defines}\n")
        for r in refs:
            f.write(f'-r:"{r}"\n')
        for src in files:
            f.write(f'"{os.path.abspath(src)}"\n')
    res = subprocess.run([DOTNET, CSC, "@" + rsp], capture_output=True, text=True)
    errors = [l for l in res.stdout.splitlines() if ": error " in l]
    print(f"{name}: {'OK' if res.returncode == 0 else str(len(errors)) + ' error(s)'}")
    for l in errors[:25]:
        print("  " + l.replace(os.path.abspath(ROOT) + os.sep, ""))
    if res.returncode != 0:
        failed = True
        break
sys.exit(1 if failed else 0)
