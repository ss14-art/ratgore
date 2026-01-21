#!/usr/bin/env python3

import requests
import os
import subprocess
from typing import Iterable
import urllib3
import sys

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

PUBLISH_TOKEN = os.environ["PUBLISH_TOKEN"]
VERSION = os.environ["GITHUB_SHA"]

RELEASE_DIR = "release"

#
# CONFIGURATION PARAMETERS
# Forks should change these to publish to their own infrastructure.
#
ROBUST_CDN_URL = "https://cdn.ss14.art/"
FORK_ID = "ratgore14"

def main():
    session = requests.Session()
    session.headers = {
        "Authorization": f"Bearer {PUBLISH_TOKEN}",
    }

    session.verify = False

    print(f"Starting publish on Robust.Cdn for version {VERSION}")
    
    print(f"Testing connection to {ROBUST_CDN_URL}...")
    try:
        test_resp = session.get(f"{ROBUST_CDN_URL}fork/{FORK_ID}/", timeout=10)
        print(f"Connection test status: {test_resp.status_code}")
    except Exception as e:
        print(f"Warning: Connection test failed: {e}")

    data = {
        "version": VERSION,
        "engineVersion": get_engine_version(),
    }
    headers = {
        "Content-Type": "application/json"
    }
    
    print(f"POST {ROBUST_CDN_URL}fork/{FORK_ID}/publish/start")
    print(f"Data: {data}")
    
    try:
        resp = session.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/start", json=data, headers=headers, timeout=30)
        print(f"Response status: {resp.status_code}")
        if resp.status_code != 200:
            print(f"Response text: {resp.text[:500]}")
        resp.raise_for_status()
    except requests.exceptions.SSLError as e:
        print(f"SSL Error: {e}")
        print("Trying without SSL verification...")
        session.verify = False
        resp = session.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/start", json=data, headers=headers, timeout=30)
        resp.raise_for_status()
    
    print("Publish successfully started, adding files...")

    for file in get_files_to_publish():
        print(f"Publishing {file} (size: {os.path.getsize(file)} bytes)")
        with open(file, "rb") as f:
            headers = {
                "Content-Type": "application/octet-stream",
                "Robust-Cdn-Publish-File": os.path.basename(file),
                "Robust-Cdn-Publish-Version": VERSION
            }
            print(f"POST {ROBUST_CDN_URL}fork/{FORK_ID}/publish/file")
            try:
                resp = session.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/file", data=f, headers=headers, timeout=120)
                print(f"Response status: {resp.status_code}")
                if resp.status_code != 200:
                    print(f"Response text: {resp.text[:500]}")
                resp.raise_for_status()
            except requests.exceptions.ConnectionError as e:
                print(f"Connection error: {e}")
                print("Possible timeout or nginx issue. Check nginx configuration.")
                sys.exit(1)

    print("Successfully pushed files, finishing publish...")

    data = {
        "version": VERSION
    }
    headers = {
        "Content-Type": "application/json"
    }
    
    print(f"POST {ROBUST_CDN_URL}fork/{FORK_ID}/publish/finish")
    resp = session.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/finish", json=data, headers=headers, timeout=30)
    print(f"Response status: {resp.status_code}")
    if resp.status_code != 200:
        print(f"Response text: {resp.text[:500]}")
    resp.raise_for_status()

    print("SUCCESS! Build published successfully.")


def get_files_to_publish() -> Iterable[str]:
    for file in os.listdir(RELEASE_DIR):
        if file.endswith('.zip'):
            yield os.path.join(RELEASE_DIR, file)


def get_engine_version() -> str:
    try:
        proc = subprocess.run(["git", "describe","--tags", "--abbrev=0"], stdout=subprocess.PIPE, cwd="RobustToolbox", check=True, encoding="UTF-8")
        tag = proc.stdout.strip()
        if tag.startswith("v"):
            return tag[1:] # Cut off v prefix.
        return tag
    except Exception as e:
        print(f"Warning: Could not get engine version: {e}")
        return "unknown"


if __name__ == '__main__':
    main()
