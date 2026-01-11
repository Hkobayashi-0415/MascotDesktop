#!/usr/bin/env python3
"""
list_characters.py - Character Registry (read-only)

Enumerates local characters under data/assets_user/characters/<slug>/
and outputs:
- Slug list
- Detected mode (mmd/pngtuber/etc)
- Presence/absence of required files
- WARN messages for missing items

Usage:
    python scripts/setup/list_characters.py          # Human-readable output
    python scripts/setup/list_characters.py --json   # JSON output
"""

import argparse
import json
import os
import sys
from pathlib import Path
from typing import Dict, List, Optional


# Workspace root detection
def get_workspace_root() -> Path:
    """Get workspace root directory."""
    script_path = Path(__file__).resolve()
    # scripts/setup/list_characters.py -> workspace
    return script_path.parent.parent.parent


# Character directory
WORKSPACE_ROOT = get_workspace_root()
CHARACTERS_DIR = WORKSPACE_ROOT / "data" / "assets_user" / "characters"


def detect_mode(char_path: Path) -> Optional[str]:
    """
    Detect character mode based on directory structure.
    
    Returns:
        - "mmd" if mmd/ directory exists
        - "pngtuber" if pngtuber/ or states/ directory exists
        - "unknown" if none detected
        - None if character directory doesn't exist
    """
    if not char_path.is_dir():
        return None
    
    if (char_path / "mmd").is_dir():
        return "mmd"
    if (char_path / "pngtuber").is_dir():
        return "pngtuber"
    if (char_path / "states").is_dir():
        return "pngtuber"
    
    return "unknown"


def check_required_files(char_path: Path, mode: str) -> Dict[str, bool]:
    """
    Check presence of required files based on mode.
    
    Returns dict with file -> exists status.
    """
    checks = {}
    
    if mode == "mmd":
        mmd_dir = char_path / "mmd"
        checks["mmd/model.pmx"] = (mmd_dir / "model.pmx").is_file()
        checks["mmd/manifest.json"] = (mmd_dir / "manifest.json").is_file()
        
        # Check for any .vmd files in motions/
        motions_dir = char_path / "motions"
        vmd_files = list(motions_dir.glob("*.vmd")) if motions_dir.is_dir() else []
        checks["motions/*.vmd"] = len(vmd_files) > 0
        
    elif mode == "pngtuber":
        pngtuber_dir = char_path / "pngtuber"
        states_dir = char_path / "states"
        
        # Check for state images
        if pngtuber_dir.is_dir():
            png_files = list(pngtuber_dir.glob("*.png"))
        elif states_dir.is_dir():
            png_files = list(states_dir.glob("*.png"))
        else:
            png_files = []
        
        checks["states/*.png"] = len(png_files) > 0
    
    return checks


def get_warnings(slug: str, mode: str, checks: Dict[str, bool]) -> List[str]:
    """Generate warning messages for missing required files."""
    warnings = []
    
    if mode == "unknown":
        warnings.append(f"[WARN] {slug}: Mode could not be detected (no mmd/ or pngtuber/ directory)")
    
    for file_pattern, exists in checks.items():
        if not exists:
            warnings.append(f"[WARN] {slug}: Missing {file_pattern}")
    
    return warnings


def scan_character(slug: str) -> Dict:
    """
    Scan a single character and return its info.
    
    Returns dict with:
        - slug
        - mode
        - checks (dict of file -> exists)
        - warnings (list of warning strings)
        - valid (bool: no critical warnings)
    """
    char_path = CHARACTERS_DIR / slug
    mode = detect_mode(char_path)
    
    if mode is None:
        return {
            "slug": slug,
            "mode": None,
            "checks": {},
            "warnings": [f"[WARN] {slug}: Directory does not exist"],
            "valid": False,
        }
    
    checks = check_required_files(char_path, mode)
    warnings = get_warnings(slug, mode, checks)
    
    # Valid if mode is known and model exists (for mmd) or states exist (for pngtuber)
    valid = mode != "unknown" and all(
        exists for file_pattern, exists in checks.items()
        if "model.pmx" in file_pattern or "states" in file_pattern
    )
    
    return {
        "slug": slug,
        "mode": mode,
        "checks": checks,
        "warnings": warnings,
        "valid": valid,
    }


def scan_all_characters() -> List[Dict]:
    """Scan all characters in the characters directory."""
    results = []
    
    if not CHARACTERS_DIR.is_dir():
        print(f"[INFO] Characters directory does not exist: {CHARACTERS_DIR}", file=sys.stderr)
        return results
    
    slugs = sorted([
        d.name for d in CHARACTERS_DIR.iterdir()
        if d.is_dir() and not d.name.startswith(".")
    ])
    
    for slug in slugs:
        result = scan_character(slug)
        results.append(result)
    
    return results


def print_human_readable(results: List[Dict]) -> None:
    """Print results in human-readable format."""
    print("=" * 60)
    print("Character Registry")
    print("=" * 60)
    print(f"Characters directory: {CHARACTERS_DIR}")
    print(f"Total characters: {len(results)}")
    print("-" * 60)
    
    if not results:
        print("[INFO] No characters found.")
        print(f"       Place character folders under: {CHARACTERS_DIR}")
        return
    
    valid_count = sum(1 for r in results if r["valid"])
    print(f"Valid: {valid_count} / {len(results)}")
    print("-" * 60)
    
    for result in results:
        slug = result["slug"]
        mode = result["mode"] or "N/A"
        valid_mark = "✓" if result["valid"] else "✗"
        
        print(f"\n{valid_mark} {slug} [{mode}]")
        
        # Print file checks
        for file_pattern, exists in result["checks"].items():
            status = "OK" if exists else "MISSING"
            print(f"    {file_pattern}: {status}")
        
        # Print warnings
        for warning in result["warnings"]:
            print(f"    {warning}")
    
    print("\n" + "=" * 60)
    
    # Summary of all warnings
    all_warnings = [w for r in results for w in r["warnings"]]
    if all_warnings:
        print(f"\nTotal warnings: {len(all_warnings)}")
        for w in all_warnings:
            print(f"  {w}")


def print_json(results: List[Dict]) -> None:
    """Print results as JSON."""
    output = {
        "characters_dir": str(CHARACTERS_DIR),
        "total": len(results),
        "valid": sum(1 for r in results if r["valid"]),
        "characters": results,
    }
    print(json.dumps(output, indent=2, ensure_ascii=False))


def main() -> int:
    parser = argparse.ArgumentParser(
        description="List local characters and check for required files."
    )
    parser.add_argument(
        "--json",
        action="store_true",
        help="Output as JSON instead of human-readable format",
    )
    parser.add_argument(
        "--dir",
        type=str,
        default=None,
        help="Override characters directory (for testing)",
    )
    
    args = parser.parse_args()
    
    # Override directory if specified
    global CHARACTERS_DIR
    if args.dir:
        CHARACTERS_DIR = Path(args.dir)
    
    try:
        results = scan_all_characters()
        
        if args.json:
            print_json(results)
        else:
            print_human_readable(results)
        
        return 0
    
    except Exception as e:
        print(f"[ERROR] Unexpected error: {e}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
