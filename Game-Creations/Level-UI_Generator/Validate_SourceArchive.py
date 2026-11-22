"""Offline source audit. Requires PyYAML. Never writes or imports any Unity asset."""
import argparse, collections, hashlib, json, re, zipfile
from pathlib import PurePosixPath
import yaml

def audit(path):
    levels={}; issues=[]; refs=[]
    with zipfile.ZipFile(path) as z:
        if z.testzip(): raise ValueError('Corrupt ZIP')
        for name in z.namelist():
            m=re.fullmatch(r'Level[_ -]*0*(\d+)\.asset',PurePosixPath(name).name,re.I)
            if not m: continue
            n=int(m.group(1)); text=z.read(name).decode('utf-8-sig');
            raw=yaml.safe_load('\n'.join(text.splitlines()[3:]))['MonoBehaviour']
            assert raw['m_EditorClassIdentifier'].endswith('ProjectSpark.Gameplay.SparkLevelDefinition'),name
            if n in levels: issues.append({'number':n,'issue':'duplicate number'})
            targets=raw.get('targets') or []; ids=collections.Counter(t.get('targetId') for t in targets)
            for key,count in ids.items():
                if not key or count>1: issues.append({'number':n,'issue':'empty/duplicate target ID','id':key})
            if raw['requiredTargetCount']!=len(targets):issues.append({'number':n,'issue':'required count differs from configured count'})
            legacy=re.match(r'^level_0*(\d+)(?:_|$)',raw['levelId'],re.I)
            if legacy and int(legacy.group(1))!=n: issues.append({'number':n,'issue':'legacy ID differs from filename'})
            levels[n]={'number':n,'key':f'PS-L{n:03d}','source':name,'legacyId':raw['levelId'],'title':raw['displayName'],'description':raw['description'],'targetCount':len(targets),'requiredTargets':raw['requiredTargetCount'],'defaultUnlocked':bool(raw['unlockedByDefault']),'autoUnlock':bool(raw['autoUnlockNextLevel']),'autoAdvance':bool(raw['autoAdvanceOnCompletion']),'targets':targets}
    return {'archiveSha256':hashlib.sha256(open(path,'rb').read()).hexdigest(),'definitions':len(levels),'configuredTargets':sum(e['targetCount'] for e in levels.values()),'missingFrom1To130':[n for n in range(1,131) if n not in levels],'issues':issues,'levels':[levels[n] for n in sorted(levels)],'unityCompilation':'NOT RUN','runtime':'NOT RUN'}
if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('archive');parser.add_argument('--out',default='SourceArchiveAudit.json');args=parser.parse_args();result=audit(args.archive);open(args.out,'w',encoding='utf8').write(json.dumps(result,indent=2,ensure_ascii=False)+'\n');print({k:result[k] for k in ('definitions','configuredTargets','missingFrom1To130')})
