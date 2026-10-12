"""Export the pinned static category table to its portable v1 data format; no vendor code execution."""
import argparse, hashlib, json, struct
from pathlib import Path
from audit_character_line_categories import audit

def table_hash(rows,profiles):
    digest=hashlib.sha256();digest.update(struct.pack('<I',len(rows)))
    for row in rows:
        text=row['text'].encode('utf8');digest.update(struct.pack('<I',len(text)))
        digest.update(text);digest.update(struct.pack('<bb',row['main'],row['alternate']))
    digest.update(struct.pack('<I',len(profiles)))
    for profile in profiles:digest.update(struct.pack('<fff',*profile))
    return digest.hexdigest()

def export(binary,output):
    result=audit(binary)
    portable=dict(format='blurmask-character-line-categories',version=1,
        sourceBinary='MyScriptDocument.dll',sourceSha256=result['source_sha256'],
        tableSha256=table_hash(result['rows'],result['profiles']),
        provenance='MyScript static category/profile data; separate from MIT-licensed implementation. Read-only PE extraction; vendor code is never executed.',
        catalogAddress='180244c50',profileAddress='180244a30',
        entries=[[r['text'],r['main'],r['alternate']] for r in result['rows']],profiles=result['profiles'])
    output.parent.mkdir(parents=True,exist_ok=True)
    # Exact model bytes must match Git's LF checkout on every platform.
    # write_text would translate the final newline to CRLF on Windows.
    output.write_bytes((json.dumps(portable,ensure_ascii=False,separators=(',',':'))+'\n').encode('utf8'))
    print(f"PASS {len(portable['entries'])} entries/{len(portable['profiles'])} profiles; tableSHA256={portable['tableSha256']}; bytes={output.stat().st_size}")

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('binary',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
    export(a.binary,a.output)
