#!/usr/bin/env python3

#
# Updates an RSS file on a remote server with updates to the changelog.
# See https://docs.spacestation14.io/en/hosting/changelogs for instructions.
#

# If you wanna test this script locally on Windows,
# you can use something like this in Powershell to set up the env var:
# $env:CHANGELOG_RSS_KEY=[System.IO.File]::ReadAllText($(gci "key"))

import os
import paramiko
import pathlib
import io
import base64
import yaml
import itertools
import html
import email.utils
import traceback
from typing import  List, Any, Tuple
from lxml import etree as ET
from datetime import datetime, timedelta, timezone

MAX_ITEM_AGE = timedelta(days=30)

# Set as a repository secret.
CHANGELOG_RSS_KEY = os.environ.get("CHANGELOG_RSS_KEY")

# Change these to suit your server settings
# https://docs.fabfile.org/en/stable/getting-started.html#run-commands-via-connections-and-run
SSH_HOST = "336929.senko.network"  # ИЗМЕНИТЕ: используйте IP или домен вашего сервера
SSH_USER = "changelog-rss"
SSH_PORT = 22
RSS_FILE = "changelog.xml"
XSL_FILE = "stylesheet.xsl"
HOST_KEYS = [
    "AAAAC3NzaC1lZDI1NTE5AAAAIGyqTLxqJlSzLyXSmvOO3fFGSMBJWizUw8by228syOps"
]

# RSS feed parameters, change these
FEED_TITLE       = "Ratgore Changelog"
FEED_LINK        = "https://github.com/ss14-art/ratgore/"
FEED_DESCRIPTION = "Changelog for the Ratgore branch of Space Station 14."
FEED_LANGUAGE    = "en-US"
FEED_GUID_PREFIX = "ratgore-changelog-"
FEED_URL         = "https://rss.ss14.art/changelog.xml"

CHANGELOG_FILE = "Resources/Changelog/Changelog.yml"

TYPES_TO_EMOJI = {
    "Fix":    "🐛",
    "Add":    "🆕",
    "Remove": "❌",
    "Tweak":  "⚒️"
}

XML_NS = "https://spacestation14.com/changelog_rss"
XML_NS_B = f"{{{XML_NS}}}"

XML_NS_ATOM = "http://www.w3.org/2005/Atom"
XML_NS_ATOM_B = f"{{{XML_NS_ATOM}}}"

ET.register_namespace("ss14", XML_NS)
ET.register_namespace("atom", XML_NS_ATOM)

# From https://stackoverflow.com/a/37958106/4678631
class NoDatesSafeLoader(yaml.SafeLoader):
    @classmethod
    def remove_implicit_resolver(cls, tag_to_remove):
        if not 'yaml_implicit_resolvers' in cls.__dict__:
            cls.yaml_implicit_resolvers = cls.yaml_implicit_resolvers.copy()

        for first_letter, mappings in cls.yaml_implicit_resolvers.items():
            cls.yaml_implicit_resolvers[first_letter] = [(tag, regexp)
                                                         for tag, regexp in mappings
                                                         if tag != tag_to_remove]

# Hrm yes let's make the fucking default of our serialization library to PARSE ISO-8601
# but then output garbage when re-serializing.
NoDatesSafeLoader.remove_implicit_resolver('tag:yaml.org,2002:timestamp')

def main():
    if not CHANGELOG_RSS_KEY:
        print("::notice ::CHANGELOG_RSS_KEY not set, skipping RSS changelogs")
        return

    # Debug info
    print(f"::debug ::=== RSS Changelog Update ===")
    print(f"::debug ::SSH Host: {SSH_HOST}")
    print(f"::debug ::SSH User: {SSH_USER}")
    print(f"::debug ::RSS File: {RSS_FILE}")
    print(f"::debug ::Changelog File: {CHANGELOG_FILE}")
    print(f"::debug ::Key length: {len(CHANGELOG_RSS_KEY) if CHANGELOG_RSS_KEY else 0}")
    
    # Check if changelog file exists
    if not os.path.exists(CHANGELOG_FILE):
        print(f"::error ::Changelog file not found: {CHANGELOG_FILE}")
        return

    try:
        with open(CHANGELOG_FILE, "r") as f:
            changelog = yaml.load(f, Loader=NoDatesSafeLoader)
        
        if not changelog or "Entries" not in changelog:
            print("::warning ::No entries in changelog")
            return
            
        print(f"::debug ::Loaded {len(changelog['Entries'])} changelog entries")
        
    except Exception as e:
        print(f"::error ::Failed to load changelog: {e}")
        return

    try:
        print(f"::debug ::Connecting to {SSH_HOST}:{SSH_PORT} as {SSH_USER}...")
        
        with paramiko.SSHClient() as client:
            # Load host keys
            load_host_keys(client.get_host_keys())
            
            # Set missing host key policy
            client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
            
            # Load private key
            key = load_key(CHANGELOG_RSS_KEY)
            print(f"::debug ::Loaded SSH key: {key.get_name()}")
            
            # Connect with timeout
            client.connect(
                SSH_HOST, 
                SSH_PORT, 
                SSH_USER, 
                pkey=key,
                timeout=30,
                banner_timeout=30,
                auth_timeout=30,
                look_for_keys=False,
                allow_agent=False
            )
            
            print(f"::debug ::SSH connection established")
            
            # Open SFTP
            sftp = client.open_sftp()
            print(f"::debug ::SFTP session opened")
            
            # Test connection by listing directory
            try:
                files = sftp.listdir('.')
                print(f"::debug ::SFTP directory listing: {files}")
            except Exception as e:
                print(f"::warning ::Could not list directory: {e}")
            
            # Load existing feed items
            last_feed_items = load_last_feed_items(sftp)
            print(f"::debug ::Found {len(last_feed_items)} existing RSS items")
            
            # Create new feed
            feed, any_new = create_feed(changelog, last_feed_items)
            
            if not any_new:
                print("No changes since last run.")
                sftp.close()
                client.close()
                return
            
            print(f"::debug ::Creating new RSS feed with updates")
            
            # Write RSS file
            et = ET.ElementTree(feed)
            
            # Create file content in memory first
            file_content = io.BytesIO()
            et.write(
                file_content,
                encoding="utf-8",
                xml_declaration=True,
                doctype='<?xml-stylesheet type="text/xsl" href="./stylesheet.xsl"?>',
                pretty_print=True
            )
            
            # Write to remote server
            with sftp.open(RSS_FILE, "wb") as f:
                f.write(file_content.getvalue())
            
            print(f"::debug ::RSS file '{RSS_FILE}' written successfully ({len(file_content.getvalue())} bytes)")
            
            # Copy stylesheet if it exists
            dir_name = os.path.dirname(__file__)
            template_path = pathlib.Path(dir_name, 'changelogs', XSL_FILE)
            
            if template_path.exists():
                try:
                    with sftp.open(XSL_FILE, "wb") as f, open(template_path, "rb") as fh:
                        f.write(fh.read())
                    print(f"::debug ::XSL stylesheet '{XSL_FILE}' copied")
                except Exception as e:
                    print(f"::warning ::Could not copy XSL stylesheet: {e}")
            else:
                print(f"::warning ::XSL template not found at {template_path}")
            
            # Clean up
            sftp.close()
            client.close()
            print(f"::debug ::Connection closed")
            print("::notice ::RSS feed updated successfully")
            
    except paramiko.AuthenticationException as e:
        print(f"::error ::SSH Authentication failed: {e}")
        print(f"::error ::Check SSH key and user permissions")
        return 1
    except paramiko.SSHException as e:
        print(f"::error ::SSH error: {e}")
        print(f"::error ::Traceback: {traceback.format_exc()}")
        return 1
    except Exception as e:
        print(f"::error ::Unexpected error: {e}")
        print(f"::error ::Traceback: {traceback.format_exc()}")
        return 1


def create_feed(changelog: Any, previous_items: List[Any]) -> Tuple[Any, bool]:
    rss = ET.Element("rss", attrib={"version": "2.0"})
    channel = ET.SubElement(rss, "channel")

    time_now = datetime.now(timezone.utc)

    # Fill out basic channel info
    ET.SubElement(channel, "title").text       = FEED_TITLE
    ET.SubElement(channel, "link").text        = FEED_LINK
    ET.SubElement(channel, "description").text = FEED_DESCRIPTION
    ET.SubElement(channel, "language").text    = FEED_LANGUAGE

    ET.SubElement(channel, "lastBuildDate").text = email.utils.format_datetime(time_now)
    ET.SubElement(channel, XML_NS_ATOM_B + "link", {"type": "application/rss+xml", "rel": "self", "href": FEED_URL})

    # Find the last item ID mentioned in the previous changelog
    last_changelog_id = find_last_changelog_id(previous_items)
    print(f"::debug ::Last changelog ID in RSS: {last_changelog_id}")

    any = create_new_item_since(changelog, channel, last_changelog_id, time_now)
    copy_previous_items(channel, previous_items, time_now)

    return rss, any


def create_new_item_since(changelog: Any, channel: Any, since: int, now: datetime) -> bool:
    entries_for_item = [entry for entry in changelog["Entries"] if entry["id"] > since]
    top_entry_id = max(map(lambda e: e["id"], entries_for_item), default=0)

    print(f"::debug ::Entries since ID {since}: {len(entries_for_item)}")
    
    if not entries_for_item:
        return False

    attrs = {XML_NS_B + "from-id": str(since), XML_NS_B + "to-id": str(top_entry_id)}
    new_item = ET.SubElement(channel, "item", attrs)
    ET.SubElement(new_item, "pubDate").text = email.utils.format_datetime(now)
    ET.SubElement(new_item, "guid", {"isPermaLink": "false"}).text = f"{FEED_GUID_PREFIX}{since}-{top_entry_id}"
    ET.SubElement(new_item, "title").text = f"Changelog entries {since}-{top_entry_id}"

    ET.SubElement(new_item, "description").text = generate_description_for_entries(entries_for_item)

    # Embed original entries inside the XML so it can be displayed more nicely by specialized tools.
    # Like the website!
    for entry in entries_for_item:
        xml_entry = ET.SubElement(new_item, XML_NS_B + "entry")
        ET.SubElement(xml_entry, XML_NS_B + "id").text = str(entry["id"])
        ET.SubElement(xml_entry, XML_NS_B + "time").text = entry["time"]
        ET.SubElement(xml_entry, XML_NS_B + "author").text = entry["author"]
        
        if "url" in entry and entry["url"]:
            ET.SubElement(xml_entry, XML_NS_B + "url").text = entry["url"]

        for change in entry["changes"]:
            attrs = {XML_NS_B + "type": change["type"]}
            ET.SubElement(xml_entry, XML_NS_B + "change", attrs).text = change["message"]

    return True


def generate_description_for_entries(entries: List[Any]) -> str:
    desc = io.StringIO()

    keyfn = lambda x: x["author"]
    sorted_author = sorted(entries, key=keyfn)
    
    desc.write("<h2>Changelog Updates</h2>\n")
    
    for author, group in itertools.groupby(sorted_author, keyfn):
        desc.write(f"<h3>{html.escape(author)} updated:</h3>\n")
        desc.write("<ul>\n")
        for entry in sorted(group, key=lambda x: x["time"]):
            for change in entry["changes"]:
                emoji = TYPES_TO_EMOJI.get(change["type"], "")
                msg = change["message"]
                desc.write(f"<li>{emoji} {html.escape(msg)}</li>")

        desc.write("</ul>\n")

    return desc.getvalue()


def copy_previous_items(channel: Any, previous: List[Any], now: datetime):
    # Copy in previous items, if we have them.
    items_copied = 0
    for item in previous:
        date_elem = item.find("./pubDate")
        if date_elem is None:
            # Item doesn't have a valid publication date?
            continue

        date = email.utils.parsedate_to_datetime(date_elem.text or "")
        if date + MAX_ITEM_AGE < now:
            # Item too old, get rid of it.
            continue

        channel.append(item)
        items_copied += 1
    
    print(f"::debug ::Copied {items_copied} previous items")


def find_last_changelog_id(items: List[Any]) -> int:
    return max(map(lambda i: int(i.get(XML_NS_B + "to-id", "0")), items), default=0)


def load_key(key_contents: str) -> paramiko.PKey:
    key_string = io.StringIO()
    key_string.write(key_contents)
    key_string.seek(0)
    return paramiko.Ed25519Key.from_private_key(key_string)


def load_host_keys(host_keys: paramiko.HostKeys):
    for key in HOST_KEYS:
        host_keys.add(SSH_HOST, "ssh-ed25519", paramiko.Ed25519Key(data=base64.b64decode(key)))


def load_last_feed_items(client: paramiko.SFTPClient) -> List[Any]:
    try:
        with client.open(RSS_FILE, "rb") as f:
            feed = ET.parse(f)

        items = feed.findall("./channel/item")
        print(f"::debug ::Loaded {len(items)} items from existing RSS")
        return items

    except FileNotFoundError:
        print(f"::debug ::RSS file not found, starting fresh")
        return []
    except Exception as e:
        print(f"::warning ::Error loading existing RSS: {e}")
        return []


if __name__ == "__main__":
    main()
